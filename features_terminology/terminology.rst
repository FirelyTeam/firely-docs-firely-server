.. _feature_terminology:

Terminology services
====================

.. note::

  The features described on this page are available in **all** :ref:`Firely Server editions <vonk_overview>`.

Firely Server provides support for Terminology operations and validation against terminologies. This is done through a local implementation based on the Administration database ('Local Terminology Service') and - if configured - extended with external FHIR Terminology Servers ('Remote Terminology Services'). The configuration allows you to set preferred services for each CodeSystem and ValueSet. Firely Server will then transparently select and query either the Local or one of the Remote Terminology Services.

Of the operations listed below, the Local Terminology Service supports ValueSet $validate-code, $expand, $lookup, CodeSystem $validate-code, $find-matches and $subsumes. $translate and $closure are always passed on to a Remote Terminology Service. Note that it only supports simple ValueSets and CodeSystems like the ones part of the FHIR specification. It cannot support complex terminologies like LOINC or SNOMED-CT.

The terminology operations can be invoked for different FHIR versions as specified in :ref:`feature_multiversion`.

.. _feature_terminologyintegration:

Terminology Integration
^^^^^^^^^^^^^^^^^^^^^^^

In earlier versions of Firely Server, local terminology services were separately configured from so-called Terminology Integration. These are now merged.

Operations
----------

Every operation below is available on the regular FHIR endpoint (``<firely-server-endpoint>/...``) and on the administration endpoint (``<firely-server-endpoint>/administration/...``), on the levels listed for it. Which of these levels are actually open is set per endpoint in ``Operations`` and ``Administration:Operations`` (see :ref:`enabling the operations <feature_terminology_operations_config>`). On the regular endpoint, CodeSystems, ValueSets and ConceptMaps are still resolved from the Administration database.

Firely Server decides per request whether the Local Terminology Service or a Remote Terminology Service handles it (see :ref:`feature_terminologyoptions`). The parameter lists below are those of the Local Terminology Service. It rejects any other parameter with the message "The input parameter(s) ... are currently not supported". A Remote Terminology Service receives the parameters of the request and decides itself which ones it supports. Parameters of a complex type, such as ``coding`` or ``valueSet``, can only be passed with ``POST``.

ValueSet $validate-code
-----------------------

:definition: https://hl7.org/fhir/valueset-operation-validate-code.html
:notes: 
   * Available on the type level ``ValueSet/$validate-code`` and the instance level ``ValueSet/<id>/$validate-code``.
   * The Local Terminology Service supports the parameters url, valueSetVersion, valueSet, code, system, display, coding, codeableConcept, abstract, inferSystem and context.
   * Pass the ValueSet either by reference (url, optionally with valueSetVersion, or the id on the instance level) or as a resource (the valueSet parameter or the request body), not both. A request that has both is rejected with ``400 Bad Request`` and "Ambiguous call. ValueSet information present both in url and in the valueSet parameter or request body".
   * Both ``GET`` and ``POST`` interactions are available. 

ValueSet $expand
----------------

:definition: https://hl7.org/fhir/valueset-operation-expand.html
:notes:
   * Available on the type level ``ValueSet/$expand`` and the instance level ``ValueSet/<id>/$expand``.
   * The Local Terminology Service supports the parameters url, valueSet and includeDesignations. It does not support valueSetVersion or any of the other expansion parameters, such as filter, count or offset. A Remote Terminology Service may support them.
   * Pass the ValueSet either by reference (url, or the id on the instance level) or as a resource (the valueSet parameter or the request body), not both. A request that has both is rejected with ``400 Bad Request`` and "Ambiguous call. ValueSet information present both in url and in the valueSet parameter or request body".
   * The size of an expansion by the Local Terminology Service is limited by ``MaxExpansionSize`` (see :ref:`feature_terminologyoptions`).
   * Both ``GET`` and ``POST`` interactions are available. 

CodeSystem $lookup
------------------

:definition: https://hl7.org/fhir/codesystem-operation-lookup.html
:notes:
   * Available on the type level ``CodeSystem/$lookup`` and the instance level ``CodeSystem/<id>/$lookup``. On the instance level the id is not used: pass the CodeSystem with system or coding as on the type level.
   * The Local Terminology Service supports the parameters code, system, version, coding and property. It does not support date.
   * Pass the concept either as code and system or as coding, not both. A request with both code and ``coding.code``, or with both system and ``coding.system``, is rejected with "Ambiguous call. code/system was provided in both code/system parameters and coding parameter".
   * Both ``GET`` and ``POST`` interactions are available. 

CodeSystem $validate-code
-------------------------

:definition: https://hl7.org/fhir/codesystem-operation-validate-code.html
:notes:
   * Available on the type level ``CodeSystem/$validate-code`` and the instance level ``CodeSystem/<id>/$validate-code``. On the instance level the CodeSystem with that id is used, unless url is given.
   * The Local Terminology Service supports the parameters url, version, codeSystem, code, display, coding, codeableConcept, abstract and displayLanguage.
   * Both ``GET`` and ``POST`` interactions are available. 

CodeSystem $find-matches / $compose
-----------------------------------

:definition: https://hl7.org/fhir/codesystem-operation-find-matches.html
:notes:
   * Available on the type level ``CodeSystem/$find-matches`` and the instance level ``CodeSystem/<id>/$find-matches``. On the instance level the CodeSystem with that id is used, unless system is given.
   * The Local Terminology Service supports the parameters system, version, exact and property (with the parts code and value).
   * property can only be passed with ``POST``. Both ``GET`` and ``POST`` interactions are available. 
   * $find-matches was named $compose in FHIR STU3. The operation is supported with both names. For STU3, the terminology service must list ``Compose`` in its ``SupportedInteractions`` (see :ref:`feature_terminologyoptions`), and ``$compose`` must be enabled in the ``Operations`` section of the endpoint. The default settings of the Local Terminology Service only list ``FindMatches``.

CodeSystem $subsumes
--------------------

:definition: https://hl7.org/fhir/codesystem-operation-subsumes.html
:notes:
   * Available on the type level ``CodeSystem/$subsumes`` and the instance level ``CodeSystem/<id>/$subsumes``. On the instance level the CodeSystem with that id is used, unless system is given.
   * The Local Terminology Service supports the parameters codeA, codeB, codingA, codingB, system and version.
   * Both ``GET`` and ``POST`` interactions are available. 

ConceptMap $closure
--------------------

:definition: https://hl7.org/fhir/conceptmap-operation-closure.html
:notes:
   * Available on the system level ``$closure``.
   * The Local Terminology Service does not support this operation. It is passed on to a Remote Terminology Service supporting it, and supports any parameters that the remote service supports.
   * Only ``POST`` interactions are available. 

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
    :PreferredSystem: If a request is directed at a specific code system, Firely Server will choose this terminology server over other available services. A system matches one of the preferred systems if the system starts with the preferred system. So ``http://loinc.org`` will match any CodeSystem or ValueSet with a canonical that starts with that url.  
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

The local terminology service built into Firely Server works well for simple ValueSets and CodeSystems such as those shipped with the FHIR specification. However, large and complex terminologies like **SNOMED CT** and **LOINC** cannot be stored as plain FHIR resources in the Administration database — they are too large, have rich hierarchical relationships, and require specialised subsumption logic.

**Conformance Archive (CAR) files** solve this problem. A CAR file is a representation of a CodeSystem produced by the `Firely.Car` library. It stores the full concept inventory together with all required relationship data in a format that can be loaded and queried efficiently at runtime from the local filesystem — without requiring an external terminology server.

How it works
^^^^^^^^^^^^

When ``AdvancedTerminologyConfiguration`` is active and at least one CAR file is configured, Firely Server registers a **CAR Terminology Host** alongside the existing local and remote terminology hosts. In addition, the default terminology implementation is replaced by the **Delegated Terminology**, which contains the following logic:

1. For every incoming terminology request, the proxy first checks whether the CAR Terminology Host can handle the request (i.e. whether the requested CodeSystem URL is present in one of the configured CAR files).
2. If the CAR host can handle the request, it serves it directly.
3. For **ValueSet/$validate-code** requests that involve complex filter criteria (e.g. ``is-a`` or ``descendent-of`` filters referencing a CAR-backed CodeSystem), the proxy decomposes the request into a series of lower-level CodeSystem operations (``CodeSystem/$validate-code``, ``CodeSystem/$subsumes``, and ``CodeSystem/$lookup``) and aggregates the results.
4. If the CAR host cannot handle a request (either because the CodeSystem is not in a CAR file, or because the operation is not supported), the request falls back to the standard terminology proxy and the configured local/remote services.

Supported operations via CAR files
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

The following FHIR terminology operations are directly supported when the requested CodeSystem is backed by a CAR file:

* ``CodeSystem/$validate-code``
* ``CodeSystem/$subsumes``
* ``CodeSystem/$lookup``

Operations that are **not** directly supported by CAR files (such as ``ValueSet/$expand``, ``ConceptMap/$translate``) will be handled by the fallback services (local or remote), just as before.

``ValueSet/$validate-code`` is supported indirectly: if the included CodeSystem concepts are backed by a CAR file, the proxy automatically decomposes the validation into the supported CodeSystem-level operations listed above.

Configuration
^^^^^^^^^^^^^

ConformanceArchives
-------------------

Configure one or more CAR files in the ``Terminology:ConformanceArchives`` array in your ``appsettings.json``:

.. code-block:: JSON

   "Terminology": {
     "MaxExpansionSize": 650,
     "ConformanceArchives": [
       {
         "Path": "/data/terminology/SCT.cs.car"
       },
       {
         "Path": "/data/terminology/LOINC.cs.car"
       }
     ],
     "LocalTerminologyService": {
       "Order": 10,
       "PreferredSystems": [ "http://hl7.org/fhir" ],
       "SupportedInteractions": [ "ValueSetValidateCode", "Expand", "FindMatches", "Lookup" ],
       "SupportedInformationModels": [ "Fhir3.0", "Fhir4.0", "Fhir5.0" ]
     }
   }

:Path: Absolute or relative path to the ``.car`` file on the local filesystem. All configured paths are validated at startup; Firely Server will refuse to start if a CAR file cannot be opened.

.. note::

   If ``ConformanceArchives`` is empty or not configured, the ``CarTerminologyHost`` is not registered and no CAR-related behaviour is active. The standard local and remote terminology services continue to work normally.

Priority and fallback
---------------------

The CAR Terminology Host is always consulted **before** other hosts for any CodeSystem that is present in a configured CAR file. If the CAR host cannot serve the request (unsupported operation or unrecognised system), the request is passed to the remaining terminology services in their configured order.

Startup validation
------------------

All configured CAR files are validated when Firely Server starts. A ``VonkConfigurationException`` is raised if any file is missing or cannot be read, preventing the server from starting with an invalid terminology configuration.

License
^^^^^^^

The Advanced Terminology feature requires its own license token, separate from the base terminology plugin:

* **Base Terminology** (local + remote services): ``http://fire.ly/vonk/plugins/terminology``
* **Advanced Terminology** (CAR file support): ``http://fire.ly/advanced-terminology``

Both tokens must be present in your Firely Server license to use CAR-backed terminology together with the standard local and remote services.

When you configure CAR files it is your responsibility to ensure you hold the appropriate distribution license for the terminology content (e.g. a SNOMED CT National Release Centre license or a LOINC license).
