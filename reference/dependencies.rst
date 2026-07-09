.. _vonk_dependencies:

Dependencies of Firely Server and their licenses
================================================

Firely Server is built on top of a wide range of open-source libraries - mainly from the .NET, ASP.NET Core,
and MongoDB ecosystems - all under permissive licenses (MIT, Apache 2.0, BSD, and similar) that are compatible
with use in commercial, closed-source software.

Rather than maintain a hand-written list here (which inevitably drifts out of sync with the actual dependency
tree as packages are added, removed, or updated), Firely automatically generates a complete Software Bill of
Materials (SBOM) - covering every direct and transitive dependency - as part of the build process for every
release. Every dependency's license is checked automatically against that same compatibility policy.

You can retrieve the SBOM for a specific release yourself:

* **Docker image**: the SBOM (and a build provenance attestation) is attached directly to the image. Retrieve
  it with ``docker buildx imagetools inspect <image>:<tag> --format "{{ json .SBOM }}"``, or with
  ``docker scout sbom <image>:<tag>`` if you have `Docker Scout <https://docs.docker.com/scout/>`_ available.
* **ZIP**: published as a sibling file next to the release download on the
  `download page <https://downloads.fire.ly/firely-server>`_, with a matching filename - for example,
  ``firely-server-v6.9.0.zip`` and ``firely-server-v6.9.0.sbom.json``.

.. _firely_oss_license:

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
