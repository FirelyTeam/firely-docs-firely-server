.. _tool_fsi:

Bulk Import via Firely Server Ingest
====================================

.. note::
  This application is licensed separately from the core Firely Server distribution. Please :ref:`contact<vonk-contact>` Firely to get the license. 
  Your license already permits the usage of FSI if it contains ``http://fire.ly/vonk/plugins/bulk-data-import``. You can also try out Firely Server Ingest. It is limited to a maximum of 10000 resources in total in the connected Firely Server database.


**Firely Server Ingest (FSI)** is a CLI application designed to optimize massive resource ingestion into a Firely Server instance. In contrast to resource ingestion by sending HTTP requests to a running Firely Server instance, this tool writes data directly to the underlying FS database which increases the throughput significantly.

The tool supports ingestion into SQL Server and MongoDB databases.

.. _tool_fsi_installation:

Installation
------------
To install the tool, you first need to have .NET Core SDK v6.x installed on your computer. You can download it `here <https://dotnet.microsoft.com/en-us/download>`__. Once it is installed, execute the following command:

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
  * FSI does not add tenant security labels, see :ref:`feature_multitenancy`.

Prerequisites
^^^^^^^^^^^^^
The tool requires that the target database already exists and contains all required indexes and tables (for SQL Server). If you don't have a database with the schema yet, you first need to run the Firely Server at least once as described in the articles :ref:`configure_sql` and :ref:`configure_mongodb`.

.. important::

  Each version of Firely Server Ingest is bound to a specific version of Firely Server. The following table shows which combinations of Firely Server (its database schema version respectively) and Firely Server Ingest can be used in combination.


+-----------------------+------------------------------+
| Firely Server Version | Firely Server Ingest Version |
+=======================+==============================+
| v5.1.0 and later      | v2.2.0 and v2.2.1            |
+-----------------------+------------------------------+
| v5.0.0                | v2.1.0                       |
+-----------------------+------------------------------+
| v5.0.0-beta1          | v2.0.0                       |
+-----------------------+------------------------------+
| v4.10.0 and later     | v1.4.0                       |
+-----------------------+------------------------------+
| v4.9.0                | v1.3.0                       |
+-----------------------+------------------------------+
| v4.8.0                | v1.2.0                       |
+-----------------------+------------------------------+
| v4.2.0 and later      | v1.1.0                       |
+-----------------------+------------------------------+
| v4.2.0                | v1.0.0                       |
+-----------------------+------------------------------+

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
    "databaseType": "SQL",
    "haltOnError": false,
    
    "absoluteUrlConversion": {
      "baseEndpoints": [
        // "http://localhost:4080/R4"
      ],
      "elements": [
        "DocumentReference.content.attachment.url"
      ]
    },
    
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

    "workflow": { //-1 = unbounded
      "readParallel": 3,
      "readBufferSize": 750,
      "metaParallel": 1,
      "metaBufferSize": 50,
      "typeParallel": 4,
      "typeBufferSize": 50,
      "absoluteToRelativeParallel": 1,
      "absoluteToRelativeBufferSize": 50,
      "indexParallel": -1, //this is usually the most time consuming process - give it as much CPU time as possible.
      "indexBufferSize": 50
    }
  }

.. _FSI_supported_arguments:

Supported arguments
^^^^^^^^^^^^^^^^^^^

+----------------------------------------------------------+-------------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| CLI argument                                             | Appsettings parameter name          | Required | Description                                                                                                                                         |
+==========================================================+=====================================+==========+=====================================================================================================================================================+
| ``--settings <settingsJsonFile>``                        |                                     |          | Custom settings json file                                                                                                                           |
+----------------------------------------------------------+-------------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``-f``, ``--fhir-version <R3|R4>``                       | fhirVersion                         |          | FHIR version of the input, R3 or R4 (not STU3)                                                                                                      |
+----------------------------------------------------------+-------------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``-s``, ``--source <source>``                            | source                              | yes      | Input directory for work (this directory is visited recursively including all the subdirectories)                                                   |
+----------------------------------------------------------+-------------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``-l``, ``--limit <limit>``                              | limit                               |          | Limit the number of resources to import. Use this for testing your setup                                                                            |
+----------------------------------------------------------+-------------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--license <license>``                                  | license                             | yes      | Firely Server license file                                                                                                                          |
+----------------------------------------------------------+-------------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--update-existing-resources <true|false|onlyIfNewer>`` | updateExistingResources             |          | When true, a resource is updated in the database if it already exists and a history record is created.                                              |
|                                                          |                                     |          | When false, existing records in the database are skipped.                                                                                           |
|                                                          |                                     |          | When onlyIfNewer, existing records with meta:LastUpdated greater in the database are skipped. ``--dbType <MongoDb>`` not supported for onlyIfNewer. |
|                                                          |                                     |          | Default = true.                                                                                                                                     |
+----------------------------------------------------------+-------------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--dbType <MongoDb|SQL>``                               | databaseType                        |          | Specifies the target database type                                                                                                                  |
+----------------------------------------------------------+-------------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--haltOnError <true|false>``                           | haltOnError                         |          | When true, stop application on single error. Default = false.                                                                                       |
+----------------------------------------------------------+-------------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--convertAbsoluteUrlsToRelative:index url``            | convertAbsoluteUrlsToRelative       |          | This setting is deprecated. You should use ``absoluteUrlConversion/baseEndpoints`` instead.                                                         |
|      with index ranging from 0 to 19                     |                                     |          |                                                                                                                                                     |
+----------------------------------------------------------+-------------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--urlConvBases:index url``                             | absoluteUrlConversion/baseEndpoints |          | Convert absolute URLs to relative for servers in this array. The array values must match exactly the base URL otherwise no changes are made.        |
|      with index ranging from 0 to 19                     |                                     |          | The conversion is done for all elements of type ``reference`` as well as the elements of type ``Uri`` or ``Url`` matching a FHIR path provided  in  |
|                                                          |                                     |          | ``absoluteUrlConversion/elements`` setting.                                                                                                         |
|                                                          |                                     |          | Example: Setting of ``http://example.org/R4`` will convert an absolute URL ``http://example.org/R4/Patient/123`` to relative as ``Patient/123``     |
|                                                          |                                     |          | When using the command line argument, the entries of the array must be provided one by one by suffixing with the relevant index. For example:       |
|                                                          |                                     |          | ``--urlConvBases:0 https://host0/fhir  --urlConvBases:1 https://host1/fhir``                                                                        |
+----------------------------------------------------------+-------------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--urlConvElems:index FHIRPath``                        | absoluteUrlConversion/elements      |          | List of FHIR paths specifying the list of ``Uri`` or ``Url`` elements that should be converted from absolute to relative URI if their base          |
|      with index ranging from 0 to 19                     |                                     |          | endpoints matches on of the base endpoint specified in ``absoluteUrlConversion/baseEndpoints``.                                                     |
|                                                          |                                     |          | When using the command line argument, the entries of the array must be provided one by one by suffixing with the relevant index. For example:       |
|                                                          |                                     |          | ``--urlConvElems:0 some.path  --urlConvElems:1 some.other.path``                                                                                    |
+----------------------------------------------------------+-------------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--mongoCollection <mongoCollection>``                  | mongodb/entryCollection             |          | Collection name for entries                                                                                                                         |
+----------------------------------------------------------+-------------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--mongoConnectionstring <connectionstring>``           | mongodb/connectionString            | yes      | Connection string to Firely Server MongoDb database                                                                                                 |
+----------------------------------------------------------+-------------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--mongoPar <mongoPar>``                                | mongodb/saveParallel                |          | The number of batches to save in parallel. Depends on your bandwidth to MongoDb and its processing power                                            |
+----------------------------------------------------------+-------------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--mongoExistQryPar <mongoExistQryPar>``                | mongodb/queryExistenceParallel      |          | The number of parallel threads querying the DB to check whether a resource exists (only when ``--update-existing-resources`` is set to false)       |
+----------------------------------------------------------+-------------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--mongoBatch <mongoBatch>``                            | mongodb/batchSize                   |          | The number of resources to save in each batch                                                                                                       |
+----------------------------------------------------------+-------------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``-c``, ``--connectionstring <connectionstring>``        | sqlServer/connectionString          | yes      | Connection string to Firely Server SQL Server database                                                                                              |
+----------------------------------------------------------+-------------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--sqlPar <sqlPar>``                                    | sqlServer/saveParallel              |          | The number of batches to save in parallel. Depends on your bandwidth to SQL Server and its processing power                                         |
+----------------------------------------------------------+-------------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--sqlBatch <sqlBatch>``                                | sqlServer/saveBatchSize             |          | The number of resources to save in each batch. SQL Server must be able to process it within the CommandTimeout.                                     |
|                                                          |                                     |          | It is recommended to set this value to at least 500 for optimal performance                                                                         |
+----------------------------------------------------------+-------------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--sqlTimeout <sqlTimeout>``                            | sqlServer/commandTimeOut            |          | The time SQL Server is allowed to process a batch of resources                                                                                      |
+----------------------------------------------------------+-------------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--sqlExistQryPar <sqlExistQryPar>``                    | sqlserver/queryExistenceParallel    |          | The number of parallel threads querying the DB to check whether a resource exists (only when ``--update-existing-resources`` is set to false).      |
+----------------------------------------------------------+-------------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--readPar <readPar>``                                  | workflow/readParallel               |          | Number of threads to read from the source. Reading is quite fast so it need not be high                                                             |
+----------------------------------------------------------+-------------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--readBuffer <readBuffer>``                            | workflow/readBufferSize             |          | Number of resources to buffer after reading                                                                                                         |
+----------------------------------------------------------+-------------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--metaPar <metaPar>``                                  | workflow/metaParallel               |          | Number of threads to assign metadata. Should be higher than ReadParallel                                                                            |
+----------------------------------------------------------+-------------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--metaBuffer <metaBuffer>``                            | workflow/metaBufferSize             |          | Number of resources to buffer for assigning metadata                                                                                                |
+----------------------------------------------------------+-------------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--typePar <typePar>``                                  | workflow/typeParallel               |          | Number of threads to add type information. Should be higher than ReadParallel                                                                       |
+----------------------------------------------------------+-------------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--typeBuffer <typeBuffer>``                            | workflow/typeBufferSize             |          | Number of resources to buffer for adding type information                                                                                           |
+----------------------------------------------------------+-------------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--absRelPar <absRelPar>``                              | workflow/                           |          | Number of threads when converting absolute to relative references. Should be higher than ReadParallel                                               |
|                                                          | absoluteToRelativeParallel          |          |                                                                                                                                                     |
+----------------------------------------------------------+-------------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--absRelBuffer <absRelBuffer>``                        | workflow/                           |          | Number of resources to buffer when converting absolute to relative references                                                                       |
|                                                          | absoluteToRelativeBufferSize        |          |                                                                                                                                                     |
+----------------------------------------------------------+-------------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--indexPar <indexPar>``                                | workflow/indexParallel              |          | Number of threads to index the search parameters. This is typically the most resource intensive step and should have the most threads               |
+----------------------------------------------------------+-------------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--indexBuffer <indexBuffer>``                          | workflow/indexBufferSize            |          | Number of resources to buffer for indexing the search parameters                                                                                    |
+----------------------------------------------------------+-------------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``--version``                                            |                                     |          | Show version information                                                                                                                            |
+----------------------------------------------------------+-------------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+
| ``-?``, ``-h``, ``--help``                               |                                     |          | Show help and usage information                                                                                                                     |
+----------------------------------------------------------+-------------------------------------+----------+-----------------------------------------------------------------------------------------------------------------------------------------------------+

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

* When importing data from large ``*.ndjson`` files, the memory consumption may be quite high.
* When importing STU3 resources, the field ``Patient.deceased`` will always be set to ``true`` if it exists. This is caused by an error in the FHIR STU3 specification. In case you would like to use FSI with STU3 resources, please :ref:`contact us<vonk-contact>`.
* If a resource is present in a workload more than once, the entries may get processed in parallel and a version that is different from the latest may be set as current.


Release notes
-------------

.. note::
    You can pull the latest version of Firely Server Ingest using the following instructions::
        
        dotnet tool update --global Firely.Server.Ingest




Release 5.5.0+
^^^^^^^^^^^^^^

The FSI release cycle has been synchronized with the Firely Server release cycle.
Please refer to the :ref:`Firely Server release notes <vonk_releasenotes>` for the FSI change log.


.. _fsi_releasenotes_2.3.0:


Release 2.3.0, November 23rd, 2023 
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

* Feature: the mode ``--update-existing-resources onlyIfNewer`` is now supported for MongoDB.
* Feature: ``Serilog.Sinks.MongoDB`` was added to the list of supported log sinks.
* Fix: the ``SqlClient`` dependency package has been updated to version v5.1.1 to address the vulnerability: CVE-2022-41064.
* Fix: the rare exception ``System.InvalidOperationException: Cannot change state from Skipped to Error`` does not get thrown anymore.
* Internal: the way of handling command line arguments has been refactored.



.. _fsi_releasenotes_2.2.1:



Release 2.2.1, September 19th, 2023
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

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

.. _fsi_releasenotes_1.4.1:

Release 1.4.1, August 28th, 2023
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

.. note::
  It is a hotfix release for the latest FSI that supports Firely Server v.4

* Added support for running FSI without the internet connection (see :ref:`tool_fsi_packages_cache`)

.. _fsi_releasenotes_2.2.0:

Release 2.2.0, June 20th, 2023
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

* Fix: Composite parameters are more accurately indexed for SQL Server, to align with Firely Server 5.1.0. See :ref:`vonk_releasenotes_5_1_0` and the accompanying warnings.
* Feature: FSI is now open to evaluation, just like Firely Server itself. It is limited though, to a maximum of 10.000 resources in the database, including history.
* Feature: FSI is updated to Firely .NET SDK 5.1.0, see `its releasenotes <https://github.com/FirelyTeam/firely-net-sdk/releases/tag/v5.1.0>`_

.. _fsi_releasenotes_2.1.0:

Release 2.1.0, March 9th, 2023
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

* Fix: Eliminated deadlocks in FSI when writing data in parallel.
* Settings: The setting ``maxActiveResources`` and the related CLI argument ``--maxActiveRes`` are no longer needed and have been removed.

.. _fsi_releasenotes_2.0.1:

Release 2.0.1, February 12th, 2023
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

* Fix: Add support for schema version 25 for MongoDb

.. _fsi_releasenotes_2.0.0:

Release 2.0.0, January 26th, 2023
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

* Upgraded to work with the database schemas for :ref:`Firely Server 5.0.0-beta1<vonk_releasenotes_5_0_0-beta1>`
* Indexing has been updated to support searching for version-specific references.

.. _fsi_releasenotes_1.4.0:

Release 1.4.0, October 6th, 2022
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

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
