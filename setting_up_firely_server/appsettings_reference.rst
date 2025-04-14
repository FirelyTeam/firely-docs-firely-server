.. _fs_settings_reference:

Firely Server settings reference
================================

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
      "AllowedNetworks": [ "127.0.0.1", "::1" ], // i.e.: ["127.0.0.1", "::1" (ipv6 localhost), "10.1.50.0/24", "10.5.3.0/24", "31.161.91.98"]
    },

The ``Administration`` section is part of the :ref:`administration_api` and its repository.

.. _configure_license:

License
-------
::

    "License": {
        "LicenseFile": "firelyserver-trial-license.json"
    }


The :ref:`vonk_getting_started` explains how to obtain a licensefile for Firely Server. Once you have it, put the path to it in the ``LicenseFile`` setting. Note that in json you either use forward slashes (/) or double backward slashes (\\\\) as path separators.

.. note::

  It is also possible to supply a license via an environment variable. This functionality is handy when Firely Server is running within a Docker container. See :ref:`license_as_environment_variable` for details.

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
    
Refer to :ref:`configure_hosting` for enabling https and adjusting port numbers.

In case Firely Server is hosted behind a reverse proxy using a subpath as part of the base url, use the `PathBase` option to specify the path after the root to enable Firely Server to generate correct links in places where the absolute base url is used (e.g. in the ``Location`` header when returning an HTTP response). See `PathBase middleware <https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.builder.usepathbaseextensions.usepathbase>`_ for more information. Only a single static path is allowed here. For more dynamic options using multiple paths, see support for the :ref:`X-Forwarded-Prefix header<xforwardedheader>`.

The `ClientCertificateMode` will instruct Firely Server to request or require a TLS client certificate. See `ASP .NET Core - Client Certificates <https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?#configure-client-certificates-in-appsettingsjson>`_ for more information.

The :code:`Limits` is mapped to 
`KestrelServerLimits <https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.server.kestrel.core.kestrelserverlimits>`_
and allows to modify the default Kestrel limits by adding the relevant property. 
In the example above, the default value of 1048576 of the property :code:`MaxRequestBufferSize` is overriden by  2097152.
You could similarly modify the default value for the maximum number of concurrent connections, 
`MaxConcurrentConnections <https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.server.kestrel.core.kestrelserverlimits.maxconcurrentconnections#microsoft-aspnetcore-server-kestrel-core-kestrelserverlimits-maxconcurrentconnections>`_, 
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
      //    "Password": "",
      //    "ClientId": "",
      //    "ClientSecret": "",
      //    "TokenEndpoint": "",
      //    "Scopes": "",
      //    "RemoteTerminologyAuthentication": "Basic", // Jwt or Basic
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
        "DefaultTotal": "none", // Allowed values: none, estimate, accurate
        "DefaultCount": 10,
        "MaxCount": 50,
        "DefaultSort": "-_lastUpdated"
    },


The Search interactions returns a bundle with results. Users can specify the number of results that they want to receive in one response with the ``_count`` parameter. Also see :ref:`_navigational_links` .

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
    "AllowCreateOnUpdate": true|false
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

The ``Operations`` section provides granular control over each operation, allowing you to enable/disable and configure authorization requirements for standard and custom operations.

Example configuration for enabling a custom operation:

.. code-block:: json

    "Operations": {
      "$myCustomOperation": {
        "Name": "$myCustomOperation",
        "Level": ["Type"],
        "Enabled": true,
        "RequireAuthorization": "WhenAuthEnabled",
        "RequireTenant": "WhenTenancyEnabled"
      }
    }

Introduction
^^^^^^^^^^^^

Firely Server 6.0 introduces a completely revamped operations configuration structure that provides more granular control over each operation. This new structure unifies previously scattered configuration settings from multiple sections into a cohesive and comprehensive model.

**Key Benefits**

- **Unified Configuration**: All operation settings are now in one place
- **Granular Control**: Fine-grained control over individual operations
- **Explicit Configuration**: All configuration options are explicitly defined
- **Enhanced Security**: More detailed access control and authorization options

New Configuration Structure
^^^^^^^^^^^^^^^^^^^^^^^^^^^

The new configuration uses a top-level ``Operations`` section that contains operation configurations organized by operation name:

.. code-block:: json

    "Operations": {
      "$closure": {
        "Name": "$closure",
        "Level": [
          "System"
        ],
        "Enabled": true,
        "RequireAuthorization": "WhenAuthEnabled",
        "RequireTenant": "Never"
      },
      "capabilities": {
        "Name": "capabilities",
        "Level": [
          "System"
        ],
        "Enabled": true,
        "RequireAuthorization": "Never",
        "RequireTenant": "Never"
      },
      "create": {
        "Name": "create",
        "Level": [
          "Type"
        ],
        "Enabled": true,
        "RequireAuthorization": "WhenAuthEnabled",
        "RequireTenant": "WhenTenancyEnabled"
      }
    }

For administrative operations, a similar structure exists under ``Administration.Operations``:

.. code-block:: json

    "Administration": {
      "Operations": {
        "$reindex": {
          "Name": "$reindex",
          "Level": [
            "System"
          ],
          "Enabled": true,
          "NetworkProtected": true
        },
        "$reset": {
          "Name": "$reset",
          "Level": [
            "System"
          ],
          "Enabled": true,
          "NetworkProtected": true
        }
      }
    }

Configuration Properties
^^^^^^^^^^^^^^^^^^^^^^^^

Each operation can be configured with the following properties:

.. list-table::
   :header-rows: 1
   :widths: 20 15 50 15

   * - Property
     - Type
     - Description
     - Availability
   * - ``Name``
     - string
     - The operation name, matching the key in the Operations dictionary
     - Regular & Admin
   * - ``Level``
     - array of strings
     - The level(s) at which the operation is available: "System", "Type", and/or "Instance"
     - Regular & Admin
   * - ``Enabled``
     - boolean
     - Whether the operation is enabled
     - Regular & Admin
   * - ``RequireAuthorization``
     - string
     - Authorization requirement: "WhenAuthEnabled", "Always", or "Never"
     - Regular only
   * - ``OperationScope``
     - string
     - Required token scope for the operation (only applies when authorization is enabled)
     - Regular only
   * - ``NetworkProtected``
     - boolean
     - Whether the operation is restricted to allowed networks
     - Admin only
   * - ``RequireTenant``
     - string
     - Tenant requirement: "WhenTenancyEnabled", "Always", or "Never"
     - Regular only

Migration from Previous Configuration
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

The new configuration structure replaces several previous configuration sections. Here's how to migrate your existing configuration:

**1. SupportedInteractions Section**

**Before (v5.x):**

.. code-block:: json

    "SupportedInteractions": {
      "InstanceLevelInteractions": "read, vread, update, delete, history, conditional_delete, conditional_update, $validate",
      "TypeLevelInteractions": "create, search, history, $validate, $snapshot, conditional_create",
      "WholeSystemInteractions": "capabilities, batch, transaction, history, search, $validate"
    }

**After (v6.x):**

For each operation, create an entry in the ``Operations`` section with appropriate settings. For standard operations, these are provided by default.

**2. Administration Security OperationsToBeSecured**

**Before (v5.x):**

.. code-block:: json

    "Administration": {
      "Security": {
        "AllowedNetworks": ["127.0.0.1", "::1"],
        "OperationsToBeSecured": ["reindex", "reset", "preload", "importResources"]
      }
    }

**After (v6.x):**

For each operation in ``OperationsToBeSecured``, set ``NetworkProtected`` to ``true`` in the corresponding operation configuration:

.. code-block:: json

    "Administration": {
      "AllowedNetworks": ["127.0.0.1/32", "::1/128"],
      "Operations": {
        "$reindex": {
          "Name": "$reindex",
          "Level": ["System"],
          "Enabled": true,
          "NetworkProtected": true
        },
        // other operations...
      }
    }

Note that the name of the operation is now prefixed with a "$" sign.

**3. SmartAuthorizationOptions Protected**

**Before (v5.x):**

.. code-block:: json

    "SmartAuthorizationOptions": {
      "Protected": {
        "Resource": ["Patient", "Observation"],
        "Operation": ["$lastn", "$everything"]
      }
    }

**After (v6.x):**

For each operation in ``SmartAuthorizationOptions.Protected.Operation``, set ``RequireAuthorization`` to ``"WhenAuthEnabled"`` or ``"Always"`` in the corresponding operation configuration:

.. code-block:: json

    "Operations": {
      "$lastn": {
        "Name": "$lastn",
        "Level": ["Type", "Instance"],
        "Enabled": true,
        "RequireAuthorization": "Always",
        "RequireTenant": "WhenTenancyEnabled"
      },
      "$everything": {
        "Name": "$everything",
        "Level": ["Instance"],
        "Enabled": true,
        "RequireAuthorization": "Always",
        "RequireTenant": "WhenTenancyEnabled"
      }
    }

Operation Configuration Options
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

**Authorization Options**

The ``RequireAuthorization`` property has three possible values:

1. ``"WhenAuthEnabled"`` (Default): Authorization is required only when authorization is enabled in Firely Server
2. ``"Always"``: Authorization is always required, server start is prevented when authorization is disabled
3. ``"Never"``: Authorization is never required, even if server authorization is enabled

This property is only configurable for standard FHIR operations under the main ``Operations`` section. Administrative operations have fixed authorization behavior that cannot be changed.

**Operation Scope**

The ``OperationScope`` property defines the required token scope for an operation. This setting only applies when authorization is enabled in Firely Server.

* If you do not provide a scope, the access token will not need to include any specific scope to perform this operation
* If you provide a scope, the access token must include that scope to perform this operation
* For standard scopes, refer to the SMART on FHIR scopes documentation (e.g., patient/Patient.read, user/Observation.write)

For example, if you configure an operation with ``"OperationScope": "http://server.fire.ly/auth/scope/erase-operation"``, then any access token used to access this operation must include the "http://server.fire.ly/auth/scope/erase-operation" scope.

**Network Protection Options**

The ``NetworkProtected`` property controls access restrictions based on IP networks:

1. ``true``: The operation can only be accessed from networks defined in the ``Administration.AllowedNetworks`` configuration
2. ``false`` (Default): The operation can be accessed from any network

Important: This property is only applicable to administrative operations (under the ``Administration.Operations`` section). It cannot be used with standard FHIR operations and is specifically designed to restrict sensitive administrative operations to specific IP networks.

**Multi-tenancy Options**

The ``RequireTenant`` property controls whether an operation requires tenant information with three possible values:

1. ``"WhenTenancyEnabled"`` (Default): The operation requires tenant information only when VirtualMultitenancy is enabled
2. ``"Always"``: The operation always requires tenant information; server start is prevented when VirtualMultitenancy is disabled
3. ``"Never"``: The operation never requires tenant information, even if VirtualMultitenancy is enabled

When VirtualMultitenancy is enabled:
- Operations with ``RequireTenant: "WhenTenancyEnabled"`` will require a tenant to be specified in the request
- Operations with ``RequireTenant: "Always"`` will require a tenant to be specified in the request
- Operations with ``RequireTenant: "Never"`` will work without a tenant specification

When VirtualMultitenancy is disabled:
- Operations with ``RequireTenant: "WhenTenancyEnabled"`` will work without tenant information
- Operations with ``RequireTenant: "Always"`` will prevent Firely Server from starting
- Operations with ``RequireTenant: "Never"`` will work without tenant information

This property is only applicable to standard FHIR operations (under the main ``Operations`` section). Administrative operations do not support this property as they operate at the system level across all tenants.
See :ref:`feature_multitenancy` for more details about multitenancy.

Example Configuration
^^^^^^^^^^^^^^^^^^^^^

Here's an example of the new operation configuration structure:

.. code-block:: json

    {
      "Operations": {
        "$closure": {
          "Name": "$closure",
          "Level": ["System"],
          "Enabled": true,
          "RequireAuthorization": "WhenAuthEnabled",
          "RequireTenant": "Never"
        },
        "capabilities": {
          "Name": "capabilities",
          "Level": ["System"],
          "Enabled": true,
          "RequireAuthorization": "Never",
          "RequireTenant": "Never"
        },
        "create": {
          "Name": "create",
          "Level": ["Type"],
          "Enabled": true,
          "RequireAuthorization": "WhenAuthEnabled",
          "RequireTenant": "WhenTenancyEnabled"
        },
        "$validate": {
          "Name": "$validate",
          "Level": ["System", "Type", "Instance"],
          "Enabled": true,
          "RequireAuthorization": "WhenAuthEnabled",
          "RequireTenant": "WhenTenancyEnabled",
          "OperationScope": "validation"
        }
      },
      "Administration": {
        "AllowedNetworks": ["127.0.0.1/32", "::1/128"],
        "Operations": {
          "$reindex": {
            "Name": "$reindex",
            "Level": ["System"],
            "Enabled": true,
            "NetworkProtected": true
          },
          "$reset": {
            "Name": "$reset",
            "Level": ["System"],
            "Enabled": true,
            "NetworkProtected": true
          }
        }
      }
    }

**Custom Operations**

For custom operations, you need to explicitly add them to the ``Operations`` section with all required properties. Core operations like read, create, update, etc. are enabled by default, but custom operations must be explicitly configured.

.. code-block:: json

    "Operations": {
      "$myCustomOperation": {
        "Name": "$myCustomOperation",
        "Level": ["Type"],
        "Enabled": true,
        "RequireAuthorization": "WhenAuthEnabled",
        "RequireTenant": "WhenTenancyEnabled",
        "OperationScope": "custom-operation"
      }
    }

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
    "StorageService": {
          "StorageType": "LocalFile", // LocalFile / AzureBlob / AzureFile
          "StoragePath": "./taskfiles",
          "ContainerName": "firelyserver" // For AzureBlob / AzureFile only
      }
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

URI conversion on import and export
-----------------------------------

Upon importing, Firely Server converts all references expressed as absolute URIs, with the root corresponding to the server URL, and stores them as *relative URIs* in the database.
For example, ``"reference": "https://someHost/fhir/Patient/someId"`` will be stored as ``"reference": "Patient/someId"``.

By default, GET requests to Firely Server will return *absolute URIs* by adding the root server location to each relative URI. The response headers ``Location`` and ``Content-Location`` will also contain absolute URIs.
For example, ``"reference": "Patient/someId"`` stored in the database for Firely Server hosted at ``http://localhost:8080`` will return in a REST API response as ``"reference": "http://localhost:8080/Patient/someId"``.
This behavior can be disabled with the setting ``ReturnAbsoluteReferences``. Note that the setting is still in beta and is subject to change in future release of Firely Server.

In addition, any element of type ``url`` or ``uri`` can also be converted upon import or export, as long as the FHIR path 
corresponding to the element in the FHIR resource are listed in the setting ``UrlMapping`` :

::

  "UrlMapping": {
     "ReturnAbsoluteReferences": true,
     "AdditionalPathsToMap": [
       "DocumentReference.content.attachment.url",
       "Bundle.entry.resource.content.attachment.url"
     ]
   },

Finally, the Bulk Data Export feature of Firely Server returns *relative URIs*. See :ref:`feature_bulkdataexport` for more information.

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
