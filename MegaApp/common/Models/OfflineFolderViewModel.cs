using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows.Controls;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Extensions;
using MegaApp.Interfaces;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.Models
{
    /// <summary>
    /// Class that handles all process and operations of a section that contains Offline nodes
    /// </summary>
    public class OfflineFolderViewModel : BaseViewModel
    {
        public OfflineFolderViewModel() : base()
        {
            this.Type = ContainerType.Offline;

            this.FolderRootNode = null;
            this.IsBusy = false;
            this.BusyText = null;
            this.ChildNodes = new ObservableCollection<IOfflineNode>();
            this.BreadCrumbs = new ObservableCollection<IBaseNode>();
            this.SelectedNodes = new List<IOfflineNode>();
            this.IsMultiSelectActive = false;

            this.ChangeViewCommand = new DelegateCommand(this.ChangeView);
            this.MultiSelectCommand = new DelegateCommand(this.MultiSelect);

            SetViewDefaults();

            SetEmptyContentTemplate(true);

            this.CurrentDisplayMode = DriveDisplayMode.SavedForOffline;
        }

        #region Commands

        public ICommand ChangeViewCommand { get; private set; }
        public ICommand MultiSelectCommand { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns boolean value to indicatie if the current folder view has any child nodes
        /// </summary>
        /// <returns>True if there are child nodes, False if child node count is zero</returns>
        public bool HasChildNodes()
        {
            return ChildNodes.Count > 0;
        }

        /// <summary>
        /// Load the nodes for this specific folder
        /// </summary>
        public void LoadChildNodes()
        {
            // First cancel any other loading task that is busy
            CancelLoad();

            // FolderRootNode should not be null
            if (FolderRootNode == null)
            {
                OnUiThread(() =>
                {
                    new CustomMessageDialog(
                            AppMessages.LoadNodesFailed_Title,
                            AppMessages.LoadNodesFailed,
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialog();
                });
                return;
            }

            SetProgressIndication(true);

            // Process is started so we can set the empty content template to loading already
            SetEmptyContentTemplate(true);            

            // Clear the child nodes to make a fresh start
            ClearChildNodes();

            // Set the correct view for the main drive. Do this after the childs are cleared to speed things up
            SetViewOnLoad();

            // Build the bread crumbs. Do this before loading the nodes so that the user can click on home
            OnUiThread(BuildBreadCrumbs);

            // Create the option to cancel
            CreateLoadCancelOption();

            // Load and create the childnodes for the folder
            Task.Factory.StartNew(() =>
            {
                try
                {
                    ObservableCollection<IOfflineNode> tempChildNodes = new ObservableCollection<IOfflineNode>();

                    String[] childFolders = Directory.GetDirectories(FolderRootNode.NodePath);
                    foreach (var folder in childFolders)
                    {
                        var childNode = new OfflineFolderNodeViewModel(new DirectoryInfo(folder));
                        if (childNode == null) continue;

                        Deployment.Current.Dispatcher.BeginInvoke(() => tempChildNodes.Add(childNode));
                    }
                                        
                    String[] childFiles = Directory.GetFiles(FolderRootNode.NodePath);
                    foreach (var file in childFiles)
                    {
                        var fileInfo = new FileInfo(file);

                        if (FileService.IsPendingTransferFile(fileInfo.Name)) continue;

                        var childNode = new OfflineFileNodeViewModel(fileInfo);
                        if (childNode == null) continue;

                        Deployment.Current.Dispatcher.BeginInvoke(() => tempChildNodes.Add(childNode));
                    }

                    OrderChildNodes(tempChildNodes);
                    
                    // Show the user that processing the childnodes is done
                    SetProgressIndication(false);

                    // Set empty content to folder instead of loading view
                    SetEmptyContentTemplate(false);
                }
                catch (OperationCanceledException)
                {
                    // Do nothing. Just exit this background process because a cancellation exception has been thrown
                }

            }, LoadingCancelToken, TaskCreationOptions.PreferFairness, TaskScheduler.Current);
        }        

        /// <summary>
        /// Cancel any running load process of this folder
        /// </summary>
        public void CancelLoad()
        {
            if (this.LoadingCancelTokenSource != null && LoadingCancelToken.CanBeCanceled)
                LoadingCancelTokenSource.Cancel();
        }

        /// <summary>
        /// Refresh the current folder.
        /// </summary>
        public void Refresh()
        {
            if(this.FolderRootNode == null)            
                this.FolderRootNode = new OfflineFolderNodeViewModel(new DirectoryInfo(AppService.GetDownloadDirectoryPath()));

            this.LoadChildNodes();
        }

        public void OnChildNodeTapped(IOfflineNode node)
        {
            if(node.IsFolder)
                BrowseToFolder(node);
            else
                ProcessFileNode(node);
        }

        public void SetView(ViewMode viewMode)
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

                        this.NodeTemplateSelector = new OfflineNodeTemplateSelector()
                        {
                            FileItemTemplate = (DataTemplate)Application.Current.Resources["OfflineNodeListLargeViewFileItemContent"],
                            FolderItemTemplate = (DataTemplate)Application.Current.Resources["OfflineNodeListLargeViewFolderItemContent"]
                        };

                        this.ViewMode = ViewMode.LargeThumbnails;
                        this.ViewStateButtonPathData = VisualResources.LargeThumbnailViewPathData;
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

                        this.NodeTemplateSelector = new OfflineNodeTemplateSelector()
                        {
                            FileItemTemplate = (DataTemplate)Application.Current.Resources["OfflineNodeListSmallViewFileItemContent"],
                            FolderItemTemplate = (DataTemplate)Application.Current.Resources["OfflineNodeListSmallViewFolderItemContent"]
                        };

                        this.ViewMode = ViewMode.SmallThumbnails;
                        this.ViewStateButtonPathData = VisualResources.SmallThumbnailViewPathData;
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

        public void SetEmptyContentTemplate(bool isLoading)
        {
            if (isLoading)
            {
                OnUiThread(() => this.EmptyContentTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListLoadingContent"]);
            }
            else
            {
                OnUiThread(() =>
                    this.EmptyContentTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListSavedForOfflineEmptyContent"]);
            }
        }

        public virtual bool GoFolderUp()
        {
            if (this.FolderRootNode == null || FolderService.IsOfflineRootFolder(FolderRootNode.NodePath)) 
                return false;

            DirectoryInfo parentNode = (new DirectoryInfo(this.FolderRootNode.NodePath)).Parent;

            if (parentNode == null) return false;

            this.FolderRootNode = new OfflineFolderNodeViewModel(parentNode, ChildNodes);

            LoadChildNodes();

            return true;
        }

        public virtual void BrowseToHome()
        {
            if (this.FolderRootNode == null) return;

            DirectoryInfo homeNode = new DirectoryInfo(AppService.GetDownloadDirectoryPath());

            if (homeNode == null) return;

            this.FolderRootNode = new OfflineFolderNodeViewModel(homeNode, ChildNodes);                

            LoadChildNodes();
        }

        public void BrowseToFolder(IOfflineNode node)
        {
            if (node == null) return;

            this.FolderRootNode = node;

            LoadChildNodes();
        }

        public void ProcessFileNode(IOfflineNode node)
        {
            this.FocusedNode = node;

            //if (node.IsImage)
            //    NavigateService.NavigateTo(typeof(PreviewImagePage), NavigationParameter.Normal, this);
            //else
            //    this.FocusedNode.Download(App.MegaTransfers);
        }

        #endregion

        #region Private Methods

        private void MultiSelect(object obj)
        {
            this.IsMultiSelectActive = !this.IsMultiSelectActive;
        }

        private void ChangeView(object obj)
        {
            if (FolderRootNode == null) return;

            switch (this.ViewMode)
            {
                case ViewMode.ListView:
                    {
                        SetView(ViewMode.LargeThumbnails);
                        UiService.SetViewMode(FolderRootNode.Base64Handle, ViewMode.LargeThumbnails);
                        break;
                    }
                case ViewMode.LargeThumbnails:
                    {
                        SetView(ViewMode.SmallThumbnails);
                        UiService.SetViewMode(FolderRootNode.Base64Handle, ViewMode.SmallThumbnails);
                        break;
                    }
                case ViewMode.SmallThumbnails:
                    {
                        SetView(ViewMode.ListView);
                        UiService.SetViewMode(FolderRootNode.Base64Handle, ViewMode.ListView);
                        break;
                    }
            }
        }

        private void SetProgressIndication(bool onOff, string busyText = null)
        {
            OnUiThread(() =>
            {
                this.IsBusy = onOff;
                this.BusyText = busyText;
            });
        }

        private void CreateLoadCancelOption()
        {
            if (this.LoadingCancelTokenSource != null)
            {
                this.LoadingCancelTokenSource.Dispose();
                this.LoadingCancelTokenSource = null;
            }
            this.LoadingCancelTokenSource = new CancellationTokenSource();
            this.LoadingCancelToken = LoadingCancelTokenSource.Token;
        }

        private void ClearChildNodes()
        {
            if (ChildNodes == null) return;

            OnUiThread(() =>
            {
                this.ChildNodes.Clear();                
            });
        }

        private void OrderChildNodes(ObservableCollection<IOfflineNode> nodes)
        {
            OnUiThread(() =>
            {
                IOrderedEnumerable<IOfflineNode> orderedNodes;

                switch (UiService.GetSortOrder(FolderRootNode.Base64Handle, FolderRootNode.Name))
                {
                    case (int)MSortOrderType.ORDER_ALPHABETICAL_ASC:
                        orderedNodes = nodes.OrderBy(node => node.Name);
                        break;
                    case (int)MSortOrderType.ORDER_ALPHABETICAL_DESC:
                        orderedNodes = nodes.OrderByDescending(node => node.Name);
                        break;
                    case (int)MSortOrderType.ORDER_CREATION_ASC:
                        orderedNodes = nodes.OrderBy(node => node.CreationTime);
                        break;
                    case (int)MSortOrderType.ORDER_CREATION_DESC:
                        orderedNodes = nodes.OrderByDescending(node => node.CreationTime);
                        break;
                    case (int)MSortOrderType.ORDER_MODIFICATION_ASC:
                        orderedNodes = nodes.OrderBy(node => node.ModificationTime);
                        break;
                    case (int)MSortOrderType.ORDER_MODIFICATION_DESC:
                        orderedNodes = nodes.OrderByDescending(node => node.ModificationTime);
                        break;
                    case (int)MSortOrderType.ORDER_SIZE_ASC:
                        orderedNodes = nodes.OrderBy(node => node.Size);
                        break;
                    case (int)MSortOrderType.ORDER_SIZE_DESC:
                        orderedNodes = nodes.OrderByDescending(node => node.Size);
                        break;
                    case (int)MSortOrderType.ORDER_DEFAULT_DESC:
                        orderedNodes = nodes.OrderBy(node => node.IsFolder);
                        break;
                    case (int)MSortOrderType.ORDER_DEFAULT_ASC:
                    case (int)MSortOrderType.ORDER_NONE:
                    default:
                        orderedNodes = nodes.OrderByDescending(node => node.IsFolder);
                        break;
                }

                ChildNodes = new ObservableCollection<IOfflineNode>(orderedNodes);
            });            
        }

        private void SetViewOnLoad()
        {
            if (FolderRootNode == null) return;

            OnUiThread(() =>
            {
                SetView(UiService.GetViewMode(FolderRootNode.Base64Handle, FolderRootNode.Name));
            });
        }

        private void SetViewDefaults()
        {
            this.VirtualizationStrategy = new StackVirtualizationStrategyDefinition()
            {
                Orientation = Orientation.Vertical
            };

            this.NodeTemplateSelector = new OfflineNodeTemplateSelector()
            {
                FileItemTemplate = (DataTemplate)Application.Current.Resources["OfflineNodeListFileItemContent"],
                FolderItemTemplate = (DataTemplate)Application.Current.Resources["OfflineNodeListFolderItemContent"]
            };

            this.ViewMode = ViewMode.ListView;
            this.ViewStateButtonPathData = VisualResources.ListViewPathData;
            this.MultiSelectCheckBoxStyle = (Style)Application.Current.Resources["DefaultCheckBoxStyle"];
        }

        public void BuildBreadCrumbs()
        {
            this.BreadCrumbs.Clear();

            // Top root nodes have no breadcrumbs
            if (this.FolderRootNode == null ||
                FolderService.IsOfflineRootFolder(this.FolderRootNode.NodePath)) return;

            this.BreadCrumbs.Add((IBaseNode)this.FolderRootNode);

            DirectoryInfo parentNode = (new DirectoryInfo(this.FolderRootNode.NodePath)).Parent;
            while ((parentNode != null) && !FolderService.IsOfflineRootFolder(parentNode.FullName))
            {
                this.BreadCrumbs.Insert(0, (IBaseNode)new OfflineFolderNodeViewModel(parentNode));
                parentNode = (new DirectoryInfo(parentNode.FullName)).Parent;
            }
        }

        #endregion

        #region IBreadCrumb

        public ObservableCollection<IBaseNode> BreadCrumbs { get; private set; }

        #endregion

        #region Properties

        public IOfflineNode FocusedNode { get; set; }
        public DriveDisplayMode CurrentDisplayMode { get; set; }
        public DriveDisplayMode PreviousDisplayMode { get; set; }
        public List<IOfflineNode> SelectedNodes { get; set; }

        private ObservableCollection<IOfflineNode> _childNodes;
        public ObservableCollection<IOfflineNode> ChildNodes
        {
            get { return _childNodes; }
            set { SetField(ref _childNodes, value); }
        }

        //private IOrderedEnumerable<IOfflineNode> _orderedChildNodes;
        //public IOrderedEnumerable<IOfflineNode> OrderedChildNodes
        //{
        //    get { return _orderedChildNodes; }
        //    set { SetField(ref _orderedChildNodes, value); }
        //}

        public ContainerType Type { get; private set; }

        public ViewMode ViewMode { get; set; }

        private IOfflineNode _folderRootNode;
        public IOfflineNode FolderRootNode
        {
            get { return _folderRootNode; }
            set { SetField(ref _folderRootNode, value); }
        }

        private CancellationTokenSource LoadingCancelTokenSource { get; set; }
        private CancellationToken LoadingCancelToken { get; set; }

        private VirtualizationStrategyDefinition _virtualizationStrategy;
        public VirtualizationStrategyDefinition VirtualizationStrategy
        {
            get { return _virtualizationStrategy; }
            private set { SetField(ref _virtualizationStrategy, value); }
        }

        private string _viewStateButtonPathData;
        public string ViewStateButtonPathData
        {
            get { return _viewStateButtonPathData; }
            set { SetField(ref _viewStateButtonPathData, value); }
        }

        private DataTemplateSelector _nodeTemplateSelector;
        public DataTemplateSelector NodeTemplateSelector
        {
            get { return _nodeTemplateSelector; }
            private set { SetField(ref _nodeTemplateSelector, value); }
        }

        private Style _multiSelectCheckBoxStyle;
        public Style MultiSelectCheckBoxStyle
        {
            get { return _multiSelectCheckBoxStyle; }
            private set { SetField(ref _multiSelectCheckBoxStyle, value); }
        }

        private bool _isMultiSelectActive;
        public bool IsMultiSelectActive
        {
            get { return _isMultiSelectActive; }
            private set { SetField(ref _isMultiSelectActive, value); }
        }

        private DataTemplate _emptyContentTemplate;
        public DataTemplate EmptyContentTemplate
        {
            get { return _emptyContentTemplate; }
            private set { SetField(ref _emptyContentTemplate, value); }
        }

        private string _busyText;
        public string BusyText
        {
            get { return _busyText; }
            private set
            {
                SetField(ref _busyText, value);
                HasBusyText = !String.IsNullOrEmpty(_busyText) && !String.IsNullOrWhiteSpace(_busyText);
            }
        }

        private bool _hasBusyText;
        public bool HasBusyText
        {
            get { return _hasBusyText; }
            private set { SetField(ref _hasBusyText, value); }
        }

        #endregion
    }
}
