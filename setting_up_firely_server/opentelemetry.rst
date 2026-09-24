.. _feature_opentelemetry:

OpenTelemetry
=============

.. note::

  The features described on this page are available for all :ref:`Firely Server editions <vonk_overview>`

Firely Server can be configured to expose `OpenTelemetry <https://opentelemetry.io/>`_ metrics and traces. By using OpenTelemetry, Firely Server enables observability into its pipeline based on an open standard.
See `What is OpenTelemetry? <https://opentelemetry.io/docs/what-is-opentelemetry/>`_ for an introduction to the standard.

Traces
------

Firely Server emits traces for incoming HTTP requests. 

A trace is composed of a span tree. The top-level span represents the HTTP request, and each middleware handling the request is represented by a child span.
An example of a span emitted by Firely Server is as follows:

* ``traceID``: A unique identifier for the trace. For example, ``fcefb9915e5b65ecbbb07e8311ba86c4``.
* ``spanID``: A unique identifier for the span within the trace. For example, ``87c0e7c64658e367``.
* ``operationName``: The name of the operation being traced. For the middleware span, it is the name of the middleware, like ``DefaultShapesService`` and for the top level span it is the HTTP verb with the path, like ``GET /Observation``.
* ``references``: A list of references to other spans. Each reference includes:
    - ``refType``: The type of reference. For example, ``CHILD_OF``.
    - ``traceID``: The trace ID of the referenced span. For example, ``fcefb9915e5b65ecbbb07e8311ba86c4``.
    - ``spanID``: The span ID of the referenced span. For example, ``9b3ba71db31a66d9``.
* ``startTime``: The start time of the span in nanoseconds since the epoch. For example, ``1742905353049797``.
* ``duration``: The duration of the span in microseconds. For example, ``138``.

The following tags are present in the the top-level span:

* ``server.address``: The address of the server handling the request. For example, ``server.fire.ly``.
* ``http.request.method``: The HTTP method of the request. For example, ``GET``.
* ``url.scheme``: The scheme of the request URL. For example, ``http``.
* ``url.path``: The path of the request URL. For example, ``/Observation``.
* ``network.protocol.version``: The version of the network protocol used. For example, ``1.1``.
* ``user_agent.original``: The user agent string from the client making the request. For example, ``PostmanRuntime/7.43.2``.
* ``scope``: The scope of the span. For example, ``request``.
* ``http.response.status_code``: The HTTP status code of the response. For example, ``200``.
* ``fhir.interaction``: The type of FHIR interaction being performed. For example, ``type_search``.
* ``fhir.model``: The FHIR version model being used. For example, ``Fhir4.0``.
* ``span.kind``: The kind of span. For the top level span, the value is set to ``server``.
* ``internal.span.format``: The format of the span. The value should be ``otlp``.

For each child span corresponding to a middleware span, only a subset of the tags are present: 

* ``fhir.interaction``: The type of FHIR interaction being performed. For example, ``type_search``.
* ``fhir.model``: The FHIR version model being used. For example, ``Fhir4.0``.
* ``url.path``: The path of the request URL. For example, ``/Observation``.
* ``http.request.method``: The HTTP method of the request. For example, ``GET``.
* ``span.kind``: The kind of span. For the child span, the value is set to ``internal``.
* ``internal.span.format``: The format of the span. The value should be ``otlp``.

Metrics
-------
Firely Server publishes the standard .Net metrics for the `HTTP Server <https://opentelemetry.io/docs/specs/semconv/dotnet/dotnet-http-metrics/#http-server>`_ and `Kestrel <https://opentelemetry.io/docs/specs/semconv/dotnet/dotnet-kestrel-metrics/>`_. 

Configuration
-------------

To enable Opentelemetry, one need to add the following configuration to the pipeline options:

.. code-block:: JavaScript

  "OpenTelemetryOptions": {
    "EnableTracing": true,
    "EnableMetrics": true,
    "Endpoint": "<otlp-collector-endpoint>",
    "RecordException": true,
    "SetDbStatementForText": true,
    "SetDbStatementForStoredProcedure": true,
    "VonkSourcesInclude": [
      "*"
    ],
    "VonkSourcesExclude": []
  }

The individual settings are described below:

* ``EnableTracing``: Enables tracing for the application. Set this to ``true`` to capture and export opentelemetry traces.
* ``EnableMetrics``: Enables metrics collection for the application. Set this to ``true`` to capture and export opentelemetry metrics.
* ``Endpoint``: Specifies the endpoint of the OpenTelemetry collector to which the traces and metrics will be sent. Replace `<otlp-collector-endpoint>` with the actual endpoint URL. Note that even though gRPC is used as the exchange protocol for the metrics and traces, the endpoint needs to use ``http`` or ``https`` as the protocol in the url.
* ``RecordException``: When set to ``true``, exceptions will be recorded as part of the trace data.
* ``SetDbStatementForText``: When set to ``true``, database statements for text-based queries will be included in the trace data.
* ``SetDbStatementForStoredProcedure``: When set to ``true``, database statements for stored procedures will be included in the trace data.
* ``VonkSourcesInclude``: A list of ``ActivitySource`` names to include in the telemetry data. Firely Server names its sources after the fully qualified class that emits the spans; a value matches the whole source name, case-insensitively, and may use the wildcards ``*`` and ``?`` (for example ``Vonk.Plugin.Cql.*``). Use ``*`` to include all sources.
* ``VonkSourcesExclude``: A list of span name prefixes to exclude from the telemetry data. A span is not recorded if its name starts with one of these values (case-sensitive). The values are matched against the span name only, not against a namespace: the span of a middleware is named after its service class without the namespace, like ``DefaultShapesService``, and the spans of the CQL plugin have names like ``EvaluateMeasure`` and ``ExecuteCqlExpressions``. For example, ``"Evaluate"`` excludes ``EvaluateMeasure``, ``EvaluateSubject``, ``EvaluateGroup`` and ``EvaluateLibrary``, but not ``ExecuteCqlExpressions``. Each span is matched on its own name, so the spans nested in an excluded span are still recorded unless their own name matches as well. A non-empty list replaces the default parent-based sampling, so the sampling decision carried by an incoming trace context is no longer taken into account. Leave this empty to exclude no spans.

.. _feature_opentelemetry_cql:

Tracing CQL operations
----------------------

The CQL operations of Firely Server (``Library/$evaluate``, ``Measure/$evaluate-measure`` and ``$cql``) emit spans of their own, in addition to the spans of the HTTP request and the middleware. These spans break an evaluation down into its steps, like loading the value sets, retrieving the data of a subject and executing the CQL expressions, so the time spent in each step can be inspected.

Each span is emitted by an OpenTelemetry ``ActivitySource`` that is named after the fully qualified name of the class that emits it, for example ``Vonk.Plugin.Cql.Operations.Library.Evaluate.Internal.LibraryEvaluateOperationService``. The values of ``VonkSourcesInclude`` are passed to OpenTelemetry as the names of the sources to listen to. OpenTelemetry matches such a value against the whole source name, case-insensitively, and supports the wildcards ``*`` (any sequence of characters) and ``?`` (a single character). To trace the CQL operations, include either ``"*"`` or a pattern like ``"Vonk.Plugin.Cql.*"``. A value without a wildcard only matches a source with exactly that name, so ``"Vonk.Plugin.Cql"`` on its own does not enable any of the CQL spans.

.. code-block:: JavaScript

  "OpenTelemetryOptions": {
    "EnableTracing": true,
    "Endpoint": "<otlp-collector-endpoint>",
    "VonkSourcesInclude": [
      "Vonk.Plugin.Cql.*"
    ],
    "VonkSourcesExclude": []
  }

The table below lists the spans per source. The source names are abbreviated: ``Library.Evaluate`` stands for ``Vonk.Plugin.Cql.Operations.Library.Evaluate.Internal``, ``Measure.Evaluate`` for ``Vonk.Plugin.Cql.Operations.Measure.Evaluate.Internal`` and ``Infrastructure`` for ``Vonk.Plugin.Cql.Infrastructure``.

.. list-table::
   :widths: 30 25 45
   :header-rows: 1

   * - ActivitySource
     - Span name
     - Tags
   * - ``Library.Evaluate.LibraryEvaluateOperationService``
     - ``EvaluateLibrary``
     - ``cql.library.url``: the canonical url of the evaluated Library, as requested.
   * -
     - ``ResolveOperationContext``
     -
   * -
     - ``ExtractLibraryParameters``
     -
   * -
     - ``ApplyResponse``
     -
   * -
     - ``ConvertCqlResults``
     - ``cql.result_count``: the number of expression results.
   * - ``Library.Evaluate.LibraryEvaluateOperationContextResolver``
     - ``ResolveLibraryDependencies``
     -
   * -
     - ``LoadValueSets``
     - ``cql.valueset_count``: the number of value sets the libraries depend on.
   * -
     - ``ExtractAssemblyBinaries``
     -
   * - ``Library.Evaluate.LibraryEvaluateOperationContext``
     - ``ValidateDataBundle``
     -
   * -
     - ``CreateCqlContext``
     - ``cql.bundle_entry_count``: the number of entries in the data bundle. ``cql.data_source_reused``: ``true`` when the data source of the subject, built by an earlier group of the same ``Measure/$evaluate-measure`` request, is reused. Both tags are only present when the evaluation has a data bundle.
   * -
     - ``ExecuteCqlLibrary``
     - ``cql.library.name``, ``cql.library.version``, ``cql.expression_count``: the number of expressions evaluated.
   * -
     - ``SelectExpressions``
     - ``cql.expression_count``
   * -
     - ``ExecuteCqlExpressions``
     - ``cql.expression_count``
   * - ``Library.Evaluate.LibraryEvaluationDataProvider``
     - ``RetrieveDataBundle``
     - ``cql.data_source``: where the data comes from, ``remote`` (a remote data endpoint), ``supplied`` (the ``data`` parameter of the request) or ``server`` (the database of Firely Server). ``fhir.bundle.entry_count``: the number of entries in the retrieved bundle.
   * -
     - ``GetPatientEverythingRemote``
     - ``fhir.bundle.page_count``: the number of pages retrieved from the remote data endpoint.
   * - ``Measure.Evaluate.MeasureEvaluateOperationService``
     - ``ResolveOperationContext``
     -
   * -
     - ``ApplyRawResult``
     -
   * -
     - ``ExtractMeasureResults``
     -
   * -
     - ``BuildMeasureReport``
     -
   * -
     - ``PersistMeasureReport``
     -
   * - ``Measure.Evaluate.MeasureEvaluateOperationContext``
     - ``EvaluateMeasure``
     - ``fhir.measure.subject_count``: the number of subjects evaluated. ``fhir.measure.name``, ``fhir.measure.version``, ``fhir.measure.group_count``: the number of groups of the Measure.
   * -
     - ``ResolveLibraryDataRequirements``
     -
   * -
     - ``EvaluateSubject``
     -
   * -
     - ``EvaluateGroup``
     - ``fhir.measure.group_id``, ``cql.expression_count``: the number of expressions evaluated for the group.
   * - ``Infrastructure.PatientEverythingBundleProvider``
     - ``GetPatientEverything``
     - ``fhir.bundle.entry_count``: the number of resources retrieved for the subject.
   * - ``Infrastructure.CqlCompilerService``
     - ``CompileCqlLibrary``
     - ``cql.library.name``, ``cql.library.version``

Some spans are only emitted when the corresponding step takes place. For example, ``GetPatientEverythingRemote`` is only emitted for a remote data endpoint, and ``ValidateDataBundle`` is only emitted when the evaluation has a data bundle. When the groups of a ``Measure/$evaluate-measure`` request share the data bundle of a subject, that bundle is validated, and ``ValidateDataBundle`` emitted, once for the subject rather than once per group.

Note the following:

* ``$cql`` and the CDS Hooks CRD hook evaluate CQL by running ``Library/$evaluate`` within the same request. Their traces therefore contain the ``EvaluateLibrary`` span and the spans nested in it as well.
* ``Measure/$evaluate-measure`` evaluates every group of the Measure for every subject. For each subject it emits an ``EvaluateSubject`` span, with an ``EvaluateGroup`` span per group, and each ``EvaluateGroup`` span holds an ``EvaluateLibrary`` span with the spans nested in it. An evaluation for a Group of patients, or for a whole population, therefore produces a large number of spans. Use the ``VonkSourcesExclude`` setting described above to leave out the spans you do not need. It matches the start of the span name, so ``"ExecuteCql"`` excludes both ``ExecuteCqlLibrary`` and ``ExecuteCqlExpressions``.
* The CQL spans do not carry patient identifiers. Their tags identify the evaluated Library, Measure and Measure group, and otherwise only carry counts and the kind of data source.
* The CQL spans do not set an error status. A failed evaluation is reported in the ``OperationOutcome`` of the response and in the log of Firely Server; the top-level span of the request carries the HTTP status code in ``http.response.status_code``.

Usages
------
Firely Server's OpenTelemetry integration provides the following usages:

1. **Inspecting Individual Traces**:
    Traces can be sent to tools like `Jaeger <https://www.jaegertracing.io/>`_ or `Seq <https://datalust.co/seq>`_ to monitor and analyze the processing of individual requests. This helps in identifying bottlenecks or errors in the request handling pipeline.

2. **Monitoring Dashboards**:
    Metrics collected by Firely Server can be visualized by the Firely Metrics Dashboard, highlighting selected metrics relevant to a Firely Server and Firely Auth deployment. See :ref:`feature_metrics_dashboard` for more details. Additionally, metrics can be exported to a time series database like `Prometheus <https://prometheus.io/>`_ and visualized in `Grafana <https://grafana.com/>`_ or ingested directly into `Azure Application Insight <https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview>`_ or its equivalent in other cloud providers. These dashboards provide insights into the overall performance and health of the server, such as request rates, latencies, and error rates.

3. **Using OpenTelemetry Collector**:
    It is recommended to use a service like `OpenTelemetry Collector <https://opentelemetry.io/docs/collector/>`_ to filter, process, and dispatch traces and metrics to different endpoints. The collector acts as a central hub for telemetry data, enabling flexible routing and aggregation of data to various backends.

4. **Real-World Testing**:
    The :ref:`Real-World Testing <feature_realworldtesting>` feature relies on OpenTelemetry traces to analyze server activities.
