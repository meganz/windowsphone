using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Windows.Devices.Geolocation;
using Telerik.Windows.Controls;

namespace MegaApp.Extensions
{
    public static class ControlExtensions
    {
        public static void TabToNextControl(this Control control, Panel parentContainer, Page parentPage)
        {
            // First hide the virtual keyboard
            parentPage.Focus(); 

            // Add 1 to calculate the next tab index
            int nextTabIndex = control.TabIndex + 1;

            // Check the controls in the parentcontainer for the next tab index and focus that control
            foreach (var c in parentContainer.ChildrenOfType<Control>())
            {
                if (c.TabIndex.Equals(nextTabIndex))
                {
                    c.Focus();
                    break;
                }
            }
           
        }
    }
}
