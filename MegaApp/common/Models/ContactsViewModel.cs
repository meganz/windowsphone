using System;
using System.Collections;
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
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;
using Microsoft.Phone.Shell;
using Telerik.Windows.Data;

namespace MegaApp.Models
{
    public class ContactsViewModel : BaseAppInfoAwareViewModel, MRequestListenerInterface
    {
        private readonly ContactsPage _contactsPage;

        public ContactsViewModel(MegaSDK megaSdk, AppInformation appInformation, ContactsPage contactsPage)
            : base(megaSdk, appInformation)
        {
            _contactsPage = contactsPage;

            ReinviteRequestCommand = new DelegateCommand(ReinviteRequest);
            DeleteRequestCommand = new DelegateCommand(DeleteRequest);
            AcceptRequestCommand = new DelegateCommand(AcceptRequest);
            IgnoreRequestCommand = new DelegateCommand(IgnoreRequest);
            DeclineRequestCommand = new DelegateCommand(DeclineRequest);

            ViewContactCommand= new DelegateCommand(ViewContact);
            DeleteContactCommand= new DelegateCommand(DeleteContact);

            UpdateUserData();
            
            InitializeMenu(HamburgerMenuItemType.Contacts);

            MegaContactsSortMode = ListSortMode.Ascending;

            MegaContactsList = new ObservableCollection<Contact>();
            MegaContactsList.CollectionChanged += MegaContacts_CollectionChanged;
            
            CreateContactsGroupDescriptors();
            CreateContactSortDescriptors();

            ReceivedContactRequests = new ObservableCollection<ContactRequest>();
            SentContactRequests = new ObservableCollection<ContactRequest>();            
        }

        void MegaContacts_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("NumberOfMegaContacts");
            OnPropertyChanged("IsMegaContactsListEmpty");
            OnPropertyChanged("NumberOfMegaContactsText");

            if (CurrentDisplayMode == ContactDisplayMode.EMPTY_CONTACTS ||
                CurrentDisplayMode == ContactDisplayMode.CONTACTS)
            {
                if (IsMegaContactsListEmpty)
                    CurrentDisplayMode = ContactDisplayMode.EMPTY_CONTACTS;
                else
                    CurrentDisplayMode = ContactDisplayMode.CONTACTS;
            }

            _contactsPage.SetApplicationBarData();
        }

        #region Commands

        public ICommand ReinviteRequestCommand { get; set; }
        public ICommand DeleteRequestCommand { get; set; }
        public ICommand AcceptRequestCommand { get; set; }
        public ICommand IgnoreRequestCommand { get; set; }
        public ICommand DeclineRequestCommand { get; set; }
        
        public ICommand ViewContactCommand { get; set; }
        public ICommand DeleteContactCommand { get; set; }

        #endregion

        #region Properties

        public ContactDisplayMode CurrentDisplayMode { get; set; }
        public ListSortMode MegaContactsSortMode { get; set; }
        
        private ObservableCollection<GenericGroupDescriptor<Contact, String>> groupDescriptors;
        public ObservableCollection<GenericGroupDescriptor<Contact, String>> GroupDescriptors
        {
            get { return groupDescriptors; }
            set
            {
                groupDescriptors = value;
                OnPropertyChanged("GroupDescriptors");
            }
        }

        private ObservableCollection<GenericSortDescriptor<Contact, String>> sortDescriptors;
        public ObservableCollection<GenericSortDescriptor<Contact, String>> SortDescriptors
        {
            get { return sortDescriptors; }
            set
            {
                sortDescriptors = value;
                OnPropertyChanged("SortDescriptors");
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

        private void ViewContact(object obj)
        {
            this.ViewContactDetails();
        }

        private void DeleteContact(object obj)
        {
            if (FocusedContact != null)
            {
                var customMessageDialog = new CustomMessageDialog(
                    AppMessages.DeleteContactQuestion_Title,
                    String.Format(AppMessages.DeleteContactQuestion, FocusedContact.Email),
                    App.AppInformation,
                    MessageDialogButtons.OkCancel);

                customMessageDialog.OkOrYesButtonTapped += (sender, args) =>
                {
                    MegaSdk.removeContact(MegaSdk.getContact(FocusedContact.Email), this);
                };

                customMessageDialog.ShowDialog();
            }
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

        public void CreateContactsGroupDescriptors()
        {
            if(GroupDescriptors == null)
                GroupDescriptors = new ObservableCollection<GenericGroupDescriptor<Contact, String>>();

            GenericGroupDescriptor<Contact, String> group = new GenericGroupDescriptor<Contact, String>();                        
            group.SortMode = MegaContactsSortMode;
            group.KeySelector = (contact) =>
            {
                if (!String.IsNullOrWhiteSpace(contact.FirstName))
                    return contact.FirstName.Substring(0, 1).ToLower();
                else if (!String.IsNullOrWhiteSpace(contact.LastName))
                    return contact.LastName.Substring(0, 1).ToLower();
                else
                    return contact.Email.Substring(0, 1).ToLower();
            };

            GroupDescriptors.Add(group);            
        }

        public void CreateContactSortDescriptors()
        {
            if (SortDescriptors == null)
                SortDescriptors = new ObservableCollection<GenericSortDescriptor<Contact, String>>();

            GenericSortDescriptor<Contact, String> sort = new GenericSortDescriptor<Contact, String>();
            sort.SortMode = MegaContactsSortMode;
            sort.KeySelector = (contact) =>
            {
                if (!String.IsNullOrWhiteSpace(contact.FirstName))
                    return contact.FirstName;
                else if (!String.IsNullOrWhiteSpace(contact.LastName))
                    return contact.LastName;
                else
                    return contact.Email;
            };

            SortDescriptors.Add(sort);
        }

        public void SortContacts(ListSortMode sortMode)
        {
            if (sortMode != ListSortMode.None && MegaContactsSortMode != sortMode)
            {
                MegaContactsSortMode = sortMode;

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    GroupDescriptors.Clear(); GroupDescriptors = null;
                    SortDescriptors.Clear(); SortDescriptors = null;

                    CreateContactsGroupDescriptors();
                    CreateContactSortDescriptors();
                });                
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

        public void ViewContactDetails()
        {
            if (FocusedContact != null)
            {
                PhoneApplicationService.Current.State["SelectedContact"] = FocusedContact;
                NavigateService.NavigateTo(typeof(ContactDetailsPage), NavigationParameter.Normal);
            }
        }

        public void ChangeMenu(IList iconButtons, IList menuItems)
        {
            switch (CurrentDisplayMode)
            {
                case ContactDisplayMode.EMPTY_CONTACTS:
                    {
                        this.TranslateAppBarItems(
                            iconButtons.Cast<ApplicationBarIconButton>().ToList(),
                            menuItems.Cast<ApplicationBarMenuItem>().ToList(),
                            new[] { UiResources.AddContact/*, UiResources.Search*/ },
                            new[] { UiResources.Refresh });
                        break;
                    }
                case ContactDisplayMode.CONTACTS:
                    {
                        this.TranslateAppBarItems(
                            iconButtons.Cast<ApplicationBarIconButton>().ToList(),
                            menuItems.Cast<ApplicationBarMenuItem>().ToList(),
                            new[] { UiResources.AddContact/*, UiResources.Search*/ },
                            new[] { UiResources.Refresh, UiResources.Sort/*, UiResources.Select*/ });
                        break;
                    }
                case ContactDisplayMode.SENT_REQUESTS:
                case ContactDisplayMode.RECEIVED_REQUESTS:
                    {
                        this.TranslateAppBarItems(
                            iconButtons.Cast<ApplicationBarIconButton>().ToList(),
                            menuItems.Cast<ApplicationBarMenuItem>().ToList(),
                            null,
                            new[] { UiResources.Refresh });
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException("CurrentDisplayMode");
            }
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

            switch(request.getType())
            {
                case MRequestType.TYPE_INVITE_CONTACT:
                    if (e.getErrorCode() == MErrorType.API_OK)
                    {
                        switch (request.getNumber())
                        {
                            case (int)MContactRequestInviteActionType.INVITE_ACTION_ADD:
                                Deployment.Current.Dispatcher.BeginInvoke(() =>
                                {
                                    new CustomMessageDialog(
                                        AppMessages.InviteContactSuccessfully_Title,
                                        String.Format(AppMessages.InviteContactSuccessfully, request.getEmail()),
                                        App.AppInformation,
                                        MessageDialogButtons.Ok).ShowDialog();
                                });
                                break;

                            case (int)MContactRequestInviteActionType.INVITE_ACTION_DELETE:
                            case (int)MContactRequestInviteActionType.INVITE_ACTION_REMIND:
                                break;
                        }
                    }
                    else
                    {
                        switch (request.getNumber())
                        {
                            case (int)MContactRequestInviteActionType.INVITE_ACTION_ADD:
                                Deployment.Current.Dispatcher.BeginInvoke(() =>
                                {
                                    new CustomMessageDialog(
                                        AppMessages.InviteContactFailed_Title,
                                        AppMessages.InviteContactFailed,
                                        App.AppInformation,
                                        MessageDialogButtons.Ok).ShowDialog();
                                });
                                break;

                            case (int)MContactRequestInviteActionType.INVITE_ACTION_DELETE:
                            case (int)MContactRequestInviteActionType.INVITE_ACTION_REMIND:
                                break;
                        }
                    }
                    break;

                case MRequestType.TYPE_REPLY_CONTACT_REQUEST:
                    if (e.getErrorCode() == MErrorType.API_OK)
                    {
                        switch (request.getNumber())
                        {
                            case (int)MContactRequestReplyActionType.REPLY_ACTION_ACCEPT:
                            case (int)MContactRequestReplyActionType.REPLY_ACTION_DENY:
                            case (int)MContactRequestReplyActionType.REPLY_ACTION_IGNORE:
                                break;
                        }
                    }
                    break;

                case MRequestType.TYPE_GET_ATTR_USER:
                    if (e.getErrorCode() == MErrorType.API_OK)
                    {
                        foreach (var contactRequest in SentContactRequests)
                        {
                            if (contactRequest.Email.Equals(request.getEmail()))
                                Deployment.Current.Dispatcher.BeginInvoke(() => contactRequest.AvatarUri = new Uri(request.getFile(), UriKind.RelativeOrAbsolute));
                        }

                        foreach (var contactRequest in ReceivedContactRequests)
                        {
                            if (contactRequest.Email.Equals(request.getEmail()))
                                Deployment.Current.Dispatcher.BeginInvoke(() => contactRequest.AvatarUri = new Uri(request.getFile(), UriKind.RelativeOrAbsolute));
                        }
                    }
                    break;

                case MRequestType.TYPE_REMOVE_CONTACT:
                    if (e.getErrorCode() == MErrorType.API_OK)
                    {

                    }
                    break;
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
