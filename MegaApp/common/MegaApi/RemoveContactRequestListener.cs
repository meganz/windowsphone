using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MegaApp.Enums;
using MegaApp.Resources;

namespace MegaApp.MegaApi
{
    class RemoveContactRequestListener : BaseRequestListener
    {
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
    }
}
