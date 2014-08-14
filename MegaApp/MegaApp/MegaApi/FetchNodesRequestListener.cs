using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using mega;
using MegaApp.Models;

namespace MegaApp.MegaApi
{
    class FetchNodesRequestListener: MRequestListenerInterface
    {
        private CloudDriveViewModel model;
        public FetchNodesRequestListener(CloudDriveViewModel model)
        {
            this.model = model;
        }

        #region MRequestListenerInterface

        public void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                for (int i = 0; i < api.getChildren(api.getRootNode()).size(); i++)
                {
                    model.ChildNodes.Add(new NodeViewModel(api.getChildren(api.getRootNode()).get(i)));
                }
            });

        }

        public void onRequestStart(MegaSDK api, MRequest request)
        {
            //throw new NotImplementedException();
        }

        public void onRequestTemporaryError(MegaSDK api, MRequest request, MError e)
        {
            //throw new NotImplementedException();
        }

        public void onRequestUpdate(MegaSDK api, MRequest request)
        {
           // throw new NotImplementedException();
        }

        #endregion
    }
}
