.. _accesscontrol_api:

=====================================
Access Control in Plugins and Facades
=====================================

The :ref:`Access Control feature <feature_accesscontrol>` is also available to users of a Firely Server Facade or Firely Server Plugins. You can use the default implementation based on SMART on FHIR, or provide an implementation of your own.

Access control implementation
=============================

The access control engine is programmed using interfaces for which you can provide your own implementation. Because we think the model behind SMART on FHIR covers many cases, these interfaces are loosely modelled after it.
The important interfaces and class are:

.. csv-table:: Access Control interfaces
   :header: "Interface / Class", "Description"
   :widths: 20, 80

   "IAuthorization", "Defines whether your are allowed to read or write a type of resource. |br| Abstraction of the concept of a scope (like user/Observation.read) in SMART"
   "ICompartment", "Confines the user to a compartment, expressed as a combination of a |br| CompartmentDefinition and a search argument. |br| Abstraction of the concept of a launch context (like patient=123) in SMART"
   "IReadAuthorizer", "Calculates access control for a type of resource given an instance of IAuthorization |br| and/or ICompartment"
   "IWriteAuthorizer", "Calculates access control for writing a new (version of a) resource given an instance |br| of IAuthorization and/or ICompartment"
   "AuthorizationResult", "Return value of IReadAuthorizer and IWriteAuthorizer methods. |br| It expresses whether you are authorized at all, and if so - under which conditions. |br| These conditions are expressed as search arguments."

IReadAuthorizer
---------------

Provides two methods to check authorization for reading types of resources.

* AuthorizeRead
* AuthorizeReadAnyType

The latter is only used if a system wide search is performed, without a _type parameter. In that case it is not efficient to call the first method for every supported resourcetype.

The input of these operations is an IAuthorization and an ICompartment. The result is an AuthorizationResult. With this class you can return:

* simply true or false
* extra search arguments to add to the search query in order to confine the search to those resources the user is allowed to read.

The AuthorizationResult Filters member is a collection of IArgumentCollections. Arguments within a collection will be AND'ed together. Multiple collections will be OR'ed together.

IWriteAuthorizer
----------------

Provides one method to assess whether the user is allowed to write a resource. Input is again IAuthorization and ICompartment, but also IResource - the resource that is to be written - and an Uri called 'serverBase'.
The 'serverBase' parameter is primarily provided because it is required to perform a search on the ISearchRepository interface.
The IAuthorization instance can be used to decide whether the user is allowed to write resources of the given resourcetype at all.
The ICompartment can be used to search in the database whether the to-be-written resource is linked to the current compartment.

.. |br| raw:: html

   <br />
