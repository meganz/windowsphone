using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Pages;
using MegaApp.Services;
using Microsoft.Phone.Controls;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace MegaApp.UserControls
{
    public class PhoneDrawerLayoutPage : MegaPhoneApplicationPage
    {
        #region Methods

        protected void InitializePage(DrawerLayout drawerLayout, RadDataBoundListBox listBox, HamburgerMenuItemType currentType)
        {
            this.PageDrawerLayout = drawerLayout;
            this.HamburgerMenuListBox = listBox;
            this.CurrentHamburgerMenuItem = currentType;
        }

        private void ProcessBackstack(Type page, bool navigateTo)
        {
            bool isMainPage = page == typeof(MainPage);
            var backStack = ((PhoneApplicationFrame)Application.Current.RootVisual).BackStack;

            if (isMainPage)
            {
                if (navigateTo)
                {
                    var lastPage = backStack.FirstOrDefault();
                    if (lastPage == null) return;
                    if(lastPage.Source.ToString().Contains(page.Name))
                        ((PhoneApplicationFrame)Application.Current.RootVisual).RemoveBackEntry();
                }
                else
                {
                    if (backStack.Count(p => p.Source.ToString().Contains(page.Name)) > 1)
                        ((PhoneApplicationFrame)Application.Current.RootVisual).RemoveBackEntry();
                }
            }
            else
            {
                if (navigateTo) return;
                ((PhoneApplicationFrame)Application.Current.RootVisual).RemoveBackEntry();
            }
        }

        #endregion

        #region Override Methods

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            // Check if Hamburger Menu is open in view. If open. First slide out before navigating
            if (this.PageDrawerLayout != null)
                this.PageDrawerLayout.CloseIfOpen();

            ProcessBackstack(this.GetType(), false);

            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ProcessBackstack(this.GetType(), true);
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            if (e.Cancel) return;

            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();
            
            // Check if Hamburger Menu is open in view. If open. First slide out before exit
            if (this.PageDrawerLayout != null)
                e.Cancel = this.PageDrawerLayout.CloseIfOpen();
        }

        #endregion

        #region Virtual Methods

        protected virtual void OnDrawerClosed(object sender)
        {
            // Do stuff when drawer is closed that is necessary on all pages
            
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();
        }

        protected virtual void OnDrawerOpened(object sender)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            // Remove application bar from display when sliding in the hamburger menu
            // This is necessary on all pages
            this.ApplicationBar = null;
        }

        protected virtual void OnHamburgerMenuItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            var menuItem = e.Item.DataContext as HamburgerMenuItem;

            if (menuItem == null) return;

            if (menuItem.Type == this.CurrentHamburgerMenuItem)
            {
                if (this.PageDrawerLayout != null)
                    this.PageDrawerLayout.CloseIfOpen();
            }
            else
            {
                menuItem.TapAction.Invoke();
            }            

            this.HamburgerMenuListBox.SelectedItem = null;
        }

        protected virtual void OnHamburgerTap(object sender, GestureEventArgs e)
        {
            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            if (this.PageDrawerLayout != null)
                this.PageDrawerLayout.OpenDrawer();
        }
        
        #endregion

        #region Properties

        private HamburgerMenuItemType CurrentHamburgerMenuItem { get; set; }
        private RadDataBoundListBox HamburgerMenuListBox { get; set; }

        private DrawerLayout _pageDrawerLayout;
        protected DrawerLayout PageDrawerLayout
        {
            get { return _pageDrawerLayout; }
            set
            {
                _pageDrawerLayout = value;
                
                // Initialize the hamburger menu / slide in
                PageDrawerLayout.InitializeDrawerLayout();
                PageDrawerLayout.DrawerOpened += OnDrawerOpened;
                PageDrawerLayout.DrawerClosed += OnDrawerClosed;
            }
        }

        #endregion
    }
}
