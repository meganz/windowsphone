using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Resources;
using MegaApp.Services;

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

            SetEmptyContentTemplate();
        }        

        #region Methods

        public void PauseTransfers()
        {
            MegaSdk.pauseTransfers(true);
        }

        public void SetEmptyContentTemplate()
        {
            OnUiThread(() =>
            {
                this.UploadsEmptyContentTemplate = (DataTemplate)Application.Current.Resources["MegaTransferListUploadEmptyContent"];
                this.UploadsEmptyInformationText = UiResources.NoUploads.ToLower();

                this.DownloadsEmptyContentTemplate = (DataTemplate)Application.Current.Resources["MegaTransferListDownloadEmptyContent"];
                this.DownloadsEmptyInformationText = UiResources.NoDownloads.ToLower();

                OnPropertyChanged("IsNetworkAvailableBinding");
            });
        }

        public void SetOfflineContentTemplate()
        {
            OnUiThread(() =>
            {
                this.UploadsEmptyContentTemplate = (DataTemplate)Application.Current.Resources["OfflineEmptyContent"];
                this.UploadsEmptyInformationText = UiResources.NoInternetConnection.ToLower();

                this.DownloadsEmptyContentTemplate = (DataTemplate)Application.Current.Resources["OfflineEmptyContent"];
                this.DownloadsEmptyInformationText = UiResources.NoInternetConnection.ToLower();

                OnPropertyChanged("IsNetworkAvailableBinding");
            });
        }

        #endregion

        #region Properties

        private TransferQueu _megaTransfers;
        public TransferQueu MegaTransfers
        {
            get { return _megaTransfers; }
            set { SetField(ref _megaTransfers, value); }
        }

        public bool IsNetworkAvailableBinding
        {
            get { return NetworkService.IsNetworkAvailable(); }
        }

        private DataTemplate _uploadsEmptyContentTemplate;
        public DataTemplate UploadsEmptyContentTemplate
        {
            get { return _uploadsEmptyContentTemplate; }
            private set { SetField(ref _uploadsEmptyContentTemplate, value); }
        }

        private String _uploadsEmptyInformationText;
        public String UploadsEmptyInformationText
        {
            get { return _uploadsEmptyInformationText; }
            private set { SetField(ref _uploadsEmptyInformationText, value); }
        }

        private DataTemplate _downloadsEmptyContentTemplate;
        public DataTemplate DownloadsEmptyContentTemplate
        {
            get { return _downloadsEmptyContentTemplate; }
            private set { SetField(ref _downloadsEmptyContentTemplate, value); }
        }

        private String _downloadsEmptyInformationText;
        public String DownloadsEmptyInformationText
        {
            get { return _downloadsEmptyInformationText; }
            private set { SetField(ref _downloadsEmptyInformationText, value); }
        }

        #endregion
    }
}
