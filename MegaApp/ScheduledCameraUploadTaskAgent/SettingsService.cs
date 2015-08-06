using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

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
                    var file = await settings.GetFileAsync(key);

                    using (var stream = await file.OpenStreamForReadAsync())
                    {
                        var dataContractSerializer = new DataContractSerializer(typeof(T));
                        result = (T)dataContractSerializer.ReadObject(stream);
                    }
                })); 
            }
            finally
            {
                Mutex.ReleaseMutex();
            }

            return result;
        }

        public static T LoadSetting<T>(string key, T defaultValue)
        {
            var settings = IsolatedStorageSettings.ApplicationSettings;

            if (settings.Contains(key))
                return (T)settings[key];
            else
                return defaultValue;
        }

        public static void SaveSetting<T>(string key, T value)
        {
            var settings = IsolatedStorageSettings.ApplicationSettings;

            if (settings.Contains(key))
                settings[key] = value;
            else
                settings.Add(key, value);

            settings.Save();
        }
    }
}
