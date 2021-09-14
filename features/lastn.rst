.. _lastn:

$lastN Observations
===================

Description
-----------

This plugin implements the Observation $lastn operation, as described in https://www.hl7.org/fhir/observation-operation-lastn.html. This operation returns the most recent or last n=number of observations for a patient within a given category (e.g. vital-signs, laboratory, etc.).

Currently, the functionality is only available if you use SQL Server for your data.

::

  GET <base-url>/Observation/$lastn
  Accept: <any supported FHIR media type>

Required parameters:

* **patient** / **subject**: a reference to the Patient;
* **category**: a category to get the most recent observations from.

Optional parameters:

* **max**: maximum number of Observations to return from each group;
* other search parameters defined for the Observation resource.

Appsettings
-----------
To start using the $lastn operation you will first have to add the plugin to the PipelineOptions in the appsettings.

.. code-block:: JavaScript

 "PipelineOptions": {
    "PluginDirectory": "./plugins",
    "Branches": [
      {
        "Path": "/",
        "Include": [
          ...
          "Vonk.Plugin.LastN",
        ],
        "Exclude": [
          ...
        ]
      }, ...etc...

.. note::
    We did not implement $lastn for all database types. Make sure the data database is configured for SQL Server.

Examples
--------

The examples below use a predefined set of resources. You can add those resources to your instance of the server to reproduce the examples. To do it, please execute the following transaction bundle: :download:`pdf <../_static/files/lastN-example-bundle.json>`. The transaction bundle contains a patient with id = **8c5a23b8-5154-a665-8704-35f0e7a386e9** and a list of Observations for that patient.


Fetch the last 3 results for all vitals for a patient
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Request:
::

  GET <base-url>/Observation/$lastn?max=3&patient=8c5a23b8-5154-a665-8704-35f0e7a386e9&category=vital-signs
  Accept: application/fhir+json

Response:

.. code-block:: JavaScript

  {
    "resourceType": "Bundle",
    "type": "searchset",
    "timestamp": "2021-09-14T11:52:58.450+04:00",
    "meta": {
      "lastUpdated": "2021-09-14T11:52:58.450+04:00",
      "versionId": "8f3c6c0a-37d7-4fde-b80b-57e72e655fe7"
    },
    "entry": [
      {
        "fullUrl": "<base-url>/Observation/bac874a3-8d89-bd0c-9157-f2fb153d6642",
        "search": {
          "mode": "match"
        },
        "resource": {
          "resourceType": "Observation",
          "id": "bac874a3-8d89-bd0c-9157-f2fb153d6642",
          "status": "final",
          "category": [
            {
              "coding": [
                {
                  "system": "http://terminology.hl7.org/CodeSystem/observation-category",
                  "code": "vital-signs",
                  "display": "vital-signs"
                }
              ]
            }
          ],
          "code": {
            "coding": [
              {
                "system": "http://loinc.org",
                "code": "2708-6",
                "display": "Oxygen saturation in Arterial blood"
              },
              {
                "system": "http://loinc.org",
                "code": "59408-5",
                "display": "Oxygen saturation in Arterial blood by Pulse oximetry"
              }
            ],
            "text": "Oxygen saturation in Arterial blood"
          },
          "subject": {
            "reference": "<base-url>/Patient/8c5a23b8-5154-a665-8704-35f0e7a386e9"
          },
          "effectiveDateTime": "2020-03-03T01:58:48+04:00",
          // ...
        }
      },
      {/* Entry Observation with code [Body Weight(29463-7)] from 2020-03-03 */},
      {/* Entry Observation with code [Body Weight(29463-7)] from 2019-04-22 */},
      {/* Entry Observation with code [Body Weight(29463-7)] from 2016-04-18 */},
      {/* Entry Observation with code [Body Mass Index(39156-5)] from 2019-04-22 */},
      {/* Entry Observation with code [Body Mass Index(39156-5)] from 2016-04-18 */},
      {/* Entry Observation with code [Body Mass Index(39156-5)] from 2013-04-15 */},
      {/* Entry Observation with code [Pain severity - 0-10 verbal numeric rating [Score] - Reported(72514-3)] from 2019-04-22 */},
      {/* Entry Observation with code [Pain severity - 0-10 verbal numeric rating [Score] - Reported(72514-3)] from 2016-04-18 */},
      {/* Entry Observation with code [Pain severity - 0-10 verbal numeric rating [Score] - Reported(72514-3)] from 2013-04-15 */},
      {/* Entry Observation with code [Body Height(8302-2)] from 2019-04-22 */},
      {/* Entry Observation with code [Body Height(8302-2)] from 2016-04-18 */},
      {/* Entry Observation with code [Body Height(8302-2)] from 2013-04-15 */},
      {/* Entry Observation with code [Body temperature(8310-5)], [Oral temperature(8331-1)] from 2020-03-03 */},
      {/* Entry Observation with code [Blood Pressure(85354-9)] from 2020-03-03 */},
      {/* Entry Observation with code [Blood Pressure(85354-9)] from 2019-04-22 */},
      {/* Entry Observation with code [Blood Pressure(85354-9)] from 2016-04-18 */},
      {/* Entry Observation with code [Heart rate(8867-4)] from 2020-03-03 */},
      {/* Entry Observation with code [Heart rate(8867-4)] from 2019-04-22 */},
      {/* Entry Observation with code [Heart rate(8867-4)] from 2016-04-18 */},
      {/* Entry Observation with code [Respiratory rate(9279-1)] from 2020-03-03 */},
      {/* Entry Observation with code [Respiratory rate(9279-1)] from 2019-04-22 */},
      {/* Entry Observation with code [Respiratory rate(9279-1)] from 2016-04-18 */}
    ],
    "total": 23,
    "link": [
      {
        "relation": "self",
        "url": "<base-url>/Observation/$lastn?max=3&patient=8c5a23b8-5154-a665-8704-35f0e7a386e9&category=vital-signs&_count=23&_skip=0"
      }
    ],
    "id": "6d6571c3-e6e0-461e-803f-c044c442191c"
  }


Fetch the last laboratory results for a patient
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Request

::

   GET <base-url>/Observation/$lastn?patient=8c5a23b8-5154-a665-8704-35f0e7a386e9&category=laboratory
   Accept: application/fhir+json

Response

.. code-block:: JavaScript

  {
    "resourceType": "Bundle",
    "type": "searchset",
    "timestamp": "2021-09-14T12:28:40.943+04:00",
    "meta": {
      "lastUpdated": "2021-09-14T12:28:40.943+04:00",
      "versionId": "748c3f1e-1199-44b8-a3c7-f06f1b1b6b49"
    },
    "entry": [
      {/* Entry Observation with code [Microalbumin Creatinine Ratio(14959-1)] from 2019-04-22 */},
      {/* Entry Observation with code [Low Density Lipoprotein Cholesterol(18262-6)] from 2019-04-22 */},
      {/* Entry Observation with code [Carbon Dioxide(20565-8)] from 2019-04-22 */},
      {/* Entry Observation with code [Chloride(2069-3)] from 2019-04-22 */},
      {/* Entry Observation with code [High Density Lipoprotein Cholesterol(2085-9)] from 2019-04-22 */},
      {/* Entry Observation with code [Total Cholesterol(2093-3)] from 2019-04-22 */},
      {/* Entry Observation with code [Erythrocyte distribution width [Entitic volume] by Automated count(21000-5)] from 2016-04-18 */},
      {/* Entry Observation with code [Glucose(2339-0)] from 2019-04-22 */},
      {/* Entry Observation with code [Triglycerides(2571-8)] from 2019-04-22 */},
      {/* Entry Observation with code [Sodium(2947-0)] from 2019-04-22 */},
      {/* Entry Observation with code [Platelet distribution width [Entitic volume] in Blood by Automated count(32207-3)] from 2016-04-18 */},
      {/* Entry Observation with code [Platelet mean volume [Entitic volume] in Blood by Automated count(32623-1)] from 2016-04-18 */},
      {/* Entry Observation with code [Estimated Glomerular Filtration Rate(33914-3)] from 2019-04-22 */},
      {/* Entry Observation with code [Creatinine(38483-4)] from 2019-04-22 */},
      {/* Entry Observation with code [Hematocrit [Volume Fraction] of Blood by Automated count(4544-3)] from 2016-04-18 */},
      {/* Entry Observation with code [Hemoglobin A1c/Hemoglobin.total in Blood(4548-4)] from 2019-04-22 */},
      {/* Entry Observation with code [Calcium(49765-1)] from 2019-04-22 */},
      {/* Entry Observation with code [Potassium(6298-4)] from 2019-04-22 */},
      {/* Entry Observation with code [Urea Nitrogen(6299-2)] from 2019-04-22 */},
      {/* Entry Observation with code [Leukocytes [#/volume] in Blood by Automated count(6690-2)] from 2016-04-18 */},
      {/* Entry Observation with code [Hemoglobin [Mass/volume] in Blood(718-7)] from 2016-04-18 */},
      {/* Entry Observation with code [Platelets [#/volume] in Blood by Automated count(777-3)] from 2016-04-18 */},
      {/* Entry Observation with code [MCH [Entitic mass] by Automated count(785-6)] from 2016-04-18 */},
      {/* Entry Observation with code [MCHC [Mass/volume] by Automated count(786-4)] from 2016-04-18 */},
      {/* Entry Observation with code [MCV [Entitic volume] by Automated count(787-2)] from 2016-04-18 */},
      {/* Entry Observation with code [Erythrocytes [#/volume] in Blood by Automated count(789-8)] from 2016-04-18 */},
      {/* Entry Observation with code [Rhinovirus RNA [Presence] in Respiratory specimen by NAA with probe detection(92130-4)] from 2020-03-03 */},
      {/* Entry Observation with code [Respiratory syncytial virus RNA [Presence] in Respiratory specimen by NAA with probe detection(92131-2)] from 2020-03-03 */},
      {/* Entry Observation with code [Human metapneumovirus RNA [Presence] in Respiratory specimen by NAA with probe detection(92134-6)] from 2020-03-03 */},
      {/* Entry Observation with code [Parainfluenza virus 3 RNA [Presence] in Respiratory specimen by NAA with probe detection(92138-7)] from 2020-03-03 */},
      {/* Entry Observation with code [Parainfluenza virus 2 RNA [Presence] in Respiratory specimen by NAA with probe detection(92139-5)] from 2020-03-03 */},
      {/* Entry Observation with code [Parainfluenza virus 1 RNA [Presence] in Respiratory specimen by NAA with probe detection(92140-3)] from 2020-03-03 */},
      {/* Entry Observation with code [Influenza virus B RNA [Presence] in Respiratory specimen by NAA with probe detection(92141-1)] from 2020-03-03 */},
      {/* Entry Observation with code [Influenza virus A RNA [Presence] in Respiratory specimen by NAA with probe detection(92142-9)] from 2020-03-03 */},
      {/* Entry Observation with code [Adenovirus A+B+C+D+E DNA [Presence] in Respiratory specimen by NAA with probe detection(94040-3)] from 2020-03-03 */},
      {/* Entry Observation with code [SARS-CoV-2 RNA Pnl Resp NAA+probe(94531-1)] from 2020-03-03 */}
    ],
    "total": 36,
    "link": [
      {
        "relation": "self",
        "url": "<base-url>/Observation/$lastn?patient=8c5a23b8-5154-a665-8704-35f0e7a386e9&category=laboratory&_count=36&_skip=0"
      }
    ],
    "id": "b6521ba6-6235-4221-95cd-e0f25edd77dc"
  }



Get the most recent Observations in category vital-signs conducted before January 1, 2015
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Request

::

    GET <base-url>/Observation/$lastn?patient=8c5a23b8-5154-a665-8704-35f0e7a386e9&category=vital-signs&date=lt2015-01-01
    Accept: application/fhir+json

Response

.. code-block:: JavaScript

  {
    "resourceType": "Bundle",
    "type": "searchset",
    "timestamp": "2021-09-14T12:35:32.952+04:00",
    "meta": {
      "lastUpdated": "2021-09-14T12:35:32.952+04:00",
      "versionId": "1b88af29-6f90-4a73-8d21-bf4594f45fec"
    },
    "entry": [
      {/* Entry Observation with code [Body Weight(29463-7)] from 2013-04-15 */},
      {/* Entry Observation with code [Body Mass Index(39156-5)] from 2013-04-15 */},
      {/* Entry Observation with code [Pain severity - 0-10 verbal numeric rating [Score] - Reported(72514-3)] from 2013-04-15 */},
      {/* Entry Observation with code [Body Height(8302-2)] from 2013-04-15 */},
      {/* Entry Observation with code [Blood Pressure(85354-9)] from 2013-04-15 */},
      {/* Entry Observation with code [Heart rate(8867-4)] from 2013-04-15 */},
      {/* Entry Observation with code [Respiratory rate(9279-1)] from 2013-04-15 */}
    ],
    "total": 7,
    "link": [
      {
        "relation": "self",
        "url": "<base-url>/Observation/$lastn?patient=8c5a23b8-5154-a665-8704-35f0e7a386e9&category=vital-signs&date=lt2015-01-01&_count=7&_skip=0"
      }
    ],
    "id": "b4178262-9bd3-4d9e-b4de-1578cb5d92de"
  }

Fetch the last 3 body weight and body height measurements for a patient
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Request

::

    GET <base-url>/Observation/$lastn?max=3&patient=8c5a23b8-5154-a665-8704-35f0e7a386e9&category=vital-signs&code=29463-7,8302-2
    Accept: application/fhir+json

Response

.. code-block:: JavaScript

  {
    "resourceType": "Bundle",
    "type": "searchset",
    "timestamp": "2021-09-14T12:55:06.929+04:00",
    "meta": {
      "lastUpdated": "2021-09-14T12:55:06.929+04:00",
      "versionId": "3dd3bcde-cbfb-4003-98d7-d7c2f3194c8a"
    },
    "entry": [
      {/* Entry Observation with code [Body Weight(29463-7)] from 2020-03-03 */},
      {/* Entry Observation with code [Body Weight(29463-7)] from 2019-04-22 */},
      {/* Entry Observation with code [Body Weight(29463-7)] from 2016-04-18 */},
      {/* Entry Observation with code [Body Height(8302-2)] from 2019-04-22 */},
      {/* Entry Observation with code [Body Height(8302-2)] from 2016-04-18 */},
      {/* Entry Observation with code [Body Height(8302-2)] from 2013-04-15 */}
    ],
    "total": 6,
    "link": [
      {
        "relation": "self",
        "url": "<base-url>/Observation/$lastn?max=3&patient=8c5a23b8-5154-a665-8704-35f0e7a386e9&category=vital-signs&code=29463-7,8302-2&_count=6&_skip=0"
      }
    ],
    "id": "49ee0b4b-00bd-40b7-8cb5-96a0e0892380"
  }


License
-------
The $lastn operation is part of the core Firely Server functionality. However, to use it, you may need to request an updated license from Firely. You can use your current license file if it contains ``http://fire.ly/vonk/plugins/lastn``.