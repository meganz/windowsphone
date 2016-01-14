using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.Models;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    public class GlobalDriveListener: MGlobalListenerInterface
    {
        private readonly AppInformation _appInformation;

        public GlobalDriveListener(AppInformation appInformation)
        {
            _appInformation = appInformation;
            this.Nodes = new List<NodeDetailsViewModel>();
            this.Folders = new List<FolderViewModel>();
            this.Contacts= new List<ContactsViewModel>();
        }

        #region MGlobalListenerInterface

        public void onNodesUpdate(MegaSDK api, MNodeList nodes)
        {
            // exit methods when node list is incorrect
            if (nodes == null || nodes.size() < 1) return;

            try
            {
                // Retrieve the listsize for performance reasons and store local
                int listSize = nodes.size();

                for (int i = 0; i < listSize; i++)
                {
                    bool isProcessed = false;
                    
                    // Get the specific node that has an update. If null exit the method
                    // and process no notification
                    MNode megaNode = nodes.get(i);
                    if (megaNode == null) return;

                    // PROCESS THE FOLDERS LISTENERS
                    if (megaNode.isRemoved())
                    {
                        // REMOVED Scenario

                        foreach (var folder in Folders)
                        {
                            IMegaNode nodeToRemoveFromView = folder.ChildNodes.FirstOrDefault(
                                node => node.Base64Handle.Equals(megaNode.getBase64Handle()));
                            
                            // If node is found in current view, process the remove action
                            if (nodeToRemoveFromView != null)
                            {
                                // Needed because we are in a foreach loop to prevent the use of the wrong 
                                // local variable in the dispatcher code.
                                var currentFolder = folder; 
                                Deployment.Current.Dispatcher.BeginInvoke(() =>
                                {
                                    try
                                    {
                                        currentFolder.ChildNodes.Remove(nodeToRemoveFromView);
                                        ((FolderNodeViewModel) currentFolder.FolderRootNode).SetFolderInfo();
                                    }
                                    catch (Exception)
                                    {
                                        // Dummy catch, surpress possible exception
                                    }
                                });
                                
                                isProcessed = true;
                                break;
                            }
                        }

                        if (!isProcessed)
                        {
                            // REMOVED in subfolder scenario

                            MNode parentNode = api.getParentNode(megaNode);
                            
                            if(parentNode != null)
                            {
                                foreach (var folder in Folders)
                                {
                                    IMegaNode nodeToUpdateInView = folder.ChildNodes.FirstOrDefault(
                                        node => node.Base64Handle.Equals(parentNode.getBase64Handle()));

                                    // If parent folder is found, process the update action
                                    if (nodeToUpdateInView != null)
                                    {
                                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                                        {
                                            try
                                            {
                                                nodeToUpdateInView.Update(parentNode, folder.Type);
                                                var folderNode = nodeToUpdateInView as FolderNodeViewModel;
                                                if (folderNode != null) folderNode.SetFolderInfo();
                                            }
                                            catch (Exception)
                                            {
                                                // Dummy catch, surpress possible exception
                                            }
                                        });
                                    }
                                }
                            }
                        }
                    }
                    // UPDATE / ADDED scenarions
                    else
                    {
                        // UPDATE Scenario

                        // PROCESS THE SINGLE NODE(S) LISTENER(S) (NodeDetailsPage live updates)
                        foreach (var node in Nodes)
                        {
                            if (megaNode.getBase64Handle() == node.getNodeBase64Handle())
                                Deployment.Current.Dispatcher.BeginInvoke(() => node.updateNode(megaNode));
                        }

                        // Used in different scenario's
                        MNode parentNode = api.getParentNode(megaNode);

                        foreach (var folder in Folders)
                        {
                            IMegaNode nodeToUpdateInView = folder.ChildNodes.FirstOrDefault(
                                node => node.Base64Handle.Equals(megaNode.getBase64Handle()));

                            // If node is found, process the update action
                            if (nodeToUpdateInView != null)
                            {
                                bool isMoved = !folder.FolderRootNode.Base64Handle.Equals(parentNode.getBase64Handle());

                                // Is node is move to different folder. Remove from current folder view
                                if (isMoved)
                                {
                                    // Needed because we are in a foreach loop to prevent the use of the wrong 
                                    // local variable in the dispatcher code.
                                    var currentFolder = folder; 
                                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                                    {
                                        try
                                        {
                                            currentFolder.ChildNodes.Remove(nodeToUpdateInView);
                                            ((FolderNodeViewModel)currentFolder.FolderRootNode).SetFolderInfo();
                                            UpdateFolders(currentFolder);
                                        }
                                        catch (Exception)
                                        {
                                            // Dummy catch, surpress possible exception
                                        }
                                    });
                                    
                                }
                                // Node is updated with new data. Update node in current view
                                else
                                {
                                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                                    {
                                        try
                                        {
                                            nodeToUpdateInView.Update(megaNode, folder.Type);
                                        }
                                        catch (Exception)
                                        {
                                            // Dummy catch, surpress possible exception
                                        }
                                    });
                                    isProcessed = true;
                                    break;
                                }
                               
                            }
                        }
                        
                        // ADDED scenario
                        
                        if (parentNode != null && !isProcessed)
                        {
                            foreach (var folder in Folders)
                            {
                                bool isAddedInFolder = folder.FolderRootNode.Base64Handle.Equals(parentNode.getBase64Handle());

                                // If node is added in current folder, process the add action
                                if (isAddedInFolder)
                                {
                                    // Retrieve the index from the SDK
                                    // Substract -1 to get a valid list index
                                    int insertIndex = api.getIndex(megaNode,
                                        UiService.GetSortOrder(parentNode.getBase64Handle(),
                                            parentNode.getName())) - 1;

                                    // If the insert position is higher than the ChilNodes size insert in the last position
                                    if (insertIndex >= folder.ChildNodes.Count())
                                    {
                                        // Needed because we are in a foreach loop to prevent the use of the wrong 
                                        // local variable in the dispatcher code.
                                        var currentFolder = folder;
                                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                                        {
                                            try
                                            {
                                                currentFolder.ChildNodes.Add(NodeService.CreateNew(api,
                                                    _appInformation, megaNode, currentFolder.Type));
                                                
                                                ((FolderNodeViewModel)currentFolder.FolderRootNode).SetFolderInfo();
                                                UpdateFolders(currentFolder);
                                            }
                                            catch (Exception)
                                            {
                                                // Dummy catch, surpress possible exception
                                            }
                                        });
                                    }
                                    // Insert the node at a specific position
                                    else
                                    {
                                        // Insert position can never be less then zero
                                        // Replace negative index with first possible index zero
                                        if (insertIndex < 0) insertIndex = 0;

                                        // Needed because we are in a foreach loop to prevent the use of the wrong 
                                        // local variable in the dispatcher code.
                                        var currentFolder = folder;
                                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                                        {
                                            try
                                            {
                                                currentFolder.ChildNodes.Insert(insertIndex,
                                                    NodeService.CreateNew(api, _appInformation, megaNode, currentFolder.Type));

                                                ((FolderNodeViewModel)currentFolder.FolderRootNode).SetFolderInfo();
                                                UpdateFolders(currentFolder);
                                            }
                                            catch (Exception)
                                            {
                                                // Dummy catch, surpress possible exception
                                            }
                                        });
                                    }
                                      
                                    break;
                                }
                                    
                                // ADDED in subfolder scenario
                                IMegaNode nodeToUpdateInView = folder.ChildNodes.FirstOrDefault(
                                    node => node.Base64Handle.Equals(parentNode.getBase64Handle()));

                                if (nodeToUpdateInView != null)
                                {
                                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                                    {
                                        try
                                        {
                                            nodeToUpdateInView.Update(parentNode, folder.Type);
                                            var folderNode = nodeToUpdateInView as FolderNodeViewModel;
                                            if (folderNode != null) folderNode.SetFolderInfo();
                                        }
                                        catch (Exception)
                                        {
                                            // Dummy catch, surpress possible exception
                                        }
                                    });
                                    break;
                                }

                                // Unconditional scenarios
                                // Move/delete/add actions in subfolders
                                var localFolder = folder;
                                Deployment.Current.Dispatcher.BeginInvoke(() =>
                                {
                                    try
                                    {
                                        UpdateFolders(localFolder);
                                    }
                                    catch (Exception)
                                    {
                                        // Dummy catch, surpress possible exception
                                    }
                                });
                            }
                        }
                    }
                }                
            }
            catch (Exception)
            {
                // Dummy catch, surpress possible exception 
            }
        }

        public void onReloadNeeded(MegaSDK api)
        {
           // throw new NotImplementedException();
        }

        public void onAccountUpdate(MegaSDK api)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                var customMessageDialog = new CustomMessageDialog(
                    AppMessages.AccountUpdated_Title,
                    AppMessages.AccountUpdate,
                    App.AppInformation,
                    MessageDialogButtons.YesNo);

                customMessageDialog.OkOrYesButtonTapped += (sender, args) =>
                {
                    NavigateService.NavigateTo(typeof(MyAccountPage), NavigationParameter.Normal);
                };                

                customMessageDialog.ShowDialog();
            });
        }

        public void onContactRequestsUpdate(MegaSDK api, MContactRequestList requests)
        {            
            foreach (var contacts in Contacts)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    contacts.GetReceivedContactRequests();
                    contacts.GetSentContactRequests();
                });
            }
        }

        public void onUsersUpdate(MegaSDK api, MUserList users)
        {
            foreach (var contacts in Contacts)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    contacts.GetMegaContacts();
                });
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Update information of all folder nodes in a folder view
        /// </summary>
        /// <param name="folder">Folder view to update</param>
        private static void UpdateFolders(FolderViewModel folder)
        {
            foreach (var folderNode in folder.ChildNodes
                .Where(f => f is FolderNodeViewModel)
                .Cast<FolderNodeViewModel>()
                .ToList())
            {
                folderNode.SetFolderInfo();
            }
        }

        #endregion

        #region Properties

        public IList<NodeDetailsViewModel> Nodes { get; private set; } 
        public IList<FolderViewModel> Folders { get; private set; }
        public IList<ContactsViewModel> Contacts { get; private set; } 

        #endregion
    }
}
