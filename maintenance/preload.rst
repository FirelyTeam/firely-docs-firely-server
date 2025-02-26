.. |br| raw:: html

   <br />

.. _feature_preload:

Preloading resources
====================

If you have set up Firely Server as a reference server in a testing environment, it can be useful to load it with an 'iron test set' of examples. 
You can do that with the preload feature. Usually you will want to :ref:`feature_resetdb` first.

To preload a set of resources, execute:
::

    POST http(s)://<firely-server-endpoint>/administration/$preload
    Content-Type: application/octet-stream
    Body: a zip file with resources, each resource in a separate file (xml or json).


Firely Server will return statuscode 200 if the operation succeeded. 

If you are :ref:`not permitted <configure_administration_access>` to preload resources into the database, Firely Server will return statuscode 403.

.. note:: The operation can take quite long if the zip contains many resources. |br|
	E.g. when uploading the `examples-json.zip <http://www.hl7.org/fhir/examples-json.zip>`__ from the specification, it took about a minute on MongoDb and about 7 minutes on SQL Server on a simple test server.

.. attention:: This feature is not meant for bulk uploading really large sets of resources. Firely Server currently has no special operation for bulk operations.
