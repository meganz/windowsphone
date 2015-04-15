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
using System.Windows.Media;
using Windows.Storage;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.MegaApi;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Telerik.Windows.Controls;

namespace MegaApp.Models
{
    public class CloudDriveViewModel : BaseAppInfoAwareViewModel
    {
        private const int DownloadLimitMessage = 100;
        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken cancellationToken;
        //private bool asyncInputPromptDialogIsOpen;

        public event EventHandler<CommandStatusArgs> CommandStatusChanged;

        public RadDataBoundListBox ListBox { private get; set; }

        public bool PickerOrDialogIsOpen { get; set; }

        public CloudDriveViewModel(MegaSDK megaSdk, AppInformation appInformation)
            : base(megaSdk, appInformation)
        {
            this.DriveDisplayMode = DriveDisplayMode.CloudDrive;
            this.CurrentRootNode = null;
            this.BreadCrumbNode = null;
            this.ChildNodes = new ObservableCollection<IMegaNode>();
            this.BreadCrumbs = new ObservableCollection<IMegaNode>();
            this.SelectedNodes = new List<IMegaNode>();
            this.IsMultiSelectActive = false;
            SetViewDefaults();

            this.RemoveItemCommand = new DelegateCommand(this.RemoveItem);
            this.RenameItemCommand = new DelegateCommand(this.RenameItem);
            this.GetPreviewLinkItemCommand = new DelegateCommand(this.GetPreviewLink);
            this.DownloadItemCommand = new DelegateCommand(this.DownloadItem);
            this.CreateShortCutCommand = new DelegateCommand(this.CreateShortCut);
            this.ChangeViewCommand = new DelegateCommand(this.ChangeView);
            this.MultiSelectCommand = new DelegateCommand(this.MultiSelect);
            this.UpgradeAccountCommand = new DelegateCommand(this.UpgradeAccount);
        }

        #region Commands

        public ICommand RemoveItemCommand { get; set; }
        public ICommand GetPreviewLinkItemCommand { get; set; }
        public ICommand DownloadItemCommand { get; set; }
        public ICommand RenameItemCommand { get; set; }
        public ICommand CreateShortCutCommand { get; set; }
        public ICommand ChangeViewCommand { get; set; }
        public ICommand MultiSelectCommand { get; set; }
        public ICommand UpgradeAccountCommand { get; set; }

        #endregion

        #region Events

        private void OnCommandStatusChanged(bool status)
        {
            if (CommandStatusChanged == null) return;

            CommandStatusChanged(this, new CommandStatusArgs(status));
        }

        #endregion

        #region Services


        #endregion

        #region Public Methods

        public void SetCommandStatus(bool status)
        {
            OnCommandStatusChanged(status);
        }

        public void TranslateAppBar(IList iconButtons, IList menuItems, MenuType menuType)
        {
            switch (menuType)
            {
                case MenuType.CloudDriveMenu:
                {
                    ((ApplicationBarIconButton)iconButtons[0]).Text = UiResources.Upload.ToLower();
                    ((ApplicationBarIconButton)iconButtons[1]).Text = UiResources.AddFolder.ToLower();
                    //((ApplicationBarIconButton)iconButtons[2]).Text = UiResources.Refresh.ToLower();
                    ((ApplicationBarIconButton)iconButtons[2]).Text = UiResources.OpenLink.ToLower();
                    
                    //((ApplicationBarMenuItem)menuItems[0]).Text = UiResources.MultiSelect.ToLower();
                    //((ApplicationBarMenuItem)menuItems[1]).Text = UiResources.Transfers.ToLower();
                    //((ApplicationBarMenuItem)menuItems[2]).Text = UiResources.MyAccount.ToLower();
                    //((ApplicationBarMenuItem)menuItems[3]).Text = UiResources.Settings.ToLower();
                    //((ApplicationBarMenuItem)menuItems[4]).Text = UiResources.About.ToLower(); 
                    break;
                }
                case MenuType.RubbishBinMenu:
                {
                    ((ApplicationBarIconButton)iconButtons[0]).Text = UiResources.Refresh.ToLower();

                    ((ApplicationBarMenuItem)menuItems[0]).Text = UiResources.CloudDriveName.ToLower();
                    //((ApplicationBarMenuItem)menuItems[1]).Text = UiResources.MultiSelect.ToLower();
                    //((ApplicationBarMenuItem)menuItems[2]).Text = UiResources.Transfers.ToLower();
                    //((ApplicationBarMenuItem)menuItems[3]).Text = UiResources.MyAccount.ToLower();
                    //((ApplicationBarMenuItem)menuItems[4]).Text = UiResources.Settings.ToLower();
                    //((ApplicationBarMenuItem)menuItems[5]).Text = UiResources.About.ToLower();
                    break;
                }
                case MenuType.MoveMenu:
                {
                    ((ApplicationBarIconButton)iconButtons[0]).Text = UiResources.Move.ToLower();
                    ((ApplicationBarIconButton)iconButtons[1]).Text = UiResources.Cancel.ToLower();
                  
                    break;
                }
                case MenuType.MultiSelectMenu:
                {
                    ((ApplicationBarIconButton)iconButtons[0]).Text = UiResources.Download.ToLower();
                    ((ApplicationBarIconButton)iconButtons[1]).Text = UiResources.Move.ToLower();
                    ((ApplicationBarIconButton)iconButtons[2]).Text = UiResources.Remove.ToLower();
                    break;
                }
                case MenuType.ImportMenu:
                {
                    ((ApplicationBarIconButton)iconButtons[0]).Text = UiResources.LinkOptions.ToLower();
                    ((ApplicationBarIconButton)iconButtons[1]).Text = UiResources.Cancel.ToLower();
                    break;
                }
            }
           
        }

        public bool MultipleRemove()
        {
            int count = ChildNodes.Count(n => n.IsMultiSelected);

            if (count < 1) return false;

            if (this.OldDriveDisplayMode == DriveDisplayMode.RubbishBin)
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

                if (this.OldDriveDisplayMode == DriveDisplayMode.RubbishBin)
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

        public async void MultipleDownload(StorageFolder downloadFolder = null)
        {
            int count = ChildNodes.Count(n => n.IsMultiSelected);

            if (count < 1) return;
            
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
            // Only 1 Folder Picker can be open at 1 time
            if (PickerOrDialogIsOpen) return;

            if (downloadFolder == null)
            {
                PickerOrDialogIsOpen = true;
                if (!await FolderService.SelectDownloadFolder())return;
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

            if (!DownloadLimitCheck(downloadCount))
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

        private bool DownloadLimitCheck(int downloadCount)
        {
            if (downloadCount <= DownloadLimitMessage) return true;

            return MessageBox.Show(String.Format(AppMessages.DownloadLimitMessage, DownloadLimitMessage, downloadCount),
                AppMessages.DownloadLimitMessage_Title, MessageBoxButton.OKCancel) == MessageBoxResult.OK;
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
            if (photoResult == null || photoResult.TaskResult != TaskResult.OK) return;

            try
            {
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
                    App.MegaTransfers.Insert(0, uploadTransfer);
                    uploadTransfer.StartTransfer();
                }
                NoFolderUpAction = true;
                NavigateService.NavigateTo(typeof(TransferPage), NavigationParameter.Normal);
            }
            catch (Exception)
            {
                MessageBox.Show(AppMessages.PhotoUploadError, AppMessages.PhotoUploadError_Title, MessageBoxButton.OK);
            }
        }

        public void GoFolderUp()
        {
            CancelLoadNodes();

            MNode parentNode = null;
            
            if (this.CurrentRootNode != null)
                parentNode = this.MegaSdk.getParentNode(this.CurrentRootNode.OriginalMNode);

            if (parentNode == null || parentNode.getType() == MNodeType.TYPE_UNKNOWN )
                parentNode = this.MegaSdk.getRootNode();
            
            this.CurrentRootNode = NodeService.CreateNew(this.MegaSdk, this.AppInformation, parentNode, ChildNodes);
        }

        public void SetView(ViewMode viewMode)
        {            
            switch (viewMode)
            {
                case ViewMode.LargeThumbnails:
                    {
                        ListBox.VirtualizationStrategyDefinition = new WrapVirtualizationStrategyDefinition()
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
                        this.ViewStateButtonIconUri = new Uri("/Assets/Images/large grid view.Screen-WXGA.png", UriKind.Relative);

                        if (ListBox != null) ListBox.CheckBoxStyle = (Style)Application.Current.Resources["MultiSelectItemCheckBoxStyle"];
                        //this.MultiSelectCheckBoxStyle = (Style)Application.Current.Resources["MultiSelectItemCheckBoxStyle"];
                        
                        break;
                    }
                case ViewMode.SmallThumbnails:
                    {
                        ListBox.VirtualizationStrategyDefinition = new WrapVirtualizationStrategyDefinition()
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
                        this.ViewStateButtonIconUri = new Uri("/Assets/Images/small grid view.Screen-WXGA.png", UriKind.Relative);

                        if (ListBox != null) ListBox.CheckBoxStyle = (Style)Application.Current.Resources["MultiSelectItemCheckBoxStyle"];
                        //this.MultiSelectCheckBoxStyle = (Style)Application.Current.Resources["MultiSelectItemCheckBoxStyle"];
                       
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
            if (CurrentRootNode == null) return;

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
            if (ListBox != null)
                ListBox.VirtualizationStrategyDefinition = new StackVirtualizationStrategyDefinition()
                {
                    Orientation = Orientation.Vertical
                };

            this.NodeTemplateSelector = new NodeTemplateSelector()
            {
                FileItemTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListFileItemContent"],
                FolderItemTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListFolderItemContent"]
            };

            this.ViewMode = ViewMode.ListView;
            this.ViewStateButtonIconUri = new Uri("/Assets/Images/list view.Screen-WXGA.png", UriKind.Relative);

            if (ListBox != null) ListBox.CheckBoxStyle = (Style) Application.Current.Resources["DefaultCheckBoxStyle"];
            //this.MultiSelectCheckBoxStyle = (Style)Application.Current.Resources["DefaultCheckBoxStyle"];
        }

        public void GoToFolder(NodeViewModel folder)
        {
            if (folder == null) return;

            CancelLoadNodes();

            this.BreadCrumbNode = this.CurrentRootNode;
            this.CurrentRootNode = folder;
            this.CurrentRootNode.ChildCollection = ChildNodes;
            
            NavigateService.NavigateTo(typeof(MainPage), NavigationParameter.BreadCrumb, new Dictionary<string, string> { { "Id", Guid.NewGuid().ToString("N") } });
        }

        public void GoToRoot()
        {
            GoToFolder(NodeService.CreateNew(this.MegaSdk, this.AppInformation, MegaSdk.getRootNode()));
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
            MNode startNode = this.BreadCrumbNode.OriginalMNode;

            while (parentNode == null || parentNode.getBase64Handle() != this.CurrentRootNode.OriginalMNode.getBase64Handle())
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

            // Only 1 RadInputPrompt can be open at the same time with ShowAsync.
            if (PickerOrDialogIsOpen) return;

            PickerOrDialogIsOpen = true;
            var inputPromptClosedEventArgs = await RadInputPrompt.ShowAsync(
				new string[] { UiResources.Open.ToLower(), UiResources.Cancel.ToLower() }, UiResources.OpenLink);
            PickerOrDialogIsOpen = false;

            if (inputPromptClosedEventArgs.Result != DialogResult.OK) return;

            this.MegaSdk.getPublicNode(inputPromptClosedEventArgs.Text, new GetPublicNodeRequestListener(null));
        }

        public void ImportLink(string link)
        {
            if (String.IsNullOrEmpty(link)) return;

            this.MegaSdk.importFileLink(
                link,
                CurrentRootNode != null ? CurrentRootNode.OriginalMNode : this.MegaSdk.getRootNode(), 
                new ImportFileRequestListener());

            LinkToImport = null;
        }

        public void DownloadLink(MNode publicNode)
        {
            // Create a temporary DownloadNodeViewModel from the public Node created from the link
            this.NoFolderUpAction = true;
            PublicNode = publicNode;
            var downloadNode = NodeService.CreateNew(this.MegaSdk, this.AppInformation, publicNode);
            downloadNode.Download(App.MegaTransfers);
        }

        public void LoadNodes()
        {
            // First cancel any other loadnodes
            CancelLoadNodes(false);

            // Initialize the nodeList and listSize variables
            MNodeList nodeList = null;
            int listSize = 0;

            try
            {
                // If for some reason the CurrentRootNode is null then create CloudDrive RootNode as replacement
                if (this.CurrentRootNode == null)
                    this.CurrentRootNode = NodeService.CreateNew(this.MegaSdk, this.AppInformation, this.MegaSdk.getRootNode());

                // Get the nodes from the MEGA SDK
                nodeList = this.MegaSdk.getChildren(this.CurrentRootNode.OriginalMNode,
                    UiService.GetSortOrder(CurrentRootNode.Handle, CurrentRootNode.Name));

                // Retrieve the size of the list to save time in the loops
                if (nodeList != null)
                {
                    listSize = nodeList.size();
                }
                else
                {
                    MessageBox.Show(AppMessages.LoadNodesFailed, AppMessages.LoadNodesFailed_Title, MessageBoxButton.OK);
                    SetEmptyContentTemplate(false, this.CurrentRootNode);
                    return;
                }
            }
            catch (Exception)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show(AppMessages.LoadNodesFailed, AppMessages.LoadNodesFailed_Title,
                        MessageBoxButton.OK);

                    SetEmptyContentTemplate(false, this.CurrentRootNode);
                });
            }

            try
            {
                // Display a minor indication for the user that the app is busy
                if (Deployment.Current.Dispatcher.CheckAccess())
                    ProgressService.SetProgressIndicator(true, String.Empty);
                else
                {
                    var autoResetEvent = new AutoResetEvent(false);
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        ProgressService.SetProgressIndicator(true, String.Empty);
                        autoResetEvent.Set();
                    });
                    autoResetEvent.WaitOne();
                }

                // Clear the child nodes to make a fresh start
                if (Deployment.Current.Dispatcher.CheckAccess())
                {
                    SetEmptyContentTemplate(true, this.CurrentRootNode);
                    this.ChildNodes.Clear();}
                    
                else
                {
                    var autoResetEvent = new AutoResetEvent(false);
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        try
                        {
                            SetEmptyContentTemplate(true, this.CurrentRootNode);
                            this.ChildNodes.Clear();
                        }
                        catch (Exception) { }
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
                        try { SetView(UiService.GetViewMode(CurrentRootNode.Handle, CurrentRootNode.Name)); }
                        catch (Exception) { }
                        autoResetEvent.Set();
                    });
                    autoResetEvent.WaitOne();
                }

                // Build the bread crumbs. Do this before loading the nodes so that the user can click on home
                if (Deployment.Current.Dispatcher.CheckAccess())
                     CalculateBreadCrumbs(this.CurrentRootNode);
                else
                {
                    var autoResetEvent = new AutoResetEvent(true);
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        try { CalculateBreadCrumbs(this.CurrentRootNode); }
                        catch (Exception) { }
                        autoResetEvent.Set();
                    });
                    autoResetEvent.WaitOne();
                }

                // Create the possibility to cancel the loadnodes task
                cancellationTokenSource = new CancellationTokenSource();
                cancellationToken = cancellationTokenSource.Token;
            }
            catch (Exception) { }
            
            Task.Factory.StartNew(() =>
            {
                int viewport = 0;
                int background = 0;

                // Each view has different performance options
                switch (ViewMode)
                {
                    case ViewMode.ListView:
                        viewport = 256;
                        background = 1024;
                        break;
                    case ViewMode.LargeThumbnails:
                        viewport = 128;
                        background = 512;
                        break;
                    case ViewMode.SmallThumbnails:
                        viewport = 72;
                        background = 512;
                        break;
                }


                var helperList = new List<NodeViewModel>((int) ViewMode);
                for (int i = 0; i < listSize; i++)
                {
                    if (cancellationToken.IsCancellationRequested) return;

                    // To avoid pass null values to CreateNew
                    if (nodeList.get(i) == null) continue;
                                        
                    var node = NodeService.CreateNew(this.MegaSdk, this.AppInformation, nodeList.get(i), ChildNodes);
                    if (node == null) continue;

                    if (DriveDisplayMode == DriveDisplayMode.MoveItem && FocusedNode != null &&
                        node.OriginalMNode.getBase64Handle() == FocusedNode.OriginalMNode.getBase64Handle())
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
                                try { ChildNodes.Add(n); }
                                catch (Exception) { }
                            });
                            waitHandleNodes.Set();
                        });
                        waitHandleNodes.WaitOne();

                        //// Remove the busy indication
                        //var waitHandleProgress = new AutoResetEvent(false);
                        //Deployment.Current.Dispatcher.BeginInvoke(() =>
                        //{
                        //    ProgressService.SetProgressIndicator(false);
                        //    waitHandleProgress.Set();
                        //});
                        //waitHandleProgress.WaitOne();

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
                                try { ChildNodes.Add(n); }
                                catch (Exception) { }
                            });
                            waitHandleProgress.Set();
                        });
                        waitHandleProgress.WaitOne();
                        try { helperList.Clear(); }
                        catch (Exception) { }
                    }
                }

                var autoResetEvent = new AutoResetEvent(false);
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    helperList.ForEach(n =>
                    {
                        if (cancellationToken.IsCancellationRequested) return;
                        try { ChildNodes.Add(n); }
                        catch (Exception) { }
                    });
                    autoResetEvent.Set();
                });
                autoResetEvent.WaitOne();

               
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    SetEmptyContentTemplate(false, this.CurrentRootNode);
                    ProgressService.SetProgressIndicator(false);
                });

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
                    this.NoFolderUpAction = true;
                    FocusedNode = node;

                    if (node.IsImage)
                        NavigateService.NavigateTo(typeof(PreviewImagePage), NavigationParameter.Normal);
                    else
                    {
                        //var fileNode = node as FileNodeViewModel;
                        
                        //if (fileNode != null && fileNode.IsDownloadAvailable)
                        //{
                        //    if (File.Exists(fileNode.PublicFilePath))
                        //        fileNode.OpenFile();
                        //    break;
                        //}
                        FocusedNode.Download(App.MegaTransfers);
                    }

                    break;
                }
            }
        }

        public void ChangeDrive(bool changeToCloudDrive)
        {
            MNode rootNode;

            if (changeToCloudDrive)
            {
                rootNode = MegaSdk.getRootNode();
                DriveDisplayMode = DriveDisplayMode.CloudDrive;
            }
            else
            {
                rootNode = MegaSdk.getRubbishNode();
                DriveDisplayMode = DriveDisplayMode.RubbishBin;
            }

            if (rootNode == null) return;

            var node = NodeService.CreateNew(App.MegaSdk, App.AppInformation, rootNode);
            App.CloudDrive.CurrentRootNode = node;
            App.CloudDrive.BreadCrumbNode = node;

            CancelLoadNodes();
            LoadNodes();
        }

        public void ShowUploadOptions()
        {
            // Only 1 dialog can be open at the same time with ShowAsync.
            if (PickerOrDialogIsOpen) return;

            PickerOrDialogIsOpen = true;
            DialogService.ShowUploadOptions(null);
        }

        public async void AddFolder(NodeViewModel parentNode)
        {
            if (!IsUserOnline()) return;

            // Only 1 RadInputPrompt can be open at the same time with ShowAsync.
            if (PickerOrDialogIsOpen) return;

            try
            {
                PickerOrDialogIsOpen = true;
                var inputPromptClosedEventArgs =await RadInputPrompt.ShowAsync(
                    new string[] {UiResources.Add.ToLower(), UiResources.Cancel.ToLower()}, UiResources.CreateFolder, 
                    vibrate: false);
                PickerOrDialogIsOpen = false;

                if (inputPromptClosedEventArgs == null || inputPromptClosedEventArgs.Result != DialogResult.OK) return;

                this.MegaSdk.createFolder(inputPromptClosedEventArgs.Text, parentNode.OriginalMNode,
                    new CreateFolderRequestListener());
            }
            catch (Exception)
            {
                MessageBox.Show(AppMessages.FolderCreateFailed, AppMessages.FolderCreateFailed_Title,
                    MessageBoxButton.OK);
            }
            finally
            {
                PickerOrDialogIsOpen = false;
            }

           
        }

        public void FetchNodes(NodeViewModel rootRefreshNode = null)
        {
            CancelLoadNodes();

            Deployment.Current.Dispatcher.BeginInvoke(() =>SetEmptyContentTemplate(true));

            //var fetchNodesRequestListener = new FetchNodesRequestListener(this, rootRefreshNode, ShortCutHandle);
            ShortCutHandle = null;
            //this.MegaSdk.fetchNodes(fetchNodesRequestListener);
        }

        public void SelectFolder(NodeViewModel selectedNode)
        {
            CancelLoadNodes();

            this.CurrentRootNode = selectedNode;
            this.CurrentRootNode.ChildCollection = ChildNodes;

            // Create unique uri string to navigate
            NavigateService.NavigateTo(typeof(MainPage), NavigationParameter.Browsing, new Dictionary<string, string> {{"Id", Guid.NewGuid().ToString("N")}});
        }

        public void CalculateBreadCrumbs(NodeViewModel currentRootNode)
        {
            this.BreadCrumbs.Clear();

            if (currentRootNode == null || currentRootNode.Type == MNodeType.TYPE_ROOT || 
                currentRootNode.Type == MNodeType.TYPE_RUBBISH) 
                return;

            this.BreadCrumbs.Add(currentRootNode);

            MNode parentNode = currentRootNode.OriginalMNode;
            parentNode = this.MegaSdk.getParentNode(parentNode);
            while ((parentNode != null) && (parentNode.getType() != MNodeType.TYPE_ROOT) && 
                (parentNode.getType() != MNodeType.TYPE_RUBBISH))
            {
                this.BreadCrumbs.Insert(0, NodeService.CreateNew(this.MegaSdk, this.AppInformation, parentNode));
                parentNode = this.MegaSdk.getParentNode(parentNode);
            }

        }
        public void SetMultiSelect(bool isMultiSelectActive)
        {
            if (isMultiSelectActive)
                this.MultiSelectStateButtonForeGroundColor = (SolidColorBrush) Application.Current.Resources["MegaRedSolidColorBrush"];
            else
                this.MultiSelectStateButtonForeGroundColor = new SolidColorBrush(Colors.White);
        }

        #endregion

        #region Private Methods

        private void UpgradeAccount(object obj)
        {
            NavigateService.NavigateTo(typeof(MyAccountPage), NavigationParameter.Normal);
        }

        private void MultiSelect(object obj)
        {
            this.IsMultiSelectActive = !this.IsMultiSelectActive;
        }

        private void GetPreviewLink(object obj)
        {
            if (!IsUserOnline()) return;

            FocusedNode.GetLink();
        }

        private void DownloadItem(object obj)
        {
            this.NoFolderUpAction = true;
            FocusedNode.Download(App.MegaTransfers);
        }

        private void RemoveItem(object obj)
        {
            FocusedNode.Remove(false);
        }

        private void RenameItem(object obj)
        {
            FocusedNode.Rename();
        }

        private void CancelLoadNodes(bool clearChilds = true)
        {
            if (cancellationToken.CanBeCanceled)
                if (cancellationTokenSource != null)
                    cancellationTokenSource.Cancel();

            if (clearChilds)
            {
                if (Deployment.Current.Dispatcher.CheckAccess())
                    this.ChildNodes.Clear();
                else
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() => this.ChildNodes.Clear()); 
                }
            }
        }

        private void SetEmptyContentTemplate(bool isLoading, NodeViewModel currentRootNode = null)
        {
            if (ListBox == null) return;

            if (isLoading)
            {
                ListBox.EmptyContentTemplate =
                    (DataTemplate) Application.Current.Resources["MegaNodeListLoadingContent"];
            }
            else
            {
                var megaRoot = this.MegaSdk.getRootNode();

                if (currentRootNode != null && megaRoot != null && currentRootNode.Handle.Equals(megaRoot.getHandle()))
                {
                    ListBox.EmptyContentTemplate =
                        (DataTemplate)Application.Current.Resources["MegaNodeListCloudDriveEmptyContent"];
                }
                else
                {
                    ListBox.EmptyContentTemplate =
                        (DataTemplate)Application.Current.Resources["MegaNodeListEmptyContent"];
                }
            }
        }

        #endregion

        #region Properties
        
        public ObservableCollection<IMegaNode> ChildNodes { get; set; }
        public ObservableCollection<IMegaNode> BreadCrumbs { get; set; }

        private NodeViewModel _currentRootNode;
        public NodeViewModel CurrentRootNode
        {
            get { return _currentRootNode; }
            set
            {
                _currentRootNode = value;
                OnPropertyChanged("CurrentRootNode");
            }
        }

        public NodeViewModel FocusedNode { get; set; }

        public MNode PublicNode { get; set; }

        public string LinkToImport { get; set; }

        public List<IMegaNode> SelectedNodes { get; set; } 

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

        private SolidColorBrush _multiSelectStateButtonForeGroundColor;
        public SolidColorBrush MultiSelectStateButtonForeGroundColor
        {
            get { return _multiSelectStateButtonForeGroundColor; }
            set
            {
                _multiSelectStateButtonForeGroundColor = value;
                OnPropertyChanged("MultiSelectStateButtonForeGroundColor");
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
                SetMultiSelect(_isMultiSelectActive);
                OnPropertyChanged("IsMultiSelectActive");
            }
        }



             

        #endregion
      
    }
}
