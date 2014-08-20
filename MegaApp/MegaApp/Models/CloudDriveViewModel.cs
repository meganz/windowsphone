using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mega;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Pages;
using MegaApp.Services;

namespace MegaApp.Models
{
    public class CloudDriveViewModel : BaseViewModel
    {
        private readonly MegaSDK _megaSdk;

        public CloudDriveViewModel(MegaSDK megaSdk)
        {
            this._megaSdk = megaSdk;
            this.CurrentRootNode = null;
            this.ChildNodes = new ObservableCollection<NodeViewModel>();
            this.BreadCrumbs = new ObservableCollection<NodeViewModel>();
        }

        #region Commands

        #endregion

        #region Methods

        public void FetchNodes()
        {
            this.ChildNodes.Clear();

            var fetchNodesRequestListener = new FetchNodesRequestListener(this);
            this._megaSdk.fetchNodes(fetchNodesRequestListener);
        }

        public void GetNodes()
        {
            this.ChildNodes.Clear();

            MNodeList nodeList = this._megaSdk.getChildren(this.CurrentRootNode.GetBaseNode());

            for (int i = 0; i < nodeList.size(); i++)
            {
                MNode baseNode = nodeList.get(i);
                ChildNodes.Add(new NodeViewModel(this._megaSdk, baseNode));
            }
        }

        public void SelectFolder(NodeViewModel selectedNode)
        {
            this.CurrentRootNode = selectedNode;
            this.BreadCrumbs.Add(selectedNode);
            NavigateService.NavigateTo(typeof(MainPage), NavigationParameter.Browsing);
        }

        #endregion

        #region Properties

        public ObservableCollection<NodeViewModel> ChildNodes { get; set; }
        public ObservableCollection<NodeViewModel> BreadCrumbs { get; set; }

        public NodeViewModel CurrentRootNode { get; set; }

        //private NodeViewModel _currentCloudDriveRootNode;
        //public NodeViewModel CurrentCloudDriveRootNode
        //{
        //    get { return _currentCloudDriveRootNode; }
        //    set
        //    {
        //        _currentCloudDriveRootNode = value;
        //        OnPropertyChanged("CurrentCloudDriveRootNode");
        //    }

        //}

        #endregion
      
    }
}
