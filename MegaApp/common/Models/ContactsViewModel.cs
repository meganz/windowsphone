using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.Models
{
    public class ContactsViewModel : BaseAppInfoAwareViewModel, MRequestListenerInterface
    {
        public ContactsViewModel(MegaSDK megaSdk, AppInformation appInformation)
            : base(megaSdk, appInformation)
        {
            ReinviteRequestCommand = new DelegateCommand(ReinviteRequest);
            DeleteRequestCommand = new DelegateCommand(DeleteRequest);
            AcceptRequestCommand = new DelegateCommand(AcceptRequest);
            IgnoreRequestCommand = new DelegateCommand(IgnoreRequest);
            DeclineRequestCommand = new DelegateCommand(DeclineRequest);

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

            OnPropertyChanged("NumberOfMegaContacts");
            OnPropertyChanged("IsMegaContactsListEmpty");
            OnPropertyChanged("NumberOfMegaContactsText");
        }

        #region Commands

        public ICommand ReinviteRequestCommand { get; set; }
        public ICommand DeleteRequestCommand { get; set; }
        public ICommand AcceptRequestCommand { get; set; }
        public ICommand IgnoreRequestCommand { get; set; }
        public ICommand DeclineRequestCommand { get; set; }

        #endregion

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
                OnPropertyChanged("NumberOfMegaContacts");
                OnPropertyChanged("IsMegaContactsListEmpty");
                OnPropertyChanged("NumberOfMegaContactsText");
            }
        }

        public Contact FocusedContact { get; set; }
        
        public int NumberOfMegaContacts
        {
            get { return _megaContactsList.Count; }            
        }

        public bool IsMegaContactsListEmpty
        {
            get { return !Convert.ToBoolean(_megaContactsList.Count); }
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

        public ContactRequest FocusedContactRequest { get; set; }

        #endregion

        #region Methods

        private void ReinviteRequest(object obj)
        {
            MegaSdk.inviteContact(this.FocusedContactRequest.Email, this.FocusedContactRequest.SourceMessage,
                MContactRequestInviteActionType.INVITE_ACTION_REMIND, this);
        }

        private void DeleteRequest(object obj)
        {
            MegaSdk.inviteContact(this.FocusedContactRequest.Email, this.FocusedContactRequest.SourceMessage,
                MContactRequestInviteActionType.INVITE_ACTION_DELETE, this);
        }

        private void AcceptRequest(object obj)
        {
            MegaSdk.replyContactRequest(MegaSdk.getContactRequestByHandle(this.FocusedContactRequest.Handle),
                MContactRequestReplyActionType.REPLY_ACTION_ACCEPT, this);
        }

        private void IgnoreRequest(object obj)
        {
            MegaSdk.replyContactRequest(MegaSdk.getContactRequestByHandle(this.FocusedContactRequest.Handle),
                MContactRequestReplyActionType.REPLY_ACTION_IGNORE, this);
        }

        private void DeclineRequest(object obj)
        {
            MegaSdk.replyContactRequest(MegaSdk.getContactRequestByHandle(this.FocusedContactRequest.Handle),
                MContactRequestReplyActionType.REPLY_ACTION_DENY, this);
        }

        #endregion

        #region Public Methods

        public void Initialize(GlobalDriveListener globalDriveListener)
        {
            // Add contacts to global drive listener to receive notifications
            globalDriveListener.Contacts.Add(this);            
        }

        public void Deinitialize(GlobalDriveListener globalDriveListener)
        {
            // Remove contacts of global drive listener
            globalDriveListener.Contacts.Remove(this);
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

        public void GetMegaContacts()
        {
            // User must be online to perform this operation
            if (!IsUserOnline()) return;

            // First cancel any other loading task that is busy
            CancelLoad();
            
            // Create the option to cancel
            CreateLoadCancelOption();

            OnUiThread(() => MegaContactsList.Clear());
            MUserList contactsList = this.MegaSdk.getContacts();            

            Task.Factory.StartNew(() =>
            {
                try
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        for (int i = 0; i < contactsList.size(); i++)
                        {
                            // If the task has been cancelled, stop processing
                            if (LoadingCancelToken.IsCancellationRequested)
                                LoadingCancelToken.ThrowIfCancellationRequested();

                            // To avoid null values
                            if (contactsList.get(i) == null) continue;

                            if (contactsList.get(i).getVisibility() == MUserVisibility.VISIBILITY_VISIBLE)
                            {
                                var _megaContact = new Contact()
                                {
                                    Email = contactsList.get(i).getEmail(),
                                    Timestamp = contactsList.get(i).getTimestamp(),
                                    Visibility = contactsList.get(i).getVisibility()
                                };

                                MegaContactsList.Add(_megaContact);

                                MegaSdk.getUserAttribute(contactsList.get(i), (int)MUserAttrType.USER_ATTR_FIRSTNAME,
                                    new GetContactDataRequestListener(_megaContact));
                                MegaSdk.getUserAttribute(contactsList.get(i), (int)MUserAttrType.USER_ATTR_LASTNAME,
                                    new GetContactDataRequestListener(_megaContact));
                                MegaSdk.getUserAvatar(contactsList.get(i), _megaContact.AvatarPath,
                                    new GetContactDataRequestListener(_megaContact));
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

        public void GetReceivedContactRequests()
        {
            this.ReceivedContactRequests.Clear();
            MContactRequestList incomingContactRequestsList = MegaSdk.getIncomingContactRequests();

            for (int i = 0; i < incomingContactRequestsList.size(); i++)
            {
                // To avoid null values
                if (incomingContactRequestsList.get(i) == null) continue;

                ContactRequest contactRequest = new ContactRequest(incomingContactRequestsList.get(i));
                this.ReceivedContactRequests.Add(contactRequest);

                MegaSdk.getUserAvatar(MegaSdk.getContact(contactRequest.Email), contactRequest.AvatarPath, this);
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

                ContactRequest contactRequest = new ContactRequest(outgoingContactRequestsList.get(i));
                this.SentContactRequests.Add(contactRequest);

                MegaSdk.getUserAvatar(MegaSdk.getContact(contactRequest.Email), contactRequest.AvatarPath, this);
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
                MegaSdk.inviteContact(args.InputText, "",
                    MContactRequestInviteActionType.INVITE_ACTION_ADD, this);
            };
            inputDialog.ShowDialog();
        }

        #endregion

        #region MRequestListenerInterface

        public virtual void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]);
                ProgressService.SetProgressIndicator(false);
            });

            if (e.getErrorCode() == MErrorType.API_OK)
            {
                if(request.getType() == MRequestType.TYPE_INVITE_CONTACT)
                {
                    switch(request.getNumber())
                    {
                        case (int)MContactRequestInviteActionType.INVITE_ACTION_ADD:
                        case (int)MContactRequestInviteActionType.INVITE_ACTION_DELETE:
                        case (int)MContactRequestInviteActionType.INVITE_ACTION_REMIND:                            
                            break;
                    }
                }
                else if(request.getType() == MRequestType.TYPE_REPLY_CONTACT_REQUEST)
                {
                    switch (request.getNumber())
                    {
                        case (int)MContactRequestReplyActionType.REPLY_ACTION_ACCEPT:
                        case (int)MContactRequestReplyActionType.REPLY_ACTION_DENY:
                        case (int)MContactRequestReplyActionType.REPLY_ACTION_IGNORE:                            
                            break;
                    }
                }
                else if(request.getType() == MRequestType.TYPE_GET_ATTR_USER)
                {
                    foreach(var contactRequest in SentContactRequests)
                    {
                        if(contactRequest.Email.Equals(request.getEmail()))
                            Deployment.Current.Dispatcher.BeginInvoke(() => contactRequest.AvatarUri = new Uri(request.getFile(), UriKind.RelativeOrAbsolute));
                    }

                    foreach (var contactRequest in ReceivedContactRequests)
                    {
                        if (contactRequest.Email.Equals(request.getEmail()))
                            Deployment.Current.Dispatcher.BeginInvoke(() => contactRequest.AvatarUri = new Uri(request.getFile(), UriKind.RelativeOrAbsolute));
                    }                    
                }
            }
        }

        public virtual void onRequestStart(MegaSDK api, MRequest request)
        {
            // Not necessary
        }

        public virtual void onRequestTemporaryError(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["MegaRedColor"]));
        }

        public virtual void onRequestUpdate(MegaSDK api, MRequest request)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]));
        }

        #endregion
    }
}
