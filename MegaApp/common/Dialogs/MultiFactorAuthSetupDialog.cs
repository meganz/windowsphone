using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MegaApp.Enums;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.ViewModels;
using MegaApp.Views;

namespace MegaApp.Dialogs
{
    public class MultiFactorAuthSetupDialog : MegaDialog
    {
        /// <summary>
        /// Dialog to setup the Multi-Factor Authentication for the account
        /// </summary>
        public MultiFactorAuthSetupDialog()
        {
            this.SetupMultiFactorAuthCommand = new DelegateCommand(this.SetupMultiFactorAuth);

            var contentStackPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                Width = Double.NaN,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(20, 0, 20, 0),
                Background = new SolidColorBrush((Color)Application.Current.Resources["PhoneChromeColor"])
            };

            var image = new Image()
            {
                Height = 120,
                Width = 120,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Source = new BitmapImage(new Uri("/Assets/MultiFactorAuth/multiFactorAuth.png", UriKind.Relative))
            };
            contentStackPanel.Children.Add(image);

            var title = new TextBlock()
            {
                Margin = new Thickness(0, 20, 0, 24),
                FontSize = Convert.ToDouble(Application.Current.Resources["PhoneFontSizeMedium"]),
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = AppMessages.AM_2FA_SetupDialogTitle,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };
            contentStackPanel.Children.Add(title);

            var description = new TextBlock()
            {
                Margin = new Thickness(0, 0, 0, 32),
                HorizontalAlignment = HorizontalAlignment.Center,
                Opacity = 0.8,
                Text = AppMessages.AM_2FA_SetupDialogDescription,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };
            contentStackPanel.Children.Add(description);

            var setupMultiFactorAuthButton = new Button()
            {
                Margin = new Thickness(0, 0, 0, 28),
                Content = UiResources.UI_Setup2FA,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Command = this.SetupMultiFactorAuthCommand
            };
            contentStackPanel.Children.Add(setupMultiFactorAuthButton);

            this.MainGrid.Children.Add(contentStackPanel);
            Grid.SetRow(contentStackPanel, 2);
        }

        #region Commands

        /// <summary>
        /// Command invoked to setup the Multi-Factor Authentication
        /// </summary>
        public ICommand SetupMultiFactorAuthCommand { get; private set; }

        #endregion

        #region Methods

        private void SetupMultiFactorAuth(object obj = null)
        {
            this.DialogResult = true;

            base.CloseDialog();

            NavigateService.NavigateTo(typeof(MultiFactorAuthAppSetupPage),
                NavigationParameter.Normal);
        }

        #endregion
    }
}
