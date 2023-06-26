.. _feature_bulkdataexport:

================
Bulk Data Export
================

Bulk Data Export (BDE) is the process for exporting a substantial amount of data from a system or database in a single operation. 
This process entails extracting a significant volume of data, usually in a structured format, to support activities like analysis, reporting, backup, or data transfer between systems. 
BDE is frequently utilized in various implementation guides to facilitate the bulk downloading or exchanging of patient data.


170.315 (b)(10) Electronic Health Information (EHI) Export
----------------------------------------------------------

170.315 (b)(10) specifically addresses the Electronic Health Information (EHI) Export requirement. 
To comply with this requirement, Firely Server offers full support through its Bulk Data Export feature. 
Before using Bulk Data Export (BDE) to facilitate EHI Export for B.10, we recommend reviewing the technical documentation provided below for setting up BDE. 
For comprehensive information on meeting the B.10 regulation, please visit our :ref:`dedicated 170.315 (b)(10) page <compliance_b_10>`.


Introduction
------------

.. note::
  This application is licensed separately from the core Firely Server distribution. Please :ref:`contact<vonk-contact>` Firely to get the license. 
  Your license already permits the usage of BDE if it contains ``http://fire.ly/vonk/plugins/bulk-data-export``. You can also try out the BDE feature using the evaluation license.


Firely Server provides the option to export resources with the Bulk Data Export Service. 
The Bulk Data Export Service enables the $export operation from the Fhir specification. Read more about the `$export request flow <https://hl7.org/fhir/uv/bulkdata/export/index.html#request-flow>`_

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

BDE introduces several new parts to the appsettings:

.. code-block:: JavaScript

  "TaskFileManagement": {
      "StorageService": {
          "StorageType": "LocalFile", // LocalFile / AzureBlob / AzureFile
          "StoragePath": "./taskfiles",
          "ContainerName": "firelyserver" // For AzureBlob / AzureFile only
      }
  },
  "AzureServices": {
      "Storage": {
          "AccountName": "<your Azure account name>",
          "AccountKey": "API key for your Azure account"
      }
  },
  "BulkDataExport": {
      "RepeatPeriod" : 60000, //ms
      "AdditionalResources": [ "Organization", "Location", "Substance", "Device", "BodyStructure", "Medication", "Coverage" ] 
  },
  "SqlDbOptions": {
      // ...
      "BulkDataExportTimeout": 300 // in seconds
  }

In `RepeatPeriod` you can configure the polling interval (in milliseconds) for checking the Task queue for a new export task.

A patient-based or group-based Bulk Data Export returns resources based on the Patient compartment definition (https://www.hl7.org/fhir/compartmentdefinition-patient.html). These resources may reference resources outside the compartment as well, such as a Practitioner who is the performer of a Procedure. Using the `AdditionalResources`-setting, you can determine which types of referenced resources are exported in addition to the compartment resources.

Exporting a large number of resources from a SQL Server database can cause a timeout exception. You can adjust the timeout period in `BulkDataExportTimeout`. There is no timeout limitation when exporting data from MongoDB.

Writing to a local disk
^^^^^^^^^^^^^^^^^^^^^^^
Set the ``StorageType`` to ``LocalDisk``.

In ``StoragePath`` you can configure the folder where the exported files will be saved to. Make sure the server has write access to this folder.

Writing to Azure Blob or Azure Files
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
Set:
  - ``StorageType`` to ``AzureBlob`` or ``AzureFiles``
  - ``StoragePath`` to the path within the container that you prefer
  - ``ContainerName`` to the name of the container to use (see documentation on Azure Blob Storage or Azure Files for details)

Also make sure you fill in the account details for Azure in ``AzureServices`` as above.

$export
-------

There are three different levels for which the $export operation can be called:

System
^^^^^^
**url:** ``[firely-server-base]/$export``

This will create a system level export task, exporting all resources in the Firely Server database to a .ndjson file per resourcetype.

Patient
^^^^^^^

**url:** ``[firely-server-base]/Patient/$export``

This will create a type level export task, exporting all resources included in the Patient Compartment in the Firely Server database to an .ndjson file per resourcetype.

Group
^^^^^
**url:** ``[firely-server-base]/Group/<group-id>/$export``

This will create an instance level export task. For each Patient in the Group, the task will export all resources included in the Patient Compartment in the Firely Server database to an .ndjson file per resourcetype.

.. note:: For now we only support inclusion in a Group through Group.member.

$export Response
^^^^^^^^^^^^^^^^

Making an **$export** request will create a new task in the database with status "Queued". The request should return an absolute **$exportstatus** URL in the Content-Location header and the OperationOutcome in the response body.  

.. START-BDE-QUEUED-BODY

.. code-block:: json
    :caption: **$Example export response body**
    
    {
        "resourceType": "OperationOutcome",
        "id": "ce82d245-ed15-4cf1-816f-784f8c937e72",
        "meta": {
            "versionId": "addcff4e-4bc1-4b68-a08c-e76409a0b5b0",
            "lastUpdated": "2023-06-16T19:15:55.092273+00:00"
        },
        "issue": [
            {
                "severity": "information",
                "code": "informational",
                "diagnostics": "The $export task is successfully added to the queue. Status updates can be requested using https://localhost:4081/$exportstatus?_id=13d8ce0d-9f96-48d4-96a7-58d0b3dd4e75. This URL can also be found in the Content-Location header."
            }
        ]
    }

.. END-BDE-QUEUED-BODY

.. _bdeexportstatus:

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

.. START-BDE-COMPLETE-BODY

.. code-block:: json
    :caption: **$Example exportstatus complete response body**

    {
        "transactionTime": "2023-06-16T17:01:04.6036373+00:00",
        "request": "/Patient/$export",
        "requiresAccessToken": false,
        "output": [
            {
                "type": "Invoice",
                "url": "https://localhost:4081/$exportfilerequest/?_id=6a8936d5-b1ab-46fb-a54b-0f69f8b4fda6&filename=contentInvoice.ndjson"
            },
            {
                "type": "Patient",
                "url": "https://localhost:4081/$exportfilerequest/?_id=6a8936d5-b1ab-46fb-a54b-0f69f8b4fda6&filename=contentPatient.ndjson"
            }
        ],
        "error": [],
        "extension": {
            "http://server.fire.ly/context/informationModel": "Fhir4.0",
            "ehiDocumentationUrl": "https://docs.fire.ly/projects/Firely-Server/en/latest/features_and_tools/bulkdataexport.html"
        }
    }

.. END-BDE-COMPLETE-BODY

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
------

We support BDE for a facade. As always with a facade implementation, the parts dealing with the underlying proprietary datastore need to be implemented by you. Below you find an overview of the relevant steps for implementing BDE for a facade.

+---------------+-------------------------------------------------+--------------------------------------------------------------------+--------------------------------------------------+
| Export level  | Area                                            | Setting                                                            | Action                                           |
+===============+=================================================+====================================================================+==================================================+
| All           | PipelineOptions for the administration endpoint | "Vonk.Repository.[database-type].[database-type]TaskConfiguration" | Enable for relevant administration database type |
+---------------+-------------------------------------------------+--------------------------------------------------------------------+--------------------------------------------------+
| All           | SupportedInteractions.WholeSystemInteractions   | $exportstatus                                                      | Enable                                           |
+---------------+-------------------------------------------------+--------------------------------------------------------------------+--------------------------------------------------+
| All           | SupportedInteractions.WholeSystemInteractions   | $exportfilerequest                                                 | Enable                                           |
+---------------+-------------------------------------------------+--------------------------------------------------------------------+--------------------------------------------------+
| All           | Facade plugin                                   | IBulkDataExportSnapshotRepository                                  | Implement                                        |
+---------------+-------------------------------------------------+--------------------------------------------------------------------+--------------------------------------------------+
| Patient       | PipelineOptions for the \ (root) endpoint       | "Vonk.Plugin.BulkDataExport.PatientBulkDataExportConfiguration"    | Enable                                           |
+---------------+-------------------------------------------------+--------------------------------------------------------------------+--------------------------------------------------+
| Patient       | SupportedInteractions.TypeLevelInteractions     | $export                                                            | Enable                                           |
+---------------+-------------------------------------------------+--------------------------------------------------------------------+--------------------------------------------------+
| Patient       | Facade plugin                                   | IPatientBulkDataExportRepository                                   | Implement                                        |
+---------------+-------------------------------------------------+--------------------------------------------------------------------+--------------------------------------------------+
| Group         | PipelineOptions for the \ (root) endpoint       | "Vonk.Plugin.BulkDataExport.GroupBulkDataExportConfiguration"      | Enable                                           |
+---------------+-------------------------------------------------+--------------------------------------------------------------------+--------------------------------------------------+
| Group         | SupportedInteractions.InstanceLevelInteractions | $export                                                            | Enable                                           |
+---------------+-------------------------------------------------+--------------------------------------------------------------------+--------------------------------------------------+
| Group         | Facade plugin                                   | IGroupBulkDataExportRepository                                     | Implement                                        |
+---------------+-------------------------------------------------+--------------------------------------------------------------------+--------------------------------------------------+
| System        | PipelineOptions for the \ (root) endpoint       | "Vonk.Plugin.BulkDataExport.SystemBulkDataExportConfiguration"     | Enable                                           |
+---------------+-------------------------------------------------+--------------------------------------------------------------------+--------------------------------------------------+
| System        | SupportedInteractions.SystemLevelInteractions   | $export                                                            | Enable                                           |
+---------------+-------------------------------------------------+--------------------------------------------------------------------+--------------------------------------------------+
| System        | Facade plugin                                   | ISystemBulkDataExportRepository                                    | Implement                                        |
+---------------+-------------------------------------------------+--------------------------------------------------------------------+--------------------------------------------------+
| Patient/Group | Facade plugin                                   | IPatientBulkDataWithPatientsFilterExportRepository                 | Implement (optional, enables 'patient' filter)   |
+---------------+-------------------------------------------------+--------------------------------------------------------------------+--------------------------------------------------+
| Patient/Group | Facade plugin                                   | IGroupBulkDataWithPatientsFilterExportRepository                   | Implement (optional, enables 'patient' filter)   |
+---------------+-------------------------------------------------+--------------------------------------------------------------------+--------------------------------------------------+

.. note::

  The interfaces below can be found in Vonk.Core version 4.7.0 and higher.

IBulkDataExportSnapshotRepository
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
The class implementing this interface is responsible for creating (and eventually deleting) a snapshot of the relevant data. This snapshot will be used at a later time for retrieving the data, mapping it to FHIR and writing the resources to the output files. How you store this snapshot is up to you. 

.. attention::

  The current implementation of the Bulk Data Export plugin for facades does not trigger IBulkDataExportSnapshotRepository.DeleteSnapshot(string taskId). This will be resolved in the upcoming release of Firely Server.

IPatientBulkDataExportRepository
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
Used when performing a Patient level export. It should retrieve the snapshot, use this to obtain the relevant data from the proprietary datastore and transform this to FHIR resources. Only data directly associated with the relevant Patient resources should be returned.

IGroupBulkDataExportRepository
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
Used when performing a Group level export. It should retrieve the snapshot, use this to obtain the relevant data from the proprietary datastore and transform this to FHIR resources.

ISystemBulkDataExportRepository
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
Used when performing a System level export. It should retrieve the snapshot, use this to obtain the relevant data from the proprietary datastore and transform this to FHIR resources.

.. note::

  The interfaces below can be found in Vonk.Core version 5.1.0 and higher.
  
IPatientBulkDataWithPatientsFilterExportRepository
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
Optional addition. Used when performing a Patient level export with the 'patient' parameter in the request. It should filter the patients from the snapshot based on the references provided as specified in https://build.fhir.org/ig/HL7/bulk-data/export.html#query-parameters.

IGroupBulkDataWithPatientsFilterExportRepository
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
Optional addition. Used when performing a Group level export with the 'patient' parameter in the request. It should filter the patients from the snapshot based on the references provided as specified in https://build.fhir.org/ig/HL7/bulk-data/export.html#query-parameters.
