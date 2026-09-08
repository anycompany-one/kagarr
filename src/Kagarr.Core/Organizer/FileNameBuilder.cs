using Kagarr.Core.Games;

namespace Kagarr.Core.Organizer
{
    public static class FileNameBuilder
    {
        public static string BuildGameFolder(Game game)
        {
            // Format: {Game Title} ({Year}) [{Platform}]
            var title = CleanFileName(game.Title);
            var platform = game.Platform.ToString().Replace("_", " ");

            if (game.Year > 0)
            {
                return $"{title} ({game.Year}) [{platform}]";
            }

            return $"{title} [{platform}]";
        }

        public static string BuildGameFileName(Game game, string originalFileName)
        {
            return BuildGameFileName(game, originalFileName, false);
        }

        public static string BuildGameFileName(Game game, string originalFileName, bool includeOriginalName)
        {
            var extension = global::System.IO.Path.GetExtension(originalFileName);
            var title = CleanFileName(game.Title);
            var platform = game.Platform.ToString().Replace("_", " ");

            var baseName = game.Year > 0
                ? $"{title} ({game.Year}) [{platform}]"
                : $"{title} [{platform}]";

            if (includeOriginalName)
            {
                var originalName = CleanFileName(global::System.IO.Path.GetFileNameWithoutExtension(originalFileName));
                if (!string.IsNullOrEmpty(originalName))
                {
                    baseName = $"{baseName} - {originalName}";
                }
            }

            return $"{baseName}{extension}";
        }

        private const string FallbackName = "Unknown Game";

        // Fixed Windows-superset set of invalid characters so sanitization is
        // identical on every platform (Path.GetInvalidFileNameChars() only
        // returns '/' and NUL on Linux, which lets names like ".." escape the
        // library root via Path.Combine).
        private static readonly char[] InvalidFileNameChars = { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };

        public static string CleanFileName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            var result = new global::System.Text.StringBuilder(name.Length);

            foreach (var c in name)
            {
                if (c < 0x20 || global::System.Array.IndexOf(InvalidFileNameChars, c) >= 0)
                {
                    result.Append('_');
                }
                else
                {
                    result.Append(c);
                }
            }

            // Trailing dots and spaces are invalid on Windows and allow
            // relative path segments ('.', '..') to survive otherwise.
            var cleaned = result.ToString().Trim().TrimEnd('.', ' ');

            if (cleaned.Length == 0 || cleaned == "." || cleaned == "..")
            {
                return FallbackName;
            }

            return cleaned;
        }
    }
}
