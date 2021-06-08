Debugging the Facade
====================

* Start your Firely Server

  .. note::
    If this is your first startup of Firely Server, it will take a while to load in all of the specification files.

* You can inspect the console log to see if the pipeline is configured to include your repository.
  See :ref:`vonk_plugins_log_detail` for more details.

* To test your Facade, open Postman, or Fiddler, or use curl to request ``GET http://localhost:4080/metadata``

  The resulting CapabilityStatement should list only the Patient resource type in its .rest.resource field,
  and -- among others -- the _id search parameter in the .rest.searchParam field.

* Now you can test that searching patients by ``_id`` works: ``GET http://localhost:4080/Patient?_id=1``
  Requesting the resource 'normally' should automatically work as well: ``GET http://localhost:4080/Patient/1``

.. important::
   If it works, congratulations! You now have a Firely Server Facade running!

Testing during implementation
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Follow these steps if you want to test your work during the implementation phase without having to build, copy and start Firely Server each time,
or with the ability to set break points in your code and debugging it:

* In the project properties, click on the ``Build`` tab.
* Set the ``Output path`` to your Firely Server plugins directory.
* Go to the ``Debug`` tab and set ``Launch`` to ``Executable``.
* Point the ``Executable`` field to your dotnet.exe.
* Set the ``Application arguments`` to ``<your-Firely-Server-working-directory>/Firely.Server.dll``.
* Set the ``Working directory`` to your Firely Server working directory.

Now, whenever you click to start debugging, Firely Server will start from your project and your project dll will be automatically
built to the Firely Server plugins directory.

Next part of the exercise
-------------------------
You can proceed to the next section to add support for Observations as well.
