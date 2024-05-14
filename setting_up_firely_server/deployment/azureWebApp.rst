.. _azure_webapp:

Firely Server deployment on Azure Web App Service
=================================================

In this section we explain how you can deploy Firely Server in the Azure cloud. 

Getting started
---------------

Before you can run Firely Server, you need to request a license and download Firely Server. See step 1 of :ref:`vonk_basic_installation`.
 
Deployment
----------

#. Go to Azure (https://portal.azure.com)  and create a web app:

   .. image:: ../../images/Azure_01_CreateWebApp.png
      :align: center

#. Choose a name for the webapp, we will use the placeholder <webapp>. Fill in an existing resource group or create a new one and select Windows for the operation system (OS):

   .. image:: ../../images/Azure_02_ChooseName.png
      :align: center
      :width: 760px

#. Add the trial license file (firelyserver-trial-license.json) to the firely-server-latest.zip by dragging the license file into the zipfile.
#. Open a webbrowser, navigate to ``https://<webapp>.scm.azurewebsites.net/ZipDeployUI`` and drag vonk_distribution.zip into the browser window. 
   This will install the Firely Server as a Web App in Azure.
   In our example the url is ``https://firelyserver.scm.azurewebsites.net/ZipDeployUI``
   This method of deployment does not work in Internet Explorer. It does work in Firefox, Chrome and Edge.
   Please make sure that after you have uploaded the .zip file, all content is extracted into the top-level webroot directory.
   
   .. image:: ../../images/Azure_05_WebRoot.png
      :align: center
      :width: 900px
   
#. Open a browser and go to the site ``https://<webapp>.azurewebsites.net/`` . This will show the Firely Server home page.

Change database
---------------

In this example Firely Server is using a memory repository. If you want to change it to another kind of repository then you could change that on the page Application Settings of the Web App. Here you can set :ref:`Environment Variables<configure_envvar>` 
with the settings for either :ref:`SQL Server<configure_sql>` or :ref:`MongoDB<configure_mongodb>`. For example for MongoDB it will look like this:

.. image:: ../../images/Azure_04_Settings.png
   :align: center
   :width: 900px

More information
----------------
About Azure zip deployment: https://docs.microsoft.com/en-us/azure/app-service/app-service-deploy-zip#deploy-zip-file

