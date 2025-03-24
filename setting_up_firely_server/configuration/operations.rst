.. _configure_operations:

Operations Configuration
========================

Introduction
-----------

Firely Server 6.0 introduces a completely revamped operations configuration structure that provides more granular control over each operation. This new structure unifies previously scattered configuration settings from multiple sections into a cohesive and comprehensive model.

Key Benefits
^^^^^^^^^^^^

- **Unified Configuration**: All operation settings are now in one place
- **Granular Control**: Fine-grained control over individual operations
- **Explicit Configuration**: All configuration options are explicitly defined
- **Enhanced Security**: More detailed access control and authorization options

New Configuration Structure
---------------------------

The new configuration uses a top-level ``Operations`` section that contains operation configurations organized by operation name:

.. code-block:: json

    "Operations": {
      "$closure": {
        "Name": "$closure",
        "Level": [
          "System"
        ],
        "Enabled": true,
        "RequireAuthorization": "WhenAuthEnabled",
        "RequireTenant": "Never"
      },
      "capabilities": {
        "Name": "capabilities",
        "Level": [
          "System"
        ],
        "Enabled": true,
        "RequireAuthorization": "Never",
        "RequireTenant": "Never"
      },
      "create": {
        "Name": "create",
        "Level": [
          "Type"
        ],
        "Enabled": true,
        "RequireAuthorization": "WhenAuthEnabled",
        "RequireTenant": "WhenTenancyEnabled"
      }
    }

For administrative operations, a similar structure exists under ``Administration.Operations``:

.. code-block:: json

    "Administration": {
      "Operations": {
        "$reindex": {
          "Name": "$reindex",
          "Level": [
            "System"
          ],
          "Enabled": true,
          "NetworkProtected": true
        },
        "$reset": {
          "Name": "$reset",
          "Level": [
            "System"
          ],
          "Enabled": true,
          "NetworkProtected": true
        }
      }
    }

Configuration Properties
-----------------------

Each operation can be configured with the following properties:

.. list-table::
   :header-rows: 1
   :widths: 20 15 50 15

   * - Property
     - Type
     - Description
     - Availability
   * - ``Name``
     - string
     - The operation name, matching the key in the Operations dictionary
     - Regular & Admin
   * - ``Level``
     - array of strings
     - The level(s) at which the operation is available: "System", "Type", and/or "Instance"
     - Regular & Admin
   * - ``Enabled``
     - boolean
     - Whether the operation is enabled
     - Regular & Admin
   * - ``RequireAuthorization``
     - string
     - Authorization requirement: "WhenAuthEnabled", "Always", or "Never"
     - Regular only
   * - ``OperationScope``
     - string
     - Required token scope for the operation (only applies when authorization is enabled)
     - Regular only
   * - ``NetworkProtected``
     - boolean
     - Whether the operation is restricted to allowed networks
     - Admin only
   * - ``RequireTenant``
     - string
     - Tenant requirement: "WhenTenancyEnabled", "Always", or "Never"
     - Regular only

Migration from Previous Configuration
------------------------------------

The new configuration structure replaces several previous configuration sections. Here's how to migrate your existing configuration:

1. SupportedInteractions Section
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

**Before (v5.x):**

.. code-block:: json

    "SupportedInteractions": {
      "InstanceLevelInteractions": "read, vread, update, delete, history, conditional_delete, conditional_update, $validate",
      "TypeLevelInteractions": "create, search, history, $validate, $snapshot, conditional_create",
      "WholeSystemInteractions": "capabilities, batch, transaction, history, search, $validate"
    }

**After (v6.x):**

For each operation, create an entry in the ``Operations`` section with appropriate settings. For standard operations, these are provided by default.

2. Administration Security OperationsToBeSecured
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

**Before (v5.x):**

.. code-block:: json

    "Administration": {
      "Security": {
        "AllowedNetworks": ["127.0.0.1", "::1"],
        "OperationsToBeSecured": ["reindex", "reset", "preload", "importResources"]
      }
    }

**After (v6.x):**

For each operation in ``OperationsToBeSecured``, set ``NetworkProtected`` to ``true`` in the corresponding operation configuration:

.. code-block:: json

    "Administration": {
      "AllowedNetworks": ["127.0.0.1", "::1"],
      "Operations": {
        "reindex": {
          "Name": "reindex",
          "Level": ["System"],
          "Enabled": true,
          "NetworkProtected": true
        },
        // other operations...
      }
    }

3. SmartAuthorizationOptions Protected
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

**Before (v5.x):**

.. code-block:: json

    "SmartAuthorizationOptions": {
      "Protected": {
        "Resource": ["Patient", "Observation"],
        "Operation": ["$lastn", "$everything"]
      }
    }

**After (v6.x):**

For each operation in ``SmartAuthorizationOptions.Protected.Operation``, set ``RequireAuthorization`` to ``"WhenAuthEnabled"`` or ``"Always"`` in the corresponding operation configuration:

.. code-block:: json

    "Operations": {
      "$lastn": {
        "Name": "$lastn",
        "Level": ["Type", "Instance"],
        "Enabled": true,
        "RequireAuthorization": "Always",
        "NetworkProtected": false,
        "RequireTenant": "WhenTenancyEnabled"
      },
      "$everything": {
        "Name": "$everything",
        "Level": ["Instance"],
        "Enabled": true,
        "RequireAuthorization": "Always",
        "NetworkProtected": false,
        "RequireTenant": "WhenTenancyEnabled"
      }
    }

Operation Configuration Options
------------------------------

Authorization Options
^^^^^^^^^^^^^^^^^^^^

The ``RequireAuthorization`` property has three possible values:

1. ``"WhenAuthEnabled"`` (Default): Authorization is required only when authorization is enabled in Firely Server
2. ``"Always"``: Authorization is always required, server start is prevented when authorization is disabled
3. ``"Never"``: Authorization is never required, even if server authorization is enabled

This property is only configurable for standard FHIR operations under the main ``Operations`` section. Administrative operations have fixed authorization behavior that cannot be changed.

Operation Scope
^^^^^^^^^^^^^^

The ``OperationScope`` property defines the required token scope for an operation. This setting only applies when authorization is enabled in Firely Server.

* If you do not provide a scope, the access token will not need to include any specific scope to perform this operation
* If you provide a scope, the access token must include that scope to perform this operation
* For standard scopes, refer to the SMART on FHIR scopes documentation (e.g., patient/Patient.read, user/Observation.write)

For example, if you configure an operation with ``"OperationScope": "http://server.fire.ly/auth/scope/erase-operation"``, then any access token used to access this operation must include the "http://server.fire.ly/auth/scope/erase-operation" scope.

Network Protection Options
^^^^^^^^^^^^^^^^^^^^^^^^^

The ``NetworkProtected`` property controls access restrictions based on IP networks:

1. ``true``: The operation can only be accessed from networks defined in the ``Administration.AllowedNetworks`` configuration
2. ``false`` (Default): The operation can be accessed from any network

Important: This property is only applicable to administrative operations (under the ``Administration.Operations`` section). It cannot be used with standard FHIR operations and is specifically designed to restrict sensitive administrative operations to specific IP networks.

Multi-tenancy Options
^^^^^^^^^^^^^^^^^^^^

The ``RequireTenant`` property controls whether an operation requires tenant information with three possible values:

1. ``"WhenTenancyEnabled"`` (Default): The operation requires tenant information only when VirtualMultitenancy is enabled
2. ``"Always"``: The operation always requires tenant information; server start is prevented when VirtualMultitenancy is disabled
3. ``"Never"``: The operation never requires tenant information, even if VirtualMultitenancy is enabled

When VirtualMultitenancy is enabled:
- Operations with ``RequireTenant: "WhenTenancyEnabled"`` will require a tenant to be specified in the request
- Operations with ``RequireTenant: "Always"`` will require a tenant to be specified in the request
- Operations with ``RequireTenant: "Never"`` will work without a tenant specification

When VirtualMultitenancy is disabled:
- Operations with ``RequireTenant: "WhenTenancyEnabled"`` will work without tenant information
- Operations with ``RequireTenant: "Always"`` will prevent Firely Server from starting
- Operations with ``RequireTenant: "Never"`` will work without tenant information

This property is only applicable to standard FHIR operations (under the main ``Operations`` section). Administrative operations do not support this property as they operate at the system level across all tenants.

Example Configuration
-------------------

Here's an example of the new operation configuration structure:

.. code-block:: json

    {
      "Operations": {
        "$closure": {
          "Name": "$closure",
          "Level": ["System"],
          "Enabled": true,
          "RequireAuthorization": "WhenAuthEnabled",
          "NetworkProtected": false,
          "RequireTenant": "Never"
        },
        "capabilities": {
          "Name": "capabilities",
          "Level": ["System"],
          "Enabled": true,
          "RequireAuthorization": "Never",
          "NetworkProtected": false,
          "RequireTenant": "Never"
        },
        "create": {
          "Name": "create",
          "Level": ["Type"],
          "Enabled": true,
          "RequireAuthorization": "WhenAuthEnabled",
          "NetworkProtected": false,
          "RequireTenant": "WhenTenancyEnabled"
        },
        "$validate": {
          "Name": "$validate",
          "Level": ["System", "Type", "Instance"],
          "Enabled": true,
          "RequireAuthorization": "WhenAuthEnabled",
          "NetworkProtected": false,
          "RequireTenant": "WhenTenancyEnabled",
          "OperationScope": "validation"
        }
      },
      "Administration": {
        "AllowedNetworks": ["127.0.0.1", "::1"],
        "Operations": {
          "$reindex": {
            "Name": "$reindex",
            "Level": ["System"],
            "Enabled": true,
            "NetworkProtected": true
          },
          "$reset": {
            "Name": "$reset",
            "Level": ["System"],
            "Enabled": true,
            "NetworkProtected": true
          }
        }
      }
    }

Custom Operations
---------------

For custom operations, you need to explicitly add them to the ``Operations`` section with all required properties. Core operations like read, create, update, etc. are enabled by default, but custom operations must be explicitly configured.

.. code-block:: json

    "Operations": {
      "$myCustomOperation": {
        "Name": "$myCustomOperation",
        "Level": ["Type"],
        "Enabled": true,
        "RequireAuthorization": "WhenAuthEnabled",
        "NetworkProtected": false,
        "RequireTenant": "WhenTenancyEnabled",
        "OperationScope": "custom-operation"
      }
    }