.. _plugin_binarywrapper:

Binary Handling
===============

.. note::

  The features described on this page are available in **all** :ref:`Firely Server editions <vonk_overview>`.

Description
-----------
A Binary resource is a digital representation of a single raw artifact that can be accessed in its original format. 
It can contain various types of content such as text, images, PDFs, zip archives, and more.

The Binary Wrapper plugin, as part of Firely Server, facilitates the sending of binary content to Firely Server, where it is stored as a Binary resource. 
Additionally, it enables retrieving a Binary resource and returning it in its original binary format. 
Essentially, the Binary Wrapper plugin simplifies the process of converting raw artifacts to base64 and manually creating Binary resources.

This plugin is particularly beneficial for workflows designed to support :ref:`170.315 (b)(10) Electronic Health Information Export <compliance_b_10>`.

The contents of the Binary resource are Base64 encoded and stored within the Firely Server database. 
Therefore, it is important to note that this plugin is suitable for handling small binary files.

Sending binary content example request::

   POST <base-url>/Binary
   Content-Type = application/pdf
   Accept = application/fhir+json; fhirVersion=4.0
   Body: enclose a file with the actual contents

* This can also be done with a PUT: ``PUT <base-url>/Binary/example``
* The Content-Type must be the mediatype of the actual contents. It will only be accepted by Firely Server if it is one of the mediatypes listed in the ``RestrictToMimeTypes`` below.
* The Accept header can be either a fhir mediatype, with any of the supported FHIR versions. You could also set it equal to the Content-Type in which case you will be returned the same contents again.

Getting binary content example request::

   GET <base-url>/Binary/example
   Accept = application/pdf; fhirVersion=4.0

* The Accept header should match the mediatype of the actual contents. If you don't know the mediatype, you could request the binary resource in FHIR format first and examine its ``contentType`` element.

Configuration
-------------

::

   "Vonk.Plugin.BinaryWrapper":{
      "RestrictToMimeTypes": ["application/pdf", "text/plain", "image/png", "image/jpeg"]
   },
   "SizeLimits": {
      "MaxResourceSize": "1MiB", // b/kB/KiB/Mb/MiB, no spaces
   },
   "PipelineOptions": {
      "Branches": [
         {
            "Path": "/",
            "Include": [
               ...
               "Vonk.Plugin.BinaryWrapper"
            ]
         },
         ...
      ]
   }

* ``RestrictToMimeTypes`` protects Firely Server from arbitrary content.
* This plugin honours the Firely Server setting for maximum resource size. This protects Firely Server from binary contents that are too large to store in the database.
* The namespace ``Vonk.Plugin.BinaryWrapper`` configures both encoding and decoding of binary contents. You can configure them separately as well::

   "PipelineOptions": {
      "Branches": [
         {
            "Path": "/",
            "Include": [
               ...
               "Vonk.Plugin.BinaryWrapper.BinaryEncodeConfiguration",
               "Vonk.Plugin.BinaryWrapper.BinaryDecodeConfiguration"
            ]
         },
         ...
      ]
   }
* Compatible with Firely Server (Vonk) 2.1.0
* Introduces the encoding of Binary resources, so you can POST binary contents and have it stored as a Binary resource.
