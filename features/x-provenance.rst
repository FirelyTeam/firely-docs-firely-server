.. _feature_x-provenance:

X-Provenance header
===================

Description
-----------

The X-Provenance header can be used to add a Provenance resource upon creating or updating another resource. See `Provenance#header <https://www.hl7.org/fhir/Provenance.html#header>`_ for more information.

.. note:: In the case of a conditional create, where the resource was already present, the Provenance resource is not created.

Linking the Provenance resource
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Provenance must point to its target resource through the ``target`` property (see `Provenance#resource <https://www.hl7.org/fhir/Provenance.html#resource>`_)
The ``target`` will be automatically deduced from the created/updated resource and does not need to be included in the header. 
The reference will point to the current version (after create/update) of the resource.

Validation of the Provenance resource
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Validation of the Provenance resource follows the settings for validation (see :ref:`feature_prevalidation`). 

.. note:: If validation is disabled, an invalid Provenance resource in the header will be ignored and the request will complete normally (if valid).

Limitations
^^^^^^^^^^^

The maximum header length of the web server applies (e.g. IIS 8KB/16KB).

Example
-------

.. code-block:: JavaScript

  POST {base-url}/Patient

  X-Provenance: { "resourceType": "Provenance", "text": { "status": "generated", "div": "<div>Record of change</div>" }, "recorded": "2022-08-24T11:05:24+02:00", "agent": [ { "who": { "reference": "[mandatory reference]" } } ] }

  {
      "resourceType": "Patient",
      "active": true,
      "name": [
        {
          "use": "official",
          "family": [
            "Doe"
          ],
          "given": [
            "John"
          ]
        }
      ],
      "gender": "male",
      "birthDate": "1974-12-25"
  }

Update ``[mandatory reference]`` to point to a valid resource in your system.

License
-------
The X-Provenance header is available for licenses with the Transaction feature enabled.