Disk & Database File Configuration
==================================

This section offers recommendations for disk and database file configuration settings for SQL Server, compiling Microsoft's best practices [#]_ to enhance SQL Server performance. 
These guidelines serve as general principles and might not universally apply. Consulting a database administrator is advisable to tailor configurations to specific environments and data volumes.

Allocation/Block Unit Size
--------------------------
The allocation unit size, also known as the cluster size, denotes the smallest disk space that can hold a file. By default, NTFS allocates 4 KB for this purpose.

In SQL Server, storage revolves around pages, each measuring 8 KB, organized into extents of eight contiguous pages, resulting in a 64 KB extent size. Enhancing SQL Server performance involves formatting the data disk with a 64-KB allocation unit size for all data files, including those housing database or tempdb files.

This recommendation extends to Linux file systems, where the default block size often ranges from 4 to 8 KB. For optimal performance, format the file system with a block size of 64 KB.

.. note::
    SQL Linux containers do not come preconfigured for production with the optimal block size or other settings below. Therefore, it is recommended to create a custom container image with the optimal configurations.

Data & Log files
----------------
Placing both data and log files on the same device can cause contention for that device, resulting in poor performance. Placing the files on separate drives allows the I/O activity to occur at the same time for both the data and log files.
When optimizing SQL Server it is highly recommended to set the tempDB, data file, and the log file on seperate drives (preferably SSDs or RAID10) [#]_. 

TempDB
------
Place the tempDB database on a separate disk and adjust the number of secondary data files according to how many (logical) processors the host features [#]_. If the number of logical processors is less than or equal to eight, use the same number of data files as logical processors. 
If the number of logical processors is greater than eight, use eight data files. For TempDB it usually is sufficient to create 8 equally sized data files. 

As a general rule of thumb:

- Number of logical processors < 8 
    --> Set the number of data files equal to the number of logical processors

- Number of logical processors >= 8
    --> Set the number of data files to 8 


Database Instant File Initialization
------------------------------------
It is recommended to enable the "Perform Volume Maintenance Tasks" policy for the SQL Server service account [#]_. 
This policy allows the SQL Server service account to perform instant file initialization, which allows for faster database file creation and growth. This policy is not enabled by default.

File size and File Growth
-------------------------
Set appropriate file size and growth settings to prevent performance issues  [#]_. Avoid the default settings of 10% file autogrowth or 1 MB autogrowth, as small increments or unnecessary growth and shrinkage can lead to both index and disk fragmentation [#]_, impacting performance.
A good practice is to set the file growth to a fixed size, such as 1 GB, to prevent frequent autogrowth events and to predict the database size growth over time.


Antivirus Exclusions
--------------------
It is recommended to exclude the SQL Server processes, data, and log files from antivirus scans [#]_. This is because the antivirus software can cause performance issues by scanning the files while SQL Server is trying to access them.

.. [#] `Best practice storage <https://docs.microsoft.com/en-us/azure/azure-sql/virtual-machines/windows/performance-guidelines-best-practices-storage>`_
.. [#] `Place Data and Log Files on Separate Drives <https://docs.microsoft.com/en-us/sql/relational-databases/policy-based-management/place-data-and-log-files-on-separate-drives?view=sql-server-ver15>`_
.. [#] `TempDB data files <https://docs.microsoft.com/en-us/sql/relational-databases/databases/tempdb-database?view=sql-server-ver15#physical-properties-of-tempdb-in-sql-server>`_
.. [#] `Database Instant File Initialization <https://learn.microsoft.com/en-us/sql/relational-databases/databases/database-instant-file-initialization>`_
.. [#] `File size and File Growth <https://learn.microsoft.com/en-us/troubleshoot/sql/database-engine/database-file-operations/considerations-autogrow-autoshrink>`_
.. [#] `Disk Fragmentation <https://learn.microsoft.com/en-us/troubleshoot/sql/database-engine/database-file-operations/defragmenting-database-disk-drives>`_
.. [#] `Antivirus Exclusions <https://learn.microsoft.com/en-us/sql/relational-databases/security/antivirus-software-on-sql-server>`_
