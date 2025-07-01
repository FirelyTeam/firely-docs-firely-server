.. _use_binaries:

=================================
Using Firely Server with Binaries
=================================

Firely Server can be deployed in a non-cloud, locally hosted environment using native binaries.
Afterwards it is possible to run Firely Server as described in the :ref:`Basic installation <vonk_basic_installation>` section.
The necessary files can be downloaded as zip files. All versions (incl. historic versions) can be accessed `here <https://downloads.fire.ly/firely-server/versions/>`_.

Please consider running a :ref:`reverse proxy <deploy_reverseProxy>` when running Firely Server natively.
Firely Server depends on the .NET Core platform and is therefore cross-platform in all supported `environments <https://github.com/dotnet/core/blob/main/release-notes/8.0/supported-os.md>`_.

For a production usage, Microsoft SQL Server or MongoDB need to be installed in the same environment in addition to Firely Server.

Firely Server should be registered as a startup / system service (Windows Service or systemd on Linux) for operational reliability.

Minimal platform requirements
-----------------------------

Firely Server is a high-performance FHIR server designed for scalability and reliability. While it can be deployed in various environments, optimal performance depends on proper provisioning of resources.

**Deployment Options & Operating System:**
  
Firely Server is supported on all platforms supported by the `.NET framework <https://github.com/dotnet/core/blob/main/release-notes/8.0/supported-os.md>`_.
In practice, the choice of operating system should align with your team's operational expertise and familiarity with the platform.
There are no limitations regarding any hypervisor being used when using a virtual machine instead of a physical server.

**Memory (RAM)**

- Minimum: 2 GB
- Recommended: 16 GB or more, particularly when working with large datasets. Firely Server caches definitional FHIR artifacts (e.g., StructureDefinitions, ValueSets) in memory to enhance performance.

**CPU**

- Minimum: 2-core X86-64 CPU
- Recommended: at least 4-core X86-64 CPU
- Note: More cores benefit concurrent request handling

**Disk Space**

- Minimum: 1.5 GB of free disk space for the raw installation of Firely Server
- Recommended: The recommendation depends on the selected production database and expected numbers of resources. MongoDB uses ~4GB for 1 million resources. SQL Server uses ~34GB for 1 million resources.
- Note: SSD storage is strongly recommended for optimal I/O performance.

See :ref:`vonk_performance` for additional considerations around choosing the right amount of system resources when scaling the server (i.e. available memory, CPUs).