.. _vonk_plugins_order:

The order of plugins
====================

Firely Server is organized as a pipeline of components - called Middleware. Every request travels through all the components until one of the components provides the response to the request. After that, the response travels back through all the components, in reverse order. Components that come *after* the responding component in the pipeline are not visited at all.

So let's say you issue a FHIR read interaction, ``GET <base-url>/Patient/example``. The component implementing this interaction sits in the pipeline after search but before create. So the request will visit the search middleware (that will ignore it and just pass it on) but will never visit the create middleware.

So many components implement an interaction and provide a response to that interaction. In Firely Server those are called Handlers. Some components may not provide responses directly, but read or alter the request on the way in. Such a component is called a PreHandler. Reversely, a component may read or alter the response on the way back. Such a component is called a PostHandler.

A plugin can configure its own component in this pipeline but as you may understand by now it makes a difference *where* in the pipeline you put that component. Especially if it is a Pre- or PostHandler. To control the position in the pipeline, Firely Server uses the concept of 'Order'.

The :ref:`VonkConfiguration attribute <vonk_reference_api_pipeline_configuration>` lets you define an ``Order`` for your component. This page explains how to choose a suitable number for that order.

.. _vonk_plugins_order_inspect:

Inspect order numbers in use
----------------------------

When you start Firely Server, the log lists all the loaded plugins, with their order. You can see an example :ref:`here <vonk_plugins_log_pipeline>`. Also the list of :ref:`vonk_available_plugins` includes the order number chosen for each of those plugins.

.. _vonk_plugins_order_minimum:

Minimum order
-------------

Registering services
^^^^^^^^^^^^^^^^^^^^

The order is mainly relevant for middleware that you register in the pipeline, in the ``Configure(IApplicationBuilder app)`` method. Some plugins, like e.g. a Facade implementation, only need to register services for the dependency injection framework, in the ``ConfigureServices(IServiceCollection services)`` method.
If that is the case, the order is only relevant if you need to *override* a registration done by Firely Server. There are two ways:

1. Choose an order before Firely Server's default registration. Firely Server in general uses ``TryAddSingleton`` or ``TryAddScoped`` to register an implementation of an interface. This means that if an implementation is already registered, the TryAdd... will not register a second implementation.

   As an example: if you want to override the registration of ``IReadAuthorizer``: that is registered from the :ref:`RepositorySearchSupport <vonk_plugins_search>` plugin, with order 140. So you would choose an order lower than 140.

2. Choose a high order (e.g. > 10000) and make sure your registration overwrites any existing registration.

   ``services.AddOrReplace<IReadAuthorizer, MyReadAuthorizer>(ServiceLifetime.Scoped);``

The latter method is the least error prone and therefore recommended. 

If you only need to register interfaces and/or classes defined by your plugin, the order is not relevant, so pick any number. All service registrations are done before the pipeline itself is configured.

Registering middleware
^^^^^^^^^^^^^^^^^^^^^^

For middleware it is more important where exactly it ends up in the pipeline. This depends mostly on what type of handler it is, see below at :ref:`vonk_plugins_order_prepost`. 

No matter what handler you have, it probably wants to act on the :ref:`IVonkContext <vonk_reference_api_ivonkcontext>`. Then it is important to be in the pipeline *after* the :ref:`HttpToVonk <vonk_plugins_httptovonk>` plugin (order: 1110), since this plugin translates information from the ``HttpContext`` to an ``IVonkContext`` and adds the latter as a feature to the ``HttpContext.Features`` collection. 

Also, you probably want to set your response on the ``IVonkContext.Response`` and not directly on the ``HttpContext.Response``. Then, you will need the :ref:`VonkToHttp <vonk_plugins_vonktohttp>` plugin (order: 1120) to translate the ``IVonkContext`` back to the ``HttpContext``. 

So in general, the minimum order you need for your plugin will be higher than 1120. 

If you want your middleware to act on all the entries in a Batch or Transaction, you need to choose an order higher than that of the :ref:`Transaction <vonk_plugins_transaction>` plugin, which is 3120.

.. _vonk_plugins_order_collisions:

Order collisions
----------------

If two plugins have the same order number, it is not defined in what order the plugins will be put in the pipeline. As long as those plugins act on disjoint sets of requests that may not be a problem. But it is recommended to avoid this by checking the orders already in use. 

.. _vonk_plugins_order_prepost:

Handlers and pre- and posthandlers
----------------------------------

In Firely Server you can define different types of middleware:

* Handler - acts on requests of a certain type, provides the response to it and ends the pipeline.
* Prehandler - acts on requests of certain type(s), may modify the request and sends the request further down the pipeline.
* Posthandler - lets the request pass by to be handled further down the pipeline. When the response passes on the way back, it acts on requests or responses of certain type(s), and may modify the response.

This is explained in the `session on Plugins <https://www.youtube.com/watch?v=odYaOM19XXc>`_ from `DevDays 2018 <https://www.devdays.com/events/devdays-europe-2018/>`_.

What type of middleware you want your service to be is defined by your use of one of the ``*Handle...`` methods from the :ref:`vonk_vonkappbuilder` or the :ref:`vonk_appbuilder_extensions`. 

Prehandler
^^^^^^^^^^

A Prehandler needs to act *before* the actual handler will provide a response. So the order of it must be lower than any Handler that may handle the requests that this Prehandler is interested in.

So if you want a Prehandler to intercept all create interactions, you should choose an order lower than that of the :ref:`Create <vonk_plugins_create>` plugin, which is 4420. 

An example of this is the :ref:`Prevalidation <vonk_plugins_prevalidation>` plugin. It needs to validate all resources that get handled by the Create, Update, Conditional Create and Conditional Update plugins. Of these, Create has the lowest order: 4420. So it must be below 4420. But it also needs to act on each resource in a :ref:`Batch <vonk_plugins_batch>` or :ref:`Transaction <vonk_plugins_transaction>`, so it must be higher than these two, which means higher than 3120. So this is why we have chosen 4320 as order for Prevalidation.

Posthandler
^^^^^^^^^^^

A Posthandler needs to act *after* the actual handler provided a response. But due to the nature of the processing pipeline that means it must have an order *lower* than that of the handler(s) it wants to post-process. The idea is that the posthandler sits in the pipeline and lets the request pass through. Then one of the handlers provides the response and sends it back through the pipeline. It will pass through the posthandler again (now 'backwards'), and then the posthandler will do its processing.

So if you want a Posthandler to process the responses of all create interactions (e.g. for logging purposes), you should choose an order lower than that of the :ref:`Create <vonk_plugins_create>` plugin.

An example for this is the :ref:`Include <vonk_plugins_include>` plugin. This must act on the response of the :ref:`Search <vonk_plugins_search>` plugin. So the Include has order 4210, right before Search which has 4220.
