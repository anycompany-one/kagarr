using System.Text.RegularExpressions;

namespace Kagarr.Core.Parser
{
    public static class GameTitleParser
    {
        // Match release group suffix: -GROUP at end of string
        private static readonly Regex ReleaseGroupRegex = new Regex(
            @"-(?<group>[A-Za-z0-9]+)$",
            RegexOptions.Compiled);

        // Match version patterns: v1.0, v1.2.3, Update.5, etc.
        private static readonly Regex VersionRegex = new Regex(
            @"[._\s]v?(?<version>\d+\.\d+(?:\.\d+)*)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Match year in parentheses or after dots: (2023) or .2023.
        private static readonly Regex YearRegex = new Regex(
            @"[\.\s\(](?<year>(?:19|20)\d{2})[\.\s\)]",
            RegexOptions.Compiled);

        // Match platform tags: [PC], (PC), .PC., [PS5], etc.
        private static readonly Regex PlatformRegex = new Regex(
            @"[\[\.\s\(](?<platform>PC|PS5|PS4|PS3|Xbox|XboxOne|XSX|Switch|NSW|NES|SNES|N64|GBA|GBC|NDS|3DS|PSP|PSX|Wii|WiiU|Genesis|Dreamcast)[\]\.\s\)]",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Common scene group names to confirm group detection
        private static readonly Regex SceneSeparatorRegex = new Regex(
            @"[\._]",
            RegexOptions.Compiled);

        public static ParsedGameTitle ParseTitle(string releaseTitle)
        {
            if (string.IsNullOrWhiteSpace(releaseTitle))
            {
                return new ParsedGameTitle
                {
                    CleanTitle = string.Empty,
                    OriginalTitle = releaseTitle ?? string.Empty
                };
            }

            var result = new ParsedGameTitle
            {
                OriginalTitle = releaseTitle
            };

            var working = releaseTitle.Trim();

            // Extract release group (must be done before other parsing to avoid confusion)
            var groupMatch = ReleaseGroupRegex.Match(working);
            if (groupMatch.Success)
            {
                result.ReleaseGroup = groupMatch.Groups["group"].Value;
                working = working.Substring(0, groupMatch.Index).TrimEnd();
            }

            // Extract version
            var versionMatch = VersionRegex.Match(working);
            if (versionMatch.Success)
            {
                result.Version = versionMatch.Groups["version"].Value;
            }

            // Extract year
            var yearMatch = YearRegex.Match(working);
            if (yearMatch.Success && int.TryParse(yearMatch.Groups["year"].Value, out var year))
            {
                result.Year = year;
            }

            // Extract platform
            var platformMatch = PlatformRegex.Match(working);
            if (platformMatch.Success)
            {
                result.Platform = platformMatch.Groups["platform"].Value.ToUpperInvariant();
            }

            // Build clean title: replace dots and underscores with spaces, strip metadata
            var clean = working;

            // Remove version string
            if (versionMatch.Success)
            {
                clean = clean.Remove(versionMatch.Index, versionMatch.Length);
            }

            // Remove year with surrounding brackets/dots
            if (yearMatch.Success)
            {
                clean = YearRegex.Replace(clean, " ");
            }

            // Remove platform tag with surrounding brackets/dots
            if (platformMatch.Success)
            {
                clean = PlatformRegex.Replace(clean, " ");
            }

            // Replace dots and underscores with spaces
            clean = SceneSeparatorRegex.Replace(clean, " ");

            // Collapse multiple spaces and trim
            clean = Regex.Replace(clean, @"\s+", " ").Trim();

            // Remove trailing brackets/parentheses artifacts
            clean = Regex.Replace(clean, @"[\[\]\(\)]+\s*$", string.Empty).Trim();

            result.CleanTitle = clean;

            return result;
        }
    }
}
