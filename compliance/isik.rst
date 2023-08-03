.. _compliance_isik:

Informationstechnische Systeme in Krankenhäusern (ISiK) - 🇩🇪
============================================================

Overview
^^^^^^^^

   .. note::

     Firely Server v4 has been officially certified against ISiK Basis - Stufe 1. For more details, see our listing on `gematik Fachportal - Bestätigungsübersicht Primärsysteme <https://fachportal.gematik.de/zulassungs-bestaetigungsuebersichten?tx_wfgemtables_tables%5B__referrer%5D%5B%40extension%5D=Wfgemtables&tx_wfgemtables_tables%5B__referrer%5D%5B%40controller%5D=Admissiontable&tx_wfgemtables_tables%5B__referrer%5D%5B%40action%5D=list&tx_wfgemtables_tables%5B__referrer%5D%5Barguments%5D=YTo1OntzOjE0OiJhZG1pc3Npb25zdGF0ZSI7czowOiIiO3M6OToiY29tcG9uZW50IjtzOjE0OiJCZXN0LUlTaUstSVNpUCI7czoxNjoibWFudWZhY3R1cmVybmFtZSI7czowOiIiO3M6Nzoic29ydGluZyI7czowOiIiO3M6MTY6InNvcnRpbmdkaXJlY3Rpb24iO3M6MToiMSI7fQ%3D%3D8108c07fbc60e09d0c87e4d8e03e9d3fe504a5ae&tx_wfgemtables_tables%5B__referrer%5D%5B%40request%5D=%7B%22%40extension%22%3A%22Wfgemtables%22%2C%22%40controller%22%3A%22Admissiontable%22%2C%22%40action%22%3A%22list%22%7D6b6cfbc1529a27354b62fb8884536afb04cbbb17&tx_wfgemtables_tables%5B__trustedProperties%5D=%7B%22admissionstate%22%3A1%2C%22component%22%3A1%2C%22manufacturername%22%3A1%2C%22productfeature%22%3A1%2C%22sorting%22%3A1%2C%22sortingdirection%22%3A1%7Da77389d183e8ca47988e3cbf448a458531a93925&tx_wfgemtables_tables%5Badmissionstate%5D=Bestätigt&tx_wfgemtables_tables%5Bcomponent%5D=Best-ISiK-ISiP&tx_wfgemtables_tables%5Bmanufacturername%5D=&tx_wfgemtables_tables%5Bproductfeature%5D=&tx_wfgemtables_tables%5Bsorting%5D=&tx_wfgemtables_tables%5Bsortingdirection%5D=1#c2947>`_.
     The offical notice of compliance by gematik can be received upon request.

Firely Server is ready to comply with the mandatory and optional ISiK certification criteria without any additional configuration specified in :ref:`ISiK - Supported Versions <isik_ig>`.

ISiK APIs
^^^^^^^^^
ISiK is categorized in different modules, each of which is versioned independently. Each ISiK module defines a set of minimal API interactions that an EHR needs to support.
In most cases a read-only API combined with required and optional search parameters are defined. All required interactions are listed in the corresponding CapabilityStatement within each implementation guide.

ISiK Basis:

* `ISiK Basis - "Stufe 1" <https://simplifier.net/isik-basis-v1>`_
* `ISiK Basis - "Stufe 2" <https://simplifier.net/isik-basis-v2>`_
* `ISiK Basis - "Stufe 3" <https://simplifier.net/isik-basis-v3>`_

ISiK Vitalparameter und Körpermaße:

* `ISiK - Vitalparameter und Körpermaße - "Stufe 2" <https://simplifier.net/isik-vitalparameter-und-koerpermasze-v2>`_
* `ISiK - Vitalparameter und Körpermaße - "Stufe 3" <https://simplifier.net/isik-vitalparameter-und-koerpermasze-v3>`_

ISiK Medikation:

* `ISiK - Medikation - "Stufe 2" <https://simplifier.net/isik-medikation-v2>`_
* `ISiK - Medikation - "Stufe 3" <https://simplifier.net/isik-medikation-v3>`_

ISiK Dokumentenaustausch:

* `ISiK - Dokumentenaustausch - "Stufe 2" <https://simplifier.net/isik-dokumentenaustausch-v2>`_
* `ISiK - Dokumentenaustausch - "Stufe 3" <https://simplifier.net/isik-dokumentenaustausch-v3>`_

ISiK Terminplanung:

* `ISiK - Terminplanung - "Stufe 2" <https://simplifier.net/isik-terminplanung-v2>`_
* `ISiK - Terminplanung - "Stufe 3" <https://simplifier.net/isik-terminplanung-v3>`_

ISiK Sicherheit:

* `ISiK - Sicherheit - "Stufe 2" <https://simplifier.net/isik-sicherheit-v2>`_
* `ISiK - Sicherheit - "Stufe 3" <https://simplifier.net/isik-sicherheit-v3>`_

To implement an ISiK API it is necessary to:

#. Optionally, enable SMART on FHIR and point Firely Server to an authorization server managing the accounts of the patients - See :ref:`feature_accesscontrol`
#. Expose the Patient records with all their data elements defined by each implementation guide
#. Import the conformance resources defined by a module into the administration database of Firely Server to enable the validation against them - See :ref:`conformance`
#. Enable the document handling plugin to support the submission of a "Bericht aus Subsystemen" - See :ref:`restful_documenthandling`
#. In case SMART on FHIR is enabled, configure the API clients to be allowed to be granted access (ready-only) to resources on behalf of the patient - See :ref:`Configuration of API clients in Firely Auth <firely_auth_settings_clients>`
#. Note: Some modules define custom operations which - if not specified otherwise above - not supported out-of-the-box and need to be implement as a plugin in Firely Server - See :ref:`vonk_overview_plugins`