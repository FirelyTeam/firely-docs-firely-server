.. _vonk_releasenotes_history_v6:

Current Firely Server release notes (v6.x)
==========================================

.. note::
    For information on how to upgrade, please have a look at our documentation on :ref:`upgrade`. You can download the binaries of the latest version from `this site <https://downloads.fire.ly/firely-server/versions/>`_, or pull the latest docker image::
        
        docker pull firely/server:latest

.. _vonk_releasenotes_6_0_0:

=======


Release 6.0.0, [Month] [Date], 2024
---------------------------------------

Firely is proud to announce a new major version of Firely Server. This release represents a significant step forward in our commitment to providing a reliable, compliant and easy to use FHIR server.
With this new version, we've focused on delivering:

- support for Sharding with MongoDB
- zero-downtime migrations with MongoDB (see :ref:`zero_downtime_migration`)
- detailed insights into Firely Server deployments based on OpenTelemetry metrics and traces
- improved integration into existing infrastructures with Kafka support for Firely Server PubSub
- out-of-the-box compliance with more HL7 DaVinci Implementation Guides, e.g. by providing support for the HRex $member-match operation
- flexibility for deployments requiring multi-tenancy

Please study the release notes carefully as they contain breaking changes to the behaviour of Firely Server, as well as the configuration of the server. 
Our support team is happy to provide assistance in the upgrade and can be reached at `server@fire.ly <mailto:server@fire.ly>`_ or through the support desk.

Security
^^^^^^^^

#. AccessPolicy resources cannot be accessed by a resource wildcard scope. E.g. ``system/*.*`` should be replaced with  - ``system/AccessPolicy.*`` to be able to access AccessPolicy resources.

Features
^^^^^^^^

#. Firely Server offers the ``$member-match`` operation to find the identifier of a member of a health plan. See :ref:`member-match` for more information.
#. Starting from this release the ``Vonk.Smart`` and ``Vonk.Plugin.SoFv2`` plugins are no longer supported and have been removed. They are replaced by the ``Vonk.Plugin.Smart`` plugin. For more information see :ref:`feature_accesscontrol_config`. It is necessary to adjust the pipeline options accordingly.
#. It is now possible to disable the create-on-update feature with a new setting in the ``FhirCapabilities`` section of the app settings. For more information see :ref:`restful_crud`.
#. With this release ``Update with no changes (No-Op)`` is enabled by default. For more information about the plugin see :ref:`restful_noop`.
#. The use of other compartments then Patient in SMART on FHIR authorization is not well defined and potentially unsafe. So we redacted the ``Filters`` settings in ``SmartAuthorizationOptions``. You can now only specify a filter on the Patient compartment. For more information see :ref:`feature_accesscontrol_config`. If you configured just a Patient filter in the old format, Firely Server will interpret it in the new format and log a warning that you should update your settings. If you configured a filter on a different compartment, Firely Server will log an error and halt.
#. Added support for reading messages from a Kafka topic when using Firely Server PubSub

Programming API changes
^^^^^^^^^^^^^^^^^^^^^^^

#. Extended the base class ``RelationalQueryFactory`` with support for the ``ResourceTypesNotValue`` (see :ref:`parameter_types`) and methods to express a predicate that is ``AlwaysFalse()`` or ``AlwaysTrue()``.
#. The ``VonkConfigurationAttribute`` no longer supports the deprecated ``isLicensedAs`` property.
#. The deprecated ``VonkConstants.MediaType`` values ``XmlR3``, ``JsonR3`` and ``TurtleR3`` have been removed. Use ``FhirXml``, ``FhirJson`` and ``FhirTurtle`` instead.
#. The deprecated method ``Check.HasValue()`` has been removed. Use ``Check.NotNull()`` instead.

Fixes
^^^^^

#. SMART on FHIR v2 scopes can include search arguments. Upon writing resources (create, update, delete) Firely Server used to only evaluate those for ``patient/`` scopes. Now, they are also evaluated for ``user/`` and ``system/`` scopes. Please check the note above whether your deployment may be affected.

Configuration
^^^^^^^^^^^^^
.. attention::
    Default behavior of Firely Server has been tweaked by changing conviguration values. 
    Make sure to reflect the desired behaviour by adjusting ``appsettings.instance.json`` or environment variables.

#. Evaluation of :ref:`Subscriptions<feature_subscription>` is now turned off by default. To enable - adjust ``SubscriptionEvaluatorOptions`` accordingly.
#. ``BundleOptions.DefaultTotal`` from now on has a default value of ``none``. For available options see :ref:`bundle_options`.

.. note::
    With the release of Firely Server 6.0, we will officially stop support for Firely Server v4.x. We will continue supporting customers that run Firely Server v5.x.
