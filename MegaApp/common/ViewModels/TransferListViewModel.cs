using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Resources;
using MegaApp.Services;
using System.Windows;

namespace MegaApp.ViewModels
{
    /// <summary>
    /// Viewmodel to display transfers in a list
    /// </summary>
    public class TransferListViewModel : BaseSdkViewModel
    {
        public TransferListViewModel(MTransferType type) : base (SdkService.MegaSdk)
        {
            this.Type = type;
            switch (this.Type)
            {
                case MTransferType.TYPE_DOWNLOAD:
                    this.Description = UiResources.Downloads;
                    this.CancelTransfersTitleText = UiResources.UI_CancelDownloads;
                    this.CancelTransfersDescriptionText = AppMessages.AM_CancelDownloadsQuestion;
                    this.Items = TransfersService.MegaTransfers.Downloads;
                    break;

                case MTransferType.TYPE_UPLOAD:
                    this.Description = UiResources.Uploads;
                    this.CancelTransfersTitleText = UiResources.UI_CancelUploads;
                    this.CancelTransfersDescriptionText = AppMessages.AM_CancelUploadsQuestion;
                    this.Items = TransfersService.MegaTransfers.Uploads;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            this.IsCompletedTransfersList = false;
            this.PauseOrResumeCommand = new DelegateCommand(PauseOrResumeTransfers);
            this.CancelCommand = new DelegateCommand(CancelTransfers);
            this.CleanCommand = new DelegateCommand(CleanTransfers);

            this.Items.CollectionChanged += ItemsOnCollectionChanged;

            this.SetEmptyContentTemplate();
        }

        public TransferListViewModel() : base(SdkService.MegaSdk)
        {
            this.Description = UiResources.UI_Completed;
            this.IsCompletedTransfersList = true;
            this.Items = TransfersService.MegaTransfers.Completed;
            this.CleanCommand = new DelegateCommand(CleanCompletedTransfers);

            this.Items.CollectionChanged += ItemsOnCollectionChanged;

            this.SetEmptyContentTemplate();
        }

        private void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("IsEmpty");
        }

        public void SetEmptyContentTemplate()
        {
            OnUiThread(() =>
            {
                if(this.IsCompletedTransfersList)
                {
                    this.EmptyContentTemplate = (DataTemplate)Application.Current.Resources["MegaTransferListCompletedEmptyContent"];
                    this.EmptyInformationText = UiResources.UI_NoCompletedTransfers.ToLower();
                }
                else
                {
                    switch (this.Type)
                    {
                        case MTransferType.TYPE_DOWNLOAD:
                            this.EmptyContentTemplate = (DataTemplate)Application.Current.Resources["MegaTransferListDownloadEmptyContent"];
                            this.EmptyInformationText = UiResources.NoDownloads.ToLower();
                            break;

                        case MTransferType.TYPE_UPLOAD:
                            this.EmptyContentTemplate = (DataTemplate)Application.Current.Resources["MegaTransferListUploadEmptyContent"];
                            this.EmptyInformationText = UiResources.NoUploads.ToLower();
                            break;
                    }
                }
            });
        }

        public void SetOfflineContentTemplate()
        {
            OnUiThread(() =>
            {
                this.EmptyContentTemplate = (DataTemplate)Application.Current.Resources["OfflineEmptyContent"];
                this.EmptyInformationText = UiResources.NoInternetConnection.ToLower();
            });
        }

        public void UpdateTransfers(bool cleanTransfers = false)
        {
            TransfersService.UpdateMegaTransferList(TransfersService.MegaTransfers, this.Type, cleanTransfers);
        }

        private void PauseOrResumeTransfers(object obj)
        {
            var playPauseStatus = !AreTransfersPaused;

            var pauseTransfers = new PauseTransferRequestListener();
            pauseTransfers.PauseTransfersFinished += OnPauseOrResumeTransfersFinished;

            SdkService.MegaSdk.pauseTransfersDirection(playPauseStatus,
                (int)this.Type, pauseTransfers);
        }

        private void OnPauseOrResumeTransfersFinished(object sender, EventArgs e)
        {
            OnPropertyChanged("AreTransfersPaused");
        }

        private void CleanTransfers(object obj)
        {
            TransfersService.UpdateMegaTransferList(TransfersService.MegaTransfers, this.Type, true);
        }

        private void CleanCompletedTransfers(object obj)
        {
            TransfersService.MegaTransfers.Completed.Clear();
        }

        /// <summary>
        /// Cancel all transfers of the current type.        
        /// </summary>
        private async void CancelTransfers(object obj)
        {
            var result = await new CustomMessageDialog(this.CancelTransfersTitleText,
                this.CancelTransfersDescriptionText, App.AppInformation,
                MessageDialogButtons.OkCancel).ShowDialogAsync();

            if (result == MessageDialogResult.CancelNo) return;

            // Use a temp list to avoid InvalidOperationException
            var transfers = Items.ToList();
            foreach (var transfer in transfers)
            {
                if(transfer == null) continue;

                // If the transfer is an upload and is being prepared (copying file to the upload temporary folder)
                if (this.Type == MTransferType.TYPE_UPLOAD && transfer.PreparingUploadCancelToken != null)
                {
                    transfer.PreparingUploadCancelToken.Cancel();
                }
                // If the transfer is ready but not started for some reason
                else if (transfer.IsBusy == false && transfer.TransferState == MTransferState.STATE_NONE)
                {
                    LogService.Log(MLogLevel.LOG_LEVEL_INFO, string.Format("Transfer ({0}) canceled: {1}",
                        this.Type == MTransferType.TYPE_UPLOAD? "UPLOAD" : "DOWNLOAD", transfer.DisplayName));                    
                    transfer.TransferState = MTransferState.STATE_CANCELLED;
                }
            }

            SdkService.MegaSdk.cancelTransfers((int)this.Type);
        }

        #region Commands

        public ICommand PauseOrResumeCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public ICommand CleanCommand { get; private set; }

        #endregion

        #region Properties

        public string Description { get; private set; }

        public MTransferType Type { get; set; }

        public ObservableCollection<TransferObjectModel> Items { get; set; }

        public bool IsEmpty
        {
            get { return (Items.Count == 0); }
        }

        private bool _isCompletedTransfersList;
        public bool IsCompletedTransfersList
        {
            get { return _isCompletedTransfersList; }
            set { SetField(ref _isCompletedTransfersList, value); }
        }

        private DataTemplate _emptyContentTemplate;
        public DataTemplate EmptyContentTemplate
        {
            get { return _emptyContentTemplate; }
            private set { SetField(ref _emptyContentTemplate, value); }
        }

        private String _emptyInformationText;
        public String EmptyInformationText
        {
            get { return _emptyInformationText; }
            private set { SetField(ref _emptyInformationText, value); }
        }

        public bool AreTransfersPaused
        {
            get
            {
                return this.IsCompletedTransfersList ? false : this.MegaSdk.areTransfersPaused((int)this.Type);
            }
        }

        public string CancelTransfersTitleText { get; private set; }
        public string CancelTransfersDescriptionText { get; private set; }

        #endregion
    }
}
