using StarRezTest.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace StarRezTest.HTTP
{
    public static class SPHandler
    {
        public const string defaultRequestString = "web/lists/GetByTitle('Reports')/Items?" +
        "$select=Id,Title,CallReferenceNumber,Series,Status,Created," +
        "Category1/Title,Category1/Provider," +
        "Room/Id,Room/Title,Room/PlanetReference,Room/field_1,Room/StarRezRoomSpace" +
        "&$expand=Category1,Room";

        static CookieContainer cookies = new CookieContainer();
        static HttpClientHandler handler = new HttpClientHandler();

        static string formDigestValue = string.Empty;
        static string rtFa = string.Empty;
        static string fedAuth = string.Empty;

        static SPHandler()
        {
            if (HasRequiredCookies(out _, false)) { return; }

            cookies = new ();
            handler = new ();
            handler.CookieContainer = cookies;

            using (HttpClient client = new(handler))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Attempting to authenticate with SharePoint...\n");
                Console.ResetColor();

                string securityToken = "";
                string getSecurityTokenBody = ResourceLoader.Embeddded["StarRezTest.Templates.AuthTokenRequest.xml"].ReadAll();
                using (var handler = client.URLEncodedRequest("https://login.microsoftonline.com/extSTS.srf", HttpMethod.Get, getSecurityTokenBody))
                {
                    securityToken = handler.ExtractFromResponse(
                     "BinarySecurityToken.*?>(.*?)<",
                     "Extracting binary security token... ") ?? string.Empty;
                }

                if (string.IsNullOrEmpty(securityToken)) { return; }

                using (var handler = client.URLEncodedRequest(Secrets.GetProperty("organisationSharePointUrl") + "/_forms/default.aspx?wa=wsignin1.0", HttpMethod.Post, securityToken))
                {
                    handler.Wait();
                }

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n{cookies.Count} cookie(s) added:");
                Console.ResetColor();

                if (HasRequiredCookies(out int cookiesNeeded))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\nACCESS GRANTED\n");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Missing x{cookiesNeeded} required security cookies");
                    Console.WriteLine("\nACCESS DENIED\n");
                    Console.ResetColor();
                }

                var getReqDigest = new HttpRequestMessage(HttpMethod.Post, Secrets.GetProperty("organisationSharePointUrl") + "/_api/contextinfo");
                using (var handler = new HttpRequestHandler(client, getReqDigest))
                {
                    formDigestValue = handler.ExtractFromResponse("d:FormDigestValue>(.*?)<", "Getting Form Digest Value... ")!;
                }
                cookies.Add(new Cookie("RequestDigest", HttpUtility.UrlEncode(formDigestValue), "/", getReqDigest.RequestUri?.Host));
            }
        }

        private static bool HasRequiredCookies(out int cookiesNeeded, bool verbose = true)
        {
            cookiesNeeded = 2;

            foreach (Cookie cookie in cookies.GetAllCookies())
            {
                switch (cookie.Name)
                {
                    case "rtFa":
                        cookiesNeeded--;
                        rtFa = cookie.Value;
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;
                    case "FedAuth":
                        cookiesNeeded--;
                        fedAuth = cookie.Value;
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;
                    default:
                        Console.ResetColor();
                        break;
                }

                if (!verbose) { continue; }

                Console.WriteLine($"{cookie.Name.ToUpper()}: {cookie.Value.GetShortHash()}");
            }

            return cookiesNeeded == 0;
        }

        public static HttpClient Create(string uri = "")
        {
            if (string.IsNullOrEmpty(uri))
            {
                uri = Secrets.GetProperty("sharePointLibraryUrl") + "/_api/";
            }

            handler = new HttpClientHandler();
            handler.CookieContainer = cookies;

            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("RequestDigest", formDigestValue);
            client.BaseAddress = new Uri(uri);
            client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");
            return client;
        }
    }
}
