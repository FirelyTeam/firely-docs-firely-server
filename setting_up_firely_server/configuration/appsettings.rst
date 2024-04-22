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
            "AutoUpdateDatabase": true,
            "MigrationTimeout": 1800 // in seconds
            //"AutoUpdateConnectionString" : "set this to the same database as 'ConnectionString' but with credentials that can alter the database. If not set, defaults to the value of 'ConnectionString'"
        },

This will ignore the AutoUpdateConnectionString.

.. _configure_settings_path:

Providing settings in a different folder
----------------------------------------

It can be useful or even necessary to provide settings outside of the Firely Server folder itself, e.g. when mounting the settings to a Docker container. That is possible. 

1. Provide an environment variable named ``VONK_PATH_TO_SETTINGS``, set to the folder where the settings are to be read from. This path can be absolute or relative to the Firely Server directory.
2. In this folder you must provide at least one of the following files:

   1. ``appsettings.instance.json``
   2. ``logsettings.instance.json``
   3. ``auditlogsettings.instance.json``

3. These files will be read with the same :ref:`priority <configure_levels>` as they would have if they were in the Firely Server directory. 

Note that if you provide this environment variable, then:

#. The designated folder must exist.
#. At least one of the three files must be present.
#. The account that runs Firely Server must have read access to each of the files.
#. The Firely Server directory itself will no longer be scanned for any of the three files. So if you want to use any of the three ``*.instance.json`` files, you must provide all of them in the designated directory.

Examples: 

::

    VONK_PATH_TO_SETTINGS=./config

This relative path would read e.g. ``<Firely Server directory>/config/appsettings.instance.json``.

::

    VONK_PATH_TO_SETTINGS=/usr/config

This absolute path would read e.g. ``/usr/config/appsettings.instance.json``.

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
      "Repository": "SQLite", //Memory / SQL / MongoDb
      "MongoDbOptions": {
        "ConnectionString": "mongodb://localhost/vonkadmin",
        "EntryCollection": "vonkentries"
      },
      "SqlDbOptions": {
        "ConnectionString": "connectionstring to your Firely Server Admin SQL Server database (SQL2012 or newer); Set MultipleActiveResultSets=True",
        "AutoUpdateDatabase": true,
        "MigrationTimeout": 1800, // in seconds
        "LogSqlQueryParameterValues": false
        //"AutoUpdateConnectionString" : "set this to the same database as 'ConnectionString' but with credentials that can alter the database. If not set, defaults to the value of 'ConnectionString'",
      },
      "SQLiteDbOptions": {
        "ConnectionString": "Data Source=./data/vonkadmin.db;Cache=Shared", //"connectionstring to your Firely Server Admin SQLite database (version 3 or newer), e.g. Data Source=c:/sqlite/vonkadmin.db;Cache=Shared"
        "AutoUpdateDatabase": true,
        "MigrationTimeout": 1800, // in seconds
        "LogSqlQueryParameterValues": false
        //"AutoUpdateConnectionString" : "set this to the same database as 'ConnectionString' but with credentials that can alter the database. If not set, defaults to the value of 'ConnectionString'"
      },
      "Security": {
        "AllowedNetworks": [ "127.0.0.1", "::1" ], // i.e.: ["127.0.0.1", "::1" (ipv6 localhost), "10.1.50.0/24", "10.5.3.0/24", "31.161.91.98"]
        "OperationsToBeSecured": [ "reindex", "reset", "preload", "importResources" ]
      }
    },

The ``Administration`` section is part of the :ref:`administration_api` and its repository.

.. _configure_license:

License
-------
::

    "License": {
        "LicenseFile": "firelyserver-trial-license.json",
        "RequestInfoFile": "./.vonk-request-info.json",
        "WriteRequestInfoFileInterval": 15 // in minutes
    }


The :ref:`vonk_getting_started` explains how to obtain a licensefile for Firely Server. Once you have it, put the path to it in the ``LicenseFile`` setting. Note that in json you either use forward slashes (/) or double backward slashes (\\\\) as path separators.

.. note::

  It is also possible to supply a license via an environment variable. This functionality is handy when Firely Server is running within a Docker container. See :ref:`license_as_environment_variable` for details.

Other settings:

* ``RequestInfoFile`` sets the location of the file with request information. This file will be used in future releases.
* ``WriteRequestInfoFileInterval`` sets the time interval (in minutes) to write aggregate information about processed requests to the RequestInfoFile.

.. _configure_repository:

Repository
----------
::

    "Repository": "SQLite", //Memory / SQL / MongoDb


#. ``Repository``: Choose which type of repository you want. Valid values are:

  #. :ref:`Memory<configure_memory>`
  #. :ref:`SQL, for Microsoft SQL Server<configure_sql>`
  #. :ref:`SQLite<configure_sqlite>`
  #. :ref:`MongoDb<configure_mongodb>`


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
      "ConnectionString": "mongodb://localhost/vonkdata",
      "EntryCollection": "vonkentries",
      "MaxLogLine": 300
    },

Refer to :ref:`configure_mongodb` for configuring the connection to your MongoDB databases.

SQL
^^^
::

    "SqlDbOptions": {
      "ConnectionString": "connectionstring to your Firely Server SQL Server database (SQL2012 or newer); Set MultipleActiveResultSets=True",
      "AutoUpdateDatabase": true,
      "MigrationTimeout": 1800, // in seconds
      "LogSqlQueryParameterValues": false
      //"AutoUpdateConnectionString" : "set this to the same database as 'ConnectionString' but with credentials that can alter the database. If not set, defaults to the value of 'ConnectionString'"
    },

Refer to :ref:`configure_sql` for configuring access to your SQL Server databases.

SQLite
^^^^^^
::

    "SQLiteDbOptions": {
      "ConnectionString": "Data Source=./data/vonkdata.db;Cache=Shared", //"connectionstring to your Firely Server SQLite database (version 3 or newer), e.g. Data Source=c:/sqlite/vonkdata.db",
      "AutoUpdateDatabase": true,
      "MigrationTimeout": 1800, // in seconds
      "LogSqlQueryParameterValues": false
      //"AutoUpdateConnectionString" : "set this to the same database as 'ConnectionString' but with credentials that can alter the database. If not set, defaults to the value of 'ConnectionString'"
    },

Refer to :ref:`configure_sqlite` for configuring access to your SQLite Server databases.

.. _hosting_options:

http and https
--------------
::

    "Hosting": {
      "HttpPort": 4080,
      //"HttpsPort": 4081, // Enable this to use https
      //"CertificateFile": "<your-certificate-file>.pfx", //Relevant when HttpsPort is present
      //"CertificatePassword" : "<cert-pass>", // Relevant when HttpsPort is present
      //"SslProtocols": [ "Tls12", "Tls13" ], // Relevant when HttpsPort is present.
      //"PathBase": "<subpath-to-firely-server>",
      "ClientCertificateMode": "NoCertificate" // NoCertificate, AllowCertificate, RequireCertificate,
      "Limits": {
          "MaxRequestBufferSize": 2097152 // Kestrel default: 1048576.
    }
    },
    
Refer to :ref:`configure_hosting` for enabling https and adjusting port numbers. The `PathBase` enables the option to specify a path as part of root path (See `PathBase middleware <https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.builder.usepathbaseextensions.usepathbase?view=aspnetcore-6.0>`_ for more information). The `ClientCertificateMode` will instruct Firely Server to request or require a TLS client certificate (See `ASP .NET Core - Client Certificates <https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-6.0#client-certificates>`_ for more information).

The :code:`Limits` is mapped to 
`KestrelServerLimits <https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.server.kestrel.core.kestrelserverlimits?view=aspnetcore-6.0>`_
and allows to modify the default Kestrel limits by adding the relevant property. 
In the example above, the default value of 1048576 of the property :code:`MaxRequestBufferSize` is overriden by  2097152.
You could similarly modify the default value for the maximum number of concurrent connections, 
`MaxConcurrentConnections <https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.server.kestrel.core.kestrelserverlimits.maxconcurrentconnections?view=aspnetcore-6.0#microsoft-aspnetcore-server-kestrel-core-kestrelserverlimits-maxconcurrentconnections>`_, 
however, we recommend using a reverse proxy in front of Firely server, see :ref:`reverse proxy<deploy_reverseProxy>`, and let the reverse proxy take care of those aspects.

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

Terminology
-----------
::

      "Terminology": {
      "MaxExpansionSize": 650,
      "LocalTerminologyService": {
        "Order": 10,
        "PreferredSystems": [ "http://hl7.org/fhir" ],
        "SupportedInteractions": [ "ValueSetValidateCode", "Expand", "FindMatches", "Lookup" ], // ValueSetValidateCode, Expand, FindMatches, Lookup
        "SupportedInformationModels": [ "Fhir3.0", "Fhir4.0", "Fhir5.0" ]
      }
      //Example settings for remote services:
      //, 
      //"RemoteTerminologyServices": [
      //  {
      //    "Order": 20,
      //    "PreferredSystems": [ "http://snomed.info/sct" ],
      //    "SupportedInteractions": [ "ValueSetValidateCode", "Expand", "Lookup", "Translate", "Subsumes", "Closure" ], // ValueSetValidateCode, Expand, Lookup, Translate, Subsumes, Closure
      //    "SupportedInformationModels": [ "Fhir4.0" ],
      //    "Endpoint": "https://r4.ontoserver.csiro.au/fhir/",
      //    "MediaType": "application/fhir+xml"
      //  },
      //  {
      //    "Order": 30,
      //    "PreferredSystems": [ "http://loinc.org" ],
      //    "SupportedInteractions": [ "ValueSetValidateCode", "Expand", "Translate" ],
      //    "SupportedInformationModels": [ "Fhir3.0", "Fhir4.0" ],
      //    "Endpoint": "https://fhir.loinc.org/",
      //    "Username": "",
      //    "Password": ""
      //  }
      //]
    },

Refer to :ref:`feature_terminology`.

.. _configure_cache:

Cache of Conformance Resources
------------------------------
::

   "Cache": {
      "MaxConformanceResources": 5000
   }

Firely Server caches StructureDefinitions and other conformance resources that are needed for (de)serialization and validation in memory. If more than ``MaxConformanceResources`` get cached, the ones that have not been used for the longest time are discarded. If you frequently encounter a delay when requesting less used resource types, a larger value may help. If you are very restricted on memory, you can lower the value.


.. _bundle_options:

Search size
-----------
::

    "BundleOptions": {
        "DefaultTotal": "accurate", // Allowed values: none, estimate, accurate
        "DefaultCount": 10,
        "MaxCount": 50,
        "DefaultSort": "-_lastUpdated"
    },


The Search interactions returns a bundle with results. Users can specify the number of results that they want to receive in one response with the ``_count`` parameter.

* ``DefaultCount`` sets the number of results if the user has not specified a ``_count`` parameter.
* ``MaxCount`` sets the number of results in case the user specifies a ``_count`` value higher than this maximum. This is to protect Firely Server from being overloaded.
* ``DefaultCount`` should be less than or equal to ``MaxCount``
* ``DefaultSort`` is what search results are sorted on if no sort order is specified in the request. If a sort order is specified, this is still added as the last sort clause.
* ``DefaultTotal`` sets default total behaviour for search requests if not specified in the request itself.

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

  .. note::

    Before Firely Server (Vonk) version 0.7.1, this setting was named ``BatchOptions`` and the logs will show a warning that it
    is deprecated when you still have it in your appsettings file.

.. _configure_reindex:

Reindexing for changes in SearchParameters
------------------------------------------
::

    "ReindexOptions": {
        "BatchSize": 100,
        "MaxDegreeOfParallelism": 10
    },

See :ref:`feature_customsp_reindex_configure`.

.. _fhir_capabilities:

FHIR Capabilities
-----------------
::

  "FhirCapabilities": {
    "ConditionalDeleteOptions": {
      "ConditionalDeleteType": "Single", // Single or Multiple,
      "ConditionalDeleteMaxItems": 1
    },
    "SearchOptions": {
      "MaximumIncludeIterationDepth": 1,
      "PagingCache": {
        "MaxSize": 1100,
        "ItemSize": 1,
        "Duration": 10
      }
    }
  },

See :ref:`restful_crud`.

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

.. _supportedmodel:

Restrict supported resources and SearchParameters
-------------------------------------------------
::

   "SupportedModel": {
     "RestrictToResources": [ "Patient", "Observation" ],
     "RestrictToSearchParameters": ["Patient.active", "Observation.patient"],
     "RestrictToCompartments": ["Patient"]
   },

By default, Firely Server supports all ResourceTypes, SearchParameters and CompartmentDefinitions from the specification. They are loaded from the :ref:`specification.zip <conformance_specification_zip>`.
If you want to limit support, you can do so with the configuration above. This is primarily targeted towards Facade builders, because they have to provide an implementation for everything that is supported.

Be aware that:

* the '_type', '_id' and '_lastupdated' search parameters on the base Resource type must be supported and cannot be disabled
* the Administration API requires support for the 'url' search parameter on the StructureDefinition type and this cannot be disabled
* this uses the search parameter names, not the path within the resource - so for example to specify `Patient.address.postalCode <http://hl7.org/fhir/R4/patient.html#search>`_ as a supported location, you'd use ``"Patient.address-postalcode"``
* if the support of `AuditEvent` resources is disabled, the AuditEvents will not get generated (see :ref:`audit_event_logging`).

.. _settings_subscriptions:

Subscriptions
-------------
::

    "SubscriptionEvaluatorOptions": {
        "Enabled": true,
        "RepeatPeriod": 20000,
        "SubscriptionBatchSize" : 1
    },

See :ref:`feature_subscription`.

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

.. _settings_smart:

SMART authorization
-------------------

The settings for authorization with SMART on FHIR are covered in :ref:`Firely Auth<feature_accesscontrol_config>`.

.. _information_model:

Information model
-----------------

Firely Server supports the use of multiple information models (currently FHIR STU3, R4, and R5) simultaneously. The ``InformationModel`` section contains the related settings.
By default, Firely Server serves both versions from the root of your web service, defaulting to R4 when the client does not use Accept or _format to specify either one. Mapping a path or a subdomain to a specific version creates an additional URI serving only that particular version.
::

  "InformationModel": {
    "Default": "Fhir4.0", // information model to use when none is specified in either mapping, the _format parameter or the ACCEPT header
    "IncludeFhirVersion": ["Fhir4.0", "Fhir5.0"],
    "Mapping": {
      "Mode": "Off"
      //"Mode": "Path", // yourserver.org/r3 => FHIR STU3; yourserver.org/r4 => FHIR R4
      //"Map": {
      //  "/R3": "Fhir3.0",
      //  "/R4": "Fhir4.0",
      //  "/R5": "Fhir5.0"
      //}
      //"Mode": "Subdomain", // r3.yourserver.org => FHIR STU3; r4.yourserver.org => FHIR R4
      //"Map": 
      //  {
      //    "r3": "Fhir3.0",
      //    "r4": "Fhir4.0",
      //    "r5": "Fhir5.0"
      //  }
    }
  },

See :ref:`feature_multiversion`.

.. _http_options:

Response options
----------------
::

    "HttpOptions": {
      "DefaultResponseType": "application/fhir+json"
    }

* If no mediatype is specified in an ``Accept`` header, use the ``DefaultResponseType``.
* Options are ``application/fhir+json`` or ``application/fhir+xml``
* Firely Server will attach the mimetype parameter ``fhirVersion`` based on the FHIR version that is requested (see :ref:`feature_multiversion`).

Task File Management and Bulk Data Export
-----------------------------------------
::

  "TaskFileManagement": {
    "StoragePath": "./taskfiles"
  },

::

  "BulkDataExport": {
    "RepeatPeriod": 60000, //ms
    "AdditionalResources": [ "Organization", "Location", "Substance", "Device", "Medication", "Practitioner" ] // included referenced resources, additional to the Patient compartment resources
  },

Refer to :ref:`feature_bulkdataexport`.

.. _patient_everything_operation:

Patient Everything Operation
----------------------------
::

  "PatientEverythingOperation": {
    "AdditionalResources": [ "Organization", "Location", "Substance", "Medication", "Device" ] // included referenced resources, additional to the Patient compartment resources
  },

The Patient $everything operation returns all resources linked to a patient that are listed in the Compartment Patient. This section allows you to define additional resources that will be included in the resulting searchset bundle.

See :ref:`feature_patienteverything`.

.. _uri_conversion:

Uri conversion on import and export
-----------------------------------

Upon importing, Firely Server converts all references expressed as absolute URIs with the root corresponding to the server URL.
For example, ``"reference": "https://someHost/fhir/Patient/someId"`` will be stored as ``"reference": "Patient/someId"`` .
Similarly,  upon exporting, the references stored as relative URIs will be converted back to an absolute URI by adding the 
root server location to the relative URI.

In addition, any element of type ``url`` or ``uri`` can also be converted upon import or export, as long as the FHIR path 
corresponding to the element in the FHIR resource are listed in the setting ``UrlMapping`` :

::

  "UrlMapping": {
     "AdditionalPathsToMap": [
       "DocumentReference.content.attachment.url",
       "Bundle.entry.resource.content.attachment.url"
     ]
   },

Note that the setting is still in beta and is subject to change in future release of Firely Server.

Binary Wrapper
--------------
::

    "Vonk.Plugin.BinaryWrapper": {
    "RestrictToMimeTypes": [ "application/pdf", "text/plain", "text/csv", "image/png", "image/jpeg" ]
  },

Refer to :ref:`plugin_binarywrapper`.

Auditing
--------
::

  "Audit": {
    "AuditEventSignatureEnabled": false,
    "AuditEventSignatureSecret": {
      "SecretType": "JWKS",
      "Secret": ""
    },
    "AsyncProcessingRepeatPeriod" : 10000,
    "AuditEventVerificationBatchSize" : 20,
    "ExcludedRequests": [],
    "InvalidAuditEventProcessingThreshold" : 100
  },

Refer to :ref:`feature_auditing`.

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
          "Vonk.Fhir.R5",
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
          "Vonk.Fhir.R5",
          // etc.
        ],
        "Exclude": [
          "Vonk.Plugin.Operations"
        ]
      }
    ]
  }

It is possible to disable a specific information model by removing Vonk.Fhir.R3, Vonk.Fhir.R4, or Vonk.Fhir.R5 from the pipeline

Please note the warning on merging arrays in :ref:`configure_levels`.

See :ref:`vonk_plugins` for more information and an example custom plugin.
