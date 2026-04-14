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

Summary of how ``$davinci-data-export`` relates to BDE in Firely Server:

* Same asynchronous export pipeline and task handling
* Same export status and file retrieval endpoints
* Same storage and retention behavior
* Same filtering behavior for shared parameters
* Different kickoff operation name (``$davinci-data-export``)
* DaVinci specific ``exportType`` parameter


Introduction
------------

**Background and Purpose**

The Da Vinci Data Export specification was developed to address a critical need in health information exchange: enabling safe, controlled data sharing between providers and payers without exposing sensitive patient clinical details. Traditional bulk data export mechanisms include all clinical data (encounters, observations, procedures, etc.), which may not be appropriate or necessary for certain provider-to-payer workflows. The DaVinci export IG constrains what data is exported to only what is essential for specific use cases, creating a more secure and efficient exchange pattern.

**Key Characteristics**

A critical distinction of DaVinci Data Export is what it **does not** include:

* Patient-specific clinical data such as **Encounters**, **Observations**, **Procedures**, and other detailed clinical records are **not exported**
* The export focuses on structural data (Patients, Groups, Coverage, Practitioners, and Organizations) that is necessary for reference and attribution purposes
* This makes DaVinci export a safe option for provider-to-payer data sharing, as sensitive clinical information remains protected at the originating system

**Supported Use Cases**

DaVinci Data Export currently supports:

* **Member Attribution** (ATR - Attribution via Terms and Rules): Enables providers to share member/patient attribution data with payers for care coordination and quality reporting purposes, without exposing detailed clinical information

Use this page for DaVinci specific behavior.
For shared export behavior and operational concepts, see :ref:`feature_bulkdataexport`.


Configuration
-------------

DaVinci Data Export builds on the same plugin and infrastructure configuration as BDE.

Use the same setup described in :ref:`feature_bulkdataexport_configuration`, including:

* BDE plugin enablement in the PipelineOptions:

	* including: ``Vonk.Plugin.BulkDataExport.GroupBulkDataExportConfiguration``
	* and additionally (for DaVinci): ``Vonk.Plugin.BulkDataExport.DaVinci.DaVinciDataExportConfiguration``

* task repository configuration on the ``/administration`` branch
* ``TaskFileManagement`` storage configuration
* ``BulkDataExport`` settings such as ``RepeatPeriod`` and retention settings

Because DaVinci export reuses the BDE task pipeline, there is no separate task storage or file storage configuration specific to ``$davinci-data-export``.

Prerequisites
-------------

There are a few prerequisites specifically for this export.

1. A search parameter for the PDex provider access use case flag on ``Consent.provision.action`` must be defined on the server. This is required for the ATR export use case, which filters ``Consent`` resources based on this flag. Add the search parameter and refresh the actual index as described in :ref:`the search parameter re-indexing documentation <feature_customsp_reindex_specific>`:

	 .. code-block:: json

			{
				"resourceType": "SearchParameter",
				"id": "consent-pdex-provider-access-use-case",
				"url": "http://hl7.org/fhir/us/davinci-pdex/SearchParameter/consent-pdex-provider-access-use-case",
				"version": "4.0.1",
				"name": "ConsentPdexProviderAccessUseCase",
				"status": "active",
				"publisher": "Firely",
				"description": "Search Consent resources by the PDex provider access use case flag on provision.action",
				"code": "pdex-provider-access-use-case",
				"base": ["Consent"],
				"type": "token",
				"expression": "Consent.provision.action.extension.where(url='http://hl7.org/fhir/us/davinci-pdex/StructureDefinition/pdex-provider-access-use-case').value.ofType(boolean)"
			}


$davinci-data-export
--------------------

Kickoff endpoint
^^^^^^^^^^^^^^^^

DaVinci Data Export is initiated as a Group-level operation:

**url:** ``[firely-server-base]/Group/<group-id>/$davinci-data-export``

In contrast to standard Group BDE kickoff (``/Group/<group-id>/$export``), this operation name is DaVinci specific.
For DaVinci kickoff, Firely Server supports POST requests only.


Supported parameters
^^^^^^^^^^^^^^^^^^^^

Firely Server handles DaVinci kickoff using the same filtering semantics as BDE for shared parameters.
The DaVinci operation also defines the ``exportType`` parameter to identify the requested Da Vinci use case.

+-------------------+-----------+--------------------+---------------------------------------------------------------+
| Parameter         | Supported | Type               | Additional Notes                                              |
+===================+===========+====================+===============================================================+
| ``exportType``    | ✅        | ``canonical``      | Indicates the type of export to perform. This parameter       |
|                   |           |                    | is DaVinci-specific and is not part of the base Bulk Data     |
|                   |           |                    | Export operation.                                             |
+-------------------+-----------+--------------------+---------------------------------------------------------------+
| ``patient``       | ✅        | ``reference``      | Same behavior as BDE for POST requests.                       |
+-------------------+-----------+--------------------+---------------------------------------------------------------+
| ``_since``        | ✅        | ``instant``        | Same behavior as BDE.                                         |
+-------------------+-----------+--------------------+---------------------------------------------------------------+
| ``_until``        | ✅        | ``instant``        | Same behavior as BDE.                                         |
+-------------------+-----------+--------------------+---------------------------------------------------------------+
| ``_type``         | ✅        | FHIR resource type | Specifies resource types to export. Must include              |
|                   |           |                    | ``Group``, ``Patient``, and ``Coverage``. If omitted,         |
|                   |           |                    | defaults to exactly these three required types.               |
|                   |           |                    | Optional types: ``RelatedPerson``, ``Practitioner``,          |
|                   |           |                    | ``PractitionerRole``, ``Organization``, ``Location``.         |
+-------------------+-----------+--------------------+---------------------------------------------------------------+
| ``_typeFilter``   | ❌        | ``string``         | Not supported.                                                |
+-------------------+-----------+--------------------+---------------------------------------------------------------+
| ``_elements``     | ✅        | FHIR element       | Same behavior as BDE.                                         |
+-------------------+-----------+--------------------+---------------------------------------------------------------+
| ``_outputFormat`` | ✅        | ``string``         | Only ``application/ndjson`` is supported.                     |
+-------------------+-----------+--------------------+---------------------------------------------------------------+

Supported ``exportType`` values (examples)
""""""""""""""""""""""""""""""""""""""""""

+--------------------------------------+----------------------------------------------------------+
| ``exportType`` value                 | Use case                                                 |
+======================================+==========================================================+
| ``hl7.fhir.us.davinci-atr``          | Da Vinci Member Attribution export.                      |
+--------------------------------------+----------------------------------------------------------+

Servers are expected to provide detailed export guidance in the applicable implementation guide.

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

If the ``_type`` parameter is omitted, Firely Server defaults to exporting exactly the three required types: ``Group``, ``Patient``, and ``Coverage``.


Request examples
^^^^^^^^^^^^^^^^

Example POST kickoff:

.. code-block:: text

	POST {{BASE_URL}}/Group/{{GROUP_ID}}/$davinci-data-export
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


Response and task lifecycle
---------------------------

After kickoff, DaVinci export follows the same asynchronous task flow as BDE:

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

When SMART on FHIR is enabled, the same token and scope behavior used by BDE applies to DaVinci export task initiation and retrieval endpoints. There is one additional requirement: as the DaVinci export checks for consent (a Patient can opt-out) the DaVinci export requires read access to Consent resources.
See the section "Filtering export results with SMART scopes" on :ref:`feature_bulkdataexport`.



