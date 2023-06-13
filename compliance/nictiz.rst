.. _compliance_nictiz:

Nictiz
=======

* Tested Version: Firely Server has been tested against `nictiz.fhir.nl.stu3.zib2017 version 2.1.12 <https://simplifier.net/nictizstu3-zib2017>`_
* The conformance resources are **NOT** loaded by default in the standard SQLite Administration Database of Firely Server. Please see :ref:`conformance_import` on how to load the profiles, extensions and ValueSets into Firely Server.

Known Limitations
^^^^^^^^^^^^^^^^^

The following tickets represent outstanding issues with the above-mentioned specification:

* https://bits.nictiz.nl/browse/MM-3218
* https://bits.nictiz.nl/browse/MM-3219
* https://bits.nictiz.nl/browse/MM-3221
* https://bits.nictiz.nl/browse/MM-3222

These issues do not influence the validation of instances against the erroneous profiles. However, the profiles cannot be loaded via the administration REST API as the validation on that endpoint will reject the StructureDefinitions. 

Test Data
^^^^^^^^^

Offical test data can be found in the Simplifier project for `Nictiz STU3 Zib 2017 <https://simplifier.net/NictizSTU3-Zib2017/~introduction>`_.
