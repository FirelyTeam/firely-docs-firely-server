.. _postman_tutorial:

Postman Setup
================

This article will walk through setting up Postman to access Firely Server restful endpoint.

Prerequisites
-------------
Sign up for and install Postman. For more information about Postman, see `Get Started with Postman <https://www.getpostman.com/>`_.


Using Postman
-------------

#. Click the following "Fork postman collection into your workspace" link:

    .. raw:: html

        <div align="center" class="postman-run-button"
        data-postman-action="collection/fork"
        data-postman-var-1="24489118-d02c5cf2-8890-4d1b-a2d5-9c7688d56793"
        data-postman-collection-url="entityId=24489118-d02c5cf2-8890-4d1b-a2d5-9c7688d56793&entityType=collection&workspaceId=822b68d8-7e7d-4b09-b8f1-68362070f0bd"></div>
        <script type="text/javascript">
          (function (p,o,s,t,m,a,n) {
            !p[s] && (p[s] = function () { (p[t] || (p[t] = [])).push(arguments); });
            !o.getElementById(s+t) && o.getElementsByTagName("head")[0].appendChild((
              (n = o.createElement("script")),
              (n.id = s+t), (n.async = 1), (n.src = m), n
            ));
          }(window, document, "_pm", "PostmanRunObject", "https://run.pstmn.io/button.js"));
        </script>

#. Click "Fork Collection"

    .. image:: ../images/Compliance_ForkTestCollectionPostman.png
           :align: center
           :width: 500

#. Sign-In with your Postman account and click "Fork Collection". Change the label and workspace names as desired.

    .. image:: ../images/postman_tutorial_forkcollection.png
           :align: center
           

#. Variables have been predefined at the collection level for ease of use. Adjust the variables to reflect your Firely Server endpoint and settings and save.

    .. image:: ../images/postman_tutorial_variables.png
       :align: center

#. Test the first request, metadata, as seen below by clicking "Send". This will return the server Capability Statement.

    .. image:: ../images/postman_tutorial_metadata.png
       :align: center    

    ::

        {
        "resourceType": "CapabilityStatement",
        "id": "e8bfb522-022d-47d4-a1e6-c05b7fc1175d",
        "meta": {
            "versionId": "de3e4181-97be-4f58-80d2-e01cb2198ef2",
            "lastUpdated": "2022-12-07T19:14:07.7140139+00:00"
        },
            "language": "en-US",
            "url": "http://server.fire.ly/fhir/CapabilityStatement/FirelyServer",
            "version": "1.0",
            "name": "Firely Server 4.10.0 CapabilityStatement",
            "status": "active",
            "experimental": true,
            "date": "2022-12-07T19:14:07.7140229+00:00",
            "publisher": "Firely"

#. Collection folders "Create Example Data" should be executed next. This will setup FHIR records for "Retrieve Example Data" basic examples and examples throughout the collection.
    
    .. image:: ../images/postman_tutorial_createdata.png
       :align: center

#. In the first example ``PUT Patient`` observe the body tab to see the resource to be created.
    
    .. image:: ../images/postman_tutorial_exampleput.png
       :align: center

#. Continue testing other requests in the collection. The Features folder in the collection aligns to the :ref:`vonk_features` documentation and provides corresponding live examples.