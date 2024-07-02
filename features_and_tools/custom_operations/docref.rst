.. _feature_docref:

Fetch DocumentReference - $docref
=================================

.. note::

  The features described on this page are available in the following :ref:`Firely Server editions <vonk_overview>`:

  * Firely Scale - 🌍 / 🇺🇸
  * Firely CMS Compliance - 🇺🇸

The $docref operations allows a client to search for DocumentReference resources representing documents relating to a patient.
$docref is being implemented as defined in the `US Core <https://hl7.org/fhir/us/core/OperationDefinition-docref.html>`_ and `International Patient Access <https://build.fhir.org/ig/HL7/fhir-ipa/OperationDefinition-docref.html>`_ ImplementationGuide.

Currently the following limitations exist:

#. The ``on-demand`` parameter is not supported, $docref will only operate on already existing DocumentReference resources
#. The ``profile`` parameter is not supported

Configuration
-------------

To include the plugin in your pipeline, add the following extra Include:

.. code-block:: JavaScript

   "PipelineOptions": {
      "Branches": [
         {
            "Path": "/",
            "Include": [
               ...
               "Vonk.Plugin.DocRefOperation"
            ]
         },
         ...
      ]
   }
