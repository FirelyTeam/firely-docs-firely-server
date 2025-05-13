.. _feature_qdm:

Digital Quality Measures
========================

.. note::

  The features described on this page are available in the following :ref:`Firely Server editions <vonk_overview>`:

  * Firely dQM - 🌍 / 🇺🇸

Digital Quality Measures (dQMs) represent a transformative capability within the healthcare data ecosystem, enabling standardized and automated assessment of clinical outcomes.
Firely Server provides native support for dQMs through based on an integrated clinical reasoning module. Firely Server’s digital quality measurement capabilities are built entirely on the HL7 Clinical Quality Language (CQL) standard, integrated with FHIR resources.

Quality in healthcare is measured by how effectively care enhances the chances of achieving desired health outcomes and reflects up-to-date clinical knowledge.
FHIR supports these capabilities through standardized representations of quality measures, the underlying clinical data, and the reporting process.

----

dQM Use Cases
-------------

Based on the foundation of dQMs, several key use cases can be effectively supported:

#. Prospective and Retrospective Analytics

	Prospective analytics leverage real-time clinical data to predict outcomes, identify at-risk patients, and guide timely interventions—often integrated into clinical decision support workflows. Retrospective analytics focus on historical clinical data to uncover trends, assess population health, and evaluate care performance over defined timeframes.

#. Official Quality Measure Reporting

	Firely Server enables formal reporting of digital quality measures for U.S.-based programs such as HEDIS, CMS MIPS, and other federal or regional quality initiatives. Automating this process reduces the burden of manual data collection, improves reporting accuracy, and supports transparent performance evaluation.

#. Gaps in Care Identification

	dQMs facilitate care gap analysis by detecting where patients have not received guideline-recommended services or interventions. This enables proactive outreach, personalized care planning, and the closing of quality gaps in alignment with value-based care models.

#. Prior Authorization Support

	Within U.S. healthcare systems, digital quality measures can enhance prior authorization** workflows by providing automated clinical justification. Pre-populated clinical data and linked measure logic demonstrate medical necessity, helping streamline the review process and reduce delays in care delivery.

How dQMs Are Represented in FHIR
--------------------------------

Digital Quality Measures are represented in FHIR using a structured set of resources that enable the definition, computation, and reporting of quality measures in a standardized, interoperable format. These resources are primarily part of the Clinical Reasoning module in FHIR and are often paired with the Clinical Quality Language (CQL) to express measure logic.

#. Measure Resource

	A FHIR Measure is a computable, shareable definition of a dQM. It describes what to evaluate, how to evaluate it, and how to report the results, using a consistent structure and formal logic.

#. Library Resource

	In the context of dQMs, the Library resource contains the computable expressions and logic definitions—most often written in CQL or its machine-readable form, Expression Logical Model (ELM). The Library resource enables modularity, versioning, and interoperability in logic sharing across systems and across multiple measures.

#. MeasureReport Resource

	The FHIR MeasureReport is a standardized resource that captures the results of a dQM after it has been evaluated. It functions as the output of the measurement process, summarizing whether patients or populations met the criteria defined in a Measure. In essence, it answers the question: “How did a patient or group perform against a specific quality measure over a defined period?”

----

Executing dQMs in FHIR
----------------------

FHIR defines several key operations that enable the execution, evaluation, and support of CQL-based logic and quality measures. Below are the most relevant operations used in dQM workflows:

* $cql

	:Purpose: Executes Clinical Quality Language (CQL) expressions dynamically.
	:Use Case: Useful for ad hoc evaluation of CQL expressions, such as testing logic during measure development or decision support prototyping.
	:Input: A CQL expression and the relevant data context (e.g., patient data).
	:Output: The evaluated result of the expression (e.g., Boolean, date, quantity) encoded in a FHIR Parameters resource.

* Library/$evaluate

	:Purpose: Executes all or specific expression from a Library resource.
	:Use Case: Used to evaluate a named expression (e.g., a defined function or value set) within a pre-defined Library. Often used to debug the logic of a Library.
	:Input: Canonical reference to a Library, expression name, and optional patient and context data.
	:Output: The result of the evaluated expression (same as $cql, but tied to named expressions in a Library).

* Library/$data-requirements

	:Purpose: Returns the data requirements (FHIR resource types, value sets, codes) needed by a CQL Library.

	:Use Case: Critical for data validation, measure packaging, or generating queries to collect required clinical data.

	:Input: Reference to a Library.

	:Output: A list of Library resource of type 'module-definition' describing what input is needed for evaluation of the inital Library.

* Measure/$evaluate-measure

	:Purpose: Evaluates a full Measure resource over a defined period for a specific patient or population.

	:Use Case: Central to calculating quality measure results, generating MeasureReport resources for submission or analysis.

	:Input: Canonical reference to a Measure, the reporting period (defined by periodStart and periodEnd), and the subject, which can be either a specific patient ID or a population group.

	:Output: A MeasureReport containing the calculated results for numerator, denominator, exclusions, stratifiers, etc.


See :ref:`feature_customoperations` for a more detailed description of each operation.

----

FHIR Measures
-------------

Understanding dQMs and Population Criteria
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
In most cases, dQMs in FHIR divide a patient population into distinct subgroups, each defined by specific population criteria. These criteria are expressed using CQL and applied to an overarching group known as the Initial Population.
Each subgroup, such as the numerator, denominator, exclusions, or exceptions represents a meaningful classification based on clinical or administrative data. The measure logic filters patients into these groups based on conditions defined in the associated CQL expressions.

The outcome of a measure evaluation is typically a proportion: the number of patients who meet the numerator criteria (e.g., those who received a recommended intervention) divided by the number of patients in the denominator (i.e., those who were eligible for that intervention based on matching data criteria).

The ``Measure`` resource brings together:

- Descriptive metadata about the measure (e.g., name, version, description)
- A canonical reference to a ``Library`` resource that contains the CQL logic
- Definitions of each population and their associated criteria

For more detailed guidance on defining and implementing FHIR-based measures, refer to the `CQF Measures Implementation Guide <http://hl7.org/fhir/us/cqfmeasures>`_.

Example Measure
^^^^^^^^^^^^^^^

Below are the detials of a Measure resource outlined.

.. code-block:: json
   :caption: FHIR Measure Resource – Blood Pressure Check for Adults
   :name: bp-measure-json

   {
     "resourceType": "Measure",
     "id": "bp-check-adults",
     "url": "http://example.org/fhir/Measure/bp-check-adults",
     "version": "1.0.0",
     "name": "BloodPressureCheckAdults",
     "title": "Blood Pressure Check for Adults",
     "status": "active",
     "experimental": true,
     "date": "2025-01-01",
     "publisher": "Example Health Org",
     "description": "Measure assessing whether adult patients (18 years or older) had at least one systolic blood pressure reading during the measurement period.",
     "library": [
       "http://example.org/fhir/Library/bp-check-logic"
     ],
     "scoring": {
       "coding": [
         {
           "system": "http://terminology.hl7.org/CodeSystem/measure-scoring",
           "code": "proportion"
         }
       ]
     },
     "group": [
       {
         "id": "9a3f3b12-4e7d-4cf2-8e6a-729e5a21f4b9",
         "population": [
           {
             "code": {
               "coding": [
                 {
                   "system": "http://terminology.hl7.org/CodeSystem/measure-population",
                   "code": "initial-population"
                 }
               ]
             },
             "criteria": {
               "language": "text/cql-identifier",
               "expression": "AdultPatients"
             }
           },
           {
             "code": {
               "coding": [
                 {
                   "system": "http://terminology.hl7.org/CodeSystem/measure-population",
                   "code": "denominator"
                 }
               ]
             },
             "criteria": {
               "language": "text/cql-identifier",
               "expression": "AdultPatients"
             }
           },
           {
             "code": {
               "coding": [
                 {
                   "system": "http://terminology.hl7.org/CodeSystem/measure-population",
                   "code": "numerator"
                 }
               ]
             },
             "criteria": {
               "language": "text/cql-identifier",
               "expression": "HasBPReading"
             }
           }
         ]
       }
     ]
   }

.. attention::

	Firely Server currently requires that each group within a Measure resource includes an "id" element to ensure correct generation of the corresponding MeasureReport.

Each population criterion corresponds to a named expression defined in the CQL within the referenced Library. To ensure the dQM engine correctly interprets the selection logic, the criteria.language must be set to "text/cql-identifier", indicating that the population is identified by a named CQL expression.

Managing Measures
^^^^^^^^^^^^^^^^^

Measures are treated as administrative resources and can be uploaded to the administration endpoint of Firely Server. See :ref:`administration_api` for more details.