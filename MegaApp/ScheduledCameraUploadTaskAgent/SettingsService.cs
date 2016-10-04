using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using mega;

namespace ScheduledCameraUploadTaskAgent
{
    static class SettingsService
    {
        private static readonly Mutex Mutex = new Mutex(false, "BackGroundAgentFileMutex");

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
                        MegaSDK.log(MLogLevel.LOG_LEVEL_ERROR, 
                            String.Format("Error loading setting from file ({0}).", e.GetType().ToString()));
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
