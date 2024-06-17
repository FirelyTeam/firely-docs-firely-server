.. _restful_documenthandling:

FHIR Document Handling
======================

.. note::

  The features described on this page are available in **all** :ref:`Firely Server editions <vonk_overview>`.

Firely Server supports submitting `FHIR document bundles <https://www.hl7.org/fhir/documents.html#3.3>`_ to the base endpoint of the server. The current version of Firely Server will only extract the unstructured part of the document, i.e. the narrative of the document bundle. The submission of the document will return a DocumentReference containing an attachment linking to a Binary resource containing the original narrative. Please note that only the top-level narrative will be extracted. No section narrative will be handled. Updates to narratives from documents with the same document identifier will result in an Update interaction on the DocumentReference.

Please make sure that ``Vonk.Plugin.DocumentHandling.DocumentHandlingConfiguration`` is enabled in the pipeline options to use this feature.

Generate a FHIR document - $document
====================================

See :ref:`feature_documentoperation`.
