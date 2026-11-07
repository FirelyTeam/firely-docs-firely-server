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

    This is a beta feature and as such it is subject to change in future releases. Disclaimers that a user agrees to while acting for someone else are not shown in the user's overview of agreed disclaimers. Test it thoroughly in a development or staging environment before you use it in production.

.. warning::

    The DAR feature does not support running more than one Firely Auth instance behind a load balancer. The relationships that Firely Server pushes to Firely Auth, and the choice the user then makes, are kept only in the memory of the Firely Auth instance that received them. They are not shared with other instances or stored in the database.

    With more than one instance, these requests can each land on a different instance:

    - Firely Server pushing the relationships to ``/api/external/representative-relationships``,
    - the user's browser loading the selection page and submitting the choice,
    - the client application requesting the access token from the token endpoint.

    If the push lands on another instance than the one serving the user, the selection page does not find the relationships and reports a technical problem. If the token request lands on another instance than the one that received the choice, the app gets no token for the patient that was chosen. Sticky sessions on the load balancer do not prevent this, because Firely Server and the client application call Firely Auth directly rather than through the user's browser.

    If you use DAR, run a single Firely Auth instance.

.. _firely_auth_dar_interactions:

How the parts work together
---------------------------

The diagram shows every interaction between the parts, from onboarding a representative to refreshing their token.

.. image:: /images/fa_dar_interactions.svg
    :align: center
    :alt: Sequence diagram of the interactions between the onboarding app or admin, the user, the client app, Firely Auth, the identity provider and Firely Server

**A. Onboarding**, once for each representative, see :ref:`firely_auth_dar_onboarding`:

1. The onboarding app registers the representative with the representative API. Firely Auth creates the account and returns its user id, as the identifier ``http://server.fire.ly/auth/identifier/sub|<user id>``.
2. The onboarding app creates the ``RelatedPerson`` of the patient in Firely Server, with that identifier, and a ``Consent`` if your organization uses them. Instead, an admin can link an existing ``RelatedPerson`` in the admin UI: Firely Auth then adds the identifier itself.
3. Firely Auth sends the invitation email.

**B. Sign-in**, see :ref:`firely_auth_dar_login`:

4. The client app sends the user to Firely Auth to sign in.
5. The user signs in at the external identity provider.
6. Firely Auth calls ``$representative-relationships`` on Firely Server, with a token that holds the user's ``sub``.
7. The plugin finds the RelatedPersons with the user's ``sub`` identifier that are authorized now.
8. The plugin pushes them to Firely Auth.
9. The operation answers whether there is anyone to act for. The push may also come after this answer.
10. Firely Auth shows the selection page.
11. The user chooses whom to act for.
12. Firely Auth calls ``Patient/$check-authorized-representative`` with the chosen pair.
13. Firely Server answers whether the pair is authorized now. Firely Auth only accepts the choice when the answer is ``result = true``.
14. The client app gets a token with ``fhirUser`` = the ``RelatedPerson`` and ``patient`` = the chosen patient.

**C. Using the token:**

15. The client app calls Firely Server with the token. With patient-level scopes, Firely Server limits the requests to the chosen patient's compartment.

**D. Refresh:**

16. The client app refreshes the token.
17. Firely Auth calls ``Patient/$check-authorized-representative`` again with the pair in the token.
18. With ``result = true``, the client app gets a new access token. Otherwise the refresh fails with ``invalid_grant``.

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

The token response has ``patient`` whatever the scopes. The userinfo endpoint returns the same ``fhirUser``, and so does the id_token of a client with ``AlwaysIncludeUserClaimsInIdToken``. Firely Server restricts requests to the chosen patient's compartment only for patient-level scopes (``patient/...``). For user-level and system-level scopes it does not evaluate a compartment, see :ref:`feature_accesscontrol_compartment`. So give apps that a representative uses patient-level scopes only. The ``patient`` claim holds the id of the Patient, as Firely Server pushed it in ``PatientId``. So ``SmartAuthorizationOptions:PatientFilter`` in Firely Server must match on the id: ``_id=#patient#``.

Every refresh of a token for someone else calls ``$check-authorized-representative`` again with the pair in the token. When Firely Server answers ``false``, or cannot be reached, the refresh fails with ``invalid_grant``. So after a revoked authorization, the app gets no new access token. An access token that was already issued stays valid until it expires, so keep the access token lifetime short. A refresh also fails once the user can no longer act for others, for example when an admin changed their user type. Firely Auth logs the ``consent`` reference that Firely Server returns.

Launch contexts
^^^^^^^^^^^^^^^

- **Standalone launch** (``launch/patient`` and other ``launch/...`` scopes, without ``launch``): the selection page is shown, and the chosen patient is the launch patient. Other launch contexts, such as ``launch/encounter``, are chosen afterwards, within that patient's compartment.
- **EHR launch** (the ``launch`` scope): a Representative gets ``access_denied``, and a Patient continues as themselves, with the patient of the launch context.

Settings
^^^^^^^^

The ``AuthorizedRepresentatives`` section of ``appsettings.instance.json`` holds the settings of this flow and of the deprecated one: which Patients are asked whom they act for (``LookupOnLogin`` and ``LookupBySub``), the scopes and the timeout of the calls to Firely Server, and the titles and messages of the selection page. See :ref:`firely_auth_settings_representatives` for the settings and their defaults.

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
- Otherwise the request is refused with ``409 Conflict``: when the email address belongs to more than one user, to a user of another type, to a ``Patient`` who may not act for others (one who signs in with a password, or any Patient while ``LookupOnLogin`` or ``LookupBySub`` is off), or to the patient themselves. The last is checked against the user's ``fhirUser``, and against Firely Server by searching that Patient for the user's sub identifier. For that search, Firely Auth uses a token that it issues to itself, with the audience ``FhirServer:FHIR_BASE_URL`` and the scope ``system/Patient.rs``. When Firely Server cannot be reached for that check, the request fails with ``500``.

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
- A ``RelatedPerson`` that is linked to another account, or whose patient is the account's own ``fhirUser`` Patient, is refused with ``409 Conflict``. A ``Patient`` who signs in with a password cannot be linked (``400 Bad Request``), for the same reason that Representatives have no password: the identity provider vouches that the person's identity was verified. Nor can any ``Patient`` while ``LookupOnLogin`` or ``LookupBySub`` is off, because such a Patient is never asked whom they act for.
- A ``RelatedPerson`` that carries the identifier of an account this Firely Auth does not have, a deleted one or one of another Firely Auth that uses the same Firely Server, is only linked after the admin confirms that those identifiers are replaced with this account's: **find** shows a warning, and **link** replaces them. The replacement is logged as a ``Warning``.
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

- ``client_id``: ``Firely-Auth-Internal-FhirLookup``. Firely Auth reserves this client id: no client can be registered with it. A client that already had this id keeps working, and Firely Auth logs a ``Warning`` about it at startup. Give such a client another id before you enable the operations.
- audience: ``FhirServer:FHIR_BASE_URL``.
- scopes: ``RepresentativeRelationshipsScopes`` for ``$representative-relationships``, and ``CheckAuthorizedRepresentativeScopes`` for ``$check-authorized-representative``. The default of the latter also holds ``system/Patient.rs``: Firely Server answers an operation for a backend client only when its token carries at least one ``system/`` scope, and it is not a scope the operation itself needs.
- a ``sub`` claim with the Firely Auth id of the user who signs in or refreshes. The token has no ``fhirUser`` or ``patient`` claim.

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
- Each push replaces the list that Firely Auth holds for the user. A push may come at any time, also after the operation has answered, or later when a relationship changes. At each sign-in, Firely Auth starts from an empty list and calls ``$representative-relationships``. So push at every call that answers ``true``, also when the list did not change since the last push.
- The endpoint answers ``400 Bad Request`` for a user that does not exist or may not act for others (not a ``Representative``, nor a ``Patient`` who signs in through an external identity provider while ``LookupOnLogin`` and ``LookupBySub`` are on), for a missing or invalid field, and for a list that names the same patient twice. One user acting for one patient through two RelatedPersons is not supported: push one of them.
- The token is a management API token, see :ref:`firely_auth_mgmt`. Register a client of the type *Management API* in Firely Auth for your plugin.

.. important::

    Firely Auth trusts the push. It does not check that each ``Representative`` carries the user's ``sub`` identifier, or that the ``RelatedPerson`` belongs to ``PatientId``. Your operation must push only RelatedPersons that do. A token search can ignore case, depending on the database, so compare the identifier value with the ``sub`` exactly before you push the RelatedPerson. Firely Auth checks each pair again with ``$check-authorized-representative`` when the user chooses, and at every refresh, before it issues a token.

The endpoint ``/api/external/authorized-representative`` is only for the deprecated Patient-account flow, see :ref:`firely_auth_dar_deprecated`.

$check-authorized-representative
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

- **URL**: ``POST [base]/Patient/$check-authorized-representative``, at type level.
- **Input**: a ``Parameters`` resource with ``patient`` (``valueReference``, ``Patient/<id>``) and ``representative`` (``valueReference``, ``RelatedPerson/<id>``). The user who chose is the ``sub`` claim of the token, also at a refresh.
- **Output**: a ``Parameters`` resource with ``result`` (boolean): whether the RelatedPerson may act for the patient now. Optionally ``consent`` (``valueReference``), which Firely Auth logs.

Check that the RelatedPerson still carries the ``sub`` identifier of that user, and is still authorized for the patient. An admin can unlink a RelatedPerson from one user and link it to another. The check on the identifier makes the first user's next refresh fail.

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

- Accept only tokens with ``client_id`` = ``Firely-Auth-Internal-FhirLookup``, and answer ``403`` otherwise. This is a check on the name: it relies on no other client in Firely Auth having that client id. Firely Auth refuses to register a client with it, but a client that already had it keeps working until you give it another id.
- Both operations take the user only from the ``sub`` claim, and answer ``403`` for a token without one.

The example plugin
^^^^^^^^^^^^^^^^^^

The example plugin ``Vonk.Plugin.AuthorizedRepresentativeExample`` implements both operations. A user may act for a patient through a ``RelatedPerson`` that:

- carries the identifier ``http://server.fire.ly/auth/identifier/sub|<user id>``, with the user id in exactly the same case. Firely Auth adds it when an admin links the RelatedPerson to the user (see :ref:`firely_auth_dar_onboarding`);
- refers to the patient in ``RelatedPerson.patient``;
- is not ``active = false``, and has a ``period`` that covers the current time, if it has one; a date in a period covers that whole day, month or year, and a time without fractions of a second covers that whole second;
- is the ``actor`` of an active ``Consent`` of that patient that is in force now:

  - FHIR R4: the RelatedPerson is an actor of the root ``provision``, whose ``type`` is not ``deny`` and whose ``period`` covers the current time.
  - FHIR R5: ``decision`` is the base rule and each ``provision`` an exception to it. So the Consent has ``decision`` = ``deny``, the RelatedPerson is an actor of one of its ``provision`` elements, and both ``period`` elements cover the current time.

  The example does not evaluate nested provisions: a provision that has them authorizes nobody. A token for the representative gives access to the patient's whole record, so a provision that permits only part of it also authorizes nobody: one with ``action``, ``securityLabel``, ``purpose``, ``class``, ``code``, ``dataPeriod``, ``data``, ``documentType``, ``resourceType`` or ``expression``. It reads Consents of FHIR R4 and R5 only. For another FHIR version, both operations answer ``500``.

To decide the authorization without Consent resources, replace ``DecideAuthorizationAsync`` in ``RepresentativeAuthorizations.cs``, and return a ``RepresentativeAuthorization`` without a Consent. The method gets the RelatedPerson and the id of its patient. The checks on the RelatedPerson in the first three points above stay in place. Then ``$check-authorized-representative`` answers ``result`` = ``true`` without ``consent``. The plugin pushes before it answers. When the push fails, ``$representative-relationships`` answers ``500``. Each search reads at most 50 resources. When there are more, the operation answers ``500`` instead of deciding on part of them.

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

    .. literalinclude:: examples/authorized-representative-plugin/AuthorizedRepresentativeExampleConfiguration.cs
        :language: csharp

.. container:: toggle

    .. container:: header

      AuthorizedRepresentativeExampleOptions.cs

    .. literalinclude:: examples/authorized-representative-plugin/AuthorizedRepresentativeExampleOptions.cs
        :language: csharp

.. container:: toggle

    .. container:: header

      FirelyAuthCaller.cs

    .. literalinclude:: examples/authorized-representative-plugin/FirelyAuthCaller.cs
        :language: csharp

.. container:: toggle

    .. container:: header

      RepresentativeRelationshipsOperation.cs

    .. literalinclude:: examples/authorized-representative-plugin/RepresentativeRelationshipsOperation.cs
        :language: csharp

.. container:: toggle

    .. container:: header

      CheckAuthorizedRepresentativeOperation.cs

    .. literalinclude:: examples/authorized-representative-plugin/CheckAuthorizedRepresentativeOperation.cs
        :language: csharp

.. container:: toggle

    .. container:: header

      RepresentativeAuthorizations.cs

    .. literalinclude:: examples/authorized-representative-plugin/RepresentativeAuthorizations.cs
        :language: csharp

.. container:: toggle

    .. container:: header

      RepresentativeAuthorization.cs

    .. literalinclude:: examples/authorized-representative-plugin/RepresentativeAuthorization.cs
        :language: csharp

.. container:: toggle

    .. container:: header

      RepresentativeLookupException.cs

    .. literalinclude:: examples/authorized-representative-plugin/RepresentativeLookupException.cs
        :language: csharp

.. container:: toggle

    .. container:: header

      FirelyAuthRelationshipsClient.cs

    .. literalinclude:: examples/authorized-representative-plugin/FirelyAuthRelationshipsClient.cs
        :language: csharp

.. container:: toggle

    .. container:: header

      OperationParameters.cs

    .. literalinclude:: examples/authorized-representative-plugin/OperationParameters.cs
        :language: csharp

.. container:: toggle

    .. container:: header

      Representee.cs

    .. literalinclude:: examples/authorized-representative-plugin/Representee.cs
        :language: csharp

.. container:: toggle

    .. container:: header

      FhirPeriod.cs

    .. literalinclude:: examples/authorized-representative-plugin/FhirPeriod.cs
        :language: csharp

The flow
^^^^^^^^

For the order of the calls, see the diagram in :ref:`firely_auth_dar_interactions`.

.. _firely_auth_dar_deprecated:

The deprecated Patient-account flow
-----------------------------------

.. warning::

    This flow is deprecated and will be removed in a later release. Implement :ref:`firely_auth_dar_contract` instead, see :ref:`firely_auth_dar_migration`.

In this flow, every person who acts for someone else has a ``Patient`` account with a ``fhirUser`` of their own. Firely Server finds the patients the user may act for through that ``fhirUser``. The flow does not support Representatives.

Settings
^^^^^^^^

Firely Auth uses this flow while ``LookupOnLogin`` is ``true`` and ``LookupBySub`` is ``false``:

.. code-block:: json

  "AuthorizedRepresentatives": {
    "LookupOnLogin": true,
    "LookupBySub": false,
    "LookupRequestScopes": "https://fire.ly/fhir/OperationDefinition/has-authorized-representative-relationships",
    "CheckAuthorizedRepresentativeTimeout": 5000,
    "TitleOnLogin": "Who Are You Signing In As?",
    "TitleOwnProfileOnLogin": "Continue as Yourself",
    "TitleAuthorizedProfilesOnLogin": "Continue On Behalf Of ...",
    "HelpMessageTitleOnFail": "Couldn't load authorized profiles",
    "HelpMessageOnFail": "A technical error prevented loading the user profiles of all authorized users. Please try again by reloading or continue as yourself."
  }

See :ref:`firely_auth_settings_representatives` for each setting. A ``Patient`` signs in through this flow whether or not they use an external identity provider.

The sign-in
^^^^^^^^^^^

.. image:: /images/fa_dar_deprecated_flow.svg
    :align: center
    :alt: Sequence diagram of the deprecated Patient-account flow between the user, the client app, Firely Auth and Firely Server

1. The client app sends the user to Firely Auth to sign in.
2. The user signs in to their ``Patient`` account, with a password or through an external identity provider.
3. Firely Auth calls ``POST [base]/Patient/$check-authorized-representative-relationships`` with a ``Parameters`` resource holding ``patient`` (``valueString``): the user's ``fhirUser``, such as ``Patient/123``. Its token has the scopes of ``LookupRequestScopes``, and the user's ``fhirUser`` and ``patient`` claims.
4. The plugin posts the relationships to ``POST /api/external/authorized-representative``, see below. It may post before it answers, or after.
5. The operation answers ``result`` (boolean): ``true`` when the user may act for at least one patient.
6. Only when the answer is ``true``, Firely Auth shows the page ``/user/authorized-representatives``. When the relationships do not arrive within ``CheckAuthorizedRepresentativeTimeout``, the page shows ``HelpMessageTitleOnFail`` and ``HelpMessageOnFail``, and the user can continue as themselves.
7. The user chooses to continue as themselves or for one of the patients.
8. The client app gets a token with the user's own ``fhirUser`` and the chosen ``patient``. Firely Auth only accepts the user's own patient or a patient that Firely Server posted for that user.
9. The client app calls Firely Server with the token.
10. The client app refreshes the token.
11. Firely Auth issues a new access token for the same ``patient``, without asking Firely Server again.

The relationships are kept for five minutes. The selection is not shown when a launch context is requested.

The operation and the post
^^^^^^^^^^^^^^^^^^^^^^^^^^

Add the operation to the ``Operations`` setting of Firely Server, with the scope that Firely Auth requests as ``OperationScope``:

.. code-block:: json

  "$check-authorized-representative-relationships": {
    "Name": "$check-authorized-representative-relationships",
    "Level": [ "Type" ],
    "Enabled": true,
    "RequireAuthorization": "WhenAuthEnabled",
    "OperationScope": "https://fire.ly/fhir/OperationDefinition/has-authorized-representative-relationships",
    "RequireTenant": "Never"
  }

The request and the answer of the operation:

.. code-block:: json

  {
    "resourceType": "Parameters",
    "parameter": [ { "name": "patient", "valueString": "Patient/123" } ]
  }

.. code-block:: json

  {
    "resourceType": "Parameters",
    "parameter": [ { "name": "result", "valueBoolean": true } ]
  }

The post to Firely Auth:

.. code-block:: HTTP

  POST <firely-auth-url>/api/external/authorized-representative HTTP/1.1
  Authorization: Bearer <management API token>
  Content-Type: application/json

  {
    "PatientFullName": "Jane de Vries",
    "PatientId": "123",
    "Relationships": [
      { "Id": "456", "FullName": "Anna de Vries", "Relationship": "Daughter" }
    ]
  }

- ``PatientFullName`` and ``PatientId`` are the name and the id of the user's own Patient.
- Each relationship has ``Id`` (the id of a Patient the user may act for), ``FullName`` (the name the page shows) and ``Relationship`` (text the page shows, such as ``Daughter``).
- The token is a management API token, see :ref:`firely_auth_mgmt`. Register a client of the type *Management API*, with the grant type *Client Credentials*, for your plugin in Firely Auth.

To implement the operation, a plugin:

1. reads the ``patient`` parameter, and answers ``400`` when it is missing or is not a reference to a Patient;
2. finds the patients that this Patient may act for, in your own data;
3. posts them to Firely Auth, with a management API token;
4. answers ``result`` = ``true`` when it found at least one patient, and ``false`` otherwise.

Firely Server ships no implementation of this operation, and no example of it is maintained.

.. _firely_auth_dar_migration:

Migrating from the Patient-account flow
---------------------------------------

The differences
^^^^^^^^^^^^^^^

.. list-table::
   :header-rows: 1
   :widths: 24 38 38

   * -
     - Patient-account flow (deprecated)
     - Representative flow
   * - Who can act for others
     - Every ``Patient`` account, with a password or through an external identity provider
     - A ``Representative``, and a ``Patient`` who signs in through an external identity provider
   * - Account of the person who acts
     - A ``Patient`` account with a ``fhirUser``, so a ``Patient`` resource in Firely Server for every representative
     - A ``Representative`` needs no ``Patient`` resource. A Patient keeps their own account
   * - How Firely Server finds the relationships
     - From the user's ``fhirUser``, in a way that the plugin decides
     - From the ``RelatedPerson`` resources with the identifier ``http://server.fire.ly/auth/identifier/sub|<user id>``
   * - Operations that the plugin implements
     - ``Patient/$check-authorized-representative-relationships``
     - ``$representative-relationships`` and ``Patient/$check-authorized-representative``
   * - Token that Firely Auth calls Firely Server with
     - Scopes of ``LookupRequestScopes``, with the user's ``fhirUser`` and ``patient``
     - Client id ``Firely-Auth-Internal-FhirLookup``, scopes of ``RepresentativeRelationshipsScopes`` or ``CheckAuthorizedRepresentativeScopes``, with the user's ``sub``
   * - Where the plugin posts
     - ``/api/external/authorized-representative``: the user's own Patient and the patients they act for
     - ``/api/external/representative-relationships``: the user id and, for each patient, the ``RelatedPerson``
   * - Selection page
     - ``/user/authorized-representatives``, only when the operation answers ``true``
     - ``/user/representees``. A Representative always sees it
   * - ``fhirUser`` in the token when acting for someone
     - The user's own ``Patient``
     - The ``RelatedPerson`` of the chosen patient
   * - Check of the choice
     - Only that Firely Server posted the patient for the user
     - ``$check-authorized-representative`` when the user chooses
   * - Refresh
     - Keeps the patient, without a call to Firely Server
     - Calls ``$check-authorized-representative``, and fails when the authorization is revoked
   * - Launch contexts
     - The selection is not shown
     - Shown in a standalone launch. In an EHR launch, a Representative is refused
   * - Messages on the page
     - ``HelpMessageTitleOnFail`` and ``HelpMessageOnFail``
     - ``RepresentativeTitleOnFail`` and ``RepresentativeMessageOnFail``

The steps
^^^^^^^^^

1. **Implement the two operations** in your Firely Server plugin, see :ref:`firely_auth_dar_contract`. Start from the example plugin. Keep ``$check-authorized-representative-relationships`` until the move is done, so that you can switch back.
2. **Configure Firely Server**: add both operations to the ``Operations`` setting, accept tokens from Firely Auth with the audience ``FhirServer:FHIR_BASE_URL``, and set ``SmartAuthorizationOptions:PatientFilter`` to ``_id=#patient#``.
3. **Register the push client**: the management API client of the deprecated flow can also push to ``/api/external/representative-relationships``. Make sure that no client in Firely Auth has the client id ``Firely-Auth-Internal-FhirLookup``.
4. **Give every representative an account that can act for others**:

   - A person who is a patient and signs in through an external identity provider keeps their ``Patient`` account.
   - A person who signs in with a password cannot act for others in the new flow. Let them sign in through an external identity provider, see :ref:`firely_auth_sso`.
   - A person who is not a patient themselves gets a ``Representative`` account, through the representative API or in the admin UI. Their ``Patient`` resource is then no longer needed for this.

5. **Link each relationship**: give the ``RelatedPerson`` of each patient that a person acts for the identifier ``http://server.fire.ly/auth/identifier/sub|<user id>`` of that person's account. Use **link representee** in the admin UI, or let your onboarding app write the identifier, with the user id that the representative API returns. Create a ``RelatedPerson``, and a ``Consent`` if you use them, where the deprecated flow had none.
6. **Check your apps**: when a user acts for someone else, ``fhirUser`` is a ``RelatedPerson``, not the user's own ``Patient``. Use patient-level scopes, and use refresh tokens instead of ``prompt=none``. See :ref:`firely_auth_dar_login`.
7. **Switch**: set ``LookupBySub`` to ``true``, which is the default. ``LookupBySub`` applies to all Patients at once. To switch back, set it to ``false``.
8. **Clean up**: remove ``$check-authorized-representative-relationships`` from your plugin and from the ``Operations`` setting.
