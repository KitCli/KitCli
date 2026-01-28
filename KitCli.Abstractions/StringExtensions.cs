namespace KitCli.Abstractions;

public static class StringExtensions
{
    public static bool IsSimilarTo(this string ctrl, string candidate)
    {
        var exact = ctrl.Equals(candidate);
        var similar = ctrl.Contains(candidate);
        
        return exact || similar;
    }
}