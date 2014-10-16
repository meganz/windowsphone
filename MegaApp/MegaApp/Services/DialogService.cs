using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Net;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MegaApp.Models;
using MegaApp.Resources;
using Microsoft.Phone.Tasks;
using Telerik.Windows.Controls;
using Binding = System.Windows.Data.Binding;

namespace MegaApp.Services
{
    static class DialogService
    {
        public static async void ShowShareLink(string link)
        {
            var buttonsDataTemplate = (DataTemplate)Application.Current.Resources["ShowShareLinkButtons"];
            MessageBoxClosedEventArgs closedEventArgs = await RadMessageBox.ShowAsync(
                buttonsTemplate:buttonsDataTemplate,
                buttonsContent: new string[] {UiResources.ShareButton, UiResources.CopyButton, UiResources.CancelButton},
                title: UiResources.MegaLinkTitle,
                message: link
                );

            switch (closedEventArgs.ButtonIndex)
            {
                // Share button clicked
                case 0:
                {
                    var shareLinkTask = new ShareLinkTask {LinkUri = new Uri(link), Title = UiResources.MegaShareLinkMessage};
                    shareLinkTask.Show();
                    break;
                }
                // Copy button clicked
                case 1:
                {
                    Clipboard.SetText(link);
                    MessageBox.Show(AppMessages.LinkCopiedToClipboard, AppMessages.LinkCopiedToClipboard_Title, MessageBoxButton.OK);
                    break;
                }
            }
        }

        public static async void ShowOpenLink(string name, string link, CloudDriveViewModel cloudDrive)
        {
            MessageBoxClosedEventArgs closedEventArgs = await RadMessageBox.ShowAsync(
                buttonsContent: new string[] { "import", "download" },
                title: "Download MEGA link",
                message: name
                );

            switch (closedEventArgs.ButtonIndex)
            {
                // Import button clicked
                case 0:
                    {
                        cloudDrive.ImportLink(link);
                        break;
                    }
                // Download button clicked
                case 1:
                    {
                        //cloudDrive.ImportLink(link);
                        break;
                    }
            }
        }

        public static void ShowUploadOptions(CloudDriveViewModel cloudDrive)
        {

            var uploadRadWindow = new RadModalWindow()
            {
                IsFullScreen = true,
                Background = new SolidColorBrush(Color.FromArgb(192,0,0,0)),
                WindowSizeMode = WindowSizeMode.FitToPlacementTarget,
                HorizontalContentAlignment= HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Top,
            };

            var buttonStackPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                Width = Double.NaN,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Background = (SolidColorBrush)Application.Current.Resources["PhoneChromeBrush"]
            };

            var headerText = new TextBlock()
            {
                Text = "Upload options",
                HorizontalAlignment = HorizontalAlignment.Left,
                FontSize = (double)Application.Current.Resources["PhoneFontSizeLarge"],
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(20, 30, 20, 20)
            };

            var takePhotoButton = new Button()
            {
                Content = "take photo",
                Width = Double.NaN,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(8, 0, 8, 20)
                
            };
            takePhotoButton.Tap += (sender, args) =>
            {
                uploadRadWindow.IsOpen = false;
                cloudDrive.CaptureCameraImage();
            };

            var selectPhotoButton = new Button()
            {
                Content = "select photo",
                Width = Double.NaN,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(8, 0, 8, 20)

            };
            selectPhotoButton.Tap += (sender, args) =>
            {
                uploadRadWindow.IsOpen = false;
                cloudDrive.SelectImage();
            };


            buttonStackPanel.Children.Add(headerText);
            buttonStackPanel.Children.Add(takePhotoButton);
            buttonStackPanel.Children.Add(selectPhotoButton);

            uploadRadWindow.Content = buttonStackPanel;

            uploadRadWindow.IsOpen = true;
        }
    }
}
