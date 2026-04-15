.. _feature_customoperations:

Custom Operations
=================

The FHIR Specification `operations framework`_ defines how custom operations can be implemented and exposed through the `FHIR RESTful API`_. Firely Server provides a range of custom operations out of the box.

The available operations are grouped below by use case:

Validation & Conformance
------------------------

Validation capabilities ensure that resources conform to FHIR conformance resources such as StructureDefinitions and ValueSets. Support is also provided for working with these artifacts directly, including generating snapshots used in validation and interoperability workflows.

- :ref:`feature_validation`
- :ref:`feature_snapshot`

Terminology
-----------

Terminology operations enable working with code systems and value sets in FHIR-based workflows. Support includes validation, lookup, expansion, and mapping of codes, with requests handled through a combination of local and external terminology services.

- :ref:`Terminology Services - $validate-code, $subsumes, $expand, $lookup, $find-matches, $translate, <feature_terminology>`

Metadata & Capability
---------------------

These operations expose resource metadata and server capabilities, enabling management of tags and profiles as well as retrieval of supported FHIR versions.

- :ref:`feature_meta`
- :ref:`versions`

System Health & Administration
------------------------------

Operational endpoints provide insight into system health and enable administrative control, including availability checks and permanent deletion of resources when required.

- :ref:`feature_healthcheck`
- :ref:`erase`

Data Conversion & Document Workflows
------------------------------------

Support for working with FHIR representations and document-based exchange includes format conversion, document generation from clinical data, and retrieval of document references linked to a patient.

- :ref:`feature_convertoperation`
- :ref:`feature_documentoperation`
- :ref:`feature_docref`

Data Retrieval & Export
-----------------------

Extended retrieval capabilities allow access to patient-level datasets and specialized query results, such as recent observations, for use in analytics, workflows, and data exchange scenarios.

- :ref:`Bulk Data Access - $export <feature_bulkdataexport>`
- :ref:`feature_patienteverything`
- :ref:`lastn`

Identity, Matching & Member Access
----------------------------------

Identity resolution across systems is supported through member matching and user context lookup, enabling consistent identification and access to relevant data.

- :ref:`member-match`
- :ref:`fhiruserlookup`

Clinical Reasoning & Quality Measurement
----------------------------------------

Execution of clinical logic and quality measures enables evaluation of FHIR data using CQL expressions and measure calculations, supporting decision support and quality reporting workflows.

- :ref:`feature_qualitymeasures`

Testing & Reporting
-------------------

Support for testing and reporting enables analysis of system usage and behavior based on collected runtime metrics. Queries can be executed asynchronously to produce aggregated insights into API interactions, supporting compliance and reporting requirements.

- :ref:`Realk World Testing - $realworldtesting <feature_realworldtesting_operation>`

.. _operations framework: http://www.hl7.org/implement/standards/fhir/operations.html
.. _FHIR RESTful API: http://www.hl7.org/implement/standards/fhir/http.html