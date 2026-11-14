.. _feature_davinci_data_export:

==========================================
DaVinci Data Export - $davinci-data-export
==========================================

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
For the DaVinci ATR use case, it is possible to hide a selection of resource elements from the result set. In Firely server, this is called 'Resource redaction'. To configure the redaction used for the ATR export, we use the ``ResourceRedaction`` section in appsettings. Redaction 'hl7.fhir.us.davinci-atr' contains the redaction definition used for the ATR export. Note that multiple FHIR paths are combined with an OR-relationship.

.. code-block:: json

	{
		"ResourceRedaction": {
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

1. The DaVinci ATR specific conformance resources must be loaded in the administration database. See :ref:`conformance`.
2. The Consent search parameters that the opt-out filter uses must be loaded and indexed. Firely Server does not load them by default. See :ref:`davinci_data_export_opt_out` for the list, and reindex after loading them:

.. code-block:: http

	POST [firely-server-base]/administration/$reindex HTTP/1.1
	Content-Type: application/x-www-form-urlencoded

.. code-block:: text

	include=Consent.provision-type,Consent.provider-access-recipient,Group.atr-intended-recipient


$davinci-data-export
--------------------

Export request
^^^^^^^^^^^^^^

DaVinci Data Export is initiated as a Group-level operation (Patient and System level exports are not supported). Both GET and POST methods are supported:

**url:** ``[firely-server-base]/Group/<group-id>/$davinci-data-export``

In case of a POST request, a FHIR Parameters resource must be provided as the payload. An example of such a request would look as follows:

.. code-block:: http

	POST [firely-server-base]/Group/{{GROUP_ID}}/$davinci-data-export HTTP/1.1
	Content-Type: application/fhir+json
	Prefer: respond-async

.. code-block:: json

	{
		"resourceType": "Parameters",
		"parameter": [
			{
				"name": "exportType",
				"valueCanonical": "hl7.fhir.us.davinci-atr"
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
|                   |           |                    | ``hl7.fhir.us.davinci-atr`` exportType. The value is                |
|                   |           |                    | case-sensitive. A request without ``exportType``, or with more      |
|                   |           |                    | than one, returns ``400 Bad Request``. Any other value returns      |
|                   |           |                    | ``501 Not Implemented``.                                            |
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

The export excludes patients that have opted out, as described in :ref:`davinci_data_export_opt_out`.

See the section "Filtering export results with SMART scopes" on :ref:`feature_bulkdataexport`.

.. _davinci_data_export_opt_out:

Patient opt-out
^^^^^^^^^^^^^^^

A patient opts out of Provider Access with a Provider Access Consent (`PDex Provider Consent profile <https://hl7.org/fhir/us/davinci-pdex/STU2.2/StructureDefinition-pdex-provider-consent.html>`_). The Consent refers to the patient in ``Consent.patient``. Firely Server excludes the patient from an export when such a Consent is ``active``, its ``provision.period`` covers the time of the export, and ``provision.type`` is ``deny``.

Firely Server recognises a Provider Access Consent by any of these:

* the category ``http://hl7.org/fhir/us/davinci-pdex/CodeSystem/pdex-consent-api-purpose|provider-access``, as PDex 2.2.0 requires;
* the profile ``http://hl7.org/fhir/us/davinci-pdex/StructureDefinition/pdex-provider-consent`` in ``meta.profile``, unless the deprecated root extension ``pdex-provider-access-use-case`` is set to ``false``;
* the deprecated root extension ``pdex-provider-access-use-case`` set to ``true``.

**Opting out of every provider.** A deny Consent that names no provider excludes the patient from the export of every Group.

**Opting out of single providers.** A deny Consent can name the providers it is about in ``provision.actor``. Give each such actor the role ``http://terminology.hl7.org/CodeSystem/v3-ParticipationType|IRCP`` (information recipient) and put the provider's identifier in ``reference.identifier``. The Consent then excludes the patient only from the export of a Group that one of these identifiers identifies. An identifier identifies a Group when its system and value equal:

* an entry of ``Group.identifier``, or
* the identifier in the ATR extension ``http://hl7.org/fhir/us/davinci-atr/StructureDefinition/atr-intended-recipient-identifier-extension`` of the Group.

An identifier without a system identifies no Group. Other actors, such as the payer as the source of the Consent, are not providers the Consent is about.

.. note::
	A provider-specific opt-out is enforced on the provider organization, not on single practitioners. Firely Server matches the identifiers of the Group itself, such as the organization-level (Type 2) NPI or the TIN of the provider. It does not match the identifiers of the Organization or Practitioner the Group refers to. A Consent that names the individual (Type 1) NPI of a practitioner excludes the patient from no export, unless a Group carries that NPI itself. To keep their records from one practitioner, a patient opts out of the practitioner's organization, or of every provider.

Example of a Consent that keeps the patient's records from the provider with NPI ``1234567890``:

.. code-block:: json

	{
	  "resourceType": "Consent",
	  "status": "active",
	  "scope": { "coding": [{ "system": "http://terminology.hl7.org/CodeSystem/consentscope", "code": "patient-privacy" }] },
	  "category": [
	    { "coding": [{ "system": "http://terminology.hl7.org/CodeSystem/v3-ActCode", "code": "IDSCL" }] },
	    { "coding": [{ "system": "http://hl7.org/fhir/us/davinci-pdex/CodeSystem/pdex-consent-api-purpose", "code": "provider-access" }] }
	  ],
	  "patient": { "reference": "Patient/example" },
	  "provision": {
	    "type": "deny",
	    "period": { "start": "2026-01-01" },
	    "actor": [
	      {
	        "role": { "coding": [{ "system": "http://terminology.hl7.org/CodeSystem/provenance-participant-type", "code": "performer" }] },
	        "reference": { "identifier": { "system": "http://hl7.org/fhir/sid/us-npi", "value": "9876543210" }, "display": "Example Health Plan" }
	      },
	      {
	        "role": { "coding": [{ "system": "http://terminology.hl7.org/CodeSystem/v3-ParticipationType", "code": "IRCP" }] },
	        "reference": { "identifier": { "system": "http://hl7.org/fhir/sid/us-npi", "value": "1234567890" } }
	      }
	    ]
	  }
	}

Search parameters
"""""""""""""""""

The opt-out filter reads the Consents and Groups through these search parameters. Load each one as a ``SearchParameter`` resource in the administration database (see :ref:`feature_customsp`) and reindex the resources it is on.

.. list-table::
	:header-rows: 1

	* - Code
	  - Canonical URL
	  - Needed for
	* - ``category`` (Consent)
	  - FHIR core
	  - Recognising a Provider Access Consent. Without it, the export answers ``501``.
	* - ``provision-type`` (Consent)
	  - ``http://hl7.org/fhir/us/davinci-pdex/SearchParameter/pdex-consent-provision-type``
	  - Recognising a deny. The deprecated ``decision`` search parameter still works.
	* - ``provider-access-recipient`` (Consent)
	  - ``http://fire.ly/fhir/SearchParameter/Consent-provider-access-recipient``
	  - Opting out of single providers. Without it, every deny Consent excludes the patient from every export.
	* - ``atr-intended-recipient`` (Group)
	  - ``http://fire.ly/fhir/SearchParameter/Group-atr-intended-recipient``
	  - Matching a provider by the intended-recipient extension of a Group.

The export answers ``501`` when Provider Access Consents exist and none of them is indexed for ``provision.type``. It also answers ``501`` when a Consent names a provider and the exported Group has an intended-recipient extension that is not indexed, because an opt-out naming that identifier would then not be applied. The server log names the search parameter to load.

The two Firely search parameters:

.. code-block:: json

	{
	  "resourceType": "SearchParameter",
	  "url": "http://fire.ly/fhir/SearchParameter/Consent-provider-access-recipient",
	  "name": "ProviderAccessRecipient",
	  "status": "active",
	  "description": "The providers a Provider Access Consent names as information recipients (role IRCP), by their identifier.",
	  "code": "provider-access-recipient",
	  "base": ["Consent"],
	  "type": "token",
	  "expression": "Consent.provision.actor.where(role.coding.where(system = 'http://terminology.hl7.org/CodeSystem/v3-ParticipationType' and code = 'IRCP').exists()).reference.identifier"
	}

.. code-block:: json

	{
	  "resourceType": "SearchParameter",
	  "url": "http://fire.ly/fhir/SearchParameter/Group-atr-intended-recipient",
	  "name": "AtrIntendedRecipient",
	  "status": "active",
	  "description": "The provider an ATR Group is meant for, by the identifier in its intended-recipient extension.",
	  "code": "atr-intended-recipient",
	  "base": ["Group"],
	  "type": "token",
	  "expression": "Group.extension('http://hl7.org/fhir/us/davinci-atr/StructureDefinition/atr-intended-recipient-identifier-extension').value.ofType(Identifier)"
	}


Restrict clients to DaVinci Data Export only
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
In addition to the standard SMART authorization, ATR clients SHOULD always be restricted to only the operations and resource types relevant to the DaVinci ATR use case. This minimizes the risk of leaking information or allowing unauthorized access. DaVinci Data Export supports a mechanism for restricting background clients in this way.

The operations are:

* ``/Group/{id}/$davinci-data-export``, to start the export
* ``/$exportstatus``, to poll for the status of the export and to cancel it (see :ref:`bdeexportstatus`)
* ``/$exportfilerequest``, to download the exported files

In order to restrict a client to only these operations, the SMART security token must include the following claims:

* http://server.fire.ly/auth/claims/critical/IsAtrClient = true
* The client uses the ``client_credentials`` flow.
* fhirUser = { a reference to a Device resource representing the client }
* groupId = { the groupId of the ATR Group that the client is allowed to export. This groupId is used in the AccessPolicyDefinition }

Since all other access is denied, the client will also have to be granted read access to the resources, used in these operations. This can be done using an AccessPolicy, referencing an AccessPolicyDefinition (see :ref:`feature_accesscontrol_permissions`)

.. code-block:: http

   	PUT [firely-server-base]/administration/AccessPolicyDefinition/AtrClientGroups HTTP/1.1
   	Content-Type: application/fhir+json

.. code-block:: json

	{
  		"resourceType": "AccessPolicyDefinition",
  		"id": "AtrClientGroups",
  		"url": "[firely-server-base]/administration/AccessPolicyDefinition/AtrClientGroups",
  		"status": "active",
  		"policy": [{
    		"type": { "coding": [{ "code": "smart-v2" }] },
    		"restriction": ["system/Group.rs?_identifier=https://example.com/fhir/sid/atr-groupid|#groupId#", "system/Consent.rs", "system/Coverage.rs", "system/Patient.rs"]
  		}]
	}

Note that in the AccessPolicyDefinition above, an identifier with system 'atr' is expected (e.g. https://example.com/fhir/sid/atr-groupid). This is just an example and will likely be a different system in your database, so make sure to use that in order to restrict the client to the correct Group. 

As you can see, only the most necessary resource types are allowed here. If you would like to allow the client to receive additional resource types (as used in the _type parameter for the export), add them to the 'restriction' array.

.. code-block:: http

   	PUT [firely-server-base]/AccessPolicy/atr-client-abc HTTP/1.1
   	Content-Type: application/fhir+json

.. code-block:: json

	{
  		"resourceType": "AccessPolicy",
  		"id": "atr-client-abc",
  		"subject": [{ "reference": "[the fhirUser in the token]" }],
  		"instantiatesCanonical": ["[firely-server-base]/administration/AccessPolicyDefinition/AtrClientGroups"]
	}

