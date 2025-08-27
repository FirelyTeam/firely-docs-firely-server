.. _feature_accesscontrol:

==================================================
Overview Authentication and Authorization workflow
==================================================

Access control generally consists of multiple interconnected components. This section will provide an overview how each of them can be implemented in Firely Server.

- Identification: Who are you? -- usually a user name, login, or some identifier.
- Authentication: Prove your identification -- usually with a password, a certificate or some other (combination of) secret(s) owned by you.
- Authorization: What are you allowed to read or change based on your identification?
- Access Control Engine: Enforce the authorization in the context of a specific request.

The whole security architecture for Firely Server is split into three parts to separate out different responsibilities and to provide flexibility regarding the choice of technology for each component. 
To enable secure access to information in Firely Server via its REST API, integrate the following components and align them within a unified authorization flow:

#. Authorization Server

   - Issues OAuth2 access tokens to registered applications and backend systems (clients).
   - Ensures only authenticated and authorized clients can request access.

#. User Account Management System

   - Maintains user identities, credentials, and roles.
   - Supplies the authorization server with identity data to issue tokens with appropriate scopes/claims.

#. Firely Server Access Control Plugin

   - Enforces fine-grained access control based on claims in OAuth2 tokens (e.g., user roles and scopes).
   - Validates tokens and authorizes reads/writes before granting access to FHIR resources.

Firely Server is meant to be used in such an `OAuth2`_ environment in which an OAuth2 provider is responsible for providing authorization information. 
Typically, a user first enters a web application, e.g. a patient portal, or a mobile app. That application interactively redirects the user to the OAuth2 provider.
A user gives their consent to delegate certain access rights to the requesting application. The authorization server may or may not handle authentication. This might be done by a separate service or by checking against a user account database in the background.
On successful authorization the application receives an OAuth2 token back. Then, the application can do a REST API request to Firely Server to send or receive resource(s), and provide the OAuth2 token in the HTTP Authentication header, thereby acting on behalf of the user.
Firely Server can then read the OAuth2 token and validate it with the OAuth2 authorization server. This functionality is not FHIR specific.

.. _OAuth2: https://oauth.net/2/