.. _firely_auth_index:

Firely Auth
===========
.. _feature_accesscontrol_idprovider:

In order to use :ref:`feature_accesscontrol` you need an Identity Provider that can provide OAuth2 JWT Tokens with claims that conform to `SMART on FHIR`_. In a production scenario, you typically already have such a provider. It could be the EHR system, the Active Directory, or a provider set up specifically for let's say a Patient Portal. It is also very well possible that the provider handing the correct claims uses a federated OAuth2 provider to do the authentication.

Firely offers Firely Auth, an external authorization service optimized for SMART on FHIR. Firely Auth is an add-on for Firely Server and licensed separately.
To allow you to test :ref:`feature_accesscontrol`, we provide you with :ref:`instructions <firely_auth_introduction>` to build and run Firely Auth in which you can configure the necessary clients, claims and users yourself to test different scenarios.

.. _SMART on FHIR: http://docs.smarthealthit.org/

.. toctree::
   :maxdepth: 1
   :titlesonly:

   firely-auth-releasenotes
   firely-auth-tutorial
   firely-auth-settings
   firely-auth-mfa
   firely-auth-deploy
   firely-auth-glossary
   firely-auth-management-app
   firely-auth-BOM