using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Extensions;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    class CreateFolderRequestListener: BaseRequestListener
    {
        /// <summary>
        /// Variable to store if is a create folder request sent during import a folder.
        /// </summary>
        private readonly bool _isImportFolderProcess;

        /// <summary>
        /// Constructor of the listener for a create folder request.
        /// </summary>
        /// <param name="isImportFolderProcess">
        /// Value to indicate if is a create folder request sent during import a folder
        /// </param>
        public CreateFolderRequestListener(bool isImportFolderProcess = false)
        {
            this._isImportFolderProcess = isImportFolderProcess;
        }

        #region Base Properties

        protected override string ProgressMessage
        {
            get { return _isImportFolderProcess ? ProgressMessages.PM_ImportFolder : ProgressMessages.PM_CreateFolder; }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return _isImportFolderProcess ? AppMessages.AM_ImportFolderFailed : AppMessages.CreateFolderFailed; }
        }

        protected override string ErrorMessageTitle
        {
            get { return _isImportFolderProcess ? AppMessages.AM_ImportFolderFailed_Title : AppMessages.CreateFolderFailed_Title; }
        }

        protected override bool ShowErrorMessage
        {
            get { return true; }
        }

        protected override string SuccessMessage
        {
            get { return AppMessages.CreateFolderSuccess; }
        }

        protected override string SuccessMessageTitle
        {
            get { return AppMessages.CreateFolderSuccess_Title; }
        }

        protected override bool ShowSuccesMessage
        {
            get { return !_isImportFolderProcess; }
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
