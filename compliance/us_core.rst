US Core / 21th Century Cures Act
================================

* Firely Server provides full profile and interaction support as defined in `"Conforming to US Core" <https://hl7.org/fhir/us/core/general-requirements.html#profile-support--interaction-support>`_:
  
  * Firely Server can be populated with resources conforming to US Core
  * All elements defined as must-support by the implementation guide are supported
  * All references between FHIR resources defined as must-support by the implementation guide are supported
  * All search and CRUD interactions defined by US Core are supported, including optional search parameters

* Firely provides offical support for the following versions of the US Core ImplementationGuide:

 ================== ========= ========================================
 US Core Version    Status    References                                                                 
 ================== ========= ========================================                                                                             
  US Core 3.1.1      ✅         http://hl7.org/fhir/us/core/STU3.1.1/                                 
  US Core 4.0.0      ✅         http://hl7.org/fhir/us/core/STU4/                                                                                                                      
 ================== ========= ======================================== 


* All StructureDefinitions for profiles and extensions (v3.1.1) are loaded by default in the standard SQLite administration database of Firely Server. No additional configuration needed in order to validate against these conformance resources.

* A mapping between USCDI and the US Core profiles can be found in the `US Core ImplementationGuide <http://build.fhir.org/ig/HL7/US-Core/uscdi.html>`_.
  
See `How to Test Firely Server on Inferno <https://fire.ly/ebook-how-to-test-firely-server-on-inferno/>`_ for more information on how to pass the official 21st Century Cures Act test. See :ref:`firely_auth_introduction` for details on how to configure a client to interact with Firely Server and Firely Auth.
  
Known Limitations
^^^^^^^^^^^^^^^^^

* In order to validate resources claiming to conform to US Core, it is necessary to configure Firely Server to use an external terminology server incl. support for expanding SNOMED CT and LOINC ValueSets. See :ref:`feature_terminology`.
* Certain parameters are not implemented for the ``$docref`` operation on DocumentReference resources. See :ref:`feature_docref` for more details.
  
Test Data
^^^^^^^^^

Firely provides test data covering all US-Core profiles and all elements marked as Must-Support. In order to load all examples, two transaction bundles need to be posted against the base endpoint of Firely Server. The following Postman collection provides you with the bundles itself, and the bundle entries as individual PUT requests.

.. raw:: html

  <div class="postman-run-button"
  data-postman-action="collection/fork"
  data-postman-var-1="6644549-bc7e4cdd-3065-4029-bcec-8dcb7c055746"
  data-postman-collection-url="entityId=6644549-bc7e4cdd-3065-4029-bcec-8dcb7c055746&entityType=collection&workspaceId=822b68d8-7e7d-4b09-b8f1-68362070f0bd"
  data-postman-param="env%5BFirely%20Server%20Public%5D=W3sia2V5IjoiQkFTRV9VUkwiLCJ2YWx1ZSI6Imh0dHBzOi8vc2VydmVyLmZpcmUubHkvIiwiZW5hYmxlZCI6dHJ1ZSwidHlwZSI6ImRlZmF1bHQiLCJzZXNzaW9uVmFsdWUiOiJodHRwczovL3NlcnZlci5maXJlLmx5LyIsInNlc3Npb25JbmRleCI6MH1d"></div>
  <script type="text/javascript">
    (function (p,o,s,t,m,a,n) {
      !p[s] && (p[s] = function () { (p[t] || (p[t] = [])).push(arguments); });
      !o.getElementById(s+t) && o.getElementsByTagName("head")[0].appendChild((
        (n = o.createElement("script")),
        (n.id = s+t), (n.async = 1), (n.src = m), n
      ));
    }(window, document, "_pm", "PostmanRunObject", "https://run.pstmn.io/button.js"));
  </script>

The following steps are necessary in order to execute the test collection against our own Firely Server instance:

#. Select "Fork Collection" or "View collection" in the Postman dialog

    .. image:: ../images/Compliance_ForkTestCollectionPostman.png
       :align: center
       :width: 500

#. Sign-In with your Postman account

#. `Create a new Postman environment <https://learning.postman.com/docs/sending-requests/managing-environments/#creating-environments>`_ with a "BASE_URL" variable and adjust the URL to your server endpoint

    .. image:: ../images/Compliance_EnvironmentTestCollectionPostman.png
       :align: center
       :width: 800

#. Make sure that the newly created environment is selected as the active environment

#. Open the collection "Firely Server - US Core Tests"

    .. image:: ../images/Compliance_USCoreTestCollectionPostman.png
       :align: center
       :width: 500

#. Execute the transaction request, the expected response is "HTTP 200 - OK".
