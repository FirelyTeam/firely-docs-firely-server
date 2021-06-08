.. _vonk_securitynotes:

Security notifications for Firely Server
========================================

January 2021
------------

Microsoft has a new Security Advisory regarding ASP.NET Core:

  * Microsoft Security Advisory CVE-2020-1161 | ASP.NET Core Denial of Service Vulnerability in all ASP.NET Core applications on runtime 3.1.3 or lower (`#416 <https://github.com/aspnet/Announcements/issues/416>`_). If you are not already up-to-date, you should install the latest runtime version from `<https://dotnet.microsoft.com/download/dotnet-core/3.1>`_

July 2020
---------

Microsoft has published several newer Security Advisories regarding ASP.NET Core:

  * Microsoft.ApplicationInsights.AspNetcore 2.12 was vulnerable to `CVE-2005-2224 <http://web.nvd.nist.gov/view/vuln/detail?vulnId=CVE-2005-2224>`_. We upgraded it to 2.14.
  * Microsoft Security Advisory CVE-2020-0602 : ASP.NET Core Denial of Service Vulnerability, `#402 <https://github.com/aspnet/Announcements/issues/402>`_ and
  * Microsoft Security Advisory CVE-2020-0603 : ASP.NET Core Remote Code Execution Vulnerability, `#403 <https://github.com/aspnet/Announcements/issues/403>`_.
    These affect applications running SignalR. Vonk does not use SignalR. Nevertheless we recommend to follow Microsoft's advice:
    For machines running .NET Core 3.1, you should download and install Runtime 3.1.1 or SDK 3.1.101 from https://dotnet.microsoft.com/download/dotnet-core/3.1
  * Microsoft Security Advisory | MessagePack Denial of Service, `#405 <https://github.com/aspnet/Announcements/issues/405>`_.
    This only affect applications using MessagePack, which Vonk does not use.

September 2019
--------------

Updates regarding previous Security Advisories:

   * Please upgrade the ASP.NET Core runtime to at least version 2.2.7, from the `runtimes download page <https://dotnet.microsoft.com/download#/runtime/>`_. 
     This solves:

        * `#334 <https://github.com/aspnet/Announcements/issues/334>`_
        * `#373 <https://github.com/aspnet/Announcements/issues/373>`_
        * `#384 <https://github.com/aspnet/Announcements/issues/384>`_
        * `#385 <https://github.com/aspnet/Announcements/issues/385>`_

   * `#295 <https://github.com/aspnet/Announcements/issues/295>`_: Vonk has been upgraded to ASP.NET Core 2.2, and is therefore no longer vulnerable to this issue. 
     It is nevertheless advised to run a publicly exposed Vonk :ref:`behind a proxy <deploy_reverseProxy>` or on an Azure Web App. 
   * `#335 <https://github.com/aspnet/Announcements/issues/335>`_: no longer relevant to Vonk since it runs on ASP.NET Core 2.2

Microsoft has published several newer Security Advisories regaring ASP.NET Core:

   * `#325 <https://github.com/aspnet/Announcements/issues/352>`_: This is not applicable yet to Vonk, since it affects AspNetCoreModuleV2 and Vonk still works on AspNetCoreModule (implicitly V1).
     We will upgrade to V2 shortly though, so we advise you to install the latest AspNetCoreModulev2 anyway.
   * `#359 <https://github.com/aspnet/Announcements/issues/359>`_: Not relevant to Vonk, it does not use SignalR.

January 2019
------------

Microsoft has published two Security Advisories regarding ASP.NET Core:

   * If you run Vonk behind Internet Information Server (IIS), you may be vulnerable to "Microsoft Security Advisory CVE-2019-0548: ASP.NET Core Denial Of Service Vulnerability".
     Refer to the related `Github issue #335 <https://github.com/aspnet/Announcements/issues/335>`_ for details and the fix.
   * When using older versions of some of the ASP.NET Core packages you may be vulnerable to "Microsoft Security Advisory CVE-2019-0564: ASP.NET Core Denial of Service Vulnerability".
     Refer to the related `Github issue #334 <https://github.com/aspnet/Announcements/issues/334>`_ for details.
     Vonk FHIR Server up until version 1.1.0 uses versions of the packages involved that are not affected (older than the vulnerable versions). 
     In a future version we will upgrade beyond the vulnerable version upto secure versions. No action is required by the administrator of Vonk.

April 2018
----------

Microsoft has published two Security Advisories regarding ASP.NET Core:

   * If you run Vonk behind Internet Information Server (IIS), you may be affected by "Microsoft Security Advisory CVE-2018-0808: ASP.NET Core Denial Of Service Vulnerability". 
     Refer to the related `GitHub issue #294 <https://github.com/aspnet/Announcements/issues/294>`_ for details and the fix.
   * If you expose Vonk directly to the internet, or host it behind a proxy which does not validate or restict host headers to known good values, you may be affected by "Microsoft Security Advisory CVE-2018-0787: ASP.NET Core Elevation Of Privilege Vulnerability".
     Refer to the related `GitHub issue #295 <https://github.com/aspnet/Announcements/issues/295>`_ for details and the correct way of hosting Vonk.
     This 'host validating middleware' mentioned by this issue is not a part of Vonk. We advise you to run a publicly exposed Vonk :ref:`behind a proxy <deploy_reverseProxy>` or on an Azure Web App. 

