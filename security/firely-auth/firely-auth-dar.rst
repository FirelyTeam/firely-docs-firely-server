.. _firely_auth_dar:

Designated Authorized Representative
====================================

Firely Auth supports Designated Authorized Representatives (DAR), enabling users to access and manage patient data on behalf of others through delegated access tokens.

When the Authorized Representatives feature is enabled, user authentication triggers a call to Firely Server's custom operations to look up and verify existing patient relationships. If relationships are found, users are presented with a selection screen to choose their acting capacity as themselves or on behalf of an authorized patient. The issued access token preserves the original user's ``fhirUser`` identity for audit purposes while setting the ``patient`` claim to the selected patient, ensuring access is restricted to that patient's compartment data.


.. image:: /images/fa_dar_selection.png
    :align: center
    :class: bordered-image
    :alt: Designated Authorized Representative Selection Screen        



.. warning::

    This is a beta feature and as such it is subject to change in future releases. Currently, this implementation is not yet working correctly together with all of Firely Auths features, mainly :ref:`firely_auth_settings_launchcontext` and :ref:`firely_auth_settings_disclaimers`. On launch context registration, the Authorized Representative feature will be skipped if launch scopes are included in the token request. For disclaimer registration, disclaimers can be accepted once for each representee, but disclaimers that were agreed to in the authorized representative flow will not be visible in the users overview of accepted disclaimers. Due to the experimental nature of this feature, it is recommended to thoroughly test its functionality in a development or staging environment before deploying it to production.

.. note::

    Two custom operations must be implemented on your Firely Server instance to enable Authorized Representative functionality: ``$representative-relationships`` and ``Patient/$check-authorized-representative``.

    Firely Auth calls ``$representative-relationships`` once per sign-in of a user. The operation posts the patients the user may act for to the Firely Auth Administrative API, and returns a Parameters resource with a ``result`` parameter set to ``true`` if there is at least one, which directs the user to a selection screen. The relationship(s) live in memory for 5 minutes.

    Firely Auth calls ``Patient/$check-authorized-representative`` when the user chooses whom to act for, when the application exchanges the authorization code, and at every refresh of the issued token, to confirm that the choice is still authorized.

.. warning::

    The DAR feature does not support running more than one Firely Auth instance behind a load balancer. The relationships that Firely Server posts to the Administrative API, and the profile the user then selects, are kept only in the memory of the Firely Auth instance that received them. They are not shared with other instances or stored in the database.

    With more than one instance, these requests can each land on a different instance:

    - Firely Server posting the relationships to ``/api/external/representative-relationships``,
    - the user's browser loading the DAR Selection Screen and submitting the selection,
    - the client application requesting the access token from the token endpoint.

    If Firely Server's post lands on another instance than the one serving the user, the selection screen does not find the relationships and shows the ``HelpMessageOnFail`` message, so the user can only continue as themselves. If the token request lands on another instance than the one that received the selection, the access token is issued without an error, but for the user's own patient context (derived from their ``fhirUser`` claim) instead of the patient they selected. Sticky sessions on the load balancer do not prevent this, because Firely Server and the client application call Firely Auth directly rather than through the user's browser.

    If you use DAR, run a single Firely Auth instance.

.. note::

    Firely Auth only accepts a selection of the user's own patient, or of a patient that Firely Server posted for that user, and only while ``LookupOnLogin`` is ``true``. The relationships are kept for five minutes; a user who chooses someone after that is asked to sign in to the app again, and can still continue as themselves. The selection applies only to the next token of the app it was made for. A refresh of that token keeps its ``patient``. The selection screen is not shown, and a selection is not applied, when a launch context is requested or registered.

Configuration Guide: Enabling Designated Authorized Representatives
-------------------------------------------------------------------

Step 1 - Configure Firely Auth Authorized Representatives in the appsettings
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

You can enable DAR functionality by configuring it in the Firely Auth App Settings be setting ``LookupOnLogin`` to true.

.. code-block:: json

  "AuthorizedRepresentatives": {
      "LookupOnLogin": true,
      "LookupRequestScopes": "https://fire.ly/fhir/OperationDefinition/has-authorized-representative-relationships",
      "CheckAuthorizedRepresentativeTimeout": 5000, // in ms
      "TitleOnLogin": "Who Are You Signing In As?",
      "TitleOwnProfileOnLogin": "Continue as Yourself",
      "TitleAuthorizedProfilesOnLogin": "Continue On Behalf Of ...",
      "HelpMessageTitleOnFail": "Couldn't load authorized profiles",
      "HelpMessageOnFail": "A technical error prevented loading the user profiles of all authorized users. Please try again by reloading or continue as yourself."
    }

Additionally, the screen for selecting authorized representatives can be customized through various static text settings:

- TitleOnLogin: Main title displayed on the page.
- TitleOwnProfileOnLogin /  TitleAuthorizedProfilesOnLogin:  
  The screen is divided into two sections — one for the user's own profile and one for authorized profiles.  
  Separate titles can be defined for each section.
- HelpMessageTitleOnFail and HelpMessageOnFail:
  These messages are shown when Firely Auth does not retrieve data in a timely manner, despite the FHIR server returning `true` for the ``$representative-relationships`` operation (see below).

Step 2 - Register Firely Server's Management API Client in Firely Auth
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

1. Log in as an Administrator to the Firely Auth

2. Navigate to the Clients section and create a new Client that Firely Server uses to push relationships, with the following settings:

   - Client ID: a value of your choice
   - Client Secret: a strong secret of your choice
   - Client Type: Management API
   - Allowed Grant Types: Client Credentials

3. Set the same Client ID and Client Secret on Firely Server, in the ``AuthorizedRepresentativeExample:ClientId`` and ``AuthorizedRepresentativeExample:ClientSecret`` configuration keys (see below).

Step 3 - Configuring the example plugin on Firely Server
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Firely Server ships an example plugin, ``Vonk.Plugin.AuthorizedRepresentativeExample``, that implements both operations. Add ``Vonk.Plugin.AuthorizedRepresentativeExample`` to ``PipelineOptions``, and add ``representative-relationships`` and ``check-authorized-representative`` to ``Operations``. Configure the management API client in the ``AuthorizedRepresentativeExample`` section:

.. code-block:: json

    "AuthorizedRepresentativeExample": {
        "FirelyAuthUrl": "https://localhost:5100",
        "ClientId": "the client id from step 2",
        "ClientSecret": "the client secret from step 2",
        "PushTimeoutSeconds": 3
    }

- ``FirelyAuthUrl``: the base url of Firely Auth. Leave it empty to use ``SmartAuthorizationOptions:Authority``. It must use https, except for a loopback address, or when ``SmartAuthorizationOptions:RequireHttpsToProvider`` is ``false``.
- ``ClientId`` / ``ClientSecret``: the management API client registered in step 2.
- ``PushTimeoutSeconds``: the number of seconds the push to Firely Auth may take, including the request for the management API token. Keep it below ``AuthorizedRepresentatives:CheckAuthorizedRepresentativeTimeout`` of Firely Auth, which is 5 seconds by default. Defaults to 3.

**Operation signatures:**

- ``POST [base]/$representative-relationships`` has no input parameters: the user is the ``sub`` claim of the token. It pushes the patients the user may act for to Firely Auth, and answers a ``result`` parameter (boolean), ``true`` when there is at least one.
- ``POST [base]/Patient/$check-authorized-representative`` takes ``patient`` (a reference ``Patient/<id>``) and ``representative`` (a reference ``RelatedPerson/<id>``); the user who chose is the ``sub`` claim of the token. It answers a ``result`` parameter (boolean), and a ``consent`` parameter with the Consent that authorizes the pair, if the authorization has one.

**Example authorization model:**

The example plugin links a user to the patients they may act for through a ``RelatedPerson`` that carries the identifier ``http://server.fire.ly/auth/identifier/sub|<user id>``. An administrator adds that identifier when linking the RelatedPerson to the user in Firely Auth. The RelatedPerson must be active, its period must cover the current time, and its Patient must exist. The example additionally requires an active privacy Consent (``Consent.scope`` in FHIR R4, ``Consent.category`` in FHIR R5, with the code ``patient-privacy`` of ``http://terminology.hl7.org/CodeSystem/consentscope``) that permits the RelatedPerson as an actor. A Consent is optional: replace this logic to decide the authorization in another way.

Posting Result of operation Example
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
This code block demonstrates the response of the ``$representative-relationships`` operation.

.. code-block:: json

    {
        "resourceType": "Parameters",
        "parameter": [
            {
                "name": "result",
                "valueBoolean": true
            }
        ]
    }

The ``$check-authorized-representative`` operation adds a ``consent`` parameter when the authorization has a Consent:

.. code-block:: json

    {
        "resourceType": "Parameters",
        "parameter": [
            {
                "name": "result",
                "valueBoolean": true
            },
            {
                "name": "consent",
                "valueReference": {
                    "reference": "Consent/1"
                }
            }
        ]
    }

Posting Relationship Details to Firely Auth Administrative API
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

The ``$representative-relationships`` operation posts relationship details to Firely Auth, replacing the patients it holds for the user. Note that for this to work, a designated management API client must be configured in Firely Auth that Firely Server uses to post the relationship details, as configured in step 2 above.

**Request:** POST to ``/api/external/representative-relationships``

.. code-block:: json

    {
        "UserId": "string",
        "Relationships": [
            {
                "PatientId": "string",
                "FullName": "string",
                "Relationship": "string",
                "Representative": "string"
            }
        ]
    }


Designated Authorized Representative Example Implementation
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

.. container:: toggle

    .. container:: header

      RepresentativeRelationshipsOperation.cs

    Implements ``POST [base]/$representative-relationships``. Firely Auth calls it once per sign-in of a user who may act for others.

    .. code-block:: csharp

        using Microsoft.AspNetCore.Http;
        using Microsoft.Extensions.Logging;
        using Vonk.Core.Context;

        namespace Vonk.Plugin.AuthorizedRepresentativeExample;

        internal sealed class RepresentativeRelationshipsOperation(
            RepresentativeAuthorizations authorizations,
            FirelyAuthRelationshipsClient firelyAuth,
            ILogger<RepresentativeRelationshipsOperation> logger)
        {
            public const string Name = "representative-relationships";

            public async Task HandleAsync(IVonkContext vonkContext)
            {
                var (_, _, response) = vonkContext.Parts();
                var httpContext = vonkContext.HttpContext();

                if (!FirelyAuthCaller.TryGetUserId(httpContext.User, out var userId))
                {
                    response.HttpResult = StatusCodes.Status403Forbidden;
                    response.Outcome.AddIssue(VonkIssue.FORBIDDEN, "You are not allowed to call this operation.");
                    return;
                }

                IReadOnlyList<Representee> representees;
                try
                {
                    representees = await authorizations.FindRepresenteesAsync(userId, vonkContext);
                }
                catch (RepresentativeLookupException)
                {
                    response.HttpResult = StatusCodes.Status500InternalServerError;
                    response.Outcome.AddIssue(VonkIssue.INTERNAL_ERROR, "The request could not be processed.");
                    return;
                }

                try
                {
                    await firelyAuth.PushAsync(userId, representees, httpContext.RequestAborted);
                }
                catch (Exception ex) when (ex is not OperationCanceledException || !httpContext.RequestAborted.IsCancellationRequested)
                {
                    response.HttpResult = StatusCodes.Status500InternalServerError;
                    response.Outcome.AddIssue(VonkIssue.INTERNAL_ERROR, "The request could not be processed.");
                    return;
                }

                response.HttpResult = StatusCodes.Status200OK;
                response.Payload = OperationParameters.Create(representees.Count > 0, consent: null, vonkContext.InformationModel);
            }
        }

.. container:: toggle

    .. container:: header

      CheckAuthorizedRepresentativeOperation.cs

    Implements ``POST [base]/Patient/$check-authorized-representative``. Firely Auth calls it when a user chooses whom to act for, when the app exchanges the authorization code, and at every refresh of a token for that choice.

    .. code-block:: csharp

        using System.Text.RegularExpressions;
        using Microsoft.AspNetCore.Http;
        using Microsoft.Extensions.Logging;
        using Vonk.Core.Context;

        namespace Vonk.Plugin.AuthorizedRepresentativeExample;

        internal sealed partial class CheckAuthorizedRepresentativeOperation(
            RepresentativeAuthorizations authorizations,
            ILogger<CheckAuthorizedRepresentativeOperation> logger)
        {
            public const string Name = "check-authorized-representative";

            private const string PatientParameter = "patient";
            private const string RepresentativeParameter = "representative";

            [GeneratedRegex(@"^Patient/(?<id>[A-Za-z0-9\-\.]{1,64})$")]
            private static partial Regex PatientReference();

            [GeneratedRegex(@"^RelatedPerson/(?<id>[A-Za-z0-9\-\.]{1,64})$")]
            private static partial Regex RelatedPersonReference();

            public async Task HandleAsync(IVonkContext vonkContext)
            {
                var (request, args, response) = vonkContext.Parts();
                args.ResourceTypeArguments().Handled();

                if (!FirelyAuthCaller.TryGetUserId(vonkContext.HttpContext().User, out var userId))
                {
                    response.HttpResult = StatusCodes.Status403Forbidden;
                    response.Outcome.AddIssue(VonkIssue.FORBIDDEN, "You are not allowed to call this operation.");
                    return;
                }

                if (!request.GetRequiredPayload(response, out var parameters))
                    return;

                var patientMatch = PatientReference().Match(OperationParameters.GetReference(parameters, PatientParameter) ?? string.Empty);
                var representativeMatch = RelatedPersonReference().Match(OperationParameters.GetReference(parameters, RepresentativeParameter) ?? string.Empty);
                if (!patientMatch.Success || !representativeMatch.Success)
                {
                    response.HttpResult = StatusCodes.Status400BadRequest;
                    response.Outcome.AddIssue(VonkIssue.PARAMETER_MISSING,
                        $"Provide '{PatientParameter}' as a reference Patient/<id> and '{RepresentativeParameter}' as a reference RelatedPerson/<id>, each once.");
                    return;
                }

                var patientId = patientMatch.Groups["id"].Value;
                var relatedPersonId = representativeMatch.Groups["id"].Value;
                RepresentativeAuthorization? authorization;
                try
                {
                    authorization = await authorizations.CheckAsync(userId, patientId, relatedPersonId, vonkContext);
                }
                catch (RepresentativeLookupException)
                {
                    response.HttpResult = StatusCodes.Status500InternalServerError;
                    response.Outcome.AddIssue(VonkIssue.INTERNAL_ERROR, "The request could not be processed.");
                    return;
                }

                response.HttpResult = StatusCodes.Status200OK;
                response.Payload = OperationParameters.Create(authorization is not null, authorization?.Consent, vonkContext.InformationModel);
            }
        }

.. container:: toggle

    .. container:: header

      AuthorizedRepresentativeExampleConfiguration.cs

    Configuration works the same way as for any other Vonk plugin.

    .. code-block:: csharp

        using Duende.AccessTokenManagement;
        using Microsoft.AspNetCore.Builder;
        using Microsoft.AspNetCore.Http;
        using Microsoft.Extensions.Configuration;
        using Microsoft.Extensions.DependencyInjection;
        using Microsoft.Extensions.DependencyInjection.Extensions;
        using Vonk.Core.Common;
        using Vonk.Core.Context;
        using Vonk.Core.Metadata;
        using Vonk.Core.Pluggability;
        using Vonk.Core.Pluggability.ContextAware;

        namespace Vonk.Plugin.AuthorizedRepresentativeExample;

        [VonkConfiguration(order: 5600)]
        public static class AuthorizedRepresentativeExampleConfiguration
        {
            private static readonly ClientCredentialsClientName FirelyAuthClient = ClientCredentialsClientName.Parse("firely-auth-representative-relationships");

            private const string ManagementApiScope = "http://server.fire.ly/auth/scope/authmanagement";
            private const string ManagementApiResource = "http://server.fire.ly/auth/api/management";

            public static IServiceCollection ConfigureServices(IServiceCollection services, IConfiguration configuration)
            {
                var options = configuration.GetSection(AuthorizedRepresentativeExampleOptions.Section).Get<AuthorizedRepresentativeExampleOptions>() ?? new();
                var firelyAuthUrl = string.IsNullOrEmpty(options.FirelyAuthUrl) ? configuration["SmartAuthorizationOptions:Authority"] : options.FirelyAuthUrl;
                var firelyAuthBase = new Uri(firelyAuthUrl!.TrimEnd('/') + "/", UriKind.Absolute);

                services.AddDistributedMemoryCache();
                services.AddClientCredentialsTokenManagement()
                    .AddClient(FirelyAuthClient, client =>
                    {
                        client.TokenEndpoint = new Uri(firelyAuthBase, "connect/token");
                        client.ClientId = ClientId.Parse(options.ClientId!);
                        client.ClientSecret = ClientSecret.Parse(options.ClientSecret!);
                        client.Scope = Scope.Parse(ManagementApiScope);
                        client.Resource = Resource.Parse(ManagementApiResource);
                    });

                services.AddHttpClient<FirelyAuthRelationshipsClient>(client =>
                    {
                        client.BaseAddress = firelyAuthBase;
                        client.Timeout = TimeSpan.FromSeconds(options.PushTimeoutSeconds);
                    })
                    .AddClientCredentialsTokenHandler(FirelyAuthClient);

                services.TryAddSingleton(TimeProvider.System);
                services.TryAddScoped<RepresentativeAuthorizations>();
                services.TryAddScoped<RepresentativeRelationshipsOperation>();
                services.TryAddScoped<CheckAuthorizedRepresentativeOperation>();
                services.TryAddContextAwareTransient<ICapabilityStatementContributor, RepresentativeOperationsCapabilityStatementContributor>();
                return services;
            }

            public static IApplicationBuilder Configure(IApplicationBuilder builder)
            {
                builder.OnCustomInteraction(VonkInteraction.system_custom, RepresentativeRelationshipsOperation.Name)
                    .AndMethod(HttpMethods.Post)
                    .HandleAsyncWith<RepresentativeRelationshipsOperation>((operation, context) => operation.HandleAsync(context));

                builder.OnCustomInteraction(VonkInteraction.type_custom, CheckAuthorizedRepresentativeOperation.Name)
                    .AndMethod(HttpMethods.Post)
                    .AndResourceTypes("Patient")
                    .HandleAsyncWith<CheckAuthorizedRepresentativeOperation>((operation, context) => operation.HandleAsync(context));

                return builder;
            }
        }

The authorization decision itself, which looks up the ``RelatedPerson`` and the privacy ``Consent`` described in step 3 above, lives in ``RepresentativeAuthorizations``, part of the full example plugin source in the Firely Server repository. Replace it to decide authorization in another way.

Designated Authorized Representative Workflow and Diagram
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

The DAR authentication flow works as follows:

1. **User initiates login** - Standard OAuth2/OIDC flow begins
2. **Firely Auth pushes the relationships** - Calls ``$representative-relationships`` once with the user's token; Firely Server looks up the patients the user may act for and posts them to the Administrative API
3. **Conditional selection screen** - Only shown if the push said ``result`` is ``true``
4. **Firely Auth confirms the choice** - Calls ``Patient/$check-authorized-representative`` with the chosen patient and representative, when the user chooses, when the app exchanges the authorization code, and at every refresh of that token
5. **Token issuance** - Contains both the original user identity and the selected patient context

The following diagram illustrates the flow of the Designated Authorized Representative lookup process:

.. code-block:: text

    ┌──────────┐                 ┌─────────────┐                 ┌───────────────┐
    │          │                 │             │                 │               │
    │   User   │                 │ Firely Auth │                 │ Firely Server │
    │          │                 │             │                 │               │
    └────┬─────┘                 └──────┬──────┘                 └───────┬───────┘
         │                              │                                │
         │  1. Login                    │                                │
         │─────────────────────────────>│                                │
         │                              │                                │
         │                              │  2. POST                       │
         │                              │     $representative-           │
         │                              │     relationships              │
         │                              │     (token: sub = user)        │
         │                              │───────────────────────────────>│
         │                              │                                │
         │                              │                                │  3. Look up patients
         │                              │                                │     the user may act for
         │                              │                                │──┐
         │                              │                                │  │
         │                              │                                │<─┘
         │                              │                                │
         │                              │                                │  4. POST relationships to
         │                              │                                │     /api/external/
         │                              │                                │     representative-
         │                              │                                │     relationships
         │                              │<───────────────────────────────│
         │                              │                                │
         │                              │  5. Response with result       │
         │                              │     (true/false)               │
         │                              │<───────────────────────────────│
         │                              │                                │
         │  6. Selection screen         │                                │
         │     (if result = true)       │                                │
         │<─────────────────────────────│                                │
         │                              │                                │
         │  7. Select capacity          │                                │
         │     (self or representative) │                                │
         │─────────────────────────────>│                                │
         │                              │                                │
         │                              │  8. POST Patient/$check-       │
         │                              │     authorized-representative  │
         │                              │     (patient, representative)  │
         │                              │───────────────────────────────>│
         │                              │                                │
         │                              │  9. Response with result       │
         │                              │     (true/false), consent      │
         │                              │<───────────────────────────────│
         │                              │                                │
         │ 10. Access token issued      │                                │
         │     (fhirUser + patient)     │                                │
         │<─────────────────────────────│                                │
         │                              │                                │

    Steps 7-9 repeat at every exchange of the authorization code, and at every refresh of the issued token.











