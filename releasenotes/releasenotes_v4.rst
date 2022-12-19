.. _vonk_releasenotes_history_v4:

Current Firely Server release notes (v4.x)
==========================================

.. _vonk_releasenotes_4_10_0:

Release 4.10.0, October 6th, 2022
---------------------------------

Feature
^^^^^^^

#. HealthCheck: We introduced endpoints for healthchecks, both liveness and readiness, see :ref:`feature_healthcheck`.
#. Provenance: We added support for the X-Provenance header, see :ref:`feature_x-provenance`.

   .. note::

     If you use this release as a facade implementation, you need to disable the provenance header configuration in the pipeline.

#. Compartments: The reference searchparameter ``Binary.securityContext`` can now be used as a link for the Patient compartment. If SMART on FHIR is enabled and a "patient"-level scope is being used, Binary resources must now point to a Patient resource matching the "patient" claim in the access token. For facade and plugin implementations this change means that the ``ICompartment`` information is now available for requests on the ``Binary`` resource type.
#. Audit on search: We enhanced audit logging by exposing the literal references of the current page of the results from a search request. For file logging, this information can be included by adding ``SearchResultSummary`` to the :ref:`output template <configure_audit_log_file>`. In AuditEvent resources, each search result (reference) is included as an ``entity`` node, in addition to the entity node for the bundle with the query parameters. 
#. Search: We added support of ``:of-type`` search modifier for search parameters targeting Identifier elements (e.g. ``Patient.identifier``). See the `FHIR spec article on search <https://www.hl7.org/fhir/r4/search.html#token>`_ for more information.

   .. note::

     After the update, all new resources containing Identifier elements will be automatically indexed and become searchable with the ``:of-type`` modifier. If you want to make discoverable the data you already have, you would need to :ref:`re-index<feature_customsp_reindex_specific>` your data for some of the search parameters you want to use with the ``:of-type`` modifier.

#. Search: Added support for the ``Prefer`` header when searching, with allowed values ``handling=strict`` or ``handling=lenient``, as per `the specification <http://build.fhir.org/search.html#errors>`_.  
#. Search: Added support for wildcards on ``_include=*`` and ```_revinclude=*```, so you can (rev)include all linked resource at once, as specified with `search <http://hl7.org/fhir/search.html#revinclude>`_.
#. Search: Canonical references can now be searched using the ``:below`` modifier, that will match a prefix of the version, as specified for `References and versions <https://www.hl7.org/fhir/search.html#versions>`_
#. US Core: Added support for the ``$docref`` operation. Firely Server can retrieve all existing DocumentReferences for a Patient, but cannot yet generate a document. This operation is defined as part of the `US Core implementation guide <http://hl7.org/fhir/us/core/OperationDefinition-docref.html)>`_.
#. Mimetype: If a request did not specify a mimetype in the ``Accept`` header, Firely Server would default to ``application/fhir+json``. If you prefer XML, this default can now be set in the :ref:`http_options`.
#. :ref:`feature_preload` is updated and now works for all FHIR versions. Please note that this feature is still meant for limited amounts of (mainly example) data. For loading large amounts of data we recommend to use :ref:`tool_fsi`.

Database
^^^^^^^^

#. We introduced new optimizations for the **MongoDB** data schema and queries. These optimizations will improve search performance for elements of type ``dateTime`` and ``decimal``. Please read below notes for the upgrade process.

   .. attention::
      The upgrade procedure for Firely Server running on MongoDb requires a mandatory migration. If your collection contains a lot of resources, this may take a very long time. Therefore, the MongoDb upgrade script has to be executed manually. The script can be found in `mongodb\FS_SchemaUpgrade_Data_v22_v23.js`
      
      Here are some guidelines:

      * We tested it on a MongoDb collection with a size of 500GB. The upgrade script took around 24 hours to complete on a fairly powerful machine.
      * As always, make sure you have a backup of your database that has been tried and tested before you begin the upgrade.
      * Please make sure that Firely Server is shutdown before you execute the script.
      * If you encounter problems running the script, or need any assistance, please :ref:`contact us<vonk-contact>`.

      The update script will update the data that is stored in the database. Although Firely Server can be started as soon as the migration is finished, it will have decreased performance during the first day of operation. This is due to a change in indexes which requires them to be rebuilt in the background.

#. We introduced user defined table types in **SQL Server** for an optimization in :ref:`Firely Server Ingest 1.4.0 <fsi_releasenotes_1.4.0>`. The update is in migration script ``FS_SchemaUpgrade_Data_v24_v25.sql`` and will be applied automatically when ``AutoUpdateDatabase=true`` in the settings.

Fix
^^^

#. Administration: The order of loading knowledge and conformance resources has been fixed. We made sure that the definitions stored in the administration database take precedence over the definitions from the ``specification.zip`` file. 
   Any custom implementations of ``IModelContributor`` are loaded after the database and before the ZIP file.
#. Search: An erratum to the specification of R4 has been made, changing the type of search parameter ``Resource-profile`` from uri to reference (with target StructureDefinition). This was an ommision in R4 and has been fixed in R5. 
   The change allows searching for _profile with the ``:above`` and ``:below`` modifier. To take advantage of it, the following steps must be taken:

   - Optionally but recommended: before upgrading, remove the current index data for Resource._profile (see :ref:`re-indexing<feature_customsp_reindex_specific>`)
   - Upgrade Firely Server, execute the database migrations and start the server
   - Re-index Resource._profile (see :ref:`re-indexing<feature_customsp_reindex_specific>`)

   .. note::

      If you have made manual changes to SearchParameter/Resource-profile-Fhir4.0 and want to search with the :above/:below modifier, you must update your definition to be of type `reference` with target `StructureDefinition`

#. Search: Indexing has been fixed for search parameters of type `reference` that index resource elements of type `uri`. The following SearchParameters were affected by the bug:
   Consider :ref:`re-indexing<feature_customsp_reindex_specific>` your database for these search parameters if you use them.

   - FHIR4: ConceptMap-source-uri, ConceptMap-target-uri, PlanDefinition-definition
   - STU3: ImplementationGuide-resource, Provenance-agent

   .. note::

      Please note that due to a mistake in the official STU3 specification, search parameters `ConceptMap-source-uri`, `ConceptMap-target-uri` still do not work as expected. The correct search parameter expressions would be `ConceptMap.source.as(uri)` and `ConceptMap.target.as(uri)` while the specification contains `ConceptMap.source.as(Uri)` and `ConceptMap.target.as(Uri)` respectively. The issue has been addressed in R4.

#. SMART: With SMART on FHIR enabled, an update-on-create (creating a new resource with an update / PUT) was allways denied. This is now fixed.
#. Subscription: if the resthook url in a Subscription did not end with a slash (``/``), it would get shortened to the last slash in the url. This is now fixed, the whole url is used.

Plugin and Facade
^^^^^^^^^^^^^^^^^

#. Facade: When building predicates in a Facade implementation of ``ISearchRepository`` / ``IRepoQueryFactory``, exceptions where only translated to the OperationOutcome, but not logged. Now they are also logged.
#. API: We will narrow the public programming API in the ``Vonk.Core`` package in the next major release. To alert you to that, we deprecated the parts that will be removed from the public API. 

   .. attention::

      Please try to build your plugin or facade against ``Vonk.Core 4.10.0`` to check if you use any of the deprecated parts. If you think some part should not be deprecated, please let us know with a support ticket.

.. _vonk_releasenotes_493:

Release 4.9.3, September 15th, 2022
-----------------------------------

Fix
^^^
#. Starting with Firely Server v4.9.0, a specific search query could fail, with multiple includes on the same parameter, having different type modifier, e.g. `Coverage?_include=Coverage:payor:Patient&_include=Coverage:payor:Organization`. That is fixed.

.. _vonk_releasenotes_492:

Release 4.9.2, August 24th, 2022
--------------------------------

Fix
^^^
#. Starting with Firely Server v4.9.0, validation was only performed against the core specification even if the validation level was set to "Full" and resources sent to Firely Server contained a meta.profile claim.

.. _vonk_releasenotes_491:

Release 4.9.1, August 1th, 2022
-------------------------------


Fix
^^^
#. Fixed an issue with _include and _revinclude in case the (rev-)include link was pointing to an element of type "canonical" and not of type "reference".
#. "_total" was added as default parameter in the v4.9.0 release. Therefore it must be handled in a facade implementation. The Vonk.Facade.Relational package now handles the case of "_total=accurate". All other argument values must still be handled in the ISearchRepository implementation.
#. Reading the specification.zip file from a read-only disk caused an exception.
#. Excluding the UrlMappingService from the pipeline configuration and executing a CRUD operation caused an exception.

Feature
^^^^^^^
#. The exposed `SMART capabilities <http://hl7.org/fhir/smart-app-launch/conformance.html#capabilities>`_ in the .well-known/smart-configuration can now be configured in the appsettings. See ``SmartAuthorizationOptions.SmartCapabilities`` in section :ref:`SMART Configuration<feature_accesscontrol_config>`.

.. _announcement_vonk_8_july_2021:

Public Endpoint Announcement 8 July 2022
----------------------------------------

The default FHIR version of the `public Firely Server endpoint <https://server.fire.ly/>`_ is now R4.

.. _vonk_releasenotes_490:

Release 4.9.0, July 6th, 2022
-----------------------------

Security
^^^^^^^^

#. Upgraded Microsoft.AspNetCore.Authentication.JwtBearer dependency as a mitigation for `CVE-2021-34532 <https://github.com/dotnet/aspnetcore/security/advisories/GHSA-q7cg-43mg-qp69>`_.

Database
^^^^^^^^

#. Switched the serialization format for decimal types from string to the native decimal type in MongoDB to improve performance.
#. For SQL Server database, if you upgrade Firely Server all the way from v4.2.1, it is likely that the resulting index ``vonk.ref.ref_name_relativereference`` differ from a clean installation of Firely Server. The upgrade procedure will try to fix the index automatically. If your database is large, this may take too long and the upgrade process will time out. If that happens you need to run the upgrade script manually. The script for the `admin` database can be found in ``sqlserver/FS_SchemaUpgrade_Admin_v22_v23.sql`` and the script for the `data` database can be found in ``sqlserver/FS_SchemaUpgrade_Data_v23_v24.sql``. 

.. attention::
    The upgrade procedure for Firely Server running on MongoDb requires a mandatory migration. If your collection contains a lot of resources, this may take a very long time. Therefore, the MongoDb upgrade script has to be executed manually. The script can be found in `mongodb\FS_SchemaUpgrade_Data_v21_v22.js`
    
    Here are some guidelines:

   * We tested it on a MongoDb collection with a size of 500GB. The upgrade script took around 24 hours to complete on a fairly powerful machine.
   * As always, make sure you have a backup of your database that has been tried and tested before you begin the upgrade.
   * Please make sure that Firely Server is shutdown before you execute the script.
   * If you encounter problems running the script, or need any assistance, please :ref:`contact us<vonk-contact>`.

Fix
^^^
#. Fixed an issue where a "/" was missing in the fullUrl of a "search" bundle in case an information model mapping with mode "Path" was used.
#. Fixed an issue where a new resource id was not created when POST was used in a batch or transaction bundle and a resource id was already provided.
#. An invalid system URI was provided by default in AuditEvent.source.observer.identifier. Now ``http://vonk.fire.ly/fhir/sid/devices|firely-server`` is being used to identify Firely Server itself.
#. Adjusted the implementation of conditional create to match the description in https://jira.hl7.org/browse/FHIR-31965.
#. Money.currency was not indexed correctly in FHIR R4. Please :ref:`contact us<vonk-contact>` if you are using the SearchParameters "price-override" on ChargeItem or "totalgross" / "totalnet" on Invoice. A migration for these fields will be provided upon request. Otherwise, please re-index these SearchParameters. See :ref:`feature_customsp_reindex` for more details.
#. Fixed an issue where bundles with conformance claims in meta.profile would have been validated against the profile claims even if the validation level was only set to "Core".
#. Validating a resource with an element containing only an extension and no value against validation level "Core" will no longer result in an error.
#. SoF: Providing an invalid token to an unsecured operation does not lead to an HTTP 401 error status code. The invalid token is now being ignored.
#. SoF: Fixed unauthorized issue when performing PATCH request with ``patient`` scope.

Feature
^^^^^^^

#. Inferno, the ONC test tool: Firely Server is updated to pass all the tests in the latest ONC test kit (version 2.2.1)! Do you want a demo of this? :ref:`vonk-contact`.
#. Transactions, including rollbacks, are now fully supported when running Firely Server on MongoDB. Please note that the SimulateTransaction setting is no longer available. See :ref:`mongodb_transactions` for more details.
#. $lastN is now available if Firely Server is running on MongoDB. See :ref:`lastn` for more details.
#. It is now possible to define exclusion criteria in the appsettings to configure which requests against Firely Server should not be audited. In certain cases, this can reduce the number of captured AuditEvent resources. See :ref:`feature_auditing` for more details.
#. By default, the AuditEvent logging will now include the query parameters sent to Firely Server. These parameters will also be stored in case a request fails (HTTP 4xx or 5xx).
#. The log sinks for AuditEvent logging are now configurable in the logsettings. See :ref:`configure_audit_log_file` for more details.
#. Firely Server will throw a startup exception if no default ``ITerminologyService`` is registered.
#. CapabilityStatement.rest.resource.conditionalRead is now set to 'full-support' by default.
#. _total is now included in every self-link of a "search" bundle by default.
#. Added support for permanently deleting resources from the database. See :ref:`erase` for more details. You will need an updated license file. Please :ref:`contact us<vonk-contact>` if you want to use the feature.
#. Improved the error message in case the JSON serialization format of a FHIR resource does not contain a valid "resourceType" Element.
#. Improved validation in case a non-conformant URI is given in Quantity.system. It MUST be a valid absolute URI. In all other cases, a warning will be logged and the element will not be indexed.
#. Improved error message logging in case SQL script fails when the database upgrade is performed automatically by Firely Server.
#. Improved log message in case Firely Server SQL schema needs to be updated by adding the current schema version and the target schema version.
#. Improved access control by no longer allowing retrieval of resources outside of the Patient compartment if SMART on FHIR is enabled and patient-level scopes are provided by the client. Additional resources need to be explicitly allowed by the token.
#. Improved error message in case a condition create/update/delete operation is executed with SMART on FHIR enabled and the client provides a token with limited permissions (e.g. only write-scopes).

Performance
^^^^^^^^^^^

#. Improved validation performance of large resources. Firely Server will now execute the validation of bundles in a linear amount of time depending on the number of resources in the bundle.
#. Improved performance for chained searches in case SMART on FHIR is enabled.

.. _vonk_releasenotes_482:

Release 4.8.2, May 10th, 2022
-----------------------------

Feature
^^^^^^^

#. A new setting has been introduced in the "Hosting" settings to configure path base. Please check `Firely Server settings page <https://docs.fire.ly/projects/Firely-Server/en/latest/configuration/appsettings.html#http-and-https>`_ for details.

Fix
^^^

#. US-Core profiles in conformance resources database `vonkadmin.db` are downgraded from version `4.0.0 <http://hl7.org/fhir/us/core/>`_ to `3.1.1 <http://hl7.org/fhir/us/core/STU3.1.1/>`_. The upgrade in previous Firely Server was unintentional.
#. CapabilityStatement is cached now based on the absolute request url. With this fix, CapabilityStatement can be properly cached when a request contains `X-Forwarded-* headers <https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Forwarded>`_.
#. For MongoDB repository, set `allowDiskUse` to `true` when using `aggregate` command. This fix solves memory restriction error during aggregation stages (See `MongoDB document <https://www.mongodb.com/docs/manual/reference/command/aggregate/#command-fields>`_ for details). 

.. _vonk_releasenotes_481:

Release 4.8.1, Mar 5th, 2022
-----------------------------

Plugins
^^^^^^^

#. Upgraded the .NET SDK to 3.8.2. Please review its `release notes <https://github.com/FirelyTeam/firely-net-sdk/releases>`_ for changes.

Feature
^^^^^^^

#. A new option to configure settings regarding TLS client certificates has been introduced in the "Hosting" options. This option allows to set the `ClientCertificateMode <https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-6.0#client-certificates>`_.
#. Validation of transaction/batch bundles has been enabled by default when posting the resources to the transaction endpoint of Firely Server. Please note that the transaction is executed synchronously. To avoid client timeouts, the default value for the MaxBatchEntries (SizeLimits options) has been reduced to 200. 

.. _vonk_releasenotes_480:

Release 4.8.0, Mar 21st, 2022
-----------------------------

Plugins
^^^^^^^

#. Upgraded the .NET SDK to 3.8.0. Please review its `release notes <https://github.com/FirelyTeam/firely-net-sdk/releases>`_ for changes.

Database
^^^^^^^^

#. SQL Server

   1. Reduced database size by compressing the resource JSON.

   .. attention::

      This change requires a complex SQL migration which can be long if you have many resources. To estimate how long it will take for you, you can try running the migration for a subset of your data. The overall migration time will grow linearly with the number of resources in the database.

      For our test database containing ~185mln FHIR resources, the migration took approximately 1.5 days.

      If you have questions about the migration, please :ref:`contact us<vonk-contact>`.


Performance
^^^^^^^^^^^

#. Improved performance for update, _include/_revinclude and conditional create interactions

Feature
^^^^^^^

#. You can now control the inclusion of the ``fhirVersion`` mimetype parameter in the Content-Type header of the response. See :ref:`feature_multiversion_endpoints`. We chose to change the default for FHIR STU3 to *not* include it as this parameter was introduced with FHIR R4.

Fix
^^^

#. Fixed exception by improving transaction handling when updating and deleting the same resource in parallel.
#. Use correct restful interaction codes in AuditEvent.subtype when recording a request to Firely Server
#. AuditEvent.action contained the wrong code when recording a SEARCH interaction
#. The name of a custom operation is now recorded in an AuditEvent
#. Fixed searching using the :identifier modifier in case the identifier system is not a valid URL
#. Searching using a If-None-Exist header was not scoped to an information model, i.e. a request using FHIR R4 also matched STU3 resources
#. Improved error message if $lastN operation is enabled but the corresponding repository is not included in the pipeline options
#. Changed CapabilityStatement.software.name to Firely Server
#. Fixed SQL Server maintenance job timeouts on large SQL Server databases
#. Improved Bundle reference resolving in some corner cases, which are clarified in the `this HL7 Jira issue <https://jira.hl7.org/browse/FHIR-29271>`_

Security
^^^^^^^^

#. According to the `best practices <https://docs.docker.com/develop/develop-images/dockerfile_best-practices/#user>`_ of docker, Firely Server container runs now under the user and group ``firely:firely`` instead of running under ``root`` privileges.

Release 4.7.1, Feb 15th, 2022
-----------------------------

Fix
^^^

#. An invalid CapabilityStatement was created by Firely Server in case a custom SearchParameter overwriting a common SearchParameter was loaded, e.g. "_id". ``CapabilityStatement.rest.resource.searchParam.definition`` contains now the canonical of the more specific SearchParameter.

#. The default CapabilityStatement contained an invalid canonical in the .url element.

#. Enforce referential integrity for the elements "Composition.patient" and "Composition.encounter" when submitting a document bundle to the base endpoint. The corresponding resources need to be already present on the server (matching based on identifier), otherwise the bundle is rejected.

.. _vonk_releasenotes_470:

Release 4.7.0, Feb 1st, 2022
----------------------------

.. attention::    
    With version 4.7.0, Firely Server migrated to .NET 6.0. In order to run the binaries, `ASP.NET Core Runtime 6.x <https://dotnet.microsoft.com/en-us/download/dotnet/6.0>`_ needs to be installed.


Feature
^^^^^^^

#. BulkDataExport is now supported for MongoDB as well. Get started with the :ref:`Bulk Data Export documentation<feature_bulkdataexport>`.
#. Circular references in transaction bundles are now supported. Bundles of type ``transaction`` and ``batch`` are permitted to contain resources referencing another resource within the same bundle. This also means that you can now cross reference ``PUT`` and ``POST`` entries.
#. An option to configure additional token issuers is now available. This is used in settings where the token issuer deviates from the token audience. This new setting replaces the existing ``AdditionalEndpointBaseAddresses``. The setting needs to be adjusted manually as it will not be migrated automatically. Please check the :ref:`configuration documentation <feature_accesscontrol_config>` on how to use it.
#. Firely Server now supports receiving document bundles on the base endpoint. Firely Server will extract the narrative of document bundles and store this within a DocumentReference resource. Read more about it in the :ref:`documentation<restful_documenthandling>`.
#. Added support for transforming :ref:`SMART scopes issued by Azure Active Directory documentation<feature_accesscontrol_aad>`.
#. Firely Server will now recognize the ``name`` claim in JSON Web Tokens and also include its content in the logs.
#. It is now possible to :ref:`provide the Firely Server license via an environment variable<license_as_environment_variable>`.

Plugins
^^^^^^^

#. BulkDataExport interfaces were made publicly available in order to provide these to Firely Server's facade implementers. The Bulk Data Export page now has a section on :ref:`BDE for facades<feature_bulkdataexport_facade>`.
#. Upgraded the .NET SDK to 3.7.0. Please review its `release notes <https://github.com/FirelyTeam/firely-net-sdk/releases>`_ for changes.

Logging improvements
^^^^^^^^^^^^^^^^^^^^

#. Error messages including information about authorization validation and authentication requests are now enriched with user information if ``ShowAuthorizationPII`` is enabled :ref:`in the configuration <feature_accesscontrol_config>`.
#. Authorization/Authentication logging messages are now enriched with more information when logging level for the namespace ``Vonk.Smart`` is set to ``Debug``.
#. In case :ref:`SSL is activated<configure_hosting>`, but the ``.pfx`` file configured in ``CertificateFile`` could not be found, Firely Server will now log this error more explicitly. 

Fix
^^^

#. Fixed a bug where newly created SQL connections were not closed properly with the raw SQL configuration.
#. Fixed a bug that prevented searching on the ContactPoint datatype with a query of type ``system|value``. Although this combination is disallowed by the FHIR specification, Firely Server still allows it. We do not provide a migration for this issue. Please :ref:`vonk-contact` if this is an issue for you.
#. Fixed a bug that returned invalid self links without escaped whitespaces in bundles.
#. Improved support for use of Firely Server with Azure SQL. 

Other
^^^^^

#. Firely Server will no longer support CosmosDb starting with version 4.7.0.
#. The Docker image name has changed from `simplifier/vonk <https://hub.docker.com/repository/docker/simplifier/vonk>`_ to `firely/server <https://hub.docker.com/r/firely/server>`_. The old image name will be maintained for a few months to allow for a smooth transition. When updating to version 4.7.0, you should start to use the new image name. Versions 4.6.2 and older will stay available (only) on 'simplifier/vonk'.

.. _vonk_releasenotes_462:

Release 4.6.2, Dec 23rd, 2021
-----------------------------

Fix
^^^

#. ``IConformanceCacheR3`` and ``IConformanceCacheR4`` are registered again in the ServiceProvider for plugins that still make use of them. Note that these interfaces are obsolete by now, so make sure you don't use them for any new plugins. 

.. _vonk_releasenotes_461:

Release 4.6.1, Dec 15th, 2021
-----------------------------

Fix
^^^

#. Improved handling of TypeLoadException and ReflectionTypeLoadException when scanning external assemblies for SerializationSupportAttribute attributes. 


.. _vonk_releasenotes_460:

Release 4.6.0, Nov 18th, 2021
-----------------------------

Database
^^^^^^^^

#. SQL Server (all changes below applicable only when plugin ``Vonk.Repository.Sql.Raw`` is enabled)

   1. A new computed column IsDeleted on table [vonk].[entry] is leveraged for more performant SQL queries
   
   .. note::

      The performance of the old ``Vonk.Repository.Sql`` may be adversely impacted by this change. We encourage you to use the new ``Vonk.Repository.Sql.Raw`` implementation.

   2. Improved performance of SQL queries by converting 5 columns from [vonk].[entry] to varchar upon retrieval: InformationModel, Type, ResourceId, Version, Reference

   .. note::
      
      These columns should - by definition of the FHIR datatypes - not contain characters outside the varchar range, but please pay attention to this change if your id's or custom resource type has those characters nonetheless. We may alter the datatype of the columns in a future release.
   
   3. Improved performance of some SQL queries by avoiding unnecessary SQL query parameter type conversion

   4. Improved performance of some SQL queries by avoiding excessive retrieval of the (large) ResourceJson column
   
#. MongoDB

   #. Improved performance of searches within a compartment
   #. Added an index ``ix_sysinfo`` to quickly retrieve the ``VonkVersion`` document.

Features
^^^^^^^^

#. Added support for SMART on FHIR v2

.. note::

   Since most users currently use SMART on FHIR v1, the plugin for v2 is by default *disabled* in the PipelineOptions. You can switch v1 out and v2 in when you want to test the use of v2.

Logging improvements
^^^^^^^^^^^^^^^^^^^^

#. The password and the username are stripped out from a connection string when it gets logged (SQL Server / Sqlite, Verbose log level)
#. SQL param values are not logged by default. This can be enabled by using a new config setting. See :ref:`configure_log_database_query_params` (SQL Server / Sqlite, Verbose log level)
#. Username and UserId are included in log and audit entries (when using SoF or another authentication plugin)
#. SQL query duration now gets logged (changed for ``Vonk.Repository.Sql.Raw.KSearchConfiguration`` plugin; was always available for other repository plugins, Verbose log level)
#. Fixed category names for some log entries to include the fully qualified type of their source. For example, category ``MetadataConfiguration`` was changed to ``Vonk.Core.Metadata.MetadataConfiguration``, and category ``BulkDataExportConfiguration`` was changed to ``Vonk.Plugin.BulkDataExport.BulkDataExportConfiguration``, etc.

Fix
^^^

#. Fixed a bug when validation was not performed on PATCH requests even when the validation level was set to Full
#. Fixed a bug when escaping of the pipe ('|') character was not working as expected for token search parameters
#. Improved error handling when FS tries to load a non-.NET DLL from the plugins directory
#. Fixed a bug (introduced in 4.5.1) when a compartment matches more than 1 Patient
#. Fix: $validate checks whether a system parameter is provided
#. Fix: ``Vonk.Repository.Sql.Raw``: searching on quantities with values having a high precision failed

Other
^^^^^

#. Firely SDK upgraded from v3.0.0 to v3.6.0. See the SDK release notes `here <https://github.com/FirelyTeam/firely-net-sdk/releases>`_

.. note::

   This will make Firely Server import a new version of specification.zip into the Administration endpoint for each FHIR version. If you share the Administration database among instances, allow 1 instance to finish this process before starting the other instances.

.. _vonk_releasenotes_451:

Release 4.5.1
-------------

.. attention::
    The upgrade procedure for Firely Server running on MongoDb will execute an upgrade script that adds a new field to store precalculated compartment links. If your collection contains a lot of resources, this may take a very long time. Therefore, the MongoDb upgrade script has to be executed manually. The script can be found in `mongodb\FS_SchemaUpgrade_Data_v17_v18.js`
    
    Here are some guidelines:

   * We tested it on a MongoDb collection with about 400k documents in total. The upgrade script took around 3.5 minutes to complete on a fairly powerful laptop.
   * As always, make sure you have a backup of your database that has been tried and tested before you begin the upgrade.
   * Please make sure that Firely Server is shutdown before you execute the script.
   * If you encounter problems running the script, or need any assistance, please :ref:`vonk-contact`.

Database
^^^^^^^^

#. MongoDB

   #. The migration script 'FS_SchemaUpgrade_Data_v17_v18.js' has been fixed. All data present in the database before the migration is now again accessible after the migration.
   
#. SQL Server

   #. Improved the query performance when using _include by using "WITH FORCESEEK".
   #. Improved performance by avoiding scanning indexes when searching on the UriHash column
   
Fix
^^^

#. Firely Server will now by default include a user-agent header when retrieving the SMART Discovery document

.. _vonk_releasenotes_450:

Release 4.5.0
-------------

Database
^^^^^^^^

.. attention::
	The release version of the MongoDB migration contains an error causing compartment searches to return no search results for all migrated resources. Only newly added resources after the migration will be returned successfully. In :ref:`vonk_releasenotes_451` we have fixed this issue, so please use that version instead.

#. MongoDB

   #. To improve the performance of compartment searches, Firely Server now precalculates the compartment links to which a resource belongs on insert in the database. An external migration script 'FS_SchemaUpgrade_Data_v17_v18.js' is provided in the distribution. It needs to be applied manually using MongoDB Shell.

Security
^^^^^^^^

#. A VonkConfigurationException, which was thrown if a SQL database migration could not be performed, included the SQL connection string in plain text in the log. Please check you log files if they include any sensitive information such as the database password, which might have been part of the connection string.

Fix
^^^

#. It is now possible to configure pre- and post-handlers for a custom operations using VonkInteraction.all_custom regardless of the interaction level of the operation handler and the interaction level on which the operation is configured in the appsettings.
#. $lastN could not handle chained arguments on the subject/patient reference
#. $lastN reported an invalid error message if the reference to a subject/patient was provided as an urn:uuid reference
#. $lastN search result bundles were missing self-links when no results were found
#. Disabling Vonk.Fhir.R4 in the pipeline resulted in an internal exception thrown by the ConformanceCache

Feature
^^^^^^^

#. $lastN can be combined with _elements and _include parameters
#. $lastN can group the results by the ``component-code`` or ``combo-code`` search parameter

Documentation
^^^^^^^^^^^^^

#. Added an explanation to the documentation why the use of ``_total=none`` influences the performance of a search query.

Plugins
^^^^^^^

#. The FHIR Mapper is no longer distributed together with Firely Server. Please contact fhir@healex.systems for any questions regarding the FHIR Mapper.
#. The packages Vonk.Fhir.R(3|4) depended on an unpublished NuGET package Vonk.Administration.Api.
#. All classes in the namespace 'Vonk.Facade.Relational' are now published on `GitHub <https://github.com/FirelyTeam/Vonk.Facade.Relational>`_.

.. _vonk_releasenotes_450-beta:

Release 4.5.0-beta
------------------

Fix
^^^

#. Security: Added a warning to the documentation that using compartments other than 'Patient' to restrict access based on patient-level SMART on FHIR scopes may result in undesired behavior. See :ref:`feature_accesscontrol_compartment` for more information.
#. The RequestCountService caused an exception on startup if the RequestInfoFile could not be accessed, e.g. due to limited filesystem permissions. The RequestCountService has been removed completely. Any remaining .vonk-request-info.json files can be deleted manually.
#. The logsettings for SQL server included an outdated configuration.
#. The logsettings for MongoDB included an outdated configuration.

Feature
^^^^^^^

#. Improved error messages if an internal exception occurred due to failing filesystem access.
#. The `$lastN operation <https://www.hl7.org/fhir/observation-operation-lastn.html>`_ is now available when using SQL Server as the backend for Firely Server. See :ref:`lastn` for more information.

Plugin and Facade
^^^^^^^^^^^^^^^^^

#. Added async support for the ISnapshotGenerator interface and its implementations.

.. _vonk_releasenotes_440:

Release 4.4.0
-------------

Database
^^^^^^^^

#. MongoDB

   #. To improve the performance of deletes, the definition of the index ``ix_container_id`` is redefined. Firely Server 4.4.0 will automatically change the definition.

#. SQL Server

   #. Improved query behind ``_include`` to leverage an index. No changes to the database schema involved. This only affects the new implementation (available since 4.3.0).

Fix
^^^

#. Improved automatic upgrading of terminology settings from pre-4.1.0 instances.
#. Added ``CapabilityStatement.status`` for R4
#. The default ``SmartAuthorizationOptions`` in ``appsettings.default.json`` only have the Filter for 'Patient' enabled. The rest is now commented out as those are generally not used.

Plugin and Facade
^^^^^^^^^^^^^^^^^

#. The interfaces PrioritizedResourceResolver(R3|R4|R5) and their implementations are no longer available. It is advised to construct your own StructureDefinitionSummaryProvider incl. a MultiResolver combining your own resource resolver and the IConformanceCache provided by Firely Server.
#. The interface IConformanceCacheInvalidation has been moved from Vonk.Core.Import to Vonk.Core.Conformance
#. The classes SpecificationZipResolver(R3|R4|R5) are no longer available. Please use the IPrioritizedResourceResolvers instead.
#. Starting from this version, a Facade should not have an order greater than or equal to 211. The reason for this is that upon configuring the administration database, Firely Server checks whether an ISearchRepository is registered. The earliest of these configurations is at order 211.

.. _vonk_releasenotes_430:

Release 4.3.0
-------------

Database
^^^^^^^^

#. SQL Server

   #. To improve the performance of searching we have rewritten a large part of our SQL Server implementation. To be able to use the new implementation go to section PipelineOptions in ``appsettings.default.json`` (or ``appsettings.instance.json`` if you have overridden the default pipeline options) and add ``"Vonk.Repository.Sql.Raw.KSearchConfiguration"``. See :ref:`configure_sql` for more details.
   #. We have identified two indexes that needed a fix to increase query performance for certain searches. The upgrade procedure will try to fix these indexes automatically. If your database is large, this may take too long and the upgrade process will time out. If that happens you need to run the upgrade script manually, The script can be found in ``sqlserver/FS_SchemaUpgrade_Data_v19_v20.sql``. If you use SQL Server as your Administration database, Firely Server will try to update it automatically as well. If you prefer a manual update, you can run the following script: ``sqlserver/FS_SchemaUpgrade_Admin_v18_v19.sql``.

Feature
^^^^^^^

#. Firely Server now allows you to execute a ValueSet expansion of large ValueSets (> 500 included concepts). Previously, Firely Server would log an error outlining that the expansion was not possible. The appsettings now contain a setting in the Terminology section allowing to select the MaxExpansionSize. See :ref:`feature_terminologyoptions` for more details.

Fix
^^^

#. Fixed a NullPointerException which occurred when indexing UCUM quantities that contained more than one annotation (e.g. "{reads}/{base}").
#. Fixed a bug where it was possible to accidentally delete a resource with a different information model then the request. Firely Server will now check the information model of the request against the information model of the resource for conditional delete and delete requests.
#. $subsumes returned HTTP 501 - Not implemented for a POST request (instance-level) even if the operation was enabled in the appsettings.
#. The _type filter on $everything and Bulk data export didn't allow for resources that are not within the Patient compartment. The operations would return an empty result set.
#. Added a clarification to the documentation that $everything and Bulk data export do not export Device resources by default. Even though the resource contains a reference to Patient, the corresponding compartment definition for Patient does not include Device as a linked resource. It is possible to export Device resources by adding the resource type to "AdditionalResources" settings of the operations.

.. _vonk_releasenotes_421:

Release 4.2.1 hotfix
--------------------

Database
^^^^^^^^
.. note::
   We found an issue in version 4.2.0, which affects the query performance for Firely Server running on a SQL Server database. If your are running FS v4.2.0 on SQL Server you should upgrade to v4.2.1 or if that is not possible, :ref:`vonk-contact`.

.. attention::
    The upgrade procedure will execute a SQL script try to validate the foreign key constraints. If your database is large, this may take too long and the upgrade process will time out. If that happens you need to run the upgrade script manually, The script can be found in ``data/20210720085032_EnableCheckConstraintForForeignKey.sql``.
    
    Here are some guidelines:

   * We tested it on a database with about 15k Patient records, and 14 million resources in total. The upgrade script took about 20 seconds to complete on a fairly powerful laptop.
   * As always, make sure you have a backup of your database that has been tried and tested before you begin the upgrade.
   * If you expect the upgrade to time out, you can choose to run the SQL script manually beforehand. Please make sure that Firely Server is shutdown before you execute the script.

Fix
^^^
#. Fixed a bug where some of the Foreign Keys in SQL Server had become untrusted. This bug has an impact on the query performance since the the SQL Server query optimizer will not consider FKs when they are not trusted. This has been fixed, all Foreign Keys have been validated and are trusted again.

.. _vonk_releasenotes_420:

Release 4.2.0
-------------

Database
^^^^^^^^

.. attention::
   For SQL Server users: this version of Firely Server running on SQL Server has a bug where some of the Foreign Keys became untrusted. This has an impact on the query performance. Please upgrade to version 4.2.1 or if that is not possible, :ref:`vonk-contact`.
   Please note that users running Firely Server running either MongoDb, CosmoDb, or SQLite are not affected by this issue.

.. attention::
   For SQL Server we changed the datatype of the primary keys. The related upgrade script (``data/20210519072216_ChangePrimaryKeyTypeFromIntToBigint.sql``) can take a lot of time if you have many resources loaded in your database. Therefore some guidelines:

   * We tested it on a database with about 15k Patient records, and 14 mln resources in total. Migrating that took about 50 minutes on a fairly powerful laptop.
   * Absolutely make sure you create a backup of your database first.
   * If you haven't done so already, first upgrade to version 4.1.x.
   * If you already expect the migration might time out, you can run it manually upfront. Shut down Firely Server, so no other users are using the database, and then run the script from SQL Server Management Studio (or a similar tool).
   * Running the second script (``20210520102224_ChangePrimaryKeyTypeFromIntToBigintBDE.sql``) is optional - that should also succeed when applied by the auto-migration.

Feature
^^^^^^^

#. Terminology operation ``$lookup`` is now also connected to remote terminology services, if enabled. See :ref:`feature_terminology`.
#. We provided a script to 'purge' data from a SQL Server database. See ``data/20210512_Purge.sql``. You can filter on the resource type only. Use with care and after a backup. If you need more elaborate support for hard deletes, please :ref:`vonk-contact`.

Fix
^^^
#. Firely Server could run out of primary keys on the index tables in SQL Server. Fixed by upgrading to bigint, see warning above.
#. Nicer handling of SQL Server migration scripts that time out on startup. It will now kindly ask you to run the related script manually if needed (usually depends on the size of your database).
#. The Patient-everything (``$everything``) operation was not mentioned in the CapabilityStatement.
#. License expired one day too early.
#. Dependencies have been upgraded to the latest versions compatible with .NET Core 3.1.
#. PATCH did not allow adding to a repeating element.
#. If your license does not allow usage of SMART on FHIR, authorization was disabled, emitting a warning in the log. Possibly causing unauthorized access without the administrator noticing it. This specific case will now block the startup of Firely Server. 

.. _vonk_releasenotes_413:

Release 4.1.3 hotfix
--------------------

Fix
^^^
#. Fixed a bug where a number of concurrent $transform requests on a freshly started Firely Server could lead to Internal Server Error responses.
#. Upgraded the Mapping plugin.

.. _vonk_releasenotes_412:

Release 4.1.2 hotfix
--------------------

Fix
^^^
#. Fixed a bug when trying to delete multiple resources at once (bulk delete, see :ref:`restful_crud_configuration` for configuration options). The operation would take a while and eventually return a ``204 No Content`` without actually deleting any resources. This is fixed, the bulk delete operation now deletes the resources.

.. _vonk_releasenotes_411:

Release 4.1.1 hotfix
--------------------

Feature
^^^^^^^
#. SMART configuration: Some identity providers use multiple endpoints with different base addresses for its authorization operations. Added an extra configuration option ``AdditionalEndpointBaseAddresses`` to define additional base endpoints addresses next to the main authority endpoint to accommodate this. See :ref:`feature_accesscontrol_config` for further details.

Fix
^^^
#. Fixed an error in SQL script ``data/20210226200007_UpdateIndexesTokenAndDatetime_Up.sql`` that is used when manually updating the database to v4.1.0. We also made the script more robust by checking if the current version the database is suitable for the manual upgrade.

.. _vonk_releasenotes_410:

Release 4.1.0
-------------

.. attention::

   We have found an issue with SMART on FHIR and searching with _(rev)include. And fixed it right away, see Fix nr 1 below.
   Your Firely Server might be affected if:

   * you enabled SMART on FHIR
   * and used patient/read.* scopes together with a patient compartment

   What happens? Patient A searches Firely Server with a patient launch scope that limits him to his own compartment. If any of the resources in his compartment links to *another* patient (let's say for Observation X, the performer is Patient B), Patient A could get to Patient B with ``<base>/Observation?_include=Observation.performer``. If you host Group or List resources on your server, a _revinclude on those might give access to other Patient resources within the same Group or List.  
   
   If you think you might be affected you can:

   * upgrade to version 4.1.0
   * or if that is not possible, :ref:`vonk-contact`.
   
Database
^^^^^^^^

#. SQL Server
   
   #. A new index table was added. The upgrade procedure will try to fill this table based on existing data. If your database is large, this may take too long and time out. Then you need to run the upgrade script found in ``data/20210303100326_AddCompartmentComponentTable.sql`` manually. 
   #. A new SQL Server index was added to improve query times when searching with date parameters. The upgrade procedure will try to build this index. If your database is large, this may take too long and time out. Then you need to run the upgrade script found in ``data/20210226200007_UpdateIndexesTokenAndDatetime_Up.sql`` manually.
   #. In both cases you may also run the script manually beforehand. 
   #. As always: make sure you have a backup of your database that is tested for restore as well.

DevOps
^^^^^^

.. attention::

   Because of a change in the devops pipeline there is no longer a ``Firely.Server.exe`` (formerly ``Vonk.Server.exe``) in the distribution zip file. You can run the server as always with ``dotnet ./Firely.Server.dll``

Features
^^^^^^^^

#. Inferno, The ONC test tool: Firely Server now passes all the tests in this suite! With version 4.1.0 we specifically added features to pass the 'Multi-patient API' tests. Do you want a demo of this? :ref:`vonk-contact`!. 

#. Terminology support has been revamped. Previously you needed to choose between using the terminology services internal to Firely Server *or* external terminology services like from OntoServer or Loinc. With this version you can use both, and based on the codesystem or valueset involved the preferred terminology service is selected and queried. 

   #. This works for terminology operations like ``$validate-code`` and ``$lookup``
   #. It also works for validation, both explicitly with ``$validate`` and implicitly, when validating resources sent to Firely Server. 
   #. The CodeSystem, ValueSet and ConceptMap resources involved are conformance resources and therefore always retrieved from the Administration database.
   #. Responses may differ on details from previous versions of Firely Server, but still conform to the specification.
   #. See :ref:`feature_terminology` for further details.

#. ``$everything``: We now support the :ref:`feature_patienteverything` operation for single Patients. (For multiple patients, there is the Bulk Data Export feature.)
#. Performance of $everything, Bulk Data Export and authorization on compartments improved. We added a special index to the database that keeps track which resource belongs to which compartment. First in SQL Server, MongoDB has less need for it. 
#. SMART on FHIR: Support for token revocation. Reference tokens can be revoked, and Firely Server can check for the revocation.

Fixes
^^^^^

#. SMART on FHIR: We have found ourselves that the authorization restrictions were bypassed when using _include or _revinclude in a FHIR Search. We solved this security issue immediately. 
#. Firely Server transparently translates absolute urls to relative urls (for internal storage) and back. There was a performance gain to be made in this part, which we did. This is mostly notable on large transaction or batch bundles.
#. Batch bundles are not allowed to have links between the resources in the entries. Firely Server will now reject batch bundles that have these links. If you need links, use a transaction bundle instead.

Plugin and Facade
^^^^^^^^^^^^^^^^^

#. We upgraded the Firely .NET SDK to version `3.0.0 <https://github.com/FirelyTeam/firely-net-sdk/releases/tag/v3.0.0-stu3>`_. This SDK version is almost fully compatible with 2.9, but it brings significant simplifications to its use because the Parameters and OperationOutcome resource POCOs are no longer FHIR-version specific. 

   .. note::

      Every new version of the SDK brings new versions of the ``specification.zip`` files. So upon upgrade these new files will be read into the Administration database. See :ref:`conformance` for more background.

.. _vonk_releasenotes_400:

Release 4.0.0
-------------

This major version introduces a new name: **Firely Server** instead of Vonk. Other than that, this release contains some significant code changes, which could impact you if you run Firely Server with your own plugins.

Features
^^^^^^^^

#. Name change Vonk -> Firely Server:

   #. The main entry point dll (formerly: ``Vonk.Server.dll``) and executable (formerly: ``Vonk.Server.exe``) names have been changed to ``Firely.Server.dll`` and ``Firely.Server.exe`` respectively.
   #. The name was changed in the CapabilityStatement.name.
   #. The name of the download zip (from Simplifier) has changed from `vonk_distribution.zip` to `firely-server-latest.zip`. Likewise the versioned zip files have changed as well.

#. We have implemented FHIR Bulk Data Access (``$export``) to allow for fast, asynchronous ndjson data exports. The :ref:`Bulk Data Export documentation<feature_bulkdataexport>` can help you to get started.
#. Firely Server now uses Firely .NET SDK 2.0.2 (formerly: FHIR .NET API)

   .. attention::
   
      If you are running Firely Server with your own self-made plugins, you will likely encounter package versioning problems and need to upgrade your NuGet Firely Server package references (package names starting with ``Vonk.``) to version 4.0.0. You also need to upgrade any Firely .NET SDK package references (package names starting with ``Hl7.Fhir.``) to version 2.0.2. The `Firely .NET SDK release notes <https://github.com/FirelyTeam/firely-net-sdk/releases>`_ and `Breaking changes in Firely SDK 2.0 <https://github.com/FirelyTeam/firely-net-sdk/wiki/Breaking-changes-in-2.0>`_ can give you an idea of the changes you may encounter in the SDK.

#. SMART on FHIR can now recognize prefixes to the claims, see its :ref:`feature_accesscontrol_config`.
#. The smart-configuration endpoint (`<url>/.well-known/smart-configuration`) relays the signature algorithms configured in the authorization server.


Fixes
^^^^^

#. Application Insights has now been disabled by default. If you need Application Insights, you can enable it in your log settings file by including the entire section mentioned in :ref:`Application Insights log settings<configure_log_insights>`.
#. When validating a resource, a non-existing code would lead to an OperationOutcome.issue with the code ``code-invalid``. That issue code has been changed to ``not-supported``.
#. On a batch or transaction bundle errors were not reported clearly if the entry in error had no fullUrl element. We fixed this by referring to the index of the entry in the entry array, and the resource type of the resource in the entry (if any).
#. The ``import[.R4]`` folder allows for importing custom StructureDefinition resources. If any of them had no id, the error on that caused an exception. Fixed that.
#. If a Facade returned a resource without an id from the Create method, an error was caused by a log statement. Fixed that.
#. Indexing ``Subscription.channel[0].endpoint[0]`` failed for R4. Fixed that. This means you can't search for existing Subscriptions by ``Subscription.url`` on the /administration endpoint for FHIR R4.
#. Postman was updated w.r.t. acquiring tokens. We adjusted the :ref:`documentation on that <firely_auth_introduction>` accordingly.
#. If a patient claim was included in a SMART on FHIR access token, the request would be scoped to the Patient compartment regardless of the scope claims. We fixed this by allowing "user" scopes to access FHIR resources outside of the Patient compartment regardless of the patient claim. See `Launch context arrives with your access_token <http://hl7.org/fhir/smart-app-launch/1.0.0/scopes-and-launch-context/index.html#launch-context-arrives-with-your-access_token>`_ for more background information.

Plugin and Facade
^^^^^^^^^^^^^^^^^

#. The mapping plugin is upgraded to the Mapping Engine 0.6.0.
#. As announced in :ref:`vonk_releasenotes_300` we removed support for creating a Facade as a standalone ASP.Net Core project. You can now only build a Facade as a plugin to Firely Server. See :ref:`vonk_facade` on how to do that.
#. The order of some plugins has changed. This way it is possible to add a plugin between PreValidation and UrlMapping:

   * :ref:`UrlMapping<vonk_plugins_urlmapping>`: from 1230 to 1235
   * :ref:`Prevalidation<vonk_plugins_prevalidation>`: from 4320 to 1228

#. A Facade based on ``Vonk.Facade.Relational`` no longer defaults to STU3

   .. attention::

	  If you developed a facade plugin based on ``Vonk.Facade.Relational``, you need to override ``RelationalQueryFactory.EntryInformationModel(string informationModel)`` in your implementation to allow the FHIR version you wish to target (see :ref:`facade_fhir_version`)

#. We took the opportunity of a major version upgrade to clean up a list of items that had been declared ``Obsolete`` already. Others have become obsolete now. This is the full list:

   # ``Obsolete``, now deleted:

      # Vonk.Core.Common.DeletedResource
      # Vonk.Core.Common.IResource.Currency, Change and Clone(), also in VonkResource.
      # Vonk.Core.Common.IResourceExtensions.ToIResource(this ISourceNode original, ResourceChange change, ResourceCurrency currency = ResourceCurrency.Current) (the overload defaulting to STU3)
      # Vonk.Core.Context.Guards.SupportedInteractionOptions.SupportsCustomOperationOnLevel()
      # Vonk.Core.Context.Internal.BatchOptions
      # Vonk.Core.Operations.Validation.ValidationOptions
      # Vonk.Core.Pluggability.InteractionHandlerAttribute.Tag
      # Vonk.Core.Pluggability.ModelOptions
      # Vonk.Core.Repository.SearchOptions.LatestOne
      # Vonk.Core.Support.LogHelpers.TryGetTelemetryClient, both overloads.
      # Vonk.Core.Support.SpecificationZipLocator.ctor(IHostingEnvironment…)
      # Vonk.Fhir.R3.IResourceVisitor + extensions
      # Vonk.Fhir.R3.Configuration.ModelContributorsFacadeConfiguration
      # Vonk.Fhir.R3.FhirExtensions.AsIResource()
      # Vonk.Fhir.R3.FhirPropertyIndex + FhirPropertyInfo + FhirPropertyIndexBuilder
      # Vonk.Fhir.R3.IConformanceBuilder + BaseConformanceBuilder + HarvestingConformanceBuilder + extensions + IConformanceContributor
      # Vonk.Fhir.R3.CompartmentDefinitionLoader + (I)SearchParameterLoader
      # Vonk.Fhir.R3.MetadataImportOptions + MetadataImportSet + ImportSource
      # Vonk.Fhir.R3.PocoResource + PocoResourceVisitor
      # Vonk.Core.InformationModelAttribute (actually made internal)

   # ``Obsolete`` since this version:

      # Vonk.Core.Configuration.CoreConfiguration: allows for integrating Vonk components in your own ASP.NET Web server, discouraged per 3.0 (see these releasenotes).
      # Vonk.Fhir.R3.FhirR3FacadeConfiguration: see above.