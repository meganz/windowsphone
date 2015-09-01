using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.Models
{
    class ContactDetailsViewModel : BaseAppInfoAwareViewModel
    {
        public ContactDetailsViewModel(MegaSDK megaSdk, AppInformation appInformation)
            : base(megaSdk, appInformation)
        {
            UpdateUserData();
            
            InitializeMenu(HamburgerMenuItemType.Contacts);

            this.InShares = new FolderViewModel(this.MegaSdk, this.AppInformation, ContainerType.ContactInShares);
        }        

        #region Properties

        private Contact _selectedContact;
        public Contact SelectedContact
        {
            get { return _selectedContact; }
            set
            {
                _selectedContact = value;
                OnPropertyChanged("SelectedContact");
            }
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
        
        #endregion

        #region Methods

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

        public void GetContactSharedFolders()
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return;

            // First cancel any other loading task that is busy
            CancelLoad();

            // Create the option to cancel
            CreateLoadCancelOption();

            OnUiThread(() => InShares.ChildNodes.Clear());
            MNodeList inSharesList = MegaSdk.getInShares(MegaSdk.getContact(_selectedContact.Email));

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

        #endregion
    }
}
