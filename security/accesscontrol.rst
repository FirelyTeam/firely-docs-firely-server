.. _feature_accesscontrol:

Access control and SMART
========================

.. contents:: Contents
  :depth: 1
  :local:

.. _feature_accesscontrol_concepts:

Concepts
--------
This explanation of access control and SMART in Firely Server requires basic understanding of the :ref:`architecture <architecture>` of Firely Server, so you know what is meant by middleware components and repository interfaces.
It also presumes general knowledge about authentication and OAuth2.

Access control generally consists of the following parts, which will be addressed one by one:

- Identification: Who are you? -- usually a user name, login, or some identifier.
- Authentication: Prove your identification -- usually with a password, a certificate or some other (combination of) secret(s) owned by you.
- Authorization: What are you allowed to read or change based on your identification?
- Access Control Engine: Enforce the authorization in the context of a specific request.

Identification and Authentication
---------------------------------
Firely Server does not authenticate users itself. It is meant to be used in an `OAuth2`_ environment in which an `OAuth2 provider`_ is responsible for the identification and authentication of users. 
Typically, a user first enters a Web Application, e.g. a patient portal, or a mobile app. That application interactively redirects the user to the OAuth2 provider - which is the authorization server and may or may not handle authentication as well - to authenticate, and receives an OAuth2 token back.
Then, the application can do an http request to Firely Server to send or receive resource(s), and provide the OAuth2 token in the http Authentication header, thereby acting on behalf of the user.
Firely Server can then read the OAuth2 token and validate it with the OAuth2 authorization server. This functionality is not FHIR specific.

.. _feature_accesscontrol_authorization:

Authorization
-------------
Authorization in Firely Server by default is based on `SMART on FHIR`_ and more specifically the `Scopes and Launch Context`_ defined by it. SMART specifies several claims that can be present in the OAuth2 token and their meaning. These are examples of scopes and launch contexts that are recognized by Firely Server:

* scope=user/Observation.read: the user is allowed to read Observation resources
* scope=user/Encounter.write: the user is allowed to write Encounter resources
* scope=user/\*.read: the user is allowed to read any type of resource
* scope=user/\*.write: the user is allowed to write any type of resource
* scope=[array of individual scopes]
* patient=123: the user is allowed access to resources in the compartment of patient 123 -- see :ref:`feature_accesscontrol_compartment`.

SMART on FHIR also defines scopes starting with 'patient/' instead of 'user/'. In Firely Server these are evaluated equally. But with a scope of 'patient/' you are required to also have a 'patient=...' launch context to know to which patient the user connects. It is also possible to apply a launch context to a user scope, for example the scope can look like "launch user/\*.read". In your authorization server you can specify the resources that are in the launch context parameter.

The assignment of these claims to users, systems or groups is managed in the OAuth2 authorization server and not in Firely Server. Firely Server does, however, need a way to access these scopes - so if your OAuth server is issuing a self-encoded token, ensure that it has a ``scope`` field with all of the granted scopes inside it.

Access Control Engine
---------------------
The Access Control Engine in Firely Server evaluates two types of authorization:

#. Type-Access: Is the user allowed to read or write resource(s) of a specific resourcetype?
#. Compartment: Is the data to be read or written within the current compartment (if any)?

As you may have noticed, Type-Access aligns with the concept of scopes, and Compartment aligns with the concept of launch context in SMART on FHIR.

The Firely Server ``SmartContextMiddleware`` component extracts the claims defined by SMART on FHIR from the OAuth2 token, and puts it into two classes that are then available for Access Control Decisions in all the interaction handlers (e.g. for read, search, create etc.)

SMART on FHIR defines launch contexts for Patient, Encounter and Location, extendible with others if needed. 
If a request is done with a Patient launch context, and the user is allowed to see other resource types as well, these other resource types are restricted by the `Patient CompartmentDefinition`_.

.. _accesscontrol_custom_authentication:

Custom Authentication
---------------------
You may build a plugin with custom middleware to provide authentication in a form that suits your needs. 
One example could be that you want to integrate `ASP.NET Core Identity`_ into Firely Server.  
Then you don't need the OAuth2 middleware, but instead can use the Identity framework to authenticate your users.
See :ref:`vonk_plugins_customauthorization` for more details.

Other forms of Authorization
----------------------------
In :ref:`accesscontrol_api` you can find the interfaces relevant to authorization in Firely Server.  
If your environment requires other authorization information than the standard SMART on FHIR claims, you can create your own implementations of these interfaces.
You do this by implementing a :ref:`custom plugin <vonk_plugins>`. 
All the standard plugins of Firely Server can then use that implementation to enforce access control. 

.. _feature_accesscontrol_config:

Configuration
-------------
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

Add ``Vonk.Smart`` to the list of included plugins. When you restart Firely Server, the Smart service will be added to the pipeline.

You can control the way Access Control based on SMART on FHIR behaves with the SmartAuthorizationOptions in the :ref:`configure_appsettings`::

    "SmartAuthorizationOptions": {
      "Enabled": true,
      "Filters": [
        {
          "FilterType": "Patient", //Filter on a Patient compartment if a 'patient' launch scope is in the auth token
          "FilterArgument": "identifier=#patient#" //... for the Patient that has an identifier matching the value of that 'patient' launch scope
        },
        {
          "FilterType": "Encounter", //Filter on an Encounter compartment if an 'encounter' launch scope is in the auth token
          "FilterArgument": "identifier=#encounter#" //... for the Encounter that has an identifier matching the value of that 'encounter' launch scope
        },
        {
          "FilterType": "RelatedPerson", //Filter on a RelatedPerson compartment if a 'relatedperson' launch scope is in the auth token
          "FilterArgument": "identifier=#relatedperson#" //... for the RelatedPerson that has an identifier matching the value of that 'relatedperson' launch scope
        },
        {
          "FilterType": "Practitioner", //Filter on a Practitioner compartment if a 'practitioner' launch scope is in the auth token
          "FilterArgument": "identifier=#practitioner#" //... for the Practitioner that has an identifier matching the value of that 'practitioner' launch scope
        },
        {
          "FilterType": "Device", //Filter on a Device compartment if a 'device' launch scope is in the auth token
          "FilterArgument": "identifier=#device#" //... for the Device that has an identifier matching the value of that 'device' launch scope
        }
      ],
      "Authority": "url-to-your-identity-provider",
    //"AdditionalEndpoints": {  //optional, only needed for certain identity provider setups
    //   "Issuers": ["additonal-url-to-your-identity-provider"],
    //   "BaseEndpoints" : ["additonal-url-to-your-identity-provider"]
    //},      
      "Audience": "name-of-your-fhir-server" //Default this is empty
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
    //"AccessTokenScopeReplace": "-"
    }

* Enabled: With this setting you can disable ('false') the authentication and authorization altogether. When it is enabled ('true'), Firely Server will also evaluate the other settings. The default value is 'false'. This implies that authorization is disabled as if no SmartAuthorizationOptions section is present in the settings.
* Filters: Defines how different launch contexts are translated to search arguments. See :ref:`feature_accesscontrol_compartment` for more background.

    * FilterType: Both a launch context and a CompartmentDefinition are defined by a resourcetype. Use FilterType to define for which launch context and related CompartmentDefinition this Filter is applicable.
    * FilterArgument: Translates the value of the launch context to a search argument. You can use any supported search parameter defined on FilterType. It should contain the name of the launch context enclosed in hashes (e.g. #patient#), which is substituted by the value of the claim.
* Authority: The base url of your identity provider, such that ``{{base_url}}/.well-known/openid-configuration`` returns a valid configuration response (`OpenID Connect Discovery documentation <https://openid.net/specs/openid-connect-discovery-1_0.html#rfc.section.4.2>`_). At minimum, the ``jwks_uri``, ``token_endpoint`` and ``authorization_endpoint`` keys are required in addition to the keys required by the speficiation. See :ref:`feature_accesscontrol_idprovider` for more background.
* AdditionalEndpoints: (FS v4.7.0) Optional configuration setting. Add additional issuers and/or base authority endpoints that your identity provider also uses for operations that are listed in the .well-known document. 
* Audience: Defines the name of this Firely Server instance as it is known to the Authorization server. Default is 'firelyserver'.
* ClaimsNamespace: Some authorization providers will prefix all their claims with a namespace, e.g. ``http://my.company.org/auth/user/*.read``. Configure the namespace here to make Firely Server interpret it correctly. It will then strip off that prefix and interpret it as just ``user/*.read``. By default no namespace is configured.
* RequireHttpsToProvider: Token exchange with an Authorization server should always happen over https. However, in a local testing scenario you may need to use http. Then you can set this to 'false'. The default value is 'true'. 
* Protected: This setting controls which of the interactions actually require authentication. In the example values provided here, $validate is not in the TypeLevelInteractions. This means that you can use POST [base-url]/Patient/$validate without authorization. Since you only read Conformance resources with this interaction, this might make sense.
* TokenIntrospection: This setting is configurable when you use `reference tokens <https://docs.duendesoftware.com/identityserver/v5/apis/aspnetcore/reference/>`_.
* ShowAuthorizationPII: This is a flag to indicate whether or not PII is shown in logs.
* AccessTokenScopeReplace: (FS v4.7.0) With this optional setting you tell Firely Server which character replaces the ``/`` (forward slash) character in a SMART scope. This setting is needed in cases like working with Azure Active Directory (see details in section :ref:`feature_accesscontrol_aad`). 

.. _feature_accesscontrol_compartment:

Compartments
------------

In FHIR a `CompartmentDefinition <http://www.hl7.org/implement/standards/fhir/compartmentdefinition.html>`_ defines a set of resources 'around' a focus resource. For each type of resource that is linked to the focus resource, it defines the reference search parameters that connect the two together. The type of the focus-resource is in CompartmentDefinition.code, and the relations are in CompartmentDefinition.resource. The values for param in it can be read as a `reverse chain <http://www.hl7.org/implement/standards/fhir/search.html#has>`_.

An example is the `Patient CompartmentDefinition`_, where a Patient resource is the focus. One of the related resourcetypes is Observation. Its params are subject and performer, so it is in the compartment of a specific Patient if that Patient is either the subject or the performer of the Observation.

FHIR defines CompartmentDefinitions for Patient, Encounter, RelatedPerson, Practitioner and Device. Although Firely Server is functionally not limited to these five, the specification does not allow you to define your own. Firely Server will use a CompartmentDefinition if:

* the CompartmentDefinition is known to Firely Server, see :ref:`conformance` for options to provide them.
* the OAuth2 Token contains a claim with the same name as the CompartmentDefinition.code (but it must be lowercase).

So some of the launch contexts mentioned in SMART on FHIR map to CompartmentDefinitions. For example, the launch context 'launch/patient' and 'launch/encounter' map to the compartment 'Patient' and 'Encounter'. Please note that launch contexts can be extended for any resource type, but not all resource types have a matching CompartmentDefinition, e.g. 'Location'.

A CompartmentDefinition defines the relationships, but it becomes useful once you combine it with a way of specifying the actual focus resource. In SMART on FHIR, the launch context can do that, e.g. patient=123. As per the SMART `Scopes and Launch Context`_, the value '123' is the value of the Patient.id. Together with the Patient CompartmentDefinition this defines a -- what we call -- Compartment in Firely Server:

* Patient with id '123'
* And all resources that link to that patient according to the Patient CompartmentDefinition.

There may be cases where the logical id of the focus resource is not known to the authorization server. Let's assume it does know one of the Identifiers of a Patient. The Filters in the :ref:`feature_accesscontrol_config` allow you to configure Firely Server to use the identifier search parameter as a filter instead of _id. The value in the configuration example does exactly that::

    "Filters": [
      {
        "FilterType": "Patient", //Filter on a Patient compartment if a 'patient' launch scope is in the auth token
        "FilterArgument": "identifier=#patient#" //... for the Patient that has an identifier matching the value of that 'patient' launch scope
      },
      ...
    ]

Please notice that it is possible that more than one Patient matches the filter. This is intended behaviour of Firely Server, and it is up to you to configure a search parameter that is guaranteed to have unique values for each Patient if you need that. You can always stick to the SMART on FHIR default of _id by specifying that as the filter::

    "Filters": [
      {
        "FilterType": "Patient", //Filter on a Patient compartment if a 'patient' launch scope is in the auth token
        "FilterArgument": "_id=#patient#" //... for the Patient that has an identifier matching the value of that 'patient' launch scope
      },
      ...
    ]

But you can also take advantage of it and allow access only to the patients from a certain General Practitioner, of whom you happen to know the Identifier::

    "Filters": [
      {
        "FilterType": "Patient", //Filter on a Patient compartment if a 'patient' launch scope is in the auth token
        "FilterArgument": "general-practitioner.identifier=#patient#" //... for the Patient that has an identifier matching the value of that 'patient' launch scope
      },
      ...
    ]

In this example the claim is still called 'patient', although it contains an Identifier of a General Practitioner. This is because the CompartmentDefinition is selected by matching its code to the name of the claim, regardless of the value the claim contains. 

If multiple resources match the Compartment, that is no problem for Firely Server. You can simply configure the Filters according to the business rules in your organization.

Tokens
------

When a client application wants to access data in Firely Server on behalf of its user, it requests a token from the authorization server (configured as the Authority in the :ref:`feature_accesscontrol_config`). The configuration of the authorization server determines which claims are *available* for a certain user, and also for the client application. The client app configuration determines which claims it *needs*. During the token request, the user is usually redirected to the authorization server, which might or might not be the authentication server as well, logs in and is then asked whether the client app is allowed to receive the requested claims. The client app cannot request any claims that are not available to that application. And it will never get any claims that are not available to the user. This flow is also explained in the `SMART App Authorization Guide`_. 

The result of this flow should be a JSON Web Token (JWT) containing zero or more of the claims defined in SMART on FHIR. The claims can either be scopes or a launch context, as in the examples listed in :ref:`feature_accesscontrol_authorization`. This token is encoded as a string, and must be sent to Firely Server in the Authorization header of the request.

A valid access token for Firely Server at minimum will have:

* the ``iss`` claim with the base url of the OAuth server
* the ``aud`` the same value you've entered in ``SmartAuthorizationOptions.Audience``
* the ``scope`` field with the scopes granted by this access token
* optionally, the compartment claim, if you'd like to limit this token to a certain compartment. Such a claim may be requested by using a context scope or launching a part of an EHR launch. See `Requesting context with scopes <http://www.hl7.org/fhir/smart-app-launch/scopes-and-launch-context/#requesting-context-with-scopes>`_ for more details. For example, in case of Patient data access where the ``launch/patient`` scope is used, include the ``patient`` claim with the patient's id or identifier (:ref:`feature_accesscontrol_compartment`) and make sure to request the ``patient/<permissions>`` scope permissions. Compartment claims combined with ``user/<permissions>`` claims are not taken into acccount.

.. warning:: Firely Server will not enforce any access control for resources outside of the specified compartment. Some compartment definitions do not include crucial resource types like 'Patient' or their corresponding resource type, i.e. all resources of this type regardless of any claims in the access token will be returned if requested. Please use this feature with caution! Additional custom access control is highly recommended.

.. _feature_accesscontrol_aad:

Azure Active Directory
----------------------

Azure Active Directory (v2.0) does not allow to define a scope with ``/`` (forward slash) in it, which is not compatible with the structure of a `SMART on FHIR scope <http://hl7.org/fhir/smart-app-launch/1.0.0/scopes-and-launch-context/index.html>`_. 
Therefor when you use AAD to provide SMART on FHIR scopes to Firely Server, you need to take following steps

1. In a SMART scope, use another character (for instance ``-``) instead of ``/``. For example:

  * ``user-*.read``
  * ``user-*.write``
  * ``patient-Observation.r``
  
  If the used character (for instance ``-``) is already in your SMART scope, then you can use ``\`` (backward slash) to escape it.
  
  * ``patient-Observation.r?_id=Test\-With\-Dashes``

2. Tell Firely Server which character is used in Step 1, then Firely Server will generate a proper `SMART on FHIR scope <http://hl7.org/fhir/smart-app-launch/1.0.0/scopes-and-launch-context/index.html>`_ and handle the request further. This can be configured via setting ``AccessTokenScopeReplace``. 

For the first step above, instead of doing it manually, you can deploy `SMART on FHIR AAD Proxy <https://github.com/azure-smart-health/smart-on-fhir-aad-proxy>`_ to Azure, which helps you to replace ``/`` to ``-`` in a SMART scope.
The other option would be to follow `Quickstart: Deploy Azure API for FHIR using Azure portal <https://docs.microsoft.com/en-us/azure/healthcare-apis/azure-api-for-fhir/fhir-paas-portal-quickstart>`_, check "SMART on FHIR proxy" box and use the proxy by following `Tutorial: Azure Active Directory SMART on FHIR proxy <https://docs.microsoft.com/en-us/azure/healthcare-apis/azure-api-for-fhir/use-smart-on-fhir-proxy>`_.

.. warning:: 
  When you use the proxy, be careful with the `SMART on FHIR v2 scopes <http://hl7.org/fhir/smart-app-launch/STU2/scopes-and-launch-context.html>`_. Becasue ``-`` is allowed there (see examples below). 
  In those cases, the proxy simply replaces ``/`` with ``-`` and not able to escape the original ``-``, then Firely Server could not figure out which ``-`` is original, which makes the request failed.

  * ``patient/Observation.rs?category=http://terminology.hl7.org/CodeSystem/observation-category|laboratory``
  * ``Observation.rs?code:in=http://valueset.example.org/ValueSet/diabetes-codes`` 

.. _feature_accesscontrol_decisions:

Access Control Decisions
------------------------

In this paragraph we will explain how Access Control Decisions are made for the various FHIR interactions. For the examples assume a Patient Compartment with identifier=123 as filter.

#. Search

   a. Direct search on compartment type

      :Request: ``GET [base]/Patient?name=fred``
      :Type-Access: User must have read access to Patient, otherwise a 401 is returned. 
      :Compartment: If a Patient Compartment is active, the Filter from it will be added to the search, e.g. ``GET [base]/Patient?name=fred&identifier=123``

   #. Search on type related to compartment

      :Request: ``GET [base]/Observation?code=x89``
      :Type-Access: User must have read access to Observation, otherwise a 401 is returned. 
      :Compartment: If a Patient Compartment is active, the links from Observation to Patient will be added to the search. In pseudo code: ``GET [base]/Obervation?code=x89& (subject:Patient.identifier=123 OR performer:Patient.identifier=123)``

   #. Search on type not related to compartment

      :Request: ``GET [base]/Organization``
      :Type-Access: User must have read access to Organization, otherwise a 401 is returned. 
      :Compartment: No compartment is applicable to Organization, so no further filters are applied.

   #. Search with include outside the compartment

      :Request: ``GET [base]/Patient?_include=Patient:organization``
      :Type-Access: User must have read access to Patient, otherwise a 401 is returned. If the user has read access to Organization, the _include is evaluated. Otherwise it is ignored.
      :Compartment: Is applied as in case 1.a.

   #. Search with chaining

      :Request: ``GET [base]/Patient?general-practitioner.identifier=123``
      :Type-Access: User must have read access to Patient, otherwise a 401 is returned. If the user has read access to Practitioner, the search argument is evaluated. Otherwise it is ignored as if the argument was not supported. If the chain has more than one link, read access is evaluated for every link in the chain. 
      :Compartment: Is applied as in case 1.a.

   #. Search with chaining into the compartment

      :Request: ``GET [base]/Patient?link:Patient.identifier=456``
      :Type-Access: User must have read access to Patient, otherwise a 401 is returned.
      :Compartment: Is applied to both Patient and link. In pseudo code: ``GET [base]/Patient?link:(Patient.identifier=456&Patient.identifier=123)&identifier=123`` In this case there will probably be no results.

#. Read: Is evaluated as a Search, but implicitly you only specify the _type and _id search parameters.
#. VRead: If a user can Read the current version of the resource, he is allowed to get the requested version as well.
#. Create

   a. Create on the compartment type

      :Request: ``POST [base]/Patient``
      :Type-Access: User must have write access to Patient. Otherwise a 401 is returned.
      :Compartment: A Search is performed as if the new Patient were in the database, like in case 1.a. If it matches the compartment filter, the create is allowed. Otherwise a 401 is returned.

   #. Create on a type related to compartment

      :Request: ``POST [base]/Observation``
      :Type-Access: User must have write access to Observation. Otherwise a 401 is returned. User must also have read access to Patient, in order to evaluate the Compartment.
      :Compartment: A Search is performed as if the new Observation were in the database, like in case 1.b. If it matches the compartment filter, the create is allowed. Otherwise a 401 is returned.

   #. Create on a type not related to compartment

      :Request: ``POST [base]/Organization``
      :Type-Access: User must have write access to Organization. Otherwise a 401 is returned.
      :Compartment: Is not evaluated.

#. Update

   a. Update on the compartment type

      :Request: ``PUT [base]/Patient/123``
      :Type-Access: User must have write access *and* read access to Patient, otherwise a 401 is returned.
      :Compartment: User should be allowed to Read Patient/123 and Create the Patient provided in the body. Then Update is allowed.

   #. Update on a type related to compartment

      :Request: ``PUT [base]/Observation/xyz``
      :Type-Access: User must have write access to Observation, and read access to both Observation and Patient (the latter to evaluate the compartment)
      :Compartment: User should be allowed to Read Observation/123 and Create the Observation provided in the body. Then Update is allowed.

#. Delete: Allowed if the user can Read the current version of the resource, and has write access to the type of resource.
#. History: Allowed on the resources that the user is allowed to Read the current versions of (although it is theoretically possible that an older version would not match the compartment). 

Testing
-------

Testing the access control functionality is possible on a local instance of Firely Server. It is not available for the `publicly hosted test server <http://server.fire.ly>`_.

You can test it using a dummy authorization server and Postman as a REST client. Please refer to these pages for instructions:

* :ref:`feature_accesscontrol_idprovider`
* :ref:`feature_accesscontrol_postman`

.. _OAuth2: https://oauth.net/2/
.. _OAuth2 provider: https://en.wikipedia.org/wiki/List_of_OAuth_providers
.. _SMART on FHIR: http://docs.smarthealthit.org/
.. _SMART App Authorization Guide: http://docs.smarthealthit.org/authorization/
.. _Scopes and Launch Context: http://hl7.org/fhir/smart-app-launch/scopes-and-launch-context/index.html
.. _Patient CompartmentDefinition: http://www.hl7.org/implement/standards/fhir/compartmentdefinition-patient.html
.. _ASP.NET Core Identity: https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity

You might also find it useful to enable more extensive authorization failure logging - Firely Server defaults to a secure setup and does not show what exactly went wrong during authorization. To do so, set the ``ASPNETCORE_ENVIRONMENT`` environment variable to ``Development``.
