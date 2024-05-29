
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
---------------------------------------

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

   VONKAUDITLOG_AuditLog__WriteTo__0__Args__path=./other/directory/AuditLog.log

.. _configure_envvar_call_stack:

Return of call stack and Environment Variables
----------------------------------------------

When first implementing Firely Server or for debugging purposes it can be convenient to have the call stack returned even though the server throws a 500 error code. If no specific environment variables are set, Firely Server will return **'Oops! Something went wrong :('** with a 500 error code. The call stack will only appear in the log. 
Setting the 'ASPNETCORE_ENVIRONMENT' variable to production will have the same result::
   
   ASPNETCORE_ENVIRONMENT=Production

When the 'ASPNETCORE_ENVIRONMENT' variable is set to development the call stack is returned, even when a 500 error code is thrown by the server::
   
   ASPNETCORE_ENVIRONMENT=Development

.. _customize_config_location:

Customize the location of configuration files
---------------------------------------------

It is possible to change the default location of the ``*.instance.json`` configuration files by setting a reserved environment variable. See :ref:`configure_settings_path` for details.

.. _configure_envvar_windows:

Changing Environment Variables on Windows
-----------------------------------------

In Windows you can change the Environment Variables with Powershell or through the UI. Based on the first example above:

	+ In Powershell run:|br| 
	  ``> $env:VONK_Repository="SQL"``
	+ or go to your `System`, open the `Advanced system settings` --> `Environment variables` and create a new variable
	  with the name :code:`VONK_Repository` and set the value to ``SQL``.
