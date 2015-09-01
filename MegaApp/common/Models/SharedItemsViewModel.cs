using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.Models
{
    class SharedItemsViewModel : BaseAppInfoAwareViewModel
    {
        public SharedItemsViewModel(MegaSDK megaSdk, AppInformation appInformation)
            : base(megaSdk, appInformation)
        {
            UpdateUserData();

            InitializeModel();

            InitializeMenu(HamburgerMenuItemType.SharedItems);            
        }

        #region Properties

        private CancellationTokenSource LoadingCancelTokenSource { get; set; }
        private CancellationToken LoadingCancelToken { get; set; }

        private FolderViewModel _inShares;
        public FolderViewModel InShares
        {
            get { return _inShares; }
            set
            {
                SetField(ref _inShares, value);
                OnPropertyChanged("NumberOfInSharedFolders");
                OnPropertyChanged("NumberOfInSharedFoldersText");
            }
        }

        public int NumberOfInSharedFolders
        {
            get { return InShares.ChildNodes.Count; }
        }

        public String NumberOfInSharedFoldersText
        {
            get
            {
                if (NumberOfInSharedFolders != 0)
                    return NumberOfInSharedFolders.ToString();
                else
                    return UiResources.No.ToLower();
            }
        }

        private FolderViewModel _outShares;
        public FolderViewModel OutShares
        {
            get { return _outShares; }
            set
            {
                SetField(ref _outShares, value);
                OnPropertyChanged("NumberOfOutSharedFolders");
                OnPropertyChanged("NumberOfOutSharedFoldersText");
            }
        }

        public int NumberOfOutSharedFolders
        {
            get { return OutShares.ChildNodes.Count; }
        }

        public String NumberOfOutSharedFoldersText
        {
            get
            {
                if (NumberOfOutSharedFolders != 0)
                    return NumberOfOutSharedFolders.ToString();
                else
                    return UiResources.No.ToLower();
            }
        }

        private FolderViewModel _activeSharedFolderView;
        public FolderViewModel ActiveSharedFolderView
        {
            get { return _activeSharedFolderView; }
            set { SetField(ref _activeSharedFolderView, value); }
        }

        private bool _isInSharedItemsRootListView;
        public bool IsInSharedItemsRootListView
        {
            get { return _isInSharedItemsRootListView; }
            set
            {
                _isInSharedItemsRootListView = value;
                OnPropertyChanged("IsInSharedItemsRootListView");
            }
        }

        private bool _isOutSharedItemsRootListView;
        public bool IsOutSharedItemsRootListView
        {
            get { return _isOutSharedItemsRootListView; }
            set
            {
                _isOutSharedItemsRootListView = value;
                OnPropertyChanged("IsOutSharedItemsRootListView");
            }
        }

        #endregion

        #region Methods

        private void InitializeModel()
        {
            this.InShares = new FolderViewModel(this.MegaSdk, this.AppInformation, ContainerType.InShares);
            this.OutShares = new FolderViewModel(this.MegaSdk, this.AppInformation, ContainerType.OutShares);

            // The In Shared Folders is always the first active folder on initalization
            this.ActiveSharedFolderView = this.InShares;
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

        /// <summary>
        /// Cancel any running load process of contacts
        /// </summary>
        public void CancelLoad()
        {
            if (this.LoadingCancelTokenSource != null && LoadingCancelToken.CanBeCanceled)
                LoadingCancelTokenSource.Cancel();
        }

        public void GetIncomingSharedFolders()
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return;

            // First cancel any other loading task that is busy
            CancelLoad();

            // Create the option to cancel
            CreateLoadCancelOption();

            OnUiThread(() => InShares.ChildNodes.Clear());
            MNodeList inSharesList = MegaSdk.getInShares();

            Task.Factory.StartNew(() =>
            {
                try
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        for (int i = 0; i < inSharesList.size(); i++)
                        {
                            // If the task has been cancelled, stop processing
                            if (LoadingCancelToken.IsCancellationRequested)
                                LoadingCancelToken.ThrowIfCancellationRequested();

                            // To avoid null values
                            if (inSharesList.get(i) == null) continue;

                            var _inSharedFolder = NodeService.CreateNew(this.MegaSdk, this.AppInformation, inSharesList.get(i), InShares.ChildNodes);
                            _inSharedFolder.DefaultImagePathData = VisualResources.FolderTypePath_shared;
                            InShares.ChildNodes.Add(_inSharedFolder);
                        }

                        OnPropertyChanged("NumberOfInSharedFolders");
                        OnPropertyChanged("NumberOfInSharedFoldersText");
                    });
                }
                catch (OperationCanceledException)
                {
                    // Do nothing. Just exit this background process because a cancellation exception has been thrown
                }

            }, LoadingCancelToken, TaskCreationOptions.PreferFairness, TaskScheduler.Current);
        }

        public void GetOutgoingSharedFolders()
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return;

            // First cancel any other loading task that is busy
            CancelLoad();

            // Create the option to cancel
            CreateLoadCancelOption();

            OnUiThread(() => OutShares.ChildNodes.Clear());
            MShareList outSharesList = MegaSdk.getOutShares();

            Task.Factory.StartNew(() =>
            {
                try
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        ulong lastFolderHandle = 0;
                        for (int i = 0; i < outSharesList.size(); i++)
                        {
                            // If the task has been cancelled, stop processing
                            if (LoadingCancelToken.IsCancellationRequested)
                                LoadingCancelToken.ThrowIfCancellationRequested();

                            MShare sharedNode = outSharesList.get(i);

                            // To avoid null values and public links
                            if ((outSharesList.get(i) != null) && !String.IsNullOrWhiteSpace(outSharesList.get(i).getUser()))
                            {
                                // To avoid repeated values, folders shared with more than one user
                                MNode node = MegaSdk.getNodeByHandle(outSharesList.get(i).getNodeHandle());
                                if(lastFolderHandle != sharedNode.getNodeHandle())
                                {
                                    lastFolderHandle = sharedNode.getNodeHandle();

                                    var _outSharedFolder = NodeService.CreateNew(this.MegaSdk, this.AppInformation, MegaSdk.getNodeByHandle(lastFolderHandle), OutShares.ChildNodes);
                                    _outSharedFolder.DefaultImagePathData = VisualResources.FolderTypePath_shared;
                                    OutShares.ChildNodes.Add(_outSharedFolder);

                                    OnPropertyChanged("NumberOfOutSharedFolders");
                                    OnPropertyChanged("NumberOfOutSharedFoldersText");
                                }
                            }                                
                        }                        
                    });
                }
                catch (OperationCanceledException)
                {
                    // Do nothing. Just exit this background process because a cancellation exception has been thrown
                }

            }, LoadingCancelToken, TaskCreationOptions.PreferFairness, TaskScheduler.Current);
        }

        #endregion
    }
}
