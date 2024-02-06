.. _vonk_plugins_log_detail:

Detailed logging of loading plugins
======================================

If your plugin or any of the Firely Server plugins appears not to be loaded correctly, you may inspect what happens in more detail in the log. See :ref:`configure_log` for where you can find the log file.
You can vary the log level for ``Vonk.Core.Pluggability.VonkConfigurer`` to hide or reveal details.

.. _vonk_plugins_log_assemblies:

On the ``Information`` level, Firely Server will tell you which assemblies are loaded and searched for ``VonkConfiguration`` attributes:

::

   Looking for Configuration in these assemblies:
      C:\data\dd18\vonk_preview\Vonk.Administration.Api.dll
      C:\data\dd18\vonk_preview\Vonk.Core.dll
      C:\data\dd18\vonk_preview\Vonk.Fhir.R3.dll
      C:\data\dd18\vonk_preview\Vonk.Fhir.R4.dll
      C:\data\dd18\vonk_preview\Vonk.Repository.Generic.dll
      C:\data\dd18\vonk_preview\Vonk.Repository.Memory.dll
      C:\data\dd18\vonk_preview\Vonk.Repository.MongoDb.dll
      C:\data\dd18\vonk_preview\Vonk.Repository.Sql.dll
      C:\data\dd18\vonk_preview\vonk.server.dll
      C:\data\dd18\vonk_preview\Vonk.Server.PrecompiledViews.dll
      C:\data\dd18\vonk_preview\Vonk.Plugin.Smart.dll
      C:\data\dd18\vonk_preview\Vonk.Subscriptions.dll
      C:\data\dd18\vonk_preview\Vonk.UI.Demo.dll
      C:\data\dd18\vonk_preview\plugins\Visi.Repository.dll
      C:\data\dd18\vonk_preview\plugins\Vonk.Facade.Relational.dll

.. _vonk_plugins_log_pipeline:

Also on the ``Information`` level, Firely Server will show the services and middleware as it has loaded, in order.
The list below is also the default pipeline as it is configured for Firely Server.

::

      Configuration:
      /
            FhirR3Configuration                        [100] | Services: V | Pipeline: X
            FhirR4Configuration                        [101] | Services: V | Pipeline: X
            MetadataConfiguration                      [110] | Services: V | Pipeline: X
            LicenseConfiguration                       [120] | Services: V | Pipeline: X
            SerializationConfiguration                 [130] | Services: V | Pipeline: X
            RepositorySearchSupportConfiguration       [140] | Services: V | Pipeline: X
            RepositoryIndexSupportConfiguration        [141] | Services: V | Pipeline: X
            PluggabilityConfiguration                  [150] | Services: V | Pipeline: X
            ViSiConfiguration                          [240] | Services: V | Pipeline: X
            DemoUIConfiguration                        [800] | Services: V | Pipeline: V
            VonkToHttpConfiguration                   [1110] | Services: V | Pipeline: V
            VonkFeaturesExtensions                    [1120] | Services: X | Pipeline: V
            FormatConfiguration                       [1130] | Services: V | Pipeline: V
            LongRunningConfiguration                  [1170] | Services: V | Pipeline: V
            VonkCompartmentsExtensions                [1210] | Services: X | Pipeline: V
            SupportedInteractionConfiguration         [1220] | Services: V | Pipeline: V
            UrlMappingConfiguration                   [1230] | Services: V | Pipeline: V
            ElementsConfiguration                     [1240] | Services: V | Pipeline: V
            FhirBatchConfiguration                    [3110] | Services: V | Pipeline: V
            FhirTransactionConfiguration              [3120] | Services: V | Pipeline: V
            SubscriptionConfiguration                 [3200] | Services: V | Pipeline: V
            ValidationConfiguration                   [4000] | Services: V | Pipeline: V
            DefaultShapesConfiguration                [4110] | Services: V | Pipeline: V
            CapabilityConfiguration                   [4120] | Services: V | Pipeline: V
            IncludeConfiguration                      [4210] | Services: V | Pipeline: X
            SearchConfiguration                       [4220] | Services: V | Pipeline: V
            ProfileFilterConfiguration                [4310] | Services: V | Pipeline: V
            PrevalidationConfiguration                [4320] | Services: V | Pipeline: V
            ReadConfiguration                         [4410] | Services: V | Pipeline: V
            CreateConfiguration                       [4420] | Services: V | Pipeline: V
            UpdateConfiguration                       [4430] | Services: V | Pipeline: V
            DeleteConfiguration                       [4440] | Services: V | Pipeline: V
            ConditionalCreateConfiguration            [4510] | Services: V | Pipeline: V
            ConditionalUpdateConfiguration            [4520] | Services: V | Pipeline: V
            ConditionalDeleteConfiguration            [4530] | Services: V | Pipeline: V
            HistoryConfiguration                      [4610] | Services: V | Pipeline: V
            VersionReadConfiguration                  [4620] | Services: V | Pipeline: V
            InstanceValidationConfiguration           [4840] | Services: V | Pipeline: V
            SnapshotGenerationConfiguration           [4850] | Services: V | Pipeline: V
      /administration
            SqlVonkConfiguration                       [220] | Services: V | Pipeline: X
            SqlAdministrationConfiguration             [221] | Services: V | Pipeline: X
            DatabasePluggabilityConfiguration          [300] | Services: V | Pipeline: X
            VonkToHttpConfiguration                   [1110] | Services: V | Pipeline: V
            VonkFeaturesExtensions                    [1120] | Services: X | Pipeline: V
            FormatConfiguration                       [1130] | Services: V | Pipeline: V
            SecurityConfiguration                     [1150] | Services: V | Pipeline: V
            AdministrationOperationConfiguration      [1160] | Services: V | Pipeline: V
            LongRunningConfiguration                  [1170] | Services: V | Pipeline: V
            VonkCompartmentsExtensions                [1210] | Services: X | Pipeline: V
            SupportedInteractionConfiguration         [1220] | Services: V | Pipeline: V
            UrlMappingConfiguration                   [1230] | Services: V | Pipeline: V
            ElementsConfiguration                     [1240] | Services: V | Pipeline: V
            DefaultShapesConfiguration                [4110] | Services: V | Pipeline: V
            AdministrationSearchConfiguration         [4221] | Services: V | Pipeline: V
            ValidationConfiguration                   [4310] | Services: V | Pipeline: X
            SubscriptionValidationConfiguration       [4330] | Services: V | Pipeline: V
            ChangeInterceptionConfiguration           [4390] | Services: X | Pipeline: V
            AdministrationReadConfiguration           [4411] | Services: V | Pipeline: V
            AdministrationCreateConfiguration         [4421] | Services: V | Pipeline: V
            AdministrationUpdateConfiguration         [4431] | Services: V | Pipeline: V
            AdministrationDeleteConfiguration         [4441] | Services: V | Pipeline: V
            AdministrationImportConfiguration         [5000] | Services: V | Pipeline: V
            CodeSystemLookupConfiguration             [5110] | Services: V | Pipeline: V
            ValueSetValidateCodeInstanceConfiguration [5120] | Services: V | Pipeline: V
            ValueSetValidateCodeTypeConfiguration     [5130] | Services: V | Pipeline: V
            ValueSetExpandInstanceConfiguration       [5140] | Services: V | Pipeline: V
            ValueSetExpandTypeConfiguration           [5150] | Services: V | Pipeline: V
            CodeSystemComposeInstanceConfiguration    [5160] | Services: V | Pipeline: V
            CodeSystemComposeTypeConfiguration        [5170] | Services: V | Pipeline: V

It shows all the configuration classes it found, and whether a ConfigureServices and / or a Configure method was found and executed.
It also displays the value of the ``order`` property of the ``VonkConfiguration`` attribute for each configuration class.
This allows you to determine an appropriate order for your own configuration class.

.. _vonk_plugins_log_includes:

On the ``Verbose`` level, Firely Server will also tell you why each configuration class that is found is being included or excluded. An example:

::

   2018-07-02 12:58:10.586 +02:00 [Firely Server] [Verbose] [Machine: XYZ] [ReqId: ] Searching for configurations in assembly "Vonk.Core, Version=0.7.0.0, Culture=neutral, PublicKeyToken=null"
   2018-07-02 12:58:10.625 +02:00 [Firely Server] [Verbose] [Machine: XYZ] [ReqId: ] "Vonk.Core.Serialization.SerializationConfiguration" was included on "/" because it matches the include "Vonk.Core"
   2018-07-02 12:58:10.625 +02:00 [Firely Server] [Verbose] [Machine: XYZ] [ReqId: ] "Vonk.Core.Serialization.SerializationConfiguration" was not included on "/administration" because it did not match any include
