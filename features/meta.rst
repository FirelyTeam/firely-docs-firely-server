.. _feature_meta:

$meta
=====

Firely Server provides an implementation of the $meta operation as defined in the `FHIR Specification <http://hl7.org/fhir/STU3/resource-operations.html#meta>`_.

By default the operation is only enabled on the level of a resource instance. It can also be enabled on the level of a resourcetype or system wide, but the cost of execution will then be high. On sufficient customer demand an optimized implementation is possible.
