using System;
using mega;
using MegaApp.Enums;
using MegaApp.Resources;

namespace MegaApp.MegaApi
{
    class PauseTransferRequestListener: BaseRequestListener
    {
        public event EventHandler PauseTransfersFinished;

        private bool _pause;

        #region Base Properties

        protected override string ProgressMessage
        {
            get { return _pause ? ProgressMessages.PauseTransfers : ProgressMessages.ResumeTransfers; }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return _pause ? AppMessages.PausingTransfersFailed : AppMessages.ResumingTransfersFailed; }
        }

        protected override string ErrorMessageTitle
        {
            get { return _pause ? AppMessages.PausingTransfersFailed_Title: AppMessages.ResumingTransfersFailed_Title; }
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

        public override void onRequestStart(MegaSDK api, MRequest request)
        {
            //Get if transfers were paused (true) or resumed (false)
            _pause = request.getFlag();

            base.onRequestStart(api, request);
        }

        protected override void OnSuccesAction(MegaSDK api, MRequest request)
        {
            //Get if transfers were paused (true) or resumed (false)
            _pause = request.getFlag();

            if (this.PauseTransfersFinished != null)
                this.PauseTransfersFinished.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
