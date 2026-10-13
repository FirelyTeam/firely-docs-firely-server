.. _feature_meta:

Meta operations - $meta, $meta-add, $meta-delete
================================================

.. note::

  The features described on this page are available in **all** :ref:`Firely Server editions <vonk_overview>`.

Firely Server provides an implementation of the $meta, $meta-add, $meta-delete operation as defined in the `FHIR Specification <http://hl7.org/fhir/resource-operations.html#meta>`_.

The default settings enable ``$meta`` on the system, type, instance and version level, and ``$meta-add`` and ``$meta-delete`` on the instance and version level (see :ref:`disable_interactions`). On the type and system level, ``$meta`` reads the resources of that type or of the whole database to collect their metadata, so on a large database it can take a long time. You can disable those levels in the ``Operations`` section.
