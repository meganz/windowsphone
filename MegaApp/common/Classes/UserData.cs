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
            if (String.IsNullOrWhiteSpace(AvatarPath) || !File.Exists(AvatarPath)) return;
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
                
        public String UserName
        {            
            get 
            {
                if (!String.IsNullOrWhiteSpace(Firstname) && !String.IsNullOrWhiteSpace(Lastname))
                    return String.Format("{0} {1}", Firstname, Lastname);
                else if (!String.IsNullOrWhiteSpace(Firstname))
                    return Firstname;
                else
                    return String.Empty;
            }
        }

        private String _firstname;
        public String Firstname
        {
            get { return _firstname; }
            set
            {
                _firstname = value;
                OnPropertyChanged("Firstname");
                OnPropertyChanged("UserName");
                OnPropertyChanged("AvatarLetter");
            }
        }

        private String _lastname;
        public String Lastname
        {
            get { return _lastname; }
            set
            {
                _lastname = value;
                OnPropertyChanged("Lastname");
                OnPropertyChanged("UserName");
                OnPropertyChanged("AvatarLetter");
            }
        }

        private bool _hasAvatarImage;
        public bool HasAvatarImage
        {
            get { return _hasAvatarImage; }
            set 
            {
                _hasAvatarImage = value;
                OnPropertyChanged("HasAvatarImage");
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
                if (String.IsNullOrWhiteSpace(UserEmail)) return null;
                
                return Path.Combine(ApplicationData.Current.LocalFolder.Path,
                    AppResources.ThumbnailsDirectory, UserEmail);
            }
        }

        public String AvatarLetter
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(UserName))
                    return UserName.Substring(0, 1).ToUpper();
                else if (!String.IsNullOrWhiteSpace(UserEmail))
                    return UserEmail.Substring(0, 1).ToUpper();
                else
                    return "M"; // If no data available, return "M" of MEGA
            }
        }
    }
}
