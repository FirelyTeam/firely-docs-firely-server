
Facade setup configuration
--------------------------

To set up a Firely Server Facade, you will need to create a library with your own implementation of the interfaces for reading and/or writing FHIR resources and provide that as a plugin to Firely Server.

Provide a plugin to Firely Server
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

This leverages the capabilities of :ref:`vonk_plugins`. With this setup you:

- create a new ASP.NET Core library
- include Firely Server NuGet packages
- implement your own repository backend to interface with your data store (can be SQL server or any other medium)
- configure the PipelineOptions to use your library instead of Firely Server's own repository implementation
- configure the PipelineOptions to limit the plugins to those that are supported by your repository implementation.

The benefit of using this approach is that you automatically get to use all of Firely Server's configuration, logging,
Application Insights integration, the :ref:`Administration API<administration_api>`, etc. described in the other sections
of this documentation.

The :ref:`exercise <facadestart>` below uses this setup.

.. note::

  Although we take care to try and avoid breaking changes, please be prepared to retest and update your plugins when you
  choose to update Firely Server.
