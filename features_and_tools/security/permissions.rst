
Permissions (AccessPolicy)
--------------------------
Firely Server supports filtering the granted access scopes for a client by using built-in custom access policy resources. 
The access policy decisions are based on HL7 SMART on FHIR scopes, both SMART on FHIR v1 and v2 scopes are supported.
The structure definitions are preloaded in Firely Server and can be viewed on the administration endpoint API or in Simplifier under 
`Firely Server Definitions - Access Policy (R4) <https://simplifier.net/Vonk-ResourcesR4/~resources?text=access&fhirVersion=R4&sortBy=RankScore_desc>`_ .
The *AccessPolicyDefinition* resource controls the scopes which are permissible. 
The *AccessPolicy* resource contains the references to Patient, Group, Practitioner, PractitionerRole, Person, RelatedPerson and Device for which the AccessPolicyDefinition applies.
If a reference (Patient, Group, ...) is not referenced by an AccessPolicy, the requested scopes are granted without filtering.

.. note::

    This functionality is a restrictive filter; it does not give additional scope permissions that were not requested by the client. 
    All previous documentation about compartments and filter argument settings still applies.

Filter Logic Examples:
^^^^^^^^^^^^^^^^^^^^^^
+-------------------------------+-------------------------------+------------------------------+
| Requested Scopes              | AccessPolicy Scopes           | Resulting Scope Permissions  |
+===============================+===============================+==============================+
| ``user/Patient.cr``           | ``user/Patient.r``            | ``user/Patient.r``           |
+-------------------------------+-------------------------------+------------------------------+
| ``user/Patient.*``            | ``user/Patient.r``            | ``user/Patient.r``           |
+-------------------------------+-------------------------------+------------------------------+
| ``user/Patient.c``            | ``user/Patient.r``            | ``no permission``            |
+-------------------------------+-------------------------------+------------------------------+
| ``user/*.r``                  | ``user/Patient.*``            | ``user/Patient.r``           |
+-------------------------------+-------------------------------+------------------------------+
| ``user/Device.cr,``           | ``user/Device.r,``            | ``user/Device.r``            |
|                               |                               |                              |
| ``user/DiagnosticReport.c``   | ``user/DiagnosticReport.r,``  |                              |
|                               |                               |                              |
|                               | ``user/Patient.r``            |                              |
+-------------------------------+-------------------------------+------------------------------+
| ``user/Device.crd,``          | ``user/*.cru``                | ``user/Device.cr,``          |
|                               |                               |                              |
| ``user/DiagnosticReport.r,``  |                               | ``user/DiagnosticReport.r``  |
|                               |                               |                              |
| ``user/Patient.d``            |                               |                              |
+-------------------------------+-------------------------------+------------------------------+

Policy Creation Example:
^^^^^^^^^^^^^^^^^^^^^^^^

1. Create an AccessPolicyDefinition resource on the administrative endpoint:

``PUT {{BASE_URL}}/administration/AccessPolicyDefinition/UserReadsPatients``

::

    {
        "resourceType": "AccessPolicyDefinition",
        "id": "UserReadsPatients",
        "url": "https://fire.ly/fhir/AccessPolicyDefinition/UserReadsPatients",
        "version": "1.0.0",
        "name": "UserReadsPatients",
        "status": "active",
        "policy": [
            {
                "type": {
                    "code": "smart-v1"
                },
                "restriction": [
                    "user/Patient.read",
                    "user/Observation.read"
                ]
            },
            {
                "type": {
                    "code": "smart-v2"
                },
                "restriction": [
                    "user/Patient.rs",
                    "user/Observation.rs",
                ]
            }
        ]
    }


2. Next create an AccessPolicy resource on the repository endpoint:

``PUT {{BASE_URL}}/AccessPolicy/AmbulatoryPractitioners``

::

    {
        "resourceType": "AccessPolicy",
        "id": "AmbulatoryPractitioners",
        "instantiatesCanonical": "https://fire.ly/fhir/AccessPolicyDefinition/UserReadsPatients",
        "subject": [
            {
                "reference": "Practitioner/Alice"
            },
            {
                "reference": "Practitioner/Bob"
            }
        ]
    }

.. note::

    Multiple AccessPolicy resources containing the same Reference will be combined. In the above example if the user Alice is found in another policy with ``user/Patient.c``, the resulting permission will be ``user/Patient.crs``
