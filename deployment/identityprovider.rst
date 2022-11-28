.. _feature_accesscontrol_idprovider:

Set up an Identity Provider
===========================

About Identity Providers and Firely Server
------------------------------------------

In order to use :ref:`feature_accesscontrol` you need an Identity Provider that can provide OAuth2 JWT Tokens with claims that conform to `SMART on FHIR`_. In a production scenario, you typically already have such a provider. It could be the EHR system, the Active Directory, or a provider set up specifically for let's say a Patient Portal. It is also very well possible that the provider handing the correct claims uses a federated OAuth2 provider to do the authentication.

Firely Auth
-----------

In order to provide a turn-key experience, Firely offers Firely Auth as an add-on. It provides an external authorization services optimized for SMART on FHIR.
For more details, see :ref:`firely_auth_index`.

To allow you to test :ref:`feature_accesscontrol`, we provide you with :ref:`instructions <firely_auth_introduction>` to build and run Firely Auth in which you can configure the necessary clients, claims and users yourself to test different scenarios.

.. |br| raw:: html

   <br />

.. _SMART on FHIR: http://docs.smarthealthit.org/