.. _feature_meta:

Meta operations - $meta, $meta-add, $meta-delete
================================================

.. note::

  The features described on this page are available in **all** :ref:`Firely Server editions <vonk_overview>`.

Firely Server provides an implementation of the $meta, $meta-add, $meta-delete operation as defined in the `FHIR Specification <http://hl7.org/fhir/resource-operations.html#meta>`_.

By default the operation is only enabled on the level of a resource instance. It can also be enabled on the level of a resourcetype or system wide, but the cost of execution will then be high. On sufficient customer demand an optimized implementation is possible.
