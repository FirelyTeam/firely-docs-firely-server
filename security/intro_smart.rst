.. _feature_accesscontrol_authorization:

=============================
Introduction to SMART on FHIR
=============================

Authorization in FHIR is often based on `SMART on FHIR`_, and more specifically, the `Scopes and Launch Context`_ defined by it.  
SMART defines a syntax for rules, using so-called "scope" claims, to specify the precise access rights that a user wants to delegate to an external application on their behalf.  
The SMART specification has been released in two different versions as of the date of publication: `SMART v1`_ and `SMART v2`_. Both versions are fully supported; see :ref:`Supported Implementation Guides - SMART App Launch <smart_app_launch_ig>`.

When a client application wants to access data in Firely Server on behalf of its user, it requests a token from the authorization server that is bound to the specific instance of Firely Server.  
The configuration of the authorization server determines which claims are *available* for a certain application. The client app configuration determines which claims it *needs*.  
During the token request, the user is usually redirected to the authorization server — which might or might not also be the authentication server — logs in, and is then asked whether the client app is allowed to receive the requested claims.  
The client app cannot request any claims that are not available to that application. For details on how to retrieve an access token as an application, please refer to the `SMART App Launch <http://www.hl7.org/fhir/smart-app-launch/app-launch.html>`_ documentation.

In summary, SMART on FHIR enables:

- **Client configuration** – Defining which scopes an application can request from the authorization server.
- **Authorization requests** – The client requests a set of scopes using OAuth2 workflows.
- **User consent** – The user approves the client's requested scopes.
- **Access enforcement** – Firely Server reads the granted scopes from the access token (the intersection of available, requested, and consented scopes).

SMART on FHIR defines different authorization levels depending on the context in which access is requested.  
These include patient-level access (e.g., for apps launched in the context of a single patient), user-leve access (e.g., for clinician-facing apps), and system-level access (e.g., for backend services without user interaction).

SMART on FHIR relies on standardized OAuth2 "flows", meaning defined sequences of steps through which clients obtain access tokens to support different access scenarios:

#. Authorization Code Flow (for interactive user login)

    This flow is used when an end-user — such as a patient or clinician — interacts with the application. It follows these steps:

   - The app redirects the user to the authorization server.
   - The user logs in and consents to the requested scopes.
   - The server returns an authorization code to the app.
   - The app exchanges this code for an access token.

   .. note::

      This flow is further divided into two distinct launch scenarios:

      - EHR Launch:
      
        The app is launched from within an EHR (Electronic Health Record) system and receives launch context parameters
        (such as the patient ID, ID of the current encounter, etc.) via the so-called ``launch`` parameter.  
        This allows the app to be preloaded with contextual information, making it highly integrated into clinical workflows.

      - Standalone Launch:
      
        The app is launched independently (e.g., from a public portal or desktop), without an initiating EHR.  
        In this case, the app must explicitly request specific scopes to prompt the user to select the initial context during the authorization process.

2. Client Credentials Flow (for backend systems)

   Used for system-level access where no user is involved. The app uses its credentials (e.g., client ID and secret) to directly obtain an access token, enabling secure server-to-server interactions.

Firely Server is not bound to any specific authorization server, as long as the access token includes the minimal required information.  
See :ref:`feature_accesscontrol_compartment` for details.

However, Firely also offers Firely Auth, a streamlined OAuth2 and OpenID Connect provider with native support for SMART on FHIR scopes.  
It supports built-in user account management and can be federated with external identity providers, enabling seamless integration into existing enterprise authentication and authorization infrastructures. See :ref:`firely_auth_index` for all details.
All authorization levels and OAuth2 flows mentioned above are supported by it.

.. _SMART v1: http://hl7.org/fhir/smart-app-launch/1.0.0/scopes-and-launch-context/index.html
.. _SMART v2: http://hl7.org/fhir/smart-app-launch/STU2/scopes-and-launch-context.html
.. _Scopes and Launch Context: http://www.hl7.org/fhir/smart-app-launch/scopes-and-launch-context.html
.. _SMART on FHIR: http://docs.smarthealthit.org/