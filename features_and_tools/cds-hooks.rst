.. _feature_cds_hooks:

=========
CDS Hooks
=========

.. note::

  The features described on this page are available in the following :ref:`Firely Server editions <vonk_overview>`:

  * Firely CMS Compliance - 🇺🇸

.. warning::

  The CDS Hooks feature is currently in beta. It's implementation and programming API may change.
  
Description
-----------
CDS Hooks is a specification that allows healthcare applications to integrate with clinical decision support systems. It enables the execution of decision support logic at the point of care, providing clinicians with relevant information and recommendations based on the context of patient care. For background and the specification, please consult the `official CDS Hooks documentation <https://cds-hooks.hl7.org/>`_.

Firely Server supports the CDS Hooks specification, allowing you to create and manage CDS Hooks services. This feature is particularly useful for organizations that must conform to regulations based on Implementation Guides using CDS Hooks. Examples include Da Vinci Implementation Guides for CDR, DTR and PAS, contributing to the electronic Prior Authorization workflow.

CDS Hooks services are only available in **FHIR R4**. If Firely Server hosts multiple FHIR versions, the CDS Hooks services will only be available for the R4 version. The services are not available for STU3 or R5.

The CDS Hooks endpoint is available at the following URL: ``<base-url>/cds-services``. This endpoint returns a list of available CDS Hooks services in the system.
Any registered CDS Hooks service can be invoked by sending a POST request to ``<base-url>/cds-services/{hook service id}``.

Enabling CDS Hooks
------------------
To enable the CDS Hooks feature, you need to add the plugin ``Vonk.Plugin.CdsHooks.Configuration`` to the ``PipelineOptions`` in the appsettings.

.. code-block:: JavaScript

  "PipelineOptions": {
    "PluginDirectory": "./plugins",
    "Branches": [
      {
        "Path": "/",
        "Include": [
          ...
          "Vonk.Plugin.CdsHooks.Configuration"
        ],
        ...
      }
    ]
  }
  
Furthermore, you have to ensure that the license token ``http://fire.ly/server/plugins/cds-hooks`` is present in the license file.

You can now access the CDS Hooks Discovery document at ``<base-url>/cds-services``, e.g.

.. code-block:: HTTP

  GET <base-url>/cds-services

Configuring an example CDS Hooks service
----------------------------------------
Firely Server provides an example CDS Hooks service to demonstrate how to configure and use CDS Hooks.

To configure the example service, add the plugin ``Vonk.Plugin.CdsHooks.PatientViewTestHook`` to the ``PipelineOptions`` in the appsettings.

.. code-block:: JavaScript

    "PipelineOptions": {
        "PluginDirectory": "./plugins",
        "Branches": [
        {
            "Path": "/",
            "Include": [
            ...
            "Vonk.Plugin.CdsHooks.PatientViewTestHook"
            ],
            ...
        }
        ]
    }

Furthermore, you have to enable the service as a custom operation in the ``Operations`` section of the :ref:`appsettings <disable_interactions>`. 
The ``Level`` is always ``System``. The example service is registered as a custom operation with the name ``cds-patient-view-test-hook``.
See also the :ref:`CDS Hooks operations <cds_hooks_operations>` section for more information on how to configure CDS Hooks services as custom operations.

.. code-block:: JavaScript

  "Operations": {
    "$cds-patient-view-test-hook": {
      "Name": "$cds-patient-view-test-hook",
      "Level": [
        "System"
      ],
      "Enabled": true,
      "RequireAuthorization": "Never",
      "RequireTenant": "Never"
    }
  },
    
The example service is not protected by a license token.

When you have added the plugin, you can request the CDS Hooks Discovery document again to see that the service is listed:
 
.. code-block:: json

  {
    "services": [
      {
        "hook": "patient-view",
        "id": "patient-view-test-hook",
        "title": "Test Hook",
        "description": "This is a test hoook",
        "preFetch": {
            "patientToGreet": "Patient/{{context.patientId}}"
        },
      }
    ]
  }

You can then access the example service at the following URL: ``<base-url>/cds-services/patient-view-test-hook``. This service is a simple CDS Hook that provides a patient view when invoked. E.g.

.. code-block:: HTTP

  POST <base-url>/cds-services/patient-view-test-hook
    Content-Type: application/json
    
    {
        "hook": "patient-view",
        "context": {
            "patientId": "example"
        },
        "prefetch": {
            "patientToGreet": {
                "resourceType": "Patient",
                "id": "example",
                "name": [
                    {
                        "family": "Doe",
                        "given": ["John"]
                    }
                ],
                "gender": "male",
                "birthDate": "1974-12-25"
            }
        }

Building a CDS Hooks service
----------------------------

To build your own CDS Hooks service, you need to create a plugin that implements the CDS Hooks service interface. The plugin should define the hook, id, title, and description of the service, as well as any pre-fetch or post-fetch logic.

This is easiest understood with a code example. This example shows how to create a simple CDS Hooks service that greets the patient by name when the patient view hook is invoked. It is the same as the example service provided by Firely Server.

.. container:: toggle

    .. container:: header

      PatientViewTestHookService.cs
      
    The service itself is responsible for handling the CDS Hooks request. It checks the hook type, retrieves the patient information from the prefetch section, and constructs a response with a greeting message.

    .. code-block:: csharp
    
        using System;
        using System.Diagnostics.CodeAnalysis;
        using System.Linq;
        using System.Threading.Tasks;
        using Hl7.Fhir.ElementModel;
        using Microsoft.AspNetCore.Http;
        using Vonk.Core.Common;
        using Vonk.Core.Context;
        using Vonk.Core.ElementModel;
        
        namespace Vonk.Plugin.CdsHooks.PatientViewTestHook;
        
        [Experimental("CdsHooks")]
        internal class PatientViewTestHookService
        {
            public async Task HandlePatientViewHook(IVonkContext vonkContext)
            {
                var hook = vonkContext.Request.Payload.Resource?.SelectText("hook");
                if (!hook?.Equals("patient-view") ?? false)
                    return;
        
                var cdsHooksResponse = SourceNode.Resource("CDSHooksResponse", "CDSHooksResponse");
                var cardNode = SourceNode.Node("cards");
        
                // Static information
                cardNode.Add(SourceNode.Valued("uuid", Guid.NewGuid().ToString()));
                cardNode.Add(SourceNode.Valued("summary", "Hello World! Firely Server loves FHIR and CDS Hooks!"));
                cardNode.Add(SourceNode.Valued("indicator", "info"));
                cardNode.Add(SourceNode.Node("source",
                    SourceNode.Valued("label", "Firely Server"),
                    SourceNode.Valued("url", vonkContext.ServerBase.ToString())));
        
                // Check information provided prefetch
                var patientPrefetchNode = vonkContext.Request.Payload.Resource?.SelectNodes("prefetch.patientToGreet")
                    .FirstOrDefault();
                
                if (!(patientPrefetchNode is { }))
                {
                    vonkContext.Response.Outcome.AddIssue(VonkIssue.PROCESSING_ERROR,
                        "No patientToGreet provided in prefetch section of CDS Hooks request.");
                    vonkContext.Response.HttpResult = StatusCodes.Status412PreconditionFailed;
                    return;
                }
        
                // Sanity check against provided context
                var contextPatientId = vonkContext.Request.Payload.Resource?.SelectText("context.patientId");
                var prefetchPatientId = patientPrefetchNode.SelectText("id");
                if (prefetchPatientId is null || !prefetchPatientId.Equals(contextPatientId))
                {
                    vonkContext.Response.Outcome.AddIssue(VonkIssue.PROCESSING_ERROR,
                        $"Patient ids in context ({contextPatientId}) and prefetch ({prefetchPatientId}) do not match.");
                    vonkContext.Response.HttpResult = StatusCodes.Status412PreconditionFailed;
                    return;
                }
                
                var family = patientPrefetchNode.SelectText("name.family");
                if (string.IsNullOrEmpty(family))
                    family = "{unknown family name}";
        
                var nameNodes = patientPrefetchNode.SelectNodes("name").ToList();
                var given = string.Empty;
                if (nameNodes.Any())
                {
                    given = nameNodes.Select(g => g.SelectText("given"))
                        .Aggregate((all, next) => $"{all} {next}");
                }
        
                if (string.IsNullOrEmpty(given))
                    given = "{unknown given name}";
        
                var gender = patientPrefetchNode.SelectText("gender");
                if (string.IsNullOrEmpty(gender))
                    gender = "{unknown gender}";
        
                var birthDate = patientPrefetchNode.SelectText("birthDate");
                if (string.IsNullOrEmpty(birthDate))
                    birthDate = "{unknown birthDate}";
        
                cardNode.Add(SourceNode.Valued("detail", $"Hello {given} {family} ({gender}, {birthDate})!"));
                cdsHooksResponse.Add(cardNode);
        
                vonkContext.Response.Payload = cdsHooksResponse.ToIResource(vonkContext.InformationModel);
                vonkContext.Response.HttpResult = StatusCodes.Status200OK;
                await Task.CompletedTask;
            }
        }

.. container:: toggle

    .. container:: header

      PatientViewTestHookContributor.cs
      
    A contributor is used to add the CDS Hooks service to the CDS Hooks Discovery document. This is where you define the hook, id, title, description, and any pre-fetch or post-fetch logic.

    .. code-block:: csharp
    
        using System.Collections.Generic;
        using System.Diagnostics.CodeAnalysis;
        using Hl7.Fhir.Model.CdsHooks;
        
        namespace Vonk.Plugin.CdsHooks.PatientViewTestHook;
        
        public class PatientViewTestHookContributor : ICdsHooksDiscoveryDocumentContributor
        {
            [Experimental("CdsHooks")]
            public void ContributeToDiscoveryDocument(ICdsHooksDiscoveryDocumentBuilder builder)
            {
                builder.UseDocumentEditor(doc => doc.AddService(
                    new Service
                    {
                        Id = "patient-view-test-hook",
                        Title = "Test Hook",
                        Description = "This is a test hook",
                        Prefetch = new Dictionary<string, string>()
                        {
                            { "patientToGreet", "Patient/{{context.patientId}}" }
                        },
                        Hook = "patient-view",
                        UsageRequirements = "none"
                    }
                ));
            } 
        }

.. container:: toggle

    .. container:: header

      PatientViewTestHookConfiguration.cs
      
    Configuration works the same way as for any other Vonk plugin. You register both the service itself and the contributor that adds the service to the CDS Hooks Discovery document.

    .. code-block:: csharp
    
        using System.Diagnostics.CodeAnalysis;
        using Microsoft.AspNetCore.Builder;
        using Microsoft.Extensions.DependencyInjection;
        using Microsoft.Extensions.DependencyInjection.Extensions;
        using Vonk.Core.Common;
        using Vonk.Core.Pluggability;
        
        namespace Vonk.Plugin.CdsHooks.PatientViewTestHook;
        
        [InternalVonkConfiguration(order: 5500, isLicensedAs: VonkConstants.Plugins.Fhir.Operation.CdsHooks)]
        [Experimental("CdsHooks")]
        public static class PatientViewTestHookConfiguration
        {
            public static IServiceCollection ConfigureServices(this IServiceCollection services)
            {
                services.TryAddSingleton<ICdsHooksDiscoveryDocumentContributor, PatientViewTestHookContributor>();
                services.TryAddScoped<PatientViewTestHookService>();
                return services;
            }
        
            public static IApplicationBuilder Configure(IApplicationBuilder builder)
            {
                builder.OnCdsHooksRequest("patient-view-test-hook")
                    .HandleAsyncWith<PatientViewTestHookService>((svc, ctx) => svc.HandlePatientViewHook(ctx));
                return builder;
            }
        }

CDS Hooks in FHIR
-----------------

CDS Hooks structures like the Discovery document, the request and response are not defined in terms of FHIR.
However, to fit them into the FHIR ecosystem, Firely Server uses the FHIR R4 resource types ``CDSHooksRequest`` and ``CDSHooksResponse`` to represent the response of a CDS Hooks service. 
These resource types are delivered with Firely Server through the ``errata.zip`` for FHIR R4, and hence also in the pre-built ``vonkadmin.db`` database.
Note however that:

* Neither of these StructureDefinitions are part of the FHIR specification. They are only available experimentally as logical models in the `FHIR tools package <https://simplifier.net/packages/hl7.fhir.uv.tools.r4>`_.
* Since logical models do not define resource types, Firely has adjusted those to the StructureDefinitions that are packaged with the server.
* ``CDSHooksRequest`` has specific elements for each hook, like ``patientToGreet`` for the example service. If a new hook requires additional elements, these should be added to the ``CDSHooksRequest`` resource type.

.. _cds_hooks_operations:

CDS Hooks operations in Firely Server
-------------------------------------

CDS Hooks services are not FHIR interactions. To fit them into the Firely Server programming API, they are transformed internally to custom operations on a system level.
As such, they must be listed in the ``Operations`` section of the :ref:`appsettings <disable_interactions>`. The naming convention for these operations is ``cds-{service-id}``, where ``{service-id}`` is the id of the CDS Hooks service.
For example, the example service ``patient-view-test-hook`` will be available as a custom operation ``cds-patient-view-test-hook``.
