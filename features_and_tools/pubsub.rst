.. _PubSub:

Firely PubSub
=============

Firely offers the PubSub plugin to enable other services to communicate with Firely Server on data changes asynchronously. Specifically, other applications can send _commands_ to update FHIR resources in the database and subscribe to _events_ published by the server whenever resources change. Both commands and events get communicated as messages via a message broker (RabbitMQ).

Using PubSub might present several advantages:

* It is quicker than communicating via the REST API as it does not involve authorization/authentication, and resource validation. PubSub assumes that all services communicating with Firely Server are internal and secure and the resources posted using `ExecuteStorePlanCommand` are correct FHIR resources, so they do not get validated.
* This set-up also enables easy integration with other applications which can be written using technologies other than .NET. As long as these applications correctly implement communication via the message broker, they will be able to communicate with Firely Server.
* Having a message broker in the middle allows for building topologies where multiple applications can send commands and/or subscribe to events. 
* If the Firely Server or any of the other applications communicating with it is down, the messages will aggregate in the message broker and get processed as soon as the service is up again.

.. attention::
  Correct configuration and maintenance of the message broker is not part of the service provided by Firely. We strongly advice to consider this set-up carefully in order to prevent data loss.

.. note::
  PubSub currently supports only RabbitMQ. AzureServiceBus support is coming later.

.. note::
  PubSub can be tested using the evaluation license for Firely Server. It is also included in the enterprise license for Firely Server.

Configuration
-------------

You can enable PubSub by including the plugin into the pipeline options of the Firely Server `appsettings.instance.json` file:

.. code-block::

    "PipelineOptions": {
        "PluginDirectory": "./plugins",
        "Branches": [
            {
            "Path": "/",
            "Include": [
                ...
                "Vonk.Plugins.PubSub"
                ]
            }
        ]
    },

You can further adjust PubSub in the PubSub section of the `appsettings.instance.json` file:

.. code-block::

    "PubSub": {
        "MessageBroker": {
            "Host": "localhost", // The URL where the message broker can be found
            "Username": "guest", // Your username
            "Password": "guest", // Your password
            "ApplicationQueueName": "FirelyServer", // The name of the message queue used by Firely Server
            "VirtualHost": "/" // RabbitMQ virtual host; see https://www.rabbitmq.com/vhosts.html for details
        },
        // The section below contains configuration related to publishing events when data gets changed in Firely Server 
        // so that other services can sync with Firely Server. Note that this is only available for Firely Server 
        // instances that use SQL server (2016 and newer) as a repository database. 
        // As of yet, it cannot work in combination with MongoDB or SQLite.
        "ResourceChangeNotifications": { 
            "SendLightEvents": false, // If enabled, FS will send out events on changes. These events will not contain the complete resource
            "SendFullEvents": false, // If enabled, FS will send out events on changes. These events will contain the complete resource
            "PollingIntervalSeconds": 5, // How often Firely Server will be polling the DB for changes
            "MaxPublishBatchSize": 1000 // The maximum amount of resources changes that can be sent in a single message
        }
    },

Message types and formats
-------------------------

To establish communication between Firely Server and other applications, the parties must share the same contract. Every message in PubSub contains data that can logically be split into two groups: an envelope and the actual payload. This section describes both parts.

Message envelope
^^^^^^^^^^^^^^^^

Firely Server uses a framework called MassTransit to interact with a message broker. If you want to integrate with Firely Server using PubSub, it is important that your messages are compatible with MassTransit. You can achieve this either by using a MassTransit library for your programming language (available for .NET) or by making sure the messages your application sends and consumes use the same schema as messages created by MassTransit.

MassTransit envelops original message payload and adds extra service information required for proper routing of messages and some other helpful features.

For additional documentation on enveloping, please refer to the `MassTransit documentation page <https://masstransit.io/documentation/concepts/messages#message-headers>`_.

See an example of a complete enveloped ``ExecuteStorePlanCommand`` message that was sent to RabbitMQ below

.. container:: toggle

    .. container:: header

      Click to expand

    * `message` - contains the original message payload
    * `responseAddress` - for commands, if present, specifies what queue FS will use to send a result of the command
    * `messageType` - contains the message type (see below for the list of types)
    * `destinationAddress` - specifies an exchange name in RabbitMq. The value contains the fully qualified type name of the message
    * `headers.fhir-release` - contains the FHIR version. Possible values are:
  
      * `STU3`
      * `R4`
      * `R5`

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
      ...,
      "headers": {
        "fhir-release": "R4"
      },
      "messageType": [
        "urn:message:Firely.Server.Contracts.Messages.V1:ExecuteStorePlanCommand"
      ],
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
      }
    }

  The ``ExecuteStorePlanCommand`` message typically contains an array of instructions, where each instruction can contain the following fields:

  * ``headers.fhir-release`` specifies the FHIR version, either ``STU3``, ``R4``, or ``R5``
  * ``messageType`` – always ``[ "urn:message:Firely.Server.Contracts.Messages.V1:ExecuteStorePlanCommand" ]``
  * ``itemId`` - An identifier for this line in the plan. It is used to correlate the returned results of executing the plan to the item within the plan.
  * ``resource`` - The complete resource as a flattened json string, this needs to be added in case of a ``create``, ``update``, or ``upsert`` event. 
  * ``resourceType`` - The type of the resource you want to execute the operation on.
  * ``resourceId`` - The id of the resource you want to execute the operation on.
  * ``currentVersion`` - The optional expected current version (for ``update``, ``upsert`` and ``delete`` operations).
  * ``operation`` - The operation to execute with the payload. The following operations can be used:
  * 
      * ``create`` – Request to create a new resource. The resource, including its id and metadata, is stored exactly as provided in the property ``Resource``. The ``id``, ``versionId`` and ``lastUpdated`` must be present. A resource with the same id should not yet exist for this operation to succeed. 
      * ``update`` – Request to update an existing resource. The resource, including its id and metadata, is stored exactly as provided in the property ``Resource``. The ``id``, ``versionId`` and ``lastUpdated`` must be present. Optionally, a ``currentVersion`` can be provided for optimistic concurrency. A resource with the given id should already exist for this operation to succeed.
      * ``upsert`` – Request to upsert a resource. If the resource already exists, this operation is exactly the same as the ``update`` above. Otherwise, this operation acts as a ``create``.
      * ``delete`` – Requests to delete a resource referred to by the properties ``resourceType`` and ``resourceId`` if it exists, or nothing otherwise. Optionally, a ``CurrentVersion`` can be provided for optimistic concurrency. 
  
.. container:: toggle

  .. container:: header

    Response

  If a client sending a ``ExecuteStorePlanCommand`` message also specified a ``responseAddress`` value, Firely Server will generate a response of type ``ExecuteStorePlanResponse``.

  .. code-block::
    
    {
      ...,
      "messageType": [
        "urn:message:Firely.Server.Contracts.Messages.V1:ExecuteStorePlanResponse"
      ],
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
      }
    }

  If Firely Server encountered errors when processing an ``ExecuteStorePlan`` message it will respond with the result of this processing by sending an ``ExecuteStorePlanResponse`` message. This message will contain a list of ``StorePlanResultItems``, each containing the following fields:

  * ``messageType`` – always ``[ "urn:message:Firely.Server.Contracts.Messages.V1:ExecuteStorePlanResponse" ]``
  * ``itemId`` - The ``itemid`` of the instruction in the earlier sent ``ExecuteStorePlan`` that caused errors
  * ``status`` - The outcome of the processing, together with details on the error:
    * ``code`` – a high-level indication of the result. Can contain one of the following values:
      * ``success`` - Operation has been completed successfully
      * ``badRequest`` – The command contained an error. Refer to ``operationStatus.details`` for a more specific description
      * ``error`` – Operation failed because some business rules might have been violated
      * ``internalServerError`` – Operation failed due to an unexpected error in Firely Server
    * ``details`` – a more detailed description of what went wrong. Possible values:
    
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
      ...,
      "headers": {
        "fhir-release": "R4"
      },
      "messageType": [
        "urn:message:Firely.Server.Contracts.Messages.V1:RetrievePlanCommand"
      ],
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
      }
    }


  * ``headers.fhir-release`` specifies the FHIR version, either ``STU3``, ``R4``, or ``R5``
  * ``messageType`` – always ``[ "urn:message:Firely.Server.Contracts.Messages.V1:RetrievePlanCommand" ]``
  * ``itemId`` - An identifier for this line in the plan. Is used to correlate the retrieved resource in the result to this item within the plan
  * ``reference`` - A reference to the resource that is to be retrieved
    * ``resourceType`` - The type of the resource that is to be retrieved
    * ``resourceId`` - The id of the resource that is to be retrieved
    * ``version`` - Optionally the version of the resource that is to be retrieved

Firely Server will respond with a ``RetrievePlanResponse``, see below.

.. container:: toggle

  .. container:: header

    Response

  This message type is the result that Firely Server sends to the message broker after ingesting a ``RetrievePlanCommand``. It contains the following fields:

  * ``messageType`` – always ``[ "urn:message:Firely.Server.Contracts.Messages.V1:RetrievePlanResponse" ]``
  * ``itemId`` - The itemid corresponding to the itemid in the original ``RetrievePlanCommand``.
  * ``resource`` - If the ingestion of the ``RetrievePlanCommand`` was successful this field will contain a flattened json of the resource that is to be retrieved.
  * ``status`` - The outcome of the processing, together with details on the error:
    * ``code`` – a high-level indication of the result. Can contain one of the following values:
      * ``success`` - Operation has been completed successfully
      * ``badRequest`` – The command contained an error. Refer to ``operationStatus.details`` for a more specific description
      * ``error`` – Operation failed because some business rules might have been violated
      * ``internalServerError`` – Operation failed due to an unexpected error in Firely Server
    * ``details`` – a more detailed description of what went wrong. Possible values:
    
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
  Publishing of this event is disabled by default and must be enabled in the `configuration <Configuration>`_.

.. container:: toggle

  .. container:: header

    Event

  .. code-block::

    {
      ...,
      "headers": {
        "fhir-release": "R4"
      },
      "messageType": [
        "urn:message:Firely.Server.Contracts.Messages.V1:ResourcesChangedEvent"
      ],
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
    }

  * ``headers.fhir-release`` specifies the FHIR version, either ``STU3``, ``R4``, or ``R5``
  * ``messageType`` – always ``urn:message:Firely.Server.Contracts.Messages.V1:ResourcesChangedEvent``
  * ``reference`` - A reference to the resource for which the change is communicated
  * ``resource`` - A flattened json of the resource reflecting its state after the change was made
  * ``changeType`` - The kind of change that was made, either a ``create``, ``update``, or ``delete``


ResourcesChangedLightEvent
^^^^^^^^^^^^^^^^^^^^^^^^^^

If enabled, Firely Server can also publish ``ResourcesChangedLightEvent``s. This message type will contain information on the resource change but will not include the entire resource resource body. As it is with the ``ResourcesChangedEvent``, clients can subscribe to the corresponding message type ``ResourcesChangedLightEvent``.

.. attention::
    This functionality is not yet supported for SQLite or MongoDB.

.. note::
  Publishing of this event is disabled by default and must be enabled in the `configuration <Configuration>`_.

.. container:: toggle

  .. container:: header

    Event

  .. code-block::

  {
    ...,
    "headers": {
      "fhir-release": "R4"
    },
    "messageType": [
      "urn:message:Firely.Server.Contracts.Messages.V1:ResourcesChangedLightEvent"
    ],
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
    }
  }

  * ``headers.fhir-release`` specifies the FHIR version, either ``STU3``, ``R4``, or ``R5``
  * ``messageType`` – always ``urn:message:Firely.Server.Contracts.Messages.V1:ResourcesChangedLightEvent``
  * ``reference`` - A reference to the resource for which the change is communicated
  * ``changeType`` - The kind of change that was made, either a ``create``, ``update``, or ``delete``

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

