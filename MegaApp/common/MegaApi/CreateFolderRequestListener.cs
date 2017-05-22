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

        protected override void OnSuccesAction(MegaSDK api, MRequest request)
        {
            if(_isImportFolderProcess)
            {
                MNode parentNode = api.getNodeByHandle(request.getParentHandle());
                MNode newFolderNode = api.getNodeByHandle(request.getNodeHandle());

                if(!App.LinkInformation.FoldersToImport.ContainsKey(parentNode.getBase64Handle()))
                    return;

                // Get from the corresponding dictionary all the folders parent folder and explore them.
                var megaNodes = App.LinkInformation.FoldersToImport[parentNode.getBase64Handle()];
                foreach(var node in megaNodes)
                {
                    if(App.LinkInformation.FolderPaths.ContainsKey(node.getBase64Handle()))
                    {
                        String nodePath = App.LinkInformation.FolderPaths[node.getBase64Handle()];

                        // If the name of the new folder matches with the last part of the node path  
                        // obtained from the dictionary, then this is the current imported folder.
                        if (String.Compare(newFolderNode.getName(), nodePath.Split('/').Last()) == 0)
                        {
                            // Import the content of a recently created node folder.
                            ImportNodeContents(node, newFolderNode);

                            // Remove the node from the list and update the dictionaries.
                            megaNodes.Remove(node);
                            if(App.LinkInformation.FoldersToImport.ContainsKey(parentNode.getBase64Handle()))
                                App.LinkInformation.FoldersToImport.Remove(parentNode.getBase64Handle());

                            if (megaNodes.Count > 0)
                            {
                                if(!App.LinkInformation.FoldersToImport.ContainsKey(parentNode.getBase64Handle()))
                                    App.LinkInformation.FoldersToImport.Add(parentNode.getBase64Handle(), megaNodes);
                            }

                            if (App.LinkInformation.FolderPaths.ContainsKey(node.getBase64Handle()))
                                App.LinkInformation.FolderPaths.Remove(node.getBase64Handle());
                            break;
                        }
                    }                    
                }
            }
        }

        /// <summary>
        /// Method to import the content of a recently created node folder during 
        /// the process to import a folder link.
        /// </summary>
        /// <param name="nodeToImport">Node folder to import.</param>
        /// <param name="parentNode">Parent folder of the node to import.</param>
        private void ImportNodeContents(MNode nodeToImport, MNode parentNode)
        {
            // Obtain the child nodes and the number of them.
            var childNodes = SdkService.MegaSdkFolderLinks.getChildren(nodeToImport);
            var childNodesSize = childNodes.size();

            // Explore the child nodes. Store the folders in a new list and add them to
            // the corresponding dictionary. Copy the file nodes.
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
                            SdkService.MegaSdkFolderLinks.getNodePath(node));
                    }
                }
                else
                {
                    SdkService.MegaSdk.copyNode(node, parentNode, new CopyNodeRequestListener(true));
                }
            }

            // Add the list with the new folder nodes to import to the corresponding dictionary.
            if (!App.LinkInformation.FoldersToImport.ContainsKey(parentNode.getBase64Handle()))
                App.LinkInformation.FoldersToImport.Add(parentNode.getBase64Handle(), folderNodesToImport);

            // Create all the new folder nodes.
            foreach (var node in folderNodesToImport)
            {
                SdkService.MegaSdk.createFolder(node.getName(), parentNode, 
                    new CreateFolderRequestListener(true));
            }
        }
    }
}
