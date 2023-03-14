.. _vonk_overview:

Introduction to Firely Server
=============================

Exploring Firely Server
-----------------------

Firely Server is a turn-key FHIR Server that you can :ref:`set up within minutes<vonk_getting_started>`. 
You can try out Firely server for free, either using the sandbox environment at https://server.fire.ly, or by downloading an evaluation or community license from `Simplifier.net <https://simplifier.net/firely-server>`_.
The sandbox environment is mostly intended for testing and educational purposes, you can explore the sandbox using our Swagger-based `web UI <_static/swagger>`_. The evaluation license allows you to explore all the functionality of Firely Server during a week. After this period, it is possible to renew your license by contacting us at server@fire.ly.

You also have the option to use Firely Server for free with a community license, however you will have fewer options than with the evaluation license, and can only use SQLite as a repository database.

If you are interested in Firely Server for commercial use within your organization, we provide professional licensing in different tiers: Startup or Scale.
For more information and pricing you can also visit the `product site <https://fire.ly/products/firely-server/>`_.


Adjust Firely Server to your needs
----------------------------------

Once you familiarized yourself with Firely Server, you can start exploring the configuration options. The first step in Firely Server configuration is your database choice. 
Firely Server makes use of a repository database to save resources in, as well as a smaller administration database. You have several options for these two :ref:`databases <configure_repository>`: :ref:`SQLite <configure_sqlite>` is configured by default, but for serious use you'd want to configure :ref:`MongoDB <configure_mongodb>` or :ref:`SQL Server <configure_sql>`.

Next, you might want to think about the method of :ref:`deploying Firely Server <deployment>`. Again, you have several options here, either running :ref:`Firely Server on Docker<use_docker>`, deploying Firely Server with :ref:`kubernetes<deploy_helm>`, hosting Firely Server on :ref:`Azure<azure_webapp>` or using a :ref:`reverse proxy<deploy_reverseProxy>`.

With the database configuration and the deployment in place, it is time to tweak your configuration. Make sure Firely Server validates all incoming resources by configuring the :ref:`validation setting<feature_prevalidation>`.
Configure :ref:`endpoints <feature_multiversion_endpoints>` for FHIR versions that you want to support, either FHIR STU3, FHIR R4, or FHIR R5. Next, configure the :ref:`processing pipeline<settings_pipeline>` to take along the :ref:`plugins<vonk_plugins_total>` that you would like to use. You also have the option to include :ref:`custom plugins<vonk_plugins>` of your own design.


You can also further configure the :ref:`administration database <administration_api>` that allows you to configure the so-called :ref:`conformance resources <conformance>` that drive parsing, serialization, validation and terminology. The administration database is pre-filled with conformance resources such as the StructureDefinitions, Searchparameters, CodeSystems and ValueSets that come with the FHIR Specification. Beyond that you can use the administration database to make Firely Server aware of:

* Custom profiles, e.g. national or institutional restrictions on the standard FHIR resources.
* :ref:`Custom resources <feature_customresources>`: you can even define resources beyond those in FHIR and they are treated as if they were standard FHIR resources.
* CodeSystem and ValueSet resources for :ref:`terminology <feature_terminology>`.
* :ref:`Custom Searchparameters <feature_customsp>`: have Firely Server index and search resources on properties that are not searchable with the searchparameters from the FHIR Specification itself.


Learning more
-------------

If you would like to get more familiar with Firely Server and the options it offers, Firely offers `courses <https://fire.ly/training/>`_ on Firely Server as well as the SDK on which it is based. These courses are tailored to the needs of you and your team. You can pick the timeslot for this training that fits your schedule. In addition to the Firely Server course there is also a wide range of other courses available to get acquainted or more experienced with FHIR and the FHIR tooling provided by Firely.
Additional information can be found `on Firely's resource page <https://fire.ly/resources/>`_. Also, don't forget to take a look at `our interesting blogposts <https://fire.ly/blog/>`_.