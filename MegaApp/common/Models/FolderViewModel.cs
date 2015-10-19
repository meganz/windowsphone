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
                case ContainerType.InShares:
                    this.CurrentDisplayMode = DriveDisplayMode.InShares;
                    break;
                case ContainerType.OutShares:
                    this.CurrentDisplayMode = DriveDisplayMode.OutShares;
                    break;
                case ContainerType.ContactInShares:
                    this.CurrentDisplayMode = DriveDisplayMode.ContactInShares;
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

            // Get the MNodes from the Mega SDK in the correct sorting order for the current folder
            MNodeList childList = NodeService.GetChildren(this.MegaSdk, this.FolderRootNode);

            if (childList == null)
            {
                OnUiThread(() =>
                {
                    new CustomMessageDialog(
                            AppMessages.LoadNodesFailed_Title,
                            AppMessages.LoadNodesFailed,
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialog();
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

        public void AddFolder()
        {
            if (!IsUserOnline()) return;

            // Only 1 CustomInputDialog should be open at the same time.
            if (this.AppInformation.PickerOrAsyncDialogIsOpen) return;

            var inputDialog = new CustomInputDialog(UiResources.AddFolder, UiResources.CreateFolder, this.AppInformation);
            inputDialog.OkButtonTapped += (sender, args) =>
            {
                this.MegaSdk.createFolder(args.InputText, this.FolderRootNode.OriginalMNode,
                     new CreateFolderRequestListener());
            };
            inputDialog.ShowDialog();
        }

        public void OpenLink()
        {
            if (!IsUserOnline()) return;

            // Only 1 CustomInputDialog should be open at the same time.
            if (this.AppInformation.PickerOrAsyncDialogIsOpen) return;

            var inputDialog = new CustomInputDialog(UiResources.OpenLink, UiResources.PasteMegaDownloadLink, this.AppInformation);
            inputDialog.OkButtonTapped += (sender, args) =>
            {
                this.MegaSdk.getPublicNode(args.InputText, new GetPublicNodeRequestListener(this));
            };
            inputDialog.ShowDialog();
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
                var megaRubbishBin = this.MegaSdk.getRubbishNode();

                if (this.FolderRootNode != null && megaRoot != null && this.FolderRootNode.Handle.Equals(megaRoot.getHandle()))
                {
                    OnUiThread(() =>
                        this.EmptyContentTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListCloudDriveEmptyContent"]);
                }
                else if (this.FolderRootNode != null && megaRubbishBin != null && this.FolderRootNode.Handle.Equals(megaRubbishBin.getHandle()))
                {
                    OnUiThread(() =>
                        this.EmptyContentTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListRubbishBinEmptyContent"]);
                }
                else
                {
                    OnUiThread(() =>
                        this.EmptyContentTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListEmptyContent"]);
                }
            }
        }

        public virtual bool GoFolderUp()
        {
            if (this.FolderRootNode == null) return false;

            MNode parentNode = this.MegaSdk.getParentNode(this.FolderRootNode.OriginalMNode);

            if (parentNode == null || parentNode.getType() == MNodeType.TYPE_UNKNOWN)
                return false;

            this.FolderRootNode = NodeService.CreateNew(this.MegaSdk, this.AppInformation, parentNode, ChildNodes);

            LoadChildNodes();

            return true;
        }

        public virtual void BrowseToHome()
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
                var result = await DialogService.ShowOptionsDialog(AppMessages.QuestionAskedDownloadOption_Title, 
                    AppMessages.QuestionAskedDownloadOption,
                    new[]
                    {
                        new DialogButton(AppMessages.QuestionAskedDownloadOption_YesButton, () =>
                        {
                            SettingsService.SaveSetting(SettingsResources.ExportImagesToPhotoAlbum, true);
                           
                        }),
                        new DialogButton(AppMessages.QuestionAskedDownloadOption_NoButton, () =>
                        {
                            SettingsService.SaveSetting(SettingsResources.ExportImagesToPhotoAlbum, false);
                        })
                    });

                if (result == MessageDialogResult.CancelNo) return;

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

            if (! await AppService.DownloadLimitCheck(downloadCount))
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

        public async Task<bool> MultipleRemoveItems()
        {
            int count = ChildNodes.Count(n => n.IsMultiSelected);

            if (count < 1) return false;

            if (this.CurrentDisplayMode == DriveDisplayMode.RubbishBin ||
                (this.CurrentDisplayMode == DriveDisplayMode.MultiSelect && 
                this.PreviousDisplayMode == DriveDisplayMode.RubbishBin))
            {
                var customMessageDialog = new CustomMessageDialog(
                    AppMessages.MultiSelectRemoveQuestion_Title,
                    String.Format(AppMessages.MultiSelectRemoveQuestion, count),
                    App.AppInformation,
                    MessageDialogButtons.OkCancel,
                    MessageDialogImage.RubbishBin);

                customMessageDialog.OkOrYesButtonTapped += (sender, args) =>
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() => ProgressService.SetProgressIndicator(true, ProgressMessages.RemoveNode));
                    RemoveOrRubbish(count);
                };

                return await customMessageDialog.ShowDialogAsync() == MessageDialogResult.OkYes;
            }
            else
            {
                var customMessageDialog = new CustomMessageDialog(
                    AppMessages.MultiMoveToRubbishBinQuestion_Title,
                    String.Format(AppMessages.MultiMoveToRubbishBinQuestion, count),
                    App.AppInformation,
                    MessageDialogButtons.OkCancel,
                    MessageDialogImage.RubbishBin);

                customMessageDialog.OkOrYesButtonTapped += (sender, args) =>
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() => ProgressService.SetProgressIndicator(true, ProgressMessages.NodeToTrash));
                    RemoveOrRubbish(count);
                };
                
                return await customMessageDialog.ShowDialogAsync() == MessageDialogResult.OkYes;
            }
        }

        #endregion

        #region Private Methods

        private void MultiSelect(object obj)
        {
            this.IsMultiSelectActive = !this.IsMultiSelectActive;
        }

        private async void RemoveItem(object obj)
        {
            await FocusedNode.RemoveAsync(false);
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

        private void RemoveOrRubbish(int count)
        {
            var helperList = new List<IMegaNode>(count);
            helperList.AddRange(ChildNodes.Where(n => n.IsMultiSelected));

            Task.Run(async() =>
            {
                WaitHandle[] waitEventRequests = new WaitHandle[count];

                int index = 0;

                foreach (var node in helperList)
                {
                    waitEventRequests[index] = new AutoResetEvent(false);
                    await node.RemoveAsync(true, (AutoResetEvent) waitEventRequests[index]);
                    index++;
                }
          
                WaitHandle.WaitAll(waitEventRequests);

                Deployment.Current.Dispatcher.BeginInvoke(() => ProgressService.SetProgressIndicator(false));

                if (this.CurrentDisplayMode == DriveDisplayMode.RubbishBin)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        new CustomMessageDialog(
                                AppMessages.MultiRemoveSucces_Title,
                                String.Format(AppMessages.MultiRemoveSucces, count),
                                App.AppInformation,
                                MessageDialogButtons.Ok).ShowDialog();
                    });
                }
                else
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        new CustomMessageDialog(
                                AppMessages.MultiMoveToRubbishBinSucces_Title,
                                String.Format(AppMessages.MultiMoveToRubbishBinSucces, count),
                                App.AppInformation,
                                MessageDialogButtons.Ok).ShowDialog();
                    });
                }
            });

            this.IsMultiSelectActive = false;
        }

        protected virtual bool CanCreateChild(MNode node)
        {
            return !node.getName().ToLower().Equals("camera uploads");
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

                MNode child = childList.get(i);
                // To avoid pass null values to CreateNew
                if (child == null) continue;

                // Avoid creating Camera Upload folder
                if (!CanCreateChild(child)) continue;

                var node = NodeService.CreateNew(this.MegaSdk, this.AppInformation, child, ChildNodes);

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
