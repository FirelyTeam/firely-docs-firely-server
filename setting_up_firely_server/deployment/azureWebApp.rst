.. _azure_webapp:

Firely Server deployment on Azure App Service
=============================================

In this section we explain how you can deploy Firely Server in the Azure cloud. 


Getting started
---------------

Before you can run Firely Server, you need to request a license and download Firely Server. See step 1 of :ref:`vonk_basic_installation`.
 
Deployment
----------

#. Go to Azure (https://portal.azure.com)  and create a web app:

   .. image:: ../../images/Azure_01_CreateWebApp.png
      :align: center

#. Choose a name for the webapp, we will use the placeholder `<firely-server-app>`, fill in an existing resource group or create a new one, unselect the unique secure hostname, select Linux for the operation system (OS), use `.Net 8` for the runtime stack and choose a region close to you:

   .. image:: ../../images/Azure_02_ChooseName.png
      :align: center
      :width: 760px


#. Add the trial license file (firelyserver-trial-license.json) to the firely-server-latest.zip by dragging the license file into the zipfile.
#. Create a file called appsettings.json with the following content and add it to the zip file:

   .. code-block:: json

      {
        "License": {
          "LicenseFile": "firelyserver-trial-license.json"
        },
        "Hosting": {
          "HttpPort": 8080,
          "ReverseProxySupport": {
            "Enabled": true,
            "TrustedProxyIPNetworks": [
              "0.0.0.0/0"
            ],
            "AllowAnyNetworkOrigins": true
          }
        }
      }

   This configuration file configures Firely Server to use the provided license file, to listen on port 8080 (the port used by Azure Web Apps for Linux) and to support reverse proxy scenarios.

   .. note::
      If you want to use another kind of repository than the SQLite repository, you can add the settings for either :ref:`SQL Server<configure_sql>` or :ref:`MongoDB<configure_mongodb>` in this appsettings.json file.

#. If not using the embedded SQLite database or if loading additional conformance resources, you might have to modify the `web.config` file by adding a `startupTimeLimit` attribute to the `aspNetCore` element to increase the startup time limit. For example, to set the limit to 600 seconds, modify the `web.config` file as follows:

   .. code-block:: xml

      <configuration>
        ...
        <aspNetCore processPath="dotnet" arguments=".\Firely.Server.dll" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" forwardWindowsAuthToken="false" hostingModel="InProcess" startupTimeLimit="600">
          ...
        </aspNetCore>
        ...
      </configuration>

   This change allows Firely Server more time to initialize, which is especially useful when connecting to external databases that may take longer to establish a connection.

#. Open a terminal and use the Azure CLI to deploy the zip file to the web app. Use the following command (replace the placeholders with your own values):

   .. code-block:: bash

      az webapp deploy \
         --resource-group <resource-group> \
         --name <firely-server-app> \
         --src-path <path-to-zip-file> \
         --type zip


   After deploying the .zip file using the Azure CLI, verify that all content has been extracted into the top-level webroot directory.
   
   .. image:: ../../images/Azure_03_WebRoot.png
      :align: center
      :width: 900px

#. Open a browser and go to the site ``https://<firely-server-app>.azurewebsites.net/`` . This will show the Firely Server home page.

Change database
---------------

In this example Firely Server is using a SQLite repository. If you want to change it to another kind of repository then you could change that on the page Application Settings of the Web App. Here you can set :ref:`Environment Variables<configure_envvar>` 
with the settings for either :ref:`SQL Server<configure_sql>` or :ref:`MongoDB<configure_mongodb>`. For example for SQL Server it will look like this:

.. image:: ../../images/Azure_04_Settings.png
   :align: center
   :width: 900px

More information
----------------
About Azure zip deployment: https://learn.microsoft.com/en-us/azure/app-service/deploy-zip?tabs=cli

.. important::

   * We recommend using either SQL Server or MongoDB as both the data and administration repositories when deploying Firely Server as an Azure Web App in Production due to autoscaling and file handling. See :ref:`Database configuration<configure_db_vonk>` for details on configuring these databases.

   * It's important to configure `Azure App Service Health Checks <https://learn.microsoft.com/en-us/azure/app-service/monitor-instances-health-check?tabs=dotnet#enable-health-check>`_ to target the /$liveness endpoint. This ensures the app has enough time to load conformance resources and start properly. Otherwise, Azure may automatically restart the app before initialization completes. See :ref:`$Liveness <feature_healthcheck>` for more information.
  


