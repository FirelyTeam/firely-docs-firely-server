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
* ``VonkSourcesInclude``: A list of namespace patterns to include in the telemetry data. Use ``*`` to include all sources.
* ``VonkSourcesExclude``: A list of namespace patterns to exclude from the telemetry data. Leave this empty to exclude no sources.

Usages
------
Firely Server's OpenTelemetry integration provides the following usages:

1. **Inspecting Individual Traces**:
    Traces can be sent to tools like `Jaeger <https://www.jaegertracing.io/>`_ or `Seq <https://datalust.co/seq>`_ to monitor and analyze the processing of individual requests. This helps in identifying bottlenecks or errors in the request handling pipeline.

2. **Monitoring Dashboards**:
    Metrics collected by Firely Server can be exported to a time series database like `Prometheus <https://prometheus.io/>`_ and visualized in `Grafana <https://grafana.com/>`_ or ingested directly into `Azure Application Insight <https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview>`_ or its equivalent in other cloud providers. These dashboards provide insights into the overall performance and health of the server, such as request rates, latencies, and error rates.

3. **Using OpenTelemetry Collector**:
    It is recommended to use a service like `OpenTelemetry Collector <https://opentelemetry.io/docs/collector/>`_ to filter, process, and dispatch traces and metrics to different endpoints. The collector acts as a central hub for telemetry data, enabling flexible routing and aggregation of data to various backends.

4. **Real-World Testing**:
    The `Real-World Testing <feature_realworldtesting>`_ feature relies on OpenTelemetry traces to analyze server activities.