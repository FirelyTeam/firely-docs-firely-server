.. _vonk_releasenotes_history_v5:

Current Firely Server release notes (v5.x)
==========================================

.. note::
    For information on how to upgrade, please have a look at our documentation on :ref:`upgrade`. You can download the binaries of the latest version from `this site <https://downloads.fire.ly/firely-server/versions/>`_, or pull the latest docker image::
        
        docker pull firely/server:latest

.. important::

    Firely is publishing the following announcement in response to important upcoming change regarding the .NET platform that affects our Firely Server.

    As you may be aware, Microsoft has decided to deprecate .NET 6, which has been the foundation of our Firely Server, as of November 12, 2024. After this date, .NET 6 will no longer receive updates, including essential security patches and stability improvements.

    To ensure that our Firely Server continues to provide the highest level of performance, security, and stability, we have proactively upgraded Firely Server to .NET 8. This upgrade is part of our commitment to offering you the most reliable and up-to-date solutions.

    *Key Updates:*

    * Firely Server v5.7.0: The upgrade to .NET 8 has been implemented in Firely Server v5.7.0 and later. We highly recommend all customers upgrade to the latest version to continue receiving stable updates and security patches.
    * Upcoming Firely Server 6: Early 2025, we will be releasing a new major version, Firely Server 6. According to our support policy, Firely Server 4 will not be supported anymore and therefore not be updated to .NET 8. It is crucial for customers using Firely Server 4 to plan their upgrade path to a supported version.
    * Upgrade Support: For customers using previous versions, especially previous major versions, we are here to assist you with your upgrade strategies. Please reach out to our support team at `server@fire.ly <mailto:server@fire.ly>`_ to discuss your specific needs.

    *What This Means for You:*

    * Continued Support: By moving to .NET 8, we guarantee that Firely Server will continue to receive regular updates, including critical security patches and new feature enhancements.
    * Enhanced Performance and Security: .NET 8 brings improvements in performance, security, and functionality, ensuring that your applications run efficiently and securely.

    We appreciate your continued trust in Firely Server. If you have any questions or require further assistance regarding this update, please do not hesitate to reach out to our support team at `server@fire.ly <mailto:server@fire.ly>`_.

    For more detailed information on the .NET lifecycle and support policies, you can refer to the official `Microsoft .NET and .NET Core lifecycle page <https://learn.microsoft.com/en-us/lifecycle/products/microsoft-net-and-net-core>`_.

.. _vonk_releasenotes_5_11_1:

Release 5.11.1, March 25th, 2025
--------------------------------

Feature
^^^^^^^
#. Enhanced logging with additional details when an invalid authorization token is received.

Fix
^^^

#. Fixed a ``FormatException`` when generating BDE exports at the group level for groups with a very large number of members.

.. _vonk_releasenotes_5_11_0:

Release 5.11.0, January 24th, 2025
----------------------------------

.. important::
    In Firely Server 5.11.0, we introduced the ``ReverseProxySupport`` setting. This setting by default disables the use of `X-Forwarded-*` headers, meaning that all deployments that utilize a reverse proxy may be impacted by this change. Please configure this setting carefully if you plan to upgrade.

Security
^^^^^^^^
#. Enhanced the security of the following custom operations. Previously, these operations would only check for any valid token if they were specified in the :ref:`Protected <feature_accesscontrol_config>` operations setting. All operations now take limitations expressed in the SMART on FHIR scopes within the access token into account:

    - $erase
    - $docref
    - $find-matches
    - $document
    - $member-match
    - $everything
    - $meta, $meta-add, $meta-delete
    - Document Handling on the root endpoint
    - Provenance handling

Feature
^^^^^^^

#. Introduced a new setting ``ReverseProxySupport`` enabling limits on which IPs or networks can specify an X-Forwarded-For header for Firely Server. In case the /administration endpoint is not otherwise protected by a firewall, the X-Forwarded-For header could be used previously to bypass the network protections of the API. See :ref:`xforwardedheader` for more details. To retain the old behaviour the following configuration can be used to allow X-Forwarded-* headers from any network source:

    .. code-block::

        "TrustedProxyIPNetworks": [
            "255.0.0.0/1",
            "0.0.0.0/1",
            "::1"
      ]

    Note: This will have security implications on the network protection of the administration API, due to the interpretation of the X-Forwarded-For header, if the access is not protected on a network layer. In addition, the ASPNETCORE_ENVIRONMENT environment variable needs to be set to "Development".

#. Improved performance of pagination in some scenarios when Firely Server is used with a MongoDB backend.
  - Note that because of the nature of this improvement, for some searches the ``last`` link in the search results bundle will not be available. This is because the last page is not known until the client requests it. The client can still use the ``next`` link to get the next page. For more information see :ref:`navigational_links`.

Database
^^^^^^^^
#. For **MongoDB** we added a new index ``ix_lu_id`` that facilitates the pagination performance improvement mentioned above. The migration adding the index is executed automatically on startup. Alternatively, you can apply it manually using the script ``FS_SchemaUpgrade_Data_v26_v27``.

Fix
^^^
#. The $versions operation now works in combination with a X-Forwarded-Prefix header. Previously it would return "The $versions operation is only supported on the root of the server or the root of a mapped endpoint".

.. _vonk_releasenotes_5_10_1:

Release 5.10.1, December 12th, 2024
-----------------------------------

.. important::

    Firely has found a vulnerability in Firely Server. In versions 5.7.0, 5.8.0, 5.9.0, 5.9.1 or 5.10.0 it does not correctly validate the signature of JWT Tokens. If you use SMART on FHIR authentication and use one of the mentioned versions, we recommend that you update to version 5.10.1 as soon as possible.
    
Fix
^^^

#. Fixed a vulnerability in the validation of JWT Tokens. 

.. _vonk_releasenotes_5_10_0:

Release 5.10.0, October 21st, 2024
----------------------------------

.. note::
    We have identified a potential security issue if your deployment matches all of the criteria below.
    Of course, we fixed the issue.
    If you match the criteria for your current deployment, or if you are in doubt, please contact the support desk.
    For background information on these criteria, see :ref:`feature_accesscontrol_config`.
    
    #. Firely Server is configured to accept write interactions, more specifically ‘create’
    #. You allow client applications with ``user/`` level scopes to do these write interactions.
    #. You use SMART on FHIR v2 scopes that include search arguments, either from the access token or from applicable AccessPolicyDefinitions.
    
    In these specific circumstances it might be possible for a client to create a resource that would not match the scopes.

Fix
^^^

#. Fixed a ``DuplicateKey`` exception occuring when creating resources under heavy load in parallel using MongoDB as the backend for the main database.
#. The ``$liveness`` and ``$readiness`` health checks were not responding while loading conformance resources to the administration database.
#. ``[ContextAware]`` attribute does not support ``Path`` argument anymore. Consider using route prefixes or alternatives to achieve the same effect when using multiple FHIR versions.

Feature
^^^^^^^

#. (Only for SQL Server): Added support for recursively including additional resources in BDE export. Read about the new config setting AdditionalResourcesMaxRecursionDepth in :ref:`feature_bulkdataexport`. The default value of this setting is 1, which maintains the behavior of previous Firely Server versions.
#. Changes in Bulk Data Export behavior: For Group- and Patient-level exports, any Group resources outside the respective Patient compartment that are referenced by resources within the Patient's compartment (i.e., Additional Resources) will no longer be included in the export.
#. Requests protected using system-level scopes with tokens containing a ``fhirUser`` claim of type Device are now rejected by default if no matching Access Policy can be found. This ensures that system-level clients are using appropriate scopes. See :ref:`system_level_scopes` for more details.
#. Added support for the ``X-Forwarded-Prefix`` header when hosting Firely Server on virtual subpaths. See :ref:`xforwardedheader` for more details.
#. Improved debug logging in case JWT / reference token validation fails with an exception.
#. Exporting relative references to absolute references when sending back a response to a client can now be disabled. See :ref:`uri_conversion` for more details.

.. _vonk_releasenotes_5_9_0:

Release 5.9.1, August 13th, 2024
--------------------------------

Fix
^^^

#. Enabled the NoOp plugin to work in combination with transaction bundles. Duplicate updates were not detected within transaction bundles.
#. Fixed a misleading error message in case a SearchParameter of type 'canonical' cannot be indexed due to invalid data.
#. Fixed a bug that would prevent Firely Server from starting up with the default appsettings if the license did not contain a token for auditlogging.

.. _vonk_releasenotes_5_9_0:

Release 5.9.0, August 7th, 2024
-------------------------------

This update was necessary to further improve the behavior with regard to the new licensing system for Firely Server Ingest. There are no changes to the functionality of Firely Server or Firely Server Ingest.
The new license system for FSI implements support for the packages mentioned in :ref:`vonk_releasenotes_5_8_0`. 

#. With the **Firely Essentials** license, there are no restrictions to the amount of resources that can be loaded into the database, but you can only load 1000 resources in one batch. The use of the :ref:`tool_fsi_recovery` functionality is disabled.
#. With the  **Firely Scale** license, there are no restrictions to the amount of resources that can be loaded into the database, and you can load an unlimited amount of resources in one batch.
#. with the **Firely Solution for CMS Interoperability & Prior Authorization Final Rule**, likewise, there are no restrictions to the amount of resources that can be loaded into the database, and you can load an unlimited amount of resources in one batch.

If you are still using an old version of the license, nothing changes for you. The new license system is only enforced for new licenses. You can check if you have the new license version by looking for the field ``LicenseVersion`` in your license file. If this field is present, you have the new license version.

.. _vonk_releasenotes_5_8_0:

Release 5.8.0, July 11th, 2024
------------------------------

.. note::

    Firely has restructured its licensing options, offering different packages of features tailored for various use cases. The new packages include:

    - **Firely Essentials** (Available for US and International customers)
    - **Firely Scale** (Available for US and International customers)
    - **Firely Solution for CMS Interoperability & Prior Authorization Final Rule** (Available for US customers only)


    Important information for existing customers:

    - Current contracts and feature sets will be honored.
    - If a license change is required in the future, our sales team will reach out to you.
    - New features may only be available in certain packages.
    
   `Read all about our new packages <http://fire.ly/packages/>`_ and please contact our sales team if you are interested in adding certain features or changing your package.

Configuration
^^^^^^^^^^^^^

#. License restrictions are now enforced for the X-Provenance header.
#. License restrictions are now enforced for Bulk Data Export in combination with PDex Member Attribution Lists.
#. License restrictions are now enforced for building custom plugins and facades.
#. License restrictions are now enforced for Access Policies.
#. License restrictions are now enforced for Virtual multi-tenancy.

Fix
^^^

#. The `Hosting.PathBase` setting was not correctly applied in version v5.7.0
#. Upgraded `System.Text.Json` to version 8.0.4 to address the vulnerability `CVE-2024-30105`. Please see the vulnerability details `here <https://github.com/advisories/GHSA-hh2w-p6rv-4g7w>`_.

.. _vonk_releasenotes_5_7_0:

Release 5.7.0, May 29th, 2024
-----------------------------

.. note::

    Due to a security vulnerability we recommend not to use version 5.7.0. Please update to version 5.10.1. See :ref:`vonk_releasenotes_5_10_1` for more information.

.. note::
    Support for .NET 6 ends in November 2024. See `.NET Support Policy <https://dotnet.microsoft.com/en-us/platform/support/policy>`_. This version of Firely Server supports .NET 8. So we recommend that you upgrade to Firely Server 5.7.0 and hence .NET 8 before November 2024.
    
.. note::
    Some internal changes in this release uncover a problem with the US Core search parameter for Observation.code and the related composite search parameters, like Observation.code-value-quantity. See :ref:`this warning<us-core_composite_parameters>` for fixes if you rely on US Core.

Features
^^^^^^^^
#. (**IMPORTANT**) Firely Server is upgraded to .NET 8. This means that you need the .NET 8 runtime to run FS. The docker image has been updated for you. We recommend that you update your plugins to .NET 8 as well.
#. (**IMPORTANT**) Implemented a check that all plugins specified in exclude section of the pipeline options are available. If not, Firely Server won't start. Please remove any missing configuration namespace from the appsettings if Firely Server encounters any error in the pipeline options during startup.
#. On SQL Server, permanent deletions of resources by the ``$erase`` and ``$purge`` operations are now processed asynchronously, so they do not block regular operations. See also :ref:`erase`.
#. Firely Server now provides functionality to run analytics queries on usage metrics collected via OpenTelemetry. This feature can be used to build reports for the ONC Real World Testing Condition and Maintenance of Certification requirement. See :ref:`feature_realworldtesting` for more information.

Improvements
^^^^^^^^^^^^
#. Loading of conformance resources from our ``errata`` zip files has been reworked. No changes functionality changes should be noticable.
#. The internal indexing implementation has been refactored and aligned between FS and FSI as well as between different database backends. No changes functionality changes should be noticable.
#. Improved the validation of fhirUser claims as part of an access token. Invalid claims are now rejected with HTTP 401 - Unauthorized.
#. Firely Server will now use the X-Security-Context HTTP header as the value for Binary.securityContext when posting a native Binary to the Binary endpoint.

Configuration
^^^^^^^^^^^^^
#. Firely Server will throw a VonkConfigurationException if $purge or $erase are configured for a repository for which this is not supported (e.g. Memory).
#. Added a new setting do disable the creation of AuditEvent resources and Audit logs independently from each other. See :ref:`feature_auditing` for more information. 

Fixes
^^^^^
#. The Administration API can be restricted by IP Network. We have fixed an issue where an IP address without a subnet prefix length was interpreted with a length of ``/24``, allowing IP addresses with a different last segment to pass. See :ref:`configure_administration_access` for more information.
#. Resolved an issue where certain element values like ``HumanName.prefix`` or ``HumanName.suffix`` were not indexed correctly for SQL/SQLite databases.
#. SMART on FHIR access tokens with ``OR`` scope combinations for a single resource type could behave as an ``AND``, causing certain requests to be unauthorized whereas access should have been granted.
#. Addressed an issue that led to the generation of extra AuditEvent resources.
#. Fixed an issue that would result in adding meta.versionId and meta.lastUpdated to contained resources.


.. _vonk_releasenotes_5_6_0:

Release 5.6.0, April 11th, 2024
-------------------------------

Features
^^^^^^^^

#. (**IMPORTANT**) Implemented a check that all plugins specified in the pipeline options are available. If not, Firely Server won't start. Please remove any missing configuration namespace from the appsettings if Firely Server encounters any error in the pipeline options during startup.
#. (**IMPORTANT**) BDE: Group members that are marked as inactive (``Group.member.inactive = true``) or not active currently (based on ``Group.member.period``) do not get exported via the group-based Bulk Data Export.
#. (:ref:`FSI<tool_fsi>`) An ingestion recovery feature has been added. For details, see :ref:`tool_fsi_recovery`.
#. Added a config setting ``BundleOptions.DefaultTotal`` that sets the default `_total` argument value for search requests if not specified in the request itself. Setting it to `none` can improve query search performance. See :ref:`Bundle Options<bundle_options>`.
#. Pubsub notifications are now supported when MongoDB is used as a repository.
#. Added ability to disable sending of AuditEvent-related notifications in PubSub. See :ref:`Pub Sub Configuration<pubsub_configuration>`.

Improvements
^^^^^^^^^^^^

#. Firely SDK has been updated to v5.7.0 - https://github.com/FirelyTeam/firely-net-sdk/releases/tag/v5.7.0

Configuration
^^^^^^^^^^^^^

#. (:ref:`FSI<tool_fsi>`) The ``--readPar`` CLI option (``workflow/readParallel`` appsettings option) has been removed.
#. Quartz has been removed from Firely Server. If you have a custom plugin that depends on Quartz, please make sure you install and initialize Quartz in the plugin itself.

Fixes
^^^^^

#. Fixed a NullReferenceException if one or more PubSub plugins are enabled but the config section is missing.
#. Fixed an endless loop of loading conformance resources with invalid search parameters.


.. _vonk_releasenotes_5_5_2:

Release 5.5.2, March 6th, 2024
------------------------------

Security
^^^^^^^^

#. Fixed a security vulnerability where the connection string to a MongoDB database acting as a Serilog sink was not redacted in the log on startup.
#. Fixed a security vulnerability where the connection string set in the AutoUpdateConnectionString setting for any database was not redacted in the log on startup.

Please note that both settings are disabled by default. We recommend to change passwords configured as part of any of the connection strings mentioned above.

.. _vonk_releasenotes_5_5_1:

Release 5.5.1, February 19th, 2024
----------------------------------


Fixes
^^^^^
#. (:ref:`FSI<tool_fsi>`) Fixed an error in indexing of contained resources when the target database is SQL Server. The issue causes an exception to be thrown when performing a search in Firely Server that filters resources based on a filed value from a contained resource. For example, an error would happen if the following search is performed ``GET {{BASE_URL}}/Observation?subject.gender=male`` if an Observation with the contained Patient subject had been ingested using FSI.

.. _vonk_releasenotes_5_5_0:

Release 5.5.0, February 6th, 2024
---------------------------------

Features
^^^^^^^^
#. Upgraded Firely Server to the latest Firely SDK v5.5.1, see its `releasenotes <https://github.com/FirelyTeam/firely-net-sdk/releases/tag/v5.5.1>`_. Any plugin using the ``ITypedElement`` interface must be recompiled using the Vonk.Core package version 5.5.* due to changes in the SDK.
#. Added new documentation on how to interact as a client with the PubSub API of Firely Server. See :ref:`pubsub_clients`.
#. Added new documentation on how to configure PubSub messaging on Azure Service Bus. See :ref:`azure_service_bus`.
#. Added a new configuration in the namespace ``Vonk.Plugin.Smart`` which enables the usage of SMART on FHIR v1 and v2 combined. The plugins ``Vonk.Smart`` and ``Vonk.Plugin.SoFv2`` are now deprecated. The deprecated plugins will continue to work in the current major version of Firely Server. See :ref:`feature_accesscontrol_config` for more details.
#. Added a new setting to the MongoDb configuration to provide a different connection string for the auto-update of the database. See ``MongoDbOptions.AutoUpdateConnectionString``.
#. Added support for AccessPolicies in FHIR STU3.
#. Added support for a No-Op check on Restful Update interactions. Firely Server can now check if a provided resource exactly matches the resource stored in the database and skip the update. See :ref:`restful_noop` for more information.

Fixes
^^^^^
#. The HTTP headers ``ETag``, ``Last-Modified`` and ``Content-Type`` were missing in a response from Firely Server in case a client included a request header with ``Prefer: return=minimal``. 
#. The HTTP headers ``ETag`` and ``Last-Modified`` were incorrect in case a client included ``Prefer: return=OperationOutcome`` as a request header.
#. Fixed the SearchParameter expression for AuditEvent.patient in FHIR STU3.

Security
^^^^^^^^
#. Updated the version of Microsoft.Data.SqlClient from 5.1.0 to 5.1.4 to include a vulnerability patch. See `CVE-2024-0056 <https://github.com/advisories/GHSA-98g6-xh36-x2p7>`_ for more details.

Configuration
^^^^^^^^^^^^^
.. warning::
    The current version of Firely Server (v5.5.0) does not support the use of In-Memory storage as administration database. This will be fixed in future versions of Firely Server. For now, it is advised to use the SQLite backend instead.

#. Improved validation of Administration.Security.AllowedNetworks settings. Invalid IP addresses or IP address ranges are now rejected.

Miscellaneous
^^^^^^^^^^^^^
#. :ref:`Firely Server Ingest <tool_fsi>` release cycle has been synchronized with the Firely Server release cycle. As of this version, the FSI changes will be captured on this page.

Database
^^^^^^^^
#. For **MongoDB** we added an index to improve the performance for Update and Create requests, ``ix_cnt_id``. The migration adding the index is executed automatically on startup.

.. _vonk_releasenotes_5_4_0:


Release 5.4.0, October 26th, 2023
---------------------------------

Features
^^^^^^^^
#. Firely Server can now interact asynchronously with a message broker (like RabbitMq or Azure Service Bus) using PubSub plugin. Resources can be ingested into Firely Server using a Firely Server-specific message. Additionally, all changes within the Firely Server database are broadcasted to the message bus. For more details, see :ref:`PubSub`.
#. Improved handling of $lastN where multiple Observations result in a tie based on their respective effective times.
#. $lastN now uses a case-sensitive comparison for text-only Observations.
#. $lastN now includes multi-codes in its grouping.
#. Added documentation regarding MongoDB security best-practices. See :ref:`configure_mongodb_security`.
#. Added support for MongoDB as a Serilog sink. See :ref:`configure_log_sinks`.
#. Compartment-wide searches can now be executed on a system level.

Fixes
^^^^^
#. Fixed an internal server where when trying to use an invalid target within a patch operation.

Security
^^^^^^^^

#. Updated the version of the base docker image from Alpine 3.14 to Alpine 3.18.
#. Introduced a setting ``SendServerHeader`` in the ``Hosting`` section to suppress the network service banner. By default, it is set to ``false``, so the Kestrel ``Server`` header will be removed in the response of a request.

   .. note::

     The setting ``SendServerHeader`` is applied only for Kestrel. If Firely Server is hosted differently, in IIS for example, and you want to remove the ``Server`` header, you need to handle it by yourself.

Configuration
^^^^^^^^^^^^^
#. Firely Server now allows for a custom location of configuration files to be set. See :ref:`configure_settings_path`.
#. When the `AllowedProfiles` setting is enabled, Firely Server will not allow non-profiled resources to be created or updated. This affects internally created AuditEvent and Provenance resources. For FHIR R4, Firely Server now uses for all AuditEvents IHE BALP meta.profile annotations. See :ref:`feature_prevalidation` for more information.
#. The "Vonk.Smart" plugin and the "Vonk.Plugin.SoFv2" plugin can now be used interchangeably in the pipeline options. Both plugins now share the same functionality.

.. _vonk_releasenotes_5_3_0:

Release 5.3.0, September 18th, 2023
-----------------------------------

Features
^^^^^^^^
#. The Da Vinci Member Attribution List implementation guide (ATR) is now officially supported by Firely Server. See :ref:`davinci_atr_ig` for more details.
#. Improved performance when evaluating access policies.
#. Improved access policies to evaluate permissions on a user-level based on a tenant id. See :ref:`feature_accesscontrol_permissions` for more details.
#. Improved transaction support for handling invalid X-Provenance headers. Resources will not be created in case the corresponding X-Provenance is invalid, similar to the behavior using SQL server.
#. Improved SQL error message in case the schema info is empty in the database.
#. Improved the BinaryWrapper plugin to return a Location header after a successful read.
#. Added support for logging to Splunk. See :ref:`configure_log_insights` for more details.
#. Improved support for the CARIN BlueButton implementation guide by differentiating between HTTP Status Code 403 - Forbidden (insufficient scopes) and HTTP 401 - Unauthorized (no token provided).
#. _include can now be used in combination with versioned references.
#. This release includes a new setting for handling the conversion of absolute to relative references: ``UrlMapping``. With this setting you can specify the FHIR Path of the elements that you would like to see converted. See :ref:`uri_conversion` for more details.

Fixes
^^^^^
#. Fixed an internal server error when passing empty values to required elements in case the setting PermissiveParsing is set to Strict and ValidationLevel is set to Full.
#. Fixed an internal server error when posting a bundle resource to the bundle or transaction endpoint in case not all resources contained a pre-assigned ID.
#. Fixed incorrect transaction handling when SQLite (for administration) and MongoDB are used in combination. This resulted in an internal server error when creating resources in an empty database.

Vonk.Core and Plugins
^^^^^^^^^^^^^^^^^^^^^
#. The `SupportsCustomOperation` method has been deprecated. Please use `SupportsOperation` instead as that method takes the interaction level into account which is more aligned with the configuration options described in :ref:`disable_interactions`.

.. _vonk_releasenotes_5_2_0:

Release 5.2.0, August 8th, 2023
-------------------------------

Configuration
^^^^^^^^^^^^^
#. Firely Server now raises a configuration error if the https port is bound to the same port as http

Features
^^^^^^^^

#. An informational message is now logged for auditing purposes if authorization for a request was successful. Previously only authorization failures were logged.
#. Improved compartment checks for writing resources to a Patient compartment with a patient-level access token. All compartment references need to refer to the same compartment. This is important for resources that have multiple compartment references which may refer to different Patients (e.g. AllergyIntolerance.recorder and AllergyIntolerance.patient).
#. Added support for permanently deleting all resources within a Patient compartment using the $purge operation. See :ref:`erase` for more details.
#. Enable FS to write logs to AWS CloudWatch, see :ref:`configure_log_sinks`.
#. We upgraded Firely Server to the latest SDK 5.2.0, see its `releasenotes <https://github.com/FirelyTeam/firely-net-sdk/releases/tag/v5.2.0>`_. 

Fixes
^^^^^

#. The ``_count`` argument was not marked as handled in the case of an HTTP 401 - Unauthorized status code, leading to a superfluous warning message in the resulting OperationOutcome.
#. modifierExtensions without a matching StructureDefinition in the administration database are no longer rejected when the validation level is set to "Core".
#. Improved transaction handling by making sure that accidentally providing a versionId in a resource within a transaction does not lead to versioned references.
#. Fixed a bug in ``$everything`` running on SQL server that resulted in contained resources being returned as individual resources outside of their container.
#. The SearchAnonymization plugin now also anonymizes URLs in a history bundle.
#. The FHIR specification does not allow the use of arbitrary search parameters on the ``_history`` operation. Firely Server now enforces this and rejects those parameters.
#. Simplifier projects specified under the AdministrationImportOptions were not imported on start-up

.. _vonk_releasenotes_5_1_1:

Release 5.1.1, June 29th, 2023
---------------------------------

.. attention::
  This is a security related release that addresses a vulnerability in Firely Server which may lead to unauthorized access using the $everything operation. This update is highly recommended for all customers.

Security
^^^^^^^^

#. Fixed an issue where the $everything operation did not respect the patient launch parameter in the SMART on FHIR access token. This means that the user could have requested information belonging to a different patient than the one mentioned in the access token. This issue only happened when an access token used for $everything actually contained a patient launch context such as when allowing a patient to request its own record.

#. Fixed an issue where the $everything and $export operation would potentially return resources belonging to different users or patients when running the these operations on a MongoDB database. In case a Patient shared a common resources with another Patient, e.g. a Group resource, all data would be returned even if it would be outside of the compartment of the Patient requesting the data.

.. _vonk_releasenotes_5_1_0:

Release 5.1.0, June 20th, 2023
------------------------------

Firely Server 5.1.0 brings enhanced support for Bulk Data Export 2.0, FHIR R5 (5.0.0) and several other features.

Existing installations may be affected by the fixes on composite search parameters for the SQL Server database repository.

Database
^^^^^^^^

* The SQL Server database schema is upgraded from version 26 to 27. The upgrade will be applied automatically, but if you have a very large database you may want to apply it manually using the script FS_SchemaUpgrade_Data_v26_v27.
* This implies that you also need to upgrade Firely Server Ingest to version 2.2.0, to match the new database schema.

Configuration
^^^^^^^^^^^^^

* The ``HistoryOptions`` configuration option has been removed, so you can delete it from your configuration in ``appsettings.instance.json`` or environment variables as well. The returned resources will be limited by the settings in the ``BundleOptions``, see :ref:`bundle_options`.
* The Bulk Data Export upgrades (see below) come with a few extra configuration settings, see :ref:`feature_bulkdataexport`

Features
^^^^^^^^
* Firely Server is upgraded to the release version (5.0.0) of FHIR R5. If you have your administration database in SQL Server or MongoDB, this means that the conformance resources will be :ref:`re-imported <conformance_import>`.
* We included ``errataR5.zip`` with fixes for a few resources and search parameters that have errors in the specification. These are imported automatically at startup.
* We upgraded Firely Server to the latest SDK 5.1.0, see its `releasenotes <https://github.com/FirelyTeam/firely-net-sdk/releases/tag/v5.1.0>`_.
* Bulk Data Export is enhanced with new support for:
  
  * patient Filter
  * _elements filter
  * HTTP POST with a Parameters resource
  * export to Azure Blob or Azure Files, see :ref:`feature_bulkdataexport` for related settings

* Our public Postman collection proving support for US-Core is updated, see :ref:`compliance_g_10`
* Updated our vulnerability scanning, to further enhance your trust in our binaries and images.
* Cross-origin requests (CORS) are restricted to requests from secure connections.
* The following security headers were added:

  * to the html output (the homepage): ``script nonce="..."``, ``cache-control``, ``content-security-policy``, ``referrer-policy``, ``x-content-type-options``
  * and to API response: ``cache-control:no-store``

* You can configure limits on Kestrel, see :ref:`hosting_options`, although using a :ref:`reverse proxy<deploy_reverseProxy>` is still preferred.
* Added a configuration error to the log if the default information model (aka FHIR version) is not loaded in the pipeline.
* SearchParameters should not be dependent upon the time of indexing. Therefore we disallow the functions below to be used in their expressions.
  Firely Server will log an error if any of these are encountered, and the SearchParameter will not be used.

    * ``now()``
    * ``timeOfDay()``
    * ``today()``

Fix
^^^
* Composite search parameters are more accurately supported on SQL Server. Previously, a match could be made across components (e.g. the code from one ``Observation.component`` and the value of another).
  This was very efficient from a database perspective, but not entirely correct as it could yield more results than expected.
  We corrected that behavior, so a resource must match all parts of the parameter in the same component. This comes with a database migration, see above.

    .. warning:: 
        For new or updated resources, the changes take effect immediately.
        To apply it to existing resources, you will need to :ref:`re-index <feature_customsp_reindex>` all resources affected by composite search parameters.
        In general that is just Observation resources. You can :ref:`feature_customsp_reindex_specific` by including the composite parameters and their components::

            POST <base>/administration/<R4 or R5>/reindex/searchparameters
            BODY:
            include=Observation.code-value-concept,Observation.code-value-date,Observation.code-value-quantity,Observation.code-value-string,Observation.combo-code-value-concept,Observation.combo-code-value-quantity,Observation.component-code-value-concept,Observation.component-code-value-quantity,Observation.code,Observation.value-concept,Observation.value-date,Observation.value-quantity,Observation.value-string,Observation.combo-code,Observation.combo-value-concept,Observation.combo-value-quantity,Observation.component-code,Observation.component-value-concept,Observation.component-value-quantity

    .. warning:: 
        If you still use the old SQL Server implementation (see :ref:`vonk_releasenotes_460`), you do not benefit from this improvement.
        Please upgrade to the new implementation.

* All warnings about composite search parameters during startup (usually caused by remaining errors in the FHIR specification) are resolved.
* Also several other errors in the FHIR specification were fixed in the various ``errata.zip`` files, so FS does not need to warn about them anymore:

  * STU3, search parameters of type `reference` that lacked a target element:

    *  Linkage.item parameter
    *  Linkage.source parameter
    *  RequestGroup-instantiates-canonical

  * R5, search parameters that lack a fhirpath expression:

    * Medication.form
    * MedicationKnowledge.packaging-cost
    * MedicationKnowledge.packaging-cost-concept

* Custom search parameters may contain errors in their FHIRPath expression. These can manifest either when adding them to Firely Server, or when they are evaluated against a new or updated resource. In both cases we improved the error reporting.
* AuditEvents generated for interactions with Firely Server using FHIR R5 were missing a link to the Patient compartment in case a Patient resource was created/read/updated/deleted. Now the AuditEvent.patient element is populated in these cases and by this linked to the Patient compartment. Previously generated AuditEvents are therefore not exported as part of a Bulk Data Export request on a Patient level or when using $everything on Patient.
* Any markdown in the CapabilityStatement is properly escaped.
* Firely Server does not support the search parameters whose field ``xpathUsage`` (STU3, R4) or ``processingMode`` (R5) is not set to ``normal``. They are now filtered at startup. See :ref:`restful_search_limitations`.
* ``CapabilityStatement.instantiates`` on the ``<url>/metadata`` endpoint only lists the CapabilityStatements from the administration API that have their ``status:active``.
* Firely Server did not support bringing a resource that has earlier been deleted back to life with a conditional update while providing the logical id of the resource in the request payload.
* Sensitive information in the settings that was logged before is now redacted: 

  * the SSL Certificate password
  * the MongoDB connectionstring
 
* Regarding :ref:`feature_customsp_reindex`: if an erroneous parameter is provided as ``include``, a proper error is returned. 
* URL query decoding was revamped. You should not see any differences, but please contact us if you do.
* Firely Server leniently accepted a literal unescaped "+" sign as part of the request url and didn't interpret it as a reserved character according to `RFC 3986 <https://www.rfc-editor.org/rfc/rfc3986#section-2.2>`_. Firely Server now correctly interprets it as whitespace.

  * This improves the cooperation with AWS API Gateway, that encodes spaces as ``+`` by default.
  * Only the '+' in the ``_format=fhir+json`` parameter is retained.

    .. warning::
        In case the ``+`` sign is used as part of a search parameter value it needs to be URL encoded as ``%2B``. An unescaped value will be interpreted as described above, which may lead to unexpected results.
    
* When using the settings to :ref:`supportedmodel`, it was easy to forget two parameters that Firely Server depends on. These parameters are now always added silently:

    * ``Resource._lastUpdated``
    * ``StructureDefinition.url``


Plugin and Facade
^^^^^^^^^^^^^^^^^

* ``Vonk.Core`` no longer references the deprecated package ``Microsoft.AspNetCore.Server.Kestrel.Core:2.2.0`` (see `related MSDN documentation <https://learn.microsoft.com/en-us/aspnet/core/fundamentals/target-aspnetcore?view=aspnetcore-6.0&tabs=visual-studio#use-the-aspnet-core-shared-framework>`_).
   
.. warning:: 
    For plugin developers, this could result in a compilation error when rebuilding  against the latest ``Vonk.Core`` nuget package::

        CS0104: 'BadHttpRequestException' is an ambiguous reference between 'Microsoft.AspNetCore.Server.Kestrel.Core.BadHttpRequestException' and 'Microsoft.AspNetCore.Http.BadHttpRequestException'

    In this case, make sure to reference ``Microsoft.AspNetCore.Http.BadHttpRequestException``, as ``Microsoft.AspNetCore.Server.Kestrel.BadHttpRequestException`` has been marked as obsolete.

* The ONC 2014 Edition Cures Update paragraph 170.315(b)(10) `Electronic Health Information Export <https://www.healthit.gov/test-method/electronic-health-information-export>`_ requires the export of a single Patients' record. 
  We made two interfaces public to allow :ref:`feature_bulkdataexport_facade` implementers to implement that export, and facilitate the new filters in BDE 2.0. 
  They are very similar to their counterparts ``IPatientBulkDataExportRepository`` and ``IGroupBulkDataExportRepository``, 
  but add the ability to filter by a list of logical id's of Patients.

  * ``IPatientBulkDataWithPatientsFilterExportRepository``
  * ``IGroupBulkDataWithPatientsFilterExportRepository``

* Loading dll's: In 5.0.0 we made the assembly loading resilient to duplicate dll's. That has led to a regression error with loading native (non .NET) dll's. We fixed that.

.. _vonk_releasenotes_5_0_0:

Release 5.0.0, March 9th, 2023
------------------------------

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

#. The database upgrade means that you also need an upgraded version of Firely Server Ingest, :ref:`version 2.0.1<fsi_releasenotes_2.0.1>`

Feature
^^^^^^^

#. The initial public version of Firely Auth has been released. Firely Auth is an optimized OAuth2 provider that understands SMART on FHIR scopes and the FHIR resource types they apply to out of the box. See :ref:`firely_auth_index` for more information.
#. The default information model for Firely Server is now R4.
#. FHIR R5 (based on v5.0.0-snapshot3) is now officially supported and not considered experimental anymore. We will also support the final release of FHIR R5 once it is published.

   .. attention::
       If you used R5 with Firely Server before and your administration database is either SQL or MongoDB based, you need to either delete it or reimport all FHIR R5 artifacts. If you use SQLite, you should use our new administration database that is distributed with Firely Server. If you need any assistance, please :ref:`contact us<vonk-contact>`.

#. Firely Server is now certified according to §170.315 (g)(10) Standardized API for patient and population services, see `our G10 feature page <https://fire.ly/g10-certification/>`_ for more information.
#. Bulk Data Export now supports SMART on FHIR v2.
#. Our :ref:`SMART on FHIR documentation <feature_accesscontrol>` has been updated for SMART on FHIR v2.
#. Support for our ``AccessPolicy`` resource has been added. This allows building of custom access policy resources. See the :ref:`AccessPolicy section <feature_accesscontrol_permissions>` to learn more about it.
#. Firely Server now generates FHIR AuditEvent resources conforming to `IHE Basic Audit Log Patterns <https://profiles.ihe.net/ITI/BALP/index.html>`_. Fields that are included in the audit event log and AuditEvent resources now contain the same content.
#. Contents of AuditEvents can now be modified via a plugin. See :ref:`AuditEvent customization <audit_event_customization>` for further info.
#. Two new operations have been added, namely ``$verify-integrity`` and ``$verify-integrity-status``. These allow you to verify that no AuditEvents have been manipulated on the server. See :ref:`audit_event_integrity` on how to use this feature.
#. You can now add signatures to ``AuditEvents``. See :ref:`audit_event_integrity` for more information.
#. Firely Server now supports searching on version-specific references. Consult the `FHIR specification <https://www.hl7.org/fhir/search.html#versions>`_ for more information.
#. Serilog CorrelationId support has been enabled in Firely Server. Please consult the `official documentation <https://github.com/ekmsystems/serilog-enrichers-correlation-id>`_ on how to configure it.
#. We have added a public :ref:`Postman collection <postman_tutorial>` to test Firely Server's RESTful endpoints.
#. Wildcard support for ``include`` is now declared in Firely Server's ``CapabilityStatement``.
#. Navigational links (next, prev, last) in a searchset bundle are now anonymized by default. Privacy-sensitive information in search parameter values are hidden behind a UUID. Please note that this behaviour is required by FHIR R5 and can only be disabled in FHIR R4 and STU3. See :ref:`navigational_links` for more information.

Fix
^^^

#. When performing a Bulk Data Export request with a Firely Server instance running on a SQL database, it will return the Group resource even if it has no members. 
#. FS now declares support for Bulk Data Export Group export operations in its CapabilityStatement. This features was available before, but missing from FS's CapabilityStatement. 
#. Bulk Data Export now returns a successful status code (``202``) instead of an erroneous status code if no resources were matched for an export. The resulting export will include an empty array as described in the `specification <https://hl7.org/fhir/uv/bulkdata/export/index.html#response---complete-status>`_.
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
#. For FHIR STU3 and R4, ``Contract``, ``GuidanceResponse`` and ``Task`` have been added to the ``Patient`` compartment. This fix is backported from the FHIR R5 release.
#. Firely Server now returns a ``404`` and ``OperationOutcome`` when the status of a canceled export is requested.
#. When preloading resources via Firely Server's import feature, no more errors will be logged if subfolders are present.
#. Warnings and errors with regards to ``AuditEvent`` indexing problems have been fixed and will no longer appear in the log.
#. Searches on ``period`` elements that have equal start/end times either at the start or beginning of the year will now return the correct results. Previously, these searches did not return any results.
#. The US Core ``patient`` search parameters have been fixed. They now only target ``Patient``, not ``Group`` and ``Patient``.
#. The response for unsupported ``Prefer`` headers has been improved. The ``Prefer`` header's value is now included in the ``OperationOutcome``.
#. Firely Server will now respond more gracefully with a ``408`` instead of a ``500`` status code in case the ``$everything`` operation times out.
#. Custom ``SearchParameters`` can now include the character '-' in ``code``.
#. The copyright data in Firely Server's executable has been updated.
#. Miscellaneous flaws in Firely Server's `Swagger documentation UI <_static/swagger>`_ have been fixed.
#. Custom resources are no longer exposed in the CapabilityStatement. The required binding on CapabilityStatement.rest.resource.type led to a validation error.

Security
^^^^^^^^

#. We upgraded our MongoDB drivers to fix a recently discovered security vulnerability. According to `CVE-2022-4828 <https://www.cve.org/CVERecord?id=CVE-2022-48282>`_ Firely Server is not vulnerable.
#. All of the contents included in Firely Server's index page are now hosted locally which prevents attackers from injecting malicious Javascript via manipulating externally hosted content.

Plugin and Facade
^^^^^^^^^^^^^^^^^

#. Firely Server and internal plugins now use the `Firely .NET SDK 5.0.0 <https://github.com/FirelyTeam/firely-net-sdk/releases/tag/v5.0.0>`_. Follow the link for an overview of all changes.
#. ``Vonk.Core`` now targets ``net6.0``. 
#. All ``Microsoft.EntityFrameworkCore.*`` packages have been updated to version ``6.0.13``. Please upgrade your plugin or facade to this version as well.

   .. warning::
       Due to the above changes, all of your plugins need to be recompiled against this FS release.

#. Please note that the ``Vonk.Smart`` package will not be published on NuGet anymore.
#. A new plugin is bundled together by default with Firely Server: Vonk.Plugin.SearchAnonymization. Please see the feature section above for a description. The plugin is enabled by default in the pipeline options.
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
    With the release of Firely Server 5.0, we will officially stop support for Firely Server v3.x. We will continue supporting customers that run Firely Server v4.x.

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
#. Bulk Data Export now returns a successful status code (``202``) instead of an erroneous status code if no resources were matched for an export. The resulting export will include an empty array as described in the `specification <https://hl7.org/fhir/uv/bulkdata/export/index.html#response---complete-status>`_.
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
#. Firely Server now returns the correct CodeSystem ``http://terminology.hl7.org/CodeSystem/restful-security-service`` within the security section of its R4 ``CapabilityStatement``. Before this change, the old R3 CodeSystem ``http://hl7.org/fhir/restful-security-service`` was falsely returned.
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
