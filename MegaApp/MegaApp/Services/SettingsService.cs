using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MegaApp.Resources;

namespace MegaApp.Services
{
    static class SettingsService
    {
        public static void SaveSetting<T>(string key, T value)
        {
            var settings = IsolatedStorageSettings.ApplicationSettings;

            if (settings.Contains(key))
                settings[key] = value;
            else
                settings.Add(key, value);

            settings.Save();
        }
        public static T LoadSetting<T>(string key, T defaultValue)
        {
            var settings = IsolatedStorageSettings.ApplicationSettings;

            if (settings.Contains(key))
                return (T)settings[key];
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

        public static void SaveMegaLoginData(string email, string session, bool stayLoggedIn)
        {
            SettingsService.SaveSetting(SettingsResources.StayLoggedIn, stayLoggedIn);
            SettingsService.SaveSetting(SettingsResources.UserMegaEmailAddress, email);
            SettingsService.SaveSetting(SettingsResources.UserMegaSession, session);
        }

        public static void ClearMegaLoginData()
        {
            //SettingsService.DeleteSetting(SettingsResources.StayLoggedIn);
            SettingsService.DeleteSetting(SettingsResources.UserMegaEmailAddress);
            SettingsService.DeleteSetting(SettingsResources.UserMegaSession);
        }
    }
}
