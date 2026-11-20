.. _features_structured_data_capture:

Features - Structured Data Capture
==================================

Firely Server supports the HL7 Structured Data Capture (SDC) implementation guide, which defines how forms are represented, filled in and turned into FHIR data.
A form is a FHIR ``Questionnaire``, and the answers to it are a ``QuestionnaireResponse``. SDC adds the parts that make such forms useful in a clinical workflow, such as filling in answers from the data a server already holds, and turning the answers into the FHIR resources they describe.

.. toctree::
   :maxdepth: 1
   :titlesonly:

   sdc
   populate
   extract
