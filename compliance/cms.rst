.. _cms:

CMS Advancing Interoperability and Improving Prior Authorization Processes (CMS-0057-P) - 🇺🇸
============================================================================================

The proposed `CMS Interoperability Rule (CMS-0057-P) <https://www.federalregister.gov/documents/2022/12/13/2022-26479/medicare-and-medicaid-programs-patient-protection-and-affordable-care-act-advancing-interoperability>`_ aims to promote greater interoperability, patient access, and innovation in the healthcare industry while also improving the quality and cost-effectiveness of care. Technically these goals are supported by multiple APIs that are required to be provided:

Firely Server aims to support all mandatory requirements out-of-the-box. The following implementation guides build the foundation of the APIs mentioned above.

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

.. note::
  There are additional Implementation Guides strongly recommended by CMS. Not all of them are currently supported by Firely Server. 

Patient Access API
------------------

Impacted payers (see `CMS definition <https://www.cms.gov/about-cms/obrhi/interoperability/faqs/patient-access-api#footnote-01>`_) are required to make claims, encounter and clinical data, including laboratory results available through the Patient Access API.
The goal is to make as much data available to patients as possible through the API to ensure patients have access to their data in a way that will be most valuable and meaningful to them. The following information should be provided via Patient Access API using the corresponding implementation guides:

* Claim details and encounters (see :ref:`carin_ig`)
* Clinical data incl. laboratory data (see :ref:`us_core_ig` and Da Vinci Payer Data Exchange)
* Plan Coverage and Formularies (US Drug Formulary)
* Prior Authorization Decisions (Da Vinci Prior Authorization Support)

.. image:: ../images/CMS-0057-P_PatientAccessAPI.pdf

.. note::
  The Da Vinci Payer Data Exchange Implementation Guide and the CARIN Blue Button Implementation Guide both use the ExplanationOfBenefits. 
  The main difference in usage is that the CARIN profiles make information available about a final claim, whereas PDex aims for sharing prior authorization information.
  Additional details about the prior authorization decisions can be exposed via the PAS profiles.

To implement a Patient Access API it is necessary to:

  #. Enable SMART on FHIR and point Firely Server to an authorization server managing the accounts of the patients. See :ref:`feature_accesscontrol`.
  #. Expose the Patient record with all its USCDI, CPCDS, and prior authorization data elements
  #. Configure the API clients to be allowed to be granted access (read-only) to resources on behalf of the patient. See :ref:`Configuration of API clients in Firely Auth <firely_auth_settings_clients>`.

Provider Access API
-------------------

Impacted payers (see `CMS definition <https://www.cms.gov/about-cms/obrhi/interoperability/faqs/patient-access-api#footnote-01>`_) are required to information exposed via a Patient Access API additionally available to providers who have a contractual relationship with the payer and a treatment relationship with the patient.
Providers could access information for an individual patient as well as a group of information, providing further insight into the patient's care activity at the point of care.

.. image:: ../images/CMS-0057-P_ProviderAccessAPI.pdf

To implement a Provider Access API (Bulk) it is necessary to:

  #. Enable SMART on FHIR and point Firely Server to an authorization server managing the accounts of the providers. See :ref:`feature_accesscontrol`.
  #. Expose the Patient records with all its USCDI, CPCDS, and prior authorization data elements
  #. Mantain a member attribution lists for providers. It is necessary to account for patients who opted out of the information sharing process. See :ref:`davinci_atr_ig`.
  #. Configure the provider API clients to be allowed to be granted access (read-only) on behalf of the provider. See :ref:`Configuration of API clients in Firely Auth <firely_auth_settings_clients>`.
  #. Create access policies to restrict access to a member attribution group based on their Taxpayer Identification Numbers (TINs) and National Provider Identifiers (NPIs). See :ref:`feature_accesscontrol_permissions`.
