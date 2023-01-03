.. _vonk_releasenotes_history_v5:

Current Firely Server release notes (v5.x)
==========================================

.. _vonk_releasenotes_5_0_0:

Release 5.0.0-beta1, TBD, 2023
------------------------------
.. attention::
    This is a beta release of Firely Server 5.0.0. Although the core functionality remains fully intact, parts of the public API have been removed or heavily modified. Please consult the list under section 'Plugin and Facade' and check whether your implementation is affected by these changes.

Feature
^^^^^^^

#. The default information model for Firely Server is now R4.
#. Firely Server is now certified according to the `ONC Certification (g)(10) Standardized API <https://inferno.healthit.gov/suites/test_sessions/10cad0a0-1d6e-4648-b2c8-70cefbf260c5>`_.
#. Bulk Data Export now supports SMART on FHIR v2.
#. Contents of AuditEvents can now be modified via a plugin. See :ref:`AuditEvent customization <audit_event_customization>` for further info.

Fix
^^^

#. Bulk Data Export now returns a succesful status code (``202``) instead of an erroneous status code if no resources were matched for an export. The resulting export will include an empty array as described in the `specification <https://hl7.org/fhir/uv/bulkdata/export/index.html#response---complete-status>`_.
#. Empty search parameters are now ignored by the server instead of resulting in an error response.
#. Firely Server now creates valid R5 AuditEvents.
#. Firely Server now supports searching on version-specific references. Consult the `FHIR specification <https://www.hl7.org/fhir/search.html#versions>`_ for more information.
#. Searching for a resource with multiple sort fields does not throw an exception anymore.
#. Fields that are included in the audit event log and AuditEvent resources now contain the same content.
#. When using the ``If-Modified-Since`` header, only resources that were modified after the specified timestamp are returned. Because of a precision mismatch (seconds vs. milliseconds), wrong resources were sometimes returned before this fix.
#. When updating a deleted resource conditionally, Firely Server does not throw an exception anymore.
#. Firely Server now returns the correct issue code when performing a conditional update using ``_id`` as a parameter. Additionally, the error message has been improved when a resource in a different information model is matched via the ``id`` field.
#. When executing a ``POST``-based search, Firely Server will now return the correct self-link as seen in ``GET``-based searches.
#. Upon commencing a Bulk Data Export, Firely Server now correctly handles ``Prefer`` headers as outlined `in the specification <https://hl7.org/fhir/uv/bulkdata/export/index.html#headers>`_.
#. ``Device`` can now be added as an additional resource in a Bulk Data export.
#. The client id of the default SMART authorization options have been changed from ``vonk`` to ``firelyserver``.
#. Firely Server now returns improved error messages if the client is not allowed to perform searches.
#. Support for Firely Server using a SQLite database on M1 Macs was improved. 
#. During SMART on FHIR v2 discovery, Firely Server now returns the ``grant_types_supported`` field.
#. Firely Server now returns the correct CodeSystem ``http://terminology.hl7.org/CodeSystem/restful-security-service`` within the security section of its ``CapabilityStatement``. Before this change, the old R3 CodeSystem ``http://hl7.org/fhir/restful-security-service`` was falsely returned.
#. When performing a Bulk Data Export request with a Firely Server instance running on a SQL database, it will return the Group resource even if it has no members. 
#. Firely Server will now handle duplicate DLLs and assemblies more gracefully in case they were accidently added to its plugin directory.

Configuration
^^^^^^^^^^^^^
#. The configuration section for additional endpoints in the discovery document and additional issuers in tokens has been reworked. Consult the :ref:`SMART Configuration section<feature_accesscontrol_config>` for more details.

Plugin and Facade
^^^^^^^^^^^^^^^^^

#. Firely Server now uses the `Firely .NET SDK 4.3.0 <https://github.com/FirelyTeam/firely-net-sdk/releases/tag/v4.3.0-stu3>`_. Follow the link for an overview of all changes.
#. Please note that the ``Vonk.Smart`` package will not be published on NuGet anymore.

The following modules of the public API have either been deprecated, are no longer part of the public API or have been altered in some other way: 

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