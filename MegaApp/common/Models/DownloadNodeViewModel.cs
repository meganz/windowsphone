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
            private set
            {
                _selectedNode = value;
                var image = _selectedNode as ImageNodeViewModel;
                if (image != null)
                    image.SetImage();
                else
                {
                    var file = _selectedNode as FileNodeViewModel;
                    if (file != null)
                        file.SetFile();
                }
                OnPropertyChanged("SelectedNode");
            }
        }

        #endregion
    }
}
