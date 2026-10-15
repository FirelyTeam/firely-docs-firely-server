.. _feature_terminology:

Terminology services
====================

.. note::

  The features described on this page are available in **all** :ref:`Firely Server editions <vonk_overview>`.

Terminologies give the codes in FHIR resources their meaning. A code such as ``44054006`` only tells a receiving system that a patient has type 2 diabetes because both sides use the same CodeSystem, and a profile can only require "a code for a blood pressure measurement" because a ValueSet lists which codes qualify.
Firely Server supports the FHIR terminology operations and validates resources against the terminology bindings in their profiles. It answers terminology requests with its own **Local Terminology Service**, which uses the CodeSystems, ValueSets and ConceptMaps in the Administration database, and can pass requests on to one or more external FHIR terminology servers, the **Remote Terminology Services**. For each CodeSystem and ValueSet you can set which service is preferred, and Firely Server selects the service transparently (see :ref:`feature_terminologyoptions`).

The Local Terminology Service works well for CodeSystems and ValueSets of a manageable size, such as the ones in the FHIR specification and in implementation guides. Large terminologies such as SNOMED CT and LOINC cannot be loaded into the Administration database. For those, use :ref:`Advanced Terminology with Conformance Archives (CAR files) <feature_advanced_terminology>`, or a Remote Terminology Service.

----

Terminology Use Cases
^^^^^^^^^^^^^^^^^^^^^

#. Validating resources against their bindings

	When Firely Server validates a resource, with ``$validate`` or with :ref:`feature_prevalidation`, it checks the codes in the resource against the ValueSets the profile binds them to. Behind the scenes this uses the same terminology services as the operations below, so a code from a large terminology can only be checked if a CAR file or a Remote Terminology Service covers it.

#. Checking codes at the point of entry

	A client can check a single code before it stores a resource, for example whether a code entered in a form is allowed for a field, or whether a code exists at all and what its display is.

#. Filling pick lists

	User interfaces need the list of codes a user may choose from. Expanding a ValueSet gives that list, including the displays to show.

#. Reasoning over hierarchies

	Many questions in clinical logic and analytics are about hierarchies: "is this diagnosis a kind of diabetes?" or "is this procedure a subtype of surgery?". The subsumption test answers such a question for a pair of codes.

#. Mapping between code systems

	Data often arrives coded in a local or national code system while reporting or exchange requires a standard one. Translating codes with a ConceptMap maps them from one system to the other.

----

Terminology Operations in Firely Server
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Some terminology operations are supported natively: the Local Terminology Service answers them, and for SNOMED CT, LOINC and ICD-10-CM, Advanced Terminology answers part of them from CAR files. Others are not implemented by Firely Server itself, and only work when a Remote Terminology Service that supports them is configured.

Supported natively
------------------

* ValueSet/$validate-code

	:Purpose: Checks whether a code is a member of a ValueSet.
	:Use Case: Checking a code entered in a form or received from another system against the ValueSet bound to that element. Validation of resources relies on the same check.
	:Input: A ValueSet (canonical url, id or the resource itself) and a code with its system, a Coding or a CodeableConcept.
	:Output: A Parameters resource with ``result`` (true or false), and a ``message`` and the ``display`` where available.

	See :ref:`terminology_valueset_validate_code` for the supported parameters. With Advanced Terminology, a ValueSet that includes concepts from a CAR-backed CodeSystem can be checked as well.

* ValueSet/$expand

	:Purpose: Lists the codes that a ValueSet contains.
	:Use Case: Filling pick lists and drop-downs in a user interface, or getting the full list of codes for a query or a report.
	:Input: A ValueSet (canonical url, id or the resource itself).
	:Output: The ValueSet with an ``expansion`` listing its codes.

	See :ref:`terminology_valueset_expand`. The Local Terminology Service expands ValueSets up to ``MaxExpansionSize`` concepts and does not support filtering or paging. Advanced Terminology does not expand ValueSets.

* CodeSystem/$lookup

	:Purpose: Returns the details of a single code: its display, its designations and its properties.
	:Use Case: Showing the display of a code to a user, or reading a property of a concept, such as whether it is inactive.
	:Input: A code and its system, or a Coding.
	:Output: A Parameters resource with the name of the CodeSystem, the display of the code, and the requested properties and designations.

	See :ref:`terminology_codesystem_lookup`. Also supported by Advanced Terminology.

* CodeSystem/$validate-code

	:Purpose: Checks whether a code exists in a CodeSystem.
	:Use Case: Checking that a code is valid at all, independent of a ValueSet, for example when importing data.
	:Input: A CodeSystem (canonical url, id or the resource itself) and a code, a Coding or a CodeableConcept.
	:Output: A Parameters resource with ``result`` (true or false), and a ``message`` and the ``display`` where available.

	See :ref:`terminology_codesystem_validate_code`. Also supported by Advanced Terminology.

* CodeSystem/$subsumes

	:Purpose: Tests how two codes of the same CodeSystem are related in its hierarchy.
	:Use Case: Answering "is code A a kind of code B?" in clinical logic, decision support or analytics, without expanding a whole ValueSet.
	:Input: Two codes (or Codings) and their system.
	:Output: A Parameters resource with the outcome: ``equivalent``, ``subsumes``, ``subsumed-by`` or ``not-subsumed``.

	See :ref:`terminology_codesystem_subsumes`. Also supported by Advanced Terminology.

* CodeSystem/$find-matches (named $compose in STU3)

	:Purpose: Finds the codes in a CodeSystem that have a given set of property values.
	:Use Case: Finding the code for a concept that is described by its properties rather than by its code.
	:Input: A system, and the properties with the values to match.
	:Output: A Parameters resource listing the matching codes.

	See :ref:`terminology_codesystem_find_matches`. Not supported by Advanced Terminology.

Only available through a Remote Terminology Service
---------------------------------------------------

Firely Server accepts the following operations on its endpoint, but does not implement them. It passes them on to a Remote Terminology Service that supports them. Without such a service, these operations are not available.

* ConceptMap/$translate

	:Purpose: Translates a code from one code system to another, using a ConceptMap.
	:Use Case: Mapping local or national codes to the standard codes required for exchange or reporting.
	:Input: A ConceptMap (canonical url, id or the resource itself), or a source and target, and the code to translate.
	:Output: A Parameters resource with ``result``, and the matching target codes with their relationship to the source code.

	See :ref:`terminology_conceptmap_translate`.

* $closure

	:Purpose: Maintains a closure table: the subsumption relationships between a set of codes that a client is interested in.
	:Use Case: A client that stores coded data in its own database and wants to query it by hierarchy ("all records with a kind of diabetes") without calling the terminology server for every record.
	:Input: The name of the closure table, and the new concepts to add to it.
	:Output: A ConceptMap with the new relationships between the concepts in the table.

	See :ref:`terminology_conceptmap_closure`.

All terminology operations are available on the regular FHIR endpoint and on the administration endpoint. See :ref:`feature_terminologyintegration` for the levels, parameters and configuration of each operation.

The terminology operations can be invoked for different FHIR versions as specified in :ref:`feature_multiversion`.

.. _feature_terminologyintegration:

Terminology Integration
^^^^^^^^^^^^^^^^^^^^^^^

In earlier versions of Firely Server, local terminology services were separately configured from so-called Terminology Integration. These are now merged.

Operations
----------

Every operation below is available on the regular FHIR endpoint (``<firely-server-endpoint>/...``) and on the administration endpoint (``<firely-server-endpoint>/administration/...``), on the levels listed for it. Which of these levels are actually open is set per endpoint in ``Operations`` and ``Administration:Operations`` (see :ref:`enabling the operations <feature_terminology_operations_config>`). On the regular endpoint, CodeSystems, ValueSets and ConceptMaps are still resolved from the Administration database.

Firely Server decides per request whether the Local Terminology Service or a Remote Terminology Service handles it (see :ref:`feature_terminologyoptions`). The parameter lists below are those of the Local Terminology Service. It rejects any other parameter with the message "The input parameter(s) ... are currently not supported". A Remote Terminology Service receives the parameters of the request and decides itself which ones it supports. Parameters of a complex type, such as ``coding`` or ``valueSet``, can only be passed with ``POST``.

.. _terminology_valueset_validate_code:

ValueSet $validate-code
-----------------------

:definition: https://hl7.org/fhir/valueset-operation-validate-code.html
:notes: 
   * Available on the type level ``ValueSet/$validate-code`` and the instance level ``ValueSet/<id>/$validate-code``.
   * The Local Terminology Service supports the parameters url, valueSetVersion, valueSet, code, system, display, coding, codeableConcept, abstract, inferSystem and context.
   * Pass the ValueSet either by reference (url, optionally with valueSetVersion, or the id on the instance level) or as a resource (the valueSet parameter or the request body), not both. A request that has both is rejected with ``400 Bad Request`` and "Ambiguous call. ValueSet information present both in url and in the valueSet parameter or request body".
   * Both ``GET`` and ``POST`` interactions are available. 

.. _terminology_valueset_expand:

ValueSet $expand
----------------

:definition: https://hl7.org/fhir/valueset-operation-expand.html
:notes:
   * Available on the type level ``ValueSet/$expand`` and the instance level ``ValueSet/<id>/$expand``.
   * The Local Terminology Service supports the parameters url, valueSet and includeDesignations. It does not support valueSetVersion or any of the other expansion parameters, such as filter, count or offset. A Remote Terminology Service may support them.
   * Pass the ValueSet either by reference (url, or the id on the instance level) or as a resource (the valueSet parameter or the request body), not both. A request that has both is rejected with ``400 Bad Request`` and "Ambiguous call. ValueSet information present both in url and in the valueSet parameter or request body".
   * The size of an expansion by the Local Terminology Service is limited by ``MaxExpansionSize`` (see :ref:`feature_terminologyoptions`).
   * Both ``GET`` and ``POST`` interactions are available. 

.. _terminology_codesystem_lookup:

CodeSystem $lookup
------------------

:definition: https://hl7.org/fhir/codesystem-operation-lookup.html
:notes:
   * Available on the type level ``CodeSystem/$lookup`` and the instance level ``CodeSystem/<id>/$lookup``. On the instance level the id is not used: pass the CodeSystem with system or coding as on the type level.
   * The Local Terminology Service supports the parameters code, system, version, coding and property. It does not support date.
   * Pass the concept either as code and system or as coding, not both. A request with both code and ``coding.code``, or with both system and ``coding.system``, is rejected with "Ambiguous call. code/system was provided in both code/system parameters and coding parameter".
   * Both ``GET`` and ``POST`` interactions are available. 

.. _terminology_codesystem_validate_code:

CodeSystem $validate-code
-------------------------

:definition: https://hl7.org/fhir/codesystem-operation-validate-code.html
:notes:
   * Available on the type level ``CodeSystem/$validate-code`` and the instance level ``CodeSystem/<id>/$validate-code``. On the instance level the CodeSystem with that id is used, unless url is given.
   * The Local Terminology Service supports the parameters url, version, codeSystem, code, display, coding, codeableConcept, abstract and displayLanguage.
   * Both ``GET`` and ``POST`` interactions are available. 

.. _terminology_codesystem_find_matches:

CodeSystem $find-matches / $compose
-----------------------------------

:definition: https://hl7.org/fhir/codesystem-operation-find-matches.html
:notes:
   * Available on the type level ``CodeSystem/$find-matches`` and the instance level ``CodeSystem/<id>/$find-matches``. On the instance level the CodeSystem with that id is used, unless system is given.
   * The Local Terminology Service supports the parameters system, version, exact and property (with the parts code and value).
   * property can only be passed with ``POST``. Both ``GET`` and ``POST`` interactions are available. 
   * $find-matches was named $compose in FHIR STU3. The operation is supported with both names. For STU3, the terminology service must list ``Compose`` in its ``SupportedInteractions`` (see :ref:`feature_terminologyoptions`), and ``$compose`` must be enabled in the ``Operations`` section of the endpoint. The default settings of the Local Terminology Service only list ``FindMatches``.

.. _terminology_codesystem_subsumes:

CodeSystem $subsumes
--------------------

:definition: https://hl7.org/fhir/codesystem-operation-subsumes.html
:notes:
   * Available on the type level ``CodeSystem/$subsumes`` and the instance level ``CodeSystem/<id>/$subsumes``. On the instance level the CodeSystem with that id is used, unless system is given.
   * The Local Terminology Service supports the parameters codeA, codeB, codingA, codingB, system and version.
   * Both ``GET`` and ``POST`` interactions are available. 

.. _terminology_conceptmap_closure:

ConceptMap $closure
--------------------

:definition: https://hl7.org/fhir/conceptmap-operation-closure.html
:notes:
   * Available on the system level ``$closure``.
   * The Local Terminology Service does not support this operation. It is passed on to a Remote Terminology Service supporting it, and supports any parameters that the remote service supports.
   * Only ``POST`` interactions are available. 

.. _terminology_conceptmap_translate:

ConceptMap $translate
---------------------

:definition: https://hl7.org/fhir/conceptmap-operation-translate.html
:notes:
   * Available on the type level ``ConceptMap/$translate`` and the instance level ``ConceptMap/<id>/$translate``.
   * The Local Terminology Service does not support this operation. It is passed on to a Remote Terminology Service supporting it. With ``GET``, the parameters url, conceptMapVersion, code, system, version, source, target, targetsystem and reverse are passed on; with ``POST`` the remote service decides which parameters it supports.
   * Pass the ConceptMap either by reference (url, or the id on the instance level) or as a resource (the conceptMap parameter or the request body), not both. A request that has both is rejected with ``400 Bad Request`` and "Ambiguous call".
   * Both ``GET`` and ``POST`` interactions are available. 


Configuration
-------------

Pipeline
^^^^^^^^

Make sure to add the ``Vonk.Plugins.Terminology`` plugin to the PipelineOptions in appsettings in order to make use of the terminology operations.
Additionally, to the "/administration" pipeline, ``Vonk.Plugins.Terminology`` can be used on the regular FHIR pipeline "/". Please note that in this case, CodeSystems and ValueSets are resolved from the Administration repository when executing a terminology operation and the corresponding resource is not provided as part of the request as a parameter.

.. code-block:: JavaScript

    "PipelineOptions": {
        "PluginDirectory": "./plugins",
        "Branches": [
          {
            "Path": "/",
            "Include": [..]
          },
          {
          "Path": "/administration",
          "Include": [
            "Vonk.Core",
              "Vonk.Fhir.R3",
              "Vonk.Fhir.R4",
              "Vonk.Administration",
              ...
              "Vonk.Plugins.Terminology"
            ],
            "Exclude": [
              "Vonk.Subscriptions.Administration"
            ]
          }, ...etc...

          
To include or exclude individual operations in the pipeline, see the available plugins under :ref:`vonk_plugins_terminology`.

.. _feature_terminology_operations_config:

Also make sure that the terminology operations are enabled in the ``Operations`` section, on the levels on which you want to use them (see :ref:`disable_interactions`). The default settings enable all of them on the regular FHIR endpoint, for example::

   "Operations": {
      "$validate-code": {
         "Name": "$validate-code",
         "Level": [ "Instance", "Type" ],
         "Enabled": true,
         "RequireAuthorization": "WhenAuthEnabled",
         "RequireTenant": "Never"
      },
      "$closure": {
         "Name": "$closure",
         "Level": [ "System" ],
         "Enabled": true,
         "RequireAuthorization": "WhenAuthEnabled",
         "RequireTenant": "Never"
      },
      // ... likewise for $expand, $lookup, $find-matches, $compose, $translate and $subsumes
   },

The operations on the administration endpoint are configured in ``Administration:Operations``. Set ``NetworkProtected`` to ``true`` to limit an operation to the IP addresses in ``Administration:AllowedNetworks`` (see :ref:`configure_administration_access`)::

   "Administration": {
      "Operations": {
         "$validate-code": {
            "Name": "$validate-code",
            "Level": [ "Instance", "Type" ],
            "Enabled": true,
            "NetworkProtected": true
         },
         // ... likewise for $expand, $lookup, $find-matches, $translate, $subsumes and $closure
      },
      "AllowedNetworks": [ "127.0.0.1", "::1" ] // i.e.: ["127.0.0.1", "::1" (ipv6 localhost), "10.1.50.0/24", "10.5.3.0/24", "31.161.91.98"]
   },

In the default settings of the administration endpoint, ``$lookup`` and ``$subsumes`` are enabled on the type level only, and ``$compose`` is not enabled. Add ``Instance`` to their ``Level``, or add a ``$compose`` entry, to use them there.

.. note::

   The top-level ``SupportedInteractions`` section and the ``Administration:Security:AllowedNetworks`` and ``Administration:Security:OperationsToBeSecured`` settings of Firely Server versions before 6.0 are no longer supported. If one of them is present in your settings, Firely Server does not start. See :ref:`disable_interactions` for the migration to the ``Operations`` section.

.. _feature_terminologyoptions:

Options
^^^^^^^

You can enable the integration with one or more external terminology services by setting the required options in the appsettings file. There is a block for the Local Terminology Service and one for each Remote Terminology Service.

For each terminology service you can set the following options:

    :Order: The order of the terminology service, or the priority. If multiple Terminology services could be used for a request, Firely Server will use the priority to select a service. Terminology services are arranged in a ascending order: so 1 will be selected over 2.
    :PreferredSystems: If a request is directed at a specific code system, Firely Server will choose this terminology server over other available services. A system matches one of the preferred systems if the system starts with the preferred system. So ``http://loinc.org`` will match any CodeSystem or ValueSet with a canonical that starts with that url.  
    :SupportedInteractions: The operations supported by the terminology service. Firely Server will only select this service if the operation is in this list. Valid values::

       "ValueSetValidateCode"
       "CodeSystemValidateCode"
       "Expand"
       "FindMatches" / "Compose"
       "Lookup"
       "Translate"
       "Subsumes"
       "Closure"

    :SupportedInformationModels: The FHIR versions supported by the terminology service. Valid values::

       "Fhir3.0" 
       "Fhir4.0" 
       "Fhir5.0"

    :Endpoint: The endpoint url where Firely Server can redirect the requests to.
    :RemoteTerminologyAuthentication: How Firely Server authenticates to the terminology service. Valid values:

       * ``Off`` (default): no credentials are sent, even if ``Username``, ``Password`` or the OAuth2 settings below are filled in.
       * ``Basic``: Basic Authentication with ``Username`` and ``Password``.
       * ``JwtClientCredentials``: an OAuth2 ``client_credentials`` flow with ``ClientId``, ``ClientSecret``, ``Scopes`` and ``TokenEndpoint``.

       If a setting that the chosen value requires is empty, Firely Server does not start.
    :Username: If ``RemoteTerminologyAuthentication`` is ``Basic``, the username to authenticate with.
    :Password: If ``RemoteTerminologyAuthentication`` is ``Basic``, the password to authenticate with.
    :ClientId: If ``RemoteTerminologyAuthentication`` is ``JwtClientCredentials``, the client_id that Firely Server uses to request a token.
    :ClientSecret: If ``RemoteTerminologyAuthentication`` is ``JwtClientCredentials``, the shared client secret that Firely Server uses to request a token.
    :Scopes: If ``RemoteTerminologyAuthentication`` is ``JwtClientCredentials``, the scopes requested by Firely Server.
    :TokenEndpoint: If ``RemoteTerminologyAuthentication`` is ``JwtClientCredentials``, the url of the token endpoint of the authorization server that issues the tokens.
    :MediaType: Default Media-Type that should be used for serialization of the Parameters resources forwarded to the external terminology servie

Notes:

* The Endpoint, Username and Password settings are not valid for the Local Terminology Server, just for the Remote services.
* If a Remote Terminology Service has different endpoints for different FHIR versions, configure each endpoint separately.
* The ``SupportedInformationModels`` cannot be broader than the corresponding ``Fhir.Rx`` plugins configured in the PipelineOptions.
* With ``JwtClientCredentials``, Firely Server automatically requests JWT tokens from the ``TokenEndpoint`` and ensures that a valid token is always submitted as part of the request to the external terminology service.

A sample Terminology section in the appsettings can look like this:

.. code-block:: JavaScript

   "Terminology": {
      "MaxExpansionSize": 650,
      "LocalTerminologyService": {
         "Order": 10,
         "PreferredSystems": [ "http://hl7.org/fhir" ],
         "SupportedInteractions": [ "ValueSetValidateCode", "Expand" ],
         "SupportedInformationModels": [ "Fhir3.0", "Fhir4.0", "Fhir5.0" ]
      }, 
      "RemoteTerminologyServices": [
      {
         "Order": 20,
         "PreferredSystems": [ "http://snomed.info/sct" ],
         "SupportedInteractions": [ "ValueSetValidateCode", "Expand", "Translate", "Subsumes", "Closure" ],
         "SupportedInformationModels": [ "Fhir4.0" ],
         "Endpoint": "https://r4.ontoserver.csiro.au/fhir/",
         "MediaType": "application/fhir+xml"
      },
      {
         "Order": 30,
         "PreferredSystems": [ "http://loinc.org" ],
         "SupportedInteractions": [ "ValueSetValidateCode", "Expand", "Translate" ],
         "SupportedInformationModels": [ "Fhir3.0", "Fhir4.0" ],
         "Endpoint": "https://fhir.loinc.org/",
         "RemoteTerminologyAuthentication": "Basic",
         "Username": "",
         "Password": ""
      }
      ]
   },

This means if you execute a terminology operation request, Firely Server will check whether the request is correct, redirect it to the preferred terminology service and finally return the result.

Additionally to the remote and local terminology services, you can configure the maximum number of concepts that are allowed to be included in a local ValueSet expansion (MaxExpansionSize). ValueSets stored in the local administration database larger than the configured setting will not be expanded, hence they cannot be used for $validate-code, $validate or $expand.

License
-------

The Terminology plugin itself is licensed with the license token ``http://fire.ly/vonk/plugins/terminology``.

When you configure Remote Terminology Services it is your responsibility to check whether you are licensed to use those services.

.. _feature_advanced_terminology:

Advanced Terminology with Conformance Archives (CAR files)
==========================================================

.. note::

   The features described in this section require the **Advanced Terminology** license token ``http://fire.ly/advanced-terminology``, which is separate from the base terminology license.

The local terminology service built into Firely Server works well for simple ValueSets and CodeSystems such as those shipped with the FHIR specification. Large terminologies with rich hierarchies cannot be represented as a FHIR CodeSystem resource in the Administration database: they are too large, and checking whether one concept is a descendant of another needs dedicated logic.

With **Conformance Archive (CAR) files**, Firely Server supports these terminologies locally, without an external terminology server:

.. list-table::
   :header-rows: 1
   :widths: 30 70

   * - Terminology
     - CodeSystem url
   * - SNOMED CT, US Edition
     - ``http://snomed.info/sct``
   * - LOINC
     - ``http://loinc.org``
   * - ICD-10-CM
     - ``http://hl7.org/fhir/sid/icd-10-cm``

SNOMED CT uses the same CodeSystem url for every edition. Use ``http://snomed.info/sct`` as the system; the edition URI ``http://snomed.info/sct/731000124108`` is not a system and is not recognized. The edition is only part of the ``version``.

A CAR file holds the complete set of concepts of one terminology, together with their relationships and properties, in a format that Firely Server reads from the local filesystem. Firely provides pre-built CAR files for these terminologies on request; please :ref:`contact us <vonk-contact>`.

How it works
^^^^^^^^^^^^

The Advanced Terminology plugin (``Vonk.Plugins.Terminology.AdvancedTerminologyConfiguration``) is part of ``Vonk.Plugins.Terminology``, so it is in the default pipeline. It is loaded when your license contains the Advanced Terminology token, and then changes two things:

#. If ``Terminology:ConformanceArchives`` lists at least one CAR file, it adds a **CAR terminology service** next to the local and remote terminology services.
#. It replaces the routing of ``ValueSet/$validate-code`` with a **delegating** implementation. This also applies when no CAR files are configured.

The CAR terminology service answers these operations for the code systems in its CAR files:

* ``CodeSystem/$validate-code``
* ``CodeSystem/$lookup``
* ``CodeSystem/$subsumes``

For those code systems it is preferred over the local and remote terminology services. Other operations, such as ``ValueSet/$expand`` or ``ConceptMap/$translate``, and code systems that are not in a CAR file are handled by the local and remote terminology services as before.

``ValueSet/$validate-code`` is handled by delegation. This includes the code checks of resource validation, which use ``ValueSet/$validate-code``. Firely Server looks for a terminology service that supports all three CodeSystem operations above for the code system of the code: first the CAR terminology service, then the local and remote services in their configured order. If it finds one, Firely Server reads the ValueSet itself, including filters such as ``is-a`` and ``descendent-of``, and answers the request with ``CodeSystem/$validate-code``, ``CodeSystem/$lookup`` and ``CodeSystem/$subsumes`` calls to that service. So a ValueSet defined on SNOMED CT (US Edition), LOINC or ICD-10-CM can be validated locally, as long as the ValueSet itself is available to Firely Server.

Firely Server uses the regular routing for ``ValueSet/$validate-code`` instead, when:

* no terminology service supports all three CodeSystem operations for the code system;
* the code system is ``urn:ietf:bcp:47`` (languages) or ``urn:ietf:bcp:13`` (MIME types);
* the request has ``inferSystem`` set to ``true`` together with a ``code``;
* the delegation fails. Firely Server then logs the reason at the ``Verbose`` level.

What still needs a remote terminology server
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

CAR files cover the checks on single codes. These operations are never answered from a CAR file, so for SNOMED CT, LOINC or ICD-10-CM they need a :ref:`remote terminology service <feature_terminologyoptions>` that supports them:

* ``ValueSet/$expand``, so also expanding a ValueSet that is defined on one of these terminologies
* ``CodeSystem/$find-matches`` (``$compose`` in STU3)
* ``ConceptMap/$translate`` and ``$closure``

The CAR files hold the code systems, not the ValueSets that are defined on them. To validate a code against such a ValueSet, the ValueSet itself must be in the Administration database. Load it with the :ref:`conformance resource import <conformance_import>`, for example from the package of the Implementation Guide that defines it, or through the :ref:`Administration API <conformance_administration_api>`.

Configuration
^^^^^^^^^^^^^

ConformanceArchives
-------------------

List the CAR files in the ``Terminology:ConformanceArchives`` array, for example in your ``appsettings.instance.json``:

.. code-block:: JSON

   "Terminology": {
     "MaxExpansionSize": 650,
     "ConformanceArchives": [
       {
         "Path": "/data/terminology/SCT.cs.car"
       },
       {
         "Path": "/data/terminology/LOINC.cs.car"
       },
       {
         "Path": "/data/terminology/ICD10CM.cs.car"
       }
     ],
     "LocalTerminologyService": {
       "Order": 10,
       "PreferredSystems": [ "http://hl7.org/fhir" ],
       "SupportedInteractions": [ "ValueSetValidateCode", "CodeSystemValidateCode", "Subsumes", "Expand", "FindMatches", "Lookup" ],
       "SupportedInformationModels": [ "Fhir3.0", "Fhir4.0", "Fhir5.0" ]
     }
   }

:Path: Path to the ``.car`` file on the local filesystem. A relative path is resolved against the working directory of Firely Server. The CAR files are used for every FHIR version.

.. note::

   If ``ConformanceArchives`` is empty or not configured, the CAR terminology service is not added. ``ValueSet/$validate-code`` is still handled by delegation, with the local and remote terminology services that support all three CodeSystem operations.

Running in Docker
-----------------

When Firely Server runs in :ref:`Docker <use_docker>`, mount the CAR files into the container and point ``Path`` to the location inside the container. The image runs as a user with uid 1000, so the files must be readable by that user. For example, with Docker Compose:

.. code-block:: yaml

   volumes:
     - ./terminology:/app/terminology:ro
   environment:
     - VONK_Terminology:ConformanceArchives:0:Path=/app/terminology/SCT.cs.car
     - VONK_Terminology:ConformanceArchives:1:Path=/app/terminology/LOINC.cs.car

Priority and fallback
---------------------

For a code system in one of the CAR files, the CAR terminology service is preferred over the other terminology services for ``CodeSystem/$validate-code``, ``CodeSystem/$lookup`` and ``CodeSystem/$subsumes``. If more than one CAR file contains the code system, Firely Server uses them together. For other operations and other code systems, the request goes to the remaining terminology services in their configured order.

Startup validation
------------------

Firely Server opens every configured CAR file at startup. If a file is missing or cannot be read, Firely Server does not start and logs "A configured CodeSystem Conformance Archive file at <path> could not be opened."

Examples
^^^^^^^^

The examples use SNOMED CT: ``73211009`` is *Diabetes mellitus*, and ``44054006`` is *Diabetes mellitus type 2*, a descendant of it. The operations are available on the regular FHIR endpoint and, as far as enabled in ``Administration:Operations``, on the administration endpoint (see :ref:`feature_terminology`).

Look up a concept:

.. code-block:: http

   GET [base]/CodeSystem/$lookup?system=http://snomed.info/sct&code=44054006 HTTP/1.1

Check that a code exists in the code system:

.. code-block:: http

   GET [base]/CodeSystem/$validate-code?url=http://snomed.info/sct&code=44054006 HTTP/1.1

Check whether one concept subsumes another. Here the result is ``subsumes``, because type 2 diabetes is a kind of diabetes:

.. code-block:: http

   GET [base]/CodeSystem/$subsumes?system=http://snomed.info/sct&codeA=73211009&codeB=44054006 HTTP/1.1

Validate a code against a ValueSet that is defined on SNOMED CT. For a ValueSet that is stored in the Administration database with the url ``http://example.org/fhir/ValueSet/diabetes`` and this definition:

.. code-block:: json

   {
     "resourceType": "ValueSet",
     "url": "http://example.org/fhir/ValueSet/diabetes",
     "status": "active",
     "compose": {
       "include": [
         {
           "system": "http://snomed.info/sct",
           "filter": [
             { "property": "concept", "op": "is-a", "value": "73211009" }
           ]
         }
       ]
     }
   }

Firely Server validates the code with delegation, using the CAR file:

.. code-block:: http

   GET [base]/ValueSet/$validate-code?url=http://example.org/fhir/ValueSet/diabetes&system=http://snomed.info/sct&code=44054006 HTTP/1.1

Troubleshooting
^^^^^^^^^^^^^^^

If delegation of ``ValueSet/$validate-code`` fails, Firely Server does not return an error. It handles the request with the regular routing, which can give a different result or go to a remote terminology service, and logs the reason at the ``Verbose`` level. To see these messages, raise the log level of the terminology plugin in your ``logsettings.instance.json`` (see :ref:`configure_log`). Also lower ``restrictedToMinimumLevel`` of the sink you look at to ``Verbose``:

.. code-block:: JSON

   "Serilog": {
     "MinimumLevel": {
       "Override": {
         "Vonk.Plugins.Terminology": "Verbose"
       }
     }
   }

Add the override before any broader namespaces such as ``Vonk``.

If the CAR terminology service is not used at all, check that:

* your license contains the token ``http://fire.ly/advanced-terminology``;
* ``Terminology:ConformanceArchives`` lists the files, and Firely Server started without an error about opening them;
* the request uses the exact CodeSystem url from the table above, such as ``http://snomed.info/sct``.

License
^^^^^^^

The Advanced Terminology feature requires its own license token, separate from the base terminology plugin:

* **Base Terminology** (local + remote services): ``http://fire.ly/vonk/plugins/terminology``
* **Advanced Terminology** (CAR file support): ``http://fire.ly/advanced-terminology``

Both tokens must be present in your Firely Server license to use CAR-backed terminology together with the standard local and remote services.

When you configure CAR files it is your responsibility to ensure you hold the appropriate distribution license for the terminology content (e.g. a SNOMED CT National Release Centre license or a LOINC license).
