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

The following is a FHIR `Measure` resource defining the populations used in an example measure for Blood Pressure Checks for Adults:

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

----

FHIR Libaries
-------------

A FHIR Library resource contains one or more representations of the CQL logic that defines the population criteria referenced by the Measure resource. 
In addition to publishing metadata, the Library includes the original CQL content—encoded in base64—within a content element annotated with contentType "text/cql".

While CQL is designed to be human-readable and author-friendly, it must be translated into ELM to be machine-readable. 
ELM uses a canonical abstract syntax tree (AST) to represent CQL expressions, decisions, and data references in a structured way. 
This makes it portable and enables any compliant engine to evaluate the logic consistently, regardless of the original authoring tool.

Firely Server internally uses the open-source `.NET CQL SDK <https://github.com/FirelyTeam/firely-cql-sdk>`_ to compile ELM into executable C# code, enabling enhanced debuggability and high-performance execution. 
As a result, the ``Library`` resource must include a compiled binary (``.dll`` file), which is dynamically loaded at runtime during the execution of operations such as ``Measure/$evaluate-measure`` or ``Library/$evaluate``.

Compiling CQL
^^^^^^^^^^^^^

When uploading ``Library`` resources to Firely Server, it is expected that the compiled `.dll` file is included as one of the content representations within the resource.
The compilation process must be performed manually using the `.NET CQL SDK <https://github.com/FirelyTeam/firely-cql-sdk>`_. After downloading the SDK, open the solution file ``Cql-Sdk-All.sln`` in your development environment.

Within the solution, the project ``PackageCli (Demo CQL -> FHIR)`` provides a demo of the packaging workflow. Any CQL files placed in the folder:

::

  LibrarySets/Demo/Cql

will be automatically compiled to ELM and C# during the build process. The resulting FHIR ``Library`` resources will be generated in:

::

  LibrarySets/Demo/Resources

Alternatively, you can perform the compilation and packaging process via command line using the ``Hl7.Cql.Packager`` tool:

::

  Hl7.Cql.Packager cql \
    --cql <path to project>/LibrarySets/Demo/Cql \
    --fhir <path to project>LibrarySets/Demo/Resources \
    --dll <path to project>/LibrarySets/Demo/Assemblies \
    --cs <path to project>/Demo/Measures.Demo/CSharp

Please make sure to adjust ``<path to project>`` according to your local environment.
This process generates the required artifacts, including the ELM, compiled C# source, and DLL, all of which are necessary for successful evaluation on Firely Server.


When generating ``Library`` resources, the compiler must assign a base URL to construct the canonical URL of each library. This can be configured using the ``BaseCanonicalUrl`` setting in the ``Hl7.Cql.Packager.appsettings.json`` file.
For external libraries, it may not be appropriate to apply the default base URL. In such cases, you can use the ``FixedLibraryCanonicals`` setting to explicitly map CQL library names to their intended canonical URLs, ensuring accurate references without overriding external sources.

In some use cases, it may be necessary to rely on existing ELM files generated by external tooling, such as the Java-based `CQF Framework <https://marketplace.visualstudio.com/items?itemName=cqframework.cql>`_.
To skip ELM generation by the .NET CQL SDK and instead use pre-generated ELM, you can invoke the ``elm`` command of the packager CLI as follows:

::

  Hl7.Cql.Packager elm \
    --cql <path to project>/LibrarySets/Demo/Cql \
    --elm <path to project>/LibrarySets/Demo/Elm \
    --fhir <path to project>/LibrarySets/Demo/Resources \
    --dll <path to project>/LibrarySets/Demo/Assemblies \
    --cs <path to project>/Demo/Measures.Demo/CSharp

Please make sure to adjust ``<path to project>`` according to your local environment.
This command assumes that the ELM files already exist in the specified ``--elm`` directory and will package them—along with the corresponding C# code and FHIR artifacts—into the compiled output structure.

.. attention::

	Firely Server currently depends on CQL SDK version v2.0.0-alpha16, which must be used for the compilation process to ensure compatibility.

Example Library
^^^^^^^^^^^^^^^

The following is a FHIR `Library` resource defining the CQL logic used in the Blood Pressure Check for Adults measure:

.. code-block:: json
   :caption: FHIR Library – Blood Pressure Check Logic
   :name: bp-check-library

   {
     "resourceType": "Library",
     "id": "bp-check-logic",
     "url": "http://example.org/fhir/Library/bp-check-logic",
     "version": "1.0.0",
     "name": "BloodPressureCheckLogic",
     "title": "Blood Pressure Check Logic",
     "status": "active",
     "experimental": true,
     "type": {
       "coding": [
         {
           "system": "http://terminology.hl7.org/CodeSystem/library-type",
           "code": "logic-library"
         }
       ]
     },
     "subjectCodeableConcept": {
       "coding": [
         {
           "system": "http://hl7.org/fhir/resource-types",
           "code": "Patient"
         }
       ]
     },
     "relatedArtifact": [
       {
         "type": "depends-on",
         "display": "Library FHIRHelpers",
         "resource": "https://fhir.org/guides/cqf/common/Library/FHIRHelpers|4.0.001"
       }
     ],
     "parameter": [
       {
         "extension": [
           {
             "url": "http://hl7.org/fhir/StructureDefinition/cqf-cqlType",
             "valueString": "Interval<DateTime>"
           }
         ],
         "name": "Measurement Period",
         "use": "in",
         "min": 0,
         "max": "1",
         "type": "Period"
       },
       {
         "extension": [
           {
             "url": "http://hl7.org/fhir/StructureDefinition/cqf-cqlType",
             "valueString": "Boolean"
           }
         ],
         "name": "AdultPatients",
         "use": "out",
         "min": 0,
         "max": "1",
         "type": "boolean"
       },
       {
         "extension": [
           {
             "url": "http://hl7.org/fhir/StructureDefinition/cqf-cqlType",
             "valueString": "Boolean"
           }
         ],
         "name": "HasBPReading",
         "use": "out",
         "min": 0,
         "max": "1",
         "type": "boolean"
       }
     ],
     "date": "2025-01-01",
     "publisher": "Example Health Org",
     "description": "CQL logic for identifying adult patients with at least one systolic blood pressure reading during the measurement period.",
     "content": [
       {
         "id": "BloodPressureCheckLogic-1.0.0+cql",
         "contentType": "text/cql",
         "data": "<base64-encoded CQL omitted for brevity>"
       },
       {
         "id": "BloodPressureCheckLogic-1.0.0+elm",
         "contentType": "application/elm+json",
         "data": "<omitted for brevity>"
       },
       {
         "id": "BloodPressureCheckLogic-1.0.0+dll",
         "contentType": "application/octet-stream",
         "data": "<omitted for brevity>"
       },
       {
         "id": "BloodPressureCheckLogic-1.0.0+csharp",
         "contentType": "text/plain",
         "data": "<omitted for brevity>"
       }
     ]
   }


The ``cqf-cqlType`` extension on input and output parameters is primarily used for documentation purposes, indicating the intended CQL type for each parameter.
However, it can also influence the behavior of the ``Library/$evaluate`` operation, particularly when a parameter is of type ``FHIR Period``. 
In such cases, the FHIR ``Period`` can be translated to either a ``CQL Interval<date>`` or ``Interval<dateTime>``, depending on how the parameter is defined in the referenced logic library.

The following CQL logic corresponds to the population expressions defined in the Blood Pressure Check library. 
It defines adult patients and checks whether they have a recorded systolic blood pressure observation during the measurement period.

.. code-block:: cql
   :caption: BloodPressureCheckLogic.cql
   :name: bp-check-cql

   library BloodPressureCheckLogic version '1.0.0'

   using FHIR version '4.0.1'

   include FHIRHelpers version '4.0.001'

   codesystem "LOINC:2.69": 'http://loinc.org' version '2.69'
   code "Systolic blood pressure": '8480-6' from "LOINC:2.69" display 'Systolic blood pressure'

   /* Define the Measurement Period */
   parameter "Measurement Period" Interval<DateTime>
     default Interval[@2025-01-01T00:00:00.0, @2025-12-31T00:00:00.0]

   context Patient

   /* Define the initial population of adult patients */
   define "AdultPatients": 
       AgeInYearsAt(date from start of "Measurement Period") >= 18

   /* Define patients with a Systolic Blood Pressure Observation */
   define "HasBPReading": 
     exists (
       [Observation] o
           where (o.code ~ "Systolic blood pressure")
           and (o.effective as dateTime) during "Measurement Period"
     )



Managing Libaries
^^^^^^^^^^^^^^^^^

Libraries are treated as administrative resources and can be uploaded to the administration endpoint of Firely Server. See :ref:`administration_api` for more details.

----

FHIR MeasureReports
-------------------