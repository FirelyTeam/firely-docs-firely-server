.. _feature_accesscontrol_config:

SMART on FHIR Configuration
===========================

.. note::

  The features described on this page are available in **all** :ref:`Firely Server editions <vonk_overview>`.

Firely Server fully supports the syntax of SMART v1: ``( 'patient' | 'user' ) '/' ( fhir-resource | '*' ) '.' ( 'read' | 'write' | '*' )``

These are examples of SMART v1 scopes, which are all supported by Firely Server:

* scope=user/Observation.read: the user is allowed to read Observation resources
* scope=user/Encounter.write: the user is allowed to write Encounter resources
* scope=user/*.read: the user is allowed to read any type of resource
* scope=user/*.write: the user is allowed to write any type of resource
* scope=[array of individual scopes]

Additionally, the syntax of SMART v2 scopes is fully supported: ``( 'patient' | 'user' | 'system' ) '/' ( fhir-resource | '*' ) '.' ( 'c' | 'r' | 'u' | 'd' | 's' | '*') ? param = value``

All SMART v1 scopes can also be expressed in SMART v2:

* scope=user/Observation.r: the user allows to read Observation resources
* scope=user/Encounter.cu: the user allows to write (create and update) Encounter resources
* scope=user/\*.r: the user allows to read any type of resource
* scope=user/\*.cu: the user allows to write (create and update) any type of resource
* scope=[array of individual scopes]

All search capabilities supported by Firely Server can also be evaluated as part of the access scope using SMART v2. 

* scope=user/Observation.r?category=laboratory: the user is allowed to read Observation resources with a category element containing the code "laboratory"
* scope=user/\*.rs?_tag=http://example.org/fhir/sid/codes|some-tag: the user is allowed to read and search all resource containing a tag in Meta.tag with system "http://example.org/fhir/sid/codes" and code "some-tag"
* scope=user/Observation.rs?encounter.id=Encounter/test: the user is allowed to see all Observation resources linked to the Encounter with id "test".
* scope=user/Observation.rs?encounter.identifier=http://my-hospital/ids|3456789: the user is allowed to see all Observation resources linked to the Encounter with identifier "345678".

.. note::
    As you can see from the last example, Chaining (and Reverse Chaining, ``_has``) is supported. Note though that using a chained search as part of authorization can make search queries significantly more complex and less performant.

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

You can control the way Access Control based on SMART on FHIR behaves with the SmartAuthorizationOptions in the :ref:`configure_appsettings`::

    "SmartAuthorizationOptions": {
      "Enabled": true,
      "PatientFilter": "identifier=#patient#", //Filter on a Patient compartment if a 'patient' launch scope is in the auth token, for the Patient that has an identifier matching the value of that 'patient' launch scope
      "Authority": "https://example.org/base-url-to-your-identity-provider",
    //"AdditionalBaseEndpointsInDiscoveryDocument": ["additional-url-to-your-identity-provider"],
    //"AdditionalIssuersInToken": ["additional-url-to-your-identity-provider"],   
    //"Audience": "https://example.org/base-url-of-firely-server", //Has to match the value the Authority provides in the audience claim.
    //"ClaimsNamespace": "http://smarthealthit.org",
      "RequireHttpsToProvider": false, //You want this set to true (the default) in a production environment!
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

* Enabled: With this setting you can disable ('false') the authentication and authorization altogether. When it is enabled ('true'), Firely Server will also evaluate the other settings. The default value is 'false'. This implies that authorization is disabled as if no SmartAuthorizationOptions section is present in the settings.
* PatientFilter: Defines how the ``patient`` launch context is translated to a search argument. See :ref:`feature_accesscontrol_compartment` for more background. You can use any supported search parameter defined on Patient. It should contain ``#patient#``, which is substituted by the value of the ``patient`` claim.
* Authority: The base url of your identity provider, such that ``{{base_url}}/.well-known/openid-configuration`` returns a valid configuration response (`OpenID Connect Discovery documentation <https://openid.net/specs/openid-connect-discovery-1_0.html#rfc.section.4.2>`_). At minimum, the ``jwks_uri``, ``token_endpoint`` and ``authorization_endpoint`` keys are required in addition to the keys required by the specification. See :ref:`Firely Auth<feature_accesscontrol_idprovider>` for more background.
* AdditionalBaseEndpointsInDiscoveryDocument: Optional configuration setting. Add additional base authority endpoints that your identity provider also uses for operations that are listed in the .well-known document. 
* AdditionalIssuersInToken: Optional configuration setting. The additional issuer setting will extend the list of issuer urls that are valid within the issuer claim in the token passed to Firely Server. The token validation will be adjusted accordingly. Please note that it does not influence which issuer urls are allowed in the .well-known/openid-configuration document of the authorization server.
* Audience: Defines the name of this Firely Server instance as it is known to the Authorization server. Default is the base url of Firely Server.
* ClaimsNamespace: Some authorization providers will prefix all their claims with a namespace, e.g. ``http://my.company.org/auth/user/*.read``. Configure the namespace here to make Firely Server interpret it correctly. It will then strip off that prefix and interpret it as just ``user/*.read``. By default no namespace is configured.
* RequireHttpsToProvider: Token exchange with an Authorization server should always happen over https. However, in a local testing scenario you may need to use http. Then you can set this to 'false'. The default value is 'true'. 
* Protected: This setting controls which of the interactions actually require authentication. In the example values provided here, $validate is not in the TypeLevelInteractions. This means that you can use POST [base-url]/Patient/$validate without authorization. Since you only read Conformance resources with this interaction, this might make sense.
* TokenIntrospection: This setting is configurable when you use `reference tokens <https://docs.duendesoftware.com/identityserver/v5/apis/aspnetcore/reference/>`_.
* ShowAuthorizationPII: This is a flag to indicate whether or not personally identifiable information is shown in logs.
* AccessTokenScopeReplace: With this optional setting you tell Firely Server which character replaces the ``/`` (forward slash) character in a SMART scope. This setting is needed in cases like working with Azure Active Directory (see details in section :ref:`feature_accesscontrol_aad`). 
* SmartCapabilities: This setting can be used to configure `SMART capabilities <http://hl7.org/fhir/smart-app-launch/conformance.html#smart-on-fhir-oauth-authorization-endpoints-and-capabilities>`_. All capabilities listed here are supported by Firely Server, you can enable/disable specific capabilities based on your authorization server implementation. 

.. note:: 
  After properly configuring Firely Server to work with an OAuth2 authorization server, enabling SMART and configuring the SmartCapabilities for Firely Server, you are able to discover the SMART configuration metadata by retrieving ``<base-url>/.well-known/smart-configuration``. 
  
  Please check section `Retrieve .well-known/smart-configuration <https://build.fhir.org/ig/HL7/smart-app-launch/app-launch.html#retrieve-well-knownsmart-configuration>`_  in the SMART specification for more details on how to request the metadata and how to interpret the response.
