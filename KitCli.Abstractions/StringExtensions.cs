namespace KitCli.Abstractions;

/// <summary>
/// Extension methods for <see cref="string"/>.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Determines whether <paramref name="candidate"/> matches <paramref name="ctrl"/> exactly or is contained
    /// within it.
    /// </summary>
    /// <param name="ctrl">The reference string to match against.</param>
    /// <param name="candidate">The string being tested.</param>
    /// <returns><see langword="true"/> if <paramref name="candidate"/> equals or is a substring of <paramref name="ctrl"/>; otherwise, <see langword="false"/>.</returns>
    public static bool IsSimilarTo(this string ctrl, string candidate)
    {
        var exact = ctrl.Equals(candidate);
        var similar = ctrl.Contains(candidate);
        
        return exact || similar;
    }
}