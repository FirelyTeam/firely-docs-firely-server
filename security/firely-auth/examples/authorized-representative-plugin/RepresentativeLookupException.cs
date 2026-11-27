namespace Vonk.Plugin.AuthorizedRepresentativeExample;

/// <summary>
/// The lookup of whom a user may act for could not be done.
/// </summary>
internal sealed class RepresentativeLookupException(string message) : Exception(message);
