using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Kagarr.Common.Serialization
{
    public static class SettingsSecretRedactor
    {
        public const string Mask = "********";

        private static readonly string[] SecretKeywords = { "password", "apikey", "api_key", "secret", "token", "passphrase" };

        public static bool IsSecretField(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return false;
            }

            return SecretKeywords.Any(k => propertyName.Contains(k, StringComparison.OrdinalIgnoreCase));
        }

        // Replaces the values of secret-like fields (password, apiKey, ...) in a
        // settings JSON blob with a fixed mask so they are never returned on GET.
        public static string RedactSecrets(string settingsJson)
        {
            if (!TryParseObject(settingsJson, out var obj))
            {
                return settingsJson;
            }

            RedactObject(obj);

            return obj.ToJsonString();
        }

        // Restores stored secret values for fields an API client sent back still
        // masked, so round-tripping a redacted GET through PUT keeps credentials.
        public static string RestoreSecrets(string incomingJson, string existingJson)
        {
            if (!TryParseObject(incomingJson, out var incoming) ||
                !TryParseObject(existingJson, out var existing))
            {
                return incomingJson;
            }

            RestoreObject(incoming, existing);

            return incoming.ToJsonString();
        }

        private static bool TryParseObject(string json, out JsonObject obj)
        {
            obj = null;

            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                obj = JsonNode.Parse(json) as JsonObject;
            }
            catch (JsonException)
            {
                return false;
            }

            return obj != null;
        }

        private static void RedactObject(JsonObject obj)
        {
            foreach (var name in obj.Select(p => p.Key).ToList())
            {
                var value = obj[name];

                if (value is JsonObject child)
                {
                    RedactObject(child);
                }
                else if (IsSecretField(name) &&
                         value is JsonValue jsonValue &&
                         jsonValue.TryGetValue<string>(out var stringValue) &&
                         !string.IsNullOrEmpty(stringValue))
                {
                    obj[name] = Mask;
                }
            }
        }

        private static void RestoreObject(JsonObject incoming, JsonObject existing)
        {
            foreach (var name in incoming.Select(p => p.Key).ToList())
            {
                var value = incoming[name];

                if (value is JsonObject child)
                {
                    if (existing[name] is JsonObject existingChild)
                    {
                        RestoreObject(child, existingChild);
                    }
                }
                else if (IsSecretField(name) &&
                         value is JsonValue jsonValue &&
                         jsonValue.TryGetValue<string>(out var stringValue) &&
                         stringValue == Mask &&
                         existing[name] is JsonValue existingValue &&
                         existingValue.TryGetValue<string>(out var storedValue))
                {
                    incoming[name] = storedValue;
                }
            }
        }
    }
}
