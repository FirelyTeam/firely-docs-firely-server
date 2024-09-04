.. _firely_auth_mgmt:

Firely Auth User Management
===========================

.. note:: 
  In Firely Auth versions < 4.0.0 there was an executable to manage users, this has been deprecated and removed.

After you have installed, started and created your admin account, you can now manage users in the UI.
After creation of a local account (not using Single Sign-on), an email will be sent to the address of the account to activate the account. When this link expires, an admin can resend this email through the UI.

When you want to manage the users programmatically, you can use the API's that are specified in the swagger documentation which you can find at: https://localhost:5001/swagger/ (or replace localhost with the url of your Firely Auth installation). The API enables the creation, updating and deletion of users.
To be able to use the admin management API, you will need a client token (obtained trough the client_credentials flow) for a client who has the ``AllowManagementApiAccess`` property set to true. Note that if you do not set this property to true and try to get a token, Firely Auth will throw an error on the scopes: ``No scopes found in request``. Firely Auth will disable the check for the presence of scopes on the admin APIs if ``AllowManagementApiAccess`` is enabled.

It is recommended to use a specific client for this API access, like:

  .. code-block:: json
    
    {
      "ClientId": "ManagementClient",
      "ClientName": "ManagementClient",
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

You can follow further instructions on :ref:`postman_tutorial`, where in bullet 4 you will have to replace the ``AUTH_BASE_URL`` and ``AUTH_CLIENT_SECRET`` to your configuration.

In case the admin account is not known anymore, there is a way to create another admin account.
For this, you can log in with the user ``FA_ADMIN`` and a password that you can configure in several ways.

- The password can be set with the environment variable ``FIRELY_AUTH_ADMIN_PASSWORD``
- It can be specified in the appsettings:

    .. code-block::

      "ADMIN_PASSWORD": "<admin password>",


- It is also possible to set the admin password in the commandline during startup of Firely Auth:

    .. code-block::

      dotnet Firely.Auth.Core.dll --ADMIN_PASSWORD=<admin password>
