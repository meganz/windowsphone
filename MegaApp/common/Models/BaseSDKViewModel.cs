using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Phone.Shell;
using mega;
using MegaApp.Classes;
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

        public bool IsUserOnline()
        {
            if (!NetworkService.IsNetworkAvailable()) return false;

            bool isOnline = Convert.ToBoolean(this.MegaSdk.isLoggedIn());

            if (!isOnline)
                OnUiThread(() =>
                {
                    new CustomMessageDialog(
                            AppMessages.UserNotOnline_Title,
                            AppMessages.UserNotOnline,
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialog();
                });

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
