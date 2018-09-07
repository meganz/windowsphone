using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using mega;

namespace ScheduledCameraUploadTaskAgent.Services
{
    static class SettingsService
    {
        private static readonly Mutex Mutex = new Mutex(false, "BackGroundAgentFileMutex");
        private static readonly Mutex SettingsMutex = new Mutex(false, "SettingsMutex");

        public static T LoadSetting<T>(string key, T defaultValue)
        {
            var returnValue = defaultValue;

            try
            {
                SettingsMutex.WaitOne();

                var settings = IsolatedStorageSettings.ApplicationSettings;

                if (settings.Contains(key))
                    returnValue = (T)settings[key];
            }
            catch (Exception e)
            {
                // Do nothing. Write a log entry and return the default type value
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error loading setting", e);
            }
            finally
            {
                SettingsMutex.ReleaseMutex();
            }

            return returnValue;
        }

        public static T LoadSettingFromFile<T>(string key)
        {
            var settings = ApplicationData.Current.LocalFolder;

            var result = default(T);

            Mutex.WaitOne();
            try
            {
                Task.WaitAll(Task.Run(async () =>
                {
                    try
                    {
                        var file = await settings.GetFileAsync(key);
                        using (var stream = await file.OpenStreamForReadAsync())
                        {
                            var dataContractSerializer = new DataContractSerializer(typeof(T));
                            result = (T)dataContractSerializer.ReadObject(stream);
                        }
                    }
                    catch (Exception e)
                    {
                        // Do nothing. Write a log entry and return the default type value                        
                        LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error loading setting from file", e);
                    }
                })); 
            }
            finally
            {
                Mutex.ReleaseMutex();
            }

            return result;
        }

        public static void SaveSettingToFile<T>(string key, T value)
        {
            var settings = ApplicationData.Current.LocalFolder;

            Mutex.WaitOne();

            try
            {
                Task.WaitAll(Task.Run(async () =>
                {
                    var file = await settings.CreateFileAsync(key, CreationCollisionOption.ReplaceExisting);

                    using (var stream = await file.OpenStreamForWriteAsync())
                    {
                        var dataContractSerializer = new DataContractSerializer(typeof(T));
                        dataContractSerializer.WriteObject(stream, value);
                    }
                }));
            }
            finally
            {
                Mutex.ReleaseMutex();
            }
        }
    }
}
