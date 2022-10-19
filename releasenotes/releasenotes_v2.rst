.. _vonk_releasenotes_history:

Old Firely Server release notes (v2.x and earlier)
==================================================

.. _vonk_releasenotes_220:

Release 2.2.0
-------------

Database
^^^^^^^^

#. SQL Server: the index tables have their clustered index on their link to the resources. 
   SQL script '20190919000000_Cluster_Indexes_On_EntryId.sql' (found in the /data folder of the Vonk distribution) must be applied to existing Vonk SQL databases (both to the admin and to the data repositories) to make this change. 

   .. attention::

      Vonk 2.2.0 (using SQL server) will not start unless this script has been applied to the databases. Please note that running the script can take considerable time, especially for large databases.


Features
^^^^^^^^

#. When running Vonk in IIS, it now profits from the `in-process hosting model <https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/?view=aspnetcore-2.2>`_ that ASP.NET Core offers.
#. Further improved concurrent throughput for SQL Server.

Fix
^^^

#. On errors in a transaction, Vonk would not point out the entry if it had no fullUrl. Improved this message by using the resourcetype and id (if present) instead.
#. _include gave a 500 responsecode if a resource contains absolute references.

.. _vonk_releasenotes_210:

Release 2.1.0
--------------------

Database
^^^^^^^^

#. SQL Server: Improved concurrent throughput.

Features
^^^^^^^^

#. Upgrade to HL7.Fhir.Net API 1.3, see the `Older SDK release notes`_.
#. Vonk calls ``UseIISIntegration`` for better integration with IIS (if present).

Fix
^^^

#. Transactions: references to other resources in the transaction were not updated if the resource being referenced was in the transaction as an update (PUT).
   (this error was introduced in 2.0.0).

.. _vonk_releasenotes_201:

Release 2.0.1 hotfix
--------------------

Fix
^^^

#. Supported Interactions were not checked for custom operations. In the `appsettings.json` the custom operations, like $meta, were ignored. This has been fixed now.

.. _vonk_releasenotes_200:

Release 2.0.0 final
-------------------

This is the final release of version 2.0.0, so the -beta is off.
If you directly upgrade from version 1.1, please also review all the 2.0.0-beta and -beta2 release notes below.

.. attention::

   We upgraded the version of .NET Core used to 2.2. Please get the latest 2.2.x runtime from the `.NET download site <https://www.microsoft.com/net/download/core#/runtime/>`_. The update was needed for several security patches and speed improvements.

.. attention::

   The structure of the Validation section in the settings has changed. See :ref:`feature_prevalidation` for details.

.. attention::

   This version of Vonk is upgraded to the Hl7.Fhir.API version 1.2.0. Plugin- and Facade builders will transitively get this dependency through the Vonk.Core package.

Database
^^^^^^^^

No changes have been made to any of the database implementations.

Fix
^^^

#. When you created a StructureDefinition for a new resourcetype on /administration, the corresponding endpoint was not enabled. 
#. Vonk does not update references in a transaction when a conditional create is used for an existing resource.
#. Paths in PipelineOptions would interfere if one was the prefix of the other.
#. Indexing a HumanName with no values but just extensions failed.
#. The selflink in a bundle did not contain the sort parameters. In this version the selflink always contains a sort and a count parameter, even if they were not in the request and the default values have been applied.
#. The import of conformance resources from specification.zip yielded warnings on .sch files.
#. Errors introduced in the 2.0.0-beta versions:
   
   #. Syntax errors in the XML or JSON payload yielded an exception, now they are reported with an OperationOutcome upon parsing.
   #. $expand and other terminology operations caused a NullReference exception.
   #. _element did not include the mandatory elements.

Feature
^^^^^^^

#. Vonk supports Custom Resources. See :ref:`feature_customresources`.
#. Operation :ref:`feature_meta` is now supported, to quickly get the tags, security labels and profiles of a resource.
#. /metadata, retrieving the CapabilityStatement performs a lot better (just the initial call for a specific Accept-Type takes a bit longer).
#. Validation can be controlled more detailed. Choose the strictness of parsing independent of the level of validation. With this, the settings section 'Validation' has also changed. See :ref:`feature_prevalidation`. 

Plugin and Facade API
^^^^^^^^^^^^^^^^^^^^^

#. We upgraded the embedded Fhir.Net API to version 1.2, see the `Older SDK release notes`_.
#. Together with the upgrade to .NET Core 2.2, several libraries were updated as well. Most notably Microsoft.EntityFrameworkCore.*, to 2.2.3.

.. _vonk_releasenotes_200-beta2:

Release 2.0.0-beta2
-------------------

Fix
^^^

* Fixed RelationalQuery in Vonk.Facade.Relational, so Vonk.Facade.Starter can be used again.

.. _vonk_releasenotes_200-beta:

Release 2.0.0-beta
------------------

We have refactored Vonk internally to accomodate future changes. There are only minor functional changes to the FHIR Server.
Facade and Plugin builders must be aware of a few interface changes, most notably to the IResource interface. 

This release is a *beta* release because of the many internal changes, and because we expect to include a few more in the final release. 
Have a go with it in your test environment to see whether you encounter any trouble. We also encourage you to build your plugin and/or facade against it to prepare for code changes upon the final release.

You can still access the latest final release (1.1.0):

* Binaries: through the `Simplifier downloads page <https://simplifier.net/downloads/vonk>`_, choose 'List previous versions'.
* Docker: ``docker pull simplifier/vonk:1.1.0``
* NuGet: ``<PackageReference Include="Vonk.Core" Version="1.1.0" />``

Database
^^^^^^^^

No changes have been made to any of the database implementations.

Fix
^^^

#. The :ref:`$validate <feature_validation>` operation processes the profile parameter.
#. If an update brings a resource 'back to life', Vonk returns statuscode 201 (previously it returned 200). 
#. On an initial Administration Import of specification.zip, Vonk found an error in valueset.xml. This file was fixed in the specification.zip that comes with Fhir.NET API 1.1.2.
#. Transaction: references within the transaction are automatically changed to the id's the referenced resources get from Vonk when processing the transaction. This did not happen for references inside extensions. It does now. 
#. Administration Import: an Internal Server Error could be triggered with a zip file with nested directories in it.

   * NB: Directories in your zip are still not supported because of `Fhir.NET API issue #883 <https://github.com/FirelyTeam/firely-net-sdk/issues/883>`_, but Vonk will not error on it anymore.

#. Search: The entry.fullUrl for an OperationOutcome in a Search bundle had a relative url.
#. Search: Processed _elements and _summary arguments were not reported in the selflink of the bundle (or any of the paging links).
#. Search: The selflink will include a _count parameter, even if it was not part of the request and hence the default value for _count from the :ref:`BundleOptions <bundle_options>` was applied.
#. Search on :exact with an escaped comma (e.g. ``/Patient?name:exact=value1\,value2``) was executed as a choice. Now the escape is recognized, and the argument processed as one term.

Feature
^^^^^^^

#. Upgraded Fhir.NET API to version 1.1.2, see the `Older SDK release notes`_.
#. The Vonk Administration API now allows for StructureMap and GraphDefinition resources to be loaded.
#. The opening page of Vonk (and the only UI part of it) is updated. It no longer contains links that you can only execute with Postman, and it has a button that shows you the CapabilityStatement.
#. We published our custom operations on `Simplifier <https://simplifier.net/vonk-resources>`_! And integrated those links into the CapabilityStatement.
#. You can now access older versions of the Vonk binaries through the Simplifier downloads. (This was already possible for the Docker images and NuGet packages through their respective hubs).
#. `Vonk.IdentityServer.Test <https://github.com/FirelyTeam/Vonk.IdentityServer.Test/>`_ and `Vonk.Facade.Starter <https://github.com/FirelyTeam/Vonk.Facade.Starter>`_ have been integrated into the Continuous Integration system.
#. In JSON, the order of the output has changed:
   
   #. If id and/or meta elements were added by Vonk (on a create or update), they will appear at the end of the resource.

Plugin and Facade API
^^^^^^^^^^^^^^^^^^^^^

#. IResource interface and related classes have had several changes. If you encounter problems with adapting your code, please contact us.

   * It derives from the ISourceNode interface from the Fhir.NET API.
   * Change and Currency are properties that were only relevant in the repository domain, and not in the rest of the pipeline. They have been deprecated. 
     You can access the values still with resource.GetChangeIndicator() and resource.GetCurrencyIndicator(). This is implemented with Annotations on the ISourceNode. 
     All of Vonk's own implementations retain those annotations, but if the relevant annotation is somehow missing, default values are returned (ResourceChange.NotSet resp. ResourceCurrency.Current).
   * The Navigator property is obsolete. The type of it (IElementNavigator) is obsolete in the Fhir.NET API. To run FhirPath you provide type information and run the FhirPath over an ITypedElement::

      //Have IStructureDefinitionSummaryProvider _schemaProvider injected in the constructor.
      var typed = resource.ToTypedElement(_schemaProvider);
      var matchingElements = typed.Select('your-fhirpath-expression'); 

   * Id, Version and LastUpdated can no longer be set directly on the IResource instance. IResource has become **immutable** (just like ISourceNode). The alternatives are::

      var resourceWithNewId = resource.SetId("newId");
      var resourceWithNewVersion = resource.SetVersion("newVersion");
      var resourceWithNewLastUpdated = resource.SetLastUpdated(DateTimeOffset.UtcNow);

   * Because the IChangeRepository is responsible for creating new id's and versions, we also included extensions methods on it to update all three fields at once::

      var updatedeResource = changeRepository.EnsureMeta(resource, KeepExisting.Id / Version / LastUpdated);
      var updatedResource = changeRepository.FreshMeta(resource); //replaces all three

#. The PocoResource class is obsolete. To go from a POCO (like an instance of the Patient class) to an IResource, use the ToIResource() extension method found in Vonk.Fhir.R3.
#. The PocoResourceVisitor class is obsolete. Visiting can more effectively be done on an ITypedElement::

      //Have IStructureDefinitionSummaryProvider _schemaProvider injected in the constructor.
      var typed = resource.ToTypedElement(_schemaProvider);
      typed.Visit((depth, element) => {//do what you want with element});

#. SearchOptions has changed:

   * Properties Count and Offset have been removed.
   * Instead, use _count and _skip arguments in the IArgumentCollection provided to the SearchRepository.Search method if you need to.

#. We have created a template for a plugin on `GitHub <https://github.com/FirelyTeam/Vonk.Plugin.ExampleOperation>`_. Fetch it for a quick start of your plugin.

.. _vonk_releasenotes_110:

Release 1.1.0
-------------

.. attention::
   
   New security issues have been identified by Microsoft. See the :ref:`vonk_securitynotes` for details.

.. attention::

   The setting for the location of the license file has moved. It was in the top level setting ``LicenseFile``. It still has the same name, but it has moved into the section ``License``. See :ref:`configure_license` for details.

.. attention::

   This version of Vonk is upgraded to the Hl7.Fhir.API version 1.1.1. Plugin- and Facade builders will transitively get this dependency through the Vonk.Core package.

Database
^^^^^^^^

No changes have been made to any of the database implementations.

Feature
^^^^^^^

#. Vonk will count the number of requests that it processes. See :ref:`configure_license` for settings on that. Because of this change, the ``LicenseFile`` setting has moved from the top level to under ``License``.
#. The plugin folder (:ref:`settings_pipeline`) may now contain subfolders. Plugins will be read from all underlying folders.
#. Vonk supports If-Match on update. See `Managing Resource Contention <http://hl7.org/fhir/http.html#concurrency>`_ in the specification for details.
#. Plugins may return non-FHIR content. See :ref:`vonk_plugins_directhttp`.
#. This feature may also be used for :ref:`accesscontrol_custom_authentication`.
#. A :ref:`vonk_plugins_template` is added to the documentation.
#. A documentation page on performance is added.
#. Upgrade of the Hl7.Fhir.API library to 1.1. See the `Older SDK release notes`_.

Fix
^^^

#. Transaction: forward references from one resource to another in a Transaction were not correctly resolved.
#. When you set ValidateIncomingResources to true, Vonk no longer accepts resources with extensions that are unknown to it. This is now also reflected in the CapabilityStatement.acceptUnknown.
#. The links in a bundle response (``Bundle.link``) were relative links. Now they are absolute links.
#. HTTP 500 instead of an OO was returned when trying to update a subscription with an invalid request status.
#. If an error is found in a SearchParameter in the Administration database, Vonk logs the (canonical) url of that SearchParameter for easier reference.
#. Transaction: Response bundle contained versioned fullUrls. We changed that to unversioned urls.
#. Bundles: Response bundles with an OperationOutcome contained a versioned fullUrl for the entry containing the OperationOutcome. We changed that to an unversioned url. 
#. Deleting a resource from the Administration API that does not exist would lead to an internal server error.

Supported Plugins
^^^^^^^^^^^^^^^^^

#. Several fixes have been done on the `Document plugin <https://github.com/FirelyTeam/Vonk.Plugin.DocumentOperation>`_.

.. _vonk_releasenotes_100:

Release 1.0.0
-------------

Yes! Vonk version 1.0 is out. It is also the first version that is released withouth the -beta postfix. It has been very stable from the very first version, and now we think it is time to make that formal. 

Release 1.0.0 is functionally identical to 0.7.4.0. But we optimized the deployment process for :ref:`yellowbutton` and :ref:`Docker <use_docker>` in general. The contents of the core specification are now preloaded in the SQLite administration database, so your first startup experience is a lot faster.

.. _vonk_releasenotes_0740:

Release 0.7.4.0
---------------

Database
^^^^^^^^

#. The index definitions for SQL Server have been updated for improved performance. This should be handled automatically when you start Vonk 0.7.4 and have :ref:`AutoUpdateDatabase <configure_sql>` enabled.

Fix
^^^

#. Posting a resource with an invalid content-type to the regular FHIR endpoint should result in HTTP 415 and not HTTP 400.
#. Warning 'End method "PocoResourceVisitor.VisitByType", could not cast entity to PocoResource.' in the log was incorrect.
#. When running Administration API on SQLite and Vonk on SQL Server, update or delete would fail.
#. Handle quantity with very low precision (e.g. '3 times per year' - 3|http://unitsofmeasure.org|/a).
#. POST to <vonk_base>/Administration/* with another Content-Type than application/json or application/xml results in HTTP 500.

Feature
^^^^^^^

#. Support forward references in a :ref:`Transaction bundle <restful_transaction>`. Previously Vonk would only process references back to resources higher up in the bundle.
#. Performance of Validation and Snapshot Generation has improved by approximately 10 times...
#. ... and correctness has improved as well.
#. Administration API also support the NamingSystem resource.

.. _vonk_releasenotes_0730:

Release 0.7.3.0
---------------

Fix
^^^
#. Search on /administration/Subscription was broken
#. Neater termination of the Subscription evaluation process upon Vonk shutdown
#. A Bundle of type batch is now rejected if it contains internal references.
#. Urls in the narrative (href and src) are also updated to the actual location on the server.
#. A system wide search on compartment returns 403, explaining that that is too costly. 

.. _vonk_releasenotes_0721:

Release 0.7.2.1
---------------

Fix
^^^

#. Delete on /administration was broken.

.. _vonk_releasenotes_0720:

Release 0.7.2.0
---------------

Database
^^^^^^^^

#. Fixes 2 and 3 require a reindex for specific searchparameters, if these parameters are relevant to you.

Features and fixes
^^^^^^^^^^^^^^^^^^

#. Fix: Reject a search containing a modifier that is incorrect or not supported.
#. Fix: The definition for searchparameter Encounter.length was unclear. We added the correct definition from FHIR R4 to the errata.zip, so it works for STU3 as well.
   If this is relevant for you, you may want to reindex for this searchparameter. See :ref:`feature_customsp_reindex_specific`, just for 'Encounter.length'.
#. Fix: Error "Unable to index for element of type 'base64Binary'". This type of element is now correctly indexed. 
   One known searchparameter that encounters this type is Device.udi-carrier. If this is relevant to you, you may want to reindex for this searchparameter. See :ref:`feature_customsp_reindex_specific`, just for 'Device.udi-carrier'.
#. Fix: Validation would fail on references between contained resources. See also fix #423 in the `Older SDK release notes`_.
#. Fix: E-tag was missing from the response on a delete interaction.
#. Fix: An invalid mimetype in the _format parameter (like _format=application/foobar) returned response code 400 instead of 415.
#. Fix: If a subscription errors upon execution, not only set the status to error, but also state the reason in Subscription.error for the user to inspect.
#. Fix: Search on /Observation?value-string:missing=false did not work. As did the missing modifier on other searchparameters on value[x] elements.
#. Feature: After /administration/importResources (see :ref:`conformance_on_demand`), return an OperationOutcome detailing the results of the operation.
#. Feature: Upon usage of a wrong value for _summary, state the possible, correct values in the OperationOutcome.
#. Feature: Allow for multiple deletes with a Conditional Delete, see :ref:`restful_crud`.
#. Feature: The version of Vonk is included in the log file, at startup.
#. Configuration: Add Vonk.Smart to the PipelineOptions by default, so the user only needs to set the SmartAuthorizationOptions.Enabled to true.
#. Upgrade: We upgraded to the latest C# driver for MongoDb (from 2.4.4 to 2.7.0).

.. _vonk_releasenotes_0711:

Release 0.7.1.1
---------------

Fix
^^^

Spinning up a Docker container would crash the container because there was no data directory for SQlite (the default repository). This has been 
solved now: Vonk will create the data directory when it does not exist. 


.. _vonk_releasenotes_0710:

Release 0.7.1.0
---------------

.. attention::

   Fix nr. 8 requires a reindex/searchparameters with ``include=Resource._id,Resource._lastUpdated,Resource._tag``. 
   Please review :ref:`feature_customsp_reindex` on how to perform a reindex and the cautions that go with it.
   Also note the changes to reindexing in fix nr. 1.

Database
^^^^^^^^

#. We added support for SQLite! See :ref:`configure_sqlite` for details.
#. We also made SQLite the default setting for both the main Vonk database and the :ref:`administration_api`.
#. With the introduction of SQLite we advise running the Administration API on SQLite. In the future we will probably deprecate running the Administration API on any of the other databases.
#. Support for CosmosDB is expanded, though there are a few limitations.

Facade
^^^^^^

#. If you rejected the value for the _id searchparameter in your repository, Vonk would report an InternalServerError. Now it reports the actual message of your ArgumentException.

Features and fixes
^^^^^^^^^^^^^^^^^^

#. We sped up :ref:`feature_customsp_reindex`. The request will be responded to immediately, while Vonk starts the actual reindex asynchronously and with many threads in parallel.
   Users are guarded against unreliable results by blocking other requests for the duration of the reindex.
   Reindexing is still not to be taken lightly. It is a **very heavy** operation that may take very long to complete.
   See :ref:`feature_customsp_reindex` for details. 
#. A really large bundle could lead Vonk (or more specifically: the validator in Vonk) to a StackOverflow. You can now set :ref:`limits <sizelimits_options>` to the size of incoming data to avoid this.
#. :ref:`Reindexing <feature_customsp_reindex>` is supported on CosmosDB, but it is less optimized than on MongoDB.
#. Using _include or _revinclude would yield an OperationOutcome if there are no search results to include anything on. Fixed that to return 404 as it should.
#. Using the :not modifier could return false positives. 
#. A batch or transaction with an entry having a value for IfModifiedSince would fail.
#. History could not be retrieved for a deleted resource. Now it can.
#. :ref:`Reindex <feature_customsp_reindex>` would ignore the generic searchparameters defined on Resource (_id, _lastUpdated, _tag). Because id and lastUpdated are also stored apart from the search index, this was really only a problem for _tag.
   If you rely on the _tag searchparameter you need to reindex **just for the searchparameter ``Resource._tag``**.
#. Vonk logs its configuration at startup. See :ref:`log_configuration` for details.

.. _vonk_releasenotes_0700:

Release 0.7.0.0
---------------

Database
^^^^^^^^

#. Indexes on the SQL Server repository were updated to improve performance. They will automatically be applied with :ref:`AutoUpdateDatabase<configure_sql>`.

Facade
^^^^^^

#. Release 0.7.0.0 is compatible again with Facade solutions built on the packages with versions 0.6.2, with a few minor changes. 
   Please review the Vonk.Facade.Starter project for an example of the necessary adjustments. All the differences can be seen in `this file comparison <https://github.com/FirelyTeam/Vonk.Facade.Starter/commit/ea4734da117e7add0d7155b225f5f320db86919c#diff-c7ac183ffadb9c835e21f6853864bad0>`_.
#. Fix: The SMART authorization failed when you don't support all the resourcetypes. It will now take into account the limited set of supported resourcetypes.
#. Fix: Vonk.Facade.Relational.RelationalQueryFactory would lose a _count argument. 
#. Documentation: We added documentation on how to implement Create, Update and Delete in a facade on a relational database. See :ref:`enablechange`. This is also added to the `example Facade solution <https://github.com/FirelyTeam/Vonk.Facade.Starter/tree/exercise/cud>`_ on GitHub.

Features and fixes
^^^^^^^^^^^^^^^^^^

#. Feature: :ref:`Vonk FHIR Plugins<vonk_plugins>` has been released. You can now add libraries with your own plugins through configuration. 
#. Feature: Through :ref:`Vonk FHIR Pluginss<vonk_plugins>` you can replace the landing page with one in your own style. We provided an :ref:`example<vonk_plugins_landingpage>` on how to do that.
#. Feature: You can now start Vonk from within another directory than the Vonk binaries directory, e.g. ``c:\programs>dotnet .\vonk\vonk.server.dll``.
#. Feature: You can configure the maximum number of entries allowed in a Batch or Transaction, to avoid overloading Vonk. See :ref:`batch_options`.
#. Upgrade: We upgraded the FHIR .NET API to version 0.96.0, see the `Older SDK release notes`_ for details.
   Mainly #599 affects Vonk, since it provides the next...
#. Fix: Under very high load the FhirPath engine would have concurrency errors. The FhirPath engine is used to extract the search parameters from the resources. This has been fixed.
#. Fix: Search on a frequently used tag took far too long on a SQL Server repository.
#. Fix: The `Patient.deceased <http://hl7.org/fhir/patient.html#search>`_ search parameter from the specification had an error in its FhirPath expression. We put a corrected version in the :ref:`errata.zip<feature_errata>`.
#. Fix: Several composite search parameters on Observation are defined incorrectly in the specification, as is reported in `GForge issue #16001 <https://gforge.hl7.org/gf/project/fhir/tracker/?action=TrackerItemEdit&tracker_item_id=16001&start=0>`_. 
   Until the specification itself is corrected, we provide corrections in the :ref:`errata.zip<feature_errata>`.
#. Fix: Relative references in a resource that start with a forward slash (like ``/Patient/123``) could not be searched on.
#. Fix: System wide search within a compartment looked for the pattern ``<base>/Patient/123/?_tag=bla``. Corrected this to ``<base>/Patient/123/*?_tag=bla``
#. Fix: When loading :ref:`Simplifier resources<conformance_fromsimplifier>`, Vonk can now limit this to the changes since the previous import, because the Simplifier FHIR endpoint supports _lastUpdated. 
#. Fix: :ref:`Conformance resources<conformance>` are always loaded into the Administration API when running on a Memory repository. Or actually, always if there are no StructureDefinitions in the Administration database.
   To enable this change, imported files are no longer moved to the :ref:`AdministrationOptions.ImportedDirectory<conformance_import>`.
#. Fix: :ref:`feature_customsp_reindex` would stop if a resource was encountered that could not properly be indexed. It will now continue working and report any errors afterwards in an `OperationOutcome <http://hl7.org/fhir/operationoutcome.html>`_.
#. Fix: The terms and privacy statement on the default landing page have been updated.
#. Fix: When searching on a search parameter of type date, with an argument precision to the minute (but not seconds), Vonk would reject the argument. It is now accepted.
#. Fix: DateTime fields are always normalized to UTC before they are stored. This was already the case on MongoDb, and we harmonized SQL and Memory to do the same. There is no need to reindex for this change. 
#. Fix: When you use accents or Chinese characters in the url for a search, Vonk gives an error.
#. Fix: A reverse chained search on MongoDb sometimes failed with an Internal Server Error. 

.. _vonk_releasenotes_0650:

Release 0.6.5.0
---------------

.. attention::

   This version changes the way conformance resources are loaded from zip files and/or directories at startup. They are no longer loaded only in memory, but are added to the Administration API's database.
   You will notice a delay at first startup, when Vonk is loading these resources into the database. See Feature #1 below.

.. attention::

   2018-06-07: We updated the Database actions for 0.6.5.0, you should always perform a reindex, see right below.

Database
^^^^^^^^

#. Feature 2, 4 and 14 below require a :ref:`reindex/all <feature_customsp_reindex>`, both for MongoDB and SQL Server.

Facade
^^^^^^

#. Release 0.6.5.0 is not released on NuGet, so the latest NuGet packages have version 0.6.2-beta. Keep an eye on it for the next release...

Features and fixes
^^^^^^^^^^^^^^^^^^

#. Feature: Run Vonk from you Simplifier project! See :ref:`simplifier_docs:simplifier_firely_server` for details.
#. Feature: Vonk supports Microsoft Azure CosmosDB.
   This required a few small changes to the MongoDB implementation (the share the drivers), so please reindex your MongoDB database: :ref:`reindex/all <feature_customsp_reindex>`.
#. Feature: Configuration to restrict support for ResourceTypes, SearchParameters and CompartmentDefinitions, see :ref:`supportedmodel`.
#. Feature: Errata.zip: collection of corrected search parameters (e.g. that had a faulty expression in the FHIR Core specification), see :ref:`feature_errata`
#. Upgrade: FHIR .NET API 0.95.0 (see the `Older SDK release notes`_)
#. Fix: a search on _id:missing=true was not processed correctly.
#. Fix: better distinction of reasons to reject updates (error codes 400 vs. 422, see `RESTful API specification <http://hl7.org/fhir/http.html#2.21.0.10.1>`_
#. Fix: recognize _format=text/xml and return xml (instead of the default json)
#. Fix: handling of the :not modifier in token searches (include resource that don't have a value at all).
#. Fix: handling of the :not modifier in searches with choice arguments
#. Fix: fullUrl in return bundles cannot be version specific.
#. Fix: evaluate _count=0 correctly (it was ignored).
#. Fix: correct error message on an invalid _include (now Vonk tells you which resourcetypes are considered for evaluating the used searchparameter).
#. Fix: indexing of Observation.combo-value-quantity failed for UCUM code for Celcius. This fix requires a :ref:`reindex/all <feature_customsp_reindex>` on this searchparameter.
#. Fix: total count in history bundle.
#. Fix: on vonk.fire.ly we disabled validating all input, so you can now create or update resources also if the relevant profiles are not loaded 
   (this was neccessary for Crucible, since it references US Core profiles, that are not present by default).
#. Fix: timeout of Azure Web App on first startup of Vonk - Vonk's first startup takes some time due to import of the specification (see :ref:`conformance_specification_zip`). 
   Since Azure Web Apps are allowed a startup time of about 3 minutes, it failed if the web app was on a low level service plan.
   Vonk will now no longer await this import. It will finish startup quickly, but until the import is finished it will return a 423 'Locked' upon every request.
#. Fix: improved logging on the import of conformance resources at startup (see :ref:`conformance_import`).

Release 0.6.4.0
---------------

.. attention::

   This version changes the way conformance resources are loaded from zip files and/or directories at startup. They are no longer loaded only in memory, but are added to the Administration API's database.
   You will notice a delay at first startup, when Vonk is loading these resources into the database. See Feature #1 below.

Database
^^^^^^^^

#. Fix #9 below requires a :ref:`reindex/all <feature_customsp_reindex>`.

Facade
^^^^^^

#. Release 0.6.4.0 is not released on NuGet, so the latest NuGet packages have version 0.6.2-beta. 
   This release is targeted towards the Administration API and :ref:`feature_terminology`, both of which are not (yet) available in Facade implementations.
   We are working on making the features of the Administration API available to Facade implementers in an easy way. 

Features and fixes
^^^^^^^^^^^^^^^^^^

#. Feature: Make all loaded conformance resources available through the Administration API. 
   
   Previously:

   * Only SearchParameter and CompartmentDefinition resources could be loaded from ZIP files and directories;
   * And those could not be read from the Administration API.
   
   Now:

   * The same set of (conformance) resourcetypes can be read from all sources (ZIP, directory, Simplifier);
   * They are all loaded into the Administration database and can be read and updated through the Administration API.

   Refer to :ref:`conformance` for details.

#. Feature: Experimental support for :ref:`feature_terminology` operations $validate-code, $expand, $lookup, $compose.
#. Feature: Support for `Compartment Search <http://www.hl7.org/implement/standards/fhir/search.html#2.21.1.2>`_.
#. Feature: Track timing of major dependencies in :ref:`Azure Application Insights <configure_log_insights>`.
#. Feature: :ref:`configure_log` can be overridden in 4 levels, just as the appsettings. The logsettings.json file will not be overwritten anymore by a Vonk distribution.
#. Fix: The check for :ref:`allowed profiles <feature_prevalidation>` is no longer applied to the Administration API. Previously setting AllowedProfiles to e.g. [http://mycompany.org/fhir/StructureDefinition/mycompany-patient] would prohibit you to actually create or update the related StructureDefinition in the Administration API.
#. Fix: When posting any other resourcetype than the supported conformance resources to the Administration API, Vonk now returns a 501 (Not Implemented).
#. Fix: Support search on Token with only a system (e.g. ``<base>/Observation?code=http://loinc.org|``)
#. Fix: Support search on Token with a fixed system, e.g. ``<base>/Patient?gender=http://hl7.org/fhir/codesystem-administrative-gender.html|female``. This fix requires a :ref:`reindex/all <feature_customsp_reindex>`.
#. Fix: Reindex could fail when a Reference Searchparameter has no targets.
#. Fix: Vonk works as Data Server on `ClinFHIR <http://clinfhir.com>`_, with help of David Hay.
#. Fix: Clearer error messages in the log on configuration errors.
#. Fix: Loading conformance resources from disk in Docker.

Documentation
^^^^^^^^^^^^^

#. We added documentation on :ref:`using IIS or NGINX as reverse proxies <deploy_reverseProxy>` for Vonk.
#. We added documentation on running Vonk on Azure Web App Services.


Release 0.6.2.0
---------------

.. attention::

  The loading of appsettings is more flexible. After installing a new version you can simply paste your previous appsettings.json in the Vonk directory. Vonk's default settings are now in appsettings.default.json. see :ref:`configure_appsettings` for details.

Database
^^^^^^^^
No changes

Features and fixes
^^^^^^^^^^^^^^^^^^

#. Feature: Conditional References in :ref:`Transactions <restful_transaction>` are resolved.
#. Feature: More flexible support for different serializers (preparing for ndjson in Bulkdata)
#. Feature: Improved handling on missing settings or errors in the :ref:`configure_appsettings`.
#. Feature: Improved :ref:`logging <configure_log>`, including Dependency Tracking on Azure Application Insights, see :ref:`configure_log_insights`
#. Feature: SearchParameter and CompartmentDefinition are now also imported from :ref:`Simplifier <conformance_fromsimplifier>`, so both Simplifier import and the :ref:`Administration API <conformance_administration_api>` support the same set of conformance resources: StructureDefinition, SearchParameter, CompartmentDefinition, ValueSet and CodeSystem. See :ref:`Conformance resources<conformance>`.
#. Feature: Loading of appsettings is more flexible, see :ref:`configure_appsettings`.
#. Feature: Added documentation on running Vonk behind IIS or NGINX: :ref:`deploy_reverseProxy`.
#. Performance: Improvement in speed of validation, especially relevant if you are :ref:`feature_prevalidation`.
#. Fix: If you try to load a SearchParameter (see :ref:`conformance_fromdisk`) that cannot be parsed correctly, Vonk puts an error about that in the log.
#. Fix: Results from _include and _revinclude are now marked with searchmode: Include (was incorrectly set to 'Match' before)
#. Fix: _format as one of the parameters in a POST Search is correctly evaluated.
#. Fix: No more errors in the log about a Session being closed before the request has finished 
   ("Error closing the session. System.OperationCanceledException: The operation was canceled.")
#. Fix: Subscription.status is evaluated correctly upon create or update on the Administration API
#. Fix: Token search with only a system is supported (``Observation.code=somesystem|``)
#. Fix: On validation errors like 'Cannot resolve reference Organization/Organization-example26"' are now suppressed since the validator is set not to follow these references.
#. Fix: New Firely logo in SVG format - looks better
#. Fix: Creating resources with duplicate canonical url's on the Administration API is prohibited, see :ref:`conformance`.
#. Fix: If a Compartment filter is used on a parameter that is not implemented, Vonk will return an error, see :ref:`feature_accesscontrol_compartment`.

Release 0.6.1.0
---------------
Name change from Furore to Firely

Release 0.6.0.0
---------------

.. attention:: 

   * SearchParametersImportOptions is renamed to :ref:`MetadataImportOptions<conformance_fromdisk>`.
   * :ref:`Subscription <feature_subscription>` can now be disabled from the settings.

Database
^^^^^^^^
#. The MongoDB implementation got a new index. It will be created automatically upon startup.

Features and fixes
^^^^^^^^^^^^^^^^^^

#. Feature: :ref:`Access control based on SMART on FHIR <feature_accesscontrol>`.
#. Feature: Vonk can also load CompartmentDefinition resources. See :ref:`conformance` for instructions.
#. Feature: ValueSet and CodeSystem resources can be loaded into the administration endpoint, and loaded from Simplifier. See :ref:`conformance` for instructions.
#. Feature: Be lenient on trailing slashes in the url.
#. Feature: OperationOutcome is now at the top of a Bundle result. For human readers this is easier to spot any errors or warnings.
#. Fix: In the :ref:`settings for SQL Server <configure_sql>` it was possible to specify the name of the Schema to use for the Vonk tables. That was actually not evaluated, so we removed the option for it. It is fixed to 'vonk'.
#. Fix: The OperationOutcome of the :ref:`Reset <feature_resetdb>` operation could state both an error and overall success.
#. Fix: If you did not set the CertificatePassword in the appsettings, Vonk would report a warning even if the password was not needed.
#. Fix: :ref:`Loading conformance resources <conformance_fromsimplifier>` in the SQL Server implementation could lead to an error.
#. Fix: Clearer error messages if the body of the request is mandatory but empty.
#. Fix: Clearer error message if the Content-Type is missing.
#. Fix: GET on [base]/ would return the UI regardless of the Accept header. Now if you specify a FHIR mimetype in the Accept header, it will return the result of a system wide search.
#. Fix: In rare circumstances a duplicate logical id could be created.
#. Fix: GET [base]/metadat would return status code 200 (OK). But it should return a 400 and an OperationOutcome stating that 'metadat' is not a supported resourcetype.

Documentation
^^^^^^^^^^^^^

#. We consolidated documentation on loading conformance resources into :ref:`conformance`.
   
Release 0.5.2.0
---------------

.. attention:: Configuration setting SearchOptions is renamed to BundleOptions.


Features and fixes
^^^^^^^^^^^^^^^^^^
#. Fix: When you specify LoadAtStartup in the :ref:`ResourceLoaderOptions <conformance_fromsimplifier>`, an warning was displayed: "WRN No server base configured, skipping resource loading."
#. Fix: `Conditional create <http://www.hl7.org/implement/standards/fhir/http.html#ccreate>`_ that matches an existing resource returned that resource instead of an OperationOutcome.
#. Fix: _has, _type and _count were in the CapabilityStatement twice.
#. Fix: _elements would affect the stored resource in the Memory implementation.
#. Fix: Getting a resource with an invalid id (with special characters or over 64 characters) now returns a 404 instead of 501.
#. Feature: :ref:`feature_customsp_reindex` now also re-indexes the Administration API database.
#. Fix: modifier :above for parameter type Url now works on the MongoDB implementation.
#. Fix: Vonk would search through inaccessible directories for the specification.zip.
#. Fix: Subscription could not be posted if 'Database' was not one of the SearchParametersImportOptions.
#. Fix: _(rev)include=* is not supported but was not reported as such.
#. Fix: In a searchresult bundle, the references to other resources are now made absolute, refering to the Vonk server itself.
#. Fix: :ref:`BundleOptions <bundle_options>` (previously: SearchOptions) settings were not evaluated.
#. Fix: Different responses for invalid resources when you change ValidateIncomingResources setting (400 vs. 501)
#. Fix: Better reporting of errors when there are invalid modifiers in the search.
#. Fix: Creating a resource that would not fit MongoDB's document size resulted in an inappropriate error.
#. Fix: There was no default sort order in the search, resulting in warnings from the SQL implementation. Added default sort on _lastUpdated (desc).
#. Fix: Preliminary disposal of LocalTerminology server by the Validator.

Facade
^^^^^^
#. Fix: _include/_revinclude on searchresults having contained resources triggered a NotImplementedException.

Release 0.5.1.1
---------------

Facade
^^^^^^

We released the Facade libraries on `NuGet <https://www.nuget.org/packages?q=vonk>`_ along with :ref:`getting started documentation <facadestart>`.

No features have been added to the Vonk FHIR Server.

Release 0.5.0.0
---------------

Database
^^^^^^^^
#. Long URI's for token and uri types are now supported, but that required a change of the SQL Server database structure. If you have AutoUpdateDatabase enabled (see :ref:`configure_sql`), Vonk will automatically apply the changes. As always, perform a backup first if you have production data in the database.
#. To prevent duplicate resources in the database we have provided a unique index on the Entry table. This update does include a migration. It can happen that that during updating of your database it cannot apply the unique index, because there are duplicate keys in your database (which is not good). Our advise is to empty your database first (with ``<vonk-endpoint>/administration/reset``, then update Vonk with this new version and then run Vonk with ``AutoUpdateDatabase=true`` (for the normal and the administration databases).

   If you run on production and encounter this problem, please contact our support. 

Features and fixes
^^^^^^^^^^^^^^^^^^
#. Feature: POST on _search is now supported
#. Fix: Statuscode of ``<vonk-endpoint>/administration/preload`` has changed when zero resources are added. The statuscode is now 200 instead of 201.
#. Fix: OPTIONS operation returns now the capability statement with statuscode 200.
#. Fix: A search operation with a wrong syntax will now respond with statuscode 400 and an OperationOutcome. For example ``GET <vonk-endpoint>/Patient?birthdate<1974`` will respond with statuscode 400.
#. Fix: A statuscode 501 could occur together with an OperationOutcome stating that the operation was successful. Not anymore.
#. Fix: An OperationOutcome stating success did not contain any issue element, which is nog valid. Solved. 
#. Improvement: In the configuration on :ref:`conformance_fromsimplifier` the section ``ArtifactResolutionOptions`` has changed to ``ResourceLoaderOptions`` and a new option has been introduced under that section named ``LoadAtStartup`` which, if set to true, will attempt to load the specified resource sets when you start Vonk
#. Improvement: the Memory implementation now also supports ``SimulateTransactions``
#. Improvement: the option ``SimulateTransactions`` in the configuration defaults to false now
#. Feature: You can now add SearchParameters at runtime by POSTing them to the Administration API. You need to apply :ref:`feature_customsp_reindex` to evaluate them on existing resources.
#. Fix: The batch operation with search entries now detects the correct interaction.
#. Fix: ETag header is not sent anymore if it is not relevant. 
#. Fix: Searching on a String SearchParameter in a MongoDB implementation could unexpectedly broaden to other string parameters.
#. Fix: If Reference.reference is empty in a Resource, it is no longer filled with Vonks base address.
#. Feature: Search operation now supports ``_summary``.
#. Fix: Paging is enabled for the history interaction.
#. Fix: Conditional updates won't create duplicate resources anymore when performing this action in parallel.
#. Fix: Indexing of CodeableConcept has been enhanced. 
#. Fix: Search on reference works now also for an absolute reference.
#. Fix: Long uri's (larger than are 128 characters) are now supported for Token and Uri SearchParameters.
#. Improvement: The configuration of IP addresses in :ref:`configure_administration_access` has changed. The format is no longer a comma-separated string but a proper JSON array of strings.


Release 0.4.0.1
---------------

Database
^^^^^^^^

#. Long URL's for absolute references are now supported, but that required a change of the SQL Server database structure. If you have AutoUpdateDatabase enabled, Vonk will automatically apply the changes. As always, perform a backup first if you have production data in the database.
#. Datetime elements have a new serialization format in MongoDB. After installing this version, you will see warnings about indexes on these fields. Please perform :ref:`feature_customsp_reindex`, for all parameters with ``<vonk-endpoint>/administration/reindex/all``. After the operation is complete, restart Vonk and the indexes will be created without errors.

Features and fixes
^^^^^^^^^^^^^^^^^^

#. Fix: SearchParameters with a hyphen ('-', e.g. general-practitioner) were not recognized in (reverse) chains.
#. Fix: CapabilityStatement is more complete, including (rev)includes and support for generic parameters besides the SearchParameters (like ``_count``). Also the SearchParameters now have their canonical url and a description.
#. Improvement: :ref:`feature_preload` gives more informative warning messages.
#. Fix: :ref:`feature_customsp_reindex` did not handle contained resources correctly. If you have used this feature on the 0.3.3 version, please apply it again with ``<vonk-endpoint>/administration/reindex/all`` to correct any errors.
#. Improvement: :ref:`Loading resources from Simplifier <conformance_fromsimplifier>` now also works for the Memory implementation.
#. Improvements on :ref:`feature_validation`: 

   * profile parameter can also be supplied on the url
   * if validation is successful, an OperationOutcome is still returned
   * it always returns 200, and not 422 if the resource could not be parsed

#. Feature: support for Conditional Read, honouring if-modified-since and if-none-match headers.
#. Fix: Allow for url's longer than 128 characters in Reference components.
#. Fix: Allow for an id in a resource on a Create interaction (and ignore that id).
#. Fix: Allow for an id in a resource on a Conditional Update interaction (and ignore that id).
#. Fix: Include Last-Modified header on Capability interaction.
#. Fix: Format Last-Modified header in `httpdate <https://www.w3.org/Protocols/rfc2616/rfc2616-sec3.html#sec3.3.1>`_ format.
#. Fix: Include version in bundle.entry.fullUrl on the History interaction.
#. Fix: Update ``_sort`` syntax from DSTU2 to STU3. Note: ``_sort`` is still only implemented for ``_lastUpdated``, mainly for the History interaction.
#. Improvement: If the request comes from a browser, the response is sent with a Content-Type of application/xml, to allow the browser to render it natively. Note that most browsers only render the narrative if they receive xml.

Release 0.3.3.0
---------------

.. attention:: We upgraded to .NET Core 2.0. For this release you have to install .NET Core Runtime 2.0, that you can download from `dot.net <https://www.microsoft.com/net/download/core#/runtime/>`_.

Hosting
^^^^^^^

The options for enabling and configuring HTTPS have moved. They are now in appsettings.json, under 'Hosting':
   ::

    "Hosting": {
      "HttpPort": 4080,
      "HttpsPort": 4081, // Enable this to use https
      "CertificateFile": "<your-certificate-file>.pfx", //Relevant when HttpsPort is present
      "CertificatePassword" : "<cert-pass>" // Relevant when HttpsPort is present
    },
  
   This means you have to adjust your environment variables for CertificateFile and CertificatePassword (if you had set them) to:
   ::

    VONK_Hosting:CertificateFile
    VONK_Hosting:CertificatePassword

   The setting 'UseHttps' is gone, in favour of Hosting:HttpsPort.

Database
^^^^^^^^

There are no changes to the database structure.

Features and fixes
^^^^^^^^^^^^^^^^^^

#. Feature: Subscription is more heavily checked on create and update. If all checks pass, status is set to active. If not, the Subscription is not stored, and Vonk returns an OperationOutcome with the errors.

   * Criteria must all be supported
   * Endpoint must be absolute and a correct url
   * Enddate is in the future
   * Payload mimetype is supported

#. Feature: use _elements on Search
#. Feature: :ref:`load profiles from your Simplifier project <conformance_fromsimplifier>` at startup.
#. Feature: Content-Length header is populated.
#. Fix: PUT or POST on /metadata returned 200 OK, but now returns 405 Method not allowed.
#. Fix: Sometimes an error message would appear twice in an OperationOutcome.
#. Fix: _summary is not yet implemented, but was not reported as 'not supported' in the OperationOutcome. Now it is. (Soon we will actually implement _summary.)
#. Fix: If-None-Exist header was also processed on an update, where it is only defined for a create. 
#. Fix: Set Bundle.entry.search.mode to 'outcome' for an OperationOutcome in the search results.
#. UI: Display software version on homepage.

Release 0.3.2.0
---------------

1. Fix: _include and _revinclude could include too many resources.

Release 0.3.1.0
---------------

1. IP address restricted access to Administration API functions.
2. Fix on Subscriptions: 
   
   #. Accept only Subscriptions with a channel of type rest-hook and the payload (if present) has a valid mimetype.
   #. Set them from requested to active if they are accepted.

Release 0.3.0.0
---------------

1. Database changes

  If you have professional support, please consult us on the best way to upgrade your database.

  #. The schema for the SQL Database has changed. It also requires re-indexing all resources. 
  #. The (implicit) schema for the documents in the MongoDb database has changed. 
  #. The Administration API requires a separate database (SQL) or collection (MongoDb).

2. New features:

  #. :ref:`Custom Search Parameters <feature_customsp>`
  #. Support for Subscriptions with rest-hook channel
  #. Preload resources from a zip.
  #. Reset database
  #. Conditional create / update / delete
  #. Support for the prefer header
  #. Validation on update / create (can be turned on/off)
  #. Restrict creates/updated to specific profiles.
  #. Configure supported interactions (turn certain interactions on/off)

3. New search features:

  #. ``_has``
  #. ``_type`` (search on system level)
  #. ``_list``
  #. ``_revinclude``

4. Enhancements

  #. ``:exact``: Correctly search case (in)sensitive when the :exact modifier is (not) used on string parameters.
  #. Enhanced reporting of errors and warnings in the OperationOutcome.
  #. Custom profiles / StructureDefinitions separated in the Administration API (instead of in the regular database).
  #. Full FHIRPath support for Search Parameters.
  #. Fixed date searches on dates without seconds and timezone
  #. Fixed evaluation of modifier :missing
  #. Correct total number of results in search result bundle.
  #. Fix paging links in search result bundle
  #. Better support for mimetypes.

5. DevOps:

  #. New :ref:`administration_api`
  #. Enabled logging of the SQL statements issued by Vonk (see :ref:`configure_log`)
  #. Migrations for SQL Server (auto create database schema, also for the Administration API)

6. Performance

  #. Added indexes to MongoDb and SQL Server implementations.

.. _Older SDK release notes: https://docs.fire.ly/projects/Firely-NET-SDK/releasenotes.html