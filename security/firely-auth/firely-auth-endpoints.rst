.. _firely_auth_endpoints:

Firely Auth Endpoints
=====================

Firely Auth provides a endpoints for a variety of different operations related to the management of OAuth clients and tokens.
The following section describes the REST API for these endpoints and summaries the intention of these interactions. For more details please refer to the corresponding RFCs.

OpenID Configuration
--------------------

Similar to a CapabilityStatement in Firely Server, Firely Auth offers an endpoint to inspect and verify the available capabilities.
The OpenID configuration endpoints returns a JSON document containing:

* URLs of all available endpoints of the service
* A URL pointing to the key material used to sign the access and identity tokens from Firely Auth, wrapped in a Json Web Key Set
* Additional flags to indentify enabled features (e.g. supported grant types, supported signing algorithms)

.. note::
    SMART on FHIR provides a compositional syntax for creating scopes, i.e. basic patient/user/system-scopes can be combined with search parameters to create more fine-granular scopes.
    Therefore not all combinations of supported scopes can be exposed in the "scopes_supported" element of the OpenID configuration.

For more information, see `Duende Documentation - Discovery Endpoint <https://docs.duendesoftware.com/identityserver/v6/reference/endpoints/discovery/>`_.

Introspection endpoint
----------------------

It is not uncommon that OAuth 2.0 clients do not contain functionality for checking the validity of a provided token, especially if the client is a webclient with reduced support for cryptographic libraries.
Firely Auth provides an token introspection endpoint conforming to `RFC7662 <https://www.rfc-editor.org/rfc/rfc7662>`_ enabling to  to determine the active state token and the meta-information about a token.
This endpoint is actively used by Firely Server in case reference tokens are being provided by a FHIR REST API client.

The introspection endpoint can be access via an HTTP POST request and is protected with the secret provided in the :ref:`firely_auth_settings_tokenintro` setting. The token to be inspected can be provided via the HTTP request body via x-www-urlencoded parameters.

.. code-block::

    POST /connect/introspect
    Authorization: Basic xxxyyy
    token=<token>

A successful response will return a status code of 200 and either an active or inactive token:

.. code-block:: json

    {
        "active": true,
        "sub": "123"
    }

Unknown or expired tokens will be marked as inactive:

.. code-block:: json

    {
        "active": false,
    }

An invalid request will return a 400, an unauthorized request 401.

Additionally, a valid request will contain meta-information about the token, including:

iss: 
    String representing the issuer of this token, as defined in JWT [RFC7519].

exp: 
    Integer timestamp, measured in the number of seconds since January 1 1970 UTC, indicating when this token will expire, as defined in JWT [RFC7519].

aud: 
    Service-specific string identifier or list of string identifiers representing the intended audience for this token, as defined in JWT [RFC7519].

client_id: 
    Client identifier for the OAuth 2.0 client that requested this token.

sub: 
    Subject of the token, as defined in JWT [RFC7519]. Usually a machine-readable identifier of the resource owner who authorized this token.

scope: 
    A JSON string containing a space-separated list of scopes associated with this token, in the format described in Section 3.3 of OAuth 2.0 [RFC6749].

active: 
    Boolean indicator of whether or not the presented token is currently active.

.. note::
    Uing the introspection endpoint with reference tokens is the only way enabling a reliable way of revoking access tokens.
    Reference tokens will be checked by Firely Server on every request for validity and activeness. JWT tokens on the other hand will be valid until they expire.   
    
For more information, see `Duende Documentation - Introspection Endpoint <https://docs.duendesoftware.com/identityserver/v6/reference/endpoints/introspection/>`_.
