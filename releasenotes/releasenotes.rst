.. _vonk_releasenotes:

Release notes Firely Server
===========================

.. toctree::
   :maxdepth: 1
   :titlesonly:

   releasenotes_old
   security_notes

.. _upgrade_vonk:

Upgrading Firely Server
-----------------------

See :ref:`upgrade` for information on how to upgrade to a new version of Firely Server.

.. _announcement_vonk_8_july_2021:

Public Endpoint Announcement 8 July 2021
----------------------------------------

The default FHIR version of the `public Firely Server endpoint <https://server.fire.ly/>`_ is now R4.

.. _vonk_releasenotes_430:

Release 4.3.0
-------------

Feature
^^^^^^^

#. Firely Server now allows you to execute a ValueSet expansion of large ValueSets (> 500 included concepts). Previously, Firely Server would log an error outlining that the expansion was not possible. The appsettings now contain a setting in the Terminology section allowing to select the MaxExpansionSize.

Fix
^^^

#. Fixed a NullPointerException which occured when indexing UCUM quantities that contained more than one annotation (e.g. "{reads}/{base}").
#. Fixed a bug where it was possible to accidentally delete a resource with a different information model then the request. Firely Server will now check the information model of the request against the information model of the resource for conditional delete and delete requests.
#. $subsumes returned HTTP 501 - Not implemented for a POST request (instance-level) even if the operation was enabled in the appsettings.
#. The _type filter on $everthing and Bulk data export din't allow for resources that are not within the Patient compartment. The operations would return an empty result set.
#. Added a clarification to the documentation that $everything and Bulk data export do not export Device resources by default. Even though the resource contains a reference to Patient, the corresponding compartment definition for Patient does not include Device as a linked resource. It is possible to export Device resources by adding the resource type to "AdditionalResources" settings of the operations.

.. _vonk_releasenotes_421:

Release 4.2.1 hotfix
--------------------

Database
^^^^^^^^
.. note::
   We found an issue in version 4.2.0, which affects the query performance for Firely Server running on a SQL Server database. If your are running FS v4.2.0 on SQL Server you should upgrade to v4.2.1 or if that is not possible, :ref:`vonk-contact`.

.. attention::
    The upgrade procedure will execute a SQL script try to validate the foreign key constraints. If your database is large, this may take too long and the upgrade process will time out. If that happens you need to run the upgrade script manually, The script can be found in ``data/20210720085032_EnableCheckConstraintForForeignKey.sql``. Here are some guidelines:

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
   For SQL Server we changed the datatype of the primary keys. The related upgradescript (`data/20210519072216_ChangePrimaryKeyTypeFromIntToBigint.sql`) can take a lot of time if you have many resources loaded in your database. Therefore some guidelines:

   * We tested it on a database with about 15k Patient records, and 14 mln resources in total. Migrating that took about 50 minutes on a fairly powerful laptop.
   * Absolutely make sure you create a backup of your database first.
   * If you haven't done so already, first upgrade to version 4.1.x.
   * If you already expect the migration might time out, you can run it manually upfront. Shut down Firely Server, so no other users are using the database, and then run the script from SQL Server Management Studio (or a similar tool).
   * Running the second script (`20210520102224_ChangePrimaryKeyTypeFromIntToBigintBDE.sql`) is optional - that should also succeed when applied by the automigration.

Feature
^^^^^^^

#. Terminology operation ``$lookup`` is now also connected to remote terminology services, if enabled. See :ref:`feature_terminology`.
#. We provided a script to 'purge' data from a SQL Server database. See `data/20210512_Purge.sql`. You can filter on the resourcetype only. Use with care and after a backup. If you need more elaborate support for hard deletes, please :ref:`vonk-contact`.

Fix
^^^
#. Firely Server could run out of primary keys on the index tables in SQL Server. Fixed by upgrading to bigint, see warning above.
#. Nicer handling of SQL Server migration scripts that time out on startup. It will now kindly ask you to run the related script manually if needed (usually depends on the size of your database).
#. The Patient-everything (`$everything`) operation was not mentioned in the CapabilityStatement.
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
#. Upgraded the Mapping plugin to :ref:`fhir_mapper_docs:mapping_releasenotes_071`.

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
#. Fixed an error in SQL script ``data/20210226200007_UpdateIndexesTokenAndDatetime_Up.sql`` that is used when manually updating the database to v4.1.0. We alse made the script more robust by checking if the current version the database is suitable for the manual upgrade.

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
#. On a batch or transaction bundle errors were not reported clearly if the entry in error had no fullUrl element. We fixed this by referring to the index of the entry in the entry array, and the resourcetype of the resource in the entry (if any).
#. The ``import[.R4]`` folder allows for importing custom StructureDefinition resources. If any of them had no id, the error on that caused an exception. Fixed that.
#. If a Facade returned a resource without an id from the Create method, an error was caused by a log statement. Fixed that.
#. Indexing ``Subscription.channel[0].endpoint[0]`` failed for R4. Fixed that. This means you can't search for existing Subscriptions by ``Subscription.url`` on the /administration endpoint for FHIR R4.
#. Postman was updated w.r.t. acquiring tokens. We adjusted the :ref:`documentation on that <feature_accesscontrol_postman>` accordingly.
#. If a patient claim was included in a SMART on FHIR access token, the request would be scoped to the Patient compartment regardless of the scope claims. We fixed this by allowing "user" scopes to access FHIR resources outside of the Patient compartment regardless of the patient claim. See `Launch context arrives with your access_token <http://hl7.org/fhir/smart-app-launch/scopes-and-launch-context/index.html#launch-context-arrives-with-your-access_token>`_ for more background information.

Plugin and Facade
^^^^^^^^^^^^^^^^^

#. The mapping plugin is upgraded to the Mapping Engine 0.6.0, see its :ref:`releasenotes <fhir_mapper_docs:mapping_releasenotes_060>`.
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

.. _vonk_releasenotes_393:

Release 3.9.3 hotfix
--------------------

.. attention::

   We changed the behaviour of resthook notifications on Subscriptions. See Fix nr 1 below.

Database
^^^^^^^^

#. SQL Server: The migration that adds the indexes described in :ref:`vonk_releasenotes_392` might run longer than the timeout period of 30 seconds. Therefore we added scripts to apply and revert this migration manually. If you encounter the timeout during upgrade: shut down vonk, run the script using SQL Server Management Studio or any similar tool, then start Vonk 3.9.3 again. In both scripts you only need to provide the databasename for the database that you want to upgrade. If you run your administration database on SQL Server you can but probably do not need to run this script on it. The administration database is typically small enough to complete the script within 30 seconds.

   #. apply: <vonk-dir>/data/2021211113200_AddIndex_ForCountAndUpdateCurrent_Up.sql
   #. revert: <vonk-dir>/data/2021211113200_AddIndex_ForCountAndUpdateCurrent_Down.sql

Fix
^^^

#. :ref:`feature_subscription`: A resthook notification was sent as a FHIR create operation, using POST. This was not compliant with the specification that states it must be an update, using PUT. We changed the default behaviour to align with the specification. In order to avoid breaking changes in an existing deployments, you may set the new setting ``SubscriptionEvaluatorOptions:SendRestHookAsCreate`` to ``true`` - that way Vonk will retain the (incorrect) behaviour from the previous versions.

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
* Because of the changes in searching for Quantities (feature 2 below), you will need to do a :ref:`reindex <feature_customsp_reindex>` in order to make use of this. You may limit the reindex to only the searchparameters of type 'quantity' that you actually use (e.g. ``Observation.value-quantity``).

Features
^^^^^^^^

#. We upgraded the FHIR .NET API to 1.9, see the `1.9 releasenotes <https://github.com/FirelyTeam/firely-net-sdk/releases>`_. This will trigger an automatic :ref:`import of the Conformance Resources <conformance_specification_zip>` at startup.
#. We upgraded the `Fhir.Metrics library <https://github.com/FirelyTeam/fhir.metrics>`_ to 1.2. This allows for a more uniform search on Quantities (mainly under the hood)
#. We upgraded the FHIR Mapping plugin to support the FHIR Mapper version 0.5. See its :ref:`FHIR Mapper releasenotes <fhir_mapper_docs:mapping_releasenotes_050>`.
#. The :ref:`built-in terminology services <feature_terminology>` now support the ``includeDesignations`` parameter. 
#. The :ref:`vonk_reference_api_ivonkcontext` now lets you access the original HttpContext.
#. The CapabilityStatement now lists the profiles that are known to Vonk (in its Administration database) under ``CapabilityStatement.rest.resource.supportedProfile`` (>= R4 only) and the base profile for a resource under ``CapabilityStatement.rest.resource.profile``.
#. We extended the security extension on the CapabilityStatement to include the endpoints for ``register``, ``manage``, ``introspect`` and ``revoke``.
#. ``IAdministrationSearchRepository`` and ``IAdministrationChangeRepository`` interfaces are no publicly available. Use with care.


Fixes
^^^^^

#. If the server configured as authorization endpoint in the Smart setting is not reachable, Vonk will log a proper error about that.
#. An error message for when a query argument has no value is improved.
#. When :ref:`SMART-on-FHIR <feature_accesscontrol>` is enabled, and the received token contains a launch context, the :ref:`_history<restful_history>` operation is no longer available. Because Vonk does not retain the search parameter index for historical resources, it cannot guarantee that these resources fall within the launch context (at least not in a performant way). To avoid information leakage we decided to disable this case altogether.
#. A Create interaction without an id in the resource, with :ref:`SMART-on-FHIR <feature_accesscontrol>` enabled, resulted in an exception.
#. You can now escape the questionmark '?' in a query argument by prepending it with a backslash '\'.
#. A Quantity search using 'lt' on MongoDb resulted in too many results. 

.. _vonk_releasenotes_370:

Release 3.7.0
-------------

Database
^^^^^^^^

.. attention::

   To accomodate for feature #2 below there is an automatic migration carried out for SQL Server and SQLite. This migration might take some time, so please test it first. For MongoDb, you will have to :ref:`feature_customsp_reindex_all`. If this is not feasible for your database, please :ref:`vonk-contact` for assistance.

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

#. The FHIR Mapper plugin is upgraded to :ref:`version 0.3.6 <fhir_mapper_docs:mapping_releasenotes_036>`.
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
#. Vonk can now index and search on searchparameters that reference a nested resource, like Bundle.message.
   
   .. attention::
   
      Note that any nested resources have to be indexed by Vonk. For new data that is done automatically. But if you want to use this on existing data, you have to :ref:`reindex for the searchparameters <feature_customsp_reindex_specific>` you want to use it on. Those will most notably be Bundle.message and Bundle.composition.

#. If you accidentally provide a body in a GET or DELETE request, Vonk will now ignore that body instead of returning an error.

Fix
^^^

#. CapabilityStatement (rev)includes now use ':' as a separator instead of '.'.

Plugins
^^^^^^^

#. The :ref:`BinaryWrapper plugin <vonk_plugins_binarywrapper>` is upgraded to 0.3.1, where the included BinaryEncodeService is made more reusable for other plugins (most notably the :ref:`FHIR Mapper <vonk_plugins_mapping>`).

.. _vonk_releasenotes_350:

Release 3.5.0
-------------

Feature
^^^^^^^

#. Search reference by identifier: FHIR R4 allows you to `search a reference by its identifier <http://hl7.org/fhir/R4/search.html#reference>`_. We added support for this in Vonk. Note that any identifiers in reference elements have to be indexed by Vonk. For new data that is done automatically. But if you want to use this on existing data, you have to :ref:`reindex for the searchparameters <feature_customsp_reindex_specific>` you want to use it on. E.g. Observation.patient. 
#. AuditEvent logging: In release 3.3.0 we already added support for logging audit information to a file. With this release we add to that logging that same information in AuditEvent resources. These resources are written to the Vonk Data database (not the Administration database). Users are not allowed to update or delete these resources. See :ref:`feature_auditing` for more background.
#. Audit logging: We added ``[Request]`` or ``[Response]`` to the log lines so you can distinguish them better.
#. Sort: We started implementing :ref:`sorting <restful_search_sort>`. This release provides sorting for searchparameters of the types string, number, uri, reference, datetime and token, on the repositories SQL, SQLite and Memory. On the roadmap is extending this support to MongoDb and to quantity searchparameters.
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
#. Composite searchparameters were not parsed correctly. Now they are. So you don't see warnings like ``Composite SearchParameter 'CodeSystem.context-type-quantity' doesn't have components.`` anymore.  
#. Indexing for the _profile searchparameter was broken for R4 since Vonk 3.2.1. We fixed it. If you added new resources with Vonk 3.2.1 - 3.4.0, you need to :ref:`reindex for the Resource._profile <feature_customsp_reindex_specific>` parameter.
#. Audit log: ``%temp%`` in the path setting was evaluated as ``<current directory>\%temp%``. Fixed that to evaluate to the systems temporary directory.
#. The logsettings.json configured the Serilog RollingFile sink by default. That is deprecated, so we replaced it with the File sink.
#. :ref:`feature_customsp_reindex_specific` now returns an error if you forget to actually specify a searchparameter.
#. An InternalServerError was returned when you validate a resource that is syntactically incorrect. Like a Patient with multiple id's. Vonk now returns an OperationOutcome with the actual problem.
#. The configuration for the FHIR Mapper was simplified. You only need to include ``Vonk.Plugin.Mapping``. Check appsettings.default.json for the new pipeline.
#. Maybe you got accustomed to ignoring a list of warnings at startup of Vonk. We cleaned up the list so that if there is a warning, it is worthwile investigating the cause of it.
#. The appsettings and logsettings can contain relative file paths for several settings, like the ``License:LicenseFile``. These were evaluated against the current working directory, but that could lead to problems if that was *not* the Vonk directory. We solved that: all relative paths are evaluated against the Vonk directory.
#. The docker image for version 3.4.0 was tagged ``3.4.0-``. With 3.5.0 we removed the superfluous hypen at the end.
#. We updated the documentation on :ref:`use_docker` on SQL Server to be clearer about the order of the steps to take.
#. We updated the documentation on :ref:`vonk_plugins_landingpage` to match .NET Core 3.1.

Plugins & Facade
^^^^^^^^^^^^^^^^

#. :ref:`FHIR Mapper <vonk_plugins_mapping>` 

   * Has been upgraded to version 0.3.4.

.. _vonk_releasenotes_340:

Release 3.4.0
-------------

Feature
^^^^^^^

#. Upgraded to FHIR .NET API 1.6.0, that features a couple of changes for working with CDA logical models. See the `release notes of the API <https://github.com/FirelyTeam/firely-net-sdk/releases>`_.
#. Included the FHIR Mapper in the distribution. It is only enabled however when you include the mapping plugin in your license. See :ref:`mappingengine_index` for more information about the FHIR Mapper.

Fix
^^^

#. When prevalidation is set to the level 'Core', Vonk no longer complains about extensions that are not known if they are not core extensions (i.e. having a url starting with 'http://hl7.org/fhir/StructureDefinition/').

.. _vonk_releasenotes_330:

Release 3.3.0
-------------

.. attention::

   To use the new features for auditing and R5, you need a new license file including the tokens for those plugins.
   For evaluation and community editions you can retrieve them from Simplifier.net.
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

#. Indexing of a quantity in resource could fail with a Statuscode 500 if it had no ``.value`` but only extensions.
#. The use of a SearchParameter of type ``reference`` having no ``target`` failed. These searchparameters are now signalled upon import.
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
#. Vonk.Facade.Relational now supports sorting. Override ``RelationalQueryFactory.AddResultShape(SortShape sortShape) and return a RelationalShorShape using the extension method ``SortQuery()``.


.. _vonk_releasenotes_313:

Release 3.1.3 hotfix
--------------------

Fix
^^^
#. Fixed behaviour on conditional updates in transactions. In odd circumstances Vonk could crash on this.

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
#. IConformanceContributor and IConformanceBuilder have moved from Vonk.Core.Pluggability to Vonk.Fhir.R3.Metadata. It is also deprecated, as Vonk.Core.Metadata.ICapabilityStatementContributor is now preferred instead. See :ref:`vonk_architecture_capabilities` for more information
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
#. :ref:`conformance_fromdisk` works for R4 also. Use a directoryname that ends with ``.R4`` for R4 conformance resources.
#. :ref:`feature_customsp_reindex` works for R4 also. Issue a reindex with a fhirVersion parameter in the Accept header, and it will be executed for the SearchParameters defined for that FHIR version.
#. Allow for non-hl7 prefixed canonical urls for conformance resources (since sdf-7 is lifted). See :ref:`feature_customresources`.
#. Custom Resources can be validated, both individually and as part of a bundle. See :ref:`feature_customresources`.
#. If the Accept header lacks a :ref:`fhirVersion parameter <feature_multiversion>`, it will fall back to the fhirVersion parameter of the Content-Type header and vice versa.
   If both are missing, Vonk will default to STU3.

Fix
^^^

#. _include did not work for R4.
#. _include gave a 500 responsecode if a resource contains absolute references.
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
#. If a long running task is active (responsecode 423, see :ref:`conformance_import` and :ref:`feature_customsp_reindex`), the OperationOutcome reporting that will now hide issues stating that all the arguments were not supported (since that is not the cause of the error).
#. Overriding an array in the settings was hard - it would still inherit part of the base setting if the base array was longer. 
   We changed this: an array will always overwrite the complete base array.
   Note that this may trick you if you currently override a single array element with an environment variable. See :ref:`configure_levels`.
#. The element ``meta.source`` cannot be changed on subsequent updates to a resource (R4 specific)
#. SearchParameter ``StructureDefinition.ext-context`` yielded many errors in the log because the definition of the fhirpath in the specification is not correct. We provided a corrected version in errataFhir40.zip (see :ref:`feature_errata`).
#. :ref:`disable_interactions` was not evaluated for custom operations.
#. Delete of an instance accepted searchparameters on the url.
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
