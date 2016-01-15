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
        //private CancellationTokenSource cancellationTokenSource;
        //private CancellationToken cancellationToken;
        //private bool asyncInputPromptDialogIsOpen;

        //public event EventHandler<CommandStatusArgs> CommandStatusChanged;

        public RadDataBoundListBox ListBox { private get; set; }

        public bool PickerOrDialogIsOpen { get; set; }

        public CloudDriveViewModel(MegaSDK megaSdk, AppInformation appInformation)
            : base(megaSdk, appInformation)
        {
            //this.DriveDisplayMode = DriveDisplayMode.CloudDrive;
            this.CurrentRootNode = null;
            //this.BreadCrumbNode = null;
            //this.ChildNodes = new ObservableCollection<IMegaNode>();
            //this.BreadCrumbs = new ObservableCollection<IMegaNode>();
            //this.SelectedNodes = new List<IMegaNode>();
            //this.IsMultiSelectActive = false;
            //SetViewDefaults();

            //this.RemoveItemCommand = new DelegateCommand(this.RemoveItem);
            //this.RenameItemCommand = new DelegateCommand(this.RenameItem);
            //this.GetPreviewLinkItemCommand = new DelegateCommand(this.GetPreviewLink);
            //this.DownloadItemCommand = new DelegateCommand(this.DownloadItem);
            //this.CreateShortCutCommand = new DelegateCommand(this.CreateShortCut);
            //this.ChangeViewCommand = new DelegateCommand(this.ChangeView);
            //this.MultiSelectCommand = new DelegateCommand(this.MultiSelect);
            //this.UpgradeAccountCommand = new DelegateCommand(this.UpgradeAccount);
        }

        #region Commands

        //public ICommand RemoveItemCommand { get; set; }
        //public ICommand GetPreviewLinkItemCommand { get; set; }
        //public ICommand DownloadItemCommand { get; set; }
        //public ICommand RenameItemCommand { get; set; }
        //public ICommand CreateShortCutCommand { get; set; }
        //public ICommand ChangeViewCommand { get; set; }
        //public ICommand MultiSelectCommand { get; set; }
        //public ICommand UpgradeAccountCommand { get; set; }

        #endregion

        //#region Events

        //private void OnCommandStatusChanged(bool status)
        //{
        //    if (CommandStatusChanged == null) return;

        //    CommandStatusChanged(this, new CommandStatusArgs(status));
        //}

        //#endregion

        #region Services


        #endregion

        #region Public Methods

        //public void SetCommandStatus(bool status)
        //{
        //    OnCommandStatusChanged(status);
        //}

        //public void TranslateAppBar(IList iconButtons, IList menuItems, MenuType menuType)
        //{
        //    switch (menuType)
        //    {
        //        case MenuType.CloudDriveMenu:
        //        {
        //            ((ApplicationBarIconButton)iconButtons[0]).Text = UiResources.Upload.ToLower();
        //            ((ApplicationBarIconButton)iconButtons[1]).Text = UiResources.AddFolder.ToLower();
        //            //((ApplicationBarIconButton)iconButtons[2]).Text = UiResources.Refresh.ToLower();
        //            ((ApplicationBarIconButton)iconButtons[2]).Text = UiResources.OpenLink.ToLower();
                    
        //            //((ApplicationBarMenuItem)menuItems[0]).Text = UiResources.MultiSelect.ToLower();
        //            //((ApplicationBarMenuItem)menuItems[1]).Text = UiResources.Transfers.ToLower();
        //            //((ApplicationBarMenuItem)menuItems[2]).Text = UiResources.MyAccount.ToLower();
        //            //((ApplicationBarMenuItem)menuItems[3]).Text = UiResources.Settings.ToLower();
        //            //((ApplicationBarMenuItem)menuItems[4]).Text = UiResources.About.ToLower(); 
        //            break;
        //        }
        //        case MenuType.RubbishBinMenu:
        //        {
        //            ((ApplicationBarIconButton)iconButtons[0]).Text = UiResources.Refresh.ToLower();

        //            ((ApplicationBarMenuItem)menuItems[0]).Text = UiResources.CloudDriveName.ToLower();
        //            //((ApplicationBarMenuItem)menuItems[1]).Text = UiResources.MultiSelect.ToLower();
        //            //((ApplicationBarMenuItem)menuItems[2]).Text = UiResources.Transfers.ToLower();
        //            //((ApplicationBarMenuItem)menuItems[3]).Text = UiResources.MyAccount.ToLower();
        //            //((ApplicationBarMenuItem)menuItems[4]).Text = UiResources.Settings.ToLower();
        //            //((ApplicationBarMenuItem)menuItems[5]).Text = UiResources.About.ToLower();
        //            break;
        //        }
        //        case MenuType.MoveMenu:
        //        {
        //            ((ApplicationBarIconButton)iconButtons[0]).Text = UiResources.Move.ToLower();
        //            ((ApplicationBarIconButton)iconButtons[1]).Text = UiResources.Cancel.ToLower();
                  
        //            break;
        //        }
        //        case MenuType.MultiSelectMenu:
        //        {
        //            ((ApplicationBarIconButton)iconButtons[0]).Text = UiResources.Download.ToLower();
        //            ((ApplicationBarIconButton)iconButtons[1]).Text = UiResources.Move.ToLower();
        //            ((ApplicationBarIconButton)iconButtons[2]).Text = UiResources.Remove.ToLower();
        //            break;
        //        }
        //        case MenuType.ImportMenu:
        //        {
        //            ((ApplicationBarIconButton)iconButtons[0]).Text = UiResources.LinkOptions.ToLower();
        //            ((ApplicationBarIconButton)iconButtons[1]).Text = UiResources.Cancel.ToLower();
        //            break;
        //        }
        //    }
           
        //}

       
        //    this.IsMultiSelectActive = false;

        //    return true;
        //}

        //public bool SelectMultipleMove()
        //{
        //    int count = ChildNodes.Count(n => n.IsMultiSelected);

        //    if (count < 1) return false;

        //    SelectedNodes.Clear();

        //    foreach (var node in ChildNodes.Where(n => n.IsMultiSelected))
        //    {
        //        node.DisplayMode = NodeDisplayMode.SelectedForMove;
        //        SelectedNodes.Add(node);
        //    }

        //    this.IsMultiSelectActive = false;
        //    OldDriveDisplayMode = DriveDisplayMode;
        //    DriveDisplayMode = DriveDisplayMode.MoveItem;

        //    return true;
        //}

        //public async void MultipleDownload(StorageFolder downloadFolder = null)
        //{
        //    int count = ChildNodes.Count(n => n.IsMultiSelected);

        //    if (count < 1) return;
            
        //    #if WINDOWS_PHONE_80
        //    if (!SettingsService.LoadSetting<bool>(SettingsResources.QuestionAskedDownloadOption, false))
        //    {
        //        switch (await DialogService.ShowOptionsDialog(AppMessages.QuestionAskedDownloadOption_Title, 
        //            AppMessages.QuestionAskedDownloadOption,
        //            new[] { AppMessages.QuestionAskedDownloadOption_YesButton, AppMessages.QuestionAskedDownloadOption_NoButton }))
        //        {
        //            case -1:
        //            {
        //                return;
        //            }
        //            case 0:
        //            {
        //                SettingsService.SaveSetting(SettingsResources.ExportImagesToPhotoAlbum, true);
        //                break;
        //            }
        //            case 1:
        //            {
        //                SettingsService.SaveSetting(SettingsResources.ExportImagesToPhotoAlbum, false);
        //                break;
        //            }
        //        }
        //        SettingsService.SaveSetting(SettingsResources.QuestionAskedDownloadOption, true);
        //    }
        //    #elif WINDOWS_PHONE_81
        //    // Only 1 Folder Picker can be open at 1 time
        //    if (PickerOrDialogIsOpen) return;

        //    if (downloadFolder == null)
        //    {
        //        PickerOrDialogIsOpen = true;
        //        if (!await FolderService.SelectDownloadFolder())return;
        //    }
        //    #endif

        //    ProgressService.SetProgressIndicator(true, ProgressMessages.PrepareDownloads);

        //    // Give the app the time to display the progress indicator
        //    await Task.Delay(5);
           
        //    // First count the number of downloads before proceeding to the transfers.
        //    int downloadCount = 0;
        //    var downloadNodes = new List<IMegaNode>();

        //    foreach (var node in ChildNodes.Where(n => n.IsMultiSelected))
        //    {
        //        // If selected file is a folder then also select it childnodes to download
        //        var folderNode = node as FolderNodeViewModel;
        //        if (folderNode != null)
        //        {
        //            List<NodeViewModel> recursiveNodes = NodeService.GetRecursiveNodes(MegaSdk, AppInformation, folderNode);
        //            foreach (var recursiveNode in recursiveNodes)
        //            {
        //                downloadNodes.Add(recursiveNode);
        //                downloadCount++;
        //            }
        //        }
        //        else
        //        {
        //            // No folder then just add node to transferlist
        //            downloadNodes.Add(node);
        //            downloadCount++;
        //        }
               
        //    }

        //    if (!DownloadLimitCheck(downloadCount))
        //    {
        //        ProgressService.SetProgressIndicator(false);
        //        return;
        //    }

        //    downloadNodes.ForEach(n =>
        //    {
        //        if (downloadFolder != null)
        //            n.Transfer.DownloadFolderPath = downloadFolder.Path;
        //        App.MegaTransfers.Add(n.Transfer);
        //        n.Transfer.StartTransfer();
        //    });

        //    ProgressService.SetProgressIndicator(false);

        //    this.IsMultiSelectActive = false;
        //    this.NoFolderUpAction = true;
        //    NavigateService.NavigateTo(typeof(TransferPage), NavigationParameter.Downloads);
        //}

        

        //private void CreateShortCut(object obj)
        //{
        //    var shortCutTile = new RadIconicTileData()
        //    {
        //        IconImage = new Uri("/Assets/Tiles/FolderIconImage.png", UriKind.Relative),
        //        SmallIconImage = new Uri("/Assets/Tiles/FolderSmallIconImage.png", UriKind.Relative),
        //        Title = FocusedNode.Name
        //    };

        //    LiveTileHelper.CreateOrUpdateTile(shortCutTile,
        //        new Uri("/Pages/MainPage.xaml?ShortCutHandle=" + FocusedNode.OriginalMNode.getHandle(), UriKind.Relative),
        //        false);
        //}

        //public bool HasChildNodes()
        //{
        //    return ChildNodes.Count > 0;
        //}

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
                new CustomMessageDialog(
                        AppMessages.PhotoUploadError_Title,
                        AppMessages.PhotoUploadError,
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
            }
        }

        //public void GoFolderUp()
        //{
        //    CancelLoadNodes();

        //    MNode parentNode = null;
            
        //    if (this.CurrentRootNode != null)
        //        parentNode = this.MegaSdk.getParentNode(this.CurrentRootNode.OriginalMNode);

        //    if (parentNode == null || parentNode.getType() == MNodeType.TYPE_UNKNOWN )
        //        parentNode = this.MegaSdk.getRootNode();
            
        //    this.CurrentRootNode = NodeService.CreateNew(this.MegaSdk, this.AppInformation, parentNode, ChildNodes);
        //}

        //public void SetView(ViewMode viewMode)
        //{            
        //    switch (viewMode)
        //    {
        //        case ViewMode.LargeThumbnails:
        //            {
        //                ListBox.VirtualizationStrategyDefinition = new WrapVirtualizationStrategyDefinition()
        //                {
        //                    Orientation = Orientation.Horizontal,
        //                    WrapLineAlignment = WrapLineAlignment.Near
        //                };

        //                this.NodeTemplateSelector = new NodeTemplateSelector()
        //                {
        //                    FileItemTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListLargeViewFileItemContent"],
        //                    FolderItemTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListLargeViewFolderItemContent"]
        //                };

        //                this.ViewMode = ViewMode.LargeThumbnails;
        //                this.ViewStateButtonIconUri = new Uri("/Assets/Images/large grid view.Screen-WXGA.png", UriKind.Relative);

        //                if (ListBox != null) ListBox.CheckBoxStyle = (Style)Application.Current.Resources["MultiSelectItemCheckBoxStyle"];
        //                //this.MultiSelectCheckBoxStyle = (Style)Application.Current.Resources["MultiSelectItemCheckBoxStyle"];
                        
        //                break;
        //            }
        //        case ViewMode.SmallThumbnails:
        //            {
        //                ListBox.VirtualizationStrategyDefinition = new WrapVirtualizationStrategyDefinition()
        //                {
        //                    Orientation = Orientation.Horizontal,
        //                    WrapLineAlignment = WrapLineAlignment.Near
        //                };

        //                this.NodeTemplateSelector = new NodeTemplateSelector()
        //                {
        //                    FileItemTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListSmallViewFileItemContent"],
        //                    FolderItemTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListSmallViewFolderItemContent"]
        //                };

        //                this.ViewMode = ViewMode.SmallThumbnails;
        //                this.ViewStateButtonIconUri = new Uri("/Assets/Images/small grid view.Screen-WXGA.png", UriKind.Relative);

        //                if (ListBox != null) ListBox.CheckBoxStyle = (Style)Application.Current.Resources["MultiSelectItemCheckBoxStyle"];
        //                //this.MultiSelectCheckBoxStyle = (Style)Application.Current.Resources["MultiSelectItemCheckBoxStyle"];
                       
        //                break;
        //            }
        //        case ViewMode.ListView:
        //            {
        //                SetViewDefaults();
        //                break;
        //            }
        //    }
        //}

        //private void ChangeView(object obj)
        //{
        //    if (CurrentRootNode == null) return;

        //    switch (this.ViewMode)
        //    {
        //        case ViewMode.ListView:
        //            {
        //                SetView(ViewMode.LargeThumbnails);
        //                UiService.SetViewMode(CurrentRootNode.Handle, ViewMode.LargeThumbnails);
        //                break;
        //            }
        //        case ViewMode.LargeThumbnails:
        //            {
        //                SetView(ViewMode.SmallThumbnails);
        //                UiService.SetViewMode(CurrentRootNode.Handle, ViewMode.SmallThumbnails);
        //                break;
        //            }
        //        case ViewMode.SmallThumbnails:
        //            {
        //                SetView(ViewMode.ListView);
        //                UiService.SetViewMode(CurrentRootNode.Handle, ViewMode.ListView);
        //                break;
        //            }
        //    }
        //}

        //private void SetViewDefaults()
        //{
        //    if (ListBox != null)
        //        ListBox.VirtualizationStrategyDefinition = new StackVirtualizationStrategyDefinition()
        //        {
        //            Orientation = Orientation.Vertical
        //        };

        //    this.NodeTemplateSelector = new NodeTemplateSelector()
        //    {
        //        FileItemTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListFileItemContent"],
        //        FolderItemTemplate = (DataTemplate)Application.Current.Resources["MegaNodeListFolderItemContent"]
        //    };

        //    this.ViewMode = ViewMode.ListView;
        //    this.ViewStateButtonIconUri = new Uri("/Assets/Images/list view.Screen-WXGA.png", UriKind.Relative);

        //    if (ListBox != null) ListBox.CheckBoxStyle = (Style) Application.Current.Resources["DefaultCheckBoxStyle"];
        //    //this.MultiSelectCheckBoxStyle = (Style)Application.Current.Resources["DefaultCheckBoxStyle"];
        //}

        //public void GoToFolder(NodeViewModel folder)
        //{
        //    if (folder == null) return;

        //    CancelLoadNodes();

        //    this.BreadCrumbNode = this.CurrentRootNode;
        //    this.CurrentRootNode = folder;
        //    this.CurrentRootNode.ChildCollection = ChildNodes;
            
        //    NavigateService.NavigateTo(typeof(MainPage), NavigationParameter.BreadCrumb, new Dictionary<string, string> { { "Id", Guid.NewGuid().ToString("N") } });
        //}

        //public void GoToRoot()
        //{
        //    GoToFolder(NodeService.CreateNew(this.MegaSdk, this.AppInformation, MegaSdk.getRootNode()));
        //}

        //public void GoToAccountDetails()
        //{
        //    this.NoFolderUpAction = true;
        //    NavigateService.NavigateTo(typeof(MyAccountPage), NavigationParameter.Normal);
        //}

        //public void GoToTransfers()
        //{
        //    this.NoFolderUpAction = true;
        //    NavigateService.NavigateTo(typeof(TransferPage), NavigationParameter.Normal);
        //}


        #endregion

        #region Private Methods

        //private void UpgradeAccount(object obj)
        //{
        //    NavigateService.NavigateTo(typeof(MyAccountPage), NavigationParameter.Normal);
        //}

        //private void MultiSelect(object obj)
        //{
        //    this.IsMultiSelectActive = !this.IsMultiSelectActive;
        //}

        //private void GetPreviewLink(object obj)
        //{
        //    if (!IsUserOnline()) return;

        //    FocusedNode.GetLink();
        //}

        //private void DownloadItem(object obj)
        //{
        //    this.NoFolderUpAction = true;
        //    FocusedNode.Download(App.MegaTransfers);
        //}

        //private void RemoveItem(object obj)
        //{
        //    FocusedNode.Remove(false);
        //}

        //private void RenameItem(object obj)
        //{
        //    FocusedNode.Rename();
        //}

        //private void CancelLoadNodes(bool clearChilds = true)
        //{
        //    if (cancellationToken.CanBeCanceled)
        //        if (cancellationTokenSource != null)
        //            cancellationTokenSource.Cancel();

        //    if (clearChilds)
        //    {
        //        if (Deployment.Current.Dispatcher.CheckAccess())
        //            this.ChildNodes.Clear();
        //        else
        //        {
        //            Deployment.Current.Dispatcher.BeginInvoke(() => this.ChildNodes.Clear()); 
        //        }
        //    }
        //}

        //private void SetEmptyContentTemplate(bool isLoading, NodeViewModel currentRootNode = null)
        //{
        //    if (ListBox == null) return;

        //    if (isLoading)
        //    {
        //        ListBox.EmptyContentTemplate =
        //            (DataTemplate) Application.Current.Resources["MegaNodeListLoadingContent"];
        //    }
        //    else
        //    {
        //        var megaRoot = this.MegaSdk.getRootNode();

        //        if (currentRootNode != null && megaRoot != null && currentRootNode.Handle.Equals(megaRoot.getHandle()))
        //        {
        //            ListBox.EmptyContentTemplate =
        //                (DataTemplate)Application.Current.Resources["MegaNodeListCloudDriveEmptyContent"];
        //        }
        //        else
        //        {
        //            ListBox.EmptyContentTemplate =
        //                (DataTemplate)Application.Current.Resources["MegaNodeListEmptyContent"];
        //        }
        //    }
        //}

        #endregion

        #region Properties
        
        //public ObservableCollection<IMegaNode> ChildNodes { get; set; }
        //public ObservableCollection<IMegaNode> BreadCrumbs { get; set; }

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

        //public NodeViewModel FocusedNode { get; set; }

        //public MNode PublicNode { get; set; }

        //public string LinkToImport { get; set; }

        //public List<IMegaNode> SelectedNodes { get; set; } 

        //public NodeViewModel BreadCrumbNode { get; set; }

        public bool NoFolderUpAction { get; set; }
        
        //public DriveDisplayMode DriveDisplayMode { get; set; }
        //public DriveDisplayMode OldDriveDisplayMode { get; set; }

        //public ulong? ShortCutHandle { get; set; }

        //public ViewMode ViewMode { get; set; }

        //private VirtualizationStrategyDefinition _virtualizationStrategy;
        //public VirtualizationStrategyDefinition VirtualizationStrategy
        //{
        //    get { return _virtualizationStrategy; }
        //    set
        //    {
        //        _virtualizationStrategy = value;
        //        OnPropertyChanged("VirtualizationStrategy");
        //    }
        //}

        //private DataTemplateSelector _nodeTemplateSelector;
        //public DataTemplateSelector NodeTemplateSelector
        //{
        //     get { return _nodeTemplateSelector; }
        //    set
        //    {
        //        _nodeTemplateSelector = value;
        //        OnPropertyChanged("NodeTemplateSelector");
        //    }
        //}

        //private Uri _viewStateButtonIconUri;
        //public Uri ViewStateButtonIconUri
        //{
        //    get { return _viewStateButtonIconUri; }
        //    set
        //    {
        //        _viewStateButtonIconUri = value;
        //        OnPropertyChanged("ViewStateButtonIconUri");
        //    }
        //}

        //private SolidColorBrush _multiSelectStateButtonForeGroundColor;
        //public SolidColorBrush MultiSelectStateButtonForeGroundColor
        //{
        //    get { return _multiSelectStateButtonForeGroundColor; }
        //    set
        //    {
        //        _multiSelectStateButtonForeGroundColor = value;
        //        OnPropertyChanged("MultiSelectStateButtonForeGroundColor");
        //    }
        //}

        //private Style _multiSelectCheckBoxStyle;
        //public Style MultiSelectCheckBoxStyle
        //{
        //    get { return _multiSelectCheckBoxStyle; }
        //    set
        //    {
        //        _multiSelectCheckBoxStyle = value;
        //        OnPropertyChanged("MultiSelectCheckBoxStyle");
        //    }
        //}

        //private bool _isMultiSelectActive;
        //public bool IsMultiSelectActive
        //{
        //    get { return _isMultiSelectActive; }
        //    set
        //    {
        //        _isMultiSelectActive = value;
        //        SetMultiSelect(_isMultiSelectActive);
        //        OnPropertyChanged("IsMultiSelectActive");
        //    }
        //}             

        #endregion      
    }
}
