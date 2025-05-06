.. _restful_documenthandling:

FHIR Document Handling
======================

.. note::

  The features described on this page are available in **all** :ref:`Firely Server editions <vonk_overview>`.

Firely Server supports submitting `FHIR document bundles <https://www.hl7.org/fhir/documents.html#3.3>`_ to the base endpoint of the server. The current version of Firely Server will only extract the unstructured part of the document, i.e. the narrative of the document bundle. The submission of the document will return a DocumentReference containing an attachment linking to a Binary resource containing the original narrative. Please note that only the top-level narrative will be extracted. No section narrative will be handled. Updates to narratives from documents with the same document identifier will result in an Update interaction on the DocumentReference.

Please make sure that ``Vonk.Plugin.DocumentHandling.DocumentHandlingConfiguration`` is enabled in the pipeline options to use this feature.

Retrieving the DocumentReference
-------------------------------

When a document bundle is submitted to the base endpoint, Firely Server creates a DocumentReference with a predictable ID derived from the document bundle's identifier. To retrieve this DocumentReference afterward:

1. Take the ``identifier`` value from your original document bundle
2. Generate a SHA-512 hash of this identifier value
3. Take the first 64 characters of the hexadecimal representation of this hash
4. Use this value as the ID to retrieve the DocumentReference: ``GET [base]/DocumentReference/[hash-derived-id]``

This deterministic ID generation allows you to find the DocumentReference without having to store the ID returned from the original submission.

Generate a FHIR document - $document
====================================

See :ref:`feature_documentoperation`.
