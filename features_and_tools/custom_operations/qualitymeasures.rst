.. _feature_qualitymeasures:

Executing Digital Quality Measures (dQMs) - $evaluate and $evaluate-measure
===========================================================================

.. note::

  The features described on this page are available in the following :ref:`Firely Server editions <vonk_overview>`:

  * Firely dQM - 🌍 / 🇺🇸

.. important::

   Please see :ref:`feature_qdm` for an introduction to Digital Quality Reporting in FHIR.

FHIR provides several mechanisms for executing dQMs, either in full or in part. The appropriate operation depends on the specific use case and execution goal:

* ``Library/$evaluate`` is most commonly used for debugging purposes. dQMs frequently reference multiple ``Library`` resources to encapsulate modular logic.  When a measure produces unexpected results—such as an incorrect or zero score—it is often useful to investigate why a particular subject meets or fails to meet specific population criteria (e.g., initial population, denominator, numerator). In such cases, it can be helpful to execute a targeted set of CQL expressions or evaluate specific sub-libraries within the measure. This allows implementers to isolate and verify individual components of the logic without executing the entire measure.

* ``Measure/$measure-evaluate`` is the primary operation for executing a digital quality measure (dQM) as a whole. This operation evaluates a ``Measure`` resource against a specified subject (such as a patient, group) using the measurement period and any associated ``Library`` parameters. ``Measure/$evaluate-measure`` is typically used in production or formal testing scenarios to generate actual measure scores. It is also suitable for automated execution in quality reporting workflows. Unlike ``Library/$evaluate``, which targets specific expressions, ``Measure/$evaluate-measure`` executes the full population logic and scoring methodology defined in the measure, making it the most comprehensive method for end-to-end dQM evaluation.

* ``Library/$data-requirements`` retrieves a structured representation of the data required to evaluate a measure, making it a supporting operation for both ``Library/$evaluate`` and ``Measure/$evaluate-measure``. This operation identifies the necessary FHIR data types and elements, including value sets, code filters, and date constraints. It returns a ``Library`` resource containing ``DataRequirement`` elements, which describe the expected inputs for measure evaluation. This operation is particularly useful during implementation, data mapping, and integration planning, as it helps clarify what data must be available for successful execution of a digital quality measure.

* ``$cql`` allows direct execution of CQL expressions, either inline or from referenced libraries. It is useful for rapid testing or prototyping when measure logic needs to be validated independently of a ``Measure`` resource.


----

.. _feature_library_evaluate:

Library/$evaluate
-----------------

Firely Server's implementation of ``Library/$evaluate`` is based on version 2.0.0 of the 
`Using CQL with FHIR Implementation Guide <https://build.fhir.org/ig/HL7/cql-ig/>`_. For the formal specification of this operation, refer to the 
`CQL Library Evaluate OperationDefinition <https://build.fhir.org/ig/HL7/cql-ig/OperationDefinition-cql-library-evaluate.html>`_.

Supported parameters
^^^^^^^^^^^^^^^^^^^^

Firely Server supports the following parameters:

+-------------------------+-----------+-------------------------+--------------------------------+
| Parameter               | Supported | Type                    | Additional Notes               |
+=========================+===========+=========================+================================+
| ``url``                 | ✅        | ``canonical``           | Specifies the ``Library`` to   |
|                         |           |                         | evaluate via canonical URL.    |
|                         |           |                         | Earlier versions of the IG     |
|                         |           |                         | used ``library`` for this.     |
|                         |           |                         |                                |
|                         |           |                         | Since v2.0.0, ``library`` is   |
|                         |           |                         | redefined to pass an inline    |
|                         |           |                         | ``Library`` resource. Firely   |
|                         |           |                         | Server uses ``url`` only for   |
|                         |           |                         | external logic.                |
|                         |           |                         |                                |
|                         |           |                         | Versioned canonical references |
|                         |           |                         | are allowed, e.g.,             |
|                         |           |                         | ``http://example.org/fhir/     |
|                         |           |                         | Library/MyLogic|1.0.0``.       |
+-------------------------+-----------+-------------------------+--------------------------------+
| ``library``             | ✅        | ``Library`` resource    | In-line logic library that     |
|                         |           |                         | contains executable CQL logic. |
|                         |           |                         | This Library will not be       |
|                         |           |                         | stored in Firely Server. It    |
|                         |           |                         | MAY only contain CQL and will  |
|                         |           |                         | be compiled dynamically. If    |
|                         |           |                         | ELM content is provided, it    |
|                         |           |                         | will be re-used.               |
+-------------------------+-----------+-------------------------+--------------------------------+
| ``subject``             | ✅        | ``string``              | Only Patient references are    |
|                         |           |                         | supported, may be omitted if   |
|                         |           |                         | no "context Patient" is        |
|                         |           |                         | included in the library.       |
+-------------------------+-----------+-------------------------+--------------------------------+
| ``expression``          | ✅        | ``reference``           | The name of the expression to  |
|                         |           |                         | evaluate. If omitted, all      |
|                         |           |                         | expressions in the library are |
|                         |           |                         | evaluated.                     |
|                         |           |                         |                                |
|                         |           |                         | `CQL Access Modifier <https:// |
|                         |           |                         | build.fhir.org/ig/HL7/fhir-    |
|                         |           |                         | extensions/StructureDefinition |
|                         |           |                         | -cqf-cqlAccessModifier.html>`_ |
|                         |           |                         | extensions are not taken into  |
|                         |           |                         | account.                       |
+-------------------------+-----------+-------------------------+--------------------------------+
| ``parameters``          | ✅        | ``Parameters`` resource | Input parameters passed into   |
|                         |           |                         | the evaluation context.        |
|                         |           |                         |                                |
|                         |           |                         | These will be mapped from FHIR |
|                         |           |                         | data types to CQL data types   |
|                         |           |                         | according to the `FHIR Type    |
|                         |           |                         | Mapping <https://build.fhir.or |
|                         |           |                         | g/ig/HL7/cql-ig/conformance.ht |
|                         |           |                         | ml#fhir-type-mapping>`_.       |
|                         |           |                         |                                |
|                         |           |                         | Most notably, this includes    |
|                         |           |                         | passing in the measurement     |
|                         |           |                         | period parameter as a FHIR     |
|                         |           |                         | Period.                        |
+-------------------------+-----------+-------------------------+--------------------------------+
| ``raw``                 | ✅        | ``boolean``             | Return the library results as  |
|                         |           |                         | a string without mapping the   |
|                         |           |                         | CQL result data types back to  |
|                         |           |                         | FHIR.                          |
|                         |           |                         |                                |
|                         |           |                         | This is a proprietary          |
|                         |           |                         | parameter of Firely Server.    |
+-------------------------+-----------+-------------------------+--------------------------------+
| ``useServerData``       | ❌        | ``boolean``             |                                |
+-------------------------+-----------+-------------------------+--------------------------------+
| ``data``                | ✅        | ``Bundle``              | Inline FHIR data bundle to use |
|                         |           |                         | as the data context during     |
|                         |           |                         | evaluation.                    |
|                         |           |                         |                                |
|                         |           |                         | The bundle type SHOULD be      |
|                         |           |                         | either ``collection`` or       |
|                         |           |                         | ``searchset`` (as the output   |
|                         |           |                         | of a $everything operation).   |
+-------------------------+-----------+-------------------------+--------------------------------+
| ``prefetchData``        | ❌        | Complex                 |                                |
+-------------------------+-----------+-------------------------+--------------------------------+
| ``dataEndpoint``        | ❌        | ``Endpoint`` resource   |                                |
+-------------------------+-----------+-------------------------+--------------------------------+
| ``contentEndpoint``     | ❌        | ``Endpoint`` resource   |                                |
+-------------------------+-----------+-------------------------+--------------------------------+
| ``terminologyEndpoint`` | ❌        | ``Endpoint`` resource   | External terminology services  |
|                         |           |                         | should be configured via the   |
|                         |           |                         | :ref:`feature_terminology`     |
|                         |           |                         | options.                       |
+-------------------------+-----------+-------------------------+--------------------------------+

.. important::

   If the Library references any ``ValueSet`` resources, they must be preloaded into the Firely Server's administration endpoint **before** executing the Library.

The ``Library/$evaluate`` operation is supported as a ``POST`` request on both the type and instance levels.  
Additionally, the instance-level operation may also be invoked using ``GET``.

Example: Type-Level Library/$evaluate Invocation
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

This example evaluates the ``bp-check-logic`` library (version 1.0.0) against a specific patient
and a defined measurement period using a ``POST`` request to the type-level operation.

**Request**

.. code-block:: http

   POST [base]/Library/$evaluate HTTP/1.1
   Content-Type: application/fhir+json

**Request Body**

.. code-block:: json

   {
     "resourceType": "Parameters",
     "parameter": [
       {
         "name": "url",
         "valueCanonical": "http://example.org/fhir/Library/bp-check-logic|1.0.0"
       },
       {
         "name": "subject",
         "valueString": "Patient/cql-patient-test"
       },
       {
         "name": "parameters",
         "resource": {
           "resourceType": "Parameters",
           "parameter": [
             {
               "name": "Measurement Period",
               "valuePeriod": {
                 "start": "2023-01-01",
                 "end": "2023-12-01"
               }
             }
           ]
         }
       }
     ]
   }

**Response Body**

Given matching input data, specifically, a ``Patient`` resource and an ``Observation`` with a ``code`` of ``8480-6`` from the LOINC CodeSystem, and an ``effectiveDateTime`` that falls within the measurement period — the following output will be returned:

.. code-block:: json

    {
      "resourceType": "Parameters",
      "parameter": [
        {
          "extension": [
            {
              "url": "http://hl7.org/fhir/StructureDefinition/cqf-cqlType",
              "valueString": "Fhir"
            }
          ],
          "name": "Patient",
          "resource": {
            "resourceType": "Patient",
            "id": "cql-blood-pressure-check-test-match",
            "meta": {
              "versionId": "d36e61f8-300a-4c2f-8247-9fb4a6837236",
              "lastUpdated": "2025-05-23T18:32:44.106+00:00"
            },
            "birthDate": "1990-06-15"
          }
        },
        {
          "extension": [
            {
              "url": "http://hl7.org/fhir/StructureDefinition/cqf-cqlType",
              "valueString": "Boolean"
            }
          ],
          "name": "HasBPReading",
          "valueBoolean": true
        },
        {
          "extension": [
            {
              "url": "http://hl7.org/fhir/StructureDefinition/cqf-cqlType",
              "valueString": "Boolean"
            }
          ],
          "name": "AdultPatients",
          "valueBoolean": true
        }
      ]
    }

.. _feature_cql_operation:

$cql
----

Firely Server's implementation of ``$cql`` is based on version 2.0.0 of the 
`Using CQL with FHIR Implementation Guide <https://build.fhir.org/ig/HL7/cql-ig/>`_. For the formal specification of this operation, refer to the 
`$cql OperationDefinition <https://build.fhir.org/ig/HL7/cql-ig/OperationDefinition-cql-cql.html>`_.

Supported parameters
^^^^^^^^^^^^^^^^^^^^

Firely Server supports the following parameters:

+-------------------------+-----------+-------------------------+--------------------------------+
| Parameter               | Supported | Type                    | Additional Notes               |
+=========================+===========+=========================+================================+
| ``expression``          | ✅        | ``string``              | Specifies an inline CQL        |
|                         |           |                         | expression to be executed.     |
|                         |           |                         | Only a single statement is     |
|                         |           |                         | supported per request. It      |
|                         |           |                         | cannot operate within a        |
|                         |           |                         | context (e.g., Patient) and    |
|                         |           |                         | will not execute correctly if  |
|                         |           |                         | input parameters are needed.   |
+-------------------------+-----------+-------------------------+--------------------------------+
| ``subject``             | ❌        | ``string``              |                                |
+-------------------------+-----------+-------------------------+--------------------------------+
| ``parameters``          | ❌        | ``Parameters`` resource |                                |
+-------------------------+-----------+-------------------------+--------------------------------+
| ``library``             | ❌        | Complex                 |                                |
+-------------------------+-----------+-------------------------+--------------------------------+
| ``useServerData``       | ❌        | ``boolean``             |                                |
+-------------------------+-----------+-------------------------+--------------------------------+
| ``data``                | ❌        | ``Bundle``              |                                |
+-------------------------+-----------+-------------------------+--------------------------------+
| ``prefetchData``        | ❌        | Complex                 |                                |
+-------------------------+-----------+-------------------------+--------------------------------+
| ``dataEndpoint``        | ❌        | ``Endpoint`` resource   |                                |
+-------------------------+-----------+-------------------------+--------------------------------+
| ``contentEndpoint``     | ❌        | ``Endpoint`` resource   |                                |
+-------------------------+-----------+-------------------------+--------------------------------+
| ``terminologyEndpoint`` | ❌        | ``Endpoint`` resource   |                                |
+-------------------------+-----------+-------------------------+--------------------------------+
| ``raw``                 | ✅        | ``boolean``             | Return the execution results as|
|                         |           |                         | a string without mapping the   |
|                         |           |                         | CQL result data types back to  |
|                         |           |                         | FHIR.                          |
|                         |           |                         |                                |
|                         |           |                         | This is a proprietary          |
|                         |           |                         | parameter of Firely Server.    |
+-------------------------+-----------+-------------------------+--------------------------------+

Example: System-Level $cql Invocation
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

This examples demonstrates a simple calculation executed via the dQM engine.

**Request**

.. code-block:: http

   POST [base]/$cql HTTP/1.1
   Content-Type: application/fhir+json

**Request Body**

.. code-block:: json

   {
    "resourceType": "Parameters",
    "parameter": [
        {
            "name": "expression",
            "valueString": "'Hello'&' '&'World'"
        }
    ]
  }

**Response Body**

.. code-block:: json

   {
    "resourceType": "Parameters",
    "parameter": [
        {
            "extension": [
                {
                    "url": "http://hl7.org/fhir/StructureDefinition/cqf-cqlType",
                    "valueString": "String"
                }
            ],
            "name": "return",
            "valueString": "Hello World"
        }
    ]
  }