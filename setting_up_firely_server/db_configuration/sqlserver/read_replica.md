(sql_read_replica)=

# SQL Server Read Replica

Firely Server supports routing read traffic to a separate SQL Server read replica, leaving the primary database free to handle writes. This reduces load on the primary instance and can improve query throughput in read-heavy deployments.

```{note}
Read replica support applies to the **resource database only**. The Administration database always uses the primary connection.
```

## How traffic is routed

| Operation | Connection used | Notes |
|---|---|---|
| Search and read (GET, search) | Replica | CTE-based queries |
| Bulk data export / $everything | Replica | |
| Write operations (POST, PUT, PATCH, DELETE) | Primary | |
| Conditional reads (If-Match, If-None-Exist) | Primary | Run inside a transaction; not affected by replica lag |
| PubSub change tracking | Primary | Must stay on primary to avoid missing events due to replication lag |
| Administration database | Primary only | Read replica is never applied to the Administration database |

## Configuration

Add `ReadReplicaConnectionString` to the `SqlDbOptions` section. When not set, all traffic uses the primary `ConnectionString` as before.

```json
"SqlDbOptions": {
    "ConnectionString": "Server=<primary>;Database=vonk;User Id=<user>;Password=<password>;Encrypt=True",
    "ReadReplicaConnectionString": "Server=<replica>;Database=vonk;User Id=<user>;Password=<password>;Encrypt=True",
    "SchemaName": "vonk",
    "AutoUpdateDatabase": true
}
```

The replica connection string is validated on startup in the same way as the primary.

## Infrastructure requirements

Firely Server routes reads to the replica endpoint you configure. It does not manage replication itself. The replica receives all changes — both data and schema — from the primary through the replication mechanism. This means schema creation and migrations only need to run against the primary; they propagate to the replica automatically.

Two common approaches:

**SQL Server Always On Availability Group (AG)**

Configure a secondary replica with `SECONDARY_ROLE (ALLOW_CONNECTIONS = ALL)` and `SEEDING_MODE = AUTOMATIC`. On first setup, SQL Server seeds the entire database (schema and data) from the primary to the secondary automatically. After that, every write to the primary — including schema migrations — is replicated to the secondary via the AG transaction log stream. Connect Firely Server to the secondary's listener or direct endpoint as the `ReadReplicaConnectionString`.

The database must exist on the primary before it can be added to the AG. In practice this means starting Firely Server first so that `AutoUpdateDatabase` creates the database and schema, and only then configuring the AG to include it.

```{note}
Adding an existing database to a new AG requires a full backup followed by a restore to the secondary before the secondary can join. Automatic seeding only works for databases that have not yet been replicated. If you are setting up an AG against a pre-existing, populated database, take a full backup on the primary, restore it with `NORECOVERY` on the secondary, and then add it to the AG.
```

**Azure SQL geo-replica**

Create a geo-replica from the Azure portal or CLI. The replica is initialised as a full copy of the primary at that point in time, including schema. All subsequent changes to the primary — data writes and schema migrations — are continuously replicated. The replica gets a separate server hostname (e.g. `myserver-replica.database.windows.net`). Use that hostname in `ReadReplicaConnectionString`.

**Azure SQL Hyperscale**

Azure SQL Hyperscale has built-in named replicas (high-availability replicas and optional named read-scale replicas). For Hyperscale, the recommended approach is to use the same connection string, with added `ApplicationIntent=ReadOnly` as a `ReadReplicaConnectionString`. Azure SQL will route the connection to a read-scale replica automatically.


## Replication lag and consistency

Both Always On AG (with `ASYNCHRONOUS_COMMIT`) and Azure SQL geo-replicas replicate asynchronously. The primary commits and returns success before the replica has applied the write. Under normal conditions the lag is sub-second, but it is never zero.

The following cases are handled safely:

- **Conditional writes** (`If-Match`, `If-None-Exist`) run inside a transaction that uses the primary connection. Their read phase is not affected by replica lag.
- **PubSub change tracking** reads from the primary, so it always sees the data it just wrote.

The following case carries a residual risk:

- **SMART on FHIR scope checks**: compartment-filter queries for patient-level scopes are sent to the replica. If a patient resource was created moments before the scope check fires, the replica may not yet have the record. This results in a transient false negative — the request is incorrectly rejected or filtered — which self-corrects once the replica catches up. This window is typically less than a second on co-located infrastructure.

This is an inherent property of asynchronous replication. If strict read-your-writes consistency is required for all operations, do not configure a read replica, or use synchronous commit (see below).

### Synchronous commit (Always On AG only)

If both instances are co-located (same machine or same datacenter), you can configure the AG with `SYNCHRONOUS_COMMIT`. The primary then waits for the replica to confirm every write before returning success, which eliminates replication lag entirely.

```{warning}
Do not use synchronous commit for cross-region replicas. The added write latency from cross-region round trips (typically 50–200 ms per write) will significantly degrade write throughput.
```

Synchronous commit is **not available** for Azure SQL geo-replicas, which are always asynchronous.

## Read-only database user

The replica connection does not need write permissions. For least-privilege access, create a dedicated read-only user and use its credentials in `ReadReplicaConnectionString`.

On a self-hosted SQL Server instance:

```sql
CREATE LOGIN vonk_readonly WITH PASSWORD = '<password>';
USE vonk;
CREATE USER vonk_readonly FOR LOGIN vonk_readonly;
ALTER ROLE db_datareader ADD MEMBER vonk_readonly;
```

On Azure SQL with contained database users (server-level logins are not available):

```sql
USE vonk;
CREATE USER vonk_readonly WITH PASSWORD = '<password>';
ALTER ROLE db_datareader ADD MEMBER vonk_readonly;
```
