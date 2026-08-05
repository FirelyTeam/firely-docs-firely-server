.. _vonk_plugins:

Firely Server Custom Plugins
============================

.. note::

  The features described on this page are available in the following :ref:`Firely Server editions <vonk_overview>`:

  * Firely Scale - 🌍 / 🇺🇸
  * Firely Prior Authorization - 🇺🇸

Firely Server Plugins is the means to adjust a Firely Server to your own special needs, beyond the configuration.
Please have a look at the :ref:`architecture` to see how plugins fit in the Firely Server.

A working example of a custom plugin is the :ref:`$document operation <feature_documentoperation>`, which generates a document bundle from a resource.

.. note::

  A plugin has to target the same .NET version as the Firely Server it is loaded into. As of Firely Server 6.9.0
  that is ``net10.0`` (C# 14); Firely Server 5.7.0 up to and including 6.8.x target ``net8.0``. When upgrading to
  Firely Server 6.9.0 or later, recompile your plugins against ``net10.0``.

.. toctree::
   :maxdepth: 1
   :titlesonly:

   plugins_config
   plugins_configclasses
   plugins_log
   plugins_order
   plugins_classes
   plugins_template
   plugins_directhttp
 
 