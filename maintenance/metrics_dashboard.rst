.. _feature_metrics_dashboard:

Metrics Dashboard
=================

.. note::

  The features described on this page are available in **all** :ref:`Firely Server editions <vonk_overview>`.

Firely Server includes a built-in Metrics Dashboard that provides an out-of-the-box observability experience.
The dashboard visualizes key metrics collected via OpenTelemetry without requiring external tools or additional setup.

The dashboard is intended for:

- Quick validation that Firely Server is operating correctly.
- Immediate insights into request throughput, latencies, and error rates.
- Environments where external monitoring solutions are not yet configured.

.. note::

   The built-in dashboard is not designed to replace production-grade monitoring
   solutions such as Grafana or Application Insights. For long-term monitoring,
   analytics, and alerting, it is recommended to export telemetry data to an
   OpenTelemetry Collector and visualize it in a dedicated monitoring stack.

.. important::

  Full support for the metrics dashboard requires Firely Server and Firely Server Ingest v6.5.0, and Firely Auth v4.5.0.

Available Metrics
-----------------

The dashboard displays a curated set of server-level and instance-level metrics relevant to FHIR workloads:

.. list-table::
   :header-rows: 1
   :widths: 25 30

   * - Category
     - Description
   * - Availability
     - Uptime and reachability of Firely Server and Firely Auth endpoints
   * - System
     - Memory footprint of each Firely Server instance
   * - System
     - CPU utilization of each Firely Server instance
   * - Workload (configurable per instance / tenant / client_id)
     - Number of FHIR interactions in the last 5 minutes
   * - Workload (configurable per instance / tenant / client_id)
     - Number of failed FHIR interactions in the last 5 minutes
   * - Workload (configurable per instance / tenant / client_id)
     - Response time of FHIR interactions in the last 5 minutes
   * - Resources
     - Number of resources stored per resource type
   * - Resources
     - Current ingestion rate via REST API, FSI, or PubSub

Accessing the Dashboard
-----------------------

To access the Metrics Dashboard:

#. Enable metrics in the configuration. See :ref:`feature_opentelemetry` for details. The metrics dashboard ingests OTLP metrics from Firely Server, Firely Auth, and Firely Server Ingest. Each application must be configured separately.
#. Configure the OpenTelemetry endpoint to point to the Firely Server Metrics Dashboard backend on port ``7174`` and start the backend service. Alternatively, configure the OpenTelemetry Collector to forward metrics to both the dashboard and Prometheus endpoints.
#. Start Firely Server.
#. Open the dashboard endpoint at ``https://example.org:7174/``.

.. note::

   Metrics become visible in the dashboard after approximately one minute.
   Metrics are cached for five minutes and discarded afterwards.

Example Use Cases
-----------------

- Health check: Verify responsiveness and request handling.
- Development and testing: Monitor latency and error spikes without additional tooling.
- Resource ingestion insights: Monitor Firely Server performance and ensure data ingestion without failures.