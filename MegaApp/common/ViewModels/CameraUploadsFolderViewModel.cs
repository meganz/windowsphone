using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public class CameraUploadsFolderViewModel: FolderViewModel
    {
        private readonly ContainerType _containerType;

        public CameraUploadsFolderViewModel(MegaSDK megaSdk, AppInformation appInformation, ContainerType containerType) 
            : base(megaSdk, appInformation, containerType)
        {
            this._containerType = containerType;
        }        

        public override bool GoFolderUp()
        {
            if (this.FolderRootNode == null) return false;

            MNode parentNode = this.MegaSdk.getParentNode(this.FolderRootNode.OriginalMNode);

            if (parentNode == null || parentNode.getType() == MNodeType.TYPE_UNKNOWN || parentNode.getType() == MNodeType.TYPE_ROOT)
                return false;

            this.FolderRootNode = NodeService.CreateNew(this.MegaSdk, this.AppInformation, parentNode, _containerType, ChildNodes);

            LoadChildNodes();

            return true;
        }

        public override void BrowseToHome()
        {
            if (this.FolderRootNode == null) return;

            MNode homeNode = NodeService.FindCameraUploadNode(this.MegaSdk, this.MegaSdk.getRootNode());

            if (homeNode == null) return;

            this.FolderRootNode = NodeService.CreateNew(this.MegaSdk, this.AppInformation, homeNode, _containerType, ChildNodes);

            LoadChildNodes();
        }

        public async Task CreateRootNodeIfNotExists()
        {
            MNode cameraUploadsNode = NodeService.FindCameraUploadNode(this.MegaSdk, this.MegaSdk.getRootNode());

            if (cameraUploadsNode != null) return;

            var tcs = new TaskCompletionSource<bool>();

            var createFolderListener = new CreateCameraUploadsRequestListener();
            createFolderListener.RequestFinished += (sender, args) =>
            {
                tcs.TrySetResult(args.Succeeded);
            };

            MegaSdk.createFolder("Camera Uploads", this.MegaSdk.getRootNode(), createFolderListener);

            await tcs.Task;
        }

        #region Internal Classes

        private class CreateCameraUploadsRequestListener : MRequestListenerInterface
        {
            public event EventHandler<SucceededEventArgs> RequestFinished;

            public void onRequestFinish(MegaSDK api, MRequest request, MError e)
            {
                if (RequestFinished == null) return;

                RequestFinished(this, new SucceededEventArgs(e.getErrorCode() == MErrorType.API_OK));
            }

            public void onRequestStart(MegaSDK api, MRequest request)
            {
                // Do nothing
            }

            public void onRequestUpdate(MegaSDK api, MRequest request)
            {
                // Do nothing
            }

            public void onRequestTemporaryError(MegaSDK api, MRequest request, MError e)
            {
                // Do nothing
            }
        }

        private class SucceededEventArgs : EventArgs
        {
            public SucceededEventArgs(bool succeeded)
            {
                this.Succeeded = succeeded;
            }

            public bool Succeeded { get; private set; }
        }

        #endregion
    }
}
