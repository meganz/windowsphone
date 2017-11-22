using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Services;

namespace MegaApp.Models
{
    class TransfersViewModel: BaseAppInfoAwareViewModel
    {
        public TransfersViewModel(MegaSDK megaSdk, AppInformation appInformation)
            : base(megaSdk, appInformation)
        {
            UpdateUserData();
            
            InitializeMenu(HamburgerMenuItemType.Transfers);

            this.Downloads = new TransferListViewModel(MTransferType.TYPE_DOWNLOAD);
            this.Uploads = new TransferListViewModel(MTransferType.TYPE_UPLOAD);
            this.Completed = new TransferListViewModel();

            this.ActiveViewModel = this.Downloads;
        }        

        #region Methods

        public void Update()
        {
            this.Downloads.UpdateTransfers();
            this.Uploads.UpdateTransfers();
        }

        #endregion

        #region Properties

        public TransferListViewModel Uploads { get; private set; }

        public TransferListViewModel Downloads { get; private set; }

        public TransferListViewModel Completed { get; private set; }

        private TransferListViewModel _activeViewModel;
        public TransferListViewModel ActiveViewModel
        {
            get { return _activeViewModel; }
            set { SetField(ref _activeViewModel, value); }
        }

        public bool IsNetworkAvailableBinding
        {
            get { return NetworkService.IsNetworkAvailable(); }
        }

        #endregion
    }
}
