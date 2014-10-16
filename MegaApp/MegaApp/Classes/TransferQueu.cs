using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MegaApp.Enums;
using MegaApp.Models;

namespace MegaApp.Classes
{
    public class TransferQueu: ObservableCollection<TransferObjectModel>
    {
        public IEnumerable<TransferObjectModel> Uploads
        {
            get { return this.Items.Where(t => t.Type == TransferType.Upload); }
        }

        public IEnumerable<TransferObjectModel> Downloads
        {
            get { return this.Items.Where(t => t.Type == TransferType.Download); }
        }
    }
}
