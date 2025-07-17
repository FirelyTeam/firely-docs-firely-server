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

Advanced Validation
-------------------

.. note::

  The features described in this section are available in the following :ref:`Firely Server editions <vonk_overview>`:

  * Firely Scale - 🌍 / 🇺🇸
  * Firely CMS Compliance - 🇺🇸

In the Firely Server editions mentioned above, Firely Server will execute additional advanced validation rules which are defined on top of the core FHIR specification for more quality control.

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

Filter validation outcome based on advisor rules
------------------------------------------------

.. note::

  The features described in this section are available in the following :ref:`Firely Server editions <vonk_overview>`:

  * Firely Scale - 🌍 / 🇺🇸
  * Firely CMS Compliance - 🇺🇸

Firely Server’s validator includes a powerful Advisor Rules system that enables users to dynamically customize validation behavior. 
Central to this system is the concept of filtering, which allows targeting specific validation issues or rule types based on precise criteria.

Filtering allows for:

  * Apply custom validation behavior (e.g., override severity, suppress issues, skip rules) only when specific conditions are met
  * Control the scope of your rules by narrowing them to targeted elements, structures, codes, or messages
  * Combine multiple filters for granular validation strategies

Configuration
^^^^^^^^^^^^^

The ``AdvisorRules`` setting in the ``Validation`` section of the appsettings allows to declaratively control how FHIR validation behaves at runtime.
``AdvisorRules`` must be provided as an escaped, JSON-encoded FHIR ``Parameters`` resource. The advisor rules are applied for all validation operations in Firely Server incl. pre-validation.

Rules Structure (Post-Processing)
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Each rule within the ``Parameters`` resource is defined as a ``parameter`` element of a specific type,
which can be further configured using nested ``part`` elements. The following rule types are used for
post-processing of the validation result:

- ``override`` – Changes the severity of specific validation issues in the final ``OperationOutcome``
- ``suppress`` – Removes matching issues entirely from the final ``OperationOutcome``

These rules do **not** influence the core validation logic, but apply only after validation has completed.

Override Rule
^^^^^^^^^^^^^

The ``override`` rule allows you to change the severity level of a validation issue **after** it has been generated
by the validator. This is a post-processing operation that rewrites the final ``OperationOutcome`` without altering
the validation logic itself.

This rule is useful when you want to:

- Downgrade known issues (e.g., from ``error`` to ``warning``)
- Upgrade certain messages (e.g., from ``information`` to ``fatal``)
- Mark ignorable issues as ``success``

Each ``override`` rule is defined using the following structure inside a FHIR ``Parameters`` resource:

.. code-block:: json

  {
    "name": "override",
    "part": [
      { "name": "<filter-type>", "valueString": "<match-value>" },
      { "name": "severity", "valueString": "<new-severity>" }
    ]
  }

Fields:

- ``filter-type``: One of the following:

  - ``code`` – Match on the validation issue code (e.g., ``6007``)
  - ``message`` – Match on the human-readable issue text
  - ``location`` – Match on the issue's path; this uses a prefix match (``startswith``), and you can use an asterisk (``*``) suffix to include child elements.
- ``match-value``: The value to match for the selected filter
- ``severity``: The new severity value to apply. Must be one of:

  - ``success``, ``information``, ``warning``, ``error``, ``fatal``

Suppress Rule
^^^^^^^^^^^^^

The ``suppress`` rule allows you to **remove specific validation issues** from the final ``OperationOutcome``.

This is useful when:

- You want to silence known or accepted validation deviations
- You want to reduce noise in the output for downstream systems
- You need to phase in conformance requirements without breaking clients

Each ``suppress`` rule is defined using the following structure inside a FHIR ``Parameters`` resource:

.. code-block:: json

  {
    "name": "suppress",
    "part": [
      { "name": "<filter-type>", "valueString": "<match-value>" }
    ]
  }

Fields:

- ``filter-type``: One of the following:

  - ``code`` – Match on the validation issue code (e.g., ``8000``)
  - ``message`` – Match on the issue’s text exactly
  - ``location`` – Match on the FHIR path or element location; uses a **prefix match**, and may include an asterisk (``*``) to match children
- ``match-value``: The value to compare for filtering

Element Rule
^^^^^^^^^^^^

The ``element`` rule in Advisor Rules allows you to selectively apply or skip specific types of validation
checks on a given FHIR element path. This is useful when you want to fine-tune which rule types are enforced
on certain parts of a resource.

Each ``element`` rule is defined using the following structure inside a FHIR ``Parameters`` resource:

.. code-block:: json

  {
    "name": "rules",
    "part": [
      {
        "name": "element",
        "part": [
          {
            "name": "filter",
            "part": [
              { "name": "path", "valueString": "<element-path>" }
            ]
          },
          { "name": "options", "valueString": "<rule-type>" }
        ]
      }
    ]
  }

Fields:

- ``path``: The FHIR path to the element to be filtered. You can use a trailing asterisk (``*``) to include child elements.
- ``options``: The types of rules to apply. You can include one or more of the following:

  - ``cardinality`` – Enforces min/max occurrence constraints
  - ``invariant`` – Evaluates invariants defined in StructureDefinition (e.g., ``ele-1``, ``dom-3``)
  - ``fixed`` – Validates fixed or pattern values defined on the element

Contained Rule
^^^^^^^^^^^^^^

The ``contained`` rule allows you to skip validation for specific contained resources within a parent resource.
Each ``contained`` rule is defined using the following structure inside a FHIR ``Parameters`` resource:

.. code-block:: json

  {
    "name": "rules",
    "part": [
      {
        "name": "contained",
        "part": [
          {
            "name": "filter",
            "part": [
              { "name": "id", "valueString": "<contained-resource-id>" },
              { "name": "kind", "valueString": "<contained-kind>" }
            ]
          },
          { "name": "options", "valueString": "<option>" }
        ]
      }
    ]
  }

Fields:

- ``id``: The local ID of the contained resource (e.g., ``invalid`` for ``#invalid``).
- ``kind``: The containment type. Typical values include:

  - ``contained`` – Standard contained resources (using ``#id`` references)
  - ``bundled`` – Contained-like entries that appear in a ``Bundle.entry`` context
  - ``parameters`` - Entries that appear in a ``Parameters.resource`` context
  - ``outcome`` - Subset of ``contained``, only works on contained resources inside ``OperationOutcome``
- ``options``: The action to take. Currently, the supported value is:

  - ``skip`` – Skip validation for the matching contained resource

Reference Rule
^^^^^^^^^^^^^^

The ``reference`` rule allows you to constrain how references are validated in a resource. This enables
you to control whether the validator enforces **type compatibility** or **existence checks** for specific
reference elements.

Each ``reference`` rule is defined using the following structure inside a FHIR ``Parameters`` resource:

.. code-block:: json

  {
    "name": "rules",
    "part": [
      {
        "name": "reference",
        "part": [
          {
            "name": "filter",
            "part": [
              { "name": "id", "valueString": "<reference-path>" }
            ]
          },
          { "name": "options", "valueString": "<option>" }
        ]
      }
    ]
  }

Fields:

- ``id``: The FHIRPath or ID of the reference element to filter (e.g., ``#Observation.subject`` or ``#Patient.generalPractitioner``)
- ``options``: One or more of the following:

  - ``exists`` – Check that the reference can be resolved
  - ``type`` – Check that the reference is of the correct resource type

Invariant Rule
^^^^^^^^^^^^^^

The ``invariant`` rule allows you to selectively enable, disable, or reclassify specific **invariants**
declared in a StructureDefinition.

This rule is useful when you want to:

- Demote strict invariants to warnings instead of errors
- Disable specific invariants temporarily for compatibility reasons
- Customize validation behavior on a per-resource-type and per-invariant basis

Each ``invariant`` rule is defined using the following structure inside a FHIR ``Parameters`` resource:

.. code-block:: json

  {
    "name": "rules",
    "part": [
      {
        "name": "invariant",
        "part": [
          {
            "name": "filter",
            "part": [
              { "name": "structure", "valueString": "<resource-structure-uri>" },
              { "name": "key", "valueString": "<invariant-key>" }
            ]
          },
          { "name": "options", "valueString": "<option>" }
        ]
      }
    ]
  }

Fields:

- ``structure``: The canonical URI of the resource or profile to which the invariant applies (e.g., ``http://hl7.org/fhir/StructureDefinition/Patient``)
- ``key``: The invariant key, as defined in the StructureDefinition (e.g., ``dom-3``, ``ele-1``)
- ``options``: One or more of the following:

  - ``warning`` – Demote this invariant to a warning
  - ``error`` – Promote or preserve this invariant as an error

Resource Rule
^^^^^^^^^^^^^

The ``resource`` rule allows you to control which profile(s) are considered when validating a resource.

This rule is useful when you want to:

- Ignore additional profiles declared in ``meta.profile``

Each ``resource`` rule is defined using the following structure inside a FHIR ``Parameters`` resource:

.. code-block:: json

  {
    "name": "rules",
    "part": [
      {
        "name": "resource",
        "part": [
          {
            "name": "filter",
            "part": [
              { "name": "path", "valueString": "<resource-path>" }
            ]
          },
          { "name": "options", "valueString": "stated" }
        ]
      }
    ]
  }

Fields:

- ``path``: The FHIRPath to the resource being filtered (e.g., ``Bundle.entry[1].resource[0]``)
- ``options``: Must be set to:

  - ``stated`` – Only validate against the explicitly declared structure or profile

Coded Rule
^^^^^^^^^^

The ``coded`` rule allows you to enable validation of value set bindings that are normally skipped by default,
such as those with strength ``preferred`` or ``example``.

Each ``coded`` rule is defined using the following structure inside a FHIR ``Parameters`` resource:

.. code-block:: json

  {
    "name": "rules",
    "part": [
      {
        "name": "coded",
        "part": [
          {
            "name": "filter",
            "part": [
              { "name": "id", "valueString": "<element-id>" }
            ]
          },
          { "name": "options", "valueString": "<option>" }
        ]
      }
    ]
  }

Fields:

- ``id``: The logical ID or FHIRPath of the coded element to apply the rule to (e.g., ``Patient.communication.language``)
- ``options``: Optional refinement(s) of what to validate:

  - ``concepts`` – Validate only the system/code pair and ignore the ``display`` field
  - ``display`` - Validate both system/code, and warn if display is not correct

Allow for Specific Profiles
---------------------------

To enable profile-specific validations, set ``Level`` to ``Full``. This ensure validation of the submitted instance against the cardinality constraints as well as any ValueSet-Bindings defined by the profile.
Firely Server will validate against all profiles listed under ``meta.profile`` claim defined in the instance.

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
