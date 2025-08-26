.. _firely_auth_sso:

Single Sign-On using external providers
=======================================

Firely Auth can use external OpenID Connect enabled systems for authenticating users instead of relying on a dedicated local user database.
If one or more single sign-on (SSO) providers are configured, users are provided with the option in the login UI to use the external login provider instead of an additional username and password. Note: if only a single SSO provider is configured, Firely Auth will not show a dedicated login screen but redirect to the external login automatically.

Multiple configuration parts are necessary to enable SSO in Firely Auth:

#. Configure Firely Auth as a client in the remote SSO system 

    Technically Firely Auth uses an implicit token workflow to access an ID token from the external system. 
    Make sure to generate an OAuth 2 client id and client secret for Firely Auth. Additionally, ensure that the client is allowed to access ID tokens.
    Firely Auth must be allowed to allowed request an ``openid``, ``profile`` and ``email`` scope.

#. Configure user accounts in the remote SSO provider
    
    Each user account must be associated with a unique claim inside the ID token.
    For each SSO user Firely Auth keeps a local password-less account which acts as a statement that at some point in time the SSO has logged into Firely Auth.
    This password-less account is auto-provisioned if the ``AllowAutoProvision`` setting is enabled for the SSO provider. Otherwise an admin needs to create the SSO account via the UI or REST API of Firely Auth.

#. Configure all SSO details in the ``ExternalIdentityProviders`` configuration section

    After a successful login accounts details are automatically updated based on the information provided by the SSO system. 
    The local password-less account is matched on all logins against the SSO account. This matching is based on either an ``email`` claim or ``oid`` claim from the ID token. This way Firely Auth always contains an up-to-date user profile.
    The fullname of the user and the email address are updated based on the SSO account information after login (based on a ``name`` and ``email`` claim). The latter requires, logically, a match based on an ``oid`` claim before.

#. Configure claims to be copied from the ID token

    Using the ``UserClaimsFromIdToken`` it is possible to store additional claims in the local Firely Auth user account in order to expose these claims to clients registered with Firely Auth.
    These claims will be copied from the ID token after a successful login and stored permanently. Each claim is updated automatically after each login with local changes being overwritten.
    It is possible to assign a new name to a claim using the ``CopyAs`` setting.

#. Configure the ``FhirUserLookupClaimsMapping`` if the FhirUser claim cannot be derived from the ID token

    The FhirUser claim is a mandatory claim in Firely Auth. If the FhirUser claim cannot be derived from the ID token, the ``FhirUserLookupClaimsMapping`` setting can be used to use claims from the ID token to look up a user, either a Practitioner or a Patient, in the Firely Server database. The FhirUser claim will then be derived from the resource Type and the ID from the resource. For example:

    .. code-block:: json

        		"FhirUserLookupClaimsMapping": [
					{
						"SearchParameterName": "identifier",
						"SearchParameterValueTemplate": "https://myidentifiersystem|{0}",
						"CopySearchParameterValuesFromClaims": ["family_name", "given_name"]
					}
					,
					{
						"SearchParameterName": "email",
						"CopySearchParameterValuesFromClaims": ["email"]
					}
				]
    
    Here, the FhirUser claim will be derived from the Patient or Practitioner resource with the identifier system "https://myidentifiersystem" and the value of the family_name claim from the ID token, and email with the value of the email claim from the ID token. Also see the :ref:`firely_auth_settings_externalidp` section.

    .. Note:: 
        Note that if Firely Auth queries resources in Firely Server, it will do so via the default FHIR information model of Firely Server. Only R3 or R4 resources are supported by Firely Auth and can be used to derive the fhirUser claim this way. If the resource is not found, Firely Auth will not be able to derive the FhirUser claim. In this case, the user will not be able to log in. Note that the resource in Firely Server will not be found by Firely Auth when virtual tenants are used in Firely Server to save the resources. Note also that Firely Auth will assert the version that is being used by Firely Server using the $versions operation. Before Firely Server 5.11.0, the $versions operation could not be used in combination with X-Forwarded-Prefix headers, so the FhirUser claim lookup will not work in a scenario that uses a reverse proxy. If you are using Firely Server 5.11.0 or later, the $versions operation can be used with X-Forwarded-Prefix headers, and the FhirUser claim lookup will work as expected.
        
        For setting up this connection with Firely Server in a secure fashion, Firely Auth will automatically request internally a JWT access token. `Firely-Auth-Internal-FhirLookup` will be used as the client id within the token. There is no need to configure this client_id via the admin interfaces of Firely Auth.

#. Configure security groups

    Based on the ``AutoProvisionFromSecurityGroup`` setting it is possible to restrict the sign-up of users based on security groups defined in the SSO provider. The attribution of a user account to a one or more security group needs to be exposed via the ``groups`` claim.
    If the ID token received from the SSO provider contains such a claim and the value is part of the whitelisted security groups in the appsetttings, the auto-provisioning is allowed by Firely Auth. Note that Azure allows you to set different values for this claim, such as the Object ID or the display name of the security group. Depending on how this claim is configured in Azure, the respective value, either Object ID or display name of the Security Group, should be added to the ``AutoProvisionFromSecurityGroup`` list to allow auto-provisioning for this group.

.. admonition:: Docker deployments

    If :ref:`testing with Docker <firely_auth_deploy_docker>`, Firely Auth should be configured to use the localhost network, instead of the default bridge network. This is necessary to allow the SSO provider to redirect back to the Firely Auth instance running in a Docker container.

A note on the fhirUser claim
----------------------------

In Firely Auth, each user profile must contain a fhirUser claim - regardless if the profile represents a Patient or Practitioner account. See `SMART App Launch - Scopes for requesting identity data <https://hl7.org/fhir/smart-app-launch/scopes-and-launch-context.html#scopes-for-requesting-identity-data>`_ for background.
This claim may be copied from the ID token of the SSO provider (see ``UserClaimsFromIdToken`` setting above), be set via the UI or account management REST API by an admin manually (see below), or by looking up the patient or practitioner id in the FHIR server (see the ``FhirUserLookupClaimsMapping`` setting above).
A login with an account not containing the claim will be blocked.

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

#. Choose the claim in the id token for account matching

    - Select "Token configuration"
    - Select "+ Add optional claim"
    - Select "ID token"
    - Choose a claim based on which the remote SSO account should be matched against a local account. Currently, Firely Auth supports the ``email`` and ``oid`` claim. Please make sure to add at least one of these claims to the locally provisioned user account.
    - The ``oid`` claim is populated based on the Object ID that can be found on each user profile in Entra ID. Matching on ``oid`` claim is preferred over the ``email`` claim as it is guaranteed not to change.

#. Configure the ``API permissions`` section

    - Select "API permissions"
    - Make sure to at least add "email", "profile", and "openid" as permissions

#. Configure the ``ExternalIdentityProviders`` section

    - Select "Overview".
    - Select "Endpoints"
    - One of the displayed OAuth 2.0 endpoints can be used as the authority in the settings. It should look like this: ``https://login.microsoftonline.com/<Directory (tenant) ID of the registered application>/v2.0``. Please check that the URL uses this exact structure, without any extra subpaths. 

#. Optional: Expose the `groups <https://learn.microsoft.com/en-us/entra/identity-platform/optional-claims?tabs=appui#configure-groups-optional-claims>`_ in the ID token if the SSO auto-provisioning is restricted to certain security groups. As mentioned above, you can configure Azure to add different values to this claim, such as Group ID (the Object ID of the Security Group) or the name of the Security Group. The values listed in the ``AutoProvisionFromSecurityGroup`` setting should match the values of the ``groups`` claim in the ID token.

#. Optional: Add a `Directory extension <https://learn.microsoft.com/en-us/graph/extensibility-overview?tabs=http#directory-microsoft-entra-id-extensions>`_ for the fhirUser claim owned by the Firely Auth application registered above. You can try it out with Microsoft Graph Explorer.
   
    - Navigate to `Microsoft Graph Explorer <https://developer.microsoft.com/en-us/graph/graph-explorer>`_ and log in.
    - Make a POST request to ``https://graph.microsoft.com/v1.0/applications/<object id of your registered app>/extensionProperties`` with the following body:
        
        ::

            { "name": "fhirUser", "dataType": "String", "targetObjects": [ "User" ] }
      
    - The response will look like this:
       
        ::
            
            { "@odata.context": "https://graph.microsoft.com/v1.0/$metadata#applications('<object id of your registered app>')/extensionProperties/$entity", "id": "<id>", "deletedDateTime": null, "appDisplayName": "<name of your registered app>", "dataType": "String", "isMultiValued": false, "isSyncedFromOnPremises": false, "name": "extension_<extension id>_fhirUser", "targetObjects": [ "User" ] }

    - The next step requires admin rights in your Azure environment. Copy the value of the ``name`` element of the response above, you need it to link the extension to an existing user along with a value for the FhirUser claim by a PATCH request to ``https://graph.microsoft.com/v1.0/users/<user object id>`` with the following body:
        
        ::
            
            { "<value of the name element>": "<value of the fhirUser claim>" }

    - You can check if the extension is succesfully linked to the user by making a GET request to ``https://graph.microsoft.com/beta/users/<user object id>?$select=<value of the name element mentioned above>``
        
    The EntraID admin needs to assure that a fhirUser claim is assigned to all accounts that are allowed to be used together with Firely Auth.
    After creating the directory extension please ensure that the extension is exposed as a claim in the ID token. It needs to be enabled via the "Add optional claim" setting above. Select "ID" as the token type, as well as "extn.fhirUser" as the claim.
    Note that EntraID creates the claim for a directory extension with an "extn" prefix. Therefore, use the ``CopyAs`` setting in Firely Auth to copy the claim as "fhirUser" instead of "extn.fhirUser":
        
        .. code-block:: json
            
		"ExternalIdentityProviders": {
		    "IdentityProvider": [{
                        "UserClaimsFromIdToken": [{
			    "Key": "extn.fhirUser",
			    "CopyAs": "fhirUser"
			    }]
			}]
		}

#. If configured successfully the login page of Firely Auth should show a button with a label identical to the chosen display name

Example configuration for Microsoft Entra
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
.. code-block:: json

    "ExternalIdentityProviders": {
        "IdentityProvider": [
            {
                "DisplayName": "My Health Clinic",
                
                // The scheme must be included in the Redirect URI, e.g. https://[Firely Auth base endpoint]/federation/MyHealthClinic/signin, assigned to the application under the 'Authorization' tab in Azure.
                "Scheme": "MyHealthClinic",
                
                // The authority should include the 'Directory (tenant) ID' of the registered application.
                // In Microsoft Entra, this is the base URL of the endpoint listed in 'Overview' -> 'Endpoints' -> 'OpenID Connect metadata document' base URL.
                "Authority": "https://login.microsoftonline.com/egqb1140-e3e3-9719-866d-9c6eabbzzqqd/v2.0/",
                
                // In Microsoft Entra, this should be the 'Application (client) ID'
                "ClientId": "2gf34c86-88bc-4645-91f8-3316be75757f",
                
                // In Microsoft Entra, this is found under 'Certificates & Secrets' -> 'New client secret'
                "ClientSecret": "My client secret",
                
                "AllowAutoProvision": true,
                "UserClaimsFromIdToken": [
                    {
                        "Key": "extn.fhirUser", // claim formatted by Microsoft Graph + Entra
                        "CopyAs": "fhirUser" // claim required by Firely Auth
                    },
                    {
                        "Key": "extn.memberid",
                        "CopyAs": "memberid"
                    }
                ],
                "FhirUserLookupClaimsMapping": [
                    {
                        "SearchParameterName": "identifier",
                        "CopySearchParameterValuesFromClaims": [ "clinicid", "groupid", "memberid" ],
                        "SearchParameterValueTemplate": "http://myhealthclinic/customer/fhir/{0}/{1}/member|{2}",
                    }
                ]
            }
        ]
    }