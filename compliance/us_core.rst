US CORE
=======

* Tested Version: Firely Server has been tested against `US Core Version 4.0.0 - STU4 Release <https://hl7.org/fhir/us/core/STU4/terminology.html>`_
* All StructureDefinitions for profiles and extensions are loaded by default in the standard SQlite Administration Database of Firely Server. No additional configuration needed in order to validate against these conformance resources.
* Firely Server provides full `"Profile Support and Interaction Support" <http://hl7.org/fhir/us/core/STU4/conformance-expectations.html#profile-support--interaction-support>`_:
  
  * Firely Server can be populated with resources conforming to US Core, including all elements marked as Must-Support
  * All search and CRUD interactions defined by US Core are supported, including optional search parameters
  
Known Limitations
^^^^^^^^^^^^^^^^^

* In order to validate resources claiming to conform to US Core, it is necessary to configure Firely Server to use an external terminology server incl. support for expanding SNOMED CT and LOINC ValueSets
* The $docref operation is not yet supported on DocumentReference resources
* No conformance claim is added by default to CapabilityStatement.instantiates
  
Test Data
^^^^^^^^^
