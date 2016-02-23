using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Storage;
using mega;
using MegaApp.Enums;
using MegaApp.Database;
using MegaApp.MegaApi;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;
using Microsoft.Phone.Shell;

namespace MegaApp.Models
{
    public class NodeDetailsViewModel : BaseSdkViewModel
    {
        private readonly NodeDetailsPage _nodeDetailsPage;        

        public NodeDetailsViewModel(NodeDetailsPage nodeDetailsPage, NodeViewModel node)
            : base(App.MegaSdk)
        {
            this._nodeDetailsPage = nodeDetailsPage;
            this._node = node;
        }

        public void Initialize(GlobalDriveListener globalDriveListener)
        {
            // Add folders to global drive listener to receive notifications
            globalDriveListener.Nodes.Add(this);
        }

        public void Deinitialize(GlobalDriveListener globalDriveListener)
        {
            // Add folders to global drive listener to receive notifications
            globalDriveListener.Nodes.Remove(this);
        }

        public void ChangeMenu(IList iconButtons, IList menuItems)
        {
            if (this._node.IsFolder)
            {
                if (this._node.IsExported)
                {
                    this.TranslateAppBarItems(
                        iconButtons.Cast<ApplicationBarIconButton>().ToList(),
                        menuItems.Cast<ApplicationBarMenuItem>().ToList(),
                        new[] { UiResources.Download, UiResources.ManageLink, UiResources.Remove },
                        new[] { UiResources.Rename, UiResources.CreateShortCut, UiResources.UI_RemoveLink });
                }
                else
                {
                    this.TranslateAppBarItems(
                        iconButtons.Cast<ApplicationBarIconButton>().ToList(),
                        menuItems.Cast<ApplicationBarMenuItem>().ToList(),
                        new[] { UiResources.Download, UiResources.UI_GetLink, UiResources.Remove },
                        new[] { UiResources.Rename, UiResources.CreateShortCut });
                }
            }
            else //Node is a File
            {
                if (this._node.IsExported)
                {
                    this.TranslateAppBarItems(
                        iconButtons.Cast<ApplicationBarIconButton>().ToList(),
                        menuItems.Cast<ApplicationBarMenuItem>().ToList(),
                        new[] { UiResources.Download, UiResources.ManageLink, UiResources.Remove },
                        new[] { UiResources.Rename, UiResources.UI_RemoveLink });
                }
                else
                {
                    this.TranslateAppBarItems(
                        iconButtons.Cast<ApplicationBarIconButton>().ToList(),
                        menuItems.Cast<ApplicationBarMenuItem>().ToList(),
                        new[] { UiResources.Download, UiResources.UI_GetLink, UiResources.Remove },
                        new[] { UiResources.Rename });
                }            
            }
        }

        public ulong getNodeHandle()
        {
            return _node.Handle;
        }

        public string getNodeBase64Handle()
        {
            return _node.Base64Handle;
        }

        public void updateNode(MNode megaNode)
        {
            _node.Update(megaNode, this._node.ParentContainerType);
            _nodeDetailsPage.SetApplicationBar();
        }

        public void Download()
        {
            _node.Download(App.MegaTransfers);
        }

        public async Task<NodeActionResult> Remove()
        {
            return await _node.RemoveAsync(false);
        }

        public void GetLink()
        {
            if (!_node.IsUserOnline()) return;

            _node.GetLink();
        }

        public void RemoveLink()
        {
            _node.RemoveLink();
        }

        public void Rename()
        {
            _node.Rename();
        }

        public void CreateShortcut()
        {
            var _folderNode = _node as FolderNodeViewModel;
            _folderNode.CreateShortCut();
        }

        public async Task SaveForOffline(bool newStatus)
        {
            if (newStatus)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    ProgressService.SetProgressIndicator(true, ProgressMessages.SaveForOffline));

                _node.IsSelectedForOffline = true;                
                await _node.SaveForOffline(App.MegaTransfers);
            }
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    ProgressService.SetProgressIndicator(true, ProgressMessages.RemoveFromOffline));

                _node.IsSelectedForOffline = false;                
                await _node.RemoveForOffline();
            }

            Deployment.Current.Dispatcher.BeginInvoke(() => ProgressService.SetProgressIndicator(false));
        }

        #region Properties

        private NodeViewModel _node;
        public NodeViewModel Node
        {
            get { return _node; }
            set { SetField(ref _node, value); }
        }

        #endregion
    }    
}
