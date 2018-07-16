using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MegaApp.Resources;

namespace MegaApp.Dialogs
{
    public class MultiFactorAuthEnabledDialog : MegaDialog
    {
        /// <summary>
        /// Dialog to indicate that the user has successfully enabled the Multi-Factor Authentication
        /// </summary>
        public MultiFactorAuthEnabledDialog()
        {
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
                Margin = new Thickness(0, 0, 0, 32),
                HorizontalAlignment = HorizontalAlignment.Center,
                Opacity = 0.8,
                Text = AppMessages.AM_2FA_EnabledDialogDescription,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };
            contentStackPanel.Children.Add(description);

            var closeButton = new Button()
            {
                Content = UiResources.UI_Close.ToLower(),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Command = CloseCommand
            };
            contentStackPanel.Children.Add(closeButton);

            this.MainGrid.Children.Add(contentStackPanel);
            Grid.SetRow(contentStackPanel, 2);
        }
    }
}
