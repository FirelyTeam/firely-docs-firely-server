using Hl7.Fhir.ElementModel;
using Vonk.Core.ElementModel;
using Vonk.Core.Support;

namespace Vonk.Plugin.AuthorizedRepresentativeExample;

/// <summary>
/// Reads a FHIR <c>Period</c>.
/// </summary>
internal static class FhirPeriod
{
    /// <summary>
    /// Returns whether <paramref name="period"/> covers <paramref name="now"/>.
    /// </summary>
    /// <remarks>
    /// A missing period, start or end is open. A partial date covers the whole year, month or day.
    /// A start or end that cannot be read covers nothing.
    /// </remarks>
    public static bool Covers(ISourceNode? period, DateTimeOffset now)
    {
        if (period is null)
            return true;

        var start = period.ChildString("start");
        var end = period.ChildString("end");

        if (start is not null && (!DateTimeRangeParser.TryParse(start, out var startRange) || now < startRange.Start))
            return false;

        // An end is inclusive at its precision: 2026-11-06 covers that whole day, and a dateTime in seconds covers that whole second
        if (end is not null && (!DateTimeRangeParser.TryParse(end, out var endRange) || now >= endRange.End))
            return false;

        return true;
    }
}
