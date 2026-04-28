.. _feature_validation:

Validation - $validate
======================

.. note::

  The features described on this page are available in **all** :ref:`Firely Server editions <vonk_overview>`.

The ``$validate`` operation validates a resource against the FHIR base specification and,
optionally, against a specific profile. In FHIR, ``$validate`` is defined as a standard
operation on ``Resource``.

Firely Server supports calling ``$validate`` at three interaction levels:

#. :ref:`feature_validation_system`
#. :ref:`feature_validation_type`
#. :ref:`feature_validation_instance`

In addition, Firely Server can validate incoming resources automatically and can restrict that validation
to specific profiles. See :ref:`feature_prevalidation`.

In all cases, Firely Server must have access to all relevant StructureDefinition resources and related conformance resources. To make them available for validation, upload them to Firely Server's administration database. See :ref:`feature_validation_pre`.

.. note::

    The very first validation call will take a considerable amount of time, typically around 5 seconds. This is because Firely Server maintains a cache of validation information, and on the first call that cache is still empty.
    Subsequent calls are much faster.

Overview
--------

**Operation name**
  ``$validate``

**FHIR specification**
  https://www.hl7.org/fhir/resource-operation-validate.html


**OperationDefinition**
  ``http://hl7.org/fhir/OperationDefinition/Resource-validate``


**Scope**
  - Invocation level: ``system`` / ``type`` / ``instance``
  - Supported resource type(s): any
  - Idempotent: ``yes``
  - Affects server state: ``no``

**HTTP methods**
  - ``POST`` (system and type level)
  - ``GET`` (instance level)

Configuration
-------------

The ``$validate`` operation is provided by the
``Vonk.Plugin.Operations.Validation`` namespace.

You can enable or disable this operation by including or excluding this namespace
in the Firely Server pipeline options. For more information about configuring available plugins in the pipeline options,
see :ref:`vonk_available_plugins`.

The validation settings described in :ref:`feature_prevalidation` also apply to
the ``$validate`` operation. This means you can use the ``Validation`` settings
to adjust validation behavior for both pre-validation and explicit
``$validate`` calls.

For example, these settings can be used to:

- adjust the strictness of parsing by configuring ``Validation:Parsing``
- control the validation level with ``Validation:Level``
- suppress, override, or selectively apply validation rules using
  ``Validation:AdvisorRules``

Input parameters
~~~~~~~~~~~~~~~~

The following input parameters are supported for the ``$validate`` operation.

+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| Parameter               | Supported | Type                    | Cardinality | Additional Notes               |
+=========================+===========+=========================+=============+================================+
| ``resource``            | ✅        | Resource                | 0..1        | The resource to validate.      |
|                         |           |                         |             | Required when using a          |
|                         |           |                         |             | ``Parameters`` resource as     |
|                         |           |                         |             | input.                         |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``profile``             | ✅        | canonical (uri)         | 0..1        | Canonical URL of the           |
|                         |           |                         |             | ``StructureDefinition`` to     |
|                         |           |                         |             | validate against. If omitted,  |
|                         |           |                         |             | validation is performed        |
|                         |           |                         |             | against the base profile and   |
|                         |           |                         |             | any profiles in                |
|                         |           |                         |             | ``meta.profile``.              |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``mode``                | ❌        | code                    | 0..1        | Validation mode as defined by  |
|                         |           |                         |             | the FHIR specification         |
|                         |           |                         |             | (``create``, ``update``,       |
|                         |           |                         |             | ``delete``). Indicates how the |
|                         |           |                         |             | resource is intended to be     |
|                         |           |                         |             | used and influences validation |
|                         |           |                         |             | rules.                         |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``usageContext``        | ❌        | UsageContext            | 0..*        | Indicates the implementation   |
|                         |           |                         |             | context for validation and may |
|                         |           |                         |             | influence additional bindings. |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+

Output parameters
~~~~~~~~~~~~~~~~~

The ``$validate`` operation returns an ``OperationOutcome`` resource describing the validation result.

+-------------------------+-------------------------+-------------+--------------------------------+
| Parameter               | Type                    | Cardinality | Description                    |
+=========================+=========================+=============+================================+
| ``return``              | OperationOutcome        | 1..1        | The outcome of the validation. |
|                         |                         |             | Contains validation issues     |
|                         |                         |             | such as errors, warnings, and  |
|                         |                         |             | informational messages.        |
+-------------------------+-------------------------+-------------+--------------------------------+

When to use this operation
--------------------------

Use ``$validate`` when you want to:

- check whether a resource is valid against the FHIR base rules;
- validate a resource against a specific profile before storing or forwarding it;
- validate a resource already stored in Firely Server;
- troubleshoot conformance problems with resources or bundles during development time.

.. _feature_validation_system:

Validate on the system level
----------------------------

.. code-block::

   POST <firely_server_endpoint>/$validate[?profile=<canonical-url-of-structuredefinition>]


Use the system-level form when Firely Server should validate the submitted resource without
inferring the resource type from the request URL.

There are two ways to call ``$validate`` on the system level:

#. Submit a Resource or a Bundle as the request body, optionally with a ``profile`` query parameter.
#. Submit a ``Parameters`` resource as the request body with:

   - a ``resource`` parameter containing the resource to validate;
   - optionally, a ``profile`` parameter containing the profile canonical URL.

In both cases the request must have a Content-Type header matching the format of the body (``application/fhir+json`` or ``application/fhir+xml``).

If you do not specify a ``profile`` parameter, Firely Server validates the resource against:

- the base profile from the FHIR specification;
- any profiles listed in ``meta.profile``.

At the system level, Firely Server does **not** assume a specific resource type from the URL.

Example request with a ``Parameters`` body:

.. code-block::

   POST <firely_server_endpoint>/$validate

.. code-block::

   {
     "resourceType": "Parameters",
     "parameter": [
       {
         "name": "resource",
         "resource": {
           "resourceType": "Patient",
           [...]
         }
       },
       {
         "name": "profile",
         "valueCanonical": "https://example.org/fhir/StructureDefinition/MyPatientProfile"
       }
     ]
   }

Example request with a ``Resource`` body:

.. code-block::

   POST <firely_server_endpoint>/$validate?profile=https://example.org/fhir/StructureDefinition/MyPatientProfile

.. code-block::

   {
    "resourceType": "Patient",
    [...]
   }

.. _feature_validation_type:

Validate on the ResourceType level
----------------------------------

.. code-block::

   POST <firely_server_endpoint>/<resourcetype>/$validate[?profile=<canonical-url-of-structuredefinition>]

At the type level, Firely Server verifies that the submitted resource type matches the
``<resourcetype>`` in the request URL.

.. _feature_validation_instance:

Validate an instance from the database
--------------------------------------

.. code-block::

   GET / POST <firely_server_endpoint>/<resourcetype>/<id>/$validate[?profile=<canonical-url-of-structuredefinition>]

At this level, Firely Server validates a stored resource. The optional ``profile`` parameter
can be used to validate against a specific profile.

.. _feature_validation_pre:

Precondition
------------

Firely Server must be aware of all the StructureDefinitions referenced directly via parameter or indirectly by a profile in ``meta.profile``. Refer to the :ref:`conformance` for more information.

Special considerations
----------------------

.. warning::

   When validating a ``Parameters`` resource, the request is inherently ambiguous,
   because the ``$validate`` operation itself also accepts a ``Parameters`` resource
   as input.

   To avoid this ambiguity, the ``Parameters`` resource to be validated **must be
   wrapped inside another ``Parameters`` resource**, using the ``resource`` parameter.

   In other words, the outer ``Parameters`` resource represents the operation input,
   while the inner ``Parameters`` resource is the actual resource being validated.

Example:

.. code-block:: json

   {
     "resourceType": "Parameters",
     "parameter": [
       {
         "name": "resource",
         "resource": {
           "resourceType": "Parameters",
           "parameter": [
             {
               "name": "example",
               "valueString": "test"
             }
           ]
         }
       }
     ]
   }

.. _`$validate`: http://www.hl7.org/implement/standards/fhir/resource-operations.html#validate
