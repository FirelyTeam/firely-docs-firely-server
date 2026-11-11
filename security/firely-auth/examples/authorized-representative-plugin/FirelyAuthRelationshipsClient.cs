using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vonk.Plugin.AuthorizedRepresentativeExample;

/// <summary>
/// Pushes the patients a user may act for to Firely Auth.
/// </summary>
/// <remarks>
/// Use the <see cref="HttpClient"/> that the plugin configures: its base address is Firely Auth, and it gets a management API token.
/// </remarks>
internal sealed class FirelyAuthRelationshipsClient(HttpClient httpClient)
{
    /// <summary>
    /// The path of the push endpoint, relative to the base url of Firely Auth.
    /// </summary>
    public const string Path = "api/external/representative-relationships";

    private static readonly JsonSerializerOptions SerializerOptions = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    /// <summary>
    /// Replaces the patients that Firely Auth holds for the user. An empty list means the user may act for nobody.
    /// </summary>
    /// <exception cref="HttpRequestException">Firely Auth could not be reached or refused the push.</exception>
    public async Task PushAsync(string userId, IReadOnlyList<Representee> representees, CancellationToken cancellationToken)
    {
        var body = new RelationshipsPush(userId,
            [..representees.Select(r => new RelationshipsPush.Entry(r.PatientId, r.FullName, r.Relationship, r.Representative))]);

        using var response = await httpClient.PostAsJsonAsync(Path, body, SerializerOptions, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var answer = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Firely Auth answered {(int)response.StatusCode} to the push: {answer}", null, response.StatusCode);
        }
    }

    /// <summary>
    /// The body of the push.
    /// </summary>
    internal sealed record RelationshipsPush(string UserId, IReadOnlyList<RelationshipsPush.Entry> Relationships)
    {
        /// <summary>
        /// One patient the user may act for.
        /// </summary>
        internal sealed record Entry(string PatientId, string FullName, string? Relationship, string Representative);
    }
}
