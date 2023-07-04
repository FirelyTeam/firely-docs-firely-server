.. _vonk_reference_api_repository:

Repository interfaces
=====================

.. _vonk_reference_api_isearchrepository:

ISearchRepository
-----------------

``Vonk.Core.Repository.ISearchRepository`` is central in the functioning of Firely Server. It defines all read-access to the underlying repository, being one of Firely Server's own database implementations or a Facade implementation.

It has a single method:

.. code-block:: csharp

   Task<SearchResult> Search(IArgumentCollection arguments, SearchOptions options);

Using ISearchRepository
^^^^^^^^^^^^^^^^^^^^^^^

#. Have it injected by the dependency injection into the class where you need it:

   .. code-block:: csharp

      public MyService(ISearchRepository searchRepository)
      {
         _searchRepository = searchRepository;
      }

   .. note::

      Implementations of ISearchRepository have a *Scoped* lifetime, so MyService should also be registered as Scoped:

      .. code-block:: csharp

         public IServiceCollection AddMyServices(this IServiceCollection services)
         {
            services.TryAddScoped<MyService>();
            return services;
         }

#. Prepare search arguments that express what you are looking for. Search arguments are effectively key-value pairs as if they came from the querystring on the request. So the key must be the code of a supported Searchparameter.

   .. code-block:: csharp

      var args = new ArgumentCollection(new IArgument[]
      {
            new Argument(ArgumentSource.Internal, ArgumentNames.resourceType, "Patient") {MustHandle = true}, // MustHandle = true is optional
            new Argument(ArgumentSource.Internal, "name", "Fred") {MustHandle = true} // MustHandle = true is optional
      }).AddCount(20);

   .. note::

      The Search implementation will in general update the arguments, especially their ``Status`` property and the ``Issue`` if something went wrong.
      So be careful with reuse of arguments. Use ``IArgumentCollection.Clone()`` if necessary .

#. Prepare search options that guide the search. Usually you can use one of the predefined options on the ``SearchOptions`` class.

   .. code-block:: csharp

      var options = SearchOptions.Latest(vonkContext.ServerBase, vonkContext.Request.Interaction, vonkContext.InformationModel);

#. Execute the search.

   .. code-block:: csharp

      var searchResult = await _searchRepository.Search(args, options);

#. Check the status of the arguments, especially if they could not be ignored (MustHandle = true). Because this is a common pattern, there is an extension method ``CheckHandled`` that throws a VonkParameterException if MustHandle arguments are not handled.

   .. code-block:: csharp

      try
      {
         args.CheckHandled("Arguments must all be handled in MyService");
      }
      catch (VonkParameterException vpe)
      {
         //report it in the vonkContext.Response.Outcome
      }

#. Inspect the number of the results to check whether anything was found. If so, you can enumerate the results or process the set as a whole, since ``SearchResult`` implements ``IEnumerable<IResource>``.

   .. code-block:: csharp

      if (searchResult.TotalCount > 0)
      {
         foreach(var resource in searchResult)
         { ... } 
      }

Implement ISearchRepository
^^^^^^^^^^^^^^^^^^^^^^^^^^^

Implementing ISearchRepository is only needed in a Facade. 

The general pattern for implementing ISearchRepository is:

#. For each of the IArguments in the IArgumentCollection:

   #. If you support the argument, translate it to a 'where' clause on your repository. If your backend is another Web API, this could have the form of a piece of a querystring.
   #. Call IArgument.Handled() to update its status. There is also .Warning() and .Error() when something is wrong with the argument. If you simply don't support the argument, you can leave the status to 'Unhandled'. 
   #. Pay special attention to the ``_count`` and ``_skip`` arguments for proper paging.

#. 'AND' all the arguments together, e.g. forming a database query or complete querystring.

#. Issue the query to your repository and await the results (await used intentionally: this should be done asynchronously).

#. For each of the resulting records or objects: map them to matching FHIR resources, either by: 

   #. Creating POCO's:
   
      .. code-block:: csharp
         
         var result = new Patient() { /*fill in from the source object*/ };
         return result.ToIResource(); //InformationModel implied by the assembly of class Patient
         
   #. or by crafting SourceNodes:
   
      .. code-block:: csharp
      
         var result = SourceNode.Resource("Patient", SourceNode.Valued("id", /* id from source */), SourceNode.Node("meta", SourceNode.Valued("versionId", "v1"), ....), ...))
         return result.ToIResource(VonkConstants.Model.FhirR3 /* or FhirR4 */);

#. Combine the mapped resources into a SearchResult:

   .. code-block:: csharp

      return new SearchResult(resources, pagesize, totalCount, skip);

   * ``pagesize``: should be the value of the _count argument, unless you changed it for some reason.
   * ``totalCount``: total number of results, if there are more than you are returning right now.
   * ``skip``: number of results skipped in this set (if you are serving page x of y).
   
For a Facade on a relational database we provide a starting point with ``Vonk.Facade.Relational.SearchRepository``. Follow the exercise in :ref:`facadestart` to see how that is done.

.. _vonk_reference_api_ichangerepository:

IResourceChangeRepository
-------------------------

``IResourceChangeRepository`` defines methods to change resources in the repository:

.. code-block:: csharp

   public interface IResourceChangeRepository
   {
      Task<IResource> Create(IResource input);
      Task<IResource> Update(ResourceKey original, IResource update);
      Task<IResource> Delete(ResourceKey toDelete, string informationModel);
      string NewId(string resourceType);
      string NewVersion(string resourceType, string resourceId);
   }

``ResourceKey`` is a simple struct to identify a resource by Type, Id and optionally VersionId.

Using IResourceChangeRepository
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

You should hardly ever need to use the ``IResourceChangeRepository``. It is used by the :ref:`Create <vonk_plugins_create>`, :ref:`Update <vonk_plugins_update>`, :ref:`Delete <vonk_plugins_delete>` and the conditional variations thereof.

Should you need to use it, the methods are fairly straightforward.

:method: Create
:description: Provide an IResource having an id and versionId. These can be obtained by calling NewId and NewVersion. The return value will contain a possibly updated IResource, if the implementation changed or added elements. To fill in all the metadata there is a convenient extension method on ``IResourceChangeRepository``:

   .. code-block:: csharp

      var withMetaInfo = changeRepository.EnsureMeta(resource); //Will keep existing id, version and lastUpdated and fill in if missing.
      //EnsureMeta also exists as extension method on IResource - that uses Guids for id and version.
      var createdResource = await changeRepository.Create(withMetaInfo);

:method: Update
:description: Assert that a resource exists that can be updated. If not, use ``Create``, otherwise go for Update.

   .. code-block:: csharp

      var existingKey = new ResourceKey(resourceType, resourceId);
      var args = existingKey.ToArguments(true);
      var args = args.AddCount(0); //We don't need the actual result - just want to know whether it is there.
      var options = SearchOptions.Latest(vonkContext.ServerBase, VonkInteraction.type_search, InformationModel: null); //search across informationmodels, we expect ids to be unique.
      var exists = (await searchRepository.Search(args, options)).TotalCount = 1; //Take care of < 1 or > 1 matches
      
      resource.EnsureMeta(KeepExisting.Id) //Will keep existing id and provide fresh version and lastUpdated.
      var updatedResource = await changeRepository.Update(existingKey, resource); 

:method: Delete
:description: Delete the resource that matches the provided key and informationModel. Returns the resource that was deleted.

   .. code-block:: csharp

      var existingKey = new ResourceKey(resourceType, resourceId);
      var deletedResource = await changeRepository(existingKey, vonkContext.InformationModel);

:method: NewId
:description: Get a new Id value generated by the repository (e.g. when the repository wants to use a sequence generator or ids in a specific format).
   Generally used through the extension method IResourceChangeRepository.EnsureMeta(IResource resource, KeepExisting keepExisting), see ``Create`` above.
   
:method: NewVersion
:description: Get a new Version value generated by the repository (e.g. when the repository wants to use a sequence generator or ids in a specific format). The repository may want to base the version on the id, therefore the Id is passed as an argument.
   Generally used through the extension method IResourceChangeRepository.EnsureMeta(IResource resource, KeepExisting keepExisting), see ``Create`` above.

Implement IResourceChangeRepository
-----------------------------------

Implementing IResourceChangeRepository is only needed in a Facade that wants to provide write-access to the underlying repository.

For all three methods, you will have to map data from FHIR resources to your internal data structures and back.

Note that you also need to implement :ref:`vonk_reference_api_isearchrepository` to support the :ref:`Create <vonk_plugins_create>` and :ref:`Update <vonk_plugins_update>` plugins and of course the conditional variants of those.
