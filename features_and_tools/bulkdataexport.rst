.. _feature_bulkdataexport:

Bulk Data Export
================

Firely Server provides the option to export resources with the Bulk Data Export Service. 
The Bulk Data Export Service enables the $export operation from the Fhir specification. Read more about the `$export request flow <https://hl7.org/fhir/uv/bulkdata/export/index.html#request-flow>`_.

.. note:: 

  To use Bulk Data Export you have to configure either :ref:`SQL Server <configure_sql>` or :ref:`MongoDB <configure_mongodb>` for the data database. Or you can implement it as part of a :ref:`feature_bulkdataexport_facade`.

  The Administration database can be configured to any of the three supported databases.

Appsettings
-----------
To start using the Bulk Data Export Service (BDE) you will first have to add the relevant plugins (Vonk.Plugin.BulkDataExport.[Level]BulkDataExportConfiguration) to the PipelineOptions in the appsettings. 
In the example below we have enabled all three levels: Patient, Group and System.

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
          "Vonk.Repository.Memory.MemoryVonkConfiguration",
          "Vonk.Subscriptions",
          "Vonk.Smart",
          "Vonk.UI.Demo",
          "Vonk.Plugin.DocumentOperation.DocumentOperationConfiguration",
          "Vonk.Plugin.ConvertOperation.ConvertOperationConfiguration",
          "Vonk.Plugin.BinaryWrapper",
          "Vonk.Plugin.Audit",
          "Vonk.Plugins.TerminologyIntegration",          
          "Vonk.Plugin.BulkDataExport.SystemBulkDataExportConfiguration",
          "Vonk.Plugin.BulkDataExport.GroupBulkDataExportConfiguration",
          "Vonk.Plugin.BulkDataExport.PatientBulkDataExportConfiguration",
        ],
        "Exclude": [
          "Vonk.Subscriptions.Administration"
        ]
      }, ...etc...

    
Bulk Data Export Service works as an asynchronous operation. To store the all operation-related information, it is necessary to enable a "Task Repository" on the admin database. Please enable the relevant "Vonk.Repository.[database-type].[database-type]TaskConfiguration" in the administration pipeline options, depending on the database type you use for the admin database. All supported databases can be used as a task repository. In the example below we have enabled the task repository for SQLite: "Vonk.Repository.Sqlite.SqliteTaskConfiguration".

.. code-block:: JavaScript

 "PipelineOptions": {
    "PluginDirectory": "./plugins",
    "Branches": [
      {
        "Path": "/administration",
        "Include": [
          "Vonk.Core",
          "Vonk.Fhir.R3",
          "Vonk.Fhir.R4",
          //"Vonk.Fhir.R5",
          //"Vonk.Repository.Sql.SqlTaskConfiguration",
          //"Vonk.Repository.Sql.SqlAdministrationConfiguration",
          "Vonk.Repository.Sql.Raw.KAdminSearchConfiguration",
          "Vonk.Repository.Sqlite.SqliteTaskConfiguration",
          "Vonk.Repository.Sqlite.SqliteAdministrationConfiguration",
          //"Vonk.Repository.MongoDb.MongoDbTaskConfiguration",
          "Vonk.Repository.MongoDb.MongoDbAdminConfiguration",
          "Vonk.Repository.Memory.MemoryAdministrationConfiguration",
          "Vonk.Subscriptions.Administration",
          "Vonk.Plugins.Terminology",
          "Vonk.Administration",
          "Vonk.Plugin.BinaryWrapper"
        ],
        "Exclude": [
          "Vonk.Core.Operations"
        ], ...etc...
    
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

.. _feature_bulkdataexport_facade:

Facade
-------

We support BDE for a facade. As always with a facade implementation, the parts dealing with the underlying proprietary datastore need to be implemented by you. Below you find an overview of the relevant steps for implementing BDE for a facade.

+--------------+-------------------------------------------------+--------------------------------------------------------------------+--------------------------------------------------+
| Export level | Area                                            | Setting                                                            | Action                                           |
+==============+=================================================+====================================================================+==================================================+
| All          | PipelineOptions for the administration endpoint | "Vonk.Repository.[database-type].[database-type]TaskConfiguration" | Enable for relevant administration database type |
+--------------+-------------------------------------------------+--------------------------------------------------------------------+--------------------------------------------------+
| All          | SupportedInteractions.WholeSystemInteractions   | $exportstatus                                                      | Enable                                           |
+--------------+-------------------------------------------------+--------------------------------------------------------------------+--------------------------------------------------+
| All          | SupportedInteractions.WholeSystemInteractions   | $exportfilerequest                                                 | Enable                                           |
+--------------+-------------------------------------------------+--------------------------------------------------------------------+--------------------------------------------------+
| All          | Facade plugin                                   | IBulkDataExportSnapshotRepository                                  | Implement                                        |
+--------------+-------------------------------------------------+--------------------------------------------------------------------+--------------------------------------------------+
| Patient      | PipelineOptions for the \ (root) endpoint       | "Vonk.Plugin.BulkDataExport.PatientBulkDataExportConfiguration"    | Enable                                           |
+--------------+-------------------------------------------------+--------------------------------------------------------------------+--------------------------------------------------+
| Patient      | SupportedInteractions.TypeLevelInteractions     | $export                                                            | Enable                                           |
+--------------+-------------------------------------------------+--------------------------------------------------------------------+--------------------------------------------------+
| Patient      | Facade plugin                                   | IPatientBulkDataExportRepository                                   | Implement                                        |
+--------------+-------------------------------------------------+--------------------------------------------------------------------+--------------------------------------------------+
| Group        | PipelineOptions for the \ (root) endpoint       | "Vonk.Plugin.BulkDataExport.GroupBulkDataExportConfiguration"      | Enable                                           |
+--------------+-------------------------------------------------+--------------------------------------------------------------------+--------------------------------------------------+
| Group        | SupportedInteractions.InstanceLevelInteractions | $export                                                            | Enable                                           |
+--------------+-------------------------------------------------+--------------------------------------------------------------------+--------------------------------------------------+
| Group        | Facade plugin                                   | IGroupBulkDataExportRepository                                     | Implement                                        |
+--------------+-------------------------------------------------+--------------------------------------------------------------------+--------------------------------------------------+
| System       | PipelineOptions for the \ (root) endpoint       | "Vonk.Plugin.BulkDataExport.SystemBulkDataExportConfiguration"     | Enable                                           |
+--------------+-------------------------------------------------+--------------------------------------------------------------------+--------------------------------------------------+
| System       | SupportedInteractions.SystemLevelInteractions   | $export                                                            | Enable                                           |
+--------------+-------------------------------------------------+--------------------------------------------------------------------+--------------------------------------------------+
| System       | Facade plugin                                   | ISystemBulkDataExportRepository                                    | Implement                                        |
+--------------+-------------------------------------------------+--------------------------------------------------------------------+--------------------------------------------------+

.. note::

  The interfaces below can be found in Vonk.Core version 4.7.0 and higher.

ISystemBulkDataExportRepository
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
The class implementing this interface is responsible for creating (and eventually deleting) a snapshot of the relevant data. This snapshot will be used at a later time for retrieving the data, mapping it to FHIR and writing the resources to the output files. How you store this snapshot is up to you. 

.. attention::

  The current implementation of the Bulk Data Export plugin for facades does not trigger ISystemBulkDataExportRepository.DeleteSnapshot(string taskId). This will be resolved in the upcoming release of Firely Server.

IPatientBulkDataExportRepository
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
Used when performing a Patient level export. It should retrieve the snapshot, use this to obtain the relevant data from the proprietary datastore and transform this to FHIR resources. Only data directly associated with the relevant Patient resources should be returned.

IGroupBulkDataExportRepository
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
Used when performing a Group level export. It should retrieve the snapshot, use this to obtain the relevant data from the proprietary datastore and transform this to FHIR resources.

ISystemBulkDataExportRepository
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
Used when performing a System level export. It should retrieve the snapshot, use this to obtain the relevant data from the proprietary datastore and transform this to FHIR resources.


  
