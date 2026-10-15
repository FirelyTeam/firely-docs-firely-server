.. _feature_accesscontrol_config:

Enforcing access control
========================

.. warning::

  #. From Firely Server version 5.11.0 to Firely Server 6.9.0 ``vread`` and ``_history`` searches are disabled when SMART on FHIR is enabled as the authorization cannot be enforced on historic resource instances in combination with compartment restrictions. From Firely Server version 6.9.1 and later, this restriction has been lifted with the introduction of the ``AllowHistory`` setting in the ``SmartAuthorizationOptions`` section of the configuration.
  #. Before version 6.0, Firely Server allowed configuring other compartments than Patient in the SmartOptions. This is no longer supported. If you have configured this, you will need to adjust the configuration to only specify a filter on the Patient compartment.  


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
      "AllowHistory": false, //When true, history and vread requests are allowed. Authorization is then evaluated against the current version of the resources, as historical versions are not indexed.
      "PatientFilter": "identifier=#patient#", //Filter on a Patient compartment if a 'patient' launch scope is in the auth token, for the Patient that has an identifier matching the value of that 'patient' launch scope
      "Authority": "https://example.org/base-url-to-your-identity-provider",
    //"AdditionalBaseEndpointsInDiscoveryDocument": ["additional-url-to-your-identity-provider"],
    //"AdditionalIssuersInToken": ["additional-url-to-your-identity-provider"],   
    //"Audience": "https://example.org/base-url-of-firely-server", //Has to match the value the Authority provides in the audience claim. If not set, the audience of the token is not validated.
    //"ClaimsNamespace": "http://smarthealthit.org",
      "RequireHttpsToProvider": false, //You want this set to true (the default) in a production environment!
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
* Audience: Defines the name of this Firely Server instance as it is known to the Authorization server. The default should be the base url of Firely Server. Firely Server only accepts tokens whose ``aud`` claim contains this value.

  .. warning::

     If ``Audience`` is not set or empty, Firely Server does not validate the ``aud`` claim at all, and accepts a token from the ``Authority`` that was issued for any audience, for example for another API. Always set ``Audience`` in a production environment.

Additional advanced configuration can be achieved through the following settings:

* EnforceAccessPolicies: Global flag that controls whether ``AccessPolicies`` are enforced for all matching ``fhirUsers``. See :ref:`feature_accesscontrol_permissions` for more details.
* AllowHistory: When true, history and vread requests are allowed. The default is ``false``. Authorization is then evaluated against the current version of the resources, as historical versions are not indexed. See :ref:`smart_history` for the restrictions that apply.
* ClockSkew: Allow potential time discrepancies between the authorization server and the FHIR server, allowing for a small tolerance window when checking token expiration and validity times. Defaults to 5 minutes.
* AdditionalBaseEndpointsInDiscoveryDocument: Optional configuration setting. Add additional base authority endpoints that your identity provider also uses for operations that are listed in the .well-known document. 
* AdditionalIssuersInToken: Optional configuration setting. The additional issuer setting will extend the list of issuer urls that are valid within the issuer claim in the token passed to Firely Server. The token validation will be adjusted accordingly. Please note that it does not influence which issuer urls are allowed in the .well-known/openid-configuration document of the authorization server.
* ClaimsNamespace: Some authorization providers will prefix all their claims with a namespace, e.g. ``http://my.company.org/auth/user/*.read``. Configure the namespace here to make Firely Server interpret it correctly. It will then strip off that prefix and interpret it as just ``user/*.read``. By default no namespace is configured.
* RequireHttpsToProvider: Token exchange with an Authorization server should always happen over https. However, in a local testing scenario you may need to use http. Then you can set this to 'false'. The default value is 'true'. 

Which interactions and operations require authorization is not configured in ``SmartAuthorizationOptions``, but per operation with ``RequireAuthorization`` in the ``Operations`` section (``WhenAuthEnabled``, ``Always`` or ``Never``), see :ref:`disable_interactions`. For example, to allow ``POST [base-url]/Patient/$validate`` without authorization, set ``RequireAuthorization`` of ``$validate`` to ``Never``.

.. note::

   The ``Protected`` setting in ``SmartAuthorizationOptions`` of Firely Server versions before 6.0 is no longer used. Firely Server ignores it, so interactions that you left out of ``Protected`` do require authorization unless their ``RequireAuthorization`` is ``Never``.

* TokenIntrospection: This setting is configurable when you use `reference tokens <https://docs.duendesoftware.com/identityserver/v7/apis/aspnetcore/reference/>`_.
* ShowAuthorizationPII: This is a flag to indicate whether or not personally identifiable information is shown in logs.
* AccessTokenScopeReplace: With this optional setting you tell Firely Server which character replaces the ``/`` (forward slash) character in a SMART scope. This setting is needed in cases like working with Azure Active Directory (see details in section :ref:`feature_accesscontrol_aad`). 
* SmartCapabilities: This setting can be used to configure `SMART capabilities <http://hl7.org/fhir/smart-app-launch/conformance.html#smart-on-fhir-oauth-authorization-endpoints-and-capabilities>`_. All capabilities listed here are supported by Firely Server, you can enable/disable specific capabilities based on your authorization server implementation. 

.. note:: 
  After properly configuring Firely Server to work with an OAuth2 authorization server, enabling SMART and configuring the SmartCapabilities for Firely Server, you are able to discover the SMART configuration metadata by retrieving ``<base-url>/.well-known/smart-configuration``. 
  
  Please check section `Retrieve .well-known/smart-configuration <https://build.fhir.org/ig/HL7/smart-app-launch/app-launch.html#retrieve-well-knownsmart-configuration>`_  in the SMART specification for more details on how to request the metadata and how to interpret the response.

.. _smart_history:

History and version reads
-------------------------

Firely Server indexes only the current version of a resource. Scopes, launch contexts and :ref:`AccessPolicies <feature_accesscontrol_permissions>` can therefore only be evaluated against the current version, not against the historical versions.
For that reason, history is disabled by default when SMART on FHIR is enabled, and can be enabled with ``SmartAuthorizationOptions:AllowHistory``.

With ``AllowHistory`` set to ``false`` (the default):

* A request that is subject to SMART on FHIR authorization is rejected with ``HTTP 403 Forbidden`` when it asks for:

  * ``_history`` on the system, type or instance level;
  * a version read (``vread``), for example ``GET <base>/Patient/123/_history/2``;
  * a custom operation on a specific version of a resource.

* ``_include`` of a versioned reference only includes the referenced resource if the referenced version is its current version.
* Requests on the :ref:`administration_api`, and operations that do not require authorization (``RequireAuthorization`` set to ``Never`` in :ref:`disable_interactions`), are not subject to SMART on FHIR authorization, so history remains available there.

With ``AllowHistory`` set to ``true``, these requests are allowed, with the following restrictions:

* **Access is decided on the current version.** A client that may read the current version of a resource may read all its earlier versions, even versions that would not have matched its scopes or launch context. For example, if an Observation was linked to another patient before it was corrected, a client in the launch context of the current patient can also read the earlier versions. The other way around, if a client may not read the current version, it cannot read any of the earlier versions either.
* **Deleted resources.** A deleted resource has no current version to evaluate the authorization against. If the access of the client is restricted to specific resources, Firely Server returns ``HTTP 404 Not Found`` on a version read of a deleted resource, and does not return the history of deleted resources. Access counts as restricted to specific resources when:

  * the token has a ``patient`` launch context, so the request is limited to the Patient compartment;
  * a SMART v2 scope carries search parameters, for example ``user/Observation.rs?category=laboratory``;
  * the client is authorized with SMART v1 scopes or by a custom authorization plugin. Firely Server supports both, but it cannot tell from them whether the access is limited to specific resources, so for deleted resources it treats them as restricted.

  A client whose access is not restricted to specific resources, for example with ``system/Observation.rs``, gets the standard FHIR behavior: a version read of the deletion returns ``HTTP 410 Gone``.
* **History on the type and system level** is assembled for at most ``BundleOptions:MaxCount`` resources that the client may read (see :ref:`bundle_options`). The history of further resources is not included, and Firely Server logs a warning when this happens.

Only enable ``AllowHistory`` if it is acceptable that access to historical versions is decided based on the current version of the resources.

.. note::

   With :ref:`virtual multi-tenancy <feature_multitenancy>` enabled, history and ``vread`` are disabled regardless of this setting.

Other forms of Authorization
----------------------------

In :ref:`accesscontrol_api` you can find the interfaces relevant to authorization in Firely Server.  
If your environment requires other authorization information than the standard SMART on FHIR claims, you can create your own implementations of these interfaces.
You do this by implementing a :ref:`custom plugin <vonk_plugins>`. 
All the standard plugins of Firely Server can then use that implementation to enforce access control.

.. _SMART App Authorization Guide: http://docs.smarthealthit.org/authorization/
.. _Patient CompartmentDefinition: http://www.hl7.org/implement/standards/fhir/compartmentdefinition-patient.html
.. _ASP.NET Core Identity: https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity