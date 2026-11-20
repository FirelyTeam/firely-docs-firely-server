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

Healthcare runs on forms: intake questionnaires, screening instruments, assessments, and the documentation a payer asks for before it approves a service.
In FHIR, the definition of a form is a ``Questionnaire``, and a filled-in form is a ``QuestionnaireResponse``.
The answers in a ``QuestionnaireResponse`` are useful to the person who reads the form, but other systems cannot use them directly. A decision support rule, a quality measure or a search for all patients with a high blood pressure looks for an ``Observation`` or a ``Condition``, not for answer 3 of question 7.

The `HL7 Structured Data Capture (SDC) implementation guide <https://hl7.org/fhir/uv/sdc/>`_ closes that gap. It extends ``Questionnaire`` with the information a system needs to work with a form: how to fill in answers from the data that is already known, how to calculate and check answers while the form is filled in, and how to turn the answers into the FHIR resources they describe.
Firely Server implements the SDC operations on the server side, so a form application does not have to implement this logic itself, and the Questionnaires are kept and versioned in one place.

----

SDC Use Cases
-------------

#. Turning answers into clinical data

   A form collects a blood pressure, a smoking status or a list of allergies. Data extraction turns these answers into ``Observation``, ``Condition`` or ``AllergyIntolerance`` resources, so they can be stored next to the data from other sources, and searched and reported on in the same way.

#. Forms that are already partly filled in

   Many answers are already known: the patient's name and date of birth, current medications, or the latest lab results. Pre-population fills in these answers from the patient's record before the form is shown, so the user only checks and completes it. This saves time and avoids typing errors.

#. Documentation for prior authorization

   In the US, the Da Vinci `Documentation Templates and Rules (DTR) <https://hl7.org/fhir/us/davinci-dtr/>`_ implementation guide uses SDC Questionnaires to collect the documentation a payer needs for a prior authorization. The payer provides the Questionnaire, the provider's system fills it in as far as it can from the patient's record, and the provider completes it. See :ref:`questionnaire-package`.

#. Data for quality measures

   Some data that a :ref:`digital quality measure <feature_qdm>` needs is not recorded anywhere else, such as a patient-reported outcome. A form captures it, and data extraction stores it in the FHIR resources that the measure logic reads.

#. One definition of a form for many applications

   When the Questionnaire is stored on the server and the server does the extraction, every application that shows the form produces the same resources from it. A change to the form or to its extraction rules is made once, in the Questionnaire.

How forms are represented in FHIR
---------------------------------

#. Questionnaire

   A ``Questionnaire`` defines a form: its items (questions and groups), the type of answer each item takes, the allowed answers, and when an item is shown. SDC adds extensions to a Questionnaire for pre-population, calculated answers, and data extraction. In Firely Server, a Questionnaire is a conformance resource, stored in the :ref:`Administration database <conformance>`.

#. QuestionnaireResponse

   A ``QuestionnaireResponse`` holds the answers to a Questionnaire, in the same structure of items. Its ``questionnaire`` element refers to the Questionnaire it answers, by canonical url and optionally a version, for example ``http://example.org/Questionnaire/intake|1.0.0``. A QuestionnaireResponse is stored with the patient data.

#. Extraction templates and expressions

   The data extraction extensions of SDC describe which resources to create from the answers. With template-based extraction, the Questionnaire contains example resources, the *templates*, with FHIRPath expressions that say which answer goes into which element.

For the full background, see the `SDC implementation guide <https://hl7.org/fhir/uv/sdc/>`_, and in particular its `Form Data Extraction <https://hl7.org/fhir/uv/sdc/extraction.html>`_ and `Form Population <https://hl7.org/fhir/uv/sdc/populate.html>`_ pages.

----

SDC Operations in Firely Server
-------------------------------

SDC defines several operations. The list below shows which ones Firely Server supports, and when to use each.

Supported
^^^^^^^^^

* QuestionnaireResponse/$extract

   :Purpose: Turns the answers in a QuestionnaireResponse into the FHIR resources that its Questionnaire describes.
   :Use Case: After a form is completed, to store the answers as clinical data, such as Observations and Conditions.
   :Input: A QuestionnaireResponse, in the request or stored on the server, and optionally the Questionnaire it answers.
   :Output: A transaction Bundle with the extracted resources. Firely Server does not store them: send the Bundle as a transaction to store them.

   See `SDC - OperationDefinition QuestionnaireResponse/$extract <https://hl7.org/fhir/uv/sdc/OperationDefinition-QuestionnaireResponse-extract.html>`_ for the HL7 specification, and :ref:`feature_sdc_extract` for details on how to execute this operation in Firely Server, including which parameters and extraction mechanisms are supported.

* Questionnaire/$questionnaire-package

   :Purpose: Returns a Questionnaire together with the resources needed to fill it in, such as the ValueSets and CQL Libraries it uses.
   :Use Case: Da Vinci DTR, where a provider's system fetches the documentation template of a payer.
   :Input: The patient's ``Coverage``, and the canonical urls of one or more Questionnaires.
   :Output: For each Questionnaire, a Bundle with the Questionnaire, the CQL Libraries it depends on and the ValueSets of its answers.

   This operation is defined by Da Vinci DTR rather than by SDC. See :ref:`questionnaire-package`.

Planned
^^^^^^^

* Questionnaire/$populate

   :Purpose: Fills in a QuestionnaireResponse with the answers that are already known, before the form is shown to the user.
   :Use Case: Pre-populating a form from the patient's record.
   :Input: A Questionnaire, and the patient (or other subject) the form is about.
   :Output: A partly filled in QuestionnaireResponse.

   ``$populate`` is planned for a later release of Firely Server. See `SDC - OperationDefinition Questionnaire/$populate <https://hl7.org/fhir/uv/sdc/OperationDefinition-Questionnaire-populate.html>`_ for the HL7 specification.

Other SDC operations, such as ``Questionnaire/$assemble`` for modular forms and ``$next-question`` for adaptive forms, are not supported.

Enabling Structured Data Capture in Firely Server
--------------------------------------------------

Structured Data Capture is not enabled by default. Enabling it takes three steps.

First, add the Structured Data Capture plugin to the pipeline. It ships with Firely Server, but it is commented out in the default ``PipelineOptions``. Add ``Vonk.Plugin.StructuredDataCapture.StructuredDataCaptureConfiguration`` to the ``Include`` list of the pipeline branch that serves your FHIR data:

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

The plugin checks the answers against the Questionnaire with the validation plugin, so validation must be in the same branch. Validation is in the default pipeline. If the validation plugin is missing, or enterprise validation is not licensed, Firely Server does not start, and the log says which one is missing.

Second, make sure the operations are enabled in the ``Operations`` section. ``$extract`` is enabled there by default, on type and instance level. See :ref:`feature_sdc_extract_configuration`.

Third, load the definitions that your Questionnaires use into the :ref:`Administration database <conformance>`:

* The Questionnaires themselves, unless the client sends the Questionnaire with every request.
* The StructureDefinitions of the SDC extensions that the Questionnaires use. The SDC implementation guide does not ship with Firely Server. Firely Server validates the Questionnaire and the QuestionnaireResponse at the ``Full`` level, and at that level an extension whose definition the server cannot find is an error. The easiest way is to import the ``hl7.fhir.uv.sdc`` package (see :ref:`conformance_import`).
