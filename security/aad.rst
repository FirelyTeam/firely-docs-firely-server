.. _feature_accesscontrol_aad:

Azure Active Directory / Microsoft Entra
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

Firely Server provides a plugin to interpret SMART on FHIR scopes by default, however it needs to be enabled and configured. For its configuration see :ref:`feature_accesscontrol_config`.
For the configuration of additional access policies, to restrict access based on the authenticated user see :ref:`feature_accesscontrol_permissions`.