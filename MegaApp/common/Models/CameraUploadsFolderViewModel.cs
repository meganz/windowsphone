using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Services;

namespace MegaApp.Models
{
    public class CameraUploadsFolderViewModel: FolderViewModel
    {
        public CameraUploadsFolderViewModel(MegaSDK megaSdk, AppInformation appInformation, ContainerType containerType) 
            : base(megaSdk, appInformation, containerType)
        {
           
        }        

        public override bool GoFolderUp()
        {
            if (this.FolderRootNode == null) return false;

            MNode parentNode = this.MegaSdk.getParentNode(this.FolderRootNode.OriginalMNode);

            if (parentNode == null || parentNode.getType() == MNodeType.TYPE_UNKNOWN || parentNode.getType() == MNodeType.TYPE_ROOT)
                return false;

            this.FolderRootNode = NodeService.CreateNew(this.MegaSdk, this.AppInformation, parentNode, ChildNodes);

            LoadChildNodes();

            return true;
        }

        public override void BrowseToHome()
        {
            if (this.FolderRootNode == null) return;

            MNode homeNode = NodeService.FindCameraUploadNode(this.MegaSdk, this.MegaSdk.getRootNode());

            if (homeNode == null) return;

            this.FolderRootNode = NodeService.CreateNew(this.MegaSdk, this.AppInformation, homeNode, ChildNodes);

            LoadChildNodes();
        }
    }
}
