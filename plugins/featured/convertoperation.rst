.. _plugin_convertoperatoin:

Convert plugin
==============

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

Release notes
-------------

Version 0.2.0
^^^^^^^^^^^^^

* Build against Firely Server (Vonk) 3.2.0
* Compatible with Firely Server (Vonk) 3.2.0, 3.3.0
* Functionally equivalent to version 0.1.0

Version 0.1.0
^^^^^^^^^^^^^ 

* Build against Firely Server (Vonk) 3.0.0
* Compatible with Firely Server (Vonk) 3.0.0, 3.1.0
* Introduces the implementation of $convert for conversion between json and xml.