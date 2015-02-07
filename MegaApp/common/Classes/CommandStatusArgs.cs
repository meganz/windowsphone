using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegaApp.Classes
{
    public class CommandStatusArgs: EventArgs
    {
        public bool Status { get; set; }

        public CommandStatusArgs(bool status)
        {
            this.Status = status;
        }
    }
}
