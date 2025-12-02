.. _vonk_plugins_config:

Configure the pipeline with custom plugins
==========================================

The configuration namespace of the custom plugin can be added to the pipeline of Firely Server. See :ref:`vonk_available_plugins` for more information.
Note that custom plugins need to be added to the ``PluginDirectory``. You can put plugins of your own (or third party) into this directory for Firely Server to pick them up, without polluting the Firely Server binaries directory itself. The directory in the default setting of ``./plugins`` is not created upon install, you may do this yourself if you want to add a plugin.