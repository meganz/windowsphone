using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using mega;
using MegaApp.Classes;
using MegaApp.Interfaces;
using MegaApp.Models;

namespace MegaApp.Services
{
    static class NodeService
    {
        public static IEnumerable<string> GetFiles(IEnumerable<IMegaNode> nodes, string directory)
        {
            return nodes.Select(node => Path.Combine(directory, node.OriginalMNode.getBase64Handle())).ToList();
        }

        public static NodeViewModel CreateNew(MegaSDK megaSdk, AppInformation appInformation, MNode megaNode, 
            ObservableCollection<IMegaNode> parentCollection = null, ObservableCollection<IMegaNode> childCollection = null)
        {
            if (megaNode == null) return null;

            try
            {
                switch (megaNode.getType())
                {
                    case MNodeType.TYPE_UNKNOWN:
                        break;
                    case MNodeType.TYPE_FILE:
                        {
                            if (megaNode.hasThumbnail() || megaNode.hasPreview() || ImageService.IsImage(megaNode.getName()))
                                return new ImageNodeViewModel(megaSdk, appInformation, megaNode, parentCollection, childCollection);

                            return new FileNodeViewModel(megaSdk, appInformation, megaNode, parentCollection, childCollection);
                        }
                    case MNodeType.TYPE_FOLDER:
                    case MNodeType.TYPE_ROOT:
                    case MNodeType.TYPE_RUBBISH:
                    {
                        return new FolderNodeViewModel(megaSdk, appInformation, megaNode, parentCollection, childCollection);
                    }
                    case MNodeType.TYPE_INCOMING:
                        break;
                }
            }
            catch (Exception)
            {
                return null;
            }

            return null;
        }

        public static List<NodeViewModel> GetRecursiveNodes(MegaSDK megaSdk, AppInformation appInformation, FolderNodeViewModel folderNode)
        {
            var result = new List<NodeViewModel>();

            var childNodeList = megaSdk.getChildren(folderNode.OriginalMNode);
        
            // Retrieve the size of the list to save time in the loops
            int listSize = childNodeList.size();

            for (int i = 0; i < listSize; i++)
            {
              // To avoid pass null values to CreateNew
                if (childNodeList.get(i) == null) continue;

                var node = CreateNew(megaSdk, appInformation, childNodeList.get(i));

                var folder = node as FolderNodeViewModel;
                if (folder != null)
                    result.AddRange(GetRecursiveNodes(megaSdk, appInformation, folder));
                else
                    result.Add(node);
            }

            return result;
        }

        public static MNodeList GetChildren(MegaSDK megaSdk, IMegaNode rootNode)
        {
            return megaSdk.getChildren(rootNode.OriginalMNode, UiService.GetSortOrder(rootNode.Handle, rootNode.Name));
        }

        public static MNode FindCameraUploadNode(MegaSDK megaSdk, MNode rootNode)
        {
            var childs = megaSdk.getChildren(rootNode);

            for (int x = 0; x < childs.size(); x++)
            {
                var node = childs.get(x);
                if (node.getType() != MNodeType.TYPE_FOLDER) continue;
                if (!node.getName().ToLower().Equals("camera uploads")) continue;
                return node;
            }

            return null;
        }
    }
}
