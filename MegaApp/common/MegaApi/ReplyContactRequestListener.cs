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
    class ReplyContactRequestListener : BaseRequestListener
    {
        private MContactRequestReplyActionType replyActionType;

        protected override string ProgressMessage
        {
            get
            {
                switch (replyActionType)
                {
                    case MContactRequestReplyActionType.REPLY_ACTION_ACCEPT:
                        return ProgressMessages.ReplyContactRequestAccept;
                    case MContactRequestReplyActionType.REPLY_ACTION_IGNORE:
                        return ProgressMessages.ReplyContactRequestIgnore;
                    case MContactRequestReplyActionType.REPLY_ACTION_DENY:
                        return ProgressMessages.ReplyContactRequestDeny;
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

        public override void onRequestStart(MegaSDK api, MRequest request)
        {
            replyActionType = (MContactRequestReplyActionType)request.getNumber();            
            base.onRequestStart(api, request);
        }

        #endregion
    }
}
