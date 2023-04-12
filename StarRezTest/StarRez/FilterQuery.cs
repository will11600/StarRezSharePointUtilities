using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarRezTest.StarRez
{
    internal class FilterQuery
    {
        protected Func<string, string> Filter { get; set; }

        public string String(string filterValue)
        {
            return Filter(filterValue);
        }

        public FilterQuery(Func<string, string> func)
        {
            Filter = func;
        }

        public static FilterQuery RoomSpace = new (r => $"WHERE RoomSpace.Description LIKE '{r}'");
        public static FilterQuery Name = new(n => $"WHERE NameFirst LIKE '{n}'");
    }
}
