using System;
using System.Runtime.InteropServices;
using Kagarr.Common.Instrumentation;
using NLog;

namespace Kagarr.Core.MediaFiles
{
    public interface IDiskTransferService
    {
        TransferMode TransferFile(string sourcePath, string targetPath, TransferMode mode);
    }

    public class DiskTransferService : IDiskTransferService
    {
        private readonly Logger _logger;

        public DiskTransferService()
        {
            _logger = KagarrLogger.GetLogger(this);
        }

        public TransferMode TransferFile(string sourcePath, string targetPath, TransferMode mode)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                throw new ArgumentException("Source path cannot be empty", nameof(sourcePath));
            }

            if (string.IsNullOrWhiteSpace(targetPath))
            {
                throw new ArgumentException("Target path cannot be empty", nameof(targetPath));
            }

            if (!global::System.IO.File.Exists(sourcePath))
            {
                throw new global::System.IO.FileNotFoundException("Source file not found", sourcePath);
            }

            // Ensure target directory exists
            var targetDir = global::System.IO.Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(targetDir))
            {
                global::System.IO.Directory.CreateDirectory(targetDir);
            }

            // Remove existing target file if it exists
            if (global::System.IO.File.Exists(targetPath))
            {
                global::System.IO.File.Delete(targetPath);
            }

            if (mode.HasFlag(TransferMode.HardLink))
            {
                if (TryHardLink(sourcePath, targetPath))
                {
                    _logger.Debug("Hard linked '{0}' to '{1}'", sourcePath, targetPath);
                    return TransferMode.HardLink;
                }

                _logger.Debug("Hard link failed for '{0}', falling back", sourcePath);

                // Fall back to copy if HardLinkOrCopy, otherwise fail
                if (mode.HasFlag(TransferMode.Copy))
                {
                    CopyFile(sourcePath, targetPath);
                    return TransferMode.Copy;
                }

                throw new global::System.IO.IOException($"Failed to create hard link from '{sourcePath}' to '{targetPath}'");
            }

            if (mode.HasFlag(TransferMode.Copy))
            {
                CopyFile(sourcePath, targetPath);
                return TransferMode.Copy;
            }

            if (mode.HasFlag(TransferMode.Move))
            {
                MoveFile(sourcePath, targetPath);
                return TransferMode.Move;
            }

            throw new ArgumentException($"Unsupported transfer mode: {mode}", nameof(mode));
        }

        private bool TryHardLink(string sourcePath, string targetPath)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return CreateHardLinkWindows(targetPath, sourcePath);
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                    RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return CreateHardLinkUnix(sourcePath, targetPath);
                }

                _logger.Warn("Hard links not supported on this platform: {0}", RuntimeInformation.OSDescription);
                return false;
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Hard link creation failed for '{0}'", sourcePath);
                return false;
            }
        }

        private void CopyFile(string sourcePath, string targetPath)
        {
            _logger.Debug("Copying '{0}' to '{1}'", sourcePath, targetPath);
            global::System.IO.File.Copy(sourcePath, targetPath, true);
        }

        private void MoveFile(string sourcePath, string targetPath)
        {
            _logger.Debug("Moving '{0}' to '{1}'", sourcePath, targetPath);
            global::System.IO.File.Move(sourcePath, targetPath);
        }

        private static bool CreateHardLinkWindows(string targetPath, string sourcePath)
        {
            return NativeMethods.CreateHardLink(targetPath, sourcePath, IntPtr.Zero);
        }

        private static bool CreateHardLinkUnix(string sourcePath, string targetPath)
        {
            var result = NativeMethods.Link(sourcePath, targetPath);
            return result == 0;
        }

        private static class NativeMethods
        {
            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool CreateHardLink(
                string lpFileName,
                string lpExistingFileName,
                IntPtr lpSecurityAttributes);

            [DllImport("libc", SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
            [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
#pragma warning disable CA2101 // libc link() requires UTF-8 byte strings, not wide chars
            public static extern int Link(
                [MarshalAs(UnmanagedType.LPUTF8Str)] string oldpath,
                [MarshalAs(UnmanagedType.LPUTF8Str)] string newpath);
#pragma warning restore CA2101
        }
    }
}
