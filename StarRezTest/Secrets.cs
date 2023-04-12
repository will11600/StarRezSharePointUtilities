using LitJson;
using StarRezTest.DataTypes;
using StarRezTest.Resources;

namespace StarRezTest
{
    public static class Secrets
    {
        private static readonly Dictionary<string, Credentials> CredentialStore = new Dictionary<string, Credentials>();
        private static readonly Dictionary<string, string> PropertyStore = new Dictionary<string, string>();

        static Secrets()
        {
            JsonData secrets = ResourceLoader.Embeddded["StarRezTest.Secrets.json"].ReadJson();
            JsonData credentials = secrets["credentials"];
            for (int index = 0; index < credentials.Count; ++index)
            {
                JsonData value = credentials[index];
                string key = (string)value["name"];
                Credentials usernamePasswordPair = new()
                {
                    Username = (string)value["username"],
                    Password = (string)value["password"]
                };
                CredentialStore.Add(key, usernamePasswordPair);
            }
            JsonData properties = secrets["properties"];
            for (int index = 0; index < properties.Count; ++index)
            {
                JsonData jsonData3 = properties[index];
                PropertyStore.Add((string)jsonData3["name"], (string)jsonData3["value"]);
            }
        }

        public static Credentials GetCredentials(string key)
        {
            Credentials credentials;
            return CredentialStore.TryGetValue(key, out credentials) ? credentials : new Credentials();
        }

        public static string GetProperty(string key)
        {
            return PropertyStore.TryGetValue(key, out string? property) ? property : string.Empty;
        }
    }
}
