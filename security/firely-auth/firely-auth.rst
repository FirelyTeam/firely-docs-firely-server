.. _firely_auth_index:

Firely Auth
===========
.. _feature_accesscontrol_idprovider:

In order to use :ref:`feature_accesscontrol` you need an Identity Provider that can provide OAuth2 tokens with claims that conform to `SMART on FHIR`_. In a production scenario, you typically already have such an identity provider. It could be the EHR system, the Active Directory, or a provider set up specifically for let's say a Patient Portal. It is also very well possible that the provider handing the correct claims uses a federated OAuth2 provider to do the authentication.

Creating SMART on FHIR conformant tokens and handling all protocol details related to a SMART app launch requires dedicated support which generic authorization servers do not offer. Firely provides Firely Auth, an external authorization service optimized for SMART on FHIR.

.. important::
   Firely Auth is licensed separately from the core Firely Server distribution. Please :ref:`contact<vonk-contact>` Firely to get the license. 
   Your license already permits the usage of Firely Auth if it contains ``http://fire.ly/server/auth``. You can also `try out Firely Auth <https://fire.ly/firely-auth-trial/>`_ using an evaluation license with a limited uptime.

To allow you to test :ref:`feature_accesscontrol`, we provide you with :ref:`instructions <firely_auth_introduction>` to build and run Firely Auth in which you can configure the necessary clients, claims and users yourself to test different scenarios.

.. _SMART on FHIR: http://docs.smarthealthit.org/

.. toctree::
   :maxdepth: 1
   :titlesonly:

   firely-auth-releasenotes
   firely-auth-tutorial
   firely-auth-endpoints
   firely-auth-settings
   firely-auth-mfa
   firely-auth-deploy
   firely-auth-glossary
   firely-auth-management-app
   firely-auth-BOM
