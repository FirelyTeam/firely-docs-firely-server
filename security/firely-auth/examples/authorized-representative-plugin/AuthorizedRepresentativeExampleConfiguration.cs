using Duende.AccessTokenManagement;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Vonk.Core.Common;
using Vonk.Core.Context;
using Vonk.Core.Pluggability;

namespace Vonk.Plugin.AuthorizedRepresentativeExample;

/// <summary>
/// Adds the operations <c>$representative-relationships</c> and <c>Patient/$check-authorized-representative</c>,
/// which Firely Auth calls to let a user act for someone they represent.
/// </summary>
/// <remarks>
/// Add <c>Vonk.Plugin.AuthorizedRepresentativeExample</c> to <c>PipelineOptions</c>, and both operations to <c>Operations</c>.
/// Configure the management API client in the section <see cref="AuthorizedRepresentativeExampleOptions.Section"/>.
/// </remarks>
[VonkConfiguration(order: 5600)]
public static class AuthorizedRepresentativeExampleConfiguration
{
    private static readonly ClientCredentialsClientName FirelyAuthClient = ClientCredentialsClientName.Parse("firely-auth-representative-relationships");

    private const string ManagementApiScope = "http://server.fire.ly/auth/scope/authmanagement";
    private const string ManagementApiResource = "http://server.fire.ly/auth/api/management";

    /// <summary>
    /// Registers the services of the plugin.
    /// </summary>
    public static IServiceCollection ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(AuthorizedRepresentativeExampleOptions.Section).Get<AuthorizedRepresentativeExampleOptions>() ?? new();
        var firelyAuthUrl = string.IsNullOrEmpty(options.FirelyAuthUrl) ? configuration["SmartAuthorizationOptions:Authority"] : options.FirelyAuthUrl;

        if (string.IsNullOrEmpty(firelyAuthUrl) || !Uri.TryCreate(firelyAuthUrl.TrimEnd('/') + "/", UriKind.Absolute, out var firelyAuthBase))
            throw new VonkConfigurationException(
                $"Set '{AuthorizedRepresentativeExampleOptions.Section}:{nameof(AuthorizedRepresentativeExampleOptions.FirelyAuthUrl)}' or 'SmartAuthorizationOptions:Authority' to the url of Firely Auth.");

        if (string.IsNullOrEmpty(options.ClientId) || string.IsNullOrEmpty(options.ClientSecret))
            throw new VonkConfigurationException(
                $"Set '{AuthorizedRepresentativeExampleOptions.Section}:{nameof(AuthorizedRepresentativeExampleOptions.ClientId)}' and " +
                $"'{AuthorizedRepresentativeExampleOptions.Section}:{nameof(AuthorizedRepresentativeExampleOptions.ClientSecret)}' to a management API client of Firely Auth.");

        services.AddClientCredentialsTokenManagement()
            .AddClient(FirelyAuthClient, client =>
            {
                client.TokenEndpoint = new Uri(firelyAuthBase, "connect/token");
                client.ClientId = ClientId.Parse(options.ClientId);
                client.ClientSecret = ClientSecret.Parse(options.ClientSecret);
                client.Scope = Scope.Parse(ManagementApiScope);
                client.Resource = Resource.Parse(ManagementApiResource);
            });

        services.AddHttpClient<FirelyAuthRelationshipsClient>(client => client.BaseAddress = firelyAuthBase)
            .AddClientCredentialsTokenHandler(FirelyAuthClient);

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddScoped<RepresentativeAuthorizations>();
        services.TryAddScoped<RepresentativeRelationshipsOperation>();
        services.TryAddScoped<CheckAuthorizedRepresentativeOperation>();
        return services;
    }

    /// <summary>
    /// Adds the operations to the pipeline.
    /// </summary>
    public static IApplicationBuilder Configure(IApplicationBuilder builder)
    {
        builder.OnCustomInteraction(VonkInteraction.system_custom, RepresentativeRelationshipsOperation.Name)
            .AndMethod(HttpMethods.Post)
            .HandleAsyncWith<RepresentativeRelationshipsOperation>((operation, context) => operation.HandleAsync(context));

        builder.OnCustomInteraction(VonkInteraction.type_custom, CheckAuthorizedRepresentativeOperation.Name)
            .AndMethod(HttpMethods.Post)
            .AndResourceTypes("Patient")
            .HandleAsyncWith<CheckAuthorizedRepresentativeOperation>((operation, context) => operation.HandleAsync(context));

        return builder;
    }
}
