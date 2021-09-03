.. _restful:

FHIR RESTful API
================

Firely Server supports most of the features in the `FHIR RESTful API <http://www.hl7.org/implement/standards/fhir/http.html>`_.

FHIR Versions
-------------

All the operations below can be called for FHIR STU3 or FHIR R4. Firely Server supports the fhirVersion mimetype parameter and fhir version endpoint mappings for that purpose. 
See :ref:`feature_multiversion` for more information.

.. _restful_crud:

Create, read, update, patch, delete
-----------------------------------

These five operations to manage the contents of the Firely Server, commonly referenced by the acronym CRUD, are implemented as per the specification. Patch is implemented as `FHIR Patch <http://hl7.org/fhir/fhirpatch.html>`_, as this is the most versatile one.
This includes version-read and the conditional variations. 
Only a few limitations apply.

Firely Server enables create-on-update: If you request an update and no resource exists for the given id, the provided resource will be created under the provided id.

Firely Server can reject a resource based on :ref:`feature_prevalidation`.

.. _restful_crud_configuration:

Configuration
^^^^^^^^^^^^^

A conditional delete interaction may match multiple resources. You can configure the server to delete all matches, or reject the operation (effectively only allowing single matches to be deleted).
Allowing multiple deletes requires support for transactions on the database (SQL Server or SQLite). 
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

#. Simultaneous conditional creates and updates are not entirely transactionally safe:
   
   * Two conditional updates may both result in a ``create``, although the result of one may be a match to the other.
   * Two conditional creates may both succeed, although the result of one may be a match to the other.
   * A conditional create and a simultaneous conditional update may both result in a ``create``, although the result of one may be a match to the other.

#. Parameter ``_pretty`` is not yet supported.
#. XML Patch and JSON Patch are not supported.

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

for the repositories:

* SQL
* SQLite
* Memory

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


.. _restful_search_limitations:

Limitations on search
^^^^^^^^^^^^^^^^^^^^^

The following parameters and options are not yet supported:

#. ``_text``
#. ``_content``
#. ``_query``
#. ``_containedType``
#. ``_filter``
#. ``:approx`` modifier on a quantity SearchParameter
#. ``:text`` modifier on a string SearchParameter
#. ``:above``, ``:below``, ``:in``, ``:not-in`` modifiers on a token SearchParameter
#. ``:above`` on a uri SearchParameter (``:below`` *is* supported)
#. ``*`` wildcard on ``_include`` and ``_revinclude``
#. ``_pretty``

Furthermore:

#. Paging is supported, but it is not isolated from intermediate changes to resources.

.. _restful_history:

History
-------

History is supported as described in the specification, on the system, type and instance level.
The ``_since`` and ``_count`` parameters are also supported.

Configuration
^^^^^^^^^^^^^
::

  "HistoryOptions": {
    "MaxReturnedResults": 100
  }

If a ``_history`` call would result in more than ``MaxReturnedResults``, Firely Server asks the user to be more specific.
Use this to avoid overloading the server or the connection.

.. _restful_history_limitations:

Limitations on history
^^^^^^^^^^^^^^^^^^^^^^

#. ``_at`` parameter is not yet supported.
#. Paging is supported, but it is not isolated from intermediate changes to resources.

.. _restful_batch:

Batch
-----

Batch is fully supported on the usual endpoint. You can limit the number of entries accepted in a single batch. See :ref:`batch_options`.

Note that batches are not supported in the ``/administration`` endpoint.

.. _restful_transaction:

Transaction
-----------

Transactions are supported, with these limitations:

#. Of the three storage implementations, only SQL Server and SQLite truly support transactions. On :ref:`MongoDB<configure_mongodb>` and :ref:`Memory<configure_memory>`, transaction support can be simulated at the FHIR level, but not be enforced on the database level.
#. References between resources in the transaction can point backwards or forwards. Only circular references are not supported. 
#. The ``/administration`` endpoint does not support transactions.

You can limit the number of entries accepted in a single transaction. See :ref:`batch_options`.

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
