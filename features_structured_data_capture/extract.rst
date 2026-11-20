.. _feature_sdc_extract:

Data extraction - QuestionnaireResponse/$extract
================================================

.. note::

  The features described on this page are available in the following :ref:`Firely Server editions <vonk_overview>`:

  * Firely Scale - 🌍 / 🇺🇸
  * Firely Prior Authorization - 🇺🇸

.. note::

  The operation requires the license tokens ``http://fire.ly/server/plugins/structured-data-capture`` and ``http://fire.ly/firely-validator``.
  See :ref:`feature_sdc` for how to enable Structured Data Capture.

The ``$extract`` operation turns the answers in a ``QuestionnaireResponse`` into the FHIR resources that its ``Questionnaire`` describes. For example, the answer to a question about the patient's weight becomes an ``Observation`` with that weight as its value.
The Questionnaire defines which resources to create and how, with the data extraction extensions of the `SDC implementation guide <https://hl7.org/fhir/uv/sdc/extraction.html>`_.

``$extract`` does not store anything. It returns a transaction Bundle with the extracted resources. To store them, send that Bundle to Firely Server, or to another FHIR server, as a :ref:`transaction <restful_transaction>`. This lets the client check the resources, or show them to the user for confirmation, before they are stored.

The operation is defined in `SDC - OperationDefinition QuestionnaireResponse/$extract <https://hl7.org/fhir/uv/sdc/OperationDefinition-QuestionnaireResponse-extract.html>`_. Firely Server follows SDC 4.0.0.

----

Invoking $extract
-----------------

``$extract`` is available on type level and on instance level:

* ``POST [base]/QuestionnaireResponse/$extract``

  Extracts from the QuestionnaireResponse in the request. The body is either the QuestionnaireResponse itself, or a ``Parameters`` resource with the QuestionnaireResponse in its ``questionnaire-response`` parameter.

* ``POST [base]/QuestionnaireResponse/[id]/$extract``

  Extracts from the QuestionnaireResponse stored under ``[id]``. The body is optional. When there is one, it must be a ``Parameters`` resource, and it must not contain the ``questionnaire-response`` parameter.

* ``GET [base]/QuestionnaireResponse/[id]/$extract``

  The same as the instance-level ``POST`` without a body. Both input parameters are resources, so they cannot be sent in a ``GET``. A ``GET`` with a query parameter other than ``_format`` is rejected with HTTP 400.

``GET`` is not supported on type level, because there the QuestionnaireResponse has to be sent in the body.

``$extract`` is available on FHIR R4 and R5 endpoints. It is not supported on STU3. To use it on R5, ``Vonk.Fhir.R5`` must be included in the pipeline (see :ref:`feature_multiversion`).

On instance level, the caller must be allowed to read the QuestionnaireResponse, in the same way as for a read of that resource. For example, with SMART on FHIR, a request with a token without a read scope for ``QuestionnaireResponse`` is rejected with HTTP 403.

Which Questionnaire is used
^^^^^^^^^^^^^^^^^^^^^^^^^^^

Firely Server needs the Questionnaire that the QuestionnaireResponse answers:

* When the request contains a ``questionnaire`` parameter, Firely Server uses that Questionnaire. It must have a ``url``, and must be the Questionnaire the QuestionnaireResponse refers to: its ``url``, and its ``version`` if the reference contains one, must match ``QuestionnaireResponse.questionnaire``. A QuestionnaireResponse without a ``questionnaire`` element is also accepted. Firely Server then treats it as an answer to the supplied Questionnaire. The supplied Questionnaire is not stored.
* Otherwise, Firely Server resolves the canonical url in ``QuestionnaireResponse.questionnaire`` in the :ref:`Administration database <conformance>`. A versioned reference, such as ``http://example.org/Questionnaire/intake|1.0.0``, resolves that version.

Before it extracts anything, Firely Server validates the QuestionnaireResponse against that Questionnaire, at the ``Full`` level, regardless of the ``Validation:Level`` setting. A supplied Questionnaire is validated at the ``Full`` level as well. Errors in the resources contained in a supplied Questionnaire, such as its extraction templates, are reported as warnings, because a template may leave a required element for extraction to fill in. At the ``Full`` level, an extension whose definition Firely Server cannot find is an error. So the definitions of the SDC extensions must be loaded (see :ref:`feature_sdc`).

----

Supported parameters
--------------------

The `OperationDefinition <https://hl7.org/fhir/uv/sdc/OperationDefinition-QuestionnaireResponse-extract.html>`_ defines two input and two output parameters. Firely Server supports all four, with the limits given in the notes below. In a ``Parameters`` body, a parameter that the OperationDefinition does not define is ignored.

Input parameters:

.. list-table::
   :header-rows: 1
   :widths: 22 10 22 12 34

   * - Parameter
     - Supported
     - Type
     - Cardinality
     - Additional Notes
   * - ``questionnaire-response``
     - ✅
     - ``QuestionnaireResponse`` resource
     - 0..1
     - The QuestionnaireResponse to extract from.

       Required on type level: send it as this parameter, or as the whole request body. On instance level it is taken from the addressed resource, and a request that also sends this parameter is rejected with HTTP 400.
   * - ``questionnaire``
     - ✅
     - ``Questionnaire`` resource
     - 0..1
     - The Questionnaire to extract with, instead of the one Firely Server resolves. See `Which Questionnaire is used`_.

       It must be a ``Questionnaire`` and must match ``QuestionnaireResponse.questionnaire``. Otherwise, the request is rejected with HTTP 400. A request with a supplied Questionnaire that is not valid is rejected with HTTP 400 as well.

Each input parameter may occur only once, and must carry a resource. A parameter that occurs more than once, or that carries a value instead of a resource, is rejected with HTTP 400.

``$extract`` returns these output parameters:

.. list-table::
   :header-rows: 1
   :widths: 22 10 22 12 34

   * - Parameter
     - Supported
     - Type
     - Cardinality
     - Additional Notes
   * - ``return``
     - ✅
     - ``Bundle`` resource
     - 0..1
     - A transaction Bundle with the extracted resources. Firely Server returns the Bundle as the extraction rules of the Questionnaire produced it, including the ``fullUrl`` and the ``request`` of each entry.

       The OperationDefinition also allows a single resource instead of a Bundle. Firely Server always returns a transaction Bundle, even when only one resource is extracted.
   * - ``issues``
     - ✅
     - ``OperationOutcome`` resource
     - 0..1
     - Warnings and information about the extraction, including the warnings from validating the QuestionnaireResponse and a supplied Questionnaire. Firely Server reports every issue it finds, not only the first one.

What the response looks like
^^^^^^^^^^^^^^^^^^^^^^^^^^^^

.. list-table::
   :header-rows: 1
   :widths: 40 15 45

   * - Result
     - Status
     - Response body
   * - Resources were extracted, without issues
     - 200 OK
     - The transaction Bundle.
   * - Resources were extracted, with warnings or information
     - 200 OK
     - A ``Parameters`` resource with the Bundle in ``return`` and an OperationOutcome in ``issues``.
   * - Nothing to extract
     - 200 OK
     - An OperationOutcome with an informational issue, instead of an empty Bundle.
   * - The request, the QuestionnaireResponse or the Questionnaire is not valid, the Questionnaire cannot be found, the extraction found errors, or the extraction reached a `limit <Limits_>`_
     - 400 Bad Request
     - An OperationOutcome with all the errors and warnings. No Bundle is returned.
   * - The addressed QuestionnaireResponse does not exist (instance level)
     - 404 Not Found
     - An OperationOutcome.

----

Supported extraction mechanisms
-------------------------------

SDC defines four ways to describe data extraction in a Questionnaire, on its `Form Data Extraction <https://hl7.org/fhir/uv/sdc/extraction.html>`_ page. Firely Server recognizes the mechanism from the SDC extensions on the Questionnaire, on its items and on the codes of its items.

.. list-table::
   :header-rows: 1
   :widths: 25 10 65

   * - Mechanism
     - Supported
     - Recognized by
   * - `Template-based <https://hl7.org/fhir/uv/sdc/extraction.html#template-extract>`_
     - ✅
     - ``sdc-questionnaire-templateExtract`` or ``sdc-questionnaire-templateExtractBundle``
   * - Observation-based
     - ❌
     - ``sdc-questionnaire-observationExtract``
   * - Definition-based
     - ❌
     - ``sdc-questionnaire-definitionExtract``, ``sdc-questionnaire-definitionExtractValue``, or the older ``sdc-questionnaire-itemExtractionContext``
   * - StructureMap-based
     - ❌
     - ``sdc-questionnaire-targetStructureMap``

A request is rejected with HTTP 400 when the Questionnaire:

* uses a mechanism that is not supported;
* combines several mechanisms;
* has none of these extensions, so Firely Server cannot determine what to extract.

Template-based extraction
^^^^^^^^^^^^^^^^^^^^^^^^^

With template-based extraction, the Questionnaire contains the resources to create as contained resources, the *templates*. An ``sdc-questionnaire-templateExtract`` extension on the Questionnaire or on an item refers to a template. A resource is created from that template at that point. Extensions inside the template define which answers go where. ``sdc-questionnaire-templateExtractBundle`` refers to a contained transaction Bundle instead, which is used as a template for the whole result.

Firely Server supports these extensions:

.. list-table::
   :header-rows: 1
   :widths: 40 10 50

   * - Extension
     - Supported
     - Additional Notes
   * - ``sdc-questionnaire-templateExtract``
     - ✅
     - On the Questionnaire or on an item. An item that is not in the QuestionnaireResponse, or a question without an answer, produces no resource. On the Questionnaire itself, the resource is created even when there are no answers.

       On a repeating group, it creates one resource for each repetition. On a repeating question, it creates one resource for the item, not one for each answer: the expressions can use all answers. To create one resource for each answer, put an ``sdc-questionnaire-templateExtractContext`` with the expression ``answer`` on the root of the template.
   * - ``sdc-questionnaire-templateExtractBundle``
     - ✅
     - On the Questionnaire only. On an item it is ignored. The Bundle it produces is checked against the rules for a transaction Bundle.

       It cannot be combined with ``sdc-questionnaire-templateExtract``, on the Questionnaire or on an item, and it can only be used once. Otherwise the request is rejected with HTTP 400.

       The Bundle template is processed differently from ``sdc-questionnaire-templateExtract``:

       * Every entry needs a ``request`` in the template. Firely Server does not add one.
       * An entry only has a ``fullUrl`` if the template gives it one. No ``urn:uuid:`` is added.
       * The ids of the resources in the entries are kept.
       * Only the root of the Questionnaire is processed. So ``%context`` is the QuestionnaireResponse, ``%qitem`` is not available, and ``sdc-questionnaire-extractAllocateId`` on items is not used.
       * When an expression on the root of the template returns several Bundles, the first one is used, with a warning.
   * - ``sdc-questionnaire-templateExtractContext``
     - ✅
     - In a template. Its expression sets the context for the elements below it. An expression that returns nothing removes the element. One that returns several results repeats the element if it can repeat. On an element that holds one value, the first result is used, with a warning. On the root of a template it creates one resource per result.

       ``%context`` stays the QuestionnaireResponse item. To refer to the new context in the expressions below it, give the Expression a ``name`` and use that variable.
   * - ``sdc-questionnaire-templateExtractValue``
     - ✅
     - In a template. Its expression gives the value of the element it is on. An expression that returns nothing removes the element. One that returns several results repeats the element if it can repeat. On an element that holds one value, the first result is used, with a warning.

       The value must be a FHIRPath expression as a string. On an element, the first ``sdc-questionnaire-templateExtractContext`` and the first ``sdc-questionnaire-templateExtractValue`` are applied. Further ones of the same kind are removed without a warning. An extraction extension on the ``id`` of the template's root is removed, with a warning: use the ``resourceId`` sub-extension to set the id.
   * - ``sdc-questionnaire-extractAllocateId``
     - ✅
     - Allocates a ``urn:uuid:`` in a variable, so that extracted resources can refer to each other.
   * - ``fullUrl``, ``resourceId``, ``ifNoneMatch``, ``ifModifiedSince``, ``ifMatch``, ``ifNoneExist``
     - ✅
     - Sub-extensions of ``sdc-questionnaire-templateExtract``. Each sets that property of the Bundle entry. A resource that gets an id is sent as a ``PUT``, and one without an id as a ``POST``.

The expressions must be FHIRPath (``text/fhirpath``). An expression in another language is reported as an error. The expressions can use the variables that `SDC defines for extraction <https://hl7.org/fhir/uv/sdc/expressions.html>`_: ``%resource`` (the QuestionnaireResponse), ``%context``, ``%questionnaire``, ``%qitem``, and the variables named by ``sdc-questionnaire-extractAllocateId``. Variables from the ``variable`` and ``sdc-questionnaire-launchContext`` extensions are not available during extraction. An expression that uses a variable that is not available at that point, such as ``%qitem`` in a template on the root of the Questionnaire, is reported as an error.

Extraction uses the answers as they are in the QuestionnaireResponse. ``enableWhen`` is not evaluated, so the answers to an item that ``enableWhen`` would disable are extracted as well. Modular Questionnaires are not assembled, so a ``sdc-questionnaire-templateExtract`` in a sub-questionnaire is not used. The ``meta`` and the ``text`` of a template are copied as they are, so the narrative does not show the answers.

With ``sdc-questionnaire-templateExtract``, every entry gets a ``urn:uuid:`` as its ``fullUrl``, unless the ``fullUrl`` sub-extension gives it another one. So the references between the extracted resources are resolved when the Bundle is processed as a transaction.

Checks on the extracted resources
"""""""""""""""""""""""""""""""""

Firely Server checks what extraction produces, so that every resource that ``$extract`` returns is valid FHIR. These are reported as errors:

* A value that the element cannot hold, such as an answer that is not a date for a ``date`` element, a code that is not in the ValueSet the element is bound to, or a decimal answer with a fraction, such as ``3.5``, for an ``integer`` element. A decimal without a fraction, such as ``3.0``, is accepted.
* A ``resourceId`` expression that does not return an id.
* A sub-extension of ``sdc-questionnaire-templateExtract`` whose value is not a string.
* Two entries for the same resource, with the same ``[type]/[id]`` or the same ``ifNoneExist``.
* A ``urn:uuid:`` reference that no entry has as its ``fullUrl``.
* For ``sdc-questionnaire-templateExtractBundle``: a Bundle that violates the rules for a transaction, such as an entry without a ``request``, or a ``fullUrl`` that is used twice or contains a version.

These are reported as warnings, and the extraction continues:

* An answer that does not fit the element as written, but of which exactly one part fits. For example, the code of a coded answer for a ``string`` element, the value of a ``Quantity`` answer for a ``decimal`` element, or the date of a ``dateTime`` answer for a ``date`` element.
* Several results for an element that holds one value. The first result is used.
* A conditional sub-extension on an entry whose method does not use it, such as ``ifMatch`` on a ``POST``.
* A ``template`` reference to a contained resource id that the Questionnaire uses more than once. The first resource with that id is used.

On a choice element such as ``Observation.value[x]``, a complex answer keeps its own type if the element allows that type. So a ``Quantity`` answer for a template written as ``valueString`` becomes ``valueQuantity``. A primitive answer keeps the type the template was written with: an ``integer`` answer for a ``valueString`` becomes a string, and a ``decimal`` answer for a ``valueQuantity`` is an error. Write the template with the type of the answer, such as ``valueInteger``.

Limits
""""""

Templates with nested ``sdc-questionnaire-templateExtractContext`` expressions can produce a very large number of results. Firely Server stops the extraction after 100,000 expression evaluations, and rejects the request with HTTP 400 and an OperationOutcome with issue type ``too-costly``. The extraction also stops when the client stops waiting for the response.

----

.. _feature_sdc_extract_configuration:

Configuration
-------------

``$extract`` is enabled in the ``Operations`` section of ``appsettings.default.json``, on type and instance level. It is only available when the Structured Data Capture plugin is in the pipeline (see :ref:`feature_sdc`).

.. code-block:: json

   "Operations": {
     "$extract": {
       "Name": "$extract",
       "Level": [ "Instance", "Type" ],
       "Enabled": true,
       "RequireAuthorization": "WhenAuthEnabled",
       "RequireTenant": "WhenTenancyEnabled"
     }
   }

See :ref:`disable_interactions` for what these settings mean.

When ``$extract`` is enabled, it is listed in the CapabilityStatement of the R4 and R5 endpoints, on ``QuestionnaireResponse``, with the OperationDefinition ``http://hl7.org/fhir/uv/sdc/OperationDefinition/QuestionnaireResponse-extract``.
