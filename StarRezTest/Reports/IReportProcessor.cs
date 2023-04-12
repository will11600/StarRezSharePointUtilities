using StarRezTest.DataTypes;
using StarRezTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarRezTest.Reports
{
    public interface IReportProcessor
    {
        public CancellationToken CancellationToken { get; }

        public void Enqueue(Report report);
        public void Enqueue(ICollection<Report> reports);
        public void BeginProcessing();
        public Task<ICollection<ReportUpdateModel>> Task { get; }
        public int Count { get; }
    }
}
