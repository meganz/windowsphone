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

        private string _userEmail;
        public string UserEmail
        {
            get { return _userEmail; }
            set
            {
                _userEmail = value;
                OnPropertyChanged("UserEmail");
            }
        }

        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
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

        public string AvatarPath
        {
            get
            {
                return Path.Combine(ApplicationData.Current.LocalFolder.Path,
                                    AppResources.DownloadsDirectory, "UserAvatarImage");
            }
        }
    }
}
