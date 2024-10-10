.. _feature_accesscontrol_compartment:

Tokens and Compartments
=======================

:ref:`feature_accesscontrol_config` outlines that SMART scopes can be defined on multiple levels: patient, user, and system. For each of these levels, different workflows apply to how the access control engine in Firely Server evaluates these scopes internally.
The following page gives context about the most important terms that are relevant for the decision if a user or system is granted access.

Launch Context and Patient Compartment
--------------------------------------

In FHIR a `CompartmentDefinition <http://www.hl7.org/implement/standards/fhir/compartmentdefinition.html>`_ defines a set of resources 'around' a focus resource.
The `Patient CompartmentDefinition <https://hl7.org/implement/standards/fhir/compartmentdefinition-patient.html>`_ in the FHIR specification defines a set of resources belonging to the patients' record. 'Belonging to' is expressed in terms of reference search parameters. As an example: One of the resource types in the Patient compartment is Observation. Its params are subject and performer, so any Observation resource is in the compartment of a specific Patient if that Patient is either the subject or the performer of the Observation.

FHIR defines CompartmentDefinitions for Patient, Encounter, RelatedPerson, Practitioner and Device. Although Firely Server is functionally not limited to these five, the specification does not allow you to define your own. For authorization purposes, Firely Server will only evaluate the Patient compartment. Other types of compartment have not seen enough real world usage to be used safely in a production scenario.

In SMART on FHIR, the ``launch/patient`` claim expresses that the session is launched in the context of a specific patient. Then the ``patient`` claim specifies which Patient. Firely Server translates this to the correct patient compartment, and limits access only to that compartment. How the value of the claim is matched against a Patient is detailed in the paragraph on Patient-level scopes below. 

It is up to the authorization server and the EHR to establish and exchange the launch context ahead of time, so the authorization server can include it in the access token. See :ref:`firely_auth_endpoints_launchcontext` on how this can be achieved in Firely Auth.

.. note::
    Before version 6.0, Firely Server allowed configuring other compartments than Patient in the SmartOptions. This is no longer supported. If you have configured this, you will need to adjust the configuration to only specify a filter on the Patient compartment. 

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

.. warning:: Firely Server will not enforce any access control for resource types that are not listed in the applicable compartment definition. Some compartment definitions do not include crucial resource types like 'Patient', i.e. all resources of this type regardless of any claims in the access token will be returned if requested. Please use compartments other than Patient with caution! Additional custom access control, like :ref:`feature_accesscontrol_permissions` is highly recommended.

Patient-level scopes
--------------------
For Patient-level scopes the access control engine in Firely Server evaluates two types of authorization:

#. Type-Access: Is the user allowed to read or write resource(s) of a specific resourcetype?
#. Compartment: Is the data to be read or written within the current compartment (if any)?

As you may have noticed, Type-Access aligns with the concept of scopes, and Compartment aligns with the concept of launch context in SMART on FHIR.
Scopes are evaluated by checking if the application has been granted the correct rights to execute a certain CRUD operation.
The compartment access is evaluated by the ``PatientFilter`` defined in the SMART appsettings. See :ref:`feature_accesscontrol_config`::

    "PatientFilter": "_id=#patient#" //ALlow access to the compartment of the Patient that has an id matching the value of the 'patient' claim

For example, the authorization server provides a patient launch context with the value ``123``. This is to be interpreted by the application as "the user instructs the application to work in the context of the resource ``Patient/123``".
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

.. note:: A conditional create, update or delete (see the `FHIR http specification <https://hl7.org/fhir/http.html>`_), requires read permissions on the condition. Therefore, ``user/*.write`` will usually require additional ``read`` scopes.

User-level scopes
-----------------

SMART on FHIR also defines scopes starting with ``user/`` instead of ``patient/``. If no patient-level scopes are present in an access token, a compartment is not enforced and not even evaluated.
But Firely Server will still apply the restrictions expressed in the user-level scopes: 

- It checks the syntax of the SMART on FHIR scopes within the access token. 
- It enforces that only allowed resources types are accessed and only allowed actions are executed.
- It enforces search arguments that may be part of a scope in SMART v2 syntax.
- It will evaluate AccessPolicies connected to the ``fhirUser``.

.. warning::
  Requests using a user-level scope are not limited to a pre-defined context, e.g. a Patient compartment. Therefore all matching resources are returned to the client. It is highly advised to implement additional security measures using a custom plugin or :ref:`access policies <feature_accesscontrol_permissions>`, e.g. by enforcing a certain Practitioner or Encounter context.

System-level scopes
-------------------

System-level scopes - starting with ``system/`` - are evaluated equally to user-level scopes.
