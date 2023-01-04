.. _project_setup:

Starting your project
---------------------

In this step, you will create and set up your project to start building the facade.

Create new project
^^^^^^^^^^^^^^^^^^

#. Open Visual Studio 2017
#. File | New | Project

   * Choose Class Library (.NET Core)
   * Project name and directory at your liking; Click OK


Add Firely Server Packages
^^^^^^^^^^^^^^^^^^^^^^^^^^

1. Tools > NuGet Package Manager > Package Manager Console

   * Run ``Install-Package Vonk.Core``
   * Run ``Install-Package Hl7.Fhir.Specification.STU3`` (if you want to use R3)
   * Run ``Install-Package Hl7.Fhir.Specification.R4`` (if you want to use R4)

.. note:: You can install the latest beta release of the Firely Server packages by adding ``-IncludePrerelease`` to the install command.
