using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Resources;

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

        protected override void OnSuccesAction(MegaSDK api, MRequest request)
        {
            if (request.getType() == MRequestType.TYPE_GET_ATTR_USER)
            {
                if(_megaContact != null)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        _megaContact.AvatarUri = new Uri(request.getFile(), UriKind.RelativeOrAbsolute);
                    });
                }

                if (_contactRequest != null)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        _contactRequest.AvatarUri = new Uri(request.getFile(), UriKind.RelativeOrAbsolute);
                    });
                }                
            }
        }

        #endregion
    }
}
