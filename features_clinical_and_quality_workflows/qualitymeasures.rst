.. _feature_qualitymeasures:

Executing Digital Quality Measures (dQMs) - $cql, $evaluate, $evaluate-measure, $data-requirements
==================================================================================================

.. note::

  The features described on this page are available in the following :ref:`Firely Server editions <vonk_overview>`:

  * Firely dQM - 🌍 / 🇺🇸

.. important::

   Please see :ref:`feature_qdm` for an introduction to Digital Quality Reporting in FHIR.

FHIR provides several operations for executing Digital Quality Measures (dQMs), either partially (e.g. evaluating expressions) or fully (evaluating a complete measure). The appropriate operation depends on the specific use case and execution goal:

* ``Library/$evaluate`` is most commonly used for debugging purposes. dQMs frequently reference multiple ``Library`` resources to encapsulate modular logic.  When a measure produces unexpected results—such as an incorrect or zero score—it is often useful to investigate why a particular subject meets or fails to meet specific population criteria (e.g., initial population, denominator, numerator). In such cases, it can be helpful to execute a targeted set of CQL expressions or evaluate specific sub-libraries within the measure. This allows implementers to isolate and verify individual components of the logic without executing the entire measure.

* ``Measure/$measure-evaluate`` is the primary operation for executing a digital quality measure (dQM) as a whole. This operation evaluates a ``Measure`` resource against a specified subject (such as a patient, group) using the measurement period and any associated ``Library`` parameters. ``Measure/$evaluate-measure`` is typically used in production or formal testing scenarios to generate actual measure scores. It is also suitable for automated execution in quality reporting workflows. Unlike ``Library/$evaluate``, which targets specific expressions, ``Measure/$evaluate-measure`` executes the full population logic and scoring methodology defined in the measure, making it the most comprehensive method for end-to-end dQM evaluation.

* ``$cql`` allows direct execution of CQL expressions, either inline or from referenced libraries. It is useful for rapid testing or prototyping when measure logic needs to be validated independently of a ``Measure`` resource.

For the preperation of the execution data-requirements can be gathered using the following operations:

* ``Library/$data-requirements`` retrieves a structured representation of the data required to evaluate a measure, making it a supporting operation for both ``Library/$evaluate`` and ``Measure/$evaluate-measure``. This operation identifies the necessary FHIR data types and elements, including value sets, code filters, and date constraints. It returns a ``Library`` resource containing ``DataRequirement`` elements, which describe the expected inputs for measure evaluation. This operation is particularly useful during implementation, data mapping, and integration planning, as it helps clarify what data must be available for successful execution of a digital quality measure.

* ``Measure/$data-requirements`` functions identically, but aggregates the data requirements across all libraries referenced by the measure, providing a complete picture of the inputs needed for measure evaluation.

----

.. _feature_library_evaluate:

Library/$evaluate
-----------------

The ``Library/$evaluate`` operation executes one or more named CQL expressions within a FHIR ``Library`` resource in a given data context (e.g. a patient), and returns the evaluated results. It is primarily used to inspect and debug the logic underlying digital quality measures by allowing targeted execution of individual expressions without running the full measure.

Overview
~~~~~~~~

**Operation name**
  ``Library/$evaluate``

**FHIR specification**
  `Using CQL with FHIR Implementation Guide - v2.0.0 <https://hl7.org/fhir/uv/cql/OperationDefinition-cql-library-evaluate.html>`_

**OperationDefinition**
  ``http://hl7.org/fhir/uv/cql/OperationDefinition/cql-library-evaluate``

**Scope**
  - Invocation level: ``type`` / ``instance``
  - Supported resource type(s): ``Library``
  - Idempotent: ``yes``
  - Affects server state: ``no``

**HTTP methods**
  - ``POST`` (type and instance level)
  - ``GET`` (type and instance level, when all parameters can be provided as query parameters)

.. _feature_library_evaluate_configuration:

Configuration
~~~~~~~~~~~~~ 

The ``Library/$evaluate`` operation is provided by the
``Vonk.Plugin.Cql.Operations.Library.Evaluate`` namespace.

You can enable or disable this operation by including or excluding this
namespace in the Firely Server pipeline options. See :ref:`vonk_available_plugins`
for more information on configuring available plugins.

You can configure the behavior of this operation using the
``LibraryEvaluateOperation`` section in the appsettings.


Database requirements
^^^^^^^^^^^^^^^^^^^^^

Execution of dQMs relies on retrieving clinical data from the Firely Server
data store. Internally, Firely Server uses the ``$everything`` operation to
collect all relevant data for a subject.

This functionality is only supported when the data store is backed by
MongoDB or SQL Server. Therefore, to execute dQMs against data stored in
Firely Server, the primary FHIR data database must use either MongoDB or
SQL Server.

In addition, the ``Vonk.Plugin.PatientEverything`` plugin must be enabled
in the pipeline options, as it provides the ``$everything`` operation used
during data retrieval. See :ref:`vonk_available_plugins` for more information
on configuring available plugins.

The administration database (used for conformance resources such as
``Library`` and ``Measure``) can still be hosted on SQLite.

Alternatively, you can configure Firely Server to use only external data
sources by enabling the ``RemoteDataEndpointsOnly`` setting. In that case,
no local data retrieval (and thus no ``$everything`` support) is required.


External data endpoints
^^^^^^^^^^^^^^^^^^^^^^^

Firely Server can retrieve clinical and claims data from external FHIR endpoints
during execution of ``Library/$evaluate``.

This is used when the ``useServerData`` parameter is set to ``false`` in a request.
In that case, data is not retrieved from the local Firely Server database, but from
a configured external endpoint.

::

  "LibraryEvaluateOperation": {
    "RemoteDataEndpointsOnly": false,
    "DataEndpoint": [
      //{
      //    "Endpoint": "<base url>",
      //    "RemoteDataEndpointAuthentication": "Jwt",
      //    "ClientId": "",
      //    "ClientSecret": "",
      //    "TokenEndpoint": "",
      //    "Audience": "",
      //    "Scopes": "system/*.rs"
      //}
      "ForwardedHeaders": [
        "X-Custom-Auth-Header"
      ]
    ]
  }

The ``DataEndpoint`` setting defines a list of pre-configured external FHIR endpoints.
Each endpoint can be referenced in a request using the ``dataEndpoint`` parameter.

Any ``dataEndpoint`` parameter provided in a request must match one of the
configured endpoints.

Each ``DataEndpoint`` entry supports the following fields:

- ``Endpoint``: Base URL of the external FHIR server  
- ``ClientId`` / ``ClientSecret``: Credentials for authentication (if required)  
- ``TokenEndpoint``: OAuth2 token endpoint (used for JWT authentication)  
- ``Audience``: Optional audience claim for the access token  
- ``Scopes``: Space-separated list of **SMART on FHIR scopes**. Since Firely Server uses a ``client_credentials``
  flow, only system-level scopes should be used (e.g. ``system/*.rs``). 
- ``RemoteDataEndpointAuthentication``: Defines how Firely Server authenticates
  against the endpoint. Supported values include ``JWT`` and ``None``

.. important::
  
   Firely Server expects that the response of the remote ``$everything`` operation
   is returned as a single Bundle page. Pagination is not supported in this context,
   and Firely Server will not follow additional pages (e.g. via ``link[relation="next"]``)
   returned by the remote endpoint.  

The ``ForwardedHeaders`` setting can be used to forward custom HTTP headers
from the incoming request to external data endpoints.

Any headers listed in ``ForwardedHeaders`` are copied from the original request
to Firely Server and included in outgoing requests to configured
``DataEndpoint`` entries. This can be used to propagate request-specific context, such as correlation IDs
or custom authorization headers, to external systems.

Supported parameters
^^^^^^^^^^^^^^^^^^^^

Firely Server supports the following parameters:

+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| Parameter               | Supported | Type                    | Cardinality | Additional Notes               |
+=========================+===========+=========================+=============+================================+
| ``url``                 | ✅        | ``canonical``           | 0..1        | Specifies the ``Library`` to   |
|                         |           |                         |             | evaluate via canonical URL.    |
|                         |           |                         |             | Earlier versions of the IG     |
|                         |           |                         |             | used ``library`` for this.     |
|                         |           |                         |             |                                |
|                         |           |                         |             | Since v2.0.0, ``library`` is   |
|                         |           |                         |             | redefined to pass an inline    |
|                         |           |                         |             | ``Library`` resource. Firely   |
|                         |           |                         |             | Server uses ``url`` only for   |
|                         |           |                         |             | external logic.                |
|                         |           |                         |             |                                |
|                         |           |                         |             | Versioned canonical references |
|                         |           |                         |             | are allowed, e.g.,             |
|                         |           |                         |             | ``http://example.org/fhir/     |
|                         |           |                         |             | Library/MyLogic|1.0.0``.       |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``library``             | ✅        | ``Library`` resource    | 0..1        | In-line logic library that     |
|                         |           |                         |             | contains executable CQL logic. |
|                         |           |                         |             | This Library will not be       |
|                         |           |                         |             | stored in Firely Server. It    |
|                         |           |                         |             | MAY only contain CQL and will  |
|                         |           |                         |             | be compiled dynamically. If    |
|                         |           |                         |             | ELM content is provided, it    |
|                         |           |                         |             | will be re-used.               |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``subject``             | ✅        | ``string``              | 0..1        | Only Patient references are    |
|                         |           |                         |             | supported, may be omitted if   |
|                         |           |                         |             | no "context Patient" is        |
|                         |           |                         |             | included in the library.       |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``expression``          | ✅        | ``reference``           | 0..*        | The name of the expression to  |
|                         |           |                         |             | evaluate. If omitted, all      |
|                         |           |                         |             | expressions in the library are |
|                         |           |                         |             | evaluated.                     |
|                         |           |                         |             |                                |
|                         |           |                         |             | `CQL Access Modifier <https:// |
|                         |           |                         |             | build.fhir.org/ig/HL7/fhir-    |
|                         |           |                         |             | extensions/StructureDefinition |
|                         |           |                         |             | -cqf-cqlAccessModifier.html>`_ |
|                         |           |                         |             | extensions are not taken into  |
|                         |           |                         |             | account.                       |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``parameters``          | ✅        | ``Parameters`` resource | 0..1        | Input parameters passed into   |
|                         |           |                         |             | the evaluation context.        |
|                         |           |                         |             |                                |
|                         |           |                         |             | These will be mapped from FHIR |
|                         |           |                         |             | data types to CQL data types   |
|                         |           |                         |             | according to the `FHIR Type    |
|                         |           |                         |             | Mapping <https://build.fhir.or |
|                         |           |                         |             | g/ig/HL7/cql-ig/conformance.ht |
|                         |           |                         |             | ml#fhir-type-mapping>`_.       |
|                         |           |                         |             |                                |
|                         |           |                         |             | Most notably, this includes    |
|                         |           |                         |             | passing in the measurement     |
|                         |           |                         |             | period parameter as a FHIR     |
|                         |           |                         |             | Period.                        |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``raw``                 | ✅        | ``boolean``             | 0..1        | Return the library results as  |
|                         |           |                         |             | a string without mapping the   |
|                         |           |                         |             | CQL result data types back to  |
|                         |           |                         |             | FHIR.                          |
|                         |           |                         |             |                                |
|                         |           |                         |             | This is a proprietary          |
|                         |           |                         |             | parameter of Firely Server.    |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``useServerData``       | ✅        | ``boolean``             | 0..1        | When ``true``, claims and      |
|                         |           |                         |             | clinical data are retrieved    |
|                         |           |                         |             | from the Firely Server         |
|                         |           |                         |             | database where the operation   |
|                         |           |                         |             | is executed.                   |
|                         |           |                         |             |                                |
|                         |           |                         |             | When ``false``, claims and     |
|                         |           |                         |             | clinical data are retrieved    |
|                         |           |                         |             | from the ``dataEndpoint``      |
|                         |           |                         |             | parameter.                     |
|                         |           |                         |             |                                |
|                         |           |                         |             | In both cases, any data passed |
|                         |           |                         |             | via the ``data`` parameter     |
|                         |           |                         |             | takes precedence.              |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``data``                | ✅        | ``Bundle``              | 0..1        | Inline FHIR data bundle to use |
|                         |           |                         |             | as the data context during     |
|                         |           |                         |             | evaluation.                    |
|                         |           |                         |             |                                |
|                         |           |                         |             | The bundle type SHOULD be      |
|                         |           |                         |             | either ``collection`` or       |
|                         |           |                         |             | ``searchset`` (as the output   |
|                         |           |                         |             | of a $everything operation).   |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``dataEndpoint``        | ✅        | ``Endpoint`` resource   | 0..1        | Used only when                 |
|                         |           |                         |             | ``useServerData`` is ``false``.|
|                         |           |                         |             | Defines the external FHIR      |
|                         |           |                         |             | endpoint from which claims and |
|                         |           |                         |             | clinical data are retrieved.   |
|                         |           |                         |             |                                |
|                         |           |                         |             | The endpoint must be           |
|                         |           |                         |             | pre-registered in              |
|                         |           |                         |             | ``LibraryEvaluateOperation``   |
|                         |           |                         |             | via the ``DataEndpoint``       |
|                         |           |                         |             | option.                        |
|                         |           |                         |             | See :ref:`dqm_appsettings`.    |
|                         |           |                         |             | Data supplied via the ``data`` |
|                         |           |                         |             | parameter always takes         |
|                         |           |                         |             | precedence.                    |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``prefetchData``        | ❌        | Complex                 | 0..*        |                                |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``includePrivate``      | ❌        | ``boolean``             | 0..1        |                                |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``contentEndpoint``     | ❌        | ``Endpoint`` resource   | 0..1        |                                |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``terminologyEndpoint`` | ❌        | ``Endpoint`` resource   | 0..1        | External terminology services  |
|                         |           |                         |             | should be configured via the   |
|                         |           |                         |             | :ref:`feature_terminology`     |
|                         |           |                         |             | options.                       |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+

.. important::

   If the Library references any ``ValueSet`` resources, they must be preloaded into the Firely Server's administration endpoint **before** executing the Library.

Output parameters
~~~~~~~~~~~~~~~~~

The ``Library/$evaluate`` operation returns a ``Parameters`` resource containing
the results of the evaluated CQL expressions.

+-------------------------+-------------------------+-------------+--------------------------------+
| Parameter               | Type                    | Cardinality | Description                    |
+=========================+=========================+=============+================================+
| ``return``              | Parameters              | 1..1        | A Parameters resource in which |
|                         |                         |             | each output parameter          |
|                         |                         |             | corresponds to a named CQL     |
|                         |                         |             | expression. Each entry         |
|                         |                         |             | includes the expression name,  |
|                         |                         |             | its evaluated value, and       |
|                         |                         |             | optional type information via  |
|                         |                         |             | an extension (                 |
|                         |                         |             | ``cqf-cqlType``).              |
+-------------------------+-------------------------+-------------+--------------------------------+

When to use this operation
~~~~~~~~~~~~~~~~~~~~~~~~~~

Use ``Library/$evaluate`` when you want to:

- evaluate specific expressions within a Library
- debug measure logic
- inspect intermediate results of CQL execution

Example: Type-Level Library/$evaluate Invocation
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

This example evaluates the ``bp-check-logic`` library (version 1.0.0) against a specific patient
and a defined measurement period using a ``POST`` request to the type-level operation.

**Request**

.. code-block::

   POST [base]/Library/$evaluate

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

Given matching input data (see :ref:`feature_qdm_example_library` for context), specifically, a ``Patient`` resource and an ``Observation`` with a ``code`` of ``8480-6`` from the LOINC CodeSystem, and an ``effectiveDateTime`` that falls within the measurement period — the following output will be returned:

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

.. _feature_measure_evaluate:

Measure/$evaluate-measure
-------------------------

The ``Measure/$evaluate-measure`` operation executes a complete digital quality measure (dQM) and returns the calculated results for a given subject or population. It evaluates all referenced ``Library`` resources, applies the defined population criteria (e.g. initial population, denominator, numerator), and computes the final measure score.

This operation is the primary mechanism for **end-to-end measure evaluation** and is typically used in production or formal testing scenarios.

Overview
~~~~~~~~

**Operation name**
  ``Measure/$evaluate-measure``

**FHIR specification**
  `FHIR Core Measure Evaluation <https://hl7.org/fhir/measure-operation-evaluate-measure.html>`_  

**OperationDefinition**
  ``http://hl7.org/fhir/OperationDefinition/Measure-evaluate-measure``

**Scope**
  - Invocation level: ``type`` / ``instance``
  - Supported resource type(s): ``Measure``
  - Idempotent: ``yes``
  - Affects server state: **conditional**

**HTTP methods**
  - ``POST`` (type level)
  - ``GET`` (type level, when all parameters can be provided as query parameters)

.. important::

   Invocation at the instance level (``[base]/Measure/[id]/$evaluate-measure``)
   is not currently supported. Use the type-level operation with the
   ``measure`` parameter instead.


.. note::

   The operation can optionally affect server state depending on the ``persist`` parameter.

   When ``persist`` is set to ``true``, the generated ``MeasureReport`` is stored
   on the server. By default (``persist = false``), the report is returned in the
   response only and is not persisted.

Configuration
~~~~~~~~~~~~~

The ``Measure/$evaluate-measure`` operation is provided by the
``Vonk.Plugin.Cql.Operations.Measure.EvaluateMeasure`` namespace.

You can enable or disable this operation by including or excluding this
namespace in the Firely Server pipeline options. See :ref:`vonk_available_plugins`
for more information.

Supported parameters
^^^^^^^^^^^^^^^^^^^^

Firely Server supports the following parameters:

+-------------------+-----------+-------------------------+-------------+---------------------------------------------+
| Parameter         | Supported | Type                    | Cardinality | Additional Notes                            |
+===================+===========+=========================+=============+=============================================+
| ``url``           | ✅        | ``canonical``           | 0..1        | Canonical URL of the Measure to evaluate.   |
|                   |           |                         |             |                                             |
|                   |           |                         |             | Required for type-level invocation.         |
|                   |           |                         |             |                                             |
|                   |           |                         |             | Versioned canonical references are allowed, |
|                   |           |                         |             | e.g.,                                       |
|                   |           |                         |             | ``http://example.org/fhir/Measure/          |
|                   |           |                         |             | ExampleMeasure|1.0.0``.                     |
+-------------------+-----------+-------------------------+-------------+---------------------------------------------+
| ``subject``       | ✅        | ``string``              | 1..1        | Reference to the subject for which the      |
|                   |           |                         |             | measure is evaluated.                       |
|                   |           |                         |             |                                             |
|                   |           |                         |             | Supported resource types are ``Patient``    |
|                   |           |                         |             | and ``Group``.                              |
|                   |           |                         |             |                                             |
|                   |           |                         |             | When a ``Patient`` is provided, the measure |
|                   |           |                         |             | is evaluated for that single subject.       |
|                   |           |                         |             |                                             |
|                   |           |                         |             | When a ``Group`` is provided, the measure   |
|                   |           |                         |             | is evaluated for all ``Patient`` references |
|                   |           |                         |             | contained in the Group.                     |
+-------------------+-----------+-------------------------+-------------+---------------------------------------------+
| ``periodStart``   | ✅        | ``date``                | 1..1        | Start of the measurement period.            |
+-------------------+-----------+-------------------------+-------------+---------------------------------------------+
| ``periodEnd``     | ✅        | ``date``                | 1..1        | End of the measurement period.              |
+-------------------+-----------+-------------------------+-------------+---------------------------------------------+
| ``reportType``    | ✅        | ``code``                | 0..1        | The type of measure report:                 |
|                   |           |                         |             |                                             |
|                   |           |                         |             | - ``individual``: Evaluates the measure for |
|                   |           |                         |             |   a single subject (e.g. Patient or Group)  |
|                   |           |                         |             |   and returns population membership and     |
|                   |           |                         |             |   score for that subject.                   |
|                   |           |                         |             |                                             |
|                   |           |                         |             | - ``summary``: Evaluates the measure across |
|                   |           |                         |             |   a population of subjects and returns      |
|                   |           |                         |             |   aggregated counts (e.g. numerator,        |
|                   |           |                         |             |   denominator).                             |
|                   |           |                         |             |                                             |
|                   |           |                         |             | The ``subject-list`` report type defined in |
|                   |           |                         |             | the FHIR specification is not supported.    |
|                   |           |                         |             |                                             |
|                   |           |                         |             | If not specified, the default is            |
|                   |           |                         |             | ``individual``.                             |
+-------------------+-----------+-------------------------+-------------+---------------------------------------------+
| ``parameters``    | ✅        | ``Parameters`` resource | 0..1        | See ``Library/$evaluate`` configuration     |
|                   |           |                         |             | for details.                                |
+-------------------+-----------+-------------------------+-------------+---------------------------------------------+
| ``useServerData`` | ✅        | ``boolean``             | 0..1        | See ``Library/$evaluate`` configuration     |
|                   |           |                         |             | for details.                                |
+-------------------+-----------+-------------------------+-------------+---------------------------------------------+
| ``data``          | ✅        | ``Bundle``              | 0..1        | See ``Library/$evaluate`` configuration     |
|                   |           |                         |             | for details.                                |
+-------------------+-----------+-------------------------+-------------+---------------------------------------------+
| ``dataEndpoint``  | ✅        | ``Endpoint``            | 0..1        | See ``Library/$evaluate`` configuration     |
|                   |           |                         |             | for details.                                |
+-------------------+-----------+-------------------------+-------------+---------------------------------------------+
| ``persist``       | ✅        | ``boolean``             | 0..1        | When ``true``, the generated                |
|                   |           |                         |             | ``MeasureReport`` is stored on the server.  |
|                   |           |                         |             |                                             |
|                   |           |                         |             | When ``false`` (default), the result is     |
|                   |           |                         |             | returned in the response only.              |
|                   |           |                         |             |                                             |
|                   |           |                         |             | This is a proprietary parameter of Firely   |
|                   |           |                         |             | Server.                                     |
+-------------------+-----------+-------------------------+-------------+---------------------------------------------+
| ``raw``           | ✅        | ``boolean``             | 0..1        | Return the results as a string without      |
|                   |           |                         |             | mapping the CQL result data types back to   |
|                   |           |                         |             | FHIR.                                       |
|                   |           |                         |             |                                             |
|                   |           |                         |             | This is a proprietary parameter of Firely   |
|                   |           |                         |             | Server.                                     |
+-------------------+-----------+-------------------------+-------------+---------------------------------------------+
| ``measure``       | ❌        | ``Measure``             | 0..1        |                                             |
+-------------------+-----------+-------------------------+-------------+---------------------------------------------+
| ``version``       | ❌        | ``string``              | 0..1        |                                             |
+-------------------+-----------+-------------------------+-------------+---------------------------------------------+
| ``provider``      | ❌        | ``string``              | 0..1        |                                             |
+-------------------+-----------+-------------------------+-------------+---------------------------------------------+
| ``location``      | ❌        | ``string``              | 0..1        |                                             |
+-------------------+-----------+-------------------------+-------------+---------------------------------------------+
| ``lastReceivedOn``| ❌        | ``dateTime``            | 0..1        |                                             |
+-------------------+-----------+-------------------------+-------------+---------------------------------------------+

Output
~~~~~~

The operation returns a ``MeasureReport`` resource containing the evaluation results.

The report includes:

- population counts (e.g. initial population, denominator, numerator)
- measure score (if applicable)
- subject-level or population-level results depending on ``reportType``

When to use this operation
~~~~~~~~~~~~~~~~~~~~~~~~~~

Use ``Measure/$evaluate-measure`` when you want to:

- execute a full digital quality measure
- calculate population membership and scores
- generate results for reporting or submission
- validate measure behavior in end-to-end scenarios

Example: Type-Level Measure Evaluation
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

**Request**

.. code-block::

   POST [base]/Measure/$evaluate-measure

**Request Body**

.. code-block:: json

   {
     "resourceType": "Parameters",
     "parameter": [
       {
         "name": "url",
         "valueCanonical": "http://example.org/fhir/Measure/ExampleMeasure|1.0.0"
       },
       {
         "name": "subject",
         "valueString": "Patient/cql-patient-test"
       },
       {
         "name": "periodStart",
         "valueDate": "2023-01-01"
       },
       {
         "name": "periodEnd",
         "valueDate": "2023-12-31"
       }
     ]
   }

**Response Body**

.. code-block:: json

   {
     "resourceType": "MeasureReport",
     "status": "complete",
     "type": "individual",
     "measure": "http://example.org/fhir/Measure/ExampleMeasure|1.0.0",
     "subject": {
       "reference": "Patient/cql-patient-test"
     },
     "period": {
       "start": "2023-01-01",
       "end": "2023-12-31"
     },
     "group": [
       {
         "population": [
           {
             "code": {
               "coding": [
                 {
                   "system": "http://terminology.hl7.org/CodeSystem/measure-population",
                   "code": "initial-population"
                 }
               ]
             },
             "count": 1
           },
           {
             "code": {
               "coding": [
                 {
                   "system": "http://terminology.hl7.org/CodeSystem/measure-population",
                   "code": "denominator"
                 }
               ]
             },
             "count": 1
           },
           {
             "code": {
               "coding": [
                 {
                   "system": "http://terminology.hl7.org/CodeSystem/measure-population",
                   "code": "numerator"
                 }
               ]
             },
             "count": 1
           }
         ],
         "measureScore": {
           "value": 1.0
         }
       }
     ]
   }

.. _feature_cql_operation:

$cql
----

The ``$cql`` operation executes a CQL expression directly and returns the
evaluated result. It is useful for rapid testing or prototyping when CQL logic
needs to be validated independently of a ``Library`` or ``Measure`` resource.

Overview
~~~~~~~~

**Operation name**
  ``$cql``


**FHIR specification**
  `Using CQL with FHIR Implementation Guide - v2.0.0 <https://build.fhir.org/ig/HL7/cql-ig/OperationDefinition-cql-cql.html>`_


**OperationDefinition**
  ``http://hl7.org/fhir/uv/cql/OperationDefinition/cql-cql``

**Scope**
  - Invocation level: ``system``
  - Idempotent: ``yes``
  - Affects server state: ``no``

**HTTP methods**
  - ``POST``

Supported parameters
^^^^^^^^^^^^^^^^^^^^

Firely Server supports the following parameters:

+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| Parameter               | Supported | Type                    | Cardinality | Additional Notes               |
+=========================+===========+=========================+=============+================================+
| ``expression``          | ✅        | ``string``              | 1..1        | Specifies an inline CQL        |
|                         |           |                         |             | expression to be executed.     |
|                         |           |                         |             |                                |
|                         |           |                         |             | Only a single statement is     |
|                         |           |                         |             | supported per request. It      |
|                         |           |                         |             | cannot operate within a        |
|                         |           |                         |             | context (e.g. Patient) and     |
|                         |           |                         |             | will not execute correctly if  |
|                         |           |                         |             | input parameters are needed.   |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``subject``             | ✅        | ``string``              | 0..1        | Only Patient references are    |
|                         |           |                         |             | supported.                     |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``parameters``          | ✅        | ``Parameters``          | 0..1        | Input parameters passed into   |
|                         |           |                         |             | the evaluation context. See    |
|                         |           |                         |             | ``Library/$evaluate``          |
|                         |           |                         |             | configuration for details.     |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``raw``                 | ✅        | ``boolean``             | 0..1        | Return the execution results   |
|                         |           |                         |             | as a string without mapping    |
|                         |           |                         |             | the CQL result data types back |
|                         |           |                         |             | to FHIR.                       |
|                         |           |                         |             |                                |
|                         |           |                         |             | This is a proprietary          |
|                         |           |                         |             | parameter of Firely Server.    |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``library``             | ❌        | Complex                 | 0..*        |                                |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``useServerData``       | ❌        | ``boolean``             | 0..1        |                                |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``data``                | ❌        | ``Bundle``              | 0..1        |                                |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``prefetchData``        | ❌        | Complex                 | 0..*        |                                |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``dataEndpoint``        | ❌        | ``Endpoint``            | 0..1        |                                |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``contentEndpoint``     | ❌        | ``Endpoint``            | 0..1        |                                |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``terminologyEndpoint`` | ❌        | ``Endpoint``            | 0..1        |                                |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+

Output
~~~~~~

The operation returns a ``Parameters`` resource containing the result of the
evaluated CQL expression.

The result is returned in a parameter named ``return``. The value is mapped back
to a FHIR data type, unless the proprietary ``raw`` parameter is set to ``true``.

When to use this operation
~~~~~~~~~~~~~~~~~~~~~~~~~~

Use ``$cql`` when you want to:

- quickly test a simple CQL expression
- validate basic CQL syntax or behavior
- prototype logic before moving it into a ``Library``
- execute logic that does not require a full ``Measure`` evaluation

Example: System-Level $cql Invocation
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

This examples demonstrates a simple calculation executed via the dQM engine.

**Request**

.. code-block::

   POST [base]/$cql

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