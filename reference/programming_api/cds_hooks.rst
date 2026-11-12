.. _vonk_reference_api_cds_hooks:

Writing a CDS Service
=====================

.. note::

  The features described on this page are available in the following :ref:`Firely Server editions <vonk_overview>`:

  * Firely Scale - 🌍 / 🇺🇸
  * Firely CMS Compliance - 🇺🇸

This page explains how to implement your own CDS Service as a Firely Server plugin. A CDS Service is the server side of the `CDS Hooks specification <https://cds-hooks.hl7.org/>`_: a CDS Client, such as an EHR, calls it at a point in the clinical workflow (a *hook*), and the service answers with cards or system actions.

Firely Server implements CDS Hooks 3.0.0. What you write is the logic of the service itself. Firely Server takes care of the endpoints, the discovery document, parsing and serializing the CDS Hooks JSON, and optionally authorization and validation of the payloads.

For enabling CDS Hooks in a deployment, and for configuring authorization and validation, see :ref:`feature_cds_hooks`. If you are new to writing plugins, read :ref:`vonk_plugins` first.

CDS Hooks is tested with FHIR R4, the FHIR version of Da Vinci CRD. The CDS Hooks branch handles all requests in the default FHIR version of Firely Server; see :ref:`feature_cds_hooks` for details.

How a CDS request flows through Firely Server
---------------------------------------------

CDS Hooks is not a FHIR RESTful API, so Firely Server serves it on a :ref:`pipeline branch <vonk_plugins_config>` of its own, mounted at ``/cds-services``. That path is fixed by the specification. The branch offers two endpoints:

* ``GET <base-url>/cds-services`` returns the discovery document, listing every CDS Service that is offered.
* ``POST <base-url>/cds-services/{id}`` invokes the CDS Service with that id.

A request on the CDS Hooks branch passes through a chain of middleware, ordered by the ``order`` of the configuration class that registers it. Middleware with a lower order runs earlier on the request, and later on the response.

.. code-block:: text

    order 1110  Response writer        writes the status and the body: the CDS Hooks response, or an OperationOutcome
    order 1115  Envelope               resolves the endpoint and builds the CdsServiceContext or CdsDiscoveryContext
    order 1116  Authorization          checks the token, when CdsHooks:Authorization:Enabled is true
    order 1120  Request                checks license, method, media types; reads the body; refuses a request without a hook
    order 1130  Discovery              on GET only: assembles the discovery document
    order 1140  Request typing         gives the request the types its logical model declares
    order 1150  Validation             validates request and response, when enabled for the service and hook
                ------------------------------------------------------------------------------------------
    1151-7999   Your CDS Service       your handlers are registered here
                ------------------------------------------------------------------------------------------
    order 8000  Not handled            nothing answered: 404

Three rules follow from this design and come back throughout this page:

#. **A CDS Service is identified by its id and its hook together.** The specification allows one id to answer several hooks, so that a service can replace the cards of an earlier call when the workflow reaches a later step. A service that answers several hooks is registered once per hook.
#. **A service is offered only when configuration says so.** A service that is registered in code but not named under ``CdsHooks:Services`` in the appsettings is not listed in the discovery document and is not invoked: it answers 404. A service receiving patient data has to be exposed deliberately.
#. **A handler states its own result.** The pipeline does not assume success. See :ref:`vonk_reference_api_cds_hooks_status`.

Describing and handling a CDS Service
-------------------------------------

A CDS Service does two separate things, and Firely Server registers them separately:

* **Describing** the service to CDS Clients, so it appears in the discovery document. You do this by implementing ``ICdsDiscoveryContributor``.
* **Handling** an invocation of the service. You do this by registering a handler with ``app.OnCdsService(...)``. No interface is needed for this.

One class may do both, and that is the usual case. Because the registrations are separate, either one can also exist without the other:

* A service that is described but has no handler answers 404, the same as a service that does not exist.
* A service that has a handler but is not described still answers when called directly, but does not appear in the discovery document.

This is the same split that FHIR operations make between ``ICapabilityStatementContributor`` (see :ref:`vonk_reference_api_capabilities`) and their handler.

Describing a service
--------------------

:namespace: Vonk.Core.CdsHooks.Pluggability

Implement ``ICdsDiscoveryContributor`` and add a ``CdsHooksServiceDefinition`` to the builder that Firely Server passes in:

.. code-block:: csharp

    public sealed class MyCdsService : ICdsDiscoveryContributor
    {
        public static readonly CdsHooksServiceDefinition Definition = new()
        {
            Id = "my-service",
            Hook = "patient-view",
            Title = "My service",
            Description = "What my service does",
            Prefetch = new CdsHooksPrefetchTemplates { ["patient"] = "Patient/{{context.patientId}}" },
        };

        public void ContributeToDiscovery(ICdsDiscoveryDocumentBuilder builder)
        {
            builder.AddService(Definition);
        }
    }

``CdsHooksServiceDefinition`` has the members the specification defines for a service in the discovery document: ``Id``, ``Hook``, ``Title``, ``Description``, ``Version``, ``Prefetch``, ``UsageRequirements`` and ``Extension``. ``Id`` and ``Hook`` are required; a definition without them is left out of the document and logged.

Hold the definition in a single ``public static readonly`` field, as above. You pass the same definition to ``OnCdsService`` when you register the handler, so the discovery document and the handler always use the same id and hook.

Every registered contributor is asked to contribute each time the discovery document is assembled. The order in which contributors are asked is not guaranteed. When two contributions claim the same id for the same hook:

* at startup, Firely Server refuses to start, and the log names the clash;
* at request time, the first is kept and the second is logged and left out.

Using the same id for *different* hooks is not a clash.

Handling a service
------------------

:namespace: Vonk.Core.CdsHooks.Pluggability

Register the handler from the ``Configure`` method of your configuration class, with the ``OnCdsService`` extension method on ``IApplicationBuilder``:

.. code-block:: csharp

    public static IApplicationBuilder Configure(IApplicationBuilder builder)
    {
        builder.OnCdsService(MyCdsService.Definition)
            .HandleAsyncWith<MyCdsService>((svc, ctx) => svc.HandleAsync(ctx));
        return builder;
    }

``HandleAsyncWith<T>`` resolves ``T`` from the dependency injection container for each request, so ``T`` must be registered in ``ConfigureServices`` (see :ref:`vonk_reference_api_cds_hooks_configuration`). Its constructor can take any registered service as a dependency.

Choosing the requests
^^^^^^^^^^^^^^^^^^^^^

There are four entry points:

``OnCdsService(CdsHooksServiceDefinition definition)``
    Handles invocations of one service for one hook, taking the id and the hook from the definition. Prefer this overload.

``OnCdsService(string serviceId, string hook)``
    The same, with the id and hook spelled out. There is no overload that takes an id alone. Both the id and the hook are matched case-sensitively.

``OnAnyCdsService()``
    Handles every invocation, whichever service it is for. Use it for cross-cutting concerns such as auditing or extra checks.

``OnCdsDiscovery()``
    Handles the discovery request. By the time this handler runs, the document has already been filled from all ``ICdsDiscoveryContributor`` implementations, so a handler here can edit it before it is sent.

Each of these returns a ``CdsAppBuilder<TContext>``, which you can narrow further:

* ``And(Func<TContext, bool> filter)`` adds a filter of your own.
* ``AndInformationModel(string informationModel)`` limits the handler to one FHIR version, for example ``VonkConstants.Model.FhirR4``.

If the id and hook passed to ``OnCdsService`` are not offered under ``CdsHooks:Services``, the handler is registered but never invoked. Firely Server then logs the setting to add; see :ref:`feature_cds_hooks_offering`.

Pre-, main and post handlers
^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Like the :ref:`FHIR REST pipeline <vonk_reference_api_interactionhandlerfluent>`, the CDS Hooks branch has three kinds of handlers. Each has an async and a sync variant:

``PreHandleAsyncWith<T>`` / ``PreHandleWith<T>``
    Runs before the handlers. A pre-handler that sets ``HttpResult`` stops the chain there.

``HandleAsyncWith<T>`` / ``HandleWith<T>``
    Handles the request. The first handler that accepts the request ends the chain. A service's own logic goes here.

``PostHandleAsyncWith<T>`` / ``PostHandleWith<T>``
    Runs after the rest of the chain. It sees the finished response and may still change it.

A post-handler wraps the rest of the chain: it waits for everything registered at a higher order and then runs. So a post-handler that should see the result of another handler must be registered at a **lower** order than that handler.

Register your configuration class at an order above 1150 and below 8000. Your handlers then see a request whose license, method, media types and body have been checked, and that has been typed and, when enabled, validated. Request typing (1140) and validation (1150) are pre-handlers themselves: a service registered at a lower order answers before they run, so its requests are not validated. Validation of the response is a post-handler at 1150, so it only sees the responses of services registered above it.

.. _vonk_reference_api_cds_hooks_context:

CdsServiceContext
-----------------

:namespace: Vonk.Core.CdsHooks

A handler receives a ``CdsServiceContext``: everything about one invocation of a CDS Service. It plays the role that ``IVonkContext`` plays in the FHIR REST pipeline.

``ServiceId``
    The id of the service being invoked: the last segment of ``<base-url>/cds-services/{id}``.

``Hook``
    The hook of this invocation, for example ``patient-view``. A shortcut to ``Request.Hook``. It is never null: the pipeline refuses a request without a hook before any handler runs.

``Request``
    The parsed request, a ``CdsHooksRequest``. See :ref:`vonk_reference_api_cds_hooks_model`.

``Response``
    The response being built, a ``CdsHooksResponse``. Handlers add to it, for example with ``context.Response.Cards.Add(...)``. It cannot be replaced, so no handler can silently discard the work of another.

``HttpResult``
    The HTTP status code to answer with. See :ref:`vonk_reference_api_cds_hooks_status`.

``Outcome``
    An ``OperationOutcome`` collecting the issues found while handling the request.

``FhirResourceRoot``
    The base URL of this Firely Server, which absolute URLs in FHIR resources resolve against. Note that this is *not* ``Request.FhirServer``, which is the CDS Client's FHIR server.

``InformationModel``
    The FHIR version this request is handled in, for example ``Fhir4.0``.

``RoutedRequestHeaders``
    The headers of the CDS request that are carried along when the service reads from this server. See :ref:`vonk_reference_api_cds_hooks_fhiraccess`.

``Features``
    An ``IFeatureCollection`` for request-scoped extras, for example the ``HttpContext``.

A handler on ``OnCdsDiscovery()`` receives a ``CdsDiscoveryContext`` instead. It has the same ``HttpResult``, ``Outcome``, ``FhirResourceRoot`` and ``InformationModel``, and a ``Response`` that holds the ``CdsHooksDiscoveryResponse``. That ``Response`` is settable, but setting it replaces the whole document. Adding to or editing the existing document is almost always what you want.

.. _vonk_reference_api_cds_hooks_status:

Signalling success and failure
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

A handler that has answered **must** set ``HttpResult``, **including on success**:

.. code-block:: csharp

    context.HttpResult = StatusCodes.Status200OK;

The pipeline does not infer success. If a request reaches the response writer without a status, Firely Server answers ``500 Internal Server Error``: no handler answered and no handler refused. So a handler that does not set the status is found in its first test. It does not return an empty response with status 200.

To refuse a request, set an error status and add an issue to ``Outcome``. The issues are built from Firely Server's ``VonkIssue`` catalogue:

.. code-block:: csharp

    context.Outcome.Issue.Add(VonkIssue.PROCESSING_ERROR.CloneWithDetails("No patient provided in prefetch."));
    context.HttpResult = StatusCodes.Status412PreconditionFailed;
    return;

The specification names ``412 Precondition Failed`` for the case where the CDS Client did not send FHIR data the service needs, such as a missing prefetch entry.

For any status of 400 or above, ``Outcome`` is written as the response body. For any other status, the ``Response`` is written, and ``Outcome`` is only logged, because a CDS Hooks response cannot contain an ``OperationOutcome``.

.. _vonk_reference_api_cds_hooks_model:

The CDS Hooks object model
--------------------------

:namespace: Vonk.Core.CdsHooks

Firely Server provides classes for the CDS Hooks request, response and discovery document, with plain .NET properties. You work with these objects, not with JSON.

``CdsHooksRequest``
    ``Hook``, ``HookInstance``, ``FhirServer``, ``FhirAuthorization``, ``Context``, ``Prefetch`` and ``Extension``.

``CdsHooksResponse``
    ``Cards`` (a list of ``CdsHooksCard``) and ``SystemActions`` (a list of ``CdsHooksAction``).

``CdsHooksCard``
    ``Uuid``, ``Summary``, ``Detail``, ``Indicator``, ``Source``, ``Suggestions``, ``SelectionBehavior``, ``OverrideReasons``, ``Links`` and ``Extension``. Related classes are ``CdsHooksSource``, ``CdsHooksSuggestion``, ``CdsHooksAction`` and ``CdsHooksLink``, with the enumerations ``CdsHooksIndicator``, ``CdsHooksSelectionBehavior``, ``CdsHooksActionType`` and ``CdsHooksLinkType``.

``CdsHooksDiscoveryResponse``
    ``Services``, a list of ``CdsHooksServiceDefinition``.

The members of ``context``, ``prefetch`` and ``extension`` are not fixed by the specification: each hook, or an implementation guide, names its own. These classes therefore behave as dictionaries.

* ``CdsHooksContext`` reads values with ``GetString(key)``, ``GetInt(key)``, ``GetBool(key)``, ``GetStrings(key)``, ``Get<T>(key)`` and ``GetAll<T>(key)``, where ``T`` is a FHIR type. For example, ``context.Request.Context.GetString("patientId")``.
* ``CdsHooksPrefetch`` holds the prefetched FHIR resources as POCOs. Use ``TryGet(key, out Resource? resource)`` to read one:

  .. code-block:: csharp

      if (context.Request.Prefetch is { } prefetch
          && prefetch.TryGet("patient", out var prefetched)
          && prefetched is Patient patient)
      {
          // use patient
      }

  A prefetch template that searches, such as ``Patient?_id={{context.patientId}}``, results in a ``Bundle``.

.. note::

  Firely Server registers these classes with its FHIR model metadata, whether or not the deployment serves CDS Hooks. Their names are therefore reserved: you cannot define a custom resource type or datatype with one of them. See :ref:`feature_cds_hooks_reserved_names` for the list.

Serialization
^^^^^^^^^^^^^

:namespace: Vonk.Core.CdsHooks.Serialization

You do not need to (de)serialize the request or response yourself; the pipeline does that. For unit tests and tooling, ``CdsHooksJsonDeserializer`` and ``CdsHooksJsonSerializer`` read and write the CDS Hooks JSON without starting the server. Embedded FHIR resources pass through the FHIR parser, so create each one with the ``ModelInspector`` of the FHIR version of the payload, for example ``ModelInfo.ModelInspector`` (namespace ``Hl7.Fhir.Model``) for R4:

.. code-block:: csharp

    var deserializer = new CdsHooksJsonDeserializer(ModelInfo.ModelInspector);
    if (!deserializer.TryDeserialize<CdsHooksRequest>(json, out var request, out var issues))
    {
        // request holds what could be read, and issues holds every problem that was found.
    }

    var serializer = new CdsHooksJsonSerializer(ModelInfo.ModelInspector);
    var responseJson = serializer.SerializeToString(context.Response);

* ``TryDeserialize<T>`` collects all issues instead of stopping at the first one. It returns ``false`` when the payload could not be read in full. The payload then holds what could be read. ``Deserialize<T>`` returns the payload, or throws a ``DeserializationFailedException``.
* ``T`` is the payload to read: ``CdsHooksRequest``, ``CdsHooksResponse`` or ``CdsHooksDiscoveryResponse``. The JSON itself does not say which one it is.
* ``SerializeToString(payload)`` returns the JSON, and ``Serialize(payload, stream)`` writes it to a stream.

A member that the specification does not define is kept and reported as an issue, as Da Vinci CRD requires. ``TryDeserialize`` still returns ``true`` for such a payload. To treat such a member as an error, create the deserializer with ``new CdsHooksDeserializerSettings { RejectUnknownMembers = true }`` as its second argument.

.. _vonk_reference_api_cds_hooks_fhiraccess:

Reading FHIR data from this server
----------------------------------

:namespace: Vonk.Core.CdsHooks.Pluggability, Vonk.Core.Context.FhirRest

A CDS Service often needs more FHIR data than the CDS Client sent in ``prefetch``, or it needs to call a FHIR operation that another plugin serves. For that, take an ``IFhirRestPipeline`` as a constructor dependency, and call one of these extension methods with the ``CdsServiceContext`` you are handling:

``ReadAsync(context, resourceType, resourceId, authorization, ...)``
    ``GET {resourceType}/{id}``

``SearchAsync(context, resourceType, arguments, authorization, ...)``
    ``GET {resourceType}?{arguments}``

``InvokeAsync(context, resourceType, operationName, method, body, authorization, ...)``
    A type-level operation, for example ``POST Patient/$member-match``.

``InvokeSystemAsync(context, operationName, method, body, authorization, ...)``
    A system-level operation, for example ``POST $export``.

``SendAsync(context, request, ...)``
    A request you built yourself as an ``IVonkContext``, for anything the four above do not cover, such as a create or an instance-level operation.

Each returns an ``IFhirRestResponse`` with the ``HttpResult``, the ``Resource`` (as a POCO) and the ``Outcome`` of the FHIR request.

Some arguments need attention:

* ``arguments`` of ``SearchAsync`` is an ``IArgumentCollection``. Create one with ``new ArgumentCollection(new Argument(ArgumentSource.Query, name, value))``, both in namespace ``Vonk.Core.Context``.
* ``operationName`` may be given with or without its ``$``.
* ``method`` is the HTTP method, as a constant of ``RequestMethod`` in namespace ``Vonk.Core.ElementModel``. Usually it is ``RequestMethod.POST``.
* ``body`` is an ``IResource``, the representation of a resource inside Firely Server. Convert a POCO with ``ToTypedElement().ToIResource(context.InformationModel)``. Pass ``null`` for an operation without input.
* Every method also takes two optional parameters. ``configureRequest`` is an action that can change the request after it is built and before it is sent. ``cancellationToken`` cancels the request; without it, the request is cancelled when the CDS request is.

The request does not go over the network. It runs in-process through the same FHIR REST pipeline that an HTTP request goes through. As a consequence:

* The plugin serving the request has to be on the branch that serves patient data, not on ``/cds-services``. Your CDS Service needs no dependency on that plugin.
* Tenant handling, URL mapping and the :ref:`audit trail <feature_auditing>` apply. When CDS Hooks authorization is enabled, the CDS Client is recorded as the agent on the ``AuditEvent``. With authorization off, the ``AuditEvent`` has no agent.
* Only the headers listed in ``CdsHooks:RoutedRequestHeaders`` are carried from the CDS request to the FHIR request. If that setting is absent, only the tenant header is carried (see :ref:`feature_multitenancy`).

The optional ``branch`` parameter chooses the database: ``FhirRestBranch.Data`` (the default) for patient data, or ``FhirRestBranch.Administration`` for conformance resources such as a ``Questionnaire``.

These methods read from this Firely Server. To read from the FHIR server of the CDS Client, use the address in ``Request.FhirServer`` and the token in ``Request.FhirAuthorization`` with a FHIR client of your own.

Stating what the request may access
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

:namespace: Vonk.Core.Security

A request built in code carries no token, so there is nothing to derive its permissions from. Each call therefore takes an ``IAuthorization`` that states what the request may access, and there is no default. The ``Authorize`` class provides the common cases:

.. list-table::
   :header-rows: 1
   :widths: 45 55

   * - Factory
     - Allows
   * - ``Authorize.ReadOnly("Patient", "Condition")``
     - Reading the named resource types, writing nothing.
   * - ``Authorize.ReadAnyType()``
     - Reading every resource type, writing nothing. Needed for a search across all types.
   * - ``Authorize.ReadWrite("Task")``
     - Reading and writing the named resource types.
   * - ``Authorize.ReadWrite(read: ["Patient", "Observation"], write: ["AuditEvent"])``
     - Reading one set of types and writing another.
   * - ``Authorize.Everything()``
     - Reading and writing every resource type.
   * - ``Authorize.Nothing()``
     - Nothing. Every interaction is refused.

For anything else, implement ``IAuthorization`` yourself.

.. warning::

  The request is **not** restricted by SMART on FHIR scopes or by an AccessPolicy. With ``Authorize.Everything()``, it is bounded only by the code that sends it. Always use the narrowest authorization that works.

For example, to read the patient in context:

.. code-block:: csharp

    if (context.Request.Context.GetString("patientId") is not { } patientId)
    {
        // Refuse the request, see "Signalling success and failure".
        return;
    }

    var read = await _fhirRestPipeline.ReadAsync(context, "Patient", patientId, Authorize.ReadOnly("Patient"));
    if (read.HttpResult == StatusCodes.Status200OK && read.Resource is Patient patient)
    {
        // Use the patient.
    }

To search the observations of that patient:

.. code-block:: csharp

    var search = await _fhirRestPipeline.SearchAsync(
        context,
        "Observation",
        new ArgumentCollection(new Argument(ArgumentSource.Query, "subject", $"Patient/{patientId}")),
        Authorize.ReadOnly("Observation"));

    if (search.Resource is Bundle bundle)
    {
        // Use bundle.Entry.
    }

To invoke an operation, here ``$member-match`` with a ``Parameters`` resource as input:

.. code-block:: csharp

    var parameters = new Parameters();
    parameters.Add("MemberPatient", memberPatient);
    parameters.Add("CoverageToMatch", coverage);

    var match = await _fhirRestPipeline.InvokeAsync(
        context,
        "Patient",
        "member-match",
        RequestMethod.POST,
        parameters.ToTypedElement().ToIResource(context.InformationModel),
        Authorize.ReadOnly("Patient", "Coverage"));

    if (match.HttpResult == StatusCodes.Status200OK && match.Resource is Parameters output)
    {
        // Read the MemberIdentifier from the output.
    }

An operation such as ``$member-match`` may write as well as read, so think about what it needs before you choose the authorization. The example ``CrdOrderSelectCdsService`` invokes ``$member-match`` and ``$evaluate`` this way.

.. _vonk_reference_api_cds_hooks_configuration:

Registering the service
-----------------------

A CDS Service is registered by a configuration class, like any other plugin (see :ref:`vonk_plugins_configclass`). Use ``ConfigureServices`` for the dependency injection registrations and ``Configure`` for the handler:

.. code-block:: csharp

    [VonkConfiguration(order: 5600)]
    public static class MyCdsServiceConfiguration
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services)
        {
            services.TryAddScoped<MyCdsService>();
            // Describing and handling are separate registrations. This class does both,
            // so the contributor resolves to the same instance.
            services.AddScoped<ICdsDiscoveryContributor>(sp => sp.GetRequiredService<MyCdsService>());
            return services;
        }

        public static IApplicationBuilder Configure(IApplicationBuilder builder)
        {
            builder.OnCdsService(MyCdsService.Definition)
                .HandleAsyncWith<MyCdsService>((svc, ctx) => svc.HandleAsync(ctx));
            return builder;
        }
    }

Your configuration class needs no license token of its own; you write it the same way as for any other plugin. The CDS Hooks branch itself requires the license token ``http://fire.ly/server/plugins/cds-hooks``.

Then make the service available in the appsettings, in two places.

First, name the configuration class in the ``Include`` of the CDS Hooks branch, after ``Vonk.Plugin.CdsHooks.Infra``:

.. code-block:: JavaScript

    "PipelineOptions": {
      "PluginDirectory": "./plugins",
      "Branches": [
        {
          "Path": "/",
          "Include": [ ... ]
        },
        {
          "Path": "/cds-services",
          "Include": [
            "Vonk.Plugin.CdsHooks.Infra",
            "MyCompany.MyCdsPlugin.MyCdsServiceConfiguration"
          ]
        }
      ]
    }

For how an ``Include`` entry is matched, see :ref:`feature_cds_hooks_branch_rules`.

Second, offer the service under ``CdsHooks:Services``, with each hook it answers:

.. code-block:: JavaScript

    "CdsHooks": {
      "Services": {
        "my-service": {
          "Hooks": {
            "patient-view": { "Enabled": true }
          }
        }
      }
    }

Leaving an id and hook out of this section, or setting ``Enabled`` to ``false``, removes the pair from the discovery document and turns off its handling. The same section configures request and response validation, and whether the service requires authorization; see :ref:`feature_cds_hooks`.

Serve CDS Services only on the CDS Hooks branch. For this and the other rules of the branch, see :ref:`feature_cds_hooks_branch_rules`.

Example: a patient-view service
-------------------------------

The ``Vonk.Plugin.CdsHooks.Examples`` plugin that ships with Firely Server contains ``PatientViewTestCdsService``. Use it as a template for your own service: it uses only the public API described on this page. It answers the ``patient-view`` hook by greeting the prefetched patient with a single card.

.. container:: toggle

    .. container:: header

      PatientViewTestCdsService.cs

    The service both describes itself and handles its hook.

    .. code-block:: csharp

        public sealed class PatientViewTestCdsService : ICdsDiscoveryContributor
        {
            internal const string ServiceId = "patient-view-test-hook";
            internal const string PatientToGreet = "patientToGreet";
            private const string PatientIdElement = "patientId";

            public static readonly CdsHooksServiceDefinition Definition = new()
            {
                Id = ServiceId,
                Hook = "patient-view",
                Title = "Test Hook",
                Description = "This is a test hook",
                Prefetch = new CdsHooksPrefetchTemplates { [PatientToGreet] = "Patient/{{context.patientId}}" },
                UsageRequirements = "none",
            };

            public void ContributeToDiscovery(ICdsDiscoveryDocumentBuilder builder)
            {
                Check.NotNull(builder);
                builder.AddService(Definition);
            }

            public Task HandleAsync(CdsServiceContext context)
            {
                Check.NotNull(context);

                var prefetch = context.Request.Prefetch;
                if (prefetch is null || !prefetch.TryGet(PatientToGreet, out var prefetched) || prefetched is not Patient patient)
                {
                    // The specification's case for 412: the CDS Client did not send FHIR data this service needs.
                    return Refuse(context, $"No {PatientToGreet} provided in prefetch section of CDS Hooks request.");
                }

                // Sanity check against the context the CDS Client sent alongside the prefetch.
                var contextPatientId = context.Request.Context.GetString(PatientIdElement);
                if (patient.Id is null || patient.Id != contextPatientId)
                {
                    return Refuse(context,
                        $"Patient ids in context ({contextPatientId}) and prefetch ({patient.Id}) do not match.");
                }

                context.Response.Cards.Add(new CdsHooksCard
                {
                    Uuid = Guid.NewGuid().ToString(),
                    Summary = "Hello World! Firely Server loves FHIR and CDS Hooks!",
                    Detail = $"Hello {GivenNames(patient)} {FamilyName(patient)} ({Gender(patient)}, {BirthDate(patient)})!",
                    Indicator = CdsHooksIndicator.Info,
                    Source = new CdsHooksSource { Label = "Firely Server", Url = context.FhirResourceRoot?.ToString() },
                });

                // A handler that answered says so, including on success - the pipeline does not infer it.
                context.HttpResult = StatusCodes.Status200OK;
                return Task.CompletedTask;
            }

            private static Task Refuse(CdsServiceContext context, string message)
            {
                context.Outcome.Issue.Add(VonkIssue.PROCESSING_ERROR.CloneWithDetails(message));
                context.HttpResult = StatusCodes.Status412PreconditionFailed;
                return Task.CompletedTask;
            }

            // FamilyName, GivenNames, Gender and BirthDate format the patient's details,
            // falling back to placeholders such as "{unknown family name}".
        }

.. container:: toggle

    .. container:: header

      PatientViewTestCdsServiceConfiguration.cs

    The configuration class registers the service, the discovery contributor and the handler. It is shown here with ``[VonkConfiguration]``, as you would write it in your own plugin; the shipped example uses the internal variant of that attribute.

    .. code-block:: csharp

        [VonkConfiguration(order: 5500)]
        public static class PatientViewTestCdsServiceConfiguration
        {
            public static IServiceCollection ConfigureServices(this IServiceCollection services)
            {
                services.TryAddScoped<PatientViewTestCdsService>();
                services.AddScoped<ICdsDiscoveryContributor>(sp => sp.GetRequiredService<PatientViewTestCdsService>());
                return services;
            }

            public static IApplicationBuilder Configure(IApplicationBuilder builder)
            {
                builder.OnCdsService(PatientViewTestCdsService.Definition)
                    .HandleAsyncWith<PatientViewTestCdsService>((svc, ctx) => svc.HandleAsync(ctx));
                return builder;
            }
        }

Configure it as described in :ref:`vonk_reference_api_cds_hooks_configuration`, with ``Vonk.Plugin.CdsHooks.Examples.PatientViewTestCdsServiceConfiguration`` in the ``Include`` and ``patient-view-test-hook`` for the ``patient-view`` hook under ``CdsHooks:Services``.

With this configuration, ``GET <base-url>/cds-services`` lists the service. For a request that invokes it, and the card it answers with, see :ref:`feature_cds_hooks_examples_patient_view`.

A more complex example
^^^^^^^^^^^^^^^^^^^^^^

The same plugin contains ``CrdOrderSelectCdsService``, a Da Vinci CRD service on the ``order-select`` and ``order-sign`` hooks, offered as ``crd-order-select-hook`` and ``crd-order-sign-hook``. It is a demonstration, not a template, and not intended for production use: its coverage logic is specific to Firely's demo setup, and a payer implementing CRD writes coverage rules of their own. Read it as a larger example:

* one class describing and handling two hooks, with a definition for each;
* reading the draft orders from ``context.draftOrders``, and the patient and coverage from prefetch searches that result in a ``Bundle``;
* calling ``$member-match`` and ``$evaluate`` with ``InvokeAsync`` (see :ref:`vonk_reference_api_cds_hooks_fhiraccess`);
* answering with a ``systemActions`` update of the draft order, next to guidance cards.

It needs the CRD license token, and the CQL and MemberMatch plugins on the branch that serves patient data.

Unit testing a CDS Service
--------------------------

``CdsServiceContext`` and the object model are plain classes with ``init``-settable properties, so you can construct the real context in a unit test. There is no test double, and none is needed:

.. code-block:: csharp

    var patient = new Patient
    {
        Id = "example",
        Name = [new HumanName { Family = "Doe", Given = ["John"] }],
        Gender = AdministrativeGender.Male,
        BirthDate = "1974-12-25",
    };

    var context = new CdsServiceContext
    {
        ServiceId = "patient-view-test-hook",
        FhirResourceRoot = new Uri("https://example.org/fhir/"),
        Request = new CdsHooksRequest
        {
            Hook = "patient-view",
            Context = new CdsHooksContext { ["patientId"] = "example" },
            Prefetch = new CdsHooksPrefetch { ["patientToGreet"] = patient },
        },
    };

    await new PatientViewTestCdsService().HandleAsync(context);

    Assert.Equal(StatusCodes.Status200OK, context.HttpResult);
    var card = Assert.Single(context.Response.Cards);
    Assert.Equal("Hello John Doe (male, 1974-12-25)!", card.Detail);

``CdsHooksContext`` and ``CdsHooksPrefetch`` take their members through an indexer, so a collection initializer fills them in. The patient in the prefetch has the id given in ``context.patientId``; with a different id, or without the prefetch, the service answers ``412 Precondition Failed`` instead. That case is worth a test of its own.

To start from a JSON fixture instead, read it with ``CdsHooksJsonDeserializer``.

Migrating from the legacy CDS Hooks API
---------------------------------------

The legacy plugin ``Vonk.Plugin.CdsHooks.Legacy`` maps each CDS Hooks request onto a FHIR custom operation. For the plugin itself and how to move a deployment off it, see :ref:`feature_cds_hooks_upgrade`. This table maps its programming API onto the API on this page:

.. list-table::
   :header-rows: 1
   :widths: 50 50

   * - Legacy API
     - CDS Hooks branch
   * - ``builder.OnCdsHooksRequest("<service-id>")`` (now ``[Obsolete]``)
     - ``builder.OnCdsService(Definition)``, once per hook
   * - ``ICdsHooksDiscoveryDocumentContributor`` with ``Hl7.Fhir.Model.CdsHooks.Service``
     - ``ICdsDiscoveryContributor`` with ``CdsHooksServiceDefinition``
   * - Handler on ``IVonkContext``, reading a ``CDSHooksRequestOld`` resource from ``Request.Payload``
     - Handler on ``CdsServiceContext``, reading ``context.Request``
   * - Response built as a ``CDSHooksResponseOld`` resource in ``Response.Payload``
     - Add to ``context.Response.Cards`` or ``context.Response.SystemActions``
   * - ``Operations:$cds-<id>`` entry in the appsettings
     - ``CdsHooks:Services:<id>:Hooks:<hook>`` entry
   * - Custom ``StructureDefinition`` for the request and response
     - Not needed
   * - Plugin on the FHIR REST branch on ``/``
     - Configuration class on the CDS Hooks branch

Known limitations
-----------------

See :ref:`feature_cds_hooks_limitations`.
