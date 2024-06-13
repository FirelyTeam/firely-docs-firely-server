.. _firely_auth_mgmt:

Firely Auth User Management
===========================

.. note:: 
  In Firely Auth versions < 4.0.0 there was an executable to manage users, this has been deprecated.

After you have installed, started and created your admin account, you can now manage users in the UI.
After creation of a non-sso account, an email will be sent to the address of the account to activate the account. When this link would be expired, you can resend this email through the UI as well.

When you want to manage the users programmatically, you can use the API's that are specified in the swagger documentation which you can find at: https://localhost:5001/swagger/ (or replace localhost with the url of your Firely Auth installation).
To be able to use the api, you will need a client token (obtained trough the client_credentials flow) for a client who has the ``AllowManagementApiAccess`` property set to true.
It is recommended to use a specific client for this API access.

In case the admin account is not known anymore, there is a way to create another admin account.
For this, you can log in with the user ``FA_ADMIN`` and a password that you can configure in several ways.

- The password can be set with the environment variable ``FIRELY_AUTH_ADMIN_PASSWORD``
- It can be specified in the appsettings:

    .. code-block::

      "ADMIN_PASSWORD": "<admin password>",


- It is also possible to set the admin password in the commandline during startup of Firely Auth:

    .. code-block::

      dotnet Firely.Auth.Core.dll --ADMIN_PASSWORD=<admin password>
