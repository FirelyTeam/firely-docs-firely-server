.. _firely_auth_sso:

Single Sign-On using external providers
=======================================

Firely Auth can use external OpenID Connect enabled systems for authenticating users instead of relying on a dedicated local user database.
If one or more single sign-on (SSO) providers are configured, users are provided with the option in the login UI to use the external login provider instead of an additional username and password.

Multiple configuration parts are necessary to enable SSO in Firely Auth:

#. Configure Firely Auth as a client in the remote SSO system 

    Technically Firely Auth uses an implicit token workflow to access an ID token from the external system. 
    Make sure to generate an OAuth 2 client id and client secret for Firely Auth. Additionally, ensure that the client is allowed to access ID tokens.

#. Configure user accounts in the remote SSO system and in the local Firely Auth instance.. 
    
    Each user account must be associated with a unique claim inside the ID token. 
    After a successful SSO login, authenticated users are matched against password-less local user accounts to see if the accounts are provisioned to use the local Firely Auth instance. 
    Admins must create an empty local user account assoicated with a matching claim. However it is not necessary to assign these accounts a password.
    The SSO account and the local account are matched based on claims issued in an ID token returned by the SSO provided via an implicit token flow. 
    Firely Auth attempts to match the accounts based on an ``email`` or ``oid`` claim.

    Automated provisioning of local user accounts based on a SSO login is not yet supported by Firely Auth.

#. Configure all SSO details in the ``ExternalIdentityProviders`` configuration section

After a successful login accounts details are automatically updated based on the information provided by the SSO system. The fullname of the user and the email address are updated. The latter requires, logically, a match based on an ``oid`` claim.

Using Microsoft Entra ID (formerly Azure Active Directory)
----------------------------------------------------------

Configuring a new client application in Azure Active Directory (Azure AD) using the Implicit Flow involves several steps:

#. Sign in to Azure Portal:
    
    Log in to the `Azure Portal <https://portal.azure.com/>`_ using your Azure AD administrator account.

#. Create an Azure AD Application:

    - Navigate to the Azure Active Directory service.
    - Select "App registrations".
    - Click on "+ New registration".
    - Fill in the basic application information, including the name (e.g. "Firely Auth SSO") and supported account types (e.g., accounts in this organizational directory only, any organizational directory, or any identity provider).
    - Under the "Redirect URI" section, specify the URI where Azure AD will send the authentication response. As the type of the redirect URL select "Web". For Implicit Flow, this should typically be the URI where the Firely Auth instance is hosted combined with "/federation/<scheme name defined in Firely Auth settings>/signin" (e.g., https://auth.example.com/federation/entraId/signin).
    - Complete the registration process and note down the "Application (client) ID" for your newly created application.

#. Enable support for implicit flow for ID tokens

    - Select "Authentication" in the configuration section of the newly created application.
    - Enable "ID tokens" in the "Implicit grant and hybrid flows" section.

#. Define a client secret for Firely Auth

    - Select "Overview".
    - Select "Add a certificate or secret".
    - Complete steps to create a new client secret and note it down safely.

#. Chose the claim in the id token for account matching

    - Select "Token configuration"
    - Select "+ Add optional claim"
    - Select "ID token"
    - Choose a claim based on which the remote SSO account should be matched against a :ref:`password-less local account <firely_auth_mgmt_sso_user>`. Currently, Firely Auth only supports the "email" claim, but additional  claims may be supported in future. Please make sure to add this claim to the locally provisioned user account.

#. Configure the ``ExternalIdentityProviders`` section

    - Select "Overview".
    - Select "Endpoints"
    - Use the base url of any of the displayed OAuth 2.0 endpoints as the authority in the settings. It should start with "https://login.microsoftonline.com/" followed by the tenant id within Microsoft Entra ID, ending in "/v2.0/".

#. If configured successfully the login page of Firely Auth should show a button with a label identical to the chosen display name
