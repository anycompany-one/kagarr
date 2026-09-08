namespace Kagarr.Core.Http
{
    /// <summary>
    /// Named HttpClient registrations that need special handler configuration.
    /// </summary>
    public static class HttpClientNames
    {
        /// <summary>
        /// A client whose primary handler has cookies disabled. Used by clients that
        /// manage session cookies manually (e.g. qBittorrent's SID), because the
        /// factory's shared handlers must not accumulate cookies across consumers.
        /// </summary>
        public const string NoCookies = "no-cookies";
    }
}
