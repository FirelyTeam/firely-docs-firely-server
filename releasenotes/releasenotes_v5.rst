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

#. G 10 certification
#. Bulk Data Export now supports SMART on FHIR v2.
#. Contents of the audit event log can now be modified via a plugin.
#. Firely Server now uses the Firely .NET SDK 4.3.0.
#. The default information model for Firely Server is now R4.
#. Firely Server will now handle duplicate DLLs and assemblies more gracefully in case you accidently added them to its plugin directory.

Fix
^^^

#. Bulk Data Export now returns an empty array with a succesful status code if no resources are exported instead of returning an erroneous status code.
#. Empty search parameters are now ignored by the server instead of resulting in an error response.
#. Firely Server now creates valid R5 AuditEvents.
#. Firely Server now supports searching on version-specific references.
#. Searching for a resource with multiple sort fields does not throw an exception anymore.
#. Fields that are included in the audit event log and AuditEvent resources now contain the same content.
#. When using the If-Modified-Since Header, only resources are returned that were modified after the specified timestamp. Before this fix, wrong resources were sometimes returned because of a precision mismatch (seconds vs. milliseconds).
#. When updating a deleted resource conditionally, Firely Server does not throw an exception anymore.
#. Firely Server now returns the correct issue code when performing a conditional update using _id as a parameter and matching the id in a different information model.
#. When executing a POST-based search, Firely Server will now return the correct self-link.
#. Upon commencing a Bulk Data Export, Firely Server now correctly handles 'Prefer' headers.
#. Device can now be added as an additional resource in a Bulk Data export.
#. The client id of the default SMART authorization options have been changed from 'vonk' to 'firelyserver'.
#. Firely Server now returns improved error messages if the client is not allowed to perform searches.
#. Support for Firely Server using a SQLite database running on M1 Macs was improved. 
#. During SMART on FHIR v2 discovery, Firely Server now returns the `grant_types_supported` field.
#. Firely Server now returns the correct CodeSystem http://terminology.hl7.org/CodeSystem/restful-security-service within the security section of its CapabilityStatement.
#. During a Bulk Data Export request with a SQL database, Firely Server now returns the Group resource, even if it has no members. 

Configuration
^^^^^^^^^^^^^
#. The configuration section for additional endpoints in the discovery document and additional issuers in tokens has been reworked. 

Deprecation
^^^^^^^^^^^

#. Vonk Loader has been deprecated.