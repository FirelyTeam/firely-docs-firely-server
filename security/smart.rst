.. _feature_accesscontrol_config:

SMART on FHIR Configuration
===========================

Firely Server fully supports the syntax of SMART v1: ``( 'patient' | 'user' ) '/' ( fhir-resource | '*' ) '.' ( 'read' | 'write' | '*' )``

Additionally, the syntax of SMART v2 scopes is fully supported: ``( 'patient' | 'user' | 'system' ) '/' ( fhir-resource | '*' ) '.' ( 'c' | 'r' | 'u' | 'd' | 's' | '*') ? param = value``

All search capabilities supported by Firely Server can also be evaluated as part of the access scope using SMART v2. Chaining and Reverse Chaining is explicitly supported here:

* scope=user/Observation.r?category=laboratory: the user is allowed to read Observation resources with a category element containing the code "laboratory"
* scope=user/\*.rs?_tag=http://example.org/fhir/sid/codes|some-tag: the user is allowed to read and search all resource containing a tag in Meta.tag with system "http://example.org/fhir/sid/codes" and code "some-tag"
* scope=user/Observation.rs?encounter.id=Encounter/test: the user is allowed to see all Observation resources linked to the Encounter with id "test".

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

Add ``Vonk.Smart`` (for SMART v1) or ``Vonk.Plugin.SoFv2`` (for SMART v2) to the list of included plugins. When you restart Firely Server, the Smart service will be added to the pipeline.
An error will be thrown if both plugins are part of the pipeline. Please note that the SMART v2 plugin will allow the usage of the SMART v1 and SMART v2 syntax.

You can control the way Access Control based on SMART on FHIR behaves with the SmartAuthorizationOptions in the :ref:`configure_appsettings`::

    "SmartAuthorizationOptions": {
      "Enabled": true,
      "Filters": [
        {
          "FilterType": "Patient", //Filter on a Patient compartment if a 'patient' launch scope is in the auth token
          "FilterArgument": "identifier=#patient#" //... for the Patient that has an identifier matching the value of that 'patient' launch scope
        }
        //{
        //  "FilterType": "Encounter", //Filter on an Encounter compartment if an 'encounter' launch scope is in the auth token
        //  "FilterArgument": "identifier=#encounter#" //... for the Encounter that has an identifier matching the value of that 'encounter' launch scope
        //},
        //{
        //  "FilterType": "RelatedPerson", //Filter on a RelatedPerson compartment if a 'relatedperson' launch scope is in the auth token
        //  "FilterArgument": "identifier=#relatedperson#" //... for the RelatedPerson that has an identifier matching the value of that 'relatedperson' launch scope
        //},
        //{
        //  "FilterType": "Practitioner", //Filter on a Practitioner compartment if a 'practitioner' launch scope is in the auth token
        //  "FilterArgument": "identifier=#practitioner#" //... for the Practitioner that has an identifier matching the value of that 'practitioner' launch scope
        //},
        //{
        //  "FilterType": "Device", //Filter on a Device compartment if a 'device' launch scope is in the auth token
        //  "FilterArgument": "identifier=#device#" //... for the Device that has an identifier matching the value of that 'device' launch scope
        //}
      ],
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
* Filters: Defines how different launch contexts are translated to search arguments. See :ref:`feature_accesscontrol_compartment` for more background.

    * FilterType: Both a launch context and a CompartmentDefinition are defined by a resourcetype. Use FilterType to define for which launch context and related CompartmentDefinition this Filter is applicable.
    * FilterArgument: Translates the value of the launch context to a search argument. You can use any supported search parameter defined on FilterType. It should contain the name of the launch context enclosed in hashes (e.g. #patient#), which is substituted by the value of the claim.
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
