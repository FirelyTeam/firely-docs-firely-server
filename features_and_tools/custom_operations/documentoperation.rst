.. _feature_documentoperation:

Generate a FHIR document - $document
====================================

The $document operation is a custom operation defined in the `FHIR core specification <https://www.hl7.org/fhir/r4/composition-operation-document.html>`_ to facilitate the creation and retrieval of a complete, structured health record document.
It plays a crucial role in infrastructures where the exchange of information depends on attested and potentially signed documents instead of individual resources. An introduction to the document exchange paradigm can be found `here <https://www.hl7.org/fhir/r4/documents.html>`_.

Yet, it is a fully functional and production ready implementation of the operation.

API Usage
---------

$document is implemented on the Composition endpoint of Firely Server. Typically the operation is invoked using a POST request with an id of Composition resource stored in Firely Server:

.. code-block:: HTTP

    POST {base-url}/Composition

.. code-block:: json

    {
        "resourceType": "Parameters",
        "parameter": [
            {
                "name": "id",
                "valueString": "example"
            }
        ]
    }

Alternatively the operation can be called via GET on an instance-level:

.. code-block:: HTTP

GET {base-url}/Composition/<id>/$document

The operation will retrieve all resources that are mentioned in any section of the composition as entry references. Absolute external references will not be resolved.

The generated Document bundle can be immediately stored on the Bundle endpoint of Firely Server via the persist parameter:

.. code-block:: HTTP

    POST {base-url}/Composition

.. code-block:: json

    {
        "resourceType": "Parameters",
        "parameter": [
            {
                "name": "id",
                "valueString": "example"
            },
            {
                "name": "persist",
                "valueBoolean": true
            }
        ]
    }

Known limitations
-----------------

* The graph parameter to create a document based on a GraphDefinition is not supported
