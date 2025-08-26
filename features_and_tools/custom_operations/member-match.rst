.. _member-match:

HRex Member Match - $member-match
=================================

.. note::

  The features described on this page are available in the following :ref:`Firely Server editions <vonk_overview>`:

  * Firely Prior Authorization - 🇺🇸

The HRex Member Match operation is used to find the identifier of a member of a health plan, with the member's demographic and coverage information as input.
The retrieved identifier can then be used to perform subsequent queries and operations on the data that the health plan holds for this member.

The operation is defined as part of the `Da Vinci Health Record Exchange (HRex) Implementation Guide <https://hl7.org/fhir/us/davinci-hrex>`_ in coordination with the `Da Vinci Payer Data Exchange Implementation Guide <https://hl7.org/fhir/us/davinci-pdex/>`_. See :ref:`davinci_pdex_ig` for supported versions.

Matching algorithm
------------------

The Patient and Coverage resources that are input parameters to the operation, have search parameters defined on them.
Firely Server will match the Patient and Coverage resources based on all elements that have any of these search parameters defined on them.
E.g. if Patient.name.family is provided, the operation will match on the Patient.family and Patient.name search parameters.

This means that the more elements are included in these resources, the less likely it is to find any exact match.
If too little elements are included, multiple members may match, which will result in a ``422 Unprocessable Entity`` response.
Therefore clients are advised to exclude elements that are commonly not stable across different records, e.g. Patient.address.

For the matching the default behavior of FHIR Search is used. Most importantly strings are matched on a case-insensitive left-match ('starts-with').

If the setting ``IsDemographicOnlyMatchAllowed`` is set to ``false`` (the default), the operation requires an identifier element to be present on either the Patient or Coverage resource, so it can match on that.

Deviations
----------

The following deviations from the specification were made:

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

.. code-block:: JavaScript

    "PipelineOptions": {
        "PluginDirectory": "./plugins",
        "Branches": [
          {
            "Path": "/",
            "Include": [
              // ...
              "Vonk.Plugin.MemberMatch",
            ],
            "Exclude": [
              // ...
            ]
          }, // ...etc...

Check that the operation is listed as supported
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

The ``$member-match`` should be listed in the ``Operations`` section of the :ref:`appsettings <disable_interactions>`.
That is by default the case, but if you have previously overridden this section, you need to make sure that the operation is listed there.

Set the options for the operation
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

The operation has a single option that can be set in the :ref:`appsettings <configure_appsettings>`:

.. code-block:: json

    "MemberMatch": {
        "IsDemographicOnlyMatchAllowed": false // true/false, default is false
    }

If this setting is set to ``true``, the operation will allow for a match based on demographic information only.
Otherwise (by default) the operation requires an identifier element in either the Patient or Coverage resource parameter.