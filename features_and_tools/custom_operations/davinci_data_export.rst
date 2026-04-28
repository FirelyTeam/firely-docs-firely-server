.. _feature_davinci_data_export:

====================
DaVinci Data Export
====================

.. note::

	The features described on this page are available in the following :ref:`Firely Server editions <vonk_overview>`:

	* Firely Prior Authorization - 🇺🇸

This operation is defined by the Da Vinci ATR implementation guide as a specialized version of Bulk Data Export:
`OperationDefinition: davinci-data-export <https://build.fhir.org/ig/HL7/davinci-atr/OperationDefinition-davinci-data-export.html>`_.

Relationship to Bulk Data Export
--------------------------------

Summary of how ``$davinci-data-export`` relates to :ref:`feature_bulkdataexport` in Firely Server:

* Same asynchronous export pipeline and task handling
* Same export status and file retrieval endpoints
* Same storage and retention behavior
* Same filtering behavior for shared parameters
* Different export operation name (``$davinci-data-export``)
* DaVinci specific ``exportType`` parameter


Introduction
------------

**Background and Purpose**

The DaVinci Data Export specification was developed to address a critical need in health information exchange: enabling safe, controlled data sharing between providers and payers without exposing sensitive patient clinical details. Traditional bulk data export mechanisms include all clinical data (encounters, observations, procedures, etc.), which may not be appropriate or necessary for certain provider-to-payer workflows. The DaVinci export IG constrains what data is exported to only what is essential for specific use cases, creating a more secure and efficient exchange pattern.

**Key Characteristics**

A critical distinction of DaVinci Data Export is what it **does not** include:

* Patient-specific clinical data such as **Encounters**, **Observations**, **Procedures**, and other detailed clinical records are **not exported**
* The export focuses on structural data (Patients, Groups, Coverage, Practitioners, and Organizations) that is necessary for reference and attribution purposes
* This makes DaVinci export a safe option for provider-to-payer data sharing, as sensitive clinical information remains protected at the originating system

**Supported Use Cases**

DaVinci Data Export currently supports:

* **Member Attribution** (ATR - Attribution via Terms and Rules): Enables providers to share member/patient attribution data with payers for care coordination and quality reporting purposes, without exposing detailed clinical information

Configuration
-------------

DaVinci Data Export builds on the same plugin and infrastructure configuration as :ref:`feature_bulkdataexport`.

It is configured similarly to what is described in :ref:`feature_bulkdataexport_configuration`, with a couple of pre-requisites:

* BDE plugin enablement in the PipelineOptions:

	* including: ``Vonk.Plugin.BulkDataExport.GroupBulkDataExportConfiguration``
	* and additionally (for DaVinci): ``Vonk.Plugin.BulkDataExport.DaVinci.DaVinciDataExportConfiguration``

* task repository configuration on the ``/administration`` branch
* ``TaskFileManagement`` storage configuration
* ``BulkDataExport`` settings such as ``RepeatPeriod`` and retention settings

Because DaVinci Data Export reuses the BDE task pipeline, it can reuse the same settings.

Element redaction
^^^^^^^^^^^^^^^^^
For the DaVinci ATR use case, it is possible to hide a selection of resource elements from the result set. In Firely server, this is called 'Resource redaction'. To configure the redaction used for the ATR export, we use the 'ResourceRedactionOptions' in appsettings. Redaction 'hl7.fhir.us.davinci-atr' contains the redaction definition used for the ATR export. Note that multiple FHIR paths are combined with an OR-relationship.

.. code-block:: json

	{
		"ResourceRedactionOptions": {
			"Redactions": {
				"hl7.fhir.us.davinci-atr": {
					"FhirVersions": [
						"Fhir4.0"
					],
					"OmitElements": [
						"descendants().where($this is Money)",
						"Coverage.costToBeneficiary"
					]
				}
			}
		}
	}


Prerequisites
-------------

There are a few prerequisites specifically for this export.

1. The DaVinci ATR specific conformance resources must be loaded in the administration database. See :ref:`_conformance`
2. Two Davinci-specific search parameters have been added to Firely Server and are needed for the export. In order to be able to use these parameters, execute the following REST call:

.. code-block:: http

	POST ``[firely-server-base]/administration/$reindex``
	Content-Type: application/x-www-form-urlencoded

	include=Consent.decision,Consent.DaVinci-pdex-provider-access-use-case


$davinci-data-export
--------------------

Export request
^^^^^^^^^^^^^^

DaVinci Data Export is initiated as a Group-level operation (Patient and System level exports are not supported). Both GET and POST methods are supported:

**url:** ``[firely-server-base]/Group/<group-id>/$davinci-data-export``

In case of a POST request, a FHIR Parameters resource must be provided as the payload. An example of such a request would look as follows:

.. code-block:: http

	POST ``[firely-server-base]/Group/{{GROUP_ID}}/$davinci-data-export
	Content-Type: application/fhir+json
	Prefer: respond-async

	{
		"resourceType": "Parameters",
		"parameter": [
			{
				"name": "exportType",
				"valueCanonical": "http://hl7.org/fhir/us/davinci-atr"
			},
			{
				"name": "_since",
				"valueInstant": "2026-01-01T00:00:00Z"
			}
		]
	}

Supported parameters
^^^^^^^^^^^^^^^^^^^^

Firely Server handles DaVinci using the same filtering semantics as BDE for shared parameters.
The DaVinci operation also defines the ``exportType`` parameter to identify the requested Da Vinci use case.

+-------------------+-----------+--------------------+---------------------------------------------------------------------+
| Parameter         | Supported | Type               | Additional Notes                                                    |
+===================+===========+====================+=====================================================================+
| ``exportType``    | ✅        | ``canonical``      | Indicates the type of export to perform. This parameter is          |
|                   |           |                    | DaVinci-specific and is not part of the base Bulk Data Export       |
|                   |           |                    | operation. Firely Server currently only supports the                |
|                   |           |                    | ``hl7.fhir.us.davinci-atr`` exportType                              |
+-------------------+-----------+--------------------+---------------------------------------------------------------------+
| ``patient``       | ✅        | ``reference``      | Same behavior as BDE for POST requests (not allowed in GET)         |
+-------------------+-----------+--------------------+---------------------------------------------------------------------+
| ``_since``        | ✅        | ``instant``        | Same behavior as BDE.                                               |
+-------------------+-----------+--------------------+---------------------------------------------------------------------+
| ``_until``        | ✅        | ``instant``        | Same behavior as BDE.                                               |
+-------------------+-----------+--------------------+---------------------------------------------------------------------+
| ``_type``         | ✅        | FHIR resource type | Specifies resource types to export. See the _type table below for   |
|                   |           |                    | the supported types                                                 |
+-------------------+-----------+--------------------+---------------------------------------------------------------------+
| ``_typeFilter``   | ❌        | ``string``         | Not supported.                                                      |
+-------------------+-----------+--------------------+---------------------------------------------------------------------+
| ``_elements``     | ✅        | FHIR element       | Same behavior as BDE.                                               |
+-------------------+-----------+--------------------+---------------------------------------------------------------------+
| ``_outputFormat`` | ✅        | ``string``         | Only ``application/ndjson`` is supported.                           |
+-------------------+-----------+--------------------+---------------------------------------------------------------------+


Supported ``_type`` values for ATR
""""""""""""""""""""""""""""""""""

+----------------------+---------------------------+
| Resource Type        | Requirement               |
+======================+===========================+
| ``Group``            | Required                  |
+----------------------+---------------------------+
| ``Patient``          | Required                  |
+----------------------+---------------------------+
| ``Coverage``         | Required                  |
+----------------------+---------------------------+
| ``RelatedPerson``    | Optional                  |
+----------------------+---------------------------+
| ``Practitioner``     | Optional                  |
+----------------------+---------------------------+
| ``PractitionerRole`` | Optional                  |
+----------------------+---------------------------+
| ``Organization``     | Optional                  |
+----------------------+---------------------------+
| ``Location``         | Optional                  |
+----------------------+---------------------------+

If the ``_type`` parameter is omitted, Firely Server defaults to exporting only the three required types: ``Group``, ``Patient``, and ``Coverage``.

Response and task lifecycle
---------------------------

After the initial DaVinci data export operation, DaVinci Data Export follows the same asynchronous task flow as BDE:

* queued task creation
* polling task state through ``$exportstatus``
* retrieving generated NDJSON files through ``$exportfilerequest``

For response examples and status details, see:

* :ref:`bdeexportstatus`
* :ref:`feature_bulkdataexport`


Group membership handling
-------------------------

The Group targeted by ``$davinci-data-export`` is the Member Attribution List defined by the Da Vinci ATR IG, not an arbitrary Group. This Group conforms to the ATR Group profile 'http://hl7.org/fhir/us/davinci-atr/StructureDefinition/atr-group'
Per the ATR specification, the Group represents a contract-specific attribution list and is expected to conform to the ATR Group profile, with member entries identifying the attributed patients and their associated attribution details.

When the optional ``patient`` parameter is supplied, only patients that are already members of the identified Group are exported.
When ``patient`` is omitted, Firely Server exports data for all members in the Group.

See :ref:`feature_bulkdataexport` for full details.


Security and authorization
--------------------------

When SMART on FHIR is enabled, the same token and scope behavior used by BDE applies to DaVinci Data Export task initiation and retrieval endpoints. There is one additional requirement: as the DaVinci Data Export checks for consent (a Patient can opt-out) the DaVinci Data Export requires read access to Consent resources.

The opt-out Consent must conform to the DaVinci ATR IG's Consent profile (http://hl7.org/fhir/us/davinci-atr/StructureDefinition/atr-consent) and must be associated with the patient through the standard Consent.patient reference. For Firely Server's DaVinci export filtering, the Consent also needs the ``DaVinci-pdex-provider-access-use-case`` extension on ``Consent.provision.action`` (see IG `PDex Provider Consent profile <https://build.fhir.org/ig/HL7/davinci-epdx/StructureDefinition-pdex-provider-consent.html>`_). The export will exclude patients that have an applicable opt-out Consent in place.

See the section "Filtering export results with SMART scopes" on :ref:`feature_bulkdataexport`.



