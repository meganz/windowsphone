using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mega;
using MegaApp.Classes;
using MegaApp.Interfaces;
using MegaApp.Models;
using Microsoft.Phone.Shell;
using IApplicationBar = MegaApp.Interfaces.IApplicationBar;

namespace MegaApp.ViewModels
{
    public abstract class BaseAppInfoAwareViewModel : BaseSdkViewModel, IHamburgerMenu, IApplicationBar
    {
        protected BaseAppInfoAwareViewModel(MegaSDK megaSdk, AppInformation appInformation): base(megaSdk)
        {
            this.AppInformation = appInformation;
            this.MenuItems = new List<HamburgerMenuItem>();
        }

        #region Properties

        public AppInformation AppInformation { get; private set; }

        #endregion

        #region IHamburgerMenu

        public IList<HamburgerMenuItem> MenuItems { get; set; }

        #endregion

        #region IApplicationBar

        public void TranslateAppBarItems(IList<ApplicationBarIconButton> iconButtons, 
            IList<ApplicationBarMenuItem> menuItems, IList<string> iconStrings, IList<string> menuStrings)
        {
            for (var i = 0; i < iconButtons.Count; i++)
            {
                if (iconStrings[i] == null) throw new IndexOutOfRangeException("iconStrings");

                iconButtons[i].Text = iconStrings[i]; 
            }

            for (var i = 0; i < menuItems.Count; i++)
            {
                if (menuStrings[i] == null) throw new IndexOutOfRangeException("menuStrings");

                menuItems[i].Text = menuStrings[i];
            }
        }

        #endregion
    }
}
