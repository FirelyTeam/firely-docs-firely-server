.. _feature_qdm:

Intro to Digital Quality Measures
=================================

.. note::

  The features described on this page are available in the following :ref:`Firely Server editions <vonk_overview>`:

  * Firely dQM - 🌍 / 🇺🇸

.. note::

  The operations require the license token ``http://fire.ly/vonk/plugins/cql`` to be present in the license file.
  If you do not have this license token, please contact `Firely <https://fire.ly/contact>`_.

Digital Quality Measures (dQMs) represent a transformative capability within the healthcare data ecosystem, enabling standardized and automated assessment of clinical outcomes.
For example, a dQM can automatically determine what percentage of adult patients had their blood pressure measured in the last year, based entirely on structured FHIR data and executable logic.
Firely Server provides native support for dQMs based on an integrated clinical reasoning module. Firely Server’s digital quality measurement capabilities are built entirely on the `HL7 Clinical Quality Language (CQL) <https://cql.hl7.org>`_ standard, integrated with FHIR resources.

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

	Within U.S. healthcare systems, digital quality measures can enhance prior authorization workflows by providing automated clinical justification. Pre-populated clinical data and linked measure logic demonstrate medical necessity, helping streamline the review process and reduce delays in care delivery.

#. Structured Data Capture via FHIR Questionnaires

  Using structured data capture tools, digital forms (Questionnaires) can be automatically filled with information already available in the patient’s record—such as age, past diagnoses, or lab results. This saves time for clinicians, reduces errors from manual entry, and ensures that the quality measures are based on up-to-date and complete information.

How dQMs Are Represented in FHIR
--------------------------------

Digital Quality Measures are represented in FHIR using a structured set of resources that enable the definition, computation, and reporting of quality measures in a standardized, interoperable format. 
These resources are primarily part of the Clinical Reasoning module in FHIR and are often paired with the Clinical Quality Language (CQL) to express measure logic.

#. Measure Resource

	A FHIR Measure is a computable, shareable definition of a dQM. It describes what to evaluate, how to evaluate it, and how to report the results, using a consistent structure and formal logic.

#. Library Resource

	In the context of dQMs, the Library resource contains the computable expressions and logic definitions—most often written in CQL or its machine-readable form, Expression Logical Model (ELM). The Library resource enables modularity, versioning, and interoperability in logic sharing across systems and across multiple measures.

#. MeasureReport Resource

	The FHIR MeasureReport is a standardized resource that captures the results of a dQM after it has been evaluated. It functions as the output of the measurement process, summarizing whether patients or populations met the criteria defined in a Measure. In essence, it answers the question: “How did a patient or group perform against a specific quality measure over a defined period?”

For more background information about Clinical Reasoning, see `Introduction to Clinical Reasoning - FHIR Core specification <https://hl7.org/fhir/R4/clinicalreasoning-module.html>`_.

----

Executing dQMs in FHIR
----------------------

FHIR defines several key operations that enable the execution, evaluation, and support of CQL-based logic and quality measures. Below are the most relevant operations used in dQM workflows:

* Library/$evaluate

	:Purpose: Executes all or specific expression from a Library resource.
	:Use Case: Used to evaluate a named expression (e.g., a defined function or value set) within a pre-defined Library. Often used to debug the logic of a Library.
	:Input: Canonical reference to a Library, expression name, and optional patient and context data.
	:Output: The result of the evaluated expression (same as $cql, but tied to named expressions in a Library).

	See `Using CQL with FHIR - OperationDefinition Library/$evaluate <https://hl7.org/fhir/uv/cql/OperationDefinition-cql-library-evaluate.html>`_ for the full HL7 specification, and :ref:`feature_library_evaluate` for details on how to execute this operation in Firely Server, including which parameters are supported.

* Library/$data-requirements

	:Purpose: Returns the data requirements (FHIR resource types, value sets, codes) declared on a CQL Library.
	:Use Case: Critical for data validation, measure packaging, or generating queries to collect required clinical data.
	:Input: Canonical reference to a Library.
	:Output: A Library resource of type 'module-definition' holding a copy of the ``dataRequirement`` elements of the Library. Firely Server does not derive them from the CQL or ELM.

	See `FHIR Core specification - OperationDefinition Library/$data-requirements <https://www.hl7.org/fhir/R4/library-operation-data-requirements.html>`_ for the HL7 specification, and :ref:`feature_data_requirements` for details on how to execute this operation in Firely Server.

* Measure/$evaluate-measure

	:Purpose: Evaluates a full Measure resource over a defined period for a specific patient or population.
	:Use Case: Central to calculating quality measure results, generating MeasureReport resources for submission or analysis.
	:Input: Canonical reference to a Measure, the reporting period (defined by periodStart and periodEnd), and the subject, which can be either a specific patient ID or a population group.
	:Output: A MeasureReport containing the calculated results for numerator, denominator, exclusions, stratifiers, etc.

	See `FHIR Core specification - OperationDefinition Measure/$evaluate-measure <https://www.hl7.org/fhir/R4/measure-operation-evaluate-measure.html>`_ on how to execute this operation.

* Measure/$data-requirements

    :Purpose: Returns the data requirements (FHIR resource types, value sets, codes) declared on the logic Library of a CQL-based measure.
    :Use Case: Used to determine what data is necessary to run a measure, support validation against EHR capabilities or generate queries for patient/population data collection.
    :Input: Canonical reference to a Measure.
    :Output: A Library resource of type module-definition holding a copy of the ``dataRequirement`` elements of the single Library the Measure references. The requirements are not aggregated across libraries.

    See `Quality Measure Implementation Guide - OperationDefinition Measure Data Requirements <https://hl7.org/fhir/us/cqfmeasures/OperationDefinition-Measure-data-requirements.html>`_ for the HL7 specification, and :ref:`feature_data_requirements` for details on how to execute this operation in Firely Server.

* $cql

	:Purpose: Executes Clinical Quality Language (CQL) expressions dynamically.
	:Use Case: Useful for ad hoc evaluation of CQL expressions, such as testing logic during measure development or decision support prototyping.
	:Input: A CQL expression and the relevant data context (e.g., patient data).
	:Output: The evaluated result of the expression (e.g., Boolean, date, quantity) encoded in a FHIR Parameters resource.

	See `Using CQL with FHIR - OperationDefinition $cql <https://hl7.org/fhir/uv/cql/OperationDefinition-cql-cql.html>`_ for the full HL7 specification, and :ref:`feature_cql_operation` for details on how to execute this operation in Firely Server, including which parameters are supported.

Enabling dQM Operations in Firely Server
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

The dQM operations above are not enabled by default. Enabling them takes two steps.

First, add the CQL plugin to the pipeline. It is not part of the default ``PipelineOptions``, but it ships with Firely Server, so no files need to be added to the plugin directory. Add ``Vonk.Plugin.Cql`` to the ``Include`` list of the pipeline branch that serves your FHIR data:

.. code-block:: json

   "PipelineOptions": {
     "Branches": [
       {
         "Path": "/",
         "Include": [
           "Vonk.Core",
           "...",
           "Vonk.Plugin.Cql"
         ]
       }
     ]
   }

This namespace enables all five operations (``Library/$evaluate``, ``Measure/$evaluate-measure``, ``$cql``, ``Library/$data-requirements`` and ``Measure/$data-requirements``). To enable only some of them, include their configuration classes instead, listed in :ref:`vonk_available_plugins`; ``Measure/$evaluate-measure`` and ``$cql`` also need ``Library/$evaluate``, and ``Measure/$data-requirements`` needs ``Library/$data-requirements``. The operations require a license that includes the ``http://fire.ly/vonk/plugins/cql`` token.

Second, add the following entries to the ``Operations`` section of ``appsettings.json``:

.. code-block:: json

   "Operations": {
     "$evaluate": {
       "Name": "$evaluate",
       "Level": [ "Type", "Instance" ],
       "Enabled": true,
       "RequireAuthorization": "WhenAuthEnabled",
       "RequireTenant": "WhenTenancyEnabled"
     },
     "$evaluate-measure": {
       "Name": "$evaluate-measure",
       "Level": [ "Type", "Instance" ],
       "Enabled": true,
       "RequireAuthorization": "WhenAuthEnabled",
       "RequireTenant": "WhenTenancyEnabled"
     },
     "$cql": {
       "Name": "$cql",
       "Level": [ "System" ],
       "Enabled": true,
       "RequireAuthorization": "WhenAuthEnabled",
       "RequireTenant": "WhenTenancyEnabled"
     },
     "$data-requirements": {
       "Name": "$data-requirements",
       "Level": [ "Type", "Instance" ],
       "Enabled": true,
       "RequireAuthorization": "WhenAuthEnabled",
       "RequireTenant": "WhenTenancyEnabled"
     }
   }

For a full description of each ``Operations`` property, see :ref:`fs_settings_reference`.

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
             "id": "initial-population",
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
             "id": "denominator",
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
             "id": "numerator",
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

	Firely Server currently requires that each group within a Measure resource includes an "id" element, unique within the Measure, to ensure correct generation of the corresponding MeasureReport. Otherwise, ``Measure/$evaluate-measure`` responds with ``412 Precondition Failed``.
	Each population needs an "id" element as well. A population without one is still evaluated, but it is left out of the MeasureReport without an error: Firely Server only logs a warning.

Each population criterion corresponds to a named expression defined in the CQL within the referenced Library. To ensure the dQM engine correctly interprets the selection logic, the criteria.language must be set to "text/cql-identifier", indicating that the population is identified by a named CQL expression.

.. _feature_dqm_scoring:

Measure Scoring Methods
^^^^^^^^^^^^^^^^^^^^^^^

The scoring method of a measure determines which populations a group defines and how their results are combined into a score.
It is declared in ``Measure.scoring``, using a code from the ``http://terminology.hl7.org/CodeSystem/measure-scoring`` CodeSystem (see the `measure-scoring value set <https://hl7.org/fhir/R4/valueset-measure-scoring.html>`_).
A Measure that reports several rates can give a single group its own scoring method with the ``cqfm-scoring`` (US realm) or ``cqm-scoring`` (UV realm) extension on ``Measure.group``; that value overrides ``Measure.scoring`` for that group only (see :ref:`feature_measure_evaluate_scoring_override`).

Which populations a group may define depends on its scoring method. Table 3-1, "Measure populations based on types of measure scoring", in the Population Criteria section of the `Quality Measure IG <https://hl7.org/fhir/us/cqfmeasures/STU5/measure-conformance.html#population-criteria>`_, marks each population as required, optional or not permitted for each scoring method.
The CodeSystem defines four scoring methods:

#. **Proportion** - "The measure score is defined using a proportion."
   A proportion measure uses the initial population, denominator, denominator exclusion, denominator exception, numerator and numerator exclusion populations.
   The populations are nested: the denominator is a subset of the initial population, and the numerator is a subset of the denominator. Exclusions and exceptions remove cases before the score is calculated.
   The `Proportion Measures <https://hl7.org/fhir/us/cqfmeasures/STU5/measure-conformance.html#proportion-measures>`_ section of the Quality Measure IG defines the score as the performance rate:
   "Performance rate = (Numerator - Numerator Exclusion) / (Denominator – Denominator Exclusion – Denominator Exception)".
   A typical proportion measure answers a question like "Of the adults eligible for a blood pressure check, what fraction had one?".

#. **Ratio** - "The measure score is defined using a ratio."
   A ratio measure uses the initial population, denominator, denominator exclusion, numerator and numerator exclusion populations. A denominator exception is not permitted.
   Unlike a proportion measure, the numerator is not a subset of the denominator: both are derived independently from the initial population.
   The `Ratio Measures <https://hl7.org/fhir/us/cqfmeasures/STU5/measure-conformance.html#ratio-measures>`_ section of the Quality Measure IG defines the numerator as "that subset of the Initial Population that meets the Numerator criteria", and notes that "Some ratio measures will require multiple initial populations, one for the numerator, and one for the denominator."
   An example is the number of central line blood stream infections relative to the number of patients with a central line.

#. **Continuous variable** - "The score is defined by a calculation of some quantity."
   A continuous-variable measure uses the initial population, measure population and measure population exclusion populations, together with a ``measure-observation`` that computes a value for each member of the measure population.
   The score is an aggregate of those observations, for example their median, with the aggregate method specified by the ``cqfm-aggregateMethod`` extension. In the words of the `Continuous Variable Measure <https://hl7.org/fhir/us/cqfmeasures/STU5/measure-conformance.html#continuous-variable-measure>`_ section of the Quality Measure IG:
   "Rather than reporting a Numerator and Denominator, a Continuous Variable measure defines variables that are computed across the Measure Population (e.g., average wait time in the emergency department)."

#. **Cohort** - "The measure is a cohort definition."
   According to the `Cohort Definitions <https://hl7.org/fhir/us/cqfmeasures/STU5/measure-conformance.html#cohort-definitions>`_ section of the Quality Measure IG, "For cohort definitions, only the Initial Population criteria type is used."
   A cohort measure has no score: its result is the population itself, for example all patients who received an immunization.

The quoted definitions of the scoring methods are those of the measure-scoring CodeSystem in FHIR R4. See also the `Quality Reporting <https://hl7.org/fhir/R4/clinicalreasoning-quality-reporting.html>`_ page of the FHIR R4 Clinical Reasoning module.

Scoring Methods in Firely Server
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Firely Server 6.10.0 supports the following scoring methods:

#. ``proportion`` - supported.
#. ``ratio`` - supported, for a group with a single initial population that feeds both the numerator and the denominator. Measures that require a separate initial population for the numerator and for the denominator are not supported.
#. ``cohort`` - supported. Only the initial population is evaluated, and no ``measureScore`` is reported.
#. ``continuous-variable`` - not supported. A Measure that uses it, in ``Measure.scoring`` or in a group-level override, is rejected with ``422 Unprocessable Entity`` and issue type ``not-supported``. A ``measure-observation`` population is rejected the same way on any Measure, as Firely Server does not evaluate measure observations.

A Measure without a scoring method is still evaluated: its initial population, denominator, numerator, exclusion and exception criteria are executed and reported, but no ``measureScore`` is calculated.

By default, Firely Server counts each population following the label-based membership rules of the Quality Measure IG: a case is only counted for a population when it also belongs to the population it is derived from.
Exclusion and exception cases remain included in the count of their parent population, and are only subtracted when the score is calculated.
As a result, the population counts in the MeasureReport can be entered directly into the performance rate formula above.

For example, suppose the "Blood Pressure Check for Adults" measure also defined a denominator exclusion (patients in hospice care) and a denominator exception (patients who declined the check). Evaluating it for six patients gives:

.. list-table::
   :header-rows: 1

   * - Patient
     - Criteria met
     - Counted in
   * - 1, 2
     - Adult, had a blood pressure reading
     - initial population, denominator, numerator
   * - 3
     - Adult, no blood pressure reading
     - initial population, denominator
   * - 4
     - Adult in hospice care, no blood pressure reading
     - initial population, denominator, denominator exclusion
   * - 5
     - Adult who declined the check, no blood pressure reading
     - initial population, denominator, denominator exception
   * - 6
     - Not an adult
     - none

The resulting summary MeasureReport reports an initial population and a denominator of 5, a denominator exclusion of 1, a denominator exception of 1 and a numerator of 2.
The ``measureScore`` is (2 - 0) / (5 - 1 - 1) = 2/3, reported as ``0.6666666666666666``.
Had patient 5 also had a blood pressure reading, that patient would have been counted in the numerator and not as a denominator exception, because a denominator exception only applies when the numerator criteria are not met.

For the full scoring rules, the formulas Firely Server applies and the validation performed before evaluation, see :ref:`feature_measure_evaluate_scoring`, :ref:`feature_measure_evaluate_counts` and :ref:`feature_measure_evaluate_validation`.

Managing Measures
^^^^^^^^^^^^^^^^^

Measures are treated as administrative resources and can be uploaded to the administration endpoint of Firely Server. See :ref:`administration_api` for more details.

----

FHIR Libraries
--------------

A FHIR Library resource contains one or more representations of the CQL logic that defines the population criteria referenced by the Measure resource. 
In addition to publishing metadata, the Library includes the original CQL content—encoded in base64—within a content element annotated with contentType ``text/cql``.

While CQL is designed to be human-readable and author-friendly, it must be translated into ELM to be machine-readable. 
ELM uses a canonical abstract syntax tree (AST) to represent CQL expressions, decisions, and data references in a structured way. 
This makes it portable and enables any compliant engine to evaluate the logic consistently, regardless of the original authoring tool.

Firely Server internally uses the open-source `.NET CQL SDK <https://github.com/FirelyTeam/firely-cql-sdk>`_ to compile ELM into executable C# code, enabling enhanced debuggability and high-performance execution. 
The resulting .NET assembly (``.dll``) is dynamically loaded at runtime during the execution of operations such as ``Measure/$evaluate-measure`` or ``Library/$evaluate``.

Compiling CQL
^^^^^^^^^^^^^

Uploading a ``Library`` resource that has a ``url``, ``name`` and ``version`` and carries CQL (``text/cql``) and/or ELM (``application/elm+json``) content as inline ``data`` is sufficient: Firely Server compiles it into a .NET assembly itself.
When an operation such as ``Library/$evaluate`` or ``Measure/$evaluate-measure`` resolves a ``Library`` from the administration database, Firely Server compiles it in memory, using the ELM content if the ``Library`` contains it and the CQL content otherwise.
The Libraries it depends on (``relatedArtifact`` of type ``depends-on``) are resolved from the administration database and compiled as well, so they must be uploaded too.
The compiled assembly is only kept in the cache of conformance resources, it is not written back to the administration database. Once the cache entry has expired or has been evicted, the ``Library`` is compiled again on its next use; see ``SlidingExpirationSeconds`` in :ref:`configure_cache`.
If the compilation fails, the operation responds with ``422 Unprocessable Entity``, reporting that no .NET dll was found in the ``Library``; the cause of a parse, translation or compilation error is written to the Firely Server log. A ``depends-on`` Library that cannot be resolved is reported with ``404 Not Found``, and a ``Library`` without ``url``, ``name`` or ``version`` with ``422 Unprocessable Entity`` naming the missing elements.

Firely Server only skips its own compilation for a precompiled ``Library``: one of type ``logic-library`` (CodeSystem ``http://terminology.hl7.org/CodeSystem/library-type``) with a ``content`` element whose element id contains ``+dll``, like the ``BloodPressureCheckLogic-1.0.0+dll`` content in the :ref:`feature_qdm_example_library` below. The assembly in that element is then used as-is; it must have contentType ``application/octet-stream``, otherwise it cannot be loaded and the operation responds with ``422 Unprocessable Entity``. A ``Library`` passed inline through the ``library`` parameter of ``Library/$evaluate`` is always compiled.

Precompiling a ``Library`` with the `.NET CQL SDK <https://github.com/FirelyTeam/firely-cql-sdk>`_ is optional. It is useful to:

* include debug symbols (``+pdb`` content), which the compilation by Firely Server does not produce, see `Debuging Libraries`_;
* avoid the compilation by Firely Server when a ``Library`` stored in the administration database is first used, and again after its cache entry has expired.

To precompile, download the SDK and open the solution file ``Cql-Sdk-All.sln`` in your development environment.

.. note::

  The process can also be used to generate a FHIR Library resource directly from a CQL library. This is particularly useful when extending official CQL-based libraries, such as those used for HEDIS certification or CMS eCQMs. These libraries can be customized to include additional business-critical population criteria.
  Moreover, extra expressions can be added for debugging purposes—for example, to inspect intermediate results during evaluation.

Within the solution, the ``PackageCLI cql (Demo CQL->FHIR)`` launch profile of the ``PackagerCLI`` project provides a demo of the packaging workflow. Any CQL files placed in the folder:

::

  LibrarySets/Demo/Cql

will be automatically compiled to ELM and C# during the build process. The resulting FHIR ``Library`` resources will be generated in:

::

  LibrarySets/Demo/Resources

Alternatively, you can perform the compilation and packaging process via command line using the ``Hl7.Cql.Packager`` tool:

::

  Hl7.Cql.Packager cql \
    --cql <path to project>/LibrarySets/Demo/Cql \
    --fhir <path to project>/LibrarySets/Demo/Resources \
    --dll <path to project>/LibrarySets/Demo/Assemblies \
    --cs <path to project>/Demo/Measures.Demo/CSharp

Please make sure to adjust ``<path to project>`` according to your local environment.
This process generates the ELM, the C# source code and the DLL, and adds them as content to the generated FHIR ``Library`` resources.


When generating ``Library`` resources, the Packager assigns a root URL to build the canonical URL of each library. Configure it with the ``Packaging:CanonicalRootUrl`` setting in ``Hl7.Cql.Packager.appsettings.json`` (default ``https://fire.ly/fhir/``), or with the ``--canonical-root-url`` command-line option.
For external libraries, it may not be appropriate to apply that root URL. In such cases, use the ``Packaging:FixedLibraryCanonicals`` setting to map CQL library names to their intended canonical URLs; by default it maps ``FHIRHelpers`` to ``http://hl7.org/fhir/uv/cql/Library/FHIRHelpers``.

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

	Firely Server 6.10.0 ships with version v2.15.0 of the .NET CQL SDK (packages ``Hl7.Cql.Fhir``, ``Hl7.Cql.Invocation`` and ``Hl7.Cql.Packaging``), which it also uses for its own compilation.
	A precompiled ``Library`` must be built with the CQL SDK version of the Firely Server release it runs on, so use v2.15.0 for Firely Server 6.10.0.

.. _feature_qdm_example_library:

Example Library
^^^^^^^^^^^^^^^

The following is a FHIR `Library` resource defining the CQL logic used in the Blood Pressure Check for Adults measure:

.. code-block:: json
   :caption: FHIR Library – Blood Pressure Check Logic
   :name: bp-check-library

   {
     "resourceType": "Library",
     "id": "76da88af-blood-pressure-check-logic-1.0.0",
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

.. code-block:: text
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



Managing Libraries
^^^^^^^^^^^^^^^^^^

Libraries are treated as administrative resources and can be uploaded to the administration endpoint of Firely Server. See :ref:`administration_api` for more details.
Libraries that contain dQM content are annotated with extensions whose corresponding ``StructureDefinition`` resources are not loaded into Firely Server by default.
These extensions must be added manually to the administration database:

* ``http://hl7.org/fhir/StructureDefinition/cqf-cqlType`` - `Download here <https://simplifier.net/packages/hl7.fhir.uv.extensions.r4/5.2.0/files/2729975>`_

  This extension maps FHIR parameter value types to CQL types for Library input parameters. Firely Server uses it to determine the FHIR type representation of calculation results in the Parameters resource.

* ``http://hl7.org/fhir/StructureDefinition/cqf-cqlOptions`` - `Download here <https://simplifier.net/packages/hl7.fhir.uv.extensions.r4/5.2.0/files/2729842>`_

  This extension documents the detailed options and parameters used when
  translating the Library’s underlying CQL into ELM. Note that these values exist for documentation purposes only, they do not influence the execution of the dQM content.

* ``http://hl7.org/fhir/StructureDefinition/cqf-cqlAccessModifier`` - `Download here <https://simplifier.net/packages/hl7.fhir.uv.extensions.r4/5.2.0/files/2729900>`_

  This extension indicates if a named expression is classified as "private" in CQL. Note that Firely Server currently does not check this extension when executing Library/$evaluate.

.. attention::

  The StructureDefinitions of the extensions listed above also reference ValueSets. These dependencies must be uploaded as well.

Debuging Libraries
^^^^^^^^^^^^^^^^^^

In certain scenarios, especially when the results of a CQL evaluation are unexpected or unclear, deeper insights into the runtime behavior are essential. 
To facilitate troubleshooting and introspection, the Firely Server dQM engine is built with debuggability as a core feature.

The engine compiles CQL expressions into .NET code, which is executed at runtime. 
For advanced debugging scenarios, such as stepping through the compiled logic or inspecting intermediate values, it is possible to examine this generated .NET code line by line using standard debugging tools (e.g., Visual Studio or JetBrains Rider).

To enable this behavior, the executed CQL library must include debug symbols, typically in the form of `.pdb` (Program Database) files. 
These symbols map the compiled code back to the original CQL expressions and are crucial for enabling breakpoints, call stacks, and other debugging features.
Debug symbols can be generated using the CQL .NET SDK, which supports emitting `.pdb` content alongside the compiled logic library content. This debug information is embedded inthe corresponding FHIR `Library` resource.
Firely Server does not produce debug symbols when it compiles a ``Library`` itself, so stepping through the code requires a precompiled ``Library`` that carries ``+pdb`` content.

Debug symbols can be generated by passing the appropriate parameters to the CQL .NET SDK during compilation, as shown below:

::

  Hl7.Cql.Packager cql \
    --cql <path to project>/LibrarySets/Demo/Cql \
    --fhir <path to project>/LibrarySets/Demo/Resources \
    --dll <path to project>/LibrarySets/Demo/Assemblies \
    --pdb <path to project>/LibrarySets/Demo/DebugSymbols \
    --debug-symbols PortablePdb \
    --cs <path to project>/Demo/Measures.Demo/CSharp

The resulting FHIR ``Library`` resource should include a new ``content`` element with the element ID ``{libraryIdentifier}+pdb``, which contains the corresponding debug symbols.
If debug symbols are provided, Firely Server will automatically load them at runtime. 
To step through the source code, open the generated C# file from the CSharp output folder in your IDE and set a breakpoint. 
When a debugger is attached and a relevant CQL expression is invoked, the IDE will enter debug mode and pause at the specified breakpoint.

.. warning::
  Visual Studio uses the original source paths embedded in the PDB file to locate source code. If you open the generated C# file from a different location than where it was originally compiled, the debugger may not correctly associate the code with the symbols. As a result, breakpoints may appear as unbound or fail to hit.

----

FHIR MeasureReports
-------------------

Understanding Population results
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
A ``MeasureReport`` represents the outcome of evaluating a FHIR ``Measure`` against clinical data. 
It contains the computed results for each population defined in the Measure, such as the initial population, denominator, numerator, exclusions, and exceptions.

Each population in the Measure that has an ``id`` is reflected in the MeasureReport under the corresponding ``group.population`` entries, with the same ``id`` and ``code``, in the order of the Measure.


The Anatomy of a MeasureReport
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
A MeasureReport is composed of several key elements that describe what was evaluated, over which data, and what the results were.

- ``type``  
  Indicates the scope of the report.

  The value of this field depends on the ``reportType`` parameter used when calling ``Measure/$evaluate-measure``:

  - If ``reportType`` is set to ``individual`` or ``subject``:  
    The resulting MeasureReport will have ``type = individual``, representing evaluation for a single subject.

  - If ``reportType`` is set to ``population`` or ``summary``:  
    The resulting MeasureReport will have ``type = summary``, representing aggregated results across multiple subjects.

  Note that ``population`` is not a valid value for ``MeasureReport.type`` in FHIR. 
  Aggregated results are always represented using ``summary``.

- ``subject``  
  Identifies the focus of the report.  
  This can reference:

  - A ``Patient`` (for individual evaluation)  
  - A ``Group`` (for population-level evaluation)

- ``period``  
  The measurement period used during evaluation.  

  This is determined by:

  - The input parameters (``periodStart`` / ``periodEnd``) provided to ``Measure/$evaluate-measure``  
  - Or, if not provided, the ``effectivePeriod`` defined in the ``Measure`` resource (if present)

  The report echoes the supplied ``periodStart`` and ``periodEnd`` values as given (for example ``2025-01-01``), not the date-time bounds of the measurement period derived from them.

  The measurement period is passed into the underlying CQL logic as a parameter (named ``Measurement Period``), which is used in the evaluation expressions.

- ``date``  
  The timestamp indicating when the MeasureReport was generated.  
  This reflects when the evaluation was performed, not the measurement period itself.

- ``group``  
  Contains the results of the evaluation. Each group represents a complete set of population calculations that belong together.

Understanding Groups
^^^^^^^^^^^^^^^^^^^^

A ``group`` in a MeasureReport represents a single set of population calculations that are evaluated together as part of a measure.

Each group contains:

- A set of related populations (e.g. initial population, denominator, numerator)
- The computed results (counts) for those populations

Conceptually, a group can be understood as:

  "One complete measure calculation with its own numerator, denominator, and logic"

In most simple measures, there is only one group, representing a single calculation.  
However, more complex measures may define multiple groups, for example:

- Different patient cohorts (e.g. age groups or clinical conditions)
- Multiple related quality metrics within one Measure
- Parallel or stratified evaluations using different criteria

Each group is evaluated independently, and its results are reported separately in the MeasureReport.

The ``group.id`` links the results in the MeasureReport back to the corresponding group definition in the Measure resource. Therefore it is required in the Measure resource.

Population Basis
^^^^^^^^^^^^^^^^

The population basis defines what type of elements are returned by the population criteria and, therefore, what is counted in the resulting MeasureReport.

In many quality measures, the counted elements are the same as the measure subject.  
For example, if:

- ``subject[x] = Patient``
- ``populationBasis = boolean``

then each population criterion evaluates to either ``true`` or ``false`` for each patient:

- ``true`` → the patient belongs to the population
- ``false`` → the patient does not belong to the population

In this model, the MeasureReport counts patients.

Measures are not limited to counting the measure subject itself.  
A measure may instead count related clinical resources.

For example, an encounter-based measure may define:

- ``subject[x] = Patient``
- ``populationBasis = Encounter``

In this case, the population criteria return lists of ``Encounter`` resources rather than boolean values.  
The MeasureReport therefore counts encounters instead of patients, meaning that a single patient may contribute multiple matching encounters.

The population basis defines the expected result type of all population criteria within the measure.  
All population-level criteria are expected to return values consistent with the configured population basis.

Interpreting Population Counts
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Each population within a group includes a ``count`` value representing the evaluation result.

The meaning of this value depends on the report type:

- **Individual reports**  
  Each population count is typically:
  
  - ``1`` - the subject meets the criteria  
  - ``0`` - the subject does not meet the criteria  

- **Population or summary reports**  
  Counts represent the total number of subjects in each population group.

For example:

- Denominator = 100 → 100 subjects were eligible  
- Numerator = 75 → 75 subjects met the measure criteria  

This eventually allows calculation of performance rates (e.g. 75%).

.. _feature_dqm_stratification:

Stratification
^^^^^^^^^^^^^^

A stratifier breaks the results of a group down by a characteristic of its members, such as age group, gender or insurance product line.
Each distinct value of that characteristic defines a stratum, and every stratum reports its own population counts and, where the scoring method has one, its own score.
This is different from defining several groups: a group is a separate calculation with its own population criteria, while the strata of a stratifier are views on the populations of one group.
According to the `Quality Reporting <https://hl7.org/fhir/R4/clinicalreasoning-quality-reporting.html>`_ page of the FHIR R4 Clinical Reasoning module, stratifiers are "Additional criteria used to calculate the measure along different dimensions within the population such as age or gender. A measure may define any number of stratifiers for each population group."

A stratifier is defined in ``Measure.group.stratifier``, in one of two forms:

#. **A single criteria**: ``stratifier.criteria`` holds one expression, and the stratum is determined by its result.
#. **Components**: ``stratifier.component[]`` holds several expressions, each with its own ``code`` and ``criteria``. The stratum is the combination of the component results, so a stratifier with an age group and a product line component has a stratum for each combination of age group and product line.

The Stratification section of the `Quality Measure IG <https://hl7.org/fhir/us/cqfmeasures/STU5/measure-conformance.html#stratification>`_ (Conformance Requirement 3.17, "Stratification Criteria", in v5.0.0) allows a stratifier expression to return one of two things:
"the same type as other population criteria expressions in the measure (i.e. the population basis), or the stratum value".
In the first approach the expression selects the members of the stratum, just like a population criterion does. In the second approach the expression returns a value, such as ``Patient.gender``, and all members with the same value share a stratum.
For components, the IG states: "If component stratifiers are used and the component expressions return the stratum value, the combination of the component values are considered the stratum value."

In a MeasureReport, each stratifier of a group is reported in ``MeasureReport.group.stratifier``, and each of its strata in ``group.stratifier.stratum``:

- ``stratum.value`` identifies the stratum of a single-criteria stratifier; ``stratum.component[]`` holds the ``code`` and ``value`` of each component of a component stratifier.
- ``stratum.population[]`` holds the counts of the group's populations, restricted to the members of the stratum.
- ``stratum.measureScore`` holds the score calculated over the members of the stratum.

Stratification in Firely Server
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Firely Server 6.10.0 supports component stratifiers whose expressions return the stratum value:

#. A stratifier defined with ``stratifier.component[]`` is supported on groups with a ``boolean`` population basis, which count subjects such as patients rather than events such as encounters.
   A component expression must return values, such as codes, codings, strings or numbers, not resources.
#. A stratifier defined with ``stratifier.criteria`` is not supported. ``Measure/$evaluate-measure`` responds with ``501 Not Implemented``.
#. A ratio-scored group cannot carry stratifiers. Conformance Requirement 15 (Stratification Criteria), in section 3.4.8 Stratification of the `Quality Measure IG STU1 <https://hl7.org/fhir/us/cqfmeasures/STU1/measure-conformance.html#stratification>`_, states that "Stratification SHALL NOT be used with ratio measures, since ratio measures may define multiple initial populations." Such a Measure is rejected with ``422 Unprocessable Entity``.

A component expression may return more than one value for a patient, for example one product line for each coverage the patient holds.
The patient then belongs to one stratum for each combination of values, and is counted in each of them.
As a result, the counts of the strata of a stratifier can add up to more than the counts of the group itself.

For example, the "Blood Pressure Check for Adults" measure could be stratified by product line and age group, using two CQL expressions: ``Product Line``, which returns the ``Coding`` of the type of each of the patient's coverages, and ``Age Group``, which returns a string such as ``'18-44'`` or ``'45+'``.
The group of the Measure then declares a ``boolean`` population basis and the following stratifier, next to the populations shown in :ref:`bp-measure-json`:

.. code-block:: json
   :caption: Measure.group with a component stratifier
   :name: bp-measure-stratifier-json

   {
     "id": "9a3f3b12-4e7d-4cf2-8e6a-729e5a21f4b9",
     "extension": [
       {
         "url": "http://hl7.org/fhir/us/cqfmeasures/StructureDefinition/cqfm-populationBasis",
         "valueCode": "boolean"
       }
     ],
     "stratifier": [
       {
         "id": "product-line-age-group",
         "code": {
           "text": "Product line and age group"
         },
         "component": [
           {
             "id": "product-line",
             "code": {
               "text": "Product line"
             },
             "criteria": {
               "language": "text/cql-identifier",
               "expression": "Product Line"
             }
           },
           {
             "id": "age-group",
             "code": {
               "text": "Age group"
             },
             "criteria": {
               "language": "text/cql-identifier",
               "expression": "Age Group"
             }
           }
         ]
       }
     ]
   }

Suppose the measure is evaluated as a summary report for a Group of two patients:

- Patient A is 30 years old, has coverage in two product lines (``PPO`` and ``MCD``) and had a blood pressure reading.
- Patient B is 50 years old, has coverage in the ``PPO`` product line and had no blood pressure reading.

The group reports an initial population and a denominator of 2, a numerator of 1 and a ``measureScore`` of 0.5.
Its stratifier reports three strata: patient A appears in both the (``MCD``, ``18-44``) and the (``PPO``, ``18-44``) stratum, and patient B in the (``PPO``, ``45+``) stratum.
The initial population counts of the strata therefore add up to 3, while the group's initial population is 2.

.. code-block:: json
   :caption: MeasureReport.group.stratifier of a summary report
   :name: bp-measurereport-stratifier-json

   {
     "stratifier": [
       {
         "id": "product-line-age-group",
         "code": [
           {
             "text": "Product line and age group"
           }
         ],
         "stratum": [
           {
             "component": [
               {
                 "code": {
                   "text": "Product line"
                 },
                 "value": {
                   "coding": [
                     {
                       "system": "http://example.org/fhir/CodeSystem/product-line",
                       "code": "MCD"
                     }
                   ],
                   "text": "MCD"
                 }
               },
               {
                 "code": {
                   "text": "Age group"
                 },
                 "value": {
                   "text": "18-44"
                 }
               }
             ],
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
                 "count": 1
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
                 "count": 1
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
                 "count": 1
               }
             ],
             "measureScore": {
               "value": 1,
               "system": "http://unitsofmeasure.org",
               "code": "{score}"
             }
           },
           {
             "component": [
               {
                 "code": {
                   "text": "Product line"
                 },
                 "value": {
                   "coding": [
                     {
                       "system": "http://example.org/fhir/CodeSystem/product-line",
                       "code": "PPO"
                     }
                   ],
                   "text": "PPO"
                 }
               },
               {
                 "code": {
                   "text": "Age group"
                 },
                 "value": {
                   "text": "18-44"
                 }
               }
             ],
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
                 "count": 1
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
                 "count": 1
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
                 "count": 1
               }
             ],
             "measureScore": {
               "value": 1,
               "system": "http://unitsofmeasure.org",
               "code": "{score}"
             }
           },
           {
             "component": [
               {
                 "code": {
                   "text": "Product line"
                 },
                 "value": {
                   "coding": [
                     {
                       "system": "http://example.org/fhir/CodeSystem/product-line",
                       "code": "PPO"
                     }
                   ],
                   "text": "PPO"
                 }
               },
               {
                 "code": {
                   "text": "Age group"
                 },
                 "value": {
                   "text": "45+"
                 }
               }
             ],
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
                 "count": 1
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
                 "count": 1
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
                 "count": 0
               }
             ],
             "measureScore": {
               "value": 0,
               "system": "http://unitsofmeasure.org",
               "code": "{score}"
             }
           }
         ]
       }
     ]
   }

The report shows how Firely Server represents strata:

- ``stratifier.id`` and ``stratifier.code`` are copied from the stratifier in the Measure.
- ``stratum.component.code`` is copied from the component in the Measure. When the component has no ``code``, Firely Server uses a ``CodeableConcept`` with the component's ``id`` as ``text``.
- A coded component value is reported as a ``CodeableConcept`` with the ``system`` and ``code`` of the coding, and with ``text`` set to the code. A value that is not coded, such as a string, number, date or boolean, is reported with ``text`` only.
- A patient for whom a component expression returns no value is placed in a stratum whose component value only carries the ``data-absent-reason`` extension:

  .. code-block:: json
     :caption: Stratum component without a value

     {
       "code": {
         "text": "Product line"
       },
       "value": {
         "extension": [
           {
             "url": "http://hl7.org/fhir/StructureDefinition/data-absent-reason",
             "valueCode": "unknown"
           }
         ]
       }
     }

- ``stratum.population`` carries the ``code`` and ``count`` of each population of the group, but no ``id``. The counts follow the same rules as the counts of the group, so a stratum never counts a patient that the group does not count.
- ``stratum.measureScore`` is reported for proportion-scored groups and is calculated in the same way as the score of the group.
- An individual report keeps strata whose counts are all 0, so the patient's component values are always visible. Summary and subject-list reports leave out strata that contain no patient of any reported population.

For the full list of rules and the validation of stratifiers, see :ref:`feature_measure_evaluate_stratifiers` and :ref:`feature_measure_evaluate_validation`.

Generating MeasureReports
^^^^^^^^^^^^^^^^^^^^^^^^^

MeasureReports are generated by executing the ``Measure/$evaluate-measure`` operation.
Unlike Measures and Libraries, MeasureReports are stored on the **FHIR data endpoint**, not the administration endpoint.

Example MeasureReports
^^^^^^^^^^^^^^^^^^^^^^

The following example shows a FHIR ``MeasureReport`` resource representing the individual evaluation of a single patient against the "Blood Pressure Check for Adults" measure, where the patient meets all three population criteria.
It is the response to a ``POST`` to ``Measure/$evaluate-measure`` with the parameters ``url``, ``periodStart``, ``periodEnd`` and ``subject`` and without ``persist``, so the report is not stored and carries no ``meta``. The contained ``Parameters`` resource holds the input parameters of the request and is referenced by the ``cqfm-inputParameters`` extension.

.. code-block:: json
   :caption: FHIR MeasureReport – Individual Result
   :name: bp-check-measurereport

   {
     "resourceType": "MeasureReport",
     "id": "bc23af57-f8a4-408b-9149-f91b4092e6dc",
     "contained": [
       {
         "resourceType": "Parameters",
         "id": "3f6c2a9e-8d41-4b7a-9c0e-51d2f7a8b6c3",
         "parameter": [
           {
             "name": "url",
             "valueCanonical": "http://example.org/fhir/Measure/bp-check-adults"
           },
           {
             "name": "periodStart",
             "valueDate": "2025-01-01"
           },
           {
             "name": "periodEnd",
             "valueDate": "2025-12-31"
           },
           {
             "name": "subject",
             "valueString": "Patient/test"
           }
         ]
       }
     ],
     "extension": [
       {
         "url": "http://hl7.org/fhir/us/cqfmeasures/StructureDefinition/cqfm-inputParameters",
         "valueReference": {
           "reference": "#3f6c2a9e-8d41-4b7a-9c0e-51d2f7a8b6c3"
         }
       }
     ],
     "status": "complete",
     "type": "individual",
     "measure": "http://example.org/fhir/Measure/bp-check-adults|1.0.0",
     "subject": {
       "reference": "http://localhost:4080/Patient/test"
     },
     "date": "2026-01-15T10:24:37.5306219+00:00",
     "period": {
       "start": "2025-01-01",
       "end": "2025-12-31"
     },
     "group": [
       {
         "id": "9a3f3b12-4e7d-4cf2-8e6a-729e5a21f4b9",
         "population": [
           {
             "id": "initial-population",
             "code": {
               "coding": [
                 {
                   "system": "http://terminology.hl7.org/CodeSystem/measure-population",
                   "code": "initial-population"
                 }
               ]
             },
             "count": 1
           },
           {
             "id": "denominator",
             "code": {
               "coding": [
                 {
                   "system": "http://terminology.hl7.org/CodeSystem/measure-population",
                   "code": "denominator"
                 }
               ]
             },
             "count": 1
           },
           {
             "id": "numerator",
             "code": {
               "coding": [
                 {
                   "system": "http://terminology.hl7.org/CodeSystem/measure-population",
                   "code": "numerator"
                 }
               ]
             },
             "count": 1
           }
         ],
         "measureScore": {
           "value": 1,
           "system": "http://unitsofmeasure.org",
           "code": "{score}"
         }
       }
     ]
   }
