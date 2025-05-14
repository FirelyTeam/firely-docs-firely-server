.. _deploy_reverseProxy:

=======================================
Deploy Firely Server on a reverse proxy
=======================================

Why
---
For ASP.NET Core 1.0 Microsoft suggested to always use another web server in front of Kestrel for public websites. 
For ASP.NET Core 2.0, while this is not a hard constraint anymore there are still a series of advantages in doing so:

- some scenarios like sharing the same IP and port by multiple applications are not yet supported in Kestrel

- helps in limiting the exposed  surface area

- provides an additional layer of configuration and defense 

- provides process management for the ASP.NET Core application (ensuring it restarts after it crashes)

- in some scenarios a certain web server already integrates very well

- helps simplifying load balancing and SSL setup

Hence using a reverse proxy together with the Kestrel server allows us to get benefits from both technologies at once.

With IIS
--------

A common option on Windows is using IIS: for instructions on how to deploy Firely Server on IIS see :ref:`iis`.

For a comparison of IIS and Kestrel features at the moment of this writing you can `check here <https://stackify.com/kestrel-web-server-asp-net-core-kestrel-vs-iis/?utm_source=DNK-224416>`_.

.. toctree::
   :maxdepth: 1
   :titlesonly:
   :hidden:

   IIS <iis>


With Nginx
----------

A popular open source alternative is Nginx. For instruction on how to deploy Firely Server on Nginx see :ref:`nginx`

.. toctree::
   :maxdepth: 1
   :titlesonly:
   :hidden:

   Nginx <nginx>
   
.. _X_Forwarded_Host:

.. _xforwardedheader:

Using X-Forwarded-Host header
-----------------------------

The `X-Forwarded-Host header <https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Forwarded-Host>`_ is used for identifying the original host by the client, which is especially useful with reverse proxies. 
As per the `FHIR specification <http://hl7.org/fhir/http.html#custom>`_, Firely Server supports usage of this header. Keep in mind that usage of the header exposes privacy sensitive information.

In practice, this header can be set by any HTTP client sending a request, not only the trusted reverse proxy. This might pose a security risk when combined with the functionality of :ref:`limiting access to the administration endpoints based on IP addresses <configure_administration_access>`.
To mitigate these risks, Firely Server offers settings that allow specifying network ranges or specific IPs for trusted reverse proxies. Firely Server ignores the `X-Forwarded-*` headers if the request comes from a non-trusted IP.

    .. code-block:: json
    
         "Hosting": {
            "ReverseProxySupport": {
               "Enabled": false,
               "TrustedProxyIPNetworks": ["127.0.0.1/32", "::1"],
               "AllowAnyNetworkOrigins": false
            }
         }


.. important::
   For security, the use of the `X-Forwarded-*` header is disabled by default in Firely Server 5.11.0 and later versions. This will impact all deployments using a reverse proxy. If you want to upgrade from a previous version, please configure this setting carefully.

.. note::
   If reverse proxy support is enabled, startup will be blocked if the ``TrustedProxyIPNetworks`` setting is configured to accept all IP addresses (e.g., ``0.0.0.0/0`` or ``0.0.0.0``). This is to prevent accidental exposure of the administration endpoints. 
   
   There are two ways to allow broad IP configurations:

   1. Set the ``AllowAnyNetworkOrigins`` option to ``true`` (available in Firely Server v6.1.0 and higher)
   2. Set the value of the ``ASPNETCORE_ENVIRONMENT`` environment variable to "Development" (not recommended for production)
   
   Both options will allow Firely Server to start, but with a warning that the configuration has security implications.

When using this header, make sure that the header value only contains the domain name like listed below:

**Works:**

- fire
- www.fire.ly
- www.fire

**Does not work:**

- fire/
- https://fire.ly

Additionally to the ``X-Forwarded-Host`` header, Firely Server will interpret the ``X-Forwarded-Prefix`` header. This header allows for setting the :ref:`PathBase<hosting_options>` dynamically per request. With this feature you can host a single Firely Server behind a reverse proxy that exposes multiple virtual base urls with subpaths in it. For example: 

* Firely Server itself is hosted on https://fhir.example.org/
* Through a reverse proxy it listens to multiple tenants:

   * "https://fhir.example.org/my/path/to/firelyserver/tenant1" (setting X-Forwarded-Prefix = "/my/path/to/firelyserver/tenant1")
   * "https://fhir.example.org/my/path/to/firelyserver/tenant2" (setting X-Forwarded-Prefix = "/my/path/to/firelyserver/tenant2")

This will result in Firely Server generating urls using the correct virtual base urls while running behind the reverse proxy.

.. _reverse_proxy_security:

Reverse Proxy Security Configurations
------------------------------------

The ``ReverseProxySupport`` section provides options to configure how Firely Server handles requests from reverse proxies, including security settings:

.. code-block:: json

   "Hosting": {
      "ReverseProxySupport": {
         "Enabled": true,
         "TrustedProxyIPNetworks": ["192.168.1.100", "10.0.0.0/24"],
         "AllowAnyNetworkOrigins": false
      }
   }

The ``AllowAnyNetworkOrigins`` Option
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

The ``AllowAnyNetworkOrigins`` setting is a boolean property that allows you to explicitly permit broad network configurations in any environment (including Production):

- When set to ``false`` (default): Firely Server will block startup if ``TrustedProxyIPNetworks`` contains ``0.0.0.0/0`` or ``0.0.0.0``, unless the environment is set to Development
- When set to ``true``: Firely Server will allow any network origin configuration in any environment, with a warning about security implications

Security Considerations
^^^^^^^^^^^^^^^^^^^^^

Using ``0.0.0.0/0`` or ``0.0.0.0`` in ``TrustedProxyIPNetworks`` means that Firely Server will trust all incoming requests as valid reverse proxy traffic. This has important security implications:

- It potentially exposes the server to spoofing attacks, where malicious actors can manipulate the X-Forwarded headers
- Setting ``AllowAnyNetworkOrigins`` to ``true`` should only be done when network security is handled at a different layer (like a firewall or network security group)
- Even with this setting enabled, Firely Server will log warning messages about the security risk
- The recommended approach is to specify exact IP addresses or specific IP networks that should be trusted

Example Configurations
^^^^^^^^^^^^^^^^^^^^

For Development/Testing:

.. code-block:: json

   "ReverseProxySupport": {
      "Enabled": true,
      "TrustedProxyIPNetworks": ["0.0.0.0/0"],
      "AllowAnyNetworkOrigins": true
   }

For Production with Known Proxy IPs (Recommended):

.. code-block:: json

   "ReverseProxySupport": {
      "Enabled": true,
      "TrustedProxyIPNetworks": ["192.168.1.100", "10.0.0.0/24"],
      "AllowAnyNetworkOrigins": false
   }

For Production with Network Security at Other Layers:

.. code-block:: json

   "ReverseProxySupport": {
      "Enabled": true,
      "TrustedProxyIPNetworks": ["0.0.0.0/0"],
      "AllowAnyNetworkOrigins": true
   }

This configuration trusts all IP addresses as reverse proxies and should only be used when security is enforced at the network layer through firewalls, network security groups, or other mechanisms.
