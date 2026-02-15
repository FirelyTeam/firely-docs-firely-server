.. _feature_accesscontrol_config:

Enforcing access control
========================

.. note::

  The features described on this page are available in **all** :ref:`Firely Server editions <vonk_overview>`.

Firely Server offers a robust and flexible framework for enforcing access control, supporting everything from SMART on FHIR scopes to custom authorization logic via plugins. This section explains how to configure these mechanisms to meet a wide range of security and integration requirements.

SMART on FHIR configuration
---------------------------

Firely Server fully supports the syntax of SMART v1: ``( 'patient' | 'user' ) '/' ( fhir-resource | '*' ) '.' ( 'read' | 'write' | '*' )``

These are examples of SMART v1 scopes, which are all supported by Firely Server:

* ``scope=user/Observation.read``: the user is allowed to read Observation resources
* ``scope=user/Encounter.write``: the user is allowed to write Encounter resources
* ``scope=user/*.read``: the user is allowed to read any type of resource
* ``scope=user/*.write``: the user is allowed to write any type of resource
* ``scope=[array of individual scopes]``: the user is allowed to do the union of what each of the scopes allow

Additionally, the syntax of SMART v2 scopes is fully supported: ``( 'patient' | 'user' | 'system' ) '/' ( fhir-resource | '*' ) '.' ( 'c' | 'r' | 'u' | 'd' | 's' | '*') ? param = value``

All SMART v1 scopes can also be expressed in SMART v2:

* ``scope=user/Observation.r``: the user allows to read Observation resources
* ``scope=user/Encounter.cu``: the user allows to write (create and update) Encounter resources
* ``scope=user/\*.r``: the user allows to read any type of resource
* ``scope=user/\*.cu``: the user allows to write (create and update) any type of resource
* ``scope=[array of individual scopes]``: the user is allowed to do the union of what each of the scopes allow

All search capabilities supported by Firely Server can also be evaluated as part of the access scope using SMART v2. 

* ``scope=user/Observation.r?category=laboratory``: the user is allowed to read Observation resources with a category element containing the code "laboratory"
* ``scope=user/*.rs?_tag=http://example.org/fhir/sid/codes|some-tag``: the user is allowed to read and search all resource containing a tag in ``Meta.tag`` with system ``http://example.org/fhir/sid/codes`` and code ``some-tag``
* ``scope=user/Observation.rs?encounter.id=Encounter/test``: the user is allowed to see all Observation resources linked to the Encounter with id "test".
* ``scope=user/Observation.rs?encounter.identifier=http://my-hospital/ids|3456789``: the user is allowed to see all Observation resources linked to the Encounter with an identifier with system ``http://my-hospital/ids`` and value ``345678``.

.. note::
    As you can see from the last example, chaining (and reverse Chaining, ``_has``) is supported. Note though that using a (reverse) chained search as part of authorization can make search queries significantly more complex and less performant.

You will need to add the Smart plugin to the Firely Server pipeline. See :ref:`vonk_plugins` for more information. In ``appsettings[.instance].json``, locate the pipeline
configuration in the ``PipelineOptions`` section, or copy that section from ``appsettings.default.json`` (see also :ref:`configure_change_settings`)::

	"PipelineOptions": {
	  "PluginDirectory": "./plugins",
	  "Branches": [
		{
		  "Path": "/",
		  "Include": [
			"Vonk.Core",
			"Vonk.Fhir.R3",
			...

Add ``Vonk.Plugin.Smart`` to the list of included plugins. When you restart Firely Server, the Smart service will be added to the pipeline.

.. note:: 
  From Firely Server v5.5.0 on, the ``Vonk.Plugin.Smart`` plugin replaces the legacy plugins ``Vonk.Smart`` (for SMART v1) and ``Vonk.Plugin.SoFv2`` (for SMART v2). For Firely Server versions older than v5.5.0, these legacy plugins may still be used. For Firely Server versions v6.0.0 and newer, the legacy plugins are no longer supported and have been removed.
  Be sure to only enable one of these plugins since an error will be thrown if both plugins are part of the pipeline. Please note that the SMART v2 plugin will allow the usage of the SMART v1 and SMART v2 syntax.

You can control the way Access Control based on `SMART on FHIR <https://fire.ly/smart-on-fhir/>`_ behaves with the SmartAuthorizationOptions in the :ref:`configure_appsettings`::

    "SmartAuthorizationOptions": {
      "Enabled": true,
      //"ClockSkew": "00:05:00",
      "EnforceAccessPolicies": true,
      "PatientFilter": "identifier=#patient#", //Filter on a Patient compartment if a 'patient' launch scope is in the auth token, for the Patient that has an identifier matching the value of that 'patient' launch scope
      "Authority": "https://example.org/base-url-to-your-identity-provider",
    //"AdditionalBaseEndpointsInDiscoveryDocument": ["additional-url-to-your-identity-provider"],
    //"AdditionalIssuersInToken": ["additional-url-to-your-identity-provider"],   
    //"Audience": "https://example.org/base-url-of-firely-server", //Has to match the value the Authority provides in the audience claim.
    //"ClaimsNamespace": "http://smarthealthit.org",
      "RequireHttpsToProvider": false, //You want this set to true (the default) in a production environment!
      "EnableAnonymousAccess": false, //Enable anonymous access with limited scopes when no token is provided
      "AnonymousScopes": "", //Space separated list of scopes that are allowed for anonymous access, e.g. "user/Organization.rs user/Location.rs"
      "Protected": {
        "InstanceLevelInteractions": "read, vread, update, delete, history, conditional_delete, conditional_update, $validate",
        "TypeLevelInteractions": "create, search, history, conditional_create",
        "WholeSystemInteractions": "batch, transaction, history, search"
      },
      "TokenIntrospection": {
        "ClientId": "Firely Server",
        "ClientSecret": "secret"
      },
      "ShowAuthorizationPII": false,      
    //"AccessTokenScopeReplace": "-",
      "SmartCapabilities": [
        "LaunchStandalone",
        "LaunchEhr",
        //"AuthorizePost",
        "ClientPublic",
        "ClientConfidentialSymmetric",
        //"ClientConfidentialAsymmetric",
        "SsoOpenidConnect",
        "ContextStandalonePatient",
        "ContextStandaloneEncounter",
        "ContextEhrPatient",
        "ContextEhrEncounter",
        "PermissionPatient",
        "PermissionUser",
        "PermissionOffline",
        "PermissionOnline",
        "PermissionV1",
        //"PermissionV2",
        "ContextStyle",
        "ContextBanner"
      ]
    }

To enable SMART on FHIR in Firely Server, the following core settings must be configured:

* Enabled: With this setting you can disable ('false') the authentication and authorization altogether. When it is enabled ('true'), Firely Server will also evaluate the other settings. The default value is 'false'. This implies that authorization is disabled as if no SmartAuthorizationOptions section is present in the settings.
* PatientFilter: Defines how the ``patient`` launch context is translated to a search argument. See :ref:`feature_accesscontrol_compartment` for more background. You can use any supported search parameter defined on Patient. It should contain ``#patient#``, which is substituted by the value of the ``patient`` claim.
* Authority: The base url of your identity provider, such that ``{{base_url}}/.well-known/openid-configuration`` returns a valid configuration response (`OpenID Connect Discovery documentation <https://openid.net/specs/openid-connect-discovery-1_0.html#rfc.section.4.2>`_). At minimum, the ``jwks_uri``, ``token_endpoint`` and ``authorization_endpoint`` keys are required in addition to the keys required by the specification. See :ref:`Firely Auth<feature_accesscontrol_idprovider>` for more background.
* Audience: Defines the name of this Firely Server instance as it is known to the Authorization server. The default should be the base url of Firely Server.
* EnableAnonymousAccess: When set to ``true``, allows limited access to FHIR resources when no valid authorization token is provided. This uses the scopes defined in ``AnonymousScopes``. The default value is ``false``. See :ref:`Anonymous Access Configuration<feature_accesscontrol_anonymous>` for more information.
* AnonymousScopes: Defines the space-separated list of SMART scopes that are permitted for anonymous (non-authenticated) access. Only ``user/`` scopes are allowed, and they cannot include wildcard access (``user/*``) or access to Patient compartment resources. This setting is only relevant when ``EnableAnonymousAccess`` is ``true``.

Additional advanced configuration can be achieved through the following settings:

* EnforceAccessPolicies: Global flag that controls whether ``AccessPolicies`` are enforced for all matching ``fhirUsers``. See :ref:`feature_accesscontrol_permissions` for more details.
* ClockSkew: Allow potential time discrepancies between the authorization server and the FHIR server, allowing for a small tolerance window when checking token expiration and validity times. Defaults to 5 minutes.
* AdditionalBaseEndpointsInDiscoveryDocument: Optional configuration setting. Add additional base authority endpoints that your identity provider also uses for operations that are listed in the .well-known document. 
* AdditionalIssuersInToken: Optional configuration setting. The additional issuer setting will extend the list of issuer urls that are valid within the issuer claim in the token passed to Firely Server. The token validation will be adjusted accordingly. Please note that it does not influence which issuer urls are allowed in the .well-known/openid-configuration document of the authorization server.
* ClaimsNamespace: Some authorization providers will prefix all their claims with a namespace, e.g. ``http://my.company.org/auth/user/*.read``. Configure the namespace here to make Firely Server interpret it correctly. It will then strip off that prefix and interpret it as just ``user/*.read``. By default no namespace is configured.
* RequireHttpsToProvider: Token exchange with an Authorization server should always happen over https. However, in a local testing scenario you may need to use http. Then you can set this to 'false'. The default value is 'true'. 
* Protected: This setting controls which of the interactions actually require authentication. In the example values provided here, $validate is not in the TypeLevelInteractions. This means that you can use POST [base-url]/Patient/$validate without authorization. Since you only read Conformance resources with this interaction, this might make sense.
* TokenIntrospection: This setting is configurable when you use `reference tokens <https://docs.duendesoftware.com/identityserver/v7/apis/aspnetcore/reference/>`_.
* ShowAuthorizationPII: This is a flag to indicate whether or not personally identifiable information is shown in logs.
* AccessTokenScopeReplace: With this optional setting you tell Firely Server which character replaces the ``/`` (forward slash) character in a SMART scope. This setting is needed in cases like working with Azure Active Directory (see details in section :ref:`feature_accesscontrol_aad`). 
* SmartCapabilities: This setting can be used to configure `SMART capabilities <http://hl7.org/fhir/smart-app-launch/conformance.html#smart-on-fhir-oauth-authorization-endpoints-and-capabilities>`_. All capabilities listed here are supported by Firely Server, you can enable/disable specific capabilities based on your authorization server implementation. 

.. note:: 
  After properly configuring Firely Server to work with an OAuth2 authorization server, enabling SMART and configuring the SmartCapabilities for Firely Server, you are able to discover the SMART configuration metadata by retrieving ``<base-url>/.well-known/smart-configuration``. 
  
  Please check section `Retrieve .well-known/smart-configuration <https://build.fhir.org/ig/HL7/smart-app-launch/app-launch.html#retrieve-well-knownsmart-configuration>`_  in the SMART specification for more details on how to request the metadata and how to interpret the response.

.. warning::

  #. In Firely Server version 5.11.0 and later versions ``vread`` and ``_history`` searches will be disabled when SMART on FHIR is enabled as the authorization cannot be enforced on historic resource instances.
  #. Before version 6.0, Firely Server allowed configuring other compartments than Patient in the SmartOptions. This is no longer supported. If you have configured this, you will need to adjust the configuration to only specify a filter on the Patient compartment.  

.. _feature_accesscontrol_anonymous:

Anonymous Access Configuration
------------------------------

Firely Server supports anonymous access to specific FHIR resources when properly configured. This feature allows limited access to non-sensitive resources without requiring authentication tokens.

.. note::
   Anonymous access should be carefully considered from a security perspective. Only enable this feature if you need to provide public access to specific, non-sensitive FHIR resources.

Configuration
^^^^^^^^^^^^^

To enable anonymous access, configure the following settings in your ``SmartAuthorizationOptions``::

    "SmartAuthorizationOptions": {
      // ...
      "EnableAnonymousAccess": true,
      "AnonymousScopes": "user/Organization.rs user/Location.rs user/Practitioner.r"
      // ...
    }

Security Restrictions
^^^^^^^^^^^^^^^^^^^^^

Anonymous access is subject to several important security restrictions:

* **User scopes only**: Anonymous scopes must use the ``user/`` prefix. ``patient/`` and ``system/`` scopes are not permitted for anonymous access.
* **No wildcard access**: Wildcard scopes like ``user/*`` are not allowed to prevent unrestricted access.
* **Patient compartment restriction**: Resources that belong to the Patient compartment (such as Patient, Observation, Condition, etc.) cannot be accessed anonymously.
* **No sensitive data**: Only resources that do not contain patient-specific or sensitive information should be made available for anonymous access.

Valid Anonymous Scope Examples
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

The following are examples of valid anonymous scopes::

    // Allow reading and searching Organizations
    "AnonymousScopes": "user/Organization.rs"
    
    // Allow reading and searching multiple resource types
    "AnonymousScopes": "user/Organization.rs user/Location.rs user/Practitioner.r"
    
    // Allow specific operations with search parameters (SMART v2)
    "AnonymousScopes": "user/Organization.rs?type=prov user/Location.cruds?status=active"

Invalid Anonymous Scope Examples
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

The following scope configurations will result in validation errors::

    // Invalid: Patient compartment resource
    "AnonymousScopes": "user/Patient.r"
    
    // Invalid: Patient compartment resource
    "AnonymousScopes": "user/Observation.rs"
    
    // Invalid: Wildcard access
    "AnonymousScopes": "user/*.r"
    
    // Invalid: Patient scope
    "AnonymousScopes": "patient/Observation.r"
    
    // Invalid: System scope  
    "AnonymousScopes": "system/Organization.r"

Configuration Validation
^^^^^^^^^^^^^^^^^^^^^^^^

Firely Server automatically validates the anonymous access configuration on startup. If invalid scopes are configured, you will see validation errors in the server logs and the server will fail to start.

Common validation error messages include:

* "Anonymous access scopes must only include user/... scopes."
* "Anonymous access scopes must not include all resource types (i.e., user/*)."  
* "Anonymous access scopes must not include resources from the Patient compartment: [resource types]."
* "Anonymous access is enabled but no scopes are specified for anonymous access."

Behavior
^^^^^^^^

When anonymous access is enabled and a request is made without an authorization token:

1. Firely Server checks if the requested operation matches any of the configured anonymous scopes
2. If a matching scope is found, the request is processed with the permissions defined by that scope
3. If no matching scope is found, the request is denied with a 403 Forbidden response
4. If an invalid or expired token is provided, the request is denied with a 401 Unauthorized response (anonymous access only applies when no token is provided)

Other forms of Authorization
----------------------------

In :ref:`accesscontrol_api` you can find the interfaces relevant to authorization in Firely Server.  
If your environment requires other authorization information than the standard SMART on FHIR claims, you can create your own implementations of these interfaces.
You do this by implementing a :ref:`custom plugin <vonk_plugins>`. 
All the standard plugins of Firely Server can then use that implementation to enforce access control.

.. _SMART App Authorization Guide: http://docs.smarthealthit.org/authorization/
.. _Patient CompartmentDefinition: http://www.hl7.org/implement/standards/fhir/compartmentdefinition-patient.html
.. _ASP.NET Core Identity: https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity