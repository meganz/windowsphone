using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Telerik.Windows.Controls.ContextMenu;
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

        private void OnVerifyTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key >= Key.D0 && e.Key <= Key.D9) ||
                (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
            {
                e.Handled = false;
                return;
            }

            if (this.VerifyButton.IsEnabled && e.Key == Key.Enter &&
                this._multiFactorAuthAppSetupViewModel != null &&
                this._multiFactorAuthAppSetupViewModel.VerifyCommand != null)
            {
                if (this._multiFactorAuthAppSetupViewModel.VerifyCommand.CanExecute(null))
                    this._multiFactorAuthAppSetupViewModel.VerifyCommand.Execute(null);
            }

            e.Handled = true;
        }
    }
}