namespace Vonk.Plugin.AuthorizedRepresentativeExample;

/// <summary>
/// A patient that a user may act for.
/// </summary>
/// <param name="PatientId">The id of the Patient.</param>
/// <param name="FullName">The name of the patient, as Firely Auth shows it to the user.</param>
/// <param name="Relationship">How the user relates to the patient, for example <c>Mother</c>, if the RelatedPerson says so.</param>
/// <param name="Representative">The RelatedPerson that links the user to the patient, as <c>RelatedPerson/&lt;id&gt;</c>.</param>
internal sealed record Representee(string PatientId, string FullName, string? Relationship, string Representative);
