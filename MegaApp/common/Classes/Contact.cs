using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Windows.Storage;
using mega;
using MegaApp.Resources;
using MegaApp.ViewModels;

namespace MegaApp.Classes
{
    public class Contact : BaseViewModel
    {
        private String _email;
        public String Email
        {
            get { return _email; }
            set
            {
                _email = value;
                OnPropertyChanged("Email");
            }
        }

        private String _fistName;
        public String FirstName
        {
            get { return _fistName; }
            set
            {
                _fistName = value;
                OnPropertyChanged("FullName");
                OnPropertyChanged("AvatarLetter");
                OnPropertyChanged("FirstName");
            }
        }

        private String _lastName;
        public String LastName
        {
            get { return _lastName; }
            set
            {
                _lastName = value;
                OnPropertyChanged("FullName");
                OnPropertyChanged("AvatarLetter");
                OnPropertyChanged("LastName");                
            }
        }

        public String FullName
        {
            get { return String.Format(FirstName + " " + LastName); }
        }

        public String AvatarLetter
        {
            get 
            {
                if (!String.IsNullOrWhiteSpace(FirstName))
                    return FullName.Substring(0, 1).ToUpper();
                if (!String.IsNullOrWhiteSpace(LastName))
                    return LastName.Substring(0, 1).ToUpper();
                else
                    return Email.Substring(0, 1).ToUpper();
            }
        }

        /// <summary>
        /// Background color for the avatar in case of the contact has not an avatar image.
        /// </summary>
        private Color _avatarColor;
        public Color AvatarColor
        {
            get { return _avatarColor; }
            set { SetField(ref _avatarColor, value); }
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
                if (String.IsNullOrWhiteSpace(Email)) return null;

                return Path.Combine(ApplicationData.Current.LocalFolder.Path,
                    AppResources.ThumbnailsDirectory, Email);
            }
        }

        private bool _isMultiSelected;
        public bool IsMultiSelected
        {
            get { return _isMultiSelected; }
            set { SetField(ref _isMultiSelected, value); }
        }

        public ulong Handle { get; set; }
        public ulong Timestamp { get; set; }
        public MUserVisibility Visibility { get; set; }
    }
}
