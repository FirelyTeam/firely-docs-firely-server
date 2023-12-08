.. _firely_auth_deploy:

Deployment
==========

License
-------

You can use your Firely Server license file, provided that it contains the license token for Firely Auth: ``http://fire.ly/server/auth``.
This token is included in the evaluation license for Firely Server, and in your production license if Firely Auth is included in your order.

Configure the path to the license file in the appsettings, section :ref:`firely_auth_settings_license`.

.. _firely_auth_deploy_exe:

Executable / binaries
---------------------

You can request a copy of the binaries from Firely through our :ref:`contact page <vonk-contact>`

.. You can download the binaries in a zip file from `the downloadserver <https://downloads.simplifier.net/firely-server/firely-auth-latest.zip>`_

.. _firely_auth_deploy_docker:

Docker image
------------

A Docker image is available on the Docker hub, under `firely/auth`. You can spin up a Docker container for Firely Auth using the following command::

  docker run -d -p5100:5100 --name firely.auth -v %CD%/firely-auth-license.json:/app/firely-auth-license.json -v %CD%/appsettings.instance.json:/app/appsettings.instance.json firely/auth:latest

Make sure to include the ``firely-auth-license.json`` and ``appsettings.instance.json`` in your working directory. For deployments in Azure or AWS, it is necessary to provide the ``*.instance.json`` files in a separate folder.
This works the same as with :ref:`configure_settings_path` for Firely Server, but here you use the environment variable ``FIRELY_AUTH_PATH_TO_SETTINGS``.
If you want to spin up a docker image with this environment variable, you can for instance use the following command::

  docker run -d -p5100:5100 --name firely.auth -v /${PWD}/firely-auth-license.json:/app/firely-auth-license.json -v /${PWD}/config:/app/config -e FIRELY_AUTH_PATH_TO_SETTINGS=/app/config firely/auth:latest

The Docker container has a network of its own. This means that localhost within a Docker container resolves to a different network than localhost on your local computer.
To make sure Firely Auth is communicating correctly with Firely Server, some adjustments need to be made to support the use cases described below.

Running Firely Auth in Docker with Firely Server running locally
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

When running Firely Auth in Docker and Firely Server locally, the communication between these services will run from the local system to the Docker container. 
To make sure Firely Auth is listening to your local network for communication from Firely Server it is necessary to adjust the ``Kestrel`` settings to point to your local network.
You can do so by pointing Kestrel in the Firely Auth ``appsettings.instance.json`` to ``host.docker.internal``, rather than ``localhost``::

  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://host.docker.internal:5100"
        }
      }
    }

In the Firely Auth ``appsettings.instance.json`` you need to point the ``FHIR_BASE_URL`` to your local Firely Server at ``localhost:4080``::

    "FhirServer": {
    "Name": "Firely Server",
    "FHIR_BASE_URL": "http://localhost:4080",
  },

In the Firely Server ``appsettings.instance.json`` you can set the ``Authority`` setting as shown below::

  "Authority": "http://localhost:5100",

Running Firely Auth locally with Firely Server running in Docker
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

If you want to run Firely Auth locally with Firely Server in Docker you need to make the following changes. 
First in your Firely Auth ``appsettings.instance.json`` point Kestrel to your localhost::

    "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5100"
      }
    }
  },

Next point the ``FHIR_BASE_URL`` in your Firely Auth ``appsettings.instance.json`` to Firely Server running in Docker::

    "FhirServer": {
    "Name": "Firely Server",
    "FHIR_BASE_URL": "http://localhost:8080",

Lastly, in the Firely Server ``appsettings.instance.json`` point the ``Authority`` setting to the Firely Auth service running on your local system, and point the ``AdditionalIssuersInToken`` to localhost::

        "Authority": "http://host.docker.internal:5100",
        "AdditionalIssuersInToken": ["http://localhost:5100"],


Running Firely Auth in Docker together with Firely Server 
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

In case you want to run both Firely Auth and Firely Server in Docker, you need to make sure both services are able to communicate via the same network in Docker.
For this, you can create a Docker network::

  docker network create internal-container-network 
  
  
In the Firely Auth ``appsettings.instance.json`` adjust the ``FHIR_BASE_URL`` to point to the Firely Server service running in Docker. As all communication from the Firely Server service to the Firely Auth service goes via the Docker network, you can also use localhost here::

    "FhirServer": {
    "Name": "Firely Server", 
    "FHIR_BASE_URL": "http://localhost:8080",

Alternatively, you can adjust this as follows::

    "FhirServer": {
    "Name": "Firely Server",
    "FHIR_BASE_URL": "http://firely.server:8080",

Adjust the ``Kestrel`` settings in the Firely Auth ``appsettings.instance.json`` as follows::

   "Kestrel": {
   "Endpoints": {
     "Http": {
       "Url": "http://firely.auth:5100"
     } 

In the Firely Server ``appsettings.instance.json`` point the ``Authority`` setting to the Firely Auth service in Docker::

  "Authority": "http://firely.auth:5100",

Next, spin up both services to use the dDcker network you created earlier::

  docker run -d -p5100:5100 --name firely.auth -v %CD%/firely-auth-license.json:/app/firely-auth-license.json -v %CD%/appsettings.instance.json:/app/appsettings.instance.json --network internal-container-network firely/auth:latest
  
  docker run -d -p8080:4080 --name firely.server -v %CD%/firelyserver-license.json:/app/firelyserver-license.json -v %CD%/appsettings.instance.json:/app/appsettings.instance.json --network internal-container-network firely/server:latest

If you want to check with your local postman if this setup works, you need to add the following to the ``AdditionalIssuersInToken`` setting in the Firely Server ``appsettings.instance.json``::

   "AdditionalIssuersInToken": ["http://localhost:5100"],

See the instructions on :ref:`running Firely Server in Docker <use_docker>` to learn about adjusting settings and providing the license file.
Firely Auth is configured in the same way.




.. _firely_auth_deploy_inmemory:

InMemory user store
-------------------

The InMemory user store is only meant for testing your setup or evaluating Firely Auth.
For production use configure the SQL Server user store.

The users for the InMemory user store can be configured in :ref:`firely_auth_settings_userstore`

.. _firely_auth_deploy_sql:

SQL Server user store
---------------------

Use of the SQL Server user store requires Microsoft SQL Server version 2016 or newer.

Using your favorite database administration tool:

- create a new database, e.g. 'firely_auth_store'
- in this database, execute the script ``scripts/InitializeSchema.sql``, available in the binaries
- create a connection string to this database
- configure :ref:`firely_auth_settings_userstore`
  
  .. code-block:: json

    {
      "Type": "SqlServer",
      "SqlServer": {
        "ConnectionString": "<connectionstring from previous step>"
      }
    }

In the connection string you can use a user that is only allowed to read and write from the existing tables, no further DDL is needed.

To add users to the store, you can use the :ref:`firely_auth_mgmt`.


Using Firely Auth behind a proxy or load balancer
-------------------------------------------------

Firely Auth issues a series of Cookies with the property ``samesite=none``, in particular 
the cookie ``.AspNetCore.Identity.Application`` from ASP.NET Core Identity.

When using a proxy, the TLS connection might end at the proxy level and hence, the last leg 
of the request is over ``HTTP`` and not ``HTTPS``. If nothing is done, this means that the Cookies
issues by Firely Auth will not have the propery ``secure`` set, and depending on the browser 
setup, it might refuses a cookie with  but without the ``secure`` flag, issuing an error like:

    .. code-block::
    
      The cookie '".AspNetCore.Identity.Application"' has set 'SameSite=None' and must also set 'Secure'.

In order to avoid this issue, you need to ensure that the 
`forwarded headers <https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-7.0#forwarded-headers>`_ 
are properly set by the proxy/load balancer so that the 
`ForwardedHeaders middleware <https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.httpoverrides.forwardedheadersmiddleware>`_ 
can retrieved the values of the public endpoint, allowing other middlewares to return the appropriate values, including 
the ``secure`` property of the cookies.


