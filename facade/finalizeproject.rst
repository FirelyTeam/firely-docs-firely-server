Finalizing your project
=======================

Add support for the ViSiBloodPressure Observations
--------------------------------------------------

First, follow similar steps as above to support ViSiBloodPressure:

#. Add a mapping method in the ResourceMapper class to map from a ViSiBloodPressure to a FHIR Observation resource,
   and return that as an IResource.
#. Create a BloodPressureQuery query class.
#. Add a BPQueryFactory extending ``RelationalQueryFactory<ViSiBloodPressure, BloodPressureQuery>``.
#. Implement support for the ``_id`` parameter by overriding ``public virtual BloodPressureQuery AddValueFilter(string parameterName, TokenValue value)``.
#. Add the Observation type to the ``SupportedModel`` section in Firely Server's appsettings.instance.json: ``"RestrictToResources": [ "Patient", "Observation" ]``

When you have completed these steps, build your project again and copy the dll to your Firely Server plugins folder.
After you (re)start Firely Server, you will be able to request an Observation through your Facade:
``GET http://localhost:4080/Observation?_id=1`` or ``GET http://localhost:4080/Observation/1``.

Since you do not always want to request Observations by their technical id, but more often might want to request Observations from
a specific patient, the next part will describe implementing support that. The Patient resource is referenced by the Observation in
its subject field. The corresponding search parameter is either subject or patient.

Add support for chaining
^^^^^^^^^^^^^^^^^^^^^^^^
To add support for searching on ``Observation?subject:Patient._id`` we need to override the ``AddValueFilter``
overload receiving a ``ReferenceToValue`` parameter in the query factory for BloodPressure (BPQueryFactory).

The ``ReferenceToValue`` type contains the possible ``Targets`` for the chain search parameter as parsed from the query string.
We are currently interested only on the Patient type so we can restrict the implementation to that target.
The ``ReferenceToValue`` type also has an extension method ``CreateQuery`` that expects an implementation of the ``RelationalQueryFactory``
of the referenced target. This will generate the query to obtain the resources referenced by it.

Searching on chained parameters involves the following steps:

    #. Retrieve all patient ids based on the chained parameter.
       You can use the ``ReferenceToValue.CreateQuery`` extension method
       to get the query and run the query with its ``Execute`` method.
    #. Create a  ``PredicateQuery`` with the condition that ``ViSiBloodPressure.PatientId`` is included in the ids retrieved at the previous step.

        The final code should look similar to this:

        ::

            public override BloodPressureQuery AddValueFilter(string parameterName, ReferenceToValue value)
            {
                if (parameterName == "subject" && value.Targets.Contains("Patient"))
                {
                    var patientQuery = value.CreateQuery(new PatientQueryFactory(OnContext));
                    var patIds = patientQuery.Execute(OnContext).Select(p => p.Id);

                    return PredicateQuery(bp => patIds.Contains(bp.PatientId));
                }
                return base.AddValueFilter(parameterName, value);
            }

        .. note::
          patIds is of type IQueryable, so the resulting BloodPressureQuery will still be executed as
          a single command to the database.

    #. Add support for the ``Observation.subject`` search parameter in the Firely Server appsettings similar to how we did it for ``_id``.

At this point you should be able to search for ``GET http://localhost:4080/Observation?subject:Patient._id=1``

Add support for reverse chaining
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
Adding support for ``Patient?_has:Observation:subject:_id=1`` is similar. You just need to use the ``AddValueFilter``
overload receiving a ``ReferenceFromValue``.

The ``ReferenceFromValue`` type has a ``Source`` property filled in with the source of the search parameter. It also has an extension method ``CreateQuery`` that given the corresponding ``RelationalQueryFactory`` implementation can generate
the query to obtain resources referenced by the reverse chaining.

So you can add reverse chaining with the following code::

    public class PatientQueryFactory
    {

        public override PatientQuery AddValueFilter(string parameterName, ReferenceFromValue value)
        {
            if (parameterName == "subject" && value.Source == "Observation")
            {
                var obsQuery = value.CreateQuery(new BPQueryFactory(OnContext));
                var obsIds = obsQuery.Execute(OnContext).Select(bp => bp.PatientId);

                return PredicateQuery(p => obsIds.Contains(p.Id));
            }
            return base.AddValueFilter(parameterName, value);
        }
    }

.. note::
  The reverse chaining example above uses the Patient resource type as its base, so you will need to implement this
  in your PatientQueryFactory.

Now you can test if reverse chaining works: ``http://localhost:4080/Patient?_has:Observation:subject:_id=1``

Get the goodies
---------------
At this point you get out of the box support for ``_include`` and  ``_revinclude`` (``:iterate`` as well), and combinations of search parameters.
You can test the following scenarios:

#. ``_include``: ``http://localhost:4080/Observation?_include=Observation:subject``
#. ``_revinclude``: ``http://localhost:4080/Patient?_revinclude=Observation:subject``
#. combinations of the above

.. _addSearchParameters:

Adding a custom SearchParameter
-------------------------------

Your Firely Server will load the standard parameters from the
specification on first startup, so the ``_id`` SearchParameter from the exercise is already known to Firely Server, as well as any of
the other standard search parameters for the resource types.

If you want to implement support for a custom search parameter, you will need to have the definition of that in the form of
a SearchParameter resource, and add it to your Firely Server. The :ref:`feature_customsp_configure` section describes how to
do that.

Of course you will also need to implement the correct AddValueFilter method in your ``<resourcetype>QueryFactory`` to handle
the parameter correctly, as is done for the _id parameter in the exercise.

The end?
--------

This concludes the exercise. An example `Github repository <https://github.com/FirelyTeam/Vonk.Facade.Starter>`_ contains
the completed exercise.

Please feel free to try out more options, and :ref:`ask for help <vonk-contact>` if you get stuck!

The next topic will show you how to enable :ref:`Create, Update and Delete<enablechange>` interactions.

Postscript
----------
If your resource is split across multiple tables in the database, you'll need to make use of ``.Include()`` to have EF `load the dependent table <https://docs.microsoft.com/en-us/ef/core/querying/related-data#eager-loading>`_. To do so in Firely Server, override the `GetEntitySet()` method in your `RelationalQuery` class, for example ::

        protected override IQueryable<ViSiPatient> GetEntitySet(DbContext dbContext)
        {
            // load the dependent Address table
            return dbContext.Set<ViSiPatient>().Include(p => p.Address).AsNoTracking();
        }
