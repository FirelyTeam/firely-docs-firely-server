Database security
=================

The following explores some of the various security measures that exist in Microsoft SQL Server and their applicability to a Firely Server SQL database.

Transparent Data Encryption (TDE)
---------------------------------
After enabling TDE, the data stored on disk will be encrypted, using a certificate to protect the keys used for encryption.
This prevents copies of the database to be read properly without the certificate.
The performance impact of using this security measure will be on the database server, as the data is encrypted/decrypted during write/read to/from disk activities.

`More information <https://learn.microsoft.com/en-us/sql/relational-databases/security/encryption/transparent-data-encryption?view=sql-server-ver16>`__

Data Masking
------------
Data masking will obfuscate (parts of) database columns on a database user level. Granting 'unmasked' privileges to the application user and stricter masking to other users will limit the exposure of sensitive data while querying the database.
This security measure should be used in conjunction with other security measures.
The performance impact for Firely Server will be minimal since it is required that the database user configured for Firely Server be set to fully 'unmasked'.

`More information <https://learn.microsoft.com/en-us/sql/relational-databases/security/dynamic-data-masking?view=sql-server-ver16>`__

Row Level Security
------------------
Row level security is not supported by Firely Server.

`More information <https://learn.microsoft.com/en-us/sql/relational-databases/security/row-level-security?redirectedfrom=MSDN&view=sql-server-ver16>`__

Encrypted Connections
---------------------
With encrypting connections the data traffic between the database and the Firely Server will be encrypted by using certificates.
The performance impact will be minimal, similar to the difference between http and https.

`More information <https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/configure-sql-server-encryption?view=sql-server-ver16>`__

Always Encrypted
----------------
Always encrypted is a client side operation before storing the data in the database. This is currently not supported by Firely Server.

`More information <https://learn.microsoft.com/en-us/sql/relational-databases/security/encryption/always-encrypted-database-engine?view=sql-server-ver16>`__
