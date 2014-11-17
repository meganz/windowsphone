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
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    class MoveNodeRequestListener: BaseRequestListener
    {
        private readonly NodeViewModel _rootNode;
        private readonly NodeViewModel _nodeToMove;
        public MoveNodeRequestListener(NodeViewModel rootNode, NodeViewModel nodeToMove)
        {
            this._rootNode = rootNode;
            this._nodeToMove = nodeToMove;
        }

        #region Base Properties

        protected override string ProgressMessage
        {
            get { return ProgressMessages.MoveNode; }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return AppMessages.MoveFailed; }
        }

        protected override string ErrorMessageTitle
        {
            get { return AppMessages.MoveFailed_Title; }
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

        protected override void OnSuccesAction(MegaSDK api, MRequest request)
        {
            ((ObservableCollection<NodeViewModel>)_rootNode.ChildCollection).Add(_nodeToMove);
        }

        #endregion
    }
}
