using StarRezTest.DataTypes;
using StarRezTest.HTTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace StarRezTest.HTTP
{
    internal sealed class PlanetHttpClientProvider : IDisposable
    {
        private HttpClientHandler clientHandler;
        private CookieContainer cookieContainer;
        private HttpClient httpClient;

        public readonly Residence Account;

        private bool Authenticated = false;

        public PlanetHttpClientProvider(Residence residence)
        {
            Account = residence;

            clientHandler = new HttpClientHandler();
            cookieContainer = new CookieContainer();

            clientHandler.CookieContainer = cookieContainer;

            httpClient = new (clientHandler);
            httpClient.BaseAddress = new Uri(Secrets.GetProperty("planetFmUrl"));
        }

        private void Authenticate()
        {
            string username = HttpUtility.UrlEncode(Account.PlanetFMLogin.Username);
            string password = HttpUtility.UrlEncode(Account.PlanetFMLogin.Password);


            HttpRequestMessage message = new(HttpMethod.Post, "Account/Login");
            message.Content = new StringContent($"returnUrl=NonPMJobs%2FLogACall%2F&Username={username}&Password={password}",
                Encoding.UTF8,
                "application/x-www-form-urlencoded");

            message.Headers.Add("Sec-Ch-Ua", "\"Chromium\";v=\"111\", \"Not(A\"Brand\";v=\"8\"");
            message.Headers.Add("Accept", "application/json");
            message.Headers.Add("X-Requested-With", "XMLHttpRequest");
            message.Headers.Add("Sec-Ch-Ua-Mobile", "?0");
            message.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.5563.111 Safari/537.36");
            message.Headers.Add("Sec-Ch-Ua-Platform", "\"Windows\"");
            message.Headers.Add("Origin", Secrets.GetProperty("basePlanetFmUrl"));
            message.Headers.Add("Sec-Fetch-Site", "same-origin");
            message.Headers.Add("Sec-Fetch-Mode", "cors");
            message.Headers.Add("Sec-Fetch-Dest", "empty");
            message.Headers.Add("Accept-Encoding", "gzip, deflate");
            message.Headers.Add("Accept-Language", "en-GB,en-US;q=0.9,en;q=0.8");
            message.Headers.Add("Connection", "close");

            using var handler = Send(message, requiresAuthentication: false, wait: true);

            Authenticated = true;
        }

        public HttpRequestHandler Send(HttpRequestMessage message, bool requiresAuthentication = true, bool wait = true)
        {
            if (requiresAuthentication && !Authenticated) { Authenticate(); }
            return new(httpClient, message, wait: wait);
        }

        public void Dispose()
        {
            httpClient.Dispose();
            clientHandler.Dispose();
        }
    }
}
