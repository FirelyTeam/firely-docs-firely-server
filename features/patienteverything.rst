.. _feature_patienteverything:

Patient $everything
===================

Description
-----------

This plugin implements the Patient $everything operation, as described in https://www.hl7.org/fhir/operation-patient-everything.html. This operation returns all resources associated with a single patient. The resources returned are determined by the Patient compartment, defined in https://www.hl7.org/fhir/compartmentdefinition-patient.html. Currently, the functionality is only available if you use SQL server for your data.

::

   GET <base-url>/Patient/<patient-id>/$everything
   Accept: <any supported FHIR media type>
   
Optional parameters:

* _since: Get only resources changed since this moment
* _type: Limit the returned resource types to only the types in this list

Please note that other defined operation parameters have not been implemented (yet).

So if you would want to fetch only Patient 1 and their Observations, changed since the 1st of January, 2021 in FHIR JSON format, you would use:

::

   GET <base-url>/Patient/1/$everything?_type=Patient,Observation&_since=2021-01-01
   Accept: application/fhir+json
   
Configuration
-------------
Many resources in the Patient compartment reference resources outside the compartment. For example: An Observation might have a performer which is a Practitioner. As Practitioner itself is not in the Patient compartment, the resource would normally not be returned. But using a setting you can control which additional resource types are returned if they are referenced by any of the resources you requested.

.. code-block:: JavaScript

   "PatientEverythingOperation": {
      "AdditionalResources": [ "Organization", "Location", "Substance", "Device", "Medication" ] 
   }
   
This is the default value for the setting. As you can see, Practitioner is not included by default out of privacy considerations but you can change that by overriding the setting. To include the plugin in your pipeline, add the following extra Include:

.. code-block:: JavaScript

   "PipelineOptions": {
      "Branches": [
         {
            "Path": "/",
            "Include": [
               ...
               "Vonk.Plugin.PatientEverything"
            ]
         },
         ...
      ]
   }   

License
-------
The Patient $everything operation is licensed. To use it, you may need to renew your license.
