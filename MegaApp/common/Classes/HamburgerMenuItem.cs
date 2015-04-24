using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MegaApp.Enums;

namespace MegaApp.Classes
{
    public class HamburgerMenuItem
    {
        public HamburgerMenuItemType Type { get; set; }

        public string DisplayName { get; set; }

        public string IconPathData { get; set; }

        public int IconWidth { get; set; }

        public int IconHeight { get; set; }

        public Thickness Margin { get; set; }

        public Action TapAction { get; set; }

        public bool IsActive { get; set; }
    }
}
