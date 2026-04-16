.. _features_terminology:

Features - Terminology
======================

Firely Server provides built-in terminology services for working with code systems, value sets, and mappings in FHIR-based workflows. 
It supports standard terminologies such as SNOMED CT, LOINC, and ICD-10, as well as concept mappings in accordance with FHIR standards.

Terminology requests are handled through a flexible architecture that combines local and external terminology services. 
Firely Server can transparently route requests to the most appropriate source, allowing seamless integration with remote terminology servers when needed.

For advanced use cases, the platform also supports handling large and complex code systems directly within the server, while maintaining fallback mechanisms to local or remote services when necessary.

.. toctree::
   :maxdepth: 1
   :titlesonly:

   terminology