using LitJson;
using StarRezTest.HTTP;
using StarRezTest.StarRez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarRezTest.DataTypes
{
    public class Student : StarRezObj
    {
        public int StudentNumber { get; set; }
        public string NameFirst { get; set; } = default!;
        public string NameLast { get; set; } = default!;
        public string NamePreferred { get; set; } = default!;
        public string Email { get; set; } = default!;
        public int EntryStatus { get; set; }
        public Room Room { get; set; } = default!;

        public Student(JsonData data) : base()
        {
            StudentNumber = int.Parse((string)data["ID1"]);
            NameFirst = (string)data["NameFirst"];
            NameLast = (string)data["NameLast"];
            NamePreferred = (string)data["NamePreferred"];
            EntryStatus = (int)data["EntryStatusEnum"];
            Email = (string)data["Email"];
            Room = GetRoom((string)data["Description"]);
        }

        protected Room GetRoom(string roomSpaceDescription)
        {
            HttpRequestMessage request = new(HttpMethod.Get, "web/lists/GetByTitle('Rooms')/Items?" +
                    "$select=Id,Title,PlanetReference,field_1,StarRezRoomSpace" +
                    $"&$filter=StarRezRoomSpace eq '{roomSpaceDescription}'");
            using (var handler = new HttpRequestHandler(SPHandler.Create(), request, true))
            {
                return new Room(handler.ResponseJson["d"]["results"][0]);
            }
        }

        public static Student FindByName(string name)
        {
            var data = Search(FilterQuery.Name.String(name));
            return new Student(data[0]);
        }

        public static Student FindByRoom(string roomSpace)
        {
            var data = Search(FilterQuery.RoomSpace.String(roomSpace));
            return new Student(data[0]);
        }

        public static Student FindByRoom(Room room) => FindByRoom(room.StarRezRoomSpace);
    }
}
