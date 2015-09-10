using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Resources;

namespace MegaApp.MegaApi
{
    class RemoveContactRequestListener : BaseRequestListener
    {
        private ContactsViewModel _contactsViewModel;        
        private Contact _contact;

        public RemoveContactRequestListener(ContactsViewModel contactsViewModel = null, Contact contact = null)
        {
            this._contactsViewModel = contactsViewModel;            
            this._contact = contact;
        }

        #region Base Properties

        protected override string ProgressMessage
        {
            get { return ProgressMessages.RemoveContact; }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return AppMessages.DeleteContactFailed; }
        }

        protected override string ErrorMessageTitle
        {
            get { return AppMessages.DeleteContactFailed_Title; }
        }

        protected override bool ShowErrorMessage
        {
            get { return true; }
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

        #endregion

        #region Override Methods        

        protected override void OnSuccesAction(MegaSDK api, MRequest request)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                try 
                {
                    if ((_contactsViewModel != null) && (_contactsViewModel.MegaContactsList != null) &&
                        _contact != null && _contactsViewModel.MegaContactsList.Contains(_contact))
                    {
                        _contactsViewModel.MegaContactsList.Remove(_contact);
                    }                    
                }
                catch (Exception) { }
            });
        }

        #endregion
    }
}
