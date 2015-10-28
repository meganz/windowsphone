using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MegaApp.Models;

namespace MegaApp.Classes
{
    public class SavedForOffline
    {
        #region Properties

        // The Fingerprint property is marked as the Primary Key
        [SQLite.PrimaryKey]        
        public String Fingerprint { get; set; }
        public String Base64Handle { get; set; }
        public String LocalPath { get; set; }
        public String ParentBase64Handle { get; set; }

        #endregion

        #region DatabaseHelper
                
        // Retrieve the specific node from the database. 
        public SavedForOffline ReadNode(String fingerprint)
        {
            using (var dbConn = new SQLiteConnection(App.DB_PATH))
            {
                var existingNode = dbConn.Query<SavedForOffline>("select * from SavedForOffline where Fingerprint =" + fingerprint).FirstOrDefault();
                return existingNode;
            }
        }

        // Retrieve the all node list from the database. 
        public ObservableCollection<SavedForOffline> ReadNodes()
        {
            using (var dbConn = new SQLiteConnection(App.DB_PATH))
            {
                List<SavedForOffline> _nodeList = dbConn.Table<SavedForOffline>().ToList<SavedForOffline>();
                ObservableCollection<SavedForOffline> nodeList = new ObservableCollection<SavedForOffline>(_nodeList);
                return nodeList;
            }
        }

        // Update existing node 
        public void UpdateNode(SavedForOffline node)
        {
            using (var dbConn = new SQLiteConnection(App.DB_PATH))
            {
                var existingNode = dbConn.Query<SavedForOffline>("select * from SavedForOffline where Fingerprint =" + node.Fingerprint).FirstOrDefault();
                if (existingNode != null)
                {
                    existingNode.Base64Handle = node.Base64Handle;
                    existingNode.LocalPath = node.LocalPath;
                    existingNode.ParentBase64Handle = node.ParentBase64Handle;
                    
                    dbConn.RunInTransaction(() =>
                    {
                        dbConn.Update(existingNode);
                    });
                }
            }
        }

        // Insert the new node in the SavedForOffline table. 
        public void Insert(SavedForOffline newNode)
        {
            using (var dbConn = new SQLiteConnection(App.DB_PATH))
            {
                dbConn.RunInTransaction(() =>
                {
                    dbConn.Insert(newNode);
                });
            }
        }

        // Delete specific node 
        public void DeleteContact(String fingerprint)
        {
            using (var dbConn = new SQLiteConnection(App.DB_PATH))
            {
                var existingNode = dbConn.Query<SavedForOffline>("select * from SavedForOffline where Fingerprint =" + fingerprint).FirstOrDefault();
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
        public void DeleteAllNodes()
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
