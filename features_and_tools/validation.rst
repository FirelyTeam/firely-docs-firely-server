.. _feature_prevalidation:

Validating incoming resources
=============================

.. note::

  The features described on this page are available in **all** :ref:`Firely Server editions <vonk_overview>`.

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
* Core: the resource is validated against the core definition of the resource type.
* Full: the resource is validated against the core definition of the resource type and against any profiles in its ``meta.profile`` element.
  
If validation is set to ``Full`` the following validation rules will be checked:

+-------------------------------+---------------------------------------------------------------+
| Validation Rule               | Description                                                   |
+===============================+===============================================================+
| BindingValidator              | Validates that a coded value exists in the bound ValueSet     |
+-------------------------------+---------------------------------------------------------------+
| CanonicalValidator            | Ensures a ‘canonical’ type is an absolute URI or fragment     |
+-------------------------------+---------------------------------------------------------------+
| CardinalityValidator          | Verifies element occurrences match defined cardinality        |
+-------------------------------+---------------------------------------------------------------+
| ExtensionContextValidator     | Ensures an extension is used in its allowed context           |
+-------------------------------+---------------------------------------------------------------+
| FhirPathValidator             | Validates resource fields against FHIRPath expressions        |
+-------------------------------+---------------------------------------------------------------+
| FhirStringValidator           | Checks that a FHIR ‘string’ is not empty                      |
+-------------------------------+---------------------------------------------------------------+
| FhirTypeLabelValidator        | Validates that instance type matches the declared label       |
+-------------------------------+---------------------------------------------------------------+
| FhirUriValidator              | Ensures element value is a valid URI when serialized          |
+-------------------------------+---------------------------------------------------------------+
| FixedValidator                | Checks that an element has the required fixed value           |
+-------------------------------+---------------------------------------------------------------+
| MaxLengthValidator            | Enforces the maximum allowed string length                    |
+-------------------------------+---------------------------------------------------------------+
| MinMaxValueValidator          | Validates numeric or primitive values against min/max limits  |
+-------------------------------+---------------------------------------------------------------+
| PatternValidator              | Validates element value against a defined pattern             |
+-------------------------------+---------------------------------------------------------------+
| RegExValidator                | Checks element value against a regular expression             |
+-------------------------------+---------------------------------------------------------------+
| ReferencedInstanceValidator   | Resolves and validates referenced resources                   |
+-------------------------------+---------------------------------------------------------------+
| SchemaReferenceValidator      | Validates element using its referenced schema                 |
+-------------------------------+---------------------------------------------------------------+
| SliceValidator                | Validates element against slice constraints                   |
+-------------------------------+---------------------------------------------------------------+
| FhirEle1Validator             | Requires element to have a value or children                  |
+-------------------------------+---------------------------------------------------------------+
| FhirExt1Validator             | Requires element to have a value or extension                 |
+-------------------------------+---------------------------------------------------------------+
| FhirTxt1Validator             | Validates that narrative contains valid HTML                  |
+-------------------------------+---------------------------------------------------------------+
| FhirTxt2Validator             | Ensures narrative is not whitespace-only                      |
+-------------------------------+---------------------------------------------------------------+
| ChildrenValidator             | Applies validation rules to child elements                    |
+-------------------------------+---------------------------------------------------------------+
| DataTypeSchema /              | Validates against data type, resource, or extension schema    |
| ResourceSchema /              |                                                               |
| ExtensionSchema               |                                                               |
+-------------------------------+---------------------------------------------------------------+

.. _feature_advancedvalidation:

Advanced Validation
-------------------

.. note::

  The features described in this section are available in the following :ref:`Firely Server editions <vonk_overview>`:

  * Firely Scale - 🌍 / 🇺🇸
  * Firely CMS Compliance - 🇺🇸

In the Firely Server edition mentioned above, Firely Server will execute additional advanced validation rules which are defined on top of the core FHIR specification for more quality control.

If validation is set to ``Full`` the following validation rules will be checked:

+--------------------------------+---------------------------------------------------------------------------------------------------------------+
| Validation Rule                | Description                                                                                                   |
+================================+===============================================================================================================+
| ElementDefinitionValidator     | Validates that ElementDefinitions paths are valid                                                             |
+--------------------------------+---------------------------------------------------------------------------------------------------------------+
| StructureDefinitionValidator   | Validates slicing and invariant definitions in StructureDefinitions                                           |
+--------------------------------+---------------------------------------------------------------------------------------------------------------+
| QuestionnaireResponseValidator | Validates a QuestionnaireResponse against a Questionnaire (can be stored in the Firely Server admin database) |
+--------------------------------+---------------------------------------------------------------------------------------------------------------+

Allow for Specific Profiles
---------------------------

To enable profile-specific validations, set ``Level`` to ``Full``.

Configuring AllowedProfiles
^^^^^^^^^^^^^^^^^^^^^^^^^^^

- When you leave the ``AllowedProfiles`` list empty, Firely Server will permit any resource, provided it passes the general validations set by ``Parsing`` and ``Level``.

- Adding canonical URLs of ``StructureDefinitions`` to the ``AllowedProfiles`` list instructs Firely Server to perform specific checks:

  1. **Profile Existence Check**:
     Firely Server will verify whether the incoming resource declares any of these profiles in its ``meta.profile`` element.

  2. **Conformance Validation**:
     Firely Server will validate the resource against any profiles it claims to conform to in its ``meta.profile`` element. This validation step is governed by the ``Level`` setting, not specifically by ``AllowedProfiles``.


**Example:** If you add the DAF Patient and DAF AllergyIntolerance profiles to ``AllowedProfiles``, Firely Server will only allow resources that declare and conform to these profiles.

Important Notes
^^^^^^^^^^^^^^^

- The resource must explicitly declare conformance to a profile in its ``meta.profile`` for Firely Server to validate against it. Firely Server will **not** try to validate a resource against all the Validation.AllowedProfiles to see whether the resource conforms to any of them, only those that the resource claims conformance to.
- **AuditEvent Logging and AuditEvent Signatures**:
  If enabled, ``AuditEvent`` and ``Provenance`` resources generated by these processes will also be subject to checks against the ``AllowedProfiles``. It is necessary to include the HL7 core canonical URLs of these resources in ``AllowedProfiles`` to ensure they are saved in the database without issues.


::

  {
    "Validation": {
      ...
      "AllowedProfiles": [
        "http://hl7.org/fhir/StructureDefinition/AuditEvent",
        "http://hl7.org/fhir/StructureDefinition/Provenance"
        ...
      ]
    }
  }
