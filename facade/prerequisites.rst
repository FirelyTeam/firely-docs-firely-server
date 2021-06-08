.. _preparations:

Prerequisites and Preparations
------------------------------

* Experience with programming ASP.NET (Core) libraries.
* Basic understanding of the `FHIR RESTful API <http://hl7.org/fhir/http.html>`_ and FHIR servers.
* Visual Studio 2017 or newer

   #. get a free community edition at https://www.visualstudio.com/downloads/
   #. be sure to select the components for C# ASP.NET Core web development

* .NET Core 2.0 SDK, from https://www.microsoft.com/net/download/windows

   #. this is probably installed along with the latest Visual Studio, but needed if your VS is not up-to-date.

* SQL Server 2012 or newer:

   #. get a free developer or express edition at https://www.microsoft.com/en-us/sql-server/sql-server-downloads
   #. add SQL Server Management Studio from https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms

* Postman, Fiddler or a similar tool to issue http requests and inspect the responses.

Installing the Firely Server
^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Before you can start implementing your facade, you need to have the Firely Server installed.
See :ref:`vonk_getting_started` on how to download the binaries and license key.

Preparing the database
^^^^^^^^^^^^^^^^^^^^^^

Download the :download:`CreateDatabase script <CreateDatabase.sql>`, and create a SQL Server database with it.

It creates a database 'ViSi' with two tables: Patient and BloodPressure. You can familiarize yourself with the table structure and
contents to prepare for the mapping to FHIR later on.

Proceed to the next step to start your facade project.
