.. _vonk_dependencies:

Dependencies of Firely Server and their licenses
================================================

Firely Server is mainly built using libraries from Microsoft .Net Core and ASP.NET Core, along with a limited list of other libraries.
This is the full list of direct depencies that Firely Server has on other libraries, along with their licenses.

This list uses the NuGet package names (or prefixes of them) so you can easily lookup further details of those packages on `NuGet.org <https://www.nuget.org>`_ if needed.

#. Microsoft.AspNetCore.* - Apache 2.0
#. Microsoft.ApplicationInsights.* - MIT
#. Microsoft.Bcl.AsyncInterfaces - MIT
#. Microsoft.EntityFrameworkCore.* - MIT
#. Microsoft.Extensions.* - MIT
#. Microsoft.AspNet.WebApi.Client - `MS-.NET-Library License <https://go.microsoft.com/fwlink/?LinkId=329770>`_
#. System.Interactive.Async - MIT
#. Microsoft.Data.SqlClient - MIT
#. Microsoft.CSharp - MIT
#. System.Interactive.Async - MIT
#. System.Text.Json - MIT
#. System.* - `MS-.NET-Library License <https://go.microsoft.com/fwlink/?LinkId=329770>`_
#. NETStandard.Library - MIT
#. NewtonSoft.Json - MIT
#. IdentityModel.* - Apache 2.0
#. IdentityServer4.AccessTokenValidation - Apache 2.0
#. IPNetwork2 - BSD 2-Clause "Simplified" License
#. Quartz - Apache 2.0
#. Serilog(.*) - Apache-2.0
#. LinqKit.Microsoft.EntityFrameworkCore - MIT
#. Hl7.Fhir.* - Firely OSS license (see below)
#. Fhir.Metrics - as Hl7.Fhir
#. Simplifier.Licensing - as Hl7.Fhir
#. Dapper - Apache 2.0
#. SqlKata.* - MIT

MongoDB: 

#. MongoDB.* - Apache 2.0

For unittesting:

#. XUnit - Apache 2.0
#. Moq - BSD 3
#. FluentAssertions - Apache 2.0
#. Microsoft.NETCore.Platforms - MIT
#. Microsoft.NET.Test.Sdk - `MS-.NET-Library License <https://go.microsoft.com/fwlink/?LinkId=329770>`_
#. System.Reactive - MIT
#. coverlet.collector - MIT
#. WireMock.Net - Apache 2.0

Firely OSS License
------------------

Firely Server relies on the reference .NET FHIR library: Hl7.Fhir.*, also created and maintained by Firely. The license is this (as stated in the `LICENSE file <https://github.com/FirelyTeam/firely-net-sdk/blob/master/LICENSE>`_:


Copyright (c) 2013-2020, HL7, Firely (info@fire.ly), Microsoft Open Technologies 
and contributors. See the file `CONTRIBUTORS <https://github.com/FirelyTeam/firely-net-sdk/blob/master/contributors.md>`_ for details

All rights reserved.

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this
  list of conditions and the following disclaimer in the documentation and/or
  other materials provided with the distribution.

* Neither the name of Firely nor the names of its
  contributors may be used to endorse or promote products derived from
  this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
