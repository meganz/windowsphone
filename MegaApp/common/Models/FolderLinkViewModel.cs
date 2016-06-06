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
    public class FolderLinkViewModel : BaseSdkViewModel
    {
        public event EventHandler<CommandStatusArgs> CommandStatusChanged;        

        public FolderLinkViewModel(MegaSDK megaSdk, AppInformation appInformation)
            :base(megaSdk)
        {            
            InitializeModel();
        }

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
            globalDriveListener.Folders.Add(this.FolderLink);            
        }

        public void Deinitialize(GlobalDriveListener globalDriveListener)
        {
            // Add folders to global drive listener to receive notifications
            globalDriveListener.Folders.Remove(this.FolderLink);            
        }

        public void SetCommandStatus(bool status)
        {
            OnCommandStatusChanged(status);
        }

        public void LoadFolders()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (this.FolderLink.FolderRootNode == null)
                {
                    this.FolderLink.FolderRootNode = NodeService.CreateNew(this.MegaSdk,
                        App.AppInformation, this.MegaSdk.getRootNode(), ContainerType.FolderLink);
                }

                this.FolderLink.LoadChildNodes();
            });
        }

        public void FetchNodes()
        {
            if (this.FolderLink != null)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() => this.FolderLink.SetEmptyContentTemplate(true));
                this.FolderLink.CancelLoad();
            }

            var fetchNodesRequestListener = new FetchNodesRequestListener(this);
            this.MegaSdk.fetchNodes(fetchNodesRequestListener);
        }

        public void ChangeMenu(FolderViewModel currentFolderViewModel, IList iconButtons, IList menuItems)
        {
            switch (currentFolderViewModel.CurrentDisplayMode)
            {
                case DriveDisplayMode.FolderLink:
                {
                    this.TranslateAppBarItems(
                        iconButtons.Cast<ApplicationBarIconButton>().ToList(),
                        menuItems.Cast<ApplicationBarMenuItem>().ToList(),
                        new[] { UiResources.Download, UiResources.Import, UiResources.Cancel },
                        new[] { UiResources.Refresh, UiResources.Sort, UiResources.MultiSelect});
                    break;
                }
                case DriveDisplayMode.MultiSelect:
                {
                    this.TranslateAppBarItems(
                        iconButtons.Cast<ApplicationBarIconButton>().ToList(),
                        menuItems.Cast<ApplicationBarMenuItem>().ToList(),
                        new[] { UiResources.Download, UiResources.Import },
                        new[] { UiResources.Cancel, UiResources.SelectAll, UiResources.DeselectAll});
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
            this.FolderLink = new FolderViewModel(this.MegaSdk, App.AppInformation, ContainerType.FolderLink);            
        }
        
        #endregion

        #region Properties
        
        private FolderViewModel _folderLink;
        public FolderViewModel FolderLink
        {
            get { return _folderLink; }
            private set { SetField(ref _folderLink, value); }
        }

        #endregion
    }
}
