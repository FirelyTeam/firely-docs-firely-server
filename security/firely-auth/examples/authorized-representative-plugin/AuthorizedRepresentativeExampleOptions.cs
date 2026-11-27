namespace Vonk.Plugin.AuthorizedRepresentativeExample;

/// <summary>
/// The settings of the example plugin, in the section <c>AuthorizedRepresentativeExample</c>.
/// </summary>
internal sealed class AuthorizedRepresentativeExampleOptions
{
    /// <summary>
    /// The name of the configuration section.
    /// </summary>
    public const string Section = "AuthorizedRepresentativeExample";

    /// <summary>
    /// The base url of Firely Auth. Leave it empty to use <c>SmartAuthorizationOptions:Authority</c>.
    /// It must use https. Plain http is accepted for a loopback address, or when <c>SmartAuthorizationOptions:RequireHttpsToProvider</c> is <c>false</c>.
    /// </summary>
    public string? FirelyAuthUrl { get; set; }

    /// <summary>
    /// The id of the management API client in Firely Auth that pushes the relationships.
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// The secret of the management API client in Firely Auth.
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// The number of seconds the push to Firely Auth may take, including the request for the management API token.
    /// Keep it below <c>AuthorizedRepresentatives:CheckAuthorizedRepresentativeTimeout</c> of Firely Auth, which is 5 seconds by default.
    /// </summary>
    public int PushTimeoutSeconds { get; set; } = 3;
}
