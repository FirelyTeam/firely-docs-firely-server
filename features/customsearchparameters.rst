.. |br| raw:: html

   <br />


.. _feature_customsp:

Custom Search Parameters
========================

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
	
		POST http(s)://<firely-server-endpoint>/administration/reindex/all
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
	
		POST http(s)://<firely-server-endpoint>administration/reindex/searchparameters
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

.. _reindex_cosmosdb_warning:

.. warning::

	CosmosDB in its default configuration (and on the CosmosDB emulator) is fairly limited in its throughput. 
	If you encounter errors stating 'Request rate is large', you will have to:

	*	lower the MaxDegreeOfParallelism, 
	*	restart Firely Server 
	*	and start a the reindex operation again.

.. _feature_customsp_limitations:

Limitations
-----------

Every search parameter has to have either:

  * a valid FhirPath in it's Expression property, or
  * be a Composite search parameter and specify at least one component.

