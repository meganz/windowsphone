using SQLite;
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using mega;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.ViewModels;

namespace MegaApp.Database
{
    public class SavedForOffline : DataBaseHelper<SavedForOffline>
    {
        #region Properties

        private const String DB_TABLE_NAME = "SavedForOffline";

        private const String FIELD_LOCAL_PATH = "LocalPath";
        private const String FIELD_FINGERPRINT = "Fingerprint";
        private const String FIELD_BASE_64_HANDLE = "Base64Handle";
        private const String FIELD_PARENT_BASE_64_HANDLE = "ParentBase64Handle";
        private const String FIELD_IS_SELECTED_FOR_OFFLINE = "IsSelectedForOffline";

        // The LocalPath property is marked as the Primary Key
        [SQLite.PrimaryKey]
        public String LocalPath { get; set; }        
        public String Fingerprint { get; set; }
        public String Base64Handle { get; set; }        
        public String ParentBase64Handle { get; set; }
        public bool IsSelectedForOffline { get; set; }

        #endregion

        #region DatabaseHelper

        /// <summary>
        /// Indicate if exists a node with the specified local path in the database.
        /// </summary>
        /// <param name="localPath">Local path of the node to search.</param>
        /// <returns>TRUE if exists or FALSE in other case.</returns>
        public static bool ExistsNodeByLocalPath(String localPath)
        {
            return ExistsItem(DB_TABLE_NAME, FIELD_LOCAL_PATH, localPath);
        }

        /// <summary>
        /// Retrieve the first node found with the specified local path in the database.
        /// </summary>
        /// <param name="localPath">Local path of the node to search.</param>
        /// <returns>The first node with the specified local path.</returns>
        public static SavedForOffline SelectNodeByLocalPath(String localPath)
        {
            return SelectItem(DB_TABLE_NAME, FIELD_LOCAL_PATH, localPath);
        }

        /// <summary>
        /// Retrieve the list of nodes found with the specified local path in the database.
        /// </summary>
        /// <param name="localPath">Local path of the nodes to search.</param>
        /// <returns>The list of nodes with the specified local path.</returns>
        public static ObservableCollection<SavedForOffline> SelectNodesByLocalPath(String localPath)
        {
            return SelectItems(DB_TABLE_NAME, FIELD_LOCAL_PATH, localPath);
        }

        /// <summary>
        /// Indicate if exists a node with the specified fingerprint in the database.
        /// </summary>
        /// <param name="fingerprint">Fingerprint of the node to search.</param>
        /// <returns>TRUE if exists or FALSE in other case.</returns>
        public static bool ExistNodeByFingerprint(String fingerprint)
        {
            return ExistsItem(DB_TABLE_NAME, FIELD_FINGERPRINT, fingerprint);
        }

        /// <summary>
        /// Retrieve the first node found with the specified fingerprint in the database.
        /// </summary>
        /// <param name="fingerprint">Fingerprint of the node to search.</param>
        /// <returns>The first node with the specified fingerprint.</returns>
        public static SavedForOffline SelectNodeByFingerprint(String fingerprint)
        {
            return SelectItem(DB_TABLE_NAME, FIELD_FINGERPRINT, fingerprint);
        }

        /// <summary>
        /// Retrieve the list of nodes found with the specified fingerprint in the database.
        /// </summary>
        /// <param name="fingerprint">Fingerprint of the nodes to search.</param>
        /// <returns>The list of nodes with the specified fingerprint.</returns>
        public static ObservableCollection<SavedForOffline> SelectNodesByFingerprint(String fingerprint)
        {
            return SelectItems(DB_TABLE_NAME, FIELD_FINGERPRINT, fingerprint);
        }

        /// <summary>
        /// Indicate if exists a node with the specified handle in the database.
        /// </summary>
        /// <param name="base64Handle">Handle of the node to search.</param>
        /// <returns>TRUE if exists or FALSE in other case.</returns>
        public static bool ExistNodeByBase64Handle(String base64Handle)
        {
            return ExistsItem(DB_TABLE_NAME, FIELD_BASE_64_HANDLE, base64Handle);
        }

        /// <summary>
        /// Retrieve the first node found with the specified handle in the database.
        /// </summary>
        /// <param name="base64Handle">Handle of the node to search.</param>
        /// <returns>The first node with the specified handle.</returns>
        public static SavedForOffline SelectNodeByBase64Handle(String base64Handle)
        {
            return SelectItem(DB_TABLE_NAME, FIELD_BASE_64_HANDLE, base64Handle);
        }

        /// <summary>
        /// Retrieve the list of nodes found with the specified handle in the database.
        /// </summary>
        /// <param name="base64Handle">Handle of the nodes to search.</param>
        /// <returns>The list of nodes with the specified handle.</returns>
        public static ObservableCollection<SavedForOffline> SelectNodesByBase64Handle(String base64Handle)
        {
            return SelectItems(DB_TABLE_NAME, FIELD_BASE_64_HANDLE, base64Handle);
        }

        /// <summary>
        /// Indicate if exists a node with the specified parent handle in the database.
        /// </summary>
        /// <param name="parentBase64Handle">Parent handle of the node to search.</param>
        /// <returns>TRUE if exists or FALSE in other case.</returns>
        public static bool ExistNodeByParentBase64Handle(String parentBase64Handle)
        {
            return ExistsItem(DB_TABLE_NAME, FIELD_PARENT_BASE_64_HANDLE, parentBase64Handle);
        }

        /// <summary>
        /// Retrieve the first node found with the specified parent handle in the database.
        /// </summary>
        /// <param name="parentBase64Handle">Parent handle of the node to search.</param>
        /// <returns>The first node with the specified parent handle.</returns>
        public static SavedForOffline SelectNodeByParentBase64Handle(String parentBase64Handle)
        {
            return SelectItem(DB_TABLE_NAME, FIELD_PARENT_BASE_64_HANDLE, parentBase64Handle);
        }

        /// <summary>
        /// Retrieve the list of nodes found with the specified parent handle in the database.
        /// </summary>
        /// <param name="parentBase64Handle">Parent handle of the node to search.</param>
        /// <returns>The list of nodes with the specified parent handle.</returns>
        public static ObservableCollection<SavedForOffline> SelectNodesByParentBase64Handle(String parentBase64Handle)
        {
            return SelectItems(DB_TABLE_NAME, FIELD_PARENT_BASE_64_HANDLE, parentBase64Handle);
        }

        /// <summary>
        /// Retrieve all nodes from the database table.
        /// </summary>
        /// <returns>List of all nodes.</returns>
        public static ObservableCollection<SavedForOffline> SelectAllNodes()
        {
            return SelectAllItems();
        }

        /// <summary>
        /// Update an existing node.
        /// </summary>
        /// <param name="node">Node to update.</param>
        public static void UpdateNode(SavedForOffline node)
        {
            UpdateItem(node);
        }

        /// <summary>
        /// Update existing node.
        /// </summary>
        /// <param name="megaNode">Node to update.</param>
        public static void UpdateNode(MNode megaNode, bool isSelectedForOffline = false)
        {
            var nodeOfflineLocalPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, AppResources.DownloadsDirectory,
                    SdkService.MegaSdk.getNodePath(megaNode).Remove(0, 1).Replace("/", "\\"));

            var sfoNode = new SavedForOffline()
            {
                Fingerprint = SdkService.MegaSdk.getNodeFingerprint(megaNode),
                Base64Handle = megaNode.getBase64Handle(),
                LocalPath = nodeOfflineLocalPath,
                ParentBase64Handle = (SdkService.MegaSdk.getParentNode(megaNode)).getBase64Handle(),
                IsSelectedForOffline = isSelectedForOffline
            };

            UpdateNode(sfoNode);
        }

        /// <summary>
        /// Insert a node in the database.
        /// </summary>
        /// <param name="node">Node to insert.</param>
        public static void Insert(SavedForOffline newNode)
        {
            InsertItem(newNode);
        }

        /// <summary>
        /// Insert a node in the database.
        /// </summary>
        /// <param name="megaNode">Node to insert.</param>
        /// <param name="isSelectedForOffline">Indicate if is specifically selected for offline.</param>
        public static void Insert(MNode megaNode, bool isSelectedForOffline = false)
        {
            var nodeOfflineLocalPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, AppResources.DownloadsDirectory,
                    SdkService.MegaSdk.getNodePath(megaNode).Remove(0, 1).Replace("/", "\\"));

            var sfoNode = new SavedForOffline()
            {
                Fingerprint = SdkService.MegaSdk.getNodeFingerprint(megaNode),
                Base64Handle = megaNode.getBase64Handle(),
                LocalPath = nodeOfflineLocalPath,
                ParentBase64Handle = (SdkService.MegaSdk.getParentNode(megaNode)).getBase64Handle(),
                IsSelectedForOffline = isSelectedForOffline
            };

            Insert(sfoNode);            
        }

        /// <summary>
        /// Delete the first node found with the specified local path.
        /// </summary>
        /// <param name="localPath">Local path of the node to delete.</param>
        public static void DeleteNodeByLocalPath(String localPath)
        {
            DeleteItem(DB_TABLE_NAME, FIELD_LOCAL_PATH, localPath);
        }

        /// <summary>
        /// Delete specific node.
        /// </summary>
        /// <param name="node">Node to delete.</param>
        public static void DeleteNode(SavedForOffline node)
        {
            DeleteItem(node);
        }

        /// <summary>
        /// Delete all node list or delete table 
        /// </summary>
        /// <returns>TRUE if all went well or FALSE in other case</returns>
        public static bool DeleteAllNodes()
        {
            return DeleteAllItems();
        }

        #endregion
    }
}
