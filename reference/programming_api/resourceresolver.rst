.. _vonk_reference_api_resourceresolver:

Resource resolver for conformance resources
===========================================

The HL7 .NET SDK defines the ``IAsyncResourceResolver``. In Firely Server an implementation of this class is available to access conformance resources.
Because ``IAsyncResourceResolver`` is bound to a single FHIR Version, Firely Server actually has as many instances as there are FHIR versions loaded (see :ref:`feature_multiversion`).
You can simply have it injected, and Firely Server will provide the implementation that fits the currently applicable information model (a.k.a. FHIR version).
Using a primary constructor this could look like:

.. code-block:: csharp

    internal class MyCustomPluginService(IAsyncResourceResolver conformanceResolver)
    {}
    
You can also get access to conformance resources directly through the ``IAdministrationSearchRepository``. However, using the ``IAsyncResourceResolver`` implementation provides extras:

- StructureDefinition resources have a freshly calculated snapshot. This is important for correct validation.
- ValueSets have a fresh expansion.
- Conformance resources are cached in memory.    