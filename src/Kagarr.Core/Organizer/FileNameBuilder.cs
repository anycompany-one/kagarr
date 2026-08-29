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

        public static string CleanFileName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            var invalid = global::System.IO.Path.GetInvalidFileNameChars();
            var result = new global::System.Text.StringBuilder(name.Length);

            foreach (var c in name)
            {
                if (global::System.Array.IndexOf(invalid, c) < 0)
                {
                    result.Append(c);
                }
                else
                {
                    result.Append('_');
                }
            }

            return result.ToString().Trim();
        }
    }
}
