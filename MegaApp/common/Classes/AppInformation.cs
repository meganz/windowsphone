using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MegaApp.Enums;

namespace MegaApp.Classes
{
    public class AppInformation
    {
        public AppInformation()
        {
            this.PickerOrAsyncDialogIsOpen = false;
            this.IsNewlyActivatedAccount = false;
            this.IsStartedAsAutoUpload = false;
            this.IsStartupModeActivate = false;
            
            this.HasPinLockIntroduced = false;

            this.HasFetchedNodes = false;

            this.UriLink = UriLinkType.None;
        }
        
        public bool PickerOrAsyncDialogIsOpen { get; set; }
        public bool IsNewlyActivatedAccount { get; set; }
        public bool IsStartedAsAutoUpload { get; set; }
        public bool IsStartupModeActivate { get; set; }
        
        public bool HasPinLockIntroduced { get; set; }

        public bool HasFetchedNodes { get; set; }

        public UriLinkType UriLink { get; set; }
    }
}
