.. _sql_index_maintenance:

Index Maintenance
=================

You can check the index fragmentation with the following script:

.. code-block:: SQL

    SELECT OBJECT_SCHEMA_NAME(i.object_id) AS schema_name,
       OBJECT_NAME(i.object_id) AS object_name,
       i.name AS index_name,
       i.type_desc AS index_type,
       100.0 * (ISNULL(SUM(rgs.deleted_rows), 0)) / NULLIF(SUM(rgs.total_rows), 0) AS avg_fragmentation_in_percent
    FROM sys.indexes AS i
    INNER JOIN sys.dm_db_column_store_row_group_physical_stats AS rgs
    ON i.object_id = rgs.object_id
       AND
       i.index_id = rgs.index_id
    WHERE rgs.state_desc = 'COMPRESSED'
    GROUP BY i.object_id, i.index_id, i.name, i.type_desc
    ORDER BY schema_name, object_name, index_name, index_type;

Some guidelines [#]_:

* **Do not assume that index maintenance will always noticeably improve your workload.**
* **If you observe that rebuilding indexes improves performance, try replacing it with updating statistics.** This may result in a similar improvement. In that case, you may not need to rebuild indexes as frequently, or at all, and instead can perform periodic statistics updates.
* Monitor index fragmentation and page density over time to see if there is a correlation between these values trending up or down, and query performance. If higher fragmentation or lower page density degrade performance unacceptably, reorganize or rebuild indexes. **It is often sufficient to only reorganize or rebuild specific indexes used by queries with degraded performance.** This avoids a higher resource cost of maintaining every index in the database.
* Establishing a correlation between fragmentation/page density and performance also lets you determine the frequency of index maintenance. **Do not assume that maintenance must be performed on a fixed schedule.** A better strategy is to monitor fragmentation and page density, and run index maintenance as needed before performance degrades to an unacceptable level.
* If you have determined that index maintenance is needed and its resource cost is acceptable, perform maintenance during low resource usage times, if any, keeping in mind that resource usage patterns may change over time.


There is also a general rule of thumb with regards to fragmentation (YMMV):

* **< 10%**:   no action needed.
* **10-30%**:  reorganize the index 
* **>30%**:    rebuild the index 


.. [#] `Index maintenance <https://docs.microsoft.com/en-us/sql/relational-databases/indexes/reorganize-and-rebuild-indexes?view=sql-server-ver15#index-maintenance-strategy>`_