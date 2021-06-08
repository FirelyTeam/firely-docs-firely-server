.. |br| raw:: html

   <br />

.. _configure_cosmosdb:

Using Microsoft Azure CosmosDB
==============================
You can connect Firely Server to CosmosDB the same way you connect to MongoDB. There are a few limitations that we will work out later. They are listed below.

.. attention::

   You cannot use CosmosDb for the Firely Server Administration database. Use :ref:`SQLite <configure_sqlite_admin>` instead.

#. Create a CosmosDB account on Azure, see the `Quickstart Tutorial <https://docs.microsoft.com/en-us/azure/cosmos-db/>`_
#. Make sure you choose the MongoDB API
#. In the Azure Portal, open your CosmosDB account and go to the 'Connection Strings' blade. Copy the 'Primary Connection String' to your clipboard.

#. Now on your own machine, navigate to your Firely Server working directory
#. Changing a setting means overriding it as described in :ref:`configure_change_settings`. 

#. Find the ``Repository`` setting::

	"Repository": "Sqlite",

#. Change the setting to ``CosmosDb``

#. If you have your own database in CosmosDB already, change the ``CosmosDbOptions`` to reflect your settings::

        "CosmosDbOptions": {
            "ConnectionString": "<see below>",
            "EntryCollection": "vonkentries",
            "SimulateTransactions": "false"
        },

   Paste the ConnectionString from step 3, and add the databasename that you want to use. The connectionstring looks like this::

      mongodb://<accountname>:<somerandomstring>==@<accountname>.documents.azure.com:10255?ssl=true&replicaSet=globaldb

   You can add the databasename after the portnumber, like this::

      mongodb://<accountname>:<somerandomstring>==@<accountname>.documents.azure.com:10255/vonk?ssl=true&replicaSet=globaldb

#. If your CosmosDB account does not have a database or collection by this name, Firely Server will create it for you.

#. You can set SimulateTransactions to "true" if you want to experiment with `FHIR transactions <https://www.hl7.org/fhir/http.html#transaction>`_.
   Firely Server does not utilize the CosmosDB way of supporting real transactions across documents, so in case of an error already processed entries will NOT be rolled back. 

.. _configure_cosmosdb_limitations:

CosmosDB Request Units
----------------------

If you upload a lot of data in a short time (as is done on :ref:`reindexing <feature_customsp_reindex>`), you quickly exceed the default maximum of 1000 Request Units / second.
If you encounter its limits, the Firely Server log will contain errors stating 'Request rate is large'. 
This is likely to happen upon :ref:`reindexing <feature_customsp_reindex>` or when using :ref:`Vonkloader <vonkloader_index>`.
Solutions are:

*   Raise the limit to at least 5000 RU/s. See the `Microsoft documentation <https://docs.microsoft.com/en-us/azure/cosmos-db/set-throughput#provision-throughput-by-using-azure-portal>`_ for instructions.
*   Lower the load

    *	on Reindexing, lower the MaxDegreeOfParallelism, see :ref:`this warning <reindex_cosmosdb_warning>`
    *	with Vonkloader, lower the value of the -parallel parameter. 

Limitations
-----------

#.  Request size for insertions to CosmosDB is limited to around 5 MB. Some bundles in the examples from the specification exceed that limit. Then you will get an error stating 'Request size too large'.
    You can avoid this by limiting the size of incoming resources in the :ref:`SizeLimits <sizelimits_options>` setting.
#.  The CosmosDB implementation of the MongoDB API is flawed on processing ``$not`` on arrays. This inhibits the use of these searches in Firely Server:
   
    *   Using the ``:not`` modifier
    *   Using ``:missing=true``


