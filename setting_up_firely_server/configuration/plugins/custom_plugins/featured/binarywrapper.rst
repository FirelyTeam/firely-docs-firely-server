.. _plugin_binarywrapper:

BinaryWrapper plugin
====================

Description
-----------
A Binary resource is a digital representation of a single raw artifact that can be accessed in its original format. 
It can contain various types of content such as text, images, PDFs, zip archives, and more.

The Binary Wrapper plugin facilitates the sending of binary content to Firely Server, where it is stored as a Binary resource. 
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

Release notes
-------------

Version 0.6.0
^^^^^^^^^^^^^

* Built against Firely Server (Vonk) 5.7.0.
* Compatible with Firely Server (Vonk) v5.0.0+.
* After a successful read a `Location` header is now returned as well.
* Updated BinaryWrapper plugin to better align with the `FHIR specification <https://hl7.org/fhir/r4/binary.html#rest>`_ when reading a Binary resource in its native form:

   * Implemented `Last-Modified` header to be able to return `Binary.meta.lastUpdated`.
   * Implemented `ETag` header to be able to return `Binary.meta.versionId`.
   * Implemented `X-Security-Header` to allow the security context for a Binary resource to be specified when it is posted or read in native form.

Version 0.5.0
^^^^^^^^^^^^^

* Built against Firely Server (Vonk) 4.1.0
* Compatible with Firely Server (Vonk) v4.1.0
* Functionally equivalent to version 0.4.0

Version 0.4.0
^^^^^^^^^^^^^

* Built against Firely Server (Vonk) 4.0.0
* Compatible with Firely Server (Vonk) v4.0.0
* Functionally equivalent to version 0.3.0

Version 0.3.0
^^^^^^^^^^^^^

* Built against Firely Server (Vonk) 3.2.0
* Compatible with Firely Server (Vonk) 3.2.0, 3.2.1, 3.3.0
* Introduces the decoding of Binary resources, so you can GET a Binary resource in its original binary format.

Version 0.2.0
^^^^^^^^^^^^^

* Build against Firely Server (Vonk) 3.0.0
* Compatible with Firely Server (Vonk) 3.0.0
* Functionally equivalent to version 0.1.0

Version 0.1.0
^^^^^^^^^^^^^

* Build against Firely Server (Vonk) 2.1.0
* Compatible with Firely Server (Vonk) 2.1.0
* Introduces the encoding of Binary resources, so you can POST binary contents and have it stored as a Binary resource.
