.. _vonk_reference_api_iresource:

IResource
=========

:namespace: Vonk.Core.Common

:purpose: IResource is the abstraction for all FHIR resources in Firely Server. It is used in the request and the response, and thereby all through the pipeline.
          It allows you to program against resources in different Fhir.NET API (the Resource class is defined in each version separately), as well as against resources that do not even have a POCO implementation.

.. code-block:: csharp

    /// <summary>
    /// Abstraction of a resource in some format. Specifies the properties of a Resource that Firely Server needs to read and maintain.
    /// <para>Future: may be extended with Tags and Labels.</para>
    /// </summary>
    public interface IResource : ISourceNode
    {
        /// <summary>
        /// Type of resource, e.g. Patient or AllergyIntolerance.
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Logical identity of resource, e.g. 'example' or 'e3f5b0b8-4570-4e4c-b597-e6523aff3a19'. Does not contain the resourcetype.
        /// Refers to Resource.id
        /// IResource is immutable, so to update this, use resourceWithNewId = this.SetId(), from IResourceExtensions.
        /// In the context of a repository, consider IResourceChangeRepository.EnsureMeta().
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Version of resource. Refers to Resource.meta.versionId.
        /// IResource is immutable, so to update this, use resourceWithNewVersion = this.SetVersion(), from IResourceExtensions.
        /// In the context of a repository, consider IResourceChangeRepository.EnsureMeta().
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Model that the resource was defined in. 
        /// Common models are the different versions of FHIR, defined in <see cref="VonkConstants.Model"/>
        /// </summary>
        string InformationModel { get; }

        /// <summary>
        /// When was the resource last updated?
        /// Refers to Resource.meta.lastUpdated.
        /// IResource is immutable, so to update this, use resourceWithNewLastUpdated = this.SetLastUpdated(DateTimeOffset) from IResourceExtensions.
        /// In the context of a repository, consider IResourceChangeRepository.EnsureMeta().
        /// </summary>
        DateTimeOffset? LastUpdated { get; }

        /// <summary>
        /// Is this a contained resource, or a container resource?
        /// A resource is a container resource if it is not contained. Even if it has no contained resources embedded.
        /// </summary>
        ResourceContained Contained { get; }

        /// <summary>
        /// Direct access to contained resources, if any. Prefer to return an empty list otherwise.
        /// Refers to DomainResource.contained.
        /// </summary>
        IEnumerable<IResource> ContainedResources { get; }
   }

If you work with a POCO, you can use an extension method ToIResource() from a FHIR version namespace, such as Vonk.Fhir.R4, to adapt it to an IResource:

.. code-block:: csharp

   var patientPoco = new Patient(); //Requires Hl7.Fhir.Model
   var resource = patientPoco.ToIResource(); //Requires a version namespace like Vonk.Fhir.R4

IResource is immutable, so changes will always result in a new instance. Changes can usually be applied with extension methods on ISourceNode, found in :ref:`Vonk.Core.ElementModel.ISourceNodeExtensions <vonk_reference_api_elementmodel>`. There are also several extension methods specifically for IResource in Vonk.Core.Common.IResourceExtensions:

.. code-block:: csharp

   var updatedResource = oldResource.Add(SourceNode.Valued("someElement", "someValue");
   //Continue with updatedResource, since oldResource will not have the change.

.. _vonk_reference_api_iresource_extensions:

IResource extension methods
---------------------------

IResource has a whole list of extension methods for manipulating them and conversion between ISourceNode and IResource. All these methods are in the namespace ``Vonk.Core.Common.IResourceExtensions``. Please check the ///-comments on the methods for more information.