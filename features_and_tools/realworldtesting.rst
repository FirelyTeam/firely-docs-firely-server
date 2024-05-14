.. _feature_realworldtesting:

==================
Real World Testing
==================

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
   The Real World Testing operation requires the use and maintenance of an externally provided InfluxDB. Firely Server does not provide these capabilities out-of-the-box. See `InfluxDB OSS <https://www.influxdata.com/products/influxdb/>`_ for more details.

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
          "Vonk.Subscriptions.Administration",
          "Vonk.Plugins.Terminology",
          "Vonk.Administration",
          "Vonk.Plugin.BinaryWrapper"
        ],
        "Exclude": [
          "Vonk.Core.Operations"
        ]
        ... etc

.. note::
   RealWorldTesting works as an asynchronous operation. To store the all operation-related information, it is necessary to enable a "Task Repository" on the admin database. Please enable the relevant "Vonk.Repository.[database-type].[database-type]TaskConfiguration" in the administration pipeline options, depending on the database type you use for the admin database. All supported databases can be used as a task repository. In the example above we have enabled the task repository for SQLite: "Vonk.Repository.Sqlite.SqliteTaskConfiguration".

Please make sure that `$realworldtesting` and `realworldtestingstatus` are listed as supported WholeSystemInteractions.

To configure RWT one needs to also have values for connecting to InfluxDB configured.

.. code-block:: json

    "InfluxDbOptions": {
        "Host": "https://influxdb-host-url",
        "Bucket": "bucket-name",
        "Token": "bucket-connection-token",
        "Organization": "organization-name"
    }

InfluxDb has a concept of buckets and organizations, so one would need to use the same bucket for writing and reading data to the backend. 
However it is advised to use tokens with different access rights, since querying data while executing RWT operation only requires read access enabled.

In addition, there is the following configuration section for the Real World Testing operation itself:

.. code-block:: json
    
    "RealWorldTesting": {
        "RepeatPeriod": 60000
    }

In `RepeatPeriod` you can configure the polling interval (in milliseconds) for checking the Task queue for a new operation task.

Next to the configuration for reading statistics from InfluxDB, it is required to setup an `OpenTelemetry collector <https://opentelemetry.io/docs/collector/>`_ which is connected to a Telegraf instance for processing OpenTelemetry traces.

.. code-block:: json

   "OpenTelemetryOptions": {
       "EnableTracing": true,
       "Endpoint": "http://localhost:4317"
   }

In Firely Server, the OpenTelemetry endpoint should point to the GRPC endpoint of the OpenTelemetry collector.

As part of the OpenTelemetry configuration, please make sure to exclude the liveness and readiness check from the statistics.

.. code-block:: yaml

  processors:
     batch: {}
     filter/health: #https://github.com/open-telemetry/opentelemetry-collector-contrib/tree/main/processor/filterprocessor
       error_mode: ignore
       traces:
         span:
           - 'attributes["url.path"] == "/$$liveness"'
           - 'attributes["url.path"] == "/$$readiness"'
     filter/requestmeter:
       error_mode: ignore
       traces:
         span:
           - 'attributes["scope"] != "request"'

The OpenTelemetry collector will forward the metrics to Telegraf for post-processing. Firely Server requires certain processing steps to be present in the Telegraf config.

.. code-block:: RST

   [[processors.starlark]]
     script = "/etc/telegraf/scripts/starlark.star"

The `starlark` file needs to contain the following content:

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

Please ensure that Telegraf is afterwards forwarding all metrics to InfluxDb to the same bucket as configured under the InfluxDbOptions. When executing any REST API request against Firely Server, corresponding traces should be visible in InfluxDB afterwards.

.. note::
   Real World Testing is a powerful feature that requires careful configuration and setup. It is recommended to test your queries and configurations in a staging environment before deploying to production.

Using Real World Testing
------------------------

To initiate a Real World Testing operation, construct a request to the administration endpoint with the necessary parameters, such as the URL of the Library resource containing the query, and any additional parameters specified within the Library resource. For example:

.. code-block:: HTTP

   GET {{BASE_URL}}/administration/$realworldtesting?url=https://fire.ly/fhir/Library/rwt-all-requests&from=2024-03-18T14:34:16.772Z&to=2024-03-18T14:34:52.453Z

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

Defautl RWT metrics
-------------------

By default the admin db of Firely Server contains the following Library resource with Flux queries:

* https://fire.ly/fhir/Library/rwt-all-requests-custom-operation
* https://fire.ly/fhir/Library/rwt-all-requests

Library Resource Requirements
-----------------------------

.. note::
   The Library resource's Flux query must be base64 encoded and should be designed to return a single numeric value. Ensure that your query properly aggregates or processes the data to meet this requirement.
   Keep in mind that the resource needs to be in administration database.

Resource should be a valid FHIR Library resource according to specification.
Its `content.data` element is expected to contain base64 encoded Flux query to be executed against InfluxDB.
In addition to the content - `parameter` element may be filled with one or more ParameterDefinition values. The following ParameterDefinition types are allowed: string, integer, decimal, date, dateTime.
Those would define query parameters that are expected to be defined in the Flux query, as well as required for $realworldtesting operation request.

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

Along with the `general guidelines on Flux <https://docs.influxdata.com/flux/v0/get-started>`_ there is a syntax rule for injecting $realworldtesting operation parameters into the queries.
The following syntax is treated as a placeholder for a parameter value.

Curly braces are treated as a placeholder for a value to be replaced with a query parameter from $realworldtesting request.

Here is an example of a complete flux query containing placeholder parameters (`{bucket}`,`{to}` and `{from}`):

.. code-block:: Flux

    from(bucket: "{bucket}")
    |> range(start: {from}, stop: {to})
    |> filter(fn: (r) => r["_measurement"] == "requests")
    |> count()
    |> group()
    |> sum()

The `{bucket}` placeholder is special, since it is used to inject bucket value from configuration. So it is advised to use it with that in mind.
All the placeholder parameters are replaced if:
1. Library resource defines parameters with the same names as a placeholder name(text in between opening and closing curly braces).
2. $realworldtesting request supplies those parameters.

.. note::
   There are some restrictions for the parameter values that can be injected. 
   Currently `'`, `"`, `|`,  `>`,  `(`,  `)`, are not allowed symbols, and the $realworldtesting operation request will return 400(BadRequest) if any of those symbols are present. 

