using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mega;
using MegaApp.Classes;

namespace MegaApp.Models
{
    class TransfersViewModel: BaseSdkViewModel
    {
        public TransfersViewModel(MegaSDK megaSdk, TransferQueu megaTransfers)
            : base(megaSdk)
        {
            MegaTransfers = megaTransfers;
        }

        #region Methods

        public void PauseTransfers()
        {
            MegaSdk.pauseTransfers(true);
        }

        #endregion

        #region Properties

        public TransferQueu MegaTransfers { get; set; }

        #endregion
    }
}
