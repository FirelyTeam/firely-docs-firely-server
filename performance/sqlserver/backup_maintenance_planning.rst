Backup maintenance plan
=======================

This is really outside the scope of Firely Server, but when using FS on SQL Server needyou to have a proper maintenance plan in place and regularly back-up the transaction log when when the recovery model is set to anything but Simple.

Microsoft says: If a database uses either the full or bulk-logged `recovery model <https://docs.microsoft.com/en-us/sql/relational-databases/backup-restore/recovery-models-sql-server?view=sql-server-ver15>`_, you must back up the transaction log regularly enough to protect your data, and to prevent the `transaction log from filling <https://docs.microsoft.com/en-us/sql/relational-databases/logs/troubleshoot-a-full-transaction-log-sql-server-error-9002?view=sql-server-ver15>`_. This truncates the log and supports restoring the database to a specific point in time.

For more information on how to back up the Transaction Log see `here <https://docs.microsoft.com/en-us/sql/relational-databases/backup-restore/back-up-a-transaction-log-sql-server?view=sql-server-ver15>`_.