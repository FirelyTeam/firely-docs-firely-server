.. _feature_davinci_data_export:

====================
DaVinci Data Export
====================

.. note::

	The features described on this page are available in the following :ref:`Firely Server editions <vonk_overview>`:

	* Firely Prior Authorization - 🇺🇸

DaVinci Data Export (``$davinci-data-export``) is a Da Vinci specific wrapper around Bulk Data Export (BDE).
In Firely Server, the export processing pipeline is the same as BDE. The key functional difference is the REST operation that initiates the export.

This operation is defined by the Da Vinci ATR implementation guide as a profiled version of Bulk Data Export:
`OperationDefinition: davinci-data-export <https://build.fhir.org/ig/HL7/davinci-atr/OperationDefinition-davinci-data-export.html>`_.


Introduction
------------

DaVinci Data Export enables export use cases where the request semantics are defined by Da Vinci implementation guides (for example ATR member attribution workflows).
For Firely Server, kickoff is done through ``$davinci-data-export``, while task lifecycle and output retrieval remain aligned with BDE.

Use this page for DaVinci specific behavior.
For shared export behavior and operational concepts, see :ref:`feature_bulkdataexport`.


Configuration
-------------

DaVinci Data Export builds on the same plugin and infrastructure configuration as BDE.

Use the same setup described in :ref:`feature_bulkdataexport_configuration`, including:

* BDE plugin enablement on the root branch:
	* including: ``Vonk.Plugin.BulkDataExport.GroupBulkDataExportConfiguration`` 
    * and additionally (for DaVinci): ``Vonk.Plugin.BulkDataExport.DaVinci.DaVinciDataExportConfiguration``

* task repository configuration on the ``/administration`` branch
* ``TaskFileManagement`` storage configuration
* ``BulkDataExport`` settings such as ``RepeatPeriod`` and retention settings

Because DaVinci export reuses the BDE task pipeline, there is no separate task storage or file storage configuration specific to ``$davinci-data-export``.


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
| ``exportType``    | ✅        | ``canonical``      | Indicates the type of export to perform. This parameter is DaVinci-specific and is not part of the base Bulk Data Export operation. |
+-------------------+-----------+--------------------+---------------------------------------------------------------+
| ``patient``       | ✅        | ``reference``      | Same behavior as BDE for POST requests.                       |
+-------------------+-----------+--------------------+---------------------------------------------------------------+
| ``_since``        | ✅        | ``instant``        | Same behavior as BDE.                                         |
+-------------------+-----------+--------------------+---------------------------------------------------------------+
| ``_until``        | ✅        | ``instant``        | Same behavior as BDE.                                         |
+-------------------+-----------+--------------------+---------------------------------------------------------------+
| ``_type``         | ✅        | FHIR resource type | Specifies resource types to export. Must include ``Group``, ``Patient``, and ``Coverage``. If omitted, defaults to exactly these three required types. Optional types: ``RelatedPerson``, ``Practitioner``, ``PractitionerRole``, ``Organization``, ``Location``. |
+-------------------+-----------+--------------------+---------------------------------------------------------------+
| ``_typeFilter``   | ❌        | ``string``         | Not supported.                                                |
+-------------------+-----------+--------------------+---------------------------------------------------------------+
| ``_elements``     | ✅        | FHIR element       | Same behavior as BDE.                                         |
+-------------------+-----------+--------------------+---------------------------------------------------------------+
| ``_outputFormat`` | ✅        | ``string``         | Only ``application/ndjson`` is supported.                     |
+-------------------+-----------+--------------------+---------------------------------------------------------------+

Supported ``exportType`` values (examples)
""""""""""""""""""""""""""""""""""""""""

+--------------------------------------+----------------------------------------------------------+
| ``exportType`` value                 | Use case                                                 |
+======================================+==========================================================+
| ``hl7.fhir.us.davinci-atr``          | Da Vinci Member Attribution export.                      |
+--------------------------------------+----------------------------------------------------------+

Servers are expected to provide detailed export guidance in the applicable implementation guide.

Supported ``_type`` values for ATR
""""""""""""""""""""""""""""""""""

+--------------------+---------------------------+
| Resource Type      | Requirement               |
+====================+===========================+
| ``Group``          | Required                  |
+--------------------+---------------------------+
| ``Patient``        | Required                  |
+--------------------+---------------------------+
| ``Coverage``       | Required                  |
+--------------------+---------------------------+
| ``RelatedPerson``  | Optional                  |
+--------------------+---------------------------+
| ``Practitioner``   | Optional                  |
+--------------------+---------------------------+
| ``PractitionerRole`` | Optional                  |
+--------------------+---------------------------+
| ``Organization``   | Optional                  |
+--------------------+---------------------------+
| ``Location``       | Optional                  |
+--------------------+---------------------------+

If the ``_type`` parameter is omitted, Firely Server defaults to exporting exactly the three required types: ``Group``, ``Patient``, and ``Coverage``.


Request examples
^^^^^^^^^^^^^^^^

Example POST kickoff:

.. code-block:: http

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

DaVinci exports inherit the same Group-based export handling used by BDE.
This includes ATR-specific Group member inclusion behavior documented on the BDE page.

See :ref:`feature_bulkdataexport` for full details.


Security and authorization
--------------------------

When SMART on FHIR is enabled, the same token and scope behavior used by BDE applies to DaVinci export task initiation and retrieval endpoints.
See the section "Filtering export results with SMART scopes" on :ref:`feature_bulkdataexport`.


Relationship to Bulk Data Export
--------------------------------

Summary of how ``$davinci-data-export`` relates to BDE in Firely Server:

* Same asynchronous export pipeline and task handling
* Same export status and file retrieval endpoints
* Same storage and retention behavior
* Same filtering behavior for shared parameters
* Different kickoff operation name (``$davinci-data-export``)
* DaVinci specific ``exportType`` parameter
