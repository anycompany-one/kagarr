using FluentAssertions;
using Kagarr.Core.Download.Clients.QBittorrent;
using NUnit.Framework;

namespace Kagarr.Core.Test.Download
{
    [TestFixture]
    public class QBittorrentClientTests
    {
        private const string HexHash = "d2474e86c95b19b8bcfdb92bc12c9d44667cfa36";

        [Test]
        public void TryGetInfoHash_should_extract_hex_hash_from_magnet_link()
        {
            var magnet = $"magnet:?xt=urn:btih:{HexHash}&dn=Test+Game";

            QBittorrentClient.TryGetInfoHash(magnet).Should().Be(HexHash);
        }

        [Test]
        public void TryGetInfoHash_should_normalize_uppercase_hex_hash_to_lowercase()
        {
            var magnet = $"magnet:?xt=urn:btih:{HexHash.ToUpperInvariant()}&dn=Test+Game";

            QBittorrentClient.TryGetInfoHash(magnet).Should().Be(HexHash);
        }

        [Test]
        public void TryGetInfoHash_should_convert_base32_hash_to_lowercase_hex()
        {
            // 2JDU5BWJLMM3RPH5XEV4CLE5IRTHZ6RW is the base32 form of d2474e86c95b19b8bcfdb92bc12c9d44667cfa36
            var magnet = "magnet:?xt=urn:btih:2JDU5BWJLMM3RPH5XEV4CLE5IRTHZ6RW&dn=Test+Game";

            QBittorrentClient.TryGetInfoHash(magnet).Should().Be(HexHash);
        }

        [Test]
        public void TryGetInfoHash_should_extract_hash_from_torrent_url()
        {
            var url = $"https://tracker.example.com/torrents/{HexHash}.torrent";

            QBittorrentClient.TryGetInfoHash(url).Should().Be(HexHash);
        }

        [Test]
        public void TryGetInfoHash_should_return_null_when_hash_cannot_be_determined()
        {
            QBittorrentClient.TryGetInfoHash("https://tracker.example.com/download?id=12345").Should().BeNull();
        }

        [Test]
        public void TryGetInfoHash_should_return_null_for_empty_url()
        {
            QBittorrentClient.TryGetInfoHash(null).Should().BeNull();
            QBittorrentClient.TryGetInfoHash(string.Empty).Should().BeNull();
        }
    }
}
