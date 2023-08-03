.. _erase:

Permanently delete resources - $erase
=====================================

Description
-----------
When Firely Server receives a DELETE request for a resource, it marks it as deleted in the database which makes it hidden from search results. However, the data is still present in the database. This approach is known as *soft deletion*. This comes in handy in scenarios when you want to recover accidentally deleted data. However, there are also scenarios when you *actually* want the data to be erased from the database. For that purpose, Firely Server provides the $erase operation.

The `$erase` operation permanently deletes a single resource or one or more historical revisions of a resource from the database. It can be executed on a resource instance level and a resource version level.

The `$purge` operation permanently deletes all resources within a patient compartment. The operation can be executed at a Patient instance level.

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


Use `ExcludeFromPatientPurge` to list resource types that are included in the Patient compartment but should not get deleted on patient `$purge` operation. By default, it contains only `AuditEvent` and `Provenance`.

Many resources in the Patient compartment reference resources outside the compartment. For example, a DeviceRequest might reference a Device. As Device itself is not in the Patient compartment, the resource would normally not be erased on `$purge`. But using the `IncludeIntoPatientPurge` setting, you can control which additional resource types get deleted if referenced by any of the resources from within the compartment. By default, no additional resources are specified.

AuditEvent & Provenance resources
---------------------------------
- It is not allowed to erase AuditEvents using `$erase`
- By default, it is not allowed to permanentely delete AuditEvent and Provenance resources using `$purge`
- AuditEvents that are created for the `$erase` and `$purge` operations will contain the list of deleted items

SMART on FHIR
-------------
To work with SMART on FHIR plugin of Firely Server, you need following custom scopes when requesting an access token

- Scope ``http://server.fire.ly/auth/scope/erase-operation`` for `$erase`
- Scope ``http://server.fire.ly/auth/scope/purge-operation`` for `$purge`

.. note::

  When the above custom scopes are used, the other SMART on FHIR scopes will be ignored by Firely Server. Due to this limitation, scopes for `$erase` and `$purge` should only be granted to admin users.

AuditEvents
-----------
- It is not allowed to erase AuditEvents
- AuditEvents for the $erase operation will contain the list of deleted items

License
-------
The `$erase` and `$purge` operations are part of the core Firely Server functionality. However, to use it, you may need to request an updated license from Firely. You can use your current license file if it contains ``http://fire.ly/vonk/plugins/erase``.
