.. _gdpr:

GDPR compliance - 🇪🇺
====================

Firely Server is a well-tested, secure HL7 FHIR® server that enables you to comply with the technical and organizational measures of the EU General Data Protection Regulation (GDPR).

On this page, we will detail how you can achieve compliance for your Firely Server deployment. To ensure your organization's specific use case, environment, and deployment are compliant, feel free to :ref:`contact us <vonk-contact>`: we'd be happy to help.
We also recommend checking `<https://gdprchecklist.io/>`_ for information. The following sections only focus on technical requirements; organizational requirements are out-of-scope for this document.

.. attention::

    Firely is not a Data Processor as defined in the GDPR, as Firely does not process any data stored in Firely Server.

Within the GDPR, chapter three defines a list of granted rights for a data subject. For some of these rights, Firely Server offers functionality to support the execution of these rights.

Right to rectification
----------------------

Every resource in Firely Server can be updated using the Restful API, given that the executing user/client has sufficient rights and permissions.
If the data subject requests rectification of their data, a data processor could update the resources containing incomplete or wrong information.

Right to erasure
----------------

Please note that the Restful "DELETE" operation is not GDPR compliant by default. After a DELETE, a resource is only marked in the database as deleted.
This behavior might be needed to comply with other (regulatory) obligations to retain records. In case it is allowed to permanently delete a resource, Firely Server offers the :ref:`$erase operation <erase>` to delete a resource without retaining history.

Conditions for consent
----------------------

FHIR offers the functionality of capturing Consent information using the `Consent resource <https://www.hl7.org/fhir/r4/consent.html>`_.
Firely Server offers a plugin framework based on which a Consent check can be implemented based on concrete business requirements.

Right to portability
--------------------

All resource belonging to a data subject can be exported by either querying individual resources through the Restful API or by using the :ref:`Bulk Data Export option <feature_bulkdataexport>`.
If the data subject is a Patient, all resources belonging to the Patient can be queried or exported using its Patient compartment.