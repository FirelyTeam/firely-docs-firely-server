.. _firely_auth_releasenotes:

Release notes
=============

.. _firelyauth_releasenotes_4.4.1:

Release 4.4.1, September 24th, 2025
-----------------------------------

Fix
^^^

#. We upgraded to IdentityServer 7.3.2 to address an issue where Firely Auth would not start with the provided Duende license key.

.. _firelyauth_releasenotes_4.4.0:

Release 4.4.0, August 19th, 2025
--------------------------------

Feature
^^^^^^^

#. Added the ability to assign custom ``tags`` to a client registration for better categorization. Admin users can label clients with tags such as ``"Testing"`` or ``"In-Production"``.
#. SMART clients can now request a wildcard scope, even when restricted to certain resource types in the client registration. When a wildcard is requested, scopes will only be granted for the registered, allowed resource types.
#. The client registration process has been streamlined to improve the experience for admin users registering applications with the introduction of client types. Clients can be created following a type, which is either a Standalone/EHR Launch type, a Backend Service type, or a Management API type. Be aware that during upgrading you might be prompted to update your clients to pass the newly introduced client validation. Clients that are not yet updated will remain active with their full functionality. Upon editing these clients the admin will be prompted to update them to the new client registration format.
#. The client registration form now includes a "Legal & Compliance" section to document the following::

     - Contact Us URL  
     - Privacy Policy URL  
     - Terms of Use URL  
     - Legal Company Name
     
#. The FHIR Server Availability section on the admin dashboard now includes the FHIR Server endpoint information.
#. The Firely Auth user interface has been upgraded to visualize given consent and disclaimers per client as well as client compliance details more clearly.
#. The setting ``TrustedProxyIPNetworks`` now supports a new flag: ``AllowAnyNetworkOrigins``.  
   - Previously, bypassing IP restrictions was only possible by setting ``ASPNETCORE_ENVIRONMENT=Development``.  
   - With this release, the same behavior can be achieved explicitly via ``AllowAnyNetworkOrigins``.  
   - **Note:** This setting is disabled by default and should only be enabled in trusted, secure environments.

Fix
^^^

#. Fixed an ``ArgumentNullException`` that could occur if ``FhirUserLookupClaimsMapping`` was missing in the appsettings.
#. Improved error handling for scenarios where the FHIR Server is not connected to Firely Auth and a client attempts to retrieve an access token.
#. Fixed a startup issue that occurred while retrieving the CapabilityStatement from the FHIR Server configured with FHIR STU3 as its default information model.

.. _firelyauth_releasenotes_4.3.0:

Release 4.3.0, February 18th, 2025
----------------------------------

Feature
^^^^^^^

#. We have made Firely Auth safer by adding security headers to the responses. For more information, see :ref:`firely_auth_security_headers`.
#. Firely Auth will now check the JWKS URL for updated keys if a new request for an access token is made by the client. Before, Firely Auth would only retrieve these keys from the JWKS URL once, and would not check for updated keys.
#. We made improvements to the Standalone Launch and EHR Launch flows for the selection of the launch scope during the token request. For more information, see :ref:`firely_auth_endpoints_launchcontext` and :ref:`firely_auth_settings_launchcontext`.
#. We improved the behavior of Firely Auth when a user requests a token for a client that enforces 2FA without that user having enabled 2FA. Users will now get prompted to activate 2FA.
#. If a fhirUser claim is added as a ClientClaim, Firely Auth now enforces that this claim points to a ``Device`` resource.

Configuration
^^^^^^^^^^^^^

#. We added two new options to the UI settings: ``OrganizationTitle`` and ``OrganizationFavIconPath``. For more information, see :ref:`firely_auth_settings_ui`.
#. We introduced the setting ``AllowedOperationScopes`` to restrict the custom operation scopes that can be requested by a client. For more information, see :ref:`firely_auth_settings_clients`.
#. We introduced the ``ReverseProxySupport`` setting to enable and configure allowed IP ranges for ``X-Forwarded-*`` headers. For more information, see :ref:`firely_auth_settings_proxyheaders`.
#. Based on the ``ResourceTypeGrouping`` setting it is now possible to create groups of resources that can be selected by the admin as ``AllowedResources`` for a client, see :ref:`firely_auth_settings_resource_grouping`.


.. _firelyauth_releasenotes_4.2.1:

Release 4.2.1, October 10th, 2024
---------------------------------

Feature
^^^^^^^

#. With this hotfix release we enabled the use of X-Forwarded-Host and X-Forwarded-Prefix headers in combination with Firely Server. Also see :ref:`X_Forwarded_Host` for more information.

Fix
^^^

#. We fixed an issue where clients could no longer be saved in the UI if no external identity provider was configured.
#. We removed the "*" option from the Allowed Resources section in the client registration form as this lead the client to not accept any resources at all. To accept all resources the Allowed Resources field can be left empty.
#. We fixed an issue where the client grant consent lifetime would be set to zero if not otherwise configured, causing the consent to expire as soon as it was granted by the user. Consent will now only expire if an expiration time or date is set.


.. _firelyauth_releasenotes_4.2.0:

Release 4.2.0, September 25th, 2024
-----------------------------------

Feature
^^^^^^^

#. The user interface for regular users has been improved in several ways. Users can now view and revoke the consent they have given to clients. In addition, they can view and revoke the disclaimers they have accepted. For more information, see :ref:`firely_auth_settings_disclaimers`.
#. Admins are now able to view the .well-known/smart-configuration of the connected Firely Server instance.
#. The client registration form has been improved to become more intuitive. Depending on the grant type (either client_credentials or authorization_code), the form will show the necessary fields to fill in.
#. Firely Auth implemented $liveness and $readiness endpoints. These endpoints can be used to check the health of Firely Auth. For more information, see :ref:`firely_auth_liveness_readiness`.

Configuration
^^^^^^^^^^^^^

#. With this release, it is possible to restrict auto provisioning of SSO users by their security groups. For this we added a new setting: ``AutoProvisionFromSecurityGroup``. For more information, see :ref:`firely_auth_settings_externalidp`.
#. It is now possible to derive the FhirUser claim for SSO auto-provisioning from existing users in the Firely Server database using the ``FhirUserLookupClaimsMapping`` setting. For more information, see :ref:`firely_auth_settings_externalidp`.
#. It is now possible to set ``ConsentLifetime`` settings to control the lifetime of consent to clients. After this period has expired, users will be prompted again to give consent to this client. For more information, see :ref:`_firely_auth_settings_clients`.  
#. Added the option ``ShowDisclaimerFor`` to the disclaimer section to control when a disclaimer should be shown to the user. For more information, see :ref:`firely_auth_settings_disclaimers`.


.. _firelyauth_releasenotes_4.1.1:

Release 4.1.1, September 4th, 2024
----------------------------------

Fix
^^^

#. This is a hotfix release where we fixed the manual update scripts for updating the user databases to v4.1.x and higher. The scripts were not working correctly in the previous release.

.. _firelyauth_releasenotes_4.1.0:

Release 4.1.0, August 1st, 2024
-------------------------------

Feature
^^^^^^^

#. With this release, it is possible to let users log in via the :ref:`firely_auth_sso` flow without them needing a user account in Firely Auth first. Upon logging in, these users will be automatically created via auto-provisioning and stored in the Firely Auth user database.
#. It is now possible to add and edit client settings via the user interface. Before, these settings could only be changed by altering the appsettings. Note that because of this change, Firely Auth will load clients from the appsettings only once. After this initial load client settings need to be removed from the appsettings, or they will block start up of Firely Auth. 
#. We have made several improvements to the UI for a better user experience.

Configuration
^^^^^^^^^^^^^

#. Added `Lockout` options to customize the lockout period and max amount of failed lock-in requests.
#. It is now possible to add custom disclaimer templates that will be visible when the user tries to retrieve an access token. For more information, see :ref:`firely_auth_settings_disclaimers`.

Fix
^^^

#. Allow a missing trailing "/" when comparing the FHIR Server base url against the `aud` parameter when requesting a token.

Database
^^^^^^^^

.. attention::

    Starting with Firely Auth 4.1.0, every user account needs to contain a fhirUser claim in login. Users will be blocked from receiving an access token if the claim is not present. Please check after the migration if every user account, especially Practitioner accounts, have the claim present or add it if necessary. 

#. This release comes with an upgrade in the database structure to support the user auto-provisioning feature mentioned above. Any necessary database migrations will be automatically performed by Firely Auth  upon start up.

.. _firelyauth_releasenotes_4.0.0:

Release 4.0.0, June 24th, 2024
------------------------------

.. attention::

    The current release of Firely Auth, version 4.0, features new API capabilities, a redesigned user interface, and enhanced SMART on FHIR capabilities.
    With this release, Firely is deprecating support for any previous version of Firely Auth. It is recommended that all customers upgrade to the latest version.

.. note::

    Support for .NET 6 ends in November 2024. See `.NET Support Policy <https://dotnet.microsoft.com/en-us/platform/support/policy>`_. This version of Firely Auth supports .NET 8. So, we recommend that you upgrade to Firely Auth 4.0.0 and hence .NET 8 before November 2024.

Feature
^^^^^^^

#. (**Important**) Firely Auth has been upgraded to .NET 8. Please update the .NET runtime accordingly if installing Firely Auth using binaries. The Docker image has been updated for you. 
#. The homepage of Firely Auth provides a logged-in admin user the possibility to visualize the local .well-known/openid-configuration document incl. an overview of exposed endpoints and requestable SMART / OpenID scopes.
#. The homepage of Firely Auth provides a logged-in admin user the possibility to view statistics about registered clients and users.
#. Implemented an overview of all registered client applications for logged-in admin users which can be filtered based on different criteria.
#. The management CLI for Firely Auth has been removed. As an alternative, all functionality has been moved to a management API. See :ref:`firely_auth_mgmt` for more details.
#. The user management for Firely Auth has been redesigned. In-Memory users are no longer available. As an alternative Firely Auth now provides, by default, a SQLite database as an administration backend. Please migrate all In-Memory users manually either through the UI or management API. See :ref:`firely_auth_deploy_sqlite` for more details.
#. Implemented an overview of all registered users (local and SSO) for logged-in admin users.

Configuration
^^^^^^^^^^^^^

#. Added the possibility to provide custom email templates for the account verification of local users.
#. Added the possibility to customize the logo and text on the welcome page of Firely Auth.


.. _firelyauth_releasenotes_3.3.1:

Release 3.3.1, April 22nd, 2024
-------------------------------

Fix
^^^
#. Fixed an issue were Firely Auth running in docker was unable to connect to a SQL server user store.


.. _firelyauth_releasenotes_3.3.0:

Release 3.3.0, March 20th, 2024
-------------------------------

Security
^^^^^^^^
#. Disabling 2FA authentication for a client will now require a 2FA token from the user as an additional security step
#. Added 'Require2fa' to the default appsettings. This replaces the current 'RequireMfa' setting.

Feature
^^^^^^^

#. Firely Auth will now warn about invalid key/value pairs submitted to the launchContext API
#. Values in the form of '<resourceType>/<id>' submitted to the launchContext API will now be automatically translate to id-only values


.. _firelyauth_releasenotes_3.3.0-rc3:

Release 3.3.0-rc3, February 1st, 2024
-------------------------------------

Configuration
^^^^^^^^^^^^^

#. ``EnableLegacyFhirContext`` is added to switch the syntax of ``fhirContext`` between SoF v2.1 and v2.0. See :ref:`firely_auth_settings` for details.
#. ``ClientClaims`` and ``ClientClaimPrefix`` are added to help a client to define custom claims in the client credential flow. See :ref:`firely_auth_settings` for details.

Feature
^^^^^^^

#. Harmonized Serilog sinks with Firely Server. See :ref:`configure_log_sinks` for details of all supported sinks.
#. Enabled clients to add static custom claims in the client credential flow. See :ref:`firely_auth_settings` for details.

Fix
^^^

#. Fixed the EHR launch context in case of a user login via an external identity provider.
#. Improved the validation of setting ``AllowedResourceTypes``. Any invalid FHIR resource types will be rejected now.
#. Improved the validation of setting ``AllowFirelySpecialScopes``. Firely special scopes can now only be requested if an registered client has the setting set to ``true``.
#. In case of the EHR launch, no ``System.ArgumentException`` is thrown if both ``launch`` and ``launch/patient`` scopes are present in the request for the access token.

.. _firelyauth_releasenotes_3.3.0-rc2:

Release 3.3.0-rc2, November 23nd, 2023
--------------------------------------

Feature
^^^^^^^

#. You can restrict a :term:`client` to specific FHIR resource types, using the setting ``AllowedResourceTypes`` in the :ref:`firely_auth_settings_clients`. If the client requests SMART scopes for other resource types, the request will be denied.

.. _firelyauth_releasenotes_3.2.0:

Release 3.2.0, June 20th, 2023
------------------------------

Configuration
^^^^^^^^^^^^^
.. attention::
    To make it easier to understand, some configuration sections are renamed or reorganized.
    Please check the bullets below for a summary of changes. For the details, please check page :ref:`firely_auth_settings`.

#. Section ``KeyManagementConfig`` is renamed to ``KeyManagement``.
#. Section ``FhirServerConfig`` is renamed to ``FhirServer``.
#. Section ``ClientRegistrationConfig`` is renamed to ``ClientRegistration``.
#. Section ``TokenConfig`` is removed, the ``AccessTokenType`` for each client is moved to the registration of the specific client.
#. Section ``TokenIntrospectionConfig`` is removed, the secret of a token introspection end point can be configured using setting ``IntrospectionSecret`` within section ``FhirServer``.
#. For registering a specific client, the ``LaunchIds`` setting is removed. A dynamic Smart on Fhir launch context can be requested via the ``LaunchContext`` endpoint. See :ref:`firely_auth_endpoints_launchcontext` for details about how to request launch context dynamically.

Feature
^^^^^^^

#. Users now can change their own passwords after login.
#. A user account will be blocked temporarily after 5 unsuccessful authentication attempts and it will be unblocked in 5 minutes.
#. Added a setting ``KeySize`` to adjust the RSA key size generated by Firely Auth. By default, it is set to 2048.
#. Added a setting ``PasswordHashIterations`` to adjust the password hashing iterations in case of different security considerations. By default it is set to 600000. See :ref:`firely_auth_settings_userstore` for more details.
#. Introduced ``LaunchContext`` endpoint for requesting Smart on Fhir launch context dynamically. See :ref:`firely_auth_endpoints_launchcontext` for more details.
#. Added security attributes to session cookies.

Fix
^^^

#. Disabled Client Initiated Backchannel Authentication (CIBA).

.. attention::
    The ``aud`` used in an access token is updated to the ``FHIR_BASE_URL`` instead of the name of FHIR server.

.. _firelyauth_releasenotes_3.1.0:

Release 3.1.0, March 9th, 2023
------------------------------

Feature
^^^^^^^

#. Added a setting to configure CORS support for only a limited set of origins. See :ref:`firely_auth_settings_allowedorigins` for more details.

.. _firelyauth_releasenotes_3.0.0:

Release 3.0.0, December 2022
----------------------------

This is the first public release of Firely Auth, providing support for SMART on FHIR v1 and v2 and a SQL Server user store.
