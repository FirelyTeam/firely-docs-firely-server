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

    1. Follow the step by step `Installation of the CQL Packager tool and use the Demo solution <https://github.com/FirelyTeam/firely-cql-sdk/blob/develop/README.md>`_ to generate a resource file.
    2. Place the CQL file ``Age.cql`` in the folder ``..\firely-cql-sdk\Demo\Cql\input``
    3. From the Demo Solution, Clean and Build CQL folder 

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

    4. From the Demo Solution, Clean and Build Elm folder 

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
                    "date": "2024-03-13T09:28:27.954Z",
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
                        "data": "ew0KICAgImxpYnJhcnkiIDogew0KICAgICAgImFubm90YXRpb24iIDogWyB7DQogICAgICAgICAidHJhbnNsYXRvclZlcnNpb24iIDogIjIuMTEuMCIsDQogICAgICAgICAidHJhbnNsYXRvck9wdGlvbnMiIDogIkVuYWJsZUxvY2F0b3JzLEVuYWJsZVJlc3VsdFR5cGVzIiwNCiAgICAgICAgICJ0eXBlIiA6ICJDcWxUb0VsbUluZm8iDQogICAgICB9IF0sDQogICAgICAiaWRlbnRpZmllciIgOiB7DQogICAgICAgICAiaWQiIDogIkFnZUZISVI0IiwNCiAgICAgICAgICJ2ZXJzaW9uIiA6ICIwLjAuMSINCiAgICAgIH0sDQogICAgICAic2NoZW1hSWRlbnRpZmllciIgOiB7DQogICAgICAgICAiaWQiIDogInVybjpobDctb3JnOmVsbSIsDQogICAgICAgICAidmVyc2lvbiIgOiAicjEiDQogICAgICB9LA0KICAgICAgInVzaW5ncyIgOiB7DQogICAgICAgICAiZGVmIiA6IFsgew0KICAgICAgICAgICAgImxvY2FsSWRlbnRpZmllciIgOiAiU3lzdGVtIiwNCiAgICAgICAgICAgICJ1cmkiIDogInVybjpobDctb3JnOmVsbS10eXBlczpyMSINCiAgICAgICAgIH0sIHsNCiAgICAgICAgICAgICJsb2NhdG9yIiA6ICIzOjEtMzoyNiIsDQogICAgICAgICAgICAibG9jYWxJZGVudGlmaWVyIiA6ICJGSElSIiwNCiAgICAgICAgICAgICJ1cmkiIDogImh0dHA6Ly9obDcub3JnL2ZoaXIiLA0KICAgICAgICAgICAgInZlcnNpb24iIDogIjQuMC4xIg0KICAgICAgICAgfSBdDQogICAgICB9LA0KICAgICAgImNvbnRleHRzIiA6IHsNCiAgICAgICAgICJkZWYiIDogWyB7DQogICAgICAgICAgICAibG9jYXRvciIgOiAiNToxLTU6MTUiLA0KICAgICAgICAgICAgIm5hbWUiIDogIlBhdGllbnQiDQogICAgICAgICB9IF0NCiAgICAgIH0sDQogICAgICAic3RhdGVtZW50cyIgOiB7DQogICAgICAgICAiZGVmIiA6IFsgew0KICAgICAgICAgICAgImxvY2F0b3IiIDogIjU6MS01OjE1IiwNCiAgICAgICAgICAgICJuYW1lIiA6ICJQYXRpZW50IiwNCiAgICAgICAgICAgICJjb250ZXh0IiA6ICJQYXRpZW50IiwNCiAgICAgICAgICAgICJleHByZXNzaW9uIiA6IHsNCiAgICAgICAgICAgICAgICJ0eXBlIiA6ICJTaW5nbGV0b25Gcm9tIiwNCiAgICAgICAgICAgICAgICJvcGVyYW5kIiA6IHsNCiAgICAgICAgICAgICAgICAgICJsb2NhdG9yIiA6ICI1OjEtNToxNSIsDQogICAgICAgICAgICAgICAgICAiZGF0YVR5cGUiIDogIntodHRwOi8vaGw3Lm9yZy9maGlyfVBhdGllbnQiLA0KICAgICAgICAgICAgICAgICAgInRlbXBsYXRlSWQiIDogImh0dHA6Ly9obDcub3JnL2ZoaXIvU3RydWN0dXJlRGVmaW5pdGlvbi9QYXRpZW50IiwNCiAgICAgICAgICAgICAgICAgICJ0eXBlIiA6ICJSZXRyaWV2ZSINCiAgICAgICAgICAgICAgIH0NCiAgICAgICAgICAgIH0NCiAgICAgICAgIH0sIHsNCiAgICAgICAgICAgICJsb2NhdG9yIiA6ICI3OjEtODoyMCIsDQogICAgICAgICAgICAicmVzdWx0VHlwZU5hbWUiIDogInt1cm46aGw3LW9yZzplbG0tdHlwZXM6cjF9Qm9vbGVhbiIsDQogICAgICAgICAgICAibmFtZSIgOiAiaXMxOE9yQWJvdmUiLA0KICAgICAgICAgICAgImNvbnRleHQiIDogIlBhdGllbnQiLA0KICAgICAgICAgICAgImFjY2Vzc0xldmVsIiA6ICJQdWJsaWMiLA0KICAgICAgICAgICAgImV4cHJlc3Npb24iIDogew0KICAgICAgICAgICAgICAgImxvY2F0b3IiIDogIjg6My04OjIwIiwNCiAgICAgICAgICAgICAgICJyZXN1bHRUeXBlTmFtZSIgOiAie3VybjpobDctb3JnOmVsbS10eXBlczpyMX1Cb29sZWFuIiwNCiAgICAgICAgICAgICAgICJ0eXBlIiA6ICJHcmVhdGVyT3JFcXVhbCIsDQogICAgICAgICAgICAgICAib3BlcmFuZCIgOiBbIHsNCiAgICAgICAgICAgICAgICAgICJsb2NhdG9yIiA6ICI4OjMtODoxNCIsDQogICAgICAgICAgICAgICAgICAicmVzdWx0VHlwZU5hbWUiIDogInt1cm46aGw3LW9yZzplbG0tdHlwZXM6cjF9SW50ZWdlciIsDQogICAgICAgICAgICAgICAgICAicHJlY2lzaW9uIiA6ICJZZWFyIiwNCiAgICAgICAgICAgICAgICAgICJ0eXBlIiA6ICJDYWxjdWxhdGVBZ2UiLA0KICAgICAgICAgICAgICAgICAgIm9wZXJhbmQiIDogew0KICAgICAgICAgICAgICAgICAgICAgInBhdGgiIDogImJpcnRoRGF0ZS52YWx1ZSIsDQogICAgICAgICAgICAgICAgICAgICAidHlwZSIgOiAiUHJvcGVydHkiLA0KICAgICAgICAgICAgICAgICAgICAgInNvdXJjZSIgOiB7DQogICAgICAgICAgICAgICAgICAgICAgICAibmFtZSIgOiAiUGF0aWVudCIsDQogICAgICAgICAgICAgICAgICAgICAgICAidHlwZSIgOiAiRXhwcmVzc2lvblJlZiINCiAgICAgICAgICAgICAgICAgICAgIH0NCiAgICAgICAgICAgICAgICAgIH0NCiAgICAgICAgICAgICAgIH0sIHsNCiAgICAgICAgICAgICAgICAgICJsb2NhdG9yIiA6ICI4OjE5LTg6MjAiLA0KICAgICAgICAgICAgICAgICAgInJlc3VsdFR5cGVOYW1lIiA6ICJ7dXJuOmhsNy1vcmc6ZWxtLXR5cGVzOnIxfUludGVnZXIiLA0KICAgICAgICAgICAgICAgICAgInZhbHVlVHlwZSIgOiAie3VybjpobDctb3JnOmVsbS10eXBlczpyMX1JbnRlZ2VyIiwNCiAgICAgICAgICAgICAgICAgICJ2YWx1ZSIgOiAiMTgiLA0KICAgICAgICAgICAgICAgICAgInR5cGUiIDogIkxpdGVyYWwiDQogICAgICAgICAgICAgICB9IF0NCiAgICAgICAgICAgIH0NCiAgICAgICAgIH0gXQ0KICAgICAgfQ0KICAgfQ0KfQ0KDQo="
                        },
                        {
                        "id": "AgeFHIR4-0.0.1+cql",
                        "contentType": "text/cql",
                        "data": "bGlicmFyeSBBZ2VGSElSNCB2ZXJzaW9uICcwLjAuMScNCg0KdXNpbmcgRkhJUiB2ZXJzaW9uICc0LjAuMScNCg0KY29udGV4dCBQYXRpZW50DQoNCmRlZmluZSAiaXMxOE9yQWJvdmUiOg0KICBBZ2VJblllYXJzKCkgPj0gMTg="
                        },
                        {
                        "id": "AgeFHIR4-0.0.1+dll",
                        "contentType": "application/octet-stream",
                        "data": "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tgAAAAAAAQAAAAAAAAAAAAAAAADdAgAAAAABAAAAAAAAAAAAAAAAADEDAAAAACMAbwAlAG8ALQChAAAAAEFnZUZISVI0LTAuMC4xAEFnZUZISVI0XzBfMF8xAEZ1bmNgMQBOdWxsYWJsZWAxAElFbnVtZXJhYmxlYDEATGF6eWAxAEludDMyAEhsNy5GaGlyLlI0ADxNb2R1bGU+AFN5c3RlbS5Qcml2YXRlLkNvcmVMaWIAU3lzdGVtLkNvbGxlY3Rpb25zLkdlbmVyaWMAQ2FsY3VsYXRlQWdlAFJhbmdlAEhsNy5DcWwuUnVudGltZQBIbDcuRmhpci5CYXNlAENxbERhdGUARW1iZWRkZWRBdHRyaWJ1dGUAQ29tcGlsZXJHZW5lcmF0ZWRBdHRyaWJ1dGUAR2VuZXJhdGVkQ29kZUF0dHJpYnV0ZQBBdHRyaWJ1dGVVc2FnZUF0dHJpYnV0ZQBEZWJ1Z2dhYmxlQXR0cmlidXRlAENxbERlY2xhcmF0aW9uQXR0cmlidXRlAFJlZlNhZmV0eVJ1bGVzQXR0cmlidXRlAENvbXBpbGF0aW9uUmVsYXhhdGlvbnNBdHRyaWJ1dGUAQ3FsTGlicmFyeUF0dHJpYnV0ZQBSdW50aW1lQ29tcGF0aWJpbGl0eUF0dHJpYnV0ZQBpczE4T3JBYm92ZV9WYWx1ZQBnZXRfVmFsdWUAUGF0aWVudF9WYWx1ZQBfX2lzMThPckFib3ZlAFRhc2sAR3JlYXRlck9yRXF1YWwASGw3LkZoaXIuTW9kZWwAQWdlRkhJUjQtMC4wLjEuZGxsAFNpbmdsZU9yTnVsbABTeXN0ZW0AVmVyc2lvbgBTeXN0ZW0uUmVmbGVjdGlvbgBBcmd1bWVudE51bGxFeGNlcHRpb24AUHJvcGVydHlJbmZvAFN5c3RlbS5Db2RlRG9tLkNvbXBpbGVyAC5jdG9yAFN5c3RlbS5EaWFnbm9zdGljcwBTeXN0ZW0uUnVudGltZS5Db21waWxlclNlcnZpY2VzAERlYnVnZ2luZ01vZGVzAEhsNy5DcWwuUHJpbWl0aXZlcwBNaWNyb3NvZnQuQ29kZUFuYWx5c2lzAEhMNy5DcWwuQWJzdHJhY3Rpb25zAEhsNy5DcWwuQWJzdHJhY3Rpb25zAEhsNy5DcWwuT3BlcmF0b3JzAGdldF9PcGVyYXRvcnMASUNxbE9wZXJhdG9ycwBBdHRyaWJ1dGVUYXJnZXRzAE9iamVjdABDcWxWYWx1ZVNldABSZXRyaWV2ZUJ5VmFsdWVTZXQAX19QYXRpZW50AGdldF9CaXJ0aERhdGVFbGVtZW50AENvbnZlcnQAQ3FsQ29udGV4dABjb250ZXh0AAAPYwBvAG4AdABlAHgAdAAACXkAZQBhAHIAAAAi82YtLiFBSYGY7zcwkrUKAAQgAQEIAyAAAQUgAQEREQUgAgEODgUgAQERIQQgAQEOBhUSWQESOQUgAgEcGAYVEjUBEjkJIAEBFRJZARMACRUSWQEVET0BAgkVEjUBFRE9AQIIBwEVEkkBEjkEIAASXQ0wAQIVEkkBHgASYRJlBAoBEjkLMAEBHgAVEkkBHgAEIAATAAsHAxI5ElEVET0BCAQgABJpAyAADgYwAQEeABwECgESUQogAhURPQEIElEOBRURPQEICSACFRE9AQIcHAh87IXXvqd5jgjXBpEUgFUPwwIGCAMGEjEHBhUSNQESOQoGFRI1ARURPQECBSABARIxBCAAEjkHIAAVET0BAggBAAgAAAAAAB4BAAEAVAIWV3JhcE5vbkV4Y2VwdGlvblRocm93cwEIAQACAAAAAAATAQAIQWdlRkhJUjQFMC4wLjEAAAwBAAdQYXRpZW50AAAQAQALaXMxOE9yQWJvdmUAAAQBAAAAJgEAAgAAAAIAVAINQWxsb3dNdWx0aXBsZQBUAglJbmhlcml0ZWQAIQEAFC5ORVQgQ29kZSBHZW5lcmF0aW9uBzEuMC4wLjAAAAgBAAsAAAAAAAAAJCsAAAAAAAAAAAAAPisAAAAgAAAAAAAAAAAAAAAAAAAAAAAAAAAAADArAAAAAAAAAAAAAAAAX0NvckRsbE1haW4AbXNjb3JlZS5kbGwAAAAAAP8lACAAEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAIAAADAAAAFA7AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=="
                        },
                        {
                        "id": "AgeFHIR4-0.0.1+csharp",
                        "contentType": "text/plain",
                        "data": "dXNpbmcgU3lzdGVtOw0KdXNpbmcgU3lzdGVtLkxpbnE7DQp1c2luZyBTeXN0ZW0uQ29sbGVjdGlvbnMuR2VuZXJpYzsNCnVzaW5nIEhsNy5DcWwuUnVudGltZTsNCnVzaW5nIEhsNy5DcWwuUHJpbWl0aXZlczsNCnVzaW5nIEhsNy5DcWwuQWJzdHJhY3Rpb25zOw0KdXNpbmcgSGw3LkNxbC5WYWx1ZVNldHM7DQp1c2luZyBIbDcuQ3FsLklzbzg2MDE7DQp1c2luZyBIbDcuRmhpci5Nb2RlbDsNCnVzaW5nIFJhbmdlID0gSGw3LkZoaXIuTW9kZWwuUmFuZ2U7DQp1c2luZyBUYXNrID0gSGw3LkZoaXIuTW9kZWwuVGFzazsNCltTeXN0ZW0uQ29kZURvbS5Db21waWxlci5HZW5lcmF0ZWRDb2RlKCIuTkVUIENvZGUgR2VuZXJhdGlvbiIsICIxLjAuMC4wIildDQpbQ3FsTGlicmFyeSgiQWdlRkhJUjQiLCAiMC4wLjEiKV0NCnB1YmxpYyBjbGFzcyBBZ2VGSElSNF8wXzBfMQ0Kew0KDQoNCiAgICBpbnRlcm5hbCBDcWxDb250ZXh0IGNvbnRleHQ7DQoNCiAgICAjcmVnaW9uIENhY2hlZCB2YWx1ZXMNCg0KICAgIGludGVybmFsIExhenk8UGF0aWVudD4gX19QYXRpZW50Ow0KICAgIGludGVybmFsIExhenk8Ym9vbD8+IF9faXMxOE9yQWJvdmU7DQoNCiAgICAjZW5kcmVnaW9uDQogICAgcHVibGljIEFnZUZISVI0XzBfMF8xKENxbENvbnRleHQgY29udGV4dCkNCiAgICB7DQogICAgICAgIHRoaXMuY29udGV4dCA9IGNvbnRleHQgPz8gdGhyb3cgbmV3IEFyZ3VtZW50TnVsbEV4Y2VwdGlvbigiY29udGV4dCIpOw0KDQoNCiAgICAgICAgX19QYXRpZW50ID0gbmV3IExhenk8UGF0aWVudD4odGhpcy5QYXRpZW50X1ZhbHVlKTsNCiAgICAgICAgX19pczE4T3JBYm92ZSA9IG5ldyBMYXp5PGJvb2w/Pih0aGlzLmlzMThPckFib3ZlX1ZhbHVlKTsNCiAgICB9DQogICAgI3JlZ2lvbiBEZXBlbmRlbmNpZXMNCg0KDQogICAgI2VuZHJlZ2lvbg0KDQoJcHJpdmF0ZSBQYXRpZW50IFBhdGllbnRfVmFsdWUoKQ0KCXsNCgkJdmFyIGFfID0gY29udGV4dC5PcGVyYXRvcnMuUmV0cmlldmVCeVZhbHVlU2V0PFBhdGllbnQ+KG51bGwsIG51bGwpOw0KCQl2YXIgYl8gPSBjb250ZXh0Lk9wZXJhdG9ycy5TaW5nbGVPck51bGw8UGF0aWVudD4oYV8pOw0KDQoJCXJldHVybiBiXzsNCgl9DQoNCiAgICBbQ3FsRGVjbGFyYXRpb24oIlBhdGllbnQiKV0NCglwdWJsaWMgUGF0aWVudCBQYXRpZW50KCkgPT4gDQoJCV9fUGF0aWVudC5WYWx1ZTsNCg0KCXByaXZhdGUgYm9vbD8gaXMxOE9yQWJvdmVfVmFsdWUoKQ0KCXsNCgkJdmFyIGFfID0gdGhpcy5QYXRpZW50KCk7DQoJCXZhciBiXyA9IGNvbnRleHQuT3BlcmF0b3JzLkNvbnZlcnQ8Q3FsRGF0ZT4oYV8/LkJpcnRoRGF0ZUVsZW1lbnQ/LlZhbHVlKTsNCgkJdmFyIGNfID0gY29udGV4dC5PcGVyYXRvcnMuQ2FsY3VsYXRlQWdlKGJfLCAieWVhciIpOw0KCQl2YXIgZF8gPSBjb250ZXh0Lk9wZXJhdG9ycy5HcmVhdGVyT3JFcXVhbChjXywgKGludD8pMTgpOw0KDQoJCXJldHVybiBkXzsNCgl9DQoNCiAgICBbQ3FsRGVjbGFyYXRpb24oImlzMThPckFib3ZlIildDQoJcHVibGljIGJvb2w/IGlzMThPckFib3ZlKCkgPT4gDQoJCV9faXMxOE9yQWJvdmUuVmFsdWU7DQoNCn0="
                        }
                    ]
                }

Firely Server CQL Plugin 
------------------------

This chapter details setting up Firely Server CQL Plugin and provides you with an example to get some hands-on experience with CQL library with Postman tool. 
 
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

Create a postman collection
    #. Set up Postman with variable : baseUrl and point to http://localhost:4080

        .. note::
            Make sure that the capability statement contains a Library operation ``https:/fire.ly/fhir/OperationDefinition/Library-evaluate-measure``. 
            This confirms that the CQL Plugin is enabled for the Firely Server.

            Request :    
                .. code-block::

                    GET {{baseUrl}}/metadata

    #.  Create a Library resource with the Library resource json file ``AgeFHIR4-0.0.1.json``.

        Request :    
            .. code-block:: 

                POST {{baseUrl}}/administration/Library

        Store the ``url``
    #.  Create a Support File with a binary file (todo : Explain this)
    #.  Create a Patient resource with age over 18 years

        Request :
            .. code-block:: 

                POST {{baseUrl}}/Patient
        
        Request Body : 
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
        
    #.  Create a POST request for $evaluate-measure operation on the Library resource created

        Request :
            .. code-block:: 

                POST {{baseUrl}}/Library/$evaluate-measure

        Request Body :
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

        Response :
            .. code-block:: jsonc

                {
                    "resourceType": "Binary",
                    "contentType": "application/json",
                    "id": "1aa7aa74-73cd-40a5-b1e9-30aaa370cb19",
                    "data": "eyJQYXRpZW50Ijp7InJlc291cmNlVHlwZSI6IlBhdGllbnQiLCJpZCI6IjFjMjI1NzVmLWE5Y2QtNGI4OS05YTI3LTk2M2VmNmU3Y2QwNyIsIm1ldGEiOnsidmVyc2lvbklkIjoiN2VjMzkzYjItOTE1ZS00MTYyLTk1YzEtYjc4Y2M4YjE5MjhlIiwibGFzdFVwZGF0ZWQiOiIyMDI0LTAzLTEzVDEyOjQyOjAyLjU4MiswMDowMCJ9LCJhY3RpdmUiOnRydWUsIm5hbWUiOlt7InVzZSI6Im9mZmljaWFsIiwiZmFtaWx5IjoiU21pdGgiLCJnaXZlbiI6WyJKb2huIl19XSwiZ2VuZGVyIjoibWFsZSIsImJpcnRoRGF0ZSI6IjIwMTgtMTEtMDYifSwiaXMxOE9yQWJvdmUiOmZhbHNlfQ=="
                }

    #.  Get the result of the $evaluate-measure by passing the parameters.
        
        Request :
            .. code-block:: 

                GET {{baseUrl}}/Library/$evaluate-measure

        .. image:: ../images/postman_cql_evaluate_measure_parameters.png
            :width: 1000px
            :alt: Illustration of parameters 

        Response :
            .. code-block:: 

                    {
                        "Patient": {
                            "resourceType": "Patient",
                            "id": "1c22575f-a9cd-4b89-9a27-963ef6e7cd07",
                            "meta": {
                                "versionId": "7ec393b2-915e-4162-95c1-b78cc8b1928e",
                                "lastUpdated": "2024-03-13T12:42:02.582+00:00"
                            },
                            "active": true,
                            "name": [
                                {
                                    "use": "official",
                                    "family": "Smith",
                                    "given": [
                                        "John"
                                    ]
                                }
                            ],
                            "gender": "male",
                            "birthDate": "1967-11-06"
                        },
                        "is18OrAbove": true
                    }

        The result displays if the Patient is evaluated as ``true`` or ``false`` for the CQL measure, in this case 18 years or above.

Fork an existing collection:
    #. Click the following "Fork postman collection into your workspace" link:

        .. raw:: html

            <div class="postman-run-button"
            data-postman-action="collection/fork"
            data-postman-visibility="public"
            data-postman-var-1="30387478-0705673a-2737-4ebd-9c59-3b53aeb1878d"
            data-postman-collection-url="entityId=30387478-0705673a-2737-4ebd-9c59-3b53aeb1878d&entityType=collection&workspaceId=822b68d8-7e7d-4b09-b8f1-68362070f0bd"></div>
            <script type="text/javascript">
            (function (p,o,s,t,m,a,n) {
                !p[s] && (p[s] = function () { (p[t] || (p[t] = [])).push(arguments); });
                !o.getElementById(s+t) && o.getElementsByTagName("head")[0].appendChild((
                (n = o.createElement("script")),
                (n.id = s+t), (n.async = 1), (n.src = m), n
                ));
            }(window, document, "_pm", "PostmanRunObject", "https://run.pstmn.io/button.js"));
            </script>

    #. Click "Fork Collection"

        .. image:: ../images/Compliance_ForkTestCollectionPostman.png
            :align: center
            :width: 500

    #. Sign-In with your Postman account and click "Fork Collection". Change the label and workspace names as desired.
    #. Run the requests in the collection
