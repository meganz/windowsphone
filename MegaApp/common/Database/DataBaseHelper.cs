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
        /// <summary>
        /// Create the table in the database if not exist.
        /// </summary>
        public static void CreateTable()
        {
            try
            {
                using (var db = new SQLiteConnection(App.DB_PATH))
                {
                    db.CreateTable<T>();
                }
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error creating the DB", e);
            }
        }

        // Indicate if the node exists in the database table.
        public static bool ExistsNode(String tableName, String fieldName, String fieldValue)
        {
            return (ReadNode(tableName, fieldName, fieldValue) != null) ? true : false;
        }

        /// <summary>
        /// Retrieve the first node found in the database table
        /// </summary>
        /// <param name="tableName">Name of the database table</param>
        /// <param name="fieldName">Field by which to search the database</param>
        /// <param name="fieldValue">Field value to search in the database table</param>
        /// <returns>The first node found in the database table</returns>
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
            catch (Exception e) 
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error reading node from DB", e);
                return default(T);
            }
        }

        /// <summary>
        /// Retrieve the node list found in the database table
        /// </summary>
        /// <param name="tableName">Name of the database table</param>
        /// <param name="fieldName">Field by which to search the database</param>
        /// <param name="fieldValue">Field value to search in the database table</param>
        /// <returns>Node list found in the database table</returns>
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
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error reading nodes from DB", e);
                return null;
            }
        }

        /// <summary>
        /// Retrieve the all node list from the database table
        /// </summary>
        /// <returns>All node list from the database table</returns>
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
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error reading all nodes from DB", e);
                return null;
            }
        }                

        /// <summary>
        /// Update existing node
        /// </summary>
        /// <param name="node">Node to update</param>
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
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error updating node of the DB", e);
            }
        }        

        /// <summary>
        /// Insert the new node in the database
        /// </summary>
        /// <param name="newNode">No to insert</param>
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
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error inserting node in the DB", e);
            }
        }        

        /// <summary>
        /// Delete the first node found with the specified field value
        /// </summary>
        /// <param name="tableName">Name of the database table</param>
        /// <param name="fieldName">Field by which to search the database</param>
        /// <param name="fieldValue">Field value to search in the database table</param>
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
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error deleting node from the DB", e);
            }
        }

        /// <summary>
        /// Delete specific node
        /// </summary>
        /// <param name="node">Node to delete</param>
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
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error deleting node from the DB", e);
            }
        }

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
            catch (Exception e) 
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error deleting DB table", e);
                return false; 
            }
        }        
    }
}
