.. |br| raw:: html

   <br />
   
.. _configure_sqlite:

Using SQLite
============

.. note::

  The features described on this page are available in **all** :ref:`Firely Server editions <vonk_overview>`.

SQLite is a file based database engine. The engine itself does not run separately on the server, but in-process in the application, Firely Server in this case. 

For more background on SQLite please refer to the `SQLite documentation <https://sqlite.org/about.html>`_.

SQLite is the default configuration of Firely Server. For the Administration database there is little reason to change this if you don't need to scale Firely Server to multiple instances or deploy to Azure Web Apps.
For the actual runtime data, (the 'Firely Server database') itself, you may run into limitations of SQLite if you put it through its paces. 
You may find one of the other repositories a better fit then. You can safely use different storage engines for Firely Server Data and Firely Server Administration.


.. _sqlite_admin_reasons:

SQLite for Firely Server Administration
----------------------------------------------

Firely Server Administration poses very limited stress on its storage engine, therefore SQLite is adequate and provides several advantages:

*   **Runs out of the box:** SQLite requires no installation of a database engine, but still provides durable storage (unlike the Memory storage). 
    Thus, you don't need to setup anything to run Firely Server Administration and you can download the Firely Server binaries and run them without any further configuration.

*   **Flexible on updates:** Many of the features that we will add to Firely Server require changes to the schema of the Administration database. By only supporting SQLite for this, we can provide these features to you more quickly.

*   **Readymade database:** In the other storage engines, the conformance resources from the specification had to be :ref:`imported<conformance_import>` before Firely Server could start. This would take a couple of minutes.
    Because SQLite is file based, we can run the import process for you and provide you with a readymade Administration database.

*   **Runs with Facades:** Perhaps the most important feature. If you build a Firely Server Facade, the facade will not provide support for hosting conformance resources. 
    With Firely Server Administration on SQLite the facade has its own storage and you can use Firely Server Administration out of the box. This enables e.g. validation against your custom resources (that can be imported from your Simplifier project), subscriptions, and other use cases.

.. important::
    In scenarios where you need to scale Firely Server to multiple instances or use containerized and auto-scale environments (such as Kubernetes and Azure Web Apps), SQLite may not be suitable due to its lack of support for concurrent writes and challenges with synchronizing database files. 
    
    For these cases, consider using :ref:`SQL Server<configure_sql>` or :ref:`MongoDB<configure_mongodb>` for Firely Server Administration Data.


.. _configure_sqlite_data:

Settings for using SQLite for Firely Server Data
------------------------------------------------

*	Changing a setting means overriding it as described in :ref:`configure_change_settings`. 

*   Find the ``Repository`` setting and set it to SQLite if it not already set to that::

	"Repository": "SQLite",

*   Find the section called ``SQLiteDbOptions``. It has these values by default::

        "SQLiteDbOptions": {
            "ConnectionString": "Data Source=./data/vonkdata.db",
            "AutoUpdateDatabase": true
        },

    Firely Server will create the database *file*, but please make sure the *directory* already exists.

*   Find the section called ``PipelineOptions``. Make sure it contains the SQLite repository in the root path::

        "PipelineOptions" : 
        {
            "Branches" : [
                "/" : {
                    "Include" : [
                        "Vonk.Repository.SQLite.SqliteVonkConfiguration"
                        //...
                    ]
                },
                //...
            ]
        }

.. _configure_sqlite_admin:

Settings for using SQLite for Firely Server Administration
----------------------------------------------------------

*   Set the ``SqlDbOptions`` under ``Administration`` for the Administration database similar to those above:
    ::
	
        "Administration" : {
            "Repository": "SQLite",
            "SQLiteDbOptions": {
                "ConnectionString": "Data Source=./data/vonkadmin.db",
                "AutoUpdateDatabase": "true"
            }
        }

    Firely Server will create the database *file*, but please make sure the *directory* already exists.

*   Find the section called ``PipelineOptions``. Make sure it contains the SQLite repository in the administration path::

        "PipelineOptions" : 
        {
            "Branches" : [
                "/": {
                    //...
                },
                "/administration" : {
                    "Include" : [
                        "Vonk.Repository.SQLite.SqliteAdministrationConfiguration"
                        //...
                    ]
                }
            ]
        }


.. _sqlite_importhistory:

Administration import history in SQLite
---------------------------------------

When Firely Server :ref:`imports Conformance resources<conformance_import>`, it keeps record of what is has imported. Unlike the SQL Server and MongoDb engines,
the SQLite storage engine does *not* use the .vonk-import-history.json file for that. Instead, in SQLite the import history is stored within the Administration database itself.

