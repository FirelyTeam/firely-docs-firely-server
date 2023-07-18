Database security
=================

To secure your database/data against malicious access you can add the following security layers. These security measures will have an impact on the performance of your Firely Server instance.

Transparent Data Encryption (TDE)
---------------------------------
After enableing TDE, the data stored on disk will be encrypted, using a certificate to protect the keys used for encryption.
This prevents copies of the database to be read properly without the certificate.
The performance hit using this security measure will be on the database server, as the data is encrypted/decrypted during write/read to/from disk activities.

`More information <https://learn.microsoft.com/en-us/sql/relational-databases/security/encryption/transparent-data-encryption?view=sql-server-ver16>`_

Data Masking
------------
Data masking will obfuscate (parts of) database columns on a database user level. Granting 'unmasked' privileges to the application user and stricter masking to other users will limit the exposure of sensitive data while querying the database.
This security measure should be used in conjunction with other security measures.
The performance hit for Firely Server will be minimal, as it there should not be any masking happening for the database user configured for Firely Server.

`More information <https://learn.microsoft.com/en-us/sql/relational-databases/security/dynamic-data-masking?view=sql-server-ver16>`_

Row Level Security
------------------
Row level security makes it that database users cannot access rows they do not have permissions for. This could potentially reduce database access when using multiple instances of Firely Server with different database users, but this is not adviced as tasks like Bulk Data Export can be handled by each instance (which might have wrong privileges).
The performance hit will probably be substantional. As for each query sent to the database, additional queries get added to determine if a database user is allowed to access data in the resultset.

`More information <https://learn.microsoft.com/en-us/sql/relational-databases/security/row-level-security?redirectedfrom=MSDN&view=sql-server-ver16>`_

Encrypted Connections
---------------------
With encrypting connections the data traffic between the database and the Firely Server will be encrypted by using certificates.
The performance hit will be minimal, just like the difference between http and https.

`More information <https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/configure-sql-server-encryption?view=sql-server-ver16>`_

Always Encrypted
----------------
Always encrypted is a client side operation before storing the data in the database. This is currently not supported by Firely Server.

`More information <https://learn.microsoft.com/en-us/sql/relational-databases/security/encryption/always-encrypted-database-engine?view=sql-server-ver16>`_
