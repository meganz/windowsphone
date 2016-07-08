using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Phone.Shell;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;
using IApplicationBar = MegaApp.Interfaces.IApplicationBar;

namespace MegaApp.Models
{
    public abstract class BaseSdkViewModel : BaseViewModel, IApplicationBar
    {
        protected BaseSdkViewModel(MegaSDK megaSdk)
        {
            this.MegaSdk = megaSdk;
        }

        #region Methods

        /// <summary>
        /// Returns if there is a network available and the user is online (logged in).
        /// <para>If there is not a network available, show the corresponding error message if enabled.</para>
        /// <para>If the user is not logged in, also Navigates to the "LoginPage".</para>
        /// </summary>        
        /// <param name="showMessageDialog">
        /// Boolean parameter to indicate if show error messages.
        /// <para>Default value is false.</para>
        /// </param>
        /// <returns>True if the user is online. False in other case.</returns>
        public bool IsUserOnline(bool showMessageDialog = false)
        {
            if (!NetworkService.IsNetworkAvailable(showMessageDialog)) return false;

            bool isOnline = Convert.ToBoolean(App.MegaSdk.isLoggedIn());
            if (!isOnline)
            {
                if (showMessageDialog)
                {
                    OnUiThread(() =>
                    {
                        var customMessageDialog = new CustomMessageDialog(
                            AppMessages.UserNotOnline_Title,
                            AppMessages.UserNotOnline,
                            App.AppInformation,
                            MessageDialogButtons.Ok);

                        customMessageDialog.OkOrYesButtonTapped += (sender, args) =>
                            NavigateService.NavigateTo(typeof(LoginPage), NavigationParameter.Normal);

                        customMessageDialog.ShowDialog();
                    });
                }
                else
                {
                    OnUiThread(() =>
                        NavigateService.NavigateTo(typeof(LoginPage), NavigationParameter.Normal));
                }
            }

            return isOnline;
        }

        #endregion

        #region IApplicationBar

        public void TranslateAppBarItems(IList<ApplicationBarIconButton> iconButtons,
            IList<ApplicationBarMenuItem> menuItems, IList<string> iconStrings, IList<string> menuStrings)
        {
            if (iconButtons != null && iconStrings != null)
            {
                for (var i = 0; i < iconButtons.Count; i++)
                {
                    if (iconButtons[i] == null) throw new IndexOutOfRangeException("iconButtons");
                    if (iconStrings[i] == null) throw new IndexOutOfRangeException("iconStrings");

                    iconButtons[i].Text = iconStrings[i].ToLower();
                }
            }

            if (menuItems != null && menuStrings != null)
            {
                for (var i = 0; i < menuItems.Count; i++)
                {
                    if (menuItems[i] == null) throw new IndexOutOfRangeException("menuItems");
                    if (menuStrings[i] == null) throw new IndexOutOfRangeException("menuStrings");

                    menuItems[i].Text = menuStrings[i].ToLower();
                }
            }
        }

        #endregion

        #region Properties

        public MegaSDK MegaSdk { get; private set; }

        #endregion
    }
}
