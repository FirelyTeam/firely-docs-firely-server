
.. |br| raw:: html

   <br />   

.. _configure_envvar:

Firely Server settings with Environment Variables
=================================================

.. warning:: It is recommended to use Environment Variables for all sensitive information you want to pass onto to Firely Server, such as connection strings and secrets.

.. _configure_envvar_appsettings:

Environment Variables for appsettings
-------------------------------------

All the settings in :ref:`configure_appsettings` can be overridden by environment variables on your OS.
This can be useful if you want to deploy Firely Server to several machines, each having their own settings for certain options.
For :ref:`use_docker` using environment variables in the docker-compose file is currently the only way to pass settings to the container.
Or if you don't want  a database password in the ``appsettings.json`` file.

The format for the environment variables is:
::

    VONK_<setting_level_1>[:<setting_level_n>]*

So you start the variable name with the prefix 'VONK\_', and then follow the properties in the json settings, separating each level with a colon ':'. Some examples:

appsettings.json::

	"Repository" : "SQL"

environment variable::

	VONK_Repository=SQL

To access an embedded value, using the ':' separator:

appsettings.json:

.. code-block:: json

	"Administration" : {
		"SqlDbOptions" : {
			"ConnectionString" : "<some connectionstring>"
		}
	}

environment variable::

	VONK_Repository:SqlDbOptions:ConnectionString=<some connectionstring>

To access an array item, use 0-based indexing::

	VONK_PipelineOptions:Branches:0:Exclude:0=Vonk.Repository.Memory
	VONK_PipelineOptions:Branches:0:Exclude:1=Vonk.Repository.Sql

Arrays in Environment Variables
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Sometimes the appsettings allow for an array of values, e.g. in the setting for ``AllowedProfiles`` in :ref:`feature_prevalidation`. You can address them by appending an extra colon and an index number.

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

environment variables:

	VONK_Validation:ValidateIncomingResources=true
	VONK_Validation:AllowedProfiles:0=http://hl7.org/fhir/StructureDefinition/daf-patient
	VONK_Validation:AllowedProfiles:1=http://hl7.org/fhir/StructureDefinition/daf-allergyintolerance


.. _configure_envvar_log:

Log settings with Environment Variables
---------------------------------------

You can control the :ref:`configure_log` with Environment Variables the same way as the :ref:`configure_envvar_appsettings` above. 
The difference is in the prefix. For the log settings we use 'VONKLOG\_'.

logsettings.json

.. code-block:: json

   "Serilog": {
        "MinimumLevel": {
            "Override": {
                "Vonk.Configuration": "Information",

environment variable:

   VONKLOG_Serilog:MinimumLevel:Override:Vonk.Configuration=Information

.. _configure_envvar_audit_log:

Audit log settings with Environment Variables
---------------------------------------------

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

   VONKAUDITLOG_AuditLog:WriteTo:0:Args:path=./other/directory/AuditLog.log

.. _configure_envvar_call_stack:

Return of call stack and Environment Variables
----------------------------------------------

When first implementing Firely Server or for debugging purposes it can be convenient to have the call stack returned even though the server throws a 500 error code. If no specific environment variables are set, Firely Server will return **'Oops! Something went wrong :('** with a 500 error code. The call stack will only appear in the log. 
Setting the 'ASPNETCORE_ENVIRONMENT' variable to production will have the same result::
   
   ASPNETCORE_ENVIRONMENT=Production

When the 'ASPNETCORE_ENVIRONMENT' variable is set to development the call stack is returned, even when a 500 error code is thrown by the server::
   
   ASPNETCORE_ENVIRONMENT=Development

.. _customize_config_location:

Customizing location of configuration files
-------------------------------------------

It is possible to change a default location of configuration files by setting a reserved environment variable - ``VONK_PATH_TO_SETTINGS``:
::

    VONK_PATH_TO_SETTINGS=./config

In the example above Firely Server will try to locate and read configuration from one of the following files inside ``config`` directory that is relative to a location of Firely Server binaries: ``appsettings.instance.json``, ``logsettings.instance.json`` and ``audit.logsettings.instance.json``.
Note that if the directory does not exist, if there are no files from the list, or they are unable to be read - an exception is to be thrown on application startup.

.. _configure_envvar_windows:

Changing Environment Variables on Windows
-----------------------------------------

In Windows you can change the Environment Variables with Powershell or through the UI. Based on the first example above:

	+ In Powershell run:|br| 
	  ``> $env:VONK_Repository="SQL"``
	+ or go to your `System`, open the `Advanced system settings` --> `Environment variables` and create a new variable
	  with the name :code:`VONK_Repository` and set the value to "SQL" (you don't need to enter the quotes here).
