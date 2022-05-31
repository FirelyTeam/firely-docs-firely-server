
This is the second step for enabling search.

2. Get the data and map to FHIR
-------------------------------

Getting the data happens in the implementation of the ``ISearchRepository``. It has only one method, ``Search``.
The Vonk.Facade.Relational package has an abstract implementation of it that you can use as a starting point.
This implementation assumes that you can support searching for exactly one ResourceType at a time.
The gist of the implementation is to switch the querying based on the ResourceType. The querying itself then looks pretty much the same for every type of resource.

* Add the new class ViSiRepository to the root of the project::

    public class ViSiRepository : SearchRepository

* You have to provide a constructor that gets a QueryContext. We'll get to that later.
  Apart from that you will need your DbContext (ViSiContext) to query on, and the ResourceMapper to perform the mapping of the results.
  So put all of that in the constructor:

 .. code-block:: csharp

    private readonly ViSiContext _visiContext;
    private readonly ResourceMapper _resourceMapper;

    public ViSiRepository(QueryContext queryContext, ViSiContext visiContext, ResourceMapper resourceMapper) : base(queryContext)
    {
        _visiContext = visiContext;
        _resourceMapper = resourceMapper;
    }

* You will have to implement the abstract method ``Task<SearchResult> Search(string resourceType, IArgumentCollection arguments, SearchOptions options)``.

   * First, let's inspect the parameters:

       :resourceType: The ResourceType that is being searched for, e.g. Patient in ``<firely-server-endpoint>/Patient?...``
       :arguments: All the arguments provided in the search, whether they come from the path (like 'Patient'), the querystring (after the '?'), the headers or the body. Usually you don't have to inspect these yourself.
       :options: A few hints on how the query should be executed: are deleted or contained resources allowed etc. Usually you just pass these on as well.

   * The pattern of the implementation is:

       1. switch on the resourceType
       2. dispatch to a method for querying for that resourceType

     Naturally we do this async, since in a web application you should never block a thread while waiting for the database.

   * To implement this, add this to the class::

       protected override async Task<SearchResult> Search(string resourceType, IArgumentCollection arguments, SearchOptions options)
       {
           switch (resourceType)
           {
               case "Patient":
                   return await SearchPatient(arguments, options);
               default:
                   throw new NotImplementedException($"ResourceType {resourceType} is not supported.");
           }
       }


* Now we moved the problem to ``SearchPatient``, so this method needs to be implemented.
  The pattern here is:

   #. Create a query - in this case, a PatientQuery via PatientQueryFactory.
   #. Execute the query against the DbContext (our _visiContext) to get a count of matches.
   #. Execute the query against the DbContext to get the current page of results.
   #. Map the results using the _resourceMapper

   The implementation of this looks like::

     private async Task<SearchResult> SearchPatient(IArgumentCollection arguments, SearchOptions options)
     {
         var query = _queryContext.CreateQuery(new PatientQueryFactory(_visiContext), arguments, options);

         var count = await query.ExecuteCount(_visiContext);
         var patientResources = new List<IResource>();

         if (count > 0)
         {
             var visiPatients = await query.Execute(_visiContext).ToListAsync();

             foreach (var visiPatient in visiPatients)
             {
                 patientResources.Add(_resourceMapper.MapPatient(visiPatient));
             }
         }
         return new SearchResult(patientResources, query.GetPageSize(), count);
     }

What happens behind the scenes is that the QueryBuilderContext creates a QueryBuilder that analyzes all the arguments and options, and translates that into calls into your PatientQueryFactory.
This pattern offers maximum assistance in processing the search, but also gives you full control over the raw arguments in case you need that for anything.
Any argument that is reported as in Error, or not handled will automatically show up in the OperationOutcome of the Firely Server response.

In the next paragraph you will configure your Firely Server to use your Facade, and can -- finally --  try out some searches.
The paragraph after that expands the project to support ViSiBloodPressure Observations, and details how to add custom search parameters.

.. note::
    Your implementation of ``ISearchRepository`` may have other dependencies than the ones listed above, but it cannot be dependent upon ``IStructureDefinitionSummaryProvider``. 
    That causes a circular dependency, and will have you wait for a response from the server indefinitely.
    This means that in the implementation you can work with POCO's (as is done in this tutorial) or with 'raw' ``SourceNode`` instances, but not with ``ITypedElement`` (for the latter two see the Firely .NET SDK).