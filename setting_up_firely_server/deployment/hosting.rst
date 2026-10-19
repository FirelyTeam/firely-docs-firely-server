.. |br| raw:: html

   <br />

.. _configure_hosting:

Configure http and https
========================

You can enable http and/or https and adjust the port numbers of Firely Server as described below.

Changing the port number
------------------------

By default Firely Server will run on port 4080 of your system. You can change the port setting by overriding it as described in :ref:`configure_change_settings`:

*	Navigate to your Firely Server working directory
*	Find this setting:
	::

		"Hosting": {
			"HttpPort": 4080
		}

*	Change the number to the port number you want


Changing from http to https
---------------------------

If you need your server to run on https instead of http, follow these steps:

*	Navigate to the location where you extracted the Firely Server files.
*	Find these settings:

    ::

		"Hosting": {
			"HttpPort": 4080,
			"HttpsPort": 4081, // Enable this to use https
			"CertificateFile": "<your-certificate-file>.pfx", //Relevant when HttpsPort is present
			"CertificatePassword" : "<cert-pass>" // Relevant when HttpsPort is present
		},

*	Uncomment the lines for :code:`HttpsPort`, :code:`CertificateFile` and :code:`CertificatePassword`.
*	Set the :code:`HttpsPort` to the port of your liking (standard https port is 443)
*	Set :code:`CertificateFile` to the location of the `.pfx` file that contains the certificate for your site
*	Set :code:`CertificatePassword` to the password for the certificate file.

    ..	note::

        We recommend setting this value as an environment variable for security reasons::

	    	VONK_Hosting:CertificatePassword=<password>

..


	To set this:

	+ In Powershell run:|br| 
	  ``> $env:VONK_Hosting:CertificatePassword="my_password"``
	  |br| where `my_password` is the password for the `.pfx` file
	+ or go to your `System`, open the `Advanced system settings` --> `Environment variables` and create a new variable
	  with the name :code:`VONK_Hosting:CertificatePassword` and the value set to your password

*   You can choose to comment-out the ``HttpPort`` setting, so Firely Server will no longer be available through unsecured http.

.. _configure_hosting_http2:

Enabling HTTP/2
---------------

By default Firely Server only accepts HTTP/1.1. To accept HTTP/2 as well, set ``IsHttp1AndHttp2`` to ``true``:

::

	"Hosting": {
		"HttpsPort": 4081,
		"CertificateFile": "<your-certificate-file>.pfx",
		"CertificatePassword" : "<cert-pass>",
		"IsHttp1AndHttp2": true
	},

The setting applies to both the ``HttpPort`` and the ``HttpsPort``. A client and Firely Server agree on HTTP/2 during the TLS handshake, so in practice HTTP/2 is only used on the ``HttpsPort``. On the ``HttpPort``, clients keep using HTTP/1.1.
If Firely Server runs behind a reverse proxy, it is the proxy that has to support HTTP/2 towards the clients.