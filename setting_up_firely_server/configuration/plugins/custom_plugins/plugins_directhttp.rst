.. _vonk_plugins_directhttp:

Returning non-FHIR content from a plugin
========================================

Some plugins may need to return content that is not a FHIR Resource. You currently cannot do that through the ``IVonkResponse``. But there is another way: write directly to the HttpContext.Response. 
The plugin with order 1110 makes the ``IVonkContext`` accessible. That means if you pick an order higher than 1110 you can read the ``IVonkContext.Request`` and ``.Arguments``, and write directly to the ``HttpContext.Response``. 

If you write the response yourself, you also need to set the StatusCode and Content-Type on the HttpContext.Response.

.. note::

    If you write to the HttpContext.Response directly, the payload of the IVonkContext.Response is ignored.

The steps to take are:

#. Configure your plugin with an order higher than 1110
#. Write the ``HttpContext.Response.Body`` directly
#. Set other properties of the ``HttpContext.Response`` (like ``StatusCode``) yourself.   
   
An example of such a plugin would look like this. Note that this is now regular ASP.NET Core Middleware, not a service like in :ref:`vonk_plugins_template`.

.. code-block:: csharp

   using Microsoft.AspNetCore.Builder;
   using Microsoft.AspNetCore.Http;
   using Microsoft.Extensions.DependencyInjection;
   using System.Text;
   using System.Threading.Tasks;
   using Vonk.Core.Context;
   using Vonk.Core.Context.Features;
   using Vonk.Core.Pluggability;

   namespace com.mycompany.vonk.myplugin
   {
       public class MyPluginMiddleware
       {
           private readonly RequestDelegate _next;

           public MyPluginMiddleware(RequestDelegate next)
           {
               _next = next;
           }

           public async Task Invoke(HttpContext httpContext)
           {
               var vonkContext = httpContext.Vonk();
               var (request, args, _) = vonkContext.Parts();
               if (VonkInteraction.system_custom.HasFlag(request.Interaction))
               {
                   //write something directly to the HttpContext.Response. Now you also have to set the Content-Type header and the Content-Length yourself.
                   var message = "This is a response that is not a FHIR resource";
                   string contentLength = Encoding.UTF8.GetByteCount(message).ToString();

                   httpContext.Response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                   httpContext.Response.Headers.Add("Content-Length", contentLength);
                   httpContext.Response.StatusCode = 200;
                   await httpContext.Response.WriteAsync("This is a response that is not a FHIR resource");
               }
               else
               {
                   await _next(httpContext);
               }
           }
       }

       [VonkConfiguration(order: 1115)] //note the order: higher than 1110
       public static class MyPluginConfiguration
       {
           public static IServiceCollection AddMyPluginServices(IServiceCollection services)
           {
               //No services to register in this example, but if you create services to do the actual work - register them here.
               return services;
           }

           public static IApplicationBuilder UseMyPlugin(IApplicationBuilder app)
           {
               app.UseMiddleware<MyPluginMiddleware>(); //You cannot use the extension methods that allow you to filter the requests.
               return app;
           }
       }
   }

.. _vonk_plugins_customauthorization:

Custom authorization plugin
---------------------------

This feature can also be used to implement custom authorization. You can find a template for that in this `gist <http://bit.ly/VonkAuthorizationMiddleware>`_.

If you just return a statuscode, you could use the regular IVonkContext.Response. If you want to return e.g. a custom json object, you should use the method described above. Do not forget to set the Content-Type (to ``application/json`` for a custom json object).

For more information about access control in plugins and facades, see :ref:`accesscontrol_api`.
