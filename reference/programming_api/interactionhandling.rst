.. _vonk_reference_api_interactionhandling:

Interaction Handling
====================

In the configuration of a plugin you specify on which interaction(s) the plugin should act. That can be done with an attribute on the main method of the service in the plugin, or with a fluent interface on IApplicationBuilder.

.. _vonk_reference_api_interactionhandlerattribute:

InteractionHandlerAttribute
---------------------------

:namespace: Vonk.Core.Pluggability

:purpose: Add an ``[InteractionHandler]`` attribute to a method to specify when the method has to be called. You specify this by providing values that the IVonkContext should match.

Without any arguments, the method will be called for every possible interaction.

.. code-block:: csharp

   [InteractionHandler()]
   public async Task DoMyOperation(IVonkContext vonkContext)

You can specify different filters, and combine them at will:

* Specific interaction(s): ``[InteractionHandler(Interaction = VonkInteraction.type_create | VonkInteraction.instance_update)]``
* Specific FHIR version(s) of the request: ``[InteractionHandler(InformationModel = VonkConstants.Model.FhirR4)]``
* Specific resource type(s): ``[InteractionHandler(AcceptedTypes = new["Patient", "Observation"])]``
* Specific custom operation: ``[InteractionHandler(Interaction = VonkInteraction.all_custom, CustomOperation = "myCustomOperation")]``. Note that the ``$`` that is used on the url is not included in the name of the custom operation here.
* Specific http method: ``[InteractionHandler(Method = "POST")]``
* Specific statuscode(s) on the response: ``[InteractionHandler(StatusCode = new[]{200, 201})]``

Now to configure your service to be a processor in the Firely Server pipeline, you use ``UseVonkInteraction[Async]()``:

.. code-block:: csharp

   public static class MyOperationConfiguration
   {
      public static IApplicationBuilder UseMyOperation(this IApplicationBuilder app)
      {
         return app.UseVonkInteractionAsync<MyService>((svc, ctx) => svc.DoMyOperation(ctx));
      }
   }

.. _vonk_reference_api_interactionhandlerfluent:

InteractionHandler fluent interface
-----------------------------------

Because ``InteractionHandler`` is an attribute, you can only use constant values. If that is not what you want, you can use the fluent interface in the `configuration class <vonk_plugins_configclass>`_ instead. The code below shows the same filters as above, although you typically would not use all of them together (e.g. the ``PUT`` excludes ``type_create``).

.. code-block:: csharp

   public static class MyOperationConfiguration
   {
      public static IApplicationBuilder UseMyOperation(this IApplicationBuilder app)
      {
         return app
            .OnInteraction(VonkInteraction.type_create | VonkInteraction.instance_update)
            .AndInformationModel(VonkConstants.Model.FhirR4)
            .AndResourceTypes(new[] {"Patient", "Observation"})
            .AndStatusCodes(new[] {200, 201})
            .AndMethod("PUT")
            .HandleAsyncWith<MyService>((svc, ctx) => svc.DoMyOperation(ctx));
      }
   }

Other ``Handle...`` methods allow you to define a pre-handler (that checks or alters the request before the actual operation) or a post-handler (that checks or alters the response after the actual operation), either synchronously or asynchronously.

If you have a very specific filter that is not covered by these methods, you can specify it directly with a function on the ``IVonkContext`` that returns a boolean whether or not to call your operation.

.. code-block:: csharp

   app
      .On(ctx => MyVerySpecificFilter(ctx))
      .Handle...

.. attention::

   The filter you specify is called for **every** request. So make sure you don't do any heavy calculations or I/O.

.. _vonk_appbuilder_extensions:

IApplicationBuilder extension methods
-------------------------------------

.. function:: UseVonkInteraction<TService>(this IApplicationBuilder app, Expression<Action<<TService, IVonkContext>> handler, OperationType operationType = OperationType.Handler) -> IApplicationBuilder

   Handle the request with the ``handler`` method when the request matches the ``InteractionHandler`` attribute on the ``handler`` method. The ``OperationType`` may also specify ``PreHandler`` or ``PostHandler``. If you need to do anything lengthy (I/O, computation), use the Async variant of this method.

.. function:: UseVonkInteractionAsync<TService>(this IApplicationBuilder app, Expression<Func<TService, IVonkContext, T.Task>> handler, OperationType operationType = OperationType.Handler) -> IApplicationBuilder

   Handle the request with the asynchronous ``handler`` method when the request matches the ``InteractionHandler`` attribute on the ``handler`` method. The ``OperationType`` may also specify ``PreHandler`` or ``PostHandler``.

.. function:: OnInteraction(this IApplicationBuilder app, VonkInteraction interaction) -> VonkAppBuilder

   Used for fluent configuration of middleware. This is one of two methods to enter the ``VonkAppBuilder``, see :ref:`vonk_vonkappbuilder`. It requires you to choose an interaction to act on. If you need your services to act on every interaction, choose ``VonkInteraction.all``.

.. function:: OnCustomInteraction(this IApplicationBuilder app, VonkInteraction interaction, string custom) -> VonkAppBuilder

   Used for fluent configuration of middleware. This is one of two methods to enter the ``VonkAppBuilder``, see :ref:`vonk_vonkappbuilder`. It requires you to choose an interaction to act on. This should be one of the ``VonkInteraction.all_custom`` interactions. ``custom`` is the name of the custom interaction to act on, without the preceding '$'.

.. _vonk_vonkappbuilder:

VonkAppBuilder extension methods
--------------------------------

``VonkAppBuilder`` is used to fluently configure your middleware. It has methods to filter the requests that your middleware should respond to. Then it has a couple of ``*Handle...`` methods to transform your service into middleware for the pipeline, and return to the IApplicationBuilder interface.

.. function:: AndInteraction(this VonkAppBuilder app, VonkInteraction interaction) -> VonkAppBuilder

   Specify an interaction to act on.

.. function:: AndResourceTypes(this VonkAppBuilder app, params string[] resourceTypes) -> VonkAppBuilder

   Specify the resourcetypes to act on.

.. function:: AndStatusCodes(this VonkAppBuilder app, params int[] statusCodes) -> VonkAppBuilder

   Specify the statuscode(s) of the response to act on. This is mainly useful for posthandlers.

.. function:: AndMethod(this VonkAppBuilder app, string method) -> VonkAppBuilder

   Specify the http method (GET, PUT, etc) to act on.

.. function:: AndInformationModel(this VonkAppBuilder app, string model) -> VonkAppBuilder

   If your service can only act on one FHIR version, specify it with this method. Common values for ``model`` are ``VonkConstants.Model.FhirR3`` and ``VonkConstants.Model.FhirR4``.

.. function:: PreHandleAsyncWith<TService>(this VonkAppBuilder app, Expression<Func<TService, IVonkContext, T.Task>> preHandler) -> IApplicationBuilder

   Mark the ``preHandler`` method as a prehandler, so it will act on the IVonkContext and send it further down the pipeline.

.. function:: PreHandleWith<TService>(this VonkAppBuilder app, Expression<Action<TService, IVonkContext>> preHandler) -> IApplicationBuilder

   Synchronous version of ``PreHandleAsyncWith`` for synchronous ``preHandler`` methods.

.. function:: HandleAsyncWith<TService>(this VonkAppBuilder app, Expression<Func<TService, IVonkContext, T.Task>> handler) -> IApplicationBuilder

   Mark the ``handler`` method as a hanlder, so it will act on the IVonkContext, provide a response and end the pipeline for the request.

.. function:: HandleWith<TService>(this VonkAppBuilder app, Expression<Action<TService, IVonkContext>> handler)

   Synchronous version of ``HandleAsyncWith`` for synchronous ``handler`` methods.

.. function:: PostHandleAsyncWith<TService>(this VonkAppBuilder app, Expression<Func<TService, IVonkContext, T.Task>> postHandler) -> IApplicationBuilder

   Mark the ``postHandler`` method as a posthandler, so it will pass on the IVonkContext to the rest of the pipeline, and on the way back through the pipeline inspect or modify the response. Make sure that the ``VonkConfiguration`` order you have for this is lower than whatever action you need to post-handle.

.. function:: PostHandleWith<TService>(this VonkAppBuilder app, Expression<Action<TService, IVonkContext>> postHandler) -> IApplicationBuilder

   Synchronous version of ``PostHandleAsyncWith`` for synchronous ``postHandler`` methods.
