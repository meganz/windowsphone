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

        public bool IsNetworkAvailable
        {
            get { return NetworkService.IsNetworkAvailable(); }
        }

        private CancellationTokenSource LoadingCancelTokenSource { get; set; }
        private CancellationToken LoadingCancelToken { get; set; }

        private FolderViewModel _inShares;
        public FolderViewModel InShares
        {
            get { return _inShares; }
            set
            {
                SetField(ref _inShares, value);
                OnPropertyChanged("HasInSharedFolders");
                OnPropertyChanged("NumberOfInSharedFolders");
                OnPropertyChanged("NumberOfInSharedFoldersText");
            }
        }

        public bool HasInSharedFolders
        {
            get { return InShares.ChildNodes.Count > 0; }
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
                OnPropertyChanged("HasOutSharedFolders");
                OnPropertyChanged("NumberOfOutSharedFolders");
                OnPropertyChanged("NumberOfOutSharedFoldersText");
            }
        }

        public bool HasOutSharedFolders
        {
            get { return OutShares.ChildNodes.Count > 0; }
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

        public void NetworkAvailabilityChanged()
        {
            OnUiThread(() => OnPropertyChanged("IsNetworkAvailable"));
        }

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

        public void ClearIncomingSharedFolders()
        {
            InShares.ClearChildNodes();

            OnUiThread(() =>
            {
                OnPropertyChanged("HasInSharedFolders");
                OnPropertyChanged("NumberOfInSharedFolders");
                OnPropertyChanged("NumberOfInSharedFoldersText");
            });
        }

        public void GetIncomingSharedFolders()
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return;

            // First cancel any other loading task that is busy
            CancelLoad();

            // Create the option to cancel
            CreateLoadCancelOption();

            InShares.SetProgressIndication(true);

            // Process is started so we can set the empty content template to loading already
            InShares.SetEmptyContentTemplate(true);

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

                            var _inSharedFolder = NodeService.CreateNew(this.MegaSdk, this.AppInformation, inSharesList.get(i), ContainerType.InShares, InShares.ChildNodes);
                            _inSharedFolder.DefaultImagePathData = VisualResources.FolderTypePath_shared;
                            InShares.ChildNodes.Add(_inSharedFolder);
                        }

                        // Show the user that processing the childnodes is done
                        InShares.SetProgressIndication(false);

                        // Set empty content to folder instead of loading view
                        InShares.SetEmptyContentTemplate(false);

                        OnUiThread(() =>
                        {
                            OnPropertyChanged("HasInSharedFolders");
                            OnPropertyChanged("NumberOfInSharedFolders");
                            OnPropertyChanged("NumberOfInSharedFoldersText");                            
                        });
                    });
                }
                catch (OperationCanceledException)
                {
                    // Do nothing. Just exit this background process because a cancellation exception has been thrown
                }

            }, LoadingCancelToken, TaskCreationOptions.PreferFairness, TaskScheduler.Current);
        }

        public void ClearOutgoingSharedFolders()
        {
            OutShares.ClearChildNodes();

            OnUiThread(() =>
            {
                OnPropertyChanged("HasOutSharedFolders");
                OnPropertyChanged("NumberOfOutSharedFolders");
                OnPropertyChanged("NumberOfOutSharedFoldersText");
            });
        }

        public void GetOutgoingSharedFolders()
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return;

            // First cancel any other loading task that is busy
            CancelLoad();

            // Create the option to cancel
            CreateLoadCancelOption();

            OutShares.SetProgressIndication(true);

            // Process is started so we can set the empty content template to loading already
            OutShares.SetEmptyContentTemplate(true);

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
                            if ((outSharesList.get(i) != null) && MegaSdk.isOutShare(MegaSdk.getNodeByHandle(outSharesList.get(i).getNodeHandle())))
                            {
                                // To avoid repeated values, folders shared with more than one user
                                MNode node = MegaSdk.getNodeByHandle(outSharesList.get(i).getNodeHandle());
                                if(lastFolderHandle != sharedNode.getNodeHandle())
                                {
                                    lastFolderHandle = sharedNode.getNodeHandle();

                                    var _outSharedFolder = NodeService.CreateNew(this.MegaSdk, this.AppInformation, MegaSdk.getNodeByHandle(lastFolderHandle), ContainerType.OutShares, OutShares.ChildNodes);
                                    _outSharedFolder.DefaultImagePathData = VisualResources.FolderTypePath_shared;
                                    OutShares.ChildNodes.Add(_outSharedFolder);                                    
                                }
                            }
                        }

                        // Show the user that processing the childnodes is done
                        OutShares.SetProgressIndication(false);

                        // Set empty content to folder instead of loading view
                        OutShares.SetEmptyContentTemplate(false);

                        OnUiThread(() =>
                        {
                            OnPropertyChanged("HasOutSharedFolders");
                            OnPropertyChanged("NumberOfOutSharedFolders");
                            OnPropertyChanged("NumberOfOutSharedFoldersText");
                        });                        
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
