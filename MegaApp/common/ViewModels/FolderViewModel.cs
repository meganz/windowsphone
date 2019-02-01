using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Windows.Storage;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using Telerik.Windows.Controls;
using mega;
using MegaApp.Classes;
using MegaApp.Database;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.MegaApi;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.Views;

namespace MegaApp.ViewModels
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
            this.BreadCrumbs = new ObservableCollection<IBaseNode>();
            this.BreadCrumbs.CollectionChanged += BreadCrumbs_CollectionChanged;
            this.SelectedNodes = new List<IMegaNode>();
            this.IsMultiSelectActive = false;
            
            this.RemoveItemCommand = new DelegateCommand(this.RemoveItem);
            this.RenameItemCommand = new DelegateCommand(this.RenameItem);
            this.RestoreItemCommand = new DelegateCommand(this.RestoreItem);
            this.DownloadItemCommand = new DelegateCommand(this.DownloadItem);
            this.ImportItemCommand = new DelegateCommand(this.ImportItem);
            this.CreateShortCutCommand = new DelegateCommand(this.CreateShortCut);
            this.ChangeViewCommand = new DelegateCommand(this.ChangeView);
            this.GetLinkCommand = new DelegateCommand(this.GetLink);
            this.MultiSelectCommand = new DelegateCommand(this.MultiSelect);
            this.ViewDetailsCommand = new DelegateCommand(this.ViewDetails);

            this.ChildNodes.CollectionChanged += ChildNodes_CollectionChanged;

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
                case ContainerType.FolderLink:
                    this.CurrentDisplayMode = DriveDisplayMode.FolderLink;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("containerType");
            }
        }

        void ChildNodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("HasChildNodesBinding");
        }

        #region Commands

        public ICommand ChangeViewCommand { get; private set; }
        public ICommand GetLinkCommand { get; private set; }
        public ICommand RenameItemCommand { get; private set; }
        public ICommand RemoveItemCommand { get; private set; }
        public ICommand RestoreItemCommand { get; private set; }
        public ICommand DownloadItemCommand { get; private set; }
        public ICommand ImportItemCommand { get; private set; }
        public ICommand CreateShortCutCommand { get; private set; }
        public ICommand MultiSelectCommand { get; set; }
        public ICommand ViewDetailsCommand { get; private set; }

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

        public void SelectAll()
        {
            foreach (var childNode in ChildNodes)
            {
                childNode.IsMultiSelected = true;
            }
        }

        public void DeselectAll()
        {
            foreach (var childNode in ChildNodes)
            {
                childNode.IsMultiSelected = false;
            }
        }

        public void ClearChildNodes()
        {
            if (ChildNodes == null || !ChildNodes.Any()) return;
           
            OnUiThread(() =>
            {
                this.ChildNodes.Clear();
            });
        }
        
        /// <summary>
        /// Load the mega nodes for this specific folder using the Mega SDK
        /// </summary>
        public void LoadChildNodes()
        {
            // User must be online to perform this operation
            if ((this.Type != ContainerType.FolderLink) && !IsUserOnline()) 
                return;

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
        public async void Refresh()
        {
            await FileService.ClearFilesAsync(
                NodeService.GetFiles(this.ChildNodes,
                Path.Combine(ApplicationData.Current.LocalFolder.Path,
                AppResources.ThumbnailsDirectory)));

            if(this.FolderRootNode == null)
            {
                switch (this.Type)
                {
                    case ContainerType.RubbishBin:
                        this.FolderRootNode = NodeService.CreateNew(this.MegaSdk, this.AppInformation, this.MegaSdk.getRubbishNode(), this.Type);
                        break;

                    case ContainerType.CloudDrive:
                    case ContainerType.FolderLink:
                        this.FolderRootNode = NodeService.CreateNew(this.MegaSdk, this.AppInformation, this.MegaSdk.getRootNode(), this.Type);
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

            if (this.FolderRootNode == null)
            {
                OnUiThread(() =>
                {
                    new CustomMessageDialog(
                        AppMessages.CreateFolderFailed_Title,
                        AppMessages.CreateFolderFailed,
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                });

                return;
            }

            var inputDialog = new CustomInputDialog(UiResources.AddFolder, UiResources.UI_CreateFolder, this.AppInformation);
            inputDialog.OkButtonTapped += (sender, args) =>
            {
                if (SdkService.ExistsNodeByName(this.FolderRootNode.OriginalMNode, args.InputText, true))
                {
                    inputDialog.HideDialog();
                    OnUiThread(() =>
                    {
                        new CustomMessageDialog(
                            AppMessages.CreateFolderFailed_Title,
                            AppMessages.AM_FolderAlreadyExists,
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialog();
                    });

                    return;
                }

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

            var inputDialog = new CustomInputDialog(UiResources.UI_OpenMegaLink, UiResources.UI_PasteMegaLink, this.AppInformation);
            inputDialog.OkButtonTapped += (sender, args) =>
            {
                if (!String.IsNullOrWhiteSpace(args.InputText))
                {
                    App.LinkInformation.ActiveLink = UriService.ReformatUri(args.InputText);

                    if (App.LinkInformation.ActiveLink.Contains("https://mega.nz/#!"))
                    {
                        App.LinkInformation.UriLink = UriLinkType.File;
                        this.MegaSdk.getPublicNode(App.LinkInformation.ActiveLink, new GetPublicNodeRequestListener(this));
                    }                        
                    else if (App.LinkInformation.ActiveLink.Contains("https://mega.nz/#F!"))
                    {
                        App.LinkInformation.UriLink = UriLinkType.Folder;
                        NavigateService.NavigateTo(typeof(FolderLinkPage), NavigationParameter.FolderLinkLaunch);
                    }
                    else
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            new CustomMessageDialog(
                                AppMessages.AM_OpenLinkFailed_Title, 
                                AppMessages.AM_InvalidLink, 
                                App.AppInformation,
                                MessageDialogButtons.Ok).ShowDialog();
                        });
                    }
                }
            };
            inputDialog.ShowDialog();
        }

        /// <summary>
        /// Method to import the selected nodes or the entire content of a folder link.
        /// </summary>
        public void ImportFolderLink()
        {
            // If no selected nodes, there is nothing to import
            if (App.LinkInformation.SelectedNodes.Count < 1) return;

            foreach (var node in App.LinkInformation.SelectedNodes)
            {
                // Extra check to avoid NullReferenceException
                if (node != null)
                {
                    this.MegaSdk.copyNode(SdkService.MegaSdkFolderLinks.authorizeNode(node.OriginalMNode),
                        FolderRootNode.OriginalMNode, new CopyNodeRequestListener(true));
                }
            }
        }

        public void ImportLink(string link)
        {
            if (String.IsNullOrWhiteSpace(link))
            {
                OnUiThread(() =>
                {
                    new CustomMessageDialog(
                        AppMessages.ImportFileFailed_Title,
                        AppMessages.AM_InvalidLink,
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                });

                return;
            }

            if (this.FolderRootNode == null)
            {
                OnUiThread(() =>
                {
                    new CustomMessageDialog(
                        AppMessages.ImportFileFailed_Title,
                        AppMessages.AM_ImportFileFailedNoErrorCode,
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                });

                return;
            }

            this.MegaSdk.importFileLink(
                link,
                this.FolderRootNode.OriginalMNode,
                new ImportFileRequestListener());
        }

        public void DownloadLink(MNode publicNode)
        {            
            var downloadNode = NodeService.CreateNew(this.MegaSdk, this.AppInformation, publicNode, ContainerType.PublicLink);

            if(downloadNode != null)
                downloadNode.Download(TransfersService.MegaTransfers);
        }

        public void OnChildNodeTapped(IMegaNode node)
        {
            switch (node.Type)
            {
                case MNodeType.TYPE_UNKNOWN:
                    break;
                case MNodeType.TYPE_FILE:
                    // If the user is moving nodes don't process the file node
                    if (CurrentDisplayMode != DriveDisplayMode.CopyOrMoveItem)
                        ProcessFileNode(node);
                    break;
                case MNodeType.TYPE_FOLDER:
                    // If the user is moving nodes and the folder is one of the selected nodes don't navigate to it
                    if ((CurrentDisplayMode == DriveDisplayMode.CopyOrMoveItem) && (IsSelectedNode(node))) return;
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

        /// <summary>
        /// Check if a node is in the selected nodes group for move, copy or any other action.
        /// </summary>        
        /// <param name="node">Node to check if is in the selected node list</param>        
        /// <returns>True if is a selected node or false in other case</returns>
        private bool IsSelectedNode(IMegaNode node)
        {
            if ((SelectedNodes != null) && (SelectedNodes.Count > 0))
            {
                for (int index = 0; index < SelectedNodes.Count; index++)
                {
                    var selectedNode = SelectedNodes[index];
                    if ((selectedNode != null) && (node.OriginalMNode.getBase64Handle() == selectedNode.OriginalMNode.getBase64Handle()))
                    {   
                        //Update the selected nodes list values
                        node.DisplayMode = NodeDisplayMode.SelectedForCopyOrMove;
                        SelectedNodes[index] = node;

                        return true;
                    }
                }
            }            

            return false;
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
                        this.NextViewButtonPathData = VisualResources.SmallThumbnailViewPathData;
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
                        this.NextViewButtonPathData = VisualResources.ListViewPathData;
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
                OnUiThread(() => 
                {
                    this.EmptyContentTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListLoadingContent"];
                    this.EmptyInformationText = "";
                });
            }
            else
            {
                switch(this.Type)
                {
                    case ContainerType.CloudDrive:
                    case ContainerType.RubbishBin:
                        var megaRoot = this.MegaSdk.getRootNode();
                        var megaRubbishBin = this.MegaSdk.getRubbishNode();
                        if (this.FolderRootNode != null && megaRoot != null && this.FolderRootNode.Base64Handle.Equals(megaRoot.getBase64Handle()))
                        {
                            OnUiThread(() =>
                            {
                                this.EmptyContentTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListCloudDriveEmptyContent"];
                                this.EmptyInformationText = UiResources.EmptyCloudDrive.ToLower();
                            });
                        }
                        else if (this.FolderRootNode != null && megaRubbishBin != null && this.FolderRootNode.Base64Handle.Equals(megaRubbishBin.getBase64Handle()))
                        {
                            OnUiThread(() =>
                            {
                                this.EmptyContentTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListRubbishBinEmptyContent"];
                                this.EmptyInformationText = UiResources.EmptyRubbishBin.ToLower();
                            });
                        }
                        else
                        {
                            OnUiThread(() =>
                            {
                                this.EmptyContentTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListEmptyContent"];
                                this.EmptyInformationText = UiResources.EmptyFolder.ToLower();
                            });
                        }
                        break;                    

                    case ContainerType.InShares:
                    case ContainerType.OutShares:
                        OnUiThread(() =>
                        {
                            this.EmptyContentTemplate = (DataTemplate)Application.Current.Resources["MegaSharedFoldersListEmptyContent"];
                            this.EmptyInformationText = UiResources.EmptySharedFolders.ToLower();
                        });
                        break;
                    
                    case ContainerType.ContactInShares:
                        break;

                    case ContainerType.Offline:
                        OnUiThread(() =>
                        {
                            this.EmptyContentTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListRubbishBinEmptyContent"];
                            this.EmptyInformationText = UiResources.EmptyOffline.ToLower();
                        });
                        break;

                    case ContainerType.FolderLink:
                        OnUiThread(() =>
                        {
                            this.EmptyContentTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListEmptyContent"];
                            this.EmptyInformationText = UiResources.EmptyFolder.ToLower();
                        });
                        break;
                }
            }
        }

        public void SetOfflineContentTemplate()
        {
            OnUiThread(() =>
            {
                this.EmptyContentTemplate = (DataTemplate)Application.Current.Resources["OfflineEmptyContent"];
                this.EmptyInformationText = UiResources.NoInternetConnection.ToLower();
            });                
        }

        public virtual bool GoFolderUp()
        {
            if (this.FolderRootNode == null) return false;

            MNode parentNode = this.MegaSdk.getParentNode(this.FolderRootNode.OriginalMNode);

            if (parentNode == null || parentNode.getType() == MNodeType.TYPE_UNKNOWN)
                return false;

            this.FolderRootNode = NodeService.CreateNew(this.MegaSdk, this.AppInformation, parentNode, this.Type, ChildNodes);

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

            this.FolderRootNode = NodeService.CreateNew(this.MegaSdk, this.AppInformation, homeNode, this.Type, ChildNodes);

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

            var existingNode = SavedForOffline.SelectNodeByFingerprint(this.MegaSdk.getNodeFingerprint(node.OriginalMNode));
            if(existingNode != null && !String.IsNullOrWhiteSpace(existingNode.LocalPath) && 
                FileService.FileExists(existingNode.LocalPath))
            {                
                var offlineNode = new OfflineFileNodeViewModel(new FileInfo(existingNode.LocalPath));
                offlineNode.Open();
            }
            else
            {
                if (node.IsImage)
                    OnUiThread(() => NavigateService.NavigateTo(typeof(PreviewImagePage), NavigationParameter.Normal, this));
                else
                    this.FocusedNode.Download(TransfersService.MegaTransfers);
            }            
        }

        public async void MultipleDownload(String downloadPath = null)
        {
            if (this.Type == ContainerType.FolderLink)
                this.SelectedNodes = App.LinkInformation.SelectedNodes;
            else
                this.SelectedNodes = ChildNodes.Where(n => n.IsMultiSelected).ToList();

            if (this.SelectedNodes.Count < 1) return;

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
            if (downloadPath == null)
            {
                if (!await FolderService.SelectDownloadFolder()) { return; }
                else { downloadPath = AppService.GetSelectedDownloadDirectoryPath(); }
            }
            #endif

            ProgressService.SetProgressIndicator(true, ProgressMessages.PrepareDownloads);

            // Give the app the time to display the progress indicator
            await Task.Delay(5);

            // First count the number of downloads before proceeding to the transfers.
            int downloadCount = 0;
            foreach (var node in SelectedNodes)
            {
                var folderNode = node as FolderNodeViewModel;
                if (folderNode != null)
                    downloadCount += NodeService.GetRecursiveNodes(this.MegaSdk, AppInformation, folderNode).Count;
                else
                    downloadCount++;
            }            

            if (! await AppService.DownloadLimitCheck(downloadCount))
            {
                ProgressService.SetProgressIndicator(false);
                return;
            }

            foreach (var node in SelectedNodes)
                node.Download(TransfersService.MegaTransfers, downloadPath);

            ProgressService.SetProgressIndicator(false);

            this.IsMultiSelectActive = false;
        }

        public bool SelectMultipleItemsForMove()
        {
            int count = ChildNodes.Count(n => n.IsMultiSelected);

            if (count < 1) return false;

            SelectedNodes.Clear();

            foreach (var node in ChildNodes.Where(n => n.IsMultiSelected))
            {
                node.DisplayMode = NodeDisplayMode.SelectedForCopyOrMove;
                SelectedNodes.Add(node);
            }

            this.IsMultiSelectActive = false;
            this.PreviousDisplayMode = this.CurrentDisplayMode;
            this.CurrentDisplayMode = DriveDisplayMode.CopyOrMoveItem;

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

        /// <summary>
        /// Restore multiple items from the rubbish bin
        /// </summary>
        public async void MultipleRestoreItems()
        {
            var helperList = new List<IMegaNode>(ChildNodes.Count(n => n.IsMultiSelected));
            helperList.AddRange(ChildNodes.Where(n => n.IsMultiSelected));

            this.IsMultiSelectActive = false;

            if (helperList.Count < 1) return;

            Deployment.Current.Dispatcher.BeginInvoke(() =>
                ProgressService.SetProgressIndicator(true, ProgressMessages.PM_Restoring));

            bool result = true;
            foreach (var node in helperList)
            {
                if (node == null) continue;
                result = result & (await node.MoveAsync(node.RestoreNode) == NodeActionResult.Succeeded);
            }

            Deployment.Current.Dispatcher.BeginInvoke(() => 
                ProgressService.SetProgressIndicator(false));

            if (!result)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    new CustomMessageDialog(
                        AppMessages.AM_RestoreFromRubbishBinFailed_Title,
                        AppMessages.AM_RestoreMultiFromRubbishBinFailed,
                        App.AppInformation, MessageDialogButtons.Ok).ShowDialog();
                });
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
            if (FocusedNode != null)
                await FocusedNode.RemoveAsync(false);
        }

        private void RenameItem(object obj)
        {
            FocusedNode.Rename();
        }

        private async void RestoreItem(object obj)
        {
            if (FocusedNode == null) return;
            if (await FocusedNode.MoveAsync(FocusedNode.RestoreNode) != NodeActionResult.Succeeded)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    new CustomMessageDialog(
                        AppMessages.AM_RestoreFromRubbishBinFailed_Title,
                        string.Format(AppMessages.AM_RestoreFromRubbishBinFailed, FocusedNode.Name),
                        App.AppInformation, MessageDialogButtons.Ok).ShowDialog();
                });
            }
        }

        private void GetLink(object obj)
        {
            if (!IsUserOnline()) return;

            FocusedNode.GetLink();
        }

        private void DownloadItem(object obj)
        {
            if (FocusedNode == null) return;
            FocusedNode.Download(TransfersService.MegaTransfers);
        }

        private void ImportItem(object obj)
        {
            App.LinkInformation.SelectedNodes.Add(FocusedNode);
            App.LinkInformation.LinkAction = LinkAction.Import;

            OnUiThread(() => NavigateService.NavigateTo(typeof(MainPage), NavigationParameter.ImportFolderLink));
        }

        private void ViewDetails(object obj)
        {
            NodeViewModel node = NodeService.CreateNew(this.MegaSdk, App.AppInformation,
                this.MegaSdk.getNodeByBase64Handle(FocusedNode.Base64Handle), this.Type);

            OnUiThread(() =>
            {
                if (node != null)
                {
                    NavigateService.NavigateTo(typeof(NodeDetailsPage), NavigationParameter.Normal, node);
                }
                else
                {
                    new CustomMessageDialog(
                        AppMessages.AM_GetNodeDetailsFailed_Title,
                        AppMessages.AM_GetNodeDetailsFailed,
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                }
            });
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
                new Uri("/Views/MainPage.xaml?ShortCutBase64Handle=" + FocusedNode.OriginalMNode.getBase64Handle(), UriKind.Relative),
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

        public void SetProgressIndication(bool onOff, string busyText = null)
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

        private void CreateChildren(MNodeList childList, int listSize)
        {
            // Set the parameters for the performance for the different view types of a folder
            int viewportItemCount, backgroundItemCount;
            InitializePerformanceParameters(out viewportItemCount, out backgroundItemCount);

            // We will not add nodes one by one in the dispatcher but in groups
            List<IMegaNode> helperList;
            try { helperList = new List<IMegaNode>(1024); }
            catch (ArgumentOutOfRangeException) { helperList = new List<IMegaNode>(); }

            for (int i = 0; i < listSize; i++)
            {
                // If the task has been cancelled, stop processing
                if (LoadingCancelToken.IsCancellationRequested)
                    LoadingCancelToken.ThrowIfCancellationRequested();
                                
                // To avoid pass null values to CreateNew
                if (childList.get(i) == null) continue;
                                
                var node = NodeService.CreateNew(this.MegaSdk, this.AppInformation, childList.get(i), this.Type, ChildNodes);

                // If node creation failed for some reason, continue with the rest and leave this one
                if (node == null) continue;

                // If the user is moving nodes, check if the node had been selected to move 
                // and establish the corresponding display mode
                if (CurrentDisplayMode == DriveDisplayMode.CopyOrMoveItem)
                {
                    // Check if it is the only focused node
                    if((FocusedNode != null) && (node.OriginalMNode.getBase64Handle() == FocusedNode.OriginalMNode.getBase64Handle()))
                    {
                        node.DisplayMode = NodeDisplayMode.SelectedForCopyOrMove;
                        FocusedNode = node;
                    }

                    // Check if it is one of the multiple selected nodes
                    IsSelectedNode(node);
                }

                helperList.Add(node);

                // First add the viewport items to show some data to the user will still loading
                if (i == viewportItemCount)
                {
                    var waitHandleViewportNodes = new AutoResetEvent(false);
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        // If the task has been cancelled, stop processing
                        foreach (var megaNode in helperList.TakeWhile(megaNode => !LoadingCancelToken.IsCancellationRequested))
                        {
                            ChildNodes.Add(megaNode);
                        }
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
                    // If the task has been cancelled, stop processing
                    foreach (var megaNode in helperList.TakeWhile(megaNode => !LoadingCancelToken.IsCancellationRequested))
                    {
                        ChildNodes.Add(megaNode);
                    }
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

                // If the task has been cancelled, stop processing
                foreach (var megaNode in helperList.TakeWhile(megaNode => !LoadingCancelToken.IsCancellationRequested))
                {
                    ChildNodes.Add(megaNode);
                }
                waitHandleRestNodes.Set();
            });
            waitHandleRestNodes.WaitOne();

            OnUiThread(() => OnPropertyChanged("HasChildNodesBinding"));
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

            this.NodeTemplateSelector = new NodeTemplateSelector()
            {
                FileItemTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListFileItemContent"],
                FolderItemTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListFolderItemContent"]
            };

            this.ViewMode = ViewMode.ListView;
            this.NextViewButtonPathData = VisualResources.LargeThumbnailViewPathData;
            this.MultiSelectCheckBoxStyle = (Style)Application.Current.Resources["DefaultCheckBoxStyle"];
        }

        public void BuildBreadCrumbs()
        {
            this.BreadCrumbs.Clear();

            // Top root nodes have no breadcrumbs
            if (this.FolderRootNode == null ||
                this.FolderRootNode.Type == MNodeType.TYPE_ROOT ||
                FolderRootNode.Type == MNodeType.TYPE_RUBBISH) return;

            this.BreadCrumbs.Add((IBaseNode)this.FolderRootNode);

            MNode parentNode = FolderRootNode.OriginalMNode;
            parentNode = this.MegaSdk.getParentNode(parentNode);
            while ((parentNode != null) && (parentNode.getType() != MNodeType.TYPE_ROOT) &&
                (parentNode.getType() != MNodeType.TYPE_RUBBISH))
            {
                this.BreadCrumbs.Insert(0, (IBaseNode)NodeService.CreateNew(this.MegaSdk, this.AppInformation, parentNode, this.Type));
                parentNode = this.MegaSdk.getParentNode(parentNode);
            }
        }

        void BreadCrumbs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.FolderRootNode == null) return;

            String folderName = String.Empty;
            switch(this.FolderRootNode.Type)
            {
                case MNodeType. TYPE_ROOT:
                    folderName = UiResources.CloudDriveName;
                    break;

                case MNodeType.TYPE_RUBBISH:
                    folderName = UiResources.RubbishBinName;
                    break;

                case MNodeType.TYPE_FOLDER:
                    folderName = this.FolderRootNode.Name;
                    break;
            }

            this.ImportLinkBorderText = String.Format(UiResources.UI_ImportLinkBorderText, folderName);
        }

        #endregion

        #region IBreadCrumb

        public ObservableCollection<IBaseNode> BreadCrumbs { get; private set; }

        #endregion

        #region Properties

        private IMegaNode _focusedNode;
        public IMegaNode FocusedNode
        {
            get { return _focusedNode; }
            set { SetField(ref _focusedNode, value); }
        }
        
        public DriveDisplayMode CurrentDisplayMode { get; set; }
        public DriveDisplayMode PreviousDisplayMode { get; set; }
        public List<IMegaNode> SelectedNodes;

        private ObservableCollection<IMegaNode> _childNodes;
        public ObservableCollection<IMegaNode> ChildNodes
        {
            get { return _childNodes; }
            set { SetField(ref _childNodes, value); }
        }

        public bool HasChildNodesBinding
        {
            get { return HasChildNodes(); }
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

        private string _nextViewButtonPathData;
        public string NextViewButtonPathData
        {
            get { return _nextViewButtonPathData; }
            set { SetField(ref _nextViewButtonPathData, value); }
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
            set { SetField(ref _isMultiSelectActive, value); }
        }

        private DataTemplate _emptyContentTemplate;
        public DataTemplate EmptyContentTemplate
        {
            get { return _emptyContentTemplate; }
            private set { SetField(ref _emptyContentTemplate, value); }
        }

        private String _emptyInformationText;
        public String EmptyInformationText
        {
            get { return _emptyInformationText; }
            private set { SetField(ref _emptyInformationText, value); }
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

        /// <summary>
        /// Property needed show a dynamic import text.
        /// </summary>        
        private String _importLinkBorderText;
        public String ImportLinkBorderText
        {
            get { return _importLinkBorderText; }
            private set { SetField(ref _importLinkBorderText, value); }
        }

        /// <summary>
        /// Indicates if can restore node(s) to their previous locations in a multi-select scenario.
        /// </summary>
        public bool CanMultiSelectRestore
        {
            get 
            {
                var helperList = new List<IMegaNode>(ChildNodes.Count(n => n.IsMultiSelected));
                helperList.AddRange(ChildNodes.Where(n => n.IsMultiSelected));

                if (helperList.Count < 1) return false;

                foreach (var node in helperList)
                {
                    if (node == null) continue;
                    if (!node.CanRestore)
                        return false;
                }

                return true;
            }
        }

        #endregion
       
    }
}
