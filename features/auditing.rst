.. _feature_auditing:

Auditing
========

Firely Server can log access through the RESTful API for auditing purposes. It has 3 features:

#. Write requests and responses to a separate audit logfile.
#. Include user id and name from the JWT token (if present) in the audit log lines.
#. Write the audit information to AuditEvent resources in the Firely Server Data database.

All features can be enabled by including ``Vonk.Plugins.Audit`` in the pipeline. See :ref:`vonk_plugins_config` for details on how to do that.

You can enable specific features by narrowing the namespace that you include in the pipeline, see the available plugins listed under :ref:`vonk_plugins_audit`.

Audit log file configuration
----------------------------

Configure where to put the audit log file and the format of its lines in the appsettings (see :ref:`configure_appsettings`)::

   "Audit": {
      "PathFormat": "./audit/AuditLog-{Date}.log"
      "OutputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Application}] [Audit] [Machine: {MachineName}] [ReqId: {RequestId}] [IP-Address: {Ip}] [Connection: {ConnectionId}] [UserId: {UserId}] [UserName: {UserName}] [Path: {Path}] [Action: {Action}] [Resource: {Resource} Key:{ResourceKey}] [StatusCode: {StatusCode}] {NewLine}"
   },

The OutputTemplate listed here contains all the properties that can be logged:

* RequestId: unique id of this request, use this to correlate request and response
* Ip: IP Address of the client
* ConnectionId: use this to correlate requests from the same client
* UserId: user id from the JWT token (if present)
* UserName: user name from the JWT token (if present)
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