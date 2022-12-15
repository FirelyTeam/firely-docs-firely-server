.. _firely_auth_mfa:

Using Multi Factor Authentication in Firely Auth
================================================

.. attention:: 

    Multi Factor Authentication is currently only supported for the InMemory store.
    Support for the SQL Server store will follow shortly.

Firely Auth authorizes a client app to access resources on behalf of a user.
If these resources are especially sensitive - as is often the case for Patient Health Information - it is more secure to require the user to use more than just a password to prove its identity.
This is called multi factor authentication. Since a client is restricted to certain scopes (see ``AllowedSmartSubjects`` in :ref:`firely_auth_settings_clients`), it makes sense to require MFA for clients that potentially have access to sensible resources.
Therefore the ``RequireMfa`` setting is part of the ``AllowedClients`` settings.

Multi factor authentication in Firely Auth is based on using a time-based one-time password. The user can use one of the many available Authenticator apps available for either Android or iOS to generate such a password. 

If this setting is set to ``true``, the user should first:

- log in to Firely Auth through the UI, so outside of an authorization request
- enable 2 Factor Authentication from the menu
- register Firely Auth with the Authenticator app using a QRCode
- log out

From now on, when the client requests an access token, the user can login as usual and they will be asked to sign in with both a password and a verification code from the authenticator app.

Should the client request an access token when the user has not set up 2FA yet, the authentication will fail with the error ``interaction_required``.