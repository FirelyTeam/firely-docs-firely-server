.. _vonk_releasenotes_history_v3:

Old Firely Server release notes (v3.x)
======================================

.. _vonk_releasenotes_393:

Release 3.9.3 hotfix
--------------------

.. attention::

   We changed the behavior of resthook notifications on Subscriptions. See Fix nr 1 below.

Database
^^^^^^^^

#. SQL Server: The migration that adds the indexes described in :ref:`vonk_releasenotes_392` might run longer than the timeout period of 30 seconds. Therefore we added scripts to apply and revert this migration manually. If you encounter the timeout during upgrade: shut down vonk, run the script using SQL Server Management Studio or any similar tool, then start Vonk 3.9.3 again. In both scripts you only need to provide the database name for the database that you want to upgrade. If you run your administration database on SQL Server you can but probably do not need to run this script on it. The administration database is typically small enough to complete the script within 30 seconds.

   #. apply: <vonk-dir>/data/2021211113200_AddIndex_ForCountAndUpdateCurrent_Up.sql
   #. revert: <vonk-dir>/data/2021211113200_AddIndex_ForCountAndUpdateCurrent_Down.sql

Fix
^^^

#. :ref:`feature_subscription`: A resthook notification was sent as a FHIR create operation, using POST. This was not compliant with the specification that states it must be an update, using PUT. We changed the default behavior to align with the specification. In order to avoid breaking changes in an existing deployments, you may set the new setting ``SubscriptionEvaluatorOptions:SendRestHookAsCreate`` to ``true`` - that way Vonk will retain the (incorrect) behavior from the previous versions.

.. _vonk_releasenotes_392:

Release 3.9.2 hotfix
--------------------

Fix
^^^

All fixes are relevant to SQL Server only.

#. The 3.9.0 fix that "Improved the handling of concurrent updates on the same resource." decreased the performance of concurrent transaction handling. We implemented another solution that does not affect performance.
#. Improved read performance by adding an index.

.. _vonk_releasenotes_391:

Release 3.9.1 hotfix
--------------------

Fix
^^^
#. Fixed a bug introduced with 3.9.0 were Vonk would throw the following exception on start-up ``System.InvalidOperationException: Unable to resolve service for type 'Vonk.Core.Conformance.IDefinitionProvider' while attempting to activate 'Vonk.Fhir.R3.SnapshotGeneration.SnapshotGeneratorR3``
#. Fixed a breaking change to public search API with the implementation of ``_total`` parameter. We had introduced a new parameter to the Next method in ResultPage, effectively breaking backwards compatibility. This has been fixed.


.. _vonk_releasenotes_390:

Release 3.9.0
-------------

Features
^^^^^^^^

#. We have made Subscriptions more robust. See :ref:`feature_subscription` for details. In summary, if an evaluation of a Subscription fails, Vonk will retry the evaluation periodically for a number amount of tries. You can control the retry period and the maximum number of retries in the subscription settings:
   
   * ``RetryPeriod`` is expressed in milliseconds. Default ``30000`` (30 sec).
   * ``MaximumRetries`` is the maximum amount of times Vonk will retry to send the resources. Default ``3`` retries.
   
#. We have implemented the ``_total`` parameter for options ``none`` and ``accurate``. Omitting the ``_total`` parameter is equivalent to ``_total=accurate``. If the total number of resources is not relevant, using ``_total=none`` in the request results in better performance when searching.
#. It is no longer necessary for the ``:type`` parameter to always be provided to distinguish between multiple reference targets. The parameter does not need to be provided anymore when the search only applies to a single target.
   For example: ``GET <base>/AllergyIntolerance?patient=xyz``
   The ``patient:Patient`` type parameter does not have to be supplied. The 'patient' search parameter on AllergyIntolerance has two possible targets. It may reference either a Patient or a Group resource. However, the fhirpath statement that goes with it, selects ‘AllergyIntolerance.patient', and that reference element may only target a Patient resource. 

Fixes
^^^^^

#. Indexing values for a string search parameter threw an exception when there was no value but only an extension. This has been corrected.
#. We made the Provenance.target available as a revInclude Parameter in the CapabilityStatement. Previously, Vonk did not account for the case that a reference is allowed to ANY resource type, which incorrectly resulted in Provenance.target to not be shown in the CapabilityStatement.

All the following fixes are only relevant for SQL Server:

#. Improved the handling of concurrent updates on the same resource.
#. Upgraded the version of the SqlClient library to fix issues when running on Linux.
#. Fixed missing language libraries for SQL Server when running on Docker.


.. _vonk_releasenotes_380:

Release 3.8.0
-------------

Database
^^^^^^^^

* We added an important note to the :ref:`3.6.0 release notes <vonk_releasenotes_360>` for MongoDb users.
* Because of the changes in searching for Quantities (feature 2 below), you will need to do a :ref:`reindex <feature_customsp_reindex>` in order to make use of this. You may limit the reindex to only the search parameters of type 'quantity' that you actually use (e.g. ``Observation.value-quantity``).

Features
^^^^^^^^

#. We upgraded the FHIR .NET API to 1.9, see the `1.9 releasenotes <https://github.com/FirelyTeam/firely-net-sdk/releases>`_. This will trigger an automatic :ref:`import of the Conformance Resources <conformance_specification_zip>` at startup.
#. We upgraded the `Fhir.Metrics library <https://github.com/FirelyTeam/fhir.metrics>`_ to 1.2. This allows for a more uniform search on Quantities (mainly under the hood)
#. We upgraded the FHIR Mapping plugin to support the FHIR Mapper version 0.5.
#. The :ref:`built-in terminology services <feature_terminology>` now support the ``includeDesignations`` parameter. 
#. The :ref:`vonk_reference_api_ivonkcontext` now lets you access the original HttpContext.
#. The CapabilityStatement now lists the profiles that are known to Vonk (in its Administration database) under ``CapabilityStatement.rest.resource.supportedProfile`` (>= R4 only) and the base profile for a resource under ``CapabilityStatement.rest.resource.profile``.
#. We extended the security extension on the CapabilityStatement to include the endpoints for ``register``, ``manage``, ``introspect`` and ``revoke``.
#. ``IAdministrationSearchRepository`` and ``IAdministrationChangeRepository`` interfaces are now publicly available. Use with care.


Fixes
^^^^^

#. If the server configured as authorization endpoint in the Smart setting is not reachable, Vonk will log a proper error about that.
#. An error message for when a query argument has no value is improved.
#. When :ref:`SMART-on-FHIR <feature_accesscontrol>` is enabled, and the received token contains a launch context, the :ref:`_history<restful_history>` operation is no longer available. Because Vonk does not retain the search parameter index for historical resources, it cannot guarantee that these resources fall within the launch context (at least not in a performant way). To avoid information leakage we decided to disable this case altogether.
#. A Create interaction without an id in the resource, with :ref:`SMART-on-FHIR <feature_accesscontrol>` enabled, resulted in an exception.
#. You can now escape the question mark '?' in a query argument by prepending it with a backslash '\'.
#. A Quantity search using 'lt' on MongoDb resulted in too many results. 

.. _vonk_releasenotes_370:

Release 3.7.0
-------------

Database
^^^^^^^^

.. attention::

   To accommodate for feature #2 below there is an automatic migration carried out for SQL Server and SQLite. This migration might take some time, so please test it first. For MongoDb, you will have to :ref:`feature_customsp_reindex_all`. If this is not feasible for your database, please :ref:`vonk-contact` for assistance.

Features
^^^^^^^^

#. Patch: We implemented FHIR Patch. You can now update a resource having only partial data for it. See :ref:`restful_crud`.
#. Search on accents and combined characters: we improved searching with and on accents and combined characters. Note the database change above.
#. API 1.7: We upgraded Vonk to use the FHIR .NET API 1.7, having its own `releasenotes <https://github.com/FirelyTeam/firely-net-sdk/releases/tag/v1.7.0-beta-may2020-r5>`__.
#. Security: The Docker image is now based on the Alpine image for .NET Core. This has far less security issues than the Ubuntu image that we used before. The base image is aspnet:3.1-alpine:3.11 (newest version 3.12 has an open bug related to SQLite).
#. Security: We revisited the list of security vulnerabilities, see :ref:`vonk_securitynotes`.
#. Administration: ConceptMaps are now :ref:`imported <conformance_import>` at startup.

Fixes
^^^^^

#. Searching on _lastUpdated could be inaccurate when time zone differences are in play. We fixed that.
#. Search arguments for a quantity search weren't allowed to be greater than 999.

.. _vonk_releasenotes_361:

Release 3.6.1
-------------

Features
^^^^^^^^

#. ConceptMap resources can be stored at the Administration endpoint, both through :ref:`import <conformance_import>` and through the :ref:`RESTful API <conformance_on_demand>`.

.. attention::

   Previous versions of Vonk did not include the ConceptMap resources in the import so they will currently not be in your Administration database. If you run your Administration database on SQL Server or MongoDb and want to use the ConceptMap resources from the spec, be sure to rerun the import of the specification resources. You can force Vonk to do so by deleting the ``.vonk-import-history.json`` file from the ImportedDirectory (see the settings under ``AdministrationImportOptions``). If you use SQLite, you can simply use the pre-built ``./data/admin.db`` from the binaries.

Plugins
^^^^^^^

#. The FHIR Mapper plugin is upgraded to version 0.3.6.
#. The FHIR Mapper plugin now fully works on the *Administration* endpoint.


.. _vonk_releasenotes_360:

Release 3.6.0
-------------

Database
^^^^^^^^

.. attention::

   For MongoDb users: We implemented feature #1 below using the Aggregation Pipeline. This makes an existing issue in MongoDb - `SERVER-7568 <https://jira.mongodb.org/browse/SERVER-7568>` - a more urgent problem. MongoDb has solved this problem in version 4.4. Therefore we advise you to upgrade to MongoDb 4.4.

Feature
^^^^^^^

#. Sort: The :ref:`sorting <restful_search_sort>` that was implemented for the SQL/SQLite repositories in the previous version is now also implemented for MongoDb.
#. Terminology: The :ref:`local terminology service <feature_terminology>`, built in to the Vonk Administration API, is upgraded to support R4 and R5 (and still R3 of course).
#. Vonk can now index and search on search parameters that reference a nested resource, like Bundle.message.
   
   .. attention::
   
      Note that any nested resources have to be indexed by Vonk. For new data that is done automatically. But if you want to use this on existing data, you have to :ref:`reindex for the search parameters <feature_customsp_reindex_specific>` you want to use it on. Those will most notably be Bundle.message and Bundle.composition.

#. If you accidentally provide a body in a GET or DELETE request, Vonk will now ignore that body instead of returning an error.

Fix
^^^

#. CapabilityStatement (rev)includes now use ':' as a separator instead of '.'.

Plugins
^^^^^^^

#. The :ref:`BinaryWrapper plugin <vonk_plugins_binarywrapper>` is upgraded to 0.3.1, where the included BinaryEncodeService is made more reusable for other plugins (most notably the FHIR mapper).

.. _vonk_releasenotes_350:

Release 3.5.0
-------------

Feature
^^^^^^^

#. Search reference by identifier: FHIR R4 allows you to `search a reference by its identifier <http://hl7.org/fhir/R4/search.html#reference>`_. We added support for this in Vonk. Note that any identifiers in reference elements have to be indexed by Vonk. For new data that is done automatically. But if you want to use this on existing data, you have to :ref:`reindex for the search parameters <feature_customsp_reindex_specific>` you want to use it on. E.g. Observation.patient. 
#. AuditEvent logging: In release 3.3.0 we already added support for logging audit information to a file. With this release we add to that logging that same information in AuditEvent resources. These resources are written to the Vonk Data database (not the Administration database). Users are not allowed to update or delete these resources. See :ref:`feature_auditing` for more background.
#. Audit logging: We added ``[Request]`` or ``[Response]`` to the log lines so you can distinguish them better.
#. Sort: We started implementing :ref:`sorting <restful_search_sort>`. This release provides sorting for search parameters of the types string, number, uri, reference, datetime and token, on the repositories SQL, SQLite and Memory. On the roadmap is extending this support to MongoDb and to quantity search parameters.
#. :ref:`feature_terminologyintegration`: You can configure Vonk to route the terminology operations to external terminology servers. You can even configure a preferred server for certain code systems like LOINC or Snomed-CT. On the roadmap is to also allow you to use these servers for validation of codes and for token searches.
#. We implemented `$meta-delete <http://hl7.org/fhir/R4/resource-operation-meta-delete.html>`_, see :ref:`Meta plugins <vonk_plugins_meta>`.
#. Loading plugins can lead to unexpected errors. We made the process and the log friendlier, so you can spot configuration errors more easily:

   * The log outputs the version of each of the plugins
   * If a duplicate .dll file is found, Vonk tells you which two dlls are causing this and then exits.
   * If you configured a plugin that you are not licensed to use, this is logged with a friendly hint to acquire a license that does allow you to use it.

#. The log is now by default configured to use asynchronous logging so Vonk is not limited by the speed of the logging sinks (like the Console and the log file). Please update your logsettings.instance.json if you created your own log settings in that. See :ref:`configure_log` for more background.


Fix
^^^

#. You could load invalid XML in the Resource.text through a JSON payload. When that resource was then retrieved in XML, it would fail with an InternalServerError. Vonk will now return an OperationOutcome telling you what the problem is. You can then correct it by using JSON.
#. Composite search parameters were not parsed correctly. Now they are. So you don't see warnings like ``Composite SearchParameter 'CodeSystem.context-type-quantity' doesn't have components.`` anymore.  
#. Indexing for the _profile search parameter was broken for R4 since Vonk 3.2.1. We fixed it. If you added new resources with Vonk 3.2.1 - 3.4.0, you need to :ref:`reindex for the Resource._profile <feature_customsp_reindex_specific>` parameter.
#. Audit log: ``%temp%`` in the path setting was evaluated as ``<current directory>\%temp%``. Fixed that to evaluate to the systems temporary directory.
#. The logsettings.json configured the Serilog RollingFile sink by default. That is deprecated, so we replaced it with the File sink.
#. :ref:`feature_customsp_reindex_specific` now returns an error if you forget to actually specify a search parameter.
#. An InternalServerError was returned when you validate a resource that is syntactically incorrect. Like a Patient with multiple id's. Vonk now returns an OperationOutcome with the actual problem.
#. The configuration for the FHIR Mapper was simplified. You only need to include ``Vonk.Plugin.Mapping``. Check appsettings.default.json for the new pipeline.
#. Maybe you got accustomed to ignoring a list of warnings at startup of Vonk. We cleaned up the list so that if there is a warning, it is worthwhile investigating the cause of it.
#. The appsettings and logsettings can contain relative file paths for several settings, like the ``License:LicenseFile``. These were evaluated against the current working directory, but that could lead to problems if that was *not* the Vonk directory. We solved that: all relative paths are evaluated against the Vonk directory.
#. The docker image for version 3.4.0 was tagged ``3.4.0-``. With 3.5.0 we removed the superfluous hyphen at the end.
#. We updated the documentation on :ref:`use_docker` on SQL Server to be clearer about the order of the steps to take.

Plugins & Facade
^^^^^^^^^^^^^^^^

#. FHIR Mapper 

   * Has been upgraded to version 0.3.4.

.. _vonk_releasenotes_340:

Release 3.4.0
-------------

Feature
^^^^^^^

#. Upgraded to FHIR .NET API 1.6.0, that features a couple of changes for working with CDA logical models. See the `release notes of the API <https://github.com/FirelyTeam/firely-net-sdk/releases>`_.
#. Included the FHIR Mapper in the distribution. It is only enabled however when you include the mapping plugin in your license.

Fix
^^^

#. When prevalidation is set to the level 'Core', Vonk no longer complains about extensions that are not known if they are not core extensions (i.e. having a url starting with 'http://hl7.org/fhir/StructureDefinition/').

.. _vonk_releasenotes_330:

Release 3.3.0
-------------

.. attention::

   To use the new features for auditing and R5, you need a new license file including the tokens for those plugins.
   For evaluation editions you can `sign up <https://fire.ly/firely-server-trial/>`_ after which you will receive an email with the license file.
   If you need these updates in your production license, please contact us.

Feature
^^^^^^^

#. Vonk was upgraded to FHIR .NET API 1.5.0. See the `release notes of the API <https://github.com/FirelyTeam/firely-net-sdk/releases>`_.
#. Vonk can now log audit lines in a separate file. This can help you achieve HIPAA/GDPR compliancy. See :ref:`feature_auditing` for more info.
#. Failed authorization attempts are now logged from the :ref:`vonk_plugins_smart` plugin.
#. Support for ``_include:iterate`` and ``_revinclude:iterate``, see :ref:`restful_search`.
#. The :ref:`plugin_binarywrapper` is now two-way. So you can POST binary content and have it stored as a Binary resource, and GET a Binary resource and have it returned in its original binary format. 
#. Experimental support for R5 is now included in the Vonk distribution. For enabling it, see :ref:`feature_multi_version_r5`.

Fix
^^^

#. Indexing of a quantity in resource could fail with a Status code 500 if it had no ``.value`` but only extensions.
#. The use of a SearchParameter of type ``reference`` having no ``target`` failed. These search parameters are now signalled upon import.
#. Since R4 it is valid to search for a quantity without specifying the unit. Vonk now accepts that.
#. A transaction response bundle could contain an empty ``response.etag`` element, which is invalid.
#. :ref:`feature_preload` was not working since the upgrade to .NET Core 3.0. That has been fixed. It is still only available for STU3 though.
#. Administration import would state that it moves a file to history when it had imported it. That is no longer true, so we removed this incorrect statement from the log.
#. $validate-code could cause a NullReference exception in some case.
#. The generated CapabilityStatement for R4 failed constraint cpb-14.
#. Content negotiation favoured a mediatype with quality < 1 over a mediatype without quality. But the default value is 1, so the latter is now favoured. 
#. :ref:`feature_validation_instance` did not account for the informationmodel (aka FHIR version) of the resource.

Plugins & Facade
^^^^^^^^^^^^^^^^

#. :ref:`Document Operation <vonk_plugins_documentoperation>` 
   
   * Has been upgraded to Vonk 3.2.0.
   * Was assigned a license token
   * Assigns an id and lastUpdated to the result bundle

#. :ref:`vonk_plugins_convert`

   * Has been upgraded to Vonk 3.2.0.
   * Was assigned a license token.

#. `Vonk.Facade.Starter <https://github.com/FirelyTeam/Vonk.Facade.Starter>`_ has been upgraded to Vonk 3.2.1 and as a consequence also to EntityFrameworkCore 3.1.0.

.. _vonk_releasenotes_321:

Release 3.2.1
-------------

Fix
^^^

#. SMART plugin now understands multiple scopes per access token.
#. SMART plugin now understands ``Resource.*`` claims, in addition to already understanding ``Resource.read`` and ``Resource.write``.

.. _vonk_releasenotes_320:

Release 3.2.0
-------------

   .. attention::

      Vonk 3.2.0 is upgraded to .NET Core 3.1.0, ASP.NET Core 3.1.0 and EntityFramework Core 3.1.0.
      
         * For running the server: install the ASPNET.Core runtime 3.1.0.
         * For developing or upgrading Facades that use Vonk.Facade.Relational: upgrade to EF Core 3.1.0.
         * Plugins that target NetStandard 2.0 need not be upgraded.

Database
^^^^^^^^

#. There are no changes to the databases. The upgrade of EntityFramework Core does not affect the structure of the SQL Server or SQLite databases, just the access to it.

Fix
^^^

#. :ref:`Supported interactions <disable_interactions>` were not enforced for custom operations like e.g. $convert.
#. If a resource failed :ref:`feature_prevalidation`, the OperationOutcome also contained issues on not supported arguments.  
#. A search with ``?summary=count`` failed.
#. Added support for FhirPath ``hasValue()`` method.
#. Resolution of canonical ``http://hl7.org/fhir/v/0360|2.7`` failed.
#. CapabilityStatement.rest.resource.searchInclude used '.' as separator, fixed to use ':' in <resource>:<search parameter code>
#. Changed default value of ``License:LicenseFile`` to ``vonk-license.json``, aligned with the default naming if you download a license from Simplifier.
#. :ref:`Reindexing <feature_customsp_reindex>` always interpreted a resource as STU3. Now it correctly honours the actual FHIR version of the resource.

Feature
^^^^^^^

#. :ref:`BinaryWrapper plugin <vonk_plugins_binarywrapper>` can now be restricted to a list of mediatypes on which to act.
#. Vonk used to sort on ``_lastUpdated`` by default, and add this as extra sort argument if it was not in the request yet. Now you can configure the element to sort on by default in ``BundleOptions:DefaultSort``. Although Vonk FHIR Server does not yet support sorting on other elements, this is useful for Facade implementations that may support that (and possibly not support sort on ``_lastUpdated``). See also :ref:`bundle_options`.
#. Implemented `$versions <http://hl7.org/fhir/R4/capabilitystatement-operation-versions.html>`_ operation
#. Extended the documentation on:

   * :ref:`vonk_plugins_order`
   * :ref:`vonk_reference_api_bundles`
   * several smaller additions

#. The SMART authorization plugin can now be configured to *not* check the audience. Although not recommended, it can be useful in testing scenarios or a highly trusted environment.

   .. attention::

      We changed the default value for the setting ``SmartAuthorizationOptions.Audience`` from ``vonk`` to empty, or 'not set'. This is to avoid awkward syntax to override it with 'not-set'. But if you rely on the value ``vonk``, please override this setting in your ``appsettings[.instance].json`` or environment variables as described in :ref:`configure_change_settings`.

Plugin and Facade API
^^^^^^^^^^^^^^^^^^^^^

#. Vonk.Facade.Relational now supports the use of the .Include() function of EntityFramework Core. To do so, override ``RelationalQuery.GetEntitySet(DbContext dbContext)``.
#. Vonk.Facade.Relational now supports sorting. Override ``RelationalQueryFactory.AddResultShape(SortShape sortShape) and return a RelationalSortShape using the extension method ``SortQuery()``.


.. _vonk_releasenotes_313:

Release 3.1.3 hotfix
--------------------

Fix
^^^
#. Fixed behavior on conditional updates in transactions. In odd circumstances Vonk could crash on this.

.. _vonk_releasenotes_310:

Release 3.1.0
-------------

Please also note the changes in :ref:`3.0.0 <vonk_releasenotes_300>` (especially the one regarding the SQL server database)

Fix
^^^
#. Validation on multi-level profiled resources no longer fails with the message `"Cannot walk into unknown StructureDefinition with canonical"`
#. Improved documentation on :ref:`upgrading Vonk<upgrade>`, the :ref:`Vonk pipeline<vonk_reference_api_pipeline_configuration>`, :ref:`CORS support<configure_cors>`, :ref:`plugins<vonk_plugins>` and :ref:`IIS deployment<iis>`
#. Using multiple parameters in _sort led to an error for all repositories
#. Vonk UI capability statement view now works for self-mapped endpoints like ``/R3`` or ``/R4``
#. A saved resource reference (e.g. ``Patient.generalPractitioner``) on a self-mapped endpoint (e.g. ``/R4/...``) would have its relative path duplicated (``/R4/R4/...``)

   .. attention::

      If you have used :ref:`self-mapped endpoints<feature_multiversion>` (appsettings: ``InformationModel.Mapping.Map`` in the 'Path' mapping mode) and you have saved resources containing references, it is possible that your database now contains some resources with broken references. Please contact us if this is the case

Feature
^^^^^^^
#. The new experimental FHIR mapping engine, which is currently exclusively available on our public FHIR server `http://vonk.fire.ly <http://vonk.fire.ly>`_
#. New licensing system, supporting the `community edition`
#. Simplifier projects are now imported for FHIR R4 as well
#. The following plugins have been bundled with the Vonk release (compare your appsettings with the new appsettings.default.json to activate them)

   #. The $document operation (see :ref:`vonk_plugins_document`)
   #. The $convert operation (see :ref:`vonk_plugins_convert`)
   #. The binary wrapper (see :ref:`vonk_plugins_binary`)   

Plugin and Facade API
^^^^^^^^^^^^^^^^^^^^^
#. Vonk.Facade.Starter has been upgraded to work with Vonk 3.1.0
#. IConformanceContributor and IConformanceBuilder have moved from Vonk.Core.Pluggability to Vonk.Fhir.R3.Metadata. It is also deprecated, as Vonk.Core.Metadata.ICapabilityStatementContributor is now preferred instead. See :ref:`vonk_reference_api_capabilities` for more information
#. Implementations of ISearchRepository can now sort on multiple parameters (in BaseQuery.Shapes). Previously this would result in an error.
#. Improved documentation on the :ref:`vonk_reference_api`
#. See :ref:`vonk_releasenotes_300` for some additional issues you may encounter upgrading your plugins

.. _vonk_releasenotes_300:

Release 3.0.0
-------------

Database
^^^^^^^^

Please also note the changes in :ref:`3.0.0-beta1 <vonk_releasenotes_300-beta1>`

#. SQL Server: SQL script '20190919000000_Cluster_Indexes_On_EntryId.sql' (found in the /data folder of the Vonk distribution) must be applied to existing Vonk SQL databases (both to the admin and to the data repositories) 

   .. attention::

      Vonk 3.0.0 (using SQL server) will not start unless this script has been applied to the databases. Please note that running the script can take considerable time, especially for large databases.

Feature
^^^^^^^
#. Information model (= FHIR version) settings

   #. Although Vonk now supports multiple information models (STU3 and R4) simultaneously, an unused model can be disabled (see :ref:`settings_pipeline`)
   #. You can set the default (or fallback) information model (previously: STU3), which is used when Vonk can not determine the information model from context (see :ref:`information_model`)
   #. You can map a path or a subdomain to a specific information model (see :ref:`information_model`), mitigating the need to specify it explicitly in a request

#. Vonk now uses `FHIR .NET API 1.4.0 <https://github.com/FirelyTeam/firely-net-sdk/releases>`_
#. Several performance enhancements have been made for SQL server and IIS setups
#. Added R4-style `Conditional Update <https://www.hl7.org/fhir/http.html#cond-update>`_ to both STU3 and R4

Fix
^^^

#. Circular references within resources are now detected, cancelling validation for now. We will re-enable validation for these resources when the FHIR .NET API has been updated
#. An $expand using incorrect data returned a 500 (instead of the correct 400)
#. Vonk now returns a 406 (Not Acceptable) when the Accept header contains an unsupported format
#. Deletes did not work for R4

#. Search parameters

   #. Search parameters were read twice (at startup and upon the first request)
   #. Search parameter 'CommunicationRequest.occurrence' is not correctly specified in the specification. We provide a correct version.

#. _history

   #. _history was not usable in a multi information model setup
   #. The resulting Bundle.entry in an STU3 _history response contained the unallowed response field
   #. Added Bundle.entry.response to the R4 _history entry

#. Batches

   #. Valid entries in batches also containing invalid entries were not processed
   #. Duplicate fullUrls are no longer accepted in a batch request, which previously led to a processing error
   #. An R4 transaction resulted in STU3 entries
   #. Transactional errors did not include fullUrl

Plugin and Facade API
^^^^^^^^^^^^^^^^^^^^^

#. Improved the message you get when the sorting/shaping operator is not implemented by your facade
#. VonkOutcome (and VonkIssue) has been simplified
#. VonkConstants has moved from Vonk.Core.Context to Vonk.Core.Common
#. IResourceChangeRepository.Delete requires a new second parameter: ``string informationModel``. Information model constants can be found in Vonk.Core.Common.VonkConstants.Model
#. Exclude Vonk.Fhir.R3 or Vonk.Fhir.R4 from the PipelineOptions if you don't support it in your Facade.
#. Updated the minimal PipelineOptions for a Facade Plugin in the `example appsettings.json <https://github.com/FirelyTeam/Vonk.Facade.Starter/blob/master/Visi.Repository/appsettings.json>`_:
   
   * updated ``Vonk.Core.Operations.SearchConfiguration`` to ``Vonk.Core.Operations.Search``
   * removed ``Vonk.UI.Demo``
   * removed ``Vonk.Core.Operations.Validate.SpecificationZipSourceConfiguration`` from the ``Exclude``
   * updated ``Vonk.Core.Operations.Terminology`` to ``Vonk.Plugins.Terminology``

.. note::

   Early Facade implementations were built with by using Vonk services and middleware in a self-built ASP.NET Core web server. This can be seen in the Vonk.Facade.Starter project in the 
   `repository <https://github.com/FirelyTeam/Vonk.Facade.Starter>`_ with the same name. Due to changes in Vonk this does not work with Vonk 3.0.0. It will be fixed in 3.1.0. 
   But after that such projects cannot be upgraded anymore and will have to be refactored to a proper plugin (as the ViSi.Repository project in the same repository). 
   Please :ref:`contact <vonk-contact>` us in case of any questions.


.. _vonk_releasenotes_300-beta2:

Release 3.0.0-beta2
--------------------

.. attention::

   We updated the :ref:`vonk_securitynotes`.

Database
^^^^^^^^

Note the changes in :ref:`3.0.0-beta1 <vonk_releasenotes_300-beta1>`, but there are no new changes in beta2.

Feature
^^^^^^^

#. :ref:`feature_subscription` works for R4 also. Note that a Subscription will only be activated for resource changes in the same FHIR version as the Subscription itself.
#. :ref:`conformance_fromdisk` works for R4 also. Use a directory name that ends with ``.R4`` for R4 conformance resources.
#. :ref:`feature_customsp_reindex` works for R4 also. Issue a reindex with a fhirVersion parameter in the Accept header, and it will be executed for the SearchParameters defined for that FHIR version.
#. Allow for non-hl7 prefixed canonical urls for conformance resources (since sdf-7 is lifted). See :ref:`feature_customresources`.
#. Custom Resources can be validated, both individually and as part of a bundle. See :ref:`feature_customresources`.
#. If the Accept header lacks a :ref:`fhirVersion parameter <feature_multiversion>`, it will fall back to the fhirVersion parameter of the Content-Type header and vice versa.
   If both are missing, Vonk will default to STU3.

Fix
^^^

#. _include did not work for R4.
#. _include gave a 500 response code if a resource contains absolute references.
#. A resource with unknown elements could result in an uncaught ``Hl7.Fhir.ElementModel.StructuralTypeException``.
#. The homepage stated that Vonk was only for STU3. Fixed that.
#. Bundle.timestamp element (new in R4) was not populated in bundles returned from Search and History operations.
#. Some operations could return an OperationOutcome with an issue *and* a success message.
#. Better error message if a resource without any meta.profile is not accepted by :ref:`feature_prevalidation`.
#. Requesting an invalid FHIR version resulted in a ArgumentNullException.

Plugin and Facade API
^^^^^^^^^^^^^^^^^^^^^

#. NuGet package ``Vonk.Fhir.R4`` had a dependency on Vonk.Administration.API, but the latter is not published. We removed the dependency.
#. ``IResourceExtensions.UpdateMetadata`` did not update the id of the resource.
#. ``VonkOutcome.RemoveIssue()`` method has been removed.

.. _vonk_releasenotes_300-beta1:

Examples
^^^^^^^^

#. Plugin example (`Vonk.Plugin.ExampleOperation <https://github.com/FirelyTeam/Vonk.Plugin.ExampleOperation>`_):

   #. Added an example of middleware directly interacting with the ``HttpContext`` (instead of just the ``VonkContext``), see the file `VonkPluginMiddleware.cs <https://github.com/FirelyTeam/Vonk.Plugin.ExampleOperation/blob/master/Vonk.Plugin.ExampleOperation/VonkPluginMiddleware.cs>`_ 
   #. CapabilityStatementBuilder was not called.

#. DocumentOperation (`Vonk.Plugin.DocumentOperation <https://github.com/FirelyTeam/Vonk.Plugin.DocumentOperation>`_):

   #. Composition ID was not determined correctly when using POST.

Release 3.0.0-beta1
--------------------

Vonk 3.0.0 is a major upgrade that incorporates handling FHIR R4. This runs in the same server core as FHIR STU3. See :ref:`feature_multiversion` for background info.

.. attention::

   If you have overridden the PipelineOptions in your own settings, you should review the new additions to it in the appsettings.default.json.
   In particular we added ``Vonk.Fhir.R4`` that is needed to support FHIR R4.

.. attention::

   MacOS: you may need to clean your temp folder from previous specification.zip expansions. Find the location of the temp folder by running ``echo $TMPDIR``.

Database
^^^^^^^^

#. SQL Server, SQLite: 

   #. vonk.entry got a new column 'InformationModel', set to 'Fhir3.0' for existing resources.
   #. vonk.ref got a new column 'Version'. 
   #. Database indexes have been updated accordingly.

   Vonk will automatically update both the Administration and the Data databases when you run Vonk 3.0.0.

#. MongoDb / CosmosDb: 

   #. The documents in the vonkentries collection got a new element im (for InformationModel), set to 'Fhir3.0' for existing resources. 
   #. The documents in the vonkentries collection got a new element ref.ver (for Version). 
   #. Database indexes have been updated accordingly. 

#. MongoDb / CosmosDb: Got a light mechanism of applying changes to the document structure. A single document is added to the collection for that, containing ``VonkVersion`` and ``LatestMigration``.
#. MongoDb: The default name for the main database was changed from 'vonkstu3' to 'vonkdata'. 
   If you want to continue using an existing 'vonkstu3' database, override ``MongoDbOption:DatabaseName``, see :ref:`configure_levels`.

Feature
^^^^^^^

#. Support for FHIR R4 next to FHIR STU3. Vonk will choose the correct handling based on the fhirVersion parameter in the mimetype. 
   The mimetype is read from the Accept header and (for POST/PUT) the Content-Type header. See :ref:`feature_multiversion` for background info.
#. Upgrade to HL7.Fhir.Net API 1.3, see its `releasenotes <https://docs.fire.ly/projects/Firely-NET-SDK/releasenotes.html#stu3-r4-released-20190710>`__.
#. Administration API imports both STU3 and R4 conformance resources, see :ref:`conformance`

   #. Note: :ref:`Terminology operations <feature_terminology>` are still only available for STU3.
   #. Note: :ref:`Subscriptions <feature_subscription>` are still only available for STU3.

#. Conditional delete on the Administration API. It works just as on the root, see :ref:`restful_crud`.
#. Defining a custom SearchParameter on a :ref:`Custom ResourceType <feature_customresources>` is now possible.
#. Canonical uris are now recognized when searching on references (`specification <http://www.hl7.org/implement/standards/fhir/search.html#versions>`_)
#. Vonk calls ``UseIISIntegration`` for better integration with IIS (if present).

Fix
^^^

#. In the settings, PipelineOptions.Branch.Path for the root had to be ``/``. Now you can choose your own base (like e.g. ``/fhir``)
#. $meta:
   
   #. enabled on history endpoint (e.g. ``/Patient/123/_history/v1``)
   #. disabled on type and system level
   #. returned empty Parameters resource if resource had no ``meta.profile``, now returns the resources ``meta`` element.
   #. when called on a non-existing resource, returns 404 (was: empty Parameters resource)
   #. added to the CapabilityStatement

#. History on non-existing resource returned OperationOutcome instead of 404.
#. The setting for SupportedInteractions was not enforced for custom operations.
#. CapabilityStatement.name is updated from ``Vonk beta conformance`` to ``Vonk FHIR Server <version> CapabilityStatement``.
#. :ref:`feature_terminology`:

   #. $lookup did not work on GET /CodeSystem
   #. $lookup did not support the ``coding`` parameter
   #. $expand did not fill in the expansion element.
   #. Operations were not listed in the CapabilityStatement.
   #. Namespace changed to Vonk.Plugins.Terminology, and adjusted accordingly in the default PipelineOptions.

#. A SearchParameter of type token did not work on an element of type string, e.g. CodeSystem.version.
#. Search with POST was broken.
#. If a long running task is active (response code 423, see :ref:`conformance_import` and :ref:`feature_customsp_reindex`), the OperationOutcome reporting that will now hide issues stating that all the arguments were not supported (since that is not the cause of the error).
#. Overriding an array in the settings was hard - it would still inherit part of the base setting if the base array was longer. 
   We changed this: an array will always overwrite the complete base array.
   Note that this may trick you if you currently override a single array element with an environment variable. See :ref:`configure_levels`.
#. The element ``meta.source`` cannot be changed on subsequent updates to a resource (R4 specific)
#. SearchParameter ``StructureDefinition.ext-context`` yielded many errors in the log because the definition of the fhirpath in the specification is not correct. We provided a corrected version in errataFhir40.zip (see :ref:`feature_errata`).
#. :ref:`disable_interactions` was not evaluated for custom operations.
#. Delete of an instance accepted search parameters on the url.
#. Transactions: references to other resources in the transaction were not updated if the resource being referenced was in the transaction as an update (PUT).
   (this error was introduced in 2.0.0).

Plugin and Facade API
^^^^^^^^^^^^^^^^^^^^^

#. A new NuGet package is introduced: Vonk.Fhir.R4.
#. ``VonkConstants`` moved to the namespace ``Vonk.Core.Common`` (was: ``Vonk.Core.Context``)
#. ``IResource.Navigator`` element is removed (was already obsolete). Instead: Turn it into an ``ITypedElement`` and use that for navigation with FhirPath.
#. ``InformationModel`` element is added to 
   
   #. ``IResource``: the model in which the resource is defined (``VonkConstants.Model.FhirR3`` or ``VonkConstants.Model.FhirR4``)
   #. ``IVonkContext``: the model that was specified in the Accept header
   #. ``IModelService``: the model for which this service is valid (implementations are available for R3 and R4)
   #. ``InteractionHandler`` attribute: to allow you to specify that an operation is only valid for a specific FHIR version.
      This can also be done in the fluent interface with the new method ``AndInformationModel``. See :ref:`vonk_reference_api_interactionhandling`

#. Dependency injection: if there are implementations of an interface for R3 and R4, the dependency injection in Vonk will automatically inject the correct one based on the InformationModel in the request.
#. If you want to register your own service just for one informationmodel, do that as follows:

   Add a ContextAware attribute to the implementation class::

      [ContextAware (InformationModels = new[] {VonkConstants.Model.FhirR3}]
      public class MySearchRepository{...}

   Then register the service as being ContextAware::

      services.TryAddContextAware<ISearchRepository, MySearchRepository>(ServiceLifeTime.Scoped);

#. ``FhirPropertyIndexBuilder`` is moved to Vonk.Fhir.R3 (and was already marked obsolete - avoid using it)
#. Implementations of the following that are heavily dependent upon version specific Hl7.Fhir libraries have been implemented in both Vonk.Fhir.R3 and Vonk.Fhir.R4. 

   #. ``IModelService``
   #. ``IStructureDefinitionSummaryProvider`` (to add type information to an ``IResource`` and turn it into an ``ITypedElement``)
   #. ``ValidationService``

#. ``IConformanceContributor`` is changed to ``ICapabilityStatementContributor``. The methods on it have changed slightly as well because internally they now work on a version-independent model. Please review your IConformanceContributor implementations.

Examples
^^^^^^^^

#. Document plugin: 
   
   #. `Document Bundle does not contain an identifier <https://github.com/FirelyTeam/Vonk.Plugin.DocumentOperation/issues/27>`_
   #. `Missing unit test for custom resources <https://github.com/FirelyTeam/Vonk.Plugin.DocumentOperation/issues/29>`_
   #. Upgraded to Vonk 2.0.0 libraries (no, not yet 3.0.0-beta1)

#. Facade example

   #. Added support for searching directly on a reference from Observation to Patient (e.g. ``/Observation?patient=Patient/3``).
   #. Fixed support for _revinclude of Observation on Patient (e.g. ``/Patient?_revinclude:Observation:subject:Patient``).
   #. Upgraded to Vonk 2.0.0 libraries (no, not yet 3.0.0-beta1)

#. Plugin example

   #. Added examples for pre- and post handlers.

Known to-dos
^^^^^^^^^^^^

#. :ref:`feature_customsp_reindex`: does not work for R4 yet.
#. :ref:`feature_preload`: does not work for R4 yet.
#. :ref:`feature_subscription`: do not work for R4 yet.
#. :ref:`feature_terminology`: operations do not work for R4.
#. During :ref:`conformance_import`: Files in the import directory and Simplifier projects are only imported for R3.
