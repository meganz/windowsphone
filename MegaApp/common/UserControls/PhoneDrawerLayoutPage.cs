using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telerik.Windows.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.Views;

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
            var lastPage = backStack.FirstOrDefault();
            var navParam = NavigateService.ProcessQueryString(NavigationContext.QueryString);

            // In some cases we should remove the last page from the stack
            if (lastPage != null)
            {
                switch(navParam)
                {
                    case NavigationParameter.PasswordLogin:
                        // If the previous page is the PasswordPage (PIN lock page)
                        if (lastPage.Source.ToString().Contains(typeof(PasswordPage).Name))
                            ((PhoneApplicationFrame)Application.Current.RootVisual).RemoveBackEntry();
                        break;

                    case NavigationParameter.SecuritySettings:
                    case NavigationParameter.MFA_Enabled:
                        // If the previous page is the "MultiFactorAuthAppSetupPage"
                        if (lastPage.Source.ToString().Contains(typeof(MultiFactorAuthAppSetupPage).Name))
                            ((PhoneApplicationFrame)Application.Current.RootVisual).RemoveBackEntry();
                        break;
                }

                // If the previous page is the "FolderLinkPage" and the
                // current page is not "PreviewImagePage" or "NodeDetailsPage".
                if (page != typeof(PreviewImagePage) || page != typeof(NodeDetailsPage))
                {
                    if (lastPage.Source.ToString().Contains(typeof(FolderLinkPage).Name))
                        ((PhoneApplicationFrame)Application.Current.RootVisual).RemoveBackEntry();
                }
            }

            if (isMainPage)
            {
                if (navigateTo)
                {
                    if (lastPage == null) return;
                    if(lastPage.Source.ToString().Contains(page.Name) || navParam == NavigationParameter.FileLinkLaunch ||
                        navParam == NavigationParameter.FolderLinkLaunch)
                    {
                        ((PhoneApplicationFrame)Application.Current.RootVisual).RemoveBackEntry();
                    }
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

        /// <summary>
        /// Check if can go back in the stack of pages.
        /// <para>
        /// Custom check to see if can go back normally in the stack of pages when the user press the back key. 
        /// If can't go back, goes to the "MainPage" (always that the current page isn't the "MainPage").
        /// </para>
        /// </summary>
        /// <param name="isCancel">Value that indicates if the go back operation should be canceled.</param>
        /// <seealso cref="OnBackKeyPress"/>
        /// <returns>Boolean value that indicates if the go back operation should be canceled.</returns>
        protected bool CheckGoBack(bool isCancel)
        {
            if (isCancel) return true;
            
            if((!NavigationService.CanGoBack) && (this.GetType() != typeof(MainPage)))
            {
                NavigateService.NavigateTo(typeof(MainPage), NavigationParameter.Normal);
                return true;
            }
            
            return false;
        }

        #endregion

        #region Override Methods

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Needed on every UI interaction
            SdkService.MegaSdk.retryPendingConnections();

            // Check if Hamburger Menu is open in view. If open. First slide out before navigating
            if (this.PageDrawerLayout != null)
                this.PageDrawerLayout.CloseIfOpen();

            bool processBackStack = true;
            // If the source page is the "SharedItemsPage" and the destination page is the "PreviewImagePage"
            // don't process the back stack to allow come back to the "SharedItemsPage"
            if (this.GetType().Equals(typeof(SharedItemsPage)) && ((e.Content as PreviewImagePage) != null))
                processBackStack = false;
            
            if(processBackStack)
                ProcessBackstack(this.GetType(), false);

            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ProcessBackstack(this.GetType(), true);

            if (App.AppInformation.IsStartupModeActivate)
            {
                if (!App.AppInformation.HasPinLockIntroduced && SettingsService.LoadSetting<bool>(SettingsResources.UserPinLockIsEnabled))
                {
                    NavigateService.NavigateTo(typeof(PasswordPage), NavigationParameter.Normal, this.GetType());
                    return;
                }

                if (!(this.GetType().Equals(typeof(MainPage))))
                {
                    App.AppInformation.IsStartupModeActivate = false;
                }
            }
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            if (e.Cancel) return;

            // Needed on every UI interaction
            SdkService.MegaSdk.retryPendingConnections();
            
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
            SdkService.MegaSdk.retryPendingConnections();
        }

        protected virtual void OnDrawerOpened(object sender)
        {
            // Needed on every UI interaction
            SdkService.MegaSdk.retryPendingConnections();

            // Remove application bar from display when sliding in the hamburger menu
            // This is necessary on all pages
            this.ApplicationBar = null;
        }

        protected virtual void OnHamburgerMenuItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            // Needed on every UI interaction
            SdkService.MegaSdk.retryPendingConnections();

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
            SdkService.MegaSdk.retryPendingConnections();

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
