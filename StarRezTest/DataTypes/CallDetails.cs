using StarRezTest.HTML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarRezTest.DataTypes
{
    public class CallDetails
    {
        public string ID { get; set; } = default!;

        [HTMLDOMObject("jtpName_input")]
        public string Category { get; set; } = default!;
        [HTMLDOMObject("subName_input")]
        public string SubCategory { get; set; } = default!;
        [HTMLDOMObject("Call_npmJobDesc")]
        public string CallDescription { get; set; } = default!;
        [HTMLDOMObject("Call_npmJobDetails")]
        public string CallDetail { get; set; } = default!;
        [HTMLDOMObject("Call_locLevel2")]
        public int Building { get; set; }
        [HTMLDOMObject("Call_locLevel3")]
        public int Floor { get; set; }
        [HTMLDOMObject("Call_locLevel4")]
        public string Room { get; set; } = default!;
        [HTMLDOMObject("perCode_input")]
        public string Person { get; set; } = default!;
        [HTMLDOMObject("slaCode_input")]
        public string SLACode { get; set; } = default!;
        [HTMLDOMObject("FriendlyStatusDisplayed")]
        public string Status { get; set; } = default!;
        [HTMLDOMObject("Call_npmProgressNotes")]
        public string ProgressNotes { get; set; } = default!;
    }
}
