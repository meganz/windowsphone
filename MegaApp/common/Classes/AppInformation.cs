using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegaApp.Classes
{
    public class AppInformation
    {
        public AppInformation()
        {
            this.PickerOrAsyncDialogIsOpen = false;

        }
        public bool PickerOrAsyncDialogIsOpen { get; set; }
    }
}
