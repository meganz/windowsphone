using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Phone.Shell;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.MegaApi;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.Views;

namespace MegaApp.ViewModels
{
    public class FolderLinkViewModel : BaseSdkViewModel
    {
        public event EventHandler<CommandStatusArgs> CommandStatusChanged;
        public readonly FolderLinkPage _folderLinkPage;

        public FolderLinkViewModel(MegaSDK megaSdk, AppInformation appInformation, 
            FolderLinkPage folderLinkPage)
            :base(megaSdk)
        {
            this._folderLinkPage = folderLinkPage;

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

        public void Initialize(GlobalListener globalListener)
        {
            // Add folders to global listener to receive notifications
            globalListener.Folders.Add(this.FolderLink);            
        }

        public void Deinitialize(GlobalListener globalListener)
        {
            // Add folders to global listener to receive notifications
            globalListener.Folders.Remove(this.FolderLink);            
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

                // Store the absolute root node of the folder link
                if (this.FolderLinkRootNode == null)
                    this.FolderLinkRootNode = this.FolderLink.FolderRootNode;

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
                        new[] { UiResources.SelectAll, UiResources.DeselectAll, UiResources.Cancel });
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

        /// <summary>
        /// Property to store the absolute root node of the folder link.
        /// <para>Used for example to download/import all the folder link.</para>
        /// </summary>        
        private IMegaNode _folderLinkRootNode;
        public IMegaNode FolderLinkRootNode
        {
            get { return _folderLinkRootNode; }
            set { SetField(ref _folderLinkRootNode, value); }
        }

        #endregion
    }
}
