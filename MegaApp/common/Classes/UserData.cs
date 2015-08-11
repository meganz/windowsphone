using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using mega;
using MegaApp.Models;
using MegaApp.Resources;

namespace MegaApp.Classes
{
    public class UserDataViewModel : BaseViewModel
    {
        public UserDataViewModel()
        {
            if (!File.Exists(AvatarPath)) return;
            AvatarUri = new Uri(AvatarPath);
        }

        private String _userEmail;
        public String UserEmail
        {
            get { return _userEmail; }
            set
            {
                _userEmail = value;
                OnPropertyChanged("AvatarLetter");
                OnPropertyChanged("UserEmail");                
            }
        }

        private String _userName;
        public String UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged("AvatarLetter");
                OnPropertyChanged("UserName");
            }
        }

        private Uri _avatarUri;
        public Uri AvatarUri
        {
            get { return _avatarUri; }
            set
            {
                _avatarUri = value;
                OnPropertyChanged("AvatarUri");
            }
        }

        public String AvatarPath
        {
            get
            {
                return Path.Combine(ApplicationData.Current.LocalFolder.Path,
                                    AppResources.DownloadsDirectory, "UserAvatarImage");
            }
        }

        public String AvatarLetter
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(UserName))
                    return UserName.Substring(0, 1).ToUpper();                
                else
                    return UserEmail.Substring(0, 1).ToUpper();
            }
        }
    }
}
