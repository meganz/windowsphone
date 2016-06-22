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
        private readonly bool _isImportFolderProcess;

        public CreateFolderRequestListener(bool isImportFolderProcess = false)
        {
            this._isImportFolderProcess = isImportFolderProcess;
        }

        #region Base Properties

        protected override string ProgressMessage
        {
            get { return ProgressMessages.PM_CreateFolder; }
        }

        protected override bool ShowProgressMessage
        {
            get { return !_isImportFolderProcess; }
        }

        protected override string ErrorMessage
        {
            get { return AppMessages.CreateFolderFailed; }
        }

        protected override string ErrorMessageTitle
        {
            get { return AppMessages.CreateFolderFailed_Title; }
        }

        protected override bool ShowErrorMessage
        {
            get { return !_isImportFolderProcess; }
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

        protected override void OnSuccesAction(MegaSDK api, MRequest request)
        {
            if(_isImportFolderProcess)
            {
                MNode parentNode = api.getNodeByHandle(request.getParentHandle());
                MNode newFolderNode = api.getNodeByHandle(request.getNodeHandle());

                if(!App.LinkInformation.FoldersToImport.ContainsKey(parentNode.getBase64Handle()))
                    return;

                var megaNodes = App.LinkInformation.FoldersToImport[parentNode.getBase64Handle()];

                MNode nodeToImport;
                foreach(var node in megaNodes)
                {
                    String nodePath = App.LinkInformation.FolderPaths[node.getBase64Handle()];                    
                    
                    if (String.Compare(newFolderNode.getName(), nodePath.Split('/').Last()) == 0)
                    {
                        nodeToImport = node;
                        ImportNodeContents(nodeToImport, newFolderNode);

                        //List<MNode> tempNodesArray = megaNodes;
                        foreach (var tempNode in megaNodes)
                        {
                            if(nodeToImport.getBase64Handle() == tempNode.getBase64Handle())
                            {
                                megaNodes.Remove(tempNode);
                                App.LinkInformation.FoldersToImport.Remove(parentNode.getBase64Handle());
                                App.LinkInformation.FoldersToImport.Add(parentNode.getBase64Handle(), megaNodes);
                                break;
                            }
                        }

                        if (megaNodes.Count == 0)
                            App.LinkInformation.FoldersToImport.Remove(parentNode.getBase64Handle());

                        App.LinkInformation.FolderPaths.Remove(nodeToImport.getBase64Handle());
                        break;
                    }
                }
            }
        }

        private void ImportNodeContents(MNode nodeToImport, MNode parentNode)
        {
            var childNodes = App.MegaSdkFolderLinks.getChildren(nodeToImport);
            var childNodesSize = childNodes.size();

            List<MNode> folderNodesToImport = new List<MNode>();
            for (int i = 0; i < childNodesSize; i++)
            {
                var node = childNodes.get(i);
                if(node.isFolder())
                {
                    folderNodesToImport.Add(node);                    

                    if(!App.LinkInformation.FolderPaths.ContainsKey(node.getBase64Handle()))
                    {
                        App.LinkInformation.FolderPaths.Add(node.getBase64Handle(),
                            App.MegaSdkFolderLinks.getNodePath(node));
                    }
                }
                else
                {
                    App.MegaSdk.copyNode(node, parentNode, new CopyNodeRequestListener());
                }
            }

            if (!App.LinkInformation.FoldersToImport.ContainsKey(parentNode.getBase64Handle()))
                App.LinkInformation.FoldersToImport.Add(parentNode.getBase64Handle(), folderNodesToImport);

            foreach (var node in folderNodesToImport)
            {
                App.MegaSdk.createFolder(node.getName(), parentNode, 
                    new CreateFolderRequestListener(true));
            }
        }
    }
}
