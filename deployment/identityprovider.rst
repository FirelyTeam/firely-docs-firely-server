.. _feature_accesscontrol_idprovider:

Set up an Identity Provider
===========================

About Identity Providers and Firely Server
------------------------------------------

In order to use :ref:`feature_accesscontrol` you need an Identity Provider that can provide OAuth2 JWT Tokens with claims that conform to `SMART on FHIR`_. In a production scenario, you typically already have such a provider. It could be the EHR system, the Active Directory, or a provider set up specifically for let's say a Patient Portal. It is also very well possible that the provider handing the correct claims uses a federated OAuth2 provider to do the authentication.

An Identity Provider for testing
--------------------------------

To allow you to test :ref:`feature_accesscontrol`, we provide you with instructions to build and run an Identity Provider in which you can configure the necessary clients, claims and users yourself to test different scenarios. The instructions are based on the excellent `IdentityServer4 project on GitHub <https://github.com/IdentityServer/IdentityServer4>`_ by Dominick Baier and Brock Allen. 

By default, the configuration is such that you can test many different cases. If you wish to adjust the configuration, that will require a bit of programming.

The Identity Provider is built in Microsoft .NET Core. Therefore it should also run cross-platform, just as Firely Server itself. However, we did not try that. 

.. note::

  The project below is provided for your convenience. It comes with no warranty and is not supported by Firely. 

In order to get tokens from the Identity Provider you need an http client. We included instructions on :ref:`feature_accesscontrol_postman`.

Instructions
------------

#. Clone the project `Vonk.IdentityServer.Test from GitHub <https://github.com/FirelyTeam/Vonk.IdentityServer.Test>`_
#. Run the Powershell script .\\scripts\\GenerateSSLCertificate.ps1 |br|
   This will generate an SSL Certificate in .\\Vonk.IdentityServer.Test\\ssl_cert.pfx, with the password |br| 'cert-password'. This is preconfigured in Program.cs.
#. Open the solution Vonk.IdentityServer.Test.sln in Visual Studio
#. Build the solution
#. Run the Vonk.IdentityServer.Test project
#. Visual Studio should automatically open http://localhost:5100 in your browser.

   You should see a page like this.

   .. image:: ../images/ac_identityprovider_startup.png

#. Also try https://localhost:5101 for the https connection. Your browser will ask you to make a security exception for the self-signed certificate. 
#. Get the openid connect configuration at http://localhost:5100/.well-known/openid-configuration.
   You can see all the available scopes in this document.

Configuration
-------------

The Identity Server is preconfigured with two users and one client:

Client
^^^^^^

:ClientId: Postman
:Secret: secret
:Redirect Uri: https://www.getpostman.com/oauth2/callback

This client is allowed to request any of the available scopes. 

It is called Postman, since many users use the Postman REST client to test FHIR Servers. If you use another client, you can still use it as the ClientId, or alter the values in Config.cs.

Users
^^^^^

Alice
~~~~~

:UserName: Alice
:Password: password
:Launch context: patient=alice-identifier

Bob
~~~

:UserName: Bob
:Password: password
:Launch context: patient=bob-identifier

You can add or alter users in Config.cs.

.. _SMART on FHIR: http://docs.smarthealthit.org/

.. |br| raw:: html

   <br />
