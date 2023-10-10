// Script to set up users and roles for use with Firely Server.
// See https://docs.fire.ly/firely-server/setting_up_firely_server/configuration/mongodb/db_mongo_auth.html

admin = db.getSiblingDB('admin');
fs_data = db.getSiblingDB('fs_data');
fs_admin = db.getSiblingDB('fs_admin');

function userExists (database, userName){
    return admin.system.users.find({db: database, user:userName}).count() > 0
}

function roleExists (database, roleName){
    return admin.system.roles.find({db: database, role:roleName}).count() > 0
}

//=== MongoDB admin user - run before enabling auth
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

//==== Data database
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

// ===== Administration database

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
