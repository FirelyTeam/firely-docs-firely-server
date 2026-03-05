.. _firely_auth_app_developer:

App Developer Portal
====================

.. note::

   The App Developer Portal feature is available starting with Firely Auth 4.4.0. For more information, see :ref:`firelyauth_releasenotes_4.4.0`.

Firely Auth includes an **App Developer Portal** that allows external application developers to self-register for an account and request client (application) registrations — all subject to administrator approval. This streamlines the onboarding of third-party app developers while keeping the administrator in full control.

.. contents:: On this page
   :local:
   :depth: 2

Overview
--------

The App Developer Portal introduces a request-based workflow for two main actions:

1. **Account requests** — An app developer requests a new user account.
2. **Client requests** — Once approved and logged in, the app developer requests new OAuth2 client registrations.

Both follow a **change request** model: the developer submits a request, and an administrator reviews it. The administrator can approve, approve with changes, or decline the request.

.. TODO: Add screenshot of the App Developer Portal login page showing the "Create App Developer Account" button.

.. image:: ../../images/firely-auth-app-developer-login.png
   :alt: Firely Auth login page with "Create App Developer Account" button
   :scale: 75%

Enabling the App Developer Portal
----------------------------------

The portal is controlled by the ``AppDeveloperPortal`` configuration section in the Firely Auth appsettings. See :ref:`firely_auth_settings_appdeveloperportal` for the full reference.

.. code-block:: json

   "AppDeveloperPortal": {
     "Enabled": true,
     "Fields": {
       "CompanyName": "Required",
       "CompanyId": "Optional",
       "CompanyBusinessAddress": "Optional",
       "CompanyWebsite": "Required",
       "Reason": "Optional"
     }
   }

- ``Enabled``: Set to ``true`` to activate the App Developer Portal. Default is ``false``.
- ``Fields``: Controls which company-related fields are shown during registration and whether they are ``Required``, ``Optional``, or ``Disabled``.

.. note::

   When ``CompanyWebsite`` is set to ``Required`` or ``Optional`` and a value is provided, Firely Auth validates that the email domain of the requesting user matches the company website domain.

App Developer User Type
-----------------------

A new user type, ``AppDeveloper``, is available alongside the existing ``Patient``, ``Practitioner``, and ``Admin`` types.

Key characteristics of App Developer users:

- They **cannot be external** (SSO-only) users — they must be registered locally in Firely Auth with a password.
- They **do not require** a ``fhirUser`` claim (unlike Patient and Practitioner users).
- They have **company information** associated with their account (name, address, registration ID, website).
- Upon approval, they receive an **account activation email** to set their password.

Change Request Workflow
-----------------------

All requests (both user account and client registration) follow a common lifecycle:

.. code-block:: text

   Requested  ──►  Approved / ApprovedWithChanges / Declined  ──►  Closed

.. TODO: Add screenshot of the admin request overview page showing a list of pending change requests.

.. image:: ../../images/firely-auth-app-developer-admin-requests.png
   :alt: Admin view of pending App Developer change requests
   :scale: 75%

**Status values:**

+-------------------------+---------------------------------------------------------------+
| Status                  | Description                                                   |
+=========================+===============================================================+
| ``Requested``           | The request has been submitted and is awaiting admin review.  |
+-------------------------+---------------------------------------------------------------+
| ``Approved``            | The admin approved the request as-is.                         |
+-------------------------+---------------------------------------------------------------+
| ``ApprovedWithChanges`` | The admin approved the request but modified some values.      |
+-------------------------+---------------------------------------------------------------+
| ``Declined``            | The admin declined the request.                               |
+-------------------------+---------------------------------------------------------------+
| ``Closed``              | The request has been acknowledged/closed by either party.     |
+-------------------------+---------------------------------------------------------------+

**Change types:**

- ``Create`` — Request to create a new entity (user account or client).
- ``Update`` — Request to modify an existing entity.
- ``Delete`` — Request to remove an existing entity.

Each request tracks:

- ``before`` — A snapshot of the original data (``null`` for new creations).
- ``requested`` — The data as the app developer wants it.
- ``after`` — The data as actually saved after admin approval (may differ from ``requested`` if the admin made changes).
- ``history`` — An audit trail of status changes, including who made the change, when, and an optional comment/reason.

Account Registration
--------------------

An app developer can request an account **without authentication** by navigating to the registration page in the Firely Auth UI and filling in the following fields:

- **Full Name** (required)
- **Email** (required)
- **Company Name**, **Company ID**, **Company Business Address**, **Company Website** (availability governed by the ``AppDeveloperPortal.Fields`` configuration)
- **Reason** (optional, depending on configuration)

.. TODO: Add screenshot of the App Developer registration form showing all available fields.

.. image:: ../../images/firely-auth-app-developer-registration-form.png
   :alt: App Developer account registration form
   :scale: 75%

After submission:

1. A confirmation email is sent to the app developer.
2. A notification email is sent to the administrator.
3. The request appears in the admin's request overview for review.

.. TODO: Add screenshot of the admin reviewing a user account request, showing the approval/decline buttons.

.. image:: ../../images/firely-auth-app-developer-admin-review.png
   :alt: Admin reviewing an App Developer account request
   :scale: 75%

When the administrator **approves** the request:

- A user account of type ``AppDeveloper`` is created.
- An account activation email is sent to the developer with a link to set their password.

When the administrator **declines** the request:

- A notification email is sent to the developer.

API reference
^^^^^^^^^^^^^

.. list-table::
   :header-rows: 1
   :widths: 10 30 15 45

   * - Method
     - Endpoint
     - Auth
     - Description
   * - POST
     - ``/api/app-developer/request/user``
     - None
     - Submit a new account request. Returns ``201 Created`` with the request ID.
   * - GET
     - ``/api/admin/request/user``
     - Admin
     - List user account requests. Supports filtering by status, search term, pagination, and sorting.
   * - GET
     - ``/api/admin/request/user/{id}``
     - Admin
     - Retrieve details of a specific user account request.
   * - PUT
     - ``/api/admin/request/user/{id}``
     - Admin
     - Update a user account request (approve, approve with changes, or decline).

Client Registration Requests
-----------------------------

Once an app developer has an active account, they can log in and request new **OAuth2 client registrations**.

An app developer can:

- **Create** a new client request — submitting the desired client configuration.
- **View** their own client requests and their statuses.
- **Close** a request (withdraw it while still in ``Requested`` status, or acknowledge an ``Approved``/``Declined`` decision).

.. note::

   Currently, only ``Create`` requests are supported for app developers. Updating or deleting existing clients through the portal is not yet implemented.

.. TODO: Add screenshot of the App Developer portal showing the client request creation wizard (e.g. the first step with client name, type, etc.).

.. image:: ../../images/firely-auth-app-developer-client-request.png
   :alt: App Developer creating a new client registration request
   :scale: 75%

Restrictions on client configuration
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

App developers have limited control over certain client settings compared to administrators:

+----------------------------+-------------------------------------------+
| Section                    | App Developer Access                      |
+============================+===========================================+
| General Info               | Tags are disabled; enabled fields only.   |
|                            | "Management API" client type cannot be    |
|                            | selected.                                 |
+----------------------------+-------------------------------------------+
| Client Secret              | All fields can be changed.                |
+----------------------------+-------------------------------------------+
| Permissions                | All fields can be changed.                |
+----------------------------+-------------------------------------------+
| Authorization Process      | MFA and Require Consent are disabled      |
|                            | (admin-only settings).                    |
+----------------------------+-------------------------------------------+
| Token                      | All fields are disabled.                  |
+----------------------------+-------------------------------------------+
| SSO                        | All fields are disabled.                  |
+----------------------------+-------------------------------------------+
| Legal & Compliance         | All fields can be changed.                |
+----------------------------+-------------------------------------------+

.. TODO: Add screenshot showing the App Developer's "My Requests" overview, listing submitted client requests and their statuses.

.. image:: ../../images/firely-auth-app-developer-my-requests.png
   :alt: App Developer view of submitted client requests
   :scale: 75%

API reference
^^^^^^^^^^^^^

.. list-table::
   :header-rows: 1
   :widths: 10 30 15 45

   * - Method
     - Endpoint
     - Auth
     - Description
   * - POST
     - ``/api/app-developer/request/client``
     - AppDeveloper
     - Submit a new client registration request. Returns ``201 Created`` with the request ID.
   * - GET
     - ``/api/app-developer/request/client``
     - AppDeveloper
     - List the app developer's own client requests. Supports filtering by status.
   * - GET
     - ``/api/app-developer/request/client/{id}``
     - AppDeveloper
     - Retrieve details of one of the app developer's client requests.
   * - PUT
     - ``/api/app-developer/request/client/{id}``
     - AppDeveloper
     - Close (withdraw or acknowledge) a client request. Only ``status: Closed`` is accepted.
   * - GET
     - ``/api/admin/request/client``
     - Admin
     - List all client requests. Supports filtering by status, search term, pagination, and sorting.
   * - GET
     - ``/api/admin/request/client/{id}``
     - Admin
     - Retrieve details of a specific client request.
   * - PUT
     - ``/api/admin/request/client/{id}``
     - Admin
     - Update a client request (approve, approve with changes, or decline).

.. TODO: Add screenshot of the admin reviewing a client registration request, showing the client configuration details and the approval/decline buttons.

.. image:: ../../images/firely-auth-app-developer-admin-client-review.png
   :alt: Admin reviewing a client registration request from an App Developer
   :scale: 75%

App Developer Dashboard
-----------------------

Once logged in, an App Developer user has access to a dedicated dashboard providing:

- **Client requests** — List and view clients that they have requested to create, along with their approval status.
- **Firely Server status** — View current Firely Server availability and connected endpoint information.
- **Available resource types** — View the FHIR resource types supported by the connected Firely Server for use in scope configuration.

.. TODO: Add screenshot of the App Developer dashboard after login, showing the client request list and server status.

.. image:: ../../images/firely-auth-app-developer-dashboard.png
   :alt: App Developer dashboard overview
   :scale: 75%
