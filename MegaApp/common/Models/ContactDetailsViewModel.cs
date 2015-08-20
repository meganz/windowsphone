using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.Resources;

namespace MegaApp.Models
{
    class ContactDetailsViewModel : BaseAppInfoAwareViewModel
    {
        public ContactDetailsViewModel(MegaSDK megaSdk, AppInformation appInformation)
            : base(megaSdk, appInformation)
        {
            UpdateUserData();
            
            InitializeMenu(HamburgerMenuItemType.Contacts);

            this.InShares = new ObservableCollection<FolderViewModel>();
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

        private int _numberOfSharedFolders;
        public int NumberOfSharedFolders
        {
            get { return _numberOfSharedFolders; }
            set
            {
                _numberOfSharedFolders = value;
                OnPropertyChanged("NumberOfSharedFolders");
                OnPropertyChanged("NumberOfSharedFoldersText");
            }
        }

        public String NumberOfSharedFoldersText
        {
            get
            {
                if (NumberOfSharedFolders != 0)
                    return NumberOfSharedFolders.ToString();
                else
                    return UiResources.No.ToLower();
            }
        }

        private ObservableCollection<FolderViewModel> _inShares;
        public ObservableCollection<FolderViewModel> InShares
        {
            get { return _inShares; }
            private set { SetField(ref _inShares, value); }
        }
        
        private ObservableCollection<IMegaNode> _inSharesList;
        public ObservableCollection<IMegaNode> InSharesList
        {
            get { return _inSharesList; }
            set { SetField(ref _inSharesList, value); }
        }

        private CancellationTokenSource LoadingCancelTokenSource { get; set; }
        private CancellationToken LoadingCancelToken { get; set; }

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
            NumberOfSharedFolders = 0;

            OnUiThread(() => InSharesList.Clear());
            MNodeList inSharesNodeList = MegaSdk.getInShares(MegaSdk.getContact(_selectedContact.Email));
        }

        #endregion
    }
}
