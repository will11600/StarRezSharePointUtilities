using LitJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarRezTest.HTTP
{
    public static class ExtensionMethods
    {
        public static string GetShortHash(this string text)
        {
            return String.Format("{0:X}", text.GetHashCode());
        }

        public static HttpRequestHandler URLEncodedRequest(this HttpClient client, Uri uri, HttpMethod method, string? content = null)
        {
            var request = new HttpRequestMessage(method, uri);
            if (content != null)
            {
                request.Content = new StringContent(content, Encoding.UTF8, "application/x-www-form-urlencoded");
            }

            return new HttpRequestHandler(client, request);
        }

        public static HttpRequestHandler URLEncodedRequest(this HttpClient client, string uri, HttpMethod method, string? content = null)
        {
            return URLEncodedRequest(client, new Uri(uri), method, content);
        }

        public static async Task<JsonData> ToJsonAsync(this HttpContent content)
        {
            return JsonMapper.ToObject(await content.ReadAsStringAsync());
        }

        public static HttpRequestHandler CreateHandler(this HttpClient httpClient, HttpRequestMessage httpMessage, bool disposeOfClient = false)
        {
            return new HttpRequestHandler(httpClient, httpMessage, disposeOfClient);
        }
    }
}
