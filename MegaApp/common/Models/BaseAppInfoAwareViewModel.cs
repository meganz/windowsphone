using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.MegaApi;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.UserControls;

namespace MegaApp.Models
{
    public class BaseAppInfoAwareViewModel : BaseSdkViewModel, IHamburgerMenu
    {
        protected BaseAppInfoAwareViewModel(MegaSDK megaSdk, AppInformation appInformation): base(megaSdk)
        {
            this.AppInformation = appInformation;
            this.MenuItems = new List<HamburgerMenuItem>();            
        }

        protected void UpdateUserData()
        {
            if (!NetworkService.IsNetworkAvailable() || !Convert.ToBoolean(SdkService.MegaSdk.isLoggedIn()))
            {
                if (String.IsNullOrWhiteSpace(AccountDetails.UserName))
                    Deployment.Current.Dispatcher.BeginInvoke(() => AccountDetails.Firstname = UiResources.MyAccount);

                if (AccountDetails.AvatarColor == null || AccountDetails.AvatarColor.ToString().Equals("#00000000"))
                    AccountDetails.AvatarColor = (Color)Application.Current.Resources["MegaRedColor"];
            }
            else
            {
                bool accountChange = false;

                String currentEmail = SdkService.MegaSdk.getMyEmail();
                if (currentEmail != null && currentEmail.Length != 0)
                {
                    if (String.IsNullOrEmpty(AccountDetails.UserEmail))
                        accountChange = true;
                    else if (!AccountDetails.UserEmail.Equals(SdkService.MegaSdk.getMyEmail()))
                        accountChange = true;
                }

                if (accountChange)
                {
                    AccountDetails.UserEmail = SdkService.MegaSdk.getMyEmail();
                    AccountService.AccountDetails.IsDataLoaded = false;
                }


                if (accountChange || AccountDetails.AvatarColor == null || AccountDetails.AvatarColor.ToString().Equals("#00000000") ||
                    AccountDetails.AvatarColor.Equals((Color)Application.Current.Resources["MegaRedColor"]))
                {
                    AccountDetails.AvatarColor = UiService.GetColorFromHex(SdkService.MegaSdk.getUserAvatarColor(SdkService.MegaSdk.getMyUser()));
                }

                if (accountChange && (!String.IsNullOrEmpty(AccountDetails.AvatarPath) && AccountDetails.AvatarUri == null))
                    SdkService.MegaSdk.getOwnUserAvatar(AccountDetails.AvatarPath, new GetUserAvatarRequestListener());

                if (accountChange || (String.IsNullOrEmpty(AccountDetails.Firstname) || AccountDetails.Firstname.Equals(UiResources.MyAccount)))
                    SdkService.MegaSdk.getOwnUserAttribute((int)MUserAttrType.USER_ATTR_FIRSTNAME, new GetUserDataRequestListener());

                if (accountChange || (String.IsNullOrEmpty(AccountDetails.Lastname)))
                    SdkService.MegaSdk.getOwnUserAttribute((int)MUserAttrType.USER_ATTR_LASTNAME, new GetUserDataRequestListener());
            }
        }

        #region Properties

        public AppInformation AppInformation { get; private set; }

        public AccountDetailsViewModel AccountDetails
        {
            get { return AccountService.AccountDetails; }
        }        

        #endregion

        #region IHamburgerMenu

        public IList<HamburgerMenuItem> MenuItems { get; set; }

        protected void InitializeMenu(HamburgerMenuItemType activeItem)
        {
            this.MenuItems.Add(new HamburgerMenuItem()
            {
                Type = HamburgerMenuItemType.CloudDrive,
                DisplayName = UiResources.CloudDriveName.ToLower(),
                IconPathData = VisualResources.CloudDriveMenuPathData,
                IconWidth = 48,
                IconHeight = 34,
                Margin = new Thickness(36, 0, 35, 0),
                TapAction = () =>
                {
                    NavigateService.NavigateTo(typeof(MainPage), NavigationParameter.Normal);
                },
                IsActive = activeItem == HamburgerMenuItemType.CloudDrive
            });
            this.MenuItems.Add(new HamburgerMenuItem()
            {
                Type = HamburgerMenuItemType.SavedForOffline,
                DisplayName = UiResources.SavedForOffline.ToLower(),
                IconPathData = VisualResources.SavedOfflineIcoData,
                IconWidth = 44,
                IconHeight = 44,
                Margin = new Thickness(38, 0, 36, 0),
                TapAction = () =>
                {
                    NavigateService.NavigateTo(typeof(SavedForOfflinePage), NavigationParameter.Normal);
                },
                IsActive = activeItem == HamburgerMenuItemType.SavedForOffline
            });
            this.MenuItems.Add(new HamburgerMenuItem()
            {
                Type = HamburgerMenuItemType.CameraUploads,
                DisplayName = UiResources.CameraUploads.ToLower(),
                IconPathData = VisualResources.CameraUploadsPathData,
                IconWidth = 46,
                IconHeight = 36,
                Margin = new Thickness(37, 0, 36, 0),
                TapAction = () =>
                {
                    NavigateService.NavigateTo(typeof(CameraUploadsPage), NavigationParameter.Normal);
                },
                IsActive = activeItem == HamburgerMenuItemType.CameraUploads
            });
            this.MenuItems.Add(new HamburgerMenuItem()
            {
                Type = HamburgerMenuItemType.SharedItems,
                DisplayName = UiResources.SharedItems.ToLower(),
                IconPathData = VisualResources.SharedItemsPathData,
                IconWidth = 45,
                IconHeight = 36,
                Margin = new Thickness(37, 0, 36, 0),
                TapAction = () =>
                {
                    NavigateService.NavigateTo(typeof(SharedItemsPage), NavigationParameter.Normal);
                },
                IsActive = activeItem == HamburgerMenuItemType.SharedItems
            });
            this.MenuItems.Add(new HamburgerMenuItem()
            {
                Type = HamburgerMenuItemType.Contacts,
                DisplayName = UiResources.Contacts.ToLower(),
                IconPathData = VisualResources.ContactsPathData,
                IconWidth = 45,
                IconHeight = 33,
                Margin = new Thickness(37, 0, 36, 0),
                TapAction = () => 
                {
                    NavigateService.NavigateTo(typeof(ContactsPage), NavigationParameter.Normal);
                },
                IsActive = activeItem == HamburgerMenuItemType.Contacts
            });
            this.MenuItems.Add(new HamburgerMenuItem()
            {
                Type = HamburgerMenuItemType.Transfers,
                DisplayName = UiResources.Transfers.ToLower(),
                IconPathData = VisualResources.TransfersPathData,
                IconWidth = 44,
                IconHeight = 44,
                Margin = new Thickness(38, 0, 36, 0),
                TapAction = () =>
                {
                    NavigateService.NavigateTo(typeof(TransferPage), NavigationParameter.Normal);
                },
                IsActive = activeItem == HamburgerMenuItemType.Transfers
            });
            this.MenuItems.Add(new HamburgerMenuItem()
            {
                Type = HamburgerMenuItemType.Settings,
                DisplayName = UiResources.Settings.ToLower(),
                IconPathData = VisualResources.SettingsPathData,
                IconWidth = 45,
                IconHeight = 45,
                Margin = new Thickness(37, 0, 36, 0),
                TapAction = () =>
                {
                    NavigateService.NavigateTo(typeof(SettingsPage), NavigationParameter.Normal);
                },
                IsActive = activeItem == HamburgerMenuItemType.Settings
            });
        }

        #endregion        
    }
}
