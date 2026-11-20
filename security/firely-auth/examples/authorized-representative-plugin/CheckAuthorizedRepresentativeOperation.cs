using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Vonk.Core.Context;

namespace Vonk.Plugin.AuthorizedRepresentativeExample;

/// <summary>
/// Implements <c>POST [base]/Patient/$check-authorized-representative</c>.
/// </summary>
/// <remarks>
/// Firely Auth calls it when a user chooses whom to act for, and at every refresh of a token for that choice.
/// The input parameters are <c>patient</c> (<c>Patient/&lt;id&gt;</c>) and <c>representative</c> (<c>RelatedPerson/&lt;id&gt;</c>).
/// The user who chose is the <c>sub</c> claim of the token.
/// It answers <c>result</c>, and <c>consent</c> with the Consent that authorizes the pair, if the authorization has one.
/// </remarks>
internal sealed partial class CheckAuthorizedRepresentativeOperation(
    RepresentativeAuthorizations authorizations,
    ILogger<CheckAuthorizedRepresentativeOperation> logger)
{
    /// <summary>
    /// The name of the operation, without <c>$</c>.
    /// </summary>
    public const string Name = "check-authorized-representative";

    private const string PatientParameter = "patient";
    private const string RepresentativeParameter = "representative";

    [GeneratedRegex(@"^Patient/(?<id>[A-Za-z0-9\-\.]{1,64})$")]
    private static partial Regex PatientReference();

    [GeneratedRegex(@"^RelatedPerson/(?<id>[A-Za-z0-9\-\.]{1,64})$")]
    private static partial Regex RelatedPersonReference();

    /// <summary>
    /// Handles the operation.
    /// </summary>
    public async Task HandleAsync(IVonkContext vonkContext)
    {
        var (request, args, response) = vonkContext.Parts();
        args.ResourceTypeArguments().Handled();

        if (!FirelyAuthCaller.TryGetUserId(vonkContext.HttpContext().User, out var userId))
        {
            logger.LogWarning("${Operation} was refused: the token is not one of Firely Auth itself, or has no single sub claim", Name);
            response.HttpResult = StatusCodes.Status403Forbidden;
            response.Outcome.AddIssue(VonkIssue.FORBIDDEN, "Only Firely Auth may call this operation.");
            return;
        }

        if (!request.GetRequiredPayload(response, out var parameters))
            return;

        var patientMatch = PatientReference().Match(OperationParameters.GetReference(parameters, PatientParameter) ?? string.Empty);
        var representativeMatch = RelatedPersonReference().Match(OperationParameters.GetReference(parameters, RepresentativeParameter) ?? string.Empty);
        if (!patientMatch.Success || !representativeMatch.Success)
        {
            response.HttpResult = StatusCodes.Status400BadRequest;
            response.Outcome.AddIssue(VonkIssue.PARAMETER_MISSING,
                $"Provide '{PatientParameter}' as a reference Patient/<id> and '{RepresentativeParameter}' as a reference RelatedPerson/<id>, each once.");
            return;
        }

        var patientId = patientMatch.Groups["id"].Value;
        var relatedPersonId = representativeMatch.Groups["id"].Value;
        RepresentativeAuthorization? authorization;
        try
        {
            authorization = await authorizations.CheckAsync(userId, patientId, relatedPersonId, vonkContext);
        }
        catch (RepresentativeLookupException)
        {
            response.HttpResult = StatusCodes.Status500InternalServerError;
            response.Outcome.AddIssue(VonkIssue.INTERNAL_ERROR, "The request could not be processed.");
            return;
        }

        logger.LogInformation("RelatedPerson/{RelatedPersonId} {Verdict} for Patient/{PatientId} ({Consent})",
            relatedPersonId, authorization is null ? "may not act" : "may act", patientId, authorization?.Consent ?? "no Consent");
        response.HttpResult = StatusCodes.Status200OK;
        response.Payload = OperationParameters.Create(authorization is not null, authorization?.Consent, vonkContext.InformationModel);
    }
}
