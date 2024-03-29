Disk Configuration
==================

When optimizing SQL Server it is highly recommended to set the tempDB, the data disk and the log disk on seperate drives (preferably SSDs) [#]_. 

TempDB
------
Place the tempDB database on a separate disk and adjust the number of secondary data files according to how many (logical) processors the host features [#]_. If the number of logical processors is less than or equal to eight, use the same number of data files as logical processors. If the number of logical processors is greater than eight, use eight data files. For TempDB it usually is sufficient to create 8 equally sized data files. As a general rule of thumb:

- Number of logical processors < 8 
    --> Set the number of data files equal to the number of logical processors

- Number of logical processors >= 8
    --> Set the number of data files to 8 

Data & Log files
----------------
Placing both data AND log files on the same device can cause contention for that device, resulting in poor performance. Placing the files on separate drives allows the I/O activity to occur at the same time for both the data and log files.

Also format your data disk to use 64-KB allocation unit size for all data files placed on a drive [#]_. Use SSDs with high IOPS where possible.


.. [#] `Place Data and Log Files on Separate Drives <https://docs.microsoft.com/en-us/sql/relational-databases/policy-based-management/place-data-and-log-files-on-separate-drives?view=sql-server-ver15>`_
.. [#] `TempDB data files <https://docs.microsoft.com/en-us/sql/relational-databases/databases/tempdb-database?view=sql-server-ver15#physical-properties-of-tempdb-in-sql-server>`_
.. [#] `Best practice storage <https://docs.microsoft.com/en-us/azure/azure-sql/virtual-machines/windows/performance-guidelines-best-practices-storage>`_