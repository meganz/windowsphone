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
        public RemoveNodeRequestListener(NodeViewModel nodeViewModel)
        {
            this._nodeViewModel = nodeViewModel;
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

        #endregion
    }
}
