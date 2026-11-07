using Hl7.Fhir.ElementModel;
using Vonk.Core.Common;
using Vonk.Core.ElementModel;

namespace Vonk.Plugin.AuthorizedRepresentativeExample;

/// <summary>
/// Reads and writes the <c>Parameters</c> of the two operations.
/// </summary>
internal static class OperationParameters
{
    /// <summary>
    /// The output parameter that says whether the user may act for someone.
    /// </summary>
    public const string Result = "result";

    /// <summary>
    /// The output parameter with the Consent that authorizes the pair.
    /// </summary>
    public const string Consent = "consent";

    /// <summary>
    /// Returns the <c>reference</c> of the <c>valueReference</c> of the parameter <paramref name="name"/>, if it is there once.
    /// </summary>
    public static string? GetReference(ISourceNode parameters, string name) =>
        parameters.Children("parameter").Where(p => p.ChildString("name") == name).ToList() is [var parameter]
            ? parameter.Child("valueReference")?.ChildString("reference")
            : null;

    /// <summary>
    /// Creates the output <c>Parameters</c> with <see cref="Result"/>, and <see cref="Consent"/> when there is one.
    /// </summary>
    public static IResource Create(bool result, string? consent, string informationModel)
    {
        var parameters = SourceNode.Resource("Parameters", "Parameters",
            SourceNode.Node("parameter",
                SourceNode.Valued("name", Result),
                SourceNode.Valued("valueBoolean", result ? "true" : "false")));

        if (consent is not null)
        {
            parameters.Add(SourceNode.Node("parameter",
                SourceNode.Valued("name", Consent),
                SourceNode.Node("valueReference", SourceNode.Valued("reference", consent))));
        }

        return parameters.ToIResource(informationModel);
    }
}
