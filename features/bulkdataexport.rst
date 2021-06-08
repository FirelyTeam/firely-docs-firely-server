.. _feature_bulkdataexport:

Bulk Data Export
================

Firely Server provides the option to export resources with the Bulk Data Export Service. 
The Bulk Data Export Service enables the $export operation from the Fhir specification. Read more about the `$export request flow <https://hl7.org/fhir/uv/bulkdata/export/index.html#request-flow>`_

Appsettings
-----------
To start using the Bulk Data Export Service (BDE) you will first have to add the plugin to the PipelineOptions in the appsettings.

.. code-block:: JavaScript

 "PipelineOptions": {
    "PluginDirectory": "./plugins",
    "Branches": [
      {
        "Path": "/",
        "Include": [
          "Vonk.Core",
          "Vonk.Fhir.R3",
          "Vonk.Fhir.R4",
          //"Vonk.Fhir.R5",
          "Vonk.Repository.Sql.SqlVonkConfiguration",
          "Vonk.Repository.Sqlite.SqliteVonkConfiguration",
          "Vonk.Repository.MongoDb.MongoDbVonkConfiguration",
          "Vonk.Repository.CosmosDb.CosmosDbVonkConfiguration",
          "Vonk.Repository.Memory.MemoryVonkConfiguration",
          "Vonk.Subscriptions",
          "Vonk.Smart",
          "Vonk.UI.Demo",
          "Vonk.Plugin.DocumentOperation.DocumentOperationConfiguration",
          "Vonk.Plugin.ConvertOperation.ConvertOperationConfiguration",
          "Vonk.Plugin.BinaryWrapper",
          "Vonk.Plugin.Audit",
          "Vonk.Plugins.TerminologyIntegration",
          "Vonk.Plugin.BulkDataExport"
        ],
        "Exclude": [
          "Vonk.Subscriptions.Administration"
        ]
      }, ...etc...

.. note::
    We did not implement BDE for all database types. Make sure the admin database is configured for either SQL Server or SQLite and the data database is configured for SQL Server.
    
BDE introduces two new parts to the appsettings, namely TaskFileManagement and BulkDataExport.

.. code-block:: JavaScript

  "TaskFileManagement": {
      "StoragePath": "./taskfiles"
    },
  "BulkDataExport": {
      "RepeatPeriod" : 60000, //ms
      "AdditionalResources": [ "Organization", "Location", "Substance", "Device", "BodyStructure", "Medication", "Coverage" ] 
    },
    
In StoragePath you can configure the folder where the exported files will be saved to. Make sure the server has write access to this folder.

In RepeatPeriod you can configure the polling interval (in milliseconds) for checking the Task queue for a new export task.

A patient-based or group-based Bulk Data Export returns resources based on the Patient compartment definition (https://www.hl7.org/fhir/compartmentdefinition-patient.html). These resources may reference resources outside the compartment as well, such as a Practitioner who is the performer of a Procedure. Using the `AdditionalResources`-setting, you can determine which types of referenced resources are exported in addition to the compartment resources.

$export
-------

There are three different levels for which the $export operation can be called:

System
^^^^^^
**url:** [firely-server-base]/$export

This will create a system level export task, exporting all resources in the Firely Server database to a .ndjson file per resourcetype.

Patient
^^^^^^^

**url:** [firely-server-base]/Patient/$export

This will create a type level export task, exporting all resources included in the Patient Compartment in the Firely Server database to an .ndjson file per resourcetype.

Group
^^^^^
**url:** [firely-server-base]/Group/<group-id>/$export

This will create an instance level export task. For each Patient in the Group, the task will export all resources included in the Patient Compartment in the Firely Server database to an .ndjson file per resourcetype.

.. note:: For now we only support inclusion in a Group through Group.member.

Making an $export request will create a new task in the database with status "Queued". The request should return an absolute **$exportstatus** URL in the Content-Location header and the OperationOutcome in the body.  

$exportstatus
-------------

The $export request should return the $exportstatus url for your export task. This url can be used to request the current status of the task through a GET request, or to cancel the task through a DELETE request.

There are six possible status options:

1. Queued
2. Active
3. Complete
4. Failed
5. CancellationRequested
6. Cancelled

* If a task is Queued or Active, GET $exportstatus will return the status in the X-Progress header
* If a task is Complete, GET $exportstatus will return the results with a **$exportfilerequest** url per exported .ndjson file. This url can be used to retrieve the files per resourcetype. If there were any problems with parts of the export, an url for the generated OperationOutcome resources can be found in the error section of the result.
* If a task is Failed, GET $exportstatus will return HTTP Statuscode 500 with an OperationOutcome.
* If a task is on status CancellationRequested or Cancelled, GET $exportstatus will return HTTP Statuscode 410 (Gone).


$exportfilerequest
------------------

If a task has the Complete status, the GET $exportstatus request should return one or more $exportfilerequest urls.
Performing a GET request on this $exportfilerequest url returns a body of FHIR resources in newline delimited json (ndjson).

.. note::
  The Accept header for this request has to be:
  
  ::    
  
    application/fhir+ndjson
