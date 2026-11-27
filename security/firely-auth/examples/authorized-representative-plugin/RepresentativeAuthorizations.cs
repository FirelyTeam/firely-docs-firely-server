using System.Text.RegularExpressions;
using Hl7.Fhir.ElementModel;
using Microsoft.Extensions.Logging;
using Vonk.Core.Common;
using Vonk.Core.Context;
using Vonk.Core.ElementModel;
using Vonk.Core.Repository;
using Vonk.Core.Support;

namespace Vonk.Plugin.AuthorizedRepresentativeExample;

/// <summary>
/// Finds out in Firely Server whom a user may act for.
/// </summary>
/// <remarks>
/// A user may act for a patient through a RelatedPerson of that patient that carries the identifier
/// <c>http://server.fire.ly/auth/identifier/sub|&lt;user id&gt;</c>. Firely Auth adds that identifier when an admin links the RelatedPerson to the user.
/// The RelatedPerson must be active, its period must cover the current time, and its Patient must exist.
/// This example also requires an active privacy Consent that permits the RelatedPerson as actor, see <see cref="DecideAuthorizationAsync"/>.
/// A Consent is optional: replace that method to decide the authorization in another way. The checks above stay in place.
/// </remarks>
internal sealed partial class RepresentativeAuthorizations(ISearchRepository searchRepository, TimeProvider timeProvider, ILogger<RepresentativeAuthorizations> logger)
{
    /// <summary>
    /// The system of the identifier that holds the id of a user in Firely Auth.
    /// </summary>
    public const string SubIdentifierSystem = "http://server.fire.ly/auth/identifier/sub";

    /// <summary>
    /// The code system of <c>Consent.scope</c> in FHIR R4. FHIR R5 has these codes in <c>Consent.category</c>.
    /// </summary>
    private const string ConsentScopeSystem = "http://terminology.hl7.org/CodeSystem/consentscope";

    /// <summary>
    /// The scope of a Consent about access to the patient's record.
    /// </summary>
    private const string PrivacyScope = "patient-privacy";

    private const int MaxResults = 50;

    private enum ConsentDecision
    {
        /// <summary>
        /// The Consent says nothing about the RelatedPerson now.
        /// </summary>
        Abstain,

        /// <summary>
        /// The Consent permits the RelatedPerson access to the whole record now.
        /// </summary>
        Permit,

        /// <summary>
        /// The Consent denies the RelatedPerson access now.
        /// </summary>
        Deny,
    }

    [GeneratedRegex(@"^Patient/(?<id>[A-Za-z0-9\-\.]{1,64})(?:/_history/[^/]+)?$")]
    private static partial Regex PatientReference();

    [GeneratedRegex(@"^RelatedPerson/(?<id>[A-Za-z0-9\-\.]{1,64})(?:/_history/[^/]+)?$")]
    private static partial Regex RelatedPersonReference();

    /// <summary>
    /// Returns the patients that the user may act for now, each patient once.
    /// </summary>
    /// <param name="userId">The id of the user in Firely Auth.</param>
    /// <param name="vonkContext">The request the lookup is for.</param>
    public async Task<IReadOnlyList<Representee>> FindRepresenteesAsync(string userId, IVonkContext vonkContext)
    {
        RequireSupportedInformationModel(vonkContext);
        var relatedPersons = await SearchAsync(vonkContext, "RelatedPerson", ("identifier", $"{SubIdentifierSystem}|{EscapeSearchValue(userId)}"));

        List<(string PatientId, IResource RelatedPerson)> authorized = [];
        // A token search can ignore case, depending on the database. Keep only the RelatedPersons linked to exactly this user.
        foreach (var relatedPerson in relatedPersons.Where(r => IsLinkedTo(r, userId)).OrderBy(r => r.Id, StringComparer.Ordinal))
        {
            if (await FindAuthorizationAsync(relatedPerson, vonkContext) is not (var patientId, _))
                continue;

            // Firely Auth refuses a push that names a patient twice, so keep the first RelatedPerson
            if (authorized.Any(a => a.PatientId == patientId))
            {
                logger.LogWarning("User {UserId} has more than one authorized RelatedPerson for Patient/{PatientId}. Only the first one is used, RelatedPerson/{RelatedPersonId} is left out",
                    userId, patientId, relatedPerson.Id);
                continue;
            }

            authorized.Add((patientId, relatedPerson));
        }

        if (authorized.Count == 0)
            return [];

        var patients = (await SearchAsync(vonkContext, "Patient", (ArgumentNames.resourceId, string.Join(',', authorized.Select(a => a.PatientId)))))
            .ToDictionary(p => p.Id, StringComparer.Ordinal);

        List<Representee> representees = [];
        foreach (var (patientId, relatedPerson) in authorized)
        {
            if (!patients.TryGetValue(patientId, out var patient))
            {
                logger.LogWarning("RelatedPerson/{RelatedPersonId} refers to Patient/{PatientId}, which does not exist", relatedPerson.Id, patientId);
                continue;
            }

            representees.Add(new Representee(patientId, PatientName(patient), RelationshipText(relatedPerson), $"RelatedPerson/{relatedPerson.Id}"));
        }

        return representees;
    }

    /// <summary>
    /// Returns whether the RelatedPerson may act for the patient now, on behalf of the user.
    /// </summary>
    /// <param name="userId">The id in Firely Auth of the user who chose.</param>
    /// <param name="patientId">The id of the Patient.</param>
    /// <param name="relatedPersonId">The id of the RelatedPerson.</param>
    /// <param name="vonkContext">The request the check is for.</param>
    /// <returns>The authorization, or <see langword="null"/> when the RelatedPerson may not act for the patient.</returns>
    public async Task<RepresentativeAuthorization?> CheckAsync(string userId, string patientId, string relatedPersonId, IVonkContext vonkContext)
    {
        RequireSupportedInformationModel(vonkContext);
        var relatedPerson = (await SearchAsync(vonkContext, "RelatedPerson", (ArgumentNames.resourceId, relatedPersonId))).SingleOrDefault();
        if (relatedPerson is null)
            return null;

        // Only a RelatedPerson that is linked to this user in Firely Auth. It may have been linked to another user since the choice.
        if (!IsLinkedTo(relatedPerson, userId))
            return null;

        if (await FindAuthorizationAsync(relatedPerson, vonkContext) is not (var authorizedPatientId, var authorization) || authorizedPatientId != patientId)
            return null;

        // A RelatedPerson and its Consent can outlive the Patient. The lookup leaves such a patient out, so the check refuses it too.
        if ((await SearchAsync(vonkContext, "Patient", (ArgumentNames.resourceId, patientId))).Count == 0)
        {
            logger.LogWarning("RelatedPerson/{RelatedPersonId} refers to Patient/{PatientId}, which does not exist", relatedPersonId, patientId);
            return null;
        }

        return authorization;
    }

    /// <summary>
    /// Returns whether the RelatedPerson is linked to exactly this user. A RelatedPerson that carries the ids of several users is linked to nobody.
    /// </summary>
    private static bool IsLinkedTo(IResource relatedPerson, string userId) =>
        relatedPerson.Children("identifier").Where(i => i.ChildString("system") == SubIdentifierSystem).Select(i => i.ChildString("value")).ToList() is [var linkedUserId] &&
        linkedUserId == userId;

    // The Consent resource differs between FHIR versions, and this example reads only R4 and R5
    private void RequireSupportedInformationModel(IVonkContext vonkContext)
    {
        if (vonkContext.InformationModel is VonkConstants.Model.FhirR4 or VonkConstants.Model.FhirR5)
            return;

        logger.LogError("This plugin decides authorizations for FHIR R4 and R5 only, not for {InformationModel}", vonkContext.InformationModel);
        throw new RepresentativeLookupException($"Consent resources of {vonkContext.InformationModel} are not supported.");
    }

    private async Task<(string PatientId, RepresentativeAuthorization Authorization)?> FindAuthorizationAsync(IResource relatedPerson, IVonkContext vonkContext)
    {
        var now = timeProvider.GetUtcNow();
        if (relatedPerson.ChildString("active") == "false" || !FhirPeriod.Covers(relatedPerson.Child("period"), now))
            return null;

        if (relatedPerson.Child("patient")?.ChildString("reference") is not { } reference ||
            PatientReference().Match(ToRelative(reference, vonkContext.ServerBase)) is not { Success: true } match)
            return null;

        var patientId = match.Groups["id"].Value;
        return await DecideAuthorizationAsync(patientId, relatedPerson, vonkContext, now) is { } authorization
            ? (patientId, authorization)
            : null;
    }

    /// <summary>
    /// Decides whether the RelatedPerson may act for the patient now.
    /// </summary>
    /// <remarks>
    /// This example requires an active privacy Consent of the patient that permits the RelatedPerson as actor now, and no such Consent that denies it.
    /// A privacy Consent has the code <c>patient-privacy</c> of <c>http://terminology.hl7.org/CodeSystem/consentscope</c>, in <c>scope</c> for FHIR R4 and in <c>category</c> for FHIR R5.
    /// A Consent for research, treatment or an advance directive authorizes nobody.
    /// For FHIR R4, the RelatedPerson is an actor of the root <c>provision</c>, whose <c>period</c> covers now. Its <c>type</c> decides: <c>permit</c> permits and <c>deny</c> denies.
    /// A root provision without a <c>type</c> authorizes nobody, because this example does not read <c>policyRule</c>, and so cannot tell whether the provision adds or removes access.
    /// For FHIR R5, <c>decision</c> is the base rule and each <c>provision</c> an exception to it. So a <c>provision</c> that names the RelatedPerson permits
    /// in a Consent with <c>decision</c> = <c>deny</c>, and denies in a Consent with <c>decision</c> = <c>permit</c>. Both the Consent's <c>period</c> and the provision's <c>period</c> must cover now.
    /// A Consent that denies overrules every Consent that permits. So a patient can withdraw an authorization with a second Consent.
    /// This example does not evaluate nested provisions: a provision that has them authorizes nobody.
    /// A provision that permits only some actions, purposes or data authorizes nobody either, because the token gives access to the whole record.
    /// A Consent is optional for Firely Auth. When you do not create Consent resources, decide the authorization here in another way,
    /// and return a <see cref="RepresentativeAuthorization"/> without a Consent.
    /// </remarks>
    /// <param name="patientId">The id of the Patient that the RelatedPerson refers to.</param>
    /// <param name="relatedPerson">The RelatedPerson. It is linked to the user, active and in force now.</param>
    /// <param name="vonkContext">The request the decision is for.</param>
    /// <param name="now">The current time.</param>
    /// <returns>The authorization, or <see langword="null"/> when the RelatedPerson may not act for the patient.</returns>
    private async Task<RepresentativeAuthorization?> DecideAuthorizationAsync(string patientId, IResource relatedPerson, IVonkContext vonkContext, DateTimeOffset now)
    {
        var relatedPersonId = relatedPerson.Id;

        var consents = await SearchAsync(vonkContext, "Consent",
            ("patient", $"Patient/{patientId}"),
            ("actor", $"RelatedPerson/{relatedPersonId}"),
            ("status", "active"));

        // The search also matches a reference to a contained resource with the same id, so check the references themselves
        var isR5 = vonkContext.InformationModel == VonkConstants.Model.FhirR5;
        var serverBase = vonkContext.ServerBase;
        var decisions = consents
            .Where(c => IsReferenceTo(c.Child(isR5 ? "subject" : "patient"), PatientReference(), patientId, serverBase))
            .Where(c => IsPrivacyConsent(c, isR5))
            .OrderBy(c => c.Id, StringComparer.Ordinal)
            .Select(c => (Consent: c, Decision: isR5 ? DecideR5(c, relatedPersonId, serverBase, now) : DecideR4(c, relatedPersonId, serverBase, now)))
            .ToList();

        // A Consent that denies overrules every Consent that permits
        if (decisions.Where(d => d.Decision == ConsentDecision.Deny).Select(d => d.Consent).FirstOrDefault() is { } denying)
        {
            logger.LogInformation("Consent/{ConsentId} denies RelatedPerson/{RelatedPersonId} access to Patient/{PatientId}", denying.Id, relatedPersonId, patientId);
            return null;
        }

        return decisions.Where(d => d.Decision == ConsentDecision.Permit).Select(d => d.Consent).FirstOrDefault() is { } permitting
            ? new RepresentativeAuthorization($"Consent/{permitting.Id}")
            : null;
    }

    /// <summary>
    /// Returns whether the Consent is about access to the patient's record. A Consent for research, treatment or an advance directive is not.
    /// </summary>
    private static bool IsPrivacyConsent(IResource consent, bool isR5) =>
        consent.Children(isR5 ? "category" : "scope")
            .SelectMany(c => c.Children("coding"))
            .Any(c => c.ChildString("system") == ConsentScopeSystem && c.ChildString("code") == PrivacyScope);

    // The elements of Consent.provision in R4 and R5 that narrow what is permitted
    private static readonly string[] RestrictingProvisionElements =
        ["action", "securityLabel", "purpose", "class", "code", "dataPeriod", "data", "documentType", "resourceType", "expression"];

    private ConsentDecision DecideR4(IResource consent, string relatedPersonId, Uri serverBase, DateTimeOffset now)
    {
        var provision = consent.Child("provision");
        if (provision is null || !IsActor(provision, relatedPersonId, serverBase) || !FhirPeriod.Covers(provision.Child("period"), now))
            return ConsentDecision.Abstain;

        switch (provision.ChildString("type"))
        {
            case "deny":
                // Also when the provision denies only part of the record: a token for the whole record cannot leave that part out
                return ConsentDecision.Deny;
            case "permit":
                return IsUnrestricted(consent, provision) ? ConsentDecision.Permit : ConsentDecision.Abstain;
            default:
                logger.LogWarning("The provision of Consent/{ConsentId} has no type 'permit' or 'deny'. It does not authorize anyone", consent.Id);
                return ConsentDecision.Abstain;
        }
    }

    private ConsentDecision DecideR5(IResource consent, string relatedPersonId, Uri serverBase, DateTimeOffset now)
    {
        if (!FhirPeriod.Covers(consent.Child("period"), now))
            return ConsentDecision.Abstain;

        var provisions = consent.Children("provision")
            .Where(p => IsActor(p, relatedPersonId, serverBase))
            .ToList();
        if (!provisions.Any(p => FhirPeriod.Covers(p.Child("period"), now)))
            return ConsentDecision.Abstain;

        // A provision is an exception to the decision. So it permits in a Consent that denies by default, and denies in a Consent that permits by default.
        switch (consent.ChildString("decision"))
        {
            case "permit":
                return ConsentDecision.Deny;
            case "deny":
                return provisions.All(p => IsUnrestricted(consent, p)) ? ConsentDecision.Permit : ConsentDecision.Abstain;
            default:
                return ConsentDecision.Abstain;
        }
    }

    /// <summary>
    /// Returns whether the provision permits everything in the patient's record.
    /// A token for the representative gives access to the whole record, so a provision that permits only some actions, purposes or data authorizes nobody.
    /// So does a provision with nested provisions, which this example does not evaluate.
    /// </summary>
    private bool IsUnrestricted(IResource consent, ISourceNode provision)
    {
        if (provision.Children("provision").Any())
        {
            logger.LogWarning("Consent/{ConsentId} has nested provisions, which this plugin does not evaluate. It does not authorize anyone", consent.Id);
            return false;
        }

        if (RestrictingProvisionElements.FirstOrDefault(e => provision.Children(e).Any()) is { } restriction)
        {
            logger.LogWarning("Consent/{ConsentId} restricts its provision with '{Element}', which a token for the whole record cannot enforce. It does not authorize anyone",
                consent.Id, restriction);
            return false;
        }

        return true;
    }

    private static bool IsActor(ISourceNode provision, string relatedPersonId, Uri serverBase) =>
        provision.Children("actor").Any(a => IsReferenceTo(a.Child("reference"), RelatedPersonReference(), relatedPersonId, serverBase));

    /// <summary>
    /// Returns whether <paramref name="reference"/> refers to the resource with <paramref name="id"/> on this server, with or without a version.
    /// A reference to another server, or to a contained resource, does not.
    /// </summary>
    private static bool IsReferenceTo(ISourceNode? reference, Regex pattern, string id, Uri serverBase) =>
        reference?.ChildString("reference") is { } value &&
        pattern.Match(ToRelative(value, serverBase)) is { Success: true } match &&
        match.Groups["id"].Value == id;

    /// <summary>
    /// Returns <paramref name="reference"/> relative to <paramref name="serverBase"/>. A reference to another server stays absolute, and so matches no local resource.
    /// </summary>
    private static string ToRelative(string reference, Uri serverBase) => UrlMapping.MakeRelative(reference, serverBase);

    /// <summary>
    /// Escapes the characters that have a meaning in a FHIR search value, so that the value is matched as it is.
    /// </summary>
    private static string EscapeSearchValue(string value) =>
        value.Replace(@"\", @"\\").Replace("|", @"\|").Replace(",", @"\,").Replace("$", @"\$");

    private async Task<IReadOnlyList<IResource>> SearchAsync(IVonkContext vonkContext, string resourceType, params (string Name, string Value)[] parameters)
    {
        ArgumentCollection arguments = new();
        arguments.AddArgument(new Argument(ArgumentSource.Internal, ArgumentNames.resourceType, resourceType));
        foreach (var (name, value) in parameters)
        {
            arguments.AddArgument(new Argument(ArgumentSource.Internal, name, value) { MustHandle = true });
        }

        // One more than the limit, to tell a complete result from a cut-off one. The total number of matches is not needed, so the repository does not count them.
        arguments.AddCount(MaxResults + 1).AddTotalNone();

        // The lookup is on behalf of Firely Auth, which has no access to these resources itself.
        // It reads from the primary database, so that a Consent that was just created or withdrawn counts.
        List<IResource> result;
        try
        {
            result = (await searchRepository.SearchCurrentWithFullAccess(arguments, vonkContext.ServerBase, vonkContext.InformationModel)).ToList();
        }
        catch (VonkParameterException ex)
        {
            // The plugin builds these searches itself, so a refused parameter is a defect here and not a fault of the caller
            logger.LogError(ex, "The repository refused the search for {ResourceType}", resourceType);
            throw new RepresentativeLookupException($"The search for {resourceType} could not be done.");
        }

        if (arguments.Any(a => a.MustHandle && a.Status != ArgumentStatus.Handled))
        {
            var unhandled = string.Join(", ", arguments.Where(a => a.MustHandle && a.Status != ArgumentStatus.Handled).Select(a => a.ArgumentName));
            logger.LogError("The repository did not handle the search parameter(s) {Parameters} on {ResourceType}", unhandled, resourceType);
            throw new RepresentativeLookupException($"The search for {resourceType} could not be done.");
        }

        // A cut-off result could leave out a RelatedPerson or the Consent that is in force, so the lookup fails instead
        if (result.Count > MaxResults)
        {
            logger.LogError("The search for {ResourceType} found more than {MaxResults} resources, which this plugin does not support", resourceType, MaxResults);
            throw new RepresentativeLookupException($"The search for {resourceType} found too many resources.");
        }

        return result;
    }

    private static string PatientName(IResource patient)
    {
        var name = patient.Child("name");
        if (name?.ChildString("text") is { Length: > 0 } text)
            return text;

        var parts = (name?.Children("given") ?? []).Select(g => g.Text).Append(name?.ChildString("family")).Where(p => !string.IsNullOrWhiteSpace(p));
        var fullName = string.Join(' ', parts);
        return fullName.Length > 0 ? fullName : $"Patient/{patient.Id}";
    }

    private static string? RelationshipText(IResource relatedPerson)
    {
        var relationship = relatedPerson.Child("relationship");
        var coding = relationship?.Child("coding");
        return relationship?.ChildString("text") ?? coding?.ChildString("display") ?? coding?.ChildString("code");
    }
}
