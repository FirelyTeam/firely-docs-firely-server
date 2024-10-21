.. _vonk_reference_api_bundles:

Constructing bundles
====================

In a Plugin or Facade you may need to construct a bundle from a set of resources, e.g. a SearchResult (see :ref:`here <vonk_reference_api_isearchrepository>`). There are two ways of doing this: with the Bundle POCO or with SourceNodes.

Bundle POCO
-----------

This is fairly straightforward. Create a new Bundle object, and fill its properties, iterating over the set of resources that you have. The code looks like this:

.. code-block:: csharp

   using Hl7.Fhir.Model; //Either from Hl7.Fhir.Core.Stu3 or Hl7.Fhir.Core.R4

   ...

      var searchResult = await searchRepository.Search(args, options)
      var bundle = new Bundle() { Type = Bundle.BundleType.Searchset }; // Type is required

      foreach (var resource in searchResult)
      {
         bundle.Entry.Add
         {
            new BundleEntryComponent
            {
               Resource = resource.ToPoco<Resource>();
               // fill in any other details like Request or Response.
            }
         }
      }

      //fill in details of the bundle as a whole, like Meta or Identifier.

The limitation of this is that you are bound to either STU3 or R4, and that also implies that you cannot include :ref:`feature_customresources` in the bundle.

Bundle from SourceNodes
-----------------------

ISourceNode is from Hl7.Fhir.ElementModel and not tied to a specific FHIR version, and Firely Server can serialize ISourceNode provided that the right StructureDefinition is available in the Administration API - which is the case for Bundle by default.

You start by creating the Bundle itself using the class SourceNode that allows for construction of ISourceNode nodes. 

.. code-block:: csharp

   using Hl7.Fhir.ElementModel;

   ...

      var bundleNode = SourceNode.Resource("Bundle", "Bundle", SourceNode.Valued("type", "document")); 
      
      //choose type as one of the bundle types from the spec, see http://hl7.org/fhir/R4/bundle-definitions.html#Bundle.type

Then you can add elements to the bundle itself that are not in the entries of the bundle. Like an identifier:

.. code-block:: csharp

      var identifier = SourceNode.Node("identifier");
      identifier.Add(SourceNode.Valued("system", "urn:ietf:rfc:3986"));
      identifier.Add(SourceNode.Valued("value", Guid.NewGuid().ToString()));
      bundleNode.Add(identifier);

Then you can turn this into the helper class ``GenericBundle`` that provides several helper methods on an ISourceNode that is known to be a Bundle.

.. code-block:: csharp

      var documentBundle = GenericBundle.FromBundle(bundleNode);
      documentBundle = documentBundle.Meta(Guid.NewGuid().ToString(), DateTimeOffset.Now);

Maybe you already saw an alternative way of adding the identifier in the intellisense by now:

.. code-block:: csharp

      documentBundle = documentBundle.Identifier("urn:ietf:rfc:3986", Guid.NewGuid().ToString());

Note that you always have to continue with the *result* of the modifying function. All these functions act on ISourceNode and that is immutable, so you get a new instance with the changes applied as a return value.

Now you have the skeleton of the Bundle, it is ready to add entries with resources to it.

.. code-block:: csharp

      IResource resourceForDocument = ... ; //Get or construct a resource that is one of the entries of the Bundle.
      documentBundle = documentBundle.AddEntry(resourceForDocument, resourceForDocument.Key().ToRelativeUri());

Other extensions methods available on ``GenericBundle``:

.. code-block:: csharp

      public static GenericBundle Total(this GenericBundle bundle, int total)
      public static GenericBundle AddLink(this GenericBundle bundle, string relation, string uri)
      public static GenericBundle Links(this GenericBundle bundle, Dictionary<string, string> links)


Search result bundles
^^^^^^^^^^^^^^^^^^^^^

Usually you don't need to construct a searchset bundle yourself, since the SearchService takes care of that when a search is issued on the FHIR endpoint. But should you want to do it in a custom operation, then the methods for doing so are at your disposal.

To help construct a bundle of type 'searchset', there is a special kind of bundle class ``SearchBundle``. Create the sourcenode for the bundle as above. Then instead of creating a ``GenericBundle``, turn it into a ``SearchBundle``:

.. code-block:: csharp

      var searchBundle = bundleNode.ToSearchBundle();

Now you can use various methods to add entries for matches, includes or an OperationOutcome:

.. code-block:: csharp

      //SearchBundle methods
      public SearchBundle AddMatch(ISourceNode resource, string fullUrl, string score = null)
      public SearchBundle AddInclude(ISourceNode resource, string fullUrl, string score = null)
      public SearchBundle AddOutcome(ISourceNode outcome, string fullUrl, string score = null)

      //Extension methods
      public static SearchBundle ToSearchBundle(this IEnumerable<SearchInfo> searchInfos, string informationModel)
      public static SearchBundle ToSearchBundle(this IEnumerable<ISourceNode> resources, string searchMode, string informationModel)
      public static SearchBundle ToSearchBundle(this IEnumerable<ITypedElement> resources, string searchMode, string informationModel)

The ``SearchInfo`` struct essentially captures all the information that goes into an entry of a searchset bundle:

.. code-block:: csharp

      public struct SearchInfo
      {
         public SearchInfo(ISourceNode resource, string mode = SearchMode.match, string fullUrl = null, string score = null)

         public string Mode { get; }
         public ISourceNode Resource { get; }
         public string FullUrl { get; }
         public string Score { get; }
      }

Using all this to turn the ``SearchResult`` returned from the ``ISearchRepository.Search()`` method into a bundle looks like this (using the second extension method above):

.. code-block:: csharp

      using Vonk.Fhir.R4;

      ...

      var bundle = searchResult
            .ToSearchBundle(SearchMode.match, vonkContext.InformationModel)
            //informationModel is needed because bundle has slight differences between STU3 and R4
            .Total(searchResult.Page.TotalCount)
            //Total is defined on GenericBundle
            .Links(searchResult.Page.PagingLinks(vonkContext));
            //Links is defined on GenericBundle
      return bundle.ToIResource(vonkContext.InformationModel).EnsureMeta();
