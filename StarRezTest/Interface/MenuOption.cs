using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarRezTest.Interface
{
    public class MenuOption
    {
        public string Name { get; set; } = default!;
        public int Order { get; set; }
        public Action OnSelect { get; set; } = default!;

        public static MenuOption[] MenuOptions =
        {
            new MenuOption
            {
                Name = "Log unreported jobs (Artic)",
                Order = 1,
                OnSelect = MainMenu.LogJobs
            },
            new MenuOption
            {
                Name = "Get Room Info (StarRez x SharePoint)",
                Order = 2,
                OnSelect = MainMenu.GetReports
            },
            new MenuOption
            {
                Name = "Preview Default Table Entry (StarRez)",
                Order = 7,
                OnSelect = MainMenu.PreviewDefaultObject
            },
            new MenuOption
            {
                Name = "Log Job - Planet (EXPERIMENTAL)",
                Order = 8,
                OnSelect = MainMenu.LogJobTest
            }
        };
    }
}
