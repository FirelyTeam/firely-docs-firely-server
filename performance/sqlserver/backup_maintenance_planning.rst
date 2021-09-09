Backup Maintenance Plan
=======================

When Firely Server is used in conjunction with SQL Server, a maintenance plan should be in place. FUrthermore, if the recovery log is not set to 'Simple' regular back-ups of the transaction logs should be made.

Microsoft advises that if a database uses either the full or bulk-logged `recovery model <https://docs.microsoft.com/en-us/sql/relational-databases/backup-restore/recovery-models-sql-server?view=sql-server-ver15>`_, you must back up the transaction log regularly enough to protect your data, and to prevent the `transaction log from filling <https://docs.microsoft.com/en-us/sql/relational-databases/logs/troubleshoot-a-full-transaction-log-sql-server-error-9002?view=sql-server-ver15>`_. This truncates the log and supports restoring the database to a specific point in time.

For more information on how to back up the Transaction Log consult the official Microsoft documentatio, which can be found `here <https://docs.microsoft.com/en-us/sql/relational-databases/backup-restore/back-up-a-transaction-log-sql-server?view=sql-server-ver15>`_.