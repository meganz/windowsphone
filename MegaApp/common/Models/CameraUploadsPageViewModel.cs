﻿using System;
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
    public class CameraUploadsPageViewModel : BaseAppInfoAwareViewModel
    {
        public CameraUploadsPageViewModel(MegaSDK megaSdk, AppInformation appInformation)
            :base(megaSdk, appInformation)
        {
            UpdateUserData();

            InitializeModel();

            InitializeMenu(HamburgerMenuItemType.CameraUploads);          
        }
        

        #region Public Methods

        public async void Initialize(GlobalDriveListener globalDriveListener)
        {
            // Add folders to global drive listener to receive notifications
            globalDriveListener.Folders.Add(this.CameraUploads);

            // Create Camera Uploads folder node if not exists
            await this.CameraUploads.CreateRootNodeIfNotExists();
        }

        public void Deinitialize(GlobalDriveListener globalDriveListener)
        {
            // Add folders to global drive listener to receive notifications
            globalDriveListener.Folders.Remove(this.CameraUploads);
        }

        public void FetchNodes()
        {
            if (this.CameraUploads != null)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() => this.CameraUploads.SetEmptyContentTemplate(true));
                this.CameraUploads.CancelLoad();
            }

            var fetchNodesRequestListener = new FetchNodesRequestListener(null, this);
            this.AppInformation.HasFetchedNodes = false;
            this.MegaSdk.fetchNodes(fetchNodesRequestListener);
        }
       

        public void LoadFolders()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (this.CameraUploads.FolderRootNode == null)
                    this.CameraUploads.FolderRootNode = NodeService.CreateNew(this.MegaSdk, this.AppInformation, 
                        NodeService.FindCameraUploadNode(this.MegaSdk, this.MegaSdk.getRootNode()),
                        ContainerType.CloudDrive);

                this.CameraUploads.LoadChildNodes();
            }); 
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
                        new[] { UiResources.Upload, UiResources.AddFolder, UiResources.UI_OpenLink},
                        new []{ UiResources.Refresh, UiResources.Sort, UiResources.MultiSelect});
                    break;
                }
                case DriveDisplayMode.CopyOrMoveItem:
                {
                    this.TranslateAppBarItems(
                        iconButtons.Cast<ApplicationBarIconButton>().ToList(),
                        menuItems.Cast<ApplicationBarMenuItem>().ToList(),
                        new[] { UiResources.AddFolder, UiResources.Copy, UiResources.Move, UiResources.Cancel },
                        null);
                    break;
                }
                case DriveDisplayMode.MultiSelect:
                {
                    this.TranslateAppBarItems(
                        iconButtons.Cast<ApplicationBarIconButton>().ToList(),
                        menuItems.Cast<ApplicationBarMenuItem>().ToList(),
                        new[] { UiResources.Download, String.Format("{0}/{1}", UiResources.Copy, UiResources.Move), UiResources.Remove },
                        new[] { UiResources.SelectAll, UiResources.DeselectAll, UiResources.Cancel });
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
                        new[] { UiResources.AddFolder, UiResources.Import, UiResources.Cancel },
                        null);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException("currentFolderViewModel");
            }
        }

        #endregion

        #region Private Methods

        private async void InitializeModel()
        {
            this.CameraUploads = new CameraUploadsFolderViewModel(this.MegaSdk, this.AppInformation, ContainerType.CloudDrive);
        }
        
        #endregion

        #region Properties
     

        private CameraUploadsFolderViewModel _cameraUploads;
        public CameraUploadsFolderViewModel CameraUploads
        {
            get { return _cameraUploads; }
            private set { SetField(ref _cameraUploads, value); }
        }
        
        #endregion
    }
}
