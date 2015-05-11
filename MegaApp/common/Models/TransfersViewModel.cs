using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;

namespace MegaApp.Models
{
    class TransfersViewModel: BaseAppInfoAwareViewModel
    {
        public TransfersViewModel(MegaSDK megaSdk, AppInformation appInformation, TransferQueu megaTransfers)
            : base(megaSdk, appInformation)
        {
            MegaTransfers = megaTransfers;
            
            UpdateUserData();
            
            InitializeMenu(HamburgerMenuItemType.Transfers);
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
