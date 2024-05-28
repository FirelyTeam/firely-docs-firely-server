.. _vonk_releasenotes_history_v6:

Current Firely Server release notes (v6.x)
==========================================

.. note::
    For information on how to upgrade, please have a look at our documentation on :ref:`upgrade`. You can download the binaries of the latest version from `this site <https://downloads.fire.ly/firely-server/versions/>`_, or pull the latest docker image::
        
        docker pull firely/server:latest

.. _vonk_releasenotes_6_0_0:

Release 6.0.0, [Month] [Date], 2024
---------------------------------------
Configuration
^^^^^^^^^^^^^
.. attention::
    Default behavior of Firely Server has been tweaked by changing conviguration values. 
    Make sure to reflect the desired behaviour by adjusting ``appsettings.instance.json`` or environment variables.

#. Evaluation of :ref:`Subscriptions<feature_subscription>` is now turned off by default. To enable - adjust ``SubscriptionEvaluatorOptions`` accordingly.

.. note::
    With the release of Firely Server 6.0, we will officially stop support for Firely Server v4.x. We will continue supporting customers that run Firely Server v5.x.
