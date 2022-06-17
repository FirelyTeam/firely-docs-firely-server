.. _feature_auditing:

Auditing
========

Firely Server can log access through the RESTful API for auditing purposes. It has 3 features:

#. Write requests and responses to a separate audit logfile.
#. Include user id and name from the JWT token (if present) in the audit log lines.
#. Write the audit information to AuditEvent resources in the Firely Server Data database.

These features can be enabled by including ``Vonk.Plugins.Audit`` in the pipeline.

.. code-block:: JavaScript

   "PipelineOptions": {
      "PluginDirectory": "./plugins",
      "Branches": [
         {
            "Path": "/",
            "Include": [
               "Vonk.Core",
               ...
               "Vonk.Plugin.Audit"
            ],
            ...
         },
         ...
      ]
   }

See :ref:`vonk_plugins_config` for more details on pipeline configuration.

At present, you can choose either to enable both file and database logging, or only database logging.
To enable only database logging, replace Vonk.Plugin.Audit with Vonk.Plugin.Audit.AuditEventConfiguration.
In addition, you can choose to log every call or only transaction batches.
When you include a specific configuration class and want to enable username logging, you have to include Vonk.Plugin.Audit.UsernameLoggingConfiguration.
Please see :ref:`vonk_plugins_audit` for the available options.

Filtering configuration
-----------------------

You can exclude requests from generating audit logs (both audit log file and audit event logging). 
This is helpful to reduce clutter in the logs. For example, you could exclude logging for an endpoint that is used for health monitoring of the server.
The example below disables audit logging for all GET requests to /Patient and sub resources or operations.

.. code-block:: JavaScript

   "Audit": {
      "ExcludedRequests": [
         {
            "UrlPath": "/Patient",
            "Method": "GET"
         },
         {
            "UrlPath": "/Patient/*",
            "Method": "GET"
         }
      ]
   },

The UrlPath property is required, but not otherwise checked (e.g. if it points to an existing resource).
The wildcard (\*) can be used to expand matching in different ways, e.g.:

* /Medication* will match /Medication, /MedicationRequest, /MedicationAdministration, etc
* /$\* will match all system level operations
* /\*/\*/$validate will match all validation operations on all resources

The Method property is optional. If left out, null, empty or given the value \*, it will match all HTTP verbs. 
You can enter multiple verbs, delimited by the \| symbol (e.g. GET\|POST).

.. _configure_audit_log_file:

Audit log file configuration
----------------------------

File
^^^^

Configure where to put the audit log file and the format of its lines in a separate file named audit.logsettings.json. Just like the Firely Server application logging, the audit log also uses Serilog for logging audit events. The audit log settings are controlled in json configuration files called ``audit.logsettings(.*).json``. The files are read in a hierarchy, exactly like the :ref:`appsettings files <configure_levels>` are.
Firely Server comes with default settings in ``audit.logsettings.default.json``. You can adjust the way Firely Server logs its information by overriding these settings by either adding an additional file called ``audit.logsettings.json`` or ``audit.logsettings.instance.json``, or in ``audit.logsettings.default.json`` directly. Alternatively you can control :ref:`configure_envvar_audit_log`.

.. code-block:: JavaScript

   {
      "AuditLog": {
         "WriteTo": [
            {
               "Name": "Async",
               "Args": {
                  "configure": [
                     {
                        "Name": "File",
                        "Args": {
                           "path": "./audit/AuditLog.log",
                           "rollingInterval": "Day",
                           "fileSizeLimitBytes": "",
                           "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Application}] [Audit] {RequestResponse} [Machine: {MachineName}] [ReqId: {RequestId}] [IP-Address: {Ip}] [Connection: {ConnectionId}] [UserId: {UserId}] [Username: {Username}] [Path: {Path}] [Parameters: {Parameters}] [Action: {Action}] [Resource: {Resource} Key:{ResourceKey}] [StatusCode: {StatusCode}] {NewLine}"
                        }
                     }
                  ]
               }
            }
         ]
      }
   },


The values that you can set for the File sink Args are:

* ``path``: The location where the audit log file should be stored.
* ``rollingInterval``: When this interval expires, the log system will start a new file. The start datetime of each interval is added to the filename. Valid values are ``Infinite``, ``Year``, ``Month``, ``Day``, ``Hour``, ``Minute``. 
* ``fileSizeLimitBytes``: Limit the size of the log file, which is 1GB by default. When it is full, the log system will start a new file.

The OutputTemplate listed here contains all the properties that can be logged:

* ``Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz``: When this was logged, with formatting.
* ``Application``: Firely Server
* ``RequestResponse``: indicates wether the audit event was a request or a response.
* ``MachineName``: Name of the machine hosting the Firely Server instance. Especially useful when running multiple instances all logging to the same file.
* ``RequestId``: Unique id of this request, use this to correlate request and response.
* ``Ip``: IP Address of the client.
* ``ConnectionId``: Use this to correlate requests from the same client.
* ``UserId``: User id from the JWT token (if present).
* ``Username``: User name from the JWT token (if present).
* ``Path``: Request url.
* ``Parameters``: The request parameters used.
* ``Action``: Interaction that was requested (like instance_read or type_search).
* ``Resource``: Resourcetype involved.
* ``ResourceKey``: 'Key' of the resource involved (if any), consisting of the resourcetype and the id, formatted as "resourcetype/id".
* ``StatusCode``: Statuscode of the response at the time of logging (by default '-1' when the request is not handled yet).

For transactions and batches, the audit plugin will write a line for the transaction/batch as a whole *and* one for every entry in the transaction/batch.

Seq
^^^

Because we use Serilog for logging audit events, other Log sinks like `Seq` are also supported. `Seq <https://datalust.co/seq>`_ is a web interface to easily inspect structured logs.

For the ``Seq`` sink, you can also specify arguments. One of them is the server URL for your
Seq server::

		"WriteTo": [
			{
				"Name": "Seq",
				"Args": { "serverUrl": "http://localhost:5341" }
			}

* Change ``serverUrl`` to the URL of your Seq server

AuditEvent logging
------------------

There is no further configuration for AuditEvent logging. If you include it in the pipeline, it will start generating AuditEvent resources.

For transactions and batches the audit plugin will create an AuditEvent for the transaction/batch as a whole *and* one for every entry in the transaction/batch.

Firely Server does not allow you to update or delete the AuditEvent resources through the RESTful API so the Audit log cannot be tampered with. You can of course still manipulate these resources directly on the database, for instance to offload a surplus of old AuditEvent resources elsewhere. Please :ref:`vonk-contact` us for details if you want to do this.