using System;
using System.Reflection;

namespace Kagarr.Common.EnvironmentInfo
{
    public static class BuildInfo
    {
        static BuildInfo()
        {
            var assembly = Assembly.GetExecutingAssembly();
            Version = assembly.GetName().Version;
            Release = Version.Build == 0 ? "main" : "develop";
        }

        public static Version Version { get; }
        public static string Release { get; }
        public static bool IsDebug => BuildInfo.Release == "develop";
    }
}
