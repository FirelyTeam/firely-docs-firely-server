.. _configure_db_vonk:

Database configuration
======================

Firely Server showcases its flexibility by providing multiple database options. In general, Firely Server is split into two logical databases: one database hosting the clinical data, and an additional administration database for all FHIR conformance resources (StructureDefinitions, ValueSets, CodeSystems).

.. image:: ../../images/FirelyStorage.png
  :align: right
  :width: 250px
  :alt: Illustration of Firely server

For the administration database, single-instance deployments should use **SQLite**, as it is lightweight and requires minimal configuration. However, in larger or clustered environments, it is advisable to use a shared SQL-based solution for the administration database to ensure consistency across nodes.

For production usage, the database for clinical data should use either **Microsoft SQL Server** or **MongoDB**, depending on your infrastructure and scaling requirements. These options offer improved performance, scalability, and operational resilience over SQLite.

This separation of clinical and administrative data allows for more flexible configuration, independent scaling, and easier maintenance of your Firely Server deployment.


.. toctree::
   :maxdepth: 1
   :titlesonly:

   db_sqlite
   Using SQL server <sqlserver/db_sql>
   Using MongoDB <mongodb/db_mongo>
   db_memory
