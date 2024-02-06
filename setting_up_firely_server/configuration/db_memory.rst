.. _configure_memory:

Using the In-Memory storage
===========================

* Navigate to your Firely Server working directory
* Changing a setting means overriding it as described in :ref:`configure_change_settings`. 

* Find the ``Repository`` setting::

	"Repository": "Memory",

* If it is not already set to ``Memory``, do so now.

* You can set SimulateTransactions to "true" if you want to experiment with `FHIR transactions <https://www.hl7.org/fhir/http.html#transaction>`_.
  The In-Memory implementation does not support real transactions, so in case of an error already processed entries will NOT be rolled back::

   "MemoryOptions": {
       "SimulateTransactions": "true"
   },


.. _configure_memory_admin:

Using the In-Memory storage for the Administration API database
---------------------------------------------------------------


.. warning::
    The current version of Firely Server (v5.5.0) does not support the use of In-Memory storage as administration database. This will be fixed in future versions of Firely Server. For now, it is adviced to use the SQLite backend instead.

This works the same as with the normal Firely Server database, except that you put the settings within the ``Administration`` section

E.g.::

   "Administration": {
       "Repository": "Memory",
       "MemoryOptions": {
           "SimulateTransactions": "false"
       }
   }

.. warning::

   Using the In-Memory storage for the administration layer will cause Firely Server to load the specification files on each startup.
   This takes several minutes, and Firely Server will respond with a '423 - Locked' error to all requests during that time. As of version
   0.7.1 we have implemented support for SQLite, which we recommend to use instead of the In-Memory storage. See :ref:`sqlite_admin_reasons`
   for more information.

