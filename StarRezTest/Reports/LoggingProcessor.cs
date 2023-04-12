using LitJson;
using StarRezTest.Bots;
using StarRezTest.DataTypes;
using StarRezTest.HTTP;
using StarRezTest.Interface;
using StarRezTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StarRezTest.Reports
{
    public class LoggingProcessor : IReportProcessor, IDisposable
    {
        internal delegate void ReportProcessorDelegate();

        protected Queue<Report> queue;
        protected CancellationToken token;
        protected TaskCompletionSource<ICollection<ReportUpdateModel>> process;

        private event ReportProcessorDelegate OnReportEnqueued;
        private PlanetFM fm;

        public LoggingProcessor()
        {
            queue = new Queue<Report>();
            token = new CancellationToken();
            process = new TaskCompletionSource<ICollection<ReportUpdateModel>>();

            fm = new PlanetFM();

            OnReportEnqueued += ReportProcessor_OnReportEnqueued;
            token.Register(() => process.TrySetCanceled(token));
        }

        public LoggingProcessor(ICollection<Report> reports1) : this()
        {
            queue = new Queue<Report>(reports1);
        }

        private void ReportProcessor_OnReportEnqueued()
        {
            if (process.Task.IsCompleted || process.Task.IsCanceled)
            {
                process = new TaskCompletionSource<ICollection<ReportUpdateModel>>();
                BeginProcessing();
            }
        }

        public CancellationToken CancellationToken => token;

        public Task<ICollection<ReportUpdateModel>> Task => process.Task;

        public int Count => queue.Count();

        public void BeginProcessing()
        {
            var output = new List<ReportUpdateModel>();

            if (queue.Count() < 1)
            {
                process.SetResult(output);
                return;
            }

            int i = 0;

            while (TryGetReportFromQueue(out var report) && !CancellationToken.IsCancellationRequested)
            {
                Console.Write($"{i++:D2} ");
                fm.OperatingOn = report!;
                ProcessReport();
                output.Add(new ReportUpdateModel(fm.OperatingOn));
            }

            CLI.ColourPrint("\nUploading updated records to SharePoint...", ConsoleColor.Yellow, true);
            output.Upload();

            process.SetResult(output);
            return;
        }

        protected void ProcessReport()
        {
            CLI.ColourPrint($"[0x{fm.OperatingOn.Id:X4}/0x{fm.OperatingOn.Room.Id:X4}] ", ConsoleColor.Blue, false);

            var pos = Console.GetCursorPosition();
            Console.Write($"{(fm.OperatingOn.Title.Length < 29 ? fm.OperatingOn.Title : $"{fm.OperatingOn.Title[..29].Trim()}...")} ");
            Console.SetCursorPosition(52, pos.Top);

            try
            {
                Thread.Sleep(200);
                fm.ClickNextPageButton();
                Thread.Sleep(200);
                if (!fm.SetCategory()) { throw new InvalidOperationException(); }
                Thread.Sleep(200);
                fm.CompleteForm(fm.PersonalDetails);
                Thread.Sleep(200);
                fm.CompleteForm(fm.Location);
                Thread.Sleep(200);
                fm.CompleteForm(fm.CallDetails);
                Thread.Sleep(200);
                fm.Submit();
                Thread.Sleep(4000);

                fm.OperatingOn.CallReferenceNumber = Regex.Match(fm.Driver.Title, "[0-9]{5}").Value;

                Console.WriteLine($"{fm.OperatingOn.CallReferenceNumber} {fm.OperatingOn.Status.ToUpper()}");
            }
            catch (InvalidOperationException) { CLI.ColourPrint("FAIL", ConsoleColor.Red); }
        }

        protected bool TryGetReportFromQueue(out Report? report)
        {
            report = null;

            if (queue.Count() < 1)
            {
                return false;
            }

            report = queue.Dequeue();
            return true;
        }

        public void Dispose()
        {
            process.Task.ContinueWith(t =>
            {
                fm.Dispose();
                OnReportEnqueued -= ReportProcessor_OnReportEnqueued;
            });
        }

        public void Enqueue(Report report)
        {
            queue.Enqueue(report);
            OnReportEnqueued?.Invoke();
        }

        public void Enqueue(ICollection<Report> reports)
        {
            foreach(Report report in reports)
            {
                queue.Enqueue(report);
            }
            OnReportEnqueued?.Invoke();
        }
    }
}
