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
        private static readonly Mutex FileSettingMutex = new Mutex(false, "FileSettingMutex");
        private static readonly Mutex SettingsMutex = new Mutex(false, "SettingsMutex");

        public static void SaveSetting<T>(string key, T value)
        {
            try
            {
                SettingsMutex.WaitOne();

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
            finally
            {
                SettingsMutex.ReleaseMutex();
            }
        }

        public static void SecureSaveSetting(string key, string value)
        {
            try
            {
                SettingsMutex.WaitOne();

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
            finally
            {
                SettingsMutex.ReleaseMutex();
            }
        }

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
            catch(Exception e)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    new CustomMessageDialog(
                        AppMessages.AM_LoadSettingsFailed_Title,
                        String.Format(AppMessages.AM_LoadSettingsFailed, e.Message),
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                });                
            }
            finally
            {
                SettingsMutex.ReleaseMutex();                
            }

            return returnValue;
        }

        public static string SecureLoadSetting(string key)
        {
            return SecureLoadSetting(key, null);
        }

        public static string SecureLoadSetting(string key, string defaultValue)
        {
            var returnValue = defaultValue;

            try
            {
                SettingsMutex.WaitOne();

                var settings = IsolatedStorageSettings.ApplicationSettings;                

                if (settings.Contains(key))
                    returnValue = CryptoService.DecryptData((string)settings[key]);
            }
            catch (Exception e)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    new CustomMessageDialog(
                        AppMessages.AM_LoadSettingsFailed_Title,
                        String.Format(AppMessages.AM_LoadSettingsFailed, e.Message),
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                });                
            }
            finally
            {
                SettingsMutex.ReleaseMutex();
            }

            return returnValue;
        }

        public static T LoadSetting<T>(string key)
        {
            return LoadSetting(key, default(T));
        }

        public static void DeleteSetting(string key)
        {
            try
            {
                SettingsMutex.WaitOne();

                var settings = IsolatedStorageSettings.ApplicationSettings;

                if (!settings.Contains(key)) return;

                settings.Remove(key);
                settings.Save();
            }
            catch(Exception e)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    new CustomMessageDialog(
                        AppMessages.AM_DeleteSettingsFailed_Title,
                        String.Format(AppMessages.AM_DeleteSettingsFailed, e.Message),
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                });
            }
            finally
            {
                SettingsMutex.ReleaseMutex();
            }
        }

        public static void DeleteFileSetting(string key)
        {
            try
            {
                FileSettingMutex.WaitOne();

                var settings = ApplicationData.Current.LocalFolder;

                Task.WaitAll(Task.Run(async () =>
                {
                    try
                    {
                        var file = await settings.GetFileAsync(key);
                        await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    }
                    catch (FileNotFoundException) { /* Do nothing */ }
                }));
            }
            catch (Exception e)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    new CustomMessageDialog(
                        AppMessages.AM_DeleteSettingsFailed_Title,
                        String.Format(AppMessages.AM_DeleteSettingsFailed, e.Message),
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                });
            }
            finally
            {
                FileSettingMutex.ReleaseMutex();
            }
        }

        public static void SaveSettingToFile<T>(string key, T value)
        {
            try
            {
                FileSettingMutex.WaitOne();
                
                var settings = ApplicationData.Current.LocalFolder;
                
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
            finally
            {
                FileSettingMutex.ReleaseMutex();
            }
        }

        public static async Task<T> LoadSettingFromFile<T>(string key)
        {
            var returnValue = default(T);

            try
            {
                FileSettingMutex.WaitOne();

                var settings = ApplicationData.Current.LocalFolder;

                var file = await settings.GetFileAsync(key);

                using (var stream = await file.OpenStreamForReadAsync())
                {
                    var dataContractSerializer = new DataContractSerializer(typeof(T));
                    returnValue = (T)dataContractSerializer.ReadObject(stream);
                }
            }
            catch (Exception e)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    new CustomMessageDialog(
                        AppMessages.AM_LoadSettingsFailed_Title,
                        String.Format(AppMessages.AM_LoadSettingsFailed, e.Message),
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                });
            }
            finally
            {
                FileSettingMutex.ReleaseMutex();
            }

            return returnValue;
        }

        public static bool HasValidSession()
        {
            try
            {
                if (SettingsService.LoadSetting<string>(SettingsResources.UserMegaSession) != null)
                    return true;
                else
                    return false;
            }
            catch (ArgumentNullException) { return false; }
        }

        public static void SaveMegaLoginData(string email, string session)
        {
            SaveSetting(SettingsResources.UserMegaEmailAddress, email);
            SaveSetting(SettingsResources.UserMegaSession, session);
            
            // Save session for automatic camera upload agent
            SaveSettingToFile(SettingsResources.UserMegaSession, session);
        }

        public static void ClearMegaLoginData()
        {
            DeleteSetting(SettingsResources.UserMegaEmailAddress);
            DeleteSetting(SettingsResources.UserMegaSession);
            DeleteSetting(SettingsResources.UserPinLockIsEnabled);
            DeleteSetting(SettingsResources.UserPinLock);

            DeleteSetting(SettingsResources.CameraUploadsFirstInit);
        }
    }
}
