.. _upgrade:

Alerting for critical errors
============================

Ensuring uninterrupted operation of the Firely Server in production is crucial, as business processes built on top of the FHIR server require high availability.

To quickly identify and respond to the causes of unplanned downtime, we recommend integrating Firely Server with a monitoring system capable of sending real-time notifications for critical errors.

We suggest using `Seq <https://docs.datalust.co/docs/alerts>`_ and its alerting capabilities for this purpose. To reduce the risk of false positives, it is advisable to exclude all requests that return an HTTP status code 501 – Not Implemented from triggering alerts.
Please make sure that logging of individual requests is enabled, see :ref:`logging_individual_requests`.

The following query can be used as a source for the alerts::

   select count(*) as count
   from stream
   where @Level = 'Error' or StatusCode = 500
   group by time(1m)
   having count > 0

Seq provides a number of app integrations for receiving notifications on these alters (e.g. Slack, SMTP).




