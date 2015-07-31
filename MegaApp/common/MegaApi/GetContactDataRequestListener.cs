using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    class GetContactDataRequestListener : BaseRequestListener
    {
        private readonly ObservableCollection<Contact> _megaContactsList;
        private readonly Contact _megaContact;

        public GetContactDataRequestListener(String contactEmail, ObservableCollection<Contact> megaContactsList)
        {
            _megaContactsList = megaContactsList;
            
            var _contact = App.MegaSdk.getContact(contactEmail);

            _megaContact = new Contact()
            {
                Email = _contact.getEmail(),
                Timestamp = _contact.getTimestamp(),
                Visibility = _contact.getVisibility()
            };

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
            get { return false; }
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
                switch (request.getParamType())
                {
                    case (int)MUserAttrType.USER_ATTR_FIRSTNAME:
                        Deployment.Current.Dispatcher.BeginInvoke(() => _megaContact.FirstName = request.getText());
                        api.getUserAttribute(api.getContact(request.getEmail()), (int)MUserAttrType.USER_ATTR_LASTNAME, this);
                        break;

                    case (int)MUserAttrType.USER_ATTR_LASTNAME:
                        Deployment.Current.Dispatcher.BeginInvoke(() => _megaContact.LastName = request.getText());
                        api.getUserAvatar(api.getContact(request.getEmail()), _megaContact.AvatarPath, this);                        
                        break;

                    default: // getUserAvatar()
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            if (e.getErrorCode() == MErrorType.API_OK)
                            {
                                _megaContact.AvatarUri = new Uri(request.getFile(), UriKind.RelativeOrAbsolute);
                            }

                            _megaContactsList.Add(_megaContact);
                        });
                        break;
                }
            }
        }

        #endregion
    }
}
