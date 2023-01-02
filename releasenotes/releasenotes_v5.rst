.. _vonk_releasenotes_history_v5:

Current Firely Server release notes (v5.x)
==========================================

.. _vonk_releasenotes_5_0_0:

Release 5.0.0-beta1, TBD, 2023
------------------------------

- Note that this is beta release
- Public API deprecated modules
- Features

Breaking changes
^^^^^^^^^^^^^^^^

The following modules of the public API have been deprecated: 

#. 

Feature
^^^^^^^

#. Bulk Data Export now supports SMART on FHIR v2.
#. Contents of the audit event log can now be modified via a plugin.
#. Firely Server now uses the Firely .NET SDK 4.3.0.
#. The default information model for Firely Server is now R4.

Fix
^^^

#. Bulk Data Export now returns an empty array with a succesful status code if no resources are exported instead of returning an erroneous status code.
#. Empty search parameters are now ignored by the server instead of resulting in an error response.
#. Firely Server now creates valid R5 AuditEvents.
#. Firely Server now supports searching on version-specific references.
#. Searching for a resource with multiple sort fields does not throw an exception anymore.
#. Fields that are included in the audit event log and AuditEvent resources now contain the same content.
#. When using the If-Modified-Since Header, only resources are returned that were modified after the specified timestamp. Before this fix, wrong resources were sometimes returned because of a precision mismatch (seconds vs. milliseconds).

Configuration
^^^^^^^^^^^^^
#. The configuration section for additional endpoints in the discovery document and additional issuers in tokens has been reworked. 