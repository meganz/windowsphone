using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using mega;
using MegaApp.Resources;

namespace MegaApp.Classes
{
    class Contact
    {
        public String Email { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public Uri AvatarUri { get; set; }
        public String AvatarPath 
        {
            get
            {
                return Path.Combine(ApplicationData.Current.LocalFolder.Path,
                                    AppResources.DownloadsDirectory, "ContactAvatarImage_"+Email);
            }
        }
        
        public ulong Timestamp { get; set; }
        public MUserVisibility Visibility { get; set; }
    }
}
