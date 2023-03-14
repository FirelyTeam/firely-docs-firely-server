:orphan:

.. Part of reverseProxy.rst

.. _nginx:

======================================
Deploy Firely Server on Nginx on Linux
======================================

About Nginx
-----------

NGINX is a popular open source web server. It can act as a reverse proxy server for TCP, UDP, HTTP, HTTPS, SMTP, POP3, 
and IMAP protocols, as well as a load balancer and a HTTP cache.
You can find the documention for the Nginx server at https://nginx.org/en/docs/.

Prerequisites
-------------

#. The following linux distribution are supported: Ubuntu, RHEL, Debian, Fedora, CentOS, SLES 

#. Install .Net Core on the machine (see https://www.microsoft.com/net/learn/get-started/linuxubuntu)

#. Install Nginx  :code:`sudo apt-get install nginx`

Start Kestrel Firely Server
---------------------------

Download the binaries for Firely Server (see :ref:`vonk_getting_started`), open a terminal console and start the Firely Server process by using:
dotnet Vonk.Server.dll.
You should be able to reach to home page at http://localhost:4080 (or a different port if you changed the default configurations)

Configure Nginx as a reverse proxy
----------------------------------

To configure Nginx as a reverse proxy to forward requests to our ASP.NET Core application, modify /etc/nginx/sites-available/default. 
Open it in a text editor, and replace the contents with the following:

.. code-block:: bash

    server {
        listen 80;
        # Match incoming requests with the following path and forward them to 
        # the location of the Kestrel server.
        # See http://nginx.org/en/docs/http/ngx_http_core_module.html#location
        location / {
            #This should match the location where you deployed the Firely Server binaries with the Kestrel server.
            #This can be on the same machine as the Nginx server or on a separate dedicated machine
            proxy_pass http://localhost:4080;
            # The Kestrel web server we are forwarding requests to only speaks HTTP 1.1.
            proxy_http_version 1.1;
            proxy_set_header Upgrade $http_upgrade;
            # Adds the 'Connection: keep-alive' HTTP header.
            proxy_set_header Connection keep-alive;
            # Forwards the Host HTTP header.
            proxy_set_header Host $host;
            proxy_cache_bypass $http_upgrade;
        }
    }

Now you can run the Firely Server.

Configuration
-------------

- To configure the Firely Server, you can use the appsettings.json file (see :ref:`configure_vonk`).

- To configure Nginx you need to add extra options to the /etc/nginx/sites-available/default or to the nginx.conf file.

- To monitor the application you can use systemd and create a service for starting, stopping and managing the process.