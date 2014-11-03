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
using Windows.Storage;
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

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());

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

            var hubSelfie = new RadHubTile()
            {
                Title = "Selfie mode",
                ImageSource = new BitmapImage(new Uri("/Assets/Images/selfie_upload.png", UriKind.Relative)),
                IsFrozen = true,
                Margin = new Thickness(20)
            };
            hubSelfie.Tap += (sender, args) =>
            {
                uploadRadWindow.IsOpen = false;
                NavigateService.NavigateTo(typeof(PhotoCameraPage), NavigationParameter.Normal);
            };

            var hubPicture = new RadHubTile()
            {
                Title = "Select photos",
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

            //var hubSong = new RadHubTile()
            //{
            //    Title = "Select songs",
            //    ImageSource = new BitmapImage(new Uri("/Assets/Images/song_upload.png", UriKind.Relative)),
            //    IsFrozen = true,
            //    Margin = new Thickness(20)
            //};
            //hubSong.Tap += (sender, args) =>
            //{
            //    uploadRadWindow.IsOpen = false;
            //    cloudDrive.NoFolderUpAction = true;
            //    NavigateService.NavigateTo(typeof(SongSelectionPage), NavigationParameter.Normal);
            //};

            grid.Children.Add(hubCamera);
            grid.Children.Add(hubSelfie);
            grid.Children.Add(hubPicture);
            //grid.Children.Add(hubSong);

            Grid.SetColumn(hubCamera, 0);
            Grid.SetColumn(hubSelfie, 1);
            Grid.SetColumn(hubPicture, 0);
            //Grid.SetColumn(hubSong, 1);
            Grid.SetRow(hubCamera, 0);
            Grid.SetRow(hubSelfie, 0);
            Grid.SetRow(hubPicture, 1);
            //Grid.SetRow(hubSong, 1);

            buttonStackPanel.Children.Add(headerText);
            buttonStackPanel.Children.Add(grid);
         
            uploadRadWindow.Content = buttonStackPanel;

            uploadRadWindow.IsOpen = true;
        }

        public static void ShowSortDialog(CloudDriveViewModel cloudDrive)
        {
            var sortRadWindow = new RadModalWindow()
            {
                IsFullScreen = true,
                Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)),
                WindowSizeMode = WindowSizeMode.FitToPlacementTarget,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
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
                Text = "Sort options",
                HorizontalAlignment = HorizontalAlignment.Left,
                FontSize = (double)Application.Current.Resources["PhoneFontSizeLarge"],
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(20, 30, 20, 20)
            };

            var fileAscButton = new Button()
            {
                Content = "files ascending",
                Tag = 1
            };
            fileAscButton.Tap += (sender, args) =>
            {
                sortRadWindow.IsOpen = false;
                SettingsService.SaveSetting(SettingsResources.SortOrderNodes, ((Button)sender).Tag);
                cloudDrive.LoadNodes();
            };

            var fileDescButton = new Button()
            {
                Content = "files descending",
                Tag = 2
            };
            fileDescButton.Tap += (sender, args) =>
            {
                sortRadWindow.IsOpen = false;
                SettingsService.SaveSetting(SettingsResources.SortOrderNodes, ((Button)sender).Tag);
                cloudDrive.LoadNodes();
            };

            var sizeAscButton = new Button()
            {
                Content = "size ascending",
                Tag = 3
            };
            sizeAscButton.Tap += (sender, args) =>
            {
                sortRadWindow.IsOpen = false;
                SettingsService.SaveSetting(SettingsResources.SortOrderNodes, ((Button)sender).Tag);
                cloudDrive.LoadNodes();
            };

            var sizeDescButton = new Button()
            {
                Content = "size descending",
                Tag = 4
            };
            sizeDescButton.Tap += (sender, args) =>
            {
                sortRadWindow.IsOpen = false;
                SettingsService.SaveSetting(SettingsResources.SortOrderNodes, ((Button)sender).Tag);
                cloudDrive.LoadNodes();
            };

            var creationAscButton = new Button()
            {
                Content = "creation date ascending",
                Tag = 5
            };
            creationAscButton.Tap += (sender, args) =>
            {
                sortRadWindow.IsOpen = false;
                SettingsService.SaveSetting(SettingsResources.SortOrderNodes, ((Button)sender).Tag);
                cloudDrive.LoadNodes();
            };

            var creationDescButton = new Button()
            {
                Content = "creation date descending",
                Tag = 6
            };
            creationDescButton.Tap += (sender, args) =>
            {
                sortRadWindow.IsOpen = false;
                SettingsService.SaveSetting(SettingsResources.SortOrderNodes, ((Button)sender).Tag);
                cloudDrive.LoadNodes();
            };

            var modificationAscButton = new Button()
            {
                Content = "modification date ascending",
                Tag = 7
            };
            modificationAscButton.Tap += (sender, args) =>
            {
                sortRadWindow.IsOpen = false;
                SettingsService.SaveSetting(SettingsResources.SortOrderNodes, ((Button)sender).Tag);
                cloudDrive.LoadNodes();
            };

            var modificationDescButton = new Button()
            {
                Content = "modification date descending",
                Tag = 8
            };
            modificationDescButton.Tap += (sender, args) =>
            {
                sortRadWindow.IsOpen = false;
                SettingsService.SaveSetting(SettingsResources.SortOrderNodes, ((Button)sender).Tag);
                cloudDrive.LoadNodes();
            };

            var alphaAscButton = new Button()
            {
                Content = "alphabetical ascending",
                Tag = 9
            };
            alphaAscButton.Tap += (sender, args) =>
            {
                sortRadWindow.IsOpen = false;
                SettingsService.SaveSetting(SettingsResources.SortOrderNodes, ((Button)sender).Tag);
                cloudDrive.LoadNodes();
            };

            var alphaDescButton = new Button()
            {
                Content = "alphabetical descending",
                Tag = 10
            };
            alphaDescButton.Tap += (sender, args) =>
            {
                sortRadWindow.IsOpen = false;
                SettingsService.SaveSetting(SettingsResources.SortOrderNodes, ((Button)sender).Tag);
                cloudDrive.LoadNodes();
            };


            buttonStackPanel.Children.Add(headerText);
            buttonStackPanel.Children.Add(fileAscButton);
            buttonStackPanel.Children.Add(fileDescButton);
            buttonStackPanel.Children.Add(sizeAscButton);
            buttonStackPanel.Children.Add(sizeDescButton);
            buttonStackPanel.Children.Add(creationAscButton);
            buttonStackPanel.Children.Add(creationDescButton);
            buttonStackPanel.Children.Add(modificationAscButton);
            buttonStackPanel.Children.Add(modificationDescButton);
            buttonStackPanel.Children.Add(alphaAscButton);
            buttonStackPanel.Children.Add(alphaDescButton);

            var scrollViewer = new ScrollViewer {Content = buttonStackPanel};

            sortRadWindow.Content = scrollViewer;


            sortRadWindow.IsOpen = true;
        }
    }
}
