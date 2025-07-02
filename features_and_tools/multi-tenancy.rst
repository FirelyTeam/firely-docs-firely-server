.. _feature_multitenancy:

Multi-tenancy
=============

.. note::

  The features described on this page are available in the following :ref:`Firely Server editions <vonk_overview>`:

  * Firely Scale - 🌍 / 🇺🇸
  * Firely CMS Compliance - 🇺🇸

Firely Server offers *virtual* multi-tenancy. All the resources are still stored in a single database, but by applying tenant information they are virtually separated.
The basics are simple:

- On every request, the tenant is read from either a specific HTTP header or a claim in the authorization token.
- If no tenant is specified, and the operation being invoked does not require a tenant, the special ``shared`` tenant is used internally.
- If the request creates or updates any resource, the tenant is applied to each of those resources. This is done by adding a *security label* to the ``meta.security`` element of the resource.
- For both reading and writing resources, the tenant is used to limit access to only the resources having a matching security label.
- Upon reading, by default, the tenant security label is removed from the resource and thus not visible from the outside. If you want to communicate the label to the client, set ``RemoveTenantLabelFromResponse`` to ``false`` in the settings.

Implications
------------

.. warning:: 

    It is very difficult to enable this feature after resources have already been loaded into Firely Server. 
    Resources that do not have a tenant security label cannot be retrieved or updated anymore.

.. warning:: 

    :ref:`Firely Server Ingest <tool_fsi>` does not apply tenant labels. You must add them to any resource before ingesting them. 
    This includes Bundle entries.

.. warning:: 

    As a security measure to prevent data leaks, when multi-tenancy is enabled, the :ref:`history <restful_history>` and ``vread`` operations are disabled.
    Without this measure, when a resource is deleted by the tenant and later another tenant creates a resource with the same id, the history of changes would be accessible.

Other implications:

- The logical id of a resource must be unique *across tenants*.
- An update cannot change the tenant of a resource.
- If a HTTP header is used to specify the tenant, this should be applied in a reverse proxy, to prevent clients from specifying their own tenant.
- If a security label for the tenant is applied already in the body of the request, it must be consistent with the tenant in the request.
- Handling the tenant information takes time and resources, and may increase the response time.

Configuration
-------------

To enable the feature, include ``Vonk.Plugin.VirtualTenants`` in the pipeline for the regular resources (typically the ``"/"`` path).

You can control the working of the feature with the settings:

    .. code-block:: json

        "VirtualTenants": { 
            "TenantHeader": "x-firely-tenant",
            "TenantClaim": "tenant",
            "TenantLabelSystem": "http://server.fire.ly/fhir/sid/tenant-label",
            "AllowTenantFromHeader": true,
            "AllowTenantFromClaim": true,
            "RemoveTenantLabelFromResponse": true
        },

:TenantHeader: The name of the HTTP header to read the tenant from. This is only evaluated if ``AllowTenantFromHeader`` is set to ``true``.
:TenantClaim: The name of the claim in the authorization token to read the tenant from. This is only evaluated if ``AllowTenantFromClaim`` is set to ``true``.
:TenantLabelSystem: The value of the ``meta.security.system`` element to use for the tenant security label. You can only choose this once.
:AllowTenantFromHeader: The tenant may be specified with an HTTP header, with the name specified in ``TenantHeader``.
:AllowTenantFromClaim: The tenant may be specified with a claim in the authorization token, with the name specified in ``TenantClaim``.
:RemoveTenantLabelFromResponse: The label will be removed from the resource by default, but this behaviour can be disabled if this option is set to ``false``.

.. warning:: 

    Choose the ``TenantLabelSystem`` wisely. Once resources have been loaded into Firely Server it is nearly impossible to update this.
