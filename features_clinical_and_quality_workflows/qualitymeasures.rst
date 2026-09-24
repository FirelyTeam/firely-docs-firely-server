.. _feature_qualitymeasures:

Executing Digital Quality Measures (dQMs) - $cql, $evaluate, $evaluate-measure, $data-requirements
==================================================================================================

.. note::

  The features described on this page are available in the following :ref:`Firely Server editions <vonk_overview>`:

  * Firely dQM - 🌍 / 🇺🇸

.. note::

  The operations require the license token ``http://fire.ly/vonk/plugins/cql`` to be present in the license file.
  If you do not have this license token, please contact `Firely <https://fire.ly/contact>`_.

.. important::

   Please see :ref:`feature_qdm` for an introduction to Digital Quality Reporting in FHIR.

FHIR provides several operations for executing Digital Quality Measures (dQMs), either partially (e.g. evaluating expressions) or fully (evaluating a complete measure). The appropriate operation depends on the specific use case and execution goal:

* ``Library/$evaluate`` is most commonly used for debugging purposes. dQMs frequently reference multiple ``Library`` resources to encapsulate modular logic.  When a measure produces unexpected results—such as an incorrect or zero score—it is often useful to investigate why a particular subject meets or fails to meet specific population criteria (e.g., initial population, denominator, numerator). In such cases, it can be helpful to execute a targeted set of CQL expressions or evaluate specific sub-libraries within the measure. This allows implementers to isolate and verify individual components of the logic without executing the entire measure.

* ``Measure/$evaluate-measure`` is the primary operation for executing a digital quality measure (dQM) as a whole. This operation evaluates a ``Measure`` resource against a specified subject (such as a patient, group) using the measurement period and any associated ``Library`` parameters. ``Measure/$evaluate-measure`` is typically used in production or formal testing scenarios to generate actual measure scores. It is also suitable for automated execution in quality reporting workflows. Unlike ``Library/$evaluate``, which targets specific expressions, ``Measure/$evaluate-measure`` executes the full population logic and scoring methodology defined in the measure, making it the most comprehensive method for end-to-end dQM evaluation.

* ``$cql`` allows direct execution of CQL expressions, either inline or from referenced libraries. It is useful for rapid testing or prototyping when measure logic needs to be validated independently of a ``Measure`` resource.

To prepare the execution, data requirements can be gathered with the following operations:

* ``Library/$data-requirements`` returns the data requirements declared on a ``Library``: a ``Library`` of type ``module-definition`` containing a copy of the target Library's ``dataRequirement`` elements, which describe the FHIR data types, value sets, codes and date constraints the logic needs. Firely Server does not derive them from the CQL or ELM, so the result is only as complete as the ``dataRequirement`` elements the ``Library`` carries. The Firely CQL SDK Packager writes them when it packages a library, including those of the libraries it depends on. This operation is useful during implementation, data mapping and integration planning, to see what data must be available for a successful evaluation.

* ``Measure/$data-requirements`` does the same for the single logic ``Library`` that the ``Measure`` references.

See :ref:`feature_data_requirements` for details.

.. important::

   ``$cql``, ``Library/$evaluate``, ``Measure/$evaluate-measure``, ``Library/$data-requirements``
   and ``Measure/$data-requirements`` are only available in **FHIR R4**. If Firely Server hosts
   multiple FHIR versions (see :ref:`feature_multiversion`), these operations are only available
   for the R4 version. They are not available for STU3 or R5.

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

::

  "LibraryEvaluateOperation": {
    "MaxCachedCompiledLibraries": 32,
    "RemoteDataEndpointsOnly": false,
    "DataEndpoint": [ ],
    "ForwardedHeaders": [ ]
  }

.. _feature_library_evaluate_compiled_library_cache:

Compiled library cache
^^^^^^^^^^^^^^^^^^^^^^

Evaluating a CQL library requires its compiled assemblies — and those of every
library it depends on — to be loaded into the process. Firely Server keeps these
loaded libraries in a cache and reuses them across evaluations, so a library is
loaded and JIT-compiled once instead of once per evaluation. This matters most for
``Measure/$evaluate-measure``, which performs one evaluation per subject per group
and would otherwise reload the same libraries hundreds of times.

The ``MaxCachedCompiledLibraries`` setting bounds how many libraries are kept
loaded (default ``32``). The count applies to the libraries that are *evaluated*,
not to their dependencies, which are loaded together with the library that
references them.

Setting it to ``0`` disables reuse and loads the libraries again for every single
evaluation.

.. warning::

   Disabling the cache is not recommended. Each load context is released
   asynchronously, and on Linux every JIT-compiled code region costs two memory
   mappings, so a large measure evaluation can reach the kernel's limit on memory
   mappings (``vm.max_map_count``) and abort the server process.

Database requirements
^^^^^^^^^^^^^^^^^^^^^

Execution of dQMs relies on retrieving clinical data from the Firely Server
data store. Firely Server reads the patient compartment of the subject (the data
``Patient/$everything`` would return) directly from the repository. The data requirements of the evaluated
``Library`` determine which resource types are collected; see
:ref:`feature_cql_data_retrieval`.

This functionality is only supported when the data store is backed by
MongoDB or SQL Server. Therefore, to execute dQMs against data stored in
Firely Server, the primary FHIR data database must use either MongoDB or
SQL Server.

The ``Vonk.Plugin.PatientEverything`` plugin, which provides the ``Patient/$everything``
operation, does not need to be enabled for this: the CQL operations read the data from the
SQL Server or MongoDB repository themselves.

The administration database (used for conformance resources such as
``Library`` and ``Measure``) can still be hosted on SQLite.

Alternatively, you can configure Firely Server to use only external data
sources by enabling the ``RemoteDataEndpointsOnly`` setting. In that case,
no local data retrieval (and thus no SQL Server or MongoDB data store) is required.

With ``RemoteDataEndpointsOnly`` enabled, every request must set ``useServerData`` to
``false``. A request that sets it to ``true``, or omits it, is rejected with HTTP 400 —
also when it supplies the data in the ``data`` parameter. With ``useServerData`` set to
``false``, the data comes from the ``data`` parameter when it is supplied, and otherwise
from the ``dataEndpoint``; see :ref:`feature_external_data_endpoints`.

.. _feature_external_data_endpoints:

External data endpoints
^^^^^^^^^^^^^^^^^^^^^^^

Firely Server can retrieve clinical and claims data from external FHIR endpoints
during execution of ``Library/$evaluate``.

This is used when the ``useServerData`` parameter is set to ``false`` in a request.
In that case, data is not retrieved from the local Firely Server database, but from
a configured external endpoint. Firely Server requests the data of the subject with
``GET [endpoint]/Patient/[id]/$everything``, listing the resource types to retrieve in
the ``_type`` parameter; see :ref:`feature_cql_data_retrieval`.

::

  "LibraryEvaluateOperation": {
    "RemoteDataEndpointsOnly": false,
    "DataEndpoint": [
      //{
      //    "Endpoint": "<base url>",
      //    "MediaType": "application/fhir+json",
      //    "RemoteDataEndpointAuthentication": "Jwt",
      //    "ClientId": "",
      //    "ClientSecret": "",
      //    "TokenEndpoint": "",
      //    "Audience": "",
      //    "Scopes": "system/*.rs"
      //}
    ],
    "ForwardedHeaders": [
      "X-Custom-Auth-Header"
    ]
  }

The ``DataEndpoint`` setting defines a list of pre-configured external FHIR endpoints.
Each endpoint can be referenced in a request using the ``dataEndpoint`` parameter.

Any ``dataEndpoint`` parameter provided in a request must match one of the
configured endpoints: its ``Endpoint.address`` is compared with the ``Endpoint`` of each
configured entry as an exact string, so letter case and a trailing slash must be the same.

Each ``DataEndpoint`` entry supports the following fields:

- ``Endpoint``: Base URL of the external FHIR server  
- ``MediaType``: The FHIR media type to use for requests to this endpoint ( ``application/fhir+json`` and ``application/fhir+xml`` are supported)  
- ``ClientId`` / ``ClientSecret``: Credentials for authentication (if required)  
- ``TokenEndpoint``: OAuth2 token endpoint (used for JWT authentication)  
- ``Audience``: Audience requested for the access token; sent as the ``aud`` parameter of
  the token request
- ``Scopes``: Space-separated list of **SMART on FHIR scopes**. Since Firely Server uses a ``client_credentials``
  flow, only system-level scopes should be used (e.g. ``system/*.rs``). 
- ``RemoteDataEndpointAuthentication``: Defines how Firely Server authenticates
  against the endpoint. Supported values are ``Jwt`` and ``None``; defaults to ``Jwt``

With ``Jwt`` authentication (the default), ``ClientId``, ``ClientSecret``, ``TokenEndpoint``,
``Audience`` and ``Scopes`` are all required: if any of them is empty, Firely Server does not
start. With ``None``, none of them is used.

With ``Jwt`` authentication, Firely Server obtains an access token from the ``TokenEndpoint``
with the ``client_credentials`` flow and sends it as a bearer token.

.. note::

   Firely Server follows pagination of the remote ``$everything`` response. It
   requests a page size of ``BundleOptions:DefaultCount``, follows every
   ``link[relation="next"]`` the remote endpoint returns, and merges all pages into
   the single Bundle the evaluation runs on.

   If the remote endpoint repeats a page link — which would make the retrieval loop
   indefinitely — the operation fails with an ``OperationOutcome`` rather than
   evaluating on a partial compartment.

When ``useServerData`` is ``false`` and no ``data`` is supplied, the operation fails
with HTTP 500 and an ``OperationOutcome`` stating that the library failed to execute and
to see the log for details, if:

- the ``dataEndpoint`` parameter is missing, or its ``Endpoint`` has no ``address``;
- the ``address`` matches no configured ``DataEndpoint`` entry;
- the remote endpoint answers with an error, including HTTP 401 when Firely Server could
  not authenticate;
- the remote endpoint repeats a page link, as described above.

The server log names the cause.

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
|                         |           |                         |             |                                |
|                         |           |                         |             | A request that supplies both   |
|                         |           |                         |             | ``url`` and ``library`` is     |
|                         |           |                         |             | rejected with HTTP 409.        |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``library``             | ✅        | ``Library`` resource    | 0..1        | In-line logic library that     |
|                         |           |                         |             | contains executable CQL logic. |
|                         |           |                         |             | This Library will not be       |
|                         |           |                         |             | stored in Firely Server. It is |
|                         |           |                         |             | validated and compiled on      |
|                         |           |                         |             | every request. When it carries |
|                         |           |                         |             | ELM content, the ELM is        |
|                         |           |                         |             | compiled instead of the CQL.   |
|                         |           |                         |             |                                |
|                         |           |                         |             | See `Inline library`_.         |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``subject``             | ✅        | ``string``              | 0..1        | The Patient whose data forms   |
|                         |           |                         |             | the evaluation context, as a   |
|                         |           |                         |             | relative reference, e.g.       |
|                         |           |                         |             | ``Patient/pat1``. Other        |
|                         |           |                         |             | reference forms are rejected;  |
|                         |           |                         |             | see :ref:`feature_cql_subject`.|
|                         |           |                         |             |                                |
|                         |           |                         |             | May be omitted when the        |
|                         |           |                         |             | library declares no "context   |
|                         |           |                         |             | Patient", and is required      |
|                         |           |                         |             | when it does.                  |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``expression``          | ✅        | ``reference``           | 0..*        | The name of the expression to  |
|                         |           |                         |             | evaluate. If omitted, all      |
|                         |           |                         |             | expressions in the library are |
|                         |           |                         |             | evaluated.                     |
|                         |           |                         |             |                                |
|                         |           |                         |             | `CQL Access Modifier <https:// |
|                         |           |                         |             | hl7.org/fhir/extensions/Struct |
|                         |           |                         |             | ureDefinition-cqf-cqlAccessMod |
|                         |           |                         |             | ifier.html>`_                  |
|                         |           |                         |             | extensions are not taken into  |
|                         |           |                         |             | account.                       |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``parameters``          | ✅        | ``Parameters`` resource | 0..1        | Input parameters passed into   |
|                         |           |                         |             | the evaluation context.        |
|                         |           |                         |             |                                |
|                         |           |                         |             | These will be mapped from FHIR |
|                         |           |                         |             | data types to CQL data types   |
|                         |           |                         |             | according to the `FHIR Type    |
|                         |           |                         |             | Mapping <https://hl7.org/fhir/ |
|                         |           |                         |             | uv/cql/conformance.html#fhir-t |
|                         |           |                         |             | ype-mapping>`_.                |
|                         |           |                         |             |                                |
|                         |           |                         |             | Most notably, this includes    |
|                         |           |                         |             | passing in the measurement     |
|                         |           |                         |             | period parameter as a FHIR     |
|                         |           |                         |             | Period.                        |
|                         |           |                         |             |                                |
|                         |           |                         |             | Only parameters the Library    |
|                         |           |                         |             | declares are bound; see        |
|                         |           |                         |             | `Input parameter binding`_.    |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``raw``                 | ✅        | ``boolean``             | 0..1        | When ``true``, the results are |
|                         |           |                         |             | not mapped back to FHIR. The   |
|                         |           |                         |             | response holds a single        |
|                         |           |                         |             | ``rawResult`` parameter: a     |
|                         |           |                         |             | ``valueString`` with a JSON    |
|                         |           |                         |             | object that has one member per |
|                         |           |                         |             | evaluated expression, named    |
|                         |           |                         |             | after the expression; see      |
|                         |           |                         |             | :ref:`raw output               |
|                         |           |                         |             | <feature_cql_raw>`.            |
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
|                         |           |                         |             |                                |
|                         |           |                         |             | With                           |
|                         |           |                         |             | ``RemoteDataEndpointsOnly``    |
|                         |           |                         |             | enabled, a request that sets   |
|                         |           |                         |             | ``useServerData`` to ``true``  |
|                         |           |                         |             | or omits it is rejected with   |
|                         |           |                         |             | HTTP 400.                      |
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

   If the Library references any ``ValueSet`` resources, they must be preloaded into the Firely Server's administration endpoint **before** executing the Library. See `ValueSets`_.

.. _feature_library_evaluate_inline_library:

Inline library
^^^^^^^^^^^^^^

A ``Library`` passed in the ``library`` parameter is handled as follows:

- It cannot be combined with ``url``: a request that supplies both is rejected with
  HTTP 409.
- It must have a ``url``, a ``name`` and a ``version``. FHIR declares all three as
  optional on ``Library``, but the CQL engine identifies a library by them. A library
  that lacks any of them is rejected with HTTP 422.
- It is validated with the server's validation settings (``Validation:Level``, see
  :ref:`feature_prevalidation`). Any issue the validation reports, warnings included,
  rejects the request with HTTP 400 and returns the issues in the ``OperationOutcome``.
  With ``Level`` set to ``Off``, the library is not validated.
- It is compiled on every request; the result of the compilation is not kept. When the
  library carries ELM content (``application/elm+json``), the ELM is compiled and the CQL
  content is not used. Otherwise its CQL (``text/cql``) is translated to ELM first. When
  compilation fails, the request is rejected with HTTP 422, reporting that the library
  holds no .NET assembly; the server log holds the compilation errors.
- Its dependencies are resolved from the administration database, like those of a stored
  library; see `Library dependencies`_.

.. _feature_library_evaluate_parameter_binding:

Input parameter binding
^^^^^^^^^^^^^^^^^^^^^^^

Firely Server binds the entries of the ``parameters`` parameter to the input parameters
declared in ``Library.parameter``, not to the ``parameter`` definitions in the CQL.
The declarations are collected from the evaluated ``Library`` and from every library in its
dependency closure. Only declarations with ``use`` set to ``in`` and one of the following
``type`` values are bound: ``string``, ``boolean``, ``integer``, ``decimal``, ``date``,
``dateTime``, ``time``, ``Quantity``, ``Period``, ``Range``, ``code``, ``Coding`` and
``Basic``.

- A supplied parameter is matched to a declaration by its ``name`` (case-sensitive). A
  supplied parameter that matches no declaration is ignored, without an error. This
  applies to every supplied parameter when the ``Library`` has no ``parameter``
  elements, which can be the case for an inline library that carries only CQL.
- A declared parameter that is not supplied is left to the CQL engine, which uses the
  ``default`` of the CQL ``parameter`` definition, if it has one.
- The cardinality of the declaration is enforced. The request is rejected with HTTP 400
  when fewer parameters with that name are supplied than ``min`` — also when a
  declaration has a ``min`` of 1 or more and the request has no ``parameters`` at all —
  or when more are supplied than ``max``. A declaration with an unusable cardinality
  (``min`` missing or negative, ``max`` missing, ``0`` or lower than ``min``) is rejected
  with HTTP 422.
- A declaration with a ``max`` greater than ``1``, or ``*``, binds a CQL ``List`` of all
  supplied values with that name. Otherwise a single value is bound.
- A supplied parameter with ``part`` elements binds a CQL ``Tuple`` with one element per
  part, named after the part. Every part needs a ``value[x]``, and nested parts are not
  allowed; both are rejected with HTTP 400.
- A value is converted according to its own FHIR type. Supported are ``string``,
  ``boolean``, ``integer``, ``decimal``, ``date``, ``dateTime``, ``time``, ``Quantity``,
  ``code`` and ``Coding`` (both bind a CQL ``Code``), ``Period`` and ``Range``. A value of
  any other type, for example ``CodeableConcept`` or ``Reference``, is rejected with
  HTTP 501.
- A ``Period`` binds an ``Interval<DateTime>`` and a ``Range`` an ``Interval<Quantity>``.
  A ``cqf-cqlType`` extension on the ``Library.parameter`` declaration of the evaluated
  ``Library`` selects another point type: ``Interval<Date>`` for a ``Period``, and
  ``Interval<Integer>``, ``Interval<Decimal>`` or ``Interval<Long>`` for a ``Range`` of
  unit-less quantities.

.. _feature_library_evaluate_dependencies:

Library dependencies
^^^^^^^^^^^^^^^^^^^^

Firely Server follows the ``relatedArtifact`` elements of type ``depends-on`` of the
evaluated ``Library``, and of every library it depends on, transitively. Other
``relatedArtifact`` types are ignored. Each ``depends-on`` entry names its dependency in
``relatedArtifact.resource``, as a canonical with an optional version (``url|version``):

- A canonical that contains ``ValueSet`` (in any letter case) and does not contain
  ``Library`` is a ValueSet to preload; see `ValueSets`_.
- A canonical that contains ``CodeSystem``, or an entry whose ``display`` contains
  ``Code system``, is skipped.
- Any other canonical is resolved as a ``Library`` from the administration database. A
  dependency that cannot be resolved, or that resolves to a resource of another type, is
  rejected with HTTP 404, naming the dependency and the library that declares it.

A ``depends-on`` entry without a ``resource``, or with a canonical that is not of the form
``url`` or ``url|version`` (for example with an empty part or with more than one ``|``),
is rejected with HTTP 400.

Every library in the dependency closure must be compiled: a library that holds no
compiled .NET assembly is rejected with HTTP 422.

.. _feature_library_evaluate_valuesets:

ValueSets
^^^^^^^^^

Before the evaluation starts, Firely Server loads every ValueSet that the evaluated
``Library``, or a library in its dependency closure, lists as a ``depends-on`` dependency
(see `Library dependencies`_). The ValueSets are resolved by canonical from the
administration database:

- When the ValueSet carries an expansion, that expansion is used. The expansion must be
  complete: a paged expansion (with ``expansion.offset`` set, or an ``expansion.total``
  larger than the number of codes it contains) is not accepted, and the operation fails.
- Otherwise, Firely Server expands the ValueSet from its ``compose``, resolving the
  ValueSets and CodeSystems it refers to from the administration database.
- A ValueSet that cannot be resolved is rejected with HTTP 404, naming the ValueSet and
  the library that lists it.

A value set that the CQL uses, but that no library lists as a ``depends-on`` dependency,
is not loaded up front. A membership test against it is passed to the server's
terminology service; see :ref:`feature_terminology`.

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

.. _feature_cql_raw:

When the proprietary ``raw`` parameter is ``true``, the results are not mapped back to
FHIR data types. Instead, the ``Parameters`` resource holds a single parameter named
``rawResult``, whose ``valueString`` is a JSON object with one member per evaluated
expression: all expressions of the ``Library``, or those named in ``expression``. Each
member is named after its expression and holds the JSON serialization of the CQL result.

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
              "valueString": "FHIR.Patient"
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
              "valueString": "System.Boolean"
            }
          ],
          "name": "HasBPReading",
          "valueBoolean": true
        },
        {
          "extension": [
            {
              "url": "http://hl7.org/fhir/StructureDefinition/cqf-cqlType",
              "valueString": "System.Boolean"
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
  - ``POST`` (type level or instance level)
  - ``GET`` (type level or instance level, when all parameters can be provided as query parameters)

.. note::

   Invocation at the instance level (``[base]/Measure/[id]/$evaluate-measure``)
   is also supported. Supplying a ``url`` parameter on an instance-level call
   is rejected with an HTTP 400 response, and an unresolvable ``Measure`` id
   returns an HTTP 404 response.


.. note::

   The operation can optionally affect server state depending on the ``persist`` parameter.

   When ``persist`` is set to ``true``, the generated ``MeasureReport`` is stored
   on the server. By default (``persist = false``), the report is returned in the
   response only and is not persisted.

.. _feature_measure_evaluate_configuration:

Configuration
~~~~~~~~~~~~~

The ``Measure/$evaluate-measure`` operation is provided by the
``Vonk.Plugin.Cql.Operations.Measure.Evaluate`` namespace.

You can enable or disable this operation by including or excluding this
namespace in the Firely Server pipeline options. See :ref:`vonk_available_plugins`
for more information.

The operation evaluates the measure through ``Library/$evaluate``, so the settings
described in :ref:`feature_library_evaluate_configuration` — the data endpoints and
the compiled library cache in particular — apply to it as well. Its own behavior is
configured in the ``MeasureEvaluateOperation`` section of the appsettings.

::

  "MeasureEvaluateOperation": {
    "MaxDegreeOfParallelism": 2,
    "MaxSubjectsForSynchronousGroupBasedMeasureEvaluation": <n>
  }

``MaxDegreeOfParallelism``
  How many groups of a measure are evaluated concurrently within a single subject
  (default ``2``). Retrieval of a subject's data is always sequential, and the
  resulting ``MeasureReport`` is identical in content and ordering regardless of
  this setting. Set it to ``1`` to evaluate groups strictly one after another. A
  value below ``1`` is rejected at startup.

  Raising it increases throughput for measures with many groups, at the cost of
  more memory and more concurrent database and terminology work per request.

``MaxSubjectsForSynchronousGroupBasedMeasureEvaluation``
  The maximum number of distinct subjects a ``Group``-based evaluation may cover
  when the operation is invoked synchronously. A ``Group`` that resolves to more
  subjects than this is rejected.

  Subjects are counted after de-duplication: a patient listed several times in the
  ``Group`` — however the member references are spelled — counts once.

Supported parameters
^^^^^^^^^^^^^^^^^^^^

Firely Server supports the following parameters:

+--------------------------+-----------+-------------------------+-------------+---------------------------------------------+
| Parameter                | Supported | Type                    | Cardinality | Additional Notes                            |
+==========================+===========+=========================+=============+=============================================+
| ``url``                  | ✅        | ``canonical``           | 0..1        | Canonical URL of the Measure to evaluate.   |
|                          |           |                         |             |                                             |
|                          |           |                         |             | Required for type-level invocation.         |
|                          |           |                         |             |                                             |
|                          |           |                         |             | Versioned canonical references are allowed, |
|                          |           |                         |             | e.g.,                                       |
|                          |           |                         |             | ``http://example.org/fhir/Measure/          |
|                          |           |                         |             | ExampleMeasure|1.0.0``.                     |
+--------------------------+-----------+-------------------------+-------------+---------------------------------------------+
| ``subject``              | ✅        | ``string``              | 1..1        | Reference to the subject for which the      |
|                          |           |                         |             | measure is evaluated.                       |
|                          |           |                         |             |                                             |
|                          |           |                         |             | Supported resource types are ``Patient``    |
|                          |           |                         |             | and ``Group``.                              |
|                          |           |                         |             |                                             |
|                          |           |                         |             | When a ``Patient`` is provided, the measure |
|                          |           |                         |             | is evaluated for that single subject.       |
|                          |           |                         |             |                                             |
|                          |           |                         |             | When a ``Group`` is provided, the measure   |
|                          |           |                         |             | is evaluated for all ``Patient`` references |
|                          |           |                         |             | contained in the Group.                     |
|                          |           |                         |             |                                             |
|                          |           |                         |             | See :ref:`feature_cql_subject`.             |
+--------------------------+-----------+-------------------------+-------------+---------------------------------------------+
| ``periodStart``          | ✅        | ``date``                | 0..1        | Start of the measurement period.            |
|                          |           |                         |             |                                             |
|                          |           |                         |             | Supply both ``periodStart`` and             |
|                          |           |                         |             | ``periodEnd``, or neither: a request that   |
|                          |           |                         |             | supplies only one of them is rejected with  |
|                          |           |                         |             | HTTP 400.                                   |
|                          |           |                         |             |                                             |
|                          |           |                         |             | When neither is supplied, the Measure's     |
|                          |           |                         |             | ``effectivePeriod`` is used. It must then   |
|                          |           |                         |             | have both a ``start`` and an ``end``,       |
|                          |           |                         |             | otherwise the request is rejected with      |
|                          |           |                         |             | HTTP 400.                                   |
|                          |           |                         |             |                                             |
|                          |           |                         |             | The dates are expanded to whole days: the   |
|                          |           |                         |             | measurement period passed to the CQL runs   |
|                          |           |                         |             | from 00:00:00.000 on ``periodStart`` to     |
|                          |           |                         |             | 23:59:59.999 on ``periodEnd``, both with    |
|                          |           |                         |             | offset ``+00:00``.                          |
+--------------------------+-----------+-------------------------+-------------+---------------------------------------------+
| ``periodEnd``            | ✅        | ``date``                | 0..1        | End of the measurement period. See          |
|                          |           |                         |             | ``periodStart``.                            |
+--------------------------+-----------+-------------------------+-------------+---------------------------------------------+
| ``reportType``           | ✅        | ``code``                | 0..1        | The type of measure report:                 |
|                          |           |                         |             |                                             |
|                          |           |                         |             | - ``individual`` (or ``subject``, the code  |
|                          |           |                         |             |   the R4 operation defines for it):         |
|                          |           |                         |             |   evaluates the measure for a single        |
|                          |           |                         |             |   ``Patient`` subject and returns           |
|                          |           |                         |             |   population membership and score for that  |
|                          |           |                         |             |   subject.                                  |
|                          |           |                         |             |                                             |
|                          |           |                         |             | - ``summary`` (or ``population``, the code  |
|                          |           |                         |             |   the R4 operation defines for it):         |
|                          |           |                         |             |   evaluates the measure for the members of  |
|                          |           |                         |             |   a ``Group`` subject and returns           |
|                          |           |                         |             |   aggregated counts (e.g. numerator,        |
|                          |           |                         |             |   denominator).                             |
|                          |           |                         |             |                                             |
|                          |           |                         |             | - ``subject-list``: for a ``Group``         |
|                          |           |                         |             |   subject, returns aggregated population    |
|                          |           |                         |             |   counts plus a contained individual        |
|                          |           |                         |             |   MeasureReport per group member.           |
|                          |           |                         |             |                                             |
|                          |           |                         |             | A ``Patient`` subject accepts only          |
|                          |           |                         |             | ``individual`` and ``subject``; a ``Group`` |
|                          |           |                         |             | subject accepts only ``summary``,           |
|                          |           |                         |             | ``population`` and ``subject-list``. Any    |
|                          |           |                         |             | other combination is rejected with          |
|                          |           |                         |             | HTTP 400.                                   |
|                          |           |                         |             |                                             |
|                          |           |                         |             | If not specified, the default is            |
|                          |           |                         |             | ``individual`` for every subject type, so a |
|                          |           |                         |             | request for a ``Group`` subject must supply |
|                          |           |                         |             | ``reportType``.                             |
+--------------------------+-----------+-------------------------+-------------+---------------------------------------------+
| ``parameters``           | ✅        | ``Parameters`` resource | 0..1        | See ``Library/$evaluate`` configuration     |
|                          |           |                         |             | for details.                                |
|                          |           |                         |             |                                             |
|                          |           |                         |             | Unlike for ``Library/$evaluate``, it must   |
|                          |           |                         |             | not hold a ``Measurement Period``           |
|                          |           |                         |             | parameter: the measurement period comes     |
|                          |           |                         |             | only from ``periodStart`` and               |
|                          |           |                         |             | ``periodEnd``, or the Measure's             |
|                          |           |                         |             | ``effectivePeriod``. A request that         |
|                          |           |                         |             | supplies a ``Measurement Period`` here is   |
|                          |           |                         |             | rejected with HTTP 400.                     |
+--------------------------+-----------+-------------------------+-------------+---------------------------------------------+
| ``useServerData``        | ✅        | ``boolean``             | 0..1        | See ``Library/$evaluate`` configuration     |
|                          |           |                         |             | for details.                                |
+--------------------------+-----------+-------------------------+-------------+---------------------------------------------+
| ``data``                 | ✅        | ``Bundle``              | 0..1        | See ``Library/$evaluate`` configuration     |
|                          |           |                         |             | for details.                                |
|                          |           |                         |             |                                             |
|                          |           |                         |             | For a ``Group`` subject, the same bundle is |
|                          |           |                         |             | used for every member. When the measure's   |
|                          |           |                         |             | library is defined in the Patient context,  |
|                          |           |                         |             | the data of the bundle must belong to       |
|                          |           |                         |             | exactly one patient: the patient it is      |
|                          |           |                         |             | evaluated for. ``data`` can therefore not   |
|                          |           |                         |             | be used with a ``Group`` of more than one   |
|                          |           |                         |             | patient; the evaluation of a member the     |
|                          |           |                         |             | bundle does not belong to fails with        |
|                          |           |                         |             | HTTP 422, and the whole request is rejected |
|                          |           |                         |             | with it.                                    |
+--------------------------+-----------+-------------------------+-------------+---------------------------------------------+
| ``dataEndpoint``         | ✅        | ``Endpoint``            | 0..1        | See ``Library/$evaluate`` configuration     |
|                          |           |                         |             | for details.                                |
+--------------------------+-----------+-------------------------+-------------+---------------------------------------------+
| ``persist``              | ✅        | ``boolean``             | 0..1        | When ``true``, the generated                |
|                          |           |                         |             | ``MeasureReport`` is stored on the server.  |
|                          |           |                         |             |                                             |
|                          |           |                         |             | When ``false`` (default), the result is     |
|                          |           |                         |             | returned in the response only.              |
|                          |           |                         |             |                                             |
|                          |           |                         |             | See                                         |
|                          |           |                         |             | :ref:`feature_measure_evaluate_persist`.    |
|                          |           |                         |             |                                             |
|                          |           |                         |             | This is a proprietary parameter of Firely   |
|                          |           |                         |             | Server.                                     |
+--------------------------+-----------+-------------------------+-------------+---------------------------------------------+
| ``rawPopulationCounts``  | ✅        | ``boolean``             | 0..1        | When ``true``, every population count in the|
|                          |           |                         |             | report reflects only the result of that     |
|                          |           |                         |             | population's own criteria expression, rather|
|                          |           |                         |             | than the label-based composition prescribed |
|                          |           |                         |             | by the Quality Measure IG. The              |
|                          |           |                         |             | ``measureScore`` is unaffected.             |
|                          |           |                         |             |                                             |
|                          |           |                         |             | Defaults to ``false``. See                  |
|                          |           |                         |             | :ref:`feature_measure_evaluate_counts`.     |
|                          |           |                         |             |                                             |
|                          |           |                         |             | This is a proprietary parameter of Firely   |
|                          |           |                         |             | Server.                                     |
+--------------------------+-----------+-------------------------+-------------+---------------------------------------------+
| ``raw``                  | ✅        | ``boolean``             | 0..1        | When ``true``, returns the unscored results |
|                          |           |                         |             | of the evaluation instead of a              |
|                          |           |                         |             | ``MeasureReport``: a ``Parameters``         |
|                          |           |                         |             | resource with one parameter per             |
|                          |           |                         |             | ``Measure.group``, named after the group    |
|                          |           |                         |             | ``id``. Each of them holds one              |
|                          |           |                         |             | ``population`` part per evaluated subject,  |
|                          |           |                         |             | with a ``subject`` part (``valueString``)   |
|                          |           |                         |             | and a ``parameters`` part holding the       |
|                          |           |                         |             | ``Library/$evaluate`` result of that        |
|                          |           |                         |             | group's CQL expressions for that subject.   |
|                          |           |                         |             |                                             |
|                          |           |                         |             | The values in these results are mapped to   |
|                          |           |                         |             | FHIR as usual (see                          |
|                          |           |                         |             | :ref:`feature_cql_result_mapping`); no      |
|                          |           |                         |             | population counts or scores are             |
|                          |           |                         |             | calculated.                                 |
|                          |           |                         |             |                                             |
|                          |           |                         |             | Combining ``raw`` with ``persist=true`` is  |
|                          |           |                         |             | rejected with HTTP 400.                     |
|                          |           |                         |             |                                             |
|                          |           |                         |             | This is a proprietary parameter of Firely   |
|                          |           |                         |             | Server.                                     |
+--------------------------+-----------+-------------------------+-------------+---------------------------------------------+
| ``measure``              | ❌        | ``Measure``             | 0..1        |                                             |
+--------------------------+-----------+-------------------------+-------------+---------------------------------------------+
| ``version``              | ❌        | ``string``              | 0..1        |                                             |
+--------------------------+-----------+-------------------------+-------------+---------------------------------------------+
| ``provider``             | ❌        | ``string``              | 0..1        |                                             |
+--------------------------+-----------+-------------------------+-------------+---------------------------------------------+
| ``location``             | ❌        | ``string``              | 0..1        |                                             |
+--------------------------+-----------+-------------------------+-------------+---------------------------------------------+
| ``lastReceivedOn``       | ❌        | ``dateTime``            | 0..1        |                                             |
+--------------------------+-----------+-------------------------+-------------+---------------------------------------------+

.. _feature_measure_evaluate_scoring:

Measure scoring
~~~~~~~~~~~~~~~

The scoring type of a measure determines which populations are evaluated, how their
counts are composed, and how the ``measureScore`` is calculated. Firely Server reads
it from ``Measure.scoring``, using the codes of the
``http://terminology.hl7.org/CodeSystem/measure-scoring`` CodeSystem.

+--------------------------+-----------+--------------------------------------------------------------+
| Scoring type             | Supported | Notes                                                        |
+==========================+===========+==============================================================+
| ``proportion``           | ✅        | The numerator is a subset of the denominator.                |
+--------------------------+-----------+--------------------------------------------------------------+
| ``ratio``                | ✅        | The numerator and denominator are derived independently.     |
+--------------------------+-----------+--------------------------------------------------------------+
| ``cohort``               | ✅        | Only an initial population is evaluated; no score.           |
+--------------------------+-----------+--------------------------------------------------------------+
| ``continuous-variable``  | ❌        | Rejected with HTTP 422, issue type ``not-supported``.        |
+--------------------------+-----------+--------------------------------------------------------------+

A ``Measure`` that declares no scoring type at all is still evaluated: every
population criteria it defines is executed and reported, including population types
that no scoring type requires. No ``measureScore`` is produced in that case.

Proportion scoring
^^^^^^^^^^^^^^^^^^

The numerator is a subset of the denominator. The score is the numerator divided by
the denominator, after the exclusion and exception populations have been subtracted::

  denominator = (initial-population ∩ denominator)
                 − denominator-exclusion − denominator-exception
  numerator   = (denominator ∩ numerator) − numerator-exclusion
  score       = numerator / denominator

When the denominator is 0, the ``measureScore`` is 0 — for example in an
``individual`` report for a patient that is not in the denominator.

Ratio scoring
^^^^^^^^^^^^^

The numerator and denominator are derived **independently** of one another — unlike
``proportion``, the numerator is not a subset of the denominator. Both are derived
from the group's single initial population::

  numerator   = (initial-population ∩ numerator) − numerator-exclusion
  denominator = (initial-population ∩ denominator) − denominator-exclusion
  score       = numerator / denominator

As for ``proportion``, a denominator of 0 gives a ``measureScore`` of 0.

A ``denominator-exception`` population is not permitted on a ratio-scored group, and
a ratio-scored group cannot carry stratifiers. Both are rejected before evaluation;
see :ref:`feature_measure_evaluate_validation`.

.. note::

   Measures that require *multiple* initial populations per group — one for the
   numerator and one for the denominator — are not yet supported. A ratio-scored
   group is evaluated against a single initial population shared by both paths.

.. _feature_measure_evaluate_scoring_override:

Group-level scoring override
^^^^^^^^^^^^^^^^^^^^^^^^^^^^

A single ``Measure`` can mix scoring types across its groups. A group that carries a
scoring extension overrides ``Measure.scoring`` for that group only:

- ``http://hl7.org/fhir/us/cqfmeasures/StructureDefinition/cqfm-scoring`` (US realm)
- ``http://hl7.org/fhir/uv/cqm/StructureDefinition/cqm-scoring`` (UV realm)

Both use the same ``measure-scoring`` codes as ``Measure.scoring``, and the value is
validated exactly like ``Measure.scoring`` — including whether the populations the
scoring type requires are present.

The *effective* scoring type of a group — its override if it has one, and
``Measure.scoring`` otherwise — is what every rule on this page is judged against.
A group that overrides a ratio ``Measure`` to ``proportion`` may therefore carry a
``denominator-exception``, while a group that overrides a proportion ``Measure`` to
``ratio`` may not.

.. _feature_measure_evaluate_populationbasis:

Population basis
~~~~~~~~~~~~~~~~

The population basis of a population declares what its criteria expression returns:
``boolean`` for a patient-based measure, or a FHIR resource type such as
``Encounter`` for a measure that counts events rather than subjects.

Firely Server reads it from either realm's extension on the population:

- ``http://hl7.org/fhir/us/cqfmeasures/StructureDefinition/cqfm-populationBasis`` (US realm)
- ``http://hl7.org/fhir/uv/cqm/StructureDefinition/cqm-populationBasis`` (UV realm)

The declared basis is validated against the return type of the CQL expression the
population's criteria names. A basis can only be a FHIR type, and FHIR type names
are case-sensitive — ``Boolean`` is not ``boolean``.

Consistency within a membership path
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Composing population counts and the score means intersecting and subtracting sets of
cases, which is only meaningful when all populations involved describe the same kind
of case. All populations within one *membership path* must therefore resolve to a
single, common basis.

Which populations form a path depends on the group's effective scoring type:

- **proportion** — the whole group is one path. The initial population, the
  denominator tree and the numerator tree must all share a basis.
- **ratio** — the numerator path (``numerator``, ``numerator-exclusion``) and the
  denominator path (``denominator``, ``denominator-exclusion``) are validated
  independently and may use different bases. The initial population feeds both, so
  its basis must be consistent with each.

Mixing bases within one path is rejected with HTTP 422. When no basis is declared,
it can only be observed from the results, so the same check is applied again during
scoring, against the types the expressions actually returned.

.. _feature_measure_evaluate_counts:

Population counts
~~~~~~~~~~~~~~~~~

By default, the population counts in the ``MeasureReport`` follow the **label-based
membership algorithm** of the FHIR Quality Measure IG, rather than the raw result of
each population's criteria expression. For ``proportion`` and ``ratio`` scoring, a
case is only counted for a population when it also belongs to the population that
population is derived from:

- the denominator only counts cases in the initial population;
- the numerator only counts cases in the denominator that are not
  denominator-excluded (``proportion``), or cases in the initial population
  (``ratio``);
- exclusion and exception counts are restricted to their parent population;
- a denominator exception does not count cases that meet the numerator criteria.

Exclusion and exception cases remain *included* in the count of their parent
population — they are only subtracted when the ``measureScore`` is calculated.

A subject whose ``Denominator Exclusion`` expression returns ``true`` but that is not
a member of the denominator therefore counts as 0 for ``denominator-exclusion``.

Raw population counts
^^^^^^^^^^^^^^^^^^^^^

Some certification programs — NCQA in particular — expect each population count to
reflect its own criteria expression, without the label-based composition. Setting the
proprietary ``rawPopulationCounts`` parameter to ``true`` reports the counts that way,
and the same rule governs the membership of each population's ``subjectResults`` list
in a ``subject-list`` report.

The ``measureScore`` is **not** affected by this parameter: it always follows the
label-based membership rules. In the example above, the subject counts as 1 for
``denominator-exclusion`` under ``rawPopulationCounts=true``, while the score stays
the same.

.. _feature_measure_evaluate_stratifiers:

Stratifiers
~~~~~~~~~~~

A stratifier partitions the reported populations of a group into strata. Firely
Server supports **value-based (component) stratifiers**, declared as
``Measure.group.stratifier.component[]``.

+-------------------------------------------+-----------+----------------------------------------------+
| Stratifier form                           | Supported | Notes                                        |
+===========================================+===========+==============================================+
| ``stratifier.component[]``                | ✅        | Only on groups with a ``boolean`` basis.     |
+-------------------------------------------+-----------+----------------------------------------------+
| ``stratifier.criteria``                   | ❌        | Rejected with HTTP 501, not yet implemented. |
+-------------------------------------------+-----------+----------------------------------------------+

Each component's ``text/cql-identifier`` expression is evaluated per subject, and the
subjects are stratified by the **cross-product of their observed component values**.

.. note::

   A subject may produce several values for one component — one product line per
   enrollment, for example. It then appears in one stratum per combination, so the
   per-stratum counts of a group can legitimately sum to more than the group's own
   population counts.

Each stratum is reported with:

- ``stratum.component[]`` code/value pairs. A coded value is reported as a
  ``CodeableConcept`` carrying the ``coding`` plus a ``text`` holding the code
  string; a non-coded primitive is reported as ``text`` only; an absent value is
  reported as a ``CodeableConcept`` carrying only the ``data-absent-reason``
  extension with code ``unknown``.
- population counts, following the same rules as the group's own counts — see
  :ref:`feature_measure_evaluate_counts`. A stratum never reports a member for a
  population its group excludes.
- a ``measureScore``, for proportion-scored groups. It always follows the
  label-based membership rules, mirroring the group-level score, so a stratum whose
  denominator is 0 has a ``measureScore`` of 0.
- ``subjectResults`` ``List`` resources, in ``subject-list`` reports.

An ``individual`` report keeps strata whose counts are all 0, so the subject's
observed component values are always visible. ``summary`` and ``subject-list``
reports omit strata that contain no in-population subject.

A ratio-scored group cannot carry stratifiers: the Quality Measure IG forbids the
combination.

.. _feature_measure_evaluate_validation:

Measure validation
~~~~~~~~~~~~~~~~~~

A ``Measure`` is validated before any CQL is evaluated, so an authoring problem is
reported as a client error naming the group and population concerned, rather than
surfacing part-way through a long population run.

Rejected with HTTP 422
^^^^^^^^^^^^^^^^^^^^^^

*Measure level*

- No ``url``. The canonical URL identifies the measure in ``MeasureReport.measure``.
- No single resolvable logic library: a missing ``Measure.library``, more than one
  reference, or an empty canonical.
- A ``Measure.library`` canonical that resolves to a resource of another type —
  canonicals are unique per resource type, but not across types.
- The ``continuous-variable`` scoring type, in ``Measure.scoring`` or in a group-level
  scoring override (issue type ``not-supported``).

*Group level*

- A population type the group's effective scoring type does not evaluate. Where the
  Quality Measure IG marks it Not Permitted — a ``denominator`` on a cohort-scored
  group, a ``denominator-exception`` on a ratio-scored group, a
  ``measure-population`` on either — the issue type is ``invalid`` and the ``Measure``
  should be corrected. Where the IG's population table does not cover it at all —
  ``measure-observation``, on any ``Measure`` — the issue type is ``not-supported``,
  because no edit to the ``Measure`` resolves it.
- The same population type defined more than once in one group.
- A population whose ``code`` the ``measure-population`` CodeSystem does not define,
  or that carries no coding from that CodeSystem. The population is named by its
  element id where it has one, and by its position in the group otherwise.
- A population whose criteria names no CQL expression: no criteria at all, an empty
  expression, or a criteria in another expression language than ``text/cql-identifier``.
- Populations within one membership path that do not share a common population basis
  (see :ref:`feature_measure_evaluate_populationbasis`).
- A ``Library`` parameter whose declared type is not a FHIR type.
- A stratifier that declares neither or both of ``criteria`` and ``component[]``,
  duplicate stratifier or component ids, a stratifier on a ratio-scored group, or a
  component on a group whose basis is not ``boolean``.
- A stratifier component expression that returns resources rather than values.

*Result level*

- A population the evaluation returned no result for. This happens when the
  ``Library`` does not define the named expression, or when the expression's value
  cannot be represented in a FHIR ``Parameters`` resource and was therefore dropped.
  The error names the group, the population, the expression and the library expected
  to define it.
- A population whose criteria returned results of more than one type.
- A population result of a type the counting cannot identify a case by. A case must
  be identifiable as a resource, a ``boolean``, a ``decimal``, a ``date`` or an
  ``integer``; a ``Quantity`` or a ``Coding``, for instance, cannot be counted.

Rejected with other status codes
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

- **HTTP 400** — a scoring code the ``measure-scoring`` CodeSystem does not define,
  in ``Measure.scoring`` or in a group-level scoring override (issue type
  ``invalid``). A ``Measure.scoring`` that carries no coding from that CodeSystem is
  rejected the same way.
- **HTTP 412** — a group without an id, or several groups sharing one. The group id
  is what the results, ``MeasureReport.group.id`` and the stratum memberships are
  filed under.
- **HTTP 404** — a ``Measure.library`` canonical that resolves to no resource. This
  is reported before any subject data is retrieved, and names the library url and
  version.
- **HTTP 501** — a criteria-based stratifier, which is not yet implemented.

Output parameters
~~~~~~~~~~~~~~~~~

The operation returns a ``MeasureReport`` resource containing the evaluation results,
or a ``Parameters`` resource when ``raw`` is ``true``.

The report includes:

- population counts (e.g. initial population, denominator, numerator)
- measure score (if applicable)
- subject-level or population-level results depending on ``reportType``
- the parameters of the request (see below)

Each ``group`` and each ``population`` of the report carries the ``id`` of the
``Measure`` element it reports on. A ``Measure.group.population`` therefore needs an
``id`` to appear in the report: a population without one is still evaluated, and takes
part in the counts and the score of the other populations, but is left out of the
``MeasureReport``. Such a ``Measure`` is not rejected; Firely Server only logs a
warning.

Request parameters in the report
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Every ``MeasureReport`` contains a ``Parameters`` resource with the parameters of the
request, and references it through the
``http://hl7.org/fhir/us/cqfmeasures/StructureDefinition/cqfm-inputParameters``
extension. All parameters of the request are included, except ``data``:

- for a ``POST`` request, the parameters are copied as they were sent, including the
  ``parameters`` resource and a ``dataEndpoint`` ``Endpoint`` resource with all of its
  ``header`` values;
- for a ``GET`` request, the ``Parameters`` resource is rebuilt from the query
  parameters.

When ``persist`` is ``true``, the contained ``Parameters`` resource is stored together
with the report. The individual reports contained in a ``subject-list`` report do not
carry it.

.. warning::

   Everything sent in the request body except ``data`` ends up in the
   ``MeasureReport`` — for example credentials in the ``header`` values of a
   ``dataEndpoint`` ``Endpoint`` — and, with ``persist=true``, in the database.
   HTTP headers of the incoming request that are forwarded to data endpoints (see
   :ref:`feature_external_data_endpoints`) are not request parameters, and are not
   included.

.. _feature_measure_evaluate_persist:

Persisting the report
^^^^^^^^^^^^^^^^^^^^^

With ``persist=true``, the ``MeasureReport`` is stored after it has been built, and
the response is the same as without it: HTTP 200 — not 201 — with the report as its
body, and no ``Location`` header.

The report is written to the repository directly, not through a regular create
interaction. It is therefore not validated, and pre-handlers registered for the
create interaction do not run for it.

Before the report is written, the write is authorized by the same check that guards
a create interaction. When the authorization denies it, the report is not stored and
the response has HTTP 403 — its body is still the ``MeasureReport``, not an
``OperationOutcome``.

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

The ``id`` of the report and of its contained ``Parameters``, and the ``date``, differ
for every invocation. The ``group`` and ``population`` ids are those of the
``Measure``. ``[base]`` stands for the base url of the server, as Firely Server returns
absolute references by default.

.. code-block:: json

   {
     "resourceType": "MeasureReport",
     "id": "5b7f3a2e-9c41-4d8b-a6e0-2f1c8d9b3e47",
     "contained": [
       {
         "resourceType": "Parameters",
         "id": "c2d94e1b-7a35-4f60-8b1e-9d4a6c0f2e18",
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
     ],
     "extension": [
       {
         "url": "http://hl7.org/fhir/us/cqfmeasures/StructureDefinition/cqfm-inputParameters",
         "valueReference": {
           "reference": "#c2d94e1b-7a35-4f60-8b1e-9d4a6c0f2e18"
         }
       }
     ],
     "status": "complete",
     "type": "individual",
     "measure": "http://example.org/fhir/Measure/ExampleMeasure|1.0.0",
     "subject": {
       "reference": "[base]/Patient/cql-patient-test"
     },
     "date": "2024-03-18T14:27:05.3176942+00:00",
     "period": {
       "start": "2023-01-01",
       "end": "2023-12-31"
     },
     "group": [
       {
         "id": "group-1",
         "population": [
           {
             "id": "initial-population",
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
             "id": "denominator",
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
             "id": "numerator",
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
           "value": 1,
           "system": "http://unitsofmeasure.org",
           "code": "{score}"
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
  `Using CQL with FHIR Implementation Guide - v2.0.0 <https://hl7.org/fhir/uv/cql/OperationDefinition-cql-cql.html>`_

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
|                         |           |                         |             | is evaluated in the Patient    |
|                         |           |                         |             | context when a ``subject`` is  |
|                         |           |                         |             | supplied, and can refer to the |
|                         |           |                         |             | supplied ``parameters`` by     |
|                         |           |                         |             | name; see                      |
|                         |           |                         |             | :ref:`feature_cql_expression`. |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``subject``             | ✅        | ``string``              | 0..1        | The Patient whose data forms   |
|                         |           |                         |             | the evaluation context, as a   |
|                         |           |                         |             | relative reference, e.g.       |
|                         |           |                         |             | ``Patient/pat1``. Other        |
|                         |           |                         |             | reference forms are rejected;  |
|                         |           |                         |             | see :ref:`feature_cql_subject`.|
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``parameters``          | ✅        | ``Parameters``          | 0..1        | Input parameters passed into   |
|                         |           |                         |             | the evaluation context. Their  |
|                         |           |                         |             | values are mapped as for       |
|                         |           |                         |             | ``Library/$evaluate``; see     |
|                         |           |                         |             | :ref:`feature_cql_expression`  |
|                         |           |                         |             | for how they are declared.     |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``raw``                 | ✅        | ``boolean``             | 0..1        | When ``true``, the result is   |
|                         |           |                         |             | not mapped back to FHIR, but   |
|                         |           |                         |             | returned as JSON in a single   |
|                         |           |                         |             | ``rawResult`` parameter; see   |
|                         |           |                         |             | the output parameters below.   |
|                         |           |                         |             |                                |
|                         |           |                         |             | This is a proprietary          |
|                         |           |                         |             | parameter of Firely Server.    |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``library``             | ❌        | Complex                 | 0..*        |                                |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``useServerData``       | ✅        | ``boolean``             | 0..1        | Controls whether the data of   |
|                         |           |                         |             | the server the operation runs  |
|                         |           |                         |             | on is used, when no ``data``   |
|                         |           |                         |             | bundle is supplied.            |
|                         |           |                         |             |                                |
|                         |           |                         |             | See ``Library/$evaluate``      |
|                         |           |                         |             | for details.                   |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``data``                | ✅        | ``Bundle``              | 0..1        | Inline FHIR data bundle to     |
|                         |           |                         |             | evaluate against. A supplied   |
|                         |           |                         |             | bundle is evaluated in         |
|                         |           |                         |             | isolation from the data of     |
|                         |           |                         |             | the server.                    |
|                         |           |                         |             |                                |
|                         |           |                         |             | See ``Library/$evaluate``      |
|                         |           |                         |             | for details.                   |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``prefetchData``        | ❌        | Complex                 | 0..*        |                                |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``dataEndpoint``        | ❌        | ``Endpoint``            | 0..1        |                                |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``contentEndpoint``     | ❌        | ``Endpoint``            | 0..1        |                                |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+
| ``terminologyEndpoint`` | ❌        | ``Endpoint``            | 0..1        |                                |
+-------------------------+-----------+-------------------------+-------------+--------------------------------+

.. _feature_cql_expression:

Evaluating the expression
^^^^^^^^^^^^^^^^^^^^^^^^^

Firely Server wraps the ``expression`` in a CQL library that it generates for the
request, compiles that library and evaluates it through ``Library/$evaluate``. The
generated library has this shape::

  library Test version '1.0.0'
  using FHIR version '4.0.1'
  parameter <name> <type>    // one for every parameter in 'parameters'

  context Patient            // only when a 'subject' is supplied

  define "ExpressionToBeEvaluated": <expression>

- When a ``subject`` is supplied, the expression is evaluated in the ``Patient``
  context, so it can use ``Patient`` and retrieve the data of that patient, such as
  ``[Encounter]``. Without a ``subject``, no context is declared.
- Every parameter in ``parameters`` is declared as a ``parameter`` of the library, so
  the expression can refer to it by name. Its CQL type is inferred from the supplied
  FHIR value. A parameter supplied more than once is declared as a ``List`` of that
  type, and a parameter that carries ``part`` elements instead of a value as a
  ``Tuple`` with an element for every part. For example, ``SomeNumber`` supplied twice
  with a ``valueInteger`` is declared as a list of integers, so ``Sum(SomeNumber)``
  returns their sum.
- Parameter and part names are written into the library unquoted, so they must be
  valid unquoted CQL identifiers: a name with spaces, such as ``Measurement Period``,
  cannot be used.
- The library declares nothing else — no ``include``, ``codesystem``, ``valueset`` or
  ``code`` — so the expression cannot refer to another library, such as
  ``FHIRHelpers``, or to a code system, value set or code by name.

A parameter whose value cannot be mapped to a CQL type is rejected with HTTP 501. A
parameter without a value, with both a value and ``part`` elements, with nested parts,
or with repetitions of different types is rejected with HTTP 400. An
``OperationOutcome`` about the evaluation itself, such as one for an unknown patient,
refers to the generated library as ``Test`` version ``1.0.0``.

Output parameters
~~~~~~~~~~~~~~~~~

The operation returns a ``Parameters`` resource containing the result of the
evaluated CQL expression.

The result is returned in a parameter named ``return``. The value is mapped back
to a FHIR data type.

When a ``subject`` is supplied, the response also contains a parameter named
``Patient`` that holds the ``Patient`` resource of the subject: the ``Patient``
definition that ``context Patient`` adds to the generated library.

When the proprietary ``raw`` parameter is set to ``true``, the result is not mapped
back to FHIR (see :ref:`raw output <feature_cql_raw>`). The response then holds a
single parameter named ``rawResult``, whose ``valueString`` is a JSON object. In that
object the result is a member named ``ExpressionToBeEvaluated``, the name of the
expression in the generated library; it is not renamed to ``return``. When a
``subject`` is supplied, the object also has a ``Patient`` member. For the example
request below with ``raw`` set to ``true``, the response is:

.. code-block:: json

   {
    "resourceType": "Parameters",
    "parameter": [
        {
            "name": "rawResult",
            "valueString": "{\"ExpressionToBeEvaluated\":\"Hello World\"}"
        }
    ]
  }

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
                    "valueString": "System.String"
                }
            ],
            "name": "return",
            "valueString": "Hello World"
        }
    ]
  }

.. _feature_data_requirements:

Library/$data-requirements and Measure/$data-requirements
---------------------------------------------------------

Both operations return the data requirements that are declared on a ``Library``. They
do not analyze the CQL or ELM of the library.

**Scope**
  - Invocation level: ``type`` / ``instance``
  - Supported resource type(s): ``Library``, ``Measure``
  - Idempotent: ``yes``
  - Affects server state: ``no``

**HTTP methods**
  - ``POST`` and ``GET``, at type and instance level

Library/$data-requirements
~~~~~~~~~~~~~~~~~~~~~~~~~~

The only supported parameter is ``target``: the canonical of the ``Library``, optionally
versioned (``url|version``), passed as a ``valueString`` in a ``POST``. It is required at
type level and not allowed at instance level (``Library/[id]/$data-requirements``), which
takes the ``Library`` with that id instead.

The response is a new ``Library`` of type ``module-definition``
(``http://terminology.hl7.org/CodeSystem/library-type``) that holds a copy of the
``dataRequirement`` elements of the target ``Library``. Nothing else is added: the
libraries the target depends on are not included, and when the target declares no
``dataRequirement`` the result contains none.

.. note::

   Firely Server compiles a ``Library`` that carries only CQL or ELM itself (see
   :ref:`feature_qdm`), but that compilation does not add ``dataRequirement`` elements.
   Package a library with the Firely CQL SDK Packager to have its data requirements,
   including those of the libraries it depends on, written into the ``Library``.

Measure/$data-requirements
~~~~~~~~~~~~~~~~~~~~~~~~~~

The ``Measure`` is identified by the ``url`` parameter (a ``valueString`` in a ``POST``,
optionally versioned) at type level, or by its id at instance level. The ``periodStart``
and ``periodEnd`` parameters are accepted, but do not affect the result.

The ``Measure`` must reference exactly one logic ``Library`` in ``Measure.library``. The
operation returns the result of ``Library/$data-requirements`` for that ``Library``; the
data requirements are not aggregated across libraries. The operation requires the
``Library/$data-requirements`` plugin to be enabled as well.

Errors
~~~~~~

- HTTP 400 — the canonical parameter is missing or empty at type level, or supplied at
  instance level.
- HTTP 404 — no ``Library`` or ``Measure`` is found for the canonical or id, or the
  ``Measure``'s ``Library`` cannot be resolved.
- HTTP 422 — the ``Measure`` references no ``Library``, or more than one.
- HTTP 501 — any other parameter is supplied.

----

.. _feature_cql_evaluation_behavior:

CQL evaluation behavior
-----------------------

The behavior described in this section is shared by ``$cql``,
``Library/$evaluate`` and ``Measure/$evaluate-measure``.

.. _feature_cql_subject:

The subject parameter
~~~~~~~~~~~~~~~~~~~~~

The ``subject`` parameter identifies whose data forms the evaluation context. It must
be a **relative reference of the form** ``ResourceType/id``::

  subject=Patient/pat1

``Library/$evaluate`` and ``$cql`` accept a ``Patient`` reference.
``Measure/$evaluate-measure`` accepts a ``Patient`` or a ``Group``.

Any other reference form is rejected with an ``OperationOutcome`` identifying
``subject`` as having an invalid value:

- an absolute url, such as ``http://example.org/fhir/Patient/pat1``
- a versioned reference, such as ``Patient/pat1/_history/2``
- a bare id without a resource type, such as ``pat1``

For ``Library/$evaluate`` and ``$cql``, ``subject`` may be omitted when the library
declares no Patient context. It is required when the library does declare one; a
request that omits it is rejected with HTTP 422 naming the library.

Group subjects
^^^^^^^^^^^^^^

When a ``Group`` is supplied, the measure is evaluated for every ``Patient`` the
group references. Member entries are de-duplicated on the identity of the referenced
resource, so a patient listed several times is evaluated and counted exactly once —
regardless of whether the entries spell the reference relatively, as an absolute url
or with a version. The ``MaxSubjectsForSynchronousGroupBasedMeasureEvaluation`` limit
applies to those distinct patients.

The ``Group`` is read from Firely Server's own data store, also when the patient data
comes from the ``data`` parameter or a ``dataEndpoint``. A ``Group`` that is not found
there is rejected with HTTP 404.

Only an actual group of patients can be evaluated. Each of the following rejects the
whole request with HTTP 422, issue type ``not-supported``:

- ``Group.actual`` is ``false``: a descriptive group.
- ``Group.type`` is another type than ``person``.
- The ``Group`` has no member entities.
- A member entity references another resource type than ``Patient``. Such a member
  is not skipped: the request is rejected.
- A member entry whose reference does not identify a resource by type and id at all —
  an empty reference, or an absolute uri that is not a resource url such as
  ``urn:uuid:…``, as produced by ingesting a ``Group`` from a transaction Bundle. The
  ``OperationOutcome`` names that reference.
- More distinct patients than ``MaxSubjectsForSynchronousGroupBasedMeasureEvaluation``
  allows.

``member.inactive`` and ``member.period`` are not taken into account: an inactive
member, or a member whose period has ended, is evaluated like any other member.

Unknown patients
^^^^^^^^^^^^^^^^

When Firely Server resolves the evaluation data itself — from its own data or from a
``dataEndpoint`` — the ``Patient`` type is always included in the retrieval. Data
without a ``Patient`` resource therefore means the requested patient does not exist,
and the ``OperationOutcome`` reports that the patient could not be found, with issue
code ``not-found``. ``Measure/$evaluate-measure`` reports this without evaluating a
single group of the measure.

When the caller supplies the data through the ``data`` parameter, the outcome instead
reports that the provided data does not contain the expected ``Patient`` — the data
lacks it, while the patient itself may well exist on the server.

.. _feature_cql_data_retrieval:

Data retrieval
~~~~~~~~~~~~~~

When Firely Server retrieves the data of the subject itself — from its own data or from
a ``dataEndpoint`` — it retrieves only the resource types listed in the
``dataRequirement.type`` elements of the evaluated ``Library``, plus ``Patient``. For
``Measure/$evaluate-measure``, the evaluated ``Library`` is the one the ``Measure``
references. When that ``Library`` declares no data requirements, all resource types the
server supports are retrieved.

The data requirements of the libraries that the evaluated ``Library`` includes are not
taken into account. A bundle supplied through the ``data`` parameter is used as
supplied, without filtering.

.. important::

   A resource type that the logic retrieves but the evaluated ``Library`` does not
   declare is not retrieved, so a retrieve of that type returns an empty result, without
   an error. Declare every resource type the logic retrieves — including those retrieved
   by included libraries — in ``dataRequirement`` of the evaluated ``Library``.

.. _feature_cql_result_mapping:

Mapping CQL results to FHIR
~~~~~~~~~~~~~~~~~~~~~~~~~~~

Unless the proprietary ``raw`` parameter of ``$cql`` or ``Library/$evaluate`` is used,
the result of each evaluated expression is mapped back to a FHIR data type and returned
as a parameter in the response ``Parameters`` resource. A list result returns one
parameter repetition per element. ``raw`` on ``Measure/$evaluate-measure`` does not
change this mapping; it only replaces the ``MeasureReport`` with the per-group results.

Declared CQL type
^^^^^^^^^^^^^^^^^

Every returned parameter carries a
``http://hl7.org/fhir/StructureDefinition/cqf-cqlType`` extension holding the CQL type
of the value, using the model names CQL itself uses — ``FHIR.Encounter``,
``System.Integer``, ``System.Boolean``.

A list is typed as the list rather than as the element it carries, on every repetition
of the parameter: ``List<FHIR.Encounter>``, ``List<System.Integer>``. An empty list
reports the type of the elements it would have held, and a null element of a list —
carried as a repetition with a ``data-absent-reason`` — carries the list's type as
well.

Type mapping notes
^^^^^^^^^^^^^^^^^^

Most values map directly onto their FHIR equivalent. The following are worth calling
out, because their FHIR representation cannot express everything the CQL value holds:

``Concept`` and ``CodeableConcept``
  Returned as a ``CodeableConcept``. A CQL ``Concept``'s codes become ``coding``
  entries (system, code, version and display), and its display becomes the ``text``.

``Interval<Integer>``, ``Interval<Decimal>``, ``Interval<Long>``
  Returned as a FHIR ``Range``. A ``Range`` has inclusive bounds only, so an open
  bound is returned as its closed equivalent — the successor of an open low bound,
  the predecessor of an open high bound. Every bound carries a ``quantity-precision``
  extension stating its number of digits after the decimal point, so the precision of
  a bound does not depend on the serializer preserving trailing zeros. An
  ``Interval<Long>`` is returned as a unit-less (UCUM ``1``) ``Range``.

``Interval<DateTime>``
  Returned as a FHIR ``Period``. A UTC instant is written with the ``Z`` designator
  rather than a ``+00:00`` offset; both denote the same instant, and offsets other
  than UTC are unaffected.

``Time``
  Returned as a FHIR ``time``, without a timezone. The FHIR ``time`` datatype has no
  timezone component.

A value that cannot be represented in a FHIR ``Parameters`` resource is dropped from
the response and logged. When a list held elements but none of them could be mapped,
the parameter is reported as an empty list and a ``CouldNotMapAnyListElement``
warning records the parameter name and the number of dropped elements.

.. _feature_cql_semantics:

CQL language semantics
~~~~~~~~~~~~~~~~~~~~~~

A few points of CQL semantics affect measure results directly, and are worth knowing
when comparing Firely Server's output against another engine's.

Quantities and units
  Comparing or ordering two quantities whose units are not of the same dimension
  (``=``, ``<``, ``>``, ``<=``, ``>=``, ``between``) evaluates to ``null``, as the CQL
  specification requires — ``1 'cm' = 0.01 'g'`` is ``null``, not ``true`` — and list
  equality propagates that unknown. Equivalence (``~``) converts units before
  comparing and always yields a boolean, so it is ``false`` for units of different
  dimensions. An operation on invalid or incommensurable units returns ``null``
  rather than failing the evaluation.

  Adding and subtracting quantities in different but compatible units is supported:
  ``1 'm' + 30 'cm'`` returns the result in the most granular of the input units.

Date and time precision
  Equivalence (``~``) between two dates, date-times or times of differing precision
  evaluates to ``false``. Matching known components is not enough.

Value set membership
  A value set that has been resolved and expanded is interpreted under **closed-world
  semantics**: membership is decided from the expansion itself, so a code that is
  absent from it yields ``false`` rather than an unknown result. Membership is decided
  from the expansion at hand without consulting the terminology service again.

.. important::

   If the library references any ``ValueSet`` resources, they must be preloaded into
   the Firely Server administration endpoint **before** the library is evaluated.
