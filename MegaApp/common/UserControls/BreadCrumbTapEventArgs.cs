using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MegaApp.Interfaces;

namespace MegaApp.UserControls
{
    public class BreadCrumbTapEventArgs: EventArgs
    {
        public IBaseNode Item { get; set; }
    }
}
