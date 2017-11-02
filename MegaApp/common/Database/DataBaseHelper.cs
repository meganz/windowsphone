using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mega;
using MegaApp.Classes;
using MegaApp.Services;

namespace MegaApp.Database
{
    public class DataBaseHelper<T> where T : new()
    {
        // Indicate if the node exists in the database table.
        public static bool ExistsNode(String tableName, String fieldName, String fieldValue)
        {
            return (ReadNode(tableName, fieldName, fieldValue) != null) ? true : false;
        }

        // Retrieve the first node found in the database table.
        public static T ReadNode(String tableName, String fieldName, String fieldValue)
        {
            try
            {
                using (var dbConn = new SQLiteConnection(App.DB_PATH, SQLiteOpenFlags.ReadOnly))
                {
                    var existingNode = dbConn.Query<T>("select * from " + tableName + " where " + fieldName + " = '" + fieldValue + "'").FirstOrDefault();                    
                    return existingNode;
                }
            }
            catch (SQLiteException) { return default(T); }
        }

        // Retrieve the node list found in the database table.
        public static ObservableCollection<T> ReadNodes(String tableName, String fieldName, String fieldValue)
        {
            try 
            {
                using (var dbConn = new SQLiteConnection(App.DB_PATH, SQLiteOpenFlags.ReadOnly))
                {
                    List<T> _nodeList = dbConn.Query<T>("select * from " + tableName + " where " + fieldName + " = '" + fieldValue + "'").ToList<T>();
                    ObservableCollection<T> nodeList = new ObservableCollection<T>(_nodeList);
                    return nodeList;
                }            
            }
            catch (SQLiteException) { return null; }
        }

        // Retrieve the all node list from the database table.
        public static ObservableCollection<T> ReadAllNodes()
        {
            try
            {
                using (var dbConn = new SQLiteConnection(App.DB_PATH, SQLiteOpenFlags.ReadOnly))
                {
                    List<T> _nodeList = dbConn.Table<T>().ToList<T>();
                    ObservableCollection<T> nodeList = new ObservableCollection<T>(_nodeList);
                    return nodeList;
                }
            }
            catch (SQLiteException) { return null; }
        }                

        // Update existing node 
        public static void UpdateNode(T node)
        {
            try
            {
                using (var dbConn = new SQLiteConnection(App.DB_PATH))
                {
                    dbConn.RunInTransaction(() =>
                    {
                        dbConn.Update(node);
                    });
                }
            }
            catch (SQLiteException) { }
        }        

        // Insert the new node in the database. 
        public static void Insert(T newNode)
        {
            try
            {
                using (var dbConn = new SQLiteConnection(App.DB_PATH))
                {
                    dbConn.RunInTransaction(() =>
                    {
                        dbConn.Insert(newNode);
                    });
                }
            }
            catch (SQLiteException) { }
        }        

        // Delete the first node found with the specified field value
        public static void DeleteNode(String tableName, String fieldName, String fieldValue)
        {
            try
            {
                using (var dbConn = new SQLiteConnection(App.DB_PATH))
                {
                    var existingNode = dbConn.Query<T>("select * from " + tableName + " where " + fieldName + " = '" + fieldValue + "'").FirstOrDefault();
                    if (existingNode != null)
                    {
                        dbConn.RunInTransaction(() =>
                        {
                            dbConn.Delete(existingNode);
                        });
                    }
                }
            }
            catch (SQLiteException) { }
        }

        // Delete specific node
        public static void DeleteNode(T node)
        {
            try
            {
                using (var dbConn = new SQLiteConnection(App.DB_PATH))
                {
                    dbConn.RunInTransaction(() =>
                    {
                        dbConn.Delete(node);
                    });
                }
            }
            catch (SQLiteException) { }
        }

        // Delete all node list or delete table 

        /// <summary>
        /// Delete all node list or delete table 
        /// </summary>
        /// <returns>TRUE if all went well or FALSE in other case</returns>
        public static bool DeleteAllNodes()
        {
            try
            {
                using (var dbConn = new SQLiteConnection(App.DB_PATH))
                {
                    dbConn.DropTable<T>();
                    dbConn.CreateTable<T>();
                    dbConn.Dispose();
                    dbConn.Close();

                    return true;
                }
            }
            catch (SQLiteException e) 
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error deleting DB table", e);
                return false; 
            }
        }        
    }
}
