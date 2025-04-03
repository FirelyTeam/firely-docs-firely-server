.. _configure_log:

Log settings
============

Firely Server uses `Serilog <https://serilog.net/>`__ for logging. The logging settings are controlled in json configuration files called ``logsettings(.*).json``. The files are read in a hierarchy, exactly like the :ref:`appsettings files <configure_levels>` are.
Firely Server comes with default settings in ``logsettings.default.json``. You can adjust the way Firely Server logs its information by overriding these default settings in ``logsettings.json`` or ``logsettings.instance.json``. You need to create this file yourself.

Alternatively you can control :ref:`configure_envvar_log`.

Firely Server by default does nog log any Patient Health Information data, regardless of the level of the log. The only PHI part that can be included in the log is the User Name, when running with Smart authorization (so the user is identified) and including this part in the outputTemplate (see below). 

.. _configure_log_level:

Changing the log event level
----------------------------
Serilog defines several levels of log events. From low to high, these are ``Verbose``, ``Debug``, ``Information``,
``Warning``, ``Error`` and ``Fatal``. You can set the minimum level you want to log, meaning that events for that
level or higher will be logged. By default, Firely Server uses ``Error`` as the minimum level of recording information.

To change the level of logging, follow these steps:

*	Find this setting::

		"MinimumLevel": {
			"Default": "Error",
		},

*	Change the setting for :code:`Default` from ``Error`` to the level you need, from the choice of
	``Verbose``, ``Debug``, ``Information``, ``Warning``, ``Error`` and ``Fatal``.

You can deviate from the default minimum level for specific namespaces. You do this by specifying the namespace
and the log event level you would like for this namespace, for example::

	"MinimumLevel": {
		"Default": "Error",
		"Override": {
			"Vonk": "Warning"
		}
	},

Some additional namespaces you might want to log are:

- ``Vonk.Configuration`` to log configuration information on startup
- ``Vonk.Core.Licensing`` to show license information in your logs
- ``Vonk.Repository.Sql.Raw`` to log SQL repository events for Firely Server v4.3.0 and above
- ``Vonk.Repository.Sql`` to log SQL repository events for Firely Server v4.2.0 and below
- ``Vonk.Repository.Document.Db`` to log MongoDB repository events 
- ``Vonk.Repository.Memory`` to log memory database repository events
- ``Vonk.Core.Repository.EntryIndexerContext``, set it to ``"Error"`` if you have excessive warnings about indexing (mostly when importing `Synthea <https://synthea.mitre.org/downloads>` data)
- ``Microsoft`` to log events from the Microsoft libraries
- ``Microsoft.AspNetCore.Diagnostics`` to report request handling times
- ``Microsoft.AspNetCore.Hosting.Diagnostics`` to log individual requests
- ``System`` to log events from the System libraries

Please note that the namespaces are evaluated in order from top to bottom, so more generic 'catch all' namespaces should be at the bottom of the list. 
So this will log events on ``Vonk.Repository.Sql.Raw`` on ``Information`` level::

	"MinimumLevel": {
		"Default": "Error",
		"Override": {
			"Vonk.Repository.Sql.Raw": "Information",
			"Vonk": "Warning"
		}
	},

But in this (purposefully incorrect) example the ``Warning`` level on the ``Vonk`` namespace will override the ``Information`` level on the ``Vonk.Repository.Sql.Raw`` namespace::

	"MinimumLevel": {
		"Default": "Error",
		"Override": {
			"Vonk": "Warning",
			"Vonk.Repository.Sql.Raw": "Information"
		}
	},

.. _logging_individual_requests:

Logging individual requests 
---------------------------

If you want to log individual requests, you can do so by adjusting the "Override" section to include ``Microsoft.AspNetCore.Hosting.Diagnostics``::

	"MinimumLevel": {
		"Default": "Error",
		"Override": {
			"Microsoft.AspNetCore.Hosting.Diagnostics": "Information"
		}
	},


Here is an example of the logs when you post a Patient resource with these logsettings::

	2023-08-09 10:25:55.028 +02:00 [UserId: ] [Username: ] [Information] [ReqId: 0HMSOM901IS1A:00000002] Request starting HTTP/1.1 POST http://localhost:4080/Patient application/fhir+json 164
 	2023-08-09 10:25:57.225 +02:00 [UserId: ] [Username: ] [Information] [ReqId: 0HMSOM901IS1A:00000002] Request finished HTTP/1.1 POST http://localhost:4080/Patient application/fhir+json 164 - 201 340 application/fhir+json;+fhirVersion=4.0;+charset=utf-8 2199.2748ms

The first log line includes on the request that was made, the second line contains information on the response of Firely Server.
As you can see, the logs include information on the request type, any headers that are included, and the time it took to make this request.

.. _configure_log_sinks:
 
Changing the sink
-----------------
Another setting you can adjust is ``WriteTo``. This tells Serilog which sink(s) to log to.
Serilog provides several sinks, and for Firely Server you can use ``Console``, ``File``, ``ApplicationInsights`` and ``Seq``. All of which can be wrapped in an ``Async`` sink to avoid blocking Firely Server when waiting for the sink to process the log statements.

Console
^^^^^^^

The Console sink will write to your shell.

*	Find the ``WriteTo`` setting::

		"WriteTo": [
			{
				"Name": "Async",
				"Args": {
					"configure": [
						{
						"Name": "Console",
						"Args": {
							"restrictedToMinimumLevel": "Information",
							"outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {UserId} {Username} [{Level}] [ReqId: {RequestId}] {Message:l}{NewLine}{Exception}"
						}
						}
					]
				}
			},
			{
				//Settings for other sinks
			}

The Console is notoriously slow at processing log statements, so it is recommended to limit the number of statements for this sink. Use the ``restrictedToMinimumLevel`` to do so. Also, if you are on Windows, the Powershell command window appears to be faster than the traditional Command Line window.

Settings for the Console sink:

	* ``outputTemplate``: What information will be in each log line. Besides regular text you can use placeholders for information from the log statement:
	
		* ``{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}``: When this was logged, with formatting
		* ``{UserId}``: Technical id of the logged in user - if applicable
		* ``{Username}``: Name of the logged in user - if applicable
		* ``{Application}``: Name of the application (in case other applications are logging to the same sink). Is set to ``Vonk`` at the bottom of the logsettings file
		* ``{Level}``: Level of the log, see the values in :ref:`configure_log_level`
		* ``{MachineName}``: Name of the machine hosting the Firely Server instance. Especially useful when running multiple instances all logging to the same file.
		* ``{RequestId}``: Unique id of the web request, useful to correlate log statements
		* ``{Message:l}``: Actual message being logged, `with format specifier <https://github.com/serilog/serilog/wiki/Formatting-Output#formatting-plain-text>`_ that makes the logs more readable
		    * The :l format specifier switches off quoting of strings
		    * The :j format specifier uses JSON-style rendering for any embedded structured data.  
		* ``{Exception}``: If an error is logged, Firely Server may include the original exception. That is then formatted here.
		* ``{SourceContext}``: The class from which the log statement originated (this is usually not needed by end users).
		* ``{NewLine}``: Well, ehh, continue on the next line,
		* ``{CorrelationId}``: In case you want to follow requests across multiple containers, you can set the ``CorrelationId`` to be included in the logs. See below.

	* ``restrictedToMinimumLevel``: Only log messages from this level up are sent to this sink.


File
^^^^

The ``File`` sink will write to a file, possibly rolling it by interval or size.

*	Find the ``WriteTo`` setting::

		"WriteTo": [
			{
			{ 
				//Settings for Console
			}
			},
			{
			"Name": "Async",
			"Args": {
				"configure": [
					{
					"Name": "File",
					"Args": {
						"path": "%temp%/vonk.log",
						"rollingInterval": "Day",
						"fileSizeLimitBytes": "",
						"retainedFileCountLimit": "7",
						"outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {UserId} {Username} [{Application}] [{Level}] [Machine: {MachineName}] [ReqId: {RequestId}] {Message:l}{NewLine}{Exception}",
						"restrictedToMinimumLevel": "Verbose"
					}
					}
				]
			}
			},
			{ 
				//Settings for Azure ApplicationInsights
			}

*	Under ``File``, change the location of the logfiles by editing the value for ``path``.
	For example::

		{
			"Name": "RollingFile",
			"Args": { 
				"path": "c:/logfiles/vonk.log" 
			}
		},

	Other values that you can set for the File log are:

	* ``rollingInterval``: When this interval expires, the log system will start a new file. The start datetime of each interval is added to the filename. Valid values are ``Infinite``, ``Year``, ``Month``, ``Day``, ``Hour``, ``Minute``. 
	* ``fileSizeLimitBytes``: Limit the size of the log file, which is 1GB by default. When it is full, the log system will start a new file.
	* ``retainedFileCountLimit``: If more than this number of log files is written, the oldest will be deleted. Default value is 31. Explicitly setting it to an empty value means files are never deleted.
	* ``outputTemplate``: as described for `Console`_.
	* ``restrictedToMinimumLevel``: as described for `Console`_.

.. _configure_log_insights:

Application Insights
^^^^^^^^^^^^^^^^^^^^

Firely Server can also log to Azure Application Insights ("Application Insights Telemetry"). What you need to do:

#. Create an Application Insights instance on Azure.
#. Get the ConnectionString from the Properties blade of this instance.
#. Add the correct sink to the logsettings.json::

		"WriteTo": [
			{
				"Name": "ApplicationInsights",
				"Args": {
					"connectionString": "[your connection string here]", 
					"telemetryConverter": "Serilog.Sinks.ApplicationInsights.TelemetryConverters.TraceTelemetryConverter, Serilog.Sinks.ApplicationInsights" 
					"restrictedToMinimumLevel": "Verbose" //Or a higher level
				}
			},
		],

#. This also enables Dependency Tracking for access to your database. This works for both SQL Server and MongoDB. And for the log sent to `Seq`_ if you enabled that.
#. If you set the level for Application Insights to ``Verbose``, and combine that with `Database details`_, you get all the database commands right into Application Insights.

Seq
^^^

`Seq <https://datalust.co/seq>`_ is a web interface to easily inspect structured logs.

For the ``Seq`` sink, you can also specify arguments. One of them is the server URL for your
Seq server::

		"WriteTo": [
			{
				"Name": "Seq",
				"Args": { "serverUrl": "http://localhost:5341" }
			}

* Change ``serverUrl`` to the URL of your Seq server
* ``restrictedToMinimumLevel``: as described for `Console`_.
* Use ``apiKey`` to use an authenticated connection between Firely Server and Seq, see `Serilog Seq documentation <https://github.com/datalust/serilog-sinks-seq?tab=readme-ov-file#json-appsettingsjson-configuration>`_.

Elasticsearch
^^^^^^^^^^^^^

`Elasticsearch <https://www.elastic.co/elasticsearch>`_ is a search engine based on the Lucene library. It provides a distributed, multitenant-capable full-text search engine with an HTTP web interface and schema-free JSON documents.

For the ``Elasticsearch`` sink, you can also specify arguments. The sink will only work for Elasticsearch versions 8.x and up. One of them is the nodes for your
Elasticsearch server::

		"WriteTo": [
			{
  				"Name": "Elasticsearch",
				"Args": {
					"bootstrapMethod": "Silent",
					"nodes": [ "http://localhost:9200" ],
				}
			}

* Change ``bootstrapMethod`` to your needs, this entry is required. It indicates if/how the sink should attempt to install component and index templates to ensure the datastream has ECS mappings. Can be be either None (the default), Silent (attempt but fail silently), Failure (attempt and fail with exceptions if bootstrapping fails).
* Change ``nodes`` to the URL of your Elasticsearch node
* More details can be found in sinks `Github repo <https://github.com/elastic/ecs-dotnet/tree/main/src/Elastic.Serilog.Sinks>`_ and `Elasticsearch docs <https://www.elastic.co/guide/en/ecs-logging/dotnet/current/serilog-data-shipper.html>`_.

MongoDb
^^^^^^^

Firely Server can also log to MongoDb. 

* Add the correct sink to the logsettings.json::

		"WriteTo": [
			{
  				"Name": "MongoDBBson",
  				"Args": {
    				"databaseUrl": "mongodb://username:password@localhost:27017/<db name>",
    				"collectionName": "vonklogs",
    				"cappedMaxSizeMb": "1024",
    				"cappedMaxDocuments": "50000",
    				"rollingInterval": "Month"
  				}
			}

* Change ``databaseUrl`` to match your MongoDb server.
* Change ``collection name`` where you want to store logs
* ``restrictedToMinimumLevel``: as described for `Console`_.

AWS Cloudwatch
^^^^^^^^^^^^^^
Firely Server can also log to AWS Cloudwatch. What you need to do:

#. Create a user with restricted privilages in AWS that can write to Cloudwatch as described `here <https://docs.aws.amazon.com/sdkref/latest/guide/access-iam-users.html>`_
#. Specify the credentials and the region in configuration files or through environment variables as described `here <https://docs.aws.amazon.com/sdkref/latest/guide/creds-config-files.html>`_
#. Add the correct sink to the logsettings.json::

		"WriteTo": [
			{
				"Name": "AmazonCloudWatch",
				"Args": {
					"logGroup": "<the name of your log group>",
					"logStreamPrefix": "<the description to prefix your log stream>", 
					"restrictedToMinimumLevel": "Verbose" //Or a higher level
				}
			},
		],

Splunk
^^^^^^
Firely Server can also log to Splunk. What you need to do:

#. Setup a Splunk environment as described by the Splunk documentation
#. Create a ``HTTP Event Collector`` for the application, save the ``Token Value`` for later use
#. Check in the ``Global Settings`` in the ``HTTP Event Collector`` screen which port is used
#. Add the correct sink to the logsettings.json::

		"WriteTo": [
			{
                "Name": "EventCollector",
                "Args": {
                    "splunkHost": "<splunk endpoint>", // e.g. https://splunk:8088
                    "eventCollectorToken": "<token value>"
                }
            }
		],


.. _configure_log_database:

Database details
----------------
Whether you use MongoDB or SQL Server, you can have Firely Server log in detail what happens towards your database. Just set the appropriate loglevel to 'Verbose'::

	"MinimumLevel": {
		"Default": "Error",
		"Override": {
			"Vonk.Repository": "Verbose"
		}
	},

You can also control the logsettings for the different repositories more finely granulated::

	"MinimumLevel": {
		"Default": "Error",
		"Override": {
			// (for versions before FS 4.6.0)
			"Vonk.Repository.Sql": "Verbose",
			// OR (for FS 4.6.0 or later AND if Sql.Raw is enabled)
			"Vonk.Repository.Sql.Raw": "Verbose",
			// OR (for MongoDb)
			"Vonk.Repository.Document.Db": "Verbose",
			// OR (for SQLite)
			"Vonk.Repository": "Verbose",
			"Microsoft.EntityFrameworkCore": "Verbose"
		}
	},

Remember to adjust your sink settings so that ``"restrictedToMinimumLevel": "Verbose"`` is set. If you do so you probably don't want all this detail in your console sink, so you can limit the level for that, see `Console`_ above.

.. _configure_log_database_query_params:

SQL query parameter logging
^^^^^^^^^^^^^^^^^^^^^^^^^^^

It might be useful to log SQL queries that Firely Server executes against your database. You can get even more insights into what is happening when SQL query parameter values also get logged.
However, this cannot be enabled by default due to data privacy concerns.

You can enable SQL query parameter values logging by setting the ``LogSqlQueryParameterValues`` to ``true`` for the corresponding database in your ``appsettings.instance.json``. Example::

	{
		"Administration": {
			"SqlDbOptions": {
				"ConnectionString": "<connection string>",
				"LogSqlQueryParameterValues": true // Add this line to your config file to log SQL query param values for your SQL Server Administration database
			}
		},
		// OR:
		{
			"SQLiteDbOptions": {
				"ConnectionString": "<connection string>",
				"LogSqlQueryParameterValues": true // Add this line to your config file to log SQL query param values for your Sqlite Data database
			}
		}
	}

.. _setting_correlation_id:

Setting CorrelationId for tracing requests across multiple services
-------------------------------------------------------------------

Firely Server can log a ``RequestId`` to identify individual requests, but this is an auto-generated GUID and cannot be adjusted. This is tricky if you want to log requests across multiple services/containers, how to recognize a particular request from EHR to Firely Server if the ``RequestId`` is set automatically?
As an answer to this, it is possible to set a ``CorrelationId`` for requests in both the normal logging as the :ref:`audit logging <configure_audit_log_file>`. The ``CorrelationId`` can be set manually by adding a header to the request that needs to be traced. Note that you can give any name to this header, as long as it matches the ``headerKey`` in the "Enrich" section of your logsettings.
This section needs to be adjusted to include the ``WithCorrelationIdHeader`` setting::

	"Enrich": [
		"FromLogContext",
		"WithMachineName",
		{
		"Name": "WithCorrelationIdHeader",
		"Args": {
			"headerKey": "custom-correlation-id"
			}
		}
	],

Be sure to add ``[CorrId: {CorrelationId}]`` to your "outputTemplate" settings to view the ``CorrelationId`` in the logs. Below is an example of the resulting loglines when the ``custom-correlation-id`` header is set to "My custom correlation Id"::

	2023-08-09 11:22:15.901 +02:00 [UserId: ] [Username: ] [Information] [ReqId: 0HMSON8FK36UF:00000002] [CorrId: My custom correlation Id] Request starting HTTP/1.1 GET http://localhost:4080/Patient - -
	2023-08-09 11:22:17.884 +02:00 [UserId: ] [Username: ] [Information] [ReqId: 0HMSON8FK36UF:00000002] [CorrId: My custom correlation Id] Request finished HTTP/1.1 GET http://localhost:4080/Patient - - - 200 6642 application/fhir+json;+fhirVersion=4.0;+charset=utf-8 1986.1211ms

Note that if this header is not included in the request, Firely Server will automatically assign a GUID to ``CorrelationId``.
 
.. _enrichResourceInformation:

Enrich logs with resource type and id
-------------------------------------

To enrich the logs with Resource type and id, you can add ``WithResource`` to the ``Enrich`` section of the logsettings.*.json::

	"Enrich": [
		"WithResource"
	],

