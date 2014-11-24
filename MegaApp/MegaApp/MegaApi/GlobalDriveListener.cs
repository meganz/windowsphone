using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using mega;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    class GlobalDriveListener: MGlobalListenerInterface
    {
        private readonly CloudDriveViewModel _cloudDriveViewModel;
        public GlobalDriveListener(CloudDriveViewModel cloudDriveViewModel)
        {
            _cloudDriveViewModel = cloudDriveViewModel;
        }

        public void onNodesUpdate(MegaSDK api, MNodeList nodes)
        {
            if (nodes == null) return;

            try
            {
                for (int i = 0; i < nodes.size(); i++)
                {
                    MNode megaNode = nodes.get(i);

                    if (megaNode == null) return;

                    if (megaNode.isRemoved())
                    {
                        NodeViewModel nodeToRemove = _cloudDriveViewModel.ChildNodes.FirstOrDefault(
                            n => n.Handle.Equals(megaNode.getHandle()));
                        if (nodeToRemove != null)
                        {
                            Deployment.Current.Dispatcher.BeginInvoke(
                                () => _cloudDriveViewModel.ChildNodes.Remove(nodeToRemove));
                        }
                        else
                        {
                            MNode parentNode = api.getParentNode(megaNode);
                            if (parentNode == null) return;
                            NodeViewModel nodeToUpdate = _cloudDriveViewModel.ChildNodes.FirstOrDefault(
                                n => n.Handle.Equals(parentNode.getHandle()));
                            if (nodeToUpdate == null) return;
                            Deployment.Current.Dispatcher.BeginInvoke(() => nodeToUpdate.Update(parentNode));
                        }
                    }
                    else
                    {
                        NodeViewModel nodeToUpdate = _cloudDriveViewModel.ChildNodes.FirstOrDefault(
                            n => n.Handle.Equals(megaNode.getHandle()));
                        if (nodeToUpdate != null)
                        {
                            MNode parentNode = api.getParentNode(megaNode);
                            if (_cloudDriveViewModel.CurrentRootNode.Handle.Equals(parentNode.getHandle()))
                            {
                                Deployment.Current.Dispatcher.BeginInvoke(() => nodeToUpdate.Update(megaNode));
                            }
                            else
                            {
                                Deployment.Current.Dispatcher.BeginInvoke(
                                () => _cloudDriveViewModel.ChildNodes.Remove(nodeToUpdate));
                            }
                        }
                        else
                        {
                            MNode parentNode = api.getParentNode(megaNode);
                            if (parentNode != null)
                            {
                                if (_cloudDriveViewModel.CurrentRootNode.Handle.Equals(parentNode.getHandle()))
                                {
                                    int insertIndex = api.getIndex(megaNode, UiService.GetSortOrder(parentNode.getHandle(),
                                        parentNode.getName()));

                                    if (insertIndex > 0)
                                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                                            _cloudDriveViewModel.ChildNodes.Insert(insertIndex - 1, new NodeViewModel(api, megaNode,
                                            _cloudDriveViewModel.ChildNodes)));
                                }
                                else
                                {
                                    NodeViewModel folderNodeToUpdate = _cloudDriveViewModel.ChildNodes.FirstOrDefault(
                                         n => n.Handle.Equals(parentNode.getHandle()));
                                    if (folderNodeToUpdate != null)
                                        Deployment.Current.Dispatcher.BeginInvoke(() => folderNodeToUpdate.Update(parentNode));
                                    else
                                    {
                                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                                        {
                                            foreach (var node in _cloudDriveViewModel.ChildNodes.Where(n => n.Type == MNodeType.TYPE_FOLDER))
                                            {
                                                node.SetFolderInfo();
                                            }
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
