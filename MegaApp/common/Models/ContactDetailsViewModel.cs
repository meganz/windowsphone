using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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

            //this.InShares = new FolderViewModel(this.MegaSdk, this.AppInformation, ContainerType.InShares);
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

        private FolderViewModel _inShares;
        public FolderViewModel InShares
        {
            get { return _inShares; }
            private set { SetField(ref _inShares, value); }
        }

        #endregion

        #region Methods

        public void GetContactSharedFolders()
        {
            NumberOfSharedFolders = 0;

            //SharedFolders = MegaSdk.getInShares(MegaSdk.getContact(_selectedContact.Email));
        }

        #endregion
    }
}
