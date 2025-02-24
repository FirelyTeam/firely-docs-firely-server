Together with Firely Server v6, we released the new way of migrating your MongoDB database. Contrary to the old migration approach where you had to allocate some maintenance window and stop your database operation while migrating the data to the new database schema, this new approach is a zero-downtime approach. This means that you can keep your old version of Firely Server operating while migrating data to the new schema.

Use the migration approach described on this page to migrate your *vonkdata* database.
If your *vonkadmin* DB is also stored in MongoDB, use also this approach.

Please note that bulk data export data will not get migrated.

The diagram below illustrates to the components involved in such a migration.

![Migration Diagram](migration-diagram.svg)

* You can see the old database containing old data
* You can see the old Firely Server instance connected to the old database instance. This instance can accept HTTP requests during the migration normally.
* There's also a new database instance for the migrated data. Initially it is empty.
* The migration tool is running in the background continuously converting the old items into the new items and populating the new database.
* There is also a new instance of Firely Server connected to interact with the new database.
* The Firely Server instances are accessible through a reverse proxy.

Please note that during the migration both databases will be running side by side.

The migration can be performed by following the steps below:

1. Think where you want to provision your new database. Write down and keep the connection string for further references.
2. If you intend to use sharding, first you need to provision the database and configure sharding. This involves some manual tests.
      - Make sure your MongoDb installation support sharding. If you are not sure, please refer to the MondoDB documentation.
      - First, you need to execute the following command to provision the schema. You can do that using the following command.

        ```bash
        export COLLECTION_NAME=vonkentries
        export CONNECTION_STRING=<YOUR_CONNECTION_STRING>
        export LICENSE_FILE=<path-to-your-license-file>

        mkdir empty_dir

        dotnet fsi.dll \
          --provisionTargetDatabase true \
          --dbType MongoDb \
          --mongoConnectionstring $CONNECTION_STRING \
          --mongoCollection $COLLECTION_NAME \
          --license $LICENSE_FILE \
          --source empty_dir
          
        rm -rf empty_dir
        ```
      - Then, you need to configure sharding for the entries collection using the command below

        ```bash
        export DB_NAME=vonkdata
        export COLLECTION_NAME=vonkentries
        export CONNECTION_STRING=<YOUR_CONNECTION_STRING>

        mongosh $CONNECTION_STRING <<EOF
        sh.shardCollection("$DB_NAME.$COLLECTION_NAME", { type: 1, im: 1, cur: 1, cnt: 1, change: 1, res_id: "hashed" });
        EOF
        ```
3. The next step is to run FSI in the migration mode.
  
    - Start the migration process by executing the following command.
      ```bash
      RECOVERY_JOURNAL_DIRECTORY=./journal # A directory where the progress will be stored in case of a crash. FSI will quickly catch up to the place where an error occurred.

      LICENSE_FILE="<path to your license file>"

      RUNNING_MODE=Continuous # Or AdHoc. If AdHoc mode is used, FSI will terminate when all the items from the old DB have been processed.

      SOURCE_CONNECTION_STRING="<old db connection string>"
      SOURCE_COLLECTION_NAME=vonkentries

      CONNECTION_STRING=<new DB connection string>
      COLLECTION_NAME=vonkentries

      dotnet fsi.dll \
          --provisionTargetDatabase true \
          --useRecoveryJournal $RECOVERY_JOURNAL_DIRECTORY \
          --sourceType MongoDb \
          --srcMongoCollection $SOURCE_COLLECTION_NAME \
          --srcMongoConnectionString $SOURCE_CONNECTION_STRING \
          --srcMongoRunningMode $RUNNING_MODE \
          --update-existing-resources ErrorOnConflict \
          --dbType MongoDb \
          --mongoConnectionstring $CONNECTION_STRING \
          --mongoCollection $COLLECTION_NAME \
          --license $LICENSE_FILE
      ```

    - When all the items in the old database have been migrated to the new database, you will see the following message: `No new items found in the database. Waiting for 00:00:05 before retrying...`.
4. Re-configure your reverse proxy to route HTTP requests to the new file server instance
5. Now you can terminate the FSI migration tool and delete the old Firely Server installation.