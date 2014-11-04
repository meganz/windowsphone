using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Windows.Storage;
using mega;
using MegaApp.Services;
using Microsoft.Phone.Tasks;

namespace MegaApp.Models
{
    class SettingsViewModel: BaseSdkViewModel
    {
        public SettingsViewModel(MegaSDK megaSdk)
            : base(megaSdk)
        {
            this.AppVersion = AppService.GetAppVersion();
            this.ShareMasterKeyCommand = new DelegateCommand(ShareMasterKey);
            this.CopyMasterKeyCommand = new DelegateCommand(CopyMasterkey);
        }

        #region Commands

        public ICommand ShareMasterKeyCommand { get; set; }
        public ICommand CopyMasterKeyCommand { get; set; }

        #endregion

        #region Methods

        private void ShareMasterKey(object obj)
        {
            var shareStatusTask = new ShareStatusTask {Status = MegaSdk.exportMasterKey()};
            shareStatusTask.Show();
        }

        private void CopyMasterkey(object obj)
        {
            Clipboard.SetText(MegaSdk.exportMasterKey());
            MessageBox.Show("Masterkey copied to clipboard", "Masterkey copied", MessageBoxButton.OK);
        }

        #endregion

        #region Properties

        public string AppVersion { get; set; }

        #endregion
    }
}
