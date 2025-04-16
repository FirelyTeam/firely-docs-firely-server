.. _administration_api:

Firely Server Administration API
================================

The Firely Server is fully manageable through RESTful interactions via the Administration API. This API provides access to the administration database and supports a range of maintenance operations.
::

   http(s)://<firely-server-endpoint>/administration

The following functions are available in the Administration API:

* :ref:`conformance`
* :ref:`Enabling Subscriptions <feature_subscription>`
* :ref:`feature_customsp_reindex`
* :ref:`Reseting the database <feature_resetdb>`
* :ref:`feature_preload`
* :ref:`Executing terminology operations <feature_terminology>`

.. _configure_administration:

Configuration
-------------

You can configure the Administration API, including restricting access to functions of the Administration API to specific ip networks.This configuration is part of :ref:`configure_appsettings`.

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
    "AllowedNetworks": [ "127.0.0.1/32", "::1/128" ] // IPv4 and IPv6 localhost with explicit subnet masks
  },

.. _configure_administration_repository:

Choosing your storage
---------------------
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

Limiting access
---------------

You can restrict access to administrative operations by setting the ``NetworkProtected`` property to ``true`` in each operation's configuration under ``Administration.Operations``:

.. code-block:: json

    "Administration": {
      "AllowedNetworks": ["127.0.0.1", "::1"],
      "Operations": {
        "$reindex": {
          "Name": "$reindex",
          "Level": ["System"],
          "Enabled": true,
          "NetworkProtected": true
        },
        "$reset": {
          "Name": "$reset",
          "Level": ["System"],
          "Enabled": true,
          "NetworkProtected": true
        }
      }
    }

The ``AllowedNetworks`` property defines which IP networks can access operations with ``NetworkProtected`` set to ``true``.

Operations that can be secured include:

* ``$reindex`` and ``$reindex-all`` (see :ref:`feature_customsp_reindex`)
* ``$reset`` (see :ref:`feature_resetdb`)
* ``$preload`` (see :ref:`feature_preload`)
* ``$import-resources`` (see :ref:`conformance_on_demand`)
* ``StructureDefinition`` (restrict both read and write)
* ``SearchParameter`` (restrict both read and write)
* ``ValueSet`` (restrict both read and write)
* ``CodeSystem`` (restrict both read and write)
* ``CompartmentDefinition`` (restrict both read and write)
* ``StructureMap`` (restrict both read and write)
* ``ConceptMap`` (restrict both read and write)
* ``Library`` (restrict both read and write)
* ``Measure`` (restrict both read and write)
* ``Subscription``: (restrict both read and write, see :ref:`feature_subscription`)

The following rules apply for network configuration:

* The ``AllowedNetworks`` have to be valid IP networks, either IPv4 or IPv6, and providing the subnet prefix length explicitly is recommended. If you provide a 'bare' IP Address, it will be interpreted as a ``/32`` for IPv4 and ``/128`` for IPv6, effectively reducing it to a single host network.
* We recommend to only use internal, single host networks.

Examples:
    
* ``127.0.0.1/32`` (IPv4 localhost)
* ``::1/128`` (IPv6 localhost)
* ``192.168.0.18/32`` (IPv4 single host)
* ``10.0.0.1/24`` (IPv4 network ranging from ``10.0.0.0`` to ``10.0.0.255``, not recommended)

.. warning::

    Are you hosting Firely Server behind a reverse proxy? Please review other relevant settings here: :ref:`X_Forwarded_Host`.

.. warning::

    If you run Firely Server **version 5.6.0 or older**, you MUST provide the subnet prefix length explicitly. 
    If you do not, the subnet will be based on the class of the IP address, which usually leads to ``/24`` for IPv4. 
    This may allow for more IP addressess than you intended to be able to access the restricted operations.

.. note::

   If these operations are not used on the Administration API, it is recommended to remove them from the API altogether:
   
    * ``$reindex``
    * ``$reindex-all``
    * ``$reset``
    * ``$preload``
    * ``$import-resources``
    
   To do so, add ``Vonk.Administration.Api.AdministrationOperationConfiguration`` to the Exclude list in the ``PipelineOptions``:
    
    .. code-block:: json
    
         "PipelineOptions": {
            "Branches": [
                {
                    "Name": "administration",
                    "Include": [
                        "Vonk.Administration",
                        ...
                    ],
                    "Exclude": [
                        "Vonk.Administration.Api.AdministrationOperationConfiguration"
                    ]
                }
            ]
         }

.. |br| raw:: html

   <br />

