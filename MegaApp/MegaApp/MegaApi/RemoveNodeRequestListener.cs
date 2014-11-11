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
using MegaApp.Extensions;
using MegaApp.Models;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    class RemoveNodeRequestListener: BaseRequestListener
    {
        private NodeViewModel _nodeViewModel;
        private bool _isMultiRemove;        
        public RemoveNodeRequestListener(NodeViewModel nodeViewModel, bool isMultiRemove)
        {
            this._nodeViewModel = nodeViewModel;
            this._isMultiRemove = isMultiRemove;
        }

        #region Base Properties

        protected override string ProgressMessage
        {
            get { return ProgressMessages.RemoveNode; }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return AppMessages.RemoveNodeFailed; }
        }

        protected override string ErrorMessageTitle
        {
            get { return AppMessages.RemoveNodeFailed_Title; }
        }

        protected override bool ShowErrorMessage
        {
            get { return true; }
        }

        protected override string SuccessMessage
        {
            get { return AppMessages.RemoveNodeSucces; }
        }

        protected override string SuccessMessageTitle
        {
            get { return AppMessages.RemoveNodeSuccess_Title; }
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

        protected override void OnSuccesAction(MRequest request)
        {
            if (_nodeViewModel.ParentCollection != null)
            {
                if (_nodeViewModel.ParentCollection is ObservableCollection<NodeViewModel>)
                    ((ObservableCollection<NodeViewModel>) _nodeViewModel.ParentCollection).Remove(_nodeViewModel);
            }
            _nodeViewModel = null;
        }

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgessService.SetProgressIndicator(false);

                if (e.getErrorCode() == MErrorType.API_OK)
                {
                    if (ShowSuccesMessage && !_isMultiRemove)
                        MessageBox.Show(SuccessMessage, SuccessMessageTitle, MessageBoxButton.OK);

                    if (ActionOnSucces)
                        OnSuccesAction(request);

                    if (NavigateOnSucces)
                        NavigateService.NavigateTo(NavigateToPage, NavigationParameter);
                }
                else if (e.getErrorCode() != MErrorType.API_EINCOMPLETE)
                    if (ShowErrorMessage)
                        MessageBox.Show(String.Format(ErrorMessage, e.getErrorString()), ErrorMessageTitle, MessageBoxButton.OK);
            });
        }

        #endregion
    }
}
