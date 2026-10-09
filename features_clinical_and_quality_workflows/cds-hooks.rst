.. _feature_cds_hooks:

=========
CDS Hooks
=========

.. note::

  The features described on this page are available in the following :ref:`Firely Server editions <vonk_overview>`:

  * Firely CMS Compliance - 🇺🇸

  The CDS Hooks branch described on this page is available as of Firely Server 6.11.0. For the implementation in earlier versions, see :ref:`feature_cds_hooks_upgrade`.

.. TODO (open question, check with dev): confirm the editions above.

Description
-----------

CDS Hooks is a specification that allows healthcare applications to integrate with clinical decision support systems. It enables the execution of decision support logic at the point of care, providing clinicians with relevant information and recommendations based on the context of patient care. For background and the specification, please consult the `official CDS Hooks documentation <https://cds-hooks.hl7.org/>`_.

Firely Server implements CDS Hooks 3.0.0 and can host CDS Services: a CDS Client, such as an EHR, calls a service at a point in its workflow (a *hook*, such as ``patient-view`` or ``order-select``), and the service answers with cards or system actions. This is particularly useful for organizations that must conform to regulations based on Implementation Guides using CDS Hooks. Examples include the Da Vinci Implementation Guides for CRD, DTR and PAS, contributing to the electronic Prior Authorization workflow.

Firely Server serves CDS Hooks on two endpoints:

* ``GET <base-url>/cds-services`` returns the discovery document, which lists the CDS Services that are offered.
* ``POST <base-url>/cds-services/{id}`` invokes the CDS Service with that id.

Firely Server ships with :ref:`example services <feature_cds_hooks_examples>`. To offer CDS Services of your own, you write them as a plugin; see :ref:`vonk_reference_api_cds_hooks`.

The CDS Hooks branch handles all requests in the default FHIR version of Firely Server, set in ``InformationModel:Default`` (see :ref:`feature_multiversion`). CDS Hooks is tested with **FHIR R4**, the FHIR version of Da Vinci CRD, so we recommend R4 as the default FHIR version for a deployment that serves CDS Hooks. Other FHIR versions may work, but are not tested.

Key concepts
^^^^^^^^^^^^

**CDS Hooks is served on a branch of its own.** CDS Hooks is not a FHIR RESTful API, so Firely Server serves it on a separate :ref:`pipeline branch <vonk_plugins_config>` at ``/cds-services``. That path is fixed by the specification.

**A CDS Service is identified by its id and its hook together.** The specification allows one id to answer several hooks, so that a service can supersede its own earlier cards as a workflow moves on. Each hook of a service is therefore offered on its own.

**Nothing is offered by default.** A CDS Service receives patient data, so it has to be exposed deliberately. A service is offered only when its id and hook are named in the ``CdsHooks:Services`` section of the appsettings. A service that is installed but not named there is neither listed in the discovery document nor invoked: it answers ``404 Not Found``.

Enabling CDS Hooks
------------------

Enabling CDS Hooks takes three steps.

#. Make sure the license token ``http://fire.ly/server/plugins/cds-hooks`` is present in your :ref:`license file <configure_license>`.

#. Add a branch for ``/cds-services`` to ``PipelineOptions:Branches`` in the appsettings. List ``Vonk.Plugin.CdsHooks.Infra`` first, followed by the CDS Services you want to offer. ``Vonk.Plugin.CdsHooks.Infra`` holds everything needed to serve CDS Hooks, including authorization and validation, but it is not a service itself.

   .. code-block:: JavaScript

     "PipelineOptions": {
       "PluginDirectory": "./plugins",
       "Branches": [
         {
           "Path": "/",
           "Include": [ ... ]
         },
         {
           "Path": "/administration",
           "Include": [ ... ]
         },
         {
           "Path": "/cds-services",
           "Include": [
             "Vonk.Plugin.CdsHooks.Infra",
             "Vonk.Plugin.CdsHooks.Examples.PatientViewTestCdsServiceConfiguration"
           ]
         }
       ]
     }

   ``appsettings.default.json`` contains this branch, commented out.

#. Offer each CDS Service under ``CdsHooks:Services``, with the hooks it answers:

   .. code-block:: JavaScript

     "CdsHooks": {
       "Services": {
         "patient-view-test-hook": {
           "Hooks": {
             "patient-view": { "Enabled": true }
           }
         }
       }
     }

You can now request the discovery document:

.. code-block:: HTTP

   GET <base-url>/cds-services HTTP/1.1
   Accept: application/json

.. code-block:: JavaScript

  {
    "services": [
      {
        "hook": "patient-view",
        "id": "patient-view-test-hook",
        "title": "Test Hook",
        "description": "This is a test hook",
        "prefetch": {
          "patientToGreet": "Patient/{{context.patientId}}"
        },
        "usageRequirements": "none"
      }
    ]
  }

A few things to keep in mind about the branch:

* **Serve CDS Hooks only from its own branch.** Firely Server refuses to start when a branch serves both FHIR REST and CDS Hooks. The easiest way to cause this is to put the entry ``Vonk.Plugin.CdsHooks`` in the branch on ``/``: an ``Include`` entry also takes in every namespace below it, so that entry includes ``Vonk.Plugin.CdsHooks.Infra`` and ``Vonk.Plugin.CdsHooks.Examples`` as well. The log names the entry to use instead.
* **An** ``Include`` **entry must match a configuration class.** An entry matches a configuration class whose full name equals it, or starts with it followed by a dot. An entry that matches nothing fails startup, so a typo is reported rather than ignored.
* **The branch handles requests in** ``InformationModel:Default``. The release-prefixed form ``<base-url>/{release}/cds-services`` is not served, because a branch path is matched before the release is taken off the path. To have subdomain mapping of the FHIR version apply on this branch as well, add ``Vonk.Core.Context.Http.InformationModelEndpointConfiguration`` to its ``Include``.
* **A service reads patient data from the branch on** ``/``. When a CDS Service reads FHIR data from Firely Server, or calls an operation such as ``$member-match``, it does so through the branch that serves patient data. That is the branch on ``/``; without a branch on ``/``, it is the first branch that serves neither the administration API nor ``/cds-services``. Plugins that a CDS Service calls must be included in *that* branch, not in ``/cds-services``.

Configuring CDS Services
------------------------

All settings for CDS Hooks are in the ``CdsHooks`` section of the appsettings. The full section looks like this:

.. code-block:: JavaScript

  "CdsHooks": {
    "Services": {
      "example-service": {
        "Authorization": {
          "Enabled": true
        },
        "Hooks": {
          "patient-view": { "Enabled": true },
          "order-sign": {
            "Enabled": true,
            "RequestValidation": {
              "Enabled": false,
              "Profile": "http://fire.ly/fhir/StructureDefinition/CdsHooksRequest"
            },
            "ResponseValidation": {
              "Enabled": false,
              "Profile": "http://fire.ly/fhir/StructureDefinition/CdsHooksResponse"
            }
          }
        }
      }
    },
    "RoutedRequestHeaders": [ "x-firely-tenant" ],
    "Authorization": {
      "Enabled": false,
      "GuardDiscovery": true,
      "Authority": "",
      "ClientId": "",
      "ClientSecret": "",
      "ReplayGuardCapacity": 100000
    }
  }

Offering a service
^^^^^^^^^^^^^^^^^^

``CdsHooks:Services:<id>:Hooks:<hook>`` offers the service with that id for that hook. Naming the hook is enough: ``"patient-view": {}`` offers it, because ``Enabled`` is ``true`` by default. Set ``Enabled`` to ``false`` to keep the entry while taking that hook of the service offline.

A pair that is not offered is left out of the discovery document and answers ``404 Not Found`` when invoked. If a service is installed but not offered, Firely Server logs the exact setting to add, for example:

.. code-block:: text

   The CDS Hooks service 'patient-view-test-hook' has a handler for hook 'patient-view' but is not offered for it,
   because it is not enabled in configuration. Add 'CdsHooks:Services:patient-view-test-hook:Hooks:patient-view' to offer it.

Both the id and the hook are case-sensitive.

Validating requests and responses
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Firely Server can validate the CDS Hooks payloads of a service, per hook. This is configured next to the hook, because which model a request conforms to depends on the hook. Both directions are off by default.

``RequestValidation``
    The request is validated before the service sees it. A non-conformant request is refused with ``400 Bad Request`` and an ``OperationOutcome``. This is the CDS Hooks counterpart of :ref:`prevalidation <feature_prevalidation>`.

``ResponseValidation``
    The response of the service is validated before it is sent. Issues are reported in the log, but the response is sent as the service decided.

Each has its own ``Enabled`` switch and an optional ``Profile``. Without a ``Profile``, the payload is validated against the base CDS Hooks logical model for a request or response. Set ``Profile`` to the canonical URL of a constrained model, such as the request model of a CRD hook, to hold that hook's payloads to it. Profiles other than the base models are not shipped with Firely Server; load them into the administration database yourself.

The FHIR resources inside the payload, such as the prefetched resources, are validated in the same pass.

Carrying headers to FHIR requests
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

A CDS Service may read FHIR data from Firely Server while it handles a request. That FHIR request runs inside Firely Server, through the same pipeline as a request over HTTP, so the :ref:`audit trail <feature_auditing>` applies to it. When authorization is enabled, the CDS Client is recorded as the agent on the ``AuditEvent``.

``CdsHooks:RoutedRequestHeaders`` lists the headers of the CDS request that are copied to such a FHIR request. When the setting is absent, only the tenant header is copied, so that :ref:`multi-tenancy <feature_multitenancy>` works. An empty list copies no headers at all. Most headers a CDS Client sends describe the CDS request rather than the FHIR request, and some, like ``Prefer``, would change what the FHIR request returns, so only add a header when you know why it is needed. ``Authorization`` cannot be copied; Firely Server refuses to start if it is listed.

.. note::

  Such a FHIR request is not restricted by SMART on FHIR scopes or by an AccessPolicy. What it may access is stated in the code of the CDS Service. When you deploy a CDS Service from a third party, check what it reads and writes.

.. _feature_cds_hooks_authorization:

Authorizing CDS Clients
-----------------------

Firely Server can require CDS Clients to authenticate, as described in the section *Trusting CDS Clients* of the `specification <https://cds-hooks.hl7.org/>`_. The CDS Client signs a JWT and sends it as a bearer token in the ``Authorization`` header. Authorization is off by default.

.. warning::

  With authorization off, anyone who can reach ``/cds-services`` can invoke the CDS Services you offer, including services that return information based on patient data. Enable authorization for any deployment that is reachable by more than trusted systems.

Prerequisite: Firely Auth
^^^^^^^^^^^^^^^^^^^^^^^^^

Firely Server does not validate the token itself. It sends the token to Firely Auth for verification, at the endpoint ``POST {authority}/connect/cdsIntrospect``. The authorization server you configure must therefore be :ref:`Firely Auth <feature_accesscontrol_idprovider>` 4.8.0 or later, which serves that endpoint. With an earlier version, every invocation is refused, and the log says that introspection failed.

Register each CDS Client in Firely Auth as a client of the type **CDS Hooks Service Client**, with the issuer of its JWTs and its public key, either as a single JWK or as a JWKS URL. Firely Auth checks the signature, the registration and the time claims of the token. Firely Server then checks:

* **The audience.** The ``aud`` of the token must be the URL of the endpoint being called: ``<base-url>/cds-services/{id}`` for an invocation, ``<base-url>/cds-services`` for discovery.
* **Replay.** Each token can be used only once. The ``jti`` of every accepted token is remembered until the token expires. A second request with the same token is refused, and so is a token without a ``jti`` or ``exp``.

A refused request is answered with ``401 Unauthorized``.

Settings
^^^^^^^^

``CdsHooks:Authorization:Enabled``
    ``false`` by default. With ``true``, every invocation of a CDS Service needs a valid token, unless the service is exempted (see below).

``CdsHooks:Authorization:GuardDiscovery``
    ``true`` by default: discovery needs a token as well, with ``<base-url>/cds-services`` as its audience. Set it to ``false`` to serve the discovery document to anyone while invocations stay guarded, for example for CDS Clients that need to read the list of services before they can obtain a token for one. The specification permits both. This setting has no effect while ``Enabled`` is ``false``.

``CdsHooks:Authorization:Authority``, ``ClientId`` and ``ClientSecret``
    The Firely Auth instance, and the credentials Firely Server introspects tokens with. When left empty, they fall back to ``SmartAuthorizationOptions:Authority``, ``SmartAuthorizationOptions:TokenIntrospection:ClientId`` and ``SmartAuthorizationOptions:TokenIntrospection:ClientSecret`` (see :ref:`feature_accesscontrol_config`). Firely Auth needs no separate client registration for this, so usually you leave these empty.

    The three fall back together, not one by one: ``ClientId`` and ``ClientSecret`` only fall back to the SMART settings when ``Authority`` is empty or equal to the SMART authority. If you set a different ``Authority``, also set its ``ClientId`` and ``ClientSecret``.

    If any of the three is missing while authorization is enabled, Firely Server refuses to start, and the log names the missing settings.

``CdsHooks:Authorization:RequireHttpsToProvider``
    Whether the authority must use ``https``. Falls back to ``SmartAuthorizationOptions:RequireHttpsToProvider``, which is ``true`` by default. With ``true``, a non-https authority prevents Firely Server from starting.

``CdsHooks:Authorization:ReplayGuardCapacity``
    The number of used tokens remembered to detect replay, ``100000`` by default. It must be greater than zero. See the limitations below.

To serve one service without a token while authorization is enabled, set ``Authorization:Enabled`` to ``false`` for that service:

.. code-block:: JavaScript

  "CdsHooks": {
    "Services": {
      "patient-view-test-hook": {
        "Authorization": { "Enabled": false },
        "Hooks": { "patient-view": {} }
      }
    }
  }

This switch is per service, not per hook, because the token is checked before the request body that names the hook is read. It has no effect while ``CdsHooks:Authorization:Enabled`` is ``false``. There are no scopes for CDS Services.

Limitations
^^^^^^^^^^^

* **Introspection is not cached.** Every invocation makes one call to Firely Auth. This adds the latency of one network round-trip per invocation, and CDS Hooks depends on Firely Auth being available. This is deliberate: every token can be used only once anyway, and caching a positive result would only widen the window in which a revoked token is still accepted.
* **The replay check is per instance.** The used tokens are remembered in memory. In a load-balanced deployment with several instances of Firely Server, a token replayed against another instance is not detected.
* **The replay check has a capacity.** When more than ``ReplayGuardCapacity`` unexpired tokens are remembered, the least recently used are forgotten, and such a token could be used a second time. Firely Server logs a warning when this happens, at most once every five minutes. Roughly, you need one entry per request per token lifetime: 500 requests per second with tokens valid for five minutes needs 150,000.
* **A token is used up even if the request fails.** The token is recorded as used before the service is invoked. A request that fails, for example with a ``404`` or a ``400`` from validation, still uses the token, and a CDS Client that retries with the same token gets ``401 Unauthorized``.

.. _feature_cds_hooks_examples:

Example CDS Services
--------------------

The plugin ``Vonk.Plugin.CdsHooks.Examples`` contains two example CDS Services. Each has its own configuration class, so you can offer one without the other.

.. list-table::
   :header-rows: 1
   :widths: 25 20 55

   * - Service id
     - Hook
     - Configuration class
   * - ``patient-view-test-hook``
     - ``patient-view``
     - ``Vonk.Plugin.CdsHooks.Examples.PatientViewTestCdsServiceConfiguration``
   * - ``crd-order-select-hook``
     - ``order-select``
     - ``Vonk.Plugin.CdsHooks.Examples.CrdOrderSelectCdsServiceConfiguration``

Including ``Vonk.Plugin.CdsHooks.Examples`` in the branch includes both. ``appsettings.default.json`` contains the ``CdsHooks:Services`` entries for both services, commented out.

Patient view
^^^^^^^^^^^^

``patient-view-test-hook`` greets the patient in view with a single card. It needs no other plugins or license tokens. With the configuration from `Enabling CDS Hooks`_, you can invoke it:

.. code-block:: HTTP

    POST <base-url>/cds-services/patient-view-test-hook HTTP/1.1
    Accept: application/json
    Content-Type: application/json

    {
        "hookInstance": "d1577c69-dfbe-44ad-ba6d-3e05e953b2ea",
        "hook": "patient-view",
        "context": {
            "userId": "Practitioner/example",
            "patientId": "example"
        },
        "prefetch": {
            "patientToGreet": {
                "resourceType": "Patient",
                "id": "example",
                "name": [ { "family": "Doe", "given": [ "John" ] } ],
                "gender": "male",
                "birthDate": "1974-12-25"
            }
        }
    }

The service answers with one card:

.. code-block:: JavaScript

    {
        "cards": [
            {
                "uuid": "0a1b7c3e-4f0e-4d9a-9b0e-3f3c1a2d5e6f",
                "summary": "Hello World! Firely Server loves FHIR and CDS Hooks!",
                "detail": "Hello John Doe (male, 1974-12-25)!",
                "indicator": "info",
                "source": {
                    "label": "Firely Server",
                    "url": "<base-url>/"
                }
            }
        ]
    }

If ``patientToGreet`` is missing from the prefetch, or its id does not match ``context.patientId``, the service answers ``412 Precondition Failed`` with an ``OperationOutcome``.

Coverage Requirements Discovery
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

``crd-order-select-hook`` is a Da Vinci Coverage Requirements Discovery (CRD) service on the ``order-select`` hook. It answers a draft ``ServiceRequest`` with coverage information, returned as a ``systemActions`` update of that order.

This service is a demonstration: its coverage logic is specific to Firely's prior authorization demo, and a payer implementing CRD writes coverage rules of its own. To run it, you need:

* the license token ``http://fire.ly/server/plugins/crd``, in addition to the CDS Hooks token;
* the CQL plugin and ``Vonk.Plugin.MemberMatch`` in the branch that serves patient data (the branch on ``/``), because the service calls ``$member-match`` and ``$evaluate`` there;
* the CQL libraries ``CrdExample``, ``CrdPayorContactDetails`` and ``PriorAuthQuestionnaire`` in Firely Server.

Building your own CDS Service
-----------------------------

You write your own CDS Service as a Firely Server plugin, and offer it in the same way as the examples: include its configuration class in the ``/cds-services`` branch and name it under ``CdsHooks:Services``. See :ref:`vonk_reference_api_cds_hooks` for the programming API, with a complete example.

Known limitations
-----------------

* The feedback endpoint, ``POST <base-url>/cds-services/{id}/feedback``, is recognized but answers ``501 Not Implemented``.
* ``<base-url>/{release}/cds-services`` is not served.
* See also the :ref:`limitations of authorization <feature_cds_hooks_authorization>`.

.. _feature_cds_hooks_upgrade:

Upgrading from earlier versions
-------------------------------

Before Firely Server 6.11.0, CDS Hooks was served by mapping each CDS Hooks request onto a FHIR custom operation, in the branch on ``/``. That implementation is still shipped, as the plugin ``Vonk.Plugin.CdsHooks.Legacy``, so that an upgrade does not have to wait for the migration. It will be removed in the next major version. **Treat it as time to migrate, not as a second option to build on.**

A deployment runs either the CDS Hooks branch or the legacy plugin, never both: they claim the same route.

Staying on the legacy plugin for now
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

If you upgrade without migrating yet, two changes are required.

#. **Rename the plugin in the** ``Include`` **of the branch on** ``/``. The plugin ``Vonk.Plugin.CdsHooks`` is renamed to ``Vonk.Plugin.CdsHooks.Legacy``, and its namespaces with it. Replace ``Vonk.Plugin.CdsHooks``, and any entry naming one of its namespaces such as ``Vonk.Plugin.CdsHooks.Configuration``, with the new name, for example ``Vonk.Plugin.CdsHooks.Legacy.Configuration``. The old entry would now also include the new CDS Hooks branch plugins, and Firely Server refuses to start with it.

#. **Rename two StructureDefinitions in the administration database.** The legacy plugin wraps a CDS Hooks request in a custom FHIR resource type, defined by a ``StructureDefinition`` in the administration database. Those types are renamed from ``CDSHooksRequest`` to ``CDSHooksRequestOld`` and from ``CDSHooksResponse`` to ``CDSHooksResponseOld``. Update the ``type``, ``name``, ``url`` and element paths of both definitions. Until you do, every CDS Hooks request is refused by structural validation. Nothing a CDS Client sends or receives changes.

Firely Server now uses the names ``CdsHooksRequest``, ``CdsHooksResponse``, ``CdsHooksCard``, ``CdsHooksAction``, ``CdsHooksSuggestion``, ``CdsHooksLink``, ``CdsHooksSource``, ``CdsHooksContext``, ``CdsHooksPrefetch``, ``CdsHooksPrefetchTemplates``, ``CdsHooksExtensions``, ``CdsHooksFhirAuthorization``, ``CdsHooksDiscoveryResponse`` and ``CdsHooksServiceDefinition`` itself. FHIR type names are matched case-insensitively, so a custom resource type or datatype with one of these names is refused on import, with a warning in the log at startup. Rename such a type before upgrading.

Migrating to the CDS Hooks branch
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

To move a deployment to the CDS Hooks branch:

#. Remove ``Vonk.Plugin.CdsHooks.Legacy`` from the branch on ``/``.
#. Add the ``/cds-services`` branch and the ``CdsHooks:Services`` entries, as described in `Enabling CDS Hooks`_. The example services have the same ids and hooks as the legacy ones, so CDS Clients do not need to change.
#. Remove the ``$cds-<id>`` entries from the ``Operations`` section of the appsettings. They are not used by the CDS Hooks branch.
#. You can remove the ``CDSHooksRequestOld`` and ``CDSHooksResponseOld`` StructureDefinitions from the administration database. The CDS Hooks branch takes the CDS Hooks payloads as they are.
#. Rewrite custom CDS Services against the new programming API. See the migration table in :ref:`vonk_reference_api_cds_hooks`.
