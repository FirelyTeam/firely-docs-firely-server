.. _cql:

CQL Plugin
============

Introduction
------------

Firely Server CQL Plugin is a means to use the Firely Server to run the CQL engine. 

This document demonstrates :

* Creation of a simple CQL file
* Transformation of the CQL file to a library resource using the CQL Packager Tool
* Setting up Firely Server CQL Plugin
* Getting the result of your CQL measure on a resource using Postman tool


Create a CQL File
-----------------

To start working with Firely Server CQL Plugin, you would need a CQL File.
An example of a CQL file ``AgeFHIR4.cql`` is displayed below: 

    .. code-block:: jsonc

        library AgeFHIR4 version '0.0.1'
        using FHIR version '4.0.1'

        context Patient

        define "is18OrAbove":
          AgeInYears() >= 18

Generate a Library resource
---------------------------

CQL File is translated to a library resource using the `CQL Packager tool <https://docs.fire.ly/projects/Firely-NET-SDK/en/latest/cql.html>`_

    1. Follow the step by step `Installation of the CQL Packager tool and use the Demo solution <https://github.com/FirelyTeam/firely-cql-sdk/blob/develop/README.md>`_ to generate a resource file:
    2. Place the CQL file ``Age.cql`` in the folder ..\firely-cql-sdk\Demo\Cql\input 
    3. Clean and Build CQL project 

        .. note::
            Running the build for this project turns these CQL files into ELM and puts the files into the source directory of the next step. 
            (Json folder under the Elm project contains the translated file AgeFHIR4.json) 

        .. container:: toggle

            .. container:: header

                Click to expand AgeFHIR4.json

            .. code-block:: jsonc  

                {
                    "library" : {
                        "annotation" : [ {
                            "translatorVersion" : "2.11.0",
                            "translatorOptions" : "EnableLocators,EnableResultTypes",
                            "type" : "CqlToElmInfo"
                        } ],
                        "identifier" : {
                            "id" : "AgeFHIR4",
                            "version" : "0.0.1"
                        },
                        "schemaIdentifier" : {
                            "id" : "urn:hl7-org:elm",
                            "version" : "r1"
                        },
                        "usings" : {
                            "def" : [ {
                                "localIdentifier" : "System",
                                "uri" : "urn:hl7-org:elm-types:r1"
                            }, {
                                "locator" : "3:1-3:26",
                                "localIdentifier" : "FHIR",
                                "uri" : "http://hl7.org/fhir",
                                "version" : "4.0.1"
                            } ]
                        },
                        "contexts" : {
                            "def" : [ {
                                "locator" : "5:1-5:15",
                                "name" : "Patient"
                            } ]
                        },
                        "statements" : {
                            "def" : [ {
                                "locator" : "5:1-5:15",
                                "name" : "Patient",
                                "context" : "Patient",
                                "expression" : {
                                "type" : "SingletonFrom",
                                "operand" : {
                                    "locator" : "5:1-5:15",
                                    "dataType" : "{http://hl7.org/fhir}Patient",
                                    "templateId" : "http://hl7.org/fhir/StructureDefinition/Patient",
                                    "type" : "Retrieve"
                                }
                                }
                            }, {
                                "locator" : "7:1-8:20",
                                "resultTypeName" : "{urn:hl7-org:elm-types:r1}Boolean",
                                "name" : "is18OrAbove",
                                "context" : "Patient",
                                "accessLevel" : "Public",
                                "expression" : {
                                "locator" : "8:3-8:20",
                                "resultTypeName" : "{urn:hl7-org:elm-types:r1}Boolean",
                                "type" : "GreaterOrEqual",
                                "operand" : [ {
                                    "locator" : "8:3-8:14",
                                    "resultTypeName" : "{urn:hl7-org:elm-types:r1}Integer",
                                    "precision" : "Year",
                                    "type" : "CalculateAge",
                                    "operand" : {
                                        "path" : "birthDate.value",
                                        "type" : "Property",
                                        "source" : {
                                            "name" : "Patient",
                                            "type" : "ExpressionRef"
                                        }
                                    }
                                }, {
                                    "locator" : "8:19-8:20",
                                    "resultTypeName" : "{urn:hl7-org:elm-types:r1}Integer",
                                    "valueType" : "{urn:hl7-org:elm-types:r1}Integer",
                                    "value" : "18",
                                    "type" : "Literal"
                                } ]
                                }
                            } ]
                        }
                    }
                }


    4. Clean and Build ELM project. 

        .. note::
    
            PackagerCLI takes the original CQL (from the input folder in step 1) and the ELM (from Json folder from this project from step 2) as input.   
            The generated C# is exported to the root directory of the Measures project and built into a Measures.dll.  
            The generated Library resource ``AgeFHIR4-0.0.1.json`` is stored in the Resources folder in the Test project. 
        
        .. container:: toggle

            .. container:: header

                Click to expand AgeFHIR4-0.0.1.json

            .. code-block:: jsonc 

                {
                    "resourceType": "Library",
                    "id": "AgeFHIR4-0.0.1",
                    "url": "https://fire.ly/fhir/Library/AgeFHIR4-0.0.1",
                    "version": "0.0.1",
                    "name": "AgeFHIR4",
                    "status": "active",
                    "type": {
                        "coding": [
                        {
                            "system": "http://terminology.hl7.org/CodeSystem/library-type",
                            "code": "logic-library"
                        }
                        ]
                    },
                    "date": "2024-02-06T16:04:23.315Z",
                    "parameter": [
                        {
                        "name": "is18OrAbove",
                        "use": "out",
                        "min": 0,
                        "max": "1",
                        "type": "boolean"
                        }
                    ],
                    "content": [
                        {
                        "id": "AgeFHIR4-0.0.1+elm",
                        "contentType": "application/elm+json",
                        "data": "ew0KICAgImxpYnJhcnkiIDogew0KICAgICAgImFubm90YXRpb24iIDogWyB7DQogICAgICAgICAidHJhbnNsYXRvclZlcnNpb24iIDogIjIuMTEuMCIsDQogICAgICAgICAidHJhbnNsYXRvck9wdGlvbnMiIDogIkVuYWJsZUxvY2F0b3JzLEVuYWJsZVJlc3VsdFR5cGVzIiwNCiAgICAgICAgICJ0eXBlIiA6ICJDcWxUb0VsbUluZm8iDQogICAgICB9IF0sDQogICAgICAiaWRlbnRpZmllciIgOiB7DQogICAgICAgICAiaWQiIDogIkFkdWx0Qk1JRkhJUjQiLA0KICAgICAgICAgInZlcnNpb24iIDogIjAuMC4xIg0KICAgICAgfSwNCiAgICAgICJzY2hlbWFJZGVudGlmaWVyIiA6IHsNCiAgICAgICAgICJpZCIgOiAidXJuOmhsNy1vcmc6ZWxtIiwNCiAgICAgICAgICJ2ZXJzaW9uIiA6ICJyMSINCiAgICAgIH0sDQogICAgICAidXNpbmdzIiA6IHsNCiAgICAgICAgICJkZWYiIDogWyB7DQogICAgICAgICAgICAibG9jYWxJZGVudGlmaWVyIiA6ICJTeXN0ZW0iLA0KICAgICAgICAgICAgInVyaSIgOiAidXJuOmhsNy1vcmc6ZWxtLXR5cGVzOnIxIg0KICAgICAgICAgfSwgew0KICAgICAgICAgICAgImxvY2F0b3IiIDogIjM6MS0zOjI2IiwNCiAgICAgICAgICAgICJsb2NhbElkZW50aWZpZXIiIDogIkZISVIiLA0KICAgICAgICAgICAgInVyaSIgOiAiaHR0cDovL2hsNy5vcmcvZmhpciIsDQogICAgICAgICAgICAidmVyc2lvbiIgOiAiNC4wLjEiDQogICAgICAgICB9IF0NCiAgICAgIH0sDQogICAgICAiY29udGV4dHMiIDogew0KICAgICAgICAgImRlZiIgOiBbIHsNCiAgICAgICAgICAgICJsb2NhdG9yIiA6ICI1OjEtNToxNSIsDQogICAgICAgICAgICAibmFtZSIgOiAiUGF0aWVudCINCiAgICAgICAgIH0gXQ0KICAgICAgfSwNCiAgICAgICJzdGF0ZW1lbnRzIiA6IHsNCiAgICAgICAgICJkZWYiIDogWyB7DQogICAgICAgICAgICAibG9jYXRvciIgOiAiNToxLTU6MTUiLA0KICAgICAgICAgICAgIm5hbWUiIDogIlBhdGllbnQiLA0KICAgICAgICAgICAgImNvbnRleHQiIDogIlBhdGllbnQiLA0KICAgICAgICAgICAgImV4cHJlc3Npb24iIDogew0KICAgICAgICAgICAgICAgInR5cGUiIDogIlNpbmdsZXRvbkZyb20iLA0KICAgICAgICAgICAgICAgIm9wZXJhbmQiIDogew0KICAgICAgICAgICAgICAgICAgImxvY2F0b3IiIDogIjU6MS01OjE1IiwNCiAgICAgICAgICAgICAgICAgICJkYXRhVHlwZSIgOiAie2h0dHA6Ly9obDcub3JnL2ZoaXJ9UGF0aWVudCIsDQogICAgICAgICAgICAgICAgICAidGVtcGxhdGVJZCIgOiAiaHR0cDovL2hsNy5vcmcvZmhpci9TdHJ1Y3R1cmVEZWZpbml0aW9uL1BhdGllbnQiLA0KICAgICAgICAgICAgICAgICAgInR5cGUiIDogIlJldHJpZXZlIg0KICAgICAgICAgICAgICAgfQ0KICAgICAgICAgICAgfQ0KICAgICAgICAgfSwgew0KICAgICAgICAgICAgImxvY2F0b3IiIDogIjc6MS04OjIwIiwNCiAgICAgICAgICAgICJyZXN1bHRUeXBlTmFtZSIgOiAie3VybjpobDctb3JnOmVsbS10eXBlczpyMX1Cb29sZWFuIiwNCiAgICAgICAgICAgICJuYW1lIiA6ICJpczE4T3JBYm92ZSIsDQogICAgICAgICAgICAiY29udGV4dCIgOiAiUGF0aWVudCIsDQogICAgICAgICAgICAiYWNjZXNzTGV2ZWwiIDogIlB1YmxpYyIsDQogICAgICAgICAgICAiZXhwcmVzc2lvbiIgOiB7DQogICAgICAgICAgICAgICAibG9jYXRvciIgOiAiODozLTg6MjAiLA0KICAgICAgICAgICAgICAgInJlc3VsdFR5cGVOYW1lIiA6ICJ7dXJuOmhsNy1vcmc6ZWxtLXR5cGVzOnIxfUJvb2xlYW4iLA0KICAgICAgICAgICAgICAgInR5cGUiIDogIkdyZWF0ZXJPckVxdWFsIiwNCiAgICAgICAgICAgICAgICJvcGVyYW5kIiA6IFsgew0KICAgICAgICAgICAgICAgICAgImxvY2F0b3IiIDogIjg6My04OjE0IiwNCiAgICAgICAgICAgICAgICAgICJyZXN1bHRUeXBlTmFtZSIgOiAie3VybjpobDctb3JnOmVsbS10eXBlczpyMX1JbnRlZ2VyIiwNCiAgICAgICAgICAgICAgICAgICJwcmVjaXNpb24iIDogIlllYXIiLA0KICAgICAgICAgICAgICAgICAgInR5cGUiIDogIkNhbGN1bGF0ZUFnZSIsDQogICAgICAgICAgICAgICAgICAib3BlcmFuZCIgOiB7DQogICAgICAgICAgICAgICAgICAgICAicGF0aCIgOiAiYmlydGhEYXRlLnZhbHVlIiwNCiAgICAgICAgICAgICAgICAgICAgICJ0eXBlIiA6ICJQcm9wZXJ0eSIsDQogICAgICAgICAgICAgICAgICAgICAic291cmNlIiA6IHsNCiAgICAgICAgICAgICAgICAgICAgICAgICJuYW1lIiA6ICJQYXRpZW50IiwNCiAgICAgICAgICAgICAgICAgICAgICAgICJ0eXBlIiA6ICJFeHByZXNzaW9uUmVmIg0KICAgICAgICAgICAgICAgICAgICAgfQ0KICAgICAgICAgICAgICAgICAgfQ0KICAgICAgICAgICAgICAgfSwgew0KICAgICAgICAgICAgICAgICAgImxvY2F0b3IiIDogIjg6MTktODoyMCIsDQogICAgICAgICAgICAgICAgICAicmVzdWx0VHlwZU5hbWUiIDogInt1cm46aGw3LW9yZzplbG0tdHlwZXM6cjF9SW50ZWdlciIsDQogICAgICAgICAgICAgICAgICAidmFsdWVUeXBlIiA6ICJ7dXJuOmhsNy1vcmc6ZWxtLXR5cGVzOnIxfUludGVnZXIiLA0KICAgICAgICAgICAgICAgICAgInZhbHVlIiA6ICIxOCIsDQogICAgICAgICAgICAgICAgICAidHlwZSIgOiAiTGl0ZXJhbCINCiAgICAgICAgICAgICAgIH0gXQ0KICAgICAgICAgICAgfQ0KICAgICAgICAgfSBdDQogICAgICB9DQogICB9DQp9DQoNCg=="
                        },
                        {
                        "id": "AgeFHIR4-0.0.1+cql",
                        "contentType": "text/cql",
                        "data": "bGlicmFyeSBBZHVsdEJNSUZISVI0IHZlcnNpb24gJzAuMC4xJw0KDQp1c2luZyBGSElSIHZlcnNpb24gJzQuMC4xJw0KDQpjb250ZXh0IFBhdGllbnQNCg0KZGVmaW5lICJpczE4T3JBYm92ZSI6DQogIEFnZUluWWVhcnMoKSA+PSAxOA=="
                        },
                        {
                        "id": "AgeFHIR4-0.0.1+dll",
                        "contentType": "application/octet-stream",
                        "data": "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wAAAAAAAAQAAAAAAAAAAAAAAAADsAgAAAAABAAAAAAAAAAAAAAAAAEADAAAAACMAbwAlAG8ALQChAAAAAEFkdWx0Qk1JRkhJUjQtMC4wLjEAQWR1bHRCTUlGSElSNF8wXzBfMQBGdW5jYDEATnVsbGFibGVgMQBJRW51bWVyYWJsZWAxAExhenlgMQBJbnQzMgBIbDcuRmhpci5SNAA8TW9kdWxlPgBTeXN0ZW0uUHJpdmF0ZS5Db3JlTGliAFN5c3RlbS5Db2xsZWN0aW9ucy5HZW5lcmljAENhbGN1bGF0ZUFnZQBSYW5nZQBIbDcuQ3FsLlJ1bnRpbWUASGw3LkZoaXIuQmFzZQBDcWxEYXRlAEVtYmVkZGVkQXR0cmlidXRlAENvbXBpbGVyR2VuZXJhdGVkQXR0cmlidXRlAEdlbmVyYXRlZENvZGVBdHRyaWJ1dGUAQXR0cmlidXRlVXNhZ2VBdHRyaWJ1dGUARGVidWdnYWJsZUF0dHJpYnV0ZQBDcWxEZWNsYXJhdGlvbkF0dHJpYnV0ZQBSZWZTYWZldHlSdWxlc0F0dHJpYnV0ZQBDb21waWxhdGlvblJlbGF4YXRpb25zQXR0cmlidXRlAENxbExpYnJhcnlBdHRyaWJ1dGUAUnVudGltZUNvbXBhdGliaWxpdHlBdHRyaWJ1dGUAaXMxOE9yQWJvdmVfVmFsdWUAZ2V0X1ZhbHVlAFBhdGllbnRfVmFsdWUAX19pczE4T3JBYm92ZQBUYXNrAEdyZWF0ZXJPckVxdWFsAEhsNy5GaGlyLk1vZGVsAEFkdWx0Qk1JRkhJUjQtMC4wLjEuZGxsAFNpbmdsZU9yTnVsbABTeXN0ZW0AVmVyc2lvbgBTeXN0ZW0uUmVmbGVjdGlvbgBBcmd1bWVudE51bGxFeGNlcHRpb24AUHJvcGVydHlJbmZvAFN5c3RlbS5Db2RlRG9tLkNvbXBpbGVyAC5jdG9yAFN5c3RlbS5EaWFnbm9zdGljcwBTeXN0ZW0uUnVudGltZS5Db21waWxlclNlcnZpY2VzAERlYnVnZ2luZ01vZGVzAEhsNy5DcWwuUHJpbWl0aXZlcwBNaWNyb3NvZnQuQ29kZUFuYWx5c2lzAEhMNy5DcWwuQWJzdHJhY3Rpb25zAEhsNy5DcWwuQWJzdHJhY3Rpb25zAEhsNy5DcWwuT3BlcmF0b3JzAGdldF9PcGVyYXRvcnMASUNxbE9wZXJhdG9ycwBBdHRyaWJ1dGVUYXJnZXRzAE9iamVjdABDcWxWYWx1ZVNldABSZXRyaWV2ZUJ5VmFsdWVTZXQAX19QYXRpZW50AGdldF9CaXJ0aERhdGVFbGVtZW50AENvbnZlcnQAQ3FsQ29udGV4dABjb250ZXh0AAAAD2MAbwBuAHQAZQB4AHQAAAl5AGUAYQByAAAAbGP+Akd8gkaIY0FKJaNJqQAEIAEBCAMgAAEFIAEBEREFIAIBDg4FIAEBESEEIAEBDgYVElkBEjkFIAIBHBgGFRI1ARI5CSABARUSWQETAAkVElkBFRE9AQIJFRI1ARURPQECCAcBFRJJARI5BCAAEl0NMAECFRJJAR4AEmESZQQKARI5CzABAR4AFRJJAR4ABCAAEwALBwMSORJRFRE9AQgEIAASaQMgAA4GMAEBHgAcBAoBElEKIAIVET0BCBJRDgUVET0BCAkgAhURPQECHBwIfOyF176neY4I1waRFIBVD8MCBggDBhIxBwYVEjUBEjkKBhUSNQEVET0BAgUgAQESMQQgABI5ByAAFRE9AQIIAQAIAAAAAAAeAQABAFQCFldyYXBOb25FeGNlcHRpb25UaHJvd3MBCAEAAgAAAAAAGAEADUFkdWx0Qk1JRkhJUjQFMC4wLjEAAAwBAAdQYXRpZW50AAAQAQALaXMxOE9yQWJvdmUAAAQBAAAAJgEAAgAAAAIAVAINQWxsb3dNdWx0aXBsZQBUAglJbmhlcml0ZWQAIQEAFC5ORVQgQ29kZSBHZW5lcmF0aW9uBzEuMC4wLjAAAAgBAAsAAAAAAAA4KwAAAAAAAAAAAABSKwAAACAAAAAAAAAAAAAAAAAAAAAAAAAAAAAARCsAAAAAAAAAAAAAAABfQ29yRGxsTWFpbgBtc2NvcmVlLmRsbAAAAAAA/yUAIAAQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAIAAADAAAAGQ7AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=="
                        },
                        {
                        "id": "AgeFHIR4-0.0.1+csharp",
                        "contentType": "text/plain",
                        "data": "dXNpbmcgU3lzdGVtOw0KdXNpbmcgU3lzdGVtLkxpbnE7DQp1c2luZyBTeXN0ZW0uQ29sbGVjdGlvbnMuR2VuZXJpYzsNCnVzaW5nIEhsNy5DcWwuUnVudGltZTsNCnVzaW5nIEhsNy5DcWwuUHJpbWl0aXZlczsNCnVzaW5nIEhsNy5DcWwuQWJzdHJhY3Rpb25zOw0KdXNpbmcgSGw3LkNxbC5WYWx1ZVNldHM7DQp1c2luZyBIbDcuQ3FsLklzbzg2MDE7DQp1c2luZyBIbDcuRmhpci5Nb2RlbDsNCnVzaW5nIFJhbmdlID0gSGw3LkZoaXIuTW9kZWwuUmFuZ2U7DQp1c2luZyBUYXNrID0gSGw3LkZoaXIuTW9kZWwuVGFzazsNCltTeXN0ZW0uQ29kZURvbS5Db21waWxlci5HZW5lcmF0ZWRDb2RlKCIuTkVUIENvZGUgR2VuZXJhdGlvbiIsICIxLjAuMC4wIildDQpbQ3FsTGlicmFyeSgiQWR1bHRCTUlGSElSNCIsICIwLjAuMSIpXQ0KcHVibGljIGNsYXNzIEFkdWx0Qk1JRkhJUjRfMF8wXzENCnsNCg0KDQogICAgaW50ZXJuYWwgQ3FsQ29udGV4dCBjb250ZXh0Ow0KDQogICAgI3JlZ2lvbiBDYWNoZWQgdmFsdWVzDQoNCiAgICBpbnRlcm5hbCBMYXp5PFBhdGllbnQ+IF9fUGF0aWVudDsNCiAgICBpbnRlcm5hbCBMYXp5PGJvb2w/PiBfX2lzMThPckFib3ZlOw0KDQogICAgI2VuZHJlZ2lvbg0KICAgIHB1YmxpYyBBZHVsdEJNSUZISVI0XzBfMF8xKENxbENvbnRleHQgY29udGV4dCkNCiAgICB7DQogICAgICAgIHRoaXMuY29udGV4dCA9IGNvbnRleHQgPz8gdGhyb3cgbmV3IEFyZ3VtZW50TnVsbEV4Y2VwdGlvbigiY29udGV4dCIpOw0KDQoNCiAgICAgICAgX19QYXRpZW50ID0gbmV3IExhenk8UGF0aWVudD4odGhpcy5QYXRpZW50X1ZhbHVlKTsNCiAgICAgICAgX19pczE4T3JBYm92ZSA9IG5ldyBMYXp5PGJvb2w/Pih0aGlzLmlzMThPckFib3ZlX1ZhbHVlKTsNCiAgICB9DQogICAgI3JlZ2lvbiBEZXBlbmRlbmNpZXMNCg0KDQogICAgI2VuZHJlZ2lvbg0KDQoJcHJpdmF0ZSBQYXRpZW50IFBhdGllbnRfVmFsdWUoKQ0KCXsNCgkJdmFyIGFfID0gY29udGV4dC5PcGVyYXRvcnMuUmV0cmlldmVCeVZhbHVlU2V0PFBhdGllbnQ+KG51bGwsIG51bGwpOw0KCQl2YXIgYl8gPSBjb250ZXh0Lk9wZXJhdG9ycy5TaW5nbGVPck51bGw8UGF0aWVudD4oYV8pOw0KDQoJCXJldHVybiBiXzsNCgl9DQoNCiAgICBbQ3FsRGVjbGFyYXRpb24oIlBhdGllbnQiKV0NCglwdWJsaWMgUGF0aWVudCBQYXRpZW50KCkgPT4gDQoJCV9fUGF0aWVudC5WYWx1ZTsNCg0KCXByaXZhdGUgYm9vbD8gaXMxOE9yQWJvdmVfVmFsdWUoKQ0KCXsNCgkJdmFyIGFfID0gdGhpcy5QYXRpZW50KCk7DQoJCXZhciBiXyA9IGNvbnRleHQuT3BlcmF0b3JzLkNvbnZlcnQ8Q3FsRGF0ZT4oYV8/LkJpcnRoRGF0ZUVsZW1lbnQ/LlZhbHVlKTsNCgkJdmFyIGNfID0gY29udGV4dC5PcGVyYXRvcnMuQ2FsY3VsYXRlQWdlKGJfLCAieWVhciIpOw0KCQl2YXIgZF8gPSBjb250ZXh0Lk9wZXJhdG9ycy5HcmVhdGVyT3JFcXVhbChjXywgKGludD8pMTgpOw0KDQoJCXJldHVybiBkXzsNCgl9DQoNCiAgICBbQ3FsRGVjbGFyYXRpb24oImlzMThPckFib3ZlIildDQoJcHVibGljIGJvb2w/IGlzMThPckFib3ZlKCkgPT4gDQoJCV9faXMxOE9yQWJvdmUuVmFsdWU7DQoNCn0="
                        }
                    ]
                }

Firely Server CQL Plugin 
------------------------

This chapter details setting up Firely ServerCQL Plugin and provides you with an example to get some hands-on experience with CQL library with Postman tool. 

 
* Getting Started 
* CQL Plugin Configuration 
* Using Postman 

Getting Started
^^^^^^^^^^^^^^^^

* Install Firely Server using `Basic installation — Firely Server documentation <https://docs.fire.ly/projects/Firely-Server/en/latest/getting_started/basic_installation.html>`_

    .. warning::
        Make sure that the license contains CQL Plugin. 

CQL Plugin Configuration in Firely Server 
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
* Edit appsettings.instance.json:

    1. Set Repository to ``SQL`` and add the database configuration 
    2. Add ``$evaluate-measure`` to the ``SupportedInteractions.TypeLevelInteractions``
    3. Ensure ``$everything`` is present in the ``SupportedInteractions.InstanceLevelInteractions``
    4. Insert ``Vonk.Plugin.Cql`` to PipelineOptions.Branches[Path="/"].Include before any other plugin 
    5. Uncomment ``Vonk.Plugin.PatientEverything`` from the same list PipelineOptions.Branches[Path="/"].Include. 

* Build and run the Vonk.Server project from your IDE 

Using Postman 
^^^^^^^^^^^^^
CQL Plugin in Firely Server uses the ``$evaluate-measure`` operation to execute a CQL measure on a resource.
To verify the result of our CQL measure, we use Postman tool

* Set up Postman with environment variable : baseUrl and point to http://localhost:4080

    .. note::
        Make sure that the capability statement contains a Library operation "https:/fire.ly/fhir/OperationDefinition/Library-evaluate-measure"
            
            .. code-block::

                GET {{baseUrl}}/metadata

        This confirms that the CQL Plugin is enabled for the Firely Server

* Create a Postman Collection with the following inputs : 

    1.  Create a Library resource with the Library resource json file ``AgeFHIR4-0.0.1.json``.
            
            .. code-block:: 

                POST {{baseUrl}}/administration/Library

        Store the ``url``
    2.  Create a Support File with a binary file (todo : Explain this)
    3.  Create a Patient resource with age over 18 years

         .. code-block:: 

                POST {{baseUrl}}/Patient
    
        .. code-block:: jsonc

            {
                "resourceType": "Patient",
                "active": true,
                "gender": "male",
                "birthDate": "1967-11-06",
                "name": [
                    {
                        "use": "official",
                        "family": "Smith",
                        "given": [
                            "John"
                        ]
                    }
                ]
            }

        Store the ``id`` of the patient resource created.
        
    4.  Create a POST request for $evaluate-measure operation on the Library resource created

        .. code-block:: 

            POST {{baseUrl}}/Library/$evaluate-measure

        .. code-block:: jsonc

            {
                "resourceType": "Parameters",
                "parameter": [
                    {
                        "name": "url",
                        "valueCanonical": "https://fire.ly/fhir/Library/AgeFHIR4-0.0.1"
                    },
                    {
                        "name": "version",
                        "valueString": "0.0.1"
                    },
                    {
                        "name": "subject",
                        "valueString": "Patient/patient-id"
                    },
                    {
                        "name": "periodStart",
                        "valueDate": "2023-01-01"
                    },
                    {
                        "name": "periodEnd",
                        "valueDate": "2023-12-01"
                    }
                ]
            }

        ``valueCanonical`` of the url comes from the ``url`` of the library resource in Step 1 and the ``patient-id`` comes from the ``id`` of the patient created in Step 3
    5.  Get the result of the $evaluate-measure by passing the parameters.
        .. code-block:: 

            GET {{baseUrl}}/Library/$evaluate-measure

        .. image:: ../images/postman_cql_evaluatemeasure_parameters.png
            :width: 1000px
            :alt: Illustration of paramters 

    6. Results

       .. code-block:: 

            Results here #todo API response and Decoded results
            The result displays if the Patient is evaluated as true or false for Age over 18 years










 
