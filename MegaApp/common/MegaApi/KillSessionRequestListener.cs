using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MegaApp.Enums;
using MegaApp.Resources;

namespace MegaApp.MegaApi
{
    class KillSessionRequestListener : BaseRequestListener
    {
        #region Base Properties

        protected override string ProgressMessage
        {
            get { return ProgressMessages.CloseSession; }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return AppMessages.CloseSessionFailed; }
        }

        protected override string ErrorMessageTitle
        {
            get { return UiResources.CloseSession.ToUpper(); }
        }

        protected override bool ShowErrorMessage
        {
            get { return true; }
        }

        protected override string SuccessMessage
        {
            get { return AppMessages.CloseSessionSucces; }
        }

        protected override string SuccessMessageTitle
        {
            get { return UiResources.CloseSession.ToUpper(); }
        }

        protected override bool ShowSuccesMessage
        {
            get { return true; }
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

        #endregion
    }
}
