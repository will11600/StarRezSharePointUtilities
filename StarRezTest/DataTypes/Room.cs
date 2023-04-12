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
    public class Room : ISPListItem
    {
        public int Id { get; }
        public string Title { get; set; }
        public string PlanetReference { get; }
        public string StarRezRoomSpace { get; }
        public Residence Location { get; } = default!;

        [JsonIgnore]
        public Student Student => Student.FindByRoom(StarRezRoomSpace);

        public JsonData Serialize() => JsonMapper.ToJson(this);

        public Room(JsonData roomData)
        {
            Title = roomData["Title"]?.ToString() ?? "";
            StarRezRoomSpace = roomData["StarRezRoomSpace"]?.ToString() ?? "";
            Id = (int)roomData["Id"];
            PlanetReference = roomData["PlanetReference"]?.ToString() ?? "";

            string residence = roomData["field_1"]?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(residence)) { return; }
            Location = Residence.AllSites.Single(r => r.Name == residence);
        }

        public ICollection<Report> GetReports()
        {
            using (var client = SPHandler.Create())
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get,
                    SPHandler.defaultRequestString
                    + $"&$filter=Room/Id eq '{Id}'");
                using (var handler = new HttpRequestHandler(client, request))
                {
                    var results = handler.ResponseJson["d"]["results"];
                    var reports = new Report[results.Count];
                    for (int i = 0; i < results.Count; i++)
                    {
                        reports[i] = new Report(results[i]);
                    }
                    return reports;
                }
            }
        }
    }
}
