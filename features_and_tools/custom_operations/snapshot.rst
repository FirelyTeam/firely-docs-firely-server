.. _feature_snapshot:

Snapshot generation - $snapshot
===============================

.. note::

  The features described on this page are available in **all** :ref:`Firely Server editions <vonk_overview>`.

The ``$snapshot`` operation generates the snapshot of a StructureDefinition. The snapshot is the full list
of element definitions of a profile, computed from its differential and the StructureDefinitions it is based on.
In FHIR, ``$snapshot`` is defined as a standard operation on ``StructureDefinition``.

Firely Server supports calling ``$snapshot`` at two interaction levels:

#. :ref:`feature_snapshot_type`
#. :ref:`feature_snapshot_instance`

In all cases, Firely Server must have access to all StructureDefinitions that the StructureDefinition depends on. To make them available, upload them to Firely Server's administration database. See :ref:`feature_snapshot_pre`.

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
  - Invocation level: ``type`` / ``instance``
  - Supported resource type(s): ``StructureDefinition``
  - Idempotent: ``yes``
  - Affects server state: ``no``

**HTTP methods**
  - ``GET`` / ``POST`` (type and instance level)

Configuration
-------------

The ``$snapshot`` operation is provided by the
``Vonk.Plugin.Operations.SnapshotGeneration`` namespace.

You can enable or disable this operation by including or excluding this namespace
in the Firely Server pipeline options. For more information about configuring available plugins in the pipeline options,
see :ref:`vonk_available_plugins`.

The ``Operations`` section of the settings controls on which levels the operation can be called and whether it requires authorization.
These are the default settings:

.. code-block:: json

    "Operations": {
      "$snapshot": {
        "Name": "$snapshot",
        "Level": [
          "Instance",
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
When you call ``$snapshot`` on the type level (``StructureDefinition/$snapshot``), provide exactly one of them.
When you call it on the instance level (``StructureDefinition/<id>/$snapshot``), Firely Server generates the snapshot of the stored StructureDefinition and ignores both parameters, see :ref:`feature_snapshot_instance`.

+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| Parameter               | Supported | Type                    | Cardinality | Additional Notes               |
+=========================+===========+=========================+=============+================================+
| ``definition``          | ✅        | StructureDefinition     | 0..1        | The StructureDefinition to     |
|                         |           |                         |             | generate a snapshot for, in a  |
|                         |           |                         |             | ``Parameters`` body. You can   |
|                         |           |                         |             | also send the                  |
|                         |           |                         |             | StructureDefinition itself as  |
|                         |           |                         |             | the request body. An existing  |
|                         |           |                         |             | snapshot in it is ignored.     |
|                         |           |                         |             | Type level only.               |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``url``                 | ✅        | string                  | 0..1        | Canonical URL of a             |
|                         |           |                         |             | StructureDefinition known to   |
|                         |           |                         |             | the server: from the FHIR      |
|                         |           |                         |             | specification or from the      |
|                         |           |                         |             | administration database. In    |
|                         |           |                         |             | the query of a ``GET`` or      |
|                         |           |                         |             | ``POST``, or in a              |
|                         |           |                         |             | ``Parameters`` body. Type      |
|                         |           |                         |             | level only.                    |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+

Output parameters
^^^^^^^^^^^^^^^^^

The ``$snapshot`` operation returns the StructureDefinition with a generated snapshot.

+-------------------------+-------------------------+-------------+--------------------------------+
| Parameter               | Type                    | Cardinality | Description                    |
+=========================+=========================+=============+================================+
| ``return``              | StructureDefinition     | 1..1        | The StructureDefinition with a |
|                         |                         |             | (re-)generated ``snapshot``    |
|                         |                         |             | element.                       |
+-------------------------+-------------------------+-------------+--------------------------------+
| ``outcome``             | OperationOutcome        | 0..1        | The warnings and information   |
|                         |                         |             | that snapshot generation       |
|                         |                         |             | reported. Not defined by the   |
|                         |                         |             | FHIR specification; only       |
|                         |                         |             | present when there are such    |
|                         |                         |             | issues.                        |
+-------------------------+-------------------------+-------------+--------------------------------+

When snapshot generation reports no issues, Firely Server returns the StructureDefinition itself, as FHIR defines for an operation with a single ``return`` parameter.
When it reports warnings or information, Firely Server returns a ``Parameters`` resource with the StructureDefinition in ``return`` and the issues in ``outcome``.
When it reports an error, Firely Server returns HTTP status ``400`` with an ``OperationOutcome`` describing the errors.

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

   POST <firely_server_endpoint>/StructureDefinition/$snapshot[?url=<canonical-url-of-structuredefinition>]
   GET <firely_server_endpoint>/StructureDefinition/$snapshot?url=<canonical-url-of-structuredefinition>

There are three ways to call ``$snapshot`` on the type level:

#. Submit the StructureDefinition as the request body.
#. Submit a ``Parameters`` resource as the request body with:

   - a ``definition`` parameter containing the StructureDefinition; or
   - a ``url`` parameter containing the canonical URL of a StructureDefinition known to the server.

#. Provide the ``url`` parameter in the query of a ``GET`` or ``POST``. A ``POST`` with ``url`` in the query needs no body.

When you send a body, the request must have a Content-Type header matching the format of the body (``application/fhir+json`` or ``application/fhir+xml``).

The StructureDefinition you submit is not stored. With ``url``, Firely Server generates the snapshot again on a copy of the StructureDefinition it knows, so the issues of generating it are reported in the same way as for a submitted StructureDefinition.

Example requests:

.. tab-set::

   .. tab-item:: StructureDefinition body

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

   .. tab-item:: Parameters body

      .. code-block::

         POST <firely_server_endpoint>/StructureDefinition/$snapshot

      .. code-block::

         {
           "resourceType": "Parameters",
           "parameter": [
             {
               "name": "url",
               "valueUri": "https://example.org/fhir/StructureDefinition/MyPatientProfile"
             }
           ]
         }

   .. tab-item:: url in the query

      .. code-block::

         GET <firely_server_endpoint>/StructureDefinition/$snapshot?url=https://example.org/fhir/StructureDefinition/MyPatientProfile

.. _feature_snapshot_instance:

Generate a snapshot for a StructureDefinition from the database
---------------------------------------------------------------

.. code-block::

   GET / POST <firely_server_endpoint>/StructureDefinition/<id>/$snapshot

At this level, Firely Server generates the snapshot of the StructureDefinition stored under ``<id>``. The ``definition`` and ``url`` parameters are ignored, as the FHIR specification requires.
The client must be allowed to read the StructureDefinition.

.. _feature_snapshot_pre:

Precondition
------------

Firely Server must be aware of all the other StructureDefinitions that are referred to by the StructureDefinition you generate a snapshot for. Refer to the :ref:`conformance` for more information.

The snapshot is generated for the FHIR version of the request, so these StructureDefinitions must be available for that FHIR version.

Special considerations
----------------------

Firely Server rejects a request with the following HTTP status codes:

+----------------------------------------------------------------------+--------+
| Situation                                                            | Status |
+======================================================================+========+
| Both ``url`` and ``definition`` are provided, including a            | 422    |
| StructureDefinition body together with ``url`` in the query.         |        |
+----------------------------------------------------------------------+--------+
| Neither ``url`` nor ``definition`` is provided at the type level,    | 400    |
| one of them is provided more than once, or a ``Parameters`` body     |        |
| contains a parameter without a name or a parameter that              |        |
| ``$snapshot`` does not define.                                       |        |
+----------------------------------------------------------------------+--------+
| The body is another resource than a ``StructureDefinition`` or a     | 400    |
| ``Parameters`` resource.                                             |        |
+----------------------------------------------------------------------+--------+
| The request has a body that cannot be read: it cannot be parsed      | 400 or |
| (400), or its Content-Type is missing or not supported (415). A      | 415    |
| ``url`` in the query does not make Firely Server ignore such a body. |        |
+----------------------------------------------------------------------+--------+
| Snapshot generation reports an error.                                | 400    |
+----------------------------------------------------------------------+--------+
| The server does not know a StructureDefinition with the given        | 404    |
| ``url``.                                                             |        |
+----------------------------------------------------------------------+--------+
| At the instance level: there is no StructureDefinition with the      | 404    |
| given id, or the client is not allowed to read it.                   |        |
+----------------------------------------------------------------------+--------+
| At the instance level: the StructureDefinition is stored in another  | 415    |
| FHIR version than the one of the request.                            |        |
+----------------------------------------------------------------------+--------+
| At the instance level: more than one StructureDefinition is stored   | 409    |
| with the given id.                                                   |        |
+----------------------------------------------------------------------+--------+

.. note::

   The ``url`` parameter resolves the StructureDefinitions of the FHIR specification and the conformance resources in the administration database.
   Firely Server treats these as public, so it does not check whether the client is allowed to read them.
