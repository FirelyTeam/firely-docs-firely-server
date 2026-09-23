.. _vonk_releasenotes_history_v6:

Current Firely Server release notes (v6.x)
==========================================

.. note::
    For information on how to upgrade, please have a look at our documentation on :ref:`upgrade`. You can download the binaries of the latest version from `this site <https://downloads.fire.ly/firely-server/versions/>`_, or pull the latest docker image::
        
        docker pull firely/server:latest

.. _vonk_releasenotes_6_10_0:

Release 6.10.0, TBD
-------------------

This release is centred on the CQL and quality measure operations: ``Measure/$evaluate-measure`` gains ratio scoring, value-based stratifiers and group-level scoring overrides, population-level evaluations are substantially faster and no longer risk aborting the server process, and invalid ``Measure`` definitions are now rejected up front instead of failing halfway through. The documentation of these operations has been extended accordingly — see :ref:`feature_qualitymeasures`. This release also fixes a number of issues outside CQL, most notably in ``$purge``, ``Patient/$everything`` paging, ``_include`` links, custom authorization and the administration database.

.. attention::
    This release contains several corrections to CQL evaluation that can change ``Library/$evaluate`` and ``Measure/$evaluate-measure`` results — and therefore measure population membership — as well as changes to the shape of their output and to the accepted ``subject`` parameter. ``Measure/$evaluate-measure`` also rejects a number of ``Measure`` definitions that were previously accepted, and ``$member-match`` is no longer reachable outside FHIR R4. Please review the "Known behavioral changes" section below before upgrading.

Improvements
^^^^^^^^^^^^

**Quality measures and CQL**

#. ``Measure/$evaluate-measure`` is substantially faster for population-level reports. The measure's ``Library`` resource, its dependency closure, the ``ValueSet`` expansions, the compiled assemblies and the metadata derived from them are now resolved once per operation instead of once per subject × group; each subject's data bundle is indexed once instead of once per group; CQL define results are memoized within a single evaluation; and value set membership is decided from an already-resolved expansion instead of calling the terminology service again. This also considerably reduces terminology load.
#. ``Library/$evaluate`` and ``Measure/$evaluate-measure`` now fetch each subject's patient compartment in a single unpaged query instead of in pages of 20 resources. On SQL Server every page re-executed the full compartment computation, so subjects with a large compartment cost many times more database work than necessary.
#. Within each subject, the groups of a measure are now evaluated in parallel, bounded by the new ``MeasureEvaluateOperation:MaxDegreeOfParallelism`` setting (default ``2``). The resulting ``MeasureReport`` is identical in content and ordering to sequential evaluation. See :ref:`feature_measure_evaluate_configuration`.

**Firely Server Ingest**

#. FSI: NDJSON ingestion now parses resources in parallel, improving import throughput on multi-core machines. Reading and numbering the lines stays strictly sequential, so journaling and import order are unaffected. Tune it with the new ``workflow:parseParallel`` (``--parsePar``, default ``-1`` = unbounded) and ``workflow:parseBufferSize`` (``--parseBuffer``, default ``50``) settings. See :ref:`tool_fsi` for more information.
#. FSI: all ``workflow:*Parallel`` and ``workflow:*BufferSize`` settings are now validated at startup, instead of surfacing as a runtime error once the import pipeline is built. The per-category timing statistics reported at the end of an import run are now measured with tick precision, so the fast per-resource steps (``Read``, ``Parse``, ``Index``) are no longer under-reported as 0ms.
#. Reduced allocations when parsing FHIR JSON during Firely Server Ingest and during MongoDB search-index updates. No behavioral change.

**Other**

#. Application Insights telemetry is now flushed on graceful shutdown. Buffered telemetry could previously be lost if the process exited before the SDK had sent it.
#. When running in the ``Development`` environment, startup no longer fails hard when a plugin referenced in ``PipelineOptions`` cannot be resolved — for example a plugin listed in ``appsettings.Development.json`` but not present in the ``./plugins`` directory. The condition is logged as an error instead. Other environments keep the existing fail-fast behavior.

Features
^^^^^^^^

**Quality measures and CQL**

#. ``Measure/$evaluate-measure`` now supports ``ratio`` scoring. Ratio measures are no longer rejected as an unsupported scoring type: their populations are evaluated and reported as population counts, and a ``measureScore`` is calculated. See :ref:`feature_measure_evaluate_scoring`.
#. ``Measure/$evaluate-measure`` supports a group-level scoring override, so a single ``Measure`` can mix scoring types across its groups: a ``cqfm-scoring`` (US realm) or ``cqm-scoring`` (UV realm) extension on a ``Measure.group`` overrides ``Measure.scoring`` for that group. See :ref:`feature_measure_evaluate_scoring_override`.
#. ``Measure/$evaluate-measure`` now supports value-based (component) stratifiers on boolean-basis groups, reported with ``stratum.component[]`` code/value pairs, per-stratum population counts and scores, and ``subjectResults`` ``List`` resources in ``subject-list`` reports. Criteria-based stratifiers remain unimplemented. See :ref:`feature_measure_evaluate_stratifiers`.
#. ``Measure/$evaluate-measure`` accepts a new optional boolean parameter ``rawPopulationCounts``, which reports each population count from its own criteria expression rather than from the label-based composition prescribed by the Quality Measure IG. This supports NCQA certification. See :ref:`feature_measure_evaluate_counts`.
#. Population basis validation now also accepts the UV-realm ``cqm-populationBasis`` extension alongside the existing US-realm ``cqfm-populationBasis``. See :ref:`feature_measure_evaluate_populationbasis`.
#. The ``$cql`` operation now supports the ``data`` and ``useServerData`` parameters, forwarding them to the underlying CQL library evaluation. See :ref:`feature_cql_operation`.
#. ``Measure/$evaluate-measure`` and ``Library/$evaluate`` now emit detailed OpenTelemetry tracing spans throughout the CQL evaluation workflow — library and ``ValueSet`` resolution, CQL compilation, data retrieval and validation, per-subject and per-group evaluation, expression execution and ``MeasureReport`` building — giving operators visibility into where time is spent. Spans are emitted on ``Vonk.Plugin.Cql.*`` activity sources and honor the existing ``Vonk:OpenTelemetryOptions`` configuration. See :ref:`feature_opentelemetry` for more information.

**Other**

#. A SMART on FHIR client's token can now be restricted to only the DaVinci ATR export workflow, through a claim in the new reserved, must-understand claim namespace ``http://server.fire.ly/auth/claims/critical/``. Such a token may only call ``$davinci-data-export``, ``$exportstatus`` and ``$exportfilerequest``; any other interaction is rejected with ``403 Forbidden``, following the same ``RequireAuthorization: Never`` exemption as all other authorization checks. A claim in this namespace that no loaded, licensed plugin can handle is also rejected with ``403 Forbidden``, rather than being silently ignored. See :ref:`feature_davinci_data_export` for more information.

Fix
^^^

**Quality measures and CQL**

#. Parallel CQL evaluation no longer exhausts the operating system's memory mappings. Every ``Library/$evaluate`` — including each per-subject, per-group evaluation of a ``Measure/$evaluate-measure`` run — loaded the compiled assemblies of its CQL library and its dependencies into a load context of its own, so they were JIT-compiled again for every evaluation and those load contexts accumulated faster than the runtime reclaimed them. On Linux this could reach the kernel's limit on memory mappings (``vm.max_map_count``) and abort the server process. A compiled library and its dependencies are now loaded once and reused, bounded by the new ``LibraryEvaluateOperation:MaxCachedCompiledLibraries`` setting. See :ref:`feature_library_evaluate_compiled_library_cache`.
#. ``Library/$evaluate`` with a remote data endpoint no longer evaluates on a truncated patient compartment. When the data was retrieved from a remote FHIR server (``useServerData=false`` with a ``dataEndpoint``), only the first page of the searchset Bundle returned by the remote ``Patient/$everything`` was used, so a compartment larger than the remote's page size silently reached the evaluation truncated. All pages are now followed and merged. See :ref:`feature_external_data_endpoints`.
#. ``Measure/$evaluate-measure`` population counts now follow the label-based membership algorithm of the FHIR Quality Measure IG, instead of reporting the raw result of each population expression. Use the new ``rawPopulationCounts`` parameter if you need the previous, per-expression counts. See :ref:`feature_measure_evaluate_counts`.
#. A ``Library/$evaluate`` expression returning a CQL ``Concept`` or a FHIR ``CodeableConcept`` value was silently omitted from the response, and is now included as a ``CodeableConcept`` parameter. This also makes such expressions usable as component stratifier expressions, where the dropped value previously reported every subject as data-absent.
#. ``Measure/$evaluate-measure`` now counts value-typed results for non-patient-based populations. Population expressions returning boolean, decimal, date or integer values rather than resources were silently counted as 0, so population counts and ``measureScore`` for such measures were wrong.
#. Parallel ``Measure/$evaluate-measure`` group evaluations no longer share one dependency-injection request scope. Concurrent groups that triggered simultaneous ``ValueSet`` resolution could previously fail with HTTP 500, because request-scoped services that are not thread-safe — notably the EF Core ``DbContext`` behind administration-database lookups — were shared.
#. ``Measure/$evaluate-measure`` no longer fails with a false "duplicate measurement period" error when CQL input parameters are supplied for a measure with multiple groups or subjects. The caller-supplied ``parameters`` resource was mutated during evaluation of the first group; it is now copied per evaluation, and a genuinely duplicated ``Measurement Period`` is rejected once, up front.
#. A ``Group`` that lists the same patient more than once no longer double-counts that patient in ``Measure/$evaluate-measure``, and a member entry that does not identify a resource by type and id is now rejected with an ``OperationOutcome`` instead of failing with HTTP 500. See :ref:`feature_cql_subject`.
#. The CQL ``Median`` aggregate now returns the true middle of its input: for a list with an even number of elements the values that are averaged are the actual middle pair, and the midpoint is computed with widened arithmetic so it no longer overflows or loses precision. The ``GeometricMean`` aggregate now handles nulls as the specification requires, yielding the geometric mean of the non-null elements instead of an unusable result. Results that use either aggregate can change to the correct value.
#. A CQL ``Time`` returned by ``Library/$evaluate`` or ``Measure/$evaluate-measure`` no longer carries a timezone, which is not valid for the FHIR ``time`` datatype and could be rejected by a strict client or downstream validator. The ``cqf-cqlType`` extension now reports the CQL type using the model names CQL uses. See :ref:`feature_cql_result_mapping`.
#. ``Library/$evaluate`` no longer crashes when a repeating input parameter converts to a reference-typed CQL value such as ``string`` or ``CqlCode``, nor when a tuple input parameter has a number of parts that is a multiple of seven — tuples of 7, 14, 21, … parts were rejected with an internal error instead of being bound and evaluated. It also reports clearer errors for invalid parameter cardinalities: a ``max`` lower than ``min`` now has its own message instead of reusing the "invalid 'min' cardinality" text, and a parameter declaring a ``min`` but no ``max`` is reported with HTTP 422 instead of failing with an HTTP 500 that named nothing.
#. The CQL operations now report several conditions that previously surfaced as an HTTP 500 with HTTP 422 and an ``OperationOutcome``: a ``Library`` that does not carry the ``url``, ``name`` and ``version`` identifying it to the CQL engine (a missing ``name`` previously gave an HTTP 400 reporting a FHIR invariant), a request that evaluates a Patient-context library without a ``subject`` parameter, and a population whose criteria names a CQL define containing an apostrophe — a legal CQL quoted identifier such as ``"Patient's initial population"`` previously produced an unhandled error before any evaluation ran.
#. ``Library/$evaluate`` and ``Measure/$evaluate-measure`` now always return a proper ``OperationOutcome`` when an evaluation fails. A failure outside of a CQL expression previously caused a secondary exception while it was being logged, so the client received a bare HTTP 500 and the log recorded the wrong exception; and a compiled library set that could not be fully loaded returned an empty body with no error status.
#. ``Measure/$data-requirements`` now returns HTTP 422 with an ``OperationOutcome`` for a ``Measure`` without a logic library, instead of failing with HTTP 500. A ``Measure`` referencing multiple libraries is now also rejected with a clear error, matching ``$evaluate-measure``, instead of silently using only the first library.
#. ``$cql`` and ``Measure/$data-requirements`` now propagate a 404 from the operation they delegate to. An unresolvable ``Library`` or ``Library`` dependency was treated as success, making ``$cql`` fail with an internal server error and ``Measure/$data-requirements`` return HTTP 200 with an empty body.
#. A ``Patient`` that does not exist is now reported as not found by the CQL operations, with issue code ``not-found``, and ``Measure/$evaluate-measure`` reports this without evaluating a single group. A library declaring a Patient context whose data requirements omit the ``Patient`` type can now be evaluated over server-resolved data; such an evaluation previously always failed. See :ref:`feature_cql_subject`.
#. The four CQL operations (``Library/$evaluate``, ``Measure/$evaluate-measure``, ``Library/$data-requirements``, ``Measure/$data-requirements``) now share one implementation for resolving their target resource by id or canonical, aligning previously drifted error messages and logging. HTTP status codes are unchanged, except that an empty ``url=``/``target=`` is now rejected with HTTP 400 in all four operations instead of falling through to a canonical lookup returning 404 in three of them.
#. ``Measure/$evaluate-measure`` now rejects an unusable ``Measure.library`` reference before it fetches any subject data, reporting an unresolvable canonical as HTTP 404 itself rather than after retrieving a subject's complete compartment. A canonical resolving to a resource of another type is now reported as HTTP 422, where the evaluation previously proceeded with population basis validation silently skipped.

**Other**

#. ``$purge`` now erases the patient's entire compartment instead of only the first 20 resources. The compartment was requested with the default page size, so a patient with a larger compartment kept every resource beyond that first page.
#. ``Patient/$everything`` no longer returns a ``next`` link that restarts pagination from the first page. When the number of matching resources was an exact multiple of ``_count``, the final page was completely full, and the operation emitted an offset-based ``next`` link instead of the keyset cursor the preceding pages used — so a client walking the ``next`` links never reached the end. The final page now omits the ``next`` link.
#. ``entry.fullUrl`` for ``_include``/``_revinclude`` entries in a search bundle no longer contains a ``/_history/{versionId}`` segment. The included resource's fullUrl was built from its versioned key, producing a version-specific URL that violates FHIR's ``bdl-8`` invariant. Matched entries were unaffected.
#. A custom ``IAuthorization`` implementation no longer has every read and search refused. When a plugin or customer implementation was adapted to ``IAuthorizationV2`` — which happens on a Virtual Tenants deployment — reads, version reads, searches and history requests were refused regardless of what ``CanRead`` returned. Writes were decided correctly. These are decided by ``CanRead`` again, and a custom operation is left to the operation's own authorization.
#. A ``StructureDefinition`` is no longer returned without a snapshot when the conformance cache changes while that snapshot is being generated. The resolver read an instance from the cache and kept returning it, but snapshot generation resolves the canonical again — so if the cache was emptied, the entry evicted, or another thread cached its own instance, the snapshot was generated on that other instance and the caller got one without a snapshot. Validation against such a profile then failed or silently checked nothing. The resolver now returns the instance the snapshot was generated on.
#. Reading the supported resource types from a large administration database no longer times out. On SQL Server or SQLite with many conformance resources, the query building the resource type to canonical mapping from the ``StructureDefinition``s could exceed the 30 second command timeout, failing the model build — at startup, and on every create/update/delete on the administration endpoint, since those trigger a rebuild. The mapping is now read through indexed token predicates instead of matching against unindexable value columns. Two defects in the same mapping are fixed along with it: a ``StructureDefinition`` that mentions ``Resource``, ``MetadataResource`` or ``CanonicalResource`` anywhere other than in its ``baseDefinition`` is no longer treated as defining a resource type, and when two ``StructureDefinition``s define the same resource type the error now names both canonicals. On SQLite, the STU3 ``base`` match is now case-insensitive as well, matching the collation SQL Server already evaluated it under.
#. SQL Server string ``:contains`` filters for short values now scope the ``ShortString``/``LongString`` ``LIKE`` predicates under one grouped condition, preventing unintended broad matches caused by SQL operator precedence.
#. Bulk Data Export (``$export``) and Real World Testing (``$realworldtesting``, ``$realworldtestingstatus``) no longer fail at startup, or silently lose their routes, when a single process builds more than one host that configures them — for example an admin pipeline alongside a patient-data pipeline. Both plugins tracked "already configured" with a process-wide static flag shared by every host, so a second host skipped real registration and ``$export`` failed with ``A service of type ExportOperationService is required for Bulk Data Export Tasks but has not been registered``. This state is tracked per host now, and the Real World Testing operations are registered on every matching pipeline branch rather than only the first.
#. ``$questionnaire-package`` no longer sets ``Bundle.timestamp`` when the request targets STU3, where that element does not exist. On bundles built through the new ``BundleBuilder`` — search, history, ``$lastn``, ``$docref``, ``Patient/$everything`` and ``$questionnaire-package`` responses — ``meta.lastUpdated`` and ``Bundle.timestamp`` now come from a single clock read, so they no longer diverge by the time it takes to assemble the entries. Batch and transaction response bundles are unchanged.

Security
^^^^^^^^

#. The product images now ship the .NET 10.0.11 runtime instead of 10.0.10, picking up the fixes for CVE-2026-62901 (denial of service, HIGH), CVE-2026-62909 (uncaught exception) and CVE-2026-62899 (HTTP request smuggling). This covers the Firely Server image, the Firely Server Ingest image and the store microservice image.

Known behavioral changes
^^^^^^^^^^^^^^^^^^^^^^^^

**Quality measures and CQL**

#. **Several points of CQL semantics were corrected**, which can change ``Library/$evaluate`` and ``Measure/$evaluate-measure`` results, and therefore measure population membership. Comparing quantities of different dimensions now yields ``null`` instead of comparing canonicalized numbers (``1 'cm' = 0.01 'g'`` is no longer ``true``), while arithmetic across compatible units now succeeds where it previously failed; equivalence (``~``) between dates, date-times or times of differing precision now yields ``false``; and a resolved value set is interpreted under closed-world semantics, so a code absent from the expansion yields ``false`` rather than an unknown result. See :ref:`feature_cql_semantics`.
#. **CQL evaluation output changed shape in several places.** An interval of ``Integer``, ``Decimal`` or ``Long`` serialized into a FHIR ``Range`` now reports an open bound as its closed equivalent and carries a ``quantity-precision`` extension on every bound; a UTC instant is written with the ``Z`` designator instead of a ``+00:00`` offset; a ``Time`` no longer carries a timezone; and the ``cqf-cqlType`` extension reports CQL model names such as ``FHIR.Encounter`` and ``List<System.Integer>``. Clients that compare these values exactly must be updated. See :ref:`feature_cql_result_mapping`.
#. **The ``subject`` parameter of ``$cql``, ``Library/$evaluate`` and ``Measure/$evaluate-measure`` must be a relative ``ResourceType/id`` reference**, for example ``subject=Patient/pat1``. Absolute URLs, versioned references and bare ids were previously accepted and silently normalized, and are now rejected with an ``OperationOutcome``. See :ref:`feature_cql_subject`.
#. **``Measure/$evaluate-measure`` validates a ``Measure`` much more strictly, before any CQL is evaluated.** Definitions that previously failed with an unhandled HTTP 500, were silently mis-evaluated, or had populations quietly dropped from the report are now rejected up front with an actionable ``OperationOutcome``. See :ref:`feature_measure_evaluate_validation` for the full set of conditions and the status code each is reported with.
#. **A patient compartment bundle that Firely Server resolved itself is no longer re-validated**, which also removes a full search-parameter indexing pass per subject. As a result, a compartment bundle containing a resource shared with another patient — for example an ``Appointment`` with several patient participants — is no longer rejected as inconsistent. Caller-supplied ``data`` bundles, bundles from a remote data endpoint and every ``Library/$evaluate`` request are still validated as before.

**Other**

#. **``$member-match`` is no longer reachable outside FHIR R4.** The operation implements the Da Vinci HRex IG, which is published for R4 only, and was already advertised in the CapabilityStatement of R4 endpoints alone. It was nevertheless registered on every information model, so on an STU3 or R5 endpoint a client that guessed the URL got a response from an operation it could not discover — and on R5 the privacy-sensitivity check read ``Consent.policy``, an element R5 replaced, so the check silently passed. It is now registered for R4 only, matching where it is advertised.

Programming API changes and plugins
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

**Quality measures and CQL**

#. The CQL engine was upgraded from ``Hl7.Cql`` 2.9.2 to 2.15.0. Most of the CQL behavioral changes listed above originate there. If you build plugins against ``Hl7.Cql``, recompile them against the same version.
#. The plugin configuration order of ``Measure/$data-requirements`` changed from ``5390`` to ``5395``, resolving a collision with the ``$cql`` operation that made their relative registration order non-deterministic. Note the new value if you reference plugin order values in custom pipeline configuration.

**Other**

#. The ``ITypedElement``- and ``ISourceNode``-based ``FhirPatch`` overloads are now marked ``[Obsolete]`` and will be removed in the next major version of Firely Server. Use ``FhirPatch(PocoNode, PocoNode, ModelInspector)`` instead, converting both inputs with ``ToPocoNode(ModelInspector)`` first, so the patch is applied to the FHIR POCO directly instead of through a lazily evaluated ``ITypedElement`` wrapper chain.
#. Added ``Vonk.Core.Common.BundleBuilder``, a POCO-based helper for building ``Bundle`` resources. ``BundleBuilder.Create``, ``AddEntry``, ``AddSearchEntries``, ``WithTotal`` and ``ToSearchsetBundle`` build a ``Hl7.Fhir.Model.Bundle`` directly, replacing the ``ISourceNode``-based ``GenericBundle``/``SearchBundle``/``HistoryBundle`` deprecated in 6.9.0.
#. CQL service registration, and that of Bulk Data Export and Real World Testing, is now idempotent per host or service collection instead of per process, so registration is no longer wrongly skipped when Firely Server pipelines are hosted more than once in a single process. There is no behavior change for a standard server process.

.. _vonk_releasenotes_6_9_1:

Release 6.9.1, July 28th, 2026
-------------------------------

Improvements
^^^^^^^^^^^^

#. Chained and reverse-chained (``_has``) searches on SQL Server are now faster for large, non-selective chains. The database uses a more efficient query plan for these searches, with no change in search results.
#. Reverse-chained (``_has``) searches on MongoDB (e.g. ``Patient?_has:Coverage:beneficiary:identifier=...``) are now significantly faster on large databases. Their performance no longer depends on the number of resources of the searched type, matching how forward chained searches already behave. The ``MongoDbOptions:ChainingThreshold`` setting now also applies to ``_has`` sub-queries.

Features
^^^^^^^^

#. Added the ``SmartAuthorizationOptions:AllowHistory`` setting (default ``false``). When enabled, ``_history`` requests, ``vread``, and version-level custom operations are no longer rejected under SMART authorization; access is evaluated against the *current* version of the resource, since historical versions are not separately indexed. The default behavior is unchanged — history access remains forbidden under SMART authorization unless this setting is enabled — and the server log now mentions the setting by name whenever such a request is rejected.
#. When ``AllowHistory`` is enabled, access to historical versions of a **deleted** resource is restricted to callers whose authorization covers the resource type as a whole (e.g. a scope like ``user/*.read``); callers who are only authorized for specific resource instances cannot retrieve historical content of a resource they never had a claim to, since a deleted resource's current version is a tombstone with no compartment or scope information to check against. Requesting the deleted version itself still correctly reports HTTP 410 Gone. Paging parameters (``_count``, ``_since``, ``_sort``, ``_skip``, ``_total``) are fully honored on authorized ``_history`` requests; a single request returns at most ``BundleOptions:MaxCount`` resources, with a warning logged if more would match.
#. FSI: added a ``--sourceFilter`` option for the filesystem source, to only import files whose name matches a given wildcard pattern (e.g. ``*.ndjson`` or ``patients_??.ndjson``) from the source directory and its subdirectories. When not set, all files are imported as before. The pattern must be a file name pattern (no directory separators), and is validated at startup.
#. Validation: Introduced the setting ``Validation:FixInvalidPrimitiveTypes`` (default ``true``). When enabled, invalid primitive types are fixed to the correct type where possible. When disabled, invalid primitive types are always returned as an error. This setting was introduced to prevent breaking existing workflows that rely on permissive parsing of invalid primitive types, but as it can increase load on the server, it is recommended to set it to ``false`` in order to avoid unnecessary overhead when processing large amounts of data. See :ref:`feature_fix_invalid_primitive_types` for more information.

Fixed
^^^^^

#. Fixed an issue where custom resources could not be used after first start-up of Firely Server due to a timing issue when generating the snapshot. Only after the second startup would the custom resource be available for use. This issue was only present in Firely Server v6.9.0 and has been fixed in this release.
#. Fixed an issue where upon first startup of Firely Server conformance resources from Simplifier projects or CAR files could not be loaded together with conformance resources from files/folders in the ``vonk-import`` directory. These conformance resources would only be loaded upon the second startup of Firely Server.

.. _vonk_releasenotes_6_9_0:

Release 6.9.0, July 22nd, 2026
-------------------------------

This release migrates Firely Server's internal resource model to be backed directly by the Firely .NET SDK POCO model, and upgrades the runtime to .NET 10. These are internal changes, but they unlock full support for custom resources as validated, typed resources, and improve performance across parsing, serializing and searching. Custom plugins should keep working unmodified, but please review the "Programming API changes and plugins" section below if you have custom plugins.

.. attention::
    The bundle ``prev`` paging link relation has been renamed to ``previous``, to align with the FHIR specification. Clients that parse paging links by relation name must be updated accordingly.

.. attention::
    Firely Server now targets .NET 10. If you build custom plugins or run Firely Server framework-dependent (rather than via the provided Docker image), make sure the .NET 10 runtime is installed and plugins are recompiled against it.

Improvements
^^^^^^^^^^^^

#. Reduced memory allocations and eliminated blocking I/O when parsing incoming FHIR JSON/XML request bodies and when serializing FHIR JSON/XML response bodies. Request and response bodies are now read/written directly from/to pooled buffers instead of being materialized as intermediate strings. No behavioral change, but this improves throughput, especially for larger payloads.
#. Improved the performance of ``batch``/``transaction`` Bundle processing. Reading per-entry fields such as ``fullUrl``, ``request.method`` and ``request.url`` no longer goes through the FHIRPath engine, so processing time for large bundles no longer scales with FHIRPath evaluation overhead.
#. Reduced the performance overhead of permissive-mode primitive-type coercion (e.g. a JSON string value in an integer field). Previously, every parsed resource was re-walked and re-validated to check for wrongly-typed primitives. Now this only happens for values that actually need correcting, so the overwhelming majority of (valid) data no longer pays any overhead. This noticeably speeds up large result sets, such as ``Patient/$everything``. Behavior is unchanged: permissive mode still auto-corrects wrongly-typed primitives and logs a warning when it does so.
#. FHIRPath ``$patch`` now operates directly on the FHIR POCO tree instead of through an intermediate lazy representation, removing redundant re-parsing and correctly preserving FHIR-specific type metadata (e.g. the ``xhtml`` type of ``Narrative.div``) through ``add``, ``delete``, ``insert``, ``move`` and ``replace`` operations.
#. The default set of additional resources returned by Patient ``$everything`` and Bulk Data ``$export`` now includes ``Practitioner`` and ``PractitionerRole`` (Bulk Data ``$export`` also already included ``Practitioner``), so both operations return these commonly-referenced supporting resources out of the box. This is configurable via ``PatientEverythingOperation:AdditionalResources`` and ``BulkDataExport:AdditionalResources`` respectively, in case you want to opt out.
#. Added a new ``SqlDbOptions:PatientEverythingTimeout`` setting (in seconds, default 300), to configure the SQL command timeout for the Patient ``$everything`` query (also used by ``$purge``) independently from other operations. Previously this query used the default SQL command timeout of 30 seconds, which could be too short for patients with a large amount of data.

Features
^^^^^^^^

#. Custom resources are now fully supported as validated, typed resources, rather than being tolerated as untyped/dynamic content, provided a matching ``StructureDefinition`` is registered in the administration database. This applies to custom resources at the root of a request, nested inside a Bundle entry, or as a contained resource, as well as to any custom datatypes they reference. Resource types that are still unknown to Firely Server continue to be tolerated permissively, as before. See :ref:`feature_customresources` for more information.
#. Firely Server Ingest (FSI) now also coerces wrongly-typed primitive values (e.g. a numeric value provided as a JSON string) to their correct native type during import, matching the behavior already applied by the regular REST API.
#. ``Measure/$evaluate-measure`` now supports ``subject-list`` as ``reportType`` for ``Group`` subjects. The response is a ``MeasureReport`` with ``type=subject-list``, containing aggregated group population counts, a contained individual ``MeasureReport`` per group member, and contained population ``List`` resources.
#. ``Measure/$evaluate-measure`` can now also be invoked on a specific ``Measure`` instance (e.g. ``Measure/{id}/$evaluate-measure``) using GET or POST, in addition to the existing type-level invocation; supplying a ``url`` parameter on an instance-level call is rejected with HTTP 400, and an unresolvable ``Measure`` id returns HTTP 404. Instance-level ``Library/$evaluate`` (``Library/{id}/$evaluate``) and instance-level ``$data-requirements`` (for both ``Library/{id}`` and ``Measure/{id}``) are likewise now fully supported, resolving their target resource the same way as the type-level operations.

Fix
^^^

#. Requesting a FHIR version that is not supported or not enabled could return an unhandled HTTP 500 error instead of a graceful ``400 Bad Request`` with an ``OperationOutcome``. This has been fixed.
#. The ``PATCH`` ``move`` operation now correctly applies multiple ``move`` operations submitted in a single request. As specified, the operations are applied sequentially.
#. ``PATCH`` no longer rejects a request because of unrelated elements elsewhere in the resource that were already tolerated on ``create``/``update`` (for example, a boolean value permissively accepted as a JSON string). Only the elements actually touched by the patch are validated. As part of this fix, the ``add`` operation now explicitly rejects unknown element names with a clear error, instead of silently accepting them.
#. Fixed reference resolution when posting Document bundles: ``Composition.subject`` and ``Composition.encounter`` are now resolved from the original, absolute references in the submitted bundle, instead of from references that had already been rewritten by the server. This prevents references from occasionally being resolved incorrectly.
#. ``create``/``update`` requests containing an unknown element (e.g. an unrecognized property on a ``Patient``) are again correctly rejected with a ``400`` structural ``OperationOutcome``, restoring behavior that had regressed during the internal POCO migration. Recoverable value-level issues, such as an invalid ``id`` literal or an out-of-cardinality element, remain tolerated by permissive parsing as before.
#. The background maintenance service no longer stops permanently after an error in a single maintenance job (e.g. a SQL timeout or deadlock). Such errors are now caught and logged per job, and the service continues with the next scheduled run instead of requiring a server restart.
#. Restored administration database command logging (``Microsoft.EntityFrameworkCore.Database.Command``) for SQL Server and SQLite, which had silently stopped emitting log entries for administration-database reads and writes (e.g. conformance reads during model building, updates to ``StructureDefinition``/``SearchParameter``).
#. CQL: when a ``Library/$evaluate`` dependency ``ValueSet`` cannot be resolved, the error and log message now include the ``url`` and ``version`` of the referencing ``Library``, making it clear where the unresolved dependency originates.
#. CQL: ``Measure/$evaluate-measure`` now populates ``MeasureReport.measure`` from the resolved ``Measure.url`` (including ``|version`` when set) instead of from the request's ``url`` parameter, so the canonical reference is also populated correctly for instance-level invocations.
#. FSI: a transaction rollback error during SQL deadlock retries (``This SqlTransaction has completed; it is no longer usable``) could prevent the automatic retry from running, causing the import to fail immediately instead of retrying with backoff. Rollback now tolerates an already-completed transaction.
#. Fixed an issue where Firely Server's SQL Server database bootstrap could fail to start when running under minimal/least-privilege SQL permissions, caused by the use of globally-scoped temporary stored procedures. These are now session-scoped, requiring less broad permissions and avoiding collisions between concurrently-starting instances.


Programming API changes and plugins
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Firely Server 6.9.0 migrates its internal resource model from an ``ISourceNode``-based wrapper chain to a FHIR SDK POCO-backed ``PocoNode``, and upgrades the target framework from .NET 8 to .NET 10. Most plugins will continue to compile and run unmodified, but there are a few areas where behavior has changed, where existing APIs are now deprecated in favor of direct POCO access, or where recompilation is required:

#. Firely Server was updated to use the Firely .NET SDK v6.3.0. For those implementing custom plugins or facades, we recommend updating these to use the same SDK version when upgrading to this version of Firely Server.
#. The target framework was upgraded from .NET 8 to .NET 10 (C# language version 14.0). Custom plugins need to be recompiled targeting ``net10.0``. The Docker images (both the server and the Firely Server Ingest CLI) are now based on .NET 10 Alpine base images.
#. ``Vonk.Fhir.R4.Internal`` no longer has a hard dependency on the CQL plugin. The dependency is now inverted through an internal ``ICqlLibraryCompiler`` hook: when the CQL plugin is loaded it registers its compiler and ``Library`` conformance resolution compiles CQL/ELM content as before; when the CQL plugin is not loaded, ``Library`` resolution simply skips compilation. This is only relevant if you build against ``Vonk.Fhir.R4.Internal`` directly.
#. ``IResource`` now extends ``IAnnotatable``, with default no-op implementations, so existing custom ``IResource`` implementations still compile. However, the new status helpers ``WithCurrency``, ``WithChange`` and ``WithMismatchedReferences`` (and their ``Get*`` counterparts) store their state as annotations. If a custom ``IResource`` implementation does not forward annotation calls to a real store, these helpers will silently do nothing on it. Implement ``IAnnotatable``/``IAnnotated`` explicitly if you have custom ``IResource`` implementations.
#. Resource mutation is now in-place rather than copy-on-write. Methods such as ``SetId``, ``EnsureMeta``, ``SetValueAt``, ``WithCurrency``, ``WithChange`` and ``WithMismatchedReferences`` used to return a new, distinct ``IResource`` instance and leave the original unchanged; they now mutate the underlying POCO in place and return the *same* instance. Plugin code that relied on the original reference staying unchanged must be updated; deep-copy the underlying FHIR POCO beforehand if the pre-mutation state needs to be preserved.
#. A number of ``ISourceNode``/``IResource`` mutation extension methods are now deprecated and will be removed in a future major release: ``Patch``, ``ForcePatch``, ``ForcePatchAt``, ``ForceAdd``, ``Add``, ``AddIf``, ``AddIfNotExists``, ``AddOrReplace``, ``Remove`` and ``Revalue`` on ``ISourceNode``; ``Patch`` and ``ForcePatch`` on ``IResource``. Cast the resource to ``PocoNode`` (or check with ``is PocoNode``) and mutate its ``.Poco`` property (``Hl7.Fhir.Model.Resource``) directly using the FHIR SDK API instead.
#. The fluent bundle-builder classes ``GenericBundle``, ``SearchBundle`` and ``HistoryBundle`` (and their builder methods, e.g. ``AddLink``, ``Total``, ``AddSearchEntries``, ``ToSearchBundle``, ``ToHistoryBundle``) are now deprecated and will be removed in a future major release. Build Bundle responses directly using the FHIR SDK ``Bundle`` POCO (``Hl7.Fhir.Model.Bundle``) instead, populating ``bundle.Entry`` yourself; use the new ``ResultPage.SetLinks`` helper for paging links.
#. ``IResourceCurrencyProvider``, ``IResourceChangeProvider`` and ``IResourceMismatchedReferenceProvider`` are now deprecated. Use the annotation-based extension methods instead: ``SearchResourceExtensions.GetCurrencyIndicator``/``WithCurrency``, ``GetChangeIndicator``/``WithChange``, and ``GetMismatchedReferences``/``WithMismatchedReferences`` respectively.

.. _vonk_releasenotes_6_8_1:

Release 6.8.1, June 12th, 2026
------------------------------

Improvements
^^^^^^^^^^^^    

#. Upgraded the enterprise validator that includes two major improvements:
    - Resources that are referenced in a Composition resource are now resolved when validating the Compostion resource. See :ref:`feature_advancedvalidation` for more information.
    - QuestionnaireResponse ``item.answers`` will now be validated against the Questionnaire ``answerOptions`` within the following specification-defined constraints: 
        - type of the ``value[x]`` should match the ``item.type``
        - ``Coding.display``, ``ResourceReference.display`` and ``Quantity.unit`` are not taken into account in answer validation, unless they are the only element provided in the answer
    

.. _vonk_releasenotes_6_8_0:

Release 6.8.0, June 8th, 2026
-----------------------------

Improvements
^^^^^^^^^^^^

#. Updated conformance cache configuration to ``ConformanceCache`` and added ``SlidingExpirationSeconds`` to control cache entry lifetime. This improves stability for scenarios that resolve or compile conformance resources over longer periods, such as CQL library dependency chains.
#. Warning on version mismatches in chained queries are now optional, and by default disabled. See :ref:`restful_search`.
#. FSI schema version mismatch error messages are clearer: Reported maximum supported schema versions are corrected to match what the current FS build actually supports.
#. PubSub configuration logging: ``BatchSize``, ``ClaimCheck``, ``ClaimCheck:AzureBlobContainerName`` and ``ClaimCheck:StorageType`` are now emitted by ConfigurationLogger instead of being masked as sensitive. 
#. FSI MessageBroker / RabbitMQ configuration logging: ``Username``, ``VirtualHost`` and the non-secret RabbitMQ.* keys (``Port``, ``UseSsl``, ``ClientCertificatePath``, ``ServerName``) are no longer masked in FSI configuration logs. ``Password`` and ``ClientCertificatePassphrase`` remain masked. 

Features
^^^^^^^^

#. Implemented the DaVinci Data Export (ATR) operation for R4. The operation is registered in the CapabilityStatement and supports both GET and POST, and is gated by a license token. See :ref:`feature_davinci_data_export` for more information.
#. Implemented configurable authentication mode for ``Library/$evaluate``, plus a clear OperationOutcome error and log message when ``useServerData=true`` is requested while ``RemoteDataEndpointsOnly`` is enabled. See :ref:`feature_external_data_endpoints` for more information.
#. Implemented ``MediaType`` configuration for ``Library/$evaluate``. See :ref:`feature_external_data_endpoints` for more information.
#. ``Measure/$evaluate-measure``: ``population`` is now accepted as ``reportType`` alongside ``summary``, restoring compatibility with older FHIR versions that used population. The response is a valid ``MeasureReport`` with ``type=summary`` and population counts.
#. FSI: Implemented SQL Server as an import source. Adds ``--srcType Sql`` with ``--srcSqlConnectionString`` and ``--srcSqlRunningMode`` CLI flags. See :ref:`tool_fsi` for more information.
#. Firely server now supports routing read traffic to a separate SQL Server read replica, leaving the primary database free to handle writes. See :ref:`sql_read_replica` for more information.

Fixed
^^^^^

#. Fixed issue preventing the Simplifier Conformance Import from working. See :ref:`conformance_fromsimplifier` for more information.
#. Validation errors for codes missing in a CodeSystem and for invalid display values now produce informative messages instead of generic ones.
#. ``$export`` POST requests with an empty request body now return ``202 Accepted`` with no filters applied, instead of ``400 Bad Request``. The empty body is valid per spec.
#. Import history: duplicate audit rows in the ``importhistory`` table no longer crash startup.
#. Fixed an issue where duplicate results would be returned when chained queries were executed against a SQL/SQLite backend.

.. _vonk_releasenotes_6_7_1:

Release 6.7.1, May 20th, 2026
-----------------------------

Fix
^^^

#. Introduced pagination for the results of the ``$everything`` operation. Before, when a large number of resources would be returned by the ``$everything`` operation, this could lead to stack overflow errors. With pagination, the results of the ``$everything`` operation are now returned in smaller chunks, improving performance and reducing the likelihood of timeouts. For more information, also see :ref:`patienteverything_pagination`.

.. warning::
    With the change in pagination for the ``$everything`` operation, ``Bundle.total`` has been removed. If your workflow relies on it, we advise to update it and iterate through all pages to retrieve all resources.

.. _vonk_releasenotes_6_7_0:

Release 6.7.0, March 26th, 2026
-------------------------------

Improvements
^^^^^^^^^^^^

#. Improved the performance of SQL Server repositories by restructuring and optimizing several indexes. See the Database section of the release notes for more information about the index changes. 
#. Improved the operation outcome of disabled operations. In case of a disabled delete operation, the outcome would incorrectly indicate that the operation was successful even though the operation was disabled. In the current situation a ``501 Not Implemented`` response is returned with an empty response body.
#. ``BundleOptions`` in the appsettings were not validated upon startup for consistency. This could lead to misconfigurations that would only be noticed when executing a bundle operation. We now validate the ``BundleOptions`` upon startup to prevent this from happening.
#. We improved the resolving of index files in the UI when the server is running in a virtual directory. Before, the UI would not be able to find the index files when running in a virtual directory, which would lead to missing styles and images. This has now been fixed by adjusting the paths to the index files in the UI.
#. We improved handling of Patient Access Metrics sent via OpenTelemetry when no fhirUser could be derived from the access token.
#. We clarified the logs when the ``lastN`` operation would be used together with a SQLite DB. This log message would suggest that only SQL Server repositories support the ``lastN`` operation, which is not the case. The log message has now been updated to clarify that the ``lastN`` operation is supported for MongoDB and SQL Server repositories, but not for SQLite repositories.


Features
^^^^^^^^

#. Introduced advanced terminology validation with Conformance Archives (CAR files), allowing for validation against large and complex terminology systems such as LOINC, ICD10, and SNOMED CT. We provide pre-built CAR files for SCT and LOINC on request. For more information see :ref:`feature_advanced_terminology`. This feature requires a separate license plugin, licenses can be updated upon request.
#. PubSub users that utilize RabbitMQ as a message broker can now specify custom queue arguments when creating queues. For more information see :ref:`pubsub_configuration_rabbitmq`.
#. Introduced the ``$fhirUser-lookup`` operation to look up the fhirUser claim of a patient or practitioner user in Firely Auth. This operation replaces the old fhirUser lookup in FA that existed internally.. It is now exposed as a public operation that can be called by custom plugins or external systems. For more information see :ref:`fhiruserlookup`.
#. FSI now supports ingestion of bundles of type ``collection``, ``transaction``, and ``batch`` in ndjson format.
#. We introduced the ``$questionnaire-package`` operation with support for the ``coverage``, ``questionnaire``, ``changedsince``, and ``packagebundle`` parameters following the specification of the `DTR Questionnaire Package Operation <https://build.fhir.org/ig/HL7/davinci-dtr/en/OperationDefinition-questionnaire-package.html#parameters>`_. This operation requires a separate license plugin. More documentation will follow.

Fix
^^^

#. Fixed an issue with BDE in multi-instance deployments of Firely Server where the same BDE task could be picked up by multiple instances at the same time, which could lead to duplicate processing of the same task. This was caused by that task not getting the correct status update. We have improved handling of these tasks in multi-instance deployments to prevent this from happening and to ensure the process is more robust in case of unexpected crashes or shutdowns of instances.
#. Fixed an issue where the ``_summary`` parameter was applied in searches but not in direct reads.
#. Consolidated the behavior of the ``_since`` filter for ``$PatientEverything`` in SQL and MongoDB repositories. Before, the ``_since`` filter would return additional results in MongoDB repositories due to the way the filter was applied. Now, the behavior of the ``_since`` filter is consistent across both repository types.
#. The ``_summary`` and ``_elements`` parameters would not be applied when used in ``batch`` or ``transaction`` bundles. This has now been fixed so that these parameters are applied correctly in these types of bundles.
#. Fixed an issue where Firely Server would throw an error when handling a ``RetrievePlanCommand`` from RabbitMQ.

Database
^^^^^^^^

#. Optimized several indexes in the SQL Server repository database to improve query performance. This requires an update of the SQL database schema to version v29. The migration will be done automatically upon startup when upgrading from FS 6.x.x, please be aware that this migration can be time-consuming when done on large databases. If you are upgrading from FS 5.x.x, please check the previous release notes for the required migration steps. The following changes were made to the indexes:
    - Updated the vonk.ref.ref_name_relativereference index to include the ``Version`` column if not already present.
    - Replaced the vonk.tkn.ix_tkn_code_name_systemhash index with a new tkn_name_code_systemhash index, reordering the columns to ``Name``, ``Code``, ``SystemHash``.
    - Updated the vonk.ref.ref_name_urlhash index to include additional columns ``EntryId``, ``Id``, ``Url``, ``Version``.
    - Updated the vonk.uri.uri_name_hash index to include the ``UriValue`` column in the INCLUDE clause.


.. _vonk_releasenotes_6_6_0:

Release 6.6.0, January 29th, 2026
---------------------------------

Improvements
^^^^^^^^^^^^

#. Replaced the technical UI framework for the Firely Server Demo Homepage to simply the deployment using subdomains.

Features
^^^^^^^^

#. Add support for the _until parameter in the Bulk Data Export operations on all levels and Patient/$everything.
#. Added support for dedicated OpenTelemetry metrics for counting the Patient Access API metrics according to the CMS definition of the `reporting requirements for CMS-0057-F <https://www.cms.gov/priorities/burden-reduction/overview/interoperability/frequently-asked-questions/patient-access-api>`_. The exporter metric is called "firely.server.cms0057.patient.count".

Fix
^^^

#. Tenant labels are now also applied on contained resources.
#. Posting a Bundle with type=collection returns now a correct OperationOutcome instead of a success message with status code HTTP 501.
#. Fixed an issue due to which $liveness was blocked longer than necessary when loading conformance resources.

.. _vonk_releasenotes_6_5_2:

Release 6.5.2, January 15th, 2026
---------------------------------

Fix
^^^

#. Updated the SQLite dependencies of Firely Server to address `CVE-2025-6965 <https://nvd.nist.gov/vuln/detail/CVE-2025-6965>`_. The package ``SQLitePCLRaw.provider.e_sqlite3`` has been updated to the latest version 3.0.2, and the SQLite version that is used is updated to version 3.50.4.2
#. Updated ``AWSSDK.Core`` dependency to version 4.0.3.8 to address `CVE-2026-22611 <https://nvd.nist.gov/vuln/detail/CVE-2026-22611>`_.

.. _vonk_releasenotes_6_5_1:

Release 6.5.1, November 25th, 2025
----------------------------------

Fix
^^^

#. We updated the dependencies of the docker image to address security vulnerabilities in some of the base layers. The updated base image is now ``mcr.microsoft.com/dotnet/aspnet:8.0.22-alpine3.22``.

.. _vonk_releasenotes_6_5_0:

Release 6.5.0, November 4th, 2025
---------------------------------

Improvements
^^^^^^^^^^^^

#. The behavior of the ``$purge`` operation has been adjusted with regard to Group resources. Purged Patient references are now removed without deleting the entire Group, as Groups may contain additional references to other Patient instances.
#. Firely Server MassTransit dependencies were updated to enhance SASL authentication with Kafka, improving message passing security.

Features
^^^^^^^^

#. It is now possible to configure the file retention period for Bulk Data Export task files. It specifies how long the exported files should be retained on the servr before they are automatically deleted. For more information see :ref:`feature_bulkdataexport_configuration`.
#. SSL configuration details are now supported for RabbitMQ in Firely Server PubSub. It enables configuring SSL settings to secure the connection between Firely Server and RabbitMQ. For more information see :ref:`pubsub_configuration_rabbitmq`.
#. To support quick and easy debugging, Serilog Log Level hot reloading capabilities can now be leverages. The log level of Serilog can now be changed in the logsettings at runtime without restarting Firely Server. For more information see :ref:`hot_reload_log_level`.
#. Added support for indexing custom search parameters in FSI. See :ref:`custom_search_parameters` for more information.
#. We provide a beta release of CDS hooks services. For more information see :ref:`feature_cds_hooks`.

Programming API changes and plugins
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

#. Firely Server was updated to use the Firely .NET SDK v6.0.1. For those implementing custom plugins or facades, we recommend updating these to use the .NET SDK v6.0.1 when upgrading to this version of Firely Server. Please check out the release notes `here <https://github.com/FirelyTeam/firely-net-sdk/releases/tag/v6.0.1>`_ for more information.
#. It is likely all custom plugins need to be recompiled against new version of `Vonk.Core` package due to SDK changes.

Fix
^^^

* Fixed an issue that resulted references not being resolved using the ``resolve()`` function in FHIRPath when validating constraints against resources wrap inside a Bundle.
* The default appsettings missed the ``EnforceAccessPolicies`` element in the ``SmartAuthorizationOptions`` section.
* ``$liveness`` and ``$readiness`` contained invalid values for the ``RequireTenant`` settings in their respective ``Operations`` configuration section.
* Fixed a FHIRPath-related issue when validating the ``ctm-1`` constraint against CarePlan resources.

Known behavioral changes
^^^^^^^^^^^^^^^^^^^^^^^^

#. You may encounter issues ingesting same resources if they contain elements unknown to the StructureDefinition. Previous versions of SDK would discard unknown elements, however, the new SDK will now report these as validation issues.

.. _vonk_releasenotes_6_4_0:

Release 6.4.0, August 26th, 2025
--------------------------------

Fixes
^^^^^

#. We improved the behavior of the validator for resolving references and applying validation in contained resources and bundle resources. FHIRPath constraints using resolve() statements will now evaluate correctly in these situations.

.. _vonk_releasenotes_6_3_1:

Release 6.3.1, August 11th, 2025
--------------------------------

Fixes
^^^^^

#. We updated dependencies of the Elasticsearch sink to fix a security vulnerability in a dependency of the `Elastic.Serilog.Sinks` package. The updated version is now 8.18.2. See the `Elastic Sink 8.18.2 release notes <https://github.com/elastic/ecs-dotnet/releases>`_ for more information.
#. We fixed a bug where FSI would take a long time to start up when the MongoDb target database would contain a large number of resources. This was caused by FSI trying to perform a count on the target database, which would take a long time when there were many resources.

.. _vonk_releasenotes_6_3_0:

Release 6.3.0, July 22th, 2025
------------------------------

Features
^^^^^^^^

#. We introduced the AdvisorRules setting for the validator for Firely Prior Authorization and Scale licenses. The implementation of the Advisor Rules system allows users to customize validation behaviour on a more granular level by setting filters with which the outcome of validation or the validation itself can be modified. Note that this feature is still in beta. For meore information see :ref:`feature_advisor_rules`.
#. The validator will now create extensions on validation errors pointing to the profile that caused the error in the http://hl7.org/fhir/StructureDefinition/operationoutcome-issue-source extension. These issues will also be annotated with line numbers in the http://hl7.org/fhir/StructureDefinition/operationoutcome-issue-col and http://hl7.org/fhir/StructureDefinition/operationoutcome-issue-line extension.

.. _vonk_releasenotes_6_2_0:

Release 6.2.0, July 15th, 2025
------------------------------

Improvements
^^^^^^^^^^^^

#. Updated Serilog ApplicationInsights sink configuration to use Connection String instead of the deprecated Instrumentation Key. Azure no longer supports Instrumentation Keys, so one should use ``connectionString`` in the ApplicationInsights sink configuration. The connection string can also be configured via ``ApplicationInsights:ConnectionString`` in appsettings.json. See :ref:`configure_log_insights` for more information.
#. Updated search anonymization to work across multiple Firely Server instances. This also changed the configuration, see: :ref:`restful_search_anonymization` on how to configure the search anonymization.
#. It is now possible for Firely Server to pick up appsettings.json files during startup by specifying the file location in the environment variable ``VONK_PATH_TO_SETTINGS``. See :ref:`configure_settings_path`. Before, the configuration was only loaded from appsettings.instance.json.
#. We improved the behavior of license checks upon startup so that users will no longer see warnings for unlicensed plugins that are not enabled in the pipeline.
#. We made some improvements to Firely Server Ingest (FSI):
    - We have improved the efficiency of FSI with regard to memory usage/CPU when generating the final usage statistics after a run. This could lead previously to excessive memory consumption and crashes.
    - FSI will now show a warning if it is unable to connect to a source database.

Fixes
^^^^^

#. Requests with a double slash (//) would lead to an uncaught exception. This will now lead to a ``501 Not Implemented`` response in case the double slash is used within the URL and to a ``404 Not Found`` response in case the double slash is at the end of the URL.
#. We made some fixes to the Vonk.Facade.Starter kit to help developers on their way with building a facade.
    - It is now possible to create Observation resources again.
    - ``_total=none`` is now handled properly. Before this would lead to an error when doing a search.

Features
^^^^^^^^

#. It is now possible to validate QuestionnaireResponse resources against their original Questionnaire resource. See :ref:`feature_advancedvalidation` for more information.
#. Message brokers can now be used as a target for Firely Server Ingest. FSI will publish messages to the message broker upon ingesting resources, which can then be consumed by Firely Server. Currently, only Azure Service Bus and RabbitMQ can be configured as message brokers for FSI. The use of a MongoDb source is not supported if the target is set to a message broker, only ingestion from files/folders is supported. See :ref:`fsi_target_pubsub` for more information.
#. We upgraded the .Net SDK to v5.12.0. See the `SDK 5.12.0 release notes <https://github.com/FirelyTeam/firely-net-sdk/releases/tag/v5.12.0>`_ for more information.


.. _vonk_releasenotes_6_1_0:

Release 6.1.0, May 23rd, 2025
-----------------------------

Security
^^^^^^^^

#. AccessPolicy resources can now only be accessed or modified with system-level scopes (e.g., ``system/AccessPolicy.*``). Patient-level scopes (``patient/AccessPolicy.*``) and user-level scopes (``user/AccessPolicy.*``) are not allowed and will be rejected with a 403 Forbidden response.
#. ``TrustedProxyIPNetworks`` now has an additional setting ``AllowAnyNetworkOrigins`` to allow any network origins to be trusted. Before, this configuration was only allowed if ``ASPNETCORE_ENVIRONMENT`` was set to ``Development``. Systems that used this environment variable to bypass the ip-range restrictions should switch to using this setting instead. This setting is disabled by default and should only be enabled if you are sure that your network is secure.
#. We added a check to the SMART on FHIR settings to ensure that ``Authority`` is always configured.
#. We added the ``ClockSkew`` setting to the ``SmartAuthorizationOptions``. This setting is used to adjust the expiration time and validity of JWT tokens. Before, you could only adjust the expiration time of a JWT token in FA, and Firely server would add an additional window of 5 minutes to this expiration time where the token would still be valid. This window can now be adjusted with this setting.  See :ref:`feature_accesscontrol_config` for more information.


Improvements and Fixes
^^^^^^^^^^^^^^^^^^^^^^

#. We improved the behavior of AuditEvent generation in combination with ``$member-match``. The AuditEvent will now capture the Patient ID and Identifier of the member after a successful match.
#. We improved the performance of snapshot generation queries for Bulk Data Export against a SQL back-end.
#. We fixed a bug for the Document Handling operation. Before, references of the posted document bundle could not always be resolved.
#. We improved error messaging of Firely Server for SMART on FHIR reference tokens. Operation Outcomes indicating errors with regard to the token would only mention JWT tokens when a reference token was used. As this was misleading, we adjusted the error message to dynamically show the type of token that was used. 
#. We fixed a bug in the handling of the ``above`` modifier in search queries. Firely Server does not support the ``above`` modifier and would show a large stack trace when this modifier was used in queries. Error handling for the use of this modifier is now improved.

Features
^^^^^^^^

#. We added support for the use of the Claim Check pattern in PubSub. This features allows you to outsource the payload of a message to an Azure Blob Storage Account that can be referenced in the message, leading to smaller messages and improved performance. See :ref:`pubsub_claimcheck` for more information.

=======

.. _vonk_releasenotes_6_0_0:

Release 6.0.0, April 15th, 2025
-------------------------------

Firely is proud to announce a new major version of Firely Server. This release represents a significant step forward in our commitment to providing a reliable, compliant, and easy to use FHIR server.
With this new version, we've focused on delivering:

- support for Sharding with MongoDB (see :ref:`configure_mongodb_sharding`)
- zero-downtime migrations with MongoDB (see :ref:`zero_downtime_migration`)
- detailed insights into Firely Server deployments based on OpenTelemetry metrics and traces (see :ref:`feature_opentelemetry`)
- improved integration into existing infrastructures with Kafka support for Firely Server PubSub  (see :ref:`pubsub_configuration`)
- out-of-the-box compliance with more HL7 DaVinci Implementation Guides, e.g. by providing support for the HRex $member-match operation (see :ref:`davinci_pdex_ig`)
- flexibility for deployments requiring multi-tenancy (see :ref:`feature_multitenancy`)

Please study the release notes carefully as they contain breaking changes to the behavior of Firely Server, as well as the configuration of the server. 
Our support team is happy to provide assistance in the upgrade and can be reached at `server@fire.ly <mailto:server@fire.ly>`_ or through the support desk.
Need hands-on support with your upgrade? Our expert consultants are here to help. Explore our `Upgrade Support Package <https://fire.ly/upgrade-support-package/>`_ to get started.

.. note::
    With the release of Firely Server 6.0, we will officially stop support for Firely Server v4.x. We will continue supporting customers that run Firely Server v5.x.

Security
^^^^^^^^

#. To avoid accidentally granting access to AccessPolicies, ``AccessPolicy`` resources cannot be accessed by a resource wildcard scope. E.g. ``system/*.*`` should be replaced with  - ``system/AccessPolicy.*`` to be able to access AccessPolicy resources.
#. The ``$lastN`` operation can now be used with in combination with permissions defined in an ``AccessPolicy`` resource.
#. Intreractions with system-level scopes where the token is bound to a fhirUser of type ``Device`` will be rejected if no matching ``AccessPolicy`` can be found.

Database
^^^^^^^^
#. Raised the minimum supported version of MongoDB to 6.0 to enable sharding.
#. Sharding is now natively supported by Firely Server when using MongoDB as the database backend (see :ref:`configure_mongodb_sharding`). Sharding improves the read/write performance of Firely Server. A new license token is required for this feature. Please contact us for an updated license.
#. Virtual multi-tenancy can now be enabled to logically separate stored resources in the database. The tenant identifier can be retrieved either from an HTTP header value or from a token claim (see :ref:`feature_multitenancy`).
#. Firely Server Ingest can now auto-provision the target database to facilitate zero-downtime migrations (see :ref:`zero_downtime_migration`). A new license token is required for this feature. Please contact us for an updated license.

.. attention::
    Firely Server requires a schema upgrade to version v28 of the database. This is only required for MongoDB database backends. The migration MUST be done using the zero-downtime migration process.

Features
^^^^^^^^

#. Firely Server now implements the ``$member-match`` operation to find members of a health plan based on demographic information. See :ref:`member-match` for more information.
#. Traces and ASP .NET metrics based on ``OpenTelemetry`` can now be exported to OTLP-enabled backends. See :ref:`feature_opentelemetry` for more information.
#. ``memberOf()`` expressions are now supported in FHIRPath constraints when validating resources.
#. Added support for validating MIME types (bcp:13) and language codes (bcp:47).
#. Firely Server has a new homepage featuring a refreshed and modern UI.
#. ``$realworldtesting`` can now be executed using a POST request.
#. It is now possible to disable the create-on-update feature with a new setting in the ``FhirCapabilities`` section of the app settings. See :ref:`fhir_capabilities` for more information.
#. With this release ``Update with no changes (No-Op)`` is enabled by default. For more information about the plugin see :ref:`restful_noop`.
#. The NoOp plugin now also works in combination with transaction bundles.
#. Added support for reading messages from a Kafka topic when using Firely Server PubSub.
#. We have updated the validator api that is used by Firely Server for improved validation.
#. Added support for JWT-based authentication against remote terminology services. See :ref:`feature_terminologyoptions` for more information.
#. Expose port option in PubSub for RabbitMQ. See :ref:`pubsub_configuration` for more information.
#. Performance counters are now exported via OpenTelemetry when ingesting data via Firely Server Ingest.
#. Enable use of AuditEvent output parameters (e.g. IP address) for regular logging.

.. attention::
    With the introduction of the new validator it is no longer allowed to use id fields containing underscores (``_``) in the resource id.

Programming API changes and plugins
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

#. Upgraded the Firely .NET SDK to v5.11.4, see its `release notes <https://github.com/FirelyTeam/firely-net-sdk/releases/tag/v5.11.4>`_.
#. Upgraded to v2.0 of the `firely-validator-api <https://github.com/FirelyTeam/firely-validator-api>`_ for validation and removed the legacy validator previous used. This applies to all validation within Firely Server.
#. ``ISearchRepository`` programming API has been changed to prevent unintended unauthorized access. It is required to explicitly set ``SearchOptions.Authorization`` when calling search, or use one of the extension methods for ISearchRepository, e.g.: ``GetByKeyWithFullAccess`` or ``SearchCurrentWithFullAccess``. ``SearchOptions`` authorization can be configured using one of the extension methods: ``WithAuthorization``, ``WithFullAccess``.
#. ``ISearchRepository`` extension methods that were not accepting ``SearchOptions`` as a parameter: ``GetByKey`` and ``SearchCurrent`` - are replaced with ``GetByKeyWithFullAccess`` and ``SearchCurrentWithFullAccess`` respectively.
#. ``SearchOptions`` is now an immutable record type, which might be a breaking change for some plugin code.
#. Extended the base class ``RelationalQueryFactory`` with support for the ``ResourceTypesNotValue`` (see :ref:`parameter_types`) and methods to express a predicate that is ``AlwaysFalse()`` or ``AlwaysTrue()``.
#. The ``VonkConfigurationAttribute`` no longer supports the deprecated ``isLicensedAs`` property.
#. The deprecated ``VonkConstants.MediaType`` values ``XmlR3``, ``JsonR3`` and ``TurtleR3`` have been removed. Use ``FhirXml``, ``FhirJson`` and ``FhirTurtle`` instead.
#. The deprecated method ``Check.HasValue()`` has been removed. Use ``Check.NotNull()`` instead.
#. Added documentation for ICapabilityStatementBuilder and related methods, see :ref:`vonk_reference_api_capabilities`.
#. Starting from this release the ``Vonk.Smart`` and ``Vonk.Plugin.SoFv2`` plugins are no longer supported and have been removed. They are replaced by the ``Vonk.Plugin.Smart`` plugin. For more information see :ref:`feature_accesscontrol_config`. It is necessary to adjust the pipeline options accordingly.
#. Removed plugin ``Vonk.Plugins.TerminologyIntegration``. ``Vonk.Pluigins.Terminology`` should be used instead.
#. Removed ``ISpecificationZipLocator`` from the public API.

Adjustments and Fixes
^^^^^^^^^^^^^^^^^^^^^

#. "This is an open FHIR endpoint for testing and educational purposes only. Uploading real personal data is strictly prohibited." will no longer be shown on the homepage when running in production mode.
#. Improved transaction handling for MongoDB to avoid duplicate key exceptions during the ingestion of resources.
#. SearchParameters of type ``Reference`` without a target are no longer logged as errors; they are now logged as warnings.
#. Improved handling of invalid resources within batch bundles. Firely Server now returns HTTP 200 - OK with individual OperationOutcomes when resources in the bundle are invalid.
#. Improved handling of large Bulk exports for MongoDB.
#. Fixed pre-validation when a pipe character (|) and a version are used within a canonical in meta.profile.
#. Improved handling of Patch exceptions.
#. Fixed ``FormatException`` when using ``$versions`` with an invalid MIME type.
#. Limited recursive Group-level Bulk exports to skip other Group resources that are transitively included.
#. Authorization endpoints listed in ``AdditionalIssuersInToken`` were previously accepted as the only valid issuers when the setting was used. Now, the authority is also accepted as a valid issuer of tokens.
#. Fixed indexing of elements of type ``url`` for URI search parameters.
#. Improved debug logging for the reindex operation to allow tracking the progress of long-running operations.
#. Administration APIs ``reset``, ``reindex/all``, ``reindex/searchparameters``, ``preload`` and ``importResources`` are now ``$reset``, ``$reindex-all``, ``$reindex``, ``$preload`` and ``$import-resources`` to conform with the naming rules for custom operations.
#. SMART on FHIR v2 scopes can include search arguments. Upon writing resources (create, update, delete) Firely Server used to only evaluate those for ``patient/`` scopes. Now, they are also evaluated for ``user/`` and ``system/`` scopes.

Configuration
^^^^^^^^^^^^^
.. attention::
    Default behavior of Firely Server has been tweaked by changing configuration values. 
    Make sure to reflect the desired behaviour by adjusting ``appsettings.instance.json`` or environment variables.

#. The use of other compartments then Patient in SMART on FHIR authorization is not well defined and potentially unsafe. So we redacted the ``Filters`` settings in ``SmartAuthorizationOptions``. You can now only specify a filter on the Patient compartment. For more information see :ref:`feature_accesscontrol_config`. If you configured just a Patient filter in the old format, Firely Server will interpret it in the new format and log a warning that you should update your settings. If you configured a filter on a different compartment, Firely Server will log an error and halt.
#. Evaluation of :ref:`Subscriptions<feature_subscription>` is now turned off by default. To enable - adjust ``SubscriptionEvaluatorOptions`` accordingly.
#. ``BundleOptions.DefaultTotal`` from now on has a default value of ``none`` for performance reasons. For available options see :ref:`bundle_options`.
#. ``TaskFileManagement.StoragePath`` was already marked as obsolete, and is now also no longer forward compatible. Use the ``TaskFileManagement.StorageService`` settings to provide the storage path, see :ref:`feature_bulkdataexport` for details.
#. ``SupportedInteractionOptions`` type has now been replaced by ``Operations<T>`` to accommodate for the requirements of a configuration revamp.
#. The configuration structure for operations has been completely revamped:

   * ``SupportedInteractionOptions`` has been replaced by a new top-level ``Operations`` configuration section
   * ``Administration.Security.OperationsToBeSecured`` has been replaced by per-operation ``NetworkProtected`` property
   * ``SmartAuthorizationOptions.Protected`` has been replaced by per-operation ``RequireAuthorization`` property
   * Each operation now has granular control over authorization, network protection, tenant requirements, etc.
   * See :ref:`disable_interactions` for detailed information about the new configuration structure and migration guide

.. note::
    If MultiTenancy is enabled, the ``history`` and ``vread`` operations are blocked for all resources. This is to prevent the possibility of cross-tenant access to resources. The ``history`` and ``vread`` operations are not supported in a multi-tenant environment.

