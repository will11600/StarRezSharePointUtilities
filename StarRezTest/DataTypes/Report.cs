using LitJson;
using StarRezTest.HTTP;
using StarRezTest.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarRezTest.DataTypes
{
    public partial class Report : ISPListItem
    {
        public int Id { get; }
        public string Title { get; set; }
        public string CallReferenceNumber;
        public string Category;
        public string ServiceProvider;
        public string Series;
        public string Status;
        [JsonIgnore]
        public DateTime Created;

        [JsonIgnore]
        public Room Room { get; set; }
        public int RoomId => Room.Id;

        public Report(JsonData data)
        {
            Id = (int)data["Id"];
            Title = (string)data["Title"];
            CallReferenceNumber = data["CallReferenceNumber"]?.ToString() ?? "";
            Category = data["Category1"]["Title"]?.ToString() ?? "";
            ServiceProvider = data["Category1"]["Provider"]?.ToString() ?? "";
            Series = data["Series"]?.ToString() ?? "";
            Status = data["Status"]?.ToString() ?? "";
            Room = new Room(data["Room"]);
            Created = DateTime.Parse((string)data["Created"]);
        }

        public static string GetJobStatus(string planetStatus, DateTime? resolvedDate)
        {
            if (planetStatus == "Closed") { return planetStatus; }
            if (resolvedDate == null)
            {
                switch (planetStatus)
                {
                    case "Active":
                        return "In Progress";
                    case "Incomplete":
                        return "Pending";
                    default:
                        return "N/A";
                }
            }
            else if (resolvedDate < DateTime.Now)
            {
                return "Closed";
            }
            return "N/A";
        }

        public JsonData Serialize() => JsonMapper.ToJson(this);

        public static Report[] GetAll(JsonData data)
        {
            if (data == null) { return Array.Empty<Report>(); }

            var results = data["d"]["results"];
            var reports = new Report[results.Count];
            for (int i = 0; i < results.Count; i++)
            {
                reports[i] = new Report(results[i]);
            }
            return reports;
        }
    }
}
