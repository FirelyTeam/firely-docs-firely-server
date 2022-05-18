.. _vonk_available_plugins:

Plugins available for Firely Server
===================================

.. _vonk_plugins_infra:

Infrastructural plugins
-----------------------

.. _vonk_plugins_scheduler:

:Name: Scheduler
:Configuration: ``Vonk.Core.Quartz.QuartzConfiguration``
:License token: http://fire.ly/vonk/plugins/infra
:Order: 10
:Description: Registers a scheduler that can run jobs periodically. You can use this yourself, but with care (you don't want jobs slowing down the server):
   
   * Implement a ``Quartz.IJob``, let's say with class MyJob {...}
   * Have the ``Quartz.IScheduler`` injected
   * Call ``IScheduler.StartJob<MyJob>(TimeSpan runInterval, CancellationToken cancellationToken)``

.. _vonk_plugins_maintenance:

:Name: Maintenance
:Configuration: ``Vonk.Core.Infra.MaintenanceConfiguration``
:License token: http://fire.ly/vonk/plugins/infra
:Order: 20
:Description: Periodically cleans the indexed values for deleted or superceded resources from the database.

.. _vonk_plugins_license:

:Name: License
:Configuration: ``Vonk.Core.Licensing.LicenseConfiguration``
:License token: http://fire.ly/vonk/plugins/infra
:Order: 120
:Description: Registers the LicenseService that checks for a valid license. Without this plugin Firely Server does not work.

.. _vonk_plugins_serialization:

:Name: Serialization
:Configuration: ``Vonk.Core.Serialization.SerializationConfiguration``
:License token: http://fire.ly/vonk/plugins/infra
:Order: 130
:Description: Registers an implementation for the ``ISerializationService`` and ``ISerializationSupport`` interfaces and actual serializers and parsers for JSON and XML.

.. _vonk_plugins_pluggability:

:Name: Pluggability
:Configuration: ``Vonk.Core.Pluggability.PluggabilityConfiguration``
:License token: http://fire.ly/vonk/plugins/infra
:Order: 150
:Description: Registers services to dynamically build the ``IModelService`` using registered ``IModelContributor`` implementations.

.. _vonk_plugins_httptovonk:

:Name: Http to Vonk
:Configuration: ``Vonk.Core.Context.Http.HttpToVonkConfiguration``
:License token: http://fire.ly/vonk/plugins/http
:Order: 1110
:Description: Builds an :ref:`vonk_reference_api_ivonkcontext` out of the `HttpContext <https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext?view=aspnetcore-3.0>`_. You can only access the IVonkContext in the pipeline from plugins that have a higher order.

.. _vonk_plugins_vonktohttp:

:Name: Vonk to Http
:Configuration: ``Vonk.Core.Context.Http.VonkToHttpConfiguration``
:License token: http://fire.ly/vonk/plugins/http
:Order: 1120
:Description: Translates the response in the :ref:`vonk_reference_api_ivonkcontext` to a response on the `HttpContext <https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext?view=aspnetcore-3.0>`_. It honors the value of the prefer header if present. It also adds the VonkExceptionMiddleware to the pipeline as a last resort for catching exceptions.

.. _vonk_plugins_formatter:

:Name: Formatter
:Configuration: ``Vonk.Core.Context.Format.FormatConfiguration``
:License token: http://fire.ly/vonk/plugins/infra
:Order: 1130
:Description: Registers an implementation of IFormatter that can write the ``IVonkContext.Response.Payload`` to the response body in the requested format. Does not add a processor to the pipeline.

.. _vonk_plugins_longrunning:

:Name: Long running tasks
:Configuration: ``Vonk.Core.Infra.LongRunning.LongRunningConfiguration``
:License token: http://fire.ly/vonk/plugins/infra
:Order: 1170
:Description: If Vonk processes a task that could lead to inconsistent output, all other requests are rejected by this plugin. Long running tasks are e.g. the :ref:`conformance_import` and :ref:`feature_customsp_reindex`.

.. _vonk_plugins_compartments:

:Name: Compartments
:Configuration: ``Vonk.Core.Context.Features.CompartmentsConfiguration``
:License token: http://fire.ly/vonk/plugins/search
:Order: 1210
:Description: Recognizes a compartment in a compartment search on system or type level (see :ref:`restful_search`). It is added as a feature of type ``ICompartment`` to the ``IVonkContext.Features`` collection, to be used by :ref:`Search <vonk_plugins_search>` later on. This ICompartment feature will limit all queries to within the specified compartment.

.. _vonk_plugins_supportedinteractions:

:Name: Supported Interactions
:Configuration: ``Vonk.Core.Context.Guards.SupportedInteractionsConfiguration``
:License token: http://fire.ly/vonk/plugins/infra
:Order: 1220
:Description: Blocks interactions that are not listed as supported.
:Options: ``SupportedInteractions``, see :ref:`disable_interactions`.

.. _vonk_plugins_sizelimits:

:Name: Size Limits
:Configuration: ``Vonk.Core.Context.Guards.SizeLimitsConfiguration``
:License token: http://fire.ly/vonk/plugins/infra
:Order: 1225
:Description: Rejects bodies that are too large and bundles with too many entries.
:Options: ``SizeLimits``, see :ref:`sizelimits_options`

.. _vonk_plugins_urlmapping:

:Name: Url mapping
:Configuration: ``Vonk.Core.Context.UrlMapping.UrlMappingConfiguration``
:License token: http://fire.ly/vonk/plugins/infra
:Order: 1235
:Description: In a resource in the request, urls pointing to this instance of Firely Server are made relative. In a resource in the response, relative urls are made absolute, by adding the base url of the server. This way the server can be addressed in multiple ways (e.g. http://intranet.acme.com/fhir and https://fhir.acme.com) and still provide correct absolute urls. 

.. _vonk_plugins_defaultshapes:

:Name: Default Shapes
:Configuration: ``Vonk.Core.Context.Guards.DefaultShapesConfiguration``
:License token: http://fire.ly/vonk/plugins/infra
:Order: 4110
:Description: If no sort order is given for a search, ``_lastUpdated:asc`` is added. If no count is given for a search, ``_count=<default count>`` is added.
:Options: ``BundleOptions.DefaultCount``, see :ref:`bundle_options`.

.. _vonk_plugins_fhir_versions:

Support for different FHIR versions
-----------------------------------

.. _vonk_plugins_fhir_r3:

:Name: FHIR R3
:Configuration: ``Vonk.Fhir.R3.FhirR3Configuration``
:License token: http://fire.ly/vonk/plugins/fhirr3
:Order: 100
:Description: Registers services to support FHIR STU3 (or R3).

:Name: FHIR R3 Specification
:Configuration: ``Vonk.Fhir.R3.FhirR3SpecificationConfiguration``
:License token: http://fire.ly/vonk/plugins/fhirr3
:Order: 112
:Description: Registers an ``Hl7.Fhir.Specification.IStructureDefinitionSummaryProvider`` for FHIR STU3 (or R3).

:Name: FHIR R3 Validation
:Configuration: ``Vonk.Fhir.R3.Validation.ValidationConfigurationR3``
:License token: http://fire.ly/vonk/plugins/fhirr3
:Order: 4845
:Description: Registers a validator for FHIR STU3 (or R3).

.. _vonk_plugins_fhir_r4:

:Name: FHIR R4
:Configuration: ``Vonk.Fhir.R4.FhirR4Configuration``
:License token: http://fire.ly/vonk/plugins/fhirr4
:Order: 101
:Description: Registers services to support FHIR R4.

:Name: FHIR R4 Specification
:Configuration: ``Vonk.Fhir.R4.FhirR4SpecificationConfiguration``
:License token: http://fire.ly/vonk/plugins/fhirr4
:Order: 112
:Description: Registers an ``Hl7.Fhir.Specification.IStructureDefinitionSummaryProvider`` for FHIR R4.

:Name: FHIR R4 Validation
:Configuration: ``Vonk.Fhir.R4.Validation.ValidationConfigurationR4``
:License token: http://fire.ly/vonk/plugins/fhirr4
:Order: 4845
:Description: Registers a validator for FHIR R4.

.. _vonk_plugins_rest:

FHIR RESTful interactions
-------------------------

.. _vonk_plugins_read:

:Name: Read
:Configuration: ``Vonk.Core.Operations.Crud.ReadConfiguration``
:License token: http://fire.ly/vonk/plugins/read
:Order: 4410
:Description: Implements FHIR instance read. It will return the Resource that matches the id *and* the FHIR version. If a Resource with matching id is found with another FHIR version you are notified.

.. _vonk_plugins_create:

:Name: Create
:Configuration: ``Vonk.Core.Operations.Crud.CreateConfiguration``
:License token: http://fire.ly/vonk/plugins/create
:Order: 4420
:Description: Implements FHIR type create.

.. _vonk_plugins_update:

:Name: Update
:Configuration: ``Vonk.Core.Operations.Crud.UpdateConfiguration``
:License token: http://fire.ly/vonk/plugins/update
:Order: 4430
:Description: Implements FHIR instance update, with support for 'upsert': creating a Resource with a pre-assigned id. Note that id's must be unique across FHIR versions.

.. _vonk_plugins_patch:

:Name: Patch
:Configuration: ``Vonk.Core.Operations.Crud.FhirPatchConfiguration``
:License token: http://fire.ly/vonk/plugins/update
:Order: 4433
:Description: Implements FHIR instance patch, as specified by `FHIR Patch <http://hl7.org/fhir/fhirpatch.html>`_.

.. _vonk_plugins_delete:

:Name: Delete
:Configuration: ``Vonk.Core.Operations.Crud.DeleteConfiguration``
:License token: http://fire.ly/vonk/plugins/delete
:Order: 4440
:Description: Implements FHIR instance delete. Since id's in Firely Server must be unique across FHIR versions, the delete is issued on the provided id, regardless of the FHIR version.

.. _vonk_plugins_search:

:Name: Search
:Configuration: ``Vonk.Core.Operations.Search.SearchConfiguration``
:License token: http://fire.ly/vonk/plugins/search
:Description: Implements FHIR Search on system and type level. For data access it uses the registered implementation of ISearchRepository, which can be any of the implementations provided by Firely Server or an implementation provided by a Facade plugin. The implementations provided by Firely Server also require the Index plugin to extract searchparameter values from the resources.
:Order: 4220
:Options: 
   * ``AdministrationImportOptions``, see :ref:`configure_admin_import`, for available Searchparameters
   * ``SupportedModel.RestrictToSearchParameters``, see :ref:`supportedmodel` for available Searchparameters
   * ``BundleOptions``, see :ref:`bundle_options`, for number of returned results
   
   See :ref:`vonk_reference_api_isearchrepository` and :ref:`vonk_facade`.

:Name: Search support
:Configuration: ``Vonk.Core.Repository.RepositorySearchSupportConfiguration``
:License token: http://fire.ly/vonk/plugins/search
:Order: 140
:Description: Registers services required for Search. It is automatically registered by Search.

.. _vonk_plugins_index:

:Name: Index
:Configuration: ``Vonk.Core.Repository.RepositoryIndexSupportConfiguration``
:License token: http://fire.ly/vonk/plugins/index
:Order: 141
:Description: Extracts values matching Searchparameters from resources, so they can be searched on.

.. _vonk_plugins_include:

:Name: Include
:Configuration: ``Vonk.Core.Operations.Search.IncludeConfiguration``
:License token: http://fire.ly/vonk/plugins/include
:Order: 4210
:Description: Implements ``_include`` and ``_revinclude``. This acts on the result bundle of a search. Therefore it also works out of the box for Facade implementations, provided that the Facade implements support for the reference Searchparameters that are used in the _(rev)include.

.. _vonk_plugins_elements:

:Name: Elements
:Configuration: ``Vonk.Core.Context.Elements.ElementsConfiguration``
:License token: http://fire.ly/vonk/plugins/search
:Order: 1240
:Description: Applies the ``_elements`` parameter to the Resource that is in the response (single resource or bundle).

.. _vonk_plugins_summary:

:Name: Summary
:Configuration: ``Vonk.Core.Context.Elements.SummaryConfiguration``
:License token: http://fire.ly/vonk/plugins/search
:Order: 1240
:Description: Applies the ``_summary`` parameter to the Resource that is in the response (single resource or bundle).

.. _vonk_plugins_history:

:Name: History
:Configuration: ``Vonk.Core.Operations.History.HistoryConfiguration``
:License token: http://fire.ly/vonk/plugins/history
:Order: 4610
:Description: Implements ``_history`` on system, type and instance level.
:Options: ``BundleOptions``, see :ref:`bundle_options`

.. _vonk_plugins_versionread:

:Name: Version Read
:Configuration: ``Vonk.Core.Operations.History.VersionReadConfiguration``
:License token: http://fire.ly/vonk/plugins/history
:Order: 4620
:Description: Implements reading a specific version of a resource (``<base>/Patient/123/_history/v3``).

.. _vonk_plugins_capability:

:Name: Capability
:Configuration: ``Vonk.Core.Operations.Capability.CapabilityConfiguration``
:License token: http://fire.ly/vonk/plugins/capability
:Order: 4120
:Description: Provides the CapabilityStatement on the ``<base>/metadata`` endpoint. The CapabilityStatement is tailored to the FHIR version of the request. The CapabilityStatement is built dynamically by visiting all the registered implementations of ICapabilityStatementContributor, see :ref:`vonk_architecture_capabilities`.

.. _vonk_plugins_conditional_create:

:Name: Conditional Create
:Configuration: ``Vonk.Core.Operations.ConditionalCrud.ConditionalCreateConfiguration``
:License token: http://fire.ly/vonk/plugins/conditionalcreate
:Order: 4510
:Description: Implements FHIR conditional create.

.. _vonk_plugins_conditional_update:

:Name: Conditional Update
:Configuration: ``Vonk.Core.Operations.ConditionalCrud.ConditionalUpdateConfiguration``
:License token: http://fire.ly/vonk/plugins/conditionalupdate
:Order: 4520
:Description: Implements FHIR conditional update.

.. _vonk_plugins_conditional_delete:

:Name: Conditional Delete
:Configuration: ``Vonk.Core.Operations.ConditionalCrud.ConditionalDeleteConfiguration``
:License token: http://fire.ly/vonk/plugins/conditionaldelete
:Order: 4530
:Description: Implements FHIR conditional delete.
:Options: ``FhirCapabilities.ConditionalDeleteOptions``, see :ref:`fhir_capabilities`

.. _vonk_plugins_validation:

:Name: Validation
:Configuration: ``Vonk.Core.Operations.Validation.ValidationConfiguration``
:License token: http://fire.ly/vonk/plugins/validation
:Order: 4000
:Description: Implements `FHIR $validate <http://hl7.org/fhir/R4/resource-operation-validate.html>`_ on type and instance level for POST: ``POST <base>/Patient/$validate`` or ``POST <base>/Patient/123/$validate``.

.. _vonk_plugins_instance_validation:

:Name: Instance Validation
:Configuration: ``Vonk.Core.Operations.Validation.InstanceValidationConfiguration``
:License token: http://fire.ly/vonk/plugins/validation
:Order: 4840
:Description: Implements `FHIR $validate <http://hl7.org/fhir/R4/resource-operation-validate.html>`_ on instance level for GET: ``GET <base>/Patient/123/$validate``

.. _vonk_plugins_structural_validation:

:Name: Structural Validation
:Configuration: ``Vonk.Core.Operations.Validation.StructuralValidationConfiguration``
:License token: http://fire.ly/vonk/plugins/validation
:Order: 1227
:Description: Validates the structure of resources sent to Firely Server (is it valid FHIR JSON or XML?).

.. _vonk_plugins_prevalidation:

:Name: Prevalidation
:Configuration: ``Vonk.Core.Operations.Validation.PreValidationConfiguration``
:License token: http://fire.ly/vonk/plugins/validation
:Order: 1228
:Description: Validates resources sent to Firely Server against their stated profile compliance (in Resource.meta.profile). The strictness of the validation is controlled by the options.
:Options: ``Validation``, see :ref:`validation_options`

.. _vonk_plugins_profile_filter:

:Name: Profile filter
:Configuration: ``Vonk.Core.Operations.Validation.ProfileFilterConfiguration``
:License token: http://fire.ly/vonk/plugins/validation
:Order: 4310
:Description: Blocks resources that do not conform to a list of profiles.
:Options: ``Validation.AllowedProfiles``, see :ref:`validation_options`

.. _vonk_plugins_meta:

:Name: Meta
:Configuration: ``Vonk.Core.Operations.MetaOperation.MetaConfiguration``
:License token: http://fire.ly/vonk/plugins/meta
:Order: 5180
:Description: Implements FHIR $meta on instance level.

:Name: Meta Add
:Configuration: ``Vonk.Core.Operations.MetaOperation.MetaAddConfiguration``
:License token: http://fire.ly/vonk/plugins/meta
:Order: 5190
:Description: Implements FHIR $meta-add on instance level.

:Name: Meta Delete
:Configuration: ``Vonk.Core.Operations.MetaOperation.MetaDeleteConfiguration``
:License token: http://fire.ly/vonk/plugins/meta
:Order: 5195
:Description: Implements FHIR $meta-delete on instance level.

.. _vonk_plugins_snapshot:

:Name: Snapshot Generation
:Configuration: ``Vonk.Core.Operations.SnapshotGeneration.SnapshotGenerationConfiguration``
:License token: http://fire.ly/vonk/plugins/snapshotgeneration
:Order: 4850
:Description: Implements `FHIR $snapshot <http://hl7.org/fhir/R4/structuredefinition-operation-snapshot.html>`_ on a type level: ``POST <base>/administration/StructureDefinition/$snapshot``.

.. _vonk_plugins_batch:

:Name: Batch
:Configuration: ``Vonk.Core.Operations.Transaction.FhirBatchConfiguration``
:License token: http://fire.ly/vonk/plugins/batch
:Order: 3110
:Description: Processes a batch Bundle by sending each entry through the rest of the processing pipeline and gathering the results.
:Options: ``SizeLimits``, see :ref:`sizelimits_options`

.. _vonk_plugins_transaction:

:Name: Transaction
:Configuration: ``Vonk.Core.Operations.Transaction.FhirTransactionConfiguration``
:License token: http://fire.ly/vonk/plugins/transaction
:Order: 3120
:Description: Process a transaction Bundle by sending each entry through the rest of the processing pipeline and gathering the results. Different from Batch, Transaction succeeds or fails as a whole. Transaction requires an implementation of ``Vonk.Core.Repository.IRepoTransactionService`` for transaction support by the underlying repository. The SQL Server and SQLite implementations provides a real one, whereas the MongoDb provides a simulated implementation, to allow you to experiment with transactions on MongoDb.
:Options: 
   * ``SizeLimits``, see :ref:`validation_options`
   * ``Repository``, see :ref:`configure_repository`

.. _vonk_plugins_lastn:

:Name: LastN
:Configuration: ``Vonk.Plugin.LastN.LastNConfiguration``
:License token: http://fire.ly/vonk/plugins/lastn
:Order: 5007
:Description: Implements `FHIR $lastn <https://www.hl7.org/fhir/observation-operation-lastn.html>`_ on Observation resources.

.. _vonk_plugins_erase:

:Name: Erase
:Configuration: ``Vonk.Plugin.EraseOperation.EraseOperationConfiguration``
:License token: http://fire.ly/vonk/plugins/erase
:Order: 5300
:Description: Provides functionality to hard-delete FHIR resources in Firely Server database as opposed to the soft-delete used by default.

.. _vonk_plugins_terminology:

Terminology
-----------

.. _vonk_plugins_codesystem_lookup:

:Name: CodeSystem Lookup
:Configuration: ``Vonk.Plugins.Terminology.[R3|R4|R5].CodeSystemLookupConfiguration``
:License token: http://fire.ly/vonk/plugins/terminology
:Order: 5110
:Description: Implements FHIR `$lookup <http://hl7.org/fhir/codesystem-operation-lookup.html>`_ on type level requests: ``POST <base>/administration/CodeSystem/$lookup`` or ``GET <base>/administration/CodeSystem/$lookup?...``

.. _vonk_plugins_codesystem_compose:

:Name: CodeSystem FindMatches / Compose
:Configuration: ``Vonk.Plugins.Terminology.CodeSystemFindMatchesConfiguration``
:License token: http://fire.ly/vonk/plugins/terminology
:Order: 5220
:Description: Implements FHIR `$compose <http://hl7.org/fhir/codesystem-operation-find-matches.html>`_ on type level requests: ``POST <base>/administration/CodeSystem/$find-matches``and on instance level requests: ``POST <base>/administration/CodeSystem/[id]/$find-matches`` or ``GET <base>/administration/CodeSystem/[id]/$find-matches?...``

.. _vonk_plugins_valueset_validatecode:

:Name: ValueSet Validate Code
:Configuration: ``Vonk.Plugins.Terminology.ValueSetValidateCodeConfiguration``
:License token: http://fire.ly/vonk/plugins/terminology
:Order: 5120
:Description: Implements FHIR `$validate-code <http://hl7.org/fhir/codesystem-operation-validate-code.html>`_ on type level requests: ``POST <base>/administration/ValueSet/$validate-code`` and instance level requests: ``GET <base>/administration/ValueSet/[id]/$validate-code?...`` and ``POST <base>/administration/ValueSet/[id]/$validate-code``

.. _vonk_plugins_valueset_expand:

:Name: ValueSet Expand
:Configuration: ``Vonk.Plugins.Terminology.ValueSetExpandConfiguration``
:License token: http://fire.ly/vonk/plugins/terminology
:Order: 5140
:Description: Implements FHIR `$expand <http://hl7.org/fhir/codesystem-operation-expand.html>`_ on instance level requests: ``GET <base>/administration/ValueSet/[id]/$expand?...`` and ``POST <base>/administration/ValueSet/[id]/$expand`` and on type level requests: ``POST <base>/administration/ValueSet/$expand``.

.. _vonk_plugins_conceptmap_translate:

:Name: ConceptMap Translate
:Configuration: ``Vonk.Plugins.Terminology.ConceptMapTranslateConfiguration``
:License token: http://fire.ly/vonk/plugins/terminology
:Order: 5260
:Description: Implements FHIR `$translate <http://hl7.org/fhir/conceptmap-operation-translate.html>`_ on instance level requests: ``GET <base>/administration/ConceptMap/[id]/$translate?...`` and ``POST <base>/administration/ValueSet/[id]/$translate`` and on type level requests: ``POST <base>/administration/ConceptMap/$translate``.

.. _vonk_plugins_codesystem_subsumes:

:Name: CodeSystem Subsumes
:Configuration: ``Vonk.Plugins.Terminology.CodeSystemSubsumesConfiguration``
:License token: http://fire.ly/vonk/plugins/terminology
:Order: 5280
:Description: Implements FHIR `$subsumes <http://hl7.org/fhir/codesystem-operation-subsumes.html>`_ on instance level requests: ``GET <base>/administration/CodeSystem/[id]/$subsumes?...`` and on type level requests: ``POST <base>/administration/CodeSystem/$subsumes`` or ``GET <base>/administration/CodeSystem/$subsumes?...``

.. _vonk_plugins_codesystem_closure:

:Name: CodeSystem Closure
:Configuration: ``Vonk.Plugins.Terminology.CodeSystemClosureConfiguration``
:License token: http://fire.ly/vonk/plugins/terminology
:Order: 5300
:Description: Implements FHIR `$closure <http://hl7.org/fhir/codesystem-operation-closure.html>`_ on system level requests: ``POST <base>/administration/$closure``

.. _vonk_plugins_smart:

SMART on FHIR
-------------

:Name: SMART on FHIR
:Configuration: ``Vonk.Smart.SmartConfiguration.SmartConfiguration``
:License token: http://fire.ly/vonk/plugins/smartonfhir
:Order: 2000
:Description: Implements SMART on FHIR authentication and authorization, see :ref:`feature_accesscontrol`. 

.. _vonk_plugins_subscriptions:

Subscriptions
-------------

:Name: Subscriptions
:Configuration: ``Vonk.Subscriptions.SubscriptionConfiguration.SubscriptionConfiguration``
:License token: http://fire.ly/vonk/plugins/subscriptions
:Order: 3200
:Description: Implements the FHIR Subscriptions framework, see :ref:`feature_subscription`. 

.. _vonk_plugins_audit:

Auditing
--------

:Name: Username log
:Configuration: ``Vonk.Plugin.Audit.UsernameLoggingConfiguration``
:License token: http://fire.ly/vonk/plugins/audit
:Order: 2010
:Description: Makes the user id and name from the JWT token (if present) available for logging. See :ref:`feature_auditing` for more info.

:Name: Audit logging for transactions
:Configuration: ``Vonk.Plugin.Audit.AuditTransactionConfiguration``
:License token: http://fire.ly/vonk/plugins/audit
:Order: 3100
:Description: Logs requests and responses for transactions to a file. See :ref:`feature_auditing` for more info.

:Name: Audit log
:Configuration: ``Vonk.Plugin.Audit.AuditConfiguration``
:License token: http://fire.ly/vonk/plugins/audit
:Order: 3150
:Description: Logs requests and responses to a file. See :ref:`feature_auditing` for more info.

:Name: AuditEvent logging for transactions
:Configuration: ``Vonk.Plugin.Audit.AuditEventTransactionConfiguration``
:License token: http://fire.ly/vonk/plugins/audit
:Order: 3160
:Description: Logs requests and responses for transactions to the database. See :ref:`feature_auditing` for more info.

:Name: AuditEvent logging
:Configuration: ``Vonk.Plugin.Audit.AuditEventConfiguration``
:License token: http://fire.ly/vonk/plugins/audit
:Order: 3170
:Description: Logs requests and responses to the database. See :ref:`feature_auditing` for more info.

.. _vonk_plugins_demoui:

Demo UI
-------

:Name: Demo UI
:Configuration: ``Vonk.UI.Demo.DemoUIConfiguration.DemoUIConfiguration``
:License token: http://fire.ly/vonk/plugins/demoui
:Order: 800
:Description: Provides the landing page that you see when you request the base url from a browser. If you want to provide your own landing page, replace this plugin with your own. There is an example of that, see :ref:`vonk_plugins_landingpage`.

.. _vonk_plugins_document:

Documents
---------

.. _vonk_plugins_documentoperation:

:Name: Document generation
:Configuration: ``Vonk.Plugins.DocumentOperation.DocumentOperationConfiguration``
:License token: http://fire.ly/vonk/plugins/document
:Order: 4900
:Description: Implements FHIR `$document <http://hl7.org/fhir/R4/composition-operation-document.html>`_ : ``POST <base>/Composition/$document`` or ``GET <base>/Composition/[id]/$document``
:Code: `GitHub <https://github.com/FirelyTeam/Vonk.Plugin.DocumentOperation>`_

.. _vonk_plugins_documentsigning:

:Name: Document signing
:Configuration: ``Vonk.Plugins.SignatureService.SignatureConfiguration``
:License token: http://fire.ly/vonk/plugins/signature
:Order: 4899
:Description: Signs a document generated by :ref:`$document <vonk_plugins_documentoperation>`.

.. _vonk_plugins_documentendpoint:

:Name: Document endpoint
:Configuration: ``Vonk.Plugins.DocumentHandling.DocumentHandlingConfiguration``
:License token: http://fire.ly/vonk/plugins/documenthandling
:Order: 4950
:Description: Allows `FHIR document bundles <https://www.hl7.org/fhir/documents.html#3.3>`_ to be posted to the base endpoint. Consult the :ref:`documentation <restful_documenthandling>` for more information.

.. _vonk_plugins_convert:

Conversion
----------

:Name: Format conversion
:Configuration: ``Vonk.Plugins.ConvertOperation.ConvertOperationConfiguration``
:License token: http://fire.ly/vonk/plugins/convert
:Order: 4600
:Description: Implements FHIR `$convert <http://hl7.org/fhir/R4/resource-operation-convert.html>`_ : ``POST <base>/$convert`` to convert between JSON and XML representation.

.. _vonk_plugins_binary:

Binary
------

.. _vonk_plugins_binarywrapper:

:Name: Binary wrapper (Encode)
:Configuration: ``Vonk.Plugins.BinaryWrapper.BinaryEncodeConfiguration``
:License token: http://fire.ly/vonk/plugins/binarywrapper
:Order: 1112
:Description: Wraps an incoming binary format in a Binary resource for further processing by the pipeline.
:Settings:
   ::

      "Vonk.Plugin.BinaryWrapper":{
         "RestrictToMimeTypes": ["application/pdf", "text/plain", "image/png", "image/jpeg"]
      },

:Name: Binary wrapper (Decode)
:Configuration: ``Vonk.Plugins.BinaryWrapper.BinaryDecodeConfiguration``
:License token: http://fire.ly/vonk/plugins/binarywrapper
:Order: 1122
:Description: Implements ``GET <base>/Binary/<id>``, retrieve back the Binary resource in its native format.

.. _vonk_plugins_repository:

Repository implementations
--------------------------

.. _vonk_plugins_repository_memory:

:Name: Memory Repository
:Configuration: ``Vonk.Repository.MemoryConfiguration``
:license token: http://fire.ly/vonk/plugins/repository/memory
:Order: 210
:Description: Implements a repository in working memory that fully supports all of the capabilities of Firely Server. This implementation is mainly used for unittesting.

:Name: Memory Administration Repository
:Configuration: ``Vonk.Repository.MemoryAdministrationConfiguration``
:license token: http://fire.ly/vonk/plugins/repository/memory
:Order: 211
:Description: Implements a repository in working memory for the Administration API. This implementation is mainly used for unittesting.

.. _vonk_plugins_repository_mongodb:

:Name: MongoDb Repository
:Configuration: ``Vonk.Repository.MongoDbConfiguration``
:license token: http://fire.ly/vonk/plugins/repository/mongo-db
:Order: 230
:Description: Implements a repository in MongoDb that fully supports all of the capabilities of Firely Server, except Transactions.

:Name: MongoDb Administration Repository
:Configuration: ``Vonk.Repository.MemoryAdministrationConfiguration``
:license token: http://fire.ly/vonk/plugins/repository/mongo-db
:Order: 231
:Description: Implements a repository in MongoDb for the Administration API.

.. _vonk_plugins_repository_sqlite:

:Name: SQLite Repository
:Configuration: ``Vonk.Repository.SqliteConfiguration``
:license token: http://fire.ly/vonk/plugins/repository/sqlite
:Order: 240
:Description: Implements a repository in SQLite that fully supports all of the capabilities of Firely Server.

:Name: SQLite Administration Repository
:Configuration: ``Vonk.Repository.SqliteAdministrationConfiguration``
:license token: http://fire.ly/vonk/plugins/repository/sqlite
:Order: 241
:Description: Implements a repository in SQLite for the Administration API.

.. _vonk_plugins_repository_sql:

:Name: SQL Server Repository
:Configuration: ``Vonk.Repository.SqlConfiguration``
:license token: http://fire.ly/vonk/plugins/repository/sql-server
:Order: 220
:Description: Implements a repository in SQL Server that fully supports all of the capabilities of Firely Server.

:Name: SQL Server Administration Repository
:Configuration: ``Vonk.Repository.SqlAdministrationConfiguration``
:license token: http://fire.ly/vonk/plugins/repository/sql-server
:Order: 221
:Description: Implements a repository in SQL Server for the Administration API.

.. _vonk_plugins_administration:

Administration API
------------------

:Name: Administration API
:Configuration: ``Vonk.Administration.Api.AdministrationOperationConfiguration``
:license token: http://fire.ly/vonk/plugins/administration
:Order: 1160
:Description: Sets up a sequence of plugins for the Administration API. Administration API is different from general plugins since it branches off of the regular processing pipeline and sets up a second pipeline for the /administration endpoint.

:Name: Fhir STU3 Administration services
:Configuration: ``Vonk.Administration.FhirR3.RepositoryConfigurationR3``
:license token: http://fire.ly/vonk/plugins/administration/fhirr3
:Order: 4310
:Description: Implements support services to work with FHIR STU3 conformance resources in the Administration API.

:Name: Fhir R4 Administration services
:Configuration: ``Vonk.Administration.FhirR4.RepositoryConfigurationR4``
:license token: http://fire.ly/vonk/plugins/administration/fhirr4
:Order: 4310
:Description: Implements support services to work with FHIR R4 conformance resources in the Administration API.

Bulk Data
---------

:Name: System Bulk Data Export
:Configuration: ``Vonk.Plugin.BulkDataExport.SystemBulkDataExportConfiguration``
:license token: ``http://fire.ly/vonk/plugins/bulk-data-export``
:Order: 5003
:Description: Support for system-level ``$export`` operation. See :ref:`feature_bulkdataexport`.

:Name: Group Bulk Data Export
:Configuration: ``Vonk.Plugin.BulkDataExport.GroupBulkDataExportConfiguration``
:license token: ``http://fire.ly/vonk/plugins/bulk-data-export``
:Order: 5004
:Description: Support for instance-level ``$export`` operation. See :ref:`feature_bulkdataexport`.

:Name: Patient Bulk Data Export
:Configuration: ``Vonk.Plugin.BulkDataExport.PatientBulkDataExportConfiguration``
:license token: ``http://fire.ly/vonk/plugins/bulk-data-export``
:Order: 5005
:Description: Support for type-level ``$export`` operation. See :ref:`feature_bulkdataexport`.

:Name: Patient everything
:Configuration: ``Vonk.Plugin.PatientEverything``
:license token: ``http://fire.ly/vonk/plugins/patient-everything``
:Order: 5006
:Description: Request a Patient record. See :ref:`feature_patienteverything`.


