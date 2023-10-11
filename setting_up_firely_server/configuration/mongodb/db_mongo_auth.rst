.. _configure_mongodb_auth:

MongoDB authorization
=====================

In a production setting, MongoDB should run with authorization enabled. This page helps you to:

#. Set up an administrative user
#. Set up user accounts to use with Firely Server
#. Enable the authorization
#. Configure the connectionstring

All of the script snippets in this page are combined in :download:`this script file </_static/files/MongoDB_roles_users.js>`. It can be run with `Mongo Shell <https://www.mongodb.com/try/download/shell>`_ (``mongosh``).
At the time of writing the ``mongosh`` integrated in MongoDB Compass unfortunately does not yet support loading script files.

.. note:: 

    The scripts and settings on this page are provided as guidance. They primarily highlight the minimum privileges needed to run Firely Server.
    If you run Firely Server on MongoDB, you are strongly advised to have these reviewed and implemented by a trained MongoDB database administrator.

.. attention:: 

    All users are configured with an easy to comprehend password to make the guidance as clear as possible. You must replace them by **strong passwords** before executing the scripts.

Convenience functions
---------------------

The scripts use these two functions for convenience.

.. code-block:: javascript

    admin = db.getSiblingDB('admin');

    function userExists (database, userName){
        return admin.system.users.find({db: database, user:userName}).count() > 0
    }

    function roleExists (database, roleName){
        return admin.system.roles.find({db: database, role:roleName}).count() > 0
    }


Set up an administrative user
-----------------------------

It is important to set up this user before you enable authorization, otherwise you might lock yourself out of any actions on the database. 
The script below will create a user name ``admin`` and password ``admin_secret``, authorized to do anything on any database.

.. code-block:: javascript

    admin = db.getSiblingDB('admin');

    if (!userExists("admin", "admin"))
    {
        print("creating MongoDB admin user")
        admin.createUser(
            {
                user: "admin",
                pwd: "admin_secret",
                roles:[
                    {
                        role: "userAdminAnyDatabase", db: "admin"
                    },
                    {
                        role: "dbAdminAnyDatabase", db: "admin"
                    },
                    {
                        role: "readWriteAnyDatabase", db: "admin"
                    }
                ]
            }
        );
    }
    else
    {
        print("MongoDB admin user already existed and was not altered");
    }

Set up user accounts to use with Firely Server
----------------------------------------------

MongoDB can be used to host the data of Firely Server - the regular resources - as well as the administration resources. Both need the same set of access rights, but on different databases.
We start with setting up the roles and users on for the data database. We assume ``fs_data`` as the name of the database.

User accounts for the Data database
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

.. code-block:: javascript

    fs_data = db.getSiblingDB('fs_data');

    print("dropping fs_data users and roles")
    if (userExists("fs_data", "fs_data_user"))
        fs_data.dropUser("fs_data_user");
    if (userExists("fs_data", "fs_data_upgrade_user"))
        fs_data.dropUser("fs_data_upgrade_user")
    if (roleExists("fs_data", "fs_user_role"))
        fs_data.dropRole("fs_user_role")
    if (roleExists("fs_data", "fs_upgrade_role"))
        fs_data.dropRole("fs_upgrade_role")

    print("creating role fs_data.fs_user_role");
    fs_data.createRole(
        {
            role: "fs_user_role",
            privileges: [
                {
                    resource:{db: "fs_data", collection: ""},
                    actions:[
                        "insert",
                        "update",
                        "remove",
                        "useUUID",
                        "bypassDocumentValidation",
                        "changeStream"
                    ]
                }
            ],
            roles: [
                {role: "read", db: "fs_data"}
            ]
        }
    );

    print("creating role fs_data.fs_upgrade_role");
    fs_data.createRole(
        {
            role: "fs_upgrade_role",
            privileges: [
                {
                    resource:{db: "fs_data", collection: ""},
                    actions:[
                        "createCollection",
                        "createIndex",
                        "dropCollection",
                        "dropIndex",
                        "killAnyCursor",
                        "listDatabases",
                        "listCollections"
                    ]
                }
            ],
            roles:[
                {role: "fs_user_role", db: "fs_data"}
            ]
        }
    );

    print("creating user fs_data.fs_data_user");
    fs_data.createUser(
        {
            user: "fs_data_user",
            pwd: "fs_data_secret",
            roles:[
                {
                    role: "fs_user_role", db: "fs_data"
                }
            ]
        }
    );

    print("creating user fs_data.fs_data_upgrade_user");
    fs_data.createUser(
        {
            user: "fs_data_upgrade_user",
            pwd: "fs_data_upgrade_secret",
            roles:[
                {
                    role: "fs_upgrade_role", db: "fs_data"
                }
            ]
        }
    );

User accounts for the Administration database
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

These accounts are only needed if you run the :ref:`Administration database on MongoDB <configure_mongodb_admi>`. By default Firely Server uses SQLite for the Administration database.

We assume ``fs_admin`` as the name of the database.

.. code-block::javascript

    print("dropping fs_admin users and roles")
    if (userExists("fs_admin", "fs_admin_user"))
        fs_admin.dropUser("fs_admin_user")
    if (userExists("fs_admin", "fs_admin_upgrade_user"))
        fs_admin.dropUser("fs_admin_upgrade_user")
    if (roleExists("fs_admin", "fs_user_role"))
        fs_admin.dropRole("fs_user_role")
    if (roleExists("fs_admin", "fs_upgrade_role"))
        fs_admin.dropRole("fs_upgrade_role")

    print("creating role fs_admin.fs_user_role");
    fs_admin.createRole(
        {
            role: "fs_user_role",
            privileges: [
                {
                    resource:{db: "fs_admin", collection: ""},
                    actions:[
                        "insert",
                        "update",
                        "remove",
                        "useUUID",
                        "bypassDocumentValidation",
                        "changeStream"
                    ]
                }
            ],
            roles: [
                {role: "read", db: "fs_admin"}
            ]
        }
    );

    print("creating role fs_admin.fs_upgrade_role");
    fs_admin.createRole(
        {
            role: "fs_upgrade_role",
            privileges: [
                {
                    resource:{db: "fs_admin", collection: ""},
                    actions:[
                        "createCollection",
                        "createIndex",
                        "dropCollection",
                        "dropIndex",
                        "killAnyCursor",
                        "listDatabases",
                        "listCollections"
                    ]
                }
            ],
            roles:[
                {role: "fs_user_role", db: "fs_admin"}
            ]
        }
    );

    print("creating user fs_admin.fs_admin_user");
    fs_admin.createUser(
        {
            user: "fs_admin_user",
            pwd: "fs_admin_secret",
            roles:[
                {
                    role: "fs_user_role", db: "fs_admin"
                }
            ]
        }
    );

    print("creating user fs_admin.fs_admin_upgrade_user");
    fs_admin.createUser(
        {
            user: "fs_admin_upgrade_user",
            pwd: "fs_admin_upgrade_secret",
            roles:[
                {
                    role: "fs_upgrade_role", db: "fs_admin"
                }
            ]
        }
    );

Enable authentication on MongoDB
--------------------------------

Authorization is enabled in different ways depending on the hosting platform. See the MongoDB documentation on this.

In short, for MongoDB Atlas authorization is mandatory and cannot be disabled. For MongoDB Enterprise or Community it can be enabled by the paramater ``--auth`` to the ``mongod`` command.

When running it in a Docker container, you can add this parameter by changing the ``command``:

.. code-block::yaml
    :linenos:
    :emphasize-lines: 11

    services:

      mongodb_latest:
        image:  mongo:latest
        container_name: mongodb
        ports:
        - 27017:27017
        volumes:
        - mongo_data:/data/db
        - mongo_config:/data/configdb
        command: mongod --auth


ConnectionStrings
-----------------

Once authorization is enabled, you have to configure the user and password in the connectionstring. The connectionstrings below serve as a template, using ``localhost`` as the host. Replace this with the correct hostname for your environment.

.. note:: 

    Currently, only a single connectionstring can be configured for MongoDB. The roles and users above differentiate between the authorization needed to perform an automatic upgrade, and the authorization needed for regular operation.
    You may choose to use a connectionstring with the ``fs_data_upgrade_user`` only when performing an upgrade, and afterwards reset it to the ``fs_data_user``.

#. Data database: ``mongodb://fs_data_upgrade_user:fs_data_upgrade_secret@localhost/fs_data?authSource=fs_data``
#. Administration database: ``mongodb://fs_admin_upgrade_user:fs_admin_upgrade_secret@localhost/fs_admin?authSource=fs_admin``

.. note:: 

    Given that the password is part of the connectionstring it is safer to feed this setting from a secure vault using an environment variable. For other options to log in securely we refer to the MongoDB documentation.
