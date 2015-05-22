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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows.Controls;
using Windows.Storage;

namespace MegaApp.Models
{
    /// <summary>
    /// Class that handles all process and operations of a section that contains MEGA nodes
    /// </summary>
    public class FolderViewModel : BaseAppInfoAwareViewModel, IBreadCrumb
    {
        public FolderViewModel(MegaSDK megaSdk, AppInformation appInformation, ContainerType containerType)
            : base(megaSdk, appInformation)
        {
            this.Type = containerType;

            this.FolderRootNode = null;
            this.IsBusy = false;
            this.BusyText = null;
            this.ChildNodes = new ObservableCollection<IMegaNode>();
            this.BreadCrumbs = new ObservableCollection<IMegaNode>();
            this.SelectedNodes = new List<IMegaNode>();
            this.IsMultiSelectActive = false;

            ////FolderRootNode depending on the container type
            //switch (this.Type)
            //{
            //    case ContainerType.RubbishBin:
            //        this.FolderRootNode = NodeService.CreateNew(this.MegaSdk, this.AppInformation, this.MegaSdk.getRubbishNode());
            //        break;
            //    case ContainerType.CloudDrive:            
            //        this.FolderRootNode = NodeService.CreateNew(this.MegaSdk, this.AppInformation, this.MegaSdk.getRootNode());
            //        break;
            //}

            this.RemoveItemCommand = new DelegateCommand(this.RemoveItem);
            this.RenameItemCommand = new DelegateCommand(this.RenameItem);
            this.DownloadItemCommand = new DelegateCommand(this.DownloadItem);
            this.CreateShortCutCommand = new DelegateCommand(this.CreateShortCut);
            this.ChangeViewCommand = new DelegateCommand(this.ChangeView);
            this.GetLinkCommand = new DelegateCommand(this.GetLink);
            this.MultiSelectCommand = new DelegateCommand(this.MultiSelect);

            SetViewDefaults();

            SetEmptyContentTemplate(true);

            switch (containerType)
            {
                case ContainerType.CloudDrive:
                    this.CurrentDisplayMode = DriveDisplayMode.CloudDrive;
                    break;
                case ContainerType.RubbishBin:
                    this.CurrentDisplayMode = DriveDisplayMode.RubbishBin;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("containerType");
            }
        }

        #region Commands

        public ICommand ChangeViewCommand { get; private set; }
        public ICommand GetLinkCommand { get; private set; }
        public ICommand RenameItemCommand { get; private set; }
        public ICommand RemoveItemCommand { get; private set; }
        public ICommand DownloadItemCommand { get; private set; }
        public ICommand CreateShortCutCommand { get; private set; }
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
        /// Load the mega nodes for this specific folder using the Mega SDK
        /// </summary>
        public void LoadChildNodes()
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return;

            // First cancel any other loading task that is busy
            CancelLoad();

            // FolderRootNode should not be null
            if (FolderRootNode == null)
            {
                OnUiThread(() => MessageBox.Show(AppMessages.LoadNodesFailed, AppMessages.LoadNodesFailed_Title, MessageBoxButton.OK));
                return;
            }

            SetProgressIndication(true);

            // Process is started so we can set the empty content template to loading already
            SetEmptyContentTemplate(true);

            // Get the MNodes from the Mega SDK in the correct sorting order for the current folder
            MNodeList childList = NodeService.GetChildren(this.MegaSdk, this.FolderRootNode);

            if (childList == null)
            {
                OnUiThread(() =>
                {
                    MessageBox.Show(AppMessages.LoadNodesFailed, AppMessages.LoadNodesFailed_Title, MessageBoxButton.OK);
                    SetEmptyContentTemplate(false);
                });

                return;
            }

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
                    CreateChildren(childList, childList.size());
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
        /// Refresh the current folder. Delete cached thumbnails and reload the nodes
        /// </summary>
        public void Refresh()
        {
            FileService.ClearFiles(
             NodeService.GetFiles(this.ChildNodes,
                Path.Combine(ApplicationData.Current.LocalFolder.Path,
                AppResources.ThumbnailsDirectory)));

            if(this.FolderRootNode == null)
            {
                switch (this.Type)
                {
                    case ContainerType.RubbishBin:
                        this.FolderRootNode = NodeService.CreateNew(this.MegaSdk, this.AppInformation, this.MegaSdk.getRubbishNode());
                        break;
                    case ContainerType.CloudDrive:                    
                        this.FolderRootNode = NodeService.CreateNew(this.MegaSdk, this.AppInformation, this.MegaSdk.getRootNode());
                        break;
                }
            }            

            this.LoadChildNodes();
        }

        public async void AddFolder()
        {
            if (!IsUserOnline()) return;

            // Only 1 RadInputPrompt can be open at the same time with ShowAsync.
            if (this.AppInformation.PickerOrAsyncDialogIsOpen) return;

            try
            {
                this.AppInformation.PickerOrAsyncDialogIsOpen = true;

                var inputPromptClosedEventArgs = await RadInputPrompt.ShowAsync(
                    new[] { UiResources.Add.ToLower(), UiResources.Cancel.ToLower() }, UiResources.CreateFolder);

                this.AppInformation.PickerOrAsyncDialogIsOpen = false;

                if (inputPromptClosedEventArgs == null || inputPromptClosedEventArgs.Result != DialogResult.OK) return;

                this.MegaSdk.createFolder(inputPromptClosedEventArgs.Text, this.FolderRootNode.OriginalMNode,
                    new CreateFolderRequestListener());
            }
            catch (Exception)
            {
                MessageBox.Show(AppMessages.FolderCreateFailed, AppMessages.FolderCreateFailed_Title,
                    MessageBoxButton.OK);
            }
            finally
            {
                this.AppInformation.PickerOrAsyncDialogIsOpen = false;
            }
        }
        public async void OpenLink()
        {
            if (!IsUserOnline()) return;

            // Only 1 RadInputPrompt can be open at the same time with ShowAsync.
            if (this.AppInformation.PickerOrAsyncDialogIsOpen) return;

            this.AppInformation.PickerOrAsyncDialogIsOpen = true;

            var inputPromptClosedEventArgs = await RadInputPrompt.ShowAsync(
                new[] { UiResources.Open, UiResources.Cancel }, UiResources.OpenLink);

            this.AppInformation.PickerOrAsyncDialogIsOpen = false;

            if (inputPromptClosedEventArgs.Result != DialogResult.OK) return;

            this.MegaSdk.getPublicNode(inputPromptClosedEventArgs.Text, new GetPublicNodeRequestListener(this));
        }

        public void ImportLink(string link)
        {
            if (String.IsNullOrEmpty(link) || String.IsNullOrWhiteSpace(link)) return;

            this.MegaSdk.importFileLink(
                link,
                this.FolderRootNode.OriginalMNode,
                new ImportFileRequestListener());

            //LinkToImport = null;
        }

        public void DownloadLink(MNode publicNode)
        {
            // Create a temporary DownloadNodeViewModel from the public Node created from the link
            var downloadNode = NodeService.CreateNew(this.MegaSdk, this.AppInformation, publicNode);
            downloadNode.Download(App.MegaTransfers);
        }

        public void OnChildNodeTapped(IMegaNode node)
        {
            switch (node.Type)
            {
                case MNodeType.TYPE_UNKNOWN:
                    break;
                case MNodeType.TYPE_FILE:
                    ProcessFileNode(node);
                    break;
                case MNodeType.TYPE_FOLDER:
                    BrowseToFolder(node);
                    break;
                case MNodeType.TYPE_ROOT:
                    break;
                case MNodeType.TYPE_INCOMING:
                    break;
                case MNodeType.TYPE_RUBBISH:
                    break;
                case MNodeType.TYPE_MAIL:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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

                        this.NodeTemplateSelector = new NodeTemplateSelector()
                        {
                            FileItemTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListLargeViewFileItemContent"],
                            FolderItemTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListLargeViewFolderItemContent"]
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

                        this.NodeTemplateSelector = new NodeTemplateSelector()
                        {
                            FileItemTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListSmallViewFileItemContent"],
                            FolderItemTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListSmallViewFolderItemContent"]
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
                var megaRoot = this.MegaSdk.getRootNode();

                if (this.FolderRootNode != null && megaRoot != null && this.FolderRootNode.Handle.Equals(megaRoot.getHandle()))
                {
                    OnUiThread(() =>
                        this.EmptyContentTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListCloudDriveEmptyContent"]);
                }
                else
                {
                    OnUiThread(() =>
                        this.EmptyContentTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListEmptyContent"]);
                }
            }
        }

        public bool GoFolderUp()
        {
            if (this.FolderRootNode == null) return false;

            MNode parentNode = this.MegaSdk.getParentNode(this.FolderRootNode.OriginalMNode);

            if (parentNode == null || parentNode.getType() == MNodeType.TYPE_UNKNOWN)
                return false;

            this.FolderRootNode = NodeService.CreateNew(this.MegaSdk, this.AppInformation, parentNode, ChildNodes);

            LoadChildNodes();

            return true;
        }

        public void BrowseToHome()
        {
            if (this.FolderRootNode == null) return;

            MNode homeNode = null;

            switch (Type)
            {
                case ContainerType.CloudDrive:
                    homeNode = this.MegaSdk.getRootNode();
                    break;
                case ContainerType.RubbishBin:
                    homeNode = this.MegaSdk.getRubbishNode();
                    break;
            }

            if (homeNode == null) return;

            this.FolderRootNode = NodeService.CreateNew(this.MegaSdk, this.AppInformation, homeNode, ChildNodes);

            LoadChildNodes();
        }

        public void BrowseToFolder(IMegaNode node)
        {
            if (node == null) return;

            this.FolderRootNode = node;

            LoadChildNodes();
        }

        public void ProcessFileNode(IMegaNode node)
        {
            this.FocusedNode = node;

            if (node.IsImage)
                NavigateService.NavigateTo(typeof(PreviewImagePage), NavigationParameter.Normal, this);
            else
                this.FocusedNode.Download(App.MegaTransfers);
        }

        public async void MultipleDownload(StorageFolder downloadFolder = null)
        {
            int count = ChildNodes.Count(n => n.IsMultiSelected);

            if (count < 1) return;

            // Only 1 Folder Picker can be open at 1 time
            if (this.AppInformation.PickerOrAsyncDialogIsOpen) return;

            #if WINDOWS_PHONE_80
            if (!SettingsService.LoadSetting<bool>(SettingsResources.QuestionAskedDownloadOption, false))
            {
                switch (await DialogService.ShowOptionsDialog(AppMessages.QuestionAskedDownloadOption_Title, 
                    AppMessages.QuestionAskedDownloadOption,
                    new[] { AppMessages.QuestionAskedDownloadOption_YesButton, AppMessages.QuestionAskedDownloadOption_NoButton }))
                {
                    case -1:
                    {
                        return;
                    }
                    case 0:
                    {
                        SettingsService.SaveSetting(SettingsResources.ExportImagesToPhotoAlbum, true);
                        break;
                    }
                    case 1:
                    {
                        SettingsService.SaveSetting(SettingsResources.ExportImagesToPhotoAlbum, false);
                        break;
                    }
                }
                SettingsService.SaveSetting(SettingsResources.QuestionAskedDownloadOption, true);
            }
            #elif WINDOWS_PHONE_81
            if (downloadFolder == null)
            {
                this.AppInformation.PickerOrAsyncDialogIsOpen = true;
                if (!await FolderService.SelectDownloadFolder()) return;
            }
            #endif

            ProgressService.SetProgressIndicator(true, ProgressMessages.PrepareDownloads);

            // Give the app the time to display the progress indicator
            await Task.Delay(5);

            // First count the number of downloads before proceeding to the transfers.
            int downloadCount = 0;
            var downloadNodes = new List<IMegaNode>();

            foreach (var node in ChildNodes.Where(n => n.IsMultiSelected))
            {
                // If selected file is a folder then also select it childnodes to download
                var folderNode = node as FolderNodeViewModel;
                if (folderNode != null)
                {
                    List<NodeViewModel> recursiveNodes = NodeService.GetRecursiveNodes(MegaSdk, AppInformation, folderNode);
                    foreach (var recursiveNode in recursiveNodes)
                    {
                        downloadNodes.Add(recursiveNode);
                        downloadCount++;
                    }
                }
                else
                {
                    // No folder then just add node to transferlist
                    downloadNodes.Add(node);
                    downloadCount++;
                }

            }

            if (!AppService.DownloadLimitCheck(downloadCount))
            {
                ProgressService.SetProgressIndicator(false);
                return;
            }

            downloadNodes.ForEach(n =>
            {
                if (downloadFolder != null)
                    n.Transfer.DownloadFolderPath = downloadFolder.Path;
                App.MegaTransfers.Add(n.Transfer);
                n.Transfer.StartTransfer();
            });

            ProgressService.SetProgressIndicator(false);

            this.IsMultiSelectActive = false;
            this.NoFolderUpAction = true;
            NavigateService.NavigateTo(typeof(TransferPage), NavigationParameter.Downloads);
        }

        public bool SelectMultipleItemsForMove()
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
            this.PreviousDisplayMode = this.CurrentDisplayMode;
            this.CurrentDisplayMode = DriveDisplayMode.MoveItem;

            return true;
        }

        public bool MultipleRemoveItems()
        {
            int count = ChildNodes.Count(n => n.IsMultiSelected);

            if (count < 1) return false;

            if (this.PreviousDisplayMode == DriveDisplayMode.RubbishBin)
            {
                if (MessageBox.Show(String.Format(AppMessages.MultiSelectRemoveQuestion, count),
                    AppMessages.MultiSelectRemoveQuestion_Title, MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) return false;

                Deployment.Current.Dispatcher.BeginInvoke(() => ProgressService.SetProgressIndicator(true, ProgressMessages.RemoveNode));
            }
            else
            {
                if (MessageBox.Show(String.Format(AppMessages.MultiMoveToRubbishBinQuestion, count),
                    AppMessages.MultiMoveToRubbishBinQuestion_Title, MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) return false;

                Deployment.Current.Dispatcher.BeginInvoke(() => ProgressService.SetProgressIndicator(true, ProgressMessages.NodeToTrash));
            }

            var helperList = new List<IMegaNode>(count);
            foreach (var node in ChildNodes.Where(n => n.IsMultiSelected))
                helperList.Add(node);

            Task.Run(() =>
            {
                AutoResetEvent[] waitEventRequests = new AutoResetEvent[count];

                int index = 0;

                foreach (var node in helperList)
                {
                    waitEventRequests[index] = new AutoResetEvent(false);
                    node.Remove(true, waitEventRequests[index]);
                    index++;
                }

                WaitHandle.WaitAll(waitEventRequests);

                Deployment.Current.Dispatcher.BeginInvoke(() => ProgressService.SetProgressIndicator(false));

                if (this.PreviousDisplayMode == DriveDisplayMode.RubbishBin)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show(String.Format(AppMessages.MultiRemoveSucces, count),
                            AppMessages.MultiRemoveSucces_Title, MessageBoxButton.OK);
                    });
                }
                else
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show(String.Format(AppMessages.MultiMoveToRubbishBinSucces, count),
                            AppMessages.MultiMoveToRubbishBinSucces_Title, MessageBoxButton.OK);
                    });
                }
            });

            this.IsMultiSelectActive = false;

            return true;
        }

        #endregion

        #region Private Methods

        private void MultiSelect(object obj)
        {
            this.IsMultiSelectActive = !this.IsMultiSelectActive;
        }

        private void RemoveItem(object obj)
        {
            FocusedNode.Remove(false);
        }

        private void RenameItem(object obj)
        {
            FocusedNode.Rename();
        }

        private void GetLink(object obj)
        {
            if (!IsUserOnline()) return;

            FocusedNode.GetLink();
        }

        private void DownloadItem(object obj)
        {
            this.NoFolderUpAction = true;
            FocusedNode.Download(App.MegaTransfers);
        }

        private void CreateShortCut(object obj)
        {
            var shortCutTile = new RadIconicTileData()
            {
                IconImage = new Uri("/Assets/Tiles/FolderIconImage.png", UriKind.Relative),
                SmallIconImage = new Uri("/Assets/Tiles/FolderSmallIconImage.png", UriKind.Relative),
                Title = FocusedNode.Name
            };

            LiveTileHelper.CreateOrUpdateTile(shortCutTile,
                new Uri("/Pages/MainPage.xaml?ShortCutHandle=" + FocusedNode.OriginalMNode.getHandle(), UriKind.Relative),
                false);
        }

        private void ChangeView(object obj)
        {
            if (FolderRootNode == null) return;

            switch (this.ViewMode)
            {
                case ViewMode.ListView:
                    {
                        SetView(ViewMode.LargeThumbnails);
                        UiService.SetViewMode(FolderRootNode.Handle, ViewMode.LargeThumbnails);
                        break;
                    }
                case ViewMode.LargeThumbnails:
                    {
                        SetView(ViewMode.SmallThumbnails);
                        UiService.SetViewMode(FolderRootNode.Handle, ViewMode.SmallThumbnails);
                        break;
                    }
                case ViewMode.SmallThumbnails:
                    {
                        SetView(ViewMode.ListView);
                        UiService.SetViewMode(FolderRootNode.Handle, ViewMode.ListView);
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

        private void CreateChildren(MNodeList childList, int listSize)
        {
            // Set the parameters for the performance for the different view types of a folder
            int viewportItemCount, backgroundItemCount;
            InitializePerformanceParameters(out viewportItemCount, out backgroundItemCount);

            // We will not add nodes one by one in the dispatcher but in groups
            var helperList = new List<IMegaNode>(1024);

            for (int i = 0; i < listSize; i++)
            {
                // If the task has been cancelled, stop processing
                if (LoadingCancelToken.IsCancellationRequested)
                    LoadingCancelToken.ThrowIfCancellationRequested();

                // To avoid pass null values to CreateNew
                if (childList.get(i) == null) continue;

                var node = NodeService.CreateNew(this.MegaSdk, this.AppInformation, childList.get(i), ChildNodes);

                // If node creation failed for some reason, continue with the rest and leave this one
                if (node == null) continue;

                //if (CurrentDisplayMode == CurrentDisplayMode.MoveItem && FocusedNode != null &&
                //    node.OriginalMNode.getBase64Handle() == FocusedNode.OriginalMNode.getBase64Handle())
                //{
                //    node.DisplayMode = NodeDisplayMode.SelectedForMove;
                //    FocusedNode = node;
                //}

                helperList.Add(node);

                // First add the viewport items to show some data to the user will still loading
                if (i == viewportItemCount)
                {
                    var waitHandleViewportNodes = new AutoResetEvent(false);
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        helperList.ForEach(n =>
                        {
                            // If the task has been cancelled, stop processing
                            if (LoadingCancelToken.IsCancellationRequested)
                                LoadingCancelToken.ThrowIfCancellationRequested();
                            ChildNodes.Add(n);
                        });
                        waitHandleViewportNodes.Set();
                    });
                    waitHandleViewportNodes.WaitOne();

                    helperList.Clear();
                    continue;
                }

                if (helperList.Count != backgroundItemCount || i <= viewportItemCount) continue;

                // Add the rest of the items in the background to the list
                var waitHandleBackgroundNodes = new AutoResetEvent(false);
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    helperList.ForEach(n =>
                    {
                        // If the task has been cancelled, stop processing
                        if (LoadingCancelToken.IsCancellationRequested)
                            LoadingCancelToken.ThrowIfCancellationRequested();
                        ChildNodes.Add(n);
                    });
                    waitHandleBackgroundNodes.Set();
                });
                waitHandleBackgroundNodes.WaitOne();

                helperList.Clear();
            }

            // Add any nodes that are left over
            var waitHandleRestNodes = new AutoResetEvent(false);
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                // Show the user that processing the childnodes is done
                SetProgressIndication(false);

                // Set empty content to folder instead of loading view
                SetEmptyContentTemplate(false);

                helperList.ForEach(n =>
                {
                    // If the task has been cancelled, stop processing
                    if (LoadingCancelToken.IsCancellationRequested)
                        LoadingCancelToken.ThrowIfCancellationRequested();
                    ChildNodes.Add(n);
                });
                waitHandleRestNodes.Set();
            });
            waitHandleRestNodes.WaitOne();
        }

        private void InitializePerformanceParameters(out int viewportItemCount, out int backgroundItemCount)
        {
            viewportItemCount = 0;
            backgroundItemCount = 0;

            // Each view has different performance options
            switch (ViewMode)
            {
                case ViewMode.ListView:
                    viewportItemCount = 256;
                    backgroundItemCount = 1024;
                    break;
                case ViewMode.LargeThumbnails:
                    viewportItemCount = 128;
                    backgroundItemCount = 512;
                    break;
                case ViewMode.SmallThumbnails:
                    viewportItemCount = 72;
                    backgroundItemCount = 512;
                    break;
            }
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

        private void SetViewOnLoad()
        {
            if (FolderRootNode == null) return;

            OnUiThread(() =>
            {
                SetView(UiService.GetViewMode(FolderRootNode.Handle, FolderRootNode.Name));
            });
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
            this.ViewStateButtonPathData = VisualResources.ListViewPathData;
            this.MultiSelectCheckBoxStyle = (Style)Application.Current.Resources["DefaultCheckBoxStyle"];
        }

        public void BuildBreadCrumbs()
        {
            this.BreadCrumbs.Clear();

            // Top root nodes have no breadcrumbs
            if (this.FolderRootNode == null ||
                this.FolderRootNode.Type == MNodeType.TYPE_ROOT ||
                FolderRootNode.Type == MNodeType.TYPE_RUBBISH) return;

            this.BreadCrumbs.Add(this.FolderRootNode);

            MNode parentNode = FolderRootNode.OriginalMNode;
            parentNode = this.MegaSdk.getParentNode(parentNode);
            while ((parentNode != null) && (parentNode.getType() != MNodeType.TYPE_ROOT) &&
                (parentNode.getType() != MNodeType.TYPE_RUBBISH))
            {
                this.BreadCrumbs.Insert(0, NodeService.CreateNew(this.MegaSdk, this.AppInformation, parentNode));
                parentNode = this.MegaSdk.getParentNode(parentNode);
            }
        }

        #endregion

        #region IBreadCrumb

        public ObservableCollection<IMegaNode> BreadCrumbs { get; private set; }

        #endregion

        #region Properties

        public IMegaNode FocusedNode { get; set; }
        public DriveDisplayMode CurrentDisplayMode { get; set; }
        public DriveDisplayMode PreviousDisplayMode { get; set; }        
        public List<IMegaNode> SelectedNodes { get; set; }

        public bool NoFolderUpAction { get; set; }

        private ObservableCollection<IMegaNode> _childNodes;
        public ObservableCollection<IMegaNode> ChildNodes
        {
            get { return _childNodes; }
            set { SetField(ref _childNodes, value); }
        }

        public ContainerType Type { get; private set; }

        public ViewMode ViewMode { get; set; }

        private IMegaNode _folderRootNode;
        public IMegaNode FolderRootNode
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
