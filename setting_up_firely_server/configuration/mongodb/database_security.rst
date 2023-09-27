Database security
=================

The following explores some of the various security measures that exist in MongoDB and their applicability to a Firely Server MongoDB database.

Access Control
--------------
Access control should be enabled on the MongdDB environment. With this enabled, you will have to make an application user with the appropriate role and use this user to connect Firely Server to the MongoDB environment.
This prevents unauthorized access to the database.

`More information <https://www.mongodb.com/docs/manual/tutorial/enable-authentication/>`_

Queryable Encryption
--------------------
Queryable Encryption is not supported by Firely Server.

`More information <https://www.mongodb.com/docs/manual/core/queryable-encryption/>`_

Client-Side Field Level Encryption
----------------------------------
Client-Side Field Level Encryption is not supported by Firely Server.

`More information <https://www.mongodb.com/docs/manual/core/csfle/>`_

Encryption at Rest
------------------
Encryption at Rest is a mechanism that that will encrypt the data stored on disk. This is enabled by default for MongoDB Atlas, and outside of Atlas only available for MongoDB Enterprise installations that use the WiredTiger Storage Engine.
This prevents copies of the database to be read properly without the certificate.
The performance impact of using this security measure will be on the database server, as the data is encrypted/decrypted during write/read to/from disk activities.

`More information <https://www.mongodb.com/docs/manual/tutorial/configure-encryption/>`_

TLS/SSL (Transport Encryption)
------------------------------
With TLS/SSL, the data traffic between the database and the Firely Server will be encrypted by using certificates.
The performance impact will be minimal, similar to the difference between http and https.

`More information <https://www.mongodb.com/docs/manual/core/security-transport-encryption/>`_


Besides these measures, there are other measures that can be taken to enhance the security of the MongoDB data and traffic between MongoDB and Firely server.
These can be found `here <https://www.mongodb.com/docs/manual/administration/security-checklist/>`_