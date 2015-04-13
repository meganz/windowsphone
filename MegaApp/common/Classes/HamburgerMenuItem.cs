using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegaApp.Classes
{
    public class HamburgerMenuItem
    {
        public string DisplayName { get; set; }

        public string IconPathData { get; set; }

        public Action TapAction { get; set; }
    }
}
