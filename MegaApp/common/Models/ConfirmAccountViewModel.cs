using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.Models
{
    class ConfirmAccountViewModel : BaseViewModel
    {
        private readonly MegaSDK _megaSdk;        

        public ConfirmAccountViewModel(MegaSDK megaSdk)
        {
            this.ControlState = true;
            this._megaSdk = megaSdk;            
            this.ConfirmAccountCommand = new DelegateCommand(this.ConfirmAccount);
        }

        #region Methods

        private void ConfirmAccount(object obj)
        {
            if (!NetworkService.IsNetworkAvailable(true)) return;

            if (String.IsNullOrEmpty(ConfirmCode))
                return;
            else
            {
                if (String.IsNullOrEmpty(Password))
                {
                    new CustomMessageDialog(
                        AppMessages.RequiredFields_Title,
                        AppMessages.RequiredFieldsConfirmAccount,
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                }
                else
                {
                    this._megaSdk.confirmAccount(ConfirmCode, Password,
                        new ConfirmAccountRequestListener(this));
                }
            }
        }

        #endregion

        #region Commands

        public ICommand ConfirmAccountCommand { get; set; }

        #endregion

        #region Properties

        public string ConfirmCode { get; set; }
        public string Password { get; set; }

        private string _email;
        public string Email
        {
            get { return _email; }
            set { SetField(ref _email, value); }
        }

        #endregion
    }
}
