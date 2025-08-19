.. _feature_accesscontrol_compartment:

Tokens and Compartments
=======================

:ref:`feature_accesscontrol_authorization` outlines that SMART scopes can be defined on multiple levels: patient, user, and system. For each of these levels, different workflows apply to how the access control engine in Firely Server evaluates these scopes internally.
The following page gives context about the most important terms that are relevant for the decision if a user or system is granted access.

Launch Context and Patient Compartment
--------------------------------------

Before exploring how patient-level scopes are enforced, it's essential to understand two key concepts that underpin access control in Firely Server: Launch Contexts and FHIR Compartments. Together, they determine how Firely Server interprets SMART on FHIR scopes and restricts access to data based on the context of the request.

In FHIR a `CompartmentDefinition <http://www.hl7.org/implement/standards/fhir/compartmentdefinition.html>`_ defines a group of resources associated with a specific focus resource.
The `Patient CompartmentDefinition <https://hl7.org/implement/standards/fhir/compartmentdefinition-patient.html>`_ defines which resources are considered part of a patient's health record. This is determined by *reference search parameters*. For example:

- The resource type ``Observation`` is part of the Patient compartment.
- Its compartment-defining parameters are ``subject`` and ``performer``.
- An ``Observation`` belongs to a given Patient’s compartment if that Patient is referenced in either the ``subject`` or the ``performer`` field.

FHIR includes predefined CompartmentDefinitions for five types of focus resources:

- ``Patient``
- ``Encounter``
- ``RelatedPerson``
- ``Practitioner``
- ``Device`` 

Although Firely Server is functionally not limited to these five, the specification does not allow you to define your own. For authorization purposes, Firely Server will only evaluate the Patient compartment. Other types of compartment have not seen enough real world usage to be used safely in a production scenario.
To enforce a Patient compartment, meaning to restrict all requests to resources that belong to a specific patient and prevent access to data associated with other patients, Firely Server requires a patient identifier or resource ID to be present in the access token. SMART on FHIR does not specify how this information should be communicated between the authorization server and the FHIR server.

Firely Server uses the ``patient`` claim in the access token to identify the relevant patient. This value can originate either from:

- an EHR launch context, such as launching an app from within a patient portal
- a standalone launch, where the ``launch/patient`` scope is requested and the patient is inferred from attributes of the authenticated user

It is the responsibility of the authorization server and the EHR to establish and exchange the launch context before issuing the access token, so that the ``patient`` claim can be included. See :ref:`firely_auth_endpoints_launchcontext` for how this can be implemented using Firely Auth.

Firely Server then maps the value of the ``patient`` claim to the appropriate Patient compartment and restricts access accordingly. The details of how the claim value is matched to a Patient resource are provided in the section on Patient-level scopes.

.. _accesstokens:

Tokens requirements
-------------------

Note that it is out-of-scope of the SMART on FHIR specification to define the format of the access token.
SMART on FHIR solely defines the scope and launch context syntax. From a client perspective an access token is fully opaque.

Firely Server is not bound to any specific authorization server, as long as the access token (JSON Web Token (JWT) or a Reference Token) contains a minimal set of information:

* the ``iss`` claim with the base url of the OAuth server
* the ``aud`` the same value you've entered in ``SmartAuthorizationOptions.Audience``
* the ``scope`` field with the scopes granted by this access token
* optionally, the compartment claim, if you'd like to limit this token to a certain compartment. For example, in case of Patient data access where the ``launch/patient`` scope is used, include the ``patient`` claim. Such a claim should be included by the authorization server if requested by using a context scope or launching a part of an EHR launch.

.. code-block::

   {
      "iss": "https://auth.fire.ly",
      "aud": "https://secure.server.fire.ly",
      "patient": "test",
      "fhirUser": "Patient/test",
      "jti": "7A331121A699254653BF184783ACA55F",
      "active": true,
      "scope": "patient/*.*"
      ...
   }

Patient-level scopes
--------------------
For patient-level scopes, the access control engine in Firely Server evaluates two types of authorization:

1. **Type access**: Determines whether the user is allowed to create, read, update, or delete resources of a specific resource type.
2. **Compartment access**: Determines whether the data being accessed falls within the current patient compartment.

Type access corresponds to the scopes granted to the application in the access token. These scopes define whether the application is permitted to perform specific CRUD operations on certain resource types.
Compartment access is evaluated using the ``PatientFilter`` configuration, which defines how the value of the ``patient`` claim is transformed into an implicit search argument. This argument is used to identify a single Patient resource, and all RESTful interactions are then restricted to that patient's compartment.
Scope evaluation ensures that the access token grants the appropriate rights for the requested operation. Compartment evaluation ensures that the request targets data belonging to the identified patient.

The ``PatientFilter`` is defined in the SMART authorization settings.  See :ref:`feature_accesscontrol_config`::

    "PatientFilter": "_id=#patient#" //ALlow access to the compartment of the Patient that has an id matching the value of the 'patient' claim

For example, the authorization server provides a patient claim with the value ``123``. This is to be interpreted by the application as "the user instructs the application to work in the context of the resource ``Patient/123``".
Firely Server internally forms a compartment around all resources that are linked to ``Patient/123`` according to the Patient CompartmentDefinition.

.. note::

  To enable access to resources outside the compartment, the client must request additional scopes for these resources specifically.

There may be cases where the logical id of the focus resource is not known to the authorization server. Let's assume it does know one of the identifiers of a Patient. The Filters in the :ref:`feature_accesscontrol_config` allow you to configure Firely Server to use the identifier search parameter as a filter instead of _id::

    "PatientFilter": "identifier=#patient#" //Allow access to the compartment of the Patient that has an identifier matching the value of the 'patient' claim

Please notice that it is possible that more than one Patient matches the filter. This is intended behaviour of Firely Server, and it is up to you to configure a search parameter that is guaranteed to have unique values for each Patient if you need that.
However, you can also take advantage of it and allow access only to the patients from a certain General Practitioner, of whom you happen to know the Identifier::

    "PatientFilter": "general-practitioner.identifier=#patient#" //Allow access to the compartments of patients that contain a reference to a matching Practitioner with an identifier containing the value of the 'patient' claim

In this example the claim is still called ``patient``, although it contains an Identifier of a General Practitioner. 
This is because the CompartmentDefinition is selected by matching its code to the name of the claim, regardless of the value the claim contains.

.. note::
   Any request is scoped to the patient compartment and requests are rejected if the patient claim is not provided in the access token.

.. _feature_accesscontrol_decisions:

Access Control Decisions for Patient-level scopes
-------------------------------------------------

In this paragraph we will explain how access control decisions are made for the various FHIR interactions. For the examples assume a Patient Compartment with identifier=123 as filter.
For the Type-Access decision, Firely Server will also take into account restrictions set by search arguments on the relevant SMART on FHIR v2 scopes, retrieved from the access token and any applicable AccessPolicyDefinitions.
These are not included in the examples, to keep those readable.

#. Search

   a. Direct search on compartment type

      :Request: ``GET [base]/Patient?name=fred``
      :Type-Access: User must have read access to Patient, otherwise HTTP Status Code 403 is returned. 
      :Compartment: If a Patient Compartment is active, the filter from it will be added to the search, e.g. ``GET [base]/Patient?name=fred&identifier=123``

   #. Search on type related to compartment

      :Request: ``GET [base]/Observation?code=x89``
      :Type-Access: User must have read access to Observation, otherwise HTTP Status Code 403 is returned. 
      :Compartment: If a Patient Compartment is active, the links from Observation to Patient will be added to the search. In pseudo code: ``GET [base]/Observation?code=x89& (subject:Patient.identifier=123 OR performer:Patient.identifier=123)``

   #. Search on type not related to compartment

      :Request: ``GET [base]/Organization``
      :Type-Access: User must have read access to Organization, otherwise HTTP Status Code 403 is returned. 
      :Compartment: No compartment is applicable to Organization, so no further filters are applied.

   #. Search with include outside the compartment

      :Request: ``GET [base]/Patient?_include=Patient:organization``
      :Type-Access: User must have read access to Patient and Organization, otherwise HTTP Status Code 403 is returned. If the user has read access to Organization, the _include is evaluated. Otherwise it is ignored.
      :Compartment: Is applied as in case 1.a.

   #. Search with chaining

      :Request: ``GET [base]/Patient?general-practitioner.identifier=123``
      :Type-Access: User must have read access to Patient, otherwise HTTP Status Code 403 is returned. If the user has read access to Practitioner, the search argument is evaluated. Otherwise a HTTP Status Code 403 is returned as well. If the chain has more than one link, read access is evaluated for every link in the chain. 
      :Compartment: Is applied as in case 1.a.

   #. Search with chaining into the compartment

      :Request: ``GET [base]/Patient?link:Patient.identifier=456``
      :Type-Access: User must have read access to Patient, otherwise HTTP Status Code 403 is returned.
      :Compartment: Is applied to both Patient and link. In pseudo code: ``GET [base]/Patient?link:(Patient.identifier=456&Patient.identifier=123)&identifier=123`` In this case there will probably be no results.

#. Read: Is evaluated as a Search, but implicitly you only specify the _type and _id search parameters.
#. VRead: If a user can Read the current version of the resource, he is allowed to get the requested version as well.
#. Create

   a. Create on the compartment type

      :Request: ``POST [base]/Patient``
      :Type-Access: User must have write access to Patient. Otherwise HTTP Status Code 403 is returned.
      :Compartment: A Search is performed as if the new Patient were in the database, like in case 1.a. If it matches the compartment filter, the create is allowed. Otherwise HTTP Status Code 403 is returned.

   #. Create on a type related to compartment

      :Request: ``POST [base]/Observation``
      :Type-Access: User must have write access to Observation. Otherwise HTTP Status Code 403 is returned. User must also have read access to Patient, in order to evaluate the Compartment.
      :Compartment: A Search is performed as if the new Observation were in the database, like in case 1.b. If it matches the compartment filter, the create is allowed. Otherwise HTTP Status Code 403 is returned.

   #. Create on a type not related to compartment

      :Request: ``POST [base]/Organization``
      :Type-Access: User must have write access to Organization. Otherwise HTTP Status Code 403 is returned.
      :Compartment: Is not evaluated.

#. Update

   a. Update on the compartment type

      :Request: ``PUT [base]/Patient/123``
      :Type-Access: User must have write access *and* read access to Patient, otherwise HTTP Status Code 403 is returned.
      :Compartment: User should be allowed to Read Patient/123 and Create the Patient provided in the body. Then Update is allowed.

   #. Update on a type related to compartment

      :Request: ``PUT [base]/Observation/xyz``
      :Type-Access: User must have write access to Observation, and read access to both Observation and Patient (the latter to evaluate the compartment)
      :Compartment: User should be allowed to Read Observation/123 and Create the Observation provided in the body. Then Update is allowed.

#. Delete: Allowed if the user can Read the current version of the resource, and has write access to the type of resource.
#. History: Allowed on the resources that the user is allowed to Read the current versions of (although it is theoretically possible that an older version would not match the compartment). 

.. note:: A conditional create, update or delete (see the `FHIR http specification <https://hl7.org/fhir/http.html>`_), requires read permissions on the condition. Therefore, ``patient/*.write`` will usually require additional ``read`` scopes.

User-level scopes
-----------------

SMART on FHIR also defines scopes starting with ``user/`` instead of ``patient/``. If no patient-level scopes are present in an access token, a compartment is not enforced and not even evaluated.
But Firely Server will still apply the restrictions expressed in the user-level scopes: 

- It checks the syntax of the SMART on FHIR scopes within the access token. 
- It enforces that only allowed resources types are accessed and only allowed actions are executed.
- It enforces search arguments that may be part of a scope in SMART v2 syntax.

.. warning::
  Requests using a user-level scope are not limited to a pre-defined context, e.g. a Patient compartment. Therefore all matching resources are returned to the client. It is highly advised to implement additional security measures using a custom plugin or :ref:`access policies <feature_accesscontrol_permissions>`, e.g. by enforcing a certain Practitioner or Encounter context.

The SMART on FHIR specification defines that the authorization server can communicate to the application which user (for example, a Practitioner) logged in during the authorization flow. This is exposed as the ``fhirUser`` claim in the access token response document.
Note that this refers to the JSON document returned from the token endpoint. Firely Auth will also embed this claim in the access token itself. Based on this verified claim, Firely Server identifies the user and enforces the configured ``AccessPolicy`` for that user.

.. _system_level_scopes:  

System-level scopes
-------------------

System-level scopes - starting with ``system/`` - are evaluated equally to user-level scopes.
When integrating backend services using system-level scopes, AccessPolicies which are bound to a fhirUser of type 'Device' can be used. Firely Server allows a Device resource to represent a fhirUser even if it's not defined in the SMART on FHIR standard.
If an access token with such a fhirUser claim is sent as part of a request, Firely Server enforces that at least one AccessPolicy is present for the corresponding fhirUser. This AccessPolicy may be bound to the same scopes that the backend service is allowed to request from the authorization server. However, additional restrictions can be applied via constraining the applicable scopes.
This reduces the risk that backend services are by default allowed more access then necessary or allowed. 
