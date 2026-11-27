namespace Vonk.Plugin.AuthorizedRepresentativeExample;

/// <summary>
/// The authorization of a RelatedPerson to act for a patient now.
/// </summary>
/// <param name="Consent">The Consent that authorizes the RelatedPerson, as <c>Consent/&lt;id&gt;</c>, or <see langword="null"/> when the authorization is decided without a Consent.</param>
internal sealed record RepresentativeAuthorization(string? Consent);
