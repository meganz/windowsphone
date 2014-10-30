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
using System.Windows.Media.Imaging;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Pages;
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

            //var takePhotoButton = new Button()
            //{
            //    Content = "take photo",
            //    Width = Double.NaN,
            //    HorizontalAlignment = HorizontalAlignment.Stretch,
            //    Margin = new Thickness(8, 0, 8, 20)
                
            //};
            //takePhotoButton.Tap += (sender, args) =>
            //{
            //    uploadRadWindow.IsOpen = false;
            //    cloudDrive.CaptureCameraImage();
            //};

            //var selectPhotoButton = new Button()
            //{
            //    Content = "select photo",
            //    Width = Double.NaN,
            //    HorizontalAlignment = HorizontalAlignment.Stretch,
            //    Margin = new Thickness(8, 0, 8, 20)

            //};
            //selectPhotoButton.Tap += (sender, args) =>
            //{
            //    uploadRadWindow.IsOpen = false;
            //    cloudDrive.SelectImage();
            //};

            //var selectPhotosButton = new Button()
            //{
            //    Content = "select photos",
            //    Width = Double.NaN,
            //    HorizontalAlignment = HorizontalAlignment.Stretch,
            //    Margin = new Thickness(8, 0, 8, 20)

            //};
            //selectPhotosButton.Tap += (sender, args) =>
            //{
            //    uploadRadWindow.IsOpen = false;
            //    cloudDrive.NoFolderUpAction = true;
            //    NavigateService.NavigateTo(typeof(MediaSelectionPage), NavigationParameter.Normal);
            //};

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            var hubCamera = new RadHubTile()
            {
                Title = "Take photo",
                ImageSource = new BitmapImage(new Uri("/Assets/Images/camera_upload.png", UriKind.Relative)),
                IsFrozen = true,
                Margin = new Thickness(20)
            };
            hubCamera.Tap += (sender, args) =>
            {
                uploadRadWindow.IsOpen = false;
                cloudDrive.CaptureCameraImage();
            };

            var hubPicture = new RadHubTile()
            {
                Title = "Select photo(s)",
                ImageSource = new BitmapImage(new Uri("/Assets/Images/picture_upload.png", UriKind.Relative)),
                IsFrozen = true,
                Margin = new Thickness(20)
            };
            hubPicture.Tap += (sender, args) =>
            {
                uploadRadWindow.IsOpen = false;
                cloudDrive.NoFolderUpAction = true;
                NavigateService.NavigateTo(typeof(MediaSelectionPage), NavigationParameter.Normal);
            };

            grid.Children.Add(hubCamera);
            grid.Children.Add(hubPicture);

            Grid.SetColumn(hubCamera, 0);
            Grid.SetColumn(hubPicture, 1);

            buttonStackPanel.Children.Add(headerText);
            buttonStackPanel.Children.Add(grid);
          

            uploadRadWindow.Content = buttonStackPanel;

            uploadRadWindow.IsOpen = true;
        }
    }
}
