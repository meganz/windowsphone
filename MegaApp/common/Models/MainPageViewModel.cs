using System;
using System.Collections;
using System.Linq;
using System.Windows;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Resources;
using MegaApp.Services;
using Microsoft.Phone.Shell;

namespace MegaApp.Models
{
    public class MainPageViewModel: BaseAppInfoAwareViewModel
    {
        public MainPageViewModel(MegaSDK megaSdk, AppInformation appInformation)
            :base(megaSdk, appInformation)
        {
            InitializeModel();

            UpdateUserData();

            InitializeMenu(HamburgerMenuItemType.CloudDrive);
        }

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


        public void LoadFolders()
        {
            if (this.CloudDrive.FolderRootNode == null)
                this.CloudDrive.FolderRootNode = NodeService.CreateNew(this.MegaSdk, this.AppInformation, this.MegaSdk.getRootNode());
            
            this.CloudDrive.LoadChildNodes();

            if (this.RubbishBin.FolderRootNode == null)
                this.RubbishBin.FolderRootNode = NodeService.CreateNew(this.MegaSdk, this.AppInformation, this.MegaSdk.getRubbishNode());
            
            this.RubbishBin.LoadChildNodes();
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

            // TODO Refactor
            //ShortCutHandle = null;
            this.MegaSdk.fetchNodes(new FetchNodesRequestListener(this));            
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

        #endregion

        #region Private Methods

        private void InitializeModel()
        {
            this.CloudDrive = new FolderViewModel(this.MegaSdk, this.AppInformation, ContainerType.CloudDrive);
            this.RubbishBin = new FolderViewModel(this.MegaSdk, this.AppInformation, ContainerType.RubbishBin);
            
            // The Cloud Drive is always the first active folder on initalization
            this.ActiveFolderView = this.CloudDrive;
        }
        
        #endregion

        #region Properties

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
    }
}
