.. _feature_prevalidation:

Validating incoming resources
=============================

You can have Firely Server validate all resources that are sent in for create or update. The setting to do that is like this:
::

  "Validation": {
    "Parsing": "Permissive", // Permissive / Strict
    "Level": "Off", // Off / Core / Full
    "AllowedProfiles": 
    [
        "http://hl7.org/fhir/StructureDefinition/daf-patient", 
        "http://hl7.org/fhir/StructureDefinition/daf-allergyintolerance"
    ]
  },

Parsing
-------

Every incoming resource - xml or json - has to be syntactically correct. That is not configurable.

Beyond that, you can choose between Permissive or Strict parsing. Permissive allows for:

* empty elements (not having any value, child elements or extensions)
* the fhir_comments element
* errors in the xhtml of the Narrative
* json specific:
   * array with a single value instead of just a value, or vice versa (json specific)
      
* xml specific:
   * repeating elements interleaved with other elements
   * elements out of order 
   * mis-representation (element instead of attribute etc.)

Validation
----------

You can choose the level of validation:

* Off: no validation is performed.
* Core: the resource is validated against the core definition of the resourcetype.
* Full: the resource is validated against the core definition of the resourcetype and against any profiles it its ``meta.profile`` element.
  
Allow for specific profiles
---------------------------

To enable this feature, set ``Level`` to ``Full``.

If you leave the list of AllowedProfiles empty, any resource will be allowed (provided it passes the validations set in Parsing and Level).

When you add canonical urls of StructureDefinitions to this list, Firely Server will:

* check whether the incoming resource has any of these profiles listed in its meta.profile element
* validate the resource against the profiles listed in its meta.profile element.

So in the example above, Firely Server will only allow resources that conform to either the DAF Patient profile or the DAF AllergyIntolerance profile.

Note that the resource has to declare conformance to the profile in its ``meta.profile`` element. Firely Server will *not* try to validate a resource against all the ``Validation.AllowedProfiles`` to see whether the resource conforms to any of them, only those that the resource claims conformance to.

Upgrading from < 2.0
--------------------

Previous to version 2.0.0, the setting was ``"ValidateIncomingResources":"true" // or false``. The corresponding settings since version 2.0.0 are:

* true / no AllowedProfiles => ``"Level":"Core"``
* true / with AllowedProfiles => ``"Level":"Full"``, keep the AllowedProfiles as it is.
* false => ``"Level":"Off"``
