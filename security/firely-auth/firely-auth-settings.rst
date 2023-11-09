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
    }
   },
 
These settings are not Firely Auth specific, and you can read more about them in the `Microsoft documentation <https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints>`_.
In that documentation you can also read how to use the ``Https`` setting instead of ``HttpsFromPem`` to use a ``.pfx`` file for your SSL certificate.

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

- ``FHIR_BASE_URL``: This also has two uses:

  - A token can have a claim in the form of ``patient=<base>/Patient/123``, to define the compartment the client is restricted to.
    This url is used as the ``base`` part in that url, and should match the base url of Firely Server, as it is accessed by the client.
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
      "RSA_Config": {
          //"RSA_JWK": "<JSON Web Key>", // JSON Web Key of type RSA
          "SupportedAlgorithms": [
              "RS256",
              "RS384",
              "RS512"
          ],
          // "KeySize": 2048 // See https://www.keylength.com/en/compare/
      },
      "EC_Config": {
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

- ``RSA_Config``: defines the RSA algorithms that are supported. In the config above all available algorithms are listed.
  Inferno tests require at least RS256 for all Single Patient tests, and for Bulk Data Export a RS384 or higher is needed.

  - ``RSA_JWK``: allows to provide a pre-generated JSON Web Key. If this is not provided, Firely Auth will generate a key.
  - ``SupportedAlgorithms``: limit this list to the algorithms that you need in your setup. In the config above all available algorithms are listed.
  - ``KeySize``: the size of RSA key generated by Firely Auth. By default, it is set to 2048.

- ``EC_Config``: defines the EC (Elliptic Curve) algorithms that are supported. Inferno tests for Bulk Data Export require support for EC keys.

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
Therefore we register the users with Firely Auth. Firely Auth supports two types of stores: In memory and SQL Server.

For the InMemory store, the users and their passwords are listed in plain text in this configuration. This is useful for testing, but not recommended for production use.

The SqlServer store stores the users and their encrypted passwords in a MS SQL Server database. Also the `fhirUser` and `patient` claims for each user can be stored. 
See :ref:`firely_auth_deploy_sql` for details on setting up the database.

.. code-block:: json

  "UserStore": {
      "Type": "InMemory", // InMemory | SqlServer
      "PasswordHashIterations": 600000,
      "InMemory": {
          "AllowedUsers": [
              {
                  "Username": "bob",
                  "Password": "password",
                  "AdditionalClaims": [
                      {
                          "Name": "patient",
                          "Value": "Patient/a123"
                      }
                  ]
              }
          ]
      },
      "SqlServer": {
          "ConnectionString": "<connection string here>"
      }
  },

- ``Type``: select the type of store to use
- ``PasswordHashIterations``: number of password hash iterations to prevent brute force attacks. Default 600000. Sync this value when using Firely Auth Management App :ref:`firely_auth_mgmt`.
- ``InMemory``: settings for the InMemory store

  - ``AllowedUsers``: list of users
  - ``Username``: login for a user
  - ``Password``: password for the user, in clear text
  - ``AdditionalClaims``: currently to be used for a single claim, to link the user to a Patient resource (and thereby to a Patient compartment), or a 'user' resource like a Practitioner in Firely Server. 

    - ``Name``: name of the claim, currently only ``patient`` and ``fhirUser`` are supported
    - ``Value``: logical id of the related Patient or Practitioner resource (``Patient/id``)
      In the token this value will be expanded to an absolute url by prepending it with ``FhirServer.FHIR_BASE_URL`` (see :ref:`firely_auth_settings_server`).

- ``SqlServer``: settings for the SQL Server store
  
  - ``ConnectionString``: connection string to the SQL Server database where the users are to be stored. 
    This database and the schema therein must be created beforehand with a script. 

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
              "RedirectUris": ["https://www.getpostman.com/oauth2/callback", "https://oauth.pstmn.io/v1/callback"],
              "ClientSecrets": [{"SecretType": "SharedSecret", "Secret": "re4&ih)+HQu~w"}], // SharedSecret, JWK
              "AllowedGrantTypes": ["client_credentials", "authorization_code"],
              "AllowedSmartLegacyActions": [],
              "AllowedSmartActions": ["c", "r", "u", "d", "s"],
              "AllowedSmartSubjects": [ "patient", "user", "system"],
              "AllowedResourceTypes": ["Patient", "Observation", "Claim"],
              "AlwaysIncludeUserClaimsInIdToken": true,
              "RequirePkce": false,
              "AllowOfflineAccess": false,
              "AllowOnlineAccess": false,
              "AllowFirelySpecialScopes": true,
              "RequireClientSecret": true,
              "RefreshTokenLifetime": "30",
              "RequireMfa": true,
              "AccessTokenType": "Jwt"
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
    Use one entry for each key in the keyset. Note that the JWK json structure is enbedded in a string, so you need to escape the quotes within the JWK.
    The url option above is recommended. 

- ``AllowedGrantTypes``: array of either or both ``"client_credentials"`` and ``"authorization_code"``, referring to :term:`client credentials` and :term:`authorization code flow`. Use ``client credentials`` only for a :term:`confidential client`.
- ``AllowedSmartLegacyActions``: Firely Auth can also still support SMART on FHIR v1, where the actions are ``read`` and ``write``.
- ``AllowedSmartActions``: Actions on resources that can be granted in SMART on FHIR v2: ``c``, ``r``, ``u``, ``d`` and/or ``s``, see `SMART on FHIR V2 scopes`_
- ``AllowedSmartSubjects``: Categories of 'subjects' to which resource actions can be granted. Can be ``system``, ``user`` and/or ``patient``
- ``AllowedResourceTypes``: The client can only request SMART scopes for these resource types. To allow all resource types, do not use ``["*"]"`` but just leave the array empty.
- ``AlwaysIncludeUserClaimsInIdToken``: true / false: When requesting both an id token and access token, should the user claims always be added to the id token instead of requiring the client to use the userinfo endpoint. Default is false
- ``Require PKCE``: true / false - see :term:`PKCE`. true is recommended for a :term:`public client` and can offer an extra layer of security for :term:`confidential client`.
- ``AllowOfflineAccess``: true / false - Whether app can request refresh tokens while the user is online, see `SMART on FHIR refresh tokens`_
- ``AllowOnlineAccess``: true / false - Whether app can request refresh tokens while the user is offline, see `SMART on FHIR refresh tokens`_. A user is offline if he is logged out of Firely Auth, either manually or by expiration
- ``AllowFirelySpecialScopes``: true / false - Allow app to request scopes for Firely Server specific operations. Currently just 'http://server.fire.ly/auth/scope/erase-operation'
- ``RequireClientSecret``: true / false - A :term:`public client` cannot hold a secret, and then this can be set to ``false``. Then the ``ClientSecrets`` section is ignored. See also the note below.
- ``RefreshTokenLifetime``: If the client is allowed to use a :term:`refresh token`, how long should it be valid? The value is in days. You can also use HH:mm:ss for lower values.
- ``RequireMfa``: true / false, default is false. A user granting access to this client has to enable and use Multi Factor Authentication. See :ref:`firely_auth_mfa`
- ``AccessTokenType``: ``Jwt`` or ``Reference``. ``Jwt`` means that this client will get self-contained Json Web Tokens. ``Reference`` means that this client will get reference tokens, that refer to the actual token kept in memory by Firely Auth. For more background see :term:`reference token`.

External identity providers
^^^^^^^^^^^^^^^^^^^^^^^^^^^

- ``LogoutMethod``: Allows the user to automatically logout of the federated identity provider if the user logs out of Firely Auth. By default the user will only be logged out locally.
- ``Scheme``: Name of the federated identity provider. Each identity provider must have a unique scheme.
- ``Authority``: Url of the external identity provider.
- ``DisplayName``: Name that will be displayed in the UI of Firely Auth for users to select which identity provider to use if multiple are configured or if a local login is enabled as well.
- ``ClientId``: ClientId of Firely Auth that will be used in the implicit token flow in order to retreive an id token from the external identity provider.
- ``ClientSecret``: ClientSecret of Firely Auth that will be used in the implicit token flow in order to retreive an id token from the external identity provider.

.. _firely_auth_settings_allowedorigins:

AllowedOrigins
^^^^^^^^^^^^^^

By default CORS is enabled for all origins communicating over https. To adjust this, change the allowed origins in the ``AllowedOrigins`` setting.
Wildcards can be used, for example to allow all ports: ``"https://localhost:*"``, or to allow all subdomains ``"https://*.fire.ly"``.

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
    "KeyManagementConfig": {
      "RSA_Config": {
        "SupportedAlgorithms": [
          "RS256",
          "RS384",
          "RS512"
        ]
      },
      "EC_Config": {
        "SupportedAlgorithms": [
          "ES256",
          "ES384",
          "ES512"
        ]
      }
    },
    "UserStore": {
      "Type": "InMemory", // InMemory | SqlServer
      "InMemory": {
        "AllowedUsers": [
          {
            "Username": "alice",
            "Password": "p@sSw0rd",
            "AdditionalClaims": [
              {
                "Name": "patient",
                "Value": "<id of a patient in your Firely Server, e.g. 'example'>"
              },
              {
                "Name": "fhirUser",
                "Value": "Practitioner/<id of a practitioner"
              }
            ]
          }
        ]
      },
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
  //PipelineOptions: make sure that Vonk.Plugin.SoFv2 is enabled
  "PipelineOptions": { 
    "PluginDirectory": "./plugins",
    "Branches": [
      {
        "Path": "/",
        "Include": [
          //all other default plugins...
          "Vonk.Plugin.SoFv2",
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