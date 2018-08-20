using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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

            var imageGrid = new Grid()
            {
                Height = 120,
                Width = 120
            };
            contentStackPanel.Children.Add(imageGrid);

            var image = new Image()
            {
                Height = 120,
                Width = 120,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Source = new BitmapImage(new Uri("/Assets/MultiFactorAuth/multiFactorAuth.png", UriKind.Relative))
            };
            imageGrid.Children.Add(image);

            var ellipse = new Ellipse()
            {
                Margin = new Thickness(12),
                Height = 24,
                Width = 24,
                Fill = new SolidColorBrush(Color.FromArgb(255, 0, 226, 44)),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            Canvas.SetZIndex(ellipse, 1);
            imageGrid.Children.Add(ellipse);

            var icon = new Image()
            {
                Margin = new Thickness(12, 12, 0, 0),
                Height = 24,
                Width = 24,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Source = new BitmapImage(new Uri("/Assets/AppBar/check.png", UriKind.Relative))
            };
            Canvas.SetZIndex(icon, 2);
            imageGrid.Children.Add(icon);

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

            var exportButtonGrid = new Grid();
            exportButtonGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
            exportButtonGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            exportButtonGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });

            var textFileIcon = new Image()
            {
                Height = 24,
                Width = 24,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Source = new BitmapImage(new Uri("/Assets/MultiFactorAuth/textFile.png", UriKind.Relative))
            };
            exportButtonGrid.Children.Add(textFileIcon);
            Grid.SetColumn(textFileIcon, 0);

            var exportTextBlock = new TextBlock()
            {
                Margin = new Thickness(8, 0, 12, 0),
                Text = AppResources.AR_RecoveryKeyFileName,
                VerticalAlignment = VerticalAlignment.Center
            };
            exportButtonGrid.Children.Add(exportTextBlock);
            Grid.SetColumn(exportTextBlock, 1);

            var saveIcon = new Image()
            {
                Height = 24,
                Width = 24,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Source = new BitmapImage(new Uri("/Assets/AppBar/save.png", UriKind.Relative))
            };
            exportButtonGrid.Children.Add(saveIcon);
            Grid.SetColumn(saveIcon, 2);

            var exportButton = new Button()
            {
                Margin = new Thickness(-12),
                BorderThickness = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Command = SaveKeyButtonCommand,
                Content = exportButtonGrid
            };

            var border = new Border()
            {
                Margin = new Thickness(0, 16, 0, 16),
                CornerRadius = new CornerRadius(6),
                HorizontalAlignment = HorizontalAlignment.Center,
                Background = (Brush)Application.Current.Resources["MegaRedColorBrush"],
                Child = exportButton,
                MinHeight = 44
            };
            contentStackPanel.Children.Add(border);

            var recommendation = new TextBlock()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Opacity = 0.8,
                Text = AppMessages.AM_2FA_EnabledDialogRecommendation,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };
            contentStackPanel.Children.Add(recommendation);
            
            this.MainGrid.Children.Add(contentStackPanel);
            Grid.SetRow(contentStackPanel, 2);

            this.closeButton = new Button()
            {
                Margin = new Thickness(12, 24, 12, 12),
                Command = CloseCommand,
                Content = UiResources.UI_Close.ToLower(),
                IsEnabled = false,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Bottom
            };
            this.MainGrid.Children.Add(this.closeButton);
            Grid.SetRow(this.closeButton, 3);

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
