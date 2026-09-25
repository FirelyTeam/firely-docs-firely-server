.. _use_binaries:

=================================
Using Firely Server with Binaries
=================================

Firely Server can be deployed in a non-cloud, locally hosted environment using native binaries.
Afterwards it is possible to run Firely Server as described in the :ref:`Basic installation <vonk_basic_installation>` section.
The necessary files can be downloaded as zip files. All versions (incl. historic versions) can be accessed `here <https://downloads.fire.ly/firely-server/versions/>`_.

Please consider running a :ref:`reverse proxy <deploy_reverseProxy>` when running Firely Server natively.
Firely Server depends on the .NET platform and is therefore cross-platform in all supported `environments <https://github.com/dotnet/core/blob/main/release-notes/10.0/supported-os.md>`_.
As of Firely Server 6.9.0 the .NET 10 runtime is required; earlier releases (5.7.0 up to and including 6.8.x) require .NET 8.

For a production usage, Microsoft SQL Server or MongoDB need to be installed in the same environment in addition to Firely Server.

Firely Server should be registered as a startup / system service (Windows Service or systemd on Linux) for operational reliability.

.. _binaries_envvar:

Starting Firely Server with environment variables
-------------------------------------------------

If you provide (part of) the configuration as environment variables in a file, e.g. ``firely-server.env`` exported by the Guided Setup, you have to load these variables into the environment of the Firely Server process. Firely Server does not read the file itself. See :ref:`configure_envvar_file` for the format of the file.

The examples below assume that ``firely-server.env`` is in the Firely Server working directory.

Windows (PowerShell)
^^^^^^^^^^^^^^^^^^^^

Create a script ``start-firely-server.ps1`` in the working directory:

.. code-block:: powershell

   # Load all NAME=value lines from firely-server.env into the environment of this process
   Get-Content "$PSScriptRoot\firely-server.env" |
     Where-Object { $_ -match '^\s*[^#\s][^=]*=' } |
     ForEach-Object {
       $name, $value = $_ -split '=', 2
       [Environment]::SetEnvironmentVariable($name.Trim(), $value, 'Process')
     }

   Set-Location $PSScriptRoot
   dotnet .\Firely.Server.dll

Start Firely Server with:

.. code-block:: powershell

   powershell -ExecutionPolicy Bypass -File .\start-firely-server.ps1

Running the script with ``-File`` starts a new process, so the variables do not stay behind in your own PowerShell session.

Linux / macOS (bash)
^^^^^^^^^^^^^^^^^^^^

Run Firely Server in a subshell, so the variables are only set for Firely Server:

.. code-block:: bash

   (
     mapfile -t vars < <(grep -Ev '^\s*(#|$)' ./firely-server.env | tr -d '\r')
     exec env "${vars[@]}" dotnet ./Firely.Server.dll
   )

Each line of the file is passed to ``env`` as-is, so special characters in values (e.g. ``;``, ``$`` or ``!`` in a connection string) are not interpreted by the shell.
Avoid ``source firely-server.env`` / ``export $(cat firely-server.env)``: the shell then parses the values, which breaks on such characters, and on variable names that contain a ``.``, such as ``VONKLOG_Serilog__MinimumLevel__Override__Vonk.Configuration``.

Running as a service
^^^^^^^^^^^^^^^^^^^^

**systemd (Linux)**: reference the file from the unit file with ``EnvironmentFile``, for example in ``/etc/systemd/system/firely-server.service``:

.. code-block:: ini

   [Service]
   WorkingDirectory=/opt/firely-server
   EnvironmentFile=/opt/firely-server/firely-server.env
   ExecStart=/usr/bin/dotnet /opt/firely-server/Firely.Server.dll
   Restart=on-failure

systemd reads the ``EnvironmentFile`` again each time the service starts. If systemd rejects a line, it logs ``Ignoring invalid environment assignment`` in the journal (``journalctl -u firely-server``).

**Windows Service**: a Windows service does not pick up variables from your PowerShell session, and it only sees changed machine-wide environment variables after a reboot. Instead, store the variables on the service itself. Run this in an elevated PowerShell (replace ``FirelyServer`` with the name of your service):

.. code-block:: powershell

   $vars = Get-Content .\firely-server.env | Where-Object { $_ -match '^\s*[^#\s][^=]*=' }
   New-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Services\FirelyServer' `
     -Name Environment -PropertyType MultiString -Value $vars -Force

Restarting after a change to the env file
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Firely Server only reads environment variables at startup. After you change ``firely-server.env``:

#. Stop Firely Server (``Ctrl+C`` in the console window, or stop the service).
#. Load the variables again and start Firely Server:

   * **PowerShell / bash**: run the start command above again. Because the variables are set for the Firely Server process only, variables that you removed from the file are gone as well.
   * **systemd**: ``sudo systemctl restart firely-server``. You only need ``sudo systemctl daemon-reload`` if you changed the unit file itself, not for a change in the env file.
   * **Windows Service**: run the ``New-ItemProperty`` command above again, then ``Restart-Service FirelyServer``.

#. Check the startup log to verify the resulting configuration (see :ref:`configure_log`).

.. note::
   If you have set Firely Server variables in your user or system environment in the past (e.g. with ``setx`` or in the System Properties dialog), they are still applied in addition to the file. Remove them to avoid confusion about which value is used.

Minimal platform requirements
-----------------------------

Firely Server is a high-performance FHIR server designed for scalability and reliability. While it can be deployed in various environments, optimal performance depends on proper provisioning of resources.

**Deployment Options & Operating System:**
  
Firely Server is supported on all platforms supported by `.NET 10 <https://github.com/dotnet/core/blob/main/release-notes/10.0/supported-os.md>`_.
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