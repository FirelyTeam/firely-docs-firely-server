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
							"outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {UserId} {Username} [{Level}] [ReqId: {RequestId}] {Message}{NewLine}{Exception}"
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
		* ``{Message}}``: Actual message being logged
		* ``{Exception}``: If an error is logged, Firely Server may include the original exception. That is then formatted here.
		* ``{SourceContext}``: The class from which the log statement originated (this is usually not needed by end users).
		* ``{NewLine``}: Well, ehh, continue on the next line

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
						"outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {UserId} {Username} [{Application}] [{Level}] [Machine: {MachineName}] [ReqId: {RequestId}] {Message}{NewLine}{Exception}",
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
#. Get the InstrumentationKey from the Properties blade of this instance.
#. Add the correct sink to the logsettings.json::

		"WriteTo": [
			{
				"Name": "ApplicationInsightsTraces",
				"Args": {
					"instrumentationKey": "<the key you copied in step 2>", 
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

You can also control the logsettings for the different repositories more finely granulated:

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
