.. _upgrade:

How to upgrade Firely Server?
=============================

The process for upgrading Firely Server depends on whether you have a vanilla Firely Server, you added your own plugins or are running a Facade.
This page describes the general process for each situation. Please refer to the :ref:`vonk_releasenotes` for details per each released version of Firely Server.

.. attention:

   In all cases, pay attention to the import of new conformance resources - especially if you have multiple instances of Firely Server running. See :ref:`vonk_conformance_instances` for details.

.. _upgrade_server: 

Upgrading Firely Server
-----------------------

.. _upgrade_server_binaries:

Using the binary distribution
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

#. Download the latest version of Firely Server, see :ref:`vonk_getting_started`, and extract it to where you want it installed.
#. Copy your appsettings.instance.json and logsettings.instance.json files from the current installation to the new installation. 
#. Check the :ref:`vonk_releasenotes` for any new settings that you may want to apply or change from their defaults.
#. Check the :ref:`vonk_releasenotes` for any actions that you need to take specifically for this upgrade.
#. Make sure the new installation can find the license file (see :ref:`configure_license`, general advice is to put the license file outside of the installation directory).
#. Create a backup of your current databases, both the main Resource database and the Administration database. See :ref:`configure_repository` to find the details on your configured database connection.
#. Stop the running instance of Firely Server (Ctrl + C if running from the console).
#. Switch to the new installation directory and start Firely Server from there (``> dotnet ./Vonk.Server.dll``)
#. Firely Server will now do several upgrade tasks, during which any web request will be responded to with 423 - Locked:

   #. If needed, an update is applied to the database structure.
   #. If Firely Server introduces a new version of the FHIR .NET API, Firely Server will load a new set of Conformance Resources from the specification.zip into the Administration database, for both FHIR STU3 and FHIR R4. In a specific case you can :ref:`prevent this step from happening <replace_admindb>`.

#. When Firely Server is done with the tasks above, it is again available to process requests.
#. Check the log for warnings stating that you use obsolete settings. If so, adjust them and restart Firely Server.

If anything went wrong, go back:

#. Stop the (new) running instance of Firely Server.
#. Restore both databases from your backup.
#. Switch to the old installation directory and start the old version of Firely Server from there (``> dotnet .\Vonk.Server.dll``)
#. It should start as it did before you began the upgrade.
#. Report the problem to the Firely Server helpdesk, see :ref:`vonk-contact`.

.. _replace_admindb:

You may be able to avoid the import of specification.zip if:

* The Administration database is in SQLite and
* You have not made alterations to the Administration API through the Web API.

In this case you can simply replace the old database (usually with the filename ``vonkadmin.db``) with the one from the new installation directory (in ``./data/vonkadmin.db``).
Do so *before* you start the new Firely Server installation.
Anything specified in :ref:`AdministrationImportOptions <configure_admin_import>` will be re-imported into the new database.

.. _upgrade_server_docker:

Using Docker
^^^^^^^^^^^^

Revisit :ref:`use_docker`.

#. Stop the running container for Firely Server: ``> docker stop vonk.server``.
#. Pull the latest image for Firely Server: ``> docker pull simplifier/vonk``
#. Check the :ref:`vonk_releasenotes` for any new settings that you may want to apply or change from their defaults, and apply that to the ``environment`` setting in the docker-compose file.
#. Check the :ref:`vonk_releasenotes` for any action that you need to take specifically for this upgrade.
#. Create a backup of your current databases, both the main Resource database and the Administration database. See :ref:`configure_repository` and your docker-compose file to find the details on where your databases are.
#. Start the new version (see :ref:`use_docker` for the various commands to run the Firely Server container).
#. Firely Server will now do several upgrade tasks, during which any web request will be responded to with 423 - Locked:

   #. If needed, an update is applied to the database structure.
   #. If Firely Server introduces a new version of the FHIR .NET API, Firely Server will load a new set of Conformance Resources from the specification.zip into the Administration database, for both FHIR STU3 and FHIR R4. In a specific case you can :ref:`prevent this step from happening <replace_admindb>`.

#. When Firely Server is done with the tasks above, it is again available to process requests.
#. Check the log for warnings stating that you use obsolete settings. If so, adjust them and restart Firely Server.

If anything went wrong, go back:

#. Stop the (new) running container of Firely Server.
#. Restore both databases from your backup.
#. Specify your previous image of Firely Server in the docker command or in the docker-compose file: ``simplifier\vonk:<previous-version-tag>``
#. Start the container based on this previous image.
#. It should start as it did before you began the upgrade.
#. Report the problem to the Firely Server helpdesk, see :ref:`vonk-contact`.

.. _upgrade_plugin:

Upgrading Plugins
-----------------

Since a Plugin runs in the context of a Firely Server we advise you to start by upgrading your Firely Server, without loading your Plugin.
Check the section on :ref:`settings_pipeline` to see how you can exclude your plugin from the pipeline.

.. attention::

   We do not guarantee that a plugin built against version x.y.z of Firely Server can be run within a newer or older version as-is.
   Between minor versions recompilation is usually enough to update your plugin. Between major versions you should prepare for breaking changes in the public programming API.
   Sometimes we choose to apply such changes even on a minor version update, if we are fairly sure that you will not or only slightly be affected by it.

Upgrade the references in your plugin:

#. Open the source code of your plugin, and open the project file (``yourplugin.csproj``).
#. Change the references to the Firely Server.* packages to the version that you want to upgrade to.
#. Build and check the errors.
#. Check the list of breaking changes for the new Firely Server version in the :ref:`vonk_releasenotes`. Applying the changes should fix the errors.
#. With some releases Firely Server is also upgraded to a newer version of the Firely .NET SDK. That is then mentioned in the release notes. If this is the case, also check the `SDK release notes`_ for breaking changes.
#. Still errors? Maybe we have overlooked a change. Please report it to us, see :ref:`vonk-contact`. And if it is easy to fix - do so :-)
#. Build and publish your plugin. 
#. Put the resulting dll's in the plugin directory of the new installation of Firely Server.
#. Re-include your plugin in the pipeline.
#. (Re)start Firely Server and test the working of your plugin.

.. _upgrade_facade:

Upgrading Facades
-----------------

A Facade implementation is technically also a plugin, but one that only adds repository access services. For this it makes no sense to try to run Firely Server without the Facade as is described for plugins.
So start with upgrading the references right away.

Especially for Facades to relational databases: match the version of EntityFrameworkCore that the new version of Firely Server is using. Check the list of changes to see whether we upgraded.

.. _SDK release notes: https://github.com/FirelyTeam/firely-net-sdk/releases