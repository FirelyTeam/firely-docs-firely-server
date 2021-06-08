.. _conformance:

Conformance Resources
=====================

Firely Server uses `Conformance Resources <http://www.hl7.org/implement/standards/fhir/conformance-module.html>`_ along with some `Terminology Resources <http://www.hl7.org/implement/standards/fhir/terminology-module.html>`_ for various operations:

* SearchParameter: For indexing resources and evaluating :ref:`search <restful_search>` interactions.
* StructureDefinition: For :ref:`snapshot generation<feature_snapshot>`, and of course -- along with ValueSet and CodeSystem -- for :ref:`validation <feature_validation>`.
* CompartmentDefinition: For :ref:`access control <feature_accesscontrol>` and Compartment :ref:`restful_search`.
* ValueSet and CodeSystem: For :ref:`feature_terminology` operations.
* StructureMap and ConceptMap: for mapping

You can control the behaviour of Firely Server for these interactions by loading resources of these types into Firely Server. There are two ways of doing this:

#. With regular FHIR interactions (create, update, delete) on the :ref:`administration_api`.
#. With the :ref:`conformance_import`.

No matter which method you use, all Conformance resources are persisted in the Administration API database (see :ref:`configure_administration` for configuring that database), and available through the Administration API endpoint (``<firely-server-endpoint>/administration``)

For each resourcetype the base profile is listed in the CapabilityStatement under ``CapabilityStatement.rest.resource.profile`` and (since FHIR R4) all the other profiles are listed under ``CapabilityStatement.rest.resource.supportedProfile``. So by requesting the :ref:`CapabilityStatement <restful_capabilities>` you can easily check whether your changes to the StructureDefinitions were correctly processed by Firely Server.

.. attention::

   Please be aware that Conformance Resources have to have a **unique canonical url** within the FHIR Version they are loaded, in their url element. Firely Server does not allow you to POST two conformance resources with the same canonical url.
   For SearchParameter resources, the combination of base and code must be unique.

.. attention::

   Creates or updates of **SearchParameter** resources should be followed by a :ref:`re-index <feature_customsp_reindex>`.

   Before you delete a SearchParameter, be sure to remove it from the index first, see the ``exclude`` parameter in :ref:`re-index <feature_customsp_reindex>`.

   Changes to the other types of resources have immediate effect.

.. attention::

   A StructureDefinition can only be posted in the context of a FHIR Version that matches the StructureDefinition.fhirVersion. See :ref:`feature_multiversion`.

.. note::

   The import process will retain any id that is already in the resource, or assign a new id if there is none in the resource.
   For FHIR versions other than STU3, a postfix is appended to the id to avoid collissions between FHIR versions. See also :ref:`feature_multiversion_conformance`.

.. toctree::
   :maxdepth: 3

.. _conformance_import:

Import of Conformance Resources
-------------------------------

The import process of conformance resources runs on every startup of Firely Server, and :ref:`on demand<conformance_on_demand>`.

The process uses these locations on disk:

* ImportDirectory;
* ImportedDirectory;
* a read history in the file .vonk-import-history.json, written in ImportedDirectory.

.. attention::

   Please make sure that the Firely Server process has write permission on the ImportedDirectory.

The process follows these steps for each FHIR version (currently STU3 and R4, and experimentally for R5)

#. Load the :ref:`conformance_specification_zip`, if they have not been loaded before.
#. Load the :ref:`feature_errata`, if they have not been loaded before.
#. :ref:`conformance_fromdisk`. After reading, the read files are registered in the read history.
#. :ref:`conformance_fromsimplifier`. After reading, the project is registered in the read history. Subsequent reads will query only for resources that have changed since the last read.

Loading the conformance resources from the various sources can take some time,
especially on first startup when the :ref:`conformance_specification_zip` have to be imported.
During the import Firely Server will respond with 423 'Locked' to every request to avoid storing or retrieving inconsistent data.

The read history keeps a record of files that have been read, with an MD5 hash of each.
If you wish to force a renewed import of a specific file, you should:

* manually edit the read history file and delete the entry about that file;
* provide the file again in the ImportDirectory (if you deleted it previously - Vonk does not delete it).

.. _vonk_conformance_history:

Retain the import history
-------------------------

If you run the Administration database on SQL Server or MongoDb it is important to *retain* the ``.vonk-import-history`` file. This means that if you run Firely Server on something stateless like a Kubernetes pod, or a webapp service, you need to attach file storage on which to store this file. If you do not do that, Firely Server will import all the conformance resources *on every start*.

.. _vonk_conformance_instances:

Running imports with multiple instances
---------------------------------------

If you run multiple instances of Firely Server each will have its own ``/administration`` pipeline. So you need to make sure that only 1 instance will perform the import. The import at startup will happen when:

- we upgraded to a new version on the FHIR .NET API (always mentioned in the releasenotes)
- you add new resources to the ``ImportDirectory``
- resources retrieved from Simplifier are renewed.

To ensure that only one instance runs the import you can do two things:

#. Make sure only 1 instance is running:

   #. Stop Firely Server
   #. Scale down to 1 instance
   #. Upgrade Firely Server (by referring to a newer image, or installing newer binaries)
   #. Start Firely Server
   #. Let it do the import
   #. Then scale back up to multiple instances.

#. Exclude the namespace ``Vonk.Administration.Api.Import`` from the :ref:`PipelineOptions<vonk_plugins_config>` in branch ``administration`` on all but one instance.

If you want to use the manual import (``<url>/administration/import``) you are advised to apply solution nr. 1 above. In the second solution the call may or may not end up on an instance having the Import functionality.

We are aware that this can be a bit cumbersome. On the :ref:`vonk_roadmap` is therefore the story to host the Administration API in its own microservice.

.. _conformance_specification_zip:

Default Conformance Resources
-----------------------------

Firely Server comes with the specification.zip file from the HL7 FHIR API. It contains all the Conformance resources from the specification. These are loaded and used for validation and snapshot generation by default.

Some of the conformance resources (especially SearchParameters) contain errors in the core specification.
We try to correct all errors in :ref:`feature_errata`. You can also override them yourself by:

* updating them through the administration api, as described below;
* providing an altered version in the ImportDirectory, with the same id and canonical url.

.. attention::
   The Core Specification provides almost 4000 Conformance Resources. Depending on the machine it may take a few minutes to load and index them.

.. _conformance_fromdisk:

Load Conformance Resources from disk
------------------------------------

Firely Server can read SearchParameter and CompartmentDefinition resources from a directory on disk at startup. The AdministrationImportOptions in the :ref:`configure_appsettings` control from which directory resources are loaded::

  "AdministrationImportOptions": {
    "ImportDirectory": "<path to the directory you want to import from, default ./vonk-import>",
    "ImportedDirectory": "<path to the directory where imported files are moved to, default ./vonk-imported>"
  },

:ImportDirectory: All files and zip files will be read, and any conformance resources in them will be imported. By default, STU3 is assumed.
                  If you have R4 conformance resources, place them in a sibling directory that has the same name as your "ImportDirectory" with ``.R4`` appended to it -- so for example ``./vonk-import.R4``.
:ImportedDirectory: This directory will contain the read history in the .vonk-import-history.json file. Please note, that this information is stored directly in the administration database when running on SQlite.

Note that in json you either use forward slashes (/) or double backward slashes (\\\\) as path separators.

.. _conformance_fromsimplifier:

Load Conformance Resources from simplifier.net
----------------------------------------------

You are encouraged to manage and publish your profiles and related Conformance Resources on `simplifier.net <https://simplifier.net>`_. If you do that, you can have Firely Server read those. You configure this in the :ref:`configure_appsettings`::

  "AdministrationImportOptions": {
    "SimplifierProjects": [
      {
        "Uri": "FHIR endpoint for retrieving StructureDefinitions",
        "UserName": "UserName for retrieving the StructureDefinitions",
        "Password": "Password for the above user name",
        "BatchSize": "<number of resources imported at once, optional - default is 20>"
      }
    ],
  }

:Uri: must point to a Simplifier project endpoint, see below on how to get this
:UserName: your username, if you access a private Simplifier project
:Password: password with the username
:BatchSize: you normally don't need to change this parameter

You can load from multiple Simplifier projects by adding them to the list. The environment variable version of this is::

  VONK_Administration:SimplifierProjects:0:Uri=<FHIR endpoint for retrieving StructureDefinitions>
  
Vonk automatically finds the FHIR version for each project and imports it only for the matching FHIR version.

Get a FHIR endpoint for a Simplifier project
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Open the project of your choice on https://simplifier.net. There are two limitations:

1. You must have access to the project (so either public or private but accessible to you)
2. The project must be STU3

Then on the overview page of the project click 'Endpoint' and copy the value you see there:

   .. image:: ../images/simplifier-vonk-endpoint.png
      :align: center

By default the endpoint is ``https://stu3.simplifier.net/<projectname>``

.. _conformance_on_demand:

Load Conformance Resources on demand
------------------------------------

It can be useful to reload the profiles, e.g. after you have finalized changes in your project.
Therefore you can instruct Firely Server to actually load the profiles from the source(s) with a separate command:

::

  POST http(s)://<firely-server-endpoint>/administration/importResources

The operation will return an OperationOutcome resource, containing details about the number of resources created and updated, as well as any errors that occurred.
Please note that this will also respect the history of already read files, and not read them again.

.. _conformance_administration_api:

Manage Conformance Resources with the Administration API
--------------------------------------------------------

The :ref:`administration_api` has a FHIR interface included, on the ``https://<firely-server-endpoint>/administration`` endpoint. On this endpoint you can do most of the FHIR interactions (create, read, update, delete, search) on these resourcetypes:

* SearchParameter
* StructureDefinition
* ValueSet
* CodeSystem
* CompartmentDefinition

If you are :ref:`not permitted <configure_administration_access>` to access the endpoint for the resource you want to manage (e.g. ``<firely-server-endpoint>/administration/StructureDefinition``), Firely Server will return statuscode 403.

.. note:: You can also do the same interactions on the same resourcetypes on the normal (or 'data') FHIR endpoint ``https://<firely-server-endpoint>``. This will only result in storing, updating or deleting the resource. But it will not have any effect on the way Firely Server operates.

Example
^^^^^^^

To add a StructureDefinition to Firely Server
::

    POST <firely-server-endpoint>/administration/StructureDefinition

* In the body provide the StructureDefinition that you want to add.
* The Content-Type header must match the format of the body (application/fhir+json or application/fhir+xml)

If you prefer to assign your own logical id to e.g. StructureDefinition 'MyPatient', you can use an update:
::

    PUT <firely-server-endpoint>/administration/StructureDefinition/MyPatient
