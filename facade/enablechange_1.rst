.. _enablechange:

Enable changes to the repository
================================

In :ref:`facadestart` you have created read and search support for a Firely Server Facade on top of an existing database.
The next part will walk you through enabling create, update and delete support.
This will be done in three steps:

1.	Map the FHIR resource to the database model;
2.	Implement the IResourceChangeRepository;
3.  Indicate support in the appsettings file


1. Map the FHIR data to the model
---------------------------------

For both the Create and Update interactions, we need to map our incoming FHIR data to the ViSi model. To do that, we will
add new mapping methods to our ResourceMapper class.

* Add a method called ``MapVisiPatient`` to the ResourceMapper class, that takes a FHIR ``Patient`` and returns a ``ViSiPatient``.
* Implement the mapping from the FHIR object to the ViSiPatient model::

        public ViSiPatient MapViSiPatient(IResource source)
        {
            var fhirPatient = source.ToPoco<Patient>();
            var visiPatient = new ViSiPatient();

            if (source.Id != null)
            {
                if (!int.TryParse(source.Id, out int id))
                    throw new VonkRepositoryException("Id needs to be integer to map resource");
                visiPatient.Id = id;
            }
            visiPatient.PatientNumber = fhirPatient.Identifier.Find(i =>
                                      (i.System == "http://mycompany.org/patientnumber")).Value;

            // etc.

            return visiPatient;
        }

* Where it says 'etc.', fill in the rest of the code to map the data to required fields of the database, and any other fields you have data for.


2. Implement the IResourceChangeRepository
------------------------------------------

You are going to implement a repository that handles changes to your database. The interface for this is called ``IResourceChangeRepository``, which
can be found in ``Vonk.Core.Repository``.

* Add a new class ``ViSiChangeRepository`` to the project, that implements the IResourceChangeRepository::

    public class ViSiChangeRepository : IResourceChangeRepository

*  Choose to implement the interface, so the required methods are added to the class.
*  Just like with the search repository, you will need your DbContext to query on, and the ResourceMapper to perform the mapping of the incoming
   data to your proprietary model.

   So put all of that in the constructor::

        private readonly ViSiContext _visiContext;
        private readonly ResourceMapper _resourceMapper;

        public ViSiChangeRepository(ViSiContext visiContext, ResourceMapper resourceMapper)
        {
            _visiContext = visiContext;
            _resourceMapper = resourceMapper;
        }

Implementing Create
^^^^^^^^^^^^^^^^^^^

*  Now implement the Create method with a switch on resource type, so you can add other resource types later::

       public async Task<IResource> Create(IResource input)
       {
            switch (input.Type)
            {
                case "Patient":
                    return await CreatePatient(input);
                default:
                    throw new NotImplementedException($"ResourceType {input.Type} is not supported.");
            }
        }

*  As you can see, we have deferred the work to a CreatePatient method, which we also need to implement. This method
   will add the new resource to the collection, and save the changes to the database::

        private async Task<IResource> CreatePatient(IResource input)
        {
            var visiPatient = _resourceMapper.MapViSiPatient(input);

            await _visiContext.Patient.AddAsync(visiPatient);
            await _visiContext.SaveChangesAsync();

            // return the new resource as it was stored by this server
            return _resourceMapper.MapPatient(_visiContext.Patient.Last());
        }

*  For the ``Create`` and ``Update`` methods, you will also need to implement the ``NewId`` and ``NewVersion`` methods,
   because Firely Server will call them. For the ``NewId`` method, we will return null, since our ViSi database does not allow us
   to create our own index value. Since our ViSi repository does not handle versions, we will let the ``NewVersion`` method
   return null as well::

        public string NewId(string resourceType)
        {
            return null;
        }

        public string NewVersion(string resourceType, string resourceId)
        {
            return null;
        }


.. note::

  For the ViSi repository we're using a null value, but you can implement this method any way that's
  useful for your own repository. The public Firely Server for example generates a GUID in these methods.

At this point you can skip ahead to :ref:`config_change_repo`, if you want to try and create a new patient in the ViSi database.

.. tip::
  This is easiest to test if you retrieve an existing resource from the database first with your HTTP tool.
  Then change some of the data in the resulting JSON or XML, and send that back to your Facade.

Implementing Update
^^^^^^^^^^^^^^^^^^^
Implementing the ``Update`` method can be done like the ``Create``, with a switch on resource type, and instead of adding
a resource to the collection, you will update the collection::

        private async Task<IResource> UpdatePatient(ResourceKey original, IResource update)
        {
            try
            {
                var visiPatient = _resourceMapper.MapViSiPatient(update);

                var result = _visiContext.Patient.Update(visiPatient);
                await _visiContext.SaveChangesAsync();

                return _resourceMapper.MapPatient(result.Entity);
            }
            catch (Exception ex)
            {
                throw new VonkRepositoryException($"Error on update of {original} to {update.Key()}", ex);
            }
        }

Implementing Delete
^^^^^^^^^^^^^^^^^^^
Deleting a resource from the collection is done by first looking up the corresponding resource, and then removing
it from the collection. Note that the database used for this exercise cannot process the deletion of the Patient
when there are still related Observations in the BloodPressure table, so we need to remove them as well or choose
to throw an error.

* First, create a switch on resource type in the main ``Delete`` method again.
* Implement the ``DeletePatient``::

        private async Task<IResource> DeletePatient(ResourceKey toDelete)
        {
            int toDelete_id = int.Parse(toDelete.ResourceId);
            var visiPatient = _visiContext.Patient.Find(toDelete_id);

            var bpEntries = _visiContext.BloodPressure.Where(bp => bp.PatientId == toDelete_id);

            var result = _resourceMapper.MapPatient(visiPatient);

            try
            {
                _visiContext.BloodPressure.RemoveRange(bpEntries);
                _visiContext.Patient.Remove(visiPatient);
                await _visiContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new VonkRepositoryException($"Error on deleting Patient with Id {toDelete_id}", ex);
            }

            return result;
        }

.. _config_change_repo:

3. Configure the service and Firely Server
------------------------------------------

Just like with the search repository, you will need to add your change repository as service to the pipeline.
Also, you will need to indicate support for the CRUD interactions in your Firely Server appsettings.

* In your project, go to the ViSiConfiguration class, and add this line to add an IResourceChangeRepository to
  the pipeline::

    services.TryAddScoped<IResourceChangeRepository, ViSiChangeRepository>();

* Add support for the interactions to the SupportedModel section of the Firely Server appsettings::

    "SupportedInteractions": {
      "InstanceLevelInteractions": "read, update, delete",
      "TypeLevelInteractions": "search, create",
      "WholeSystemInteractions": "capabilities, search"
    },
    
* Adjust ``PipelineOptions.Branches.Include`` from ``Vonk.Core.Operations.Crud.ReadConfiguration`` to ````Vonk.Core.Operations`` include all operations, including ``Create``.

You can now build your project, copy the dll to the Firely Server plugins folder and run Firely Server to test the new interactions
on your Facade.

The end?
--------

This concludes the second exercise. Please feel free to try out more options, and :ref:`ask for help <vonk-contact>` if you get stuck!

The next topic will show you how to integrate :ref:`Access Control<feature_accesscontrol>`.
