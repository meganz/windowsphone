using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MegaApp.Classes
{
    public enum Resolutions { WVGA, WXGA, HD };

    public static class ResolutionHelper
    {
        private static bool IsWvga
        {
            get
            {
                return  Application.Current.Host.Content.ScaleFactor == 100;
            }
        }

        private static bool IsWxga
        {
            get
            {
                return Application.Current.Host.Content.ScaleFactor == 160;
            }
        }

        private static bool IsHd
        {
            get
            {
                return Application.Current.Host.Content.ScaleFactor == 150 || Application.Current.Host.Content.ScaleFactor == 225;
            }
        }

        public static Resolutions CurrentResolution
        {
            get
            {
                if (IsWvga) return Resolutions.WVGA;
                if (IsWxga) return Resolutions.WXGA;
                if (IsHd) return Resolutions.HD;
                
                return Resolutions.HD;
                //throw new InvalidOperationException("Unknown resolution");
            }
        }
    }
}
