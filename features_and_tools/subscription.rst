.. _feature_subscription:

Subscriptions
=============

Subscriptions can be managed in the :ref:`administration_api`, on the ``/administration/Subscription`` endpoint. If you post a Subscription
to the regular FHIR endpoint, it will be stored but not evaluated. Subscriptions posted to the
``/administration`` endpoint will be processed and evaluated for each POST/PUT to the server.
If the a POST/PUT matches the criteria specified in the subscription. A POST or PUT (default) call is sent to the specifies channel endpoint.

Firely Server currently only supports STU3/R4-style Subscriptions.

If you are :ref:`not permitted <configure_administration_access>` to access the /Subscription endpoint, Firely Server will return statuscode 403.

See `Subscriptions in the specification <http://www.hl7.org/fhir/stu3/subscription.html>`_ for more background on Subscriptions.

FHIR versions
-------------

You POST a Subscription with a fhirVersion parameter (see :ref:`feature_multiversion`) or to a version specific endpoint. It will then respond to changes on resources *in that FHIR version*.
So if you need a Subscription on both STU3 and R4 resources, POST that Subscription for both FHIR versions.

Channels
--------

According to the FHIR specification, a channel defines how a FHIR server notifies other systems when resources get created or updated. The specification describes several channel types. Currently, Firely Server supports only *rest-hook* channel type.

Below is an example of a Subscription resource that uses this channel type.

.. code-block:: json

  {
    "resourceType": "Subscription",
    "id": "example",
    "status": "requested",
    "contact": [
      {
        "system": "phone",
        "value": "ext 4123"
      }
    ],
    "end": "2025-01-01T00:00:00Z",
    "reason": "Monitor new neonatal function",
    "criteria": "Observation?code=http://loinc.org|1975-2",
    "channel": {
      "type": "rest-hook",
      "endpoint": "https://my-subscription-endpoint.com",
      "payload": "application/fhir+json",
      "header": [
        "Authorization: Bearer secret-token-abc-123"
      ]
    }
  }

Once this Subscription resource is posted to Firely Server, the server will be sending notifications (PUT by default) to the specified *endpoint* whenever a resource matching the search *criteria* gets created or updated. It is possible to provide *headers* that will be copied over to the notification requests. It may come in handy if the notifications endpoint is secured and the Authorization header must be used. The *payload* option defines the format of the notification payload. The following values can be used:

- *application/fhir+json* and *application/json*
- *application/fhir+xml* and *application/xml*

The payload will contain the created/updated resource in the FHIR format. If the payload option is omitted, the notifications will be sent without the body.

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
* ``SendRestHookAsCreate``: in versions < 3.9.3, Vonk sent RestHook notifications as a create operation using a PUT. This was not compliant with the specification that requires POST. The default value of ``false`` provides the old behaviour and sends a PUT. If set to ``true``, the rest hook call is compliant with the FHIR spec and a POST call is made. 

Note that the logs for subscriptions can be turned on by including ``"Vonk.Subscriptions.Evaluation.SubscriptionEvaluatorService": "Verbose"`` in the :ref:`configure_log`. 