using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mega;
using MegaApp.Enums;
using MegaApp.Resources;

namespace MegaApp.MegaApi
{
    class InviteContactRequestListener : BaseRequestListener
    {
        private MContactRequestInviteActionType inviteActionType;
        private String contactEmail;

        protected override string ProgressMessage
        {
            get 
            {
                switch(inviteActionType)
                {
                    case MContactRequestInviteActionType.INVITE_ACTION_ADD:
                        return ProgressMessages.InviteContactAdd;
                    case MContactRequestInviteActionType.INVITE_ACTION_REMIND:
                        return ProgressMessages.InviteContactRemind;
                    case MContactRequestInviteActionType.INVITE_ACTION_DELETE:
                        return ProgressMessages.InviteContactDelete;
                    default:
                        throw new NotImplementedException();
                }                    
            }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get
            {
                switch (inviteActionType)
                {
                    case MContactRequestInviteActionType.INVITE_ACTION_ADD:
                        return AppMessages.InviteContactAddFailed;
                    case MContactRequestInviteActionType.INVITE_ACTION_REMIND:                        
                    case MContactRequestInviteActionType.INVITE_ACTION_DELETE:
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        protected override string ErrorMessageTitle
        {
            get
            {
                switch (inviteActionType)
                {
                    case MContactRequestInviteActionType.INVITE_ACTION_ADD:
                        return AppMessages.InviteContactAddFailed_Title;
                    case MContactRequestInviteActionType.INVITE_ACTION_REMIND:
                    case MContactRequestInviteActionType.INVITE_ACTION_DELETE:
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        protected override bool ShowErrorMessage
        {
            get
            {
                switch (inviteActionType)
                {
                    case MContactRequestInviteActionType.INVITE_ACTION_ADD:
                        return true;
                    case MContactRequestInviteActionType.INVITE_ACTION_REMIND:
                    case MContactRequestInviteActionType.INVITE_ACTION_DELETE:
                    default:
                        return false;
                }
            }
        }

        protected override string SuccessMessage
        {
            get 
            {
                switch(inviteActionType)
                {
                    case MContactRequestInviteActionType.INVITE_ACTION_ADD:
                        return String.Format(AppMessages.InviteContactAddSuccessfully, contactEmail);
                    case MContactRequestInviteActionType.INVITE_ACTION_REMIND:                        
                    case MContactRequestInviteActionType.INVITE_ACTION_DELETE:
                    default:
                        throw new NotImplementedException();
                }                    
            }
        }

        protected override string SuccessMessageTitle
        {
            get
            {
                switch(inviteActionType)
                {
                    case MContactRequestInviteActionType.INVITE_ACTION_ADD:
                        return AppMessages.InviteContactAddSuccessfully_Title;
                    case MContactRequestInviteActionType.INVITE_ACTION_REMIND:
                    case MContactRequestInviteActionType.INVITE_ACTION_DELETE:
                    default:
                        throw new NotImplementedException();
                }                    
            }
        }

        protected override bool ShowSuccesMessage
        {
            get
            {
                switch (inviteActionType)
                {
                    case MContactRequestInviteActionType.INVITE_ACTION_ADD:
                        return true;
                    case MContactRequestInviteActionType.INVITE_ACTION_REMIND:                        
                    case MContactRequestInviteActionType.INVITE_ACTION_DELETE:
                    default:
                        return false;
                }
            }
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

        public override void onRequestStart(MegaSDK api, MRequest request)
        {
            inviteActionType = (MContactRequestInviteActionType)request.getNumber();
            contactEmail = request.getEmail();
            base.onRequestStart(api, request);
        }

        #endregion
    }
}
