.. _vonk_reference_api_pipeline_configuration:

Pipeline configuration
======================

VonkConfigurationAttribute
--------------------------

:namespace: Vonk.Core.Pluggability

:purpose: This attribute is used on a static class to make Firely Server recognize it as part of the configuration of the processing pipeline. See :ref:`vonk_plugins_configclass`. 

:properties:

   * ``Order``: Determines the place in the pipeline. See :ref:`vonk_plugins_order` for background.

   .. (Firely Server (Vonk) 3.1.0) * ``IsLicensedAs``: If this configuration configures functionality that is licensed (that usually means: payed for), this defines the token that must be listed in the Firely Server license file to enable this configuration. We advise to use a url that is within your web domain as a token, e.g. ``http://acme.com/vonk/plugins/myawesomeplugin``.
