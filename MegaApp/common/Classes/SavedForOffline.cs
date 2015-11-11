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

namespace MegaApp.Classes
{
    public class SavedForOffline
    {
        #region Properties

        // The LocalPath property is marked as the Primary Key
        [SQLite.PrimaryKey]
        public String LocalPath { get; set; }        
        public String Fingerprint { get; set; }
        public String Base64Handle { get; set; }        
        public String ParentBase64Handle { get; set; }
        public bool IsSelectedForOffline { get; set; }

        #endregion

        #region DatabaseHelper
                
        // Retrieve the specific node from the database using the fingerprint. 
        public static SavedForOffline ReadNodeByFingerprint(String fingerprint)
        {
            using (var dbConn = new SQLiteConnection(App.DB_PATH))
            {
                var existingNode = dbConn.Query<SavedForOffline>("select * from SavedForOffline where Fingerprint = '" + fingerprint + "'").FirstOrDefault();
                return existingNode;
            }
        }

        public static bool ExistsByFingerprint(String fingerprint)
        {
            return (ReadNodeByFingerprint(fingerprint) != null) ? true : false;
        }

        // Retrieve the specific node from the database using the file path. 
        public static SavedForOffline ReadNodeByLocalPath(String localPath)
        {
            using (var dbConn = new SQLiteConnection(App.DB_PATH))
            {
                var existingNode = dbConn.Query<SavedForOffline>("select * from SavedForOffline where LocalPath = '" + localPath + "'").FirstOrDefault();
                return existingNode;
            }
        }

        public static bool ExistsByLocalPath(String localPath)
        {
            return (ReadNodeByLocalPath(localPath) != null) ? true : false;
        }

        // Retrieve the specific node from the database using the ParentBase64Handle. 
        public static SavedForOffline ReadNodeByParentBase64Handle(String parentBase64Handle)
        {
            using (var dbConn = new SQLiteConnection(App.DB_PATH))
            {
                var existingNode = dbConn.Query<SavedForOffline>("select * from SavedForOffline where ParentBase64Handle = '" + parentBase64Handle + "'").FirstOrDefault();
                return existingNode;
            }
        }

        public static bool ExistsByParentBase64Handle(String parentBase64Handle)
        {
            return (ReadNodeByParentBase64Handle(parentBase64Handle) != null) ? true : false;
        }

        // Retrieve the all node list from the database. 
        public static ObservableCollection<SavedForOffline> ReadNodes()
        {
            using (var dbConn = new SQLiteConnection(App.DB_PATH))
            {
                List<SavedForOffline> _nodeList = dbConn.Table<SavedForOffline>().ToList<SavedForOffline>();
                ObservableCollection<SavedForOffline> nodeList = new ObservableCollection<SavedForOffline>(_nodeList);
                return nodeList;
            }
        }                

        // Update existing node 
        public static void UpdateNode(SavedForOffline node)
        {
            using (var dbConn = new SQLiteConnection(App.DB_PATH))
            {
                var existingNode = dbConn.Query<SavedForOffline>("select * from SavedForOffline where LocalPath = '" + node.LocalPath + "'").FirstOrDefault();
                if (existingNode != null)
                {
                    existingNode.Base64Handle = node.Base64Handle;
                    existingNode.LocalPath = node.LocalPath;
                    existingNode.ParentBase64Handle = node.ParentBase64Handle;
                    existingNode.IsSelectedForOffline = node.IsSelectedForOffline;
                    
                    dbConn.RunInTransaction(() =>
                    {
                        dbConn.Update(existingNode);
                    });
                }
            }
        }

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

            UpdateNode(sfoNode);
        }

        // Insert the new node in the SavedForOffline table. 
        public static void Insert(SavedForOffline newNode)
        {
            using (var dbConn = new SQLiteConnection(App.DB_PATH))
            {
                dbConn.RunInTransaction(() =>
                {
                    dbConn.Insert(newNode);
                });
            }
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

            Insert(sfoNode);
        }

        // Delete specific node using the file fingerprint
        public static void DeleteNodeByFingerprint(String fingerprint)
        {
            using (var dbConn = new SQLiteConnection(App.DB_PATH))
            {
                var existingNode = dbConn.Query<SavedForOffline>("select * from SavedForOffline where Fingerprint = '" + fingerprint + "'").FirstOrDefault();
                if (existingNode != null)
                {
                    dbConn.RunInTransaction(() =>
                    {
                        dbConn.Delete(existingNode);
                    });
                }
            }
        }

        // Delete specific node using the file path
        public static void DeleteNodeByLocalPath(String localPath)
        {
            using (var dbConn = new SQLiteConnection(App.DB_PATH))
            {
                var existingNode = dbConn.Query<SavedForOffline>("select * from SavedForOffline where LocalPath = '" + localPath + "'").FirstOrDefault();
                if (existingNode != null)
                {
                    dbConn.RunInTransaction(() =>
                    {
                        dbConn.Delete(existingNode);
                    });
                }
            }
        }

        // Delete all node list or delete SavedForOffline table 
        public static void DeleteAllNodes()
        {
            using (var dbConn = new SQLiteConnection(App.DB_PATH))
            {
                dbConn.DropTable<SavedForOffline>();
                dbConn.CreateTable<SavedForOffline>();
                dbConn.Dispose();
                dbConn.Close();                
            }
        }

        #endregion
    }
}
