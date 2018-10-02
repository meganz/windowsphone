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

        // Indicate if an item exists in the database table.
        public static bool ExistsItem(String tableName, String fieldName, String fieldValue)
        {
            return (SelectItem(tableName, fieldName, fieldValue) != null) ? true : false;
        }

        /// <summary>
        /// Retrieve the first item found in the database table
        /// </summary>
        /// <param name="tableName">Name of the database table</param>
        /// <param name="fieldName">Field by which to search the database</param>
        /// <param name="fieldValue">Field value to search in the database table</param>
        /// <returns>The first item found in the database table</returns>
        public static T SelectItem(String tableName, String fieldName, String fieldValue)
        {
            try
            {
                using (var dbConn = new SQLiteConnection(App.DB_PATH, SQLiteOpenFlags.ReadOnly))
                {
                    return dbConn.Query<T>(
                        "select * from " + tableName + " where " + fieldName + " = '" + fieldValue + "'")
                        .FirstOrDefault();
                }
            }
            catch (Exception e) 
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error selecting item from DB", e);
                return default(T);
            }
        }

        /// <summary>
        /// Retrieve the list of items found in the database table
        /// </summary>
        /// <param name="tableName">Name of the database table</param>
        /// <param name="fieldName">Field by which to search the database</param>
        /// <param name="fieldValue">Field value to search in the database table</param>
        /// <returns>List of items found in the database table</returns>
        public static ObservableCollection<T> SelectItems(String tableName, String fieldName, String fieldValue)
        {
            try 
            {
                using (var dbConn = new SQLiteConnection(App.DB_PATH, SQLiteOpenFlags.ReadOnly))
                {
                    List<T> _itemList = dbConn.Query<T>("select * from " + tableName + " where " + fieldName + " = '" + fieldValue + "'").ToList<T>();
                    ObservableCollection<T> itemList = new ObservableCollection<T>(_itemList);
                    return itemList;
                }            
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error selecting items from DB", e);
                return null;
            }
        }

        /// <summary>
        /// Retrieve the all items from the database table
        /// </summary>
        /// <returns>List of all items from the database table</returns>
        public static ObservableCollection<T> SelectAllItems()
        {
            try
            {
                using (var dbConn = new SQLiteConnection(App.DB_PATH, SQLiteOpenFlags.ReadOnly))
                {
                    List<T> _itemList = dbConn.Table<T>().ToList<T>();
                    ObservableCollection<T> itemList = new ObservableCollection<T>(_itemList);
                    return itemList;
                }
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error selecting all items from DB", e);
                return null;
            }
        }                

        /// <summary>
        /// Update existing item
        /// </summary>
        /// <param name="item">Item to update</param>
        public static void UpdateItem(T item)
        {
            try
            {
                using (var dbConn = new SQLiteConnection(App.DB_PATH))
                {
                    dbConn.RunInTransaction(() =>
                    {
                        dbConn.Update(item);
                    });
                }
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error updating item of the DB", e);
            }
        }        

        /// <summary>
        /// Insert a new item in the database
        /// </summary>
        /// <param name="newItem">Item to insert</param>
        public static void InsertItem(T newItem)
        {
            try
            {
                using (var dbConn = new SQLiteConnection(App.DB_PATH))
                {
                    dbConn.RunInTransaction(() =>
                    {
                        dbConn.Insert(newItem);
                    });
                }
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error inserting item in the DB", e);
            }
        }        

        /// <summary>
        /// Delete the first item found with the specified field value
        /// </summary>
        /// <param name="tableName">Name of the database table</param>
        /// <param name="fieldName">Field by which to search the database</param>
        /// <param name="fieldValue">Field value to search in the database table</param>
        public static void DeleteItem(String tableName, String fieldName, String fieldValue)
        {
            try
            {
                using (var dbConn = new SQLiteConnection(App.DB_PATH))
                {
                    var existingItem = dbConn.Query<T>("select * from " + tableName + " where " + fieldName + " = '" + fieldValue + "'").FirstOrDefault();
                    if (existingItem != null)
                    {
                        dbConn.RunInTransaction(() =>
                        {
                            dbConn.Delete(existingItem);
                        });
                    }
                }
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error deleting item from the DB", e);
            }
        }

        /// <summary>
        /// Delete specific item
        /// </summary>
        /// <param name="item">Item to delete</param>
        public static void DeleteItem(T item)
        {
            try
            {
                using (var dbConn = new SQLiteConnection(App.DB_PATH))
                {
                    dbConn.RunInTransaction(() =>
                    {
                        dbConn.Delete(item);
                    });
                }
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error deleting item from the DB", e);
            }
        }

        /// <summary>
        /// Delete all item or delete table 
        /// </summary>
        /// <returns>TRUE if all went well or FALSE in other case</returns>
        public static bool DeleteAllItems()
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
