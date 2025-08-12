.. _connecting_data_source:

Connecting Data Sources
=======================

.. toctree::
   :maxdepth: 1
   :titlesonly:
   :hidden:

   firely-server-ingest
   pubsub
   
Firely Server offers several advanced integration mechanisms to support scalable, real-time, and legacy-compatible use cases. This section details the differences between **Bulk Import (Ingest)**, **PubSub**, and **Facade** capabilities.

.. contents::

Bulk Import via Firely Server Ingest
------------------------------------

Firely Server Ingest enables efficient, large-scale data imports into Firely Server.

Key Features
^^^^^^^^^^^^

* Designed for bulk migration of FHIR resources.
* Avoids overhead of individual REST operations.
* Accepts FHIR `Bundle` resources or Bulk ndjson files
* Suitable for initial data loading, regular batch loading, or syncing legacy datasets.

Use Cases
^^^^^^^^^

* Seeding Firely Server instances with production or synthetic data.
* Migrating from non-FHIR data repositories.

.. seealso:: 
   See :ref:`tool_fsi`

Firely PubSub
-------------

Firely PubSub enables asynchronous, message-based communication between Firely Server and other systems using a message broker such as RabbitMQ, Azure Service Bus or Kafka.

Key Features
^^^^^^^^^^^^

* Publish events when resources are created, updated, or deleted.
* Subscribe to broker messages to execute create/update commands on the server.
* Loosely-coupled communication with `MassTransit` envelopes.
* Supports multiple channels and scalable topologies.

Use Cases
^^^^^^^^^

* Trigger downstream workflows when FHIR resources change.
* Integrate Firely Server into event-driven microservices.
* Feed changes into analytics or external data processors.

.. seealso:: 
   See :ref:`PubSub`

Facade
------

The Facade feature allows Firely Server to expose a **FHIR interface over existing data sources** without storing the data natively in FHIR.

Key Features
^^^^^^^^^^^^

* Acts as a **read/write proxy** over external systems (e.g., SQL, HL7v2).
* Enables FHIR compliance without migrating data.
* Requests are dynamically translated into the backend system’s native format.

Use Cases
^^^^^^^^^

* Wrapping legacy EMRs or hospital systems in a FHIR-compliant API.
* Supporting hybrid infrastructures with both FHIR and non-FHIR components.

.. seealso:: 
   See :ref:`vonk_facade`
