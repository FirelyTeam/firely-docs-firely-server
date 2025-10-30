.. _vonk_releasenotes_history_v6:

Current Firely Server release notes (v6.x)
==========================================

.. note::
    For information on how to upgrade, please have a look at our documentation on :ref:`upgrade`. You can download the binaries of the latest version from `this site <https://downloads.fire.ly/firely-server/versions/>`_, or pull the latest docker image::
        
        docker pull firely/server:latest


.. _vonk_releasenotes_6_5_0:

Release 6.5.0, October 31st, 2025
---------------------------------

Improvements
^^^^^^^^^^^^

#. The behavior of the ``$purge`` operation has been adjusted with regard to Group resources. Purged Patient references are now removed without deleting the entire Group, as Groups may contain additional references to other Patient instances.
#. Firely Server MassTransit dependencies were updated to enhance SASL authentication with Kafka, improving message passing security.

Features
^^^^^^^^

#. It is now possible to configure the file retention period for Bulk Data Export task files. It specifies how long the exported files should be retained on the servr before they are automatically deleted. For more information see :ref:`feature_bulkdataexport_configuration`.
#. SSL configuration details are now supported for RabbitMQ in Firely Server PubSub. It enables configuring SSL settings to secure the connection between Firely Server and RabbitMQ. For more information see :ref:`pubsub_configuration_rabbitmq`.
#. To support quick and easy debugging, Serilog Log Level hot reloading capabilities can now be leverages. The log level of Serilog can now be changed in the logsettings at runtime without restarting Firely Server. For more information see :ref:`hot_reload_log_level`.
* Added support for indexing custom search parameters in FSI. See :ref:`custom_search_parameters` for more information.

Programming API changes and plugins
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

#. Firely Server was updated to use the Firely .NET SDK v6.0.1. For those implementing custom plugins or facades, we recommend updating these to use the .NET SDK v6.0.1 when upgrading to this version of Firely Server. Please check out the release notes `here <https://github.com/FirelyTeam/firely-net-sdk/releases/tag/v6.0.1>`_ for more information.
#. It is likely all custom plugins need to be recompiled against new version of `Vonk.Core` package due to SDK changes.

Fix
^^^

* Fixed an issue that resulted references not being resolved using the ``resolve()`` function in FHIRPath when validating constraints against resources wrap inside a Bundle.
* The default appsettings missed the ``EnforceAccessPolicies`` element in the ``SmartAuthorizationOptions`` section.
* ``$liveness`` and ``$readiness`` contained invalid values for the ``RequireTenant`` settings in their respective ``Operations`` configuration section.
* Fixed a FHIRPath-related issue when validating the ``ctm-1`` constraint against CarePlan resources.

Known behavioral changes
^^^^^^^^^^^^^^^^^^^^^^^^

#. You may encounter issues ingesting same resources if they contain elements unknown to the StructureDefinition. Previous versions of SDK would discard unknown elements, however, the new SDK will now report these as validation issues.

.. _vonk_releasenotes_6_4_0:

Release 6.4.0, August 26th, 2025
--------------------------------

Fixes
^^^^^

#. We improved the behavior of the validator for resolving references and applying validation in contained resources and bundle resources. FHIRPath constraints using resolve() statements will now evaluate correctly in these situations.

.. _vonk_releasenotes_6_3_1:

Release 6.3.1, August 11th, 2025
--------------------------------

Fixes
^^^^^

#. We updated dependencies of the Elasticsearch sink to fix a security vulnerability in a dependency of the `Elastic.Serilog.Sinks` package. The updated version is now 8.18.2. See the `Elastic Sink 8.18.2 release notes <https://github.com/elastic/ecs-dotnet/releases>`_ for more information.
#. We fixed a bug where FSI would take a long time to start up when the MongoDb target database would contain a large number of resources. This was caused by FSI trying to perform a count on the target database, which would take a long time when there were many resources.

.. _vonk_releasenotes_6_3_0:

Release 6.3.0, July 22th, 2025
------------------------------

Features
^^^^^^^^

#. We introduced the AdvisorRules setting for the validator for Firely Prior Authorization and Scale licenses. The implementation of the Advisor Rules system allows users to customize validation behaviour on a more granular level by setting filters with which the outcome of validation or the validation itself can be modified. Note that this feature is still in beta. For meore information see :ref:`feature_advisor_rules`.
#. The validator will now create extensions on validation errors pointing to the profile that caused the error in the http://hl7.org/fhir/StructureDefinition/operationoutcome-issue-source extension. These issues will also be annotated with line numbers in the http://hl7.org/fhir/StructureDefinition/operationoutcome-issue-col and http://hl7.org/fhir/StructureDefinition/operationoutcome-issue-line extension.

.. _vonk_releasenotes_6_2_0:

Release 6.2.0, July 15th, 2025
------------------------------

Improvements
^^^^^^^^^^^^

#. Updated Serilog ApplicationInsights sink configuration to use Connection String instead of the deprecated Instrumentation Key. Azure no longer supports Instrumentation Keys, so one should use ``connectionString`` in the ApplicationInsights sink configuration. The connection string can also be configured via ``ApplicationInsights:ConnectionString`` in appsettings.json. See :ref:`configure_log_insights` for more information.
#. Updated search anonymization to work across multiple Firely Server instances. This also changed the configuration, see: :ref:`restful_search_anonymization` on how to configure the search anonymization.
#. It is now possible for Firely Server to pick up appsettings.json files during startup by specifying the file location in the environment variable ``VONK_PATH_TO_SETTINGS``. See :ref:`configure_settings_path`. Before, the configuration was only loaded from appsettings.instance.json.
#. We improved the behavior of license checks upon startup so that users will no longer see warnings for unlicensed plugins that are not enabled in the pipeline.
#. We made some improvements to Firely Server Ingest (FSI):
    - We have improved the efficiency of FSI with regard to memory usage/CPU when generating the final usage statistics after a run. This could lead previously to excessive memory consumption and crashes.
    - FSI will now show a warning if it is unable to connect to a source database.

Fixes
^^^^^

#. Requests with a double slash (//) would lead to an uncaught exception. This will now lead to a ``501 Not Implemented`` response in case the double slash is used within the URL and to a ``404 Not Found`` response in case the double slash is at the end of the URL.
#. We made some fixes to the Vonk.Facade.Starter kit to help developers on their way with building a facade.
    - It is now possible to create Observation resources again.
    - ``_total=none`` is now handled properly. Before this would lead to an error when doing a search.

Features
^^^^^^^^

#. It is now possible to validate QuestionnaireResponse resources against their original Questionnaire resource. See :ref:`feature_advancedvalidation` for more information.
#. Message brokers can now be used as a target for Firely Server Ingest. FSI will publish messages to the message broker upon ingesting resources, which can then be consumed by Firely Server. Currently, only Azure Service Bus and RabbitMQ can be configured as message brokers for FSI. The use of a MongoDb source is not supported if the target is set to a message broker, only ingestion from files/folders is supported. See :ref:`fsi_target_pubsub` for more information.
#. We upgraded the .Net SDK to v5.12.0. See the `SDK 5.12.0 release notes <https://github.com/FirelyTeam/firely-net-sdk/releases/tag/v5.12.0>`_ for more information.


.. _vonk_releasenotes_6_1_0:

Release 6.1.0, May 23rd, 2025
-----------------------------

Security
^^^^^^^^

#. AccessPolicy resources can now only be accessed or modified with system-level scopes (e.g., ``system/AccessPolicy.*``). Patient-level scopes (``patient/AccessPolicy.*``) and user-level scopes (``user/AccessPolicy.*``) are not allowed and will be rejected with a 403 Forbidden response.
#. ``TrustedProxyIPNetworks`` now has an additional setting ``AllowAnyNetworkOrigins`` to allow any network origins to be trusted. Before, this configuration was only allowed if ``ASPNETCORE_ENVIRONMENT`` was set to ``Development``. Systems that used this environment variable to bypass the ip-range restrictions should switch to using this setting instead. This setting is disabled by default and should only be enabled if you are sure that your network is secure.
#. We added a check to the SMART on FHIR settings to ensure that ``Authority`` is always configured.
#. We added the ``ClockSkew`` setting to the ``SmartAuthorizationOptions``. This setting is used to adjust the expiration time and validity of JWT tokens. Before, you could only adjust the expiration time of a JWT token in FA, and Firely server would add an additional window of 5 minutes to this expiration time where the token would still be valid. This window can now be adjusted with this setting.  See :ref:`feature_accesscontrol_config` for more information.


Improvements and Fixes
^^^^^^^^^^^^^^^^^^^^^^

#. We improved the behavior of AuditEvent generation in combination with ``$member-match``. The AuditEvent will now capture the Patient ID and Identifier of the member after a successful match.
#. We improved the performance of snapshot generation queries for Bulk Data Export against a SQL back-end.
#. We fixed a bug for the Document Handling operation. Before, references of the posted document bundle could not always be resolved.
#. We improved error messaging of Firely Server for SMART on FHIR reference tokens. Operation Outcomes indicating errors with regard to the token would only mention JWT tokens when a reference token was used. As this was misleading, we adjusted the error message to dynamically show the type of token that was used. 
#. We fixed a bug in the handling of the ``above`` modifier in search queries. Firely Server does not support the ``above`` modifier and would show a large stack trace when this modifier was used in queries. Error handling for the use of this modifier is now improved.

Features
^^^^^^^^

#. We added support for the use of the Claim Check pattern in PubSub. This features allows you to outsource the payload of a message to an Azure Blob Storage Account that can be referenced in the message, leading to smaller messages and improved performance. See :ref:`pubsub_claimcheck` for more information.

=======

.. _vonk_releasenotes_6_0_0:

Release 6.0.0, April 15th, 2025
-------------------------------

Firely is proud to announce a new major version of Firely Server. This release represents a significant step forward in our commitment to providing a reliable, compliant, and easy to use FHIR server.
With this new version, we've focused on delivering:

- support for Sharding with MongoDB (see :ref:`configure_mongodb_sharding`)
- zero-downtime migrations with MongoDB (see :ref:`zero_downtime_migration`)
- detailed insights into Firely Server deployments based on OpenTelemetry metrics and traces (see :ref:`feature_opentelemetry`)
- improved integration into existing infrastructures with Kafka support for Firely Server PubSub  (see :ref:`pubsub_configuration`)
- out-of-the-box compliance with more HL7 DaVinci Implementation Guides, e.g. by providing support for the HRex $member-match operation (see :ref:`davinci_pdex_ig`)
- flexibility for deployments requiring multi-tenancy (see :ref:`feature_multitenancy`)

Please study the release notes carefully as they contain breaking changes to the behavior of Firely Server, as well as the configuration of the server. 
Our support team is happy to provide assistance in the upgrade and can be reached at `server@fire.ly <mailto:server@fire.ly>`_ or through the support desk.
Need hands-on support with your upgrade? Our expert consultants are here to help. Explore our `Upgrade Support Package <https://fire.ly/upgrade-support-package/>`_ to get started.

.. note::
    With the release of Firely Server 6.0, we will officially stop support for Firely Server v4.x. We will continue supporting customers that run Firely Server v5.x.

Security
^^^^^^^^

#. To avoid accidentally granting access to AccessPolicies, ``AccessPolicy`` resources cannot be accessed by a resource wildcard scope. E.g. ``system/*.*`` should be replaced with  - ``system/AccessPolicy.*`` to be able to access AccessPolicy resources.
#. The ``$lastN`` operation can now be used with in combination with permissions defined in an ``AccessPolicy`` resource.
#. Intreractions with system-level scopes where the token is bound to a fhirUser of type ``Device`` will be rejected if no matching ``AccessPolicy`` can be found.

Database
^^^^^^^^
#. Raised the minimum supported version of MongoDB to 6.0 to enable sharding.
#. Sharding is now natively supported by Firely Server when using MongoDB as the database backend (see :ref:`configure_mongodb_sharding`). Sharding improves the read/write performance of Firely Server. A new license token is required for this feature. Please contact us for an updated license.
#. Virtual multi-tenancy can now be enabled to logically separate stored resources in the database. The tenant identifier can be retrieved either from an HTTP header value or from a token claim (see :ref:`feature_multitenancy`).
#. Firely Server Ingest can now auto-provision the target database to facilitate zero-downtime migrations (see :ref:`zero_downtime_migration`). A new license token is required for this feature. Please contact us for an updated license.

.. attention::
    Firely Server requires a schema upgrade to version v28 of the database. This is only required for MongoDB database backends. The migration MUST be done using the zero-downtime migration process.

Features
^^^^^^^^

#. Firely Server now implements the ``$member-match`` operation to find members of a health plan based on demographic information. See :ref:`member-match` for more information.
#. Traces and ASP .NET metrics based on ``OpenTelemetry`` can now be exported to OTLP-enabled backends. See :ref:`feature_opentelemetry` for more information.
#. ``memberOf()`` expressions are now supported in FHIRPath constraints when validating resources.
#. Added support for validating MIME types (bcp:13) and language codes (bcp:47).
#. Firely Server has a new homepage featuring a refreshed and modern UI.
#. ``$realworldtesting`` can now be executed using a POST request.
#. It is now possible to disable the create-on-update feature with a new setting in the ``FhirCapabilities`` section of the app settings. See :ref:`fhir_capabilities` for more information.
#. With this release ``Update with no changes (No-Op)`` is enabled by default. For more information about the plugin see :ref:`restful_noop`.
#. The NoOp plugin now also works in combination with transaction bundles.
#. Added support for reading messages from a Kafka topic when using Firely Server PubSub.
#. We have updated the validator api that is used by Firely Server for improved validation.
#. Added support for JWT-based authentication against remote terminology services. See :ref:`feature_terminologyoptions` for more information.
#. Expose port option in PubSub for RabbitMQ. See :ref:`pubsub_configuration` for more information.
#. Performance counters are now exported via OpenTelemetry when ingesting data via Firely Server Ingest.
#. Enable use of AuditEvent output parameters (e.g. IP address) for regular logging.

.. attention::
    With the introduction of the new validator it is no longer allowed to use id fields containing underscores (``_``) in the resource id.

Programming API changes and plugins
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

#. Upgraded the Firely .NET SDK to v5.11.4, see its `release notes <https://github.com/FirelyTeam/firely-net-sdk/releases/tag/v5.11.4>`_.
#. Upgraded to v2.0 of the `firely-validator-api <https://github.com/FirelyTeam/firely-validator-api>`_ for validation and removed the legacy validator previous used. This applies to all validation within Firely Server.
#. ``ISearchRepository`` programming API has been changed to prevent unintended unauthorized access. It is required to explicitly set ``SearchOptions.Authorization`` when calling search, or use one of the extension methods for ISearchRepository, e.g.: ``GetByKeyWithFullAccess`` or ``SearchCurrentWithFullAccess``. ``SearchOptions`` authorization can be configured using one of the extension methods: ``WithAuthorization``, ``WithFullAccess``.
#. ``ISearchRepository`` extension methods that were not accepting ``SearchOptions`` as a parameter: ``GetByKey`` and ``SearchCurrent`` - are replaced with ``GetByKeyWithFullAccess`` and ``SearchCurrentWithFullAccess`` respectively.
#. ``SearchOptions`` is now an immutable record type, which might be a breaking change for some plugin code.
#. Extended the base class ``RelationalQueryFactory`` with support for the ``ResourceTypesNotValue`` (see :ref:`parameter_types`) and methods to express a predicate that is ``AlwaysFalse()`` or ``AlwaysTrue()``.
#. The ``VonkConfigurationAttribute`` no longer supports the deprecated ``isLicensedAs`` property.
#. The deprecated ``VonkConstants.MediaType`` values ``XmlR3``, ``JsonR3`` and ``TurtleR3`` have been removed. Use ``FhirXml``, ``FhirJson`` and ``FhirTurtle`` instead.
#. The deprecated method ``Check.HasValue()`` has been removed. Use ``Check.NotNull()`` instead.
#. Added documentation for ICapabilityStatementBuilder and related methods, see :ref:`vonk_reference_api_capabilities`.
#. Starting from this release the ``Vonk.Smart`` and ``Vonk.Plugin.SoFv2`` plugins are no longer supported and have been removed. They are replaced by the ``Vonk.Plugin.Smart`` plugin. For more information see :ref:`feature_accesscontrol_config`. It is necessary to adjust the pipeline options accordingly.
#. Removed plugin ``Vonk.Plugins.TerminologyIntegration``. ``Vonk.Pluigins.Terminology`` should be used instead.
#. Removed ``ISpecificationZipLocator`` from the public API.

Adjustments and Fixes
^^^^^^^^^^^^^^^^^^^^^

#. "This is an open FHIR endpoint for testing and educational purposes only. Uploading real personal data is strictly prohibited." will no longer be shown on the homepage when running in production mode.
#. Improved transaction handling for MongoDB to avoid duplicate key exceptions during the ingestion of resources.
#. SearchParameters of type ``Reference`` without a target are no longer logged as errors; they are now logged as warnings.
#. Improved handling of invalid resources within batch bundles. Firely Server now returns HTTP 200 - OK with individual OperationOutcomes when resources in the bundle are invalid.
#. Improved handling of large Bulk exports for MongoDB.
#. Fixed pre-validation when a pipe character (|) and a version are used within a canonical in meta.profile.
#. Improved handling of Patch exceptions.
#. Fixed ``FormatException`` when using ``$versions`` with an invalid MIME type.
#. Limited recursive Group-level Bulk exports to skip other Group resources that are transitively included.
#. Authorization endpoints listed in ``AdditionalIssuersInToken`` were previously accepted as the only valid issuers when the setting was used. Now, the authority is also accepted as a valid issuer of tokens.
#. Fixed indexing of elements of type ``url`` for URI search parameters.
#. Improved debug logging for the reindex operation to allow tracking the progress of long-running operations.
#. Administration APIs ``reset``, ``reindex/all``, ``reindex/searchparameters``, ``preload`` and ``importResources`` are now ``$reset``, ``$reindex-all``, ``$reindex``, ``$preload`` and ``$import-resources`` to conform with the naming rules for custom operations.
#. SMART on FHIR v2 scopes can include search arguments. Upon writing resources (create, update, delete) Firely Server used to only evaluate those for ``patient/`` scopes. Now, they are also evaluated for ``user/`` and ``system/`` scopes.

Configuration
^^^^^^^^^^^^^
.. attention::
    Default behavior of Firely Server has been tweaked by changing configuration values. 
    Make sure to reflect the desired behaviour by adjusting ``appsettings.instance.json`` or environment variables.

#. The use of other compartments then Patient in SMART on FHIR authorization is not well defined and potentially unsafe. So we redacted the ``Filters`` settings in ``SmartAuthorizationOptions``. You can now only specify a filter on the Patient compartment. For more information see :ref:`feature_accesscontrol_config`. If you configured just a Patient filter in the old format, Firely Server will interpret it in the new format and log a warning that you should update your settings. If you configured a filter on a different compartment, Firely Server will log an error and halt.
#. Evaluation of :ref:`Subscriptions<feature_subscription>` is now turned off by default. To enable - adjust ``SubscriptionEvaluatorOptions`` accordingly.
#. ``BundleOptions.DefaultTotal`` from now on has a default value of ``none`` for performance reasons. For available options see :ref:`bundle_options`.
#. ``TaskFileManagement.StoragePath`` was already marked as obsolete, and is now also no longer forward compatible. Use the ``TaskFileManagement.StorageService`` settings to provide the storage path, see :ref:`feature_bulkdataexport` for details.
#. ``SupportedInteractionOptions`` type has now been replaced by ``Operations<T>`` to accommodate for the requirements of a configuration revamp.
#. The configuration structure for operations has been completely revamped:

   * ``SupportedInteractionOptions`` has been replaced by a new top-level ``Operations`` configuration section
   * ``Administration.Security.OperationsToBeSecured`` has been replaced by per-operation ``NetworkProtected`` property
   * ``SmartAuthorizationOptions.Protected`` has been replaced by per-operation ``RequireAuthorization`` property
   * Each operation now has granular control over authorization, network protection, tenant requirements, etc.
   * See :ref:`disable_interactions` for detailed information about the new configuration structure and migration guide

.. note::
    If MultiTenancy is enabled, the ``history`` and ``vread`` operations are blocked for all resources. This is to prevent the possibility of cross-tenant access to resources. The ``history`` and ``vread`` operations are not supported in a multi-tenant environment.

