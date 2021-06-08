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


With Nginx
----------

A popular open source alternative is Nginx. For instruction on how to deploy Firely Server on Nginx see :ref:`nginx`

.. toctree::
   :maxdepth: 2
   :titlesonly:
   :hidden:

   IIS <iis>
   Nginx <nginx>

