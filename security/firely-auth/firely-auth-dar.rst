.. _firely_auth_dar:

Designated Authorized Representative
====================================

Firely Auth supports Designated Authorized Representatives (DAR), enabling users to access and manage patient data on behalf of others through delegated access tokens.

When the Authorized Representatives feature is enabled, user authentication triggers a call to Firely Server's custom operation to verify existing patient relationships. If relationships are found, users are presented with a selection screen to choose their acting capacity as themselves or on behalf of an authorized patient. The issued access token preserves the original user's ``fhirUser`` identity for audit purposes while setting the ``patient`` claim to the selected patient, ensuring access is restricted to that patient's compartment data.


.. warning::

    **Deprecated:** letting a ``Patient`` account act on behalf of other patients, with Firely Server finding the relationships through the user's ``Patient`` ``fhirUser``, is deprecated. A later release moves this to ``Representative`` accounts (see :ref:`firely_auth_dar_onboarding`): Firely Server finds the relationships through the ``sub`` identifier of the representative's Firely Auth account, and the representative's token carries their ``RelatedPerson`` as ``fhirUser``. The current flow keeps working until then. While ``LookupOnLogin`` is on, Firely Auth logs a ``Warning`` about this at startup.

.. image:: /images/fa_dar_selection.png
    :align: center
    :class: bordered-image
    :alt: Designated Authorized Representative Selection Screen        



.. warning::

    This is a beta feature and as such it is subject to change in future releases. Currently, this implementation is not yet working correctly together with all of Firely Auths features, mainly :ref:`firely_auth_settings_launchcontext` and :ref:`firely_auth_settings_disclaimers`. On launch context registration, the Authorized Representative feature will be skipped if launch scopes are included in the token request. For disclaimer registration, disclaimers can be accepted once for each representee, but disclaimers that were agreed to in the authorized representative flow will not be visible in the users overview of accepted disclaimers. Due to the experimental nature of this feature, it is recommended to thoroughly test its functionality in a development or staging environment before deploying it to production.

.. note::

    The custom operation ``$check-authorized-representative-relationships`` must be implemented on your Firely Server instance to enable Authorized Representative functionality.
    This operation must return a Parameters resource with a ``result`` parameter set to ``true`` if authorized representative relationships exist, which directs the user to a selection screen.
    
    The DAR Selection Screen is only shown if the operation returns ``true`` during the authorized code flow. The operation can post relationship details to the Firely Auth Administrative API either synchronously (before returning the response) or asynchronously (after returning the response). The relationship(s) will live in memory for 5 minutes.

.. warning::

    The DAR feature does not support running more than one Firely Auth instance behind a load balancer. The relationships that Firely Server posts to the Administrative API, and the profile the user then selects, are kept only in the memory of the Firely Auth instance that received them. They are not shared with other instances or stored in the database.

    With more than one instance, these requests can each land on a different instance:

    - Firely Server posting the relationships to ``/api/external/authorized-representative``,
    - the user's browser loading the DAR Selection Screen and submitting the selection,
    - the client application requesting the access token from the token endpoint.

    If Firely Server's post lands on another instance than the one serving the user, the selection screen does not find the relationships and shows the ``HelpMessageOnFail`` message, so the user can only continue as themselves. If the token request lands on another instance than the one that received the selection, the access token is issued without an error, but for the user's own patient context (derived from their ``fhirUser`` claim) instead of the patient they selected. Sticky sessions on the load balancer do not prevent this, because Firely Server and the client application call Firely Auth directly rather than through the user's browser.

    If you use DAR, run a single Firely Auth instance.

.. note::

    Firely Auth only accepts a selection of the user's own patient, or of a patient that Firely Server posted for that user, and only while ``LookupOnLogin`` is ``true``. The relationships are kept for five minutes; a user who chooses someone after that is asked to sign in to the app again, and can still continue as themselves. The selection applies only to the next token of the app it was made for. A refresh of that token keeps its ``patient``. The selection screen is not shown, and a selection is not applied, when a launch context is requested or registered.

.. _firely_auth_dar_onboarding:

Onboarding representatives
--------------------------

The Firely Auth account of a representative is created before their first login, in one of two ways:

- by an application, through the representative API, for example the application in which a patient registers the person who may act for them;
- by an admin, in the admin UI.

The representative then gets an invitation email and signs in through an external identity provider (see :ref:`firely_auth_sso`).

A ``Representative`` account is for people who only act on behalf of others. Such a person does not need to be a patient: their account has no ``fhirUser`` and needs no ``Patient`` resource of their own in Firely Server. In the deprecated flow, a representative needed a ``Patient`` account, so every representative had to be registered as a patient first. Firely Server finds the patients a Representative may act for through the ``sub`` identifier of their Firely Auth account on the ``RelatedPerson``.

The representative API
^^^^^^^^^^^^^^^^^^^^^^

.. code-block:: HTTP

  POST <base-url>/api/external/representative HTTP/1.1
  Authorization: Bearer <representative API token>
  Content-Type: application/json

  {
    "fullName": "Jane Doe",
    "email": "jane.doe@example.org",
    "patient": "Patient/123"
  }

``patient`` is the patient the person acts for, as a relative reference. Firely Auth looks up the users with this email address:

- No user has it: a new user of the type ``Representative`` is created and invited, and the response is ``201 Created``.
- One user of the type ``Patient`` or ``Representative`` has it: that user is returned as is, with ``200 OK``. A ``Representative`` that has not logged in yet is sent the invitation again.
- Otherwise the request is refused with ``409 Conflict``: when the email address belongs to more than one user, to a user of another type, or to the patient themselves. The last is checked against the user's ``fhirUser``, and against Firely Server by searching that Patient for the user's sub identifier. For that search, Firely Auth uses a token that it issues to itself, with the audience ``FhirServer:FHIR_BASE_URL`` and the scope ``system/Patient.rs``. When Firely Server cannot be reached for that check, the request fails with ``500``.

The response holds the user's ``id``, ``email`` and ``fullName``, and the same id as an ``identifier`` for the representative's ``RelatedPerson``:

.. code-block:: json

  {
    "id": "8f0c…",
    "identifier": { "system": "http://server.fire.ly/auth/identifier/sub", "value": "8f0c…" },
    "email": "jane.doe@example.org",
    "fullName": "Jane Doe"
  }

Calling the API needs a token with the scope ``http://server.fire.ly/auth/scope/representative``. Only a ``BackendService`` client with ``AllowRepresentativeApiAccess`` enabled (see :ref:`firely_auth_settings_clients`) gets it, with the ``client_credentials`` grant, and only when it requests no other scope. Name the scope in the token request (``scope=http://server.fire.ly/auth/scope/representative``): a request without a ``scope`` parameter asks for all scopes of the client and is refused when the client has other scopes. The token's only audience is ``http://server.fire.ly/auth/api/representative``: Firely Server and the management API do not accept it, and this endpoint does not accept a management API token.

The Representative user type
^^^^^^^^^^^^^^^^^^^^^^^^^^^^

A ``Representative`` signs in only through an external identity provider, and has no password and no ``fhirUser``. The authorized representative selection at login is only shown to users with a ``Patient`` ``fhirUser``, so it is not shown to a Representative. An SSO login does not change a Representative into a Patient or Practitioner. An admin can turn a Representative into a Patient by changing its user type.

.. note::

    Representatives cannot have a local account with a password. This is on purpose. A Representative acts on behalf of a patient, so their identity must have been verified, and that verification is what the identity provider vouches for. With a local account, Firely Auth only knows that someone controls an email address, which is not enough to act for someone else.

Representatives in the admin UI
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

- **Add User** offers the user type *Representative* when at least one external identity provider is configured. A Representative gets no password and no claims. Saving the user sends the invitation. An email address that another account uses is refused. When the invitation cannot be sent, the account is kept and the admin UI says so; send it again once the configuration is fixed.
- The user list shows the type *Representative*, and the user type filter has it.
- The panel of a Representative shows whether the invitation has been accepted, with **resend invitation** until the first login, and the patients the Representative is linked to, each with **unlink**. The list does not show whether a ``Consent`` for that patient is active.
- **link representee** links a ``RelatedPerson`` in Firely Server to the Representative: enter its reference, **find** it, check the name, patient and relationship that Firely Auth shows, and link it. Firely Auth adds the identifier ``http://server.fire.ly/auth/identifier/sub|<user id>`` to the ``RelatedPerson``. Linking does not create the ``RelatedPerson`` or its ``Consent``; the application that onboards the representative creates those in Firely Server.
- A Backend Service client has the switch *Allow Representative Api Access* in its **Permissions**, the client setting ``AllowRepresentativeApiAccess``. Changing the client type to another type turns it off.

The admin UI is the only place to create a Representative by hand and to link or unlink its patients. The management API (``/api/external/user``) refuses to create a Representative with ``400 Bad Request`` and does not offer linking. An application uses the representative API instead.

Firely Auth reads and changes the ``RelatedPerson`` in Firely Server with a token that it issues to itself, with the audience ``FhirServer:FHIR_BASE_URL``. To show the linked patients and to **find** a ``RelatedPerson``, the token holds the scopes ``system/RelatedPerson.rs`` and ``system/Patient.rs``. For **link representee** and **unlink**, it holds ``system/RelatedPerson.rsu`` and ``system/Patient.rs``. **link representee** and **unlink** read the ``RelatedPerson``, change only its identifier and write it back with a ``PUT`` with ``If-Match`` on the version that was read. When the ``RelatedPerson`` was changed in Firely Server in the meantime, Firely Auth asks the admin to find it again and retry. A ``RelatedPerson`` that is linked to another account is refused.

No client in Firely Auth needs a setting for this, also not the admin UI. Firely Server needs to accept this token for writes:

- Firely Server accepts tokens from Firely Auth with the audience ``FhirServer:FHIR_BASE_URL``, see :ref:`firely_auth_settings`.
- The ``update`` interaction is enabled for ``RelatedPerson``.
- Firely Server returns a version for the ``RelatedPerson``, as an ``ETag`` or in ``meta.versionId``. Firely Auth does not update a ``RelatedPerson`` without a version.
- No proxy or gateway in front of Firely Server refuses writes with this token.

When Firely Server refuses the ``PUT``, **link representee** shows an error that names the scope ``system/RelatedPerson.rsu``, and the list of linked patients stays readable.

The invitation
^^^^^^^^^^^^^^

The invitation email uses the template ``representativeinvitation.liquid`` and the subject ``Email:RepresentativeInvitationEmailSubject`` (see :ref:`firely_auth_settings_email`). Its link signs in at the identity provider selected by ``RepresentativeInvitation:IdentityProviderScheme`` (see :ref:`firely_auth_settings_representativeinvitation`). The link holds no token and does not expire. At the first login, the Representative is linked to the identity provider account by email address, the same as any other SSO user created in advance, also when the person signs in through the login page instead of the link.

An admin can send the invitation again until the Representative has logged in:

.. code-block:: HTTP

  POST <base-url>/api/admin/user/<user-id>/resend-invitation HTTP/1.1

After the first login, this returns ``400 Bad Request``.

.. note::

    Every call that returns ``200 OK`` for a Representative that has not logged in sends the invitation again, and Firely Auth does not limit how often. An application that retries a request it already got a response for, or several requests that name the same representative, send the person one email each. Call the API once per registration, and only retry a request that failed. When an invitation cannot be sent, the representative API returns ``500`` and keeps the Representative, so a retry of the same request sends it again. An error is logged, without patient information.

.. warning::

    The identity provider must verify email addresses, because the email address is what links the Representative to their identity provider account. A person who signs in at the identity provider with another email address is not matched to the Representative. With ``AllowAutoProvision`` off for that identity provider, they see the error page. With it on, they get a separate account, but never access to the patient they were registered for.

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
  These messages are shown when Firely Auth does not retrieve data in a timely manner, despite the FHIR server returning `true` for the ``$check-authorized-representative-relationships`` operation (see below).

Step 2 - Register Firely Server DAR Plugin Client in Firely Auth
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

1. Log in as an Administrator to the Firely Auth
   
2. Navigate to the Clients section and create a new Client for the Firely Server DAR Plugin with the following settings to update the relationships:

   - Client ID: firely-server-dar-plugin
   - Client Secret: (a strong secret of your choice)
   - Client Type: Management API
   - Allowed Grant Types: Client Credentials
   

Step 3 - Building the $check-authorized-representative-relationships operation on Firely Server
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

The custom operation ``$check-authorized-representative-relationships`` must be implemented as a plugin on your Firely Server instance. This operation receives the patient parameter from Firely Auth and returns a Parameters resource indicating whether authorized representative relationships exist.

**Operation Signature:**

- **URL**: ``POST [base]/Patient/$check-authorized-representative-relationships``
- **Input**: Parameters resource containing a ``patient`` parameter with the fhirUser reference
- **Output**: Parameters resource with a ``result`` parameter (boolean) indicating if relationships exist

**Implementation Approach:**

The operation should:

1. Receive and validate the ``patient`` parameter from the input Parameters resource
2. Query your data store to check for authorized representative relationships
3. Return a Parameters resource with ``result`` set to ``true`` if relationships exist, ``false`` otherwise
4. Optionally post relationship details to Firely Auth's Administrative API (either synchronously or asynchronously)

Posting Result of operation Example
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
This code block demonstrates an example of a parameter resource response for the ``$check-authorized-representative-relationships`` operation.

.. code-block:: json

    {
        "resourceType": "Parameters",
        "parameter": [
            {
                "name": "result",
                "valueBoolean": true // In case there are additional relationships to be returned
            }
        ]
    }

Posting Relationship Details to Firely Auth Administrative API
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Posts relationship details to Firely Auth. Note that for this to work, a designated management API client must be configured in Firely Auth that can be used by Firely Server to post the relationship details. In the ``AuthorizedRepresentativeExampleConfiguration.cs`` example below, the name and secret for this client are configured statically as ``firely-server-dar-plugin``.

**Request:** POST to ``/api/external/authorized-representative``

.. code-block:: json

    {
        "PatientFullName": "string",
        "PatientId": "string", 
        "Relationships": [
            {
                "Id": "string",
                "FullName": "string",
                "Relationship": "string"
            }
        ]
    }


Designated Authorized Representative Example Implementation
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
.. container:: toggle

    .. container:: header

      CheckAuthorizedRepresentativeRelationshipsExampleService.cs
      
    The service is responsible for returning a parameter resource based on the authorized representative relationships.

    .. code-block:: csharp
        
        using System.Net.Http.Headers;
        using System.Text.Json;
        using Duende.AccessTokenManagement;
        using Duende.IdentityModel.Client;
        using Hl7.Fhir.ElementModel;
        using Hl7.Fhir.Model;
        using Hl7.Fhir.Utility;
        using Microsoft.Extensions.Logging;
        using Vonk.Core.Common;
        using Vonk.Core.Context;
        using Vonk.Core.Support;
        using Parameters = Hl7.Fhir.Model.Parameters;
        using ST = System.Threading.Tasks;

        namespace Firely.Server.Plugin.CheckAuthorizedRepresentativeRelationships
        {
            public class CheckAuthorizedRepresentativeRelationshipsExampleService
            {
                private readonly ILogger<CheckAuthorizedRepresentativeRelationshipsExampleService> _logger;
                private readonly IHttpClientFactory _httpClientFactory;
                private readonly IClientCredentialsTokenManagementService _clientCredentialsTokenManagementService;


                public CheckAuthorizedRepresentativeRelationshipsExampleService(ILogger<CheckAuthorizedRepresentativeRelationshipsExampleService> logger,
                    IHttpClientFactory httpClientFactory,
                    IClientCredentialsTokenManagementService clientCredentialsTokenManagementService
                    )
                {
                    Check.NotNull(logger, nameof(logger));
                    Check.NotNull(httpClientFactory, nameof(httpClientFactory));
                    Check.NotNull(clientCredentialsTokenManagementService, nameof(clientCredentialsTokenManagementService));

                    _logger = logger;
                    _httpClientFactory = httpClientFactory;
                    _clientCredentialsTokenManagementService = clientCredentialsTokenManagementService;
                }

                public async ST.Task CheckAuthorizedRepresentativeRelationshipsOnPost(IVonkContext vonkContext)
                {
                    _logger.LogInformation("Processing Custom $check-authorized-representative-relationships example.");

                    try
                    {
                        var requestResource = vonkContext.Request.Payload.Resource;
                        if (TryGetParameters(vonkContext, out var checkAuthorizedRepresentativeRelationshipsParameters))
                        {
                            _logger.LogDebug("CheckAuthorizedRepresentativeRelationshipsExampleService - Successfully extracted parameters for $check-authorized-representative-relationships");
                            vonkContext.Arguments.ResourceTypeArguments().Handled();
                            await CheckAuthorizedRepresentativeRelationshipsInternal(vonkContext, checkAuthorizedRepresentativeRelationshipsParameters!);          
                        }
                        else
                        {
                            _logger.LogDebug("CheckAuthorizedRepresentativeRelationshipsExampleService - Failed to extract parameters for $check-authorized-representative-relationships");
                            vonkContext.Arguments.ResourceTypeArguments().Handled();
                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug("CheckAuthorizedRepresentativeRelationshipsExampleService - Failed to extract parameters for $check-authorized-representative-relationships. Error: {0}", ex.Message);
                        vonkContext.Arguments.ResourceTypeArguments().Handled();
                    }
                }

                private async ST.Task CheckAuthorizedRepresentativeRelationshipsInternal(IVonkContext vonkContext,
                Parameters checkAuthorizedRepresentativeRelationshipsParameters)
                {
                    var fhirUser = checkAuthorizedRepresentativeRelationshipsParameters.GetSingleValue<FhirString>("patient");
                    if (fhirUser.IsNullOrEmpty())
                    {
                        vonkContext.Response.HttpResult = 400;
                        vonkContext.Response.Outcome.AddIssue(VonkOutcome.IssueSeverity.Error, VonkOutcome.IssueType.Invalid,
                            "Missing 'patient' parameter. Cannot proceed check of authorized representative relationships.");
                        return;
                    }

                    if (!fhirUser!.Value?.StartsWith("Patient/") ?? false)
                    {
                        vonkContext.Response.HttpResult = 400;
                        vonkContext.Response.Outcome.AddIssue(VonkOutcome.IssueSeverity.Error, VonkOutcome.IssueType.Invalid,
                            "Invalid 'patient' parameter value. The 'patient' parameter must point to a fhirUser of type 'Patient'. Cannot proceed check of authorized representative relationships.");
                        return;
                    }

                    // Do something with the patient id here
                    // Fake for "test" patient
                    var response = new Parameters();

                    var patientId = fhirUser.Value!.Substring(8);

                    if (patientId == "test")
                    {
                        response.Add("result", new FhirBoolean(true));
                        
                        _logger.LogDebug("CheckAuthorizedRepresentativeRelationshipsExampleService - Sending relationships to FA");

                        var json = JsonSerializer.Serialize(new
                        {
                            PatientFullName = "My Test Patient",
                            PatientId = "test",
                            Relationships = new[]
                            {
                            new { Id = "1234", FullName = "Son Test", Relationship = "Son" },
                            new { Id = "0987", FullName = "Daughter Test", Relationship = "Daughter" }
                            }
                        }, new JsonSerializerOptions { WriteIndented = true });

                        var firelyAuthEndpoint = "http://localhost:5100/api/external/authorized-representative";
                        var httpClient = _httpClientFactory.CreateClient(firelyAuthEndpoint);
                        var token = await _clientCredentialsTokenManagementService.GetAccessTokenAsync(firelyAuthEndpoint);
                        if (!token.IsError)
                            httpClient.SetBearerToken(token.AccessToken!);

                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, firelyAuthEndpoint)
                        {
                            Content = new StringContent(json)
                        };
                        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        var res = await httpClient.SendAsync(request);

                        _logger.LogDebug($"Relationships Sent to FA Mgmt API. Response {res.StatusCode} received.");

                    }
                    else
                    {
                        response.Add("result", new FhirBoolean(false));
                    }

                    vonkContext.Response.HttpResult = 200;
                    vonkContext.Response.Payload = response.ToTypedElement().ToIResource(vonkContext.InformationModel);

                    await ST.Task.CompletedTask;
                }

                private bool TryGetParameters(IVonkContext vonkContext, out Parameters? checkAuthorizedRepresentativeRelationshipsParameters)
                {
                    checkAuthorizedRepresentativeRelationshipsParameters = null;

                    var (request, _, response) = vonkContext.Parts();
                    if (!request.GetRequiredPayload(response, out var resource))
                    {
                        return false;
                    }

                    try
                    {
                        checkAuthorizedRepresentativeRelationshipsParameters = resource.ToPoco<Parameters>();
                    }
                    catch (Exception e)
                    {
                        vonkContext.Response.HttpResult = 400;
                        vonkContext.Response.Outcome.AddIssue(VonkOutcome.IssueSeverity.Error, VonkOutcome.IssueType.Invalid, details: e.Message);
                        return false;
                    }

                    return true;
                }
            }
        }


    
       
.. container:: toggle

    .. container:: header

      AuthorizedRepresentativeExampleConfiguration.cs
      
    Configuration works the same way as for any other Vonk plugin.

    .. code-block:: csharp

        using Duende.IdentityModel.Client;
        using Microsoft.AspNetCore.Builder;
        using Microsoft.Extensions.DependencyInjection;
        using Microsoft.Extensions.DependencyInjection.Extensions;
        using Vonk.Core.Context;
        using Vonk.Core.Pluggability;

        namespace Firely.Server.Plugin.CheckAuthorizedRepresentativeRelationships;

        [VonkConfiguration(order: 5600)]
        public static class AuthorizedRepresentativeExampleConfiguration
        {
            public static IServiceCollection ConfigureServices(this IServiceCollection services)
            {
                var clientCredentialsTokenManagementBuilder = services.AddClientCredentialsTokenManagement();

                var firelyAuthEndpoint = "http://localhost:5100/api/external/authorized-representative";
                clientCredentialsTokenManagementBuilder.AddClient(firelyAuthEndpoint, client =>
                {
                    client.ClientId = "firely-server-dar-plugin";
                    client.ClientSecret = "firely-server-dar-plugin";
                    client.Scope = "http://server.fire.ly/auth/scope/authmanagement";
                    client.TokenEndpoint = "http://localhost:5100/connect/token";
                    client.Resource = "http://server.fire.ly/auth/api/management";
                });

                services.AddClientCredentialsHttpClient(firelyAuthEndpoint, firelyAuthEndpoint, client =>
                {
                    client.BaseAddress = new Uri(firelyAuthEndpoint);
                });

                Console.WriteLine("Setting up Auth Rep Plugin Services");
                services.TryAddScoped<CheckAuthorizedRepresentativeRelationshipsExampleService>();
                return services;
            }

            public static IApplicationBuilder ConfigureHandlers(this IApplicationBuilder app)
            {
                return app
                    .OnCustomInteraction(VonkInteraction.type_custom, "check-authorized-representative-relationships")
                    .AndMethod("POST").AndResourceTypes(new[] { "Patient" })            
                    .HandleAsyncWith<CheckAuthorizedRepresentativeRelationshipsExampleService>((svc, context) => 
                        svc.CheckAuthorizedRepresentativeRelationshipsOnPost(context));
            }
        }




Designated Authorized Representative Workflow and Diagram
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

The DAR authentication flow works as follows:

1. **User initiates login** - Standard OAuth2/OIDC flow begins
2. **Firely Auth checks for relationships** - Calls custom operation with user's fhirUser claim
3. **Firely Server validates relationships** - Plugin queries your relationship data store  
4. **Conditional selection screen** - Only shown if relationships exist
5. **Token issuance** - Contains both original user identity and selected patient context

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
         │                              │  2. POST $check-authorized-    │
         │                              │     representative-            │
         │                              │     relationships              │
         │                              │     (patient: fhirUser)        │
         │                              │───────────────────────────────>│
         │                              │                                │
         │                              │                                │  3. Query relationships
         │                              │                                │     in data store
         │                              │                                │──┐
         │                              │                                │  │
         │                              │                                │<─┘
         │                              │                                │
         │                              │  4. Response with result       │
         │                              │     (true/false)               │
         │                              │<───────────────────────────────│
         │                              │                                │
         │                              │                                │  5. (Optional) Async POST
         │                              │  6. POST /api/external/        │     relationship details
         │                              │     authorized-representative  │     to Firely Auth Admin API
         │                              │     (relationships details)    │──┐
         │                              │<───────────────────────────────│  │
         │                              │                                │<─┘
         │  7. Selection screen         │                                │
         │     (if result = true)       │                                │
         │<─────────────────────────────│                                │
         │                              │                                │
         │  8. Select capacity          │                                │
         │     (self or representative) │                                │
         │─────────────────────────────>│                                │
         │                              │                                │
         │  9. Access token issued      │                                │
         │     (fhirUser + patient)     │                                │
         │<─────────────────────────────│                                │
         │                              │                                │











