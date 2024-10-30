.. _firely_auth_settings:

Firely Auth Settings
====================

Firely Auth can be configured extensively. This page lists all the settings and refers to some detail pages for some of the sections.

Settings files and variables
----------------------------

Just like Firely Server itself, Firely Auth features a hierarchy of settings files and variables. From lowest to highest priority:

- ``appsettings.default.json`` - This comes with the binaries (and in the Docker container) and contains sensible defaults for most settings. 
  You can change this file, but it might accidentally be overwritten upon a new release. Instead, put your settings in one of the following places.
- ``appsettings.instance.json`` - This file is meant to override settings for this instance of Firely Auth. You can create this file yourself by copying (parts of) ``appsettings.default.json``.
- Environment variables with the prefix ``FIRELY_AUTH_`` - Use environment variables to more easily configure settings from CI/CD pipelines, secure vaults etc.

Unsure how to name your variables? This works the same as with :ref:`configure_envvar`.

If you are working with deployments on Azure or AWS, it is necessary to load any configuration in a separate folder in the root of Firely Auth. Providing a folder for your settings files also works the same as with :ref:`configure_settings_path`, but then using the environment variable ``FIRELY_AUTH_PATH_TO_SETTINGS``.

Sections
--------

.. _firely_auth_settings_license:

License
^^^^^^^

Use the License settings to set the location to the license file. A relative path is evaluated relative to the executable ``Firely.Auth.Core.exe``.
You can use the same license file that came with Firely Server.

.. code-block:: json

  "License": {
    "LicenseFile": "./firelyserver-license.json"
  },


.. _firely_auth_settings_kestrel:

Kestrel
^^^^^^^

Kestrel is the web server that is integrated into Firely Auth, just like for Firely Server. We recommend to run both behind a :ref:`reverse proxy <deploy_reverseProxy>`.
Nevertheless you can control the settings for Kestrel.

.. code-block:: json
    
  "Kestrel": {
    "Endpoints": {
      "HttpsFromPem": {
        "Url": "https://localhost:5101",
        "SslProtocols": [ "Tls12", "Tls13" ],
        "Certificate": {
          "Path": "cert.pem",
          "KeyPath": "cert-key.pem"
        }
      }
    }
   },
 
These settings are not Firely Auth specific, and you can read more about them in the `Microsoft documentation <https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints>`_.
In that documentation you can also read how to use the ``Https`` setting instead of ``HttpsFromPem`` to use a ``.pfx`` file for your SSL certificate.

Note: you can configure a http endpoint like:

.. code-block:: json

    "Http": {
      "Url": "http://localhost:5100"
    },

But this is not supported when you do not use a proxy in front of the application that exposes it to the outside world over ``Https``. 
Without a proxy, this would lead to security issues and the authorization flow not working properly.  

.. _firely_auth_settings_account:

Account
^^^^^^^

These settings control the account specific options:

.. code-block:: json

  "Account": {
    "AuthenticationCookieExpiration": "01:30", // [ws][-]{ d | [d.]hh:mm[:ss[.ff]] }[ws] (provide days or timespan)
    "Password": {
      "RequireDigit": true,
      "RequiredLength": 12,
      "RequireUppercase": true,
      "RequireLowercase": true,
      "RequireNonAlphanumeric": false
    }
  },

- ``AuthenticationCookieExpiration``: specifies how long the authentication cookie is valid. You can specify just a number that specifies the days the token is valid, or you can provide a timespan.

- ``Password``: Here you can specify where the user passwords must comply to.

.. _firely_auth_settings_email:

Email
^^^^^

These settings are the configuration settings for the email client Firely Auth uses to send emails to users.
Currently SMTP and SendGrid are the supported email clients.

.. code-block:: json

  "Email": {
    "Type": "Smtp",
    "FromEmailAddress": "", 
    "EmailTemplateFolder": "./Data/EmailTemplates",
    "ActivateAccountEmailSubject": "Firely Server account activation.",
    "ForgotPasswordEmailSubject":  "Firely Server forgot password.",
    //,"Smtp": {
    //	"Server": "",
    //	"Port": 0,
    //	"RequiresAuthentication":true,
    //	"User": "",
    //	"Password": "",
    //	"UseSsl": true
    //}
    //,"SendGrid": {
    //    "ApiKey": ""
    //}
  },

- ``Type``: The type of email client: ``Smtp`` or ``SendGrid``. 
- ``FromEmailAddress``: The email address to use as sender.
- ``EmailTemplateFolder``: The path to email templates that are used. These use the liquid format (https://shopify.github.io/liquid/). You can change these templates and store them in a folder that does not get overwritten when you update Firely Auth. You should not change the name of the template files, and only the variables that are used in the original template are available to use in custom templates.
- ``ActivateAccountEmailSubject``: The subject that will be put in account activation emails.
- ``ForgotPasswordEmailSubject``: The subject that will be put in forgot password emails.
- ``Smtp``: Fill these settings when you use the ``Smtp`` type.
- ``SendGrid``: Fill this setting when you use the ``SendGrid`` type.

.. _firely_auth_settings_ui:

UI Settings
^^^^^^^^^^^

These settings control the white labelling options for Firely Auth:

.. code-block:: json

  "UISettings": {
    "LoginPageText": "Please login to Firely Auth",
    "OrganizationTitle": "Firely Auth",
    "OrganizationLogoPath": "<firely logo>",
    "OrganizationFavIconPath": "<firely favicon>"
  },

- ``LoginPageText``: Here you can put a text that will be displayed on the login page.

- ``OrganizationTitle``: Here you can put a text that will be displayed in the title bar of the browser.

- ``OrganizationLogoPath``: Here you can point to an image file you want to use as logo in the application.

- ``OrganizationFavIconPath``: Here you can point to an image file you want to use as favicon in the browser.

.. _firely_auth_settings_server:

Firely Server
^^^^^^^^^^^^^

Firely Auth hands out SMART on FHIR access tokens to access resources on Firely Server. 
To make Firely Server known to Firely Auth, fill in the ``FhirServer``:

.. code-block:: json

   "FhirServer": {
      "Name": "Firely Server",
      "FHIR_BASE_URL": "http://localhost:4080",
      // "IntrospectionSecret": "<secret>"
   },

- ``Name``: This name serves two purposes:

  - It is used to translate to ``FHIR_BASE_URL`` which will be added to the token as the value of the ``aud`` (audience) claim, if the client requests so. 
    To have it accepted by Firely Server, set its ``SmartAuthorizationOptions:Audience`` setting to the same value as ``FHIR_BASE_URL``.
  - It correlates with the clients allowed to access the token introspection endpoint.

- ``FHIR_BASE_URL``:

  - If an ``aud`` parameter is provided *in the authorize request*, it has to match this url. 
    E.g. in Postman you can provide this parameter by adding it to the Auth URL, like this: ``{{ids}}/connect/authorize?aud=http://localhost:4080`` 
    See the ``aud`` parameter in `SMART on FHIR authorization request`_

- ``IntrospectionSecret``: When using a :term:`reference token`, Firely Server must verify the token with Firely Auth and the communication needs to be authenticated by providing the name and the secret. This configuration is only needed if at least one :term:`client` is configured to use reference tokens, see :ref:`firely_auth_settings_tokentypes` for the configuration.

.. _firely_auth_settings_tokentypes:

Token types
^^^^^^^^^^^

Define for each client what type of token it can request. See :ref:`firely_auth_settings_clients` for the configuration of a specific client.

.. _firely_auth_settings_keymanagement:

Key management
^^^^^^^^^^^^^^

.. code-block:: json

  "KeyManagement": {
      "RSA": {
          //"JWK": "<JSON Web Key>", // JSON Web Key of type RSA
          "SupportedAlgorithms": [
              "RS256",
              "RS384",
              "RS512"
          ],
          // "KeySize": 2048 // See https://www.keylength.com/en/compare/
      },
      "EC": {
          //"JWK_ES256": "<JSON Web Key>", // JSON Web Key of type EC with crv P-256
          //"JWK_ES384": "<JSON Web Key>", // JSON Web Key of type EC with crv P-384
          //"JWK_ES512": "<JSON Web Key>", // JSON Web Key of type EC with crv P-512
          "SupportedAlgorithms": [
              "ES256",
              "ES384",
              "ES512"
          ]
      }
  }

Firely Auth can work with multiple signature keys, used to sign access and other tokens. 

- ``RSA``: defines the RSA algorithms that are supported. In the config above all available algorithms are listed.
  Inferno tests require at least RS256 for all Single Patient tests, and for Bulk Data Export a RS384 or higher is needed.

  - ``JWK``: allows to provide a pre-generated JSON Web Key. If this is not provided, Firely Auth will generate a key.
  - ``SupportedAlgorithms``: limit this list to the algorithms that you need in your setup. In the config above all available algorithms are listed.
  - ``KeySize``: the size of RSA key generated by Firely Auth. By default, it is set to 2048.

- ``EC``: defines the EC (Elliptic Curve) algorithms that are supported. Inferno tests for Bulk Data Export require support for EC keys.

  - ``JWK_ES*``: allows to provide a pre-generated JSON Web Key. If this is not provided, Firely Auth will generate a key for each of the supported algorithms.
  - ``SupportedAlgorithms``: limit this list to the algorithms that you need in your setup. In the config above all available algorithms are listed.

Note that a single RSA key can be used for all supported algorithms. However, an EC key is tied to a specific algorithm, therefore you can supply a key for each of the algorithms.

For more background on JSON Web Keys see `RFC 7517 <see https://tools.ietf.org/html/rfc7517>`_.

.. _firely_auth_settings_tokenintro:

Token introspection
^^^^^^^^^^^^^^^^^^^

When using a :term:`reference token`, Firely Server must verify the token with Firely Auth. See :ref:`firely_auth_settings_server`. 
Whether to use reference token or JWT's is configured per client in :ref:`firely_auth_settings_clients`, with the ``AccessTokenType`` setting.

.. _firely_auth_settings_userstore:

User store
^^^^^^^^^^

A :term:`user` must be able to authenticate to Firely Auth before granting permissions to a :term:`client`. 
Therefore we register the users with Firely Auth. Firely Auth supports two types of stores: Sqlite and SQL Server.

The store stores the user information, their encrypted passwords and their claims in the database.
See :ref:`firely_auth_deploy_sqlite` and :ref:`firely_auth_deploy_sql` for details on setting up the database.

.. code-block:: json

  "UserStore": {
      "Type": "Sqlite", // Sqlite | SqlServer
      "PasswordHashIterations": 600000,
      "LogSqlQueryParameterValues": false,
      "Sqlite": {
          "ConnectionString": "<connection string here>"
      },
      "SqlServer": {
          "ConnectionString": "<connection string here>"
      }
  },

- ``Type``: select the type of store to use
- ``PasswordHashIterations``: number of password hash iterations to prevent brute force attacks. Default 600000. Sync this value when using Firely Auth Management App :ref:`firely_auth_mgmt`.
- ``LogSqlQueryParameterValues``: when you configured logging of executed queries, the parameter values that are sent to the database are hidden by default. By putting this setting to ``true``, the values will be unhidden and visible in the logs. This might expose sensitive data. You will have to change the ``Serilog->MinimumLevel->Default`` and ``Serilog->MinimumLevel->Override->Microsoft`` log settings to ``Information``. 
- ``Sqlite``: settings for the Sqlite store

  - ``ConnectionString``: connection string to the SQL Server database where the users are to be stored.

- ``SqlServer``: settings for the SQL Server store

  - ``ConnectionString``: connection string to the SQL Server database where the users are to be stored. This database and the schema therein must be created beforehand with a script when you use a database account with limited permissions. 

.. _firely_auth_settings_clients:

Clients
^^^^^^^

The ``ClientRegistration`` is used to register the :term:`clients <client>` that are allowed to request access tokens from Firely Auth.

.. code-block:: json

  "ClientRegistration": {
      "AllowedClients": [
          {
              "ClientId": "Jv3nZkaxN36ucP33",
              "ClientName": "Postman",
              "Description": "Postman API testing tool",
              "Enabled": true,
              "RequireConsent": true,
              "RedirectUris": ["https://www.getpostman.com/oauth2/callback", "https://oauth.pstmn.io/v1/callback", "https://oauth.pstmn.io/v1/browser-callback"],
              "ClientSecrets": [{"SecretType": "SharedSecret", "Secret": "re4&ih)+HQu~w"}], // SharedSecret, JWK
              "AllowedGrantTypes": ["client_credentials", "authorization_code"],
              "AllowedSmartLegacyActions": [],
              "AllowedSmartActions": ["c", "r", "u", "d", "s"],
              "AllowedSmartSubjects": [ "patient", "user", "system"],
              "AllowedResourceTypes": ["Patient", "Observation", "Claim"],
              "EnableLegacyFhirContext": false,
              "AlwaysIncludeUserClaimsInIdToken": true,
              "RequirePkce": false,
              "Require2fa": false,
              "AllowOfflineAccess": false,
              "AllowOnlineAccess": false,
              "AllowFirelySpecialScopes": true,
              "RequireClientSecret": true,
              "RefreshTokenLifetime": "30",
              "ConsentLifetime": "365",
              "RequireMfa": true,
              "AccessTokenType": "Jwt",
              "ClientClaims": [
                {
                  "Name": "ClaimName",
                  "Value": "ClaimValue"
                }
              ],
              "ClientClaimPrefix": "",
              "AlwaysSendClientClaims": false,
              "AllowManagementApiAccess": false
          }
      ]
  }

You register a :term:`client` in the ``AllowedClients`` array. For each client you can configure these settings:

- ``ClientId``: string: unique identifier for this client. It should be known to the client as well
- ``ClientName``: string: human readable name for the client, it is shown on the consent page
- ``Description``: string:  human readable description of the client
- ``Enabled``: true / false: simple switch to enable or disable a client (instead of removing it from the list)
- ``RequireConsent``: true / false: when true, Firely Auth will show the user a page for consent to granting the requested scopes to the client, otherwise all requested and valid scopes will be granted automatically.
- ``RedirectUris``: array of strings: url(s) on which Firely Auth will send the authorization code and access token. The client can specify one of the preregistered urls for a specific request.
- ``ClientSecrets``: secrets can be of type ``SharedSecret`` or ``JWK``. You can have multiple of each, so you can accept two secrets for a short period of time to support key rotation and an update window for the client. The ``ClientSecrets`` section is ignored if ``RequireClientSecret`` is set to ``false``.

  - SharedSecret: ``{"SecretType": "SharedSecret", "Secret": "<a secret string shared with the client>"}`` - this can be used for either :term:`client credentials` or :term:`authorization code flow`, but only with a :term:`confidential client`.
  - JWK: ``{"SecretType": "JWK", "SecretUrl": "<JWKS url>"}`` - where the JWKS url hosts a JSON Web Key Set that can be retrieved by Firely Auth, see also :term:`JWK`.
  - JWK: ``{"SecretType": "JWK", "Secret": "<JWK>"}`` - where JWK is the contents of a :term:`JWK`. Use this if the client cannot host a url with a JWKS. 
    Use one entry for each key in the keyset. Note that the JWK json structure is embedded in a string, so you need to escape the quotes within the JWK.
    The url option above is recommended. 

- ``AllowedGrantTypes``: array of either or both ``"client_credentials"`` and ``"authorization_code"``, referring to :term:`client credentials` and :term:`authorization code flow`. Use ``client credentials`` only for a :term:`confidential client`.
- ``AllowedSmartLegacyActions``: Firely Auth can also still support SMART on FHIR v1, where the actions are ``read`` and ``write``.
- ``AllowedSmartActions``: Actions on resources that can be granted in SMART on FHIR v2: ``c``, ``r``, ``u``, ``d`` and/or ``s``, see `SMART on FHIR V2 scopes`_
- ``AllowedSmartSubjects``: Categories of 'subjects' to which resource actions can be granted. Can be ``system``, ``user`` and/or ``patient``
- ``AllowedResourceTypes``: The client can only request SMART scopes for these resource types. To allow all resource types, do not use ``["*"]"`` but just leave the array empty.
- ``EnableLegacyFhirContext``: true / false - Whether to use the new syntax of ``fhirContext`` defined in `SMART on FHIR v2.1.0 <https://hl7.org/fhir/smart-app-launch/scopes-and-launch-context.html#fhir-context>`_. Default is false, when set to true the old syntax of ``fhirContext`` defined in `SMART on FHIR v2.0.0 <https://hl7.org/fhir/smart-app-launch/STU2/scopes-and-launch-context.html#fhircontext>`_ is used.
- ``AlwaysIncludeUserClaimsInIdToken``: true / false: When requesting both an id token and access token, should the user claims always be added to the id token instead of requiring the client to use the userinfo endpoint. Default is false
- ``Require PKCE``: true / false - see :term:`PKCE`. true is recommended for a :term:`public client` and can offer an extra layer of security for :term:`confidential client`.
- ``Require2fa``: true / false - Whether users are obliged to set up Multi Factor Authentication before they can use their account to get a token.
- ``AllowOfflineAccess``: true / false - Whether app can request refresh tokens while the user is online, see `SMART on FHIR refresh tokens`_
- ``AllowOnlineAccess``: true / false - Whether app can request refresh tokens while the user is offline, see `SMART on FHIR refresh tokens`_. A user is offline if he is logged out of Firely Auth, either manually or by expiration
- ``AllowFirelySpecialScopes``: true / false - Allow app to request scopes for Firely Server specific operations. Currently just 'http://server.fire.ly/auth/scope/erase-operation'
- ``RequireClientSecret``: true / false - A :term:`public client` cannot hold a secret, and then this can be set to ``false``. Then the ``ClientSecrets`` section is ignored. See also the note below.
- ``RefreshTokenLifetime``: If the client is allowed to use a :term:`refresh token`, how long should it be valid? The value is in days. You can also use HH:mm:ss for lower values.
- ``ConsentLifetime`` : This is an optional setting which can specify a period after which the users consent will be revoked. The value is in days. You can also use HH:mm:ss for lower values.
- ``AccessTokenType``: ``Jwt`` or ``Reference``. ``Jwt`` means that this client will get self-contained Json Web Tokens. ``Reference`` means that this client will get reference tokens, that refer to the actual token kept in memory by Firely Auth. For more background see :term:`reference token`.
- ``ClientClaims``: Enable a client to add static custom claims in the client credential flow. 

  - ``Name``: name of the claim
  - ``Value``: the value of the claim

- ``ClientClaimPrefix``: Add custom defined prefix to the name of all custom client claims. Works together with the setting ``ClientClaims``. 
- ``AlwaysSendClientClaims``: Add the claims defined in ``ClientClaims`` regardless of the OAuth 2.0 flow used by a client (e.g. even if a authorization_code flow is used)
- ``AllowManagementApiAccess``: Allows this client to use the :ref:`firely_auth_mgmt`

.. note::

    Please follow the principle of least privilege to register a SMART Backend Service client, especially when the settings ``ClientClaims`` and ``ClientClaimPrefix`` are used.

.. _firely_auth_settings_externalidp:

External identity providers
^^^^^^^^^^^^^^^^^^^^^^^^^^^

.. code-block:: json

  "ExternalIdentityProviders": {
		"IdentityProvider": [
			{
			"LogoutMethod": "LocalOnly", // <LocalOnly> logout of Firely Auth only | <SingleSignout> also logout of external provider
			"Scheme": "OpenIdConnect-SAMPLE", // generate a unique name for each Identity Provider
			"Authority": "<url to external OpenId Connect endpoint>",
			"DisplayName": "Login via SSO - <Name of IdentityProvider>",
			"ClientId": "ClientId for Firely Auth, pre-registered with external service",
			"ClientSecret": "secret for clientId",
			"AllowAutoProvision": true|false,
			"AutoProvisionFromSecurityGroup": ["<Security Group>"],
			"UserClaimsFromIdToken": [{
				"Key": "<key of claim to copy>",
				"CopyAs": "<optional name if claim to be renamed>"
			}],
			"FhirUserLookupClaimsMapping": [{
				"SearchParameterName": "<code>",
				"SearchParameterValueTemplate": "{NumericalIndexForClaim}",
			  "CopySearchParameterValuesFromClaims": []
			}]
			}
		]
	}

- ``LogoutMethod``: Allows the user to automatically logout of the federated identity provider if the user logs out of Firely Auth. By default the user will only be logged out locally.
- ``Scheme``: Name of the federated identity provider. Each identity provider must have a unique scheme.
- ``Authority``: Url of the external identity provider.
- ``DisplayName``: Name that will be displayed in the UI of Firely Auth for users to select which identity provider to use if multiple are configured or if a local login is enabled as well.
- ``ClientId``: ClientId of Firely Auth that will be used in the implicit token flow in order to retrieve an id token from the external identity provider.
- ``ClientSecret``: ClientSecret of Firely Auth that will be used in the implicit token flow in order to retrieve an id token from the external identity provider.
-	``AllowAutoProvision``: true / false - If true, Firely Auth will automatically create a user in its own database if the user logs in with an external identity provider for the first time. The user will be created with the claims that are provided by the external identity provider.
- ``AutoProvisionFromSecurityGroup``: When ``AllowAutoProvision`` is true, this setting allows you to specify a security group that the user must be a member of in order to be automatically provisioned. If the user is not a member of this group, the user will not be automatically provisioned.
- ``UserClaimsFromIdToken``: This setting allows you to map the claims from the token that is received from the external identity provider to the claims that are stored in the Firely Auth database. The key is the claim that is received from the external identity provider. This key can be copied as a value that is recognized by Firely Auth. For intance, Azure is able to provide fhirUser claim to the token, but will prefix this claim with ``extn.``. The CopyAs field can be used to remove this prefix, so that Firely Auth is able to recognize the fhirUser claim.
- ``FhirUserLookupClaimsMapping``: As an alternative for retrieving the FhirUser Claim from the ``UserClaimsFromIdToken`` setting, ``FhirUserLookupClaimsMapping`` allows you to use the claims from the ID token to search for a users respective resource in Firely Server. This can either be a Patient resource or a Practitioner recource. Firely Auth will then use the id of this resource to derive the fhirUser claim of the user upon SSO auto-provisioning. Multiple mappings can be provided. Each search parameter will be combined using a logical  AND while searching for the fhirUser resource. The fhirUser is only derived if there is an unambiguous match in Firely Server.
- ``SearchParameterName``: The search parameter that will be used to search for the user in Firely Server. This can be any search parameter that can be used to query ``Patient`` or ``Practitioner`` resources. This search parameter will be used on a system-level search against Firely Server.
- ``SearchParameterValueTemplate``: The template that will be used to construct the value that will be used to search for the user in Firely Server. The template can contain placeholders that will be replaced by the values of the claims from the ID token. The placeholders should be in the format ``{NumericalIndexForClaim}``. The numerical index is the index of the claim in the array of claims that are provided by the external identity provider. The index starts at 0.
- ``CopySearchParameterValuesFromClaims``: This setting allows you to copy the values of the claims from the ID token to the template that is used to construct the value that will be used to search for the user in Firely Server. The values of the claims will be copied in the order that they are provided in the array. The values will be copied to the placeholders in the template that are in the format ``{NumericalIndexForClaim}``.

.. _firely_auth_settings_allowedorigins:

AllowedOrigins
^^^^^^^^^^^^^^

By default CORS is enabled for all origins communicating over https. To adjust this, change the allowed origins in the ``AllowedOrigins`` setting.
Wildcards can be used, for example to allow all ports: ``"https://localhost:*"``, or to allow all subdomains ``"https://*.fire.ly"``.

.. _firely_auth_settings_disclaimers:

Disclaimer Registration
^^^^^^^^^^^^^^^^^^^^^^^

Firely Auth can render custom disclaimers that will be shown to the user to collect user consent for custom policies (e.g. terms of service or privacy policies).
These policies will be presented in the UI after the user has been authenticated after a login, but still before a token is issued. Agreeing to all disclaimers is mandatory.

.. code-block:: json

  	"DisclaimerRegistration": {
      "Disclaimers": [
        {
        	"Id" : "<string>", // some id that will not change for this disclaimer
        	"Template": "<path to .liquid template for this disclaimer>",
        	"Description": "<string>" // the text that will be shown next to the checkbox
        	"TemplateProperties":{ // this is a dictionary of additional properties that will be provided to the template
        		"propertyName":"propertyValue",
        		"propertyName2":"propertyValue2"
        	},
          "ShowDisclaimerFor": {
				  "EveryLogin": false|true, // if true then the disclaimer is shown on each login, there is a grace period here where the consent is temporary stored
				  "Clients": [ "<ClientId>" ], // if set then this disclaimer will only be shown for the specified clients
				  }
        }
      ]
	  }

Each disclaimer needs to be uniquely identifable. Please ensure that all an id is provided to all disclaimers. We recommend assigning an UUID here.
The content of a disclaimer is user-defined and can be expressed in a `liquid template <https://github.com/Shopify/liquid>`_.
For each disclaimer a checkbox is rendered in the UI by Firely Auth on the disclaimer page. A description shown next the checkbox can be defined for each disclaimer.
Firely Auth will automatically fill out placeholders defined in the liquid template based on static properties defined as ``TemplateProperties``.

For versioning, the ``Id`` property can be used, like using ``GeneralTermsV1`` and then changing it to ``GeneralTermsV2`` if needed.
After doing a change like this, the system will ask for agreement to ``GeneralTermsV2`` upon next login that requires this disclaimers consent.
The consent for the previous disclaimer will stay in the database for future reference.

See the ``Data\DisclaimerTemplates`` folder in the Firely Auth disribution for an example disclaimer template.

Inferno test settings
---------------------

The Inferno test suite for ONC Certification (g)(10) Standardized API has tests using the "Inferno-Public" client. For this client, ``RequireClientSecret`` has to be set to ``false``.
The same suite also issues a launch id as part of test 3.3. For this to succeed, use the :ref:`firely_auth_endpoints_launchcontext` end point to request a dynamic launch context.

Below you will find the settings that can act as a reference for testing this suite. On top of that you will need to arrange:

For hosting (either directly with Kestrel as shown below, or with a reverse proxy that sits in front)

- SSL certificate for Firely Auth
- SSL certificate for Firely Server
- Configure both to use SSL protocols TLS 1.2 and 1.3

Necessary data:

- Pre-load one version of US-Core conformance resources to the Firely Server administration endpoint
  (please note :ref:`this warning<us-core_composite_parameters>`)
- Pre-load the example resource of the same version of US-Core to the regular endpoint

We have a full walkthrough of Inferno testing available as a whitepaper, see `our resources <https://fire.ly/resources/>`_.

.. note::
    Firely Auth 3.2.0 introduces a new end point ``launchContext``, which can be used to request a ``launch`` identifier dynamically. Therefore no need to configure the static ``LaunchIds`` in the Inferno client settings.
    See more details in the :ref:`firely_auth_endpoints_launchcontext` for requesting ``launch`` identifier dynamically

Firely Auth settings
^^^^^^^^^^^^^^^^^^^^

Put these settings in ``appsettings.instance.json`` next to the executable. 

For Inferno you have to host it on https, with TLS 1.2 minimum. So you also need to provide a certificate for that (either to Kestrel as shown below, or to a reverse proxy that sits in front).

.. code-block:: json

  {
    "Kestrel": {
      "Endpoints": {
        "Http": {
          "Url": "http://localhost:5100"
        },
        "HttpsFromPem": {
          "Url": "https://localhost:5101",
          "SslProtocols": [ "Tls12", "Tls13" ],
          "Certificate": {
            "Path": "cert.pem",
            "KeyPath": "cert-key.pem"
          }
        }
    // Use "Https" option instead if you want to use a .pfx file. See https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints
    }
   },
    "FhirServer": {
      "Name": "Firely Server",
      "FHIR_BASE_URL": "<url where you host Firely Server>",
      "IntrospectionSecret": "secret"
    },
    "KeyManagement": {
      "RSA": {
        "SupportedAlgorithms": [
          "RS256",
          "RS384",
          "RS512"
        ]
      },
      "EC": {
        "SupportedAlgorithms": [
          "ES256",
          "ES384",
          "ES512"
        ]
      }
    },
    "Email": {
      "Type": "Smtp", // Smtp/SendGrid
      "FromEmailAddress": "", // the email address to use as sender
      "EmailTemplateFolder": "./Data/EmailTemplates", // the path to the folder with the email templates
      "ActivateAccountEmailSubject": "Firely Server account activation.", // the subject that will be put in account activation emails
      "ForgotPasswordEmailSubject":  "Firely Server forgot password.", // the subject that will be put in forgot password emails
      //,"Smtp": { // either provide your smtp settings or your sendgrid settings
      //	"Server": "",
      //	"Port": 0,
      //	"RequiresAuthentication":true,
      //	"User": "",
      //	"Password": "",
      //	"UseSsl": true
      //}
      //,"SendGrid": {
      //    "ApiKey": ""
      //}
    },
    "ClientRegistration": {
      "AllowedClients": [
        {
          "ClientId": "Inferno",
          "ClientName": "Inferno",
          "Enabled": true,
          "RequireConsent": true,
          "RedirectUris": [ "https://inferno.healthit.gov/suites/custom/smart/launch", "https://inferno.healthit.gov/suites/custom/smart/redirect" ],
          "AllowedGrantTypes": [ "authorization_code" ],
          "ClientSecrets": [
            {
              "SecretType": "SharedSecret",
              "Secret": "secret"
            }
          ],
          "AllowFirelySpecialScopes": false,
          "AllowedSmartLegacyActions": [ "read", "write", "*" ],
          "AllowedSmartActions": [ "c", "r", "u", "d", "s" ],
          "AllowedSmartSubjects": [ "patient", "user" ],
          "AlwaysIncludeUserClaimsInIdToken": true,
          "RequirePkce": false,
          "AllowOfflineAccess": true,
          "AllowOnlineAccess": false,
          "RequireClientSecret": true,
          "RefreshTokenLifetime": "90",
          "AccessTokenType": "Reference"
        },
        {
          "ClientId": "Inferno-Public",
          "ClientName": "InfernoPublic",
          "Enabled": true,
          "RequireConsent": true,
          "RedirectUris": [ "https://inferno.healthit.gov/suites/custom/smart/launch", "https://inferno.healthit.gov/suites/custom/smart/redirect"],
          "AllowedGrantTypes": [ "authorization_code" ],
          "AllowFirelySpecialScopes": false,
          "AllowedSmartLegacyActions": [ "read", "write", "*" ],
          "AllowedSmartActions": [ "c", "r", "u", "d", "s" ],
          "AllowedSmartSubjects": [ "patient", "user" ],
          "AlwaysIncludeUserClaimsInIdToken": true,
          "RequirePkce": false,
          "AllowOfflineAccess": true,
          "AllowOnlineAccess": false,
          "RequireClientSecret": false,
          "RefreshTokenLifetime": "90",
          "AccessTokenType": "Reference"
        },
        {
          "ClientId": "Inferno-Bulk",
          "ClientName": "InfernoBulk",
          "Enabled": true,
          "RedirectUris": [ "https://inferno.healthit.gov/suites/custom/smart/launch", "https://inferno.healthit.gov/suites/custom/smart/redirect"],
          "AllowedGrantTypes": [ "authorization_code", "client_credentials" ],
          "AllowFirelySpecialScopes": false,
          "AllowedSmartLegacyActions": [ "read" ],
          "AllowedSmartActions": [ "c", "r", "u", "d", "s" ],
          "AllowedSmartSubjects": [ "system" ],
          "RequirePkce": false,
          "AllowOfflineAccess": true,
          "AllowOnlineAccess": false,
          "ClientSecrets": [
            {
              "SecretType": "JWK",
              "Secret": "{'e':'AQAB','kid':'b41528b6f37a9500edb8a905a595bdd7','kty':'RSA','n':'vjbIzTqiY8K8zApeNng5ekNNIxJfXAue9BjoMrZ9Qy9m7yIA-tf6muEupEXWhq70tC7vIGLqJJ4O8m7yiH8H2qklX2mCAMg3xG3nbykY2X7JXtW9P8VIdG0sAMt5aZQnUGCgSS3n0qaooGn2LUlTGIR88Qi-4Nrao9_3Ki3UCiICeCiAE224jGCg0OlQU6qj2gEB3o-DWJFlG_dz1y-Mxo5ivaeM0vWuodjDrp-aiabJcSF_dx26sdC9dZdBKXFDq0t19I9S9AyGpGDJwzGRtWHY6LsskNHLvo8Zb5AsJ9eRZKpnh30SYBZI9WHtzU85M9WQqdScR69Vyp-6Uhfbvw'}"
            },
            {
              "SecretType": "JWK",
              "Secret": "{'kty':'EC','crv':'P-384','x':'JQKTsV6PT5Szf4QtDA1qrs0EJ1pbimQmM2SKvzOlIAqlph3h1OHmZ2i7MXahIF2C','y':'bRWWQRJBgDa6CTgwofYrHjVGcO-A7WNEnu4oJA5OUJPPPpczgx1g2NsfinK-D2Rw','key_ops':['verify'],'ext':true,'kid':'4b49a739d1eb115b3225f4cf9beb6d1b','alg':'ES384'}"
            }
          ],
          "RequireClientSecret": true,
          "RefreshTokenLifetime": "90",
          "AccessTokenType": "Jwt"
        }
      ]
    }
  }

Firely Server settings
^^^^^^^^^^^^^^^^^^^^^^

Put these settings in appsettings.instance.json, next to the executable.

For Inferno you have to host it on https, with TLS 1.2 minimum. So you also need to provide a certificate for that (either to Kestrel as shown below, or to a reverse proxy that sits in front).

.. code-block:: json

  "Hosting": {
    "HttpPort": 4080,
    "HttpsPort": 4081, // Enable this to use https
    "CertificateFile": "<your-certificate-file>.pfx", //Relevant when HttpsPort is present
    "CertificatePassword" : "<cert-pass>", // Relevant when HttpsPort is present
    "SslProtocols": [ "Tls12", "Tls13" ] // Relevant when HttpsPort is present.
  },
  "SmartAuthorizationOptions": {
    "Enabled": true,
    "Filters": [
      {
        "FilterType": "Patient",
        "FilterArgument": "_id=#patient#"
      }
    ],
    "Authority": "<url where Firely Auth is hosted>",
    "Audience": ""<url where you host Firely Server>", 
    "RequireHttpsToProvider": true, 
    "Protected": {
      "InstanceLevelInteractions": "read, vread, update, patch, delete, history, conditional_delete, conditional_update, $validate, $meta, $meta-add, $meta-delete, $export, $everything, $erase",
      "TypeLevelInteractions": "create, search, history, conditional_create, compartment_type_search, $export, $lastn, $docref",
      "WholeSystemInteractions": "batch, transaction, history, search, compartment_system_search, $export, $exportstatus, $exportfilerequest"
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
  },
  //PipelineOptions: make sure that Vonk.Plugin.Smart is enabled
  "PipelineOptions": { 
    "PluginDirectory": "./plugins",
    "Branches": [
      {
        "Path": "/",
        "Include": [
          //all other default plugins...
          "Vonk.Plugin.Smart",
        ],
        "Exclude": [
          //...
        ]
      },
      {
        "Path": "/administration",
        "Include": [
          //...
        ],
        "Exclude": [
          //...
        ]
      }
    ]
  }


.. _SMART on FHIR V2 scopes: http://hl7.org/fhir/smart-app-launch/scopes-and-launch-context.html#scopes-for-requesting-clinical-data
.. _SMART on FHIR refresh tokens: http://hl7.org/fhir/smart-app-launch/scopes-and-launch-context.html#scopes-for-requesting-a-refresh-token
.. _SMART on FHIR authorization request: http://hl7.org/fhir/smart-app-launch/app-launch.html#request-4
