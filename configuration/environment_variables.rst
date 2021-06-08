
.. |br| raw:: html

   <br />   

.. _configure_envvar:

Firely Server settings with Environment Variables
=================================================

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

appsettings.json::

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

appsettings.json::

  "Validation": {
    "ValidateIncomingResources": "true",
    "AllowedProfiles": 
    [
        http://hl7.org/fhir/StructureDefinition/daf-patient, 
        http://hl7.org/fhir/StructureDefinition/daf-allergyintolerance
    ]
  },

environment variables::

	VONK_Validation:ValidateIncomingResources=true
	VONK_Validation:AllowedProfiles:0=http://hl7.org/fhir/StructureDefinition/daf-patient
	VONK_Validation:AllowedProfiles:1=http://hl7.org/fhir/StructureDefinition/daf-allergyintolerance


.. _configure_envvar_log:

Log settings with Environment Variables
---------------------------------------

You can control the :ref:`configure_log` with Environment Variables the same way as the :ref:`configure_envvar_appsettings` above. 
The difference is in the prefix. For the log settings we use 'VONKLOG\_'.

logsettings.json

   "Serilog": {
        "MinimumLevel": {
            "Override": {
                "Vonk.Configuration": "Information",

environment variable::

   VONKLOG_Serilog:MinimumLevel:Override:Vonk.Configuration=Information

.. _configure_envvar_windows:

Changing Environment Variables on Windows
-----------------------------------------

In Windows you can change the Environment Variables with Powershell or through the UI. Based on the first example above:

	+ In Powershell run:|br| 
	  ``> $env:VONK_Repository="SQL"``
	+ or go to your `System`, open the `Advanced system settings` --> `Environment variables` and create a new variable
	  with the name :code:`VONK_Repository` and set the value to "SQL" (you don't need to enter the quotes here).
