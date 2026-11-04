.. _feature_x-provenance:

X-Provenance header
===================

.. note::

  The features described on this page are available in the following :ref:`Firely Server editions <vonk_overview>`:

  * Firely Scale - 🌍 / 🇺🇸
  * Firely Prior Authorization - 🇺🇸


Description
-----------

The X-Provenance header can be used to add a Provenance resource upon creating, updating or patching another resource, and upon a transaction Bundle (see :ref:`x_provenance_transactions`). See `Provenance#header <https://www.hl7.org/fhir/Provenance.html#header>`_ for more information.

.. note:: In the case of a conditional create, where the resource was already present, the Provenance resource is not created.

Facade implementations
^^^^^^^^^^^^^^^^^^^^^^

.. note:: The provenance header functionality is dependent on transaction services. If transaction support is disabled in your facade implementation, the configuration for the provenance header should be disabled as well. 

Linking the Provenance resource
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Provenance must point to its target resource through the ``target`` property (see `Provenance#resource <https://www.hl7.org/fhir/Provenance.html#resource>`_)
The ``target`` will be automatically deduced from the created/updated resource and does not need to be included in the header. 
The reference will point to the current version (after create/update) of the resource.

Validation of the Provenance resource
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Validation of the Provenance resource follows the settings for validation (see :ref:`feature_prevalidation`). 

.. note:: If validation is disabled, an invalid Provenance resource in the header will be ignored and the request will complete normally (if valid).

.. _x_provenance_transactions:

Transactions
^^^^^^^^^^^^

The X-Provenance header can also be sent with a transaction Bundle (``POST {base-url}`` with a Bundle of type ``transaction``). Firely Server then stores one Provenance for the whole transaction.

* **Targets.** The ``target`` of the Provenance lists one versioned reference (``Patient/123/_history/<version>``) for every entry that created, updated or patched a resource. Entries that delete, read or search, operations, and conditional creates that matched an existing resource are not targets. Any ``target`` in the header is replaced.
* **Writes that change nothing.** An update or patch that changes nothing is not a target. Firely Server detects such a write only when the UpdateNoOp plugins are active and ``UpdateNoOp:EnableForHttp`` is ``true`` (the default); see :ref:`vonk_plugins_update_noop` and :ref:`vonk_plugins_patch_noop`. Without them, such a write creates a new version, and that version is a target.
* **A resource written twice** is listed twice, once for each version. The targets follow the order of the entries in the request Bundle.
* **No target, no Provenance.** If no entry qualifies, for example in a transaction that only reads, the transaction is processed as usual and no Provenance is stored.
* **Atomicity.** The Provenance is stored in the same database transaction as the entries. If it cannot be stored, the transaction fails and no entry persists. This holds on the backends where a transaction is atomic: SQL Server, SQLite and MongoDB with a replica set or sharded cluster (see :ref:`mongodb_transactions`). On MongoDB without a replica set and on the in-memory store, the entries are not rolled back, as for any failed transaction.
* **Response.** On success the response is the usual transaction-response; it does not mention the Provenance. Find it with a search, for example ``GET {base-url}/Provenance?target=Patient/123``. To find one specific Provenance again, mark it in the header, for example with a ``meta.tag``. If the Provenance cannot be stored, the response is an OperationOutcome with the status of the failure (for example 400 or 403). Each issue starts with ``X-Provenance:``.
* **An unusable header** - empty, not valid JSON, not a Provenance, or with unknown elements - is rejected with 400 before any entry is processed, whatever the validation level. This differs from a single resource, where an unusable header is ignored when validation is off.
* **References in the header** are stored as sent. A reference to an entry of the same Bundle, such as a ``urn:uuid`` reference, is not resolved. An absolute reference to this server is stored as a relative reference.
* **Visibility.** A Provenance can target resources in the compartments of several Patients. A client that can read it through one Patient compartment also sees the references to the resources of the other Patients.
* **Patient-scoped access.** With a patient-scoped SMART on FHIR token, the transaction must write the Patient of the client's own compartment, so that the Provenance belongs to that compartment. Otherwise the Provenance is refused with 403 and no entry persists. Creating that Patient in the same transaction is not supported.
* **Batch Bundles** are not supported: a batch Bundle with the header is rejected with 400.
* **Plugins.** The X-Provenance plugin and the transaction plugin are both needed (see :ref:`vonk_plugins_transaction`). Without the X-Provenance plugin the header is ignored and no Provenance is stored. Plugins in the pipeline between order 1230 and 3120 do not see the request that creates the Provenance, as they do not see the entries of a transaction.

Example of a transaction that patches two Patients:

.. code-block:: JavaScript

  POST {base-url}

  X-Provenance: { "resourceType": "Provenance", "recorded": "2026-11-04T10:00:00+01:00", "agent": [ { "who": { "reference": "[mandatory reference]" } } ] }

  {
    "resourceType": "Bundle",
    "type": "transaction",
    "entry": [
      {
        "resource": { "resourceType": "Parameters", "parameter": [ { "name": "operation", "part": [
          { "name": "type", "valueCode": "replace" },
          { "name": "path", "valueString": "Patient.active" },
          { "name": "value", "valueBoolean": false } ] } ] },
        "request": { "method": "PATCH", "url": "Patient/123" }
      },
      {
        "resource": { "resourceType": "Parameters", "parameter": [ { "name": "operation", "part": [
          { "name": "type", "valueCode": "replace" },
          { "name": "path", "valueString": "Patient.active" },
          { "name": "value", "valueBoolean": false } ] } ] },
        "request": { "method": "PATCH", "url": "Patient/456" }
      }
    ]
  }

The stored Provenance has the targets ``Patient/123/_history/<new version>`` and ``Patient/456/_history/<new version>``.

Limitations
^^^^^^^^^^^

The maximum header length of the web server applies (e.g. IIS 8KB/16KB).

Example
-------

.. code-block:: JavaScript

  POST {base-url}/Patient

  X-Provenance: { "resourceType": "Provenance", "text": { "status": "generated", "div": "<div>Record of change</div>" }, "recorded": "2022-08-24T11:05:24+02:00", "agent": [ { "who": { "reference": "[mandatory reference]" } } ] }

  {
      "resourceType": "Patient",
      "active": true,
      "name": [
        {
          "use": "official",
          "family": [
            "Doe"
          ],
          "given": [
            "John"
          ]
        }
      ],
      "gender": "male",
      "birthDate": "1974-12-25"
  }

Update ``[mandatory reference]`` to point to a valid resource in your system.

License
-------
The X-Provenance header is available for licenses with the Transaction feature enabled.