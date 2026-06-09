.. _questionnaire-package:

Da Vinci DTR - $questionnaire-package
======================================

.. note::

  The features described on this page are available in the following :ref:`Firely Server editions <vonk_overview>`:

  * Firely Prior Authorization - 🇺🇸

The ``$questionnaire-package`` operation is used to retrieve a package of Questionnaire-related resources needed by a SMART on FHIR application to render and complete a set of clinical forms. The operation returns, for each relevant Questionnaire, a self-contained collection Bundle comprising the Questionnaire itself, its CQL Library dependencies, and the ValueSets required for answer sets.

The operation is defined as part of the `Da Vinci Documentation Templates and Rules (DTR) Implementation Guide <https://hl7.org/fhir/us/davinci-dtr>`_. For the formal specification refer to the `$questionnaire-package OperationDefinition <https://hl7.org/fhir/us/davinci-dtr/OperationDefinition-questionnaire-package.html>`_.

----

Supported parameters
--------------------

Firely Server supports the following input parameters:

+-------------------+-----------+-----------------------+-----------------------------------------------------+
| Parameter         | Supported | Type                  | Additional Notes                                    |
+===================+===========+=======================+=====================================================+
| ``coverage``      | ✅        | ``Coverage``          | Required. The Coverage resource identifying the     |
|                   |           |                       | payer context. Must have ``status = active``; a     |
|                   |           |                       | ``412 Precondition Failed`` response is returned    |
|                   |           |                       | when the coverage is not active.                    |
+-------------------+-----------+-----------------------+-----------------------------------------------------+
| ``questionnaire`` | ✅        | ``canonical``         | One or more canonical URLs identifying specific     |
|                   |           |                       | Questionnaires to include in the package. At least  |
|                   |           |                       | one ``questionnaire`` canonical must be provided.   |
+-------------------+-----------+-----------------------+-----------------------------------------------------+
| ``changedsince``  | ✅        | ``instant``           | When provided, only Questionnaires (and their       |
|                   |           |                       | dependencies) that have been updated after this     |
|                   |           |                       | timestamp are included in the response. Entries     |
|                   |           |                       | with an earlier ``lastUpdated`` are omitted.        |
+-------------------+-----------+-----------------------+-----------------------------------------------------+

The output ``Parameters`` resource conforms to the
`dtr-qpackage-output-parameters <http://hl7.org/fhir/us/davinci-dtr/StructureDefinition/dtr-qpackage-output-parameters>`_
profile and contains the following parameters:

+---------------------+----------------+--------------------------------------------------------------+
| Parameter           | Type           | Description                                                  |
+=====================+================+==============================================================+
| ``PackageBundle``   | ``Bundle``     | One collection Bundle per resolved Questionnaire. Each       |
|                     |                | Bundle contains: the Questionnaire (first entry), all        |
|                     |                | transitive ``Library`` dependencies, and all ``ValueSet``    |
|                     |                | resources used as answer sets (expanded inline).             |
+---------------------+----------------+--------------------------------------------------------------+
| ``operationOutcome``| ``OperationOutcome`` | Present only when warnings or informational messages   |
|                     |                | accompany a successful response.                             |
+---------------------+----------------+--------------------------------------------------------------+

----

Bundle contents
---------------

Each ``PackageBundle`` in the response is a FHIR ``collection`` Bundle structured as follows:

1. **Questionnaire** — always the first entry.
2. **Library resources** — all CQL Library dependencies resolved transitively. Libraries are discovered
   via the ``cqf-library`` extension on the Questionnaire and via ``relatedArtifact`` entries of type
   ``depends-on`` on each Library. Resolution is recursive: dependencies of dependencies are included.
3. **ValueSet resources** — all ValueSets referenced by ``answerValueSet`` elements in the Questionnaire.
   ValueSets are expanded inline using Firely Server's terminology service.

----

Configuration
-------------

Enable the $questionnaire-package operation
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Enabling the operation requires the following steps. When enabled correctly, the operation will be listed
in the CapabilityStatement with its canonical URL:
``http://hl7.org/fhir/us/davinci-dtr/OperationDefinition/questionnaire-package``.

Check the license
~~~~~~~~~~~~~~~~~

The ``$questionnaire-package`` operation requires the license token
``http://fire.ly/server/plugins/questionnaire`` to be present in the license file.
If you do not have this license token, please contact `Firely <https://fire.ly/contact>`_.

Include the plugin in the pipeline
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

In the ``PipelineOptions`` section of the :ref:`appsettings <configure_appsettings>`, add the namespace
of the plugin:

.. code-block:: javascript

    "PipelineOptions": {
        "PluginDirectory": "./plugins",
        "Branches": [
          {
            "Path": "/",
            "Include": [
              // ...
              "Vonk.Plugin.Questionnaire",
            ],
            "Exclude": [
              // ...
            ]
          }
        ]
    }

Check that the operation is listed as supported
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

The ``$questionnaire-package`` operation should be listed in the ``Operations`` section of the
:ref:`appsettings <disable_interactions>`. By default this is the case:

.. code-block:: json

    "$questionnaire-package": {
        "Name": "$questionnaire-package",
        "Level": ["Type"],
        "Enabled": true,
        "RequireAuthorization": "WhenAuthEnabled",
        "RequireTenant": "WhenTenancyEnabled"
    }

If you have previously overridden the ``Operations`` section, make sure the entry above is included.

----

Example
-------

The following example requests the questionnaire package for a specific Questionnaire canonical, filtered
to resources updated since a given timestamp.

**Request**

.. code-block:: http

   POST [base]/Questionnaire/$questionnaire-package HTTP/1.1
   Content-Type: application/fhir+json

**Request Body**

.. code-block:: json

    {
      "resourceType": "Parameters",
      "parameter": [
        {
          "name": "coverage",
          "resource": {
            "resourceType": "Coverage",
            "status": "active",
            "beneficiary": {
              "reference": "Patient/example"
            },
            "payor": [
              {
                "reference": "Organization/example-payer"
              }
            ]
          }
        },
        {
          "name": "questionnaire",
          "valueCanonical": "http://example.org/fhir/Questionnaire/prior-auth-form|1.0.0"
        },
        {
          "name": "changedsince",
          "valueInstant": "2024-01-01T00:00:00Z"
        }
      ]
    }

**Response Body**

.. code-block:: json

    {
      "resourceType": "Parameters",
      "meta": {
        "profile": [
          "http://hl7.org/fhir/us/davinci-dtr/StructureDefinition/dtr-qpackage-output-parameters"
        ]
      },
      "parameter": [
        {
          "name": "PackageBundle",
          "resource": {
            "resourceType": "Bundle",
            "type": "collection",
            "timestamp": "2025-03-26T12:00:00Z",
            "entry": [
              {
                "resource": {
                  "resourceType": "Questionnaire",
                  "id": "prior-auth-form",
                  "url": "http://example.org/fhir/Questionnaire/prior-auth-form",
                  "version": "1.0.0",
                  "status": "active"
                }
              },
              {
                "resource": {
                  "resourceType": "Library",
                  "id": "prior-auth-logic",
                  "url": "http://example.org/fhir/Library/prior-auth-logic",
                  "status": "active",
                  "content": [
                    { "contentType": "text/cql" },
                    { "contentType": "application/elm+xml" }
                  ]
                }
              },
              {
                "resource": {
                  "resourceType": "ValueSet",
                  "id": "procedure-codes",
                  "url": "http://example.org/fhir/ValueSet/procedure-codes",
                  "status": "active",
                  "expansion": {
                    "timestamp": "2025-03-26T12:00:00Z",
                    "contains": [
                      { "system": "http://www.ama-assn.org/go/cpt", "code": "27447" }
                    ]
                  }
                }
              }
            ]
          }
        }
      ]
    }
