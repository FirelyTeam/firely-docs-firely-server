.. _firely_auth_endpoints:

Available endpoints
===================

Firely Auth provides endpoints for a variety of different operations related to the management of OAuth clients and tokens.
The following section describes the REST API for these endpoints and summaries the intention of these interactions. For more details please refer to the corresponding RFCs.

OpenID Configuration
--------------------

Similar to a CapabilityStatement in Firely Server, Firely Auth offers an endpoint to inspect and verify the available capabilities.
The OpenID configuration endpoints returns a JSON document containing:

* URLs of all available endpoints of the service
* A URL pointing to the key material used to sign the access and identity tokens from Firely Auth, wrapped in a Json Web Key Set
* Additional flags to identify enabled features (e.g. supported grant types, supported signing algorithms)

.. note::
    SMART on FHIR provides a compositional syntax for creating scopes, i.e. basic patient/user/system-scopes can be combined with search parameters to create more fine-granular scopes.
    Therefore not all combinations of supported scopes can be exposed in the "scopes_supported" element of the OpenID configuration.

For more information, see `Duende Documentation - Discovery Endpoint <https://docs.duendesoftware.com/identityserver/v6/reference/endpoints/discovery/>`_.

Introspection endpoint
----------------------

It is not uncommon that OAuth 2.0 clients do not contain functionality for checking the validity of a provided token, especially if the client is a webclient with reduced support for cryptographic libraries.
Firely Auth provides an token introspection endpoint conforming to `RFC7662 <https://www.rfc-editor.org/rfc/rfc7662>`_ enabling clients to determine the validity and metadata of the token.
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

.. _firely_auth_endpoints_launchcontext:

LaunchContext endpoint
----------------------

In SMART on FHIR's EHR launch flow, a ``launch`` URL parameter is required when the EHR initiates a launch sequence. It is an identifier for this specific launch and any EHR context associated with it. For more information, see `EHR Launch <https://hl7.org/fhir/smart-app-launch/app-launch.html#launch-app-ehr-launch>`_.
Firely Auth offers an endpoint to request such identifier. 

The ``launchContext`` endpoint can be accessed via an HTTP POST request and is protected with the secret provided in the :ref:`firely_auth_settings_tokenintro` setting. The username used for basic auth is the same as the name of the FHIR Server. The password used for basic auth is the same as the introspection secret. The EHR context to be associated with can be provided via the HTTP request body via x-www-urlencoded parameters. FHIR resource ids that are of interest for the EHR launch can be submitted by the EHR to Firely Auth in the form of ``<resourceType>=<id>``. Note that no "launch" prefix is used for the resourceType.
Example below requests a ``launch`` identifier with ``patient`` context associated.

.. code-block::

    POST /connect/launchContext
    Authorization: Basic xxxyyy
    patient=<patient-id>

A successful response will return a status code of 200 and a ``launch`` identifier:

.. code-block:: json

    {
        "launchContextIdentifier": "b0599233-8548-4d56-ae4a-d31babc4ab82"
    }

An unauthorized request will return a 401.

Known Limitations
-----------------

* In Firely Auth no Backchannel Authentication Endpoint is available, therefore Client Initiated Backchannel Authentication (CIBA) requests are not supported. For more information, see `Duende Documentation - Client Initiated Backchannel Authentication (CIBA) <https://docs.duendesoftware.com/identityserver/v6/reference/endpoints/ciba/>`_.
* A Device Authorization Flow is not supported by SMART on FHIR. Therefore it is not available in Firely Auth. For more information, see `Duende Documentation - Device Authorization Endpoint <https://docs.duendesoftware.com/identityserver/v6/reference/endpoints/device_authorization/>`_.

Liveness and readiness
----------------------

It can be useful to check whether Firely Auth is still up and running, and ready to handle requests. Either just for notification, or for automatic failover.
A prominent use case is to start dependent services only after Firely Auth is up and running, e.g. in a docker-compose or in a Helm chart.

Firely Auth provides two endpoints, for different purposes:

* ``GET <base>/$liveness``
* ``GET <base>/$readiness``

These align - intentionally - with the use of liveness and readiness probes in Kubernetes, see `Probes <https://kubernetes.io/docs/concepts/configuration/liveness-readiness-startup-probes/>`_.

Results
-------

The ``$liveness`` operation may return one of these http status codes:

#. 200 OK: Firely Auth is up and running.
#. 402 Payment Required: The license is expired or otherwise invalid.
#. 500 or higher: An unexpected error happened, the server is not running or not reachable (in the latter case the error originates from a component in front of Firely Auth).

The ``$readiness`` operation may return one of these http status codes:

#. 200 OK: Firely Auth is up and running and ready to process requests.
#. 402 Payment Required: The license is expired or otherwise invalid.
#. 500 or higher: An unexpected error happened, the server is not running or not reachable (in the latter case the error originates from a component in front of Firely Auth).
