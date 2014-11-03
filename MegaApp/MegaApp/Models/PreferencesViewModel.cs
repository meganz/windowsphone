using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using mega;

namespace MegaApp.Models
{
    class PreferencesViewModel: BaseSdkViewModel
    {
        public PreferencesViewModel(MegaSDK megaSdk)
            : base(megaSdk)
        {
            this.ExportMasterKeyCommand = new DelegateCommand(ExportMasterKey);
        }

        #region Commands

        public ICommand ExportMasterKeyCommand { get; set; }

        #endregion

        #region Methods

        private void ExportMasterKey(object obj)
        {
           //
        }

        #endregion
    }
}
