.. _tool_fsi:

Bulk Import via Firely Server Ingest
====================================

.. note::

  The features described on this page are available in **all** :ref:`Firely Server editions <vonk_overview>`.

.. note::
  This application is licensed separately from the core Firely Server distribution. Please :ref:`contact<vonk-contact>` Firely to get the license. 
  Your license already permits the usage of FSI if it contains ``http://fire.ly/vonk/plugins/bulk-data-import``. You can also try out Firely Server Ingest with an Evaluation license. It is limited to a maximum of 10000 resources in total in the connected Firely Server database with a maximum number of 1000 resources that can be loaded per run, in addition to the Recovery Journal feature being disabled. For the production licenses the following behavior applies:
  
  #. **Firely Essentials**
    * Contains the ``http://fire.ly/vonk/plugins/bulk-data-import`` token
    * Is unrestricted in the amount of resources that can be loaded in total
    * Restricts the amount of resources that can be loaded in one go to 1000
    * Does not support the Recovery Journal feature
  #. **Firely Scale**
    * Contains the ``http://fire.ly/vonk/plugins/bulk-data-import/unlimited`` token
    * Is unrestricted in the amount of resources that can be loaded in total
    * Is unrestricted in the amount of resources that can be loaded in one go 
    * Supports the Recovery Journal feature
  #. **Firely Solution for CMS Interoperability & Prior Authorization Final Rule**
    * Contains the ``http://fire.ly/vonk/plugins/bulk-data-import/unlimited`` token
    * Is unrestricted in the amount of resources that can be loaded in total
    * Is unrestricted in the amount of resources that can be loaded in one go
    * Supports the Recovery Journal feature
    
**Firely Server Ingest (FSI)** is a CLI application designed to optimize massive resource ingestion into a Firely Server instance. In contrast to resource ingestion by sending HTTP requests to a running Firely Server instance, this tool writes data directly to the underlying FS database which increases the throughput significantly.

The tool supports ingestion into SQL Server and MongoDB databases or ingestion through the :ref:`PubSub <PubSub>` mechanism.

.. _tool_fsi_installation:

Installation
------------
To install the tool, you first need to have .NET Core SDK v8.x installed on your computer. You can download it `here <https://dotnet.microsoft.com/en-us/download>`__. Once it is installed, execute the following command:

::

  dotnet tool install --global Firely.Server.Ingest

You can update to the latest version using the following command:

::

  dotnet tool update --global Firely.Server.Ingest

The commands above will install FSI from this `NuGet package <https://www.nuget.org/packages/Firely.Server.Ingest/>`_.

.. note::

  Make sure that the dotnet tools directory is added to your path, this makes it possible to run the ``fsi`` command from any directory.

    - Please find more information on where globally installed tools are located in `this article <https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools#install-a-global-tool>`_. 
    - For Linux and Mac, make sure you add your ``.profile`` or ``.bash_profile`` to your path.
    - In the Firely Essentials edition, FSI is limited to importing 1000 resource per batch. It is required to split up larger datasets into multiple batches and re-run FSI.

General usage
-------------

.. attention::

  * Firely Server instances targeting the same database will be impacted severely by the workload that FSI puts on the database. We advise to stop the Firely Server instances while the import is performed.
  * Only one instance of FSI per database should be run at a time. FSI can utilize all the cores on the machine it is run on, and insert data over several connections to the database in parallel. Multiple instances would probably cause congestion in the database.
  * FSI does not add tenant security labels, see :ref:`feature_multitenancy`.

Prerequisites
^^^^^^^^^^^^^

.. note::

  This prerequisite does not apply to FSI v6+ targeting a MongoDB database. In this case you can instruct FSI to provision the database automatically by setting the ``--provisionTargetDatabase`` flag to ``true``.
  This prerequisite also does not apply to FSI using PubSub as a target. In this case the consuming Firely Server instance(s) will take care of the database setup.

The tool requires that the target database already exists and contains all required indexes and tables (for SQL Server). If you don't have a database with the schema yet, you first need to run the Firely Server at least once as described in the articles :ref:`configure_sql` and :ref:`configure_mongodb`.

Each version of Firely Server Ingest is bound to a specific version of Firely Server.
Starting from FS version 5.5.0, the FSI version number aligns with the FS version number.

.. container:: toggle

    .. container:: header

      Expand to see the matching FSI versions for older FS releases

    The following list shows which combinations of Firely Server (its database schema version respectively) and Firely Server Ingest can be used in combination.

    * **FS 5.5.0 and later**: FSI v5.5.0 and later
    * **FS v5.1.0 - v5.4.0**: FSI v2.2.0 or v2.2.1
    * **FS v5.0.0**: FSI v2.1.0
    * **FS v5.0.0-beta1**: FSI v2.0.0
    * **FS v4.10.0 and later**: FSI v1.4.0
    * **FS v4.9.0**: FSI v1.3.0
    * **FS v4.8.0**: FSI v1.2.0
    * **FS v4.2.0 and later**: FSI v1.1.0
    * **FS v4.2.0**: FSI v1.0.0

Input files formats
^^^^^^^^^^^^^^^^^^^

FSI supports the following input file formats:

* FHIR *collection* bundles stored in ``*.json`` files, and
* ``*.ndjson`` files where each line contains a separate FHIR resource in JSON format.


After the import
^^^^^^^^^^^^^^^^

After ingesting massive amounts of data, it is important to make sure the SQL Server indexes are in good shape. You can read more on this topic here: :ref:`sql_index_maintenance`.

Arguments
---------

The execution of FSI can be configured using input parameters. These parameters can be supplied either as CLI arguments or specified in the file ``appsettings.instance.json`` which must be created in the same directory as the ``fsi`` executable.

If you want to specify input parameters in the file, you can use the snippet below as a base for your ``appsettings.instance.json``. In this case, you need to update the values that you want to set yourself and delete all other records.


.. container:: toggle

    .. container:: header

      Click to expand the appsettings.instance.json template file

    .. code-block:: JavaScript

      {
          // General
          "license": "C:\\data\\deploy\\vonk\\license\\performance-test-license.json",

          // Import settings
          "limit": -1,
          "fhirVersion": "R4",
          "updateExistingResources": true,
          "haltOnError": false,
          "recoveryJournalDirectory": null,

          "absoluteUrlConversion": {
              "baseEndpoints": [
                  // "http://localhost:4080/R4"
              ],
              "elements": [
                  "DocumentReference.content.attachment.url"
              ]
          },

          "workflow": { //-1 = unbounded
              "readBufferSize": 750,
              "metaParallel": 1,
              "metaBufferSize": 50,
              "typeParallel": 4,
              "typeBufferSize": 50,
              "absoluteToRelativeParallel": 1,
              "absoluteToRelativeBufferSize": 50,
              "indexParallel": -1, //this is usually the most time consuming process - give it as much CPU time as possible.
              "indexBufferSize": 50
          },

          // Source
          "sourceType": "Filesystem", // Filesystem | MongoDb | None, None will try only to provision the target database
          "source": "./fsi-source", // source directory when Filesystem source is used
          "mongoDbSource": {
              "connectionString": "<connectionstring to the Firely Server MongoDb source database>",
              "collectionName": "vonkentries",
              "runningMode": "AdHoc",
              "documentFilterBson": "{ }" // See https://www.mongodb.com/docs/manual/reference/operator/aggregation/match/ for the syntax
          },

          // Target
          "provisionTargetDatabase": false,
          "databaseType": "SQL", // SQL | MongoDb | PubSub

          "sqlserver": {
              "connectionString": "<connectionstring to the Firely Server SQL Server database>",
              "saveParallel": 2,
              "queryExistenceParallel": 4,
              "batchSize": 500,
              "commandTimeOut": 60 //seconds
          },

          "mongodb": {
              "entryCollection": "vonkentries",
              "connectionString": "<connectionstring to the Firely Server MongoDb database>",
              "saveParallel": 2,
              "queryExistenceParallel": 4,
              "batchSize": 500
          },

          "PubSub": {
            "batchSize": 1,
            "MessageBroker": {
                "Host": "<connectionstring to the pubsub endpoint>",
                "Username": "guest",
                "Password": "guest",
                "ApplicationQueueName": "FirelyServer",
                "PrefetchCount": 1,
                "ConcurrencyNumber": 1,
                "RabbitMQ": {
                    "Port": 5672
                },
                "Kafka": {
                    "TopicPrefix": "FirelyServerCommands",
                    "ClientGroupId": "FirelyServer",
                    "ClientId": "FirelyServer",
                    "AuthenticationMechanism": "SaslScram256",
                    "ExecuteStorePlanCommandErrorTopicName": "FirelyServerCommands.ExecuteStorePlanCommand.Errors",
                    "NumberOfConcurrentConsumers": 1,
                    "Username": "username",
                    "Password": "password",
                    "CaLocation": "", // path to ca file
                    "KeystoreLocation": "", // path to private key file
                    "KeystorePassword": "" // password to private key file
                },
                "VirtualHost": "/",
                "BrokerType": "AzureServiceBus" //  RabbitMq, AzureServiceBus, Kafka
            },
            "ResourceChangeNotifications": {
                "ExcludeAuditEvents": false,
                "SendLightEvents": false,
                "SendFullEvents": false,
                "PollingIntervalSeconds": 5,
                "MaxPublishStorePlanSize": 1000
            },
            "ClaimCheck": {
                "StorageType": "Disabled", //"AzureBlobStorage", // Or "Disabled"
                "AzureBlobContainerName": "messages-data",
                "AzureBlobStorageConnectionString": "<connection string>"
            }
        },

        // Telemetry
        "OpenTelemetryOptions": {
            "EnableMetrics": false,
            "Endpoint": "http://localhost:4317"
        }
      }

.. _FSI_supported_arguments:

General
^^^^^^^

* ``--license <license>``: 

  * **Config**: license
  * **Required**: Yes
  * **Description**: Firely Server license file.

* ``-l``, ``--limit <limit>``: 

  * **Config**: limit
  * **Required**: No
  * **Default**: -1 (no limit)
  * **Description**: Limit the number of resources to import. Use this for testing your setup.

* ``-f``, ``--fhir-version <R3|R4>``: 

  * **Config file parameter**: ``fhirVersion``
  * **Required**: Yes
  * **Description**: Specifies the FHIR version of the input data.

* ``--update-existing-resources <true|false|onlyIfNewer>``: 

  * **Config**: updateExistingResources
  * **Required**: No
  * **Default**: true
  * **Description**: Defines the action to take when a resource with a given Type and Id already exists in the target database.
  * **Options**:

    * **true**: the existing resource gets marked as historical and the incoming resource gets saved as current
    * **false**: the existing resource remains unchanged; the incoming resource gets logged as skipped
    * **errorOnConflict**: the existing resource remains unchanged; the incoming resource errors out
    * **onlyIfNewer**:

      * MongoDb: if an existing resource has a ``meta:LastUpdated`` greater than the incoming resource, the incoming resource gets saved as historical and the existing resource remains unchanged.
      * SQL Server: if an existing resource has a ``meta:LastUpdated`` greater than the incoming resource, the incoming resource gets skipped. 
      * Note: when this mode is used, the incoming resources must have the ``meta:LastUpdated`` field set.

* ``--haltOnError <true|false>``: 

  * **Config**: haltOnError
  * **Required**: No
  * **Default**: false
  * **Description**: When true, stop application on a single error

* ``--useRecoveryJournal <recoveryJournalDirectory>``: 

  * **Config**: recoveryJournalDirectory
  * **Required**: No
  * **Default**: null
  * **Description**: A directory containing the recovery journal. See :ref:`Recovery Journal<tool_fsi_recovery>`.

* ``--urlConvBases:index url``: 

  * **Config**: absoluteUrlConversion/baseEndpoints
  * **Required**: No
  * **Default**: None
  * **Description**: Convert absolute URLs to relative for endpoints included in this array. The array values must match exactly the base URL otherwise no changes are made.

* ``--urlConvElems:index FHIRPath``: 

  * **Config**: absoluteUrlConversion/elements
  * **Required**: No
  * **Default**: None
  * **Description**: List of FHIR paths specifying the list of ``Uri`` or ``Url`` elements that should be converted from absolute to relative URI if their base endpoints match one of the base endpoint specified in ``absoluteUrlConversion/baseEndpoints``.

Source
^^^^^^


* ``--sourceType <Filesystem|MongoDb|None>``: 

  * **Config**: sourceType
  * **Required**: No
  * **Default**: Filesystem
  * **Description**: Specifies the source type
  * **Options**:

    * **Filesystem**: read data from the filesystem
    * **MongoDb**: read data from a Firely Server MongoDB database 
    * **None**: use this option if you only want to provision the target database

Source (for Filesystem)
^^^^^^^^^^^^^^^^^^^^^^^

* ``-s``, ``--source <source>``: 

  * **Config**: source
  * **Required**: Yes when ``sourceType`` is set to ``Filesystem``
  * **Description**: Input directory for work (this directory is visited recursively including all the subdirectories).

Source (for MongoDb)
^^^^^^^^^^^^^^^^^^^^

This source is intended to be used in zero-downtime migration scenarios. Currently, it is only possible to use another MongoDB database as the target database.

See more information on how to run migrations in :ref:`this article <zero_downtime_migration>`.

* ``--srcMongoConnectionString <srcMongoConnectionString>``: 

  * **Config**: mongoDbSource/connectionString
  * **Required**: Yes
  * **Description**: Connection string to read resources from.

* ``--srcMongoCollection <srcMongoCollection>``: 

  * **Config**: mongoDbSource/collectionName
  * **Required**: No
  * **Default**: vonkentries
  * **Description**: Collection name to read entries from.

* ``--srcMongoRunningMode <AdHoc|Continuous>``: 

  * **Config**: mongoDbSource/runningMode
  * **Required**: No
  * **Default**: AdHoc
  * **Description**: The mode in which the application should run.
  * **Options**:

    * **AdHoc**: the application will run once and exit
    * **Continuous**: the application will run continuously and listen for changes in the source database until terminated by the user

* Documents filter (can be set only via the config file): 

  * **Config**: mongoDbSource/documentFilterBson
  * **Required**: No
  * **Default**: None
  * **Description**: BSON filter to apply when reading documents. See `MongoDB aggregation match syntax <https://www.mongodb.com/docs/manual/reference/operator/aggregation/match/>`_ for details.


Target
^^^^^^

* ``--dbType <MongoDb|SQL>``: 

  * **Config**: databaseType
  * **Required**: No
  * **Default**: SQL
  * **Description**: Specifies the target database type.
  * **Options**:

    * **SQL**
    * **MongoDb**


* ``--provisionTargetDatabase <true|false>``: 

  * **Config**: provisionTargetDatabase
  * **Required**: No
  * **Default**: false
  * **Description**: Whether to provision the target database. *Note: currently only supported for MongoDB.*

Target (for SQL Server)
^^^^^^^^^^^^^^^^^^^^^^^

* ``-c``, ``--connectionstring <connectionstring>``: 

  * **Config**: sqlServer/connectionString
  * **Required**: Yes
  * **Description**: Connection string to Firely Server SQL Server database.

* ``--sqlPar <sqlPar>``: 

  * **Config**: sqlServer/saveParallel
  * **Required**: No
  * **Default**: 2
  * **Description**: The number of batches to save in parallel. Depends on your bandwidth to SQL Server and its processing power.

* ``--sqlBatch <sqlBatch>``: 

  * **Config**: sqlServer/saveBatchSize
  * **Required**: No
  * **Default**: 500
  * **Description**: The number of resources to save in each batch. SQL Server must be able to process it within the CommandTimeout. It is recommended to set this value to at least 500 for optimal performance.

* ``--sqlTimeout <sqlTimeout>``: 

  * **Config**: sqlServer/commandTimeOut
  * **Required**: No
  * **Default**: 60
  * **Description**: The time SQL Server is allowed to process a batch of resources.

* ``--sqlExistQryPar <sqlExistQryPar>``: 

  * **Config**: sqlserver/queryExistenceParallel
  * **Required**: No
  * **Default**: 4
  * **Description**: The number of parallel threads querying the DB to check whether a resource exists (only when ``--update-existing-resources`` is set to false).

Target (for MongoDB)
^^^^^^^^^^^^^^^^^^^^

* ``--mongoConnectionstring <connectionstring>``: 

  * **Config**: mongodb/connectionString
  * **Required**: Yes
  * **Description**: Connection string to Firely Server MongoDb source database.

* ``--mongoCollection <mongoCollection>``: 

  * **Config**: mongodb/entryCollection
  * **Required**: No
  * **Default**: vonkentries
  * **Description**: Collection name for entries.

* ``--mongoPar <mongoPar>``: 

  * **Config**: mongodb/saveParallel
  * **Required**: No
  * **Default**: 2
  * **Description**: The number of batches to save in parallel. Depends on your bandwidth to MongoDb and its processing power.

* ``--mongoExistQryPar <mongoExistQryPar>``: 

  * **Config**: mongodb/queryExistenceParallel
  * **Required**: No
  * **Default**: 4
  * **Description**: The number of parallel threads querying the DB to check whether a resource exists (only when ``--update-existing-resources`` is set to false).

* ``--mongoBatch <mongoBatch>``: 

  * **Config**: mongodb/batchSize
  * **Required**: No
  * **Default**: 500
  * **Description**: The number of resources to save in each batch.

Target (for PubSub)
^^^^^^^^^^^^^^^^^^^

PubSub options are not exposed trough command line parameters and must be provided in a ``appsettings.instance.json`` file as described above.
The settings, except for the batchSize, are the same as in Firely Server and can be found in :ref:`this article <pubsub_configuration>`.

Workflow
^^^^^^^^

* ``--readBuffer <readBuffer>``: 

  * **Config**: workflow/readBufferSize
  * **Required**: No
  * **Default**: 750
  * **Description**: Number of resources to buffer after reading.

* ``--metaPar <metaPar>``: 

  * **Config**: workflow/metaParallel
  * **Required**: No
  * **Default**: 1
  * **Description**: Number of threads to assign metadata. Should be higher than ReadParallel.

* ``--metaBuffer <metaBuffer>``: 

  * **Config**: workflow/metaBufferSize
  * **Required**: No
  * **Default**: 50
  * **Description**: Number of resources to buffer for assigning metadata.

* ``--typePar <typePar>``: 

  * **Config**: workflow/typeParallel
  * **Required**: No
  * **Default**: 4
  * **Description**: Number of threads to add type information. Should be higher than ReadParallel.

* ``--typeBuffer <typeBuffer>``: 

  * **Config**: workflow/typeBufferSize
  * **Required**: No
  * **Default**: 50
  * **Description**: Number of resources to buffer for adding type information.

* ``--absRelPar <absRelPar>``: 

  * **Config**: workflow/absoluteToRelativeParallel
  * **Required**: No
  * **Default**: 1
  * **Description**: Number of threads when converting absolute to relative references. Should be higher than ReadParallel.

* ``--absRelBuffer <absRelBuffer>``: 

  * **Config**: workflow/absoluteToRelativeBufferSize
  * **Required**: No
  * **Default**: 50
  * **Description**: Number of resources to buffer when converting absolute to relative references.

* ``--indexPar <indexPar>``: 

  * **Config**: workflow/indexParallel
  * **Required**: No
  * **Default**: -1 (no limit)
  * **Description**: Number of threads to index the search parameters. This is typically the most resource-intensive step and should have the most threads.

* ``--indexBuffer <indexBuffer>``: 

  * **Config**: workflow/indexBufferSize
  * **Required**: No
  * **Default**: 50
  * **Description**: Number of resources to buffer for indexing the search parameters.


Telemetry
^^^^^^^^^

* ``--OpenTelemetryOptions/EnableMetrics <true|false>``: 

  * **Config**: OpenTelemetryOptions/EnableMetrics
  * **Required**: No
  * **Default**: false
  * **Description**: Enable or disable OpenTelemetry metrics.

* ``--OpenTelemetryOptions/Endpoint <endpoint>``: 

  * **Config**: OpenTelemetryOptions/Endpoint
  * **Required**: No
  * **Default**: http://localhost:4317
  * **Description**: OpenTelemetry endpoint for metrics.

Other
^^^^^

* ``--version``: 

  * **Required**: No
  * **Description**: Show version information.

* ``-?``, ``-h``, ``--help``: 

  * **Required**: No
  * **Description**: Show help and usage information.


.. _tool_fsi_examples:

Examples
--------

Specify a custom settings file **/path/to/your/custom/settings/appsettings.instance.json**.

.. code-block:: bash

  fsi --settings ./path/to/your/custom/settings/appsettings.instance.json 

.. note::
  If ``--settings`` is omitted, FSI searches the following folders sequentially and tries to find ``appsettings.instance.json``. The first occurrence will be used if FSI finds one, otherwise the default ``appsettings.json`` will be used.  
  
  * Current launched folder |br| 
    e.g. ``C:\Users\Bob\Desktop``  
  * FSI installation folder |br|
    e.g. ``C:\Users\Bob\.dotnet\tools``  
  * FSI installation ``dll`` folder |br| 
    e.g. ``C:\Users\Bob\.dotnet\tools\.store\firely.server.ingest\version\firely.server.ingest\version\tools\net8.0\any``

Run the import for files located in directory **/path/to/your/input/files** and its subdirectories using license file **/path/to/your/license/fsi-license.json** targeting the database defined by the connection string. In case a resource being imported already exists in the target database, it gets skipped.

.. code-block:: bash

  fsi \
  -s ./path/to/your/input/files \
  --license /path/to/your/license/fsi-license.json \
  -c 'Initial Catalog=VonkData;Data Source=server.hostname,1433;User ID=username;Password=PaSSSword!' \
  --update-existing-resources false 

Same as above but if a resource being imported already exists in the target database, it gets updated. The old resource gets preserved as a historical record.

.. code-block:: bash

  fsi \
  -s ./path/to/your/input/files \
  --license /path/to/your/license/fsi-license.json \
  -c 'Initial Catalog=VonkData;Data Source=server.hostname,1433;User ID=username;Password=PaSSSword!'

Same as above but targeting a MongoDB database.

.. code-block:: bash

  fsi \
  --dbType MongoDb
  -s ./path/to/your/input/files \
  --license /path/to/your/license/fsi-license.json \
  --mongoConnectionstring 'mongodb://username:password@localhost:27017/vonkdata'

.. _tool_fsi_packages_cache:

Packages cache
--------------
Upon its first execution, FSI requires internet access to download and cache packages with core FHIR conformance resources (such as StructureDefinitions and SearchParameters, etc.) The internet connection is not required for the subsequent runs. 

It is possible to copy the cached files from one computer to another. It is also possible to mount the cached files to a Docker container if you run FSI in Docker.

The cached files can be found in the following locations:

* for v. ≥ v2.2.1

  * Windows: ``%USERPROFILE%\.fhir\packages``
  * Linux/MacOS: ``$HOME/.fhir/packages``
* for v. ≥ v1.4.1
  
  * Windows: ``%APPDATA%\.fhir_packages``
  * Linux/MacOS: ``$XDG_CONFIG_HOME/.fhir_packages`` if the environment variable ``XDG_CONFIG_HOME`` is defined  otherwise ``$HOME/.config/.fhir_packages``

.. _tool_fsi_recovery:

Recovery Journal
----------------

If a transient error occurs while ingestion is running or the FSI instance gets interrupted, the *recovery journal* feature allows recovery from such a situation. To enable it, use the ``--useRecoveryJournal <recoveryJournalDirectory>`` option in the CLI or set field ``recoveryJournalDirectory`` in the ``appsettings.instance.config``. 

When enabled, the process runs as follows:

#. Upon the first ingestion attempt, FSI will take a snapshot of all the files in the specified source directory and save that snapshot to the ``<recoveryJournalDirectory>``.
#. Then the data ingestion will start. Information about every successfully ingested resource also gets added to the journal.

If the ingestion procedure gets interrupted at any point, or some of the resources do not get ingested because of a transient error (e.g. network connection to the target DB is temporarily down), the ingestion process can be restarted by running the application with the same parameters. The application will skip all the previously ingested resources based on the journal.
.. note::
  
  - Note that the recovery journal directory must be empty before performing the initial ingestion attempt for a given set of files. 
  - Furthermore, the source files must not be changed between ingestion attempts. If any changes are detected, the FSI will throw an error.

.. note::
  
  Please do not use the source directory or any subdirectories within the source directory as the recovery journal directory.

Monitoring
----------

Logs
^^^^

When importing the data, it is handy to have the logging enabled, as it would capture any issues if they occur. By default, the log messages are written both to the console window and to the log files in the ``%temp%`` directory.

You can configure the log settings the same way as you do for Firely Server: :ref:`configure_log`. 

.. _tool_fsi_performance_counters:

Performance counters
^^^^^^^^^^^^^^^^^^^^
You can get insights into the tool performance by means of performance counters. There are many ways to monitor the performance counters. One of the options is using `dotnet-counters <https://docs.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-counters>`_.

To monitor the counters for FSI, you can execute the following command:

::

  dotnet-counters monitor --counters 'System.Runtime','FSI Processing'  --process-id <process_id>

where *<process_id>* is the PID of the running FSI tool.

.. note::

  If you think the ingestion process is going too slow for your amount of data and the hardware specifications, please :ref:`contact us<vonk-contact>` for advice.


Known issues
------------

* FSI does not support scenarios where resources of different FHIR versions are stored in the same database;
  
  * Please note that FSI will not check or warn you if the database already contains resources of a FHIR version different from that specified via the CLI options ``-f``, ``--fhir-version <R3|R4>`` or ``fhirVersion`` in the config file.

* When importing STU3 resources, the field ``Patient.deceased`` will always be set to ``true`` if it exists. This is caused by an error in the FHIR STU3 specification. In case you would like to use FSI with STU3 resources, please :ref:`contact us<vonk-contact>`.
* If a resource is present in a workload more than once, the entries may get processed in parallel and a version that is different from the latest may be set as current.


Release notes
-------------

Release 5.5.0+
^^^^^^^^^^^^^^

The FSI release cycle has been synchronized with the Firely Server release cycle.
Please refer to the :ref:`Firely Server release notes <vonk_releasenotes>` for the FSI change log.


.. container:: toggle

    .. container:: header

      Changelog before Firely Server 5.5.0

    **Release 2.3.0, November 23rd, 2023**

    * Feature: the mode ``--update-existing-resources onlyIfNewer`` is now supported for MongoDB.
    * Feature: ``Serilog.Sinks.MongoDB`` was added to the list of supported log sinks.
    * Fix: the ``SqlClient`` dependency package has been updated to version v5.1.1 to address the vulnerability: CVE-2022-41064.
    * Fix: the rare exception ``System.InvalidOperationException: Cannot change state from Skipped to Error`` does not get thrown anymore.
    * Internal: the way of handling command line arguments has been refactored.

    **Release 2.2.1, September 19th, 2023**

    * Added support for running FSI without the internet connection (see :ref:`tool_fsi_packages_cache`)
    * This release includes a new setting for handling the conversion of absolute to relative references: ``absoluteUrlConversion``. This setting replaces the old ``convertAbsoluteUrlsToRelative`` setting. With this setting you can specify the FHIR Path of the elements that you would like to see converted. See also the ``urlConvBases:index url`` and ``urlConvElems:index FHIRPath`` arguments in the :ref:`FSI_supported_arguments` section for more information.
      ::
      
        "absoluteUrlConversion": {
          "baseEndpoints": [
            // "http://localhost:4080/R4"
          ],
          "elements": [
            "DocumentReference.content.attachment.url"
          ]
        }

    **Release 1.4.1, August 28th, 2023**

    .. note::
      It is a hotfix release for the latest FSI that supports Firely Server v.4

    * Added support for running FSI without the internet connection (see :ref:`tool_fsi_packages_cache`)

    **Release 2.2.0, June 20th, 2023**

    * Fix: Composite parameters are more accurately indexed for SQL Server, to align with Firely Server 5.1.0. See :ref:`vonk_releasenotes_5_1_0` and the accompanying warnings.
    * Feature: FSI is now open to evaluation, just like Firely Server itself. It is limited though, to a maximum of 10.000 resources in the database, including history.
    * Feature: FSI is updated to Firely .NET SDK 5.1.0, see `its releasenotes <https://github.com/FirelyTeam/firely-net-sdk/releases/tag/v5.1.0>`_

    **Release 2.1.0, March 9th, 2023**

    * Fix: Eliminated deadlocks in FSI when writing data in parallel.
    * Settings: The setting ``maxActiveResources`` and the related CLI argument ``--maxActiveRes`` are no longer needed and have been removed.

    **Release 2.0.1, February 12th, 2023**

    * Fix: Add support for schema version 25 for MongoDb

    **Release 2.0.0, January 26th, 2023**

    * Upgraded to work with the database schemas for :ref:`Firely Server 5.0.0-beta1<vonk_releasenotes_5_0_0-beta1>`
    * Indexing has been updated to support searching for version-specific references.

    **Release 1.4.0, October 6th, 2022**

    * Added new setting ``convertAbsoluteUrlsToRelative`` which is an array of server URL base values. This feature converts absolute URL references to relative references for the given server URL base array. Example: Setting of ``http://example.org/R4`` will convert an absolute URL ``http://example.org/R4/Patient/123`` to relative as ``Patient/123``. 

    * Added a new mode ``onlyIfNewer`` for option ``--update-existing-resources`` (see the CLI options above)

      .. note::

        This option is currently supported only for SQL Server

    * The setting ``--useUcum`` has been removed. From now on, all quantitative values get automatically canonicalized to UCUM values

    * Indexing has been fixed for search parameters of type `reference` that index resource elements of type `uri`. The following SearchParameters were affected by the bug:

      - FHIR4: ConceptMap-source-uri, ConceptMap-target-uri, PlanDefinition-definition
      - STU3: ImplementationGuide-resource, Provenance-agent
      
      Consider :ref:`re-indexing<feature_customsp_reindex_specific>` your database for these search parameters if you use them.

      .. note::

        Please note that due to a mistake in the official STU3 specification, search parameters `ConceptMap-source-uri`, `ConceptMap-target-uri` still do not work as expected. The correct search parameter expressions would be `ConceptMap.source.as(uri)` and `ConceptMap.target.as(uri)` while the specification contains `ConceptMap.source.as(Uri)` and `ConceptMap.target.as(Uri)` respectively. The issue has been addressed in R4.
        
    **Release 1.3.1**

    * Corrected an exception when multiple batch threads are processing and saving in parallel to SQL Server.

    **Release 1.3.0**

    * Add configuration ``haltOnError``. When ``true``, the FSI will be stopped on a single error. Otherwise, it will log error and continue.  
    * Changed the serialization format of decimal from string to use the native decimal type in MongoDB to improve performance.
    * Bugfix: Fixed Money.currency indexing for FHIR STU3 and R4

    **Release 1.2.0**

    * Ability to provide a path to a custom ``appsettings.json`` file via a command-line argument (see :ref:`examples<tool_fsi_examples>` above)
    * Bugfix: ensure FSI uses all available values from the SQL PK-generating sequences when inserting data to the vonk.entry and component tables

    **Release 1.1.0**

    * Feature: added support for MongoDb!
    * Feature: added support for performance counters using dotnet-counters. See :ref:`tool_fsi_performance_counters` on how to setup and use dotnet-counters.
    * FSI has been upgraded to .NET 6. To install the tool, you first need to have .NET Core SDK v6.x installed on your computer. See :ref:`tool_fsi_installation` for more information.
    * The Firely .NET SDK that FSI uses has been upgraded to 3.7.0. The release notes for the SDK v3.7.0 can be found `here <https://github.com/FirelyTeam/firely-net-sdk/releases>`_.
    * Multiple smaller fixes to improve reliability and performance of the tool.

    **Release 1.0.0**

    * First public release
    * Performance: optimized memory consumption (especially, when reading large `*.ndjson` files)
    * Feature: quantitative values can be automatically canonicalized to UCUM values (see --useUcum CLI option)
    * Multiple smaller fixes to improve reliability and performance of the tool


    .. |br| raw:: html

      <br />

.. _tool_fsi_bill_of_materials:

Bill of Materials
-----------------

Firely Server Ingest is mainly built using libraries from Microsoft .Net Core and ASP.NET Core, along with a limited list of other libraries. This is the full list of direct dependencies that Firely Server Ingest has on other libraries, along with their licenses.

This list uses the NuGet package names (or prefixes of them) so you can easily lookup further details of those packages on `NuGet.org <https://www.nuget.org>`_ if needed.

#. Microsoft.Extensions.* - MIT
#. Serilog(.*) - Apache-2.0
#. System.CommandLine.Hosting - MIT
#. System.ComponentModel.Annotations - MIT
#. System.Threading.Tasks.Dataflow - MIT
#. Hl7.Fhir.* - Firely OSS license
#. Firely.Fhir.* - Firely OSS license
#. Simplifier.Licensing - as Hl7.Fhir

For MongoDB:

#. MongoDB.* - Apache 2.0

For SQL Server:

#. Microsoft.Data.SqlClient - MIT
#. Microsoft.SqlServer.SqlManagementObjects - MIT
