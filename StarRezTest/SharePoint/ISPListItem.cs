using LitJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarRezTest.SharePoint
{
    public interface ISPListItem
    {
        public int Id { get; }
        public string Title { get; set; }

        public JsonData Serialize();
    }
}
