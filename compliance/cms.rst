.. _cms:

Compliance with CMS Advancing Interoperability and Improving Prior Authorization Processes - 🇺🇸
===============================================================================================

The CMS Interoperability Rule aims to promote greater interoperability, patient access, and innovation in the healthcare industry, while also improving the quality and cost-effectiveness of care. Technically these goals are supported by multiple APIs that are required to be provided:

* Patient Access API
* Provider Access API
* Provider Directory API
* Payor-to-Payor API
* Prior Authorization Requirements, Documentation and Decision (PARDD) 

Firely Server aims to support all mandatory requirements out-of-the-box. The following ImplementationsGuides built the foundation of the APIs mentioned above.

.. list-table:: Firely Server CMS Interoperability Mandatory IGs
   :widths: 10, 10, 10, 10, 10
   :header-rows: 1
   
   * - API
     - FHIR v4.0.1
     - :ref:`US Core<compliance_g_10>`
     - :ref:`SMART App Launch<feature_accesscontrol>`
     - :ref:`Bulk Data Access<feature_bulkdataexport>`

   * - * Patient Access API
     
     - X
     - X
     - X
     - Not needed
     
   * - * Provider Access API
   
     - X
     - X
     - X
     - X
     
   * - * Provider Directory API
   
     - X
     - X
     - X
     - Not needed
     
   * - * Payor-to-Payor API
   
     - X
     - X
     - X
     - X
     
   * - * PARDD API
   
     - X
     - X
     - X
     - Not needed
