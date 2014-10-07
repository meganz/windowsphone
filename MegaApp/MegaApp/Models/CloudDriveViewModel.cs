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

        public bool HasChildNodes()
        {
            return ChildNodes.Count > 0;
        }

        public void GoFolderUp()
        {
            MNode parentNode = this.MegaSdk.getParentNode(this.CurrentRootNode.GetBaseNode());

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

        public int CountBreadCrumbs()
        {
            int result = 0;
            MNode parentNode = null;
            MNode startNode = this.BreadCrumbNode.GetBaseNode();

            while (parentNode == null || parentNode.getBase64Handle() != this.CurrentRootNode.GetBaseNode().getBase64Handle())
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

            if (this.MegaSdk.checkMove(FocusedNode.GetBaseNode(), selectedRootNode.GetBaseNode()).getErrorCode() == MErrorType.API_OK)
                this.MegaSdk.moveNode(FocusedNode.GetBaseNode(), selectedRootNode.GetBaseNode(), new MoveNodeRequestListener(this));
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
            this.MegaSdk.importFileLink(link, CurrentRootNode.GetBaseNode(), new ImportFileRequestListener(this)); ;
        }

        public void LoadNodes()
        {
            this.ChildNodes.Clear();

            MNodeList nodeList = this.MegaSdk.getChildren(this.CurrentRootNode.GetBaseNode());

            for (int i = 0; i < nodeList.size(); i++)
            {
                ChildNodes.Add(new NodeViewModel(this.MegaSdk, nodeList.get(i)));
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

            this.MegaSdk.createFolder(inputPromptClosedEventArgs.Text, parentNode.GetBaseNode(), new CreateFolderRequestListener(this));
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

            MNode parentNode = currentRootNode.GetBaseNode();
            while ((parentNode = this.MegaSdk.getParentNode(parentNode)).getType() !=
                   MNodeType.TYPE_ROOT)
            {
                this.BreadCrumbs.Insert(0, new NodeViewModel(this.MegaSdk, parentNode));
            }

        }

        private void GetPreviewLink(object obj)
        {
            if (!IsUserOnline()) return;

            MegaService.GetPreviewLink(this.MegaSdk, FocusedNode);
        }

        private void DownloadItem(object obj)
        {
            this.NoFolderUpAction = true;
            NavigateService.NavigateTo(typeof(DownloadImagePage), NavigationParameter.Normal);
        }

        private void RemoveItem(object obj)
        {
            if (!IsUserOnline()) return;

            if (MessageBox.Show(String.Format(AppMessages.RemoveItemQuestion, FocusedNode.Name), AppMessages.RemoveItemQuestion_Title, MessageBoxButton.OKCancel) ==
                MessageBoxResult.Cancel) return;

            this.MegaSdk.moveNode(FocusedNode.GetBaseNode(), this.MegaSdk.getRubbishNode(), new RemoveNodeRequestListener(this));
        }

        private async void RenameItem(object obj)
        {
            if (!IsUserOnline()) return;
            
            var textboxStyle = new Style(typeof(RadTextBox));
            textboxStyle.Setters.Add(new Setter(TextBox.TextProperty, FocusedNode.Name));

            var inputPromptClosedEventArgs = await RadInputPrompt.ShowAsync(new string[] { UiResources.RenameButton, UiResources.CancelButton }, UiResources.RenameItem,
                vibrate: false, inputStyle: textboxStyle);

            if (inputPromptClosedEventArgs.Result != DialogResult.OK) return;

            this.MegaSdk.renameNode(FocusedNode.GetBaseNode(), inputPromptClosedEventArgs.Text, new RenameNodeRequestListener(this));
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
