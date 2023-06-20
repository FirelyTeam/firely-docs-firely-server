.. _firely_auth_deploy:

Deployment of Firely Auth
=========================

License
-------

You can use your Firely Server license file, provided that it contains the license token for Firely Auth: ``http://fire.ly/server/auth``.
This token is included in the evaluation license for Firely Server, and in your production license if Firely Auth is included in your order.

Configure the path to the license file in the appsettings, section :ref:`firely_auth_settings_license`.

.. _firely_auth_deploy_exe:

Executable / binaries
---------------------

You can request a copy of the binaries from Firely through our :ref:`contact page <vonk-contact>`

.. You can download the binaries in a zip file from `the downloadserver <https://downloads.simplifier.net/firely-server/firely-auth-latest.zip>`_

.. _firely_auth_deploy_docker:

Docker image
------------

A docker image is available on the Docker hub, under `firely/auth`.

See the instructions on :ref:`running Firely Server in Docker <use_docker>` to learn about adjusting settings and providing the license file.
Firely Auth is configured in the same way.

.. _firely_auth_deploy_inmemory:

InMemory user store
-------------------

The InMemory user store is only meant for testing your setup or evaluating Firely Auth.
For production use configure the SQL Server user store.

The users for the InMemory user store can be configured in :ref:`firely_auth_settings_userstore`

.. _firely_auth_deploy_sql:

SQL Server user store
---------------------

Use of the SQL Server user store requires Microsoft SQL Server version 2016 or newer.

Using your favorite database administration tool:

- create a new database, e.g. 'firely_auth_store'
- in this database, execute the script ``scripts/InitializeSchema.sql``, available in the binaries
- create a connection string to this database
- configure :ref:`firely_auth_settings_userstore`
  
  .. code-block:: json

    {
      "Type": "SqlServer",
      "SqlServer": {
        "ConnectionString": "<connectionstring from previous step>"
      }
    }

In the connection string you can use a user that is only allowed to read and write from the existing tables, no further DDL is needed.

To add users to the store, you can use the :ref:`firely_auth_mgmt`.
