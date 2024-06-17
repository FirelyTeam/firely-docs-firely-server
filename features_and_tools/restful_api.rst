.. _restful:

FHIR RESTful API
================

.. note::

  The features described on this page are available in **all** :ref:`Firely Server editions <vonk_overview>`.

Firely Server supports most of the features in the `FHIR RESTful API <http://www.hl7.org/implement/standards/fhir/http.html>`_.

FHIR Versions
-------------

All the operations below can be called for FHIR STU3 or FHIR R4. Firely Server supports the fhirVersion mimetype parameter and fhir version endpoint mappings for that purpose. 
See :ref:`feature_multiversion` for more information.

.. _restful_crud:

Create, read, update, patch, delete
-----------------------------------

These five operations to manage the contents of the Firely Server, commonly referenced by the acronym CRUD, are implemented as per the specification. Patch is implemented as `FHIR Patch <http://hl7.org/fhir/fhirpatch.html>`_, as this is the most versatile one.
A few limitations apply.

Firely Server enables create-on-update: If you request an update and no resource exists for the given id, the provided resource will be created under the provided id.

Firely Server can reject a resource based on :ref:`feature_prevalidation`.

.. _restful_crud_configuration:

Configuration
^^^^^^^^^^^^^

A conditional delete interaction may match multiple resources. You can configure the server to delete all matches, or reject the operation (effectively only allowing single matches to be deleted).
Allowing multiple deletes requires support for transactions on the database (MongoDb, SQL Server or SQLite). 
If you allow for multiple deletes, you have to specify a maximum number of resources that can be deleted at once, to save you from accidentally deleting too many resources.

::

    "FhirCapabilities": {
        "ConditionalDeleteOptions": {
            "ConditionalDeleteType": "Single", // Single or Multiple,
            "ConditionalDeleteMaxItems": 1
        }
    }

.. _restful_crud_limitations:

Limitations on CRUD
^^^^^^^^^^^^^^^^^^^

#. Simultaneous conditional creates and updates are not entirely transactional safe:
   
   * Two conditional updates may both result in a ``create``, although the result of one may be a match to the other.
   * Two conditional creates may both succeed, although the result of one may be a match to the other.
   * A conditional create and a simultaneous conditional update may both result in a ``create``, although the result of one may be a match to the other.

#. It is not possible to bring a resource that has earlier been deleted back to life with a conditional update while providing the logical id of the resource in the request payload. This operation will result in an ``HTTP 409 Conflict`` error. As a workaround, it is possible to create a new resource (with a new logical id) by omitting the ``id`` field in the payload.
#. Parameter ``_pretty`` is not yet supported.
#. XML Patch and JSON Patch, as well as version-read and conditional variations of FHIR Patch are not yet supported.

.. _restful_noop:

Update with no changes 
----------------------

Updating Firely Server with a resource that is identical to an existing resource in the database will normally create a new version of that resource, along with new AuditEvent and Provenance resources if auditing is enabled.
This can put extra load on the server where this is not entirely necessary. To avoid these updates that can unnecessarily increase server load and database growth, the No-Op (No Operation) plugins can be enabled.
By enabling these plugins, the server acknowledges the request without making any actual modifications to the database. If an update resulted in a No-Op scenario, this can be observed in the OperationOutcome that is returned by Firely Server.

Configuration for No-Op
^^^^^^^^^^^^^^^^^^^^^^^

To make sure Firely server uses the No-Op scenario, the `UpdateNoOp` plugins need to be enabled in the `PipelineOptions`. 
::

  "PipelineOptions": {
    "PluginDirectory": "./plugins",
    "Branches": [
      {
        "Path": "/",
        "Include": [
          "Vonk.Plugin.UpdateNoOp.UpdateNoOpConfiguration",
          "Vonk.Plugin.UpdateNoOp.PatchNoOpConfiguration",
          "Vonk.Plugin.UpdateNoOp.ConditionalUpdateNoOpConfiguration",
        ]
      }
    ]

    "UpdateNoOp": {
      "AdditionalMetaToBeIgnored": [
        "security",
        "tag",
        "profile"
      ]
    }

There are three No-Op plugins available:

* ``Vonk.Plugin.UpdateNoOp.UpdateNoOpConfiguration`` - For regular updates
* ``Vonk.Plugin.UpdateNoOp.PatchNoOpConfiguration`` - For Patch operations
* ``Vonk.Plugin.UpdateNoOp.ConditionalUpdateNoOpConfiguration`` - For conditional updates

By default the following meta elements are ignored during resource comparison: ``versionId``, ``lastUpdated`` and ``source``. You can also add ``security``, ``tag`` and ``profile`` or any other meta element to be ignored, but it depends on your specific usage of meta. For more information see `the hl7 specification <https://www.hl7.org/fhir/resource.html#tag-updates>`__.

To determine if your action resulted in a No-Op scenario, you can configure Firely Server to return an OperationOutcome. For this it is necessary to configure the Prefer Header as Firely Server does not return this response by default.
The Prefer Header can be set in three ways, as per `the hl7 specification <https://build.fhir.org/http.html#ops>`__:

* ``return=minimal``- Nothing is returned by the server
* ``return=representation`` - The resource is returned as present in the database
* ``return=OperationOutcome`` - Return an OperationOutcome

In the example below an OperationOutcome for a No-Op scenario is returned when the Prefer Header is set to ``return=OperationOutcome``:
::

  {
    "resourceType": "OperationOutcome",
    "id": "26a724d9-10e4-4a71-819e-15d52f6f821c",
    "meta": {
      "versionId": "b6063533-a93e-4cd1-bb0b-5f37381d0f20",
      "lastUpdated": "2024-02-12T11:12:40.6172822+00:00"
    },
    "issue": [
      {
        "severity": "information",
        "code": "informational",
        "details": {
          "coding": [
            {
              "system": "http://hl7.org/fhir/dotnet-api-operation-outcome",
              "code": "5025"
            }
          ],
          "text": "No changes were performed as the provided resource contains no changes to the existing resource"
        }
      },
      {
        "severity": "information",
        "code": "informational",
        "diagnostics": "The operation was successful"
      }
    ]
  }

.. _restful_versioning:

Versioning
----------

Firely Server keeps a full version history of every resource, including the resources on the :ref:`administration_api`.

.. _restful_search:

Search
------

Search is supported as per the specification, with a few :ref:`restful_search_limitations`.

In the default configuration the SearchParameters from the `FHIR specification <http://www.hl7.org/implement/standards/fhir/searchparameter-registry.html>`_ 
are available. But Firely Server also allows :ref:`feature_customsp`. 

Chaining and reverse chaining is fully supported.

Quantity search on UCUM quantities automatically converts units to a canonical form. This means you can have kg in an Observation and search by lbs, or vice versa.

`Compartment Search <http://www.hl7.org/implement/standards/fhir/search.html#2.21.1.2>`_ is supported.

.. warning:: Queries that request resource types not included in the current compartment's CompartmentDefinition will yield default search results. Example: Searching for Practitioner resources within a Patient's compartment will return all Practitioner resources, including the ones not linked to the patient.

Firely Server also supports ``_include:iterate`` and ``_revinclude:iterate``, as well as its STU3 counterparts ``_include:recurse`` and ``_revinclude:recurse``. See `the specification <http://hl7.org/fhir/R4/search.html#revinclude>`_ for the definition of those. You can configure the maximum level of recursion::

   "FhirCapabilities": {
      "SearchOptions": {
         "MaximumIncludeIterationDepth": 1
      }
   },

.. warning:: ``_include`` isn't supported for a versioned reference

.. _navigational_links:

Navigational links
^^^^^^^^^^^^^^^^^^
The "next", "prev", and "last" link may contain privacy-sensitive information as part of a search parameter value. In order to not expose these values in logs, the :ref:`Vonk.Plugin.SearchAnonymization<vonk_plugins_searchAnonymization>` plugin can be used. It will replace the query parameter part of the navigational link with an opaque UUID. The plugin must be used starting with FHIR R5 as the specification mandates the removal of sensitive information.

Modifiers
^^^^^^^^^

Modifiers can influence the behaviour of a search parameter. Modifiers are defined per search parameter type in the `FHIR core specification <https://www.hl7.org/fhir/search.html#modifiers>`_.
Firely Server supports modifiers for the following data types:

+-----------------------------+----------------+-------------+
| Search parameter types      | Modifier name  | Supported?  |
+=============================+================+=============+
| All search parameter types  | :missing       | ✅          |
+-----------------------------+----------------+-------------+
| string                      | :exact         | ✅          |
+-----------------------------+----------------+-------------+
| string                      | :contains      | ✅          |
+-----------------------------+----------------+-------------+
| token                       | :text          | ✅          |
+-----------------------------+----------------+-------------+
| token                       | :in            | ❌          |
+-----------------------------+----------------+-------------+
| token                       | :below         | ❌          |
+-----------------------------+----------------+-------------+
| token                       | :above         | ❌          |
+-----------------------------+----------------+-------------+
| token                       | :not-in        | ❌          |
+-----------------------------+----------------+-------------+
| reference                   | :[type]        | ✅          |
+-----------------------------+----------------+-------------+
| reference                   | :identifier    | ✅          |
+-----------------------------+----------------+-------------+
| reference                   | :above         | ❌          |
+-----------------------------+----------------+-------------+
| reference                   | :below         | ❌          |
+-----------------------------+----------------+-------------+
| uri                         | :below         | ✅          |
+-----------------------------+----------------+-------------+
| uri                         | :above         | ❌          |
+-----------------------------+----------------+-------------+


When searching with the ``:exact`` modifier the server handles `grapheme clusters <http://hl7.org/fhir/R4B/search.html#modifiers>`_. 

.. _restful_search_sort:

Sorting
^^^^^^^

``_sort`` is implemented for searchparameters of types: 

* string 
* number 
* uri
* reference
* datetime
* token

for the all supported repositories.

How is sort evaluated?

* A searchparameter may be indexed with multiple values for a single resource. E.g. Patient.name for Angelina Jolie would have name=Angelina and name=Jolie. And George Clooney: name=George and name=Clooney. As the FHIR Specification phrases it: "In this case, the sort is based on the item in the set of multiple parameters that comes earliest in the specified sort order when ordering the returned resources." Here is an example of how Firely Server evaluates this.

   * In ascending order: ``Patient?_sort=name``

      +-------------+--------------------+------------------+
      | Name values | Asc. per resource  | Asc. resources   |
      +=============+====================+==================+
      | Angelina    | Angelina           | *Angelina* Jolie |
      +-------------+--------------------+------------------+
      | Jolie       | Jolie              |                  |
      +-------------+--------------------+------------------+
      |             |                    |                  |
      +-------------+--------------------+------------------+
      | George      | Clooney            | George *Clooney* |
      +-------------+--------------------+------------------+
      | Clooney     | George             |                  |
      +-------------+--------------------+------------------+

   * Now in descending order: ``Patient?_sort=-name``

      +-------------+--------------------+------------------+
      | Name values | Desc. per resource | Desc. resources  |
      +=============+====================+==================+
      | Angelina    | Jolie              | Angelina *Jolie* |
      +-------------+--------------------+------------------+
      | Jolie       | Angelina           |                  |
      +-------------+--------------------+------------------+
      |             |                    |                  |
      +-------------+--------------------+------------------+
      | George      | George             | *George* Clooney |
      +-------------+--------------------+------------------+
      | Clooney     | Clooney            |                  |
      +-------------+--------------------+------------------+


* The searchparameter to sort on may not be indexed at all for some of the resources in the resultset. E.g. a Patient without any identifier will not be indexed for Patient.identifier. Resources not having that parameter always end up last (both in ascending and descending order). This is similar to the ‘nulls last’ option in some SQL languages.

* Token parameters are sorted only on their code element. The system element is ignored in the sorting.

* Firely Server uses the default collation as configured on the database server. This collation defines the ordering of characters.
 
* All elements of type ``date`` and ``Period`` are treated as being a ``Period`` for sorting. When sorting ascending, the ``start`` of the period will be used. Similarly, when sorting descending the ``end`` of the period will be used. When sorting on a search parameter that references multiple ``date`` and/or ``Period`` values, the minimum (for ascending) or maximum (for descending) of the combined values will be used.

* Sorting on ``_score`` is not supported.

.. _restful_search_limitations:

Limitations on search
^^^^^^^^^^^^^^^^^^^^^

The following parameters and options are not yet supported:

#. ``_text``
#. ``_content``
#. ``_query``
#. ``_containedType``
#. ``_filter``
#. ``Location.near`` (geo matching is not supported)
#. ``:approx`` modifier on a quantity SearchParameter
#. ``:text`` modifier on a string SearchParameter
#. ``:above``, ``:below``, ``:in`` and ``:not-in`` modifiers on a token SearchParameter, ``above`` and ``below`` are also not supported for `Mime Types <http://hl7.org/fhir/R4B/search.html#mimetype>`_.
#. ``:above``, ``:below`` modifiers on a reference SearchParameter (only valid on a `strict hierarchy <http://hl7.org/fhir/R4B/search.html#recursive>`_)
#. ``_include`` and ``_revinclude`` will match the current version of the referenced resources, also if the reference is versioned.
#. ``_pretty``
#. Implicit ranges are supported on dates, datetimes and quantities with a UCUM unit. But not on other quantities and number parameters.
#. Search parameter arguments in exponential form (e.g. 1.8e2).
#. ``_total=estimate``, only ``none`` and ``accurate`` are supported.

In addition, Firely Server does not support the search parameters whose field ``xpathUsage`` (STU3, R4) or ``processingMode`` (R5) is not set to ``normal``. Concretely, this means that the following search parameters are not supported:

#. ``http://hl7.org/fhir/SearchParameter/individual-phonetic`` (STU3, R4, R5).
#. ``http://hl7.org/fhir/SearchParameter/InsurancePlan-phonetic`` (R4, R5)
#. ``http://hl7.org/fhir/SearchParameter/Location-near`` (STU3, R4, R5), 
#. ``http://hl7.org/fhir/SearchParameter/Location-near-distance`` (STU3), 
#. ``http://hl7.org/fhir/SearchParameter/Organization-phonetic`` (STU3, R4, R5), 
#. ``http://hl7.org/fhir/SearchParameter/Resource-in`` (R5), 


Furthermore:

#. Paging is supported, but it is not isolated from intermediate changes to resources.

.. _us-core_composite_parameters:
.. warning::

    US-Core search parameters interfere with the evaluation of composite search parameters in Firely Server. 
    US-Core redefines the Observation.code parameter, but does not redefine the related composite search parameters. 
    
    If you load the artifacts of US-Core into the administration endpoint, be aware that you need updated versions of the composite search parameters as well. 
    
    The pre-built SQLite administration database, that comes with the Firely Server distribution, has US-Core 3.1.1 preloaded. In this database, Firely has already taken care of this for you.
    
    Corrected versions of the search parameters are:
    
    - Observation.code-value-concept: :download:`download <../_static/files/us-core-composite-parameters/SearchParameter-firely-us-core-observation-code-value-concept.json>`
    - Observation.code-value-date: :download:`download <../_static/files/us-core-composite-parameters/SearchParameter-firely-us-core-observation-code-value-date.json>`
    - Observation.code-value-quantity: :download:`download <../_static/files/us-core-composite-parameters/SearchParameter-firely-us-core-observation-code-value-quantity.json>`
    - Observation.code-value-string: :download:`download <../_static/files/us-core-composite-parameters/SearchParameter-firely-us-core-observation-code-value-string.json>`
    
    You can add these as individual files to your administration :ref:`import folder<conformance_fromdisk>`, or merge them into the US-Core package.

.. _restful_history:

History
-------

History is supported as described in the specification, on the system, type and instance level.
The ``_since`` and ``_count`` parameters are also supported. 
The response will be a ``Bundle`` which adheres to the ``BundleOptions`` configuration, see :ref:`bundle_options`.

.. _restful_history_limitations:

Limitations on history
^^^^^^^^^^^^^^^^^^^^^^

#. ``_at`` parameter is not yet supported.
#. Paging is supported, but it is not isolated from intermediate changes to resources.

.. _restful_batch:

Batch
-----

Batch is fully supported on the usual endpoint. You can limit the number of entries accepted in a single batch. See :ref:`sizelimits_options`.

Note that batches are not supported in the ``/administration`` endpoint.

.. _restful_transaction:

Transaction
-----------

Transactions are supported, but with the following limitation:

#. The ``/administration`` endpoint does not support transactions.

You can limit the number of entries accepted in a single transaction. See :ref:`sizelimits_options`.

.. _restful_capabilities:

Capabilities
------------

On the Capabilities interaction (``<firely-server-endpoint>/metadata``) Firely Server returns a CapabilityStatement that is built dynamically from the 
supported ResourceTypes, SearchParameters and interactions. E.g. if you :ref:`feature_customsp_configure`, the SearchParameters that are actually loaded appear in the CapabilityStatement.

.. _restful_notsupported:

Not supported interactions
--------------------------

These interactions are not yet supported by Firely Server:

#. HEAD

Besides that, Firely Server does not yet return the ``date`` header as specified in `HTTP return values <http://hl7.org/fhir/R4/http.html#return>`_
