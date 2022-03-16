.. _vonk_faq:

Frequently asked questions
==========================

... and known issues.

Conflicting resources upon import
---------------------------------

When importing specification.zip for R4, Firely Server will report errors like these::

::

   Artifact C:\Users\<user>\AppData\Local\Temp\FhirArtifactCache-1.5.0-Hl7.Fhir.R4.Specification\specification_Fhir4_0\dataelements.xml could not be imported. Error message: Found multiple conflicting resources with the same resource uri identifier.

   Url: http://hl7.org/fhir/StructureDefinition/de-Quantity.value
      File: C:\Users\Christiaan\AppData\Local\Temp\FhirArtifactCache-1.5.0-Hl7.Fhir.R4.Specification\specification_Fhir4_0\dataelements.xml
      File: C:\Users\Christiaan\AppData\Local\Temp\FhirArtifactCache-1.5.0-Hl7.Fhir.R4.Specification\specification_Fhir4_0\dataelements.xml
      File: C:\Users\Christiaan\AppData\Local\Temp\FhirArtifactCache-1.5.0-Hl7.Fhir.R4.Specification\specification_Fhir4_0\dataelements.xml

The error message is actually correct, since there *are* duplicate fullUrls in dataelements.xml in the specification. This has been reported in `Jira issue FHIR-25430 <https://jira.hl7.org/browse/FHIR-25430>`_.

Searchparameter errors for composite parameters
-----------------------------------------------

When importing specification.zip for various FHIR versions, Firely Server will report errors like these:

::

   Composite SearchParameter 'CodeSystem.context-type-quantity' doesn't have components.

A searchparameter of type 'composite' should define which components it consists of. Firely Server checks whether all the components of such a composite searchparameter are present. If no components are defined at all - that is, SearchParameter.component is empty - it will display this error. This indicates an error in the definition of the searchparameter and should be solved by the author of it.

However, the implementation of this check seems to have an error so too many composite parameters are reported as faulty. We will address this issue in the next release.

.NET SDK not found
------------------

Since version 4.0 Vonk was renamed to Firely Server, including the main entrypoint. It changed from ``vonk.server.dll`` to ``firely.server.dll``.

If you now still run ``dotnet vonk.server.dll`` on .NET runtime 3.1 it will state this error:

   ::
      It was not possible to find any installed .NET Core SDKs
      Did you mean to run .NET Core SDK commands? Install a .NET Core SDK from: https://aka.ms/dotnet-download

This is very misleading. The actual error is that you probably tried to run ``dotnet vonk.server.dll`` but this dll no longer exists.

The same error can happen if you have built a Docker image of your own with ``dotnet vonk.server.dll`` as entrypoint.

.NET 5 fixed this and more clearly states that the dll is missing.

Homepage takes long to load
---------------------------

The html homepage that is provided with Firely Server may take a long time to load, even though the server seems fully up and running.

The homepage consumes the bootstrap.js library from a CDN. The delay may be caused by a firewall slowing down that download.

The remedy is to disable the homepage in the pipelinesettings:

   ::
   
      "PipelineOptions": {
        "PluginDirectory": "./plugins",
        "Branches": [
        {
          "Path": "/",
          "Include": [
            "Vonk.Core",
            ...,
            // "Vonk.UI.Demo", <-- disable this one
            ...
          ],
        },
        {
          "Path": "/administration",
          ...
        }
        ]
      }
