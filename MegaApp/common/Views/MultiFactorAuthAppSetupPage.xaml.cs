using System.ComponentModel;
using System.Windows.Input;
using MegaApp.Enums;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels;

namespace MegaApp.Views
{
    public partial class MultiFactorAuthAppSetupPage : MegaPhoneApplicationPage
    {
        private readonly MultiFactorAuthAppSetupViewModel _multiFactorAuthAppSetupViewModel;

        public MultiFactorAuthAppSetupPage()
        {
            _multiFactorAuthAppSetupViewModel = new MultiFactorAuthAppSetupViewModel();

            this.DataContext = _multiFactorAuthAppSetupViewModel;

            InitializeComponent();
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            if (e.Cancel) return;

            NavigateService.NavigateTo(typeof(SettingsPage),
                NavigationParameter.SecuritySettings);

            e.Cancel = true;
        }

        private void OnBackButtonTapped(object sender, GestureEventArgs e)
        {
            NavigateService.NavigateTo(typeof(SettingsPage),
                NavigationParameter.SecuritySettings);
        }
    }
}