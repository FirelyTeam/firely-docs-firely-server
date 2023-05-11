.. _vonk_roadmap:

Firely Server Roadmap
=====================

This page lists the features and changes we have planned for the foreseeable future. This planning is volatile, so changes will happen. We will update this page accordingly. You are also very welcome to provide input to us on features or fixes that you would like to see prioritized. 

Disclaimer: No rights can be derived from this roadmap.

2023
----

Q2
^^

* Support for release version of FHIR R5
* Package-based import of FHIR conformance resources
* Improved support for §170.315(b)(10) Electronic health information export
* Improved support for EHR App Launch scenarios in Firely Auth
* Azure Active Directory and Single Sign-on support in Firely Auth
* Native support for ServiceBus-based messaging to improve EHR integration scenarios

Q3
^^

* Support for QI Core / DaVinci DEQM Implementation Guide

The following features are intended to support the CMS-0057-P regulation:

* Improved support for Patient Access APIs:
  * UI framework in Firely Auth to build webpages to select launch context during SMART app launch

* Improved support for Provider Access APIs:
  * Filtering patients based on Consent during Bulk Data Export requests
  * Discovery of member-attribution lists for providers (Da Vinci - Member Attribution (ATR) List)

* Improved support for Payor-to-Payor APIs:
  * Request and apply patient/user level scopes during a Bulk Data Export request

* Improved support for PARDD APIs:
  * Validation of QuestionnaireResponses against Questionnaires
