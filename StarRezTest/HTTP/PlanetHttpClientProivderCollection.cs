using StarRezTest.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarRezTest.HTTP
{
    internal class PlanetHttpClientProivderCollection : IDisposable
    {
        private Dictionary<string, PlanetHttpClientProvider> providers = new Dictionary<string, PlanetHttpClientProvider>();

        public HttpRequestHandler Send(HttpRequestMessage message, Residence residence, bool wait = true)
        {
            string username = residence.PlanetFMLogin.Username;
            if (!providers.ContainsKey(username))
            {
                providers.Add(username, new(residence));
            }

            return providers[username].Send(message, wait: wait);
        }

        public void Dispose()
        {
            foreach (var provider in providers.Values) { provider.Dispose(); }
        }
    }
}
