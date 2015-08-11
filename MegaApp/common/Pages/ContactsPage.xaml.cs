using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
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
            _contactsViewModel = new ContactsViewModel(App.MegaSdk, App.AppInformation);
            this.DataContext = _contactsViewModel;

            InitializeComponent();
            InitializePage(MainDrawerLayout, LstHamburgerMenu, HamburgerMenuItemType.Contacts);

            SetApplicationBarData();
        }

        private void SetApplicationBarData()
        {
            this.ApplicationBar = (ApplicationBar)Resources["ContactsMenu"];

            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.AddContact.ToLower();
            //((ApplicationBarIconButton)ApplicationBar.Buttons[1]).Text = UiResources.Search.ToLower();

            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[0]).Text = UiResources.Refresh.ToLower();
            //((ApplicationBarMenuItem)ApplicationBar.MenuItems[1]).Text = UiResources.Sort.ToLower();
            //((ApplicationBarMenuItem)ApplicationBar.MenuItems[2]).Text = UiResources.Select.ToLower();
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

        private void OnItemTap(object sender, GestureEventArgs e)
        {
            var contact = LstMegaContacts.SelectedItem as Contact;
            
            if(contact != null)
            {
                PhoneApplicationService.Current.State["SelectedContact"] = contact;
                NavigateService.NavigateTo(typeof(ContactDetailsPage), NavigationParameter.Normal);
            }                
        }

        private void OnMenuOpening(object sender, ContextMenuOpeningEventArgs e)
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