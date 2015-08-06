using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Windows.Storage;
using MegaApp.Classes;
using MegaApp.Resources;

namespace MegaApp.Services
{
    static class SettingsService
    {
        private static readonly Mutex Mutex = new Mutex(false, "BackGroundAgentFileMutex");

        public static void SaveSetting<T>(string key, T value)
        {

            try
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;

                if (settings.Contains(key))
                    settings[key] = value;
                else
                    settings.Add(key, value);

                settings.Save();
            }
            catch (Exception e)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    new CustomMessageDialog(
                            AppMessages.SaveSettingsFailed_Title,
                            String.Format(AppMessages.SaveSettingsFailed, e.Message),
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialog();
                });
            }            
        }
        public static void SecureSaveSetting(string key, string value)
        {
            try
            {
                var settings = IsolatedStorageSettings.ApplicationSettings;

                if (settings.Contains(key))
                    settings[key] = CryptoService.EncryptData(value);
                else
                    settings.Add(key, CryptoService.EncryptData(value));

                settings.Save();
            }
            catch (Exception e)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    new CustomMessageDialog(
                            AppMessages.SaveSettingsFailed_Title,
                            String.Format(AppMessages.SaveSettingsFailed, e.Message),
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialog();
                });
            }            
        }

        public static T LoadSetting<T>(string key, T defaultValue)
        {
            var settings = IsolatedStorageSettings.ApplicationSettings;

            if (settings.Contains(key))
                return (T)settings[key];
            else
                return defaultValue;
        }

        public static string SecureLoadSetting(string key)
        {
            return SecureLoadSetting(key, null);
        }

        public static string SecureLoadSetting(string key, string defaultValue)
        {
            var settings = IsolatedStorageSettings.ApplicationSettings;

            if (settings.Contains(key))
                return CryptoService.DecryptData((string)settings[key]);
            else
                return defaultValue;
        }

        public static T LoadSetting<T>(string key)
        {
            return LoadSetting(key, default(T));
        }

        public static void DeleteSetting(string key)
        {
            var settings = IsolatedStorageSettings.ApplicationSettings;

            if (!settings.Contains(key)) return;
            
            settings.Remove(key);
            settings.Save();
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
                        var dataContractSerializer = new DataContractSerializer(typeof (T));
                        dataContractSerializer.WriteObject(stream, value);
                    }
                }));
            }
            finally
            {
                Mutex.ReleaseMutex();
            }
        }

        public static async Task<T> LoadSettingFromFile<T>(string key)
        {
            var settings = ApplicationData.Current.LocalFolder;

            var file = await settings.GetFileAsync(key);

            using (var stream = await file.OpenStreamForReadAsync())
            {
                var dataContractSerializer = new DataContractSerializer(typeof(T));
                return (T)dataContractSerializer.ReadObject(stream);
            }
        }

        public static void SaveMegaLoginData(string email, string session, bool stayLoggedIn)
        {
            SaveSetting(SettingsResources.StayLoggedIn, stayLoggedIn);
            SaveSetting(SettingsResources.UserMegaEmailAddress, email);
            SaveSetting(SettingsResources.UserMegaSession, session);
            // Save session for automatic camera upload agent
            SaveSettingToFile(SettingsResources.UserMegaSession, session);
        }

        public static void ClearMegaLoginData()
        {
            //SettingsService.DeleteSetting(SettingsResources.StayLoggedIn);
            DeleteSetting(SettingsResources.UserMegaEmailAddress);
            DeleteSetting(SettingsResources.UserMegaSession);
            DeleteSetting(SettingsResources.UserPinLockIsEnabled);
            DeleteSetting(SettingsResources.UserPinLock);

            DeleteSetting(SettingsResources.CameraUploadsFirstInit);
        }
    }
}
