namespace Kagarr.Common.EnvironmentInfo
{
    public static class RuntimeInfo
    {
        public static bool IsProduction => !BuildInfo.IsDebug;
    }
}
