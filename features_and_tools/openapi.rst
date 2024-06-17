.. _openapi:

OpenAPI
=======

.. note::

  The features described on this page are available in the following :ref:`Firely Server editions <vonk_overview>`:

  * Firely Scale - 🌍 / 🇺🇸
  * Firely CMS Compliance - 🇺🇸

Firely Server is capable of generating Swagger / OpenAPI documentation. The content of these definitional artifacts are based on a `CapabilityStatement <http://hl7.org/fhir/capabilitystatement.html>`_.
A full overview of the REST API provided by Firely Server can be found here: `OpenAPI documentation <../_static/swagger>`_. Please note that due to the feature-richness of the CapabilityStatement resource, it is not possible to expose all information through the OpenAPI documents and some limitations exist.

Limitations of Capability Statement to OpenAPI mapping
------------------------------------------------------

* Resource profile definition references are not displayed.
* Field profile definition references are not displayed.
* Supported interactions are not displayed.

	* i.e. ``searchInclude, searchRevInclude``

* Supported interactions are not directly listed, but are inferred from the output. 

	* i.e. ``GET {BASE_URL}/Appointment/{logical_id}/_history`` infers ``readhistory`` is permitted.


Generating OpenAPI for a local instance of Firely Server
--------------------------------------------------------

OpenAPI documents can be generated for a local instance of Firely Server using the open source github repository `Microsoft/Fhir-CodeGen <https://github.com/microsoft/fhir-codegen>`_.

A sample configuration is listed below:

::

	fhir-codegen-cli 
		--fhir-server-url <FhirServerUrl> 
		--resolve-external false 
		--language-options "OpenApi|SingleResponses=false|Metadata=true|SchemaLevel=names|MultiFile=true" 
		--output-path <OpenApiOutputDirectory> 
		--language OpenApi

See Microsoft's documentation for other configuration options and usage details.

