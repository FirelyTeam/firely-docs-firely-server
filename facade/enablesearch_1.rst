Enable Search
=============

Enabling search involves two major steps:

#. Creating a query to the database based on the bits and pieces in the search url
#. Getting a count and actual data from the database with that query, and map it to a SearchResult

The next paragraphs will walk you through these steps.

1. Create a query
-----------------

Firely Server Facade is meant to be used across all kinds of database paradigms and schemas. Or even against underlying web services or stored procedures.
This means Firely Server cannot prescribe the way your query should be expressed. After all, it could be an http call to a webservice, or a json command to MongoDB.

In our case we will build a LINQ query against our ViSi model, that is translated by Entity Framework to a SQL query.
Because this is a quite common case, Firely Server provides a basis for it in the package ``Vonk.Facade.Relational``.

* Go back to the NuGet Package Manager Console and run ``Install-Package Vonk.Facade.Relational``

.. note:: If you did this previously for the other Firely Server packages, you can install the latest beta release of this package as well by adding
          ``-IncludePrerelease`` to the install command.

Adding classes for the query
^^^^^^^^^^^^^^^^^^^^^^^^^^^^

You usually create a query class per ResourceType. The Query object is used to capture the elements of the search that are provided to the QueryFactory.

In this exercise we start with resource type Patient, and will create a ``PatientQuery`` and ``PatientQueryFactory`` class.
Because PatientQuery has no specific content of its own, we will include both in one file.

* Add a new class ``PatientQueryFactory`` to the root of the project
* Add using statements for ``Vonk.Facade.Relational``, ``Microsoft.EntityFrameworkCore``, and ``<your project>.Models``
* Above the actual ``PatientQueryFactory`` class insert the ``PatientQuery`` class::

    public class PatientQuery: RelationalQuery<ViSiPatient>
    {}

* Now flesh out the ``PatientQueryFactory``::

    public class PatientQueryFactory: RelationalQueryFactory<ViSiPatient, PatientQuery>
    {}

Adding a constructor
^^^^^^^^^^^^^^^^^^^^

You have to provide a constructor for the factory class. With this you tell Firely Server for which resource type this QueryFactory is valid.
The DbContext is used for retrieving DbSets for related entities, as we will see later::

    public PatientQueryFactory(DbContext onContext) : base("Patient", onContext) { }


.. _facade_fhir_version:

Deciding on a FHIR version
^^^^^^^^^^^^^^^^^^^^^^^^^^

You need to explicitly tell Firely Server for which FHIR version(s) you wish to return resources. If you don't override ``EntryInformationModel``, any search will fail with a ``501 Not Implemented``. The following override will allow searches for any possible FHIR version to be handled by your facade::
       
    public override PatientQuery EntryInformationModel(string informationModel)
    {
        return default(PatientQuery);
    }

If you wish to implement search only for a single FHIR version or for a limited set of versions you can override the method like this::

    public override PatientQuery EntryInformationModel(string informationModel)
    {
        if (informationModel == VonkConstants.Model.FhirR4)
        {
            return default(PatientQuery);
        }
        
        throw new NotImplementedException($"FHIR version {informationModel} is not supported");        
    }
	

Handling the search request
^^^^^^^^^^^^^^^^^^^^^^^^^^^
Each of the searchparameters in the search request triggers a call to the ``Filter`` method. This method takes a
``parameterName`` and ``IFilterValue`` as its arguments.

The ``parameterName`` is the searchparameter as it was used in the search url. This name corresponds with the code field in a `SearchParameter <https://www.hl7.org/fhir/searchparameter.html>`_ resource.
The ``IFilterValue value`` has a list of possible implementations, one for `each type of SearchParameter <http://hl7.org/fhir/search.html#ptypes>`_. See :ref:`parameter_types`
for a short description of these possibilities.

By default the ``Filter`` method dispatches the call to a suitable overload of ``AddValueFilter``, based on the actual type of the ``value`` parameter.
It is up to you to override the ones you support any parameters for.

* Override the method ``PatientQuery AddValueFilter(string parameterName, TokenValue value)`` in the ``PatientQueryFactory`` class to implement support for the ``_id`` parameter, which
  is a token type parameter.

  The ``_id`` parameter must be matched against the ViSiPatient.Id property. So we have to:

  * Parse the Token.Code to an integer (ViSiPatient.Id is of type int)
  * Create a query with a predicate on ViSiPatient.Id.

    This is how:

    .. code-block:: csharp

     if (parameterName == "_id")
     {
         if (!int.TryParse(value.Code, out int patientId))
         {
             throw new ArgumentException("Patient Id must be an integer value.");
         }
         else
         {
             return PredicateQuery(vp => vp.Id == patientId);
         }
     }
     return base.AddValueFilter(parameterName, value);

.. note::
  The ``ArgumentException`` in this code will automatically result in setting the argument status to error, so the Firely Server
  will send a response with an error code and OperationOutcome. See the information about the ``IArgumentCollection``
  and ``IArgument`` classes in :ref:`vonk_reference_api_ivonkcontext`.

That's it for now, we will add support for another parameter later.

.. _parameter_types:

IFilterValue implementations
^^^^^^^^^^^^^^^^^^^^^^^^^^^^

There are 12 possible implementations you can use as value for the IFilterValue parameter in the Query.
The first 7 are the `general search parameter types <http://hl7.org/fhir/search.html#ptypes>`_: StringValue, DateTimeValue, TokenValue, NumberValue, QuantityValue, UriValue and ReferenceValue.

Then there are ResourceTypesValue and ResourceTypesNotValue. These typically define the context of your query: Which type of resource is being searched for.
Both can have multiple resource types as value, since FHIR allows for searching across multiple resource types at once.
If you base your implementation on the ``Vonk.Facade.Relational`` package, these are handled for you, but you can override it if you need to.

Besides that there are two special values for chaining and reverse chaining:
ReferenceToValue and ReferenceFromValue.

And finally there is a special value for when Firely Server does not know the SearchParameter and hence not the type of it:
RawValue.
