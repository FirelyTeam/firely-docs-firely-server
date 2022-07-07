.. _feature_healthcheck:

Liveness and readiness
========================

Description
-----------

It can be useful to check whether Firely Server is still up and running, and ready to handle requests. Either just for notification, or for automatic failover.
Another use is to have dependent services wait with starting for Firely Server to be up, e.g. in a docker-compose or in a Helm chart.

Firely Server provides two endpoints, each in the form a of a FHIR custom operation, for different purposes:

* ``GET <base>/$liveness``
* ``GET <base>/$readiness``

These align - intentionally - with the use of liveness and readiness probes in Kubernetes.

The major difference is in the ability to handle requests. Some operations on Firely Server can trigger a long running process during which the server cannot reliably handle requests, see the :ref:`Long running tasks plugin <vonk_plugins_longrunning>`. 
If this is the case, the ``$liveness`` operation will return a ``200 OK`` status regardless. So you can be sure the server is up, and should not be restarted (that would just delay the long running process). The ``$readiness`` operation however, will in this case return ``423 Locked``. If no long running processes are active, both operations will have the same output.

If you have assigned different endpoints to different FHIR versions (see :ref:`here <feature_multiversion_endpoints>`), you can also invoke it on each FHIR version. The result is always the same for all versions that are configured in the server. E.g:

.. code-block:: 

   GET <base-url>/$liveness
   GET <base-url>/STU3/$liveness
   GET <base-url>/R4/$liveness

Results
-------

The ``$liveness`` operation may return one of these http status codes:

#. 200 OK: Firely Server is up and running (but may still be blocked by a long running process).
#. 402 Payment Required: The license is expired or otherwise invalid.
#. 500 or higher: An unexpected error happened, the server is not running or not reachable (in the latter case the error actually comes from a component in front of Firely Server).

The ``$readiness`` operation may return one of these http status codes:

#. 200 OK: Firely Server is up and running and ready to process requests.
#. 423 Locked: Firely Server is busy with a long running operation and cannot process requests.  This could among others be a :ref:`database migration <upgrade>` or an :ref:`import of conformance resources <conformance_import>`. The response will have an OperationOutcome with additional details.
#. 402 Payment Required: The license is expired or otherwise invalid.
#. 500 or higher: An unexpected error happened, the server is not running or not reachable (in the latter case the error actually comes from a component in front of Firely Server).


Configuration
-------------

Both operations should be configured in the pipeline, see the plugin reference for :ref:`$liveness <vonk_plugins_liveness>` and :ref:`$readiness <vonk_plugins_readiness>`. In the default settings this is the case.
Both plugins have no further configuration in the :ref:`appsettings <configure_appsettings>`.
