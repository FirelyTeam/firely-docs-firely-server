.. _feature_snapshot:

Snapshot generation - $snapshot
===============================

.. note::

  The features described on this page are available in **all** :ref:`Firely Server editions <vonk_overview>`.

Firely Server is capable of generating a snapshot for a StructureDefinition. This operation is not defined in the FHIR Specification.

You can invoke this operation with
::

    POST <firely-server-endpoint>/StructureDefinition/$snapshot

* The body must contain the StructureDefinition that you want filled with a fresh snapshot. The StructureDefinition may contain an existing snapshot, it will be ignored.
* The Content-Type header must match the format of the body (application/fhir+json or application/fhir+xml)

Firely Server will return the same StructureDefinition, but with the snapshot element (re-)generated.

.. note::

    The very first call to $snapshot will take a considerable amount of time, typically around 5 seconds. This is because Firely Server maintains a cache of StructureDefinition information, and on the first call that cache is still empty.
    Subsequent calls are much faster.

.. _feature_snapshot_pre:

Precondition
------------

Firely Server must be aware of all the other StructureDefinitions that are referred to by the StructureDefinition in the body of the request. Refer to the :ref:`conformance` for more information.
