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
               "TrustedProxyIPNetworks": ["127.0.0.1/32", "::1"]
            }
         }


.. important::
   For security, the use of the `X-Forwarded-*` header is disabled by default in Firely Server 5.11.0 and later versions. This will impact all deployments using a reverse proxy. If you want to upgrade from a previous version, please configure this setting carefully.

.. note::
   If reverse proxy support is enabled, startup will be blocked if the ``TrustedProxyIPNetworks`` setting is configured to accept all ip addresses. This is to prevent accidental exposure of the administration endpoints. If you want to allow all ip addresses for testing purposes, you can set the value of the ``ASPNETCORE_ENVIRONMENT`` environment variable to "Development". Firely Server will then startup with a warning that the configuration is not secure.

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
