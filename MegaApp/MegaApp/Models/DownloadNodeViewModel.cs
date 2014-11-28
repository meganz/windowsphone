using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegaApp.Models
{
    class DownloadNodeViewModel : BaseViewModel
    {
        public DownloadNodeViewModel(NodeViewModel selectedNode)
        {
            SelectedNode = selectedNode;
        }

        #region Properties

        private NodeViewModel _selectedNode;

        public NodeViewModel SelectedNode
        {
            get { return _selectedNode; }
            set
            {
                _selectedNode = value;
                var node = _selectedNode as ImageNodeViewModel;
                if (node != null)
                    node.SetImage();
                else
                    ((FileNodeViewModel)_selectedNode).SetFile();
                OnPropertyChanged("SelectedNode");
            }
        }

        #endregion
    }
}
