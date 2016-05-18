using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    class GetContactAvatarRequestListener : BaseRequestListener
    {
        private readonly Contact _megaContact;
        private readonly ContactRequest _contactRequest;

        public GetContactAvatarRequestListener(Contact megaContact)
        {
            _megaContact = megaContact;
        }

        public GetContactAvatarRequestListener(ContactRequest contactRequest)
        {
            _contactRequest = contactRequest;
        }

        protected override string ProgressMessage
        {
            get { return ProgressMessages.GetContactData; }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { throw new NotImplementedException(); }
        }

        protected override string ErrorMessageTitle
        {
            get { throw new NotImplementedException(); }
        }

        protected override bool ShowErrorMessage
        {
            get { return false; }
        }

        protected override string SuccessMessage
        {
            get { throw new NotImplementedException(); }
        }

        protected override string SuccessMessageTitle
        {
            get { throw new NotImplementedException(); }
        }

        protected override bool ShowSuccesMessage
        {
            get { return false; }
        }

        protected override bool NavigateOnSucces
        {
            get { return false; }
        }

        protected override bool ActionOnSucces
        {
            get { return true; }
        }

        protected override Type NavigateToPage
        {
            get { throw new NotImplementedException(); }
        }

        protected override NavigationParameter NavigationParameter
        {
            get { throw new NotImplementedException(); }
        }

        #region Override Methods

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]);
                ProgressService.SetProgressIndicator(false);
            });

            if (request.getType() == MRequestType.TYPE_GET_ATTR_USER)
            {
                if (e.getErrorCode() == MErrorType.API_OK)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        var img = new BitmapImage();
                        img.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                        img.UriSource = new Uri(request.getFile());

                        if (_megaContact != null)
                        {
                            _megaContact.HasAvatarImage = true;
                            _megaContact.AvatarUri = img.UriSource;
                        }

                        if (_contactRequest != null)
                        {
                            _contactRequest.HasAvatarImage = true;
                            _contactRequest.AvatarUri = img.UriSource;
                        }
                    });                    
                }
                else
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        if (_megaContact != null)
                        {
                            _megaContact.HasAvatarImage = false;
                            _megaContact.AvatarUri = null;                            
                        }

                        if (_contactRequest != null)
                        {
                            _contactRequest.HasAvatarImage = false;
                            _contactRequest.AvatarUri = null;                            
                        }
                    });                    
                }
            }
        }

        #endregion
    }
}
