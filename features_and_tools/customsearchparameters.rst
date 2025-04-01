.. |br| raw:: html

   <br />


.. _feature_customsp:

Custom Search Parameters
========================

.. note::

  The features described on this page are available in **all** :ref:`Firely Server editions <vonk_overview>`.

.. _feature_customsp_configure:

Configure Search Parameters
---------------------------

You can control which search parameters are known to Firely Server. This is managed in the same way as all the conformance resources, see :ref:`conformance`.

.. _feature_customsp_reindex:

Re-indexing for new or changed SearchParameters
-----------------------------------------------

Firely Server extracts values from resources based on the available search parameters upon create or update.
This means that if you already had resources in your database before adding a custom search parameter, 
those resources will not be indexed for that parameter. If you on the other hand removed a previously used 
search parameter, the index will contain superfluous data.

To fix that, you should re-index (repeat the extraction) for these parameters.

In short, both reindex operations below will:

*	Return an Operation Outcome stating that the reindex procedure was started successfully. 
*	Run the actual reindex asynchronously, using a configured number of threads, thereby using most of the hardware resources available to Firely Server.
*	Block any other requests for the duration of the reindex.
*	Log progress in the log.

.. caution:: This is a possibly lengthy operation, so use it with care. 
	
	*	Always try the reindex on a representative (sub)set of your data in a test environment to assess how long the operation may take in the production environment.
	*	Always make a backup of your data before performing a reindex.

.. warning:: During the re-index operation, all other operations are blocked and responded to with response code '423 - Locked'.

Reindexing and FHIR versions
^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Reindexing is also controlled by the fhirVersion parameter (see :ref:`feature_multiversion`) in the Accept header or the version-mapped endpoint. It will then reindex only for SearchParameters and resources *in that FHIR version*.
So for a full reindex of everything you may need to issue the command twice, once for each fhirVersion.

.. _feature_customsp_reindex_all:

Rebuild the whole search index
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

This is only needed if we changed something very significant to the way Firely Server searches, like

*	The way values are extracted for all or many searchparameters.
*	The structure in which Firely Server stores the search index.

To re-index all resources for all search parameters, use:

	::
	
		POST http(s)://<firely-server-endpoint>/administration/$reindex-all
		Accept=application/fhir+json (or xml); fhirVersion=3.0 (or 4.0)

This will delete any previously indexed data and extract it again from the resources.

.. _feature_customsp_reindex_specific:

Rebuild the search index for specific searchparameters
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

This is needed if:

*	The definition (usually the ``expression``) of a searchparameter has changed.
*	A searchparameter was added.
*	A searchparameter was removed and you want the search index to be tidy and not have this parameter in it anymore. 

To re-index all resources for certain search parameters, use:

	::
	
		POST http(s)://<firely-server-endpoint>administration/$reindex
		Accept=application/fhir+json (or xml); fhirVersion=3.0 (or 4.0)

In the body of the POST, you put the name of the search parameters to actually re-index as form parameters:

	::
	
		include=Patient.name,Observation.code
		exclude=Organization.name

``include`` means that resources will be re-indexed only for those search parameters.
You use this if you added or changed one or few search parameters.

``exclude`` means that any existing index data for those search parameters will be erased.
You use this when you removed a search parameter.

Remember to adjust the Content-Type header: ``application/x-www-form-urlencoded``.


If you are :ref:`not permitted <configure_administration_access>` to perform the reindex, Firely Server will return statuscode 403.

.. _feature_customsp_reindex_configure:

Re-index Configuration
^^^^^^^^^^^^^^^^^^^^^^

Firely Server will not re-index the resources in the database all at once, but in batches. The re-index operation will process all batches until all resources are re-indexed.
You can control the size of the batches in the :ref:`configure_appsettings`. 
Besides that you can also control how many threads run in parallel to speed up the reindex process. The configured value is a maximum, since Firely Server will also be limited by the available computing resources.
::

    "ReindexOptions": {
        "BatchSize": 100,
        "MaxDegreeOfParallelism": 10
    },

Use any integer value >= 1.

.. _feature_customsp_add:

Adding a New SearchParameter
----------------------------

Follow these steps to add a new `SearchParameter` to a running Firely Server instance:

1. **Create the SearchParameter Resource**  
   Define the `SearchParameter` resource. Ensure that it includes the required fields. For example:

   .. code-block:: json

      {
        "resourceType": "SearchParameter",
        "url": "http://example.org/fhir/SearchParameter/Patient-example",
        "name": "example",
        "description": "example description",
        "status": "active",
        "code": "example",
        "base": ["Patient"],
        "type": "string",
        "expression": "Patient.name"
      }

2. **Post the SearchParameter to the Administration API**  
   Use the Administration API to add the `SearchParameter` to Firely Server. Send a `POST` request to the following endpoint:

   .. code-block:: bash

      POST http(s)://<firely-server-endpoint>/administration/SearchParameter
      Content-Type: application/fhir+json

   Include the `SearchParameter` resource in the body of the request.

3. **Re-index the Resources**  
   After adding the `SearchParameter`, you need to re-index the resources in the database to ensure the new parameter is applied. Use the `$reindex` operation:

   .. code-block:: bash

      POST http(s)://<firely-server-endpoint>/administration/$reindex
      Content-Type: application/x-www-form-urlencoded

   In the body of the request, specify the `include` parameter with the name of the new `SearchParameter`:

   .. code-block:: text

      include=Patient.example

4. **Verify the SearchParameter**  
   Once the re-indexing is complete, verify that the new `SearchParameter` is working as expected by performing a search query using the parameter. For example:

   .. code-block:: bash

      GET http(s)://<firely-server-endpoint>/Patient?example=<value>

5. **Monitor Logs and Results**  
   Check the Firely Server logs for any errors or warnings during the process. Ensure that the search results match the expected behavior.

.. note::
   If you encounter any issues, ensure that the `SearchParameter` resource is valid and that the `expression` field correctly references the desired element in the FHIR resource.

.. _feature_customsp_limitations:

Limitations
-----------

Every search parameter has to have either:

  * a valid FhirPath in it's Expression property, or
  * be a Composite search parameter and specify at least one component.

