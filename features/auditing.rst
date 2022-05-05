.. _feature_auditing:

Auditing
========

Firely Server can log access through the RESTful API for auditing purposes. It has 3 features:

#. Write requests and responses to a separate audit logfile.
#. Include user id and name from the JWT token (if present) in the audit log lines.
#. Write the audit information to AuditEvent resources in the Firely Server Data database.

All features can be enabled by including ``Vonk.Plugins.Audit`` in the pipeline. See :ref:`vonk_plugins_config` for details on how to do that.

You can enable specific features by narrowing the namespace that you include in the pipeline, see the available plugins listed under :ref:`vonk_plugins_audit`.

General configuration
----------------------------

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

Audit log file configuration
----------------------------

Configure where to put the audit log file and the format of its lines in the appsettings (see :ref:`configure_appsettings`)::

   "Audit": {
      "PathFormat": "./audit/AuditLog-{Date}.log"
      "OutputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Application}] [Audit] [Machine: {MachineName}] [ReqId: {RequestId}] [IP-Address: {Ip}] [Connection: {ConnectionId}] [UserId: {UserId}] [Username: {Username}] [Path: {Path}] [Action: {Action}] [Resource: {Resource} Key:{ResourceKey}] [StatusCode: {StatusCode}] {NewLine}"
   },

The OutputTemplate listed here contains all the properties that can be logged:

* RequestId: unique id of this request, use this to correlate request and response
* Ip: IP Address of the client
* ConnectionId: use this to correlate requests from the same client
* UserId: user id from the JWT token (if present)
* Username: user name from the JWT token (if present)
* Path: request url
* Action: interaction that was requested (like instance_read or type_search)
* Resource: resourcetype involved
* ResourceKey: 'key' of the resource involved (if any), consisting of the resourcetype and the id, formatted as "resourcetype/id"
* StatusCode: statuscode of the response at the time of logging (by default '-1' when the request is not handled yet)

For transactions and batches, the audit plugin will write a line for the transaction/batch as a whole *and* one for every entry in the transaction/batch.

AuditEvent logging
------------------

There is no further configuration for AuditEvent logging. If you include it in the pipeline, it will start generating AuditEvent resources.

For transactions and batches the audit plugin will create an AuditEvent for the transaction/batch as a whole *and* one for every entry in the transaction/batch.

Firely Server does not allow you to update or delete the AuditEvent resources through the RESTful API so the Audit log cannot be tampered with. You can of course still manipulate these resources directly on the database, for instance to offload a surplus of old AuditEvent resources elsewhere. Please :ref:`vonk-contact` us for details if you want to do this.