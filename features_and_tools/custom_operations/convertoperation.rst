.. _feature_convertoperation:

Convert XML <-> JSON - $convert
===============================

.. note::

  The features described on this page are available in **all** :ref:`Firely Server editions <vonk_overview>`.

Description
-----------

Enables you to convert between json and xml representation of a resource using the $convert operation. It can not convert between different FHIR versions.

Convert example request::

   POST <base-url>/$convert
   Content-Type = application/fhir+json; fhirVersion=4.0
   Accept = application/fhir+xml; fhirVersion=4.0
   Body: a resource in JSON format

* This can also be done reversely, with a body in XML format

Configuration
-------------

::

   "PipelineOptions": {
      "Branches": [
         {
            "Path": "/",
            "Include": [
               ...
               "Vonk.Plugin.ConvertOperation"
            ]
         },
         ...
      ]
   }