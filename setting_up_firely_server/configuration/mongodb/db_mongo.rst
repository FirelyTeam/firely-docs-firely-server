.. |br| raw:: html

   <br />

.. _configure_mongodb:

Using MongoDB
=============
We assume you already have MongoDB installed. If not, please refer to the `MongoDB download <https://www.mongodb.com/download-center>`_ pages.

Firely Server can work with MongoDb 4.0 and higher. Since Firely Server (Vonk) version 3.7.0 Firely Server uses the MongoDb Aggregation Framework heavily, and you are advised to upgrade to MongoDb 4.4 (or newer). 
In this version issue `SERVER-7568 <https://jira.mongodb.org/browse/SERVER-7568>` is solved, so the most selective index is used more often.

.. note:: 
    Using the correct Read and Write Concern for your MongoDb replica set is very important:

        * Write Concern: 
        
            #. For more information, check the MongoDb manual about `Write Concern <https://www.mongodb.com/docs/manual/reference/write-concern/>`_
            #. For PSA (Primary-Secondary-Arbiter) replica sets you would want the Write Concern to be `"w=1"`
            #. For PSS (Primary-Secondary-Secondary) replica sets you would want the Write Concern to be `"w=majority"`
        
        * Read Concern:

            #. For more information, check the MongoDb manual about `Read Concern <https://www.mongodb.com/docs/manual/reference/read-concern/>`_
            #. Default Read Concern is `local`.

* Navigate to your Firely Server working directory

* Changing a setting means overriding it as described in :ref:`configure_change_settings`. 

* Find the ``Repository`` setting:	
    
    ``"Repository": "SQLite",``

* Change the setting to ``MongoDb``

* If you have your own database in MongoDB already, change the ``MongoDbOptions`` to reflect your settings::

   "MongoDbOptions": {
       "ConnectionString": "mongodb://localhost/vonkdata",
       "EntryCollection": "vonkentries"
   },

* If MongoDB does not have a database and/or collection by this name, Firely Server will create it for you.

*   Find the section called ``PipelineOptions``. Make sure it contains the MongoDB repository in the root path for Firely Server Data::

        "PipelineOptions" : 
        {
            "Branches" : [
                "/" : { 
                    "Include" : [
                        "Vonk.Repository.MongoDb.MongoDbVonkConfiguration"
                        //...
                    ]
                }
            ]
        }

.. _configure_mongodb_admin:

Using MongoDB for the Administration API database
-------------------------------------------------

Although we encourage you to use :ref:`SQLite for Firely Server Administration <sqlite_admin_reasons>`, you can still use MongoDB for Firely Server Administration as well.

This works the same as with the normal Firely Server database, except that you:

*   put the settings within the ``Administration`` section

*   provide a different ConnectionString and/or EntryCollection, e.g.::

     "Administration": {
         "Repository": "MongoDB",
         "MongoDbOptions": {
             "ConnectionString": "mongodb://localhost/vonkadmin",
             "EntryCollection": "vonkadmin"
         }
     }

*   Find the section called ``PipelineOptions``. Make sure it contains the MongoDB repository in the administration path for Firely Server Administration::

        "PipelineOptions" : 
        {
            "Branches" : [
                "/administration" : { 
                    "Include" : [
                        "Vonk.Repository.MongoDb.MongoDbAdministrationConfiguration"
                        //...
                    ]
                }
            ]
        }

.. attention::

    For MongoDb it is essential to retain the ``.vonk-import-history.json`` file. Please read :ref:`vonk_conformance_history` for details.

.. _mongodb_transactions:

MongoDB Transactions
--------------------

.. note::
    When utilizing MongoDb transactions we strongly advise to use MongoDb v4.2 or higher.

In Firely Server versions prior to v4.9.0 transactions were simulated for development and test purposes. From Firely Server v4.9.0 and onwards transactions using MongoDb are now fully supported.

With MongoDb transactions, there are a few things to consider:

#. MongoDB supports transactions only for `Replica Sets` and `Sharded Clusters`. If you are running Firely Server on a MongoDb standalone instance you still will be able to upload a transaction bundle, but it will not be processed within a transaction. I.e.: if an exception occurs with a resource during processing the bundle, any previous resources will have been persisted to the database and not rolled back.
#. Firely Server currently uses transactions in the following cases:

    #. When uploading a transaction bundle.
    #. When performing a conditional delete that targets more than one resource.
    #. When using the X-Provenance header.

#. MongoDb transactions in Firely Server always use Read Concern `"snapshot"` and Write Concern `"majority"`.
#. MongoDb imposes a transaction runtime limit of `60s`. For self-hosted MongoDb instances you can modify this limit using `"transactionLifetimeLimitSeconds"`. However, for MongoDb Atlas deployments this limit cannot be changed. 
#. Although MongoDb transactions are supported as early as v4.0, please be aware of the following issue. In MongoDb v4.0 all write operations are contained in a single oplog entry. The oplog entry for the transaction must be within the BSON document size limit of 16MB. For v4.2+ every write operation gets its own oplog entry. This removes the 16MB total size limit for a transaction imposed by the single oplog entry for all its write operations. Note that each single oplog entry still has a limit of 16 MB. We highly recommend in using MongoDb v4.2 or higher when using transactions.
#. Please read the official MongoDb documentation for production considerations when using transactions: `MongoDb manual <https://www.mongodb.com/docs/manual/core/transactions-production-consideration/>`_

Tips and hints for using MongoDb for Firely Server
--------------------------------------------------

#. If searches and/or creates and updates are excessively slow, you may be limited by the IOPS on your MongoDb deployment (e.g. MongoDb Atlas). Try upgrading it and check the timings again.
#. If for any reason you would like to see how Firely Server is interacting with MongoDb, make the following adjustments to the :ref:`configure_log`:

    #. In the section ``Serilog.MinimumLevel.Override`` add ``"Vonk.Repository.DocumentDb": "Verbose"``. Add it before any broader namespaces like ``Vonk``.
    #. In the section on the File sink, change the ``restrictedToMinimumLevel`` to ``Verbose``.

#. With regards to Firely Server version and MongoDB version:
    #. If you are on a Firely Server (Vonk) version < v3.6, you can keep using MongoDB v4.0 or higher.
    #. If you are on Firely Server (Vonk) v3.6 or higher and are unable to migrate to MongoDB 4.4 (relatively soon), please contact us if you need assistance.

.. _mongodb_diskspace:

MongoDB disk space requirements
-------------------------------

A MongoDB database has no single size, but several characteristics that tell us something about the size of the data:

:nr of documents: This is not indicative of any storage size, because documents can be sized very differently.
:document size: The size in bytes of all the documents in a database, uncompressed.
:index size: The size in bytes of all the indexes in a database, uncompressed.
:storage size: The actual space used for storage of the documents and indexes. Since the storage engine of MongoDB (WiredTiger) compresses data by default, this is often substantially less than the document and index sizes combined.
:disk space used: The size of the database file on disk. Because data can become fragmented, this is larger than the storage size.

The sizes that are relevant for estimating the disk space requirements are the storage size and the disk space used. At Firely we host a test instance on a MongoDB Atlas M40 cluster, with 1 primary and 2 replicas. 
Based on that we can calculate these sizes:

#. storage size: ~ 1 GB per 1.000.000 (1 mln) resources
#. disk size: ~ 2 times the storage size, so 2 GB per 1 mln resources, *per replica*, allowing for a fragmentation ratio up to 50%
#. buffer: 20% of disk size (see below)

Storage size may differ based on the size of the resources you host. These estimations are based on Synthea patient records. But e.g. ExplanationOfBenefit resources are typically much larger than the average Observation resource.

Fragmentation in MongoDB occurs when data is deleted. New data is appended at the end of the data file. Firely Server will delete data upon update, delete and $erase operations. 
This means that you should account for more or less fragmentation based on how many of these operations you expect will be performed on your Firely Server instance.
If fragmentation gets too high, you can `compact <https://www.mongodb.com/docs/manual/reference/command/compact/>`_ the collection with resources (by default named ``vonkentries``).

On top of the requirements for storing the resources and indexes, we allow MongoDB to use the disk as an overflow buffer for larger-than-memory operations (like sorting a very large resultset). Please reserve about 20% extra disk space for that.

We recommend to monitor the health of your MongoDB cluster actively to avoid disk space issues.

MongoDB Guidelines
---------------------

In this section we offer general guidelines on how to configure and maintain your MongoDB database. This topic only covers the most common options and should by no means be regarded as exhaustive.

.. toctree::
   :maxdepth: 1
   :titlesonly:

   database_security