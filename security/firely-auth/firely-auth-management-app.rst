.. _firely_auth_mgmt:

Firely Auth User Management
===========================

.. note:: 
  In Firely Auth versions < 4.0.0 there was an executable to manage users, this has been deprecated and removed.

After you have installed, started and created your admin account, you can now manage users in the UI.
After creation of a local account (not using Single Sign-on), an email will be sent to the address of the account to activate the account. When this link expires, an admin can resend this email through the UI.

When you want to manage the users programmatically, you can use the API's that are specified in the swagger documentation which you can find at: https://localhost:5001/swagger/ (or replace localhost with the url of your Firely Auth installation). The API enables the creation, updating and deletion of users. User claims of the reserved claim types ``role``, ``sub``, ``scope``, ``client_id``, ``amr``, ``idp``, ``aud``, ``iss``, ``auth_time`` and ``fineGrainedScope`` cannot be set and are refused with ``400 Bad Request``.
A ``POST``, ``PUT``, ``PATCH`` or ``DELETE`` by an admin in the admin UI needs a recent login, see ``AdminReauthenticationWindow`` in :ref:`firely_auth_settings_account`. Management API clients are not affected.
To be able to use the admin management API, you will need a client token (obtained trough the client_credentials flow) for a client who has the ``AllowManagementApiAccess`` property set to true. You will have to request the scope ``http://server.fire.ly/auth/scope/authmanagement`` to get access to the management api's. Note that if you do not set this property to true and try to get a token, Firely Auth will throw an error on the scopes: ``No scopes found in request``. Firely Auth will disable the check for the presence of scopes on the admin APIs if ``AllowManagementApiAccess`` is enabled.

It is recommended to use a specific client for this API access, like:

  .. code-block:: json
    
    {
      "ClientId": "ManagementClient",
      "ClientName": "ManagementClient",
      "ClientType": "ManagementApi",
      "Description": "Client for using management APIs",
      "Enabled": true,
      "ClientSecrets": [
        {
          "SecretType": "SharedSecret",
          "Secret": "[your secrect]"
        }
      ],
      "AllowedSmartActions": [ "r" ],
      "AllowedSmartSubjects": [ "system" ],
      "AllowedGrantTypes": [ "client_credentials" ],
      "RequireClientSecret": true,
      "AllowManagementApiAccess": true
    },

Requesting a management API token
---------------------------------

A management API token is issued for Firely Auth itself, not for Firely Server. The management API only accepts a token whose only ``aud`` is ``http://server.fire.ly/auth/api/management``.

Name the management API with the ``resource`` parameter (RFC 8707) when you request the token:

.. code-block:: HTTP

  POST <base-url>/connect/token HTTP/1.1
  Content-Type: application/x-www-form-urlencoded

  grant_type=client_credentials&client_id=ManagementClient&client_secret=<your secret>&scope=http%3A%2F%2Fserver.fire.ly%2Fauth%2Fscope%2Fauthmanagement&resource=http%3A%2F%2Fserver.fire.ly%2Fauth%2Fapi%2Fmanagement

The ``resource`` parameter is optional: a request without it gets the same token. ``http://server.fire.ly/auth/api/management`` is the only supported ``resource`` for a management token; other values may be refused.

Management clients do not send the ``aud`` parameter. Firely Auth ignores it for them and logs a warning when a client sends it. Firely Server does not accept a management API token either: when it introspects one, Firely Auth reports the token as inactive.

A Postman collection with demo requests of the administration API can be found here:

    .. raw:: html

      <div class="postman-run-button"
      data-postman-action="collection/fork"
      data-postman-visibility="public"
      data-postman-var-1="6644549-e9cbac4a-154a-4b41-95f1-c97d5c42f5f3"
      data-postman-collection-url="entityId=6644549-e9cbac4a-154a-4b41-95f1-c97d5c42f5f3&entityType=collection&workspaceId=822b68d8-7e7d-4b09-b8f1-68362070f0bd"></div>
      <script type="text/javascript">
        (function (p,o,s,t,m,a,n) {
          !p[s] && (p[s] = function () { (p[t] || (p[t] = [])).push(arguments); });
          !o.getElementById(s+t) && o.getElementsByTagName("head")[0].appendChild((
            (n = o.createElement("script")),
            (n.id = s+t), (n.async = 1), (n.src = m), n
          ));
        }(window, document, "_pm", "PostmanRunObject", "https://run.pstmn.io/button.js"));
      </script>

In case the admin account is not known anymore, there is a way to create another admin account.
For this, you can log in with the user ``FA_ADMIN`` and a password that you can configure in several ways.

- The password can be set with the environment variable ``FIRELY_AUTH_ADMIN_PASSWORD``
- It can be specified in the appsettings:

    .. code-block::

      "ADMIN_PASSWORD": "<admin password>",


- It is also possible to set the admin password in the commandline during startup of Firely Auth:

    .. code-block::

      dotnet Firely.Auth.Core.dll --ADMIN_PASSWORD=<admin password>


.. _firely_auth_mgmt_reset2fa:

Resetting two-factor authentication
-----------------------------------

When a user lost their authenticator and recovery codes, an admin can reset their two-factor authentication in the UI (see :ref:`firely_auth_mfa`), and a management client through the API:

.. code-block:: HTTP

  POST <base-url>/api/external/user/<user-id>/reset-2fa HTTP/1.1
  Authorization: Bearer <management API token>

This turns 2FA off for the user, replaces their authenticator key and removes their recovery codes. The response is ``200 OK``, ``400 Bad Request`` when the user does not have 2FA enabled, or ``404 Not Found`` for an unknown user. The user endpoints also return ``twoFactorEnabled`` for each user. Each reset logs one ``Information`` line with the ids of the user and of the admin or client that reset it.

.. _firely_auth_mgmt_fhirrecord:

One account per FHIR record
---------------------------

Only one account can be linked to a FHIR record. Two ``fhirUser`` values point to the same record when their resource type and id are the same; the base URL is not compared, so ``Patient/123`` and ``https://fhir.example.org/Patient/123`` are the same record. There is no setting to turn this off.

- **Admin UI and management API.** Creating or updating a user (``POST`` and ``PUT`` on ``/api/admin/user`` and ``/api/external/user``) with a ``fhirUser`` that another account is linked to is refused with ``409 Conflict``. The response names the other account, and the admin UI shows it in the user form. A refused request changes nothing.
- **Single sign-on.** A login whose ``fhirUser``, mapped from the ID token or found through ``FhirUserLookupClaimsMapping``, belongs to another account is refused with the page that asks the user to contact their administrator. See :ref:`firely_auth_sso`.
- **Valid values only.** A ``fhirUser`` must be a reference to a ``Patient`` or ``Practitioner`` resource with a valid FHIR id, such as ``Patient/123``, optionally with a base URL that may have a path, such as ``https://ehr.example.org/fhir/Patient/123``. Other values, such as ``Patient/abc_1`` (a FHIR id has no ``_``), an id longer than 64 characters or a value with whitespace, are refused with ``400 Bad Request`` for every user type, and an SSO login that maps or looks up such a value is refused.

Accounts from before the upgrade
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Accounts that were linked to the same FHIR record before the upgrade keep working: they can sign in and can be edited, as long as their ``fhirUser`` is not changed to another record that is taken. The same holds for a local account with a ``fhirUser`` that is not valid, but such an account is linked to no FHIR record: it gets no patient launch context and no authorized representative selection. An SSO account whose identity provider sends a value that is not valid is refused at login.

Firely Auth reports these accounts so that an admin can resolve them:

- At startup, Firely Auth logs one ``Warning`` per FHIR record that two or more accounts are linked to, and one per account with a ``fhirUser`` that is not valid. These lines name only the account ids, not the ``fhirUser`` values or email addresses.
- The user list in the admin UI shows a warning with the number of these accounts, and gives each of them the tag *Needs attention*, with the reason in its tooltip. The *Needs attention* filter lists all of them, and the panel of such a user shows the other accounts linked to the same FHIR record.
- The user list of the management API (``GET /api/external/user``) returns ``sharesFhirRecord`` and ``hasInvalidFhirUser`` for each account, and accepts the filter ``NeedsAttention``. A single user returns ``hasInvalidFhirUser`` and ``usersLinkedToSameFhirRecord``.

To resolve an account, delete the account that is not used, or change its ``fhirUser``.
