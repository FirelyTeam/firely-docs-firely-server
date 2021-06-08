.. _feature_subscription:

Subscriptions
=============

Subscriptions can be managed in the :ref:`administration_api`, on the ``/administration/Subscription`` endpoint. If you post a Subscription
to the regular FHIR endpoint, it will be stored but not evaluated. Subscriptions posted to the
``/administration`` endpoint will be processed and evaluated for each POST/PUT to the server.

Firely Server currently only supports STU3/R4-style Subscriptions with a Channel of type rest-hook.

If you are :ref:`not permitted <configure_administration_access>` to access the /Subscription endpoint, Firely Server will return statuscode 403.

See `Subscriptions in the specification <http://www.hl7.org/fhir/stu3/subscription.html>`_ for more background on Subscriptions.

FHIR versions
-------------

You POST a Subscription with a fhirVersion parameter (see :ref:`feature_multiversion`) or to a version specific endpoint. It will then respond to changes on resources *in that FHIR version*.
So if you need a Subscription on both STU3 and R4 resources, POST that Subscription for both FHIR versions.

.. _subscription_configure:

Configuration
-------------
Firely Server evaluates the active Subscriptions periodically, and in batches (x at a time, until all the active Subscriptions have been evaluated).
You can control the period and the batchsize. If an evaluation of a Subscription fails, Firely Server will retry the evaluation periodically for a maximum amount of tries. You can control the retry period and the maximum number of retries.

::

    "SubscriptionEvaluatorOptions": {
        "Enabled" : true
        "RepeatPeriod": 20000,
        "SubscriptionBatchSize" : 1,
        "RetryPeriod": 60000,
        "MaximumRetries":  3,
        "SendRestHookAsCreate": false
    },

* ``Enabled`` allows you to quickly enable or disable the evaluation of Subscriptions. Default value is 'false', which implies that Subscription evaluation is also off if this section is left out of the settings.
* ``RepeatPeriod`` is expressed in milliseconds. In the example above the period is set to 20 seconds, meaning that after a change a subscriber will be notified in at most 20 seconds.
* ``SubscriptionBatchSize`` is expressed in number of Subscriptions that is retrieved and evaluated at once. Default is 1, but you can set it higher if you have a lot of Subscriptions.
* ``RetryPeriod`` is expressed in milliseconds. In the example above the period is set to 60 seconds, meaning that Firely Server will retry to send the resources after a minimum of 60 seconds. Retry is included in the normal evaluation process, so the RetryPeriod cannot be smaller than RepeatPeriod.
* ``MaximumRetries`` is the maximum amount of times Firely Server will retry to send the resources.
* ``SendRestHookAsCreate``: in versions < 3.9.3, Vonk sent RestHook notifications as a create operation using a POST. That was not compliant with the specification that requires an update operation using a PUT. The default value of ``false`` provides compliant behaviour. Only set it to ``true`` if you need Firely Servder to keep sending create operations as it did previously. 
