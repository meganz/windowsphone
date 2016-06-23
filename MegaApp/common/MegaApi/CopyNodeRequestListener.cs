using System;
using MegaApp.Enums;
using MegaApp.Resources;

namespace MegaApp.MegaApi
{
    class CopyNodeRequestListener : BaseRequestListener
    {
        /// <summary>
        /// Variable to store if is a copy node request sent during import a folder.
        /// </summary>
        private readonly bool _isImportFolderProcess;

        /// <summary>
        /// Constructor of the listener for a copy node request.
        /// </summary>
        /// <param name="isImportFolderProcess">
        /// Value to indicate if is a copy node request sent during import a folder
        /// </param>
        public CopyNodeRequestListener(bool isImportFolderProcess = false)
        {
            this._isImportFolderProcess = isImportFolderProcess;
        }

        #region Base Properties

        protected override string ProgressMessage
        {
            get { return _isImportFolderProcess ? ProgressMessages.ImportFile : ProgressMessages.PM_CopyNode; }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return _isImportFolderProcess ? AppMessages.AM_ImportFileFailedNoErrorCode : AppMessages.AM_CopyFailed; }
        }

        protected override string ErrorMessageTitle
        {
            get { return _isImportFolderProcess ? AppMessages.ImportFileFailed_Title : AppMessages.AM_CopyFailed_Title; }
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
    }
}
