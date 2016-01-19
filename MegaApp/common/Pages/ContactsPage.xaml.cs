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
using Microsoft.Phone.Net.NetworkInformation;
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

            // Subscribe to the NetworkAvailabilityChanged event
            DeviceNetworkInformation.NetworkAvailabilityChanged += new EventHandler<NetworkNotificationEventArgs>(NetworkAvailabilityChanged);
        }

        // Code to execute when a Network change is detected.
        private void NetworkAvailabilityChanged(object sender, NetworkNotificationEventArgs e)
        {
            switch (e.NotificationType)
            {
                case NetworkNotificationType.InterfaceConnected:
                    UpdateGUI();
                    break;
                case NetworkNotificationType.InterfaceDisconnected:
                    UpdateGUI(false);
                    break;
                case NetworkNotificationType.CharacteristicUpdate:
                default:
                    break;
            }

            _contactsViewModel.NetworkAvailabilityChanged();
        }

        private void UpdateGUI(bool isNetworkConnected = true)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (isNetworkConnected)
                {
                    if (!Convert.ToBoolean(App.MegaSdk.isLoggedIn()))
                    {
                        NavigateService.NavigateTo(typeof(MainPage), NavigationParameter.None);
                        return;
                    }

                    _contactsViewModel.SetEmptyContentTemplate(true);

                    _contactsViewModel.GetMegaContacts();
                    _contactsViewModel.GetReceivedContactRequests();
                    _contactsViewModel.GetSentContactRequests();
                }
                else
                {
                    _contactsViewModel.MegaContactsList.Clear();
                    _contactsViewModel.ReceivedContactRequests.Clear();
                    _contactsViewModel.SentContactRequests.Clear();

                    _contactsViewModel.SetOfflineContentTemplate();
                }

                SetApplicationBarData(isNetworkConnected);
            });
        }

        public void SetApplicationBarData(bool isNetworkConnected = true)
        {
            // Set the Application Bar to one of the available menu resources in this page
            SetAppbarResources(_contactsViewModel.CurrentDisplayMode);
            
            // Change and translate the current application bar
            _contactsViewModel.ChangeMenu(this.ApplicationBar.Buttons, this.ApplicationBar.MenuItems);

            UiService.ChangeAppBarStatus(this.ApplicationBar.Buttons,
                this.ApplicationBar.MenuItems, isNetworkConnected);
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
                case ContactDisplayMode.CONTACTS_MULTISELECT:
                    this.ApplicationBar = (ApplicationBar)Resources["ContactsMultiSelectMenu"];
                    break;
                case ContactDisplayMode.SENT_REQUESTS:
                    this.ApplicationBar = (ApplicationBar)Resources["SentContactRequestsMenu"];
                    break;
                case ContactDisplayMode.RECEIVED_REQUESTS:
                    this.ApplicationBar = (ApplicationBar)Resources["ReceivedContactRequestsMenu"];
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

            if (!NetworkService.IsNetworkAvailable())
            {
                UpdateGUI(false);
                return;
            }
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

        private void OnMultiSelectClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            ChangeMultiSelectMode();
        }

        private void ChangeMultiSelectMode()
        {
            if (ContactsPivot.SelectedItem == MegaContacts)
                LstMegaContacts.IsCheckModeActive = !LstMegaContacts.IsCheckModeActive;
        }

        private void OnCheckModeChanged(object sender, IsCheckModeActiveChangedEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            ChangeCheckModeAction(e.CheckBoxesVisible, (RadJumpList)sender, e.TappedItem);

            SetApplicationBarData();
        }

        private void ChangeCheckModeAction(bool onOff, RadJumpList listBox, object item)
        {
            if (onOff)
            {
                if (item != null)
                    listBox.CheckedItems.Add(item);

                if (_contactsViewModel.CurrentDisplayMode != ContactDisplayMode.CONTACTS_MULTISELECT)
                    _contactsViewModel.PreviousDisplayMode = _contactsViewModel.CurrentDisplayMode;
                _contactsViewModel.CurrentDisplayMode = ContactDisplayMode.CONTACTS_MULTISELECT;
            }
            else
            {
                listBox.CheckedItems.Clear();
                _contactsViewModel.CurrentDisplayMode = _contactsViewModel.PreviousDisplayMode;
            }
        }

        private void OnMultiSelectDeleteContactClick(object sender, EventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            MultiSelectDeleteContactAction();
        }

        private async void MultiSelectDeleteContactAction()
        {
            if (!await _contactsViewModel.MultipleDeleteContacts()) return;

            _contactsViewModel.CurrentDisplayMode = _contactsViewModel.PreviousDisplayMode;

            SetApplicationBarData();
        }

        private void OnMultiSelectShareFolderClick(object sender, EventArgs e)
        {

        }

        private void OnPivotLoaded(object sender, RoutedEventArgs e)
        {
            if (!NetworkService.IsNetworkAvailable()) return;

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
                {
                    if(!LstMegaContacts.IsCheckModeActive)
                        _contactsViewModel.CurrentDisplayMode = ContactDisplayMode.CONTACTS;
                    else
                        _contactsViewModel.CurrentDisplayMode = ContactDisplayMode.CONTACTS_MULTISELECT;
                }
            }
            
            if (e.AddedItems[0] == SentContactRequests)
                _contactsViewModel.CurrentDisplayMode = ContactDisplayMode.SENT_REQUESTS;
            
            if (e.AddedItems[0] == ReceivedContactRequests)
                _contactsViewModel.CurrentDisplayMode = ContactDisplayMode.RECEIVED_REQUESTS;

            SetApplicationBarData(NetworkService.IsNetworkAvailable());
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
            SetApplicationBarData(NetworkService.IsNetworkAvailable());
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