.. _erase:

Permanently delete resources - $erase
=====================================

Description
-----------
When Firely Server receives a DELETE request for a resource, it marks it as deleted in the database which makes it hidden from search results. However, the data is still present in the database. This approach is known as *soft deletion*. This comes in handy in scenarios when you want to recover accidentally deleted data. However, there are also scenarios when you *actually* want the data to be erased from the database. For that purpose, Firely Server provides the $erase operation.

The `$erase` operation permanentely deletes a single resource or one or more historical revisions of a resource from the database. It can be executed on a resource instance level and a resource version level.

The `$purge` operation permanentely deletes all resources within a patient compartment. The operation can be executed at a Patient instance level.

.. note::

  The `$purge` operation is not available when SQLite is used as data storage.

Examples
^^^^^^^^

Use the following request to erase all versions (including the historical versions) of the ``Patient/example`` resource from the database.

::

  POST <base-url>/Patient/example/$erase

Use the following request to erase the specified version ``xyz`` and all the older versions (based on meta.lastUpdated) of the ``Patient/example`` resource from the database.

::

  POST <base-url>/Observation/example/_history/xyz/$erase

Use the following request to erase resources within the patient compartment of the Patient with id 'example'. Note that AuditEvent and Provenance resources won't get erased by default. Additionally, resources that have been soft deleted before are not being purged. See the configuration details in the :ref:`Appsettings` section, more specifically the ExcludeFromPatientPurge option, on how to exclude more resources from being purged.

::

  POST <base-url>/Patient/example/$purge

Appsettings
-----------
To enable the $erase operation you will first have to make sure the plugin ``Vonk.Plugin.EraseOperation.EraseOperationConfiguration`` is added to the PipelineOptions in the appsettings.

.. code-block:: JavaScript

 "PipelineOptions": {
    "PluginDirectory": "./plugins",
    "Branches": [
      {
        "Path": "/",
        "Include": [
          ...
          "Vonk.Plugin.EraseOperation.EraseOperationConfiguration"
        ],
        "Exclude": [
          ...
        ]
      }, ...etc...
    ]
  },
  "EraseOperation": {
      "ExcludeFromPatientPurge": [ ] // AuditEvents and Provenances will never be deleted 
  }


AuditEvents
-----------
- It is not allowed to erase AuditEvents
- AuditEvents for the $erase operation will contain the list of deleted items

License
-------
The $erase operation is part of the core Firely Server functionality. However, to use it, you may need to request an updated license from Firely. You can use your current license file if it contains ``http://fire.ly/vonk/plugins/erase``.
