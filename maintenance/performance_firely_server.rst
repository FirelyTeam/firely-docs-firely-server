.. _vonk_performance:

Performance of Firely Server
============================

About Performance
-----------------

What is the performance of Firely Server? That is a simple question, but unfortunately it has no answer. Or more precisely, the answer depends on many variables. On this page we try to give you insight into those variables, introduce you to testing performance yourself, and finally present the results of the tests that we run ourselves.

Performance variables
---------------------

Firely Server Configuration
^^^^^^^^^^^^^^^^^^^^^^^^^^^
Firely Server can be run as self contained FHIR Server or as a Facade on top of an existing system. The performance of a Facade is heavily dependent on the performance of the system it is built on top of. The self contained server can run on different databases. On top of that you can configure a couple of features in the settings that influence the performance. These are the most important configuration variables to take into account:

Repository 
~~~~~~~~~~

#. Memory: Memory is only meant for quick tests, and for use in unittests. Do not use it in any serious scenario, much less for performance critical scenarios.
#. SQLite: SQLite is mainly used for the Administration database of Firely Server, but you can also use it for the main database. Deployment is very easy because of the zero footprint of the driver, but be aware of its limits. Firely Server must have very fast access to the database file, so effectively it has to be on a local disk. Multithreading does work, but does not scale as well as other databases.
#. SQL Server: Performance tuning of SQL Server is a topic on its own. Firely Server manages the tables it needs, and the indexes on top of it are optimized for the way Firely Server queries them. See :ref:`performance_sqlserver_guidelines` for more details about SQL Server database maintenance and tuning.
#. MongoDB: Performance tuning of MongoDB is, as well, a topic on its own. Firely Server manages the collections it needs, and the indexes on top of it are optimized for the way Firely Server queries them. MongoDB is used in our own performance tests, see below.

Prevalidation
~~~~~~~~~~~~~

By default, a resource that is sent to Firely Server is fully validated against its StructureDefinition. This requires extra processing and thus extra time. But you can disable full validation if needed, with the :ref:`ValidateIncomingResources <feature_prevalidation>` setting. We have no tests in place yet to time the difference caused by this setting.

Search Parameters
~~~~~~~~~~~~~~~~~

When a resource is sent to Firely Server for storage, Firely Server indexes the resource for all the search parameters that are applicable to the resource. The more search parameters are known to Firely Server, the more resources will be used for indexing - both time (for the indexing processing) and storage (for storing the values of each search parameter). This also increases the size of the index tables and indexes, and therefore querying times. Thus, if you know you will only use a portion of the predefined search parameters you can choose to delete the others from the Administration API - see :ref:`conformance` and :ref:`supportedmodel`.

Additionally, these search parameters can have an impact on the search performance:

#. ``_total``: Searchbundles contain the element ``total`` which indicates the total number of resources that match the query's search parameters. Setting this parameter to ``_total = none`` results in faster searches as it saves the query that would generate the result of the aforementioned element.


Pipeline
~~~~~~~~

Firely Server is made up of a pipeline of plugins. You can leave out any plugin that you don't need - so if you don't need conditional processing (create, update, delete), just exclude them from the pipeline. Excluded plugins are not loaded and thus never executed - see :ref:`settings_pipeline`.

Platform
^^^^^^^^

Firely Server can run on Windows, Linux and MacOS. Directly or in a Docker container. On real hardware, virtual machine, app service or Kubernetes cluster. And then you can choose the dimensions of the platform, scaling up (bigger 'hard'ware) or scaling out (more (virtual) machines) as you see fit. Each of these choices will influence performance.

Besides the way Firely Server is deployed, the way the database is deployed is an important factor. Firely Server needs a very low latency connection between the Firely Server process(es) and the database server. If you have configured :ref:`configure_log_insights`, the calls to the database are recorded as separate dependencies so you can check whether this may be a bottleneck.

Firely Server is optimized for multithreaded processing. This means that it will fully benefit from extra processing cores, either in the same machine (multi core processor) or by adding additional machines (and thus processors). 

Firely Server is fully stateless, so no data about connections is saved between requests. First of all this helps in scaling out, since you don't need thread affinity. On top of that this reduces the memory requirements of Firely Server.

Usage patterns
^^^^^^^^^^^^^^

How will Firely Server be used in your environment? 

#. Mostly for querying, or rather for creating and updating resources?
   Altering resources requires more processing than reading them. Also see the comment on indexing and search parameters above.
#. How is the distribution of values in the resources that you query on?
   E.g. if you use only a few types of resources, query them just by tag and the resources have only about 5 different tags, calculating the number of results will take a lot of time. Using more finegrained distributed values to query on solves this.
#. With many individual resources or with (large) batches or transactions?
   Transactions take a lot longer to process and require more memory, proportionally to the number of resources in them. If many transactions are run in parallel, requests may queue up. 
#. Many users with a low request rate each, or a few heavy users? 
   Since Firely Server is stateless, this has little influence. The total request rate is what counts. 

Testing performance yourself
----------------------------

Because of all the variables mentioned above the best way to find out whether Firely Server's performance is sufficient for your use is: test it yourself.

We provide an evaluation license that you can use for any testing, including performance testing. To obtain the evaluation license you can `sign up <https://fire.ly/firely-server-trial/>`_.

Variables
^^^^^^^^^

Before you start testing, study the variables above and provide answers to them. Then you can configure your platform and your tests in a way that comes closest to the expected real use.

Requests
^^^^^^^^

You need a set of requests that you want to test. Based on your use case, identify the 5 (or more) most frequent requests. For extra realism you should provide the parameters to the requests from a dataset (like a .csv file with search parameter values).  

What to measure?
^^^^^^^^^^^^^^^^

There are essentially two questions that you can investigate:

#. Given this deployment, (mix of) requests and an expected request rate, what are the response times?
#. Given this deployment and (a mix of) requests, how many requests can Firely Server handle before it starts returning time-outs?

Besides response times more insight can be gained by measuring the load on the server (processor / memory usage, disk and network latency, for both the Firely Server and the database server) as well as the machine you are generating the requests from (to ensure that is not bottlenecked).

Always make sure to use at least 2 separate machines for testing: one for Firely Server, and a separate one for generating the requests. Testing Firely Server on the same machine as you're generating requests from will make Firely Server compete with the load testing tool for resources which'll hamper the legitimacy of the test results.

Based on the answers you can retry with different parameters (e.g. add/remove hardware) to get a sense of the requirements for real use deployment.

Data
^^^^

Performance testing is best done with data as realistic to your situation as possible. So if you happen to have historic data that you can transform to FHIR resources, that is the best data to test with.

But if you don't have data of your own, you can use synthesized data. We use data from the Synthea project for our own tests. And we provide :ref:`tool_fsi` to upload the collection bundles from Synthea to Firely Server (or any FHIR Server for that matter). 

If you build a Facade, the historical data is probably already in a test environment of the system you build the Facade on. That is a perfect start.

Test framework
^^^^^^^^^^^^^^

To run performance tests you need a framework to send the requests in parallel and measure the response times. Test automation is a profession in itself so we cannot go into much detail here. You can search for 'REST Performance test tools' to get some options.

Available performance figures
-----------------------------

We are in the process of setting up performance tests as part of our Continuous Integration and Deployment. Here we describe how this test is currently set up. Because of the beta phase this is in, the output is not yet complete nor fully reliable. Nevertheless we share the preliminary results to give you a first insight.

Firely Server performance test setup
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

#. Configuration

   #. Repository: MongoDB, both for Administration and for the main database.
   #. Prevalidation: off
   #. Search parameters: support all types of resources and all search parameters from the FHIR specification.
   #. Pipeline: load all available plugins except authorization.

#. Platform

   #. Azure Kubernetes Service, 2 nodes.
   #. Each node: Standard F2s (2 vcpus, 4 GB memory), running Linux
   #. 1 MongoDB pod and 2 Firely Server pods, plus the Kubernetes manager

#. Usage pattern - we created a simple mix of requests

   #. Upload the first 100 Synthea bundles from the precalculated set, each collection bundle transformed to a Batch.
   #. A 'general' test, consisting of:

      #. Query Patient by name: ``GET {url}/Patient?name=...``
      #. Query Patient by name and maximum age: ``GET {url}/Patient?name={name}&birthdate=ge{year}``
      #. Query all Conditions: ``GET {url}/Condition``
      #. Query a Patient by identifier, with Observations: ``GET {url}/Patient?identifier={some identifier}&_revinclude=Observation:subject``
      #. Query a Patient by identifier, with Observations and DiagnosticReports: ``GET {url}/Patient?identifier={some identifier}&_revinclude=Observation:subject&_revinclude=DiagnosticReport:patient``

   #. Page through all the CarePlan resources: ``GET {url}/CarePlan?_count=10``, and follow ``next`` links.
   #. Page through 1/5 of the Patient resources and delete them: ``DELETE {url}/Patient/{id}``
   #. 20 concurrent users, randomly waiting up to 1 second before issuing the next request. 
   #. Test run of 5 minutes

#. Test framework

   #. Locust for defining and running tests
   #. Telegraf agents for collection metrics
   #. InfluxDB for storing results
   #. Grafana for displaying results

Test results
^^^^^^^^^^^^

#. Upload: not properly timed yet.
#. General test: 75 percentile of response times around 200 ms.
   Note that the responses on queries with '_revinclude' contain over 30 resources on average, sometimes over 100.
#. Page through all CarePlan resources: 75 percentile of response times around 110 ms.
#. Delete patients: This test always runs with 40 concurrent users, and 75 percentile of response times are around 350ms.
   Note that in Firely Server a delete is essentially an update, since all old versions are retained. 

