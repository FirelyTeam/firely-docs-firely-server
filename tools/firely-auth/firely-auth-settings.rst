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

Use the License settings to set the location to the license file. A relative path is evaluated relative to the executable ``Firely.IdentityServer.Core.exe``.
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
To make Firely Server known to Firely Auth, fill in the ``FhirServerConfig``:

.. code-block:: json

   "FhirServerConfig": {
      "Name": "Firely Server",
      "FHIR_BASE_URL": "http://localhost:4080"
   },

- ``Name``: This name serves two purposes:

  - It can be added to the token as the value of the ``aud`` (audience) claim, if the client requests so. 
    To have it accepted by Firely Server, set its ``SmartAuthorizationOptions:Audience`` setting to the same value.
    (This currently only works for the Authorization Code flow.)
  - It correlates with the clients allowed to access the token introspection endpoint (see below).
    Therefore it should match the value of ``TokenIntrospectionConfig:TokenIntrospectionResources:Name``

- ``FHIR_BASE_URL``: A token can have a claim in the form of ``patient=<base>/Patient/123``, to define the compartment the client is restricted to.
  This url is used as the ``base`` part in that url, and should match the base url of Firely Server, as it is accessed by the client.

.. _firely_auth_settings_tokentypes:

Token types
^^^^^^^^^^^

Define for each client what type of token it can request.

.. code-block:: json

   "TokenConfig": {
      "AccessTokenType": {
          "<ClientId>": "Jwt"
      }
  },

- ``<ClientId>`` should match one of the clients defined in ``ClientRegistrationConfig``.
- The value can be one of ``Jwt`` or ``Reference``. ``Jwt`` means that this client will get self-contained Json Web Tokens.
  ``Reference`` means that this client will get reference tokens, that refer to the actual token kept in memory by Firely Auth.
  For more background see :term:`reference token`.

E.g. ``"MySmartApp": "Reference"`` 

.. _firely_auth_settings_keymanagement:

Key management
^^^^^^^^^^^^^^

.. code-block:: json

  "KeyManagementConfig": {
      "RSA_Config": {
          //"RSA_JWK": "<JSON Web Key>", // JSON Web Key of type RSA
          "SupportedAlgorithms": [
              "RS256",
              "RS384",
              "RS512"
          ]
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

Firely Auth can work with multiple signature keys, used to sign access tokens. 

- ``RSA_Config``: defines the RSA algorithms that are supported. In the config above all available algoriths are listed.
  Inferno tests require at least RS256 for all Single Patient tests, and for Bulk Data Export a RS384 or higher is needed.

  - ``RSA_JWK``: allows to provide a pre-generated JSON Web Key. If this is not provided, Firely Auth will generate a key.
  - ``SupportedAlgorithms``: limit this list to the algorithms that you need in your setup. In the config above all available algoriths are listed.

- ``EC_Config``: defines the EC (Elliptic Curve) algorithms that are supported. Inferno tests for Bulk Data Export require support for EC keys.

  - ``JWK_ES*``: allows to provide a pre-generated JSON Web Key. If this is not provided, Firely Auth will generate a key for each of the supported algorithms.
  - ``SupportedAlgorithms``: limit this list to the algorithms that you need in your setup. In the config above all available algoriths are listed.

Note that a single RSA key can be used for all supported algorithms. However, an EC key is tied to a specific algorithm, therefore you can supply a key for each of the algorithms.

For more background on JSON Web Keys see `RFC 7517 <see https://tools.ietf.org/html/rfc7517>`_.

.. _firely_auth_settings_tokenintro:

Token introspection
^^^^^^^^^^^^^^^^^^^

When using a :term:`reference token`, Firely Server must verify the token with Firely Auth.
Not just any system can ask for inspection though, therefore we list the systems that can with a name and a secret.

.. code-block:: json

  "TokenIntrospectionConfig": {
      "TokenIntrospectionResources": [{
          "Name": "Firely Server",
          "Secret": "<generate some hard to hack secret>"
      }]
  },

This configuration is only needed if at least one :term:`client` is configured to use reference tokens, see :ref:`firely_auth_settings_tokentypes`.

.. _firely_auth_settings_userstore:

User store
^^^^^^^^^^

A :term:`user` must be able to authenticate to Firely Auth before granting permissions to a :term:`client`. 
Therefore we register the users with Firely Auth. Firely Auth supports two types of stores: In memory and SQL Server.

For the InMemory store, the users and their passwords are listed in plain text in this configuration. This is useful for testing, but not recommended for production use.

The SqlServer store stores the users and their encrypted passwords in a MS SQL Server database. 
See :ref:`firely_auth_deploy_sql` for details on setting up the database.

.. code-block:: json

  "UserStore": {
      "Type": "InMemory", // InMemory | SqlServer
      "InMemory": {
          "AllowedUsers": [
              {
                  "Username": "bob",
                  "Password": "password",
                  "Claims": [
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
- ``InMemory``: settings for the InMemory store

  - ``AllowedUsers``: list of users
  - ``Username``: login for a user
  - ``Password``: password for the user, in clear text
  - ``Claims``: currently to be used for a single claim, to link the user to a Patient resource (and thereby to a Patient compartment) in Firely Server. 

    - ``Name``: name of the claim, currently only ``patient`` is supported
    - ``Value``: logical id of the related Patient resource (``Patient/id``)
      In the token this value will be expanded to an absolute url by prepending it with ``FhirServerConfig.FHIR_BASE_URL`` (see :ref:`firely_auth_settings_server`).

- ``SqlServer``: settings for the SQL Server store
  
  - ``ConnectionString``: connection string to the SQL Server database where the users are to be stored. 
    This database and the schema therein must be created beforehand with a script. 

.. _firely_auth_settings_clients:

Clients
^^^^^^^

The ``ClientRegistrationConfig`` is used to register the :term:`clients <client>` that are allowed to request access tokens from Firely Auth.

.. code-block:: json

  "ClientRegistrationConfig": {
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
              "AlwaysIncludeUserClaimsInIdToken": true,
              "RequirePkce": false,
              "AllowOfflineAccess": false,
              "AllowOnlineAccess": false,
              "AllowFirelySpecialScopes": true,
              "RequireClientSecret": true,
              "LaunchIds": [],
              "RefreshTokenLifetime": "30"
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
- ``ClientSecrets``: secrets can be of type ``SharedSecret`` or ``JWK``. You can have multiple of each, so you can accept two secrets for a short period of time to support key rotation and an update window for the client

  - SharedSecret: ``{"SecretType": "SharedSecret", "Secret": "<a secret string shared with the client>"}`` - this can be used for either :term:`client credentials` or :term:`authorization code flow`, but only with a :term:`confidential client`.
  - JWK: ``{"SecretType": "JWK", "SecretUrl": "<JWKS url>"}`` - where the JWKS url hosts a JSON Web Key Set that can be retrieved by Firely Auth, see also :term:`JWK`.
  - JWK: ``{"SecretType": "JWK", "Secret": "<JWK>"}`` - where JWK is the contents of a :term:`JWK`. Use this if the client cannot host a url with a JWKS. 
    Use one entry for each key in the keyset. Note that the JWK json structure is enbedded in a string, so you need to escape the quotes within the JWK.
    The url option above is recommended. 

- ``AllowedGrantTypes``: array of either or both ``"client_credentials"`` and ``"authorization_code"``, referring to :term:`client credentials` and :term:`authorization code flow`. Use ``client credentials`` only for a :term:`confidential client`.
- ``AllowedSmartLegacyActions``: Firely Auth can also still support SMART on FHIR v1, where the actions are ``read`` and ``write``.
- ``AllowedSmartActions``: Actions on resources that can be granted in SMART on FHIR v2: ``c``, ``r``, ``u``, ``d`` and/or ``s``, see `SMART on FHIR V2 scopes`_
- ``AllowedSmartSubjects``: Categories of 'subjects' to which resource actions can be granted. Can be ``system``, ``user`` and/or ``patient``
- ``AlwaysIncludeUserClaimsInIdToken``: true / false: When requesting both an id token and access token, should the user claims always be added to the id token instead of requiring the client to use the userinfo endpoint. Default is false
- ``Require PKCE``: true / false - see :term:`PKCE`. true is recommended for a :term:`public client` and can offer an extra layer of security for :term:`confidential client`.
- ``AllowOfflineAccess``: true / false - Whether app can request refresh tokens while the user is online, see `SMART on FHIR refresh tokens`_
- ``AllowOnlineAccess``: true / false - Whether app can request refresh tokens while the user is offline, see `SMART on FHIR refresh tokens`_. A user is offline if he is logged out of Firely Auth, either manually or by expiration
- ``AllowFirelySpecialScopes``: true / false - Allow app to request scopes for Firely Server specific operations. Currently just 'http://server.fire.ly/auth/scope/erase-operation'
- ``RequireClientSecret``: true / false - In theory you could allow clients without a client secret. That is not recommeded.
- ``LaunchIds``: array of string - a 'launch id' could restrict access based on e.g. some currently active EHR context. Since Firely Auth is not connected to an EHR, this currently can only be set statically.
  Providing an empty array will make Firely Auth accept any launch id sent by the client (including none).  
- ``RefreshTokenLifetime``: If the client is allowed to use a :term:`refresh token`, how long should it be valid? The value is in days. You can also use HH:mm:ss for lower values.


.. _SMART on FHIR V2 scopes: http://hl7.org/fhir/smart-app-launch/scopes-and-launch-context.html#scopes-for-requesting-clinical-data
.. _SMART on FHIR refresh tokens: http://hl7.org/fhir/smart-app-launch/scopes-and-launch-context.html#scopes-for-requesting-a-refresh-token
