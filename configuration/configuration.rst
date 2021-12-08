.. _configure_vonk:

Configuring Firely Server
=========================

In this section we assume you have downloaded and installed the Firely Server binaries, and have obtained a license file.
If not, please see the :ref:`vonk_getting_started` and follow the steps there first.

The steps you followed to get started will provide you with a basic Firely Server,
that runs on a standard port and keeps the data in a SQLite database.

If you need to adjust the port, or want to use a MongoDB or SQL database you can
configure Firely Server by adjusting the :ref:`configure_appsettings`.

If you want to change the way Firely Server logs its information, you can adjust the :ref:`configure_log`.

.. toctree::
   :maxdepth: 1
   :titlesonly:

   appsettings
   environment_variables
   administration
   ../features/conformanceresources
   ../features/prevalidation
   db_memory
   db_mongo
   db_sql
   db_sqlite
   hosting
   cors
   logsettings
