using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using mega;
using MegaApp.Models;
using MegaApp.Resources;

namespace MegaApp.Services
{
    static class NodeService
    {
        public static IEnumerable<string> GetFiles(IEnumerable<NodeViewModel> nodes, string directory)
        {
            return nodes.Select(node => Path.Combine(directory, node.GetMegaNode().getBase64Handle())).ToList();
        }

        public static NodeViewModel CreateNew(MegaSDK megaSdk, MNode megaNode, object parentCollection = null,
            object childCollection = null)
        {
            if (megaNode == null) return null;

            switch (megaNode.getType())
            {
                case MNodeType.TYPE_UNKNOWN:
                    break;
                case MNodeType.TYPE_FILE:
                {
                    if(megaNode.hasThumbnail() || megaNode.hasPreview() || ImageService.IsImage(megaNode.getName()))
                        return new ImageNodeViewModel(megaSdk, megaNode, parentCollection, childCollection);
                    
                    return new FileNodeViewModel(megaSdk, megaNode, parentCollection, childCollection);
                }
                case MNodeType.TYPE_FOLDER:
                case MNodeType.TYPE_ROOT:
                case MNodeType.TYPE_RUBBISH:
                    return new FolderNodeViewModel(megaSdk, megaNode, parentCollection, childCollection);
                case MNodeType.TYPE_INCOMING:
                    break;
                case MNodeType.TYPE_MAIL:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            throw new ArgumentOutOfRangeException();
        }

        public static List<NodeViewModel> GetRecursiveNodes(MegaSDK megaSdk, FolderNodeViewModel folderNode)
        {
            var result = new List<NodeViewModel>();

            var childNodeList = megaSdk.getChildren(folderNode.GetMegaNode());
        
            // Retrieve the size of the list to save time in the loops
            int listSize = childNodeList.size();

            for (int i = 0; i < listSize; i++)
            {
              // To avoid pass null values to CreateNew
                if (childNodeList.get(i) == null) continue;

                var node = CreateNew(megaSdk, childNodeList.get(i));

                var folder = node as FolderNodeViewModel;
                if (folder != null)
                    result.AddRange(GetRecursiveNodes(megaSdk, folder));
                else
                    result.Add(node);
            }

            return result;
        }
    }
}
