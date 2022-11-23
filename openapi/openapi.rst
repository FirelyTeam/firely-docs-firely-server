.. _openapi:

OpenAPI
=======

Firely Server is capable of generating Swagger / OpenAPI documentation. The content of these definitional artifcats are based on a `CapabilityStatement <http://hl7.org/fhir/capabilitystatement.html>`_.
A full overview of the REST API provided by Firely Server can be found here: `OpenAPI documentation <../_static/swagger>`_. Please note that due to the feature-richness of the CapabilityStatement resource, it is not possible to expose all information through the OpenAPI documents and some limitations exist.

Limitations of Capability Statement to OpenAPI mapping
------------------------------------------------------

- Resource profile definition references are not displayed.
- Field profile definition references are not displayed.
- Supported interactions are not displayed.

	- i.e. ``searchInclude, searchRevInclude``

- Supported interactions are not directly listed, but are inferred from the output. 

	- i.e. ``GET {BASE_URL}/Appointment/{logical_id}/_history`` infers ``readhistory`` is permitted.

