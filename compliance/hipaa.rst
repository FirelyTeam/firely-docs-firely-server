.. _hipaa:

HIPAA compliance - 🇺🇸
=====================

Firely Server is a well-tested, secure HL7 FHIR® server that enables you to comply with the Technical Safeguards of the HIPAA Security Rule.

On this page we will detail how you can achieve compliance for your Firely Server deployment. To ensure your organization's specific use-case, environment, and deployment are compliant, feel free to :ref:`contact us <vonk-contact>`: we'd be happy to help.

.. _hipaa_164.312.a.1:

164.312(a)(1) Standard: Access control
--------------------------------------

   Implement technical policies and procedures for electronic information systems that maintain electronic protected health information to allow access only to those persons or software programs that have been granted access rights as specified in 164.308(a)(4).

There are several ways to approach this:

1. ensure Firely Server is deployed in a secure environment where only those with correct permissions are able to access it,
2. use SMART on FHIR as a means of controlling access,
3. or add custom authentication based on a plugin.

Deploying in a secure environment (1) would mean access to Firely Server is controlled by third-party software or policy, placing this scenario outside the scope of this guide.

For scenario (2), Firely Server implements support for `Smart on FHIR <http://hl7.org/fhir/smart-app-launch/index.html>`_, a sibling specification to FHIR for securely connecting third-party applications to Electronic Health Record data. See :ref:`feature_accesscontrol` on how to configure Firely Server with it.

You may also wish to setup custom authentication (3). Given how Firely Server is based on a pipeline architecture, you can insert a plugin at the start of the pipeline to call out to your authentication service(s) prior to handling the request. See `this gist <http://bit.ly/VonkAuthorizationMiddleware>`_ as an example.

.. _hipaa_164.312.c.1:

164.312(c)(1) Standard: Integrity
---------------------------------

   Implement policies and procedures to protect electronic protected health information from improper alteration or destruction.

The same solutions apply to this point as :ref:`hipaa_164.312.a.1` and :ref:`hipaa_164.312.b`.

.. _hipaa_164.312.d:

164.312(d) Standard: Person or entity authentication
----------------------------------------------------

   Implement procedures to verify that a person or entity seeking access to electronic protected health information is the one claimed.

The same solutions apply to this point as :ref:`hipaa_164.312.a.1`.

.. _hipaa_164.312.a.2.i:

164.312(a)(2)(i) Unique user identification
-------------------------------------------

   Assign a unique name and/or number for identifying and tracking user identity.

The same solution applies to this point as :ref:`hipaa_164.312.b`.
For Firely Server to be able to log the identity of the user, this identity must be present in or derivable from the authentication token, and it must be added to the log properties. If you use the SMART on FHIR plugin, that is automatically configured. If you want to do this from within a custom authentication plugin, feel free to contact us for details.

.. _hipaa_164.312.b:

164.312(b) Standard: Audit control
-----------------------------------

   Implement hardware, software, and/or procedural mechanisms that record and examine activity in information systems that contain or use electronic protected health information.

With the use of the :ref:`Audit Event log <vonk_plugins_audit>` plugin, Firely Server will thoroughly log every interaction as a note in a log file and/or in an AuditEvent resource. Logged information will be a trace record of all system activity: viewing, modification, deletion and creation of all Eletronic Protected Health Information (ePHI).

The audit trail can track the source IP, event type, date/time, and more. If a JWT token is provided (for SMART on FHIR), the user/patient identity can be logged as well.

.. _hipaa_164.312.e.1-2:

164.312(e)(1, 2) Standard: Transmission security
------------------------------------------------

    Implement technical security measures to guard against unauthorized access to electronic protected health information that is being transmitted over an electronic communications network.

    Implement a mechanism to encrypt electronic protected health information whenever deemed appropriate.

Transmission security in Firely Server can be achieved by encrypting the communications with TLS/SSL. Standard industry practice is to use a reverse proxy (e.g. nginx or IIS) for this purpose. If you'd like, you can also enable secure connections in Firely Server :ref:`directly <configure_hosting>` without a proxy as well.

Firely Server is regularly updated with the latest versions of ASP.NET to ensure that the latest cryptographic algorithms are available for use.

.. _hipaa_164.312.e.2.ii:

164.312(e)(2)(ii) Encryption
----------------------------

    Implement a mechanism to encrypt electronic protected health information whenever deemed appropriate.

The recommended way to ensure that e-PHI is encrypted as necessary is to use disk encryption, and there are several solutions for this depending on your deployment environment. If you're deploying in the cloud - see your vendors options for disk encryption, as most have options for encrypted disks already. If you're deploying locally, look into BitLocker on Windows or dm-crypt/LUKS for Linux.

Disk encryption is preferred over individual database field encryption as the latter would severely impact the search performance.

.. _hipaa_164.312.a.2.ii:

164.312(a)(2)(ii) Emergency access procedure
--------------------------------------------

    Establish (and implement as needed) procedures for obtaining necessary electronic protected health information during an emergency.

This depends on the solution you went with for :ref:`hipaa_164.312.a.1`.

In case you went with SMART on FHIR, add an authorization workflow that grants emergency access rights - essentially, a "super" access token. The application can then use this token with Firely Server, just like any other token. 

If you went with a custom authentication scheme, add a special measure to handle this scenario.

.. _hipaa_164.312.c.2:

164.312(a)(c) Implementation specification: Mechanism to authenticate electronic protected health information
-------------------------------------------------------------------------------------------------------------

    Implement electronic mechanisms to corroborate that electronic protected health information has not been altered or destroyed in an unauthorized manner.

Firely Server does not allow you to delete resources through its RESTful API. Old versions of resources are retained by default. The only way to alter or destroy resources is through direct database access.

Therefore database-level safety mechanisms must ensure that information is not altered or destroyed unless it's desired.
