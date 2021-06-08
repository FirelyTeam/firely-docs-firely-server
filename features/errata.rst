.. _feature_errata:

Errata to the specification
===========================

The FHIR Specification is good, but not perfect. Some of the SearchParameters have errors. If we find these errors, we report them in the issue tracking system of HL7. 
But it takes time until the fix is applied to the specification. In the meantime Firely Server provides you with updated versions of the resources that have errors, so you can use them already while we await the fixes in the specification.

These corrections come with the Firely Server installation, in the files:

* ``errataFhir3.0.zip``, with corrections to the STU3 version of the Specification
* ``errataFhir4.0.zip``, with corrections to the R4 version of the Specification

These files are imported automatically during startup, as are other conformance resources, see :ref:`conformance`.

Currently the errata.zip file contains the following corrections:

clinical-patient
	This parameter incorrectly specified that both Patient and Group were target resource types for the patient search parameter. |br|
	For DeviceUseStatement-patient this was correct, so we created a separate file for this parameter, still listing the Group as a valid target type.

search parameters with FhirPath expression .as(DateTime)
	Several search parameters had an incorrect FhirPath expression using .as(DateTime) instead of .as(dateTime). As a result, Firely Server 
	could not index the fields correctly and searches on the dates would not work. The search parameters that were corrected are:
	clinical-date, DeviceRequest-event-date, Observation-code-value-date, Observation-value-date and patient-death-date.

Resource.<xyz> expressions
	The FhirPath library did not support polymorphism yet, so all the search parameters defined with an expression of Resource.<xyz> -- for example
	Resource.meta.lastUpdated -- did not work correctly. We have changed the expression to have just the <xyz> part -- for example meta.lastUpdated.

StructureDefinition.ext-context (R4 only)
	The FhirPath expression ended on a BackboneElement that cannot be indexed. Changed to the expression ``StructureDefinition.context.where(type='element').expression``.

.. |br| raw:: html

   <br />
