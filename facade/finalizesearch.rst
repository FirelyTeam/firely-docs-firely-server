Finalizing search
=================

In the previous steps you have created search support for the _id parameter on a Patient resource type.
In order to test if your Facade implementation works correctly, you will need to perform a couple of steps:

#. Create a configuration class for the `ASP .Net Core pipeline <https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-2.2>`_
#. Plug the Facade into the Firely Server
#. Configure the Firely Server to use your repository

1. Add configuration class
--------------------------

To add your repository service to the Firely Server pipeline, you will need to add a configuration class that sets
the order of inclusion, and adds to the services. For background information, see :ref:`vonk_plugins_configclass`.

* Add a static class to your project called ``ViSiConfiguration``
* Add the following code to it::

    [VonkConfiguration(order: 240)]
    public static class ViSiConfiguration
    {
        public static IServiceCollection AddViSiServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ViSiContext>();
            services.TryAddSingleton<ResourceMapper>();
            services.TryAddScoped<ISearchRepository, ViSiRepository>();

            services.Configure<DbOptions>(configuration.GetSection(nameof(DbOptions)));
            return services;
        }
    }

2. Create your Facade plugin
----------------------------

* First, build your project
* Find the resulting dll and copy that to the ``plugins`` folder in the working directory of your Firely Server

.. note::
  If your Firely Server working directory does not contain a plugins folder yet, you can create one. Within it, you can
  create subfolders, which can be useful if you work with multiple plugins.

  You can also configure the name and location of this folder with the ``PipelineOptions.PluginDirectory`` setting
  in the appsettings file.

.. _configure_facade:

3. Configure your Firely Server Facade
--------------------------------------

* Create an appsettings.instance.json file in your Firely Server working directory.

  .. tip::
    See :ref:`configure_appsettings` for more information about the hierarchy of the ``appsettings(.*).json``
    files and the settings that can be configured.

* Add a setting for the connectionstring to the appsettings.instance.json file::

      "DbOptions" : { "ConnectionString" : "<paste the connection string to your ViSi database here>" },

* Add the ``SupportedInteractions`` section. You can look at :ref:`disable_interactions` to check what this section should contain.
  For now you only need ``"WholeSystemInteractions": "capabilities"``, ``"InstanceLevelInteractions": "read"`` and
  ``"TypeLevelInteractions": "search"``:
  ::

    "SupportedInteractions": {
        "InstanceLevelInteractions": "read",
        "TypeLevelInteractions": "search",
        "WholeSystemInteractions": "capabilities"
    },

* Add the ``SupportedModel`` section to indicate which resource types and search parameters you support in your Facade
  implementation::

    "SupportedModel": {
      "RestrictToResources": [ "Patient" ],
      "RestrictToSearchParameters": ["Resource._id", "StructureDefinition.url"]
    },

* You will need to add your repository to the Firely Server pipeline, and remove the existing repository implementations.
  The standard settings for the pipeline configuration can be found in the appsettings.default.json file, or see
  :ref:`vonk_plugins_config` for an example.

  * Copy the whole PipelineOptions section to your appsettings.instance.json file (both ``/`` and ``/administration``)
  * To the ``Include`` part of the branch with ``"Path":"/"`` add your namespace, and remove the Vonk.Repository.* lines from it:

    ::

      {
        "Path": "/",
        "Include": [
          "Vonk.Core",
          "Vonk.Fhir.R3",
          "Vonk.Subscriptions",
          "Vonk.Smart",
          "Vonk.UI.Demo",
          "ViSiProject"  // fill in (a prefix of) the namespace of your project here
        ]
      },

  * Remove the PipelineOptions from appsettings.default.json, because of the warning mentioned on the :ref:`configure_levels`.

Test your work
--------------
Proceed to the next section to test your Facade, and for some helpful tips about debugging your code.
