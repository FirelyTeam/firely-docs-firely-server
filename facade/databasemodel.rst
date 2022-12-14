Mapping the database
====================

In this step you will start mapping the existing database model to FHIR resources.

Reverse engineer the database model
-----------------------------------

To use `EF Core <https://docs.microsoft.com/en-us/ef/core/>`_, install the package for the database provider(s) you want to target. This walkthrough uses SQL Server. For a list of available providers see Database Providers.

* Tools ➡️ NuGet Package Manager ➡️ Package Manager Console
* Run ``Install-Package Microsoft.EntityFrameworkCore.SqlServer``

We will be using some Entity Framework Tools to create a model from the database. So we will install the tools package as well:

* Run ``Install-Package Microsoft.EntityFrameworkCore.Tools``

.. note::
  Please check the version of ``Microsoft.EntityFrameworkCore.SqlServer.dll`` in your Firely Server distribution folder. Add ``-Version <version>`` to the commands above to use
  the same version in your Facade implementation.

Now it's time to create the EF model based on your existing database.

* Tools ➡️ NuGet Package Manager ➡️ Package Manager Console
* Run the following command to create a model from the existing database. Adjust the Data Source to your instance of SQL Server. If you receive an error stating The term 'Scaffold-DbContext' is not recognized as the name of a cmdlet, then close and reopen Visual Studio.::

    Scaffold-DbContext "MultipleActiveResultSets=true;Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=ViSi;Data Source=localhost" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models
    //For localdb: Scaffold-DbContext "Server=(localdb)\mssqllocaldb;Database=ViSi;Trusted_Connection=True;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models
    //For SQLEXPRESS: Scaffold-DBContext "Data Source=(local)\SQLEXPRESS;Initial Catalog=ViSi;Integrated Security=True" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models

You can also generate the scaffolding using the `EF CLI tools <https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/dotnet>`_ which are crossplatform: ::

    dotnet ef dbcontext scaffold "User ID=SA;Password=<enter your password here>;MultipleActiveResultSets=true;Server=tcp:.;Connect Timeout=5;Integrated Security=false;Persist Security Info=False;Initial Catalog=ViSi;Data Source=localhost" Microsoft.EntityFrameworkCore.SqlServer --output-dir Models

The reverse engineer process creates entity classes (Patient.cs & BloodPressure.cs) and a derived context (ViSiContext.cs) based on the schema of the existing database.

The entity classes are simple C# objects that represent the data you will be querying and saving. Later on you will use these classes to define your queries on and to map the resources from.

Clean up generated code
-----------------------

* To avoid naming confusion with the FHIR Resourcetype Patient, rename both files and classes:

  * Patient ➡️ ViSiPatient
  * BloodPressure ➡️ ViSiBloodPressure

  In ``ViSiContext.cs``, ensure that the EF objects mapping our class to the database table are correct and without prefixes (since it's just our local classes that have them): ::

        public virtual DbSet<ViSiBloodPressure> BloodPressure { get; set; }
        public virtual DbSet<ViSiPatient> Patient { get; set; }

* The Scaffold command puts your connectionstring in the ViSiContext class. That is not very configurable.
  Later in the exercise, we will add it as 'DbOptions' to the appsettings.instance.json file in :ref:`configure_facade`.

  * Rename the default Class1 class to DbOptions, and add this to interpret the setting::

        public class DbOptions
        {
            public string ConnectionString { get; set; }
        }

  * Remove the empty constructors from the ViSiContext class

  * Use the options in your ViSiContext class, by adding::

        private readonly IOptions<DbOptions> _dbOptionsAccessor;

        public ViSiContext(IOptions<DbOptions> dbOptionsAccessor)
        {
            _dbOptionsAccessor = dbOptionsAccessor;
        }

  * Change the existing ``OnConfiguring`` method that contains the connectionstring to::

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_dbOptionsAccessor.Value.ConnectionString);
            }
        }


Create your first mapping
-------------------------

#. Add a new ``public`` class ``ResourceMapper`` to the project
#. Add usings for ``Vonk.Core.Common``, for ``Hl7.Fhir.Model``, for ``Hl7.Fhir.Support`` and for ``<your project>.Models``
#. Add a method to the class ``public IResource MapPatient(ViSiPatient source)``
#. In this method, put code to create a FHIR Patient object, and fill its elements with data from the ViSiPatient:

   .. code-block:: c#

     var patient = new Patient
     {
         Id = source.Id.ToString(),
         BirthDate = source.DateOfBirth.ToFhirDate()
     };
     patient.Identifier.Add(new Identifier("http://mycompany.org/patientnumber",
                                           source.PatientNumber));
     // etc.

  For more examples of filling the elements, see the FHIR API documentation: `FHIR-model <https://docs.fire.ly/projects/Firely-NET-SDK/model.html>`_.

5. Then return the created Patient object as an IResource with ``patient.ToIResource()``.

   ``IResource`` is an abstraction from actual Resource objects as they are known to specific versions of the Hl7.Fhir.Net API.
   See :ref:`vonk_reference_api_iresource`.
