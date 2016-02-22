using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Phone.Shell;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Resources;

namespace MegaApp.Models
{
    class CameraUploadsViewModel : BaseAppInfoAwareViewModel
    {
        public CameraUploadsViewModel(MegaSDK megaSdk, AppInformation appInformation)
            : base(megaSdk, appInformation)
        {
            UpdateUserData();

            InitializeMenu(HamburgerMenuItemType.CameraUploads);
        }

        public void ChangeMenu(IList iconButtons, IList menuItems)
        {
            this.TranslateAppBarItems(
                iconButtons.Cast<ApplicationBarIconButton>().ToList(),
                menuItems.Cast<ApplicationBarMenuItem>().ToList(),
                new[] { UiResources.Ok, UiResources.Skip },
                null);
        }
    }
}
