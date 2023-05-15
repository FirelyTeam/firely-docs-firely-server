.. _sync:

Firely Sync
===========

Firely offers Firely Sync to enable other services to asynchronically communicate with Firely Server on data updates. These updates are communicated via messages with a message broker such as RabbitMQ or Azure Service Bus. 
This set-up enables easy integration with other frameworks outside .NET; as long as these frameworks correctly implement the library of the message broker, they will be able to communicate with Firely Server. 
Another benefit is that if the outside service or Firely Server is down, the messages containing information on data updates will be retained by the message broker. These messages will be processed again after the services are back up.

.. attention::
    Correct configuration and maintenance of the message broker is not part of the service provided by Firely. We strongly advice to consider this set-up carefully in order to prevent data loss.


Firely Sync has two parts: Sync-Out and Sync-In.

Sync-Out
--------
Sync-Out is useful for communicating changes in the data by Firely Server to outside services.
When Firely Server updates a resource, this can trigger a message to the outside service containing the resource change. 
The outside service is then able to retrieve this change using a retrieve plan.

Resource change
^^^^^^^^^^^^^^^
The resource change contains a ``Reference`` to the resource, the ``Resource`` payload, and the ``ChangeType`` that was made. 

Within the resource change the resource ``Reference`` is a combination of ``Area``, ``ResourceType``, ``ResourceId`` and ``Version`` that uniquely refers to a resource within a distributed system.
Here, an ``Area`` is a string that can be used to divide the collection of resources in different parts, e.g. to partition data for scaling purposes or separate data by tenant. 

The ``Resource`` payload reflects the state of that resource in json after the change was made. This payload is optional, meaning that it can show for a Create or Update operation, but not for a Deletion. 

The ``ChangeType`` can be one of the Create, Update, or Delete operations.

Retrieve plan
^^^^^^^^^^^^^

After receiving the resource change message, the outside service can request the changed resources with a retrieve plan. A retrieve plan is a batch of retrieve instructions to fetch resources from a store. 
The operation will return the resources requested, or an error for each of the instructions that failed. 

An instruction for a retrieve plan typically contains an ``ItemId``, which is an identifier for this line in the plan. It is used to correlate the retrieved resource in the result to this item within the plan.
An instruction also contains the ``Reference`` to the resource that is to be retrieved. This ``Reference`` is the same as mentioned before, i.e. a combination of ``Area``, ``ChangeType``, ``ResourceId``, and ``Version``.
Since the store does not keep history, the version serves only to assure the specified version in the instruction and the current version in the store are the same.

Sending a retrieve plan to Firely Server will yield a retrieve plan result. The retrieve plan result contains the items of a retrieve plan, one for each line in the retrieve plan. 
An item consists of an ``ItemId``, a json representation of the ``Resource`` to be fetched (upon success), the ``Status`` of the retrieve operation, and additional information on the operation outcome in the form of a ``Message`` field.

The following response statuses will be shown by Firely Server for the outcome of a retrieve plan:

* ``http status: 200`` - Resource was found and returned.
* ``http status: 400`` - Invalid or incomplete (this message includes parse errors in the payload)
* ``http status: 404`` - Resource with the given reference could not be found.
* ``http status: 412`` - The resource was found but did not match the given current version.
* ``http status: 500`` - Server error.


Sync-In
-------

Sync-In comes into play when services outside of Firely Server are performing changes on the data that Firely Server is using.
Sync-In is quicker than communicating these changes via the Firely Server REST API as it does not involve authorization/authentication. 
It assumes that all services communicating with Firely Server are internal and secure.
You can configure Sync-In in the appsettings:

.. code-block:: json
    
    "SyncIn": {
        "Host": "localhost",
        "Username": "guest",
        "Password": "guest",
        "QueueName": "sync-in",
        "VirtualHost": "/",
        "BrokerType": "RabbitMq" //  RabbitMq, AzureServiceBus
    },


Sync-In works with a store plan, which is a batch of instructions to store or delete resources that should be processed as a transaction, so either
all of the instructions in the plan are performed, or none. The instructions in the plan are CRUD-type operations that operate on a store of resources, each with its own ``id``. 
These ids are unique per type of resource. Conceptually, the store holds one "current" version of each resource, for which it tracks a ``versionId`` and a ``lastUpdated``. 
The store is expected to store the resource payloads in the plan as-is, including the metadata. As a consequence, it is the client's responsibility to provide the ``id``, ``versionId`` and ``lastUpdated``. 


If the store is not able to store (parts) of the data, it should return a 422 (Unprocessable Entity). If the client fails to provide any of the necessary metadata, it should return a
400 (Bad Request). 
A store plan should only contain one operation per resource (so per resource type + id) as the operations in the plan are supposed to bring each resource involved to its desired final state, 
rather than reflect a set of operations that would present a history of operations on a resource.

Upon receiving a store plan, Firely Server will send out a store plan result containing for each of the items in the original received store plan an ``ItemId``, ``Status``, and a ``Message`` with additional information.