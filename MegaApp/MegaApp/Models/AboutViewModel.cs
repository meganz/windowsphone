using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MegaApp.Enums;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;
using Microsoft.Phone.Tasks;

namespace MegaApp.Models
{
    class AboutViewModel: BaseViewModel
    {
        public AboutViewModel()
        {
            this.AppVersion = AppService.GetAppVersion();
            this.TermsOfServiceCommand = new DelegateCommand(NavigateToTermsOfService);
            this.PrivacyPolicyCommand = new DelegateCommand(NavigateToPrivacyPolicy);
            this.AcknowledgementsCommands = new DelegateCommand(NavigateToAcknowledgements);
        }

        #region Private Methods

        private static void NavigateToPrivacyPolicy(object obj)
        {
            var webBrowserTask = new WebBrowserTask {Uri = new Uri(AppResources.PrivacyPolicyUrl)};
            webBrowserTask.Show();
        }

        private static void NavigateToTermsOfService(object obj)
        {
            var webBrowserTask = new WebBrowserTask { Uri = new Uri(AppResources.TermsOfUseUrl) };
            webBrowserTask.Show();
        }

        private static void NavigateToAcknowledgements(object obj)
        {
            NavigateService.NavigateTo(typeof(AcknowledgementsPage), NavigationParameter.Normal);
        }

        #endregion

        #region Commands

        public ICommand TermsOfServiceCommand { get; set; }
        public ICommand PrivacyPolicyCommand { get; set; }
        public ICommand AcknowledgementsCommands { get; set; }

        #endregion

        #region Properties

        public string AppVersion { get; set; }

        #endregion
    }
}
