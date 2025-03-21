.. _use_binaries:

=================================
Using Firely Server with Binaries
=================================

Firely Server can be deployed in a non-cloud, locally hosted environment using native binaries.
Afterwards it is possible to run Firely Server as described in the :ref:`Basic installation <vonk_basic_installation>` section.
The necessary files can be downloaded as zip files. All versions (incl. historic versions) can be accessed `here <https://downloads.fire.ly/firely-server/versions/>`_.

Please consider running a :rev:`reverse proxy <deploy_reverseProxy>` when running Firely Server natively.
Firely Server depends on the .NET Core platform and is therefore cross-platform in all supported `environments <https://github.com/dotnet/core/blob/main/release-notes/8.0/supported-os.md>`_.

For a production usage, Microsoft SQL Server or MongoDB need to be installed locally in addition to Firely Server.

Firely Server should be registered as a startup / system service ((Windows Service or systemd on Linux) for operational reliability.

See :ref:`vonk_performance` for some considerations around choosing the right amount of system resources (i.e. available memory, CPUs).
