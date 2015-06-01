using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;

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
    }
}
