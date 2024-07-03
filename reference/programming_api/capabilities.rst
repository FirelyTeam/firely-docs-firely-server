.. _vonk_reference_api_capabilities:

Capability Statement Management
===============================

A FHIR server has to express its capabilities in a CapabilityStatement, available on the ``/metadata`` endpoint. Firely Server's capabilities are defined by the middleware components that make up its pipeline. Every component knows what interaction it adds to the capabilities. Therefore, we keep that information close to the component itself. Typically, every component has an implementation of :code:`ICapabilityStatementContributor`, in which it gets access to the :code:`ICapabilityStatementBuilder`. The latter provides methods to add information to the CapabilityStatement without having to worry about what is already added by other components or the order of execution.

These methods are especially handy for implementers of :ref:`custom plugins <vonk_plugins>` or a :ref:`facade <vonk_facade>`. Custom plugins or plugins implemented in the facade do not automatically end up in the ``/metadata`` endpoint of Firely Server, but :code:`ICapabilityStatementContributor` and :code:`ICapabilityStatementBuilder` can be used to make sure the plugins are visible in the CapabilityStatement. For example, you have implemented a Bulk Data Export plugin in your facade and you would like to make sure this is visible in the CapabilityStatement.instantiates of Firely Server. You can add a CapabilityStatementContributor class to your plugin code that implements the :code:`ICapabilityStatementContributor`. Within this class you can implement the :code:`ICapabilityStatementBuilder` to add your plugin to the CapabilityStatement.instantiates. See the following code snippet::

    internal class CapabilityStatementContributor: ICapabilityStatementContributor
    {
        public void ContributeToCapabilityStatement(ICapabilityStatementBuilder builder)
        {
            builder.UseCapabilityStatementEditor(cse =>
            {
                cse.AddInstantiates("http://hl7.org/fhir/uv/bulkdata/CapabilityStatement/bulk-data");
            });
        }
    }

Make sure to register this class in your PluginConfiguration.cs::

    public static IServiceCollection ConfigureServices(IServiceCollection services)
    {
        services.TryAddContextAware<ICapabilityStatementContributor, CapabilityStatementContributor>(ServiceLifetime.Transient);
        return services;
    }

ICapabilityStatementBuilder
---------------------------

The :code:`ICapabilityStatementBuilder` interface is used to construct and manipulate FHIR :code:`CapabilityStatement`. This interface facilitates the customization of capability statements by allowing developers to define and configure the server's capabilities.

Key Methods
^^^^^^^^^^^

**UseCapabilityStatementEditor**

    Adds an action to modify the entire :code:`CapabilityStatement`.

    .. code-block:: csharp

        ICapabilityStatementBuilder UseCapabilityStatementEditor(Action<ICapabilityStatement> statementEditor);

    **Usage:** Use this method when you need to set or modify properties at the top level of the :code:`CapabilityStatement`.

    **Example:**

    .. code-block:: csharp

        builder.UseCapabilityStatementEditor(cs => {
            cs.Name = "MyCapabilityStatement";
            cs.Version = "1.0.0";
        });

**UseRestComponentEditor**

    Adds an action to modify the :code:`RestComponent` of the :code:`CapabilityStatement`.

    .. code-block:: csharp

        ICapabilityStatementBuilder UseRestComponentEditor(Action<IRestComponent> restComponentEditor);

    **Usage:** Use this method to define RESTful interactions and resource configurations.

    **Example:**

    .. code-block:: csharp

        builder.UseRestComponentEditor(rc => {
            rc.AddInteraction(CapabilityStatementSystemRestfulInteraction.Transaction);
        });

**UseResourceComponentEditor**

    Adds an action to modify the :code:`ResourceComponent` of the :code:`CapabilityStatement`.

    .. code-block:: csharp

        ICapabilityStatementBuilder UseResourceComponentEditor(Action<IResourceComponent> resourceComponentEditor);

    **Usage:** Use this method to add or configure interactions, operations, and search parameters at the resource level.

    **Example:**

    .. code-block:: csharp

        builder.UseResourceComponentEditor(rc => {
            if (rc.TypeLiteral == "Patient") {
                rc.AddReadInteraction();
                rc.AddSearchParameter("family", SearchParamType.String, "http://hl7.org/fhir/SearchParameter/Patient-family", "Search by family name");
            }
        });

**Build**

    Finalizes and constructs the :code:`CapabilityStatement` using the provided editors.

    .. code-block:: csharp

        ICapabilityStatement Build();

    **Usage:** Call this method to obtain the fully constructed :code:`CapabilityStatement`.

    **Example:**

    .. code-block:: csharp

        ICapabilityStatement capabilityStatement = builder.Build();

Extension Methods for IResourceComponent
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Most of the extension methods for IResourceComponent have a similar implementation for :code:`IRestComponent`. Here are some of those:

**AddOperation**

    Adds an operation to the :code:`ResourceComponent`.

    .. code-block:: csharp

        public static IOperationComponent AddOperation(this IResourceComponent resourceComponent, string name, string definitionUri);

    **Example:**

    .. code-block:: csharp

        resourceComponent.AddOperation("validate", "http://hl7.org/fhir/OperationDefinition/Resource-validate");

**AddSearchParameter**

    Adds a search parameter to the :code:`ResourceComponent`, ensuring no duplicates by name.

    .. code-block:: csharp

        public static ISearchParamComponent AddSearchParameter(this IResourceComponent resourceComponent, string parameterName, SearchParamType parameterType, string definition, string documentation);

    **Example:**

    .. code-block:: csharp

        resourceComponent.AddSearchParameter("name", SearchParamType.String, "http://hl7.org/fhir/SearchParameter/Patient-name", "Search by patient name");

**AddInteraction**

    Adds an interaction to the :code:`ResourceComponent`.

    .. code-block:: csharp

        public static IInteractionComponent<CapabilityStatementTypeRestfulInteraction> AddInteraction(this IResourceComponent resourceComponent, CapabilityStatementTypeRestfulInteraction interaction);

    **Example:**

    .. code-block:: csharp

        resourceComponent.AddInteraction(CapabilityStatementTypeRestfulInteraction.Read);

Example Usage of ICapabilityStatementContributor
------------------------------------------------

To showcase the usage of :code:`ICapabilityStatementBuilder` within a contributor, here is an example:

**CapabilityStatementContributor Example**

.. code-block:: csharp

    public class ExampleCapabilityStatementContributor : ICapabilityStatementContributor
    {
        public void ContributeToCapabilityStatement(ICapabilityStatementBuilder builder)
        {
            builder.UseCapabilityStatementEditor(cs => {
                cs.Name = "ComprehensiveCapabilityStatement";
                cs.Version = "2.0.0";
                cs.AddFormats("xml", "json");
            });

            builder.UseRestComponentEditor(rc => {
                rc.AddInteraction(CapabilityStatementSystemRestfulInteraction.Transaction);
                rc.AddOperation("batch", "http://hl7.org/fhir/OperationDefinition/Resource-batch");
            });

            builder.UseResourceComponentEditor(rc => {
                if (rc.TypeLiteral == "Observation") {
                    rc.AddReadInteraction();
                    rc.AddSearchParameter("code", SearchParamType.Token, "http://hl7.org/fhir/SearchParameter/Observation-code", "Search by observation code");
                }
                if (rc.TypeLiteral == "Patient") {
                    rce.AddOperation("member-match", "http://hl7.org/fhir/us/davinci-hrex/OperationDefinition/member-match");
                }
            });
        }
    }
