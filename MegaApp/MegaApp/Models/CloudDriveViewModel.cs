using System.Collections;
using mega;
using MegaApp.Classes;
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
            this.MoveItemMode = false;
            this.NoFolderUpAction = false;
            this.CurrentRootNode = null;
            this.BreadCrumbNode = null;
            this.ChildNodes = new ObservableCollection<NodeViewModel>();
            this.BreadCrumbs = new ObservableCollection<NodeViewModel>();

            this.RemoveItemCommand = new DelegateCommand(this.RemoveItem);
            this.RenameItemCommand = new DelegateCommand(this.RenameItem);
            this.GetPreviewLinkItemCommand = new DelegateCommand(this.GetPreviewLink);
            this.DownloadItemCommand = new DelegateCommand(this.DownloadItem);

            this.UiService = new UiService();
        }
       
        #region Commands

        public ICommand RemoveItemCommand { get; set; }
        public ICommand GetPreviewLinkItemCommand { get; set; }
        public ICommand DownloadItemCommand { get; set; }
        public ICommand RenameItemCommand { get; set; }

        #endregion

        #region Services

        public IUiService UiService { get; set; }

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

                    ((ApplicationBarMenuItem)menuItems[0]).Text = UiResources.MyAccount;
                    break;
                }
                case MenuType.MoveMenu:
                {
                    ((ApplicationBarIconButton)iconButtons[0]).Text = UiResources.Move;
                    ((ApplicationBarIconButton)iconButtons[1]).Text = UiResources.CancelButton;
                  
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
            cameraCaptureTask.Completed += CameraCaptureTaskOnCompleted;
            cameraCaptureTask.Show();
        }

        private void CameraCaptureTaskOnCompleted(object sender, PhotoResult photoResult)
        {
            //
        }

        public void GoFolderUp()
        {
            MNode parentNode = this.MegaSdk.getParentNode(this.CurrentRootNode.GetMegaNode());

            if (parentNode == null || parentNode.getType() == MNodeType.TYPE_UNKNOWN )
                parentNode = this.MegaSdk.getRootNode();
            
            this.CurrentRootNode = new NodeViewModel(App.MegaSdk, parentNode);
            CalculateBreadCrumbs(this.CurrentRootNode);
        }

        public void GoToFolder(NodeViewModel folder)
        {
            this.BreadCrumbNode = this.CurrentRootNode;
            this.CurrentRootNode = folder;
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

        public void MoveItem(NodeViewModel selectedRootNode)
        {
            if (!IsUserOnline()) return;

            if (this.MegaSdk.checkMove(FocusedNode.GetMegaNode(), selectedRootNode.GetMegaNode()).getErrorCode() == MErrorType.API_OK)
                this.MegaSdk.moveNode(FocusedNode.GetMegaNode(), selectedRootNode.GetMegaNode(), new MoveNodeRequestListener(this));
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

            MNodeList nodeList = this.MegaSdk.getChildren(this.CurrentRootNode.GetMegaNode());

            for (int i = 0; i < nodeList.size(); i++)
            {
                ChildNodes.Add(new NodeViewModel(this.MegaSdk, nodeList.get(i), ChildNodes));
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

        public NodeViewModel BreadCrumbNode { get; set; }

        public bool NoFolderUpAction { get; set; }

        private bool _moveItemMode;
        public bool MoveItemMode
        {
            get { return _moveItemMode; }
            set
            {
                _moveItemMode = value;
                OnPropertyChanged("MoveItemMode");
            }
        }

        #endregion
      
    }
}
