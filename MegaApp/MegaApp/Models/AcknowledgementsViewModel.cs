using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MegaApp.Resources;
using Microsoft.Phone.Tasks;

namespace MegaApp.Models
{
    class AcknowledgementsViewModel: BaseViewModel
    {
        public AcknowledgementsViewModel()
        {
            this.SpecialThankYou = UiResources.SpecialThankYou;
            this.GoedWareCommand = new DelegateCommand(NavigateToGoedWare);
        }

        #region Private Methods

        private static void NavigateToGoedWare(object obj)
        {
            var webBrowserTask = new WebBrowserTask { Uri = new Uri(@"http://www.goedware.com") };
            webBrowserTask.Show();
        }

        #endregion

        #region Commands

        public ICommand GoedWareCommand { get; set; }

        #endregion

        #region Properties

        public string SpecialThankYou { get; set; }

        #endregion
    }
}
