using System.Collections;
using System.IO;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.MegaApi;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Telerik.Windows.Controls;

namespace MegaApp.Models
{
    public class CloudDriveViewModel : BaseSdkViewModel
    {
        public CloudDriveViewModel(MegaSDK megaSdk)
            : base(megaSdk)
        {
            this.DriveDisplayMode = DriveDisplayMode.CloudDrive;
            this.CurrentRootNode = null;
            this.BreadCrumbNode = null;
            this.ChildNodes = new ObservableCollection<NodeViewModel>();
            this.BreadCrumbs = new ObservableCollection<NodeViewModel>();
            this.SelectedNodes = new List<NodeViewModel>();

            this.RemoveItemCommand = new DelegateCommand(this.RemoveItem);
            this.RenameItemCommand = new DelegateCommand(this.RenameItem);
            this.GetPreviewLinkItemCommand = new DelegateCommand(this.GetPreviewLink);
            this.DownloadItemCommand = new DelegateCommand(this.DownloadItem);
        }
       
        #region Commands

        public ICommand RemoveItemCommand { get; set; }
        public ICommand GetPreviewLinkItemCommand { get; set; }
        public ICommand DownloadItemCommand { get; set; }
        public ICommand RenameItemCommand { get; set; }

        #endregion

        #region Services
       

        #endregion


        #region Public Methods

        public void TranslateAppBar(IList iconButtons, IList menuItems, MenuType menuType)
        {
            switch (menuType)
            {
                case MenuType.CloudDriveMenu:
                {
                    ((ApplicationBarIconButton)iconButtons[0]).Text = UiResources.Upload;
                    ((ApplicationBarIconButton)iconButtons[1]).Text = UiResources.AddFolder;
                    ((ApplicationBarIconButton)iconButtons[2]).Text = UiResources.Refresh;
                    ((ApplicationBarIconButton)iconButtons[3]).Text = UiResources.OpenLinkAppBar;

                    ((ApplicationBarMenuItem)menuItems[0]).Text = UiResources.Transfers;
                    ((ApplicationBarMenuItem)menuItems[1]).Text = UiResources.MyAccount;
                    break;
                }
                case MenuType.MoveMenu:
                {
                    ((ApplicationBarIconButton)iconButtons[0]).Text = UiResources.Move;
                    ((ApplicationBarIconButton)iconButtons[1]).Text = UiResources.CancelButton;
                  
                    break;
                }
                case MenuType.MultiSelectMenu:
                {
                    ((ApplicationBarIconButton)iconButtons[0]).Text = UiResources.Download;
                    ((ApplicationBarIconButton)iconButtons[1]).Text = UiResources.Move;
                    ((ApplicationBarIconButton)iconButtons[2]).Text = UiResources.Remove;
                    break;
                }
            }
           
        }

        public bool HasChildNodes()
        {
            return ChildNodes.Count > 0;
        }

        public void CaptureCameraImage()
        {
            var cameraCaptureTask = new CameraCaptureTask();
            cameraCaptureTask.Completed += PhotoTaskOnCompleted;
            NoFolderUpAction = true;
            cameraCaptureTask.Show();
        }

        public void SelectImage()
        {
            var photoChooserTask = new PhotoChooserTask();
            photoChooserTask.Completed += PhotoTaskOnCompleted;
            photoChooserTask.ShowCamera = true;
            NoFolderUpAction = true;
            photoChooserTask.Show();
        }

        private async void PhotoTaskOnCompleted(object sender, PhotoResult photoResult)
        {
            if (photoResult.TaskResult != TaskResult.OK) return;

            string fileName = Path.GetFileName(photoResult.OriginalFileName);
            if (fileName != null)
            {
                string newFilePath = Path.Combine(AppService.GetUploadDirectoryPath(), fileName);
                using (var fs = new FileStream(newFilePath, FileMode.Create))
                {
                    await photoResult.ChosenPhoto.CopyToAsync(fs);
                    await fs.FlushAsync();
                    fs.Close();
                }
                var uploadTransfer = new TransferObjectModel(MegaSdk, CurrentRootNode, TransferType.Upload, newFilePath);
                App.MegaTransfers.Insert(0,uploadTransfer);
                uploadTransfer.StartTransfer();
            }
            NoFolderUpAction = true;
            NavigateService.NavigateTo(typeof(TransferPage), NavigationParameter.Normal);
        }

        public void GoFolderUp()
        {
            MNode parentNode = this.MegaSdk.getParentNode(this.CurrentRootNode.GetMegaNode());

            if (parentNode == null || parentNode.getType() == MNodeType.TYPE_UNKNOWN )
                parentNode = this.MegaSdk.getRootNode();
            
            this.CurrentRootNode = new NodeViewModel(App.MegaSdk, parentNode, childCollection:ChildNodes);
            CalculateBreadCrumbs(this.CurrentRootNode);
        }

        public void GoToFolder(NodeViewModel folder)
        {
            this.BreadCrumbNode = this.CurrentRootNode;
            this.CurrentRootNode = folder;
            this.CurrentRootNode.ChildCollection = ChildNodes;
            CalculateBreadCrumbs(this.CurrentRootNode);
            NavigateService.NavigateTo(typeof(MainPage), NavigationParameter.BreadCrumb, new Dictionary<string, string> { { "Id", Guid.NewGuid().ToString("N") } });
        }

        public void GoToRoot()
        {
            GoToFolder(new NodeViewModel(this.MegaSdk, MegaSdk.getRootNode()));
        }

        public void GoToAccountDetails()
        {
            this.NoFolderUpAction = true;
            NavigateService.NavigateTo(typeof(MyAccountPage), NavigationParameter.Normal);
        }

        public void GoToTransfers()
        {
            this.NoFolderUpAction = true;
            NavigateService.NavigateTo(typeof(TransferPage), NavigationParameter.Normal);
        }

        public int CountBreadCrumbs()
        {
            int result = 0;
            MNode parentNode = null;
            MNode startNode = this.BreadCrumbNode.GetMegaNode();

            while (parentNode == null || parentNode.getBase64Handle() != this.CurrentRootNode.GetMegaNode().getBase64Handle())
            {
                parentNode = this.MegaSdk.getParentNode(startNode);
                startNode = parentNode;
                result++;
            }

            return result;
        }

        public async void OpenLink()
        {
            if (!IsUserOnline()) return;

            var inputPromptClosedEventArgs = await RadInputPrompt.ShowAsync(new string[] { UiResources.OpenButton, UiResources.CancelButton }, UiResources.OpenLink, vibrate: false);

            if (inputPromptClosedEventArgs.Result != DialogResult.OK) return;

            this.MegaSdk.getPublicNode(inputPromptClosedEventArgs.Text, new GetPublicNodeRequestListener(this));
        }

        public void ImportLink(string link)
        {
            this.MegaSdk.importFileLink(link, CurrentRootNode.GetMegaNode(), new ImportFileRequestListener(this)); ;
        }

        public void LoadNodes()
        {
            this.ChildNodes.Clear();

            var sortOrder = SettingsService.LoadSetting<int>(SettingsResources.SortOrderNodes) == 0
                ? (int)MSortOrderType.ORDER_DEFAULT_ASC
                : SettingsService.LoadSetting<int>(SettingsResources.SortOrderNodes);
            
            MNodeList nodeList = this.MegaSdk.getChildren(this.CurrentRootNode.GetMegaNode(), sortOrder);

            for (int i = 0; i < nodeList.size(); i++)
            {
                var node = new NodeViewModel(this.MegaSdk, nodeList.get(i), ChildNodes);

                if (DriveDisplayMode == DriveDisplayMode.MoveItem && FocusedNode != null &&
                    node.GetMegaNode().getBase64Handle() == FocusedNode.GetMegaNode().getBase64Handle())
                {
                    node.DisplayMode = NodeDisplayMode.SelectedForMove;
                    FocusedNode = node;
                }

                ChildNodes.Add(node);
            }
        }

        public void OnNodeTap(NodeViewModel node)
        {
            switch (node.Type)
            {
                case MNodeType.TYPE_FOLDER:
                {
                    SelectFolder(node);
                    break;
                }
                case MNodeType.TYPE_FILE:
                {
                    if (!node.IsImage) return;
                    this.NoFolderUpAction = true;
                    FocusedNode = node;
                    NavigateService.NavigateTo(typeof(PreviewImagePage), NavigationParameter.Normal);
                    break;
                }
            }
        }

        public async void AddFolder(NodeViewModel parentNode)
        {
            if (!IsUserOnline()) return;

            var inputPromptClosedEventArgs = await RadInputPrompt.ShowAsync(new string[] {UiResources.AddButton, UiResources.CancelButton}, UiResources.CreateFolder, vibrate: false);

            if (inputPromptClosedEventArgs.Result != DialogResult.OK) return;

            this.MegaSdk.createFolder(inputPromptClosedEventArgs.Text, parentNode.GetMegaNode(), new CreateFolderRequestListener(this));
        }

        public void FetchNodes(NodeViewModel rootRefreshNode = null)
        {
            this.ChildNodes.Clear();

            var fetchNodesRequestListener = new FetchNodesRequestListener(this, rootRefreshNode);
            this.MegaSdk.fetchNodes(fetchNodesRequestListener);
        }

        public void SelectFolder(NodeViewModel selectedNode)
        {
            this.CurrentRootNode = selectedNode;
            this.CurrentRootNode.ChildCollection = ChildNodes;
            CalculateBreadCrumbs(this.CurrentRootNode);
            // Create unique uri string to navigate
            NavigateService.NavigateTo(typeof(MainPage), NavigationParameter.Browsing, new Dictionary<string, string> {{"Id", Guid.NewGuid().ToString("N")}});
        }

        #endregion

        #region Private Methods

        private void CalculateBreadCrumbs(NodeViewModel currentRootNode)
        {
            this.BreadCrumbs.Clear();

            if (currentRootNode.Type == MNodeType.TYPE_ROOT) return;

            this.BreadCrumbs.Add(currentRootNode);

            MNode parentNode = currentRootNode.GetMegaNode();
            while ((parentNode = this.MegaSdk.getParentNode(parentNode)).getType() !=
                   MNodeType.TYPE_ROOT)
            {
                this.BreadCrumbs.Insert(0, new NodeViewModel(this.MegaSdk, parentNode));
            }

        }

        private void GetPreviewLink(object obj)
        {
            if (!IsUserOnline()) return;

            FocusedNode.GetPreviewLink();
        }

        private void DownloadItem(object obj)
        {
            this.NoFolderUpAction = true;
            FocusedNode.ViewOriginal();
        }

        private void RemoveItem(object obj)
        {
            FocusedNode.Remove();
        }

        private void RenameItem(object obj)
        {
            FocusedNode.Rename();
        }

        #endregion

        #region Properties

        public ObservableCollection<NodeViewModel> ChildNodes { get; set; }
        public ObservableCollection<NodeViewModel> BreadCrumbs { get; set; }

        public NodeViewModel CurrentRootNode { get; set; }

        public NodeViewModel FocusedNode { get; set; }

        public List<NodeViewModel> SelectedNodes { get; set; } 

        public NodeViewModel BreadCrumbNode { get; set; }

        public bool NoFolderUpAction { get; set; }

        public DriveDisplayMode DriveDisplayMode { get; set; }

        #endregion
      
    }
}
