.. _feature_terminology:

Terminology services
====================

.. note::

  The features described on this page are available in **all** :ref:`Firely Server editions <vonk_overview>`.

Firely Server provides support for Terminology operations and validation against terminologies. This is done through a local implementation based on the Administration database ('Local Terminology Service') and - if configured - extended with external FHIR Terminology Servers ('Remote Terminology Services'). The configuration allows you to set preferred services for each CodeSystem and ValueSet. Firely Server will then transparently select and query either the Local or one of the Remote Terminology Services.

Of the operations listed below the following can be supported by the Local Terminology Service: $validate-code, $expand, $lookup, $find-matches. Note that it only supports simple ValueSets and CodeSystems like the ones part of the FHIR specification. It cannot support complex terminologies like LOINC or SNOMED-CT.

The terminology operations can be invoked for different FHIR versions as specified in :ref:`feature_multiversion`.

.. _feature_terminologyintegration:

Terminology Integration
^^^^^^^^^^^^^^^^^^^^^^^

In earlier versions of Firely Server, local terminology services were separately configured from so-called Terminology Integration. These are now merged.

Operations
----------

ValueSet $validate-code
-----------------------

:definition: http://www.hl7.org/implement/standards/fhir/valueset-operation-validate-code.html
:notes: 
   * Available on the type level ``<firely-server-endpoint>/administration/ValueSet/$validate-code`` and the instance level ``<firely-server-endpoint>/administration/ValueSet/<id>/$validate-code``.
   * Only the parameters url, valueSet, valueSetVersion, code, system, display, coding, codeableConcept, abstract are supported.
   * The url and valueSetVersion input parameters are only taken into consideration if no valueSet resource was provided in the body. So the valueSet in the body takes priority.
   * Both ``GET`` and ``POST`` interactions are available. 

ValueSet $expand
----------------

:definition: http://www.hl7.org/implement/standards/fhir/valueset-operation-expand.html
:notes:
   * Available on the type level ``<firely-server-endpoint>/administration/ValueSet/$expand`` and the instance level ``<firely-server-endpoint>/administration/ValueSet/<id>/$expand``.
   * Only the parameters url, valueSet, valueSetVersion and includeDesignations are supported.
   * The url and valueSetVersion input parameters are only taken into consideration if no valueSet resource was provided in the body. So the valueSet in the body takes priority.
   * Both ``GET`` and ``POST`` interactions are available. 

CodeSystem $lookup
------------------

:definition: http://www.hl7.org/implement/standards/fhir/codesystem-operation-lookup.html
:notes:
   * Available on the type level ``<firely-server-endpoint>/administration/CodeSystem/$lookup``.
   * Only the parameters code, system, version, coding and date are supported. 
   * Code & system combination takes priority over the coding parameter.
   * Both ``GET`` and ``POST`` interactions are available. 

CodeSystem $find-matches / $compose
-----------------------------------

:definition: http://www.hl7.org/implement/standards/fhir/codesystem-operation-find-matches.html
:notes:
   * Available on the type level ``<firely-server-endpoint>/administration/CodeSystem/$find-matches`` and the instance level ``<firely-server-endpoint>/administration/CodeSystem/<id>/$find-matches``.
   * Only the parameters system, exact, version, property.code and property.value are supported.
   * The url and valueSetVersion input parameters are only taken into consideration if no valueSet resource was provided in the body. So the valueSet in the body takes priority.
   * Both ``GET`` and ``POST`` interactions are available. 
   * $find-matches was named $compose in FHIR STU3. The operation is supported with both names.

   
CodeSystem $subsumes
--------------------

:definition: http://www.hl7.org/implement/standards/fhir/codesystem-operation-subsumes.html
:notes:
   * Available on the type level ``<firely-server-endpoint>/administration/CodeSystem/$subsumes``.
   * Only the parameters codeA, codeB, system, and version are supported.
   * The system input parameters is only taken into when called on the type level.
   * Both ``GET`` and ``POST`` interactions are available. 

ConceptMap $closure
--------------------

:definition: http://www.hl7.org/implement/standards/fhir/conceptmap-operation-closure.html
:notes:
   * Available on the system level ``<firely-server-endpoint>/administration/$closure``.
   * This operation is passed on to a Remote Terminology Service supporting it. It supports any parameters that the Remote service supports. 
   * Only ``POST`` interactions are available. 

ConceptMap $translate
---------------------

:definition: http://www.hl7.org/implement/standards/fhir/conceptmap-operation-translate.html
:notes:
   * Available on the instance level ``<firely-server-endpoint>/administration/ConceptMap/[id]/$translate`` and the type level ``<firely-server-endpoint>/administration/ConceptMap/$translate``.
   * Only the parameters url, conceptMap (on POST), conceptMapVersion, code, system, version, source, target, targetsystem and reverse are supported.
   * Both ``GET`` and ``POST`` interactions are available. 


Configuration
-------------

Pipeline
^^^^^^^^

Make sure to add the ``Vonk.Plugins.Terminology`` plugin to the PipelineOptions in appsettings in order to make use of the ``TerminologyIntegration`` plugin.
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

Also make sure that the terminology operations are allowed at all in the ``SupportedInteractions`` section::

   "SupportedInteractions": {
      "InstanceLevelInteractions": "$validate-code, $expand, $compose, $translate, $subsumes",
      "TypeLevelInteractions": "$validate-code, $expand, $lookup, $compose, $translate, $subsumes",
      "WholeSystemInteractions": "$closure"
   },

Lastly, operation on the administration endpoint can be limited to specific IP addresses::

   "Administration": {
      "Security": {
         "AllowedNetworks": [ "127.0.0.1", "::1" ], // i.e.: ["127.0.0.1", "::1" (ipv6 localhost), "10.1.50.0/24", "10.5.3.0/24", "31.161.91.98"]
         "OperationsToBeSecured": [ "$validate-code", "$expand", "$compose", "$translate", "$subsumes", "$lookup", "$closure" ]
      }
   },

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
    :Username: If the terminology service uses Basic Authentication, you can set the required username here. 
    :Password: If the terminology service uses Basic Authentication, you can set the required password here.
    :ClientId: If the terminology service uses a `client_credentials`-based OAuth2 flow, you can set the client_id here.
    :ClientSecret: If the terminology service uses a `client_credentials`-based OAuth2 flow, you can set the shared client secret here.
    :Scopes: If the terminology service uses a `client_credentials`-based OAuth2 flow, you can set the scopes requested by Firely Server here.
    :MediaType: Default Media-Type that should be used for serialization of the Parameters resources forwarded to the external terminology servie

Notes:

* The Endpoint, Username and Password settings are not valid for the Local Terminology Server, just for the Remote services.
* If a Remote Terminology Service has different endpoints for different FHIR versions, configure each endpoint separately.
* The ``SupportedInformationModels`` cannot be broader than the corresponding ``Fhir.Rx`` plugins configured in the PipelineOptions.
* Firely Server automatically requests JWT token for an OAuth2-protected endpoint and ensures that a valid token is always submitted as part of the request to the external terminology service.

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
