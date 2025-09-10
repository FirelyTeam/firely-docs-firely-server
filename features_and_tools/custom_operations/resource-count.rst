.. _resource-count:

Resource Count Statistics - $resource-count
==========================================

.. note::

  The feature described on this page is available in **all** :ref:`Firely Server editions <vonk_overview>`.

Description
-----------

The ``$resource-count`` operation provides statistics about the number of resources stored in the Firely Server database, grouped by resource type. This operation is useful for monitoring, analytics, and understanding the distribution of data in your FHIR server.

This is a system-level administration operation that returns a JSON object with resource types as keys and their corresponding counts as values.

.. note::

  The operation only counts:
  
  * Current versions of resources (not historical versions)
  * Non-contained resources (resources inside another resource's "contained" array are excluded)
  * Non-deleted resources (soft-deleted resources are excluded)

Operation details
-----------------

The client shall invoke the operation with no parameters. The response is a JSON object containing resource counts.

**Request:**
::

  GET <base-url>/administration/$resource-count

**Response:**
The response is a JSON object where each key represents a FHIR resource type and the corresponding value represents the count of resources of that type stored in the database.

Example response:

.. code-block:: json

    {
        "Patient": 1250,
        "Observation": 4892,
        "Encounter": 892,
        "Practitioner": 45,
        "Organization": 12,
        "Medication": 234
    }

Configuration
-------------

The ``$resource-count`` operation is enabled by default in the administration pipeline. It can be found in the ``Administration:Operations`` section of the :ref:`appsettings <configure_appsettings>`:

.. code-block:: json

    "Administration": {
        "Operations": {
            "$resource-count": {
                "Name": "$resource-count",
                "Level": [
                    "System"
                ],
                "Enabled": true,
                "NetworkProtected": true
            }
        }
    }

The operation is configured as:

* **System-level**: Can only be invoked at the server root level
* **Network Protected**: Access is restricted by network security settings
* **Enabled by default**: No additional configuration is required

Repository Support
------------------

The ``$resource-count`` operation is supported by all repository implementations:

* **SQL Server**: Uses Entity Framework with efficient grouped queries
* **SQLite**: Uses Entity Framework with efficient grouped queries  
* **MongoDB**: Uses MongoDB aggregation pipeline for optimal performance

Performance Considerations
--------------------------

* The operation performs aggregation queries on the database to count resources by type
* For large datasets, the operation may take some time to complete
* The MongoDB implementation uses parallel counting for better performance
* Results are calculated in real-time and are not cached

Security
--------

The ``$resource-count`` operation is part of the administration API and requires appropriate access permissions. Since it only returns count statistics without exposing actual resource data, it has a low security risk profile.

The operation respects the ``NetworkProtected`` setting and will only be accessible from allowed network ranges when this protection is enabled.

Use Cases
---------

The ``$resource-count`` operation can be used for:

* **System monitoring**: Understanding resource distribution and growth over time
* **Analytics and reporting**: Generating statistics about FHIR data usage
* **Data validation**: Verifying expected resource counts after data imports
* **Capacity planning**: Understanding storage requirements and growth patterns
* **Debugging**: Investigating resource count discrepancies

Example Usage
-------------

**Basic request:**
::

    GET https://your-server.com/administration/$resource-count
    Accept: application/json

**Response:**

.. code-block:: json

    {
        "Patient": 1250,
        "Observation": 4892,
        "Encounter": 892,
        "DiagnosticReport": 445,
        "Practitioner": 45,
        "Organization": 12,
        "Medication": 234,
        "MedicationRequest": 1834
    }

**Using with cURL:**
::

    curl -X GET "https://your-server.com/administration/$resource-count" \
         -H "Accept: application/json" \
         -H "Authorization: Bearer <your-token>"

Limitations
-----------

* The operation does not provide historical data or trends
* Counts represent the current state of the database at the time of the request
* Very large databases may experience longer response times
* The operation requires read access to the entire database