using System.Collections.Generic;
using System.Linq;

namespace Kagarr.Core.MediaFiles
{
    public static class GameFileDetector
    {
        private static readonly HashSet<string> InstallerExtensions = new HashSet<string>(global::System.StringComparer.OrdinalIgnoreCase)
        {
            ".exe", ".msi", ".sh", ".pkg", ".dmg", ".AppImage"
        };

        private static readonly HashSet<string> IsoExtensions = new HashSet<string>(global::System.StringComparer.OrdinalIgnoreCase)
        {
            ".iso", ".bin", ".cue", ".img", ".mdf", ".mds", ".nrg"
        };

        private static readonly HashSet<string> CompressedExtensions = new HashSet<string>(global::System.StringComparer.OrdinalIgnoreCase)
        {
            ".zip", ".7z", ".rar", ".tar", ".gz", ".xz", ".bz2"
        };

        private static readonly HashSet<string> RomExtensions = new HashSet<string>(global::System.StringComparer.OrdinalIgnoreCase)
        {
            ".nsp", ".xci", ".rom", ".nes", ".snes", ".sfc", ".smc",
            ".gba", ".gbc", ".gb", ".nds", ".3ds", ".cia",
            ".n64", ".z64", ".v64", ".gcm", ".wbfs", ".wad",
            ".gen", ".md", ".sms", ".gg",
            ".psx", ".pbp",
            ".vpk"
        };

        private static readonly HashSet<string> AllGameExtensions = BuildAllExtensions();

        private static HashSet<string> BuildAllExtensions()
        {
            var all = new HashSet<string>(global::System.StringComparer.OrdinalIgnoreCase);
            all.UnionWith(InstallerExtensions);
            all.UnionWith(IsoExtensions);
            all.UnionWith(CompressedExtensions);
            all.UnionWith(RomExtensions);
            return all;
        }

        public static GameFileType DetectFileType(string filePath)
        {
            var extension = global::System.IO.Path.GetExtension(filePath);
            if (string.IsNullOrEmpty(extension))
            {
                return GameFileType.Unknown;
            }

            if (InstallerExtensions.Contains(extension))
            {
                return GameFileType.Installer;
            }

            if (IsoExtensions.Contains(extension))
            {
                return GameFileType.Iso;
            }

            if (CompressedExtensions.Contains(extension))
            {
                return GameFileType.Compressed;
            }

            if (RomExtensions.Contains(extension))
            {
                return GameFileType.Rom;
            }

            return GameFileType.Unknown;
        }

        public static bool IsGameFile(string filePath)
        {
            var extension = global::System.IO.Path.GetExtension(filePath);
            return !string.IsNullOrEmpty(extension) && AllGameExtensions.Contains(extension);
        }

        public static List<string> ScanDirectory(string path)
        {
            if (!global::System.IO.Directory.Exists(path))
            {
                return new List<string>();
            }

            return global::System.IO.Directory.GetFiles(path, "*", global::System.IO.SearchOption.AllDirectories)
                .Where(IsGameFile)
                .ToList();
        }
    }
}
