using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using mega;
using MegaApp.Enums;
using MegaApp.Database;
using MegaApp.MegaApi;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.Models
{
    public class NodeDetailsViewModel : BaseViewModel
    {
        private readonly NodeDetailsPage _nodeDetailsPage;        

        public NodeDetailsViewModel(NodeDetailsPage nodeDetailsPage, NodeViewModel node)
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

        public ulong getNodeHandle()
        {
            return _node.Handle;
        }

        public void updateNode(MNode megaNode)
        {
            _node.Update(megaNode);
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

        public void SaveForOffline(bool newStatus)
        {
            MNode parentNode = App.MegaSdk.getParentNode(_node.OriginalMNode);

            String parentNodePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, 
                AppResources.DownloadsDirectory.Replace("\\", ""),
                (App.MegaSdk.getNodePath(parentNode)).Remove(0, 1).Replace("/", "\\"));

            String sfoRootPath = Path.Combine(ApplicationData.Current.LocalFolder.Path,
                    AppResources.DownloadsDirectory.Replace("\\", ""));

            if (newStatus)
            {
                _node.IsSelectedForOfflineText = Resources.UiResources.On;
                _node.SaveForOffline(App.MegaTransfers, parentNodePath);
                
                while (String.Compare(parentNodePath, sfoRootPath) != 0)
                {
                    var folderPathToAdd = parentNodePath;
                    parentNodePath = ((new DirectoryInfo(parentNodePath)).Parent).FullName;

                    if (!SavedForOffline.ExistsNodeByLocalPath(folderPathToAdd))
                        SavedForOffline.Insert(parentNode);

                    parentNode = App.MegaSdk.getParentNode(parentNode);
                }
            }
            else
            {
                _node.IsSelectedForOfflineText = Resources.UiResources.Off;
                _node.RemoveForOffline(parentNodePath);

                while (String.Compare(parentNodePath, sfoRootPath) != 0)
                {
                    var folderPathToRemove = parentNodePath;
                    parentNodePath = ((new DirectoryInfo(parentNodePath)).Parent).FullName;

                    if (FolderService.IsEmptyFolder(folderPathToRemove))
                    {
                        FolderService.DeleteFolder(folderPathToRemove);
                        SavedForOffline.DeleteNodeByLocalPath(folderPathToRemove);
                    }
                }
            }
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
