.. _cms:

CMS Advancing Interoperability and Improving Prior Authorization Processes (CMS-0057-P) - 🇺🇸
============================================================================================

The proposed `CMS Interoperability Rule (CMS-0057-P) <https://www.federalregister.gov/documents/2022/12/13/2022-26479/medicare-and-medicaid-programs-patient-protection-and-affordable-care-act-advancing-interoperability>`_ aims to promote greater interoperability, patient access, and innovation in the healthcare industry, while also improving the quality and cost-effectiveness of care. Technically these goals are supported by multiple APIs that are required to be provided:

Firely Server aims to support all mandatory requirements out-of-the-box. The following implementation guides built the foundation of the APIs mentioned above.

.. list-table:: Firely Server Compliance CMS Interoperability Mandatory IGs
   :widths: 10, 10, 10, 10, 10
   :header-rows: 1
   
   * - API
     - FHIR v4.0.1
     - :ref:`us_core_ig`
     - :ref:`smart_app_launch_ig`
     - :ref:`bulk_data_access_ig`

   * - Patient Access API
     
     - ✅ 
     - ✅ 
     - ✅ 
     - Not needed
     
   * - Provider Access API
   
     - ✅ 
     - ✅ 
     - ✅ 
     - ✅ 
     
   * - Provider Directory API
   
     - ✅ 
     - ✅ 
     - ✅ 
     - Not needed
     
   * - Payor-to-Payor API
   
     - ✅ 
     - ✅ 
     - ✅ 
     - ✅ 
     
   * - PARDD API
   
     - ✅ 
     - ✅ 
     - ✅ 
     - Not needed

Patient Access API
------------------

Impacted payers (see `CMS definition <https://www.cms.gov/about-cms/obrhi/interoperability/faqs/patient-access-api#footnote-01>`_) are required to make claims, encounter and clinical data, including laboratory results available through the Patient Access API.
The goal is to to make as much data available to patients as possible through the API to ensure patients have access to their data in a way that will be most valuable and meaningful to them. The following information should be provided via Patient Access API using the corresponding implementation guides:

* Claim details and encounters (see :ref:`carin_ig`)
* Clinical data incl. laboratory data (see :ref:`us_core_ig`)
* Plan Coverage and Formularies (US Drug Formulary)
* Prior Authorization Decisions (Da Vinci Prior Authorization Support)
