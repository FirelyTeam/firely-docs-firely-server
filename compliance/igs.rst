.. _igs:

Supported Implementation Guides
===============================

All of the following FHIR Implementation Guides have been verified by Firely to work out-of-the-box with Firely Server.
Conformance to these Implementation Guides, more specifically their corresponding Capability Statements, is usually listed in `CapabilityStatement.instantiates <https://www.hl7.org/fhir/r4/capabilitystatement-definitions.html#CapabilityStatement.instantiates>`_.

.. _smart_app_launch_ig:

SMART App Launch
^^^^^^^^^^^^^^^^

The `SMART App Launch Implementation Guide <https://hl7.org/fhir/smart-app-launch/>`_ facilitates the integration of third-party applications with Electronic Health Record data, enabling their launch from within or outside the EHR system's user interface.
It offers a robust and secure authorization protocol for various app architectures, accommodating both end-user device-based apps and those operating on a secure server, accessible to clinicians, patients, and others through PHRs, Patient Portals, or any FHIR system.

This implementation guide does not specify any FHIR conformance resources and provides textual guidance only.

.. list-table:: SMART App Launch Overview
   :widths: 10, 10, 10, 10
   :header-rows: 1
   
   * - Supported version
     - Supporting documentation
     - Realm
     - Package Link

   * - * ✔️ v1.0.0
       * ✔️ v2.0.0
  
     - * :ref:`Firely Auth <firely_auth_index>`
       * :ref:`feature_accesscontrol`

     - * 🌍
   
     - * `hl7.fhir.uv.smart-app-launch|1.0.0 <https://registry.fhir.org/package/hl7.fhir.uv.smart-app-launch|1.0.0>`_
       * `hl7.fhir.uv.smart-app-launch|2.0.0 <https://registry.fhir.org/package/hl7.fhir.uv.smart-app-launch|2.0.0>`_

------------       

.. _bulk_data_access_ig:      

Bulk Data Access
^^^^^^^^^^^^^^^^

The `FHIR Bulk Data Access Implementation Guide <https://hl7.org/fhir/uv/bulkdata/>`_ is designed to facilitate the seamless exchange of large-scale healthcare data. This IG offers comprehensive guidelines and specifications for accessing and sharing bulk data, enabling healthcare organizations and researchers to efficiently retrieve, process, and analyze vast amounts of patient information.

This implementation guide does not specify any FHIR conformance resources and provides textual guidance only.

.. list-table:: Bulk Data Access Overview
   :widths: 10, 10, 10, 10
   :header-rows: 1
   
   * - Supported version
     - Supporting documentation
     - Realm
     - Package Link

   * - * ✔️ v1.0.1
       * ✔️ v2.0.0
  
     - * :ref:`Bulk Data Export <feature_bulkdataexport>`

     - * 🌍

     - * `hl7.fhir.uv.bulkdata|1.0.0 <https://registry.fhir.org/package/hl7.fhir.uv.bulkdata|1.0.0>`_
       * `hl7.fhir.uv.bulkdata|2.0.0 <https://registry.fhir.org/package/hl7.fhir.uv.bulkdata|2.0.0>`_

------------

.. _bulp_ig:       

Basic Audit Log Patterns
^^^^^^^^^^^^^^^^^^^^^^^^
The `Basic Audit Log Patterns (BALP) Implementation Guide <https://profiles.ihe.net/ITI/BALP/index.html>`_ is a IHE Content Profile designed to establish fundamental and reusable patterns for AuditEvent logs in the FHIR. 
These patterns are intended for FHIR RESTful operations while focusing on the main objective to enable Privacy-centric AuditEvent logs that clearly indicate the Patient when they are the subject of the recorded activity.

.. list-table:: BALP Overview
   :widths: 10, 10, 10, 10
   :header-rows: 1
   
   * - Supported version
     - Supporting documentation
     - Realm
     - Package Link

   * - * ✔️ v1.1.1
  
     - * :ref:`Auditing<feature_auditing>`

     - * 🌍

     - * `ihe.iti.balp|1.1.1 <https://registry.fhir.org/package/ihe.iti.balp|1.1.1>`_

------------

.. _us_core_ig:

USCDI & US Core
^^^^^^^^^^^^^^^

The `United States Core Data for Interoperability (USCDI) <https://www.healthit.gov/isa/united-states-core-data-interoperability-uscdi>`_ is a standardized set of health data elements and their associated value sets. 
It serves as a foundational health data standard to support seamless and secure health information exchange across the healthcare ecosystem in the United States.

The `US Core FHIR Implementation Guide <http://hl7.org/fhir/us/core/>`_ is a set of implementation specifications and guidance to support the effective FHIR in the United States. 
The US Core FHIR Implementation Guide aligns with the USCDI, providing detailed instructions on how to implement the necessary FHIR resources and profiles to ensure consistency and interoperability with the USCDI's data elements.

In summary, the USCDI defines the core health data elements for nationwide interoperability, while the US Core FHIR Implementation Guide complements it by offering practical guidelines and technical specifications for implementing FHIR to support seamless data exchange and improve care coordination within the US healthcare system.

.. list-table:: USCDI Overview
   :widths: 10, 10, 10, 10
   :header-rows: 1
   
   * - Supported version
     - Supporting documentation
     - Realm
     - Specification Link

   * - * ✔️ v1 - based on US Core 3.1.1, US Core 4.0.0
       * ✔️ v2 - based on US Core 5.0.1
  
     - n/A

     - * 🇺🇸

     - * `USCDI|1.0 - Errata <https://www.healthit.gov/isa/sites/isa/files/2020-10/USCDI-Version-1-July-2020-Errata-Final_0.pdf>`_
       * `USCDI|2.0 <https://www.healthit.gov/isa/sites/isa/files/2021-07/USCDI-Version-2-July-2021-Final.pdf>`_

.. list-table:: US Core Overview
   :widths: 10, 10, 10, 10
   :header-rows: 1
   
   * - Supported version
     - Supporting documentation
     - Realm
     - Package Link

   * - * ✔️ v3.1.1
       * ✔️ v4.0.0
       * ✔️ v5.0.1
  
     - n/A

     - * 🇺🇸

     - * `hl7.fhir.us.core|3.1.1 <https://registry.fhir.org/package/hl7.fhir.us.core|3.1.1>`_
       * `hl7.fhir.us.core|4.0.0 <https://registry.fhir.org/package/hl7.fhir.us.core|4.0.0>`_
       * `hl7.fhir.us.core|5.0.1 <https://registry.fhir.org/package/hl7.fhir.us.core|5.0.1>`_

Known Limitations
-----------------

* In order to validate resources claiming to conform to US Core, it is necessary to configure Firely Server to use an external terminology server incl. support for expanding SNOMED CT and LOINC ValueSets. See :ref:`feature_terminology`.
* Certain parameters are not implemented for the ``$docref`` operation on DocumentReference resources. See :ref:`feature_docref` for more details.

Test Data
---------

Firely provides test data covering all US-Core profiles and all elements marked as Must-Support. In order to load all examples, two transaction bundles need to be posted against the base endpoint of Firely Server. The following Postman collection provides you with the bundles itself, and the bundle entries as individual PUT requests.

.. raw:: html

  <div class="postman-run-button"
  data-postman-action="collection/fork"
  data-postman-var-1="24489118-e7d6d401-f82e-4695-a434-3d40399e2d2c"
  data-postman-collection-url="entityId=24489118-e7d6d401-f82e-4695-a434-3d40399e2d2c&entityType=collection&workspaceId=822b68d8-7e7d-4b09-b8f1-68362070f0bd"
  data-postman-param="env%5BFirely%20Server%20Public%5D=W3sia2V5IjoiQkFTRV9VUkwiLCJ2YWx1ZSI6Imh0dHBzOi8vc2VydmVyLmZpcmUubHkvIiwiZW5hYmxlZCI6dHJ1ZSwidHlwZSI6ImRlZmF1bHQifV0="></div>
  <script type="text/javascript">
    (function (p,o,s,t,m,a,n) {
      !p[s] && (p[s] = function () { (p[t] || (p[t] = [])).push(arguments); });
      !o.getElementById(s+t) && o.getElementsByTagName("head")[0].appendChild((
        (n = o.createElement("script")),
        (n.id = s+t), (n.async = 1), (n.src = m), n
      ));
    }(window, document, "_pm", "PostmanRunObject", "https://run.pstmn.io/button.js"));
  </script>

The following steps are necessary in order to execute the test collection against our own Firely Server instance:

#. Select "Fork Collection" or "View collection" in the Postman dialog

    .. image:: ../images/Compliance_ForkTestCollectionPostman.png
       :align: center
       :width: 500

#. Sign-In with your Postman account

#. `Create a new Postman environment <https://learning.postman.com/docs/sending-requests/managing-environments/#creating-environments>`_ with a "BASE_URL" variable and adjust the URL to your server endpoint

    .. image:: ../images/Compliance_EnvironmentTestCollectionPostman.png
       :align: center
       :width: 800

#. Make sure that the newly created environment is selected as the active environment

#. Open the collection "Firely Server - US Core Tests"

    .. image:: ../images/Compliance_USCoreTestCollectionPostman.png
       :align: center
       :width: 500

#. Execute the transaction request, the expected response is "HTTP 200 - OK".

------------

.. _carin_ig:

CPCDS & CARIN Blue Button
^^^^^^^^^^^^^^^^^^^^^^^^^

The `CARIN Blue Button FHIR Implementation Guide <https://hl7.org/fhir/us/carin-bb/>`_ is designed to facilitate the exchange of healthcare data between healthcare providers, payers, and patients.
It enables a payor to provide secure access to a Common Payer Consumer Data Set (CPCDS) for a patient. API clients can hereby access, interpret and display the content of the data set.

The CPCDS includes a comprehensive set of health care data elements, such as claims and encounter data, enrollment and eligibility information, pharmacy data, and clinical data. 
By creating a common data format, the CPCDS facilitates the seamless sharing of health information across different payers and health systems, promoting interoperability and data-driven decision-making.

.. list-table:: CARIN Blue Button Overview
   :widths: 10, 10, 10, 10
   :header-rows: 1
   
   * - Supported version
     - Supporting documentation
     - Realm
     - Specification Link

   * - * ✔️ v2.0.0
  
     - n/A

     - * 🇺🇸

     - * `hl7.fhir.us.carin-bb|2.0.0 <https://registry.fhir.org/package/hl7.fhir.us.carin-bb|2.0.0>`_

Known Limitations
-----------------

* FHIR ExplanationOfBenefits instances are not rejected if the claim conformance to the `abstract "C4BB Explanation Of Benefit" <https://hl7.org/fhir/us/carin-bb/StructureDefinition-C4BB-ExplanationOfBenefit.html>`_ profile
* In order to validate resources claiming to conform to CARIN Blue Button, it is necessary to configure Firely Server to use an external terminology server incl. support for expanding SNOMED CT, LOINC, NUBC, CPT, ICD-10, NCPDP, X12 ValueSets. See :ref:`feature_terminology`.
* By default invalid values for a search parameter are not rejected by Firely Server with an HTTP 400 - Bad Request status code. To enable this behavior required by CARIN, include a "Prefer: handling=strict" HTTP header in the search request.
* FHIRPath constraints using the "memberOf" function are not evaluated by Firely Server

------------

.. _davinci_atr_ig:

Da Vinci - Member Attribution (ATR) List
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

The goal of `Da Vinci - Member Attribution (ATR) List <https://hl7.org/fhir/us/davinci-atr/2023Jan/>`_ implementation guide is to enable providers to gain access to managed lists of all members (Patients) attibuted to their organization.
Payors are responsible of managing these lists. Based on ATR lists, providers can retreive administrative information in bulk about all members. Additionally, ATR lists can serve as the basis to allow providers to access claims and encounter data.

.. list-table:: Da Vinci - Member Attribution (ATR) List Overview
   :widths: 10, 10, 10, 10
   :header-rows: 1
   
   * - Supported version
     - Supporting documentation
     - Realm
     - Specification Link

   * - * ✔️ v2.0.0-ballot
  
     - n/A

     - * 🇺🇸

     - * `hl7.fhir.us.davinci-atr|2.0.0-ballot <https://registry.fhir.org/package/hl7.fhir.us.davinci-atr|2.0.0-ballot>`_

Known Limitations
-----------------

* The custom operations ``$member-add`` and ``$member-remove`` are not supported. Therefore for all member updates, a new version of a Group resources is created.
* The ``_until`` parameter is not supported as part of the Bulk Date Export operations.
* The ``$davinci-data-export`` wrapper around ``$export`` is not supported.

------------

.. _isik_ig:

ISiK
^^^^

The `"ISiK" FHIR implementation guide <https://fachportal.gematik.de/informationen-fuer/isik>`_ was developed by gematik (national agency for digital health in Germany). The specification defines specific implementation guidelines for the use of FHIR in the German healthcare system.
The ISiK FHIR implementation guide aims to improve interoperability and the exchange of health data in Germany. It specifies which FHIR resources, profiles, and terminologies should be implemented to ensure a uniform and secure communication between different IT systems in the stationary healthcare sector. 

.. list-table:: ISiK Overview
   :widths: 10, 10, 10, 10
   :header-rows: 1
   
   * - Supported version
     - Supporting documentation
     - Realm
     - Package Link

   * - * ✔️ v1.0.7
  
     - n/A

     - * 🇩🇪

     - * `de.gematik.isik-basismodul-stufe1|1.0.7 <https://registry.fhir.org/package/de.gematik.isik-basismodul-stufe1|1.0.7>`_
