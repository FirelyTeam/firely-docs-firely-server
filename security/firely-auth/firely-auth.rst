.. _firely_auth_index:

Firely Auth
===========
.. _feature_accesscontrol_idprovider:

.. note::

  The features described on this page are available in **all** :ref:`Firely Server editions <vonk_overview>`.

In order to use :ref:`access control <feature_accesscontrol>` you need an authorization server that can provide OAuth2 tokens with claims that conform to `SMART on FHIR`_. 
In a production scenario, you typically already have at least an identity provider, i.e. authentication server, in place. It could be the EHR system, an Azure Active Directory / Microsoft EntraID, or a provider set up specifically for let's say a Patient Portal.

Creating SMART on FHIR conformant tokens and handling all protocol details related to a SMART app launch requires dedicated support which generic authorization servers do not offer. Firely provides Firely Auth, an external authorization service optimized for SMART on FHIR, which enables a out-of-the-box experience with your existing authentication services. 

.. note::
   Firely Auth is licensed separately from the core Firely Server distribution. Please :ref:`contact<vonk-contact>` Firely to get the license. 
   Your license already permits the usage of Firely Auth if it contains ``http://fire.ly/server/auth/unlimited``. You can also `try out Firely Auth <https://fire.ly/firely-auth-trial/>`_ using an evaluation license with a limited uptime.
   Firely Auth as part of the Essentials edition (license token ``http://fire.ly/server/auth``) is limited to three registered clients in total.

To allow you to test :ref:`access control <feature_accesscontrol>`, we provide you with :ref:`instructions <firely_auth_introduction>` to build and run Firely Auth in which you can configure the necessary clients, claims and users yourself to test different scenarios.

.. _SMART on FHIR: http://docs.smarthealthit.org/

.. toctree::
   :maxdepth: 1
   :titlesonly:

   firely-auth-releasenotes
   firely-auth-tutorial
   firely-auth-deploy
   firely-auth-metrics
   firely-auth-settings
   firely-auth-endpoints
   firely-auth-sso
   firely-auth-mfa
   firely-auth-management-app
   firely-auth-dar
   firely-auth-glossary
   firely-auth-BOM
