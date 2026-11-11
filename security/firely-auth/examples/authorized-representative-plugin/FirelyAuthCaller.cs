using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace Vonk.Plugin.AuthorizedRepresentativeExample;

/// <summary>
/// Checks that a request comes from Firely Auth itself.
/// </summary>
/// <remarks>
/// Firely Auth calls both operations with a token it issues to itself, with the client id <see cref="ClientId"/>.
/// Firely Auth refuses to register a client with that id. But a client that already had that id keeps working, and Firely Auth logs a warning about it at startup.
/// This is a check on the name only: it relies on no other client in Firely Auth having that client id. Give such a client another id before you enable the operations.
/// </remarks>
internal static class FirelyAuthCaller
{
    /// <summary>
    /// The client id of the tokens that Firely Auth issues to itself.
    /// </summary>
    public const string ClientId = "Firely-Auth-Internal-FhirLookup";

    private const string ClientIdClaim = "client_id";
    private const string SubjectClaim = "sub";

    /// <summary>
    /// Returns whether <paramref name="user"/> holds a token of Firely Auth itself.
    /// </summary>
    public static bool IsFirelyAuth(ClaimsPrincipal user) =>
        user.Claims.Where(c => c.Type == ClientIdClaim).Select(c => c.Value).ToList() is [ClientId];

    /// <summary>
    /// Returns the Firely Auth user id that a token of Firely Auth itself carries in its <c>sub</c> claim.
    /// </summary>
    /// <returns><see langword="false"/> when the token is not one of Firely Auth itself, or has no <c>sub</c> claim.</returns>
    public static bool TryGetUserId(ClaimsPrincipal user, [NotNullWhen(true)] out string? userId)
    {
        userId = null;
        if (!IsFirelyAuth(user))
            return false;

        // A JWT bearer token has 'sub' mapped to NameIdentifier, a token read by introspection keeps 'sub'
        var subjects = user.Claims
            .Where(c => c.Type is SubjectClaim or ClaimTypes.NameIdentifier)
            .Select(c => c.Value)
            .Distinct()
            .ToList();
        if (subjects is not [{ Length: > 0 } subject])
            return false;

        userId = subject;
        return true;
    }
}
