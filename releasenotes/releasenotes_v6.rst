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

Features
^^^^^^^^

#. Firely Server offers the ``$member-match`` operation to find the identifier of a member of a health plan. See :ref:`member-match` for more information.
#. Starting from this release the ``Vonk.Smart`` and ``Vonk.Plugin.SoFv2`` plugins are no longer supported and have been removed. They are replaced by the ``Vonk.Plugin.Smart`` plugin. For more information see :ref:`feature_accesscontrol_config`. It is necessary to adjust the pipeline options accordingly.
#. It is now possible to disable the create-on-update feature with a new setting in the ``FhirCapabilities`` section of the app settings. For more information see :ref:`restful_crud`.
#. With this release ``Update with no changes (No-Op)`` is enabled by default. For more information about the plugin see :ref:`restful_noop`.
#. The use of other compartments then Patient in SMART on FHIR authorization is not well defined and potentially unsafe. So we redacted the ``Filters`` settings in ``SmartAuthorizationOptions``. You can now only specify a filter on the Patient compartment. For more information see :ref:`feature_accesscontrol_config`. If you configured just a Patient filter in the old format, Firely Server will interpret it in the new format and log a warning that you should update your settings. If you configured a filter on a different compartment, Firely Server will log an error and halt.
#. SMART on FHIR v2 scopes can include search arguments. Upon writing resources (create, update, delete) Firely Server used to only evaluate those for ``patient/`` scopes. Now, they are also evaluated for ``user/`` and ``system/`` scopes.

Configuration
^^^^^^^^^^^^^
.. attention::
    Default behavior of Firely Server has been tweaked by changing conviguration values. 
    Make sure to reflect the desired behaviour by adjusting ``appsettings.instance.json`` or environment variables.

#. Evaluation of :ref:`Subscriptions<feature_subscription>` is now turned off by default. To enable - adjust ``SubscriptionEvaluatorOptions`` accordingly.
#. ``BundleOptions.DefaultTotal`` from now on has a default value of ``none``. For available options see :ref:`bundle_options`.

.. note::
    With the release of Firely Server 6.0, we will officially stop support for Firely Server v4.x. We will continue supporting customers that run Firely Server v5.x.
