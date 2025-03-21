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
        "NetworkProtected": false,
        "RequireTenant": false
      },
      "capabilities": {
        "Name": "capabilities",
        "Level": [
          "System"
        ],
        "Enabled": true,
        "RequireAuthorization": "Never",
        "NetworkProtected": false,
        "RequireTenant": false
      },
      "create": {
        "Name": "create",
        "Level": [
          "Type"
        ],
        "Enabled": true,
        "RequireAuthorization": "WhenAuthEnabled",
        "NetworkProtected": false,
        "RequireTenant": true
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

+------------------------+----------------------------+---------------------------------------------------------------------------------------------------+
| Property               | Type                       | Description                                                                                       |
+========================+============================+===================================================================================================+
| ``Name``               | string                     | The operation name, matching the key in the Operations dictionary                                 |
+------------------------+----------------------------+---------------------------------------------------------------------------------------------------+
| ``Level``              | array of strings           | The level(s) at which the operation is available: "System", "Type", and/or "Instance"            |
+------------------------+----------------------------+---------------------------------------------------------------------------------------------------+
| ``Enabled``            | boolean                    | Whether the operation is enabled                                                                  |
+------------------------+----------------------------+---------------------------------------------------------------------------------------------------+
| ``RequireAuthorization``| string                    | Authorization requirement: "WhenAuthEnabled", "Always", or "Never"                               |
+------------------------+----------------------------+---------------------------------------------------------------------------------------------------+
| ``OperationScope``     | string                     | Required token scope for the operation (only applies when authorization is enabled)               |
+------------------------+----------------------------+---------------------------------------------------------------------------------------------------+
| ``NetworkProtected``   | boolean                    | Whether the operation is restricted to allowed networks                                           |
+------------------------+----------------------------+---------------------------------------------------------------------------------------------------+
| ``RequireTenant``      | boolean                    | Whether the operation requires tenant information                                                 |
+------------------------+----------------------------+---------------------------------------------------------------------------------------------------+

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
        "RequireTenant": true
      },
      "$everything": {
        "Name": "$everything",
        "Level": ["Instance"],
        "Enabled": true,
        "RequireAuthorization": "Always",
        "NetworkProtected": false,
        "RequireTenant": true
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

The ``RequireTenant`` property determines whether an operation requires tenant information:

1. ``true``: The operation requires tenant information and will only work in a multi-tenant environment
2. ``false``: The operation does not require tenant information and works in both single and multi-tenant environments

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
          "RequireTenant": false
        },
        "capabilities": {
          "Name": "capabilities",
          "Level": ["System"],
          "Enabled": true,
          "RequireAuthorization": "Never",
          "NetworkProtected": false,
          "RequireTenant": false
        },
        "create": {
          "Name": "create",
          "Level": ["Type"],
          "Enabled": true,
          "RequireAuthorization": "WhenAuthEnabled",
          "NetworkProtected": false,
          "RequireTenant": true
        },
        "$validate": {
          "Name": "$validate",
          "Level": ["System", "Type", "Instance"],
          "Enabled": true,
          "RequireAuthorization": "WhenAuthEnabled",
          "NetworkProtected": false,
          "RequireTenant": true,
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
        "RequireTenant": true,
        "OperationScope": "custom-operation"
      }
    }