.. _configure_appsettings:

Firely Server settings
======================

Firely Server settings are controlled in json configuration files called ``appsettings(.*).json``. The possible settings in these files are all the same and described below.
The different files are read in a hierarchy so you can control settings on different levels. All appsettings files are in the Firely Server distribution directory, next to Firely.Server.dll.
We go through all the sections of this file and refer you to detailed pages on each of them.

You can also control :ref:`configure_envvar`.

Changes to the settings require a restart of Firely Server.

.. _configure_levels:

Hierarchy of settings
---------------------

Firely Server reads its settings from these sources, in this order:

:appsettings.default.json: Installed with Firely Server, contains default settings and a template setting if no sensible default is available.
:appsettings.json: You can create this one for your own settings. Because it is not part of the Firely Server distribution, it will not be overwritten by a next Firely Server version.
:environment variables: See :ref:`configure_envvar`.
:appsettings.instance.json: You can create this one to override settings for a specific instance of Firely Server. It is not part of the Firely Server distribution.
                            This file is especially useful if you run multiple instances on the same machine.

Settings lower in the list override the settings higher in the list (think CSS, if you're familiar with that).

.. warning::

   JSON settings files can have arrays in them. The configuration system can NOT merge arrays.
   So if you override an array value, you need to provide all the values that you want in the array.
   In the Firely Server settings this is relevant for e.g. Validation.AllowedProfiles and for the PipelineOptions.

.. note::
   By default in ASP.NET Core, if on a lower level the array has more items, you will still inherit those extra items.
   We fixed this in Firely Server, an array will always overwrite the complete base array.
   To nullify an array, add the value with an array with just an empty string in it::

     "PipelineOptions": {
       "Branches": [
         {
           "Path": "myroot",
           "Exclude": [""]
         }
       ]
     }

   This also means you cannot override a single array element with an environment variable. (Which was tricky anyway - relying on the exact number and order of items in the original array.)

.. _configure_change_settings:

Changing the settings
---------------------

In general you do not change the settings in ``appsettings.default.json`` but create your own overrides in ``appsettings.json`` or ``appsettings.instance.json``. That way your settings are not overwritten by a new version of Firely Server (with a new ``appsettings.default.json`` therein), and you automatically get sensible defaults for any new settings introduced in ``appsettings.default.json``.

Settings after first install
^^^^^^^^^^^^^^^^^^^^^^^^^^^^

After you installed Firely Server (see :ref:`vonk_getting_started`), either:

* copy the ``appsettings.default.json`` to ``appsettings[.instance].json`` and remove settings that you do not intend to alter, or
* create an empty ``appsettings[.instance].json`` and copy individual parts from the ``appsettings.default.json`` if you wish to adjust them.

Adjust the new ``appsettings[.instance].json`` to your liking using the explanation below.

When running :ref:`Firely Server on Docker<use_docker>` you probably want to adjust the settings using the :ref:`Environment Variables<configure_envvar>`.

Settings after update
^^^^^^^^^^^^^^^^^^^^^

If you install the binaries of an updated version of Firely Server, you can:

* copy the new binaries over the old ones, or
* deploy the new version to a new directory and copy the ``appsettings[.instance].json`` over from the old version.

In both cases, check the :ref:`vonk_releasenotes` to see if settings have changed, or new settings have been introduced.
If you want to adjust a changed / new setting, copy the relevant section from ``appsettings.default.json`` to your own ``appsettings[.instance].json`` and then adjust it.

Commenting out sections
^^^^^^^^^^^^^^^^^^^^^^^

JSON formally has no notion of comments. But the configuration system of ASP.Net Core (and hence Firely Server) accepts double slashes just fine::

    "Administration": {
        "Repository": "SQLite", //Memory / SQL / MongoDb
        "SqlDbOptions": {
            "ConnectionString": "connectionstring to your Firely Server Admin SQL Server database (SQL2012 or newer); Set MultipleActiveResultSets=True",
            "SchemaName": "vonkadmin",
            "AutoUpdateDatabase": true
            //"AutoUpdateConnectionString" : "set this to the same database as 'ConnectionString' but with credentials that can alter the database. If not set, defaults to the value of 'ConnectionString'"
        },

This will ignore the AutoUpdateConnectionString.

.. _log_configuration:

Log of your configuration
-------------------------

Because the hierarchy of settings can be overwhelming, Firely Server logs the resulting configuration.
To enable that, the loglevel for ``Vonk.Server`` must be ``Information`` or more detailed. That is set for you by default in ``logsettings.default.json``.
Refer to :ref:`configure_log` for information on setting log levels.

Administration
--------------
::

    "Administration": {
        "Repository": "SQLite", //Memory / SQL / MongoDb are other options, but SQLite is advised.
        "MongoDbOptions": {
            "ConnectionString": "mongodb://localhost/vonkadmin",
            "EntryCollection": "vonkentries"
        },
        "SqlDbOptions": {
            "ConnectionString": "connectionstring to your Firely Server Admin SQL Server database (SQL2012 or newer); Set MultipleActiveResultSets=True",
            "SchemaName": "vonkadmin",
            "AutoUpdateDatabase": true
            //"AutoUpdateConnectionString" : "set this to the same database as 'ConnectionString' but with credentials that can alter the database. If not set, defaults to the value of 'ConnectionString'"
        },
       "SQLiteDbOptions": {
            "ConnectionString": "Data Source=./data/vonkadmin.db",
            "AutoUpdateDatabase": true
        },
        "Security": {
        "AllowedNetworks": [ "::1" ], // i.e.: ["127.0.0.1", "::1" (ipv6 localhost), "10.1.50.0/24", "10.5.3.0/24", "31.161.91.98"]
        "OperationsToBeSecured": [ "reindex", "reset", "preload" ]
        }
    },

The ``Administration`` section is to :ref:`configure_administration` and its repository.

.. _configure_license:

License
-------
::

    "License": {
        "LicenseFile": "firelyserver-trial-license.json",
        "RequestInfoFile": "./.vonk-request-info.json",
        "WriteRequestInfoFileInterval": 15 // in minutes
    }


The :ref:`vonk_getting_started` explains how to obtain a licensefile for Firely Server. Once you have it, put the path to it in the ``LicenseFile`` setting. Note that in json you either use forward slashes (/) or double backward slashes (\\\\\\) as path separators.

Other settings:

* ``RequestInfoFile`` sets the location of the file with request information. This file will be used in future releases.
* ``WriteRequestInfoFileInterval`` sets the time interval (in minutes) to write aggregate information about processed requests to the RequestInfoFile.

.. _configure_repository:

Repository
----------
::

    "Repository": "SQLite", //Memory / SQL / MongoDb / CosmosDb


#. ``Repository``: Choose which type of repository you want. Valid values are:

  #. Memory
  #. SQL, for Microsoft SQL Server
  #. SQLite
  #. MongoDb
  #. CosmosDb


Memory
^^^^^^
::

    "MemoryOptions": {
        "SimulateTransactions": "false"
    },

Refer to :ref:`configure_memory` for configuring the In-Memory storage.

MongoDB
^^^^^^^
::

    "MongoDbOptions": {
        "ConnectionString": "mongodb://localhost/vonkstu3",
        "EntryCollection": "vonkentries",
        "SimulateTransactions": "false"
    },


Refer to :ref:`configure_mongodb` for configuring the connection to your MongoDB databases.

SQL
^^^
::

    "SqlDbOptions": {
        "ConnectionString": "connectionstring to your Firely Server SQL Server database (SQL2012 or newer); Set MultipleActiveResultSets=True",
        "SchemaName": "vonk",
        "AutoUpdateDatabase": true
        //"AutoUpdateConnectionString" : "set this to the same database as 'ConnectionString' but with credentials that can alter the database. If not set, defaults to the value of 'ConnectionString'"
    },


Refer to :ref:`configure_sql` for configuring access to your SQL Server databases.

SQLite
^^^^^^
::

    "SQLiteDbOptions": {
        "ConnectionString": "Data Source=./data/vonkdata.db",
        "AutoUpdateDatabase": true
    },


Refer to :ref:`configure_sqlite` for configuring access to your SQLite Server databases.

CosmosDb
^^^^^^^^
::

    "CosmosDbOptions": {
        "ConnectionString": "mongodb://<password>@<server>:10255/vonk?ssl=true&replicaSet=globaldb",
        "EntryCollection": "vonkentries"
    },

Refer to :ref:`configure_cosmosdb` for configuring access to your CosmosDb databases.

.. _hosting_options:

http and https
--------------
::

    "Hosting": {
        "HttpPort": 4080,
        //"HttpsPort": 4081, // Enable this to use https
        //"CertificateFile": "<your-certificate-file>.pfx", //Relevant when HttpsPort is present
        //"CertificatePassword" : "<cert-pass>" // Relevant when HttpsPort is present
    },

Refer to :ref:`configure_hosting` for enabling https and adjusting port numbers.

.. _validation_options:

Validation
----------
::

  "Validation": {
    "Parsing": "Permissive", // Permissive / Strict
    "Level": "Off", // Off / Core / Full
    "AllowedProfiles": []
  },


Refer to :ref:`feature_prevalidation`.

.. _bundle_options:

Search and History
------------------
::

    "BundleOptions": {
        "DefaultCount": 10,
        "MaxCount": 50,
        "DefaultSort": "-_lastUpdated"
    },


The Search and History interactions return a bundle with results. Users can specify the number of results that they want to receive in one response with the ``_count`` parameter.

* ``DefaultCount`` sets the number of results if the user has not specified a ``_count`` parameter.
* ``MaxCount`` sets the number of results in case the user specifies a ``_count`` value higher than this maximum. This is to protect Firely Server from being overloaded.
* ``DefaultCount`` should be less than or equal to ``MaxCount``
* ``DefaultSort`` is what search results are sorted on if no sort order is specified in the request. If a sort order is specified, this is still added as the last sort clause.

.. _batch_options:

Batch and transaction
---------------------
::

    "BatchOptions": {
        "MaxNumberOfEntries": 100
    },

This will limit the number of entries that are accepted in a single Batch or Transaction bundle.

.. note::

  This setting has been moved to the ``SizeLimits`` setting as of Firely Server (Vonk) version 0.7.1, and the logs will show a warning that it
  is deprecated when you still have it in your appsettings file.

.. _sizelimits_options:

Protect against large input
---------------------------
::

    "SizeLimits": {
        "MaxResourceSize": "1MiB",
        "MaxBatchSize": "5MiB",
        "MaxBatchEntries": 150
    },

* ``MaxResourceSize`` sets the maximum size of a resource that is sent in a create or update.
* ``MaxBatchSize`` sets the maximum size of a batch or transaction bundle.
  (Note that a POST http(s)://<firely-server-endpoint>/Bundle will be limited by MaxResourceSize, since the bundle must be processed as a whole then.)
* ``MaxBatchEntries`` limits the number of entries that is allowed in a batch or transaction bundle.
* The values for ``MaxResourceSize`` and ``MaxBatchSize`` can be expressed in b (bytes, the default), kB (kilobytes), KiB (kibibytes), MB (megabytes), or MiB (mebibytes).
  Do not put a space between the amount and the unit.

.. _configure_admin_import:

SearchParameters and other Conformance Resources
------------------------------------------------
::

    "AdministrationImportOptions": {
        "ImportDirectory": "./vonk-import",
        "ImportedDirectory": "./vonk-imported", //Do not place ImportedDirectory *under* ImportDirectory, since an import will recursively read all subdirectories.
        "SimplifierProjects": [
          {
            "Uri": "https://stu3.simplifier.net/<your-project>",
            "UserName": "Simplifier user name",
            "Password": "Password for the above user name",
            "BatchSize": 20
          }
        ]
    }

See :ref:`conformance` and :ref:`feature_customsp`.

.. _configure_cache:

Cache of Conformance Resources
------------------------------
::

   "Cache": {
      "MaxConformanceResources": 5000
   }

Firely Server caches StructureDefinitions and other conformance resources that are needed for (de)serialization and validation in memory. If more than ``MaxConformanceResources`` get cached, the ones that have not been used for the longest time are discarded. If you frequently encounter a delay when requesting less used resource types, a larger value may help. If you are very restricted on memory, you can lower the value.

.. _configure_reindex:

Reindexing for changes in SearchParameters
------------------------------------------
::

    "ReindexOptions": {
        "BatchSize": 100,
        "MaxDegreeOfParallelism": 10
    },

See :ref:`feature_customsp_reindex_configure`.

.. _supportedmodel:

Restrict supported resources and SearchParameters
-------------------------------------------------
::

   "SupportedModel": {
     "RestrictToResources": [ "Patient", "Observation" ],
     "RestrictToSearchParameters": ["Patient.active", "Observation.patient", "Resource._id", "StructureDefinition.url"],
     "RestrictToCompartments": ["Patient"]
   },

By default, Firely Server supports all ResourceTypes, SearchParameters and CompartmentDefinitions from the specification. They are loaded from the :ref:`specification.zip <conformance_specification_zip>`.
If you want to limit support, you can do so with the configuration above. This is primarily targeted towards Facade builders, because they have to provide an implementation for everything that is supported.

Be aware that:

* support for _type and _id must not be disabled
* the Administration API requires support for the 'url' SearchParameter on the conformance resourcetypes
* this uses the search parameter names, not the path within the resource - so for example to specify `Patient.address.postalCode <http://hl7.org/fhir/R4/patient.html#search>`_ as a supported location, you'd use ``"Patient.address-postalcode"``.

.. _disable_interactions:

Enable or disable interactions
------------------------------

By default, the value ``SupportedInteractions`` contains all the interactions that are implemented in Firely Server.
But you can disable interactions by removing them from these lists.
::

    "SupportedInteractions": {
        "InstanceLevelInteractions": "read, vread, update, delete, history, conditional_delete, conditional_update, $validate",
        "TypeLevelInteractions": "create, search, history, $validate, $snapshot, conditional_create",
        "WholeSystemInteractions": "capabilities, batch, transaction, history, search, $validate"
    },

If you implement a custom operation in a plugin, you should also add the name of that operation at the correct level. E.g. add ``$convert`` to ``TypeLevelInteractions`` to allow ``<base>/<resourcetype>/$convert``.

Subscriptions
-------------
::

    "SubscriptionEvaluatorOptions": {
        "Enabled": true,
        "RepeatPeriod": 20000,
        "SubscriptionBatchSize" : 1
    },

See :ref:`feature_subscription`.

.. _information_model:

Information model
-----------------

Firely Server supports the use of multiple information models (currently FHIR STU3 and R4) simultaneously. The ``InformationModel`` section contains the related settings.
By default, Firely Server serves both versions from the root of your web service, defaulting to STU3 when the client does not use Accept or _format to specify either one. Mapping a path or a subdomain to a specific version creates an additional URI serving only that particular version.
::

  "InformationModel": {
    "Default": "Fhir4.0", // For STU3: "Fhir3.0". Information model to use when none is specified in either mapping, the _format parameter or the ACCEPT header.
    "Mapping": {
      "Mode": "Off"
      //"Mode": "Path", // yourserver.org/r3 => FHIR STU3; yourserver.org/r4 => FHIR R4
      //"Map": {
      //  "/R3": "Fhir3.0",
      //  "/R4": "Fhir4.0"
      //}
      //"Mode": "Subdomain", // r3.yourserver.org => FHIR STU3; r4.yourserver.org => FHIR R4
      //"Map":
      //  {
      //    "r3": "Fhir3.0",
      //    "r4": "Fhir4.0"
      //  }
    }
  },

See :ref:`feature_multiversion`.

.. _fhir_capabilities:

FHIR Capabilities
-----------------
::

  "FhirCapabilities": {
    "ConditionalDeleteOptions": {
      "ConditionalDeleteType": "Single", // Single or Multiple,
      "ConditionalDeleteMaxItems": 1
    }
  },

See :ref:`restful_crud`.

History size
------------
::

  "HistoryOptions": {
    "MaxReturnedResults": 100
  }

See :ref:`restful_history`.

.. _settings_pipeline:

Configuring the Firely Server Pipeline
--------------------------------------

You can add your own plugins to the Firely Server pipeline, or control which of the standard Firely Server plugins
are used for your Firely Server, by changing the ``PipelineOptions``.
::

  "PipelineOptions": {
    "PluginDirectory": "./plugins",
    "Branches": [
      {
        "Path": "/",
        "Include": [
          "Vonk.Core",
          "Vonk.Fhir.R3",
          "Vonk.Fhir.R4",
          // etc.
        ],
        "Exclude": [
        ]
      },
      {
        "Path": "/administration",
        "Include": [
          "Vonk.Core",
          "Vonk.Fhir.R3",
          "Vonk.Fhir.R4",
          // etc.
        ],
        "Exclude": [
          "Vonk.Core.Operations"
        ]
      }
    ]
  }

It is possible to disable a specific information model by removing Vonk.Fhir.R3 or Vonk.Fhir.R4 from the pipeline

Please note the warning on merging arrays in :ref:`configure_levels`.

See :ref:`vonk_plugins` for more information and an example custom plugin.
