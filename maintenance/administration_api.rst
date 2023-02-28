.. _administration_api:

Firely Server Administration API
================================

Besides the regular FHIR endpoint, Firely Server also exposes an Administration API. The endpoint for this is:
::

   http(s)://<firely-server-endpoint>/administration

Functions
---------

The following functions are available in the Administration API.

* :ref:`conformance`
* :ref:`feature_subscription`
* :ref:`feature_customsp_reindex`
* :ref:`feature_resetdb`
* :ref:`feature_preload`
* :ref:`feature_terminology`

.. _configure_administration:

Configuration
-------------

You can configure the Administration API, including restricting access to functions of the Administration API to specific ip addresses.This configuration is part of :ref:`configure_appsettings`.

::

  "Administration": {
    "Repository": "SQLite", //Memory / SQL / MongoDb
    "MongoDbOptions": {
      "ConnectionString": "mongodb://localhost/vonkadmin",
      "EntryCollection": "vonkentries"
    },
    "SqlDbOptions": {
      "ConnectionString": "connectionstring to your Firely Server Admin SQL Server database (SQL2012 or newer); Set MultipleActiveResultSets=True",
      "SchemaName": "vonkadmin",
      "AutoUpdateDatabase": true,
      "MigrationTimeout": 1800 // in seconds
      //"AutoUpdateConnectionString" : "set this to the same database as 'ConnectionString' but with credentials that can alter the database. If not set, defaults to the value of 'ConnectionString'"
    },
    "SQLiteDbOptions": {
      "ConnectionString": "Data Source=./data/vonkadmin.db",
      "AutoUpdateDatabase": true,
      "MigrationTimeout": 1800 // in seconds
    },
    "Security": {
      "AllowedNetworks": [ "::1" ], // i.e.: ["127.0.0.1", "::1" (ipv6 localhost), "10.1.50.0/24", "10.5.3.0/24", "31.161.91.98"]
      "OperationsToBeSecured": [ "reindex", "reset", "preload" ]
    }
  },

.. _configure_administration_repository:

Choosing your storage
^^^^^^^^^^^^^^^^^^^^^
The Administration API uses a database separately from the main 'Firely Server Data' database. Historically, SQL Server, MongoDB and Memory are supported as databases for the Administration API.
|br| As of Firely Server (Vonk) version 0.7.1, SQLite is advised for this, and we have made that the default configuration. See :ref:`configure_sqlite` on how to configure for this.


#. ``Repository``: Choose which type of repository you want. Valid values are:

  #. Memory
  #. SQL
  #. SQLite
  #. MongoDb

#. ``MongoDbOptions``: Use these with ``"Repository": "MongoDb"``, see :ref:`configure_mongodb` for details.
#. ``SqlDbOptions``: Use these with ``"Repository": "SQL"``, see :ref:`configure_sql` for details.
#. ``SQLiteDbOptions``: Use these with ``"Repository": "SQLite"``, see :ref:`configure_sqlite` for details.

.. _configure_administration_access:

Limited access
^^^^^^^^^^^^^^

#. ``Security``: You can restrict access to the operations listed in ``OperationsToBeSecured`` to only be invoked from the IP addresses listed in ``AllowedNetworks``.

  * Operations that can be secured are:

    * ``reindex`` (see :ref:`feature_customsp_reindex`)
    * ``reset`` (see :ref:`feature_resetdb`)
    * ``preload`` (see :ref:`feature_preload`)
    * ``StructureDefinition`` (restrict both read and write)
    * ``SearchParameter`` (restrict both read and write)
    * ``ValueSet`` (restrict both read and write)
    * ``CodeSystem`` (restrict both read and write)
    * ``CompartmentDefinition`` (restrict both read and write)
    * ``Subscription``: (restrict both read and write, see :ref:`feature_subscription`)

  * The ``AllowedNetworks`` have to be valid IP addresses, either IPv4 or IPv6, and masks are allowed.

.. |br| raw:: html

   <br />

