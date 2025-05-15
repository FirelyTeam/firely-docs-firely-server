.. _PubSub:

Firely PubSub
=============

.. note::

  The features described on this page are available in the following :ref:`Firely Server editions <vonk_overview>`:

  * Firely Scale - 🌍 / 🇺🇸
  * Firely CMS Compliance - 🇺🇸

Firely offers the PubSub feature to enable other services to communicate with Firely Server on data changes asynchronously. Specifically, other applications can send *commands* to update FHIR resources in the database and subscribe to *events* published by the server whenever resources change. Both commands and events get communicated as messages via a message broker (RabbitMQ, Azure Service Bus, or Kafka).

Using PubSub might provide several advantages:

* It is quicker than communicating via the REST API as it does not involve authorization/authentication and resource validation. PubSub assumes that all services communicating with Firely Server are internal and secure and that resources posted using `ExecuteStorePlanCommand` are correct FHIR resources, so they do not get validated.
* This set-up also enables easy integration with other applications which can be written using technologies other than .NET. As long as these applications correctly implement communication via the message broker, they are able to communicate with Firely Server.
* Having a message broker in the middle allows for building topologies where multiple applications can send commands and/or subscribe to events. 
* If Firely Server or any of the other applications communicating with it is down, the messages will aggregate in the message broker and get processed as soon as the service is up again.

.. attention::
  Correct configuration and maintenance of the message broker is not part of the service provided by Firely. We strongly advise you to consider this setup carefully in order to prevent data loss.

.. note::
  PubSub can be tested using the evaluation license for Firely Server. It is also included in the enterprise license for Firely Server. Your license allows the use of PubSub if ``http://fire.ly/vonk/plugins/pubsub`` is included in the plugins list of the license file.

.. warning::
  PubSub does not fully support :ref:`feature_multitenancy`. It will process the related security label if that is present in the resource. But the PubSub client sending or receiving the message is supposed to have access to all tenants and cannot specify a tenant in the message header. This limitation applies to all message brokers (RabbitMQ, Kafka, and Azure Service Bus).
  
  Likewise, PubSub does not support authorization - SMART on FHIR or otherwise - or auditing.

.. _pubsub_configuration:

Configuration
-------------

You can enable PubSub by including the plugins in the pipeline options of the Firely Server ``appsettings.instance.json`` file:

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

The ``Vonk.Plugin.PubSub.Sub`` plugin allows Firely Server to subscribe to messages published to a message broker by other clients. This is available for all repositories and all supported message brokers.

The ``Vonk.Plugin.PubSub.Pub.Sql`` and ``Vonk.Plugin.PubSub.Pub.MongoDb`` plugins allow Firely Server to publish changes of the respective database. This is available for the SQL Server and MongoDB repositories, in combination with either RabbitMQ or Azure Service Bus.

You can further adjust PubSub in the PubSub section of the ``appsettings.instance.json`` file. Firely Server supports the following message brokers: RabbitMQ, Azure Service Bus, and Kafka. The configuration for each of them is slightly different.

You can configure the notifications sent by ``Vonk.Plugin.PubSub.Pub``:

.. code-block:: JSON

    "PubSub": {
        "ResourceChangeNotifications": { 
            "SendLightEvents": false, // If enabled, FS will send out events on changes. These events will not contain the complete resource
            "SendFullEvents": false, // If enabled, FS will send out events on changes. These events will contain the complete resource
            "ExcludeAuditEvents": false, // If enabled, FS will send out events on changes of resources, except Audit Events
            "PollingIntervalSeconds": 5, // How often Firely Server will be polling the DB for changes
            "MaxPublishBatchSize": 1000 // The maximum amount of resource changes that can be sent in a single message
        }
    },

.. _pubsub_claimcheck:

Claim Check Pattern
^^^^^^^^^^^^^^^^^^^

To reduce the size of messages sent to the message broker, Firely Server supports the claim check pattern for ``ExecuteStorePlanCommand`` messages. With this pattern, the original payload is stored externally (currently only Azure Blob Storage is supported), and the message broker only carries a reference to the payload.

To enable the consumption of such messages, add a ``ClaimCheck`` section under ``PubSub`` in your configuration:

.. code-block:: JSON

    "PubSub": {
        "ClaimCheck": {
            "StorageType": "AzureBlobStorage", // or "Disabled"
            "AzureBlobContainerName": "<your-container-name>",
            "AzureBlobStorageConnectionString": "<your-connection-string>"
        }
    }

- ``StorageType``: Set to ``AzureBlobStorage`` to enable, or ``Disabled`` to turn off claim check.
- ``AzureBlobContainerName``: The name of the Azure Blob Storage container to use.
- ``AzureBlobStorageConnectionString``: The connection string for accessing Azure Blob Storage.

Only ``ExecuteStorePlanCommand`` messages can be offloaded to external storage using this pattern. Other message types are always sent in full via the broker.

Please refer to :ref:`pubsub_clients` to see how to use the claim check pattern in your client application.

.. note::
  Please note that files in Azure Blob Storage are not deleted automatically. To remove old files, you’ll need to implement a custom cleanup process. 
  
  One effective approach is to use `Azure Blob Storage lifecycle management <https://learn.microsoft.com/en-us/azure/storage/blobs/storage-lifecycle-management-concepts>`_.

RabbitMQ Configuration
^^^^^^^^^^^^^^^^^^^^^^

.. code-block:: JSON

    "PubSub": {
        "MessageBroker": {
            "BrokerType": "RabbitMq",
            "Host": "localhost",
            "Username": "guest",
            "Password": "guest",
            "PrefetchCount": 1,
            "ConcurrencyNumber": 1,
            "ApplicationQueueName": "FirelyServer", // The name of the message queue used by Firely Server
            "VirtualHost": "/"
        }
    },
    
- Host: The URL where the message broker can be found
- PrefetchCount: Number of messages to prefetch from the message broker. Sets the `PrefetchCout` MassTransit parameter https://masstransit.io/documentation/configuration#receive-endpoints.
- ConcurrencyNumber: Number of concurrent messages that can be consumed. Sets the `ConcurrentMessageLimit` MassTransit parameter https://masstransit.io/documentation/configuration#receive-endpoints
- ApplicationQueueName: The name of the message queue used by Firely Server
- VirtualHost: RabbitMQ virtual host; see https://www.rabbitmq.com/vhosts.html for details

Azure Service Bus Configuration
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

.. code-block:: JSON

    "PubSub": {
        "MessageBroker": {
            "BrokerType": "AzureServiceBus",
            "Host": "localhost",
            "PrefetchCount": 1,
            "ConcurrencyNumber": 1,
            "ApplicationQueueName": "FirelyServer"
        }
    },
    
- Host: ConnectionString to Azure Service Bus
- PrefetchCount: Number of messages to prefetch from the message broker. Sets the `PrefetchCout` MassTransit parameter https://masstransit.io/documentation/configuration#receive-endpoints.
- ConcurrencyNumber: Number of concurrent messages that can be consumed. Sets the `ConcurrentMessageLimit` MassTransit parameter https://masstransit.io/documentation/configuration#receive-endpoints
- ApplicationQueueName: The name of the message queue used by Firely Server


Kafka Configuration
^^^^^^^^^^^^^^^^^^^

.. code-block:: JSON

    "PubSub": {
      "MessageBroker": {
        "BrokerType": "Kafka", // Set to Kafka to use Kafka as the broker
        "Host": "localhost:9092", // Address of Kafka service 
        "Kafka": {
          "TopicPrefix": "FirelyServerCommands", // Prefix for topic names (OPTIONAL)
          "ClientGroupId": "FirelyServer", // Consumer group ID
          "ClientId": "FirelyServer", // Unique client identifier
          "NumberOfConcurrentConsumers": 5, // Number of parallel consumers
          "AuthenticationMechanism": "SaslScram256", // None, SaslPlain, SaslScram256, SaslScram512
          "Username": "admin", // Only needed for SASL authentication
          "Password": "******", // Only needed for SASL authentication
          "CaLocation": "/path/to/ca.pem", // Path to CA certificate for SSL
          "KeystoreLocation": "/path/to/kafka.keystore.p12", // Path to client keystore for SSL
          "KeystorePassword": "******", // Password for the keystore
          "ExecuteStorePlanCommandErrorTopicName": "FirelyServerCommands.ExecuteStorePlanCommand.Errors" // Optional custom topic for error messages
        }
      }
    }
    
- Setting AuthenticationMechanism to anything other than ``None`` will enable SASL authentication. The ``Username`` and ``Password`` fields are required for SASL authentication.
- Setting a value for ``CaLocation`` enables SSL.
- Setting a value for ``CaLocation`` *and* ``KeystoreLocation`` enables Mutual SSL. The ``KeystorePassword`` field is required for to read from the Keystore.

.. attention::
  SQLite backend is not supported for ResourceChangeNotifications.

.. note::
  Enabling ResourceChangeNotifications requires one-time DB configuration to enable changes tracking for SQL server backends. See :ref:`SQL Server Tracking Initialization<pubsub_sql_tracking_init>` for the instructions.

.. note::
  If you have configured MongoDb as your Firely Server repository database, note that the publication plugin ``Vonk.Plugin.PubSub.Pub.MongoDb`` can only be used in combination with MongoDb `replica sets <https://www.mongodb.com/docs/manual/replication/>`_ or `sharded clusters <https://www.mongodb.com/docs/manual/sharding>`_, as the plugin utilizes the `Change Stream <https://www.mongodb.com/docs/manual/changeStreams/>`_ functionality of MongoDb and is thus restricted.

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

Firely Server PubSub supports different message brokers, each with its own specific routing mechanisms: RabbitMQ, Kafka, and Azure Service Bus. Choose the one that best fits your infrastructure needs.

RabbitMQ
^^^^^^^^

All applications involved in message exchange are connected to the same message broker. Hypothetically, every party can publish and consume messages of any type. However, in practice, it is far more common that applications are only interested in consuming specific types of messages. Scenarios covered by PubSub are no exception. RabbitMQ allows for flexible configuration of message routing by decoupling message producers from message consumers using primitives such as `exchanges` and `queues`. You can read more about them in the `RabbitMQ documentation <https://www.rabbitmq.com/tutorials/amqp-concepts.html#amqp-model>`_.

**Additional configuration**

RabbitMQ has inbuilt support for `TLS <https://www.rabbitmq.com/docs/ssl#overview>`_. By default Firely Server PubSub assumes that TLS support is disabled for the message broker and connects to port `5672`. It is possible to change the port to `5671` in order to automatically enable TLS support.

      "PubSub": {
        "MessageBroker": {
            "Host": "Endpoint=sb://<Service Bus Namespace>.servicebus.windows.net/;SharedAccessKeyName=<Shared Access Key name>;SharedAccessKey=<Shared Access Key>",
            // "Username": "guest",
            // "Password": "guest",
            // "RabbitMQ": {
            //   "Port": 5672
            // },

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
5. Listen for the response published by Firely Server

.. _kafka:

Kafka
^^^^^

Kafka is a distributed event streaming platform that is well-suited for high-throughput, scalable message processing. Firely Server supports Kafka as a message broker for PubSub, allowing you to leverage Kafka's strengths in your FHIR infrastructure.

**Advantages of Kafka**

Kafka offers several advantages over other message brokers for FHIR data processing:

* **Scalability**: Kafka's partitioned design allows horizontal scaling to handle high volumes of healthcare data
* **Durability**: Persistent storage of messages enables replay and recovery scenarios
* **High Throughput**: Optimized for handling thousands of messages per second, beneficial for large healthcare systems
* **Fault Tolerance**: Built-in replication provides resilience against node failures
* **Message Retention**: Configurable retention policies allow historical data access when needed
* **Stream Processing**: Native compatibility with stream processing frameworks for real-time analytics

**Authentication and Security**

Kafka in Firely Server supports several authentication mechanisms specified by the ``AuthenticationMechanism`` setting:

* ``None`` - No authentication (only suitable for development environments)
* ``SaslPlain`` - Basic username and password authentication
* ``SaslScram256`` - SCRAM-SHA-256 authentication, more secure than SASL/PLAIN
* ``SaslScram512`` - SCRAM-SHA-512 authentication, the most secure SASL option

When using SASL authentication, you must provide:
* ``Username`` - The Kafka username for authentication
* ``Password`` - The corresponding password

**SSL/TLS Configuration**

Firely Server supports both one-way and two-way SSL/TLS for Kafka connections:

**One-way SSL/TLS** (Server authentication only):
* ``CaLocation`` - Path to the trusted CA certificate that signed the Kafka broker's certificate
* No client certificate is provided; the client (Firely Server) verifies the server but not vice versa

**Two-way SSL/TLS** (Mutual authentication):
* ``CaLocation`` - Path to the trusted CA certificate
* ``KeystoreLocation`` - Path to the PKCS#12 keystore containing the client certificate
* ``KeystorePassword`` - Password for accessing the keystore

For production environments, we strongly recommend:
1. Using SSL/TLS to encrypt all communication
2. Implementing SASL authentication (preferably SCRAM-SHA-256 or SCRAM-SHA-512)
3. If possible, using mutual TLS authentication for the strongest security model

**Topic Naming Convention**

Kafka uses topics to organize and categorize messages. In Firely Server, the topic names follow this convention:

* Command topics: ``<TopicPrefix>.<CommandName>``
* Error topics: ``<TopicPrefix>.<CommandName>.Errors``
* Event topics: Topic names match the message types (e.g., ``Firely.Server.Contracts.Messages.V1:ResourcesChangedEvent``)

The ``TopicPrefix`` is configurable and defaults to "FirelyServerCommands" if not specified.

**Message Routing**

When using Kafka, messages are routed using Kafka's publish-subscribe pattern:

1. **For sending commands to Firely Server**, publish messages to the corresponding topic:
   * ``<TopicPrefix>.ExecuteStorePlanCommand`` - For storing resources
   * ``<TopicPrefix>.RetrievePlanCommand`` - For retrieving resources

2. **For receiving events from Firely Server**, subscribe to these topics:
   * For full resource change events: ``Firely.Server.Contracts.Messages.V1:ResourcesChangedEvent`` (format may vary based on MassTransit configuration; in some setups, it might use a dot or slash instead of a colon)
   * For lightweight resource change events: ``Firely.Server.Contracts.Messages.V1:ResourcesChangedLightEvent``

3. **For handling command results**, Firely Server will publish responses to:
   * The topic specified in the ``responseAddress`` header of the command message. The response address format for Kafka should be: ``kafka://kafka-broker:9092/response-topic?type=topic``
   * Error messages to ``<TopicPrefix>.ExecuteStorePlanCommand.Errors`` by default

**Topic Creation**

Unlike RabbitMQ exchanges, Kafka topics need to be created before they can be used. You should create the required topics before starting to use PubSub with Kafka. Most Kafka distributions include tools like the Kafka Admin UI or command-line utilities for creating topics.

Required topics:
* ``<TopicPrefix>.ExecuteStorePlanCommand`` - For sending storage commands
* ``<TopicPrefix>.RetrievePlanCommand`` - For sending retrieval commands
* ``<TopicPrefix>.ExecuteStorePlanCommand.Errors`` - For error messages 
* Any custom error topics you've configured

The ``<TopicPrefix>`` is the value set in your configuration (defaults to "FirelyServerCommands" if not specified).

For resource change notifications, you may need to create event topics as well, depending on your MassTransit configuration.

**Delivery Guarantees and Message Ordering**

Kafka provides at-least-once delivery semantics in its default configuration. This means:
* Messages will be delivered to consumers even in case of temporary failures
* Duplicate deliveries are possible in failure scenarios, so consumers should handle this possibility

Message ordering in Kafka is guaranteed only within a single partition. Messages sent to the same partition will be processed in the order they were produced.

**Concurrency and Partitioning**

Kafka's partitioning allows for parallel processing of messages. The ``NumberOfConcurrentConsumers`` setting controls how many consumers Firely Server will create to process messages in parallel. This should be aligned with the number of partitions in your Kafka topics for optimal performance.

**Considerations for FHIR Resource Processing**

When dealing with FHIR resources, there are important considerations for partitioning:

1. **Resource Dependencies**: FHIR resources often have dependencies on other resources (e.g., an Observation referencing a Patient). If these related resources are processed in different partitions, there's a risk of processing them out of order. 

2. **Partition Key Selection**: Choosing the right partition key is critical:
   * Using resource ID as the key ensures operations on the same resource are processed in order
   * However, this doesn't account for relationships between different resources
   
3. **Transaction Boundaries**: For operations that must be atomic across multiple resources, consider:
   * Using the ExecuteStorePlanCommand to handle multiple resources in a single transaction
   * Implementing application-level checks to verify referential integrity

4. **Balancing Throughput and Consistency**:
   * More partitions increase throughput but may complicate ordering guarantees
   * Fewer partitions provide better ordering but limit parallelism

The optimal configuration depends on your specific use case and consistency requirements. For critical healthcare data, you may need to implement additional application-level validation to ensure data integrity when using highly concurrent processing.

**Message Serialization**

When using Kafka with Firely Server, messages are serialized as JSON. When implementing your own Kafka clients, you need to structure your messages appropriately:

* **Message body** - Contains the actual command or event payload
* **Headers** - Contains metadata needed for message routing and processing

For example, here's how to structure an ``ExecuteStorePlanCommand`` message for Kafka:

**Message Body:**

.. code-block::

    {
      "instructions": [
        {
          "itemId": "Patient/03",
          "resource": "{\"resourceType\":\"Patient\",\"id\":\"03\",\"meta\":{\"versionId\":\"1\",\"lastUpdated\":\"2024-07-29T14:20:43.49818+02:00\"},\"name\":[{\"family\":\"sam\"}]}",
          "resourceType": "Patient",
          "resourceId": "03",
          "currentVersion": "1",
          "operation": "create"
        }
      ]
    }

**Headers:**

.. code-block::

    {
      "SourceAddress": "loopback://localhost/",
      "ConversationId": "98640000-5d8f-0015-12be-08dcaa663884",
      "MessageId": "706e59e9-8ce2-4a23-83b2-4d2c4a0f70e7",
      "DestinationAddress": "loopback://localhost/kafka/FirelyServerCommands.ExecuteStorePlanCommand",
      "fhir-release": "R5"
    }

Note that the ``fhir-release`` header is important as it specifies which FHIR version is being used (R3, R4, or R5).

The message body contains the serialized FHIR resource within the "resource" field, which itself is a JSON string properly escaped. This format ensures the FHIR data maintains its structure while being transported through Kafka.

.. _azure_service_bus:

Azure Service Bus
^^^^^^^^^^^^^^^^^

As an alternative for RabbitMQ, it is also possible to set up Azure Service Bus as a message broker. The setup of Azure Service Bus is similar to that of RabbitMQ in that it differentiates between message producers and consumers, using `topics` and `subscriptions` rather than the RabbitMQ fanout `exchanges` for 1:n relations between these producers and consumers. More information on the workings of Azure Service Bus can also be found in `the Microsoft documentation <https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-overview>`_.

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
5. Listen for the response published by Firely Server


Database Tracking Initialization
--------------------------------

.. _pubsub_sql_tracking_init:

SQL Server
^^^^^^^^^^

If you want to enable publishing notifications whenever resources get changed in Firely Server and you use SQL Server, some initial configuration is required to enable tracking of changes in the DB. This can be done automatically by Firely Server or manually.

.. note::

    Not all editions of SQL Server support the required Change Data Capture features. See :ref:`configure_sql` for more information.

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

**Enhanced Logging for Message Brokers**

For more detailed logging of the message broker interactions, especially when troubleshooting Kafka connectivity or message processing, you can enable MassTransit logging:

.. code-block::

  {
    "Serilog": {
      "Using": [ "Firely.Server" ],
      "MinimumLevel": {
        "Default": "Error",
        "Override": {
          "Vonk.Plugin.PubSub": "Debug",
          "MassTransit": "Verbose",  // Enables detailed logging for all message broker operations
          ...
        }
      },
      ...
    }
  }

The "MassTransit" logging category covers all broker-specific operations, including Kafka connections, consumer operations, and message publishing. Setting this to "Verbose" provides the most detailed logs but may generate significant output in production environments.

.. _pubsub_clients:

PubSub Clients
--------------

The recommended way for accessing the PubSub API from Firely Server is to use the `Firely Server Contract nuget package <https://www.nuget.org/packages/Firely.Server.Contracts>`_. 
This package contains the class definitions for all messages and as well as a client (``Firely.Server.Contracts.MassTransit.PubSubClient``).

Alternatively, you can use other platforms. In that case, you need to make sure that the messages you send and receive are compatible with the messages sent by Firely Server. 
See the `MassTransit documentation page <https://masstransit.io/documentation/concepts/messages#message-headers>`_ for more information on how to achieve that.

We provide sample code to connect to the pubsub API in the `firely-pubsub-sample Github Repository <https://github.com/FirelyTeam/firely-pubsub-sample>`_:

* A C# client using the `Firely Server Contract nuget package <https://www.nuget.org/packages/Firely.Server.Contracts>`_ in a ``.Net`` app, 

  * If you want to use the :ref:`pubsub_claimcheck` in your client app, you also need to `configure MassTransit ClaimCheck <https://masstransit.io/documentation/patterns/claim-check>`_ middleware. Then, you need to instantiate a `PubSubClient` with the constructor parameter **useClaimCheckPattern = false**.
* A typescript client using the `masstransit-rabbitmq npm package <https://www.npmjs.com/package/masstransit-rabbitmq>`_  in a ``Node.js`` app,
* Python scripts that demo how to send ``ExecuteStorePlanCommand`` messages

  * Using plain ``ExecuteStorePlanCommand`` payload
  * And using the :ref:`Claim Check <pubsub_claimcheck>` capablilities to send large resources
* A postman collection displaying the raw queries to setup the infrastructure and send commands and receive events.

.. note::
  Before a client starts consuming ``ResourceChangedEvent`` or ``ResourceLightChangedEvent``, it needs to create the appropriate message infrastructure:
  
  * For RabbitMQ: Create a queue and bind it to the RabbitMQ Exchange corresponding to the message type
    (``Firely.Server.Contracts.Messages.V1:ResourcesChangedEvent`` and ``Firely.Server.Contracts.Messages.V1:ResourcesChangedLightEvent``).
  
  * For Kafka: Create the necessary topics before starting to use them. Unlike RabbitMQ, Kafka requires topics to be
    explicitly created before they can be used.
  
  Currently, Firely Server will setup RabbitMQ exchanges only once the first change in the database is detected. For Kafka, topics must be created explicitly.
  
  If using the `MassTransit RabbitMQ nuget package <https://www.nuget.org/packages/MassTransit.RabbitMQ>`_, it will automatically create exchanges if not present.
  For Kafka with `MassTransit Kafka nuget package <https://www.nuget.org/packages/MassTransit.Kafka>`_, automatic topic creation depends on specific Kafka broker 
  settings and MassTransit configuration (TopicEndpoint configuration with CreateIfMissing option). In most production Kafka deployments, 
  auto-creation of topics is disabled for security reasons, so you should create topics manually.
  
  If not using these packages, the client must take responsibility for creating the correct infrastructure before exchanging messages,
  or risk message loss.