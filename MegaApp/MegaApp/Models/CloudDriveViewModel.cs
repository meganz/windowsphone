using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace MegaApp.Models
{
    public class CloudDriveViewModel : BaseSdkViewModel
    {
        private const int ProgressDisplaySize = 4000;
        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken cancellationToken;        

        public CloudDriveViewModel(MegaSDK megaSdk)
            : base(megaSdk)
        {
            this.DriveDisplayMode = DriveDisplayMode.CloudDrive;
            this.CurrentRootNode = null;
            this.BreadCrumbNode = null;
            this.ChildNodes = new ObservableCollection<NodeViewModel>();
            this.BreadCrumbs = new ObservableCollection<NodeViewModel>();
            this.SelectedNodes = new List<NodeViewModel>();
            this.IsMultiSelectActive = false;
            SetViewDefaults();

            this.RemoveItemCommand = new DelegateCommand(this.RemoveItem);
            this.RenameItemCommand = new DelegateCommand(this.RenameItem);
            this.GetPreviewLinkItemCommand = new DelegateCommand(this.GetPreviewLink);
            this.DownloadItemCommand = new DelegateCommand(this.DownloadItem);
            this.CreateShortCutCommand = new DelegateCommand(this.CreateShortCut);
            this.ChangeViewCommand = new DelegateCommand(this.ChangeView);
        }

        #region Commands

        public ICommand RemoveItemCommand { get; set; }
        public ICommand GetPreviewLinkItemCommand { get; set; }
        public ICommand DownloadItemCommand { get; set; }
        public ICommand RenameItemCommand { get; set; }
        public ICommand CreateShortCutCommand { get; set; }

        public ICommand ChangeViewCommand { get; set; }

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

                    ((ApplicationBarMenuItem)menuItems[0]).Text = UiResources.RubbishBin;
                    ((ApplicationBarMenuItem)menuItems[1]).Text = UiResources.MultiSelect;
                    ((ApplicationBarMenuItem)menuItems[2]).Text = UiResources.Transfers;
                    ((ApplicationBarMenuItem)menuItems[3]).Text = UiResources.MyAccount;
                    ((ApplicationBarMenuItem)menuItems[4]).Text = UiResources.Settings;
                    ((ApplicationBarMenuItem)menuItems[5]).Text = UiResources.About; 
                    break;
                }
                case MenuType.RubbishBinMenu:
                {
                    ((ApplicationBarIconButton)iconButtons[0]).Text = UiResources.Refresh;                    

                    ((ApplicationBarMenuItem)menuItems[0]).Text = UiResources.CloudDriveName;                    
                    ((ApplicationBarMenuItem)menuItems[1]).Text = UiResources.MultiSelect;
                    ((ApplicationBarMenuItem)menuItems[2]).Text = UiResources.Transfers;
                    ((ApplicationBarMenuItem)menuItems[3]).Text = UiResources.MyAccount;
                    ((ApplicationBarMenuItem)menuItems[4]).Text = UiResources.Settings;
                    ((ApplicationBarMenuItem)menuItems[5]).Text = UiResources.About;
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

        public bool MultipleRemove()
        {
            int count = ChildNodes.Count(n => n.IsMultiSelected);

            if (count < 1) return false;

            if (MessageBox.Show(String.Format(AppMessages.MultiSelectRemoveQuestion,count), 
                AppMessages.MultiSelectRemoveQuestion_Title, MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) return false;

            foreach (var node in ChildNodes.Where(n => n.IsMultiSelected))
            {
                node.Remove(true);
            }

            this.IsMultiSelectActive = false;

            MessageBox.Show(String.Format(AppMessages.MultiRemoveSucces, count),
                AppMessages.MultiRemoveSucces_Title, MessageBoxButton.OK);

            return true;
        }

        public bool SelectMultipleMove()
        {
            int count = ChildNodes.Count(n => n.IsMultiSelected);

            if (count < 1) return false;

            SelectedNodes.Clear();

            foreach (var node in ChildNodes.Where(n => n.IsMultiSelected))
            {
                node.DisplayMode = NodeDisplayMode.SelectedForMove;
                SelectedNodes.Add(node);
            }

            this.IsMultiSelectActive = false;
            OldDriveDisplayMode = DriveDisplayMode;
            DriveDisplayMode = DriveDisplayMode.MoveItem;

            return true;
        }

        public void MultipleDownload()
        {
            int count = ChildNodes.Count(n => n.IsMultiSelected);

            if (count < 1) return;

            foreach (var node in ChildNodes.Where(n => n.IsMultiSelected))
            {
                App.MegaTransfers.Add(node.Transfer);
                node.Transfer.StartTransfer();
            }
            this.IsMultiSelectActive = false;
            this.NoFolderUpAction = true;
            NavigateService.NavigateTo(typeof(TransferPage), NavigationParameter.Downloads);
        }

        private void CreateShortCut(object obj)
        {
            var shortCut = new RadExtendedTileData
            {
                BackgroundImage = new Uri("/Assets/Images/shortcut.png", UriKind.Relative),
                Title = FocusedNode.Name
            };
            
            
            LiveTileHelper.CreateOrUpdateTile(shortCut,
                new Uri("/Pages/MainPage.xaml?ShortCutHandle=" + FocusedNode.GetMegaNode().getHandle(), UriKind.Relative));
            
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
            if (cancellationToken.CanBeCanceled)
                cancellationTokenSource.Cancel();

            MNode parentNode = null;
            
            if (this.CurrentRootNode != null) 
                parentNode = this.MegaSdk.getParentNode(this.CurrentRootNode.GetMegaNode());

            if (parentNode == null || parentNode.getType() == MNodeType.TYPE_UNKNOWN )
                parentNode = this.MegaSdk.getRootNode();
            
            this.CurrentRootNode = new NodeViewModel(App.MegaSdk, parentNode, childCollection:ChildNodes);
            this.ChildNodes.Clear();
            //TODO REMOVE CalculateBreadCrumbs(this.CurrentRootNode);
        }

        private void SetView(ViewMode viewMode)
        {
            switch (viewMode)
            {
                case ViewMode.LargeThumbnails:
                    {
                        this.VirtualizationStrategy = new WrapVirtualizationStrategyDefinition()
                        {
                            Orientation = Orientation.Horizontal,
                            WrapLineAlignment = WrapLineAlignment.Near
                        };

                        this.NodeTemplateSelector = new NodeTemplateSelector()
                        {
                            FileItemTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListLargeViewFileItemContent"],
                            FolderItemTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListLargeViewFolderItemContent"]
                        };

                        this.ViewMode = ViewMode.LargeThumbnails;
                        this.ViewStateButtonIconUri = new Uri("/Assets/Images/view_large.png", UriKind.Relative);

                        this.MultiSelectCheckBoxStyle = (Style)Application.Current.Resources["MultiSelectItemCheckBoxStyle"];
                        break;
                    }
                case ViewMode.SmallThumbnails:
                    {
                        this.VirtualizationStrategy = new WrapVirtualizationStrategyDefinition()
                        {
                            Orientation = Orientation.Horizontal,
                            WrapLineAlignment = WrapLineAlignment.Near
                        };

                        this.NodeTemplateSelector = new NodeTemplateSelector()
                        {
                            FileItemTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListSmallViewFileItemContent"],
                            FolderItemTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListSmallViewFolderItemContent"]
                        };

                        this.ViewMode = ViewMode.SmallThumbnails;
                        this.ViewStateButtonIconUri = new Uri("/Assets/Images/view_small.png", UriKind.Relative);

                        this.MultiSelectCheckBoxStyle = (Style)Application.Current.Resources["MultiSelectItemCheckBoxStyle"];
                        break;
                    }
                case ViewMode.ListView:
                    {
                        SetViewDefaults();
                        break;
                    }
            }
        }

        private void ChangeView(object obj)
        {
            switch (this.ViewMode)
            {
                case ViewMode.ListView:
                    {
                        SetView(ViewMode.LargeThumbnails);
                        UiService.SetViewMode(CurrentRootNode.Handle, ViewMode.LargeThumbnails);
                        break;
                    }
                case ViewMode.LargeThumbnails:
                    {
                        SetView(ViewMode.SmallThumbnails);
                        UiService.SetViewMode(CurrentRootNode.Handle, ViewMode.SmallThumbnails);
                        break;
                    }
                case ViewMode.SmallThumbnails:
                    {
                        SetView(ViewMode.ListView);
                        UiService.SetViewMode(CurrentRootNode.Handle, ViewMode.ListView);
                        break;
                    }
            }
        }

        private void SetViewDefaults()
        {
            this.VirtualizationStrategy = new StackVirtualizationStrategyDefinition()
            {
                Orientation = Orientation.Vertical
            };

            this.NodeTemplateSelector = new NodeTemplateSelector()
            {
                FileItemTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListFileItemContent"],
                FolderItemTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListFolderItemContent"]
            };

            this.ViewMode = ViewMode.ListView;
            this.ViewStateButtonIconUri = new Uri("/Assets/Images/view_list.png", UriKind.Relative);

            this.MultiSelectCheckBoxStyle = null;
        }

        public void GoToFolder(NodeViewModel folder)
        {
            if(cancellationToken.CanBeCanceled)
                cancellationTokenSource.Cancel();

            this.BreadCrumbNode = this.CurrentRootNode;
            this.CurrentRootNode = folder;
            this.CurrentRootNode.ChildCollection = ChildNodes;
            this.ChildNodes.Clear();
            // TODO REMOVE CalculateBreadCrumbs(this.CurrentRootNode);
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
            // Get the nodes from the MEGA SDK
            MNodeList nodeList = this.MegaSdk.getChildren(this.CurrentRootNode.GetMegaNode(),
                UiService.GetSortOrder(CurrentRootNode.Handle, CurrentRootNode.Name));
            
            // Retrieve the size of the list to save time in the loops
            int listSize = nodeList.size();

            // Display a minor indication for the user that the app is busy
            if (Deployment.Current.Dispatcher.CheckAccess())
                ProgessService.SetProgressIndicator(true, String.Empty);
            else
            {
                var autoResetEvent = new AutoResetEvent(false);
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    ProgessService.SetProgressIndicator(true, String.Empty);
                    autoResetEvent.Set();
                });
                autoResetEvent.WaitOne();
            }
            // Clear the child nodes to make a fresh start
            if (Deployment.Current.Dispatcher.CheckAccess())
                this.ChildNodes.Clear();
            else
            {
                var autoResetEvent = new AutoResetEvent(false);
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    this.ChildNodes.Clear();
                    autoResetEvent.Set();
                });
                autoResetEvent.WaitOne();
            }

            // Build the bread crumbs. Do this before loading the nodes so that the user can click on home
            if (Deployment.Current.Dispatcher.CheckAccess())
                 CalculateBreadCrumbs(this.CurrentRootNode);
            else
            {
                var autoResetEvent = new AutoResetEvent(false);
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    CalculateBreadCrumbs(this.CurrentRootNode);
                    autoResetEvent.Set();
                });
                autoResetEvent.WaitOne();
            }

            // Set the correct view for the main drive. Do this after the childs are cleared to speed things up
            if (Deployment.Current.Dispatcher.CheckAccess())
                SetView(UiService.GetViewMode(CurrentRootNode.Handle, CurrentRootNode.Name));
            else
            {
                var autoResetEvent = new AutoResetEvent(false);
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    SetView(UiService.GetViewMode(CurrentRootNode.Handle, CurrentRootNode.Name));
                    autoResetEvent.Set();
                });
                autoResetEvent.WaitOne();
            }

            // Create the possibility to cancel the loadnodes task
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;

            Task.Factory.StartNew(() =>
            {
                //var stopwatch = new Stopwatch();
                //stopwatch.Start();

                int viewport = 0;
                int background = 0;

                // Each view has different performance options
                switch (ViewMode)
                {
                    case ViewMode.ListView:
                        viewport = 20;
                        background = 20;
                        break;
                    case ViewMode.LargeThumbnails:
                        viewport = 24;
                        background = 24;
                        break;
                    case ViewMode.SmallThumbnails:
                        viewport = 60;
                        background = 60;
                        break;
                }


                var helperList = new List<NodeViewModel>((int) ViewMode);
                for (int i = 0; i < listSize; i++)
                {
                    if (cancellationToken.IsCancellationRequested) return;
                    
                    var node = new NodeViewModel(this.MegaSdk, nodeList.get(i), ChildNodes);

                    if (DriveDisplayMode == DriveDisplayMode.MoveItem && FocusedNode != null &&
                        node.GetMegaNode().getBase64Handle() == FocusedNode.GetMegaNode().getBase64Handle())
                    {
                        node.DisplayMode = NodeDisplayMode.SelectedForMove;
                        FocusedNode = node;
                    }

                    helperList.Add(node);

                    if (i == viewport)
                    {
                        var waitHandleNodes = new AutoResetEvent(false);
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            helperList.ForEach(n =>
                            {
                                if (cancellationToken.IsCancellationRequested) return;
                                ChildNodes.Add(n);
                            });
                            waitHandleNodes.Set();
                        });
                        waitHandleNodes.WaitOne();

                        // Remove the busy indication
                        var waitHandleProgress = new AutoResetEvent(false);
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            ProgessService.SetProgressIndicator(false);
                            waitHandleProgress.Set();
                        });
                        waitHandleProgress.WaitOne();

                        helperList.Clear();
                    }
                    else if (helperList.Count == background && i > viewport)
                    {
                        EventWaitHandle waitHandleProgress = new AutoResetEvent(false);
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            helperList.ForEach(n =>
                            {
                                if (cancellationToken.IsCancellationRequested) return;
                                ChildNodes.Add(n);
                            });
                            waitHandleProgress.Set();
                        });
                        waitHandleProgress.WaitOne();
                        helperList.Clear();
                    }
                }

                var autoResetEvent = new AutoResetEvent(false);
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    helperList.ForEach(n =>
                    {
                        if (cancellationToken.IsCancellationRequested) return;
                        ChildNodes.Add(n);
                    });
                    autoResetEvent.Set();
                });
                autoResetEvent.WaitOne();


                //stopwatch.Stop();
                //Deployment.Current.Dispatcher.BeginInvoke(() =>
                //    MessageBox.Show(stopwatch.Elapsed.ToString("g")));
               
                Deployment.Current.Dispatcher.BeginInvoke(() => ProgessService.SetProgressIndicator(false));

            }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Current);
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
            var fetchNodesRequestListener = new FetchNodesRequestListener(this, rootRefreshNode, ShortCutHandle);
            ShortCutHandle = null;
            this.MegaSdk.fetchNodes(fetchNodesRequestListener);
        }

        public void SelectFolder(NodeViewModel selectedNode)
        {
            if (cancellationToken.CanBeCanceled)
                cancellationTokenSource.Cancel();

            this.ChildNodes.Clear();

            this.CurrentRootNode = selectedNode;
            this.CurrentRootNode.ChildCollection = ChildNodes;

            // Create unique uri string to navigate
            NavigateService.NavigateTo(typeof(MainPage), NavigationParameter.Browsing, new Dictionary<string, string> {{"Id", Guid.NewGuid().ToString("N")}});
        }

        public void CalculateBreadCrumbs(NodeViewModel currentRootNode)
        {
            this.BreadCrumbs.Clear();

            if (currentRootNode.Type == MNodeType.TYPE_ROOT || currentRootNode.Type == MNodeType.TYPE_RUBBISH) return;

            this.BreadCrumbs.Add(currentRootNode);

            MNode parentNode = currentRootNode.GetMegaNode();
            parentNode = this.MegaSdk.getParentNode(parentNode);
            while ((parentNode.getType() != MNodeType.TYPE_ROOT) && (parentNode.getType() != MNodeType.TYPE_RUBBISH))
            {
                this.BreadCrumbs.Insert(0, new NodeViewModel(this.MegaSdk, parentNode));
                parentNode = this.MegaSdk.getParentNode(parentNode);
            }

        }

        #endregion

        #region Private Methods
       

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
            FocusedNode.Remove(false);
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
        public DriveDisplayMode OldDriveDisplayMode { get; set; }

        public ulong? ShortCutHandle { get; set; }

        public ViewMode ViewMode { get; set; }

        private VirtualizationStrategyDefinition _virtualizationStrategy;
        public VirtualizationStrategyDefinition VirtualizationStrategy
        {
            get { return _virtualizationStrategy; }
            set
            {
                _virtualizationStrategy = value;
                OnPropertyChanged("VirtualizationStrategy");
            }
        }

        private DataTemplateSelector _nodeTemplateSelector;
        public DataTemplateSelector NodeTemplateSelector
        {
             get { return _nodeTemplateSelector; }
            set
            {
                _nodeTemplateSelector = value;
                OnPropertyChanged("NodeTemplateSelector");
            }
        }

        private Uri _viewStateButtonIconUri;
        public Uri ViewStateButtonIconUri
        {
            get { return _viewStateButtonIconUri; }
            set
            {
                _viewStateButtonIconUri = value;
                OnPropertyChanged("ViewStateButtonIconUri");
            }
        }

        private Style _multiSelectCheckBoxStyle;
        public Style MultiSelectCheckBoxStyle
        {
            get { return _multiSelectCheckBoxStyle; }
            set
            {
                _multiSelectCheckBoxStyle = value;
                OnPropertyChanged("MultiSelectCheckBoxStyle");
            }
        }

        private bool _isMultiSelectActive;
        public bool IsMultiSelectActive
        {
            get { return _isMultiSelectActive; }
            set
            {
                _isMultiSelectActive = value;
                OnPropertyChanged("IsMultiSelectActive");
            }
        }



             

        #endregion
      
    }
}
