using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mega;
using MegaApp.MegaApi;

namespace MegaApp.Models
{
    class CloudDriveViewModel : BaseViewModel
    {
        private readonly MegaSDK _megaSdk;

        public CloudDriveViewModel(MegaSDK megaSdk)
        {
            this._megaSdk = megaSdk;
            this.ChildNodes = new ObservableCollection<NodeViewModel>();
        }

        #region Methods

        public void GetNodes()
        {
            var fetchNodesRequestListener = new FetchNodesRequestListener(this);
            this._megaSdk.fetchNodes(fetchNodesRequestListener);
        }

        #endregion

        #region Properties

        public ObservableCollection<NodeViewModel> ChildNodes { get; set; }

        private NodeViewModel _currentCloudDriveRootNode;
        public NodeViewModel CurrentCloudDriveRootNode
        {
            get { return _currentCloudDriveRootNode; }
            set
            {
                _currentCloudDriveRootNode = value;
                OnPropertyChanged("CurrentCloudDriveRootNode");
            }

        }

        #endregion
      
    }
}
