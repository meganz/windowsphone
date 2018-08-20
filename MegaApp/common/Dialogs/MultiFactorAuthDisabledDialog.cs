using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MegaApp.Resources;

namespace MegaApp.Dialogs
{
    public class MultiFactorAuthDisabledDialog : MegaDialog
    {
        /// <summary>
        /// Dialog to indicate that the user has successfully disabled the Multi-Factor Authentication
        /// </summary>
        public MultiFactorAuthDisabledDialog()
        {
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
                Fill = new SolidColorBrush(Color.FromArgb(255, 237, 24, 53)),
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
                Source = new BitmapImage(new Uri("/Assets/AppBar/cancel.png", UriKind.Relative))
            };
            Canvas.SetZIndex(icon, 2);
            imageGrid.Children.Add(icon);

            var title = new TextBlock()
            {
                Margin = new Thickness(0, 20, 0, 24),
                FontSize = Convert.ToDouble(Application.Current.Resources["PhoneFontSizeMedium"]),
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = AppMessages.AM_2FA_DisabledDialogTitle,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };
            contentStackPanel.Children.Add(title);

            this.MainGrid.Children.Add(contentStackPanel);
            Grid.SetRow(contentStackPanel, 2);

            var closeButton = new Button()
            {
                Margin = new Thickness(12, 24, 12, 12),
                Command = CloseCommand,
                Content = UiResources.UI_Close.ToLower(),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Bottom
            };
            this.MainGrid.Children.Add(closeButton);
            Grid.SetRow(closeButton, 3);
        }
    }
}
