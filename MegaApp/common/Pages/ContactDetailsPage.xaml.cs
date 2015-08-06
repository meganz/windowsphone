using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using MegaApp.Enums;
using MegaApp.Classes;
using MegaApp.Models;
using MegaApp.UserControls;
using MegaApp.Resources;
using MegaApp.Services;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace MegaApp.Pages
{
    public partial class ContactDetailsPage : PhoneDrawerLayoutPage
    {
        private readonly ContactDetailsViewModel _contactDetailsViewModel;

        public ContactDetailsPage()
        {
            _contactDetailsViewModel = new ContactDetailsViewModel(App.MegaSdk, App.AppInformation);
            this.DataContext = _contactDetailsViewModel;

            InitializeComponent();
            InitializePage(MainDrawerLayout, LstHamburgerMenu, HamburgerMenuItemType.Contacts);

            SetApplicationBarData();
        }

        private void SetApplicationBarData()
        {
            this.ApplicationBar = (ApplicationBar)Resources["ContactDetailsMenu"];

            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.Message.ToLower();
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).Text = UiResources.SendFile.ToLower();
            ((ApplicationBarIconButton)ApplicationBar.Buttons[2]).Text = UiResources.ShareFolders.ToLower();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _contactDetailsViewModel.SelectedContact = (Contact)PhoneApplicationService.Current.State["SelectedContact"];
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            if (e.Cancel) return;

            NavigateService.NavigateTo(typeof(ContactsPage), NavigationParameter.Normal);

            e.Cancel = true;            
        }

        private void OnMessageClick(object sender, EventArgs e)
        {

        }

        private void OnSendFilesClick(object sender, EventArgs e)
        {

        }

        private void OnShareFolderClick(object sender, EventArgs e)
        {

        }

        private void OnPivotLoaded(object sender, RoutedEventArgs e)
        {
            if (sender == SharedItems)
                _contactDetailsViewModel.GetContactSharedFolders();
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