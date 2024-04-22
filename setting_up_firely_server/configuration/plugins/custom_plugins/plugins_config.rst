.. _vonk_plugins_config:

Configure the pipeline
======================

Configuration of the pipeline in Firely Server is done with ``PipelineOptions`` in combination with ``SupportedInteractions``. A default setup is installed with Firely Server in appsettings.default.json, and it looks like this:
::

  "PipelineOptions": {
    "PluginDirectory": "./plugins",
    "Branches": [
      {
        "Path": "/",
        "Include": [
          "Vonk.Core",
          "Vonk.Fhir.R3",
          "Vonk.Fhir.R4",
          "Vonk.Repository.Sql.SqlVonkConfiguration",
          "Vonk.Repository.Sqlite.SqliteVonkConfiguration",
          "Vonk.Repository.MongoDb.MongoDbVonkConfiguration",
          "Vonk.Repository.Memory.MemoryVonkConfiguration",
          "Vonk.Subscriptions",
          "Vonk.Plugin.Smart",
          "Vonk.UI.Demo",
          "Vonk.Plugins.Terminology",
          "Vonk.Plugin.DocumentOperation.DocumentOperationConfiguration",
          "Vonk.Plugin.ConvertOperation.ConvertOperationConfiguration",
          "Vonk.Plugin.BinaryWrapper.BinaryWrapperConfiguration",
          "Vonk.Plugin.MappingToStructureMap.MappingToStructureMapConfiguration",
          "Vonk.Plugin.TransformOperation.TransformOperationConfiguration"
        ],
        "Exclude": [
          "Vonk.Subscriptions.Administration"
        ]
      },
      {
        "Path": "/administration",
        "Include": [
          "Vonk.Core",
          "Vonk.Fhir.R3",
          "Vonk.Fhir.R4",
          "Vonk.Repository.Sql.SqlAdministrationConfiguration",
          "Vonk.Repository.Sqlite.SqliteAdministrationConfiguration",
          "Vonk.Repository.MongoDb.MongoDbAdminConfiguration",
          "Vonk.Repository.Memory.MemoryAdministrationConfiguration",
          "Vonk.Subscriptions.Administration",
          "Vonk.Plugins.Terminology",
          "Vonk.Administration"
        ],
        "Exclude": [
          "Vonk.Plugin.Operations"
        ]
      }
    ]
  },
  "SupportedInteractions": {
    "InstanceLevelInteractions": "read, vread, update, delete, history, conditional_delete, conditional_update, $validate, $validate-code, $expand, $compose, $meta, $meta-add, $document",
    "TypeLevelInteractions": "create, search, history, conditional_create, compartment_type_search, $validate, $snapshot, $validate-code, $expand, $lookup, $compose, $document",
    "WholeSystemInteractions": "capabilities, batch, transaction, history, search, compartment_system_search, $validate, $convert"
  },

PluginDirectory:
   You can put plugins of your own (or third party) into this directory for Firely Server to pick them up, without polluting the Firely Server binaries directory itself. For a list of available plugins in Firely Server, see :ref:`vonk_available_plugins`. The directory in the default setting of ``./plugins`` is not created upon install, you may do this yourself if you want to add a plugin.
PluginDirectory.Branches:
   A web application can branch into different paths, and Firely Server has two by default:

   * ``/``: the root branch, where the main :ref:`restful` is hosted;
   * ``/administration``: where the :ref:`administration_api` is hosted.
 
   ``Branches`` contains a subdocument for each of the defined paths:
   
   Path
      The path for this branch. This is the part after the base URL that Firely Server is hosted on.
   Include
      (Prefixes of) :ref:`vonk_plugins_configclass` that add services and middleware to Firely Server.
   Exclude
      (Prefixes of) :ref:`vonk_plugins_configclass` that may not be executed. ``Exclude`` overrides ``Include`` and is useful if you want to use all but one configuration class from a namespace.

SupportedInteractions:
  A comma-separated list of all interactions Firely Server should enable on ``[base]/[type]/[id]`` (InstanceLevelInteractions), ``[base]/[type]`` (TypeLevelInteractions), and ``[base]`` (WholeSystemInteractions) levels. Firely Server will use this list to enable/disable supported interactions and reflect it in ``/metadata`` accordingly.
  
  If you'd like to limit what operations your Firely Server supports, remove them from this list.
  
  If you've added a custom plugin that enables a new interaction, make sure to load the plugin (see ``PluginDirectory`` above) and enable the interaction in this list. For example, if you've added the ``Vonk.Plugin.ConvertOperation`` $convert plugin in ``PipelineOptions.Branches.Include``, make sure to enable the operation ``$convert`` as well: ::
  
  "WholeSystemInteractions": "$convert, capabilities, batch, transaction, history, search, compartment_system_search, $validate"
