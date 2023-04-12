using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarRezTest.DataTypes
{
    public partial class Residence
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public virtual string AddressLine1 => Name;
        public string AddressLine2 { get; set; } = string.Empty;
        public string AddressLine3 { get; set; } = string.Empty;
        public string City { get; set; } = "London";
        public string Postcode { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NameShort { get; set; } = string.Empty;
        public Credentials PlanetFMLogin { get; set; }
    }
}
