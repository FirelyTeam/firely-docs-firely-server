.. _versions:

Version Availability - $versions
================================

Firely Server can be configured to support multiple FHIR versions. ``$versions`` operation returns the list of available versions. 
This is a system-level operation. 

Operation details
-----------------

The client shall invoke the operation with no parameters. The response lists all currently available and enabled FHIR versions.
It also lists the default version which is used when no ``fhirVersion`` parameter is listed in a request.
Supported response formats are:

#. ``application/fhir+xml`` or ``application/fhir+json`` results in a ``Parameters`` resource as a response. FHIR Versions are listed as items of the resource.
#. ``application/json`` or ``application/xml``.

Configuration
-------------

In the ``PipelineOptions`` section of the :ref:`appsettings <configure_appsettings>` it is enabled by default when ``Vonk.Plugin.Operations`` is present in the ``include`` section:

.. code-block:: JavaScript

    "PipelineOptions": {
        "PluginDirectory": "./plugins",
        "Branches": [
          {
            "Path": "/",
            "Include": [
              // ...
              "Vonk.Plugin.Operations",
            ],
            "Exclude": [
              // ...
            ]
          }, // ...etc...

Manual control over this operation is possible by using ``Vonk.Plugin.Operations.VersionsOperationConfiguration`` in either ``include`` or ``exclude`` sections.

$versions and Firely Auth
-------------------------

When using SMART with Firely Auth as the authorization server, ensure that the ``RequireAuthorization`` setting for the ``$versions`` operation is set to ``Never`` in the operation configuration. 

This setting allows Firely Auth to correctly detect the default FHIR version and connect to Firely Server to retrieve the ``CapabilityStatement``.

Since ``$versions`` only returns the enabled FHIR versions and does not expose any sensitive information, it is safe to make this operation accessible without authorization.

Example
-------

Request:
::

    GET <base-url>/$versions
    Accept: application/fhir+json

Response:

.. code-block:: json

    {
        "resourceType": "Parameters",
        "parameter": [
            {
                "name": "version",
                "valueString": "3.0"
            },
            {
                "name": "version",
                "valueString": "4.0"
            },
            {
                "name": "version",
                "valueString": "5.0"
            },
            {
                "name": "default",
                "valueString": "4.0"
            }
        ]
    }
