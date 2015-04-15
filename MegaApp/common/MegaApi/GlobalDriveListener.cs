using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    class GlobalDriveListener: MGlobalListenerInterface
    {
        private readonly CloudDriveViewModel _cloudDriveViewModel;
        private readonly AppInformation _appInformation;
        public GlobalDriveListener(CloudDriveViewModel cloudDriveViewModel, AppInformation appInformation)
        {
            _cloudDriveViewModel = cloudDriveViewModel;
            _appInformation = appInformation;
        }

        public void onNodesUpdate(MegaSDK api, MNodeList nodes)
        {
            if (nodes == null) return;

            try
            {
                for (int i = 0; i < nodes.size(); i++)
                {
                    MNode megaNode = nodes.get(i);

                    // Don't process the node, because it will be processed in the request listener
                    //if (megaNode.getTag() != 0) continue;

                    if (megaNode == null) return;

                    // Removed node
                    if (megaNode.isRemoved())
                    {
                        var nodeToRemove = _cloudDriveViewModel.ChildNodes.FirstOrDefault(
                            n => n.Handle.Equals(megaNode.getHandle()));

                        if (nodeToRemove != null)
                        {
                            Deployment.Current.Dispatcher.BeginInvoke(() => 
                                {
                                    try{ _cloudDriveViewModel.ChildNodes.Remove(nodeToRemove); }
                                    catch (Exception) { }
                                });
                        }
                        else
                        {
                            MNode parentNode = api.getParentNode(megaNode);
                            if (parentNode == null) return;
                            var nodeToUpdate = _cloudDriveViewModel.ChildNodes.FirstOrDefault(
                                n => n.Handle.Equals(parentNode.getHandle()));
                            if (nodeToUpdate == null) return;
                            Deployment.Current.Dispatcher.BeginInvoke(() => 
                                {
                                    try { nodeToUpdate.Update(parentNode); }
                                    catch (Exception) { }
                                });
                        }
                    }
                    else // Added/Updated node
                    {
                        var nodeToUpdate = _cloudDriveViewModel.ChildNodes.FirstOrDefault(
                            n => n.Handle.Equals(megaNode.getHandle()));
                        if (nodeToUpdate != null)
                        {
                            MNode parentNode = api.getParentNode(megaNode);
                            if (_cloudDriveViewModel.CurrentRootNode.Handle.Equals(parentNode.getHandle()))
                            {
                                Deployment.Current.Dispatcher.BeginInvoke(() => 
                                    {
                                        try { nodeToUpdate.Update(megaNode); }
                                        catch (Exception) { }
                                    });
                            }
                            else
                            {
                                Deployment.Current.Dispatcher.BeginInvoke(() =>
                                    {
                                        try { _cloudDriveViewModel.ChildNodes.Remove(nodeToUpdate); }
                                        catch (Exception) { }
                                    });
                            }
                        }
                        else // Added node
                        {
                            MNode parentNode = api.getParentNode(megaNode);
                            if (parentNode != null)
                            {
                                if (_cloudDriveViewModel.CurrentRootNode.Handle.Equals(parentNode.getHandle()))
                                {
                                    int insertIndex = api.getIndex(megaNode, UiService.GetSortOrder(parentNode.getHandle(), 
                                        parentNode.getName()));

                                    // If the insert position is higher than the ChilNodes size insert in the last position
                                    if (insertIndex > _cloudDriveViewModel.ChildNodes.Count())
                                        insertIndex = _cloudDriveViewModel.ChildNodes.Count() - 1;

                                    // Force to be at least the first position
                                    if (insertIndex <= 0) insertIndex = 1;                                    

                                    if (insertIndex > 0)
                                    {
                                       
                                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                                        {
                                            try 
                                            { 
                                                _cloudDriveViewModel.ChildNodes.Insert(
                                                    insertIndex - 1, 
                                                    NodeService.CreateNew(
                                                        api,
                                                        _appInformation,
                                                        megaNode,
                                                        _cloudDriveViewModel.ChildNodes));
                                               
                                            }
                                            catch (Exception) { }
                                        });
                                      
                                    }                                        
                                }
                                else
                                {
                                    var folderNodeToUpdate = _cloudDriveViewModel.ChildNodes.FirstOrDefault(
                                         n => n.Handle.Equals(parentNode.getHandle()));
                                    if (folderNodeToUpdate != null)
                                    {
                                        Deployment.Current.Dispatcher.BeginInvoke(() => 
                                            {
                                                try { folderNodeToUpdate.Update(parentNode); }
                                                catch (Exception){ }
                                            });
                                    }                                    
                                }
                            }
                        }
                    }
                }                
            }
            catch (Exception)
            {
                // No exception handling. If it fails. 
            }

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    ((FolderNodeViewModel)(_cloudDriveViewModel.CurrentRootNode)).SetFolderInfo();

                    foreach (var node in _cloudDriveViewModel.ChildNodes.Where(
                        n => n is FolderNodeViewModel).Cast<FolderNodeViewModel>())
                    {
                        node.SetFolderInfo();
                    }
                }
                catch (Exception) { }
            });
        }

        public void onReloadNeeded(MegaSDK api)
        {
           // throw new NotImplementedException();
        }

        public void onUsersUpdate(MegaSDK api, MUserList users)
        {
           // throw new NotImplementedException();
        }
    }
}
