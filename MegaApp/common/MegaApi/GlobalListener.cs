using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.ViewModels;
using MegaApp.Views;

namespace MegaApp.MegaApi
{
    public class GlobalListener: MGlobalListenerInterface
    {
        private readonly AppInformation _appInformation;

        public GlobalListener(AppInformation appInformation)
        {
            _appInformation = appInformation;
            this.Nodes = new List<NodeDetailsViewModel>();
            this.Folders = new List<FolderViewModel>();
            this.Contacts = new List<ContactsViewModel>();
            this.ContactsDetails = new List<ContactDetailsViewModel>();
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
            AccountService.GetAccountDetails();

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
            if (users == null || users.size() < 1) return;

            for (int i = 0; i < users.size(); i++)
            {
                MUser user = users.get(i);
                if (user == null) continue;

                // If the change is on the current user                
                if(user.getHandle().Equals(api.getMyUser().getHandle()) && !Convert.ToBoolean(user.isOwnChange()))
                {
                    if (user.hasChanged((int)MUserChangeType.CHANGE_TYPE_AVATAR) &&
                            !String.IsNullOrWhiteSpace(AccountService.AccountDetails.AvatarPath))
                    {
                        api.getUserAvatar(user, AccountService.AccountDetails.AvatarPath,
                            new GetUserAvatarRequestListener());
                    }

                    if (user.hasChanged((int)MUserChangeType.CHANGE_TYPE_EMAIL))
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                            AccountService.AccountDetails.UserEmail = user.getEmail());
                    }

                    if (user.hasChanged((int)MUserChangeType.CHANGE_TYPE_FIRSTNAME))
                    {
                        api.getUserAttribute(user, (int)MUserAttrType.USER_ATTR_FIRSTNAME,
                            new GetUserDataRequestListener());
                    }

                    if (user.hasChanged((int)MUserChangeType.CHANGE_TYPE_LASTNAME))
                    {
                        api.getUserAttribute(user, (int)MUserAttrType.USER_ATTR_LASTNAME,
                            new GetUserDataRequestListener());
                    }                    
                }
                else // If the change is on a contact
                {
                    // If there are any ContactsViewModel active
                    foreach (var contactViewModel in Contacts)
                    {
                        Contact existingContact = contactViewModel.MegaContactsList.FirstOrDefault(
                            contact => contact.Handle.Equals(user.getHandle()));

                        // If the contact exists in the contact list
                        if(existingContact != null)
                        {
                            // If the contact is no longer a contact (REMOVE CONTACT SCENARIO)
                            if (!existingContact.Visibility.Equals(user.getVisibility()) && 
                                !(user.getVisibility().Equals(MUserVisibility.VISIBILITY_VISIBLE)))
                            {
                                Deployment.Current.Dispatcher.BeginInvoke(() =>
                                    contactViewModel.MegaContactsList.Remove(existingContact));
                            }
                            // If the contact has been changed (UPDATE CONTACT SCENARIO) and is not an own change
                            else if (!Convert.ToBoolean(user.isOwnChange())) 
                            {
                                if (user.hasChanged((int)MUserChangeType.CHANGE_TYPE_AVATAR) &&
                                    !String.IsNullOrWhiteSpace(existingContact.AvatarPath))
                                {
                                    api.getUserAvatar(user, existingContact.AvatarPath, 
                                        new GetContactAvatarRequestListener(existingContact));
                                }
                                
                                if (user.hasChanged((int)MUserChangeType.CHANGE_TYPE_EMAIL))
                                {
                                    Deployment.Current.Dispatcher.BeginInvoke(() => 
                                        existingContact.Email = user.getEmail());
                                }
                                
                                if (user.hasChanged((int)MUserChangeType.CHANGE_TYPE_FIRSTNAME))
                                {
                                    api.getUserAttribute(user, (int)MUserAttrType.USER_ATTR_FIRSTNAME, 
                                        new GetContactDataRequestListener(existingContact));
                                }

                                if (user.hasChanged((int)MUserChangeType.CHANGE_TYPE_LASTNAME))
                                {
                                    api.getUserAttribute(user, (int)MUserAttrType.USER_ATTR_LASTNAME, 
                                        new GetContactDataRequestListener(existingContact));
                                }
                            }
                        }
                        // If is a new contact (ADD CONTACT SCENARIO - REQUEST ACCEPTED)
                        else if (user.getVisibility().Equals(MUserVisibility.VISIBILITY_VISIBLE))
                        {
                            var _megaContact = new Contact()
                            {
                                Handle = user.getHandle(),
                                Email = user.getEmail(),
                                Timestamp = user.getTimestamp(),
                                Visibility = user.getVisibility(),
                                AvatarColor = UiService.GetColorFromHex(SdkService.MegaSdk.getUserAvatarColor(user))
                            };

                            Deployment.Current.Dispatcher.BeginInvoke(() => 
                                contactViewModel.MegaContactsList.Add(_megaContact));

                            api.getUserAttribute(user, (int)MUserAttrType.USER_ATTR_FIRSTNAME, 
                                new GetContactDataRequestListener(_megaContact));
                            api.getUserAttribute(user, (int)MUserAttrType.USER_ATTR_LASTNAME, 
                                new GetContactDataRequestListener(_megaContact));
                            api.getUserAvatar(user, _megaContact.AvatarPath, 
                                new GetContactAvatarRequestListener(_megaContact));                            
                        }
                    }

                    // If there are any ContactDetailsViewModel active
                    foreach (var contactDetailsViewModel in ContactsDetails)
                    {
                        // If the selected contact has been changed (UPDATE CONTACT SCENARIO)
                        if (contactDetailsViewModel.SelectedContact.Handle.Equals(user.getHandle()))
                        {                            
                            if (user.hasChanged((int)MUserChangeType.CHANGE_TYPE_AVATAR) &&
                                !String.IsNullOrWhiteSpace(contactDetailsViewModel.SelectedContact.AvatarPath))
                            {
                                api.getUserAvatar(user, contactDetailsViewModel.SelectedContact.AvatarPath,
                                    new GetContactAvatarRequestListener(contactDetailsViewModel.SelectedContact));
                            }

                            if (user.hasChanged((int)MUserChangeType.CHANGE_TYPE_EMAIL))
                            {
                                Deployment.Current.Dispatcher.BeginInvoke(() => 
                                    contactDetailsViewModel.SelectedContact.Email = user.getEmail());
                            }

                            if (user.hasChanged((int)MUserChangeType.CHANGE_TYPE_FIRSTNAME))
                            {
                                api.getUserAttribute(user, (int)MUserAttrType.USER_ATTR_FIRSTNAME,
                                        new GetContactDataRequestListener(contactDetailsViewModel.SelectedContact));                                
                            }

                            if (user.hasChanged((int)MUserChangeType.CHANGE_TYPE_LASTNAME))
                            {
                                api.getUserAttribute(user, (int)MUserAttrType.USER_ATTR_LASTNAME,
                                        new GetContactDataRequestListener(contactDetailsViewModel.SelectedContact));
                            }
                        }
                    }
                }
            }
        }

        public void onEvent(MegaSDK api, MEvent ev)
        {
            // If the account has been blocked
            if (ev.getType() == MEventType.EVENT_ACCOUNT_BLOCKED)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Blocked account: " + ev.getText());

                // A blocked account automatically triggers a logout
                AppService.LogoutActions();

                // Show the login page with the corresponding navigation parameter
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    NavigateService.NavigateTo(typeof(InitTourPage), NavigationParameter.API_EBLOCKED,
                        new Dictionary<string, string>
                        {
                            { "Number", ev.getNumber().ToString() },
                            { "Text", ev.getText() }
                        });
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
        public IList<ContactDetailsViewModel> ContactsDetails { get; private set; }

        #endregion
    }
}
