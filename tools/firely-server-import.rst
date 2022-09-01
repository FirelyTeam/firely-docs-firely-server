.. _tool_fsi:

Firely Server Ingest (FSI)
==========================

**Firely Server Ingest (FSI)** is a CLI application designed to optimize massive resource ingestion into a Firely Server instance. In contrast to resource ingestion by sending HTTP requests to a running Firely Server instance, this tool writes data directly to the underlying FS database which increases the throughput significantly.

The tool supports ingestion into SQL Server and MongoDB databases.

.. _tool_fsi_installation:

Installation
------------
To install the tool, you first need to have .NET Core SDK v6.x installed on your computer. You can download it `here <https://dotnet.microsoft.com/en-us/download>`_. Once it is installed, execute the following command:

::

  dotnet tool install --global Firely.Server.Ingest

The command above will install FSI from this `NuGet package <https://www.nuget.org/packages/Firely.Server.Ingest/>`_.

.. note::

  Make sure that the dotnet tools directory is added to your path, this makes it possible to run the ``fsi`` command from any directory.

    - Please find more information on where globally installed tools are located in `this article <https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools#install-a-global-tool>`_. 
    - For Linux and Mac, make sure you add your ``.profile`` or ``.bash_profile`` to your path.



General usage
-------------

.. attention::

  * Firely Server instances targeting the same database will be impacted severely by the workload that FSI puts on the database. We advise to stop the Firely Server instances while the import is performed.
  * Only one instance of FSI per database should be run at a time. FSI can utilize all the cores on the machine it is run on, and insert data over several connections to the database in parallel. Multiple instances would probably cause congestion in the database.

Prerequisites
^^^^^^^^^^^^^
The tool requires that the target database already exists and contains all required indexes and tables (for SQL Server). If you don't have a database with the schema yet, you first need to run the Firely Server at least once as described in the articles :ref:`configure_sql` and :ref:`configure_mongodb`.


Input files formats
^^^^^^^^^^^^^^^^^^^

FSI supports the following input file formats:

* FHIR *collection* bundles stored in ``*.json`` files, and
* ``*.ndjson`` files where each line contains a separate FHIR resource in JSON format.


After the import
^^^^^^^^^^^^^^^^

After ingesting massive amount of data, it is important to make sure the SQL Server indexes are in good shape. You can read more on this topic here: :ref:`sql_index_maintenance`.

Arguments
---------

The execution of FSI can be configured using input parameters. These parameters can be supplied either as CLI arguments or specified in the file ``appsettings.instance.json`` which must be created in the same directory as the ``fsi`` executable.

If you want to specify input parameters in the file, you can use the snippet below as a base for your ``appsettings.instance.json``. In this case, you need to update the values that you want to set yourself and delete all other records.

.. code-block:: JavaScript

  {
    "source": "./fsi-source", //valid directory
    "limit": -1,
    "fhirVersion": "R4",
    "license": "C:\\data\\deploy\\vonk\\license\\performance-test-license.json",
    "updateExistingResources": true,
    "useUcum": false,
    "databaseType": "SQL",
    "haltOnError": false,

    "sqlserver": {
      "connectionString": "<connectionstring to the Firely Server SQL Server database>",
      "saveParallel": 2,
      "queryExistenceParallel": 4,
      "batchSize": 500,
      "commandTimeOut": 60, //seconds
    },

    "mongodb": {
      "entryCollection": "vonkentries",
      "connectionString": "<connectionstring to the Firely Server MongoDb database>",
      "saveParallel": 2,
      "queryExistenceParallel": 4,
      "batchSize": 500
    },
    
    "workflow": { //-1 = unbounded
      "readParallel": 3,
      "readBufferSize": 200,
      "metaParallel": 1,
      "metaBufferSize": 50,
      "typeParallel": 4,
      "typeBufferSize": 50,
      "indexParallel": -1, //this is usually the most time consuming process - give it as much CPU time as possible.
      "indexBufferSize": 50,
      "maxActiveResources": 15000
    }
  }

Supported arguments
^^^^^^^^^^^^^^^^^^^

+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| CLI argument                                      | Appsettings parameter name       | Required | Description                                                                                                                                         |
+===================================================+==================================+==========+=====================================================================================================================================================+
| ``--settings <settingsJsonFile>``                 |                                  |          | Custom settings json file                                                                                                                           |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``-f``, ``--fhir-version <R3|R4>``                | fhirVersion                      |          | FHIR version of the input                                                                                                                           |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``-s``, ``--source <source>``                     | source                           | yes      | Input directory for work (this directory is visited recursively including all the subdirectories)                                                   |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``-l``, ``--limit <limit>``                       | limit                            |          | Limit the number of resources to import. Use this for testing your setup                                                                            |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--license <license>``                           | license                          | yes      | Firely Server license file                                                                                                                          |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--update-existing-resources <true|false>``      | updateExistingResources          |          | When true, a resource is updated in the database if it already exists and a history record is created. Otherwise, an existing resource gets skipped |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--useUcum <true|false>``                        | useUcum                          |          | When true, any quantitative data will be canonicalized to UCUM. Otherwise, only the original value and unit will be kept                            |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--dbType <MongoDb|SQL>``                        | databaseType                     |          | Specifies the target database type                                                                                                                  |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--haltOnError <true|false>``                    | haltOnError                      |          | When true, stop application on single error. Default = false.                                                                                       |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--mongoCollection <mongoCollection>``           | mongodb/entryCollection          |          | Collection name for entries                                                                                                                         |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--mongoConnectionstring <connectionstring>``    | mongodb/connectionString         | yes      | Connection string to Firely Server MongoDb database                                                                                                 |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--mongoPar <mongoPar>``                         | mongodb/saveParallel             |          | The number of batches to save in parallel. Depends on your bandwidth to MongoDb and its processing power                                            |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--mongoExistQryPar <mongoExistQryPar>``         | mongodb/queryExistenceParallel   |          | The number of parallel threads querying the DB to check whether a resource exists (only when ``--update-existing-resources`` is set to false)       |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--mongoBatch <mongoBatch>``                     | mongodb/batchSize                |          | The number of resources to save in each batch                                                                                                       |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``-c``, ``--connectionstring <connectionstring>`` | sqlServer/connectionString       | yes      | Connection string to Firely Server SQL Server database                                                                                              |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--sqlPar <sqlPar>``                             | sqlServer/saveParallel           |          | The number of batches to save in parallel. Depends on your bandwidth to SQL Server and its processing power                                         |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--sqlBatch <sqlBatch>``                         | sqlServer/saveBatchSize          |          | The number of resources to save in each batch. SQL Server must be able to process it within the CommandTimeout.                                     |
|                                                   |                                  |          | It is recommended to set this value to at least 500 for optimal performance                                                                         |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--sqlTimeout <sqlTimeout>``                     | sqlServer/commandTimeOut         |          | The time SQL Server is allowed to process a batch of resources                                                                                      |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--sqlExistQryPar <sqlExistQryPar>``             | sqlserver/queryExistenceParallel |          | The number of parallel threads querying the DB to check whether a resource exists (only when ``--update-existing-resources`` is set to false).      |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--sqlIndexes``                                  | sqlServer/liftIndexes            |          | Experimental! Removes all the indexes before the import and re-applies them afterwards                                                              |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--readPar <readPar>``                           | workflow/readParallel            |          | Number of threads to read from the source. Reading is quite fast so it need not be high                                                             |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--readBuffer <readBuffer>``                     | workflow/readBufferSize          |          | Number of resources to buffer after reading                                                                                                         |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--metaPar <metaPar>``                           | workflow/metaParallel            |          | Number of threads to assign metadata. Should be higher than ReadParallel                                                                            |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--metaBuffer <metaBuffer>``                     | workflow/metaBufferSize          |          | Number of resources to buffer for assigning metadata                                                                                                |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--typePar <typePar>``                           | workflow/typeParallel            |          | Number of threads to add type information. Should be higher than ReadParallel                                                                       |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--typeBuffer <typeBuffer>``                     | workflow/typeBufferSize          |          | Number of resources to buffer for adding type information                                                                                           |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--indexPar <indexPar>``                         | workflow/indexParallel           |          | Number of threads to index the search parameters. This is typically the most resource intensive step and should have the most threads               |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--indexBuffer <indexBuffer>``                   | workflow/indexBufferSize         |          | Number of resources to buffer for indexing the search parameters                                                                                    |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--maxActiveRes <maxActiveRes>``                 | workflow/maxActiveResources      |          | Maximum number of actively processed resources. Reduce the value to reduce memory consumption                                                       |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--version``                                     |                                  |          | Show version information                                                                                                                            |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``-?``, ``-h``, ``--help``                        |                                  |          | Show help and usage information                                                                                                                     |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+

.. _tool_fsi_examples:

Examples
--------

Specify a custom settings file **/path/to/your/custom/settings/appsettings.instance.json**.

.. code-block:: bash

  fsi --settings ./path/to/your/custom/settings/appsettings.instance.json 

.. note::
  If ``--settings`` is omitted, FSI searches following folders sequentially and tries to find ``appsettings.instance.json``. The first occurrence will be used if FSI finds one, otherwise the default ``appsettings.json`` will be used.  
  
  * Current launched folder |br| 
    e.g. ``C:\Users\Bob\Desktop``  
  * FSI installation folder |br|
    e.g. ``C:\Users\Bob\.dotnet\tools``  
  * FSI installation ``dll`` folder |br| 
    e.g. ``C:\Users\Bob\.dotnet\tools\.store\firely.server.ingest\version\firely.server.ingest\version\tools\net6.0\any``

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
* When importing data from large ``*.ndjson`` files, the memory consumption may be quite high.
* When importing STU3 resources, the field ``Patient.deceased`` will always be set to ``true`` if it exists. This is caused by an error in the FHIR STU3 specification. In case you would like to use FSI with STU3 resources, please :ref:`contact us<vonk-contact>`.

Licensing
---------

The application is licensed separately from the core Firely Server distribution. Please :ref:`contact<vonk-contact>` Firely to get the license. 

Your license already permits the usage of FSI if it contains ``http://fire.ly/vonk/plugins/bulk-data-import``.

Release notes
-------------

.. _fsi_releasenotes_1.3.1:

Release 1.3.1
^^^^^^^^^^^^^

* Corrected an exception when multiple batch threads are processing and saving in parallel to SQL Server.

.. _fsi_releasenotes_1.3.0:

Release 1.3.0
^^^^^^^^^^^^^

* Add configuration ``haltOnError``. When ``true``, the FSI will be stopped on a single error. Otherwise, it will log error and continue.  
* Changed the serialization format of decimal from string to use the native decimal type in MongoDB to improve performance.
* Bugfix: Fixed Money.currency indexing for FHIR STU3 and R4

.. _fsi_releasenotes_1.2.0:

Release 1.2.0
^^^^^^^^^^^^^

* Ability to provide a path to a custom ``appsettings.json`` file via a command-line argument (see :ref:`examples<tool_fsi_examples>` above)
* Bugfix: ensure FSI uses all available values from the SQL PK-generating sequences when inserting data to the vonk.entry and component tables


.. _fsi_releasenotes_1.1.0:

Release 1.1.0
^^^^^^^^^^^^^

* Feature: added support for MongoDb!
* Feature: added support for performance counters using dotnet-counters. See :ref:`tool_fsi_performance_counters` on how to setup and use dotnet-counters.
* FSI has been upgraded to .NET 6. To install the tool, you first need to have .NET Core SDK v6.x installed on your computer. See :ref:`tool_fsi_installation` for more information.
* The Firely .NET SDK that FSI uses has been upgraded to 3.7.0. The release notes for the SDK v3.7.0 can be found `here <https://github.com/FirelyTeam/firely-net-sdk/releases>`_.
* Multiple smaller fixes to improve reliability and performance of the tool.

.. _fsi_releasenotes_1.0.0:

Release 1.0.0
^^^^^^^^^^^^^

* First public release
* Performance: optimized memory consumption (especially, when reading large `*.ndjson` files)
* Feature: quantitative values can be automatically canonicalized to UCUM values (see --useUcum CLI option)
* Multiple smaller fixes to improve reliability and performance of the tool


.. |br| raw:: html

   <br />
