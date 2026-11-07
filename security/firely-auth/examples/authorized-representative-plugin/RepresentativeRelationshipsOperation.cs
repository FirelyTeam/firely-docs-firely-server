using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Vonk.Core.Context;

namespace Vonk.Plugin.AuthorizedRepresentativeExample;

/// <summary>
/// Implements <c>POST [base]/$representative-relationships</c>.
/// </summary>
/// <remarks>
/// Firely Auth calls it once per sign-in of a user who may act for others. The operation has no input parameters: the user is the <c>sub</c> of the token.
/// It pushes the patients the user may act for to Firely Auth, and then answers <c>result</c>: <see langword="true"/> when there is at least one.
/// This example pushes before it answers. A push may also come after the answer, or at any other time.
/// </remarks>
internal sealed class RepresentativeRelationshipsOperation(
    RepresentativeAuthorizations authorizations,
    FirelyAuthRelationshipsClient firelyAuth,
    ILogger<RepresentativeRelationshipsOperation> logger)
{
    /// <summary>
    /// The name of the operation, without <c>$</c>.
    /// </summary>
    public const string Name = "representative-relationships";

    /// <summary>
    /// Handles the operation.
    /// </summary>
    public async Task HandleAsync(IVonkContext vonkContext)
    {
        var (_, _, response) = vonkContext.Parts();
        var httpContext = vonkContext.HttpContext();

        if (!FirelyAuthCaller.TryGetUserId(httpContext.User, out var userId))
        {
            logger.LogWarning("${Operation} was refused: the token is not one of Firely Auth itself, or has no sub claim", Name);
            response.HttpResult = StatusCodes.Status403Forbidden;
            response.Outcome.AddIssue(VonkIssue.FORBIDDEN, "Only Firely Auth may call this operation, with a token for a user.");
            return;
        }

        IReadOnlyList<Representee> representees;
        try
        {
            representees = await authorizations.FindRepresenteesAsync(userId, vonkContext);
        }
        catch (RepresentativeLookupException)
        {
            response.HttpResult = StatusCodes.Status500InternalServerError;
            response.Outcome.AddIssue(VonkIssue.INTERNAL_ERROR, "The request could not be processed.");
            return;
        }

        try
        {
            await firelyAuth.PushAsync(userId, representees, httpContext.RequestAborted);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Pushing {Count} representative relationships of user {UserId} to Firely Auth failed", representees.Count, userId);
            response.HttpResult = StatusCodes.Status500InternalServerError;
            response.Outcome.AddIssue(VonkIssue.INTERNAL_ERROR, "The request could not be processed.");
            return;
        }

        logger.LogInformation("Pushed {Count} representative relationships of user {UserId} to Firely Auth", representees.Count, userId);
        response.HttpResult = StatusCodes.Status200OK;
        response.Payload = OperationParameters.Create(representees.Count > 0, consent: null, vonkContext.InformationModel);
    }
}
