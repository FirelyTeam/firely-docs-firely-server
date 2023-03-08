.. _vonk_releasenotes_history_v5:

Current Firely Server release notes (v5.x)
==========================================

.. _vonk_releasenotes_5_0_0:

Release 5.0.0, February 9th, 2023
----------------------------------

We are thrilled to announce the release of our new major version 5.0 of Firely Server. The team has worked hard to incorporate new features and improvements that we believe will enhance your experience greatly. We are excited to share this new release with our customers and look forward to their feedback.

Configuration
^^^^^^^^^^^^^
.. attention::
    Parts of the configuration were overhauled.
    If you have adjusted the :ref:`appsettings<configure_appsettings>` either in ``appsettings.instance.json`` or in environment variables, 
    make sure to to update your configuration accordingly. Please follow the bullets below.

#. The configuration section for additional endpoints in the discovery document and additional issuers in tokens has been reworked. Consult the :ref:`SMART Configuration section<feature_accesscontrol_config>` for more details.
#. The client id of the default SMART authorization options have been changed from ``vonk`` to ``firelyserver``.
#. Add this new namespace to the root (``/``) path of the :ref:`PipelineOptions<settings_pipeline>`: ``Vonk.Plugin.Operations``. The result should look like this:

    .. code-block::
        :emphasize-lines: 8

        "PipelineOptions": {
            "PluginDirectory": "./plugins",
            "Branches": [
            {
                "Path": "/",
                "Include": [
                    "Vonk.Core",
                    "Vonk.Plugin.Operations",
                    "Vonk.Fhir.R3",
                    "Vonk.Fhir.R4",
                    //etc.
                ]
            },
            {
                "Path": "/administration",
                "Include": [
                    "Vonk.Core",
                    //etc.
                ]
            }
            ]
        }


Database
^^^^^^^^

#. Due to improvements for searches on version-specific references, the database was updated for both **SQL Server** and **MongoDB**. Firely Server will usually perform the upgrade automatically. For details, see :ref:`migrations`.

   #. SQL Server is upgraded from schema 25 to **26**. The upgrade script file is named ``/sqlserver/FS_SchemaUpgrade_Data_v25_v26.sql``.
   #. MongoDB is upgraded from schema 24 to **25**. The upgrade script file is named ``/mongodb/FS_SchemaUpgrade_Data_v24_v25``.
   #. The administration database is not affected by this change, so you don't need to upgrade that.

#. The database upgrade means that you also need an upgraded version of Firely Server Ingest, :ref:`version 2.0<fsi_releasenotes_2.0.1>`

Feature
^^^^^^^

#. The initial public version of Firely Auth has been released. Firely Auth is an optimized OAuth2 provider that understands SMART on FHIR scopes and the FHIR resource types they apply to out of the box. See :ref:`firely_auth_index` for more information.
#. The default information model for Firely Server is now R4.
#. FHIR R5 (based on v5.0.0-snapshot3) is now officially supported and not considered experimental anymore. We will also support the final release of FHIR R5 once it is published.

   .. attention::
       If you used R5 with Firely Server before and your administration database is either SQL or MongoDB based, you need to either delete it or reimport all FHIR R5 artifacts. If you use SQLite, you should use our new administration database that is distributed with Firely Server. If you need any assistance, please :ref:`contact us<vonk-contact>`.

#. Bulk Data Export now supports SMART on FHIR v2.
#. Our :ref:`SMART on FHIR documentation <feature_accesscontrol>` has been updated for SMART on FHIR v2.
#. Firely Server now generates FHIR AuditEvent resources conforming to `IHE Basic Audit Log Patterns <https://profiles.ihe.net/ITI/BALP/index.html>`_. Fields that are included in the audit event log and AuditEvent resources now contain the same content.
#. Contents of AuditEvents can now be modified via a plugin. See :ref:`AuditEvent customization <audit_event_customization>` for further info.
#. Two new operations have been added, namely ``$verify-integrity`` and ``$verify-integrity-status``. These allow you to verify that no AuditEvents have been manipulated on the server. See :ref:`audit_event_integrity` on how to use this feature.
#. You can now add signatures to ``AuditEvents``. See :ref:`audit_event_integrity` for more information.
#. Firely Server now supports searching on version-specific references. Consult the `FHIR specification <https://www.hl7.org/fhir/search.html#versions>`_ for more information.
#. Serilog CorrelationId support has been enabled in Firely Server. Please consult the `official documentation <https://github.com/ekmsystems/serilog-enrichers-correlation-id>`_ on how to configure it.
#. We have added a public :ref:`Postman collection <postman_tutorial>` to test Firely Server's RESTful endpoints.
#. Wildcard support for ``include`` is now declared in Firely Server's ``CapabilityStatement``.

Fix
^^^

#. When performing a Bulk Data Export request with a Firely Server instance running on a SQL database, it will return the Group resource even if it has no members. 
#. FS now declares support for Bulk Data Export Group export operations in its CapabilityStatement. This features was available before, but missing from FS's CapabilityStatement. 
#. Bulk Data Export now returns a succesful status code (``202``) instead of an erroneous status code if no resources were matched for an export. The resulting export will include an empty array as described in the `specification <https://hl7.org/fhir/uv/bulkdata/export/index.html#response---complete-status>`_.
#. Upon commencing a Bulk Data Export, Firely Server now correctly handles ``Prefer`` headers as outlined `in the specification <https://hl7.org/fhir/uv/bulkdata/export/index.html#headers>`_.
#. ``Device`` can now be added as an additional resource in a Bulk Data export.
#. Search parameters without a value are now ignored by the server instead of resulting in an error response.
#. Firely Server now creates valid FHIR R5 AuditEvents.
#. Searching for a resource with multiple sort fields does not throw an exception anymore when Firely Server runs on a SQL database.
#. When using the ``If-Modified-Since`` header, only resources that were modified after the specified timestamp are returned. Because of a precision mismatch (seconds vs. milliseconds), wrong resources were sometimes returned before this fix.
#. When updating a deleted resource conditionally, Firely Server does not throw an exception anymore.
#. Firely Server now returns the correct issue code (``business-rule`` instead of ``invalid``) in the OperationOutcome when performing a conditional update using ``_id`` as a parameter. Additionally, the error message has been improved when a resource in a different information model is matched via the ``id`` field.
#. When executing a ``POST``-based search, Firely Server will now return the correct self-link as seen in ``GET``-based searches.
#. Firely Server now returns improved error messages if the client is not allowed to perform searches due to insufficient SMART v2 scopes.
#. Support for Firely Server using a SQLite database on arm64-based Macs was improved. 
#. During SMART on FHIR v2 discovery, Firely Server now returns the ``grant_types_supported`` field.
#. Firely Server now returns the correct CodeSystem ``http://terminology.hl7.org/CodeSystem/restful-security-service`` within the security section of its ``CapabilityStatement``. Before this change, the old R3 CodeSystem ``http://hl7.org/fhir/restful-security-service`` was falsely returned.
#. Firely Server will now handle duplicate DLLs and assemblies more gracefully in case they were accidentally added to its plugin directory.
#. When overwriting Search Parameters, the new Search Parameters will now be included in the CapabilityStatement instead of the overwritten ones. This feature was introduced with Firely Server ``4.7.0`` but broke in between the last releases.
#. The two SearchParameters ``ConceptMap-target-uri`` and ``ConceptMap-source-uri`` for ``ConceptMap`` have been fixed.
#. For FHIR STU3 and R4, ``Contract``, ``GuidanceResponse`` and ``Task`` have been added to the ``Patient`` compartment.
#. Firely Server now returns a ``404`` and ``OperationOutcome`` when the status of a cancelled export is requested.
#. When preloading resources via Firely Server's import feature, no more errors will be logged if subfolders are present.
#. Warnings and errors with regards to ``AuditEvent`` indexing problems have been fixed and will no longer appear in the log.
#. Searches on ``period`` elements that have equal start/end times either at the start or beginning of the year will now return the correct results. Previously, these searches did not return any results.
#. The US Core ``patient`` search parameters have been fixed. They now only target ``Patient``, not ``Group`` and ``Patient``.
#. The response for unsupported ``Prefer`` headers has been improved. The ``Prefer`` header's value is now included in the ``OperationOutcome``.
#. Firely Server will now respond with a ``408`` instead of a ``500`` status code in case the ``$everything`` operation times out.
#. Custom ``SearchParameters`` can now include the character '-' in ``code``.
#. The copyright data in Firely Server's executable has been updated.
#. Miscellaneous flaws in Firely Server's `Swagger documentation UI <_static/swagger>`_ have been fixed.

Security
^^^^^^^^

#. All of the contents included in Firely Server's index page are now hosted locally which prevents attackers from injecting malicious Javascript via manipulating externally hosted content.

Plugin and Facade
^^^^^^^^^^^^^^^^^

#. Firely Server and internal plugins now use the `Firely .NET SDK 5.0.0 <https://github.com/FirelyTeam/firely-net-sdk/releases/tag/v5.0.0>`_. Follow the link for an overview of all changes.
#. ``Vonk.Core`` now targets ``net6.0``. 

   .. warning::
       Due to the above changes, all of your plugins need to be recompiled against this FS release.

#. Please note that the ``Vonk.Smart`` package will not be published on NuGet anymore.
#. The ``appsettings`` in our `Vonk.Facade.Starter project <https://github.com/FirelyTeam/Vonk.Facade.Starter>`_ now reflect the namespace changes introduced with FS 5.0.0.

API cleanup (relevant to plugin developers)
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

We cleaned up the public API: classes and methods that had been earlier marked as deprecated have now been made private and therefore not available for plugin developers anymore. This makes us more flexible in developing Firely Server in the future because we don't need to maintain the functionality that anyone has hardly used. If you find out that something that you've been using in the previous versions is not available anymore, please get in touch with us.

Additionally, in many places where we used to refer to SearchParameter.name, we are now using SearchParameter.code. This was made to be more aligned with the specification. For you, as a plugin developer, that means several changes:

* Class ``Vonk.Core.Common.VonkConstants.ParameterNames`` has been renamed to ``Vonk.Core.Common.VonkConstants.ParameterCodes``
* Method ``static VonkSearchParameter IModelServiceExtensions.FindSearchParameterByName`` has been renamed to ``static VonkSearchParameter FindSearchParameterByCode``
* Method ``static IEnumerable<VonkSearchParameter> IModelServiceExtensions.FindSearchParametersByName`` has been renamed to ``static IEnumerable<VonkSearchParameter> IModelServiceExtensions.FindSearchParametersByCode``
* Property ``String VonkSearchParameter.Name`` has been renamed to ``String VonkSearchParameter.Code``
* Property ``String VonkSearchParameterComponent.ParameterName`` has been renamed to ``String VonkSearchParameterComponent.ParameterCode``

.. container:: toggle

    .. container:: header

        List of classes/structs/interfaces removed from the public API

    .. code ::

        Vonk.Core.Common.IGenericResourceResolver
        Vonk.Core.Common.VonkConstants.ParameterNames
            renamed to Vonk.Core.Common.VonkConstants.ParameterCodes
        Vonk.Core.Configuration.ConfigurationLogger
        Vonk.Core.Configuration.CoreConfiguration
        Vonk.Core.Conformance.ConformanceConfiguration
        Vonk.Core.Conformance.IConformanceCache
        Vonk.Core.Conformance.IConformanceCacheInvalidation
        Vonk.Core.Context.ContextConfiguration
        Vonk.Core.Context.Elements.ElementsConfiguration
        Vonk.Core.Context.Elements.ElementsHandler
        Vonk.Core.Context.Elements.ElementsMiddleware
        Vonk.Core.Context.Elements.SummaryConfiguration
        Vonk.Core.Context.Elements.SummaryMiddleware
        Vonk.Core.Context.Features.CompartmentFeatureMiddleware
        Vonk.Core.Context.Features.CompartmentsConfiguration
        Vonk.Core.Context.Features.VonkContextFeaturesExtensions
        Vonk.Core.Context.Format.FormatConfiguration
        Vonk.Core.Context.Format.FormatConformance
        Vonk.Core.Context.Format.Formatter
        Vonk.Core.Context.Guards.DefaultShapesConfiguration
        Vonk.Core.Context.Guards.DefaultShapesService
        Vonk.Core.Context.Guards.SizeLimits
        Vonk.Core.Context.Guards.SizeLimitsConfiguration
        Vonk.Core.Context.Guards.SizeLimitsMiddleware
        Vonk.Core.Context.Guards.SupportedInteractionConfiguration
        Vonk.Core.Context.Guards.SupportedInteractionsService
        Vonk.Core.Context.Http.EndpointMapping
        Vonk.Core.Context.Http.HttpToVonkConfiguration
        Vonk.Core.Context.Http.InformationModelEndpointConfiguration
        Vonk.Core.Context.Http.InformationModelMappingMode
        Vonk.Core.Context.Http.InformationModelOptions
        Vonk.Core.Context.Http.VonkExceptionMiddleware
        Vonk.Core.Context.Http.VonkHttpRequest
        Vonk.Core.Context.Http.VonkToHttpConfiguration
        Vonk.Core.Context.Http.VonkToHttpMiddleware
        Vonk.Core.Context.Internal.VonkInternalArguments
        Vonk.Core.Context.Internal.VonkResourceContext
        Vonk.Core.Context.Internal.VonkResourceRequest
        Vonk.Core.Context.Internal.VonkUrlArguments
        Vonk.Core.Context.IVonkResponseFeatureExtensions
        Vonk.Core.Context.OutputPreference.Prefer
        Vonk.Core.Context.OutputPreference.PreferService
        Vonk.Core.Context.OutputPreference.SupportedPreferHeaders
        Vonk.Core.Context.UrlMapping.UriPatchFactory
        Vonk.Core.Context.UrlMapping.UrlMappingConfiguration
        Vonk.Core.Context.UrlMapping.UrlMappingService
        Vonk.Core.Context.VonkBaseArguments
        Vonk.Core.Context.VonkBaseRequest
        Vonk.Core.Context.VonkHttpArguments
        Vonk.Core.Context.VonkResponse
        Vonk.Core.Import.ArtifactReadService
        Vonk.Core.Import.FhirRestEndpoint
        Vonk.Core.Import.FhirRestReader
        Vonk.Core.Import.IArtifactReader
        Vonk.Core.Import.IArtifactReaderFactory
        Vonk.Core.Import.ImportSource
        Vonk.Core.Import.ReadResult
        Vonk.Core.Import.ReadResult.ResultState
        Vonk.Core.Import.SourceSupportAttribute
        Vonk.Core.Infra.LivenessCheckConfiguration
        Vonk.Core.Infra.LongRunning.LongRunningConfiguration
        Vonk.Core.Infra.Maintenance.IMaintenanceJob
        Vonk.Core.Infra.Maintenance.MaintenanceConfiguration
        Vonk.Core.Infra.ReadinessCheckConfiguration
        Vonk.Core.Infra.ResponseCache.CapabilityCache
        Vonk.Core.Infra.ResponseCache.CapabilityCacheConfiguration
        Vonk.Core.Infra.ResponseCache.CapabilityCacheExtensions
        Vonk.Core.Infra.ResponseCache.CapabilityCacheOptions
        Vonk.Core.Infra.ResponseCache.CapabilityCacheServicesExtensions
        Vonk.Core.Licensing.LicenseConfiguration
        Vonk.Core.Licensing.LicenseOptions
        Vonk.Core.Licensing.LicenseService
        Vonk.Core.Metadata.CapabilityStatementBuilder
        Vonk.Core.Metadata.CompartmentInfo
        Vonk.Core.Metadata.CompartmentReference
        Vonk.Core.Metadata.CompartmentService
        Vonk.Core.Metadata.MetadataCache
        Vonk.Core.Metadata.MetadataConfiguration
        Vonk.Core.Metadata.ModelService
        Vonk.Core.Metadata.ModelServiceConformance
        Vonk.Core.Model.CommonExtensions
        Vonk.Core.Model.Compartment
        Vonk.Core.Operations.Capability.CapabilityConfiguration
        Vonk.Core.Operations.Capability.ConformanceService
        Vonk.Core.Operations.Capability.VonkCoreConformance
        Vonk.Core.Operations.Common.IPagingSource
        Vonk.Core.Operations.Common.PagingService
        Vonk.Core.Operations.Common.ResourceResolutionException
        Vonk.Core.Operations.ConditionalCrud.ConditionalCreateConfiguration
        Vonk.Core.Operations.ConditionalCrud.ConditionalCreateConformance
        Vonk.Core.Operations.ConditionalCrud.ConditionalCreateService
        Vonk.Core.Operations.ConditionalCrud.ConditionalCrudConfiguration
        Vonk.Core.Operations.ConditionalCrud.ConditionalDeleteConfiguration
        Vonk.Core.Operations.ConditionalCrud.ConditionalDeleteConformance
        Vonk.Core.Operations.ConditionalCrud.ConditionalDeleteService
        Vonk.Core.Operations.ConditionalCrud.ConditionalUpdateConfiguration
        Vonk.Core.Operations.ConditionalCrud.ConditionalUpdateConformance
        Vonk.Core.Operations.ConditionalCrud.ConditionalUpdateService
        Vonk.Core.Operations.ConditionalDeleteOptions
        Vonk.Core.Operations.ConditionalDeleteType
        Vonk.Core.Operations.Crud.CreateConfiguration
        Vonk.Core.Operations.Crud.CreateConformance
        Vonk.Core.Operations.Crud.CreateService
        Vonk.Core.Operations.Crud.DeleteConfiguration
        Vonk.Core.Operations.Crud.DeleteConformance
        Vonk.Core.Operations.Crud.DeleteService
        Vonk.Core.Operations.Crud.DeleteValidationService
        Vonk.Core.Operations.Crud.FhirPatchConfiguration
        Vonk.Core.Operations.Crud.PatchConformance
        Vonk.Core.Operations.Crud.ReadConfiguration
        Vonk.Core.Operations.Crud.ReadConformance
        Vonk.Core.Operations.Crud.ReadService
        Vonk.Core.Operations.Crud.UpdateConfiguration
        Vonk.Core.Operations.Crud.UpdateConformance
        Vonk.Core.Operations.Crud.UpdateService
        Vonk.Core.Operations.Crud.UpdateServiceBase
        Vonk.Core.Operations.FhirCapabilities
        Vonk.Core.Operations.FhirSearchOptions
        Vonk.Core.Operations.History.HistoryConfiguration
        Vonk.Core.Operations.History.HistoryConformance
        Vonk.Core.Operations.History.HistoryOptions
        Vonk.Core.Operations.History.HistoryService
        Vonk.Core.Operations.History.VersionReadConfiguration
        Vonk.Core.Operations.MetaOperation.MetaAddConfiguration
        Vonk.Core.Operations.MetaOperation.MetaAddService
        Vonk.Core.Operations.MetaOperation.MetaConfiguration
        Vonk.Core.Operations.MetaOperation.MetaDeleteConfiguration
        Vonk.Core.Operations.MetaOperation.MetaDeleteService
        Vonk.Core.Operations.MetaOperation.MetaService
        Vonk.Core.Operations.MetaOperation.MetaUtils
        Vonk.Core.Operations.Provenance.ProvenanceHeaderConfiguration
        Vonk.Core.Operations.Search.IncludeConfiguration
        Vonk.Core.Operations.Search.IncludeService
        Vonk.Core.Operations.Search.SearchConfiguration
        Vonk.Core.Operations.Search.SearchConformance
        Vonk.Core.Operations.Search.SearchService
        Vonk.Core.Operations.SnapshotGeneration.ISnapshotGenerator
        Vonk.Core.Operations.SnapshotGeneration.SnapshotGenerationConfiguration
        Vonk.Core.Operations.SnapshotGeneration.SnapshotGenerationConformance
        Vonk.Core.Operations.SnapshotGeneration.SnapshotGenerationService
        Vonk.Core.Operations.Transaction.BatchConformance
        Vonk.Core.Operations.Transaction.BatchMiddleware
        Vonk.Core.Operations.Transaction.BatchService
        Vonk.Core.Operations.Transaction.FhirBatchConfiguration
        Vonk.Core.Operations.Transaction.FhirTransactionConfiguration
        Vonk.Core.Operations.Transaction.FhirTransactionConformance
        Vonk.Core.Operations.Transaction.FhirTransactionMiddleware
        Vonk.Core.Operations.Transaction.FhirTransactionService
        Vonk.Core.Operations.Transaction.ReferenceResolver
        Vonk.Core.Operations.Validation.InstanceValidationConfiguration
        Vonk.Core.Operations.Validation.InstanceValidationService
        Vonk.Core.Operations.Validation.PrevalidationConfiguration
        Vonk.Core.Operations.Validation.ProfileFilterConfiguration
        Vonk.Core.Operations.Validation.ProfileFilterService
        Vonk.Core.Operations.Validation.StructuralValidationConfiguration
        Vonk.Core.Operations.Validation.ValidationConfiguration
        Vonk.Core.Operations.Validation.ValidationConformance
        Vonk.Core.Operations.Validation.ValidationOptions
        Vonk.Core.Operations.Validation.ValidationOptions.ValidationLevel
        Vonk.Core.Operations.Validation.ValidationService
        Vonk.Core.Operations.VersionsOperation.SupportedFhirVersionsDTO
        Vonk.Core.Operations.VersionsOperation.VersionsOperationConfiguration
        Vonk.Core.Operations.VonkImplementationConformance
        Vonk.Core.Operations.VonkServerConformance
        Vonk.Core.Pluggability.BaseModelBuilder
        Vonk.Core.Pluggability.IModelBuilder
        Vonk.Core.Pluggability.IModelBuilderExtensions
        Vonk.Core.Pluggability.IRepositoryConformanceSource
        Vonk.Core.Pluggability.ModelContributors.CompartmentDefinitionConverter
        Vonk.Core.Pluggability.ModelContributors.ContributorChanged
        Vonk.Core.Pluggability.ModelContributors.IInformationModelContributor
        Vonk.Core.Pluggability.ModelContributors.IModelContributor
        Vonk.Core.Pluggability.ModelContributors.IObservableModelContributor
        Vonk.Core.Pluggability.ModelContributors.ModelContributorsConfiguration
        Vonk.Core.Pluggability.ModelServiceCollectionExtensions
        Vonk.Core.Pluggability.OperationType
        Vonk.Core.Pluggability.PipelineBranch
        Vonk.Core.Pluggability.PipelineOptions
        Vonk.Core.Pluggability.PluggabilityConfiguration
        Vonk.Core.Pluggability.SupportedModelConfigurationService
        Vonk.Core.Pluggability.SupportedModelOptions
        Vonk.Core.Pluggability.VonkConfigurer
        Vonk.Core.Pluggability.VonkConfigurerConfiguration
        Vonk.Core.Pluggability.VonkInteractionAsyncMiddleware<TService>
        Vonk.Core.Pluggability.VonkInteractionMiddleware<TService>
        Vonk.Core.Pluggability.VonkInteractionMiddlewareExtensions
        Vonk.Core.Quartz.QuartzConfiguration
        Vonk.Core.Quartz.QuartzJobFactory
        Vonk.Core.Quartz.QuartzServicesUtilities
        Vonk.Core.Repository.ComponentFilterFactory
        Vonk.Core.Repository.EntryComponent
        Vonk.Core.Repository.EntryIndexerContext
        Vonk.Core.Repository.Generic.GenericEntryBuilder<B, E>
        Vonk.Core.Repository.Generic.GenericEntryFactory<E>
        Vonk.Core.Repository.Generic.GenericEntryIndexerContext<B, E>
        Vonk.Core.Repository.Generic.IGenericEntry
        Vonk.Core.Repository.HistoryEntry
        Vonk.Core.Repository.HistoryEntryExtensions
        Vonk.Core.Repository.HistoryResult
        Vonk.Core.Repository.IAdministrationChangeRepository
        Vonk.Core.Repository.IDateTimeComponent
        Vonk.Core.Repository.IEntryComponent
        Vonk.Core.Repository.IEntryQuery<T>
        Vonk.Core.Repository.IIndexBatchProcessor
        Vonk.Core.Repository.INumberComponent
        Vonk.Core.Repository.IQuantityComponent
        Vonk.Core.Repository.IReferenceComponent
        Vonk.Core.Repository.IReplaceRepository
        Vonk.Core.Repository.IResetRepository
        Vonk.Core.Repository.IStringComponent
        Vonk.Core.Repository.ITokenComponent
        Vonk.Core.Repository.IUriComponent
        Vonk.Core.Repository.Memory.CanonicalComponent
        Vonk.Core.Repository.Memory.CompartmentComponent
        Vonk.Core.Repository.Memory.DateTimeComponent
        Vonk.Core.Repository.Memory.MemoryEntry
        Vonk.Core.Repository.Memory.MemoryEntryBuilder
        Vonk.Core.Repository.Memory.MemoryEntryExtensions
        Vonk.Core.Repository.Memory.MemoryEntryFactory
        Vonk.Core.Repository.Memory.MemoryEntryIndexerContext
        Vonk.Core.Repository.Memory.MemoryIndexingBatch
        Vonk.Core.Repository.Memory.MemoryQuery
        Vonk.Core.Repository.Memory.MemoryQueryFactory
        Vonk.Core.Repository.Memory.NumberComponent
        Vonk.Core.Repository.Memory.QuantityComponent
        Vonk.Core.Repository.Memory.ReferenceComponent
        Vonk.Core.Repository.Memory.StringComponent
        Vonk.Core.Repository.Memory.TokenComponent
        Vonk.Core.Repository.Memory.UriComponent
        Vonk.Core.Repository.QueryBuilderConformance
        Vonk.Core.Repository.RepositoryIndexSupportConfiguration
        Vonk.Core.Repository.RepositorySearchSupportConfiguration
        Vonk.Core.Security.AuthorizationConfiguration
        Vonk.Core.Security.AuthorizationExceptionMiddleware
        Vonk.Core.Security.WriteAuthorizer
        Vonk.Core.Serialization.ParsingOptions
        Vonk.Core.Serialization.SerializationConfiguration
        Vonk.Core.Serialization.SerializationService
        Vonk.Core.Support.AttributeSupportExtensions
        Vonk.Core.Support.BundleHelpers
        Vonk.Core.Support.CachedDictionary<K, V>
        Vonk.Core.Support.Configuration.ConfigurationExtensions
        Vonk.Core.Support.EnumWrapper<TWrapperEnum, TWrappedEnum>
        Vonk.Core.Support.Fail<T>
        Vonk.Core.Support.HttpContextExtensions
        Vonk.Core.Support.IApplicationBuilderExtensions
        Vonk.Core.Support.IoAccessWrapper
        Vonk.Core.Support.IServiceScopeExtensions
        Vonk.Core.Support.LinqKitExtensions
        Vonk.Core.Support.ListWrapper<TItemInterface, TItemWrapper, TWrappedItem>
        Vonk.Core.Support.Ok<T>
        Vonk.Core.Support.QuantityExtensions
        Vonk.Core.Support.Result
        Vonk.Core.Support.Result<T>
        Vonk.Core.Support.TypedElementExtensions
        Vonk.Core.Support.UriExtensions
        Vonk.Core.Support.VonkSearchParameterEqualityComparer
        Vonk.Core.Support.Wrapper<T>
        Vonk.Fhir.Operations.Validation.ValidationClient

.. container:: toggle

    .. container:: header
    
        List of methods/properties removed from the public API

    .. code ::

        static IResource IResourceExtensions.Cache(this IResource original, String name, Object toCache, Type cacheAsType)
        static IResource IResourceExtensions.Cache(this IResource original, Object toCache)
        static IResource IResourceExtensions.Cache<T>(this IResource original, T toCache)
        static IResource IResourceExtensions.Cache(this IResource original, String name, Object toCache)
        static IResource IResourceExtensions.Cache<T>(this IResource original, String name, T toCache)
        static IEnumerable<Object> IResourceExtensions.GetCached(this IResource from, Type cachedAsType = null, String name = null)
        static IEnumerable<T> IResourceExtensions.GetCached<T>(this IResource from, String name = null)
        static Boolean IResourceExtensions.TryGetCached<T>(this IResource from, out T result)
        static Boolean IResourceExtensions.TryGetCached<T>(this IResource from, String name, out T result)
        static IEnumerable<Object> IResourceExtensions.GetCached(this IResource from, String name)
        static OperationOutcome IVonkOutcomeExtensions.ToOperationOutcome(this VonkOutcome vonkOutcome, IStructureDefinitionSummaryProvider schemaProvider)
        static VonkOutcome IVonkOutcomeExtensions.ToVonkOutcome(this OperationOutcome operationOutcome)
        static void IVonkOutcomeExtensions.AddIssue(this VonkOutcome vonkOutcome, IssueComponent issueComponent)
        static void QueryableExtensions.RunInBatches<T>(this IQueryable<T> collection, Int32 batchSize, Action<IEnumerable<T>> action)
        static Task QueryableExtensions.RunInBatchesAsync<T>(this IQueryable<T> collection, Int32 batchSize, Func<IEnumerable<T>, Task> action)
        SpecificationZipLocator.SpecificationZipLocator(IHostingEnvironment hostingEnv, ILogger<SpecificationZipLocator> logger)
        static Boolean StringExtensions.TrySplitCanonical(this String reference, out String uri, out String version)

        static VonkSearchParameter IModelServiceExtensions.FindSearchParameterByName(this IModelService modelService, String parameterName, String resourceTypeName)
            signature changed to static VonkSearchParameter FindSearchParameterByCode(this IModelService modelService, string parameterCode, string resourceTypeName)
        static IEnumerable<VonkSearchParameter> IModelServiceExtensions.FindSearchParametersByName(this IModelService modelService, String parameterName, params String[] resourceTypeNames)
            signature changed to static IEnumerable<VonkSearchParameter> IModelServiceExtensions.FindSearchParametersByCode(this IModelService modelService, String parameterCode, params String[] resourceTypeNames)
        String VonkSearchParameter.Name.get
            signature changed to String VonkSearchParameter.Code.get
        void VonkSearchParameter.Name.set
            signature changed void VonkSearchParameter.Code.set
        String VonkSearchParameterComponent.ParameterName.get
            signature changed String VonkSearchParameterComponent.ParameterCode.get
        void VonkSearchParameterComponent.ParameterName.set
            signature changed void VonkSearchParameterComponent.ParameterCode.set
        Q IRepoQueryFactory<Q>.Filter(String parameterName, IFilterValue value)
            signature changed to Q IRepoQueryFactory<Q>.Filter(String parameterCode, IFilterValue value)
        IncludeShape.IncludeShape(String sourceType, String parameterName, String[] targetTypes, Boolean recurse = false)
            signature changed to IncludeShape.IncludeShape(String sourceType, String parameterCode, String[] targetTypes, Boolean recurse = false)
        RevIncludeShape.RevIncludeShape(String sourceType, String parameterName, String[] targetTypes, Boolean recurse = false)
            signature changed to RevIncludeShape.RevIncludeShape(String sourceType, String parameterName, String[] targetTypes, Boolean recurse = false)
        SortShape.SortShape(String parameterName, SearchParamType parameterType, SortDirection direction = SortDirection.ascending, Int32 priority = 1)
            signature changed to SortShape.SortShape(String parameterCode, SearchParamType parameterType, SortDirection direction = SortDirection.ascending, Int32 priority = 1)

Other
^^^^^

#. Vonk Loader has been deprecated.

.. note::
    With the release of Firely Server 5.0, we will officially stop support for Firely Server v3.x. We will continue supporting customers that run Firely Server v4.x instances.

.. _vonk_releasenotes_5_0_0-beta1:

Release 5.0.0-beta1, January 19th, 2023
---------------------------------------
.. attention::
    This is a beta release of Firely Server 5.0.0. Although the core functionality remains fully intact, parts of the public API have been removed or heavily modified. Please consult the list under section 'Plugin and Facade' and check whether your implementation is affected by these changes.

Configuration
^^^^^^^^^^^^^
.. attention::
    Parts of the configuration were overhauled, starting with FS 5.0.0-beta1. 
    If you have adjusted the :ref:`appsettings<configure_appsettings>` either in ``appsettings.instance.json`` or in environment variables, 
    make sure to to update your configuration accordingly. Please follow the bullets below.

#. The configuration section for additional endpoints in the discovery document and additional issuers in tokens has been reworked. Consult the :ref:`SMART Configuration section<feature_accesscontrol_config>` for more details.
#. Add this new namespace to the root (``/``) path of the :ref:`PipelineOptions<settings_pipeline>`: ``Vonk.Plugin.Operations``. The result should look like this:

    .. code-block::
        :emphasize-lines: 8

        "PipelineOptions": {
            "PluginDirectory": "./plugins",
            "Branches": [
            {
                "Path": "/",
                "Include": [
                    "Vonk.Core",
                    "Vonk.Plugin.Operations",
                    "Vonk.Fhir.R3",
                    "Vonk.Fhir.R4",
                    //etc.
                ]
            },
            {
                "Path": "/administration",
                "Include": [
                    "Vonk.Core",
                    //etc.
                ]
            }
            ]
        }


Database
^^^^^^^^

#. Because of feature 6 below, searching on version-specific references, the database was updated for both **SQL Server** and **MongoDB**. Firely Server will usually perform the upgrade automatically. For details, see :ref:`migrations`.

   #. SQL Server is upgraded from schema 25 to **26**. The upgrade script file is named ``/sqlserver/FS_SchemaUpgrade_Data_v25_v26.sql``.
   #. MongoDB is upgraded from schema 24 to **25**. The upgrade script file is named ``/mongodb/FS_SchemaUpgrade_Data_v24_v25``.
   #. The administration database is not affected by this change, so you don't need to upgrade that.

#. The database upgrade means that you also need an upgraded version of Firely Server Ingest, :ref:`version 2.0<fsi_releasenotes_2.0.0>`

Feature
^^^^^^^

#. The initial public version of Firely Auth has been released. Firely Auth is an optimized OAuth2 provider that understands SMART on FHIR scopes and the FHIR resource types they apply to out of the box. See :ref:`firely_auth_index` for more information.
#. The default information model for Firely Server is now R4.
#. Bulk Data Export now supports SMART on FHIR v2.
#. Our :ref:`SMART on FHIR documentation <feature_accesscontrol>` has been updated for SMART on FHIR v2.
#. Contents of AuditEvents can now be modified via a plugin. See :ref:`AuditEvent customization <audit_event_customization>` for further info.
#. Firely Server now supports searching on version-specific references. Consult the `FHIR specification <https://www.hl7.org/fhir/search.html#versions>`_ for more information.
#. Firely Server now generates FHIR AuditEvent resources conforming to `IHE Basic Audit Log Patterns <https://profiles.ihe.net/ITI/BALP/index.html>`_. Fields that are included in the audit event log and AuditEvent resources now contain the same content.

Fix
^^^

#. When performing a Bulk Data Export request with a Firely Server instance running on a SQL database, it will return the Group resource even if it has no members. 
#. FS now declares support for Bulk Data Export Group export operations in its CapabilityStatement. This features was available before, but missing from FS's CapabilityStatement. 
#. Bulk Data Export now returns a succesful status code (``202``) instead of an erroneous status code if no resources were matched for an export. The resulting export will include an empty array as described in the `specification <https://hl7.org/fhir/uv/bulkdata/export/index.html#response---complete-status>`_.
#. Upon commencing a Bulk Data Export, Firely Server now correctly handles ``Prefer`` headers as outlined `in the specification <https://hl7.org/fhir/uv/bulkdata/export/index.html#headers>`_.
#. ``Device`` can now be added as an additional resource in a Bulk Data export.
#. Search parameters without a value are now ignored by the server instead of resulting in an error response.
#. Firely Server now creates valid FHIR R5 AuditEvents.
#. Searching for a resource with multiple sort fields does not throw an exception anymore when Firely Server runs on a SQL database.
#. When using the ``If-Modified-Since`` header, only resources that were modified after the specified timestamp are returned. Because of a precision mismatch (seconds vs. milliseconds), wrong resources were sometimes returned before this fix.
#. When updating a deleted resource conditionally, Firely Server does not throw an exception anymore.
#. Firely Server now returns the correct issue code (``business-rule`` instead of ``invalid``) in the OperationOutcome when performing a conditional update using ``_id`` as a parameter. Additionally, the error message has been improved when a resource in a different information model is matched via the ``id`` field.
#. When executing a ``POST``-based search, Firely Server will now return the correct self-link as seen in ``GET``-based searches.
#. The client id of the default SMART authorization options have been changed from ``vonk`` to ``firelyserver``.
#. Firely Server now returns improved error messages if the client is not allowed to perform searches.
#. Support for Firely Server using a SQLite database on arm64-based Macs was improved. 
#. During SMART on FHIR v2 discovery, Firely Server now returns the ``grant_types_supported`` field.
#. Firely Server now returns the correct CodeSystem ``http://terminology.hl7.org/CodeSystem/restful-security-service`` within the security section of its ``CapabilityStatement``. Before this change, the old R3 CodeSystem ``http://hl7.org/fhir/restful-security-service`` was falsely returned.
#. Firely Server will now handle duplicate DLLs and assemblies more gracefully in case they were accidentally added to its plugin directory.
#. When overwriting Search Parameters, the new Search Parameters will now be included in the CapabilityStatement instead of the overwritten ones. This feature was introduced with Firely Server ``4.7.0`` but broke in between the last releases.

Plugin and Facade
^^^^^^^^^^^^^^^^^

#. Firely Server now uses the `Firely .NET SDK 4.3.0 <https://github.com/FirelyTeam/firely-net-sdk/releases/tag/v4.3.0-stu3>`_. Follow the link for an overview of all changes.

.. warning::
    Due to the above namespace change, all of your plugins need to be recompiled against this FS release.

#. Please note that the ``Vonk.Smart`` package will not be published on NuGet anymore.

Below modules of the public API are deprecated and no longer available to Facade developers. Please consult chapter :ref:`vonk_reference` for a full overview of the public API.

#. ``Simplifier.Licensing``
#. ``Vonk.Core.Common.IGenericResourceResolver``
#. ``Vonk.Core.Common.ResourceWithCache.ResourceExtensions``
#. ``Vonk.Core.Configuration.ConfigurationLogger``
#. ``Vonk.Core.Conformance.ConformanceConfiguration``
#. ``Vonk.Core.Conformance.IConformanceCache``
#. ``Vonk.Core.Conformance.IConformanceCacheInvalidation``
#. ``Vonk.Core.Context.Elements``
#. ``Vonk.Core.Context.Features.CompartmentFeatureMiddleware``
#. ``Vonk.Core.Context.Features.VonkContextFeaturesExtensions``
#. ``Vonk.Core.Context.Format``
#. ``Vonk.Core.Context.Http``
#. ``Vonk.Core.Context.Internal``
#. ``Vonk.Core.Context.OutputPreference``
#. ``Vonk.Core.Context.ContextConfiguration``
#. ``Vonk.Core.Context.VonkBaseArguments``
#. ``Vonk.Core.Context.VonkBaseRequest``
#. ``Vonk.Core.Context.VonkResponse``
#. ``Vonk.Core.Import``
#. ``Vonk.Core.Infra.LongRunning.LongRunningTaskConfiguration``
#. ``Vonk.Core.Infra.Maintenance.IMaintenanceJob``
#. ``Vonk.Core.Infra.Maintenance.MaintenanceConfiguration``
#. ``Vonk.Core.Infra.ResponseCache.CapabilityCache``
#. ``Vonk.Core.Infra.ResponseCache.CapabilityCacheConfiguration``
#. ``Vonk.Core.Licensing.LicenseConfiguration``
#. ``Vonk.Core.Licensing.LicenseOptions``
#. ``Vonk.Core.Licensing.LicenseService``
#. ``Vonk.Core.Metadata.CapabilityStatementBuilder``
#. ``Vonk.Core.Metadata.CompartmentInfo``
#. ``Vonk.Core.Metadata.CompartmentReference``
#. ``Vonk.Core.Metadata.IArgumentValidationService``
#. ``Vonk.Core.Metadata.MetadataCache``
#. ``Vonk.Core.Metadata.MetaDataConfiguration``
#. ``Vonk.Core.Metadata.ModelService``
#. ``Vonk.Core.Metadata.ModelServiceConformance``
#. ``Vonk.Core.Model.CommonExtensions``
#. ``Vonk.Core.Model.Compartment``
#. ``Vonk.Core.Operations.*``
#. ``Vonk.Core.Operations.PagingService``
#. ``Vonk.Core.Operations.IPagingService``
#. ``Vonk.Core.Pluggability.ModelContributors``
#. ``Vonk.Core.Pluggability.ModelContributors.IModelContributor``
#. ``Vonk.Core.Pluggability.IModelBuilder``
#. ``Vonk.Core.Quartz.QuartzServiceUtilities``
#. ``Vonk.Core.Repository.IAdministrationChangeRepository``
#. ``Vonk.Core.Repository.IReplaceRepository``
#. ``Vonk.Core.Repository.IResetRepository``
#. ``Vonk.Core.Repository.HistoryEntry``
#. ``Vonk.Core.Repository.HistoryResult``
#. ``Vonk.Core.Serialization.ParsingOptions``
#. ``Vonk.Core.Serialization.SerializationConfiguration``
#. ``Vonk.Core.Serialization.SerializationService``
#. ``Vonk.Core.Support.AttributeSupportExtensions``
#. ``Vonk.Core.Support.BundleHelpers``
#. ``Vonk.Core.Support.BundleResolver``
#. ``Vonk.Core.Support.CachedDictionary``
#. ``Vonk.Core.Support.ConfigurationExtensions``
#. ``Vonk.Core.Support.HttpContextExtensions``
#. ``Vonk.Core.Support.IApplicationBuilderExtensions``
#. ``Vonk.Core.Support.IOAccessWrapper``
#. ``Vonk.Core.Support.IServiceScopeExtensions``
#. ``Vonk.Core.Support.LinqKitExtensions`` (Moved to ``Vonk.Facade.Relational``)
#. ``Vonk.Core.Support.QuantityExtensions``
#. ``Vonk.Core.Support.Result<T>``
#. ``Vonk.Core.Support.VonkSearchParameterEqualityComparer``
#. ``Vonk.Core.Support.TypedElementExtensions``
#. ``Vonk.Core.Support.Wrapper``
#. ``Vonk.Core.Support.EnumWrapper``
#. ``Vonk.Fhir.R3.Configuration.*``
#. ``Vonk.Fhir.R3.Import.*``
#. ``Vonk.Fhir.R3.Metadata.ICapabilityResourceProviderR3``
#. ``Vonk.Fhir.R3.Model.Capability.SystemRestfulInteractionComponentR3``
#. ``Vonk.Fhir.R3.Model.Capability.TypeRestfulInteractionComponentR3``
#. ``Vonk.Fhir.R3.Validation.ValidationConfigurationR3``
#. ``Vonk.Fhir.R3.FhirClientWithBasicAuthentication``
#. ``Vonk.Fhir.R3.FhirContextModelContributor``
#. ``Vonk.Fhir.R3.IConformanceCacheR3``
#. ``Vonk.Fhir.R3.ConformanceCacheR3``
#. ``Vonk.Fhir.R3.MetadataCacheR3``
#. ``Vonk.Fhir.R3.QuantityExtensions``
#. ``Vonk.Fhir.R4.Configuration.*``
#. ``Vonk.Fhir.R4.Import.*``
#. ``Vonk.Fhir.R4.Metadata.ICapabilityResourceProviderR4``
#. ``Vonk.Fhir.R4.Model.Capability.SystemRestfulInteractionComponentR4``
#. ``Vonk.Fhir.R4.Model.Capability.TypeRestfulInteractionComponentR4``
#. ``Vonk.Fhir.R4.Validation.ValidationConfigurationR4``
#. ``Vonk.Fhir.R4.FhirClientWithBasicAuthentication``
#. ``Vonk.Fhir.R4.FhirContextModelContributor``
#. ``Vonk.Fhir.R4.IConformanceCacheR4``
#. ``Vonk.Fhir.R4.ConformanceCacheR4``
#. ``Vonk.Fhir.R4.MetadataCacheR4``
#. ``Vonk.Fhir.R4.QuantityExtensions``
#. ``Vonk.Fhir.R5.Configuration.*``
#. ``Vonk.Fhir.R5.Import.*``
#. ``Vonk.Fhir.R5.Metadata.ICapabilityResourceProviderR5``
#. ``Vonk.Fhir.R5.Model.Capability.SystemRestfulInteractionComponentR5``
#. ``Vonk.Fhir.R5.Model.Capability.TypeRestfulInteractionComponentR5``
#. ``Vonk.Fhir.R5.Validation.ValidationConfigurationR5``
#. ``Vonk.Fhir.R5.FhirClientWithBasicAuthentication``
#. ``Vonk.Fhir.R5.FhirContextModelContributor``
#. ``Vonk.Fhir.R5.IConformanceCacheR5``
#. ``Vonk.Fhir.R5.ConformanceCacheR5``
#. ``Vonk.Fhir.R5.MetadataCacheR5``
#. ``Vonk.Fhir.R5.QuantityExtensions``

Other
^^^^^

#. Vonk Loader has been deprecated.