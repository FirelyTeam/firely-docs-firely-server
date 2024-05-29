.. _feature_accesscontrol:

===========================
Introduction Access control
===========================

Access control generally consists of multiple interconnected components. This section will provide an overview how each of them can be implemented in Firely Server.

- Identification: Who are you? -- usually a user name, login, or some identifier.
- Authentication: Prove your identification -- usually with a password, a certificate or some other (combination of) secret(s) owned by you.
- Authorization: What are you allowed to read or change based on your identification?
- Access Control Engine: Enforce the authorization in the context of a specific request.

Overview Authentication and Authorization workflow in Firely Server
===================================================================
The whole security architecture for Firely Server is split into three parts to separate out different responsibilities and to provide flexibility regarding the choice of technology for each component. 
For secure access to information in Firely Server through its REST API an authorization server, a system for managing user accounts, and an access control plugin in Firely Server is needed.

Firely Server is meant to be used in an `OAuth2`_ environment in which an `OAuth2 provider`_ is responsible for providing authorization information. 
Typically, a user first enters a web application, e.g. a patient portal, or a mobile app. That application interactively redirects the user to the OAuth2 provider.
A user gives their consent to delegate certain access rights to the requesting application. The authorization server may or may not handle authentication. This might be done by a separate service or by checking against a user account database in the background.
On successful authorization the application receives an OAuth2 token back. Then, the application can do a REST API request to Firely Server to send or receive resource(s), and provide the OAuth2 token in the HTTP Authentication header, thereby acting on behalf of the user.
Firely Server can then read the OAuth2 token and validate it with the OAuth2 authorization server. This functionality is not FHIR specific. Firely Server can apply more restrictive access control based on the authenticated user using access policies, further limiting the resources that can be requested by the user.

.. _feature_accesscontrol_authorization:

Access Control Engine
=====================

Authorization in Firely Server by default is based on `SMART on FHIR`_ and more specifically the `Scopes and Launch Context`_ defined by it. 
The SMART specification is released in two different version as of the date of publication: `SMART v1`_ and `SMART v2`_. Both versions are fully supported, see :ref:`Supported Implementation Guides - SMART App Launch <smart_app_launch_ig>`.
SMART defines a syntax for rules, using so called "scope"-claims, to specify the precise access rights that a user wants to delegate to an external application on their behalf.

These are examples of scopes that are recognized by Firely Server (SMART v1):

* scope=user/Observation.read: the user is allowed to read Observation resources
* scope=user/Encounter.write: the user is allowed to write Encounter resources
* scope=user/*.read: the user is allowed to read any type of resource
* scope=user/*.write: the user is allowed to write any type of resource
* scope=[array of individual scopes]

All scopes using SMART v1 can also be expressed in SMART v2:

* scope=user/Observation.r: the user allows to read Observation resources
* scope=user/Encounter.cu: the user allows to write (create and update) Encounter resources
* scope=user/\*.r: the user allows to read any type of resource
* scope=user/\*.cu: the user allows to write (create and update) any type of resource
* scope=[array of individual scopes]

When a client application wants to access data in Firely Server on behalf of its user, it requests a token from the authorization server that is bound to the specific instance of Firely Server. 
The configuration of the authorization server determines which claims are *available* for a certain application. The client app configuration determines which claims it *needs*.
During the token request, the user is usually redirected to the authorization server, which might or might not be the authentication server as well, logs in and is then asked whether the client app is allowed to receive the requested claims.
The client app cannot request any claims that are not available to that application. For details on how to retrieve an access token as an application, please refer to `SMART App Launch <http://www.hl7.org/fhir/smart-app-launch/app-launch.html>`_.

In summary SMART on FHIR is used to:

- configure a client: which scopes can an application request from an authorization server?
- request authorization: client requests a set of scopes using OAuth2 workflows
- consent: user consents to the client using the requested scopes
- access token: Firely Server can read from the access token which scopes the client is granted (an intersection of the three above)

Firely Server provides a plugin to interpret SMART on FHIR scopes by default, however it needs to be enabled and configured. For its configuration see :ref:`feature_accesscontrol_config`.
For the configuration of additional access policies, to restrict access based on the authenticated user see :ref:`feature_accesscontrol_permissions`.

Other forms of Authorization
============================

In :ref:`accesscontrol_api` you can find the interfaces relevant to authorization in Firely Server.  
If your environment requires other authorization information than the standard SMART on FHIR claims, you can create your own implementations of these interfaces.
You do this by implementing a :ref:`custom plugin <vonk_plugins>`. 
All the standard plugins of Firely Server can then use that implementation to enforce access control.

.. _feature_accesscontrol_auth_server:

Authorization Server and User Management
========================================

Firely Server is not bound to any specific authorization server, as long as the access token contains a set of minimal information. See :ref:`feature_accesscontrol_compartment`.

.. _feature_accesscontrol_firely_auth:

Firely Auth
-----------

Firely provides an optimized OAuth2 provider that understands SMART on FHIR scopes and the FHIR resource types they apply to out of the box. Additionally it can be used for user account management or integrated using OAuth2 federation into existing infrastructures. This product is called Firely Auth and can be acquired as part of Firely Server. You can also evaluate it using a Firely Server evaluation license. See :ref:`firely_auth_index` for all details.

.. _feature_accesscontrol_aad:

Azure Active Directory
----------------------

Azure Active Directory can be used independently as an authorization server, however some caveats exist regarding the usage of SMART on FHIR here.

.. note::
  Firely only provides support for deployment and configuration of Firely Auth. The usage of any other authorization server falls outside of the scope of support and may be subject of consultancy instead. 

Azure Active Directory (v2.0) does not allow to define a scope with ``/`` (forward slash) in it, which is not compatible with the structure of a `SMART on FHIR scope <http://www.hl7.org/fhir/smart-app-launch/scopes-and-launch-context.html>`_. 
Therefore when you use AAD to provide SMART on FHIR scopes to Firely Server, you need to take the following steps

1. In a SMART scope, use another character (for instance ``-``) instead of ``/``. For example:

  * ``user/*.read`` becomes ``user-*.read``
  * ``user/*.write`` becomes ``user-*.write``
  * ``patient/Observation.r`` becomes ``patient-Observation.r``
  
  If the used character (for instance ``-``) is already in your SMART scope, then you can use ``\`` (backward slash) to escape it.
  
  * ``patient/Observation.r?_id=Id-With-Dashes`` becomes ``patient-Observation.r?_id=Id\-With\-Dashes``

  If a ``\`` (backward slash) is already in your SMART scope, then you can escape it with another ``\``.

  * ``patient/Observation.r?_id=Id\With\BackwardSlash`` becomes ``patient-Observation.r?_id=Id\\With\\BackwardSlash`` 

2. Configure Firely Server which character is used in Step 1, then Firely Server will generate a proper `SMART on FHIR scope <http://www.hl7.org/fhir/smart-app-launch/scopes-and-launch-context.html>`_ and handle the request further. This can be configured via setting ``AccessTokenScopeReplace``. 

For the first step above, instead of requesting different scopes in the user application, you can deploy `SMART on FHIR AAD Proxy <https://github.com/azure-smart-health/smart-on-fhir-aad-proxy>`_ to Azure, which helps you to replace ``/`` to ``-`` in a SMART scope when you request your access token.
The other option would be to follow `Quickstart: Deploy Azure API for FHIR using Azure portal <https://docs.microsoft.com/en-us/azure/healthcare-apis/azure-api-for-fhir/fhir-paas-portal-quickstart>`__, check "SMART on FHIR proxy" box in the "Additional settings" and use the proxy by following `Quickstart: Deploy Azure API for FHIR using Azure portal <https://learn.microsoft.com/en-us/azure/healthcare-apis/azure-api-for-fhir/fhir-paas-portal-quickstart>`__.

.. warning:: 
  When you use the SMART on FHIR AAD Proxy, be careful with `SMART on FHIR v2 scopes <http://hl7.org/fhir/smart-app-launch/STU2/scopes-and-launch-context.html>`_.  ``-`` is an allowed character within the access scope (see examples below). 
  In those cases, the proxy simply replaces ``/`` with ``-`` and does not escape the original ``-``, then Firely Server cannot figure out which ``-`` is original, which will result in a failed request.

  * ``patient/Observation.rs?category=http://terminology.hl7.org/CodeSystem/observation-category|laboratory``
  * ``Observation.rs?code:in=http://valueset.example.org/ValueSet/diabetes-codes`` 

.. _OAuth2: https://oauth.net/2/
.. _OAuth2 provider: https://en.wikipedia.org/wiki/List_of_OAuth_providers
.. _SMART on FHIR: http://docs.smarthealthit.org/
.. _SMART App Authorization Guide: http://docs.smarthealthit.org/authorization/
.. _Scopes and Launch Context: http://www.hl7.org/fhir/smart-app-launch/scopes-and-launch-context.html
.. _Patient CompartmentDefinition: http://www.hl7.org/implement/standards/fhir/compartmentdefinition-patient.html
.. _ASP.NET Core Identity: https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity
.. _SMART v1: http://hl7.org/fhir/smart-app-launch/1.0.0/scopes-and-launch-context/index.html
.. _SMART v2: http://hl7.org/fhir/smart-app-launch/STU2/scopes-and-launch-context.html
