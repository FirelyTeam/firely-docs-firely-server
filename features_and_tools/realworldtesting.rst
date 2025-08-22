.. _feature_realworldtesting:

Real World Testing
==================

.. note::

  The features described on this page are available in the following :ref:`Firely Server editions <vonk_overview>`:

  * Firely Scale - 🌍 / 🇺🇸
  * Firely Prior Authorization - 🇺🇸

The Real World Testing functionality of Firely Server is designed to fulfill the requirements of the ONC Health IT Certification Program defined by the 21st Century Cures Act. See `ONC Health IT Certification Program - Real World Testing Resource Guide <https://www.healthit.gov/sites/default/files/page/2021-08/ONC-Real%20World%20Testing%20Resource%20Guide_Aug%202021.pdf>`_ for background.

Real World Testing (RWT) is a process for recording and analyzing statistical data about the REST API behaviour of Firely Server. It allows for retrospectively gathering insights into the response codes of FHIR CRUD requests, as well as custom operations. The functionality enables the collection of all statistics needed for the `Firely Server Real World Testing Plans <https://fire.ly/g10-certification/>`_.

Technically, Firely Server allows to execute pre-defined queries against a dataset containing statistical data, stored in an external InfluxDB. The queries, defined as Flux queries, are distributed using a Library resource as part of the Firely Server admin db.

Introduction
------------

Firely Server provides an API for executing the mentioned Flux queries remotely against the metrics collected in the InfluxDB backend. The response contains aggregations using a denominator/numerator style as outlined by the Firely real world testing plan. This API allows users with access to administration endpoint to query PII-free data to get insights about Firely Server usage.

The operation is based on the ``Library`` resource, which must contain a base64 encoded Flux query. This query can include placeholders for parameters that are dynamically replaced when the operation is executed. 
The RWT operation follows the FHIR Asynchronous Interaction Request Pattern, similar to Bulk Data Export, providing a robust mechanism for handling intensive data processing tasks.
Read more about the `async request flow <https://build.fhir.org/async-bundle.html>`_.

.. note::
   The Real World Testing operation requires the use and maintenance of an externally provided components: InfluxDB, Telegraf and OpenTelemetry collector. 
   Firely Server does not provide these capabilities out-of-the-box. See `InfluxDB OSS <https://www.influxdata.com/products/influxdb/>`_ , `Telegraf OSS <https://www.influxdata.com/time-series-platform/telegraf/>`_  and `OpenTelemetry Collector OSS <https://opentelemetry.io/docs/collector/>`_ for more details.

Configuration
-------------

To start using Real World Testing you will first have to add the relevant plugins (`Vonk.Plugin.RealWorldTesting`) to the PipelineOptions in the appsettings.

.. code-block:: JavaScript

 "PipelineOptions": {
    "PluginDirectory": "./plugins",
    "Branches": [
      {
        "Path": "/",
        "Include": [
          "Vonk.Core",
          "Vonk.Fhir.R4",
          //"Vonk.Fhir.R5",
          "Vonk.Repository.Sqlite.SqliteVonkConfiguration",
          ...
        ],
        "Exclude": [
          "Vonk.Subscriptions.Administration"
        ]
      }, 
      {
        "Path": "/administration",
        "Include": [
          "Vonk.Core",
          "Vonk.Fhir.R4",
          "Vonk.Plugin.RealWorldTesting"
          "Vonk.Repository.Sqlite.SqliteTaskConfiguration",
          "Vonk.Repository.Sqlite.SqliteAdministrationConfiguration",
          "Vonk.Plugins.Terminology",
          "Vonk.Administration",
          ...
        ],
        "Exclude": [
          "Vonk.Core.Operations"
        ]
        ... etc

.. note::
   RealWorldTesting works as an asynchronous operation. To store all operation-related information, it is necessary to enable a "Task Repository" on the admin database. 
   Please enable the relevant "Vonk.Repository.[database-type].[database-type]TaskConfiguration" in the administration pipeline options, depending on the database type you use for the admin database. 
   All supported databases can be used as a task repository. In the example above we have enabled the task repository for SQLite: "Vonk.Repository.Sqlite.SqliteTaskConfiguration".

Please make sure that `$realworldtesting` and `realworldtestingstatus` are enabled in the administration operations in the settings:

.. code-block:: JavaScript

    {
      "Administration": {
        ...
        "Operations": {
            "$realworldtesting": {
                "Enabled": true
            },
            "$realworldtestingstatus": {
                "Enabled": true
            },
        }
        ...
      }
      ...
    }

.. important::

   When the RWT operations are disabled (``"Enabled": false``) but the ``Vonk.Plugin.RealWorldTesting`` plugin remains loaded:
   
   * The HTTP endpoints (``$realworldtesting``, ``$realworldtestingstatus``) are NOT registered
   * New RWT tasks cannot be created via the API  
   * **However**, the background task processor continues to run and will:
   
     * Poll for existing tasks in the database
     * Process any queued or active tasks
     * Handle cancellation requests
   
   To completely stop RWT task processing, you must remove the ``Vonk.Plugin.RealWorldTesting`` plugin from the pipeline configuration.


To configure RWT one needs to also have values for connecting to InfluxDB configured.

.. code-block:: json

    "RealWorldTesting": {
        "InfluxDbOptions": {
            "Host": "https://influxdb-host-url",
            "Bucket": "bucket-name",
            "Token": "bucket-connection-token",
            "Organization": "organization-name"
        }
    }

InfluxDb has a concept of buckets and organizations, so one would need to use the same bucket for writing and reading data to the backend. 
However it is advised to use tokens with different access rights, since querying data while executing RWT operation only requires read access enabled.

In addition, there is the following configuration section for the Real World Testing operation itself:

.. code-block:: json
    
    "RealWorldTesting": {
        "RepeatPeriod": 60000,
        "InfluxDbOptions": {
            // ... see above
        }
    }

In `RepeatPeriod` you can configure the polling interval (in milliseconds) for checking the Task queue for a new operation task.

Next to the configuration for reading statistics from InfluxDB, as the RWT operations rely on the Opentelemewtry traces generated by Firely Server, one needs to enable the OpenTelemetry tracing in the appsettings and configure the endpoint to which the traces are sent :

.. code-block:: json

   "OpenTelemetryOptions": {
       "EnableTracing": true,
       "Endpoint": "http://otlp-collector-url:4317"
   }

The specified endpoint should point to the GRPC endpoint of the `OpenTelemetry collector <https://opentelemetry.io/docs/collector/>`_ which is connected to a Telegraf instance for processing OpenTelemetry traces.

.. seealso::

   For more details on configuring OpenTelemetry, refer to :ref:`feature_opentelemetry`.

As part of the OpenTelemetry collector configuration, one has to (at least) specify: 

* an importer exposing OTLP using the GRPC protocol, 
* a processor filtering out the liveness and readiness check from the statistics
* a processor selecting the requests, 
* an exporter targeting a telegraf service
* a service connecting the above components.

Below is an example of configuration, where the OpenTelemetry collector is configured to receive traces from Firely Server and forward them to Telegraf:

.. code-block:: yaml

  receivers:
    otlp:
      protocols:
        grpc:
    ...

  exporters:    
    otlp/telegraf:
      endpoint: http://telegraf.influxdb.svc.cluster.local:4311
      tls:
        insecure: true
    
  processors:
    batch: {}
    filter/health: #https://github.com/open-telemetry/opentelemetry-collector-contrib/tree/main/processor/filterprocessor
      error_mode: ignore
      traces:
        span:
          - 'attributes["url.path"] == "/$$liveness"'
          - 'attributes["url.path"] == "/$$readiness"'
        filter/health: #https://github.com/open-telemetry/opentelemetry-collector-contrib/tree/main/processor/filterprocessor
      error_mode: ignore
      traces:
        span:
          - 'attributes["url.path"] == "/$$liveness"'
          - 'attributes["url.path"] == "/$$readiness"'
    filter/requestmeter:
      spans:
        include:
          match_type: strict
          attributes:
            - key: "scope"
              value: "request"
 
  service:
    pipelines:
      traces/requestmeter:
        receivers: [otlp]
        exporters: [otlp/telegraf]
        processors: [filter/health, filter/requestmeter, batch]
      ...


Firely Server also requires a specific Telegraf config.
In particular, 

*  an input corresponding to the output of the OpenTelemetry collector
*  a processor for executing a Starlark script converting traces into metric points
*  an output for sending the metrics to InfluxDB

Below is an example of configuration:

.. code-block:: RST

  [[inputs.prometheusremotewrite]]
    listen_address = ":9201"
    metrics_schema = "prometheus-v2"
  [[processors.starlark]]
    script = "/etc/telegraf/scripts/starlark.star"
  [[outputs.influxdb_v2]]
    urls = ["http://influxdb.influxdb.svc.cluster.local:8086"]
    token = "<influxdb-write-token>"


The script for the Starlark processor should be placed in the specified location and should look like this:

.. code-block:: RST

    load("json.star", "json")

    def apply(metric):
        if "attributes" in metric.fields:
            attrs_json = metric.fields["attributes"]
            attrs = json.decode(attrs_json)

            # if it is a request move measurment to requests collection
            if "scope" in attrs and attrs["scope"] == "request":
                metric.name = "requests"
                attrs.pop("scope") # remove scope from attributes
            else:
                return metric #if it is not a request, return the metric as is
                
            # copy attributes to tags and drop
            for k, v in attrs.items():
                metric.tags[k] = str(v)
            metric.fields.pop("attributes")

            # Collect only duration field and drop the rest
            fields_to_remove = [field for field in metric.fields if field != "duration_nano"]
        
            # Drop unwanted fields
            for field in fields_to_remove:
            metric.fields.pop(field)
        else: 
            return None #if there are no attributes, drop this trace
        
        return metric

Please ensure that Telegraf is afterwards forwarding all metrics to InfluxDb to the same bucket as configured under the InfluxDbOptions. 
When executing any REST API request against Firely Server, corresponding traces should be visible in InfluxDB afterwards.

.. note::
   Real World Testing is a powerful feature that requires careful configuration and setup. It is recommended to test your queries and configurations in a staging environment before deploying to production.

.. note::
    In order to demonstrate the required setup for the RWT feature on a Kubernetes cluster, we have added the required dependencies
    in the `Firely Server helm chart <https://github.com/FirelyTeam/Helm.Charts/blob/main/charts/firely-server/Chart.yaml>`_ and 
    the `values.yaml <https://github.com/FirelyTeam/Helm.Charts/blob/main/charts/firely-server/values.yaml>`_ contains basic settings
    for the influxdb2, telegraf and opentelemetry collector charts. 
    However, we highly recommend deploying and configuring independently InfluxDB, Telegraf and OpenTelemetry collector.  

Using Real World Testing
------------------------

To initiate a Real World Testing operation, construct a request to the administration endpoint with the necessary parameters, such as the URL of the Library resource containing the query, and any additional parameters specified within the Library resource. For example:

.. code-block:: HTTP

   GET {{BASE_URL}}/administration/$realworldtesting?url=https://fire.ly/fhir/Library/rwt-all-requests&from=2024-03-18T14:34:16.772Z&to=2024-03-18T14:34:52.453Z

Alternatively a POST request might be executed, here query parameters are passed as a Parameters resource in request body:

.. code-block:: HTTP
    
   POST {{BASE_URL}}/administration/$realworldtesting

.. code-block:: json

    {
        "resourceType": "Parameters",
        "parameter": [
            {
                "name": "url",
                "valueUri": "https://fire.ly/fhir/Library/rwt-all-requests"
            },
            {
                "name": "from",
                "valueDateTime": "2024-03-18T14:34:16.772Z"
            },
            {
                "name": "to",
                "valueDateTime": "2024-03-18T14:34:52.453Z"
            }
        ]
    }


This request triggers the execution of the specified Flux query against the InfluxDB dataset, with the provided parameters dynamically injected into the query.

Operation Response
------------------

Upon successful initiation, the operation returns a 202 status code with a ``Content-Location`` header pointing to a status endpoint where the operation's progress and results can be monitored:

.. code-block:: HTTP

   {{BASE_URL}}/administration/$realworldtestingstatus?_id=7e700b18-d8b0-40da-8deb-f6d1d6a51b23

There are six possible status options:

1. Queued
2. Active
3. Complete
4. Failed
5. CancellationRequested
6. Cancelled

* If a task is Queued or Active, GET $realworldtestingstatus will return the status in the X-Progress header
* If a task is Complete, GET $realworldtestingstatus will return the results with a result bundle (see example below).
* If a task is Failed, GET $realworldtestingstatus will return HTTP Statuscode 500 with an OperationOutcome.
* If a task is on status CancellationRequested or Cancelled, GET $realworldtestingstatus will return HTTP Statuscode 410 (Gone).

.. code-block:: json

    {
        "resourceType": "Bundle",
        "type": "batch-response",
        "entry": [
            {
                "response": {
                    "status": "200 OK",
                    "location": "{{BASE_URL}}/administration/$realworldtesting?url=https://fire.ly/fhir/Library/rwt-all-requests&from=2024-03-18T14:34:16.772Z&to=2024-03-18T14:34:52.453Z"
                },
                "resource": {
                    "resourceType": "Parameters",
                    "parameter": [
                        {
                            "name": "value",
                            "valueInteger": 42
                        }
                    ]
                }
            }
        ]
    }

Default RWT metrics
-------------------

By default the admin db of Firely Server contains the following Library resource with Flux queries:

* https://fire.ly/fhir/Library/rwt-all-requests-custom-operation

This metrics reports the total number of requests per custom operation

* https://fire.ly/fhir/Library/rwt-all-requests

This metrics reports the total number of requests over all REST API interactions

Library Resource Requirements
-----------------------------

For evaluating statistics it is possible to create custom Flux queries stored within Library resources. The following requirements need to be meet:

*  The Library resource should be a valid FHIR Library resource according to specification
* The `content.data` element is expected to contain base64 encoded Flux query to be executed against InfluxDB.
* The `parameter` element may be filled with one or more ParameterDefinition values. The following ParameterDefinition types are allowed: string, integer, decimal, date, dateTime. These parameters define query parameters that are expected to be defined in the Flux query, as well as required for $realworldtesting operation request.

.. note::
   The Library resource's Flux query must be designed to return a single numeric value. Ensure that your query properly aggregates or processes the data to meet this requirement.
   Keep in mind that the Library needs to added to the administration database.

An example Library can be found below:

.. code-block:: json

    {
        "id": "rwt-all-requests",
        "resourceType": "Library",
        "type": {
            "coding": [
                {
                    "system": "http://terminology.hl7.org/CodeSystem/library-type",
                    "code": "logic-library",
                    "display": "Logic Library"
                }
            ]
        },
        "url": "https://fire.ly/fhir/Library/rwt-all-requests",
        "version": "1.0.0",
        "name": "rwt-get-all-requests",
        "title": "RWT All requests",
        "subtitle": "RWT query to collect all requests for a specific period of time",
        "status": "active",
        "experimental": true,
        "date": "2024-03-05T00:00:00+00:00",
        "publisher": "Firely",
        "description": "RWT query to collect all requests for a specific period of time from InfluxDb",
        "copyright": "Firely",
        "parameter": [
            {
                "name": "from",
                "use": "in",
                "min": 1,
                "max": "1",
                "type": "dateTime",
                "documentation": "Start date of the period to be queried"
            },
            {
                "name": "to",
                "use": "in",
                "min": 1,
                "max": "1",
                "type": "dateTime",
                "documentation": "End date of the period to be queried"
            },
            {
                "name": "bucket",
                "use": "in",
                "min": 1,
                "max": "1",
                "type": "string",
                "documentation": "InfluxDb bucket to be queried"
            }
        ],
        "content": [
            {
                "contentType": "text/plain",
                "title": "Get all requests query",
                "data": "ZnJvbShidWNrZXQ6ICJ7YnVja2V0fSIpCiAgfD4gcmFuZ2Uoc3RhcnQ6IHtmcm9tfSwgc3RvcDoge3RvfSkKICB8PiBmaWx0ZXIoZm46IChyKSA9PiByWyJfbWVhc3VyZW1lbnQiXSA9PSAicmVxdWVzdHMiKQogIHw+IGNvdW50KCkKICB8PiBncm91cCgpCiAgfD4gc3VtKCk="
            }
        ]
    }

Inserting Request Data Into Flux Query
--------------------------------------

Along with the `general guidelines on Flux <https://docs.influxdata.com/flux/v0/get-started>`_, there is a syntax rule for injecting $realworldtesting operation parameters into the queries.
The following syntax is treated as a placeholder for a parameter values.

Curly braces are treated as a placeholder for a value to be replaced with a query parameter from $realworldtesting request.

Here is an example of a complete flux query containing placeholder parameters (`{bucket}`, `{to}` and `{from}`):

.. code-block:: python

    from(bucket: "{bucket}")
    |> range(start: {from}, stop: {to})
    |> filter(fn: (r) => r["_measurement"] == "requests")
    |> count()
    |> group()
    |> sum()

The `{bucket}` placeholder is special, since it is used to inject the bucket value from the appsettings. So it is advised to use it with that in mind.
All the placeholder parameters are replaced if:

#. The Library resource defines parameters with the same names as a placeholder name (text in between opening and closing curly braces)
#. $realworldtesting request supplies those parameters

.. note::
   There are some restrictions for the parameter values that can be injected. 
   Currently `'`, `"`, `|`,  `>`,  `(`,  `)`, are not allowed symbols, and the $realworldtesting operation request will return HTTP 400 (BadRequest) if any of those symbols are present. 

