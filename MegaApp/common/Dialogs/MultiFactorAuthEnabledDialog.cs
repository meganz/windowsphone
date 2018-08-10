using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.ViewModels;

namespace MegaApp.Dialogs
{
    public class MultiFactorAuthEnabledDialog : MegaDialog
    {
        /// <summary>
        /// Dialog to indicate that the user has successfully enabled the Multi-Factor Authentication
        /// </summary>
        public MultiFactorAuthEnabledDialog()
        {
            this.SaveKeyButtonCommand = new DelegateCommand(this.SaveKey);

            this.IsClosedOnBackButton = false;
            this.IsClosedOnOutsideTap = false;

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
                Text = AppMessages.AM_2FA_EnabledDialogTitle,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };
            contentStackPanel.Children.Add(title);

            var description = new TextBlock()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Opacity = 0.8,
                Text = AppMessages.AM_2FA_EnabledDialogDescription,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };
            contentStackPanel.Children.Add(description);

            var border = new Border()
            {
                Margin = new Thickness(-20, 24, -20, 32),
                Background = (Brush)Application.Current.Resources["PhoneInactiveBrush"]
            };
            border.Child = new TextBlock()
            {
                Margin = new Thickness(20),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Opacity = 0.8,
                Text = AppMessages.AM_2FA_EnabledDialogRecommendation,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };
            contentStackPanel.Children.Add(border);
            
            this.MainGrid.Children.Add(contentStackPanel);
            Grid.SetRow(contentStackPanel, 2);

            var buttonsGrid = new Grid()
            {
                Width = Double.NaN,
                Margin = new Thickness(12),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Bottom,
                Background = new SolidColorBrush((Color)Application.Current.Resources["PhoneChromeColor"])
            };
            buttonsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            buttonsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            var exportButton = new Button()
            {
                Content = UiResources.UI_Export.ToLower(),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Command = SaveKeyButtonCommand
            };
            buttonsGrid.Children.Add(exportButton);
            Grid.SetColumn(exportButton, 0);

            this.closeButton = new Button()
            {
                Content = UiResources.UI_Close.ToLower(),
                IsEnabled = false,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Command = CloseCommand
            };
            buttonsGrid.Children.Add(this.closeButton);
            Grid.SetColumn(this.closeButton, 1);

            this.MainGrid.Children.Add(buttonsGrid);
            Grid.SetRow(buttonsGrid, 3);

            this.Initialize();
        }

        #region Commands

        /// <summary>
        /// Command invoked when the user select the "Backup Recovery key" option
        /// </summary>
        public ICommand SaveKeyButtonCommand { get; private set; }

        #endregion

        #region Methods

        private async void Initialize()
        {
            var isRecoveryKeyExported = new IsMasterKeyExportedRequestListenerAsync();
            this.closeButton.IsEnabled = await isRecoveryKeyExported.ExecuteAsync(() =>
                SdkService.MegaSdk.isMasterKeyExported(isRecoveryKeyExported));
        }

        /// <summary>
        /// Backup the Recovery key
        /// </summary>
        private void SaveKey(object obj)
        {
            try
            {
                this.CloseDialog();
                Clipboard.SetText(SdkService.MegaSdk.exportMasterKey());
                SdkService.MegaSdk.masterKeyExported();

                new CustomMessageDialog(
                    AppMessages.AM_RecoveryKeyCopied_Title,
                    AppMessages.AM_RecoveryKeyCopied,
                    App.AppInformation,
                    MessageDialogButtons.Ok).ShowDialog();
            }
            catch (Exception)
            {
                new CustomMessageDialog(
                    AppMessages.AM_RecoveryKeyClipboardFailed_Title,
                    AppMessages.AM_RecoveryKeyClipboardFailed,
                    App.AppInformation,
                    MessageDialogButtons.Ok).ShowDialog();
            }
        }

        #endregion

        #region Controls

        private Button closeButton;

        #endregion
    }
}
