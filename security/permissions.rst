.. _feature_accesscontrol_permissions:

Permissions (AccessPolicy)
--------------------------

.. note::

  The features described on this page are available in the following :ref:`Firely Server editions <vonk_overview>`:

  * Firely Scale - 🌍 / 🇺🇸
  * Firely CMS Compliance - 🇺🇸

Firely Server supports enforcing custom permissions per user next to the granted authorization as part of a SMART on FHIR based access token.
In general, the access token represents the set of scopes that a client (e.g. a SMART app) is allowed to request. These scopes may not overlap with the set of scopes that the user using the app is allowed to request. Firely Server can therefore filter the granted access scopes for a authenticated user by using a custom AccessPolicy resource. 

The access policy decisions are based on HL7 SMART on FHIR scopes, both SMART on FHIR v1 and v2 scopes are supported.
The structure definitions defining AccessPolicyDefinition and AccessPolicy are preloaded in Firely Server and can be viewed on the administration endpoint API or in Simplifier under 
`Firely Server Definitions - Access Policy (R4) <https://simplifier.net/Vonk-ResourcesR4/~resources?text=access&fhirVersion=R4&sortBy=RankScore_desc>`_ .
The *AccessPolicyDefinition* resource controls the scopes which are permissible. 
The *AccessPolicy* resource contains the references to Patient, Group, Practitioner, PractitionerRole, Person, RelatedPerson and Device for which the AccessPolicyDefinition applies.
If a user (Patient, Practitioner, etc. - identified by the ``fhirUser`` claim) is not referenced by any AccessPolicy, the requested scopes are granted without filtering.

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

    Multiple AccessPolicy resources containing the same Reference will be combined. In the above example if the user Alice is found in another policy with ``user/Patient.c``, the resulting permission will be ``user/Patient.crs``.
    
    
    (**IMPORTANT**) AccessPolicy resources cannot be accessed or edited with a wildcard resource scope (e.g., ``system/*.*``). To manage AccessPolicy resources, use specific scopes such as ``system/AccessPolicy.*``.

3. Any request where the 'fhirUser' claim within an access token corresponds to any subject listed in the AccessPolicy, will be filtered according to the AccessPolicyDefinition.

Search capabilities:
^^^^^^^^^^^^^^^^^^^^
The ``restriction`` for ``type.code = "smart-v2"`` support search capabilites, and placeholders within that search statement. The placeholder will have the following format: ``#placeholder#``, and will be replaced by a claim with the same name of the placeholder that is provided in the authorization token. If the placeholder claim is not provided, it will result in a unauthorized result. Example restrictions:

* ``user/Observation.rs?category=laboratory``: the user is allowed to read and search Observation resources with a category element containing the code "laboratory". When the scope ``user/Observation.rs`` was requested in the authorization, the search filter will get added to read/search queries.
* ``system/Patient.rs?_has:Group:member:identifier=#tenant#``: there must be a claim in the authorization code called ``tenant``, and the value of this claim will replace the ``#tenant#`` placeholder in the restriction. In this case the client can only read/search ``Patients`` who are in the ``Group`` with the identifier specified in the ``tenant`` claim.
