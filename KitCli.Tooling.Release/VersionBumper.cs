using System.Text;
using System.Text.RegularExpressions;

namespace KitCli.Tooling.Release;

public static class VersionBumper
{
    public static void BumpPatchVersion(ProjectInfo project)
    {
        var text = File.ReadAllText(project.Path);
        var match = Regex.Match(text, @"<Version>(\d+)\.(\d+)\.(\d+)</Version>");
        var newPatch = int.Parse(match.Groups[3].Value) + 1;
        var newVersion = $"{match.Groups[1].Value}.{match.Groups[2].Value}.{newPatch}";

        // A plain string replace on the matched text, not an XDocument round-trip — csproj
        // files in this repo have their own formatting (4-space indent, blank lines between
        // PropertyGroups), and re-serializing the whole document would blow that away for a
        // one-line change.
        var updatedText = text.Remove(match.Index, match.Length).Insert(match.Index, $"<Version>{newVersion}</Version>");
        File.WriteAllText(project.Path, updatedText, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        project.Version = newVersion;
        Console.WriteLine($"Bumped {project.PackageId} to {newVersion}");
    }
}
