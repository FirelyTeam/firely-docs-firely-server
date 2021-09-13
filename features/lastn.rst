.. _lastn:

$lastN Observations
===================

Description
-----------

This plugin implements the Observation $lastn operation, as described in https://www.hl7.org/fhir/observation-operation-lastn.html. This operation returns the most recent or last n=number of observations for a patient within a given category (e.g. vital-signs or laboratory).

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
::

   GET <base-url>/Observation/$lastn?max=3&patient=8c5a23b8-5154-a665-8704-35f0e7a386e9&category=vital-signs
   Accept: <any supported FHIR media type>


Fetch the last laboratory results for a patient
::

   GET <base-url>/Observation/$lastn?patient=8c5a23b8-5154-a665-8704-35f0e7a386e9&category=laboratory
   Accept: <any supported FHIR media type>


Get the most recent Observations in category vital-signs conducted before January 1, 2015
::

    GET <base-url>/Observation/$lastn?patient=8c5a23b8-5154-a665-8704-35f0e7a386e9&category=vital-signs&date=lt2015-01-01 HTTP/1.1
    Accept: <any supported FHIR media type>

Fetch the last 3 body weight and body height measurements for a patient
::

    GET <base-url>/Observation/$lastn?max=3&patient=8c5a23b8-5154-a665-8704-35f0e7a386e9&category=vital-signs&code=29463-7,8302-2 HTTP/1.1
    Accept: <any supported FHIR media type>

License
-------
The $lastn operation is part of the core Firely Server functionality. However, to use it, you may need to request an updated license from Firely. You can use your current license file if it contains ``http://fire.ly/vonk/plugins/lastn``.