.. _member-match:

HRex Member Match Operation
===========================

The HRex Member Match operation is used to find the identifier of a member of a health plan, with the member's demographic and coverage information as input.
The retrieved identifier can then be used to perform subsequent queries and operations on the data that the health plan holds for this member.

The operation is defined as part of the `Da Vinci Health Record Exchange (HRex) Implementation Guide <https://hl7.org/fhir/us/davinci-hrex>`_. Version 1.0.0 was used as the basis for this implementation.

Matching algorithm
------------------

The Patient and Coverage resources that are input parameters to the operation, have search parameters defined on them.
Firely Server will match the Patient and Coverage resources based on all elements that have any of these search parameters defined on them.
E.g. if Patient.name.family is provided, the operation will match on the Patient.family and Patient.name search parameters.

This means that the more elements are included in these resources, the less likely it is to find any exact match.
If too little elements are included, multiple members may match, which will result in a ``422 Unprocessable Entity`` response.
For the matching the default behavior of FHIR Search is used. Most importantly strings are matched on a case-insensitive left-match ('starts-with').

Deviations
----------

The following deviations from the specification were made:

#. If the required HRex or US-Core profiles are not available in Firely Servers' administration database, the server will not be able to validate the Parameters resource in the request. 
    Firely Server will log a warning, but process the request nonetheless. See configuration below on solving this warning.
#. The ``Consent`` parameter is not required by Firely Server. The IG does not provide any guidance on how to process a Consent parameter, so Firely Server will ignore it.
    In version 1.1.0 of the IG this parameter is not required in the profile anymore either.
    
Configuration
-------------

Enable the $member-match operation
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Enabling the operation requires a few steps. If the operation is enabled correctly, it will be listed in the CapabilityStatement with its canonical url: ``http://hl7.org/fhir/us/davinci-hrex/OperationDefinition/member-match``.

Check the license
~~~~~~~~~~~~~~~~~

The ``$member-match`` requires the license token ``http://fire.ly/vonk/plugins/member-match`` to be present in the license file.
If you do not have this license token, please contact `Firely <https://fire.ly/contact>`_.

Include the plugin in the pipeline
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

In the ``PipelineOptions`` section of the :ref:`appsettings <configure_appsettings>`, add the namespace of the plugin:

.. code-block:: json

    "PipelineOptions": {
        "PluginDirectory": "./plugins",
        "Branches": [
          {
            "Path": "/",
            "Include": [
              ...
              "Vonk.Plugin.MemberMatch",
            ],
            "Exclude": [
              ...
            ]
          }, ...etc...

Assure that the profiles are available
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

By default Firely Server uses a SQLite administration database, that is part of the distribution. This database contains the profiles that are used for validation. 

If you are using SQL Server or MongoDB as the administration database, you need to make sure that the HRex and US-Core profiles are available in the database.
To do so:

- download the package for US-Core 3.1 from the `downloads page <http://hl7.org/fhir/us/core/STU3.1.1/downloads.html>`_.
- this is a `.tgz` (tarball) file, so you need to extract it, and repackage at least the profiles into a zip file.
- download the resource definitions for HRex from the `downloads page <https://hl7.org/fhir/us/davinci-hrex/downloads.html>`_.
- put both zip files in the administration import folder. See :ref:`conformance` for more information.

Note that HRex 1.0.0 still depends on US-Core 3.1.0. 

If the HRex or US-Core profiles are not available in this database, the server will not be able to validate the Parameters resource in the request.
It will issue a warning in the log, but process the request nonetheless.
