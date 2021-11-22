.. _tool_fsi:

Firely Server Ingest (FSI)
==========================

**Firely Server Ingest (FSI)** is a CLI application designed to optimize massive resource ingestion into a Firely Server instance. In contrast to resource ingestion by sending HTTP requests to a running Firely Server instance, this tool writes data directly to the underlying FS database which increases the throughput significantly.

.. note::

    * Currently, this tool only supports import into SQL Server database, however, we are working on support for MongoDB.
    * The tool is in beta stage and can be tested only after consulting us.

.. Installation
.. ------------
.. TBD: describe the usage of dotnet install command

General usage
-------------

.. attention::

  * Firely Server instances targeting the same database will be impacted severely by the workload that FSI puts on the database. We advise to stop the Firely Server instances while the import is performed.
  * Only one instance of FSI per database should be run at a time. FSI can utilize all the cores on the machine it is run on, and insert data over several connections to the database in parallel. Multiple instances would probably cause congestion in the database.

Prerequisites
^^^^^^^^^^^^^
The tool requires that the target SQL Server database already exists and contains all required tables and indexes. If you don't have a database with the schema yet, you first need to run the Firely Server at least once as described in article :ref:`configure_sql`.


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

The execution of FSI can be configured using input parameters. These parameters can be supplied either as CLI arguments or specified in the file ``appsettings.instance.json`` which must be created in the same directory as the ``fsi.exe``.

If you want to specify input parameters in the file, you can use the snippet below as a base for your ``appsettings.instance.json``. In this case, you need to update the values that you want to set yourself and delete all other records.

.. code-block:: JavaScript

  {
    "source": "./fsi-source", //valid directory
    "limit": -1,
    "fhirVersion": "R4",
    "license": "C:/some/path/to/your/license/license.json", // See the Licensing section below
    "updateExistingResources": "true",
    "useUcum": false,
    "sqlserver": {
      "connectionString": "<connectionstring to the Firely Server SQL Server database>",
      "saveParallel": 2,
      "queryExistenceParallel": 4,
      "saveBatchSize": 500,
      "commandTimeOut": 60, //seconds
      "liftIndexes": false
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
      "maxActiveResources": 3000
    }
  }

Supported arguments
^^^^^^^^^^^^^^^^^^^

+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| CLI argument                                      | Appsettings parameter name       | Required | Description                                                                                                                                         |
+===================================================+==================================+==========+=====================================================================================================================================================+
| ``-f``, ``--fhir-version <R3|R4>``                | fhirVersion                      |          | FHIR version of the input                                                                                                                           |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``-s``, ``--source <source>``                     | source                           | yes      | Input directory for work (this directory is visited recursively including all the subdirectories)                                                   |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``-l``, ``--limit <limit>``                       | limit                            |          | Limit the number of resources to import. Use this for testing your setup.                                                                           |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--license <license>``                           | license                          | yes      | Firely Server license file                                                                                                                          |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--update-existing-resources <true|false>``      | updateExistingResources          |          | When true, a resource is updated in the database if it already exists and a history record is created. Otherwise, an existing resource gets skipped.|
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--useUcum <true|false>``                        | useUcum                          |          | When true, any quantitative data will be canonicalized to UCUM. Otherwise, only the original value and unit will be kept.                           |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``-c``, ``--connectionstring <connectionstring>`` | sqlServer/connectionString       | yes      | Connection string to Firely Server SQL Server database                                                                                              |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--sqlPar <sqlPar>``                             | sqlServer/saveParallel           |          | The # of batches to save in parallel. Depends on your bandwidth to SQL Server and its processing power.                                             |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--sqlBatch <sqlBatch>``                         | sqlServer/saveBatchSize          |          | The # of resources to save in each batch. SQL Server must be able to process it within the CommandTimeout.                                          |
|                                                   |                                  |          | It is recommended to set this value to at least 500 for optimal performance.                                                                        |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--sqlTimeout <sqlTimeout>``                     | sqlServer/commandTimeOut         |          | The time SQL Server is allowed to process a batch of resources.                                                                                     |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--sqlExistQryPar <sqlExistQryPar>``             | sqlserver/queryExistenceParallel |          | The number of parallel threads querying the DB to check whether a resource exists (only when ``--update-existing-resources`` is set to false).      |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--sqlIndexes``                                  | sqlServer/liftIndexes            |          | Experimental! Removes all the indexes before the import and re-applies them afterwards.                                                             |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--readPar <readPar>``                           | workflow/readParallel            |          | Number of threads to read from the source. Reading is quite fast so it need not be high.                                                            |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--readBuffer <readBuffer>``                     | workflow/readBufferSize          |          | Number of resources to buffer after reading.                                                                                                        |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--metaPar <metaPar>``                           | workflow/metaParallel            |          | Number of threads to assign metadata. Should be higher than ReadParallel.                                                                           |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--metaBuffer <metaBuffer>``                     | workflow/metaBufferSize          |          | Number of resources to buffer for assigning metadata.                                                                                               |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--typePar <typePar>``                           | workflow/typeParallel            |          | Number of threads to add type information. Should be higher than ReadParallel.                                                                      |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--typeBuffer <typeBuffer>``                     | workflow/typeBufferSize          |          | Number of resources to buffer for adding type information.                                                                                          |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--indexPar <indexPar>``                         | workflow/indexParallel           |          | Number of threads to index the search parameters. This is typically the most resource intensive step and should have the most threads.              |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--indexBuffer <indexBuffer>``                   | workflow/indexBufferSize         |          | Number of resources to buffer for indexing the search parameters.                                                                                   |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--maxActiveRes <maxActiveRes>``                 | workflow/maxActiveResources      |          | Maximum number of actively processed resources. Reduce the value to reduce memory consumption.                                                      |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--version``                                     |                                  |          | Show version information                                                                                                                            |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``-?``, ``-h``, ``--help``                        |                                  |          | Show help and usage information                                                                                                                     |
+---------------------------------------------------+----------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+

Examples
--------

Runs the import for files located in directory **/path/to/your/input/files** and its subdirectories using license file **/path/to/your/license/fsi-license.json** targeting the database defined by the connection string. In case if a resource being imported already exists in the target database, it gets skipped.

.. code-block:: bash

  dotnet fsi.exe \
  -s ./path/to/your/input/files \
  --license /path/to/your/license/fsi-license.json \
  -c 'Initial Catalog=VonkData;Data Source=server.hostname,1433;User ID=username;Password=PaSSSword!' \
  --update-existing-resources false 

Same as above but if a resource being imported already exists in the target database, it gets updated. The old resource gets preserved as a historical record.

.. code-block:: bash

  dotnet fsi.exe \
  -s ./path/to/your/input/files \
  --license /path/to/your/license/fsi-license.json \
  -c 'Initial Catalog=VonkData;Data Source=server.hostname,1433;User ID=username;Password=PaSSSword!'

Monitoring
----------

Logs
^^^^

When importing the data, it is handy to have the logging enabled, as it would capture any issues if they occur. By default, the log messages are written both to the console window and to the log files in the ``%temp%`` directory.

You can configure the log settings the same way as you do for Firely Server: :ref:`configure_log`. 

Performance counters
^^^^^^^^^^^^^^^^^^^^
You can get insights into the tool performance by means of performance counters. There are many ways to monitor the performance counters. One of the options is using `dotnet-counters <https://docs.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-counters>`_.

To monitor the counters for FSI, you can execute the following command:
:: 

  dotnet-counters monitor --counters 'System.Runtime','FSI Processing'  --process-id <process_id>

where *<process_id>* is the PID of the running FSI tool.


Known issues
------------

* FSI does not support scenarios where resources of different FHIR versions are stored in the same database;
* When importing data from large ``*.ndjson`` files, the memory consumption may be quite high.

Licensing
---------

The application is licensed separately from the core Firely Server distribution. Please :ref:`contact<vonk-contact>` Firely to get the license. 

Your license already permits the usage of FSI if it contains ``http://fire.ly/vonk/plugins/bulk-data-import``.

Release notes
-------------

.. _fsi_releasenotes_1.0.0:

Release 1.0.0
^^^^^^^^^^^^^

* First public release
* Performance: optimized memory consumption (especially, when reading large `*.ndjson` files)
* Feature: quantitative values can be automatically canonicalized to UCUM values (see --useUcum CLI option)
* Multiple smaller fixes to improve reliability and performance of the tool