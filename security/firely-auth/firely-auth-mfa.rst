.. _firely_auth_mfa:

Using Multi Factor Authentication / Two Factor Authentication
=============================================================

Firely Auth authorizes a client app to access resources on behalf of a user.
If these resources are especially sensitive - as is often the case for Patient Health Information - it is more secure to require the user to use more than just a password to prove its identity.
This is called multi factor authentication. Since a client is restricted to certain scopes (see ``AllowedSmartSubjects`` in :ref:`firely_auth_settings_clients`), it makes sense to require MFA for clients that potentially have access to sensible resources.
Therefore the ``Require2fa`` setting is part of the ``AllowedClients`` settings.

Multi factor authentication in Firely Auth is based on using a time-based one-time password. The user can use one of the many available Authenticator apps available for either Android or iOS to generate such a password. 

If this setting is set to ``true``, the user should first:

- log in to Firely Auth through the UI, so outside of an authorization request
- enable 2 Factor Authentication from the menu
- register Firely Auth with the Authenticator app using a QRCode
- log out

From now on, when the client requests an access token, the user can login as usual and they will be asked to sign in with both a password and a verification code from the authenticator app.

Should the client request an access token when the user has not set up 2FA yet, the authentication will fail with the error ``interaction_required``.

Enabling and disabling 2FA
--------------------------

.. versionchanged:: 4.8.0

- To enable 2FA, the user has to enter their current password. Accounts that sign in through an external identity provider have no password in Firely Auth and cannot enroll.
- Each enrollment issues a new authenticator key. While 2FA is already enabled, enrollment is refused: to register a new authenticator, the user disables 2FA first.
- Disabling 2FA with a recovery code requires the full code.
- Wrong passwords and 2FA codes entered while signed in, when enabling or disabling 2FA or changing the password, count towards account lockout like failed logins do (see ``Lockout`` in :ref:`firely_auth_settings_account`). The failure that locks the account also signs the user out.
- The user gets an email when 2FA is enabled or disabled, and when their password is changed or reset. See ``SecurityNotificationEmailSubject`` in :ref:`firely_auth_settings_email`.

Resetting 2FA for a user
------------------------

.. versionadded:: 4.8.0

A user who lost their authenticator and has no recovery codes left can no longer sign in. An admin can reset 2FA for such a user: in *User Management*, the user panel shows whether 2FA is enabled and offers *deactivate*. This turns 2FA off, replaces the authenticator key and removes the recovery codes. The user's existing login sessions end at their next security stamp check, by default within 30 minutes. The user can then sign in with their password, enroll again, and gets a security notification email.

Verify the identity of the user outside Firely Auth before you reset their 2FA. Admins cannot reset their own 2FA.

Management clients can do the same through the API, see :ref:`firely_auth_mgmt_reset2fa`.
