.. _feature_snapshot:

Snapshot generation - $snapshot
===============================

.. note::

  The features described on this page are available in **all** :ref:`Firely Server editions <vonk_overview>`.

The ``$snapshot`` operation generates the snapshot of a StructureDefinition. The snapshot is the full list
of element definitions of a profile, computed from its differential and the StructureDefinitions it is based on.
In FHIR, ``$snapshot`` is defined as a standard operation on ``StructureDefinition``.

Firely Server supports calling ``$snapshot`` at one interaction level:

#. :ref:`feature_snapshot_type`

In all cases, Firely Server must have access to all StructureDefinitions that the submitted StructureDefinition depends on. To make them available, upload them to Firely Server's administration database. See :ref:`feature_snapshot_pre`.

.. note::

    The very first call to $snapshot will take a considerable amount of time, typically around 5 seconds. This is because Firely Server maintains a cache of StructureDefinition information, and on the first call that cache is still empty.
    Subsequent calls are much faster.

Overview
--------

**Operation name**
  ``$snapshot``

**FHIR specification**
  https://www.hl7.org/fhir/structuredefinition-operation-snapshot.html

**OperationDefinition**
  ``http://hl7.org/fhir/OperationDefinition/StructureDefinition-snapshot``

**Scope**
  - Invocation level: ``type``
  - Supported resource type(s): ``StructureDefinition``
  - Idempotent: ``yes``
  - Affects server state: ``no``

**HTTP methods**
  - ``POST`` (type level)

Configuration
-------------

The ``$snapshot`` operation is provided by the
``Vonk.Plugin.Operations.SnapshotGeneration`` namespace.

You can enable or disable this operation by including or excluding this namespace
in the Firely Server pipeline options. For more information about configuring available plugins in the pipeline options,
see :ref:`vonk_available_plugins`.

The ``Operations`` section of the settings controls on which level the operation can be called and whether it requires authorization.
These are the default settings:

.. code-block:: json

    "Operations": {
      "$snapshot": {
        "Name": "$snapshot",
        "Level": [
          "Type"
        ],
        "Enabled": true,
        "RequireAuthorization": "WhenAuthEnabled",
        "RequireTenant": "Never"
      }
    }

See :ref:`disable_interactions` for more information about these settings.

Input parameters
^^^^^^^^^^^^^^^^

The following input parameters are supported for the ``$snapshot`` operation.

+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| Parameter               | Supported | Type                    | Cardinality | Additional Notes               |
+=========================+===========+=========================+=============+================================+
| ``definition``          | ✅        | StructureDefinition     | 0..1        | The StructureDefinition to     |
|                         |           |                         |             | generate a snapshot for. Send  |
|                         |           |                         |             | it as the request body itself, |
|                         |           |                         |             | not wrapped in a               |
|                         |           |                         |             | ``Parameters`` resource. An    |
|                         |           |                         |             | existing snapshot in it is     |
|                         |           |                         |             | ignored.                       |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``url``                 | ❌        | string                  | 0..1        | Canonical URL of a             |
|                         |           |                         |             | StructureDefinition known to   |
|                         |           |                         |             | the server. Generating a       |
|                         |           |                         |             | snapshot for a stored          |
|                         |           |                         |             | StructureDefinition by its URL |
|                         |           |                         |             | is not supported.              |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+

Output parameters
^^^^^^^^^^^^^^^^^

The ``$snapshot`` operation returns the StructureDefinition with a generated snapshot.

+-------------------------+-------------------------+-------------+--------------------------------+
| Parameter               | Type                    | Cardinality | Description                    |
+=========================+=========================+=============+================================+
| ``return``              | StructureDefinition     | 1..1        | The submitted                  |
|                         |                         |             | StructureDefinition with a     |
|                         |                         |             | (re-)generated ``snapshot``    |
|                         |                         |             | element.                       |
+-------------------------+-------------------------+-------------+--------------------------------+

When to use this operation
--------------------------

Use ``$snapshot`` when you want to:

- complete a profile that you authored as a differential only, before publishing or distributing it;
- check how the elements of a profile are constrained after all base profiles have been applied;
- troubleshoot a profile that does not validate as expected during development time.

.. _feature_snapshot_type:

Generate a snapshot on the ResourceType level
---------------------------------------------

.. code-block::

   POST <firely_server_endpoint>/StructureDefinition/$snapshot

Submit the StructureDefinition that you want filled with a fresh snapshot as the request body.
The request must have a Content-Type header matching the format of the body (``application/fhir+json`` or ``application/fhir+xml``).

Firely Server returns the same StructureDefinition, but with the ``snapshot`` element (re-)generated.
The StructureDefinition is not stored.

Example request:

.. code-block::

   POST <firely_server_endpoint>/StructureDefinition/$snapshot

.. code-block::

   {
     "resourceType": "StructureDefinition",
     "url": "https://example.org/fhir/StructureDefinition/MyPatientProfile",
     "type": "Patient",
     "baseDefinition": "http://hl7.org/fhir/StructureDefinition/Patient",
     "derivation": "constraint",
     "differential": {
       [...]
     },
     [...]
   }

.. _feature_snapshot_pre:

Precondition
------------

Firely Server must be aware of all the other StructureDefinitions that are referred to by the StructureDefinition in the body of the request. Refer to the :ref:`conformance` for more information.

The snapshot is generated for the FHIR version of the request, so these StructureDefinitions must be available for that FHIR version.

Special considerations
----------------------

.. warning::

   If snapshot generation fails with an error, Firely Server returns HTTP status ``200`` with an
   ``OperationOutcome`` describing the errors instead of the StructureDefinition. A client must check
   the ``resourceType`` of the response, not only the HTTP status.

- When snapshot generation succeeds, warnings that occurred during generation are not returned.
- If the request body contains another resource type than ``StructureDefinition``, Firely Server returns HTTP status ``400``.
