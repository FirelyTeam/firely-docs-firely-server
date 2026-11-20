.. _feature_sdc_populate:

Pre-population - Questionnaire/$populate
========================================

.. note::

  The features described on this page are available in the following :ref:`Firely Server editions <vonk_overview>`:

  * Firely Scale - 🌍 / 🇺🇸
  * Firely Prior Authorization - 🇺🇸

.. note::

  The operation requires the license tokens ``http://fire.ly/server/plugins/structured-data-capture`` and ``http://fire.ly/firely-validator``.
  See :ref:`feature_sdc` for how to enable Structured Data Capture.

The ``$populate`` operation fills in a ``QuestionnaireResponse`` with the answers that are already known, before the form is shown to the user. For example, it fills in the patient's name and date of birth from the ``Patient`` resource, and the latest body weight from an ``Observation``.
The Questionnaire says where each answer comes from, with the population extensions of the `SDC implementation guide <https://hl7.org/fhir/uv/sdc/populate.html>`_.

``$populate`` does not store anything. It returns the QuestionnaireResponse, which the form application shows to the user to check and complete. Population is best effort: an answer that cannot be found is left empty, and the problem is reported in the ``issues`` output, rather than failing the whole request.

The operation is defined in `SDC - OperationDefinition Questionnaire/$populate <https://hl7.org/fhir/uv/sdc/OperationDefinition-Questionnaire-populate.html>`_. Firely Server follows SDC 4.0.0.

----

Invoking $populate
------------------

``$populate`` is available on type level and on instance level:

* ``POST [base]/Questionnaire/$populate``

  The body is a ``Parameters`` resource that names the Questionnaire in its ``questionnaire`` or ``identifier`` parameter.

* ``POST [base]/Questionnaire/[id]/$populate``

  Populates the Questionnaire stored under ``[id]``. The body is optional. When there is one, it must be a ``Parameters`` resource, and it cannot contain the ``questionnaire`` or ``identifier`` parameter.

* ``GET [base]/Questionnaire/[id]/$populate``

  The same as the instance-level ``POST`` without a body, so without a subject or context. The only query parameters allowed are ``local`` and ``_format``. Any other query parameter is rejected with HTTP 400.

``$populate`` is available on FHIR R4 and R5 endpoints. On STU3 it is answered with HTTP 501. To use it on R5, ``Vonk.Fhir.R5`` must be included in the pipeline (see :ref:`feature_multiversion`).

Which Questionnaire is used
^^^^^^^^^^^^^^^^^^^^^^^^^^^

* On instance level, Firely Server reads the addressed Questionnaire like any other read of a resource, with the access rights of the caller.
* On type level, a Questionnaire named by a canonical url, a reference or an ``identifier`` is resolved in the :ref:`Administration database <conformance>`, as for ``$extract``. The caller must be allowed to read Questionnaires, otherwise the request is answered with HTTP 403.
* On type level, a Questionnaire can also be sent as a resource in the ``questionnaire`` parameter. Firely Server validates it at the ``Full`` level, as for ``$extract``, and rejects it with HTTP 400 when it is not valid. A supplied Questionnaire is not stored.

So the instance-level and the type-level form can find a Questionnaire in different places.

Where the answers come from
^^^^^^^^^^^^^^^^^^^^^^^^^^^

The expressions in the Questionnaire read the data they need from:

* the resources in the ``context`` parameter, under the name of a ``launchContext`` or a root-level ``variable`` of the Questionnaire;
* the ``subject``, which is also used as the ``patient`` launch context when no ``context`` gives one;
* ``x-fhir-query`` searches, which run against this Firely Server, with the access rights of the caller.

Firely Server does not read data from other servers. A ``context`` content given as a reference is read from this Firely Server as well.

----

Supported parameters
--------------------

The `OperationDefinition <https://hl7.org/fhir/uv/sdc/OperationDefinition-Questionnaire-populate.html>`_ defines seven input and two output parameters. A request that contains a parameter marked ❌ is rejected with HTTP 501. A parameter that the OperationDefinition does not define is ignored.

Input parameters:

.. list-table::
   :header-rows: 1
   :widths: 18 10 26 12 34

   * - Parameter
     - Supported
     - Type
     - Cardinality
     - Additional Notes
   * - ``identifier``
     - ✅
     - ``Identifier``
     - 0..1
     - The ``Questionnaire.identifier`` of the Questionnaire to populate. It must have a ``value``.

       Type level only. Give either ``identifier`` or ``questionnaire``, not both.
   * - ``questionnaire``
     - ✅
     - ``canonical``, ``Reference`` or ``Questionnaire`` resource
     - 0..1
     - The Questionnaire to populate: its canonical url (also accepted as ``valueUri`` or ``valueString``), a reference to it, or the Questionnaire itself. See `Which Questionnaire is used`_.

       Type level only. Give either ``questionnaire`` or ``identifier``, not both. A request with neither is rejected with HTTP 400.
   * - ``subject``
     - ✅
     - ``Reference``
     - 0..1
     - The subject of the QuestionnaireResponse, usually the patient. The whole reference is copied into ``QuestionnaireResponse.subject``, so a reference with only an ``identifier`` is kept as it is.

       It is also used as the ``patient`` launch context when no ``context`` parameter gives one.
   * - ``context``
     - ✅
     - (parts)
     - 0..*
     - A resource to use during population. Each ``context`` has these parts:

       * ``name`` (``string``, 1..1): the name of a ``launchContext`` or of a root-level ``variable`` of the Questionnaire.
       * ``content`` (``Reference`` or resource, 1..*): the resource itself, or a reference to it on this Firely Server.

       A name that the Questionnaire does not declare is reported in ``issues``.
   * - ``dataEndpoint``
     - ❌
     - ``Endpoint``
     - 0..1
     - Firely Server only reads data from itself. A request with this parameter is rejected with HTTP 501.
   * - ``data``
     - ❌
     - ``Bundle``
     - 0..1
     - Not supported. A request with this parameter is rejected with HTTP 501. Send the resources in ``context`` instead.
   * - ``local``
     - ✅
     - ``boolean``
     - 0..1
     - Accepted, without effect: Firely Server always uses the data it holds. It is the only parameter of the operation that can be sent in the query string of a ``GET``.

Each of ``identifier``, ``questionnaire`` and ``subject`` may occur only once. A parameter that occurs more than once, or that has a value of the wrong type, is rejected with HTTP 400.

``$populate`` returns these output parameters, always in a ``Parameters`` resource:

.. list-table::
   :header-rows: 1
   :widths: 18 10 26 12 34

   * - Parameter
     - Supported
     - Type
     - Cardinality
     - Additional Notes
   * - ``response``
     - ✅
     - ``QuestionnaireResponse`` resource
     - 1..1
     - The populated QuestionnaireResponse, with status ``in-progress``. Items without an answer are left out.
   * - ``issues``
     - ✅
     - ``OperationOutcome`` resource
     - 0..1
     - Present when population ran into problems, such as an expression that could not be evaluated, or a query that was not run. Firely Server reports every issue it finds, not only the first one.

What the response looks like
^^^^^^^^^^^^^^^^^^^^^^^^^^^^

.. list-table::
   :header-rows: 1
   :widths: 40 15 45

   * - Result
     - Status
     - Response body
   * - The Questionnaire was populated
     - 200 OK
     - A ``Parameters`` resource with the QuestionnaireResponse in ``response``, and an OperationOutcome in ``issues`` when there are problems to report.
   * - The Questionnaire could not be found
     - 404 Not Found
     - An OperationOutcome.
   * - The request or the Questionnaire cannot be used
     - 400 Bad Request
     - An OperationOutcome with the errors. This includes a Questionnaire that uses a population mechanism that is not supported.
   * - The caller may not read Questionnaires
     - 403 Forbidden
     - An OperationOutcome.
   * - ``dataEndpoint`` or ``data`` was sent, or the endpoint is STU3
     - 501 Not Implemented
     - An OperationOutcome.

----

Supported population mechanisms
-------------------------------

SDC defines three ways to describe population in a Questionnaire, on its `Form Population <https://hl7.org/fhir/uv/sdc/populate.html>`_ page. Firely Server recognizes the mechanism from the SDC extensions in the Questionnaire.

.. list-table::
   :header-rows: 1
   :widths: 25 10 65

   * - Mechanism
     - Supported
     - Recognized by
   * - Expression-based
     - ✅
     - ``sdc-questionnaire-initialExpression``, ``sdc-questionnaire-itemPopulationContext`` or ``sdc-questionnaire-candidateExpression``
   * - Observation-based
     - ❌
     - ``sdc-questionnaire-observationLinkPeriod``
   * - StructureMap-based
     - ❌
     - ``sdc-questionnaire-sourceStructureMap`` or ``sdc-questionnaire-sourceQueries``

A request is rejected with HTTP 400 when the Questionnaire:

* uses a mechanism that is not supported;
* combines several mechanisms;
* has none of these extensions.

Expression-based population
^^^^^^^^^^^^^^^^^^^^^^^^^^^

Firely Server supports these parts of expression-based population:

.. list-table::
   :header-rows: 1
   :widths: 40 10 50

   * - Extension or element
     - Supported
     - Additional Notes
   * - ``sdc-questionnaire-launchContext``
     - ✅
     - Bound from the ``context`` parameter. The ``patient`` launch context falls back on ``subject``.
   * - ``variable``
     - ✅
     - On the Questionnaire and on items, in FHIRPath or as an ``x-fhir-query``. The variables of an item are bound after its ``itemPopulationContext`` is evaluated, so they can use its name, but cannot be used in it.
   * - ``sdc-questionnaire-itemPopulationContext``
     - ✅
     - Sets the context for a group. An expression with several results repeats the group.
   * - ``sdc-questionnaire-initialExpression``
     - ✅
     - Gives the answer of an item. Its name is available to the child items.
   * - ``Questionnaire.item.initial``
     - ✅
     - Static initial values.
   * - ``answerOption.initialSelected``
     - ✅
     - Selects the answer option.
   * - ``sdc-questionnaire-candidateExpression``
     - ❌
     - The candidates are not calculated, because the user picks one. Its name is available to the child items, but holds nothing.

The expressions can be FHIRPath (``text/fhirpath``), and a ``variable`` can also be an ``x-fhir-query`` (``application/x-fhir-query``). CQL expressions are not evaluated, and are reported as a warning in ``issues``.

The expressions can use ``%resource``, ``%rootResource``, ``%context``, ``%questionnaire``, ``%qitem``, and the names of the launch contexts, variables and initial expressions. In the expressions of an item, ``%context`` is the response item of that Questionnaire item. A Questionnaire cannot declare a variable named ``context``, ``resource``, ``rootResource``, ``questionnaire`` or ``qitem``; such a declaration is reported and ignored.

During population, ``%resource`` holds no items yet, and a response item is not yet attached to its parent. So expressions such as ``%resource.item.where(linkId = 'x')`` or ``%context.parent()`` return nothing.

Queries
"""""""

An ``x-fhir-query`` runs as a search on this Firely Server, with the access rights of the caller:

* An embedded ``{{ }}`` expression is replaced by all its results, separated by commas. A Coding, CodeableConcept, Identifier, Reference or Quantity result is written in the format of a search parameter.
* The path must be a resource type, such as ``Observation?code=...``. A query on another path, such as ``Patient/_history`` or ``Patient/$everything``, is not run.
* A query with ``_include``, ``_revinclude``, ``_elements``, ``_summary`` or ``_contained`` is not run.
* A query returns at most 100 resources: the first page, with ``_count`` capped at 100.
* One population runs at most 20 queries. The queries of an item in a repeating group run once for each repetition.

A query that is not run is reported in ``issues``, and the items that depend on it are left empty.

Answers
"""""""

* An answer that matches no answer option is handled according to the item: ``choice`` or ``open-choice`` in R4, and ``answerConstraint`` in R5.
* Some values are converted to fit the item, with a warning in ``issues``: a Coding or CodeableConcept answers a ``string`` item with its text, and a ``dateTime`` answers a ``time`` item with its time of day.
* Some values are converted without a warning: a string answers a ``reference`` item as its reference, a FHIRPath quantity literal answers a ``quantity`` item, and a CodeableConcept with only a ``text`` is a free-text answer where the item allows free text.
* FHIRPath ``resolve()`` only finds contained resources and Bundle entries. Any other reference returns nothing, with a warning in ``issues``.
* A contained resource, a reference to one, a ``urn:`` reference, or the resource of a Bundle entry with a ``urn:`` fullUrl is not used as the answer of a ``reference`` item, with a warning in ``issues``.

Not supported
^^^^^^^^^^^^^

.. list-table::
   :header-rows: 1
   :widths: 35 10 55

   * - Feature
     - Supported
     - Additional Notes
   * - ``dataEndpoint`` and ``data`` parameters
     - ❌
     - Rejected with HTTP 501. Firely Server only reads data from itself.
   * - Observation-based and StructureMap-based population
     - ❌
     - Rejected with HTTP 400. See `Supported population mechanisms`_.
   * - CQL expressions
     - ❌
     - Not evaluated, and reported as a warning in ``issues``.
   * - ``sdc-questionnaire-candidateExpression``
     - ❌
     - The candidates are not calculated.
   * - Checking answers against ``answerValueSet``
     - ❌
     - Answers are only checked against ``answerOption``.
   * - ``x-fhir-query`` with ``_include``, ``_revinclude``, ``_elements``, ``_summary`` or ``_contained``, or on a path other than a resource type
     - ❌
     - The query is not run, and this is reported in ``issues``.
   * - ``resolve()`` of a reference to a resource on a server
     - ❌
     - Only contained resources and Bundle entries are found.
   * - ``GET [base]/Questionnaire/$populate``
     - ❌
     - On type level, the Questionnaire is named in the body, so only ``POST`` is supported.
   * - FHIR STU3
     - ❌
     - Answered with HTTP 501.

Limits
""""""

Population stops, and the request is rejected with HTTP 400 and an OperationOutcome with issue type ``too-costly``, when the QuestionnaireResponse would get more than 10,000 items and answers, or when the expressions need more than 100,000 evaluations. Population also stops when the client stops waiting for the response.

----

.. _feature_sdc_populate_configuration:

Configuration
-------------

``$populate`` is enabled in the ``Operations`` section of ``appsettings.default.json``, on type and instance level. It is only available once the Structured Data Capture plugin is in the pipeline (see :ref:`feature_sdc`).

.. code-block:: json

   "Operations": {
     "$populate": {
       "Name": "$populate",
       "Level": [ "Instance", "Type" ],
       "Enabled": true,
       "RequireAuthorization": "WhenAuthEnabled",
       "RequireTenant": "WhenTenancyEnabled"
     }
   }

See :ref:`disable_interactions` for what these settings mean.

When ``$populate`` is enabled, it is listed in the CapabilityStatement of the R4 and R5 endpoints, on ``Questionnaire``, with the OperationDefinition ``http://hl7.org/fhir/uv/sdc/OperationDefinition/Questionnaire-populate``.
