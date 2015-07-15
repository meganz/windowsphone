using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.Models
{
    class PasswordViewModel: BaseViewModel
    {
        public PasswordViewModel()
        {
            //IsDisablePassword = false;
        }

        #region Methods

        public void CheckPassword()
        {
            if (String.IsNullOrEmpty(Password) || String.IsNullOrWhiteSpace(Password)) return;

            string hashValue = CryptoService.HashData(Password);
            if (!hashValue.Equals(SettingsService.LoadSetting<string>(SettingsResources.UserPinLock)))
            {
                new CustomMessageDialog(
                        AppMessages.WrongPassword_Title,
                        AppMessages.WrongPassword,
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                return;
            }

            //if (IsDisablePassword)
            //{
            //    SettingsService.DeleteSetting(SettingsResources.UserPasswordIsEnabled);
            //    SettingsService.DeleteSetting(SettingsResources.UserPassword);
            //    NavigateService.GoBack();
            //}
            
            NavigateService.NavigateTo(typeof(MainPage), NavigationParameter.PasswordLogin);
        }

        #endregion

        #region Properties

        public string Password { get; set; }

        //public bool IsDisablePassword { get; set; }

        #endregion
    }
}
