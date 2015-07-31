using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.Models
{
    class ContactsViewModel : BaseAppInfoAwareViewModel
    {
        public ContactsViewModel(MegaSDK megaSdk, AppInformation appInformation)
            : base(megaSdk, appInformation)
        {
            UpdateUserData();
            
            InitializeMenu(HamburgerMenuItemType.Contacts);

            MegaContactsList = new ObservableCollection<Contact>();
            MegaContactsList.CollectionChanged += MegaContacts_CollectionChanged;

            ReceivedContactRequests = new ObservableCollection<ContactRequest>();
            SentContactRequests = new ObservableCollection<ContactRequest>();
        }

        void MegaContacts_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            MegaContactsDataSource = AlphaKeyGroup<Contact>.CreateGroups(MegaContactsList,
                System.Threading.Thread.CurrentThread.CurrentUICulture,
                (Contact s) => 
                {
                    if (!String.IsNullOrWhiteSpace(s.FirstName))
                        return s.FirstName;
                    else if (!String.IsNullOrWhiteSpace(s.LastName))
                        return s.LastName;
                    else
                        return s.Email;
                }, 
                true);
        }        

        #region Properties

        private List<AlphaKeyGroup<Contact>> _megaContactsDataSource;
        public List<AlphaKeyGroup<Contact>> MegaContactsDataSource
        {
            get { return _megaContactsDataSource; }
            set
            {
                _megaContactsDataSource = value;
                OnPropertyChanged("MegaContactsDataSource");
            }
        }

        private ObservableCollection<Contact> _megaContactsList;
        public ObservableCollection<Contact> MegaContactsList
        {
            get { return _megaContactsList; }
            set
            {
                _megaContactsList = value;
                OnPropertyChanged("MegaContactsList");
            }
        }
                
        private int _numberOfMegaContacts;
        public int NumberOfMegaContacts
        {
            get { return _numberOfMegaContacts; }
            set
            {
                _numberOfMegaContacts = value;
                OnPropertyChanged("NumberOfMegaContacts");
                OnPropertyChanged("IsMegaContactsListEmpty");
                OnPropertyChanged("NumberOfMegaContactsText");
            }
        }

        public bool IsMegaContactsListEmpty
        {
            get { return !Convert.ToBoolean(_numberOfMegaContacts); }
        }
                
        public String NumberOfMegaContactsText
        {
            get 
            {
                if (NumberOfMegaContacts != 0)
                    return NumberOfMegaContacts.ToString();
                else
                    return UiResources.No.ToLower();
            }            
        }        

        private CancellationTokenSource LoadingCancelTokenSource { get; set; }
        private CancellationToken LoadingCancelToken { get; set; }

        private ObservableCollection<ContactRequest> _receivedContactRequests;
        public ObservableCollection<ContactRequest> ReceivedContactRequests
        {
            get { return _receivedContactRequests; }
            set
            {
                _receivedContactRequests = value;
                OnPropertyChanged("ReceivedContactRequests");
            }
        }

        private ObservableCollection<ContactRequest> _sentContactRequests;
        public ObservableCollection<ContactRequest> SentContactRequests
        {
            get { return _sentContactRequests; }
            set
            {
                _sentContactRequests = value;
                OnPropertyChanged("SentContactRequests");
            }
        }

        #endregion

        #region Methods

        public void GetMegaContacts()
        {
            MegaContactsList.Clear();
            MUserList contactsList = this.MegaSdk.getContacts();

            NumberOfMegaContacts = 0;

            for (int i = 0; i < contactsList.size(); i++)
            {
                // If the task has been cancelled, stop processing
                if (LoadingCancelToken.IsCancellationRequested)
                    LoadingCancelToken.ThrowIfCancellationRequested();

                // To avoid null values
                if (contactsList.get(i) == null) continue;

                if(contactsList.get(i).getVisibility() == MUserVisibility.VISIBILITY_VISIBLE)
                {
                    NumberOfMegaContacts++;
                    MegaSdk.getUserAttribute(contactsList.get(i), (int)MUserAttrType.USER_ATTR_FIRSTNAME,
                        new GetContactDataRequestListener(contactsList.get(i).getEmail(), MegaContactsList));
                }
            }
        }

        public void GetReceivedContactRequests()
        {
            this.ReceivedContactRequests.Clear();
            MContactRequestList incomingContactRequestsList = MegaSdk.getIncomingContactRequests();

            for (int i = 0; i < incomingContactRequestsList.size(); i++)
            {
                // To avoid null values
                if (incomingContactRequestsList.get(i) == null) continue;

                this.ReceivedContactRequests.Add(new ContactRequest(incomingContactRequestsList.get(i)));
            }
        }

        public void GetSentContactRequests()
        {
            this.SentContactRequests.Clear();
            MContactRequestList outgoingContactRequestsList = MegaSdk.getOutgoingContactRequests();

            for (int i = 0; i < outgoingContactRequestsList.size(); i++)
            {
                // To avoid null values
                if (outgoingContactRequestsList.get(i) == null) continue;
                this.SentContactRequests.Add(new ContactRequest(outgoingContactRequestsList.get(i)));
            }
        }        

        public void AddContact()
        {
            if (!IsUserOnline()) return;

            // Only 1 CustomInputDialog should be open at the same time.
            if (this.AppInformation.PickerOrAsyncDialogIsOpen) return;

            var inputDialog = new CustomInputDialog(UiResources.AddContact, UiResources.CreateContact, this.AppInformation);
            inputDialog.OkButtonTapped += (sender, args) =>
            {
                this.MegaSdk.addContact(args.InputText, new AddContactRequestListener());
            };
            inputDialog.ShowDialog();
        }

        #endregion        
    }
}
