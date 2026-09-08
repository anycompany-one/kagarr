using System.Text.Json;
using FluentAssertions;
using Kagarr.Api.V1.Indexers;
using Kagarr.Common.Serialization;
using Kagarr.Core.Indexers;
using NUnit.Framework;

namespace Kagarr.Host.Test.Security
{
    [TestFixture]
    public class SettingsSecretRedactorTests
    {
        [Test]
        public void should_redact_password_and_api_key_values()
        {
            var json = "{\"host\":\"localhost\",\"password\":\"hunter2\",\"apiKey\":\"abc123\"}";

            var redacted = SettingsSecretRedactor.RedactSecrets(json);

            using var doc = JsonDocument.Parse(redacted);
            doc.RootElement.GetProperty("host").GetString().Should().Be("localhost");
            doc.RootElement.GetProperty("password").GetString().Should().Be(SettingsSecretRedactor.Mask);
            doc.RootElement.GetProperty("apiKey").GetString().Should().Be(SettingsSecretRedactor.Mask);
        }

        [Test]
        public void should_redact_nested_secrets()
        {
            var json = "{\"auth\":{\"username\":\"admin\",\"password\":\"hunter2\"}}";

            var redacted = SettingsSecretRedactor.RedactSecrets(json);

            using var doc = JsonDocument.Parse(redacted);
            var auth = doc.RootElement.GetProperty("auth");
            auth.GetProperty("username").GetString().Should().Be("admin");
            auth.GetProperty("password").GetString().Should().Be(SettingsSecretRedactor.Mask);
        }

        [Test]
        public void should_not_redact_empty_secret_values()
        {
            var json = "{\"password\":\"\"}";

            var redacted = SettingsSecretRedactor.RedactSecrets(json);

            using var doc = JsonDocument.Parse(redacted);
            doc.RootElement.GetProperty("password").GetString().Should().BeEmpty();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("not json")]
        [TestCase("[1,2,3]")]
        public void should_pass_through_non_object_settings(string json)
        {
            SettingsSecretRedactor.RedactSecrets(json).Should().Be(json);
        }

        [Test]
        public void should_restore_masked_secrets_from_existing_settings()
        {
            var stored = "{\"host\":\"localhost\",\"password\":\"hunter2\"}";
            var incoming = "{\"host\":\"newhost\",\"password\":\"" + SettingsSecretRedactor.Mask + "\"}";

            var restored = SettingsSecretRedactor.RestoreSecrets(incoming, stored);

            using var doc = JsonDocument.Parse(restored);
            doc.RootElement.GetProperty("host").GetString().Should().Be("newhost");
            doc.RootElement.GetProperty("password").GetString().Should().Be("hunter2");
        }

        [Test]
        public void should_keep_changed_secret_values_on_restore()
        {
            var stored = "{\"password\":\"hunter2\"}";
            var incoming = "{\"password\":\"new-password\"}";

            var restored = SettingsSecretRedactor.RestoreSecrets(incoming, stored);

            using var doc = JsonDocument.Parse(restored);
            doc.RootElement.GetProperty("password").GetString().Should().Be("new-password");
        }

        [Test]
        public void should_restore_nested_masked_secrets()
        {
            var stored = "{\"auth\":{\"password\":\"hunter2\"}}";
            var incoming = "{\"auth\":{\"password\":\"" + SettingsSecretRedactor.Mask + "\"}}";

            var restored = SettingsSecretRedactor.RestoreSecrets(incoming, stored);

            using var doc = JsonDocument.Parse(restored);
            doc.RootElement.GetProperty("auth").GetProperty("password").GetString().Should().Be("hunter2");
        }

        [Test]
        public void indexer_resource_should_redact_settings_on_from_model()
        {
            var model = new IndexerDefinition
            {
                Id = 1,
                Name = "Test",
                Implementation = "Torznab",
                Settings = "{\"baseUrl\":\"http://localhost\",\"apiKey\":\"abc123\"}"
            };

            var resource = IndexerResource.FromModel(model);

            resource.Settings.Should().NotContain("abc123");
            resource.Settings.Should().Contain(SettingsSecretRedactor.Mask);
        }
    }
}
