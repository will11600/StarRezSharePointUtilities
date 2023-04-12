using LitJson;
using StarRezTest.DataTypes;
using StarRezTest.HTTP;
using StarRezTest.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarRezTest.Models
{
    public class ReportUpdateModel : ISPListItem
    {
        public int Id { get; }
        public string Title { get; set; }
        public string CallReferenceNumber { get; set; }
        public string Status { get; set; }

        public ReportUpdateModel(Report report)
        {
            Id = report.Id;
            Title = report.Title;
            CallReferenceNumber = report.CallReferenceNumber;
            Status = report.Status;
        }

        public JsonData Serialize() => JsonMapper.ToJson(this);

        public void Upload()
        {
            ExtensionMethods.Upload(new ReportUpdateModel[] { this });
        }
    }

    public static partial class ExtensionMethods
    {
        public static void Upload(this ICollection<ReportUpdateModel> reportUpdateModels)
        {
            using (var client = new HttpClient())
            {
                HttpRequestMessage patch = new HttpRequestMessage(HttpMethod.Post, Secrets.GetProperty("reportUpdateEndpointUrl"));
                patch.Content = new StringContent(
                    JsonMapper.ToJson(reportUpdateModels),
                    Encoding.UTF8,
                    "application/json");
                using var handler = new HttpRequestHandler(client, patch);
            }
        }
    }
}
