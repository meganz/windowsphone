using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using mega;
using MegaApp.Resources;

namespace MegaApp.Models
{
    class ConfirmAccountViewModel: BaseViewModel
    {
        private readonly MegaSDK _megaSdk;

        public ConfirmAccountViewModel(MegaSDK megaSdk)
        {
            this._megaSdk = megaSdk;
            this.ConfirmAccountCommand = new DelegateCommand(this.ConfirmAccount);
        }

        #region Methods

        private void ConfirmAccount(object obj)
        {
            if (String.IsNullOrEmpty(ConfirmCode))
                return;
            else
            {
                if (String.IsNullOrEmpty(Password))
                    MessageBox.Show(AppMessages.RequiredFieldsConfirmAccount, AppMessages.RequiredFields_Title,
                        MessageBoxButton.OK);
                else
                {
                    this._megaSdk.fastConfirmAccount(ConfirmCode, this._megaSdk.getBase64PwKey(Password));
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

        #endregion
    }
}
