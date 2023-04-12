using StarRezTest.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarRezTest.Models
{
    public class EmailModel
    {
        public string Subject { get; set; } = default!;
        public Student Student { get; } = default!;
        public Room Room { get; } = default!;
        public Report[] Maintainance => Reports.Where(r => r.ServiceProvider != "Reception").ToArray();
        public Report[] ConfiscatedItems => Reports.Where(r => r.Category == "Item(s) Confiscated").ToArray();
        protected List<Report> Reports { get; set; } = default!;

        public EmailModel(Student student, string subject)
        {
            Subject = subject;

            Student = student;
            Room = student.Room;

            Reports = GetReportsFromRoom();
        }

        public EmailModel(Room room, string subject)
        {
            Subject = subject;

            Room = room;
            Student = room.Student;

            Reports = GetReportsFromRoom();
        }

        protected EmailModel() { }

        protected List<Report> GetReportsFromRoom()
        {
            var reports = new List<Report>(Room.GetReports());
            var output = new List<Report>();

            foreach (var report in reports)
            {
                /*if (report.Status == "Closed" ||
                    report.Category == "Resi Projects" ||
                    string.IsNullOrWhiteSpace(report.CallReferenceNumber))
                {
                    continue;
                }*/

                output.Add(report);
            }

            return output;
        }
    }
}
