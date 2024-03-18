.. _PubSub:

Firely PubSub
=============

Firely offers the PubSub feature to enable other services to communicate with Firely Server on data changes asynchronously. Specifically, other applications can send *commands* to update FHIR resources in the database and subscribe to *events* published by the server whenever resources change. Both commands and events get communicated as messages via a message broker (RabbitMQ).

Using PubSub might provide several advantages:

* It is quicker than communicating via the REST API as it does not involve authorization/authentication and resource validation. PubSub assumes that all services communicating with Firely Server are internal and secure and that resources posted using `ExecuteStorePlanCommand` are correct FHIR resources, so they do not get validated.
* This set-up also enables easy integration with other applications which can be written using technologies other than .NET. As long as these applications correctly implement communication via the message broker, they are able to communicate with Firely Server.
* Having a message broker in the middle allows for building topologies where multiple applications can send commands and/or subscribe to events. 
* If Firely Server or any of the other applications communicating with it is down, the messages will aggregate in the message broker and get processed as soon as the service is up again.

.. attention::
  Correct configuration and maintenance of the message broker is not part of the service provided by Firely. We strongly advise you to consider this setup carefully in order to prevent data loss.

.. note::
  PubSub can be tested using the evaluation license for Firely Server. It is also included in the enterprise license for Firely Server. Your license allows the use of PubSub if "http://fire.ly/vonk/plugins/pubsub" is included in the plugins list of the license file.

.. _pubsub_configuration:

Configuration
-------------

You can enable PubSub by including the plugins in the pipeline options of the Firely Server `appsettings.instance.json` file:

.. code-block::

    "PipelineOptions": {
        "PluginDirectory": "./plugins",
        "Branches": [
            {
            "Path": "/",
            "Include": [
                ...
                "Vonk.Plugin.PubSub.Pub.MongoDb",
                "Vonk.Plugin.PubSub.Pub.Sql",
                "Vonk.Plugin.PubSub.Sub",
                ]
            }
        ]
    },
The ``Vonk.Plugin.PubSub.Sub`` plugin allows Firely Server to subscribe to messages published to a message broker by other clients. The ``Vonk.Plugin.PubSub.Pub.Sql`` and ``Vonk.Plugin.PubSub.Pub.MongoDb`` plugins allow Firely Server to publish changes of the respective database (either SQL or MongoDb) to the message broker. 
You can further adjust PubSub in the PubSub section of the `appsettings.instance.json` file:

.. code-block::

    "PubSub": {
        "MessageBroker": {
            "Host": "localhost", // The URL where the message broker can be found
            "Username": "guest", // RabbitMQ username
            "Password": "guest", // RabbitMQ password
            "ApplicationQueueName": "FirelyServer", // The name of the message queue used by Firely Server
            "VirtualHost": "/" // RabbitMQ virtual host; see https://www.rabbitmq.com/vhosts.html for details
        },
        // The section below contains configuration related to publishing events when data gets changed in Firely Server 
        // so that other services can sync with Firely Server. Note that this is only available for Firely Server 
        // instances that use SQL server (2016 and newer) or MongoDb as a repository database. 
        // It does not work in combination with SQLite. 
        // If you have configured MongoDB as a backend, note that only replica sets and sharded cluster scenarios are supported in combination with PubSub.
        "ResourceChangeNotifications": { 
            "SendLightEvents": false, // If enabled, FS will send out events on changes. These events will not contain the complete resource
            "SendFullEvents": false, // If enabled, FS will send out events on changes. These events will contain the complete resource
            "ExcludeAuditEvents": false, // If enabled, FS will send out events on changes of resources, except Audit Events
            "PollingIntervalSeconds": 5, // How often Firely Server will be polling the DB for changes
            "MaxPublishBatchSize": 1000 // The maximum amount of resource changes that can be sent in a single message
        }
    },

.. note::
  Enabling ResourceChangeNotifications requires one-time DB configuration to enable changes tracking for SQL server backends. See :ref:`SQL Server Tracking Initialization<pubsub_sql_tracking_init>` for the instructions.

.. note::
  If you have configured MongoDb as your Firely Server repository database, note that the publication plugin ``Vonk.Plugin.PubSub.Pub.MongoDb`` can only be used in combination with MongoDb `replica sets <https://www.mongodb.com/docs/manual/replication/>`_ or `sharded clusters <https://www.mongodb.com/docs/manual/sharding>`_, as the plugin utilizes the :ref:`Change Stream <https://www.mongodb.com/docs/manual/changeStreams/>` functionality of MongoDb and is thus restricted.

Message types and formats
-------------------------

To establish communication between Firely Server and other applications the parties must share the same contract. Every message in PubSub contains data that can logically be split into two groups: an envelope and the actual payload. This section describes both parts.

Message Envelope
^^^^^^^^^^^^^^^^

Firely Server uses a framework called MassTransit to interact with a message broker. If you want to integrate with Firely Server using PubSub, it is important that your messages are compatible with MassTransit. You can achieve this either by using a MassTransit library for your programming language (available for .NET) or by making sure the messages your application sends and consumes use the same schema as messages created by MassTransit.

MassTransit envelops the original domain-specific message payload and adds extra service information required for the proper routing of messages and some other helpful features.

For additional documentation on enveloping, please refer to the `MassTransit documentation page <https://masstransit.io/documentation/concepts/messages#message-headers>`_.

See an example of a complete enveloped ``ExecuteStorePlanCommand`` message that was sent to RabbitMQ below

.. container:: toggle

    .. container:: header

      Click to expand

    At least the following fields must be included:

    * `messageType` - contains a message type (see below for the list of message types)
    * `message` - contains the original domain-specific message payload
    * `headers` - a list of message headers
    * `responseAddress` - optional, but if present for commands, specifies what exchange FS will use to communicate a result of the command

    .. code-block::

      {
        "messageId": "ea230000-90d6-1865-57a4-08dbd54cb610",
        "requestId": "ea230000-90d6-1865-e314-08dbd54cb610",
        "correlationId": null,
        "conversationId": "ea230000-90d6-1865-c4a8-08dbd54cb810",
        "initiatorId": null,
        "sourceAddress": "rabbitmq://rabbitmq-host/source?temporary=true",
        "destinationAddress": "rabbitmq://rabbitmq-host/Firely.Server.Contracts.Messages.V1:ExecuteStorePlanCommand",
        "responseAddress": "rabbitmq://rabbitmq-host/response?temporary=true",
        "faultAddress": null,
        "messageType": [
          "urn:message:Firely.Server.Contracts.Messages.V1:ExecuteStorePlanCommand"
        ],
        "message": {
          "instructions": [
            {
              "itemId": "Patient/1",
              "resource": "{\"resourceType\":\"Patient\",\"id\":\"1\",\"meta\":{\"versionId\":\"1\"},\"name\":[{\"family\":\"Smith\"}]}",
              "resourceType": null,
              "resourceId": null,
              "currentVersion": null,
              "operation": "create"
            }
          ]
        },
        "headers": {
          "MT-Request-AcceptType": [
            "urn:message:Firely.Server.Contracts.Messages.V1:ExecuteStorePlanResponse"
          ],
          "fhir-release": "STU3"
        }
      }


ExecuteStorePlanCommand
^^^^^^^^^^^^^^^^^^^^^^^

This command can be sent to the message broker by your client to let Firely Server execute a batch of instructions to create, update, upsert, or delete resources that should be processed as a transaction, so either all of the instructions are performed, or none.

Note that this message should only contain one operation per resource (so per resource type + id) as the operations in the message are supposed to bring each resource involved to its desired final state, rather than reflect a set of operations that would present a history of operations on a resource.

.. container:: toggle

  .. container:: header

    Command

  .. code-block::

    {
      "messageType": [
        "urn:message:Firely.Server.Contracts.Messages.V1:ExecuteStorePlanCommand"
      ],
      "headers": {
        "fhir-release": "R4"
      },
      "responseAddress": "rabbitmq://rabbitmq-host/response-exchange?temporary=true",
      "message": {
        "instructions": [
            {
              "itemId": "example-operation",
              "resource": "{\"resourceType\":\"Patient\",\"id\":\"testid\",\"meta\":{\"versionId\":\"test\",\"lastUpdated\":\"2023-10-09T12:00:22.8990506+02:00\"},\"name\":[{\"family\":\"id=test\"}]}",
              "resourceType": "Patient",
              "resourceId": "testid",
              "currentVersion": "test",
              "operation": "create"
          }
        ]
      },
      ...
    }

  **Metadata**

  * ``messageType`` - always ``[ "urn:message:Firely.Server.Contracts.Messages.V1:ExecuteStorePlanCommand" ]``
  * ``headers.fhir-release`` specifies the FHIR version, either ``STU3``, ``R4``, or ``R5``
  * ``responseAddress`` - exchange that is going to be used by FS to communicate the result of the command (optional)

  **Message body**

  The ``ExecuteStorePlanCommand`` message contains an array of instructions, where each instruction can contain the following fields:

  * ``itemId`` - An identifier for this line in the plan. It is used to correlate the returned results of executing the plan to the item within the plan
  * ``resource`` - The complete resource as a json string, this needs to be added in case of a ``create``, ``update``, or ``upsert`` event
  * ``resourceType`` - The type of the resource you want to execute the operation on
  * ``resourceId`` - The id of the resource you want to execute the operation on
  * ``currentVersion`` - The optional expected current version (for ``update``, ``upsert`` and ``delete`` operations)
  * ``operation`` - The operation to execute with the payload. The following operations can be used:
  
      * ``create`` - Request to create a new resource. The resource, including its id and metadata, is stored exactly as provided in the property ``Resource``. The ``id``, ``versionId`` and ``lastUpdated`` must be present. A resource with the same id should not yet exist for this operation to succeed. 
      * ``update`` - Request to update an existing resource. The resource, including its id and metadata, is stored exactly as provided in the property ``Resource``. The ``id``, ``versionId`` and ``lastUpdated`` must be present. Optionally, a ``currentVersion`` can be provided for optimistic concurrency. A resource with the given id should already exist for this operation to succeed.
      * ``upsert`` - Request to upsert a resource. If the resource already exists, this operation is exactly the same as the ``update`` above. Otherwise, this operation acts as a ``create``.
      * ``delete`` - Requests to delete a resource referred to by the properties ``resourceType`` and ``resourceId`` if it exists, or nothing otherwise. Optionally, a ``CurrentVersion`` can be provided for optimistic concurrency. 
  
.. container:: toggle

  .. container:: header

    Response

  If a client sending a ``ExecuteStorePlanCommand`` message also specified a ``responseAddress`` value, Firely Server will generate a response of type ``ExecuteStorePlanResponse``.

  .. code-block::
    
    {
      "messageType": [
        "urn:message:Firely.Server.Contracts.Messages.V1:ExecuteStorePlanResponse"
      ],
      "headers": {
        "fhir-release": "R4"
      },
      "message": {
        "errors": [
          {
            "itemId": "example-operation",
            "status": {
              "code": "badRequest",
              "details": "BadRequestPayloadMissingLastUpdated"
            },
            "message": "No lastUpdated provided"
          }
        ]
      },
      ...
    }



  If Firely Server encounters errors when processing an ``ExecuteStorePlan`` message, it will respond with the result of this processing by sending an ``ExecuteStorePlanResponse`` message. This message will contain a list of ``StorePlanResultItems``, each containing the following fields:

  **Metadata**

  * ``messageType`` - always ``[ "urn:message:Firely.Server.Contracts.Messages.V1:ExecuteStorePlanResponse" ]``
  * ``headers.fhir-release`` specifies the FHIR version, either ``STU3``, ``R4``, or ``R5``

  **Message body**

  * ``itemId`` - The ``itemid`` of the instruction in the earlier sent ``ExecuteStorePlan`` that caused errors
  * ``status`` - The outcome of the processing, together with details on the error:

    * ``code`` - a high-level indication of the result. Can contain one of the following values:

      * ``success`` - Operation has been completed successfully
      * ``badRequest`` - The command contained an error. Refer to ``operationStatus.details`` for a more specific description
      * ``error`` - Operation failed because some business rules might have been violated
      * ``internalServerError`` - Operation failed due to an unexpected error in Firely Server

    * ``details`` - a more detailed description of what went wrong. Possible values:
    
      * ``BadRequestMissingItemId``
      * ``BadRequestMissingResourceId``
      * ``BadRequestPayloadMissingResourceId``
      * ``BadRequestPayloadMissingVersionId``
      * ``BadRequestPayloadMissingLastUpdated``
      * ``BadRequestMissingResourceType``
      * ``BadRequestMissingResourcePayload``
      * ``BadRequestWrongPayloadFormat``
      * ``BadRequestOperationNotSupported``
      * ``CreationSucceeded``
      * ``CreationFailedResourceAlreadyExists``
      * ``CreationFailedVersionIdCannotBeReused``
      * ``UpdateSucceeded``
      * ``UpdateFailedResourceNotFound``
      * ``UpdateFailedVersionIdMismatch``
      * ``UpdateFailedVersionIdCannotBeReused``
      * ``DeletionSucceeded``
      * ``DeletionFailedVersionIdMismatch``
  * ``message`` - a human-readable string containing information about the outcome

RetrievePlanCommand
^^^^^^^^^^^^^^^^^^^

As opposed to the ``ExecuteStorePlanCommand``, which can only be used for create, update, upsert, or delete operations, the ``RetrievePlanCommand`` can be sent by the client to retrieve a resource from Firely Server:

.. container:: toggle

  .. container:: header

    Command

  .. code-block::

    {
      "messageType": [
        "urn:message:Firely.Server.Contracts.Messages.V1:RetrievePlanCommand"
      ],
      "headers": {
        "fhir-release": "R4"
      },
      "responseAddress": "rabbitmq://rabbitmq-host/response-exchange?temporary=true",
      "message": {
        "instructions": [
          {
            "itemId": "example-operation",
            "reference": {
              "resourceType": "Patient",
              "resourceId": "test",
              "version": null
            }
          }
        ]
      },
      ...
    }

  
  **Metadata**

  * ``messageType`` - always ``[ "urn:message:Firely.Server.Contracts.Messages.V1:RetrievePlanCommand" ]``
  * ``headers.fhir-release`` specifies the FHIR version, either ``STU3``, ``R4``, or ``R5``
  * ``responseAddress`` - exchange that is going to be used by FS to communicate the result of the command

  **Message body**

  * ``itemId`` - An identifier for this line in the plan. Is used to correlate the retrieved resource in the result to this item within the plan
  * ``reference`` - A reference to the resource that is to be retrieved

    * ``resourceType`` - The type of the resource that is to be retrieved
    * ``resourceId`` - The id of the resource that is to be retrieved
    * ``version`` - Optionally the version of the resource that is to be retrieved

.. container:: toggle

  .. container:: header

    Response

  If a client sending a ``RetrievePlanCommand`` message also specified a ``responseAddress`` value, Firely Server will generate a response of type ``RetrievePlanResponse``.

  .. code-block::

    {
      "messageType": [
        "urn:message:Firely.Server.Contracts.Messages.V1:RetrievePlanResponse"
      ],
      "headers": {
        "fhir-release": "R4"
      },
      "message": {
        "items": [
          {
            "itemId": "example-operation",
            "resource": "{\"resourceType\":\"Patient\",\"id\":\"1\",\"meta\":{\"versionId\":\"2\",\"lastUpdated\":\"2023-01-01T00:00:00Z\"},\"name\":[{\"family\":\"Smith\"}]}",
            "status": {
              "code": "success",
              "details": "Ok"
            },
            "message": "Retrieved."
          }
        ]
      },
      ...
    }

  **Metadata**

  * ``messageType`` - always ``[ "urn:message:Firely.Server.Contracts.Messages.V1:RetrievePlanResponse" ]``
  * ``headers.fhir-release`` specifies the FHIR version, either ``STU3``, ``R4``, or ``R5``

  **Message body**

  This message type is the result that Firely Server sends to the message broker after ingesting a ``RetrievePlanCommand``. It contains the following fields:

  * ``itemId`` - The itemid corresponding to the itemid in the original ``RetrievePlanCommand``.
  * ``resource`` - If the ingestion of the ``RetrievePlanCommand`` was successful this field will contain a flattened json of the resource that is to be retrieved.
  * ``status`` - The outcome of the processing, together with details on the error:

    * ``code`` - a high-level indication of the result. Can contain one of the following values:

      * ``success`` - Operation has been completed successfully
      * ``badRequest`` - The command contained an error. Refer to ``operationStatus.details`` for a more specific description
      * ``error`` - Operation failed because some business rules might have been violated
      * ``internalServerError`` - Operation failed due to an unexpected error in Firely Server

    * ``details`` - a more detailed description of what went wrong. Possible values:
    
      * ``BadRequestMissingItemId``
      * ``BadRequestMissingReference``
      * ``ResourceNotFound``
      * ``MatchingVersionNotFound``
      * ``Ok``
      
  * ``message`` - Optional, this field may contain additional human-readable diagnostic information on the retrieve

ResourcesChangedEvent
^^^^^^^^^^^^^^^^^^^^^

If enabled, Firely Server can publish a ``ResourcesChangedEvent`` when one or more resources get changed. Other clients can then subscribe to this event.

.. attention::
    This functionality is not yet supported for SQLite or MongoDB.

.. note::
  Publishing of this event is disabled by default and must be enabled in the :ref:`configuration<pubsub_configuration>`.

.. container:: toggle

  .. container:: header

    Event

  .. code-block::

    {
      "messageType": [
        "urn:message:Firely.Server.Contracts.Messages.V1:ResourcesChangedEvent"
      ],
      "headers": {
        "fhir-release": "R4"
      },
      "message": {
        "changes": [
          {
            "reference": {
              "resourceType": "Patient",
              "resourceId": "example-id",
              "version": "59f47104-395a-4883-9689-259651939ca2"
            },
            "resource": "{\n  \"resourceType\": \"Patient\",\n  \"id\": \"example-id\",\n  \"meta\": {\n    \"versionId\": \"59f47104-395a-4883-9689-259651939ca2\",\n    \"lastUpdated\": \"2023-10-26T15:39:44.319+00:00\"\n  }\n}",
            "changeType": "create"
          }
        ]
      },
      ...
    }

    
  **Metadata**

  * ``messageType`` - always ``urn:message:Firely.Server.Contracts.Messages.V1:ResourcesChangedEvent``
  * ``headers.fhir-release`` specifies the FHIR version, either ``STU3``, ``R4``, or ``R5``

  **Message body**

  * ``reference`` - A reference to the resource for which the change is communicated
  * ``resource`` - A flattened json of the resource reflecting its state after the change was made
  * ``changeType`` - The kind of change that was made, either a ``create``, ``update``, or ``delete``


ResourcesChangedLightEvent
^^^^^^^^^^^^^^^^^^^^^^^^^^

If enabled, Firely Server can also publish ``ResourcesChangedLightEvent`` messages. This message type will contain information on the resource change but will not include the entire resource resource body. As it is with the ``ResourcesChangedEvent``, clients can subscribe to the corresponding message type ``ResourcesChangedLightEvent``.

.. attention::
    This functionality is not yet supported for SQLite or MongoDB.

.. note::
  Publishing of this event is disabled by default and must be enabled in the :ref:`configuration<pubsub_configuration>`.

.. container:: toggle

  .. container:: header

    Event

  .. code-block::

    {
      "messageType": [
        "urn:message:Firely.Server.Contracts.Messages.V1:ResourcesChangedLightEvent"
      ],
      "headers": {
        "fhir-release": "R4"
      },
      "message": {
        "changes": [
          {
            "reference": {
              "resourceType": "Patient",
              "resourceId": "fsiTestingPatient",
              "version": "41098b04-68ce-4b04-bce2-2d3c738d24f7"
            },
            "changeType": "create"
          }
        ]
      },
      ...
    }

  **Metadata**

  * ``messageType`` - always ``urn:message:Firely.Server.Contracts.Messages.V1:ResourcesChangedLightEvent``
  * ``headers.fhir-release`` specifies the FHIR version, either ``STU3``, ``R4``, or ``R5``

  **Message body**

  * ``reference`` - A reference to the resource for which the change is communicated
  * ``changeType`` - The kind of change that was made, either a ``create``, ``update``, or ``delete``


Message Routing
---------------

RabbitMQ
^^^^^^^^

All applications involved in message exchange are connected to the same message broker. Hypothetically, every party can publish and consume messages of any type. However, in practice, it is far more common that applications are only interested in consuming specific types of messages. Scenarios covered by PubSub are no exception. RabbitMQ allows for flexible configuration of message routing by decoupling message producers from message consumers using primitives such as `exchanges` and `queues`. You can read more about them in the `RabbitMQ documentation <https://www.rabbitmq.com/tutorials/amqp-concepts.html#amqp-model>`_.

**Events**

If you want to subscribe to events from Firely Server, your application will need to create a queue bound to either or both of these exchanges:

* ``Firely.Server.Contracts.Messages.V1:ResourcesChangedEvent``
* ``Firely.Server.Contracts.Messages.V1:ResourcesChangedLightEvent``

**Commands**

Likewise, to send a command to Firely Server, your application needs to publish it to the corresponding exchange:

* ``Firely.Server.Contracts.Messages.V1:ExecuteStorePlanCommand``
* ``Firely.Server.Contracts.Messages.V1:RetrievePlanCommand``

**Results**

If you are interested in the result of a command execution, your application should:

1. Create an exchange for capturing the response
2. Bind the exchange to the incoming queue of your application
3. Specify the exchange name in the ``responseAddress`` header of the command message (e.g. ``rabbitmq://rabbitmq-host/response-exchange-name?temporary=true`` where ``response-exchange-name`` is a name of your exchange)
4. Send the command
5. Listen for the response published by Firey Server

.. _azure_service_bus:

Azure Service Bus
^^^^^^^^^^^^^^^^^

As an alternative for RabbitMQ, it is also possible to set up Azure Service Bus as a messagebroker. The setup of Azure Service Bus is similar to that of RabbitMQ in that it differentiates between message producers and consumers, using `topics` and `subscriptions` rather than the RabbitMQ fanout `exchanges` for 1:n relations between these producers and consumers. More information on the workings of Azure Service Bus can also be found in `the Microsoft documentation <https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-overview>`_.

**Configuration**

To use Azure Service Bus rather than RabbitMQ you need to set this in the ``BrokerType`` field in your appsettings.instance.json::

      "PubSub": {
        "MessageBroker": {
            "Host": "Endpoint=sb://<Service Bus Namespace>.servicebus.windows.net/;SharedAccessKeyName=<Shared Access Key name>;SharedAccessKey=<Shared Access Key>",
            // "Username": "guest",
            // "Password": "guest",
            "ApplicationQueueName": "FirelyServer",
            // "VirtualHost": "/",
            "BrokerType": "AzureServiceBus" 
You can comment out the ``Username``, ``Password``, and ``VirtualHost`` fields, since these are specifically meant for connecting to RabbitMQ. For connecting to Azure Service Bus, it is necessary to provide a complete Shared Access Key connection string in the ``Host`` section.

**Events**

If you enabled ``ResourceChangeNotifications``, the following topics will automatically be generated by Firely Server after making a change to the Firely Server database:

* ``Firely.Server.Contracts.Messages.V1~ResourcesChangedEvent``
* ``Firely.Server.Contracts.Messages.V1~ResourcesChangedLightEvent``

Notice the ``~`` as opposed to the colon in the RabbitMQ exchanges. These topics will not have any subscriptions yet, so your application would need to create subscriptions for these topics. You then have the option to bind this subscription to a queue and connect your application to this queue, or you can retrieve the message directly from the subscription. 
With the latter option, it is possible to create multiple subscriptions to which multiple clients can connect for retrieving the message. If the subscription is bound to a queue, only one client would be able to retrieve the message via this queue.

Note that for retrieving these events it is best to replace the ``~`` in the topic with a forward slash, so when specifying the topic in your request you can use:

* ``Firely.Server.Contracts.Messages.V1/ResourcesChangedEvent``
* ``Firely.Server.Contracts.Messages.V1/ResourcesChangedLightEvent``

**Commands**

Upon startup of Firely Server, it will connect with Azure Service Bus and automatically generate a queue, ``firelyserver``, and two topics:

* ``Firely.Server.Contracts.Messages.V1~ExecuteStorePlanCommand``
* ``Firely.Server.Contracts.Messages.V1~RetrievePlanCommand``

Again, notice the ``~`` as opposed to the colon in the RabbitMQ exchanges. These topics wil already have a ``FirelyServer`` subscription, which is bound to the ``firelyserver`` queue mentioned earlier.

To send a command to Firely Server, your application would need to send it to the corresponding topics mentioned above, however rather than using the ``~`` in the topic, you can use a forward slash for making the connection:

* ``Firely.Server.Contracts.Messages.V1/ExecuteStorePlanCommand``
* ``Firely.Server.Contracts.Messages.V1/RetrievePlanCommand``

**Results**

Similar to RabbitMQ, if you are interested in the result of a command execution in Azure Service Bus your application should:

1. Create a `topic` for capturing the response
2. Create a `subscription` under that topic and bind this subscription to the incoming queue of your application
3. Specify the `topic` in the ``responseAddress`` header of the command message (e.g. ``sb://<Azure Service Bus namespace>.servicebus.windows.net/<topic>?type=topic``, it is important not to forget ``?type=topic`` in your connection string)
4. Send the command
5. Listen for the response published by Firey Server


Database Tracking Initialization
--------------------------------

.. _pubsub_sql_tracking_init:

SQL Server
^^^^^^^^^^

If you want to enable publishing notifications whenever resources get changed in Firely Server and you use SQL Server, some initial configuration is required to enable tracking of changes in the DB. This can be done automatically by Firely Server or manually.

**Automatic initialization**

If you want Firely Server to do that configuration for you, based on your settings:

.. code-block::

  {
    "SqlDbOptions": {
        "ConnectionString": "...",
        "AutoUpdateDatabase": true,
        "AutoUpdateConnectionString" : "..."
    },
    ...
  }

* The user mentioned in ``ConnectionString`` needs to have enough permissions to ``ALTER DATABASE``, or
* ``AutoUpdateDatabase`` is set to ``true`` and ``AutoUpdateConnectionString`` user can ``ALTER DATABASE``.

**Manual initialization**

Alternatively, you can initialize the tracking manually using the following script:

.. code-block::

  USE %YOUR_DB_NAME%

  ALTER DATABASE %YOUR_DB_NAME%
  SET CHANGE_TRACKING = ON  
  (CHANGE_RETENTION = 2 DAYS, AUTO_CLEANUP = ON)

  ALTER TABLE vonk.entry 
  ENABLE CHANGE_TRACKING

  CREATE TABLE vonk.ctdata
  (
    syncversion bigint
  )

  INSERT INTO vonk.ctdata (SYNCVERSION) VALUES (NULL)



Logging
-------

To enable logging for PubSub, you can add the PubSub plugin to the override section of your logsettings.json file:

.. code-block::

  {
    "Serilog": {
      "Using": [ "Firely.Server" ],
      "MinimumLevel": {
      "Default": "Error",
      "Override": {
          ...
          "Vonk.Plugin.PubSub": "Information"
      }
    },
    ...
  }

.. _pubsub_clients:

PubSub Clients
--------------

The recommended way for accessing the PubSub API from Firely Server is to use the `Firely Server Contract nuget package <https://www.nuget.org/packages/Firely.Server.Contracts>`_. 
This package contains the class definitions for all messages and as well as a client (``Firely.Server.Contracts.MassTransit.PubSubClient``).

Alternatively, you can use other platforms. In that case, you need to make sure that the messages you send and receive are compatible with the messages sent by Firely Server. 
See the `MassTransit documentation page <https://masstransit.io/documentation/concepts/messages#message-headers>`_ for more information on how to achieve that.

We provide sample code to connect to the pubsub API in the `firely-pubsub-sample Github Repository <https://github.com/FirelyTeam/firely-pubsub-sample>`_:

* A C# client using the `Firely Server Contract nuget package <https://www.nuget.org/packages/Firely.Server.Contracts>`_ in a ``.Net`` app, 
* A typescript client using the `masstransit-rabbitmq npm package <https://www.npmjs.com/package/masstransit-rabbitmq>`_  in a ``Node.js`` app,
* A postman collection displaying the raw queries to setup the infrastructure and send commands and receive events.

.. note::
  Before a client start consuming ``ResourceChangedEvent`` or ``ResourceLightChangedEvent``, it needs to create a queue and bind it the RabbitMq Exchange corresponding to the message type,
  ``Firely.Server.Contracts.Messages.V1:ResourcesChangedEvent`` and ``Firely.Server.Contracts.Messages.V1:ResourcesChangedLightEvent`` respectively. 
  Currently, Firely Server will setup those exchanges only once the first change in the database was detected.
  If using the `MassTransit RabbitMq nuget package <https://www.nuget.org/packages/MassTransit.RabbitMQ>`_, it will take care of setting up the exchange if not yet present.
  However, if not using this package, the client has to either take the responsibility of creating the correct exchange or wait until the exchanges are created, resulting in the loss of the first message.
  
