using LitJson;
using StarRezTest.DataTypes;
using StarRezTest.HTTP;
using StarRezTest.Models;
using StarRezTest.Reports;
using StarRezTest.StarRez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace StarRezTest.Interface
{
    public static class MainMenu
    {
        public static void StarRezQuery()
        {
            string query = ReadConsoleInputString();
            Console.WriteLine(StarRezObj.StarRezQuery(query, false, out JsonData jsonData));
            if (YesNoPrompt("Save output to file?"))
            {
                Console.Write("Path: ");
                string path = ReadConsoleInputString();
                File.WriteAllText(path, JsonMapper.ToJson(jsonData));
            }
        }

        private static string ReadConsoleInputString()
        {
            string query;
            do { query = Console.ReadLine() ?? string.Empty; }
            while (string.IsNullOrWhiteSpace(query));
            return query;
        }
        public static void PreviewDefaultObject()
        {
            string query = ReadConsoleInputString();

            Console.WriteLine(StarRezObj.GetDefaultPreview(query));
        }
        private static bool YesNoPrompt(string promptText)
        {
            CLI.ColourPrint($"\n{promptText} (Y/N) ", ConsoleColor.Yellow, false);
            var pos = Console.GetCursorPosition();
            while (true)
            {
                Console.SetCursorPosition(pos.Left, pos.Top);
                var key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.Y:
                        return true;
                    case ConsoleKey.N:
                        return false;
                    default:
                        Console.Beep();
                        break;
                }
            }
            Console.WriteLine($"\n");
        }

        public static void GetReports()
        {
            Console.Write("RoomSpace: ");
            var student = Student.FindByRoom(Console.ReadLine());
            var sb = new StringBuilder();
            sb.AppendLine($"STUDENT {student.StudentNumber}");
            sb.AppendLine($"{student.NameLast}, {student.NameFirst} ({student.NamePreferred})");
            sb.AppendLine(student.Email + "\n");
            sb.AppendLine($"ROOM 0x{student.Room.Id:X4}/{student.Room.Title}");
            sb.AppendLine(student.Room.Location.Name);
            var reports = student.Room.GetReports();
            sb.AppendLine($"\nMAINTAINANCE REPORTS: {reports.Count}");
            foreach (var report in reports)
            {
                sb.AppendLine($"[0x{report.Id:X4}] {report.Created.ToShortDateString()} {report.Title}");
            }
            CLI.ColourPrint("\nOperation completed successfully!\n", ConsoleColor.Yellow);
            CLI.HorizonalLine('-', "▼ OUTPUT ▼");
            Console.WriteLine("\n" + sb.ToString());
        }

        public static void LogJobs()
        {
            using (var client = SPHandler.Create("https://liveuclac-my.sharepoint.com/personal/uczbwcb_ucl_ac_uk/_api/"))
            {
                CLI.ColourPrint("\nSearching for reports with missing reference numbers...", ConsoleColor.Yellow);
                using (var processor = new LoggingProcessor())
                {
                    foreach (var residence in Residence.AllSites)
                    {
                        CLI.ColourPrint($"Fetching reports for {residence.Name}...", ConsoleColor.Yellow);
                        processor.Enqueue(GetReports(client, residence));
                    }

                    CLI.ColourPrint($"Processing {processor.Count} report(s):", ConsoleColor.Yellow);

                    processor.BeginProcessing();

                    Task.WaitAll(processor.Task);
                }
            }
        }

        private static Report[] GetReports(HttpClient client, Residence residence)
        {
            StringBuilder sb = new StringBuilder(SPHandler.defaultRequestString +
                "&$filter=CallReferenceNumber eq null and Category1/Provider eq 'Artic'");

            if (!string.IsNullOrWhiteSpace(residence.Name))
            {
                sb.Append($" and Room/field_1 eq '{residence.Name}'");
            }

            var assignJobNumbs = new HttpRequestMessage(HttpMethod.Get, sb.ToString());
            using var handler = new HttpRequestHandler(client, assignJobNumbs);

            return Report.GetAll(handler.ResponseJson);
        }

        public static void LogJobTest()
        {
            HttpRequestMessage message = new (
                HttpMethod.Get,
                SPHandler.defaultRequestString +
                "&$filter=CallReferenceNumber ne null and Category1/Provider eq 'Artic' and Status ne 'Closed'" +
                "&$orderby=ResolveBy asc");

            Report[] reports;
            using (var handler = new HttpRequestHandler(SPHandler.Create(), message, true))
            {
                reports = Report.GetAll(handler.ResponseJson);
            }

            CLI.ColourPrint("\nUpdating Jobs...\n", ConsoleColor.Yellow);

            using PlanetREST planet = new();

            List<Task<CallDetails>> tasks = new();
            List<ReportUpdateModel> uploads = new();

            Task GetResults = Task.Run(async () =>
            {
                for (int i = 0; i < reports.Length; i++)
                {
                    while (tasks.Count < 1) { await Task.Delay(100); }

                    var task = await Task.WhenAny(tasks);
                    lock (tasks) { tasks.Remove(task); }

                    Report report = reports.Single(r => r.CallReferenceNumber == task.Result.ID);

                    string status = task.Result.Status switch
                    {
                        "Active" => "In Progress",
                        "Incomplete" => "Pending",
                        "Closed" => "Closed",
                        _ => report.Status,
                    };

                    if (status != report.Status)
                    {
                        report.Status = status;
                        uploads.Add(new ReportUpdateModel(report));
                    }
                }
            });

            for (int i = 0; i < reports.Length; i++)
            {
                Report report = reports[i];

                lock (Console.Out)
                {
                    CLI.WaitFor("Waiting... (concurrent request cap)", () => tasks.Count > 5);
                    Console.Write($"{i:D2} ");
                    CLI.ColourPrint($"[0x{report.Id:X4}] ", ConsoleColor.Blue, false);
                    Console.WriteLine($"{report.CallReferenceNumber}: {report.Title}");
                }
                tasks.Add(planet.GetCallDetailsAsync(reports[i]));

                lock (Console.Out)
                {
                    CLI.WaitFor("Waiting... (rate limit)", new TimeSpan(0, 0, 0, 0, 1000)); // Due to rate limiting
                }
            }

            try { GetResults.Wait(); }
            catch (AggregateException ex)
            {
                foreach (Exception exception in ex.InnerExceptions)
                {
                    CLI.ColourPrint($"{exception.GetType().ToString().ToUpper()}: {exception.Message}" +
                        (exception.StackTrace == null ? "" : $"\n{exception.StackTrace}"), ConsoleColor.Red);
                }
            }
            finally
            {
                if (uploads.Count > 0) { uploads.Upload(); }
            }
        }
    }
}
