using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;
using Microsoft.Phone.Shell;

namespace MegaApp.Models
{
    public class MainPageViewModel : BaseAppInfoAwareViewModel, MRequestListenerInterface
    {
        public event EventHandler<CommandStatusArgs> CommandStatusChanged;
        private readonly MainPage _mainPage;

        public MainPageViewModel(MegaSDK megaSdk, AppInformation appInformation, MainPage mainPage)
            :base(megaSdk, appInformation)
        {
            _mainPage = mainPage;            
            UpgradeAccountCommand = new DelegateCommand(UpgradeAccount);
            CancelUpgradeAccountCommand = new DelegateCommand(CancelUpgradeAccount);

            InitializeModel();

            UpdateUserData();

            InitializeMenu(HamburgerMenuItemType.CloudDrive);
        }

        #region Commands
                
        public ICommand UpgradeAccountCommand { get; set; }
        public ICommand CancelUpgradeAccountCommand { get; set; }

        #endregion

        #region Events

        private void OnCommandStatusChanged(bool status)
        {
            if (CommandStatusChanged == null) return;

            CommandStatusChanged(this, new CommandStatusArgs(status));
        }

        #endregion

        #region Public Methods

        public void Initialize(GlobalDriveListener globalDriveListener)
        {
            // Add folders to global drive listener to receive notifications
            globalDriveListener.Folders.Add(this.CloudDrive);
            globalDriveListener.Folders.Add(this.RubbishBin);
        }

        public void Deinitialize(GlobalDriveListener globalDriveListener)
        {
            // Add folders to global drive listener to receive notifications
            globalDriveListener.Folders.Remove(this.CloudDrive);
            globalDriveListener.Folders.Remove(this.RubbishBin);
        }

        public void SetCommandStatus(bool status)
        {
            OnCommandStatusChanged(status);
        }

        public void LoadFolders()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (this.CloudDrive.FolderRootNode == null)
                    this.CloudDrive.FolderRootNode = NodeService.CreateNew(this.MegaSdk, this.AppInformation, this.MegaSdk.getRootNode());

                this.CloudDrive.LoadChildNodes();

                if (this.RubbishBin.FolderRootNode == null)
                    this.RubbishBin.FolderRootNode = NodeService.CreateNew(this.MegaSdk, this.AppInformation, this.MegaSdk.getRubbishNode());

                this.RubbishBin.LoadChildNodes();

                // UNCOMMENT TO CONTINUE WORKING ON CAMERA UPLOADS
                //if (SettingsService.LoadSetting<bool>(SettingsResources.CameraUploadsFirstInit, true))
                //{
                //    SettingsService.SaveSetting<bool>(SettingsResources.CameraUploadsFirstInit, false);
                //    NavigateService.NavigateTo(typeof(InitCameraUploadsPage), NavigationParameter.Normal);
                //}
            }); 
        }

        public void FetchNodes()
        {
            if (this.CloudDrive != null)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() => this.CloudDrive.SetEmptyContentTemplate(true));
                this.CloudDrive.CancelLoad();
            }

            if (this.RubbishBin != null)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() => this.RubbishBin.SetEmptyContentTemplate(true));
                this.RubbishBin.CancelLoad();
            }

            var fetchNodesRequestListener = new FetchNodesRequestListener(this, ShortCutHandle);
            ShortCutHandle = null;
            this.MegaSdk.fetchNodes(fetchNodesRequestListener);
        }

        public void ChangeMenu(FolderViewModel currentFolderViewModel, IList iconButtons, IList menuItems)
        {
            switch (currentFolderViewModel.CurrentDisplayMode)
            {
                case DriveDisplayMode.CloudDrive:
                {
                    this.TranslateAppBarItems(
                        iconButtons.Cast<ApplicationBarIconButton>().ToList(),
                        menuItems.Cast<ApplicationBarMenuItem>().ToList(),
                        new[] { UiResources.Upload, UiResources.AddFolder, UiResources.OpenLink},
                        new []{ UiResources.Refresh, UiResources.Sort, UiResources.MultiSelect});
                    break;
                }
                case DriveDisplayMode.MoveItem:
                {
                    this.TranslateAppBarItems(
                        iconButtons.Cast<ApplicationBarIconButton>().ToList(),
                        menuItems.Cast<ApplicationBarMenuItem>().ToList(),
                        new[] { UiResources.Move, UiResources.Cancel, },
                        null);
                    break;
                }
                case DriveDisplayMode.MultiSelect:
                {
                    this.TranslateAppBarItems(
                        iconButtons.Cast<ApplicationBarIconButton>().ToList(),
                        menuItems.Cast<ApplicationBarMenuItem>().ToList(),
                        new[] { UiResources.Download, UiResources.Move, UiResources.Remove },
                        new[] { UiResources.Cancel});
                    break;
                }
                case DriveDisplayMode.RubbishBin:
                {
                    this.TranslateAppBarItems(
                        iconButtons.Cast<ApplicationBarIconButton>().ToList(),
                        menuItems.Cast<ApplicationBarMenuItem>().ToList(),
                        new[] { UiResources.ClearRubbishBin },
                        new[] { UiResources.Refresh, UiResources.Sort, UiResources.MultiSelect });
                    break;
                }
                case DriveDisplayMode.ImportItem:
                {
                    this.TranslateAppBarItems(
                        iconButtons.Cast<ApplicationBarIconButton>().ToList(),
                        menuItems.Cast<ApplicationBarMenuItem>().ToList(),
                        new[] { UiResources.Import, UiResources.Cancel},
                        null);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException("currentFolderViewModel");
            }
        }

        public void GetAccountDetails()
        {
            MegaSdk.getAccountDetails(this);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Get a random visibility
        /// </summary>
        /// <param name="PercentOfTimes">Argument with the "%" of times that the visibility should be true</param>
        private Visibility GetRandomVisibility(int PercentOfTimes)
        {            
            if (new Random().Next(100) < PercentOfTimes)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        /// <summary>
        /// Timer for the visibility of the border/dialog to ask user to upgrade when is a free account
        /// </summary>
        /// <param name="milliseconds">Argument with the milliseconds that the visibility will be true and then will change to false</param>
        private async void TimerGetProAccountVisibility(int milliseconds)
        {            
            await Task.Delay(milliseconds);
            Deployment.Current.Dispatcher.BeginInvoke(() => _mainPage.ChangeGetProAccountBorderVisibility(Visibility.Collapsed));
        }

        /// <summary>
        /// Timer for the visibility of the warning border/dialog to ask user to upgrade because is going out of space
        /// </summary>
        /// <param name="milliseconds">Argument with the milliseconds that the visibility will be true and then will change to false</param>
        private async void TimerWarningOutOfSpaceVisibility(int milliseconds)
        {            
            await Task.Delay(milliseconds);
            Deployment.Current.Dispatcher.BeginInvoke(() => _mainPage.ChangeWarningOutOfSpaceBorderVisibility(Visibility.Collapsed));
        }

        private void UpgradeAccount(object obj)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                _mainPage.ChangeGetProAccountBorderVisibility(Visibility.Collapsed);
                _mainPage.ChangeWarningOutOfSpaceBorderVisibility(Visibility.Collapsed);
            });

            var extraParams = new Dictionary<string, string>(1);
            extraParams.Add("Pivot", "1");
            NavigateService.NavigateTo(typeof(MyAccountPage), NavigationParameter.Normal, extraParams);            
        }

        private void CancelUpgradeAccount(object obj)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                _mainPage.ChangeGetProAccountBorderVisibility(Visibility.Collapsed);
                _mainPage.ChangeWarningOutOfSpaceBorderVisibility(Visibility.Collapsed);
            });
        }

        private void InitializeModel()
        {
            this.CloudDrive = new FolderViewModel(this.MegaSdk, this.AppInformation, ContainerType.CloudDrive);
            this.RubbishBin = new FolderViewModel(this.MegaSdk, this.AppInformation, ContainerType.RubbishBin);
            
            // The Cloud Drive is always the first active folder on initalization
            this.ActiveFolderView = this.CloudDrive;
        }
        
        #endregion

        #region Properties

        public ulong? ShortCutHandle { get; set; }

        private string _activeImportLink;
        public string ActiveImportLink
        {
            get { return _activeImportLink; }
            set { SetField(ref _activeImportLink, value); }
        }

        private FolderViewModel _cloudDrive;
        public FolderViewModel CloudDrive
        {
            get { return _cloudDrive; }
            private set { SetField(ref _cloudDrive, value); }
        }

        private FolderViewModel _rubbishBin;
        public FolderViewModel RubbishBin
        {
            get { return _rubbishBin; }
            private set { SetField(ref _rubbishBin, value); }
        }

        private FolderViewModel _activeFolderView;
        public FolderViewModel ActiveFolderView
        {
            get { return _activeFolderView; }
            set { SetField(ref _activeFolderView, value); }
        }

        #endregion

        #region MRequestListenerInterface

        public void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            if (e.getErrorCode() == MErrorType.API_OK)
            {
                switch (request.getType())
                {
                    case MRequestType.TYPE_ACCOUNT_DETAILS:

                        ulong TotalSpace = request.getMAccountDetails().getStorageMax();
                        ulong UsedSpace = request.getMAccountDetails().getStorageUsed();
                        int usedSpacePercent;

                        if ((TotalSpace > 0) && (UsedSpace > 0))
                            usedSpacePercent = (int)(UsedSpace * 100 / TotalSpace);
                        else
                            usedSpacePercent = 0;

                        // If used space is less than 95% and is a free account, the 5% of the times show a message to upgrade the account
                        if (usedSpacePercent <= 95)
                        {
                            if (request.getMAccountDetails().getProLevel() == MAccountType.ACCOUNT_TYPE_FREE)
                            {
                                Task.Run(() =>
                                {
                                    Visibility visibility = GetRandomVisibility(5);
                                    Deployment.Current.Dispatcher.BeginInvoke(() => _mainPage.ChangeGetProAccountBorderVisibility(visibility));

                                    if (visibility == Visibility.Visible)
                                        this.TimerGetProAccountVisibility(30000);
                                });
                            }
                        }
                        // Else show a warning message indicating the user is running out of space
                        else
                        {
                            Task.Run(() =>
                            {
                                Deployment.Current.Dispatcher.BeginInvoke(() => _mainPage.ChangeWarningOutOfSpaceBorderVisibility(Visibility.Visible));
                                this.TimerWarningOutOfSpaceVisibility(15000);
                            });
                        }

                        break;

                    default:
                        break;
                }
            }            
        }

        public void onRequestStart(MegaSDK api, MRequest request)
        {
            // Not necessary
        }

        public void onRequestTemporaryError(MegaSDK api, MRequest request, MError e)
        {
            // Not necessary
        }

        public void onRequestUpdate(MegaSDK api, MRequest request)
        {
            // Not necessary
        }

        #endregion
    }
}
