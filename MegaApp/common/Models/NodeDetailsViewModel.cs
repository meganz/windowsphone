using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mega;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Pages;

namespace MegaApp.Models
{
    public class NodeDetailsViewModel : BaseViewModel
    {
        private readonly NodeDetailsPage _nodeDetailsPage;
        private DownloadNodeViewModel _downloadViewModel;

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

        public void SaveForOffline(bool newStatus)
        {
            if (newStatus)
            {
                _node.Transfer.IsSaveForOfflineTransfer = true;
                _downloadViewModel = new DownloadNodeViewModel(_node);
            }
            else
            {
                if (this._node.IsImage)
                {
                    var node = _node as ImageNodeViewModel;
                    if (File.Exists(node.LocalImagePath))
                        File.Delete(node.LocalImagePath);
                }
                else
                {
                    var node = _node as FileNodeViewModel;
                    if (File.Exists(node.LocalFilePath))
                        File.Delete(node.LocalFilePath);
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
