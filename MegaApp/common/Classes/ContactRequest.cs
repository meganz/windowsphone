using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using mega;
using MegaApp.Models;
using MegaApp.Resources;

namespace MegaApp.Classes
{
    public class ContactRequest : BaseViewModel
    {
        public ContactRequest(MContactRequest contactRequest)
        {
            Handle = contactRequest.getHandle();
            SourceEmail = contactRequest.getSourceEmail();
            SourceMessage = contactRequest.getSourceMessage();
            TargetEmail = contactRequest.getTargetEmail();
            CreationTime = contactRequest.getCreationTime();
            ModificationTime = contactRequest.getModificationTime();
            Status = contactRequest.getStatus();
            IsOutgoing = contactRequest.isOutgoing();
        }

        public ulong Handle { get; set; }
        public String SourceEmail { get; set; }
        public String SourceMessage { get; set; }
        public String TargetEmail { get; set; }
        public long CreationTime { get; set; }
        public long ModificationTime { get; set; }
        public int Status { get; set; }
        public bool IsOutgoing { get; set; }

        private bool _hasAvatarImage;
        public bool HasAvatarImage
        {
            get { return _hasAvatarImage; }
            set { SetField(ref _hasAvatarImage, value); }
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

        public String AvatarLetter
        {
            get 
            {
                if (String.IsNullOrWhiteSpace(Email)) return null;
                return Email.Substring(0, 1).ToUpper(); 
            }
        }

        public String Email
        {
            get
            {
                if (IsOutgoing)
                    return TargetEmail;
                else
                    return SourceEmail;
            }
        }

        public String RelativeCreationTimeSpan
        {
            get
            {
                DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                DateTime creation = (start.AddSeconds(CreationTime)).ToLocalTime();

                TimeSpan span = DateTime.Now - creation;

                if (span.TotalSeconds < 0)
                    return String.Format(UiResources.TimeSecondsAgo, 0);
                else if(span.TotalSeconds < 60)
                    return String.Format(UiResources.TimeSecondsAgo, Convert.ToInt32(span.TotalSeconds));
                else if (span.TotalMinutes < 60)
                    return String.Format(UiResources.TimeMinutesAgo, Convert.ToInt32(span.TotalMinutes));
                else if (span.TotalHours < 24)
                    return String.Format(UiResources.TimeHoursAgo, Convert.ToInt32(span.TotalHours));
                else
                    return String.Format(UiResources.TimeDaysAgo, Convert.ToInt32(span.TotalDays));
            }
        }

        public String StatusText
        {
            get
            {
                switch(Status)
                {
                    case (int)MContactRequestStatusType.STATUS_UNRESOLVED:
                        return String.Format("(" + UiResources.Pending.ToUpper() + ")");
                    
                    case (int)MContactRequestStatusType.STATUS_ACCEPTED:
                        return String.Format("(" + UiResources.Accepted.ToUpper() + ")");
                        
                    case (int)MContactRequestStatusType.STATUS_DENIED:
                        return String.Format("(" + UiResources.Denied.ToUpper() + ")");
                    
                    case (int)MContactRequestStatusType.STATUS_IGNORED:
                        return String.Format("(" + UiResources.Ignored.ToUpper() + ")");
                    
                    case (int)MContactRequestStatusType.STATUS_DELETED:
                        return String.Format("(" + UiResources.Deleted.ToUpper() + ")");
                    
                    case (int)MContactRequestStatusType.STATUS_REMINDED:
                        return String.Format("(" + UiResources.Reminded.ToUpper() + ")");

                    default:
                        return String.Format("(" + UiResources.Unknown.ToUpper() + ")");
                }
            }
        }
    }
}
