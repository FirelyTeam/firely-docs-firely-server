using Microsoft.Extensions.Options;
using Vonk.Core.Context;
using Vonk.Core.Context.Guards;
using Vonk.Core.Metadata;
using Vonk.Core.Model.Capability;
using Vonk.Core.Support;

namespace Vonk.Plugin.AuthorizedRepresentativeExample;

/// <summary>
/// Lists the two operations in the CapabilityStatement, each when it is enabled in <c>Operations</c>.
/// </summary>
/// <remarks>
/// Firely Auth reads the CapabilityStatement at startup. It looks for both operations by name, or by a definition that equals the scope it requests.
/// Without them it logs an error, and a Representative cannot sign in to apps.
/// </remarks>
internal sealed class RepresentativeOperationsCapabilityStatementContributor(IOptions<SupportedInteractionOptions> supportedInteractionOptions) : ICapabilityStatementContributor
{
    /// <summary>
    /// The definition of <c>$representative-relationships</c>. It is the scope that Firely Auth requests for the operation by default.
    /// </summary>
    public const string RepresentativeRelationshipsDefinition = "http://server.fire.ly/auth/scope/representative-relationships";

    /// <summary>
    /// The definition of <c>$check-authorized-representative</c>. It is the scope that Firely Auth requests for the operation by default.
    /// </summary>
    public const string CheckAuthorizedRepresentativeDefinition = "http://server.fire.ly/auth/scope/check-authorized-representative";

    /// <inheritdoc/>
    public void ContributeToCapabilityStatement(ICapabilityStatementBuilder builder)
    {
        Check.NotNull(builder);
        var options = supportedInteractionOptions.Value;

        if (options.SupportsOperation(VonkInteraction.system_custom, RepresentativeRelationshipsOperation.Name))
        {
            builder.UseRestComponentEditor(rest => rest.AddOperation(RepresentativeRelationshipsOperation.Name, RepresentativeRelationshipsDefinition));
        }

        if (options.SupportsOperation(VonkInteraction.type_custom, CheckAuthorizedRepresentativeOperation.Name))
        {
            builder.UseRestComponentEditor(rest => rest.AddOperation(CheckAuthorizedRepresentativeOperation.Name, CheckAuthorizedRepresentativeDefinition));
            builder.UseResourceComponentEditor(resource =>
            {
                if (resource.TypeLiteral == "Patient")
                    resource.AddOperation(CheckAuthorizedRepresentativeOperation.Name, CheckAuthorizedRepresentativeDefinition);
            });
        }
    }
}
