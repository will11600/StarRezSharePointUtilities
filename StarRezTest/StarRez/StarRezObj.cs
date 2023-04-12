using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LitJson;
using StarRezTest.DataTypes;
using StarRezTest.HTTP;

namespace StarRezTest.StarRez
{
    public class StarRezObj : BasicAuthenticationAPI
    {
        protected static string selectStatement = "SELECT ID1, NameFirst, NameLast, NamePreferred, RoomSpace.Description, Booking.EntryStatusEnum, EntryAddress.Email FROM Entry " +
            "INNER JOIN Booking ON Entry.BookingID=Booking.BookingID " +
            "INNER JOIN RoomSpace ON Booking.RoomSpaceID=RoomSpace.RoomSpaceID " +
            "INNER JOIN EntryAddress ON EntryAddress.EntryID=Entry.EntryID " +
            "WHERE EntryAddress.AddressTypeID = 4 " +
            "AND Booking.EntryStatusEnum = 5";

        protected static JsonData Search(string filter)
        {
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, "StarRezREST/services/query");
            message.Content = new StringContent(string.Join(' ', selectStatement, filter));

            System.Diagnostics.Debug.WriteLine(message.Content.ReadAsStringAsync().Result);
            using var client = CreateClient();
            using (var handler = new HttpRequestHandler(client, message))
            {
                return handler.ResponseJson;
            }
        }

        private static string FormatJson(Match m)
        {
            var matchString = m.ToString();

            if (Regex.IsMatch(matchString, @"[}\]]")) { return "\n" + matchString; }
            else { return matchString + "\n"; }
        }

        private static string FormatJsonString(string jsonText) => Regex.Replace(jsonText, @"[{}[\],]", FormatJson);

        public static string StarRezQuery(string query, bool defaultSelect, out JsonData jsonData)
        {
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, "StarRezREST/services/query");
            message.Content = new StringContent(defaultSelect ? string.Join(" AND ", selectStatement, query) : query);

            using var client = CreateClient();
            using (var handler = new HttpRequestHandler(client, message))
            {
                jsonData = handler.ResponseJson;
                return FormatJsonString(handler.ResponseText);
            }
        }

        protected static JsonData CreateDefault(HttpClient client, string tableName)
        {
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, $"/StarRezREST/services/createdefault/{tableName}");

            using (var handler = new HttpRequestHandler(client, message))
            {
                return handler.ResponseJson;
            }
        }

        public static string GetDefaultPreview(string tableName)
        {
            using (var client = CreateClient())
            {
                return JsonMapper.ToJson(CreateDefault(client, tableName));
            }
        }

        protected static HttpClient CreateClient()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(Secrets.GetProperty("starRezUrl"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", GetAuthString());
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            return client;
        }
    }
}
