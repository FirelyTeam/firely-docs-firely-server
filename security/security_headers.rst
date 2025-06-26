.. _security_headers:

HTTP security headers
=====================

HTTP security headers are a critical part of web application security, helping to protect against various attacks such as cross-site scripting (XSS), clickjacking, and other code injection vulnerabilities. 
By configuring these headers, web developers can enforce stricter communication policies between clients and servers, enhancing the overall security posture of their websites.

Firely Server and Firely Auth apply only limited security headers by default. For each deployment it is advised to adjust them depending on the need and risk, e.g. by enabling a reverse proxy to add these request headers.

Firely Server applies by default the following headers:

* ``Cache-Control``: ``no-store`` is used to prevent sensitive information from being stored in caches
* ``X-Content-Type-Options``: Firely Server uses ``nosniff`` to prevent MIME type confusion attacks
* ``Content-Security-Policy``: To mitigating cross-site scripting Firely Server uses specific nounces to avoid executing injected executable code

The following security may be added through a reverse proxy for enhanced security:

* ``X-Frame-Options``: This disables showing content within an X-Frame to reduce the risk of interacting with content served by Firely Server / Firely Auth in a malicious third-party context. This might interfere with the login UI for Firely Auth when using it as part of EHR launch scenarios. 
* ``Permissions-Policy``: Use this header in case it is necessary to limit the permissions of the frontend components.
* ``Referrer-Policy``: For privacy preserving measures Firely Server and Firely Auth work if ``strict-origin-when-cross-origin`` is enabled as the Referrer-Policy. This is the default value of this HTTP header.