.. _vonk_reference_api_ivonkcontext:

IVonkContext
============

:namespace: Vonk.Core.Context

:purpose: IVonkContext is the Vonk-specific counterpart to HttpContext from ASP.NET Core. It contains an IVonkRequest and IVonkResponse object that allow you to get information from the request and set results in the response, both in Firely Server terms.  

Have ``IVonkContext`` injected in the method where you need it. Use a `configuration class <vonk_plugins_configclass>`_ to call this method from the pipeline and have the actual context injected. A more complete template is found at :ref:`vonk_plugins_template`.

.. code-block:: csharp

   public class SomeService
   {
      public async Task DoMyOperation(IVonkContext vonkContext)
      {
         //...
      }
   }

   public static class SomeServiceConfiguration
   {
      public static IApplicationBuilder UseMyOperation(this IApplicationBuilder app)
      {
         return app.UseVonkInteractionAsync<SomeService>((svc, context) => svc.DoMyOperation(context));
      }
   }

If you also need access to the raw ``HttpContext``, you have two options:

#. The ``IVonkContext.HttpContext()`` extension method gives you the original HttpContext. Be aware though:

   * Situations may arise where there is no HttpContext, so be prepared for that.
   * When handling batches or transactions, an IVonkContext is created for each entry in the bundle. But they all refer to the same original HttpContext.
   
#. Create a normal ASP.NET Core Middleware class and access the IVonkContext with the extension method ``Vonk()`` on ``HttpRequest``. A more complete template is found at :ref:`vonk_plugins_directhttp`.

   .. code-block:: csharp

      public class SomeMiddleware
      {
         public SomeMiddleware(RequestDelegate next)
         {
            //...
         }

         public async Task Invoke(HttpContext httpContext)
         {
            var vonkContext = httpContext.Vonk();
            //...
         }
      }

      public static class SomeMiddlewareConfiguration
      {
         public static IApplicationBuilder UseSomeMiddleware(this IApplicationBuilder app)
         {
            return app.UseMiddleware<SomeMiddleware>(); //Just plain ASP.NET Core, nothing Firely Server specific here.
         }
      }

IVonkContext has three major parts, that are explained below. The ``InformationModel`` tells you the FHIR version for which the request was made.

.. code-block:: csharp

   public interface IVonkContext
   {
      IVonkRequest Request {get;}

      IArgumentCollection: Arguments{get;}

      IVonkResponse Response {get;}

      string InformationModel {get;}
   }

And because you frequently need the parts instead of the context itself, there is an extension method on ``IVonkContext``:

.. code-block:: csharp

   public (IVonkRequest request, IArgumentCollection args, IVonkResponse respons) Parts(this IVonkContext vonkContext)

.. _vonk_reference_api_ivonkrequest:

IVonkRequest
------------

:namespace: Vonk.Core.Context

:purpose: Get information about the request made, in Firely Server / FHIR terms.

You can access the current ``IVonkRequest`` through the `IVonkContext`_. Its properties are:

.. code-block:: csharp

   public interface IVonkRequest
   {
      string Path { get; }
      string Method { get; }
      string CustomOperation { get; }
      VonkInteraction Interaction { get; }
      RequestPayload Payload { get; set; }
   }

``Path`` and ``Method`` relate directly to the equivalents on HttpContext. ``Interaction`` tells you which of the FHIR RESTful interactions was called. ``CustomOperation`` is only filled if one of the custom operations was invoked, like e.g. ``$validate``. All of these can be filtered by the :ref:`vonk_reference_api_interactionhandlerattribute`, so you typically don't need to inspect them manually.

Payload indirectly contains the resource that was sent in the body of the request. You are advised to only use the extension methods to access it:

.. code-block:: csharp

   public static bool TryGetPayload(this IVonkRequest request, out IResource resource)

TryGetPayload is useful if your code wants to act on the payload *if it is present*, but does not care if it is not.

.. code-block:: csharp

   public void ThisMethodActsOnThePayloadIfPresent(IVonkContext vonkContext)
   {
      var (request, args, response) = vonkContext.Parts();
      if (request.TryGetPayload(response, out var resource))
      {
         // do something with the resource.
      }

   }

.. code-block:: csharp

   public static bool GetRequiredPayload(this IVonkRequest request, IVonkResponse response, out IResource resource)

GetRequiredPayload is useful if your code expects the payload to be present. It will set the appropriate response code and OperationOutcome on the provided response if it is not present or could not be parsed. Then you can choose to end the pipeline and thus return the error to the user.

.. code-block:: csharp

   public void ThisMethodNeedsAPayload(IVonkContext vonkContext)
   {
      var (request, args, response) = vonkContext.Parts();
      if (!request.GetRequiredPayload(response, out var resource))
      {
         return; //If you return with an error code in the response, Firely Server will end the pipeline
      }
      // do something with the resource.
   }

If you want to **change** the payload, assign a whole new one. Generally you would want to change something to the old payload. But IResource is immutable, so changes to it yield a new instance. That leads to this pattern

.. code-block:: csharp

   if (request.TryGetPayload(response, out var resource)
   {
      //Explicit typing of variables for clarity, normally you would use 'var'.
      ISourceNode updatedNode = resource.Add(SourceNode.Valued("someElement", "someValue");
      IResource updatedResource = updatedNode.ToIResource();
      request.Payload = updatedResource.ToPayload();
   }

.. _vonk_reference_api_iargument:

IArgumentCollection, IArgument
------------------------------

:namespace: Vonk.Core.Context

:purpose: Access arguments provided in the request.

The ``IVonkContext.Arguments`` property contains all the arguments from the request, from the various places:

#. The path segments: /Patient/123/_history/v1 will translate to three arguments, _type, _id and _version.
#. The query parameters: ?name=Fred&active=true will translate to two arguments, name and active.
#. The headers: 
   
   #.   If-None-Exists = identifier=abc&active=true will translate to two arguments, identifier and active.   
   #.   If-Modified-Since, If-None-Match, If-Match: will each translate to one argument
        
An individual argument will tell you its name (``ArgumentName``), raw value (``ArgumentValue``) and where it came from (``Source``).

Handling arguments
^^^^^^^^^^^^^^^^^^

An argument by default has a ``Status`` of ``Unhandled``.

If an argument is of interest to the operation you implement in your plugin, you can handle the argument. It is important to mark arguments handled if:

* you handled them
* or the handling is not relevant anymore because of some error you encountered
  
In both cases you simply set the ``Status`` to ``Handled``. 

If an argument is incorrect, you can set its status to ``Error`` and set the ``Issue`` to report to the client what the problem was. These issues will be accumulated in the response by Firely Server automatically.

Any argument that is not handled will automatically be reported as such in an OperationOutcome.

Useful extension methods:

.. code-block:: csharp

   IArgument.Handled()
   IArgument.Warning(string message, Issue issue)
   IArgument.Error(string message, Issue issue)

Firely Server has a lot of issues predefined in ``Vonk.Core.Support.VonkIssues``.

.. _vonk_reference_api_ivonkresponse:

IVonkResponse
-------------

:namespace: Vonk.Core.Context

:purpose: Inspect response values set by other middleware, or set it yourself.

.. code-block:: csharp

   public interface IVonkResponse
   {
      Dictionary<VonkResultHeader, string> Headers { get; }
      int HttpResult { get; set; }
      OperationOutcome Outcome { get; }
      IResource Payload { get; set; }
   }

If your operation provides a response, you should:

#. Set the response code ``HttpResult``.
#. Provide a resource in the ``Payload``, if applicable.
#. Add an issue if something is wrong.

If you just listen in on the pipeline, you can check the values of the response. Besides that, the :ref:`vonk_reference_api_interactionhandlerattribute` allows you to filter on the ``HttpStatus`` of the response.

.. _vonk_reference_api_iformatter:

IFormatter
----------

:namespace: Vonk.Core.Context.Format

:purpose: Serialize response resource in requested format to the body of the HttpContext.Response. Although this interface is public, you should never need it yourself, since the :ref:`VonkToHttp plugin <vonk_plugins_vonktohttp>` takes care of this for you.
