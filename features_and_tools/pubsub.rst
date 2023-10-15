.. _PubSub:

Firely PubSub
=============

Firely offers the PubSub plugin to enable other services to communicate with Firely Server on data changes asynchronously. Specifically, other applications can send _commands_ to update FHIR resources in the database and subscribe to _events_ published by the server whenever resources change. These changes get communicated via messages with a message broker such as RabbitMQ or Azure Service Bus. 
In doing so it is quicker than communicating these changes via the Firely Server REST API as it does not involve authorization/authentication. PubSub assumes that all services communicating with Firely Server are internal and secure.
This set-up also enables easy integration with other applications which can even be written using technologies other than .NET. As long as these applications correctly implement communication via the message broker, they will be able to communicate with Firely Server.
Having a message broker in the middle allows for building topologies where multiple applications can send commands and/or subscribe to events. Another benefit is that if the outside service or Firely Server is down, the messages containing information on data updates can be retained by the message broker. These messages will be processed again after the services are back up.

.. attention::
    Correct configuration and maintenance of the message broker is not part of the service provided by Firely. We strongly advice to consider this set-up carefully in order to prevent data loss.

.. note::
  PubSub currently supports two kinds of message brokers: AzureServiceBus and RabbitMQ.

.. note::
    PubSub can be tested using the evaluation license for Firely Server. It is also included in the enterprice license for Firely Server.

You can enable PubSub by including the plugin into the pipeline options of the Firely Server appsettings.json file:

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

You can further adjust PubSub in the PubSub section of the appsettings.json file:

.. code-block::

    "PubSub": {
        "MessageBroker": {
            "Host": "localhost",
            "Username": "guest",
            "Password": "guest",
            "ApplicationQueueName": "FirelyServer",
            "VirtualHost": "/",
            "BrokerType": "RabbitMq" //  RabbitMq, AzureServiceBus
        },
        "ResourceChangeNotifications": {
            "SendLightEvents": false,
            "SendFullEvents": false,
            "PollingIntervalSeconds": 5,
            "MaxPublishBatchSize": 1000
        }
    },

* ``MessageBroker`` - This section is for syncing Firely Server with incoming messages in the message broker.
* ``Host`` - The URL where the messagebroker can be found. If you use AzureServiceBus, this URL will already contain the credentials necessary to make the connection, so that you do not have to fill the username and password sections here.
* ``Username`` - Your RabbitMQ username.
* ``Password`` - Your RabbitMQ password.
* ``ApplicationQueueName`` - The name of the message queue used by Firely Server.
* ``VirtualHost`` - RabbitMQ virtual host; see https://www.rabbitmq.com/vhosts.html for details.
* ``BrokerType`` - The message broker that you want to use, either AzureServiceBus or RabbitMQ.
* ``ResourceChangeNotifications`` - This section  contains configuration related to publishing events when data gets changed in Firely Server so that other services can sync with Firely Server. Note that this is only available for Firely Server instances that use SQL server (2016 and newer) as a repository database. As of yet, it cannot work in combination with MongoDB or SQLite.
* ``SendLightEvents`` - Either set to true or false, this will enable Firely Server to send out messages with information on changes, these messages will not contain the complete resource.
* ``SendFullEvents`` - Either set to true or false, this will enable Firely Server to send out messages with information on changes, these messages contain the complete resource.
* ``PollingIntervalSeconds`` - By default set to five seconds, Firely Server will check the database every five seconds for changes. 
* ``MaxPublishBatchSize`` - The maximum amount of resources that can be sent to the message broker in one message.


Message types and formats
-------------------------

To establish communication between Firely Server and other applications, the parties must share the same contract. This section describes the contract.
PubSub uses MassTransit to envelope messages using specific headers, for documentation on these headers we refer to the `MassTransit documentation page <https://masstransit.io/documentation/concepts/messages#message-headers>`_.
Though most headers are automatically assigned by MassTransit, some need to be specified by the client, these are listed below:

* ``responseAddress`` - Where responses to the request should be sent. If not present, responses are published.
* ``expirationTime`` - Messages will be removed if not consumed before the expiration time.
* ``requestId`` - Assigned by the client to correlate responses to the original request.
* ``faultAddress`` -Where consumer faults should be sent. If not present, faults are published.
* ``correlationId`` - Assigned by the client to uniquely identify the event.
* ``messageType`` - One of the message types described below.
* ``headers`` - Additional headers that can be added by the client. In the example below, these headers include information on the FHIR version and the expected message type of the response.

Below is an example of a complete enveloped ``ExecuteStorePlanCommand`` message that was sent to RabbitMQ:

.. code-block::

    {
  "messageId": "<automatically assigned id>",
  "requestId": "<automatically assigned id>",
  "correlationId": null,
  "conversationId": "<automatically assigned id>",
  "initiatorId": null,
  "sourceAddress": "<source address of the message, this may be a temporary address for messages sent from IBus>",
  "destinationAddress": "<where the message was sent, such as an exchange in RabbitMQ>",
  "responseAddress": "<where responses to the request should be sent>",
  "faultAddress": null,
  "messageType": [
    "urn:message:Firely.Server.Contracts.Messages.V1:ExecuteStorePlanCommand"
  ],
  "message": {
    "instructions": [
      {
        "itemId": "Patient/1",
        "resource": "{\"resourceType\":\"Patient\",\"id\":\"1\",\"meta\":{\"lastUpdated\":\"2023-10-15T12:35:54.5070437+02:00\"},\"name\":[{\"family\":\"test\"}]}",
        "resourceType": null,
        "resourceId": null,
        "currentVersion": null,
        "operation": "create"
      }
    ]
  },

  "expirationTime": "2023-10-15T10:36:55.0495563Z",
  "sentTime": "2023-10-15T10:35:54.8989292Z",
  "headers": {
    "MT-Request-AcceptType": [
      "urn:message:Firely.Server.Contracts.Messages.V1:ExecuteStorePlanResponse"
    ]
    "fhir-release": "STU3"
  },
  "host": {
    "machineName": "COMPUTER",
    "processName": "Firely.Server.MessageSender",
    "processId": 25884,
    "assembly": "Firely.Server.MessageSender",
    "assemblyVersion": "5.4.0.0",
    "frameworkVersion": "6.0.22",
    "massTransitVersion": "8.0.16.0",
    "operatingSystemVersion": "Microsoft Windows NT 10.0.19045.0"
  }
}

Below we give an overview of the different message types that can be communicated with PubSub.

ExecuteStorePlanCommand
^^^^^^^^^^^^^^^^^^^^^^^

This command can be sent to the message broker by your client to let Firely Server execute a batch of instructions to create, update, upsert, or delete resources that should be processed as a transaction, so either
all of the instructions are performed, or none. The instructions are CRUD-type operations that operate on a store of resources, each with its own id. These ids are unique per type of resource.
Note that this message should only contain one operation per resource (so per resource type + id) as the operations in the message are supposed to bring each resource involved to its desired final state, rather than reflect a set of operations that would present a history of operations on a resource.


.. code-block::

      "message": {
        "instructions": [
            {
            "itemId": "Patient/testid",
            "resource": "{\"resourceType\":\"Patient\",\"id\":\"testid\",\"meta\":{\"versionId\":\"versionId=test\",\"lastUpdated\":\"2023-10-09T12:00:22.8990506+02:00\"},\"name\":[{\"family\":\"id=test\"}]}",
            "resourceType": "",
            "resourceId": "",
            "currentVersion": "",
            "operation": create
        }
        ]
    }

The ``ExecuteStorePlanCommand`` message typically contains an array of instructions, where each instruction can contain the following fields:

* ``itemId`` - An identifier for this line in the plan. It is used to correlate the returned results of executing the plan to the item within the plan.
* ``resource`` - The complete resource as a flattened json string, this needs to be added in case of a Create, Update, or Upsert event. 
* ``resourceType`` - The type of the resource that you want to delete, in case of a Delete event.
* ``resourceId`` - The unique id of the resource that you want to delete, in case of a Delete event.
* ``currentVersion`` - The expected current version number of the resource, for an Update, Upsert, or Delete event.
* ``operation`` - The kind of change this resource had undergone, see below

The following operations can be included in the message:

* None
* Create
* Update
* Upsert
* Delete


ExecuteStorePlanResponse
^^^^^^^^^^^^^^^^^^^^^^^^

If Firely Server encountered errors when  processing an ``ExecuteStorePlan`` message it will respond with the result of this processing by sending an ``ExecuteStorePlanResponse`` message. This message will contain a list of ``StorePlanResultItems``, each containing the following fields:

* ``itemId`` - The itemid of the instruction in the earlier sent ``ExecuteStorePlan`` that caused errors
* ``status`` - The outcome of the processing, together with details on the error
* ``message`` - Additional information on the error

RetrievePlanCommand
^^^^^^^^^^^^^^^^^^^

As opposed to the ``ExecuteStorePlanCommand``, which can only be used for create, update, upsert, or delete operations, the ``RetrievePlanCommand`` can be sent by the client for read operations on Firely Server resources:

.. code-block::

    
  "message": {
    "instructions": [
      {
        "itemId": "Patient/Patient/test",
        "reference": {
          "resourceType": "Patient",
          "resourceId": "Patient/test",
          "version": null
        }
      }
    ]
  },


* ``itemId`` - An identifier for this line in the plan. Is used to correlate the retrieved resource in the result to this item within the plan.
* ``reference`` - A reference to the resource that is to be retrieved.
* ``resourceType`` - The type of the resource that is to be retrieved.
* ``resourceId`` - The id of the resource that is to be retrieved.
* ``version`` - Optionally the version of the resource that is to be retrieved.

Firely Server will respond with a ``RetrievePlanResponse``, see below.

RetrievePlanResponse
^^^^^^^^^^^^^^^^^^^^

This message type is the result that Firely Server sends to the message broker after ingesting a ``RetrievePlanCommand``. It contains the following fields:

* ``itemId`` - The itemid corresponding to the itemid in the original ``RetrievePlanCommand``.
* ``resource`` - If the ingestion of the ``RetrievePlanCommand`` was successful this field will contain a flattened json of the resource that is to be retrieved.
* ``status`` - The http status code of the result of the retrieve.
* ``message`` - Optional, this field may contain additional diagnostic information on the retrieve.


.. attention::
    The messages below are part of the functionality of PubSub that will communicate changes from Firely Server to the message broker. This functionality is currently only supported for Firely Server instances running with an SQL (2016 or newer) backend. This functionality is not yet supported for SQLite or MongoDB.

ResourceChangedEvent
^^^^^^^^^^^^^^^^^^^^

If enabled, Firely Server can send a ``ResourceChangedEvent`` to the message broker when there are changes in the database. Other clients can then subscribe to the corresponding message queue to ingest this message.  A ``ResourceChangedEvent`` will contain the following fields:

* ``reference`` - A reference to the resource for which the change is communicated.
* ``resource`` - A flattened json of the resource reflecting its state after the change was made.
* ``changeType`` - The kind of change that was made, either a Create, update, or Delete.


ResourceChangedLightEvent
^^^^^^^^^^^^^^^^^^^^^^^^^

If enabled, Firely Server can also send a ``ResourceChangedLightEvent``. This message type will contain information on the resource change but will not include the entire resource json. As it is with the ``ResourceChangedEvent``, clients can subscribe to the corresponding message queue of ``ResourceChangedLightEvent`` to ingest this message. 
It contains the following fields:

* ``reference`` - A reference to the resource for which the change is communicated.
* ``changeType`` - The kind of change that was made, either a Create, update, or Delete.

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