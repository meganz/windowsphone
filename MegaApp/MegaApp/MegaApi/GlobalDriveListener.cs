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

            for (int i = 0; i < nodes.size(); i++)
            {
                MNode megaNode = nodes.get(i);
                
                if (megaNode.isRemoved())
                {
                    NodeViewModel nodeToRemove = _cloudDriveViewModel.ChildNodes.FirstOrDefault(
                        n => n.Handle.Equals(megaNode.getHandle()));
                    if (nodeToRemove != null)
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(
                            () => _cloudDriveViewModel.ChildNodes.Remove(nodeToRemove));
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
                                Deployment.Current.Dispatcher.BeginInvoke(() =>
                                    _cloudDriveViewModel.ChildNodes.Add(new NodeViewModel(api, megaNode,
                                        _cloudDriveViewModel.ChildNodes)));
                            }
                        }
                    }
                }
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
