using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.UserControls;
using MegaApp.Resources;
using MegaApp.Services;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace MegaApp.Pages
{
    public partial class ContactsPage : PhoneDrawerLayoutPage
    {
        private readonly ContactsViewModel _contactsViewModel;

        public ContactsPage()
        {
            _contactsViewModel = new ContactsViewModel(App.MegaSdk, App.AppInformation, this);
            this.DataContext = _contactsViewModel;

            InitializeComponent();
            InitializePage(MainDrawerLayout, LstHamburgerMenu, HamburgerMenuItemType.Contacts);

            SetApplicationBarData();
        }

        public void SetApplicationBarData()
        {
            // Set the Application Bar to one of the available menu resources in this page
            SetAppbarResources(_contactsViewModel.CurrentDisplayMode);
            
            // Change and translate the current application bar
            _contactsViewModel.ChangeMenu(this.ApplicationBar.Buttons, this.ApplicationBar.MenuItems);
        }

        private void SetAppbarResources(ContactDisplayMode contactsDisplayMode)
        {
            switch (contactsDisplayMode)
            {
                case ContactDisplayMode.EMPTY_CONTACTS:
                    this.ApplicationBar = (ApplicationBar)Resources["ContactsEmptyMenu"];
                    break;
                case ContactDisplayMode.CONTACTS:
                    this.ApplicationBar = (ApplicationBar)Resources["ContactsMenu"];
                    break;
                case ContactDisplayMode.SENT_REQUESTS:
                case ContactDisplayMode.RECEIVED_REQUESTS:
                    this.ApplicationBar = (ApplicationBar)Resources["ContactRequestMenu"];
                    break;
                default:
                    throw new ArgumentOutOfRangeException("contactsDisplayMode");
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _contactsViewModel.Deinitialize(App.GlobalDriveListener);
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _contactsViewModel.Initialize(App.GlobalDriveListener);
        }

        private void OnAddContactClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            _contactsViewModel.AddContact();
        }

        private void OnSearchContactClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();
        }

        private void OnRefreshClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            if (ContactsPivot.SelectedItem == MegaContacts)
                _contactsViewModel.GetMegaContacts();
            else if (ContactsPivot.SelectedItem == SentContactRequests)
                _contactsViewModel.GetSentContactRequests();
            else if (ContactsPivot.SelectedItem == ReceivedContactRequests)
                _contactsViewModel.GetReceivedContactRequests();
        }

        private void OnSortClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            DialogService.ShowSortContactsDialog(_contactsViewModel);
        }

        private void OnSelectClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();
        }

        private void OnPivotLoaded(object sender, RoutedEventArgs e)
        {
            if (sender == MegaContacts)
                _contactsViewModel.GetMegaContacts();
            else if (sender == SentContactRequests)
                _contactsViewModel.GetSentContactRequests();
            else if (sender == ReceivedContactRequests)
                _contactsViewModel.GetReceivedContactRequests();
        }

        private void OnPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems[0] == MegaContacts)
            {
                if (_contactsViewModel.IsMegaContactsListEmpty)
                    _contactsViewModel.CurrentDisplayMode = ContactDisplayMode.EMPTY_CONTACTS;
                else
                    _contactsViewModel.CurrentDisplayMode = ContactDisplayMode.CONTACTS;                                
            }
            
            if (e.AddedItems[0] == SentContactRequests)
                _contactsViewModel.CurrentDisplayMode = ContactDisplayMode.SENT_REQUESTS;
            
            if (e.AddedItems[0] == ReceivedContactRequests)
                _contactsViewModel.CurrentDisplayMode = ContactDisplayMode.RECEIVED_REQUESTS;

            SetApplicationBarData();
        }

        private void OnItemTap(object sender, GestureEventArgs e)
        {
            _contactsViewModel.FocusedContact = LstMegaContacts.SelectedItem as Contact;
            _contactsViewModel.ViewContactDetails();
        }        

        private void OnContactsMenuOpening(object sender, ContextMenuOpeningEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            var focusedListBoxItem = e.FocusedElement as RadDataBoundListBoxItem;
            if (focusedListBoxItem == null || !(focusedListBoxItem.DataContext is Contact))
            {
                // We don't want to open the menu if the focused element is not a list box item.
                // If the list box is empty focusedItem will be null.
                e.Cancel = true;
            }
            else
            {
                _contactsViewModel.FocusedContact = (Contact)focusedListBoxItem.DataContext;
            }
        }

        private void OnContactRequestsMenuOpening(object sender, ContextMenuOpeningEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            var focusedListBoxItem = e.FocusedElement as RadDataBoundListBoxItem;
            if (focusedListBoxItem == null || !(focusedListBoxItem.DataContext is ContactRequest))
            {
                // We don't want to open the menu if the focused element is not a list box item.
                // If the list box is empty focusedItem will be null.
                e.Cancel = true;
            }
            else
            {
                _contactsViewModel.FocusedContactRequest = (ContactRequest)focusedListBoxItem.DataContext;
            }
        }

        protected override void OnDrawerClosed(object sender)
        {
            base.OnDrawerClosed(sender);
            SetApplicationBarData();
        }

        private void OnMyAccountTap(object sender, GestureEventArgs e)
        {
            NavigateService.NavigateTo(typeof(MyAccountPage), NavigationParameter.Normal);
        }

        #region Override Events

        // XAML can not bind them direct from the base class
        // That is why these are dummy event handlers

        protected override void OnHamburgerTap(object sender, GestureEventArgs e)
        {
            base.OnHamburgerTap(sender, e);
        }

        protected override void OnHamburgerMenuItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            base.OnHamburgerMenuItemTap(sender, e);
        }

        #endregion        
    }
}