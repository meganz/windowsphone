using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegaApp.Models
{
    class DownloadImageViewModel : BaseViewModel
    {
        public DownloadImageViewModel(NodeViewModel selectedNode)
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
                _selectedNode.SetImage();
                OnPropertyChanged("SelectedNode");
            }
        }

        #endregion
    }
}
