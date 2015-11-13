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
using MegaApp.Models;
using MegaApp.Resources;

namespace MegaApp.Database
{
    public class SavedForOffline
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

        // Indicate if the node exists by LocalPaht in the database table.
        public static bool ExistsNodeByLocalPath(String localPath)
        {
            return DataBaseHelper<SavedForOffline>.ExistsNode(DB_TABLE_NAME, FIELD_LOCAL_PATH, localPath);
        }

        // Retrieve the first node found by LocalPath in the database table.
        public static SavedForOffline ReadNodeByLocalPath(String localPath)
        {
            return DataBaseHelper<SavedForOffline>.ReadNode(DB_TABLE_NAME, FIELD_LOCAL_PATH, localPath);
        }

        // Retrieve the node list found by LocalPath in the database table.
        public static ObservableCollection<SavedForOffline> ReadNodesByLocalPath(String localPath)
        {
            return DataBaseHelper<SavedForOffline>.ReadNodes(DB_TABLE_NAME, FIELD_LOCAL_PATH, localPath);
        }

        // Indicate if the node exists by Fingerprint in the database table.
        public static bool ExistNodeByFingerprint(String fingerprint)
        {
            return DataBaseHelper<SavedForOffline>.ExistsNode(DB_TABLE_NAME, FIELD_FINGERPRINT, fingerprint);
        }

        // Retrieve the first node found by Fingerprint in the database table.
        public static SavedForOffline ReadNodeByFingerprint(String fingerprint)
        {
            return DataBaseHelper<SavedForOffline>.ReadNode(DB_TABLE_NAME, FIELD_FINGERPRINT, fingerprint);
        }

        // Retrieve the node list found by Fingerprint in the database table.
        public static ObservableCollection<SavedForOffline> ReadNodesByFingerprint(String fingerprint)
        {
            return DataBaseHelper<SavedForOffline>.ReadNodes(DB_TABLE_NAME, FIELD_FINGERPRINT, fingerprint);
        }

        // Indicate if the node exists by Base64Handle in the database table.
        public static bool ExistNodeByBase64Handle(String base64Handle)
        {
            return DataBaseHelper<SavedForOffline>.ExistsNode(DB_TABLE_NAME, FIELD_BASE_64_HANDLE, base64Handle);
        }

        // Retrieve the first node found by Base64Handle in the database table.
        public static SavedForOffline ReadNodeByBase64Handle(String base64Handle)
        {
            return DataBaseHelper<SavedForOffline>.ReadNode(DB_TABLE_NAME, FIELD_BASE_64_HANDLE, base64Handle);
        }

        // Retrieve the node list found by Base64Handle in the database table.
        public static ObservableCollection<SavedForOffline> ReadNodesByBase64Handle(String base64Handle)
        {
            return DataBaseHelper<SavedForOffline>.ReadNodes(DB_TABLE_NAME, FIELD_BASE_64_HANDLE, base64Handle);
        }

        // Indicate if the node exists by ParentBase64Handle in the database table.
        public static bool ExistNodeByParentBase64Handle(String parentBase64Handle)
        {
            return DataBaseHelper<SavedForOffline>.ExistsNode(DB_TABLE_NAME, FIELD_PARENT_BASE_64_HANDLE, parentBase64Handle);
        }

        // Retrieve the first node found by ParentBase64Handle in the database table.
        public static SavedForOffline ReadNodeByParentBase64Handle(String parentBase64Handle)
        {
            return DataBaseHelper<SavedForOffline>.ReadNode(DB_TABLE_NAME, FIELD_PARENT_BASE_64_HANDLE, parentBase64Handle);
        }

        // Retrieve the node list found by ParentBase64Handle in the database table.
        public static ObservableCollection<SavedForOffline> ReadNodesByParentBase64Handle(String parentBase64Handle)
        {
            return DataBaseHelper<SavedForOffline>.ReadNodes(DB_TABLE_NAME, FIELD_PARENT_BASE_64_HANDLE, parentBase64Handle);
        }

        // Indicate if the node exists by IsSelectedForOffline in the database table.
        public static bool ExistNodeByIsSelectedForOffline(bool isSelectedForOffline)
        {
            return DataBaseHelper<SavedForOffline>.ExistsNode(DB_TABLE_NAME, FIELD_IS_SELECTED_FOR_OFFLINE, isSelectedForOffline.ToString());
        }

        // Retrieve the first node found by IsSelectedForOffline in the database table.
        public static SavedForOffline ReadNodeByIsSelectedForOffline(bool isSelectedForOffline)
        {
            return DataBaseHelper<SavedForOffline>.ReadNode(DB_TABLE_NAME, FIELD_IS_SELECTED_FOR_OFFLINE, isSelectedForOffline.ToString());
        }

        // Retrieve the node list found by IsSelectedForOffline in the database table.
        public static ObservableCollection<SavedForOffline> ReadNodesByIsSelectedForOffline(bool isSelectedForOffline)
        {
            return DataBaseHelper<SavedForOffline>.ReadNodes(DB_TABLE_NAME, FIELD_IS_SELECTED_FOR_OFFLINE, isSelectedForOffline.ToString());
        }

        // Retrieve the all node list from the database table.
        public static ObservableCollection<SavedForOffline> ReadAllNodes()
        {
            return DataBaseHelper<SavedForOffline>.ReadAllNodes();
        }

        // Update existing node 
        public static void UpdateNode(SavedForOffline node)
        {
            DataBaseHelper<SavedForOffline>.UpdateNode(node);
        }

        // Update existing node 
        public static void UpdateNode(MNode megaNode, bool isSelectedForOffline = false)
        {
            var nodeOfflineLocalPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, AppResources.DownloadsDirectory,
                    App.MegaSdk.getNodePath(megaNode).Remove(0, 1).Replace("/", "\\"));

            var sfoNode = new SavedForOffline()
            {
                Fingerprint = App.MegaSdk.getNodeFingerprint(megaNode),
                Base64Handle = megaNode.getBase64Handle(),
                LocalPath = nodeOfflineLocalPath,
                ParentBase64Handle = (App.MegaSdk.getParentNode(megaNode)).getBase64Handle(),
                IsSelectedForOffline = isSelectedForOffline
            };

            DataBaseHelper<SavedForOffline>.UpdateNode(sfoNode);
        }

        // Insert the new node in the database. 
        public static void Insert(SavedForOffline newNode)
        {
            DataBaseHelper<SavedForOffline>.Insert(newNode);
        }

        // Insert the new node in the SavedForOffline table. 
        public static void Insert(MNode megaNode, bool isSelectedForOffline = false)
        {
            var nodeOfflineLocalPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, AppResources.DownloadsDirectory,
                    App.MegaSdk.getNodePath(megaNode).Remove(0, 1).Replace("/", "\\"));

            var sfoNode = new SavedForOffline()
            {
                Fingerprint = App.MegaSdk.getNodeFingerprint(megaNode),
                Base64Handle = megaNode.getBase64Handle(),
                LocalPath = nodeOfflineLocalPath,
                ParentBase64Handle = (App.MegaSdk.getParentNode(megaNode)).getBase64Handle(),
                IsSelectedForOffline = isSelectedForOffline
            };

            DataBaseHelper<SavedForOffline>.Insert(sfoNode);
        }

        // Delete the first node found with the specified field value
        public static void DeleteNodeByLocalPath(String localPath)
        {
            DataBaseHelper<SavedForOffline>.DeleteNode(DB_TABLE_NAME, FIELD_LOCAL_PATH, localPath);
        }

        // Delete specific node
        public static void DeleteNode(SavedForOffline node)
        {
            DataBaseHelper<SavedForOffline>.DeleteNode(node);
        }

        // Delete all node list or delete table 
        public static void DeleteAllNodes()
        {
            DataBaseHelper<SavedForOffline>.DeleteAllNodes();
        }

        #endregion
    }
}
