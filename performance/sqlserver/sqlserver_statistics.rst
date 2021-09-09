SQL Server Statistics
=====================

The maintenance of statistics falls outside the scope of Firely Server.

Normally SQL Server will update and maintain the statistics automatically. But when the database becomes large this process might happen too infrequently. There is no magic bullet for updating statistics, so you are advised to experiment and test what works best for your setup.

Consider updating statistics for the following conditions [#]_:

* Query execution times are slow.
* Insert operations occur on ascending or descending key columns.
* After maintenance operations.

To manually update statistics there are two options [#]_ [#]_:

* A stored procedure ``sp_updatestats``: this runs ``UPDATE STATISTICS`` against all user-defined and internal tables in the current database.
* A SQL statement against a single table (here we use the Entry table) ``UPDATE STATISTICS [vonk].[entry]``;

.. [#] `SQL Server Statistic docs <https://docs.microsoft.com/en-us/sql/relational-databases/statistics/statistics?view=sql-server-ver15#UpdateStatistics>`_
.. [#] `Store procedure sp_updatestats <https://docs.microsoft.com/en-us/sql/relational-databases/system-stored-procedures/sp-updatestats-transact-sql?view=sql-server-ver15>`_
.. [#] `Update statistics single table <https://docs.microsoft.com/en-us/sql/t-sql/statements/update-statistics-transact-sql?view=sql-server-ver15>`_