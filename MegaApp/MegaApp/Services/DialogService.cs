using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Pages;
using MegaApp.Resources;
using Microsoft.Phone.Tasks;
using Telerik.Windows.Controls;

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
                        cloudDrive.DownloadLink(link);
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
                ImageSource =new BitmapImage(new Uri("/Assets/Images/take photos.Screen-WVGA.png", UriKind.Relative)),
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
                ImageSource = new BitmapImage(new Uri("/Assets/Images/selfie_upload.Screen-WVGA.png", UriKind.Relative)),
                IsFrozen = true,
                Margin = new Thickness(20)
            };
            hubSelfie.Tap += (sender, args) =>
            {
                uploadRadWindow.IsOpen = false;
                cloudDrive.NoFolderUpAction = true;
                NavigateService.NavigateTo(typeof(PhotoCameraPage), NavigationParameter.Normal);
            };

            var hubPicture = new RadHubTile()
            {
                Title = "Select photos",
                ImageSource = new BitmapImage(new Uri("/Assets/Images/picture_upload.Screen-WVGA.png", UriKind.Relative)),
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
            // If rootnode is not determined yet. Do nothing
            if (cloudDrive.CurrentRootNode == null) return;

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
                Content = "Files type/name ascending"
            };
            fileAscButton.Tap += (sender, args) =>
            {
                sortRadWindow.IsOpen = false;
                UiService.SetSortOrder(cloudDrive.CurrentRootNode.Handle, (int) MSortOrderType.ORDER_DEFAULT_ASC);
                Task.Run(() => cloudDrive.LoadNodes());
            };

            var fileDescButton = new Button()
            {
                Content = "Files type/name descending"
            };
            fileDescButton.Tap += (sender, args) =>
            {
                sortRadWindow.IsOpen = false;
                UiService.SetSortOrder(cloudDrive.CurrentRootNode.Handle, (int)MSortOrderType.ORDER_DEFAULT_DESC);
                Task.Run(() => cloudDrive.LoadNodes());
            };

            var sizeAscButton = new Button()
            {
                Content = "Size ascending"
            };
            sizeAscButton.Tap += (sender, args) =>
            {
                sortRadWindow.IsOpen = false;
                UiService.SetSortOrder(cloudDrive.CurrentRootNode.Handle, (int)MSortOrderType.ORDER_SIZE_ASC);
                Task.Run(() => cloudDrive.LoadNodes());
            };

            var sizeDescButton = new Button()
            {
                Content = "Size descending"
            };
            sizeDescButton.Tap += (sender, args) =>
            {
                sortRadWindow.IsOpen = false;
                UiService.SetSortOrder(cloudDrive.CurrentRootNode.Handle, (int)MSortOrderType.ORDER_SIZE_DESC);
                Task.Run(() => cloudDrive.LoadNodes());
            };

            var modificationAscButton = new Button()
            {
                Content = "Modification date ascending"
            };
            modificationAscButton.Tap += (sender, args) =>
            {
                sortRadWindow.IsOpen = false;
                UiService.SetSortOrder(cloudDrive.CurrentRootNode.Handle, (int)MSortOrderType.ORDER_MODIFICATION_ASC);
                Task.Run(() => cloudDrive.LoadNodes());
            };

            var modificationDescButton = new Button()
            {
                Content = "Modification date descending"
            };
            modificationDescButton.Tap += (sender, args) =>
            {
                sortRadWindow.IsOpen = false;
                UiService.SetSortOrder(cloudDrive.CurrentRootNode.Handle, (int)MSortOrderType.ORDER_MODIFICATION_DESC);
                Task.Run(() => cloudDrive.LoadNodes());
            };

            var alphaAscButton = new Button()
            {
                Content = "Alphabetical ascending"
            };
            alphaAscButton.Tap += (sender, args) =>
            {
                sortRadWindow.IsOpen = false;
                UiService.SetSortOrder(cloudDrive.CurrentRootNode.Handle, (int)MSortOrderType.ORDER_ALPHABETICAL_ASC);
                Task.Run(() => cloudDrive.LoadNodes());
            };

            var alphaDescButton = new Button()
            {
                Content = "Alphabetical descending"
            };
            alphaDescButton.Tap += (sender, args) =>
            {
                sortRadWindow.IsOpen = false;
                UiService.SetSortOrder(cloudDrive.CurrentRootNode.Handle, (int)MSortOrderType.ORDER_ALPHABETICAL_DESC);
                Task.Run(() => cloudDrive.LoadNodes());
            };


            buttonStackPanel.Children.Add(headerText);
            buttonStackPanel.Children.Add(fileAscButton);
            buttonStackPanel.Children.Add(fileDescButton);
            buttonStackPanel.Children.Add(sizeAscButton);
            buttonStackPanel.Children.Add(sizeDescButton);
            buttonStackPanel.Children.Add(modificationAscButton);
            buttonStackPanel.Children.Add(modificationDescButton);
            buttonStackPanel.Children.Add(alphaAscButton);
            buttonStackPanel.Children.Add(alphaDescButton);

            var scrollViewer = new ScrollViewer {Content = buttonStackPanel};

            sortRadWindow.Content = scrollViewer;


            sortRadWindow.IsOpen = true;
        }

        public static void ShowPasswordDialog(bool isChange, SettingsViewModel settingsViewModel)
        {
            var openAnimation = new RadMoveAnimation()
            {
                MoveDirection = MoveDirection.TopIn
            };

            var closeAnimation = new RadMoveAnimation()
            {
                MoveDirection = MoveDirection.BottomOut
            };

            var passwordRadWindow = new RadModalWindow()
            {
                IsFullScreen = true,
                Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)),
                WindowSizeMode = WindowSizeMode.FitToPlacementTarget,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                IsAnimationEnabled = true,
                OpenAnimation = openAnimation,
                CloseAnimation = closeAnimation
            };

            var passwordStackPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                Width = Double.NaN,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(12)
            };

            var passwordButtonsGrid = new Grid()
            {
                Width = Double.NaN,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Bottom,
                Background = (SolidColorBrush)Application.Current.Resources["PhoneChromeBrush"]
            };
            passwordButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            passwordButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });


            var titleLabel = new TextBlock()
            {
                Margin = new Thickness(12),
                FontSize = Convert.ToDouble(Application.Current.Resources["PhoneFontSizeLarge"])
            };
            passwordStackPanel.Children.Add(titleLabel);
            
            NumericPasswordBox currentPassword = null;

            if (isChange)
            {
                titleLabel.Text = "Change password";
                currentPassword = new NumericPasswordBox()
                {
                    Watermark = "current password",
                    ClearButtonVisibility = Visibility.Visible
                };
                passwordStackPanel.Children.Add(currentPassword);
            }
            else
            {
                titleLabel.Text = "Make a password";
            }

            var password = new NumericPasswordBox()
            {
                Watermark = UiResources.PasswordWatermark,
                ClearButtonVisibility = Visibility.Visible
            };


            var confirmPassword = new NumericPasswordBox()
            {
                Watermark = UiResources.ConfirmPasswordWatermark,
                ClearButtonVisibility = Visibility.Visible
            };

            var confirmButton = new Button()
            {
                Content = UiResources.DoneButton,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            confirmButton.Tap += (sender, args) =>
            {
                if (isChange)
                {
                    if (currentPassword != null)
                    {
                        string hashValue = CryptoService.HashData(currentPassword.Password);

                        if (!hashValue.Equals(SettingsService.LoadSetting<string>(SettingsResources.UserPassword)))
                        {
                            MessageBox.Show("Current password does not match", "Current password no match",
                                MessageBoxButton.OK);
                            return;
                        }
                    }

                }

                if (password.Password.Length < 4)
                {
                    MessageBox.Show("Password must be at least 4 digits", "Password too short",
                                MessageBoxButton.OK);
                    return;
                }

                if (!password.Password.Equals(confirmPassword.Password))
                {
                    MessageBox.Show(AppMessages.PasswordsDoNotMatch, AppMessages.PasswordsDoNotMatch_Title,
                        MessageBoxButton.OK);
                    return;
                }
               
                SettingsService.SaveSetting(SettingsResources.UserPassword, CryptoService.HashData(password.Password));
                SettingsService.SaveSetting(SettingsResources.UserPasswordIsEnabled, true);

                passwordRadWindow.IsOpen = false;
            };

            var cancelButton = new Button()
            {
                Content = UiResources.CancelButton,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            cancelButton.Tap += (sender, args) =>
            {
                if (!isChange)
                {
                    settingsViewModel.PasswordIsEnabled = false;
                }
                passwordRadWindow.IsOpen = false;
            };


            passwordStackPanel.Children.Add(password);
            passwordStackPanel.Children.Add(confirmPassword);

            passwordButtonsGrid.Children.Add(confirmButton);
            passwordButtonsGrid.Children.Add(cancelButton);
            Grid.SetColumn(confirmButton, 0);
            Grid.SetColumn(cancelButton, 1);

            var grid = new Grid()
            {
                Width = Double.NaN,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            grid.Children.Add(passwordStackPanel);
            grid.Children.Add(passwordButtonsGrid);

            passwordRadWindow.Content = grid;
      
            passwordRadWindow.IsOpen = true;
        }

        public static async Task<int> ShowOptionsDialog(string title, string message, string[] buttons)
        {
            MessageBoxClosedEventArgs closedEventArgs = await RadMessageBox.ShowAsync(
               buttonsContent: buttons,
               title: title,
               message: message
               );
            
            return closedEventArgs.ButtonIndex;
        }
    }
}
