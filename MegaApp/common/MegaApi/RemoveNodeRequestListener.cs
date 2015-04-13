using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
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
        private MNodeType _nodeType;
        private AutoResetEvent _waitEventRequest;

        public RemoveNodeRequestListener(NodeViewModel nodeViewModel, bool isMultiRemove, MNodeType nodeType, AutoResetEvent waitEventRequest)
        {
            this._nodeViewModel = nodeViewModel;
            this._isMultiRemove = isMultiRemove;
            this._nodeType = nodeType;
            this._waitEventRequest = waitEventRequest;
        }

        #region Base Properties

        protected override string ProgressMessage
        {
            get 
            {
                if (_nodeType == MNodeType.TYPE_RUBBISH)
                    return ProgressMessages.RemoveNode;
                else
                    return ProgressMessages.MoveNode;
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
                if (_nodeType == MNodeType.TYPE_RUBBISH)
                    return AppMessages.RemoveNodeFailed;
                else
                    return AppMessages.MoveToRubbishBinFailed;                    
            }
        }

        protected override string ErrorMessageTitle
        {
            get 
            {
                if (_nodeType == MNodeType.TYPE_RUBBISH)
                    return AppMessages.RemoveNodeFailed_Title;
                else
                    return AppMessages.MoveToRubbishBinFailed_Title;                    
            }
        }

        protected override bool ShowErrorMessage
        {
            get { return true; }
        }

        protected override string SuccessMessage
        {            
            get 
            {
                if (_nodeType == MNodeType.TYPE_RUBBISH)
                    return AppMessages.RemoveNodeSucces;
                else
                    return AppMessages.MoveToRubbishBinSuccess;                    
            }
        }

        protected override string SuccessMessageTitle
        {
            get 
            {
                if (_nodeType == MNodeType.TYPE_RUBBISH)
                    return AppMessages.RemoveNodeSuccess_Title;
                else
                    return AppMessages.MoveToRubbishBinSuccess_Title;                    
            }
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

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["MegaGrayBackgroundColor"]);
                ProgressService.SetProgressIndicator(false);
            });

            if (e.getErrorCode() == MErrorType.API_OK)
            {
                if (this._waitEventRequest != null)
                    this._waitEventRequest.Set();

                if (ShowSuccesMessage && !_isMultiRemove)
                    Deployment.Current.Dispatcher.BeginInvoke(
                        () => MessageBox.Show(SuccessMessage, SuccessMessageTitle, MessageBoxButton.OK));

                if (ActionOnSucces)
                    OnSuccesAction(api, request);

                if (NavigateOnSucces)
                    Deployment.Current.Dispatcher.BeginInvoke(() => NavigateService.NavigateTo(NavigateToPage, NavigationParameter));
            }
            else if (e.getErrorCode() != MErrorType.API_EINCOMPLETE)
                if (ShowErrorMessage)
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        MessageBox.Show(String.Format(ErrorMessage, e.getErrorString()), ErrorMessageTitle, MessageBoxButton.OK));

        }

        protected override void OnSuccesAction(MegaSDK api, MRequest request)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                try 
                {
                    if ((_nodeViewModel != null) && (_nodeViewModel.ParentCollection != null))
                    {
                        if (_nodeViewModel.ParentCollection is ObservableCollection<NodeViewModel>)
                        {
                            ((ObservableCollection<NodeViewModel>)_nodeViewModel.ParentCollection).Remove(_nodeViewModel);
                        }
                    }

                    _nodeViewModel = null;
                }
                catch (Exception) { }
            });
        }

        #endregion
    }
}
