.. _firely_auth_dar:

Designated Authorized Representative
====================================

Firely Auth supports Designated Authorized Representatives (DAR): a user signs in to an app and acts for a patient they represent, such as a parent for a child. The app gets an access token for that patient's data.

Who may act for whom is held in Firely Server. A representative is linked to each patient through a ``RelatedPerson`` of that patient, which carries the identifier ``http://server.fire.ly/auth/identifier/sub|<user id>`` of the representative's Firely Auth account. Firely Auth asks Firely Server, through two custom operations, whom the user may act for and whether a choice is still authorized. You implement these operations in a Firely Server plugin, see :ref:`firely_auth_dar_contract`.

Two kinds of accounts can act for others:

- a ``Representative``: an account for a person who only acts for others, see :ref:`firely_auth_dar_onboarding`;
- a ``Patient`` who signs in through an external identity provider, and also acts for someone else.

.. image:: /images/fa_dar_selection.png
    :align: center
    :class: bordered-image
    :alt: Designated Authorized Representative Selection Screen

.. warning::

    **Deprecated:** the Patient-account flow, in which Firely Server finds the relationships of a ``Patient`` account through the user's ``Patient`` ``fhirUser``, is deprecated. It is only used while ``AuthorizedRepresentatives:LookupBySub`` is ``false``, see :ref:`firely_auth_dar_deprecated`. Move your Firely Server plugin to the operations in :ref:`firely_auth_dar_contract`. While ``LookupOnLogin`` is on and ``LookupBySub`` is ``false``, Firely Auth logs a ``Warning`` about this at startup.

.. warning::

    This is a beta feature and as such it is subject to change in future releases. Test it thoroughly in a development or staging environment before you use it in production.

.. warning::

    The DAR feature does not support running more than one Firely Auth instance behind a load balancer. The relationships that Firely Server pushes to Firely Auth, and the choice the user then makes, are kept only in the memory of the Firely Auth instance that received them. They are not shared with other instances or stored in the database.

    With more than one instance, these requests can each land on a different instance:

    - Firely Server pushing the relationships to ``/api/external/representative-relationships``,
    - the user's browser loading the selection page and submitting the choice,
    - the client application requesting the access token from the token endpoint.

    If the push lands on another instance than the one serving the user, the selection page does not find the relationships and reports a technical problem. If the token request lands on another instance than the one that received the choice, the app gets no token for the patient that was chosen. Sticky sessions on the load balancer do not prevent this, because Firely Server and the client application call Firely Auth directly rather than through the user's browser.

    If you use DAR, run a single Firely Auth instance.

.. _firely_auth_dar_login:

Signing in for someone else
---------------------------

When a user who may act for others signs in to an app, Firely Auth calls ``$representative-relationships`` on Firely Server, once per sign-in, after the user has authenticated. Firely Server pushes the patients the user may act for to Firely Auth, and Firely Auth shows the selection page ``/user/representees``. The user chooses whom they act for, and the app gets a token for that choice.

A Representative
^^^^^^^^^^^^^^^^

- The selection page is always shown, also when there is only one patient to choose. It only lists the patients the Representative may act for: a Representative does not have a record of their own.
- With nobody to act for, the page shows ``RepresentativeTitleOnFail`` and ``RepresentativeMessageOnFail`` and a **back to the app** button, and the app gets ``access_denied``. The app also gets ``access_denied`` when Firely Server does not answer ``$representative-relationships``.
- This works whatever ``LookupOnLogin`` is set to.

A Patient who acts for others
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

- Only a ``Patient`` who signs in through an external identity provider, and only while ``LookupOnLogin`` is ``true`` and ``LookupBySub`` is ``true``. A ``Patient`` with a password signs in as themselves, without any call to Firely Server, for the same reason that a Representative has no password (see :ref:`firely_auth_dar_onboarding`).
- The selection page offers the user themselves first, under ``TitleOwnProfileOnLogin``, and then the patients they may act for.
- With nobody to act for, or when Firely Server fails, the page is skipped and the user continues as themselves. Firely Auth logs a ``Warning`` when Firely Server fails.
- Firely Auth writes nothing to Firely Server during the sign-in.

The selection page
^^^^^^^^^^^^^^^^^^

- The page waits up to 20 seconds for the push. After that, it reports a technical problem. A Patient can then continue as themselves.
- The page shows the name and the relationship that Firely Server pushed. It never shows the ``RelatedPerson``.
- When the user chooses someone, Firely Auth calls ``Patient/$check-authorized-representative`` with the chosen pair before it accepts the choice. Unless Firely Server answers ``result = true``, the page says that the user cannot act for that person. A Representative can then go back to the app, and a Patient can continue as themselves.
- A choice serves one token request of the app it was made for. Disclaimers are agreed once for each person the user acts for.
- A request with ``prompt=none`` cannot show the page. Keep a representative signed in with refresh tokens, not with silent renewal.

The token
^^^^^^^^^

.. list-table::
   :header-rows: 1
   :widths: 30 35 35

   * - Who acts for whom
     - ``fhirUser``
     - ``patient``
   * - A Representative for a patient
     - ``RelatedPerson/<id>``, the RelatedPerson that Firely Server pushed for that patient
     - the chosen patient
   * - A Patient for another patient
     - ``RelatedPerson/<id>``, the RelatedPerson that Firely Server pushed for that patient
     - the chosen patient
   * - A Patient as themselves
     - their own ``Patient``
     - their own patient, as without DAR

The token response has ``patient`` whatever the scopes. The userinfo endpoint returns the same ``fhirUser``, and so does the id_token of a client with ``AlwaysIncludeUserClaimsInIdToken``. Firely Server restricts the token to the chosen patient's compartment, as for any token with a ``patient`` claim.

Every refresh of a token for someone else calls ``$check-authorized-representative`` again with the pair in the token. When Firely Server answers ``false``, or cannot be reached, the refresh fails with ``invalid_grant``. So a revoked authorization ends access at the next refresh. A refresh also fails once the user can no longer act for others, for example when an admin changed their user type. Firely Auth logs the ``consent`` reference that Firely Server returns.

Launch contexts
^^^^^^^^^^^^^^^

- **Standalone launch** (``launch/patient`` and other ``launch/...`` scopes, without ``launch``): the selection page is shown, and the chosen patient is the launch patient. Other launch contexts, such as ``launch/encounter``, are chosen afterwards, within that patient's compartment.
- **EHR launch** (the ``launch`` scope): a Representative gets ``access_denied``, and a Patient continues as themselves, with the patient of the launch context.

Settings
^^^^^^^^

.. code-block:: json

  "AuthorizedRepresentatives": {
    "LookupOnLogin": false,
    "LookupBySub": true,
    "RepresentativeRelationshipsScopes": "http://server.fire.ly/auth/scope/representative-relationships",
    "CheckAuthorizedRepresentativeScopes": "http://server.fire.ly/auth/scope/check-authorized-representative system/Patient.rs",
    "CheckAuthorizedRepresentativeTimeout": 5000, // in ms
    "TitleOnLogin": "Who Are You Signing In As?",
    "TitleOwnProfileOnLogin": "Continue as Yourself",
    "TitleAuthorizedProfilesOnLogin": "Continue On Behalf Of ...",
    "RepresentativeTitleOnFail": "No one to act for",
    "RepresentativeMessageOnFail": "Your account is not linked to anyone you can act for at the moment. Please contact the organization that invited you."
  }

- ``LookupOnLogin``: whether a ``Patient`` who signs in through an external identity provider is asked whom they act for. It does not affect Representatives.
- ``LookupBySub``: ``true`` (the default) uses the operations in :ref:`firely_auth_dar_contract` for Patients. ``false`` keeps the deprecated Patient-account flow for all Patients, for a Firely Server plugin that does not implement ``$representative-relationships`` yet.
- ``RepresentativeRelationshipsScopes``: the scopes of the token for ``$representative-relationships``.
- ``CheckAuthorizedRepresentativeScopes``: the scopes of the token for ``$check-authorized-representative``.
- ``CheckAuthorizedRepresentativeTimeout``: the timeout in milliseconds of each call to Firely Server.
- ``TitleOnLogin``, ``TitleOwnProfileOnLogin``, ``TitleAuthorizedProfilesOnLogin``: the title of the page, and the titles above the user's own card and above the patients they may act for.
- ``RepresentativeTitleOnFail``, ``RepresentativeMessageOnFail``: the title and message for a Representative with nobody to act for.

See :ref:`firely_auth_settings_representatives` for the settings of the deprecated flow.

.. _firely_auth_dar_onboarding:

Onboarding representatives
--------------------------

The Firely Auth account of a representative is created before their first login, in one of two ways:

- by an application, through the representative API, for example the application in which a patient registers the person who may act for them;
- by an admin, in the admin UI.

The representative then gets an invitation email and signs in through an external identity provider (see :ref:`firely_auth_sso`).

A ``Representative`` account is for people who only act on behalf of others. Such a person does not need to be a patient: their account has no ``fhirUser`` and needs no ``Patient`` resource of their own in Firely Server. In the deprecated flow, a representative needed a ``Patient`` account, so every representative had to be registered as a patient first. Firely Server finds the patients a Representative may act for through the ``sub`` identifier of their Firely Auth account on the ``RelatedPerson``.

Acting for others is not a user type: it is held in Firely Server, on the ``RelatedPerson``. So a person who is a patient and also acts for someone else, such as a parent, keeps one ``Patient`` account, as long as they sign in through an external identity provider.

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

A ``Representative`` signs in only through an external identity provider, and has no password and no ``fhirUser``. When they sign in to an app, they choose whom they act for, see :ref:`firely_auth_dar_login`. An SSO login does not change a Representative into a Patient or Practitioner. An admin can turn a Representative into a Patient by changing its user type.

.. note::

    Representatives cannot have a local account with a password. This is on purpose. A Representative acts on behalf of a patient, so their identity must have been verified, and that verification is what the identity provider vouches for. With a local account, Firely Auth only knows that someone controls an email address, which is not enough to act for someone else.

Representatives in the admin UI
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

- **Add User** offers the user type *Representative* when at least one external identity provider is configured. A Representative gets no password and no claims. Saving the user sends the invitation. An email address that another account uses is refused. When the invitation cannot be sent, the account is kept and the admin UI says so; send it again once the configuration is fixed.
- The user list shows the type *Representative*, and the user type filter has it.
- The panel of a Representative shows whether the invitation has been accepted, with **resend invitation** until the first login.
- The panel of a Representative, and of a ``Patient`` who signs in through an external identity provider, lists the patients the account is linked to, each with **unlink**. For each patient, it shows whether Firely Server authorizes the account to act for that patient now, as ``$check-authorized-representative`` answers (see :ref:`firely_auth_dar_contract`): *Authorized*, with the ``Consent`` when Firely Server returns one, *Not authorized*, or *Authorization unknown* when Firely Server gave no answer.
- **link representee** links a ``RelatedPerson`` in Firely Server to the account: enter its reference, **find** it, check the name, patient and relationship that Firely Auth shows, and link it. Firely Auth adds the identifier ``http://server.fire.ly/auth/identifier/sub|<user id>`` to the ``RelatedPerson``. Linking does not create the ``RelatedPerson`` or its ``Consent``; the application that onboards the representative creates those in Firely Server.
- A ``RelatedPerson`` that is linked to another account, or whose patient is the account's own ``fhirUser`` Patient, is refused with ``409 Conflict``. A ``Patient`` who signs in with a password cannot be linked (``400 Bad Request``), for the same reason that Representatives have no password: the identity provider vouches that the person's identity was verified.
- A Backend Service client has the switch *Allow Representative Api Access* in its **Permissions**, the client setting ``AllowRepresentativeApiAccess``. Changing the client type to another type turns it off.

The admin UI is the only place to create a Representative by hand and to link or unlink its patients. The management API (``/api/external/user``) refuses to create a Representative with ``400 Bad Request`` and does not offer linking. An application uses the representative API instead.

Firely Auth reads and changes the ``RelatedPerson`` in Firely Server with a token that it issues to itself, with the audience ``FhirServer:FHIR_BASE_URL``. To show the linked patients and to **find** a ``RelatedPerson``, the token holds the scopes ``system/RelatedPerson.rs`` and ``system/Patient.rs``. For **link representee** and **unlink**, it holds ``system/RelatedPerson.rus`` and ``system/Patient.rs``. **link representee** and **unlink** read the ``RelatedPerson``, change only its identifier and write it back with a ``PUT`` with ``If-Match`` on the version that was read. When the ``RelatedPerson`` was changed in Firely Server in the meantime, Firely Auth asks the admin to find it again and retry.

No client in Firely Auth needs a setting for this, also not the admin UI. Firely Server needs to accept this token for writes:

- Firely Server accepts tokens from Firely Auth with the audience ``FhirServer:FHIR_BASE_URL``, see :ref:`firely_auth_settings`.
- The ``update`` interaction is enabled for ``RelatedPerson``.
- Firely Server returns a version for the ``RelatedPerson``, as an ``ETag`` or in ``meta.versionId``. Firely Auth does not update a ``RelatedPerson`` without a version.
- No proxy or gateway in front of Firely Server refuses writes with this token.

When Firely Server refuses the ``PUT``, **link representee** shows an error that names the scope ``system/RelatedPerson.rus``, and the list of linked patients stays readable.

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

.. _firely_auth_dar_contract:

Implementing the operations in Firely Server
--------------------------------------------

Firely Auth calls two custom operations on Firely Server, and Firely Server pushes the relationships to Firely Auth. Implement both operations in a Firely Server plugin. Firely Server ships no implementation. The example plugin below implements both, with ``RelatedPerson`` and ``Consent`` resources in Firely Server.

The calls of Firely Auth
^^^^^^^^^^^^^^^^^^^^^^^^

Firely Auth calls both operations with a token that it issues to itself:

- ``client_id``: ``Firely-Auth-Internal-FhirLookup``. Firely Auth reserves this client id: no client can be registered with it.
- audience: ``FhirServer:FHIR_BASE_URL``.
- scopes: ``RepresentativeRelationshipsScopes`` for ``$representative-relationships``, and ``CheckAuthorizedRepresentativeScopes`` for ``$check-authorized-representative``.
- for ``$representative-relationships`` only, a ``sub`` claim with the user's Firely Auth id. The token has no ``fhirUser`` or ``patient`` claim.

Firely Server must accept tokens from Firely Auth with this audience (see :ref:`firely_auth_settings`), and both operations must be in the ``Operations`` setting of Firely Server, with the scope that Firely Auth requests as ``OperationScope``:

.. code-block:: json

  "$representative-relationships": {
    "Name": "$representative-relationships",
    "Level": [ "System" ],
    "Enabled": true,
    "RequireAuthorization": "WhenAuthEnabled",
    "OperationScope": "http://server.fire.ly/auth/scope/representative-relationships",
    "RequireTenant": "Never"
  },
  "$check-authorized-representative": {
    "Name": "$check-authorized-representative",
    "Level": [ "Type" ],
    "Enabled": true,
    "RequireAuthorization": "WhenAuthEnabled",
    "OperationScope": "http://server.fire.ly/auth/scope/check-authorized-representative",
    "RequireTenant": "Never"
  }

$representative-relationships
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

- **URL**: ``POST [base]/$representative-relationships``, at system level.
- **Input**: none. The user is the ``sub`` claim of the token. Because the operation has no parameter that names a user, a token can only lead to a push for its own user.
- **What it does**: it finds the patients the user may act for now, and pushes them to Firely Auth (see below). It may push before it answers, or after.
- **Output**: a ``Parameters`` resource with ``result`` = ``true`` when the user may act for at least one patient.

.. code-block:: json

  {
    "resourceType": "Parameters",
    "parameter": [ { "name": "result", "valueBoolean": true } ]
  }

What "may act for" means is up to the operation. Typically it finds the ``RelatedPerson`` resources with the identifier ``http://server.fire.ly/auth/identifier/sub|<sub>``, and keeps those with an authorization that is in force. A person can represent several patients. A ``Consent`` resource is optional for Firely Auth: an organization that does not create Consent resources decides the authorization in its own way, for example from the ``RelatedPerson`` itself or from another system.

The push to Firely Auth
^^^^^^^^^^^^^^^^^^^^^^^

.. code-block:: HTTP

  POST <firely-auth-url>/api/external/representative-relationships HTTP/1.1
  Authorization: Bearer <management API token>
  Content-Type: application/json

  {
    "UserId": "8f0c…",
    "Relationships": [
      { "PatientId": "123", "FullName": "Anna de Vries", "Relationship": "Mother", "Representative": "RelatedPerson/456" }
    ]
  }

- ``UserId`` is the ``sub`` of the token.
- Each relationship has ``PatientId`` (the id of the Patient), ``FullName`` (the patient's name, which the page shows), ``Representative`` (the ``RelatedPerson`` as a relative reference ``RelatedPerson/<id>``) and optionally ``Relationship`` (text that the page shows, such as ``Mother``).
- An empty ``Relationships`` list means that the user may act for nobody.
- Each push replaces the list that Firely Auth holds for the user. A push may come at any time, also after the operation has answered, or later when a relationship changes. The next sign-in uses the list that Firely Auth holds.
- The endpoint answers ``400 Bad Request`` for a user that does not exist or is not a ``Representative`` or ``Patient``, for a missing or invalid field, and for a list that names the same patient twice. One user acting for one patient through two RelatedPersons is not supported: push one of them.
- The token is a management API token, see :ref:`firely_auth_mgmt`. Register a client of the type *Management API* in Firely Auth for your plugin.

.. important::

    Firely Auth trusts the push. It does not check that each ``Representative`` carries the user's ``sub`` identifier, or that the ``RelatedPerson`` belongs to ``PatientId``. Your operation must push only RelatedPersons that do. Firely Auth checks each pair again with ``$check-authorized-representative`` when the user chooses, and at every refresh, before it issues a token.

The endpoint ``/api/external/authorized-representative`` is only for the deprecated Patient-account flow, see :ref:`firely_auth_dar_deprecated`.

$check-authorized-representative
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

- **URL**: ``POST [base]/Patient/$check-authorized-representative``, at type level.
- **Input**: a ``Parameters`` resource with ``patient`` (``valueReference``, ``Patient/<id>``) and ``representative`` (``valueReference``, ``RelatedPerson/<id>``). The token has no user claims.
- **Output**: a ``Parameters`` resource with ``result`` (boolean): whether the RelatedPerson may act for the patient now. Optionally ``consent`` (``valueReference``), which Firely Auth logs.

The token has no user, so the check cannot see which user made the choice. The example checks that the RelatedPerson still carries a ``sub`` identifier and is still authorized for the patient. That the RelatedPerson belongs to the user who signed in is up to Firely Auth: it offers only the RelatedPersons of the last push for that user.

.. code-block:: json

  {
    "resourceType": "Parameters",
    "parameter": [
      { "name": "patient", "valueReference": { "reference": "Patient/123" } },
      { "name": "representative", "valueReference": { "reference": "RelatedPerson/456" } }
    ]
  }

.. code-block:: json

  {
    "resourceType": "Parameters",
    "parameter": [
      { "name": "result", "valueBoolean": true },
      { "name": "consent", "valueReference": { "reference": "Consent/789" } }
    ]
  }

Firely Auth calls it when the user chooses someone, and at every refresh of a token for that choice. Any answer other than ``result = true``, also an error or a timeout, refuses the choice or fails the refresh.

Checking the caller
^^^^^^^^^^^^^^^^^^^

Both operations must refuse a caller other than Firely Auth itself, because a push replaces what Firely Auth holds for a user:

- Accept only tokens with ``client_id`` = ``Firely-Auth-Internal-FhirLookup``, and answer ``403`` otherwise. This is a check on the name: it relies on no other client in Firely Auth having that client id, which Firely Auth makes sure of.
- ``$representative-relationships`` takes the user only from the ``sub`` claim, and answers ``403`` for a token without one.

The example plugin
^^^^^^^^^^^^^^^^^^

The example plugin ``Vonk.Plugin.AuthorizedRepresentativeExample`` implements both operations. A user may act for a patient through a ``RelatedPerson`` that:

- carries the identifier ``http://server.fire.ly/auth/identifier/sub|<user id>``. Firely Auth adds it when an admin links the RelatedPerson to the user (see :ref:`firely_auth_dar_onboarding`);
- refers to the patient in ``RelatedPerson.patient``;
- is not ``active = false``, and has a ``period`` that covers the current time, if it has one; a date in a period covers that whole day, month or year, and a time without fractions of a second covers that whole second;
- is the ``actor`` of an active ``Consent`` of that patient, which does not deny, and whose period covers the current time. For FHIR R4 that is ``provision.type`` and ``provision.period``, for FHIR R5 ``decision`` and ``period``.

To decide the authorization without Consent resources, replace ``FindConsentAsync`` in ``RepresentativeAuthorizations.cs``. The plugin pushes before it answers. When the push fails, ``$representative-relationships`` answers ``500``.

Configure the plugin in Firely Server:

.. code-block:: json

  "AuthorizedRepresentativeExample": {
    "FirelyAuthUrl": "https://auth.example.org", // optional, defaults to SmartAuthorizationOptions:Authority
    "ClientId": "firely-server-representatives",
    "ClientSecret": "<secret>"
  }

- ``ClientId`` and ``ClientSecret``: a client in Firely Auth of the type *Management API*, with ``AllowManagementApiAccess`` on. The plugin requests the scope ``http://server.fire.ly/auth/scope/authmanagement``.
- Add ``Vonk.Plugin.AuthorizedRepresentativeExample`` to the ``Include`` of the ``/`` branch in ``PipelineOptions``, and both operations to ``Operations`` (see above).
- SMART on FHIR authorization must be enabled in Firely Server. Without a token, both operations answer ``403``.

The plugin references ``Vonk.Core`` and ``Duende.AccessTokenManagement`` only.

.. container:: toggle

    .. container:: header

      AuthorizedRepresentativeExampleConfiguration.cs

    .. code-block:: csharp

        using Duende.AccessTokenManagement;
        using Microsoft.AspNetCore.Builder;
        using Microsoft.AspNetCore.Http;
        using Microsoft.Extensions.Configuration;
        using Microsoft.Extensions.DependencyInjection;
        using Microsoft.Extensions.DependencyInjection.Extensions;
        using Vonk.Core.Common;
        using Vonk.Core.Context;
        using Vonk.Core.Pluggability;

        namespace Vonk.Plugin.AuthorizedRepresentativeExample;

        /// <summary>
        /// Adds the operations <c>$representative-relationships</c> and <c>Patient/$check-authorized-representative</c>,
        /// which Firely Auth calls to let a user act for someone they represent.
        /// </summary>
        /// <remarks>
        /// Add <c>Vonk.Plugin.AuthorizedRepresentativeExample</c> to <c>PipelineOptions</c>, and both operations to <c>Operations</c>.
        /// Configure the management API client in the section <see cref="AuthorizedRepresentativeExampleOptions.Section"/>.
        /// </remarks>
        [VonkConfiguration(order: 5600)]
        public static class AuthorizedRepresentativeExampleConfiguration
        {
            private static readonly ClientCredentialsClientName FirelyAuthClient = ClientCredentialsClientName.Parse("firely-auth-representative-relationships");

            private const string ManagementApiScope = "http://server.fire.ly/auth/scope/authmanagement";
            private const string ManagementApiResource = "http://server.fire.ly/auth/api/management";

            /// <summary>
            /// Registers the services of the plugin.
            /// </summary>
            public static IServiceCollection ConfigureServices(IServiceCollection services, IConfiguration configuration)
            {
                var options = configuration.GetSection(AuthorizedRepresentativeExampleOptions.Section).Get<AuthorizedRepresentativeExampleOptions>() ?? new();
                var firelyAuthUrl = string.IsNullOrEmpty(options.FirelyAuthUrl) ? configuration["SmartAuthorizationOptions:Authority"] : options.FirelyAuthUrl;

                if (string.IsNullOrEmpty(firelyAuthUrl) || !Uri.TryCreate(firelyAuthUrl.TrimEnd('/') + "/", UriKind.Absolute, out var firelyAuthBase))
                    throw new VonkConfigurationException(
                        $"Set '{AuthorizedRepresentativeExampleOptions.Section}:{nameof(AuthorizedRepresentativeExampleOptions.FirelyAuthUrl)}' or 'SmartAuthorizationOptions:Authority' to the url of Firely Auth.");

                if (string.IsNullOrEmpty(options.ClientId) || string.IsNullOrEmpty(options.ClientSecret))
                    throw new VonkConfigurationException(
                        $"Set '{AuthorizedRepresentativeExampleOptions.Section}:{nameof(AuthorizedRepresentativeExampleOptions.ClientId)}' and " +
                        $"'{AuthorizedRepresentativeExampleOptions.Section}:{nameof(AuthorizedRepresentativeExampleOptions.ClientSecret)}' to a management API client of Firely Auth.");

                services.AddClientCredentialsTokenManagement()
                    .AddClient(FirelyAuthClient, client =>
                    {
                        client.TokenEndpoint = new Uri(firelyAuthBase, "connect/token");
                        client.ClientId = ClientId.Parse(options.ClientId);
                        client.ClientSecret = ClientSecret.Parse(options.ClientSecret);
                        client.Scope = Scope.Parse(ManagementApiScope);
                        client.Resource = Resource.Parse(ManagementApiResource);
                    });

                services.AddHttpClient<FirelyAuthRelationshipsClient>(client => client.BaseAddress = firelyAuthBase)
                    .AddClientCredentialsTokenHandler(FirelyAuthClient);

                services.TryAddSingleton(TimeProvider.System);
                services.TryAddScoped<RepresentativeAuthorizations>();
                services.TryAddScoped<RepresentativeRelationshipsOperation>();
                services.TryAddScoped<CheckAuthorizedRepresentativeOperation>();
                return services;
            }

            /// <summary>
            /// Adds the operations to the pipeline.
            /// </summary>
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

.. container:: toggle

    .. container:: header

      AuthorizedRepresentativeExampleOptions.cs

    .. code-block:: csharp

        namespace Vonk.Plugin.AuthorizedRepresentativeExample;

        /// <summary>
        /// The settings of the example plugin, in the section <c>AuthorizedRepresentativeExample</c>.
        /// </summary>
        internal sealed class AuthorizedRepresentativeExampleOptions
        {
            /// <summary>
            /// The name of the configuration section.
            /// </summary>
            public const string Section = "AuthorizedRepresentativeExample";

            /// <summary>
            /// The base url of Firely Auth. Leave it empty to use <c>SmartAuthorizationOptions:Authority</c>.
            /// </summary>
            public string? FirelyAuthUrl { get; set; }

            /// <summary>
            /// The id of the management API client in Firely Auth that pushes the relationships.
            /// </summary>
            public string? ClientId { get; set; }

            /// <summary>
            /// The secret of the management API client in Firely Auth.
            /// </summary>
            public string? ClientSecret { get; set; }
        }

.. container:: toggle

    .. container:: header

      FirelyAuthCaller.cs

    .. code-block:: csharp

        using System.Diagnostics.CodeAnalysis;
        using System.Security.Claims;

        namespace Vonk.Plugin.AuthorizedRepresentativeExample;

        /// <summary>
        /// Checks that a request comes from Firely Auth itself.
        /// </summary>
        /// <remarks>
        /// Firely Auth calls both operations with a token it issues to itself, with the client id <see cref="ClientId"/>.
        /// Firely Auth refuses to register a client with that id, so no app can get a token with it.
        /// This is a check on the name only: it relies on no other client in Firely Auth having that client id.
        /// </remarks>
        internal static class FirelyAuthCaller
        {
            /// <summary>
            /// The client id of the tokens that Firely Auth issues to itself.
            /// </summary>
            public const string ClientId = "Firely-Auth-Internal-FhirLookup";

            private const string ClientIdClaim = "client_id";
            private const string SubjectClaim = "sub";

            /// <summary>
            /// Returns whether <paramref name="user"/> holds a token of Firely Auth itself.
            /// </summary>
            public static bool IsFirelyAuth(ClaimsPrincipal user) =>
                user.Claims.Where(c => c.Type == ClientIdClaim).Select(c => c.Value).ToList() is [ClientId];

            /// <summary>
            /// Returns the Firely Auth user id that a token of Firely Auth itself carries in its <c>sub</c> claim.
            /// </summary>
            /// <returns><see langword="false"/> when the token is not one of Firely Auth itself, or has no <c>sub</c> claim.</returns>
            public static bool TryGetUserId(ClaimsPrincipal user, [NotNullWhen(true)] out string? userId)
            {
                userId = null;
                if (!IsFirelyAuth(user))
                    return false;

                // A JWT bearer token has 'sub' mapped to NameIdentifier, a token read by introspection keeps 'sub'
                var subjects = user.Claims
                    .Where(c => c.Type is SubjectClaim or ClaimTypes.NameIdentifier)
                    .Select(c => c.Value)
                    .Distinct()
                    .ToList();
                if (subjects is not [{ Length: > 0 } subject])
                    return false;

                userId = subject;
                return true;
            }
        }

.. container:: toggle

    .. container:: header

      RepresentativeRelationshipsOperation.cs

    .. code-block:: csharp

        using Microsoft.AspNetCore.Http;
        using Microsoft.Extensions.Logging;
        using Vonk.Core.Context;

        namespace Vonk.Plugin.AuthorizedRepresentativeExample;

        /// <summary>
        /// Implements <c>POST [base]/$representative-relationships</c>.
        /// </summary>
        /// <remarks>
        /// Firely Auth calls it once per sign-in of a user who may act for others. The operation has no input parameters: the user is the <c>sub</c> of the token.
        /// It pushes the patients the user may act for to Firely Auth, and then answers <c>result</c>: <see langword="true"/> when there is at least one.
        /// This example pushes before it answers. A push may also come after the answer, or at any other time.
        /// </remarks>
        internal sealed class RepresentativeRelationshipsOperation(
            RepresentativeAuthorizations authorizations,
            FirelyAuthRelationshipsClient firelyAuth,
            ILogger<RepresentativeRelationshipsOperation> logger)
        {
            /// <summary>
            /// The name of the operation, without <c>$</c>.
            /// </summary>
            public const string Name = "representative-relationships";

            /// <summary>
            /// Handles the operation.
            /// </summary>
            public async Task HandleAsync(IVonkContext vonkContext)
            {
                var (_, _, response) = vonkContext.Parts();
                var httpContext = vonkContext.HttpContext();

                if (!FirelyAuthCaller.TryGetUserId(httpContext.User, out var userId))
                {
                    logger.LogWarning("${Operation} was refused: the token is not one of Firely Auth itself, or has no sub claim", Name);
                    response.HttpResult = StatusCodes.Status403Forbidden;
                    response.Outcome.AddIssue(VonkIssue.FORBIDDEN, "Only Firely Auth may call this operation, with a token for a user.");
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
                catch (HttpRequestException ex)
                {
                    logger.LogError(ex, "Pushing {Count} representative relationships of user {UserId} to Firely Auth failed", representees.Count, userId);
                    response.HttpResult = StatusCodes.Status500InternalServerError;
                    response.Outcome.AddIssue(VonkIssue.INTERNAL_ERROR, "The request could not be processed.");
                    return;
                }

                logger.LogInformation("Pushed {Count} representative relationships of user {UserId} to Firely Auth", representees.Count, userId);
                response.HttpResult = StatusCodes.Status200OK;
                response.Payload = OperationParameters.Create(representees.Count > 0, consent: null, vonkContext.InformationModel);
            }
        }

.. container:: toggle

    .. container:: header

      CheckAuthorizedRepresentativeOperation.cs

    .. code-block:: csharp

        using System.Text.RegularExpressions;
        using Microsoft.AspNetCore.Http;
        using Microsoft.Extensions.Logging;
        using Vonk.Core.Context;

        namespace Vonk.Plugin.AuthorizedRepresentativeExample;

        /// <summary>
        /// Implements <c>POST [base]/Patient/$check-authorized-representative</c>.
        /// </summary>
        /// <remarks>
        /// Firely Auth calls it when a user chooses whom to act for, and at every refresh of a token for that choice.
        /// The input parameters are <c>patient</c> (<c>Patient/&lt;id&gt;</c>) and <c>representative</c> (<c>RelatedPerson/&lt;id&gt;</c>).
        /// It answers <c>result</c>, and <c>consent</c> with the Consent that authorizes the pair.
        /// </remarks>
        internal sealed partial class CheckAuthorizedRepresentativeOperation(
            RepresentativeAuthorizations authorizations,
            ILogger<CheckAuthorizedRepresentativeOperation> logger)
        {
            /// <summary>
            /// The name of the operation, without <c>$</c>.
            /// </summary>
            public const string Name = "check-authorized-representative";

            private const string PatientParameter = "patient";
            private const string RepresentativeParameter = "representative";

            [GeneratedRegex(@"^Patient/(?<id>[A-Za-z0-9\-\.]{1,64})$")]
            private static partial Regex PatientReference();

            [GeneratedRegex(@"^RelatedPerson/(?<id>[A-Za-z0-9\-\.]{1,64})$")]
            private static partial Regex RelatedPersonReference();

            /// <summary>
            /// Handles the operation.
            /// </summary>
            public async Task HandleAsync(IVonkContext vonkContext)
            {
                var (request, args, response) = vonkContext.Parts();
                args.ResourceTypeArguments().Handled();

                if (!FirelyAuthCaller.IsFirelyAuth(vonkContext.HttpContext().User))
                {
                    logger.LogWarning("${Operation} was refused: the token is not one of Firely Auth itself", Name);
                    response.HttpResult = StatusCodes.Status403Forbidden;
                    response.Outcome.AddIssue(VonkIssue.FORBIDDEN, "Only Firely Auth may call this operation.");
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
                string? consent;
                try
                {
                    consent = await authorizations.CheckAsync(patientId, relatedPersonId, vonkContext);
                }
                catch (RepresentativeLookupException)
                {
                    response.HttpResult = StatusCodes.Status500InternalServerError;
                    response.Outcome.AddIssue(VonkIssue.INTERNAL_ERROR, "The request could not be processed.");
                    return;
                }

                logger.LogInformation("RelatedPerson/{RelatedPersonId} {Verdict} for Patient/{PatientId} ({Consent})",
                    relatedPersonId, consent is null ? "may not act" : "may act", patientId, consent ?? "no Consent in force");
                response.HttpResult = StatusCodes.Status200OK;
                response.Payload = OperationParameters.Create(consent is not null, consent, vonkContext.InformationModel);
            }
        }

.. container:: toggle

    .. container:: header

      RepresentativeAuthorizations.cs

    .. code-block:: csharp

        using System.Text.RegularExpressions;
        using Hl7.Fhir.ElementModel;
        using Microsoft.Extensions.Logging;
        using Vonk.Core.Common;
        using Vonk.Core.Context;
        using Vonk.Core.ElementModel;
        using Vonk.Core.Repository;

        namespace Vonk.Plugin.AuthorizedRepresentativeExample;

        /// <summary>
        /// Finds out in Firely Server whom a user may act for.
        /// </summary>
        /// <remarks>
        /// A user may act for a patient through a RelatedPerson of that patient that carries the identifier
        /// <c>http://server.fire.ly/auth/identifier/sub|&lt;user id&gt;</c>. Firely Auth adds that identifier when an admin links the RelatedPerson to the user.
        /// The RelatedPerson must be active, and its period must cover the current time.
        /// This example also requires an active Consent with the RelatedPerson as actor, see <see cref="FindConsentAsync"/>.
        /// A Consent is optional: replace that method to decide the authorization in another way.
        /// </remarks>
        internal sealed partial class RepresentativeAuthorizations(ISearchRepository searchRepository, TimeProvider timeProvider, ILogger<RepresentativeAuthorizations> logger)
        {
            /// <summary>
            /// The system of the identifier that holds the id of a user in Firely Auth.
            /// </summary>
            public const string SubIdentifierSystem = "http://server.fire.ly/auth/identifier/sub";

            private const int MaxResults = 50;

            [GeneratedRegex(@"^Patient/(?<id>[A-Za-z0-9\-\.]{1,64})(?:/_history/[^/]+)?$")]
            private static partial Regex PatientReference();

            /// <summary>
            /// Returns the patients that the user may act for now, each patient once.
            /// </summary>
            /// <param name="userId">The id of the user in Firely Auth.</param>
            /// <param name="vonkContext">The request the lookup is for.</param>
            public async Task<IReadOnlyList<Representee>> FindRepresenteesAsync(string userId, IVonkContext vonkContext)
            {
                var relatedPersons = await SearchAsync(vonkContext, "RelatedPerson", ("identifier", $"{SubIdentifierSystem}|{EscapeSearchValue(userId)}"));

                List<(string PatientId, IResource RelatedPerson, string Consent)> authorized = [];
                foreach (var relatedPerson in relatedPersons.OrderBy(r => r.Id, StringComparer.Ordinal))
                {
                    if (await FindAuthorizationAsync(relatedPerson, vonkContext) is not (var patientId, var consent))
                        continue;

                    // Firely Auth refuses a push that names a patient twice, so keep the first RelatedPerson
                    if (authorized.Any(a => a.PatientId == patientId))
                    {
                        logger.LogWarning("User {UserId} has more than one authorized RelatedPerson for Patient/{PatientId}. Only the first one is used, RelatedPerson/{RelatedPersonId} is left out",
                            userId, patientId, relatedPerson.Id);
                        continue;
                    }

                    authorized.Add((patientId, relatedPerson, consent));
                }

                if (authorized.Count == 0)
                    return [];

                var patients = (await SearchAsync(vonkContext, "Patient", (ArgumentNames.resourceId, string.Join(',', authorized.Select(a => a.PatientId)))))
                    .ToDictionary(p => p.Id, StringComparer.Ordinal);

                List<Representee> representees = [];
                foreach (var (patientId, relatedPerson, consent) in authorized)
                {
                    if (!patients.TryGetValue(patientId, out var patient))
                    {
                        logger.LogWarning("RelatedPerson/{RelatedPersonId} refers to Patient/{PatientId}, which does not exist", relatedPerson.Id, patientId);
                        continue;
                    }

                    representees.Add(new Representee(patientId, PatientName(patient), RelationshipText(relatedPerson), $"RelatedPerson/{relatedPerson.Id}", consent));
                }

                return representees;
            }

            /// <summary>
            /// Returns the Consent that authorizes the RelatedPerson to act for the patient now.
            /// </summary>
            /// <param name="patientId">The id of the Patient.</param>
            /// <param name="relatedPersonId">The id of the RelatedPerson.</param>
            /// <param name="vonkContext">The request the check is for.</param>
            /// <returns>The Consent as <c>Consent/&lt;id&gt;</c>, or <see langword="null"/> when the RelatedPerson may not act for the patient.</returns>
            public async Task<string?> CheckAsync(string patientId, string relatedPersonId, IVonkContext vonkContext)
            {
                var relatedPerson = (await SearchAsync(vonkContext, "RelatedPerson", (ArgumentNames.resourceId, relatedPersonId))).SingleOrDefault();
                if (relatedPerson is null)
                    return null;

                // Only a RelatedPerson that is linked to a user in Firely Auth
                if (!relatedPerson.Children("identifier").Any(i => i.ChildString("system") == SubIdentifierSystem && !string.IsNullOrEmpty(i.ChildString("value"))))
                    return null;

                return await FindAuthorizationAsync(relatedPerson, vonkContext) is (var authorizedPatientId, var consent) && authorizedPatientId == patientId
                    ? consent
                    : null;
            }

            private async Task<(string PatientId, string Consent)?> FindAuthorizationAsync(IResource relatedPerson, IVonkContext vonkContext)
            {
                var now = timeProvider.GetUtcNow();
                if (relatedPerson.ChildString("active") == "false" || !FhirPeriod.Covers(relatedPerson.Child("period"), now))
                    return null;

                if (relatedPerson.Child("patient")?.ChildString("reference") is not { } reference ||
                    PatientReference().Match(ToRelative(reference, vonkContext.ServerBase)) is not { Success: true } match)
                    return null;

                var patientId = match.Groups["id"].Value;
                return await FindConsentAsync(patientId, relatedPerson.Id, vonkContext, now) is { } consent
                    ? (patientId, consent)
                    : null;
            }

            /// <summary>
            /// Finds an active Consent of the patient with the RelatedPerson as actor, that is in force now and does not deny.
            /// </summary>
            /// <remarks>
            /// This is the place to decide the authorization in another way, when you do not create Consent resources.
            /// It reads both FHIR R4 (<c>provision.period</c>, <c>provision.type</c>) and FHIR R5 (<c>period</c>, <c>decision</c>).
            /// </remarks>
            private async Task<string?> FindConsentAsync(string patientId, string relatedPersonId, IVonkContext vonkContext, DateTimeOffset now)
            {
                var consents = await SearchAsync(vonkContext, "Consent",
                    ("patient", $"Patient/{patientId}"),
                    ("actor", $"RelatedPerson/{relatedPersonId}"),
                    ("status", "active"));

                var consent = consents
                    .Where(c => c.ChildString("decision") != "deny" && c.Child("provision")?.ChildString("type") != "deny")
                    .Where(c => FhirPeriod.Covers(c.Child("period") ?? c.Child("provision")?.Child("period"), now))
                    .OrderBy(c => c.Id, StringComparer.Ordinal)
                    .FirstOrDefault();

                return consent is null ? null : $"Consent/{consent.Id}";
            }

            /// <summary>
            /// Returns <paramref name="reference"/> relative to <paramref name="serverBase"/>. A reference to another server stays absolute, and so matches no local Patient.
            /// </summary>
            private static string ToRelative(string reference, Uri serverBase)
            {
                var baseUrl = serverBase.ToString().TrimEnd('/') + "/";
                return reference.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase) ? reference[baseUrl.Length..] : reference;
            }

            /// <summary>
            /// Escapes the characters that have a meaning in a FHIR search value, so that the value is matched as it is.
            /// </summary>
            private static string EscapeSearchValue(string value) =>
                value.Replace(@"\", @"\\").Replace("|", @"\|").Replace(",", @"\,").Replace("$", @"\$");

            private async Task<IReadOnlyList<IResource>> SearchAsync(IVonkContext vonkContext, string resourceType, params (string Name, string Value)[] parameters)
            {
                ArgumentCollection arguments = new();
                arguments.AddArgument(new Argument(ArgumentSource.Internal, ArgumentNames.resourceType, resourceType));
                foreach (var (name, value) in parameters)
                {
                    arguments.AddArgument(new Argument(ArgumentSource.Internal, name, value) { MustHandle = true });
                }

                arguments.AddCount(MaxResults);

                // The lookup is on behalf of Firely Auth, which has no access to these resources itself.
                // It reads from the primary database, so that a Consent that was just created or withdrawn counts.
                var result = (await searchRepository.SearchCurrentWithFullAccess(arguments, vonkContext.ServerBase, vonkContext.InformationModel)).ToList();

                if (arguments.Any(a => a.MustHandle && a.Status != ArgumentStatus.Handled))
                {
                    var unhandled = string.Join(", ", arguments.Where(a => a.MustHandle && a.Status != ArgumentStatus.Handled).Select(a => a.ArgumentName));
                    logger.LogError("The repository did not handle the search parameter(s) {Parameters} on {ResourceType}", unhandled, resourceType);
                    throw new RepresentativeLookupException($"The search for {resourceType} could not be done.");
                }

                if (result.Count >= MaxResults)
                    logger.LogWarning("The search for {ResourceType} found {MaxResults} or more resources. Only the first {MaxResults} are used", resourceType, MaxResults, MaxResults);

                return result;
            }

            private static string PatientName(IResource patient)
            {
                var name = patient.Child("name");
                if (name?.ChildString("text") is { Length: > 0 } text)
                    return text;

                var parts = (name?.Children("given") ?? []).Select(g => g.Text).Append(name?.ChildString("family")).Where(p => !string.IsNullOrWhiteSpace(p));
                var fullName = string.Join(' ', parts);
                return fullName.Length > 0 ? fullName : $"Patient/{patient.Id}";
            }

            private static string? RelationshipText(IResource relatedPerson)
            {
                var relationship = relatedPerson.Child("relationship");
                var coding = relationship?.Child("coding");
                return relationship?.ChildString("text") ?? coding?.ChildString("display") ?? coding?.ChildString("code");
            }
        }

.. container:: toggle

    .. container:: header

      FirelyAuthRelationshipsClient.cs

    .. code-block:: csharp

        using System.Net.Http.Json;
        using System.Text.Json;
        using System.Text.Json.Serialization;

        namespace Vonk.Plugin.AuthorizedRepresentativeExample;

        /// <summary>
        /// Pushes the patients a user may act for to Firely Auth.
        /// </summary>
        /// <remarks>
        /// Use the <see cref="HttpClient"/> that the plugin configures: its base address is Firely Auth, and it gets a management API token.
        /// </remarks>
        internal sealed class FirelyAuthRelationshipsClient(HttpClient httpClient)
        {
            /// <summary>
            /// The path of the push endpoint, relative to the base url of Firely Auth.
            /// </summary>
            public const string Path = "api/external/representative-relationships";

            private static readonly JsonSerializerOptions SerializerOptions = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

            /// <summary>
            /// Replaces the patients that Firely Auth holds for the user. An empty list means the user may act for nobody.
            /// </summary>
            /// <exception cref="HttpRequestException">Firely Auth could not be reached or refused the push.</exception>
            public async Task PushAsync(string userId, IReadOnlyList<Representee> representees, CancellationToken cancellationToken)
            {
                var body = new RelationshipsPush(userId,
                    [..representees.Select(r => new RelationshipsPush.Entry(r.PatientId, r.FullName, r.Relationship, r.Representative))]);

                using var response = await httpClient.PostAsJsonAsync(Path, body, SerializerOptions, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var answer = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new HttpRequestException($"Firely Auth answered {(int)response.StatusCode} to the push: {answer}", null, response.StatusCode);
                }
            }

            /// <summary>
            /// The body of the push.
            /// </summary>
            internal sealed record RelationshipsPush(string UserId, IReadOnlyList<RelationshipsPush.Entry> Relationships)
            {
                /// <summary>
                /// One patient the user may act for.
                /// </summary>
                internal sealed record Entry(string PatientId, string FullName, string? Relationship, string Representative);
            }
        }

.. container:: toggle

    .. container:: header

      OperationParameters.cs

    .. code-block:: csharp

        using Hl7.Fhir.ElementModel;
        using Vonk.Core.Common;
        using Vonk.Core.ElementModel;

        namespace Vonk.Plugin.AuthorizedRepresentativeExample;

        /// <summary>
        /// Reads and writes the <c>Parameters</c> of the two operations.
        /// </summary>
        internal static class OperationParameters
        {
            /// <summary>
            /// The output parameter that says whether the user may act for someone.
            /// </summary>
            public const string Result = "result";

            /// <summary>
            /// The output parameter with the Consent that authorizes the pair.
            /// </summary>
            public const string Consent = "consent";

            /// <summary>
            /// Returns the <c>reference</c> of the <c>valueReference</c> of the parameter <paramref name="name"/>, if it is there once.
            /// </summary>
            public static string? GetReference(ISourceNode parameters, string name) =>
                parameters.Children("parameter").Where(p => p.ChildString("name") == name).ToList() is [var parameter]
                    ? parameter.Child("valueReference")?.ChildString("reference")
                    : null;

            /// <summary>
            /// Creates the output <c>Parameters</c> with <see cref="Result"/>, and <see cref="Consent"/> when there is one.
            /// </summary>
            public static IResource Create(bool result, string? consent, string informationModel)
            {
                var parameters = SourceNode.Resource("Parameters", "Parameters",
                    SourceNode.Node("parameter",
                        SourceNode.Valued("name", Result),
                        SourceNode.Valued("valueBoolean", result ? "true" : "false")));

                if (consent is not null)
                {
                    parameters.Add(SourceNode.Node("parameter",
                        SourceNode.Valued("name", Consent),
                        SourceNode.Node("valueReference", SourceNode.Valued("reference", consent))));
                }

                return parameters.ToIResource(informationModel);
            }
        }

.. container:: toggle

    .. container:: header

      Representee.cs

    .. code-block:: csharp

        namespace Vonk.Plugin.AuthorizedRepresentativeExample;

        /// <summary>
        /// A patient that a user may act for.
        /// </summary>
        /// <param name="PatientId">The id of the Patient.</param>
        /// <param name="FullName">The name of the patient, as Firely Auth shows it to the user.</param>
        /// <param name="Relationship">How the user relates to the patient, for example <c>Mother</c>, if the RelatedPerson says so.</param>
        /// <param name="Representative">The RelatedPerson that links the user to the patient, as <c>RelatedPerson/&lt;id&gt;</c>.</param>
        /// <param name="Consent">The Consent that authorizes the RelatedPerson, as <c>Consent/&lt;id&gt;</c>.</param>
        internal sealed record Representee(string PatientId, string FullName, string? Relationship, string Representative, string Consent);

.. container:: toggle

    .. container:: header

      FhirPeriod.cs

    .. code-block:: csharp

        using System.Globalization;
        using Hl7.Fhir.ElementModel;
        using Vonk.Core.ElementModel;

        namespace Vonk.Plugin.AuthorizedRepresentativeExample;

        /// <summary>
        /// Reads a FHIR <c>Period</c>.
        /// </summary>
        internal static class FhirPeriod
        {
            /// <summary>
            /// Returns whether <paramref name="period"/> covers <paramref name="now"/>.
            /// </summary>
            /// <remarks>
            /// A missing period, start or end is open. A partial date covers the whole year, month or day.
            /// A start or end that cannot be read covers nothing.
            /// </remarks>
            public static bool Covers(ISourceNode? period, DateTimeOffset now)
            {
                if (period is null)
                    return true;

                var start = period.ChildString("start");
                var end = period.ChildString("end");

                if (start is not null && (TryParse(start) is not { } startRange || now < startRange.From))
                    return false;

                if (end is not null && (TryParse(end) is not { } endRange || now >= endRange.Until))
                    return false;

                return true;
            }

            /// <summary>
            /// The instants that a FHIR date or dateTime stands for: from <c>From</c>, up to but not including <c>Until</c>.
            /// </summary>
            private static (DateTimeOffset From, DateTimeOffset Until)? TryParse(string value)
            {
                const DateTimeStyles styles = DateTimeStyles.AssumeUniversal;
                var culture = CultureInfo.InvariantCulture;

                switch (value.Length)
                {
                    case 4 when DateTimeOffset.TryParseExact(value, "yyyy", culture, styles, out var year):
                        return (year, year.AddYears(1));
                    case 7 when DateTimeOffset.TryParseExact(value, "yyyy-MM", culture, styles, out var month):
                        return (month, month.AddMonths(1));
                    case 10 when DateTimeOffset.TryParseExact(value, "yyyy-MM-dd", culture, styles, out var day):
                        return (day, day.AddDays(1));
                    default:
                        // An end is inclusive at its precision: a dateTime in seconds covers that whole second
                        return DateTimeOffset.TryParse(value, culture, styles, out var instant)
                            ? (instant, value.Contains('.') ? instant.AddTicks(1) : instant.AddSeconds(1))
                            : null;
                }
            }
        }


The flow
^^^^^^^^

.. code-block:: text

    ┌──────────┐                 ┌─────────────┐                 ┌───────────────┐
    │   User   │                 │ Firely Auth │                 │ Firely Server │
    └────┬─────┘                 └──────┬──────┘                 └───────┬───────┘
         │  1. Sign in to an app        │                                │
         │─────────────────────────────>│                                │
         │                              │  2. POST $representative-      │
         │                              │     relationships (sub)        │
         │                              │───────────────────────────────>│
         │                              │                                │  3. Find RelatedPersons
         │                              │                                │     with the sub identifier
         │                              │  4. POST /api/external/        │
         │                              │     representative-            │
         │                              │     relationships              │
         │                              │<───────────────────────────────│
         │                              │  5. result (true/false)        │
         │                              │<───────────────────────────────│
         │  6. Selection page           │                                │
         │<─────────────────────────────│                                │
         │  7. Choose a patient         │                                │
         │─────────────────────────────>│                                │
         │                              │  8. POST Patient/$check-       │
         │                              │     authorized-representative  │
         │                              │───────────────────────────────>│
         │                              │  9. result (+ consent)         │
         │                              │<───────────────────────────────│
         │ 10. Token for the app        │                                │
         │     (fhirUser = RelatedPerson│                                │
         │      patient = the choice)   │                                │
         │<─────────────────────────────│                                │

The push (4) may also come after the answer (5). At every refresh of the token, Firely Auth repeats steps 8 and 9.

.. _firely_auth_dar_deprecated:

The deprecated Patient-account flow
-----------------------------------

While ``LookupOnLogin`` is ``true`` and ``LookupBySub`` is ``false``, a ``Patient`` signs in through the deprecated flow, whether or not they use an external identity provider. Firely Auth then:

- calls ``POST [base]/Patient/$check-authorized-representative-relationships`` with a ``Parameters`` resource holding ``patient`` (``valueString``): the user's ``fhirUser``, such as ``Patient/123``. Its token has the scopes of ``LookupRequestScopes``, and the user's ``fhirUser`` and ``patient`` claims. The operation answers ``result`` (boolean);
- expects the relationships at ``POST /api/external/authorized-representative``, with a management API token:

  .. code-block:: json

    {
      "PatientFullName": "string",
      "PatientId": "string",
      "Relationships": [
        { "Id": "string", "FullName": "string", "Relationship": "string" }
      ]
    }

  ``PatientId`` is the id of the user's own Patient, and each ``Id`` is the id of a Patient the user may act for;
- shows the page ``/user/authorized-representatives`` only when the operation answered ``true``, with ``HelpMessageTitleOnFail`` and ``HelpMessageOnFail`` when the relationships do not arrive in time;
- issues a token with the user's own ``fhirUser`` and the chosen ``patient``. Firely Auth only accepts the user's own patient or a patient that Firely Server posted for that user. The relationships are kept for five minutes. A refresh keeps the ``patient`` of the token, without asking Firely Server again. The selection is not shown when a launch context is requested.

This flow does not support Representatives. It will be removed in a later release: implement :ref:`firely_auth_dar_contract` instead, and set ``LookupBySub`` back to ``true``.
