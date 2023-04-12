using LitJson;
using StarRezTest.HTML;
using StarRezTest.Interface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace StarRezTest.HTTP
{
    public class HttpRequestHandler : IDisposable
    {
        private HttpRequestMessage request;
        private Task<HttpResponseMessage> response;
        private HttpClient client;
        private bool disposeOfHttpClient;

        public async Task<string> ResponseTextAsync() => await (await response).Content.ReadAsStringAsync();
        public string ResponseText => ResponseTextAsync().Result;

        public async Task<JsonData> ResponseJsonAsync() => await (await response).Content.ToJsonAsync();
        public JsonData ResponseJson => ResponseJsonAsync().Result;

        public async Task<HTMLData> GetResponseHTMLAsync() => new(await ResponseTextAsync());
        public HTMLData ResponseHTML => GetResponseHTMLAsync().Result;

        public HttpContent ResponseContent => response.Result.Content;

        public HttpRequestHandler(HttpClient httpClient, HttpRequestMessage httpRequestMessage, bool disposeOfHttpClient = false, bool wait = true)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;

            client = httpClient;
            request = httpRequestMessage;
            this.disposeOfHttpClient = disposeOfHttpClient;

            Uri absoluteUri = new ($"{client.BaseAddress?.AbsoluteUri}{request.RequestUri?.ToString()}");
            string host = absoluteUri.Host;

            Console.Write($"Sending a {request.Method.Method.ToUpper()} request to {host}... ");
            var firstMessagePosition = Console.GetCursorPosition();
            Console.WriteLine();

            response = client.SendAsync(httpRequestMessage);
            response.ContinueWith(task =>
            {
                (int Left, int Top) lastLinePosition;
                lock (Console.Out)
                {
                    lastLinePosition = Console.GetCursorPosition();
                    Console.SetCursorPosition(firstMessagePosition.Left, firstMessagePosition.Top);
                }

                if (!task.IsCompletedSuccessfully)
                {
                    lock (Console.Out)
                    {
                        if (task.Exception != null)
                        {
                            foreach (Exception exception in task.Exception.InnerExceptions)
                            {
                                Debug.WriteLine($"{exception.GetType().ToString().ToUpper()}: {exception.Message}" +
                                    (exception.StackTrace == null ? "" : $"\n{exception.StackTrace}"));
                            }
                            CLI.ColourPrint($"{task.Exception.ToString().ToUpper()}: {task.Exception.Message}", ConsoleColor.DarkRed);
                        }
                        else { CLI.ColourPrint("FAIL", ConsoleColor.DarkRed); }
                        Console.SetCursorPosition(lastLinePosition.Left, lastLinePosition.Top);
                    }
                    return;
                }

                var result = task.Result;
                if (result == null) { return; }

                lock (Console.Out)
                {
                    if (result.IsSuccessStatusCode) { Console.ForegroundColor = ConsoleColor.DarkGreen; }
                    else { Console.ForegroundColor = ConsoleColor.DarkRed; }
                    Console.WriteLine($"{(int)result.StatusCode:D2} {result.StatusCode.ToString().ToUpper()}");

                    Console.ResetColor();
                    Console.SetCursorPosition(lastLinePosition.Left, lastLinePosition.Top);
                }
            });
            if (wait) { Wait(); }
        }

        public void Wait() => response.Wait();
        public async Task WaitAsync() => await response;

        public string? ExtractFromResponse(string matchPattern, string consoleMessage = "")
        {
            Console.Write(consoleMessage);

            if (ResponseText == null)
            {
                CLI.ColourPrint("FAIL", ConsoleColor.Red);
                return null;
            }

            Match match = Regex.Match(ResponseText, matchPattern);
            if (match.Success)
            {
                string output = HttpUtility.HtmlDecode(match.Groups[1].Value);

                CLI.ColourPrint(output.GetShortHash(), ConsoleColor.Green);

                return output;
            }

            CLI.ColourPrint("FAIL", ConsoleColor.Red);
            return null;
        }

        public void Dispose()
        {
            request.Dispose();
            response?.Dispose();
            if (disposeOfHttpClient) { client.Dispose(); }
        }
    }
}
