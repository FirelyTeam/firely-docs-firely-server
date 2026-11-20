.. _feature_sdc:

Intro to Structured Data Capture
================================

.. note::

  The features described on this page are available in the following :ref:`Firely Server editions <vonk_overview>`:

  * Firely Scale - 🌍 / 🇺🇸
  * Firely Prior Authorization - 🇺🇸

.. note::

  Structured Data Capture requires the license token ``http://fire.ly/server/plugins/structured-data-capture`` in the license file.
  It also requires enterprise validation, with the license token ``http://fire.ly/firely-validator``.
  If you do not have these license tokens, please contact `Firely <https://fire.ly/contact>`_.

Healthcare uses many forms: intake questionnaires, screening instruments, assessments, and the documentation that a payer requests before it approves a service.
In FHIR, the definition of a form is a ``Questionnaire``, and a filled-in form is a ``QuestionnaireResponse``.
The answers in a ``QuestionnaireResponse`` are useful to the person who reads the form, but other systems cannot use them directly. A decision support rule, a quality measure or a search for all patients with a high blood pressure looks for an ``Observation`` or a ``Condition``, not for answer 3 of question 7.

The `HL7 Structured Data Capture (SDC) implementation guide <https://hl7.org/fhir/uv/sdc/>`_ solves this. It extends ``Questionnaire`` with the information a system needs to work with a form: how to fill in answers from data that is already known, how to calculate and check answers while the user fills in the form, and how to turn the answers into the FHIR resources they describe.
Firely Server implements the SDC operations for pre-population and data extraction on the server. A form application therefore does not have to implement this logic itself. The Questionnaires are stored and versioned in one place.

----

SDC Use Cases
-------------

#. Turning answers into clinical data

   A form collects a blood pressure, a smoking status or a list of allergies. Data extraction turns these answers into ``Observation``, ``Condition`` or ``AllergyIntolerance`` resources. These resources can be stored with the data from other sources, and searched and reported on in the same way.

#. Forms that are already partly filled in

   Many answers are already known: the patient's name and date of birth, current medications, or the latest lab results. Pre-population fills in these answers from the patient's record before the form is shown. The user only checks and completes the form. This saves time and avoids typing errors.

#. Documentation for prior authorization

   In the US, the Da Vinci `Documentation Templates and Rules (DTR) <https://hl7.org/fhir/us/davinci-dtr/>`_ implementation guide uses SDC Questionnaires to collect the documentation a payer needs for a prior authorization. The payer provides the Questionnaire, the provider's system fills in the answers that it finds in the patient's record, and the provider completes the form. See :ref:`questionnaire-package`.

#. Data for quality measures

   Some data that a :ref:`digital quality measure <feature_qdm>` needs is only recorded in a form, such as a patient-reported outcome. A form captures it, and data extraction stores it in the FHIR resources that the measure logic reads.

#. One definition of a form for many applications

   When the Questionnaire is stored on the server and the server does the extraction, every application that shows the form produces the same resources from it. A change to the form or to its extraction rules is made once, in the Questionnaire.

How forms are represented in FHIR
---------------------------------

#. Questionnaire

   A ``Questionnaire`` defines a form: its items (questions and groups), the type of answer each item takes, the allowed answers, and when an item is shown. SDC adds extensions to a Questionnaire for pre-population, calculated answers, and data extraction. In Firely Server, a Questionnaire is a conformance resource, stored in the :ref:`Administration database <conformance>`. Instance-level ``$populate`` is the exception; see :ref:`feature_sdc_populate`.

#. QuestionnaireResponse

   A ``QuestionnaireResponse`` holds the answers to a Questionnaire, in the same structure of items. Its ``questionnaire`` element refers to the Questionnaire it answers, by canonical url and optionally a version, for example ``http://example.org/Questionnaire/intake|1.0.0``. A QuestionnaireResponse is stored in the database with the FHIR data.

#. Extraction templates and expressions

   The data extraction extensions of SDC describe which resources to create from the answers. With template-based extraction, the Questionnaire contains example resources, the *templates*, with FHIRPath expressions that define which answer goes into which element.

For more information, see the `SDC implementation guide <https://hl7.org/fhir/uv/sdc/>`_, and in particular its `Form Data Extraction <https://hl7.org/fhir/uv/sdc/extraction.html>`_ and `Form Population <https://hl7.org/fhir/uv/sdc/populate.html>`_ pages.

----

SDC Operations in Firely Server
-------------------------------

SDC defines several operations. Firely Server supports the operations below. The list shows when to use each of them.

* Questionnaire/$populate

   :Purpose: Fills in a QuestionnaireResponse with the answers that are already known, before the form is shown to the user.
   :Use Case: Pre-populating a form from the patient's record, so that the user only checks and completes it.
   :Input: The Questionnaire, the patient (or other subject) the form is about, and optionally other resources to use, such as the encounter.
   :Output: A partly filled in QuestionnaireResponse, and the problems found during population.

   See `SDC - OperationDefinition Questionnaire/$populate <https://hl7.org/fhir/uv/sdc/OperationDefinition-Questionnaire-populate.html>`_ for the HL7 specification, and :ref:`feature_sdc_populate` for details on how to execute this operation in Firely Server, including which parameters and population mechanisms are supported.

* QuestionnaireResponse/$extract

   :Purpose: Turns the answers in a QuestionnaireResponse into the FHIR resources that its Questionnaire describes.
   :Use Case: After a form is completed, to store the answers as clinical data, such as Observations and Conditions.
   :Input: A QuestionnaireResponse, in the request or stored on the server, and optionally the Questionnaire it answers.
   :Output: A transaction Bundle with the extracted resources. Firely Server does not store these resources. To store them, send the Bundle as a transaction.

   See `SDC - OperationDefinition QuestionnaireResponse/$extract <https://hl7.org/fhir/uv/sdc/OperationDefinition-QuestionnaireResponse-extract.html>`_ for the HL7 specification, and :ref:`feature_sdc_extract` for details on how to execute this operation in Firely Server, including which parameters and extraction mechanisms are supported.

* Questionnaire/$questionnaire-package

   :Purpose: Returns a Questionnaire together with the resources needed to fill it in, such as the ValueSets and CQL Libraries it uses.
   :Use Case: Da Vinci DTR, where a provider's system fetches the documentation template of a payer.
   :Input: The patient's ``Coverage``, and the canonical urls of one or more Questionnaires.
   :Output: For each Questionnaire, a Bundle with the Questionnaire, the CQL Libraries it depends on and the ValueSets of its answers.

   This operation is defined by Da Vinci DTR, not by SDC. See :ref:`questionnaire-package`.

The other SDC operations are not supported. These include ``Questionnaire/$assemble`` for modular forms, ``$next-question`` for adaptive forms, and ``$populatehtml`` and ``$populatelink``.

Enabling Structured Data Capture in Firely Server
--------------------------------------------------

Structured Data Capture is disabled by default. Enabling it takes three steps.

First, add the Structured Data Capture plugin to the pipeline. The plugin is included in Firely Server, but it is commented out in the default ``PipelineOptions``. Add ``Vonk.Plugin.StructuredDataCapture.StructuredDataCaptureConfiguration`` to the ``Include`` list of the pipeline branch that serves your FHIR data:

.. code-block:: json

   "PipelineOptions": {
     "Branches": [
       {
         "Path": "/",
         "Include": [
           "Vonk.Core",
           "...",
           "Vonk.Plugin.StructuredDataCapture.StructuredDataCaptureConfiguration"
         ]
       }
     ]
   }

The plugin validates Questionnaires and QuestionnaireResponses with the validation plugin, so the validation plugin must be in the same branch. The default pipeline includes the validation plugin. If the validation plugin is missing, or enterprise validation is not licensed, Firely Server does not start. The log states which of the two is missing.

Second, check that the operations are enabled in the ``Operations`` section. ``$populate`` and ``$extract`` are enabled there by default, on type and instance level. See the configuration of :ref:`$populate <feature_sdc_populate_configuration>` and of :ref:`$extract <feature_sdc_extract_configuration>`.

Third, load the definitions that your Questionnaires use into the :ref:`Administration database <conformance>`:

* The Questionnaires themselves, unless the client sends the Questionnaire with every request. ``Questionnaire/[id]/$populate`` is the exception: it reads the Questionnaire from the database with the FHIR data, so a Questionnaire used that way must be stored there as well.
* The StructureDefinitions of the SDC extensions that the Questionnaires use. The SDC implementation guide does not ship with Firely Server. Firely Server validates the Questionnaire and the QuestionnaireResponse at the ``Full`` level. At that level, an extension whose definition Firely Server cannot find is an error. The simplest way to load the definitions is to import the ``hl7.fhir.uv.sdc`` package (see :ref:`conformance_import`).
