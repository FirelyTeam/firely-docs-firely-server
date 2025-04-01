.. |br| raw:: html

   <br />

.. _configure_appsettings:

Adjusting Firely Server settings
================================

Firely Server settings are controlled in json configuration files called ``appsettings(.*).json``. The possible settings in these files are all the same and described below.
The different files are read in a hierarchy so you can control settings on different levels. All appsettings files are in the Firely Server distribution directory, next to Firely.Server.dll.
We go through all the sections of this file and refer you to detailed pages on each of them.

You can also control :ref:`configure_envvar`.

Changes to the settings require a restart of Firely Server.

.. _configure_levels:

Hierarchy of settings
---------------------

Firely Server reads its settings from these sources, in this order:

:appsettings.default.json: Installed with Firely Server, contains default settings and a template setting if no sensible default is available.
:appsettings.json: You can create this one for your own settings. Because it is not part of the Firely Server distribution, it will not be overwritten by a next Firely Server version.
:environment variables: See :ref:`configure_envvar`.
:appsettings.instance.json: You can create this one to override settings for a specific instance of Firely Server. It is not part of the Firely Server distribution.
                            This file is especially useful if you run multiple instances on the same machine.

Settings lower in the list override the settings higher in the list (think CSS, if you're familiar with that).

.. warning::

   JSON settings files can have arrays in them. The configuration system can NOT merge arrays.
   So if you override an array value, you need to provide all the values that you want in the array.
   In the Firely Server settings this is relevant for e.g. Validation.AllowedProfiles and for the PipelineOptions.

.. note::
   By default in ASP.NET Core, if on a lower level the array has more items, you will still inherit those extra items.
   We fixed this in Firely Server, an array will always overwrite the complete base array.
   To nullify an array, add the value with an array with just an empty string in it::

     "PipelineOptions": {
       "Branches": [
         {
           "Path": "myroot",
           "Exclude": [""]
         }
       ]
     }

   This also means you cannot override a single array element with an environment variable. (Which was tricky anyway - relying on the exact number and order of items in the original array.)

.. _configure_change_settings:

Changing the settings
---------------------

In general you do not change the settings in ``appsettings.default.json`` but create your own overrides in ``appsettings.json`` or ``appsettings.instance.json``. That way your settings are not overwritten by a new version of Firely Server (with a new ``appsettings.default.json`` therein), and you automatically get sensible defaults for any new settings introduced in ``appsettings.default.json``.

Settings after first install
^^^^^^^^^^^^^^^^^^^^^^^^^^^^

After you installed Firely Server (see :ref:`vonk_getting_started`), either:

* copy the ``appsettings.default.json`` to ``appsettings[.instance].json`` and remove settings that you do not intend to alter, or
* create an empty ``appsettings[.instance].json`` and copy individual parts from the ``appsettings.default.json`` if you wish to adjust them.

Adjust the new ``appsettings[.instance].json`` to your liking using the explanation below.

When running :ref:`Firely Server on Docker<use_docker>` you probably want to adjust the settings using the :ref:`Environment Variables<configure_envvar>`.

Settings after update
^^^^^^^^^^^^^^^^^^^^^

If you install the binaries of an updated version of Firely Server, you can:

* copy the new binaries over the old ones, or
* deploy the new version to a new directory and copy the ``appsettings[.instance].json`` over from the old version.

In both cases, check the :ref:`vonk_releasenotes` to see if settings have changed, or new settings have been introduced.
If you want to adjust a changed / new setting, copy the relevant section from ``appsettings.default.json`` to your own ``appsettings[.instance].json`` and then adjust it.

Commenting out sections
^^^^^^^^^^^^^^^^^^^^^^^

JSON formally has no notion of comments. But the configuration system of ASP.Net Core (and hence Firely Server) accepts double slashes just fine::

    "Administration": {
        "Repository": "SQLite", //Memory / SQL / MongoDb
        "SqlDbOptions": {
            "ConnectionString": "connectionstring to your Firely Server Admin SQL Server database (SQL2012 or newer); Set MultipleActiveResultSets=True",
            "SchemaName": "vonkadmin",
            "AutoUpdateDatabase": true,
            "MigrationTimeout": 1800 // in seconds
            //"AutoUpdateConnectionString" : "set this to the same database as 'ConnectionString' but with credentials that can alter the database. If not set, defaults to the value of 'ConnectionString'"
        },

This will ignore the AutoUpdateConnectionString.

.. _configure_settings_path:

Providing settings in a different folder
----------------------------------------

It can be useful or even necessary to provide settings outside of the Firely Server folder itself, e.g. when mounting the settings to a Docker container. That is possible. 

1. Provide an environment variable named ``VONK_PATH_TO_SETTINGS``, set to the folder where the settings are to be read from. This path can be absolute or relative to the Firely Server directory.
2. In this folder you must provide at least one of the following files:

   1. ``appsettings.instance.json``
   2. ``logsettings.instance.json``
   3. ``auditlogsettings.instance.json``

3. These files will be read with the same :ref:`priority <configure_levels>` as they would have if they were in the Firely Server directory. 

Note that if you provide this environment variable, then:

#. The designated folder must exist.
#. At least one of the three files must be present.
#. The account that runs Firely Server must have read access to each of the files.
#. The Firely Server directory itself will no longer be scanned for any of the three files. So if you want to use any of the three ``*.instance.json`` files, you must provide all of them in the designated directory.

Examples: 

::

    VONK_PATH_TO_SETTINGS=./config

This relative path would read e.g. ``<Firely Server directory>/config/appsettings.instance.json``.

::

    VONK_PATH_TO_SETTINGS=/usr/config

This absolute path would read e.g. ``/usr/config/appsettings.instance.json``.

.. _log_configuration:

Log of your configuration
-------------------------

Because the hierarchy of settings can be overwhelming, Firely Server logs the resulting configuration.
To enable that, the loglevel for ``Vonk.Server`` must be ``Information`` or more detailed. That is set for you by default in ``logsettings.default.json``.
Refer to :ref:`configure_log` for information on setting log levels.

.. _configure_envvar:

Firely Server settings with Environment Variables
-------------------------------------------------

.. warning:: It is recommended to use Environment Variables for all sensitive information you want to pass onto to Firely Server, such as connection strings and secrets.

.. _configure_envvar_appsettings:

Environment Variables for appsettings
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

All the settings in :ref:`configure_appsettings` can be overridden by environment variables on your OS.
This can be useful if you want to deploy Firely Server to several machines, each having their own settings for certain options.
For :ref:`use_docker` using environment variables in the docker-compose file is currently the only way to pass settings to the container.
Or if you don't want  a database password in the ``appsettings.json`` file.

The format for the environment variables is:
::

    VONK_<setting_level_1>[__<setting_level_n>]*

So you start the variable name with the prefix ``VONK_``, and then follow the properties in the json settings, separating each level with a double underscore ``__``. Some examples:

appsettings.json::

	"Repository" : "SQL"

environment variable::

	VONK_Repository=SQL

To access an embedded value, using the '__' separator:

appsettings.json:

.. code-block:: json

	"Administration" : {
		"SqlDbOptions" : {
			"ConnectionString" : "<some connectionstring>"
		}
	}

environment variable::

	VONK_Administration__SqlDbOptions__ConnectionString=<some connectionstring>
	
.. note:: 
    A colon ``:`` is also valid as a separator in some environments, but not all. For its wider support we recommend to use ``__``.
    See `this article <https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-8.0#non-prefixed-environment-variables>`_ for more information.

Arrays in Environment Variables
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Sometimes the appsettings allow for an array of values, e.g. in the setting for ``AllowedProfiles`` in :ref:`feature_prevalidation`. You can address them by appending an extra ``__`` and an index number.

appsettings.json:

.. code-block:: json

   "Validation": {
      "ValidateIncomingResources": "true",
      "AllowedProfiles": 
      [
         "http://hl7.org/fhir/StructureDefinition/daf-patient", 
         "http://hl7.org/fhir/StructureDefinition/daf-allergyintolerance"
      ]
   }

environment variables::

	VONK_Validation__ValidateIncomingResources=true
	VONK_Validation__AllowedProfiles__0=http://hl7.org/fhir/StructureDefinition/daf-patient
	VONK_Validation__AllowedProfiles__1=http://hl7.org/fhir/StructureDefinition/daf-allergyintolerance

Another example for excluding namespaces in the ``PipelineOptions``:

.. code-block:: json

   "PipelineOptions": {
   "Branches": [
      {
         "Path": "/",
         "Exclude": [
            "Vonk.Repository.Memory",
            "Vonk.Repository.Sql"
         ]
      }
   } 

environment variables::
    
        VONK_PipelineOptions__Branches__0__Exclude__0=Vonk.Repository.Memory
        VONK_PipelineOptions__Branches__0__Exclude__1=Vonk.Repository.Sql

.. _configure_envvar_log:

Log settings with Environment Variables
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

You can control the :ref:`configure_log` with Environment Variables the same way as the :ref:`configure_envvar_appsettings` above. 
The difference is in the prefix. For the log settings we use 'VONKLOG\_'.

logsettings.json

.. code-block:: json

   "Serilog": {
        "MinimumLevel": {
            "Override": {
                "Vonk.Configuration": "Information",

environment variable::

   VONKLOG_Serilog__MinimumLevel__Override__Vonk.Configuration=Information
   
Note that the ``.`` in ``Vonk.Configuration`` is part of a namespace and should not be replaced by a double underscore.

.. _configure_envvar_audit_log:

Audit log settings with Environment Variables
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

You can control the :ref:`configure_audit_log_file` with Environment Variables the same way as the :ref:`configure_envvar_appsettings` above. 
The difference is in the prefix. For the log settings we use 'VONKAUDITLOG\_'.

audit.logsettings.json

.. code-block:: json

   "AuditLog": {
      "WriteTo": [
         {
            "Name": "File", 
            "Args": {
               "path": "./audit/AuditLog.log"

environment variable::

   VONKAUDITLOG_AuditLog__WriteTo__0__Args__path=./other/directory/AuditLog.log

.. _configure_envvar_call_stack:

Return of call stack and Environment Variables
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

When first implementing Firely Server or for debugging purposes it can be convenient to have the call stack returned even though the server throws a 500 error code. If no specific environment variables are set, Firely Server will return **'Oops! Something went wrong :('** with a 500 error code. The call stack will only appear in the log. 
Setting the 'ASPNETCORE_ENVIRONMENT' variable to production will have the same result::
   
   ASPNETCORE_ENVIRONMENT=Production

When the 'ASPNETCORE_ENVIRONMENT' variable is set to development the call stack is returned, even when a 500 error code is thrown by the server::
   
   ASPNETCORE_ENVIRONMENT=Development

.. _configure_envvar_windows:

Changing Environment Variables on Windows
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

In Windows you can change the Environment Variables with Powershell or through the UI. Based on the first example above:

	+ In Powershell run:|br| 
	  ``> $env:VONK_Repository="SQL"``
	+ or go to your `System`, open the `Advanced system settings` --> `Environment variables` and create a new variable
	  with the name :code:`VONK_Repository` and set the value to ``SQL``.
