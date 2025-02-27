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
          "RequireAuthorization": "WhenAuthEnabled",
          "NetworkProtected": true,
          "RequireTenant": false
        },
        "$reset": {
          "Name": "$reset",
          "Level": [
            "System"
          ],
          "Enabled": true,
          "RequireAuthorization": "WhenAuthEnabled",
          "NetworkProtected": true,
          "RequireTenant": false
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
| ``OperationScope``     | string                     | Required token scope for the operation (optional)                                                 |
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
          "RequireAuthorization": "WhenAuthEnabled",
          "NetworkProtected": true,
          "RequireTenant": false
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

Operation Authorization Options
------------------------------

The ``RequireAuthorization`` property has three possible values:

1. ``"WhenAuthEnabled"`` (Default): Authorization is required only when authorization is enabled in Firely Server
2. ``"Always"``: Authorization is always required, server start is prevented when Smart is disabled
3. ``"Never"``: Authorization is never required, even if server authorization is enabled

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
            "RequireAuthorization": "WhenAuthEnabled",
            "NetworkProtected": true,
            "RequireTenant": false
          },
          "$reset": {
            "Name": "$reset",
            "Level": ["System"],
            "Enabled": true,
            "RequireAuthorization": "WhenAuthEnabled",
            "NetworkProtected": true,
            "RequireTenant": false
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