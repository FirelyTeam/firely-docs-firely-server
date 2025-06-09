.. _FeatureAvailability:

Feature Availability per Database
=================================

This page provides an overview of the features supported by Firely Server when using different database backends. The compatibility table below lists the features and indicates their support status across SQL Server, SQLite, MongoDB, and the Facade.

Legend:

- **✅** - Full support
- **❌** - Not supported
- **\*** - Supported, but requires additional setup

.. list-table::
   :widths: 28 15 15 15 15
   :header-rows: 1
   :align: center

   * - Feature
     - SQL Server
     - SQLite
     - MongoDB
     - Facade
   * - FHIR Transactions
     - ✅
     - ✅
     - ✅
     - \*
   * - Bulk Data Export
     - ✅
     - ❌
     - ✅
     - \*
   * - $everything
     - ✅
     - ❌
     - ✅
     - ❌
   * - $lastn
     - ✅
     - ❌
     - ✅
     - ❌
   * - $erase
     - ✅
     - ✅
     - ✅
     - ❌
   * - $purge
     - ✅
     - ❌
     - ✅
     - ❌
   * - Reset Database
     - ✅
     - ✅
     - ✅
     - ❌
   * - SMART on FHIR
     - ✅
     - ✅
     - ✅
     - \*
   * - Audit Events
     - ✅
     - ✅
     - ✅
     - \*
   * - Audit Log File
     - ✅
     - ✅
     - ✅
     - ✅
   * - Binary Wrapper
     - ✅
     - ✅
     - ✅
     - \*
   * - Subscriptions
     - ✅
     - ✅
     - ✅
     - ✅
   * - Preload
     - ✅
     - ✅
     - ✅
     - ✅
   * - Multiple versions of FHIR
     - ✅
     - ✅
     - ✅
     - \*
   * - FHIR document bundles
     - ✅
     - ✅
     - ✅
     - ✅
   * - $docref
     - ✅
     - ✅
     - ✅
     - ✅
   * - PubSub
     - ✅
     - ✅
     - ✅
     - \*
   * - PubSub - ResourceChangeNotifications
     - ✅
     - ❌
     - ❌
     - ❌
   * - Digital Quality Measures
     - ✅
     - ❌
     - ✅
     - ✅
