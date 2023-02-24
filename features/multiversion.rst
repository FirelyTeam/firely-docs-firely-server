.. _feature_multiversion:

Multiple versions of FHIR
=========================

Since version 3.0.0 Firely Server can run multiple versions of FHIR side-by-side in the same server. This page explains how this works and what the consequences are.

Requests
--------

The FHIR Specification explains the mimetype parameter that distinguishes one FHIR version from another in the paragraph on the `FHIR Version parameter <http://hl7.org/fhir/R4/http.html#version-parameter>`_.
Firely Server uses this approach to let you choose the model for your request. Below are examples on how to use the fhirVersion parameter and how in influences the behaviour of Firely Server. 
Accepted values for the parameter are:

* fhirVersion=3.0, for FHIR STU3
* fhirVersion=4.0, for FHIR R4

You can add the fhirVersion to the Accept and/or the Content-Type header. If you specify it on both, the fhirVersion parameters have to be the same.

.. note::
   The fhirVersion parameter is also part of the Content-Type header of the response by Firely Server. Settings can control this, see :ref:`feature_multiversion_endpoints` below.

The examples below explain the behaviour with STU3, but if you replace fhirVersion with 4.0, it works exactly the same on R4. 

.. note:: 
   If you do not specify a fhirVersion parameter, Firely Server will use fhirVersion=3.0 (STU3) as a default. This way the behaviour is compatible with previous versions of Firely Server. If you like, you can change the ``Default`` in :ref:`information_model`

.. note:: 
   If you use both an Accept header and a Content-Type header, the fhirVersion parameter for both must be the same. So this would be *invalid*:
   ::

      POST <base>/Patient
      Accept=application/fhir+json; fhirVersion=3.0
      Content-Type=application/fhir+json; fhirVersion=4.0

Search for all Patients in STU3. In Firely Server this means Patient resources that were also stored as STU3. There is no automatic conversion of resources that were stored as R4 to the STU3 format (or vice versa). ::

      GET <base>/Patient
      Accept=application/fhir+json; fhirVersion=3.0

Search for Patients with the name 'Fred' in STU3. The search parameters used in the query must be valid in STU3. ::

   GET <base>/Patient?name=Fred
   Accept=application/fhir+json; fhirVersion=3.0

Create a Patient resource in STU3. This will only be retrievable when accessed with STU3: ::

   POST <base>/Patient
   Content-Type=application/fhir+json; fhirVersion=3.0
   Accept=application/fhir+json; fhirVersion=3.0

   {<valid Patient JSON body>}

Update a Patient resource in STU3.::

   PUT <base>/Patient/123
   Content-Type=application/fhir+json; fhirVersion=3.0
   Accept=application/fhir+json; fhirVersion=3.0

   {<valid Patient JSON body with id: 123>}

#. If no resource with this id existed before: it will be created with this id. (This was already always the behaviour of Firely Server.)
#. If a resource with this id existed before, in STU3: update it.
#. If a resource with this id already exists in R4: you will get an error with an OperationOutcome saying that a resource with this id already exists with a different informationmodel.

.. note:: Id's still have to be unique within a resourcetype, regardless of the FHIR version.

Delete a Patient resource.::

   DELETE <base>/Patient/123
   Accept=application/fhir+json; fhirVersion=3.0

This will delete Patient/123, regardless of its FHIR version. The Accept header is needed for Firely Server to know how to format an OperationOutcome if there is an error.

.. _feature_multiversion_conformance:

Conformance resources
---------------------

Conformance resources like StructureDefinition and SearchParameter are registered *per FHIR version*. This implies:

#. Conformance resources will be imported during :ref:`conformance_import` for both STU3 and R4. To avoid id clashes (see note above), the id's in R4 are appended with '-Fhir4.0'

   #. So the StructureDefinition for Patient will be available for STU3 and R4 respectively like this:
   
   ::

      GET <base>/StructureDefinition/Patient
      Accept=application/fhir+json; fhirVersion=3.0

      GET <base>/StructureDefinition/Patient-Fhir4.0
      Accept=application/fhir+json; fhirVersion=4.0

#. If you add a StructureDefinition or SearchParameter via the Administration API, you can decide for yourself whether to append the FHIR version to the id or not. 
   Just note that you cannot use the same id for different FHIR versions.
#. Depending on the fhirVersion parameter Firely Server evaluates whether a resourcetype or searchparameter is valid in that FHIR version. E.g. 'VerificationResult' is only valid in R4, but 'DataElement' is only valid in R3.
#. For validation, the StructureDefinitions and terminology resources needed are only searched for in the FHIR version of the resource that is being validated.
#. When you :ref:`conformance_administration_api`, a StructureDefinition can only be posted to the Administration API in the context of a FHIR Version that matches the StructureDefinition.fhirVersion.
   So this works::
   
      POST <base>/administration/StructureDefinition
      Accept=application/fhir+json; fhirVersion=4.0
      Content-Type=application/fhir+json; fhirVersion=4.0

      {
         "resourcetype": "StructureDefinition"
         ...
         "fhirVersion": "4.0.0" //Note the FHIR version matching the Content-Type
      }

   But it would not work if ``"fhirVersion"="3.0.1"``

#. If you :ref:`conformance_on_demand`, this will be done for all the importfiles described above, regardless of the fhirVersion in the Accept header.

.. _feature_multiversion_singleversion:

Running a single version
------------------------

To use only a single version you set the ``Default`` information model in :ref:`information_model` to the version you want to use. In addition, you can exclude the namespace of the version you don't need (``Vonk.Fhir.R3`` or ``Vonk.Fhir.R4``) from the :ref:`PipelineOptions <vonk_plugins_config>` to disable its use. If you exclude a namespace, make sure to exclude it from all branches.

.. _feature_multiversion_endpoints:

Running different versions on different endpoints
-------------------------------------------------

To assign endpoints to different versions, create a mapping in :ref:`information_model`. Use the ``Mode`` switch to select either a path or a subdomain mapping, assigning your endpoints in the ``Map`` array. Mapped endpoints will only accept the version you have specified. The web service root ('/' and '/administration/') will still accept all supported versions.

Assigning an endpoint to a FHIR version is exactly equivalent to adding that particular ``fhirVersion`` MIME parameter to every single request sent to that endpoint. So using these settings:
::   

   "InformationModel": {
      "Default": "Fhir4.0",
      "IncludeFhirVersion": ["Fhir4.0", "Fhir5.0"],
      "Mapping": {
         "Mode": "Path",
         "Map": {
            "/R3": "Fhir3.0",
            "/R4": "Fhir4.0"
         }
      }
   }

The call

::

   GET http://myserver.org/Patient
   Accept=application/fhir+json; fhirVersion=3.0

   is equivalent to

   GET http://myserver.org/R3/Patient

and the call

::

   GET http://myserver.org/Patient (defaults to R4)

   is equivalent to

   GET http://myserver.org/R4/Patient

and the administration call

::

   GET http://myserver.org/administration/StructureDefinition (defaults to R4)

   is equivalent to

   GET http://myserver.org/administration/R4/StructureDefinition (/R4 is a postfix to '/administration')


As you can see, on a mapped endpoint it is never necessary to use a FHIR ``_format`` parameter or a ``fhirVersion`` MIME parameter in a ``Content-Type`` or ``Accept`` header.

Response Content-Type
^^^^^^^^^^^^^^^^^^^^^

The setting ``IncludeFhirVersion`` is used for the Content-Type of the response from Firely Server. Some clients cannot handle a parameter on the mimetype, and the fhirVersion parameter was originally not part of FHIR STU3. Therefore this settings allows you to specify for which FHIR versions this parameter should be included in the Content-Type header.
By default we set it to FHIR R4 and R5, as for STU3 the fhirVersion may be unexpected for clients.

.. _feature_multi_version_r5:

Support for R5 (experimental!)
------------------------------

By default the binaries for supporting R5 are included in the Firely Server distribution (since Firely Server (Vonk) 3.3.0). By default these binaries are not loaded. See the PipelineOptions in appsettings.default, where ``Vonk.Fhir.R5`` is commented out. Re-enable these in your appsettings.instance.

Note that there is not yet an ``errata_Fhir5.0.zip`` and Firely Server will complain about that in the log. You can ignore that message.
