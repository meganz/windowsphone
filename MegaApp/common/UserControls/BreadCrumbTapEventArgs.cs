using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegaApp.UserControls
{
    public class BreadCrumbTapEventArgs: EventArgs
    {
        public string Text { get; set; }
        public object Value { get; set; }
        public object Item { get; set; }
    }
}
