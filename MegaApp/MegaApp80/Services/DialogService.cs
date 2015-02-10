using System;
using System.Collections.Generic;
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

        public static async void ShowOpenLink(MNode publicNode, string link, CloudDriveViewModel cloudDrive, bool isImage = false)
        {
            IEnumerable<string> buttons;

            // Only allows download directly if is an image file
            if (isImage)
                buttons = new string[] { "import", "download" };
            else
                buttons = new string[] { "import" };

            MessageBoxClosedEventArgs closedEventArgs = await RadMessageBox.ShowAsync(
                buttonsContent: buttons,
                title: "Download MEGA link",
                message: publicNode.getName()
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
                        cloudDrive.DownloadLink(publicNode);
                        break;
                    }
            }
        }

        public static async void ShowOverquotaAlert()
        {
            MessageBoxClosedEventArgs closedEventArgs = await RadMessageBox.ShowAsync(
                buttonsContent: new[] { UiResources.Yes.ToLower(), UiResources.No.ToLower() },
                title: AppMessages.OverquotaAlert_Title,
                message: AppMessages.OverquotaAlert
                );

            switch (closedEventArgs.ButtonIndex)
            {
                case 0: // "Yes" button clicked
                    NavigateService.NavigateTo(typeof(MyAccountPage), NavigationParameter.Normal);                    
                    break;

                case 1: // "No" button clicked
                default:
                    break;
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
                Text = "SORT BY",
                HorizontalAlignment = HorizontalAlignment.Left,
                FontSize = (double)Application.Current.Resources["PhoneFontSizeLarge"],
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(20, 30, 20, 20)
            };

            var sortItems = new List<AdvancedMenuItem>();
            sortItems.Add(new AdvancedMenuItem()
            {
                Name = "files (ascending)",
                TapAction = () =>
                {
                    // Needed on every UI interaction
                    App.MegaSdk.retryPendingConnections();

                    sortRadWindow.IsOpen = false;
                    UiService.SetSortOrder(cloudDrive.CurrentRootNode.Handle, (int)MSortOrderType.ORDER_DEFAULT_ASC);
                    Task.Run(() => cloudDrive.LoadNodes());
                }
            });

            sortItems.Add(new AdvancedMenuItem()
            {
                Name = "files (descending)",
                TapAction = () =>
                {
                    // Needed on every UI interaction
                    App.MegaSdk.retryPendingConnections();

                    sortRadWindow.IsOpen = false;
                    UiService.SetSortOrder(cloudDrive.CurrentRootNode.Handle, (int)MSortOrderType.ORDER_DEFAULT_DESC);
                    Task.Run(() => cloudDrive.LoadNodes());
                }
            });

            sortItems.Add(new AdvancedMenuItem()
            {
                Name = "largest",
                TapAction = () =>
                {
                    // Needed on every UI interaction
                    App.MegaSdk.retryPendingConnections();

                    sortRadWindow.IsOpen = false;
                    UiService.SetSortOrder(cloudDrive.CurrentRootNode.Handle, (int)MSortOrderType.ORDER_SIZE_DESC);
                    Task.Run(() => cloudDrive.LoadNodes());
                }
            });

            sortItems.Add(new AdvancedMenuItem()
            {
                Name = "smallest",
                TapAction = () =>
                {
                    // Needed on every UI interaction
                    App.MegaSdk.retryPendingConnections();

                    sortRadWindow.IsOpen = false;
                    UiService.SetSortOrder(cloudDrive.CurrentRootNode.Handle, (int)MSortOrderType.ORDER_SIZE_ASC);
                    Task.Run(() => cloudDrive.LoadNodes());
                }
            });

            sortItems.Add(new AdvancedMenuItem()
            {
                Name = "newest",
                TapAction = () =>
                {
                    // Needed on every UI interaction
                    App.MegaSdk.retryPendingConnections();

                    sortRadWindow.IsOpen = false;
                    UiService.SetSortOrder(cloudDrive.CurrentRootNode.Handle, (int)MSortOrderType.ORDER_MODIFICATION_DESC);
                    Task.Run(() => cloudDrive.LoadNodes());
                }
            });

            sortItems.Add(new AdvancedMenuItem()
            {
                Name = "oldest",
                TapAction = () =>
                {
                    // Needed on every UI interaction
                    App.MegaSdk.retryPendingConnections();

                    sortRadWindow.IsOpen = false;
                    UiService.SetSortOrder(cloudDrive.CurrentRootNode.Handle, (int)MSortOrderType.ORDER_MODIFICATION_ASC);
                    Task.Run(() => cloudDrive.LoadNodes());
                }
            });

            sortItems.Add(new AdvancedMenuItem()
            {
                Name = "name (ascending)",
                TapAction = () =>
                {
                    // Needed on every UI interaction
                    App.MegaSdk.retryPendingConnections();

                    sortRadWindow.IsOpen = false;
                    UiService.SetSortOrder(cloudDrive.CurrentRootNode.Handle, (int)MSortOrderType.ORDER_ALPHABETICAL_ASC);
                    Task.Run(() => cloudDrive.LoadNodes());
                }
            });

            sortItems.Add(new AdvancedMenuItem()
            {
                Name = "name (descending)",
                TapAction = () =>
                {
                    // Needed on every UI interaction
                    App.MegaSdk.retryPendingConnections();

                    sortRadWindow.IsOpen = false;
                    UiService.SetSortOrder(cloudDrive.CurrentRootNode.Handle, (int)MSortOrderType.ORDER_ALPHABETICAL_DESC);
                    Task.Run(() => cloudDrive.LoadNodes());
                }
            });


            var sortList = new RadDataBoundListBox
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                ItemsSource = sortItems,
                Margin = new Thickness(20),
                ItemTemplate = (DataTemplate)Application.Current.Resources["AdvancedMenuItem"],
            };
            ScrollViewer.SetVerticalScrollBarVisibility(sortList, ScrollBarVisibility.Disabled);
            InteractionEffectManager.SetIsInteractionEnabled(sortList, true);
            InteractionEffectManager.AllowedTypes.Add(typeof(RadDataBoundListBoxItem));
            sortList.ItemTap += (sender, args) => ((AdvancedMenuItem) args.Item.DataContext).TapAction.Invoke();
        

            //var fileAscButton = new Button()
            //{
            //    Content = "Files type/name ascending"
            //};
            //fileAscButton.Tap += (sender, args) =>
            //{
            //    sortRadWindow.IsOpen = false;
            //    UiService.SetSortOrder(cloudDrive.CurrentRootNode.Handle, (int) MSortOrderType.ORDER_DEFAULT_ASC);
            //    Task.Run(() => cloudDrive.LoadNodes());
            //};

            //var fileDescButton = new Button()
            //{
            //    Content = "Files type/name descending"
            //};
            //fileDescButton.Tap += (sender, args) =>
            //{
            //    sortRadWindow.IsOpen = false;
            //    UiService.SetSortOrder(cloudDrive.CurrentRootNode.Handle, (int)MSortOrderType.ORDER_DEFAULT_DESC);
            //    Task.Run(() => cloudDrive.LoadNodes());
            //};

            //var sizeAscButton = new Button()
            //{
            //    Content = "Size ascending"
            //};
            //sizeAscButton.Tap += (sender, args) =>
            //{
            //    sortRadWindow.IsOpen = false;
            //    UiService.SetSortOrder(cloudDrive.CurrentRootNode.Handle, (int)MSortOrderType.ORDER_SIZE_ASC);
            //    Task.Run(() => cloudDrive.LoadNodes());
            //};

            //var sizeDescButton = new Button()
            //{
            //    Content = "Size descending"
            //};
            //sizeDescButton.Tap += (sender, args) =>
            //{
            //    sortRadWindow.IsOpen = false;
            //    UiService.SetSortOrder(cloudDrive.CurrentRootNode.Handle, (int)MSortOrderType.ORDER_SIZE_DESC);
            //    Task.Run(() => cloudDrive.LoadNodes());
            //};

            //var modificationAscButton = new Button()
            //{
            //    Content = "Modification date ascending"
            //};
            //modificationAscButton.Tap += (sender, args) =>
            //{
            //    sortRadWindow.IsOpen = false;
            //    UiService.SetSortOrder(cloudDrive.CurrentRootNode.Handle, (int)MSortOrderType.ORDER_MODIFICATION_ASC);
            //    Task.Run(() => cloudDrive.LoadNodes());
            //};

            //var modificationDescButton = new Button()
            //{
            //    Content = "Modification date descending"
            //};
            //modificationDescButton.Tap += (sender, args) =>
            //{
            //    sortRadWindow.IsOpen = false;
            //    UiService.SetSortOrder(cloudDrive.CurrentRootNode.Handle, (int)MSortOrderType.ORDER_MODIFICATION_DESC);
            //    Task.Run(() => cloudDrive.LoadNodes());
            //};

            //var alphaAscButton = new Button()
            //{
            //    Content = "Alphabetical ascending"
            //};
            //alphaAscButton.Tap += (sender, args) =>
            //{
            //    sortRadWindow.IsOpen = false;
            //    UiService.SetSortOrder(cloudDrive.CurrentRootNode.Handle, (int)MSortOrderType.ORDER_ALPHABETICAL_ASC);
            //    Task.Run(() => cloudDrive.LoadNodes());
            //};

            //var alphaDescButton = new Button()
            //{
            //    Content = "Alphabetical descending"
            //};
            //alphaDescButton.Tap += (sender, args) =>
            //{
            //    sortRadWindow.IsOpen = false;
            //    UiService.SetSortOrder(cloudDrive.CurrentRootNode.Handle, (int)MSortOrderType.ORDER_ALPHABETICAL_DESC);
            //    Task.Run(() => cloudDrive.LoadNodes());
            //};


            buttonStackPanel.Children.Add(headerText);
            buttonStackPanel.Children.Add(sortList);
            //buttonStackPanel.Children.Add(fileAscButton);
            //buttonStackPanel.Children.Add(fileDescButton);
            //buttonStackPanel.Children.Add(sizeAscButton);
            //buttonStackPanel.Children.Add(sizeDescButton);
            //buttonStackPanel.Children.Add(modificationAscButton);
            //buttonStackPanel.Children.Add(modificationDescButton);
            //buttonStackPanel.Children.Add(alphaAscButton);
            //buttonStackPanel.Children.Add(alphaDescButton);

            //var scrollViewer = new ScrollViewer {Content = buttonStackPanel};

            sortRadWindow.Content = buttonStackPanel;


            sortRadWindow.IsOpen = true;
        }

        public static void ShowPinLockDialog(bool isChange, SettingsViewModel settingsViewModel)
        {
            var openAnimation = new RadMoveAnimation()
            {
                MoveDirection = MoveDirection.TopIn
            };

            var closeAnimation = new RadMoveAnimation()
            {
                MoveDirection = MoveDirection.BottomOut
            };

            var pinLockRadWindow = new RadModalWindow()
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

            var pinLockStackPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                Width = Double.NaN,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(12)
            };

            var pinLockButtonsGrid = new Grid()
            {
                Width = Double.NaN,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Bottom,
                Background = (SolidColorBrush)Application.Current.Resources["PhoneChromeBrush"]
            };
            pinLockButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            pinLockButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });


            var titleLabel = new TextBlock()
            {
                Margin = new Thickness(12),
                FontSize = Convert.ToDouble(Application.Current.Resources["PhoneFontSizeLarge"])
            };
            pinLockStackPanel.Children.Add(titleLabel);
            
            NumericPasswordBox currentPinLock = null;

            if (isChange)
            {
                titleLabel.Text = "Change PIN Lock";
                currentPinLock = new NumericPasswordBox()
                {
                    Watermark = "current PIN Lock",
                    ClearButtonVisibility = Visibility.Visible
                };
                pinLockStackPanel.Children.Add(currentPinLock);
            }
            else
            {
                titleLabel.Text = "Make a PIN Lock";
            }

            var pinLock = new NumericPasswordBox()
            {
                Watermark = UiResources.PinLockWatermark,
                ClearButtonVisibility = Visibility.Visible
            };


            var confirmPinLock = new NumericPasswordBox()
            {
                Watermark = UiResources.ConfirmPinLockWatermark,
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
                    if (currentPinLock != null)
                    {
                        string hashValue = CryptoService.HashData(currentPinLock.Password);

                        if (!hashValue.Equals(SettingsService.LoadSetting<string>(SettingsResources.UserPinLock)))
                        {
                            MessageBox.Show("Current PIN lock does not match", "Current PIN lock no match",
                                MessageBoxButton.OK);
                            return;
                        }
                    }

                }

                if (pinLock.Password.Length < 4)
                {
                    MessageBox.Show("PIN lock must be at least 4 digits", "PIN lock too short",
                                MessageBoxButton.OK);
                    return;
                }

                if (!pinLock.Password.Equals(confirmPinLock.Password))
                {
                    MessageBox.Show(AppMessages.PinLockCodesDoNotMatch, AppMessages.PinLockCodesDoNotMatch_Title,
                        MessageBoxButton.OK);
                    return;
                }
               
                SettingsService.SaveSetting(SettingsResources.UserPinLock, CryptoService.HashData(pinLock.Password));
                SettingsService.SaveSetting(SettingsResources.UserPinLockIsEnabled, true);

                pinLockRadWindow.IsOpen = false;
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
                    settingsViewModel.PinLockIsEnabled = false;                    
                }
                pinLockRadWindow.IsOpen = false;
            };


            pinLockStackPanel.Children.Add(pinLock);
            pinLockStackPanel.Children.Add(confirmPinLock);

            pinLockButtonsGrid.Children.Add(confirmButton);
            pinLockButtonsGrid.Children.Add(cancelButton);
            Grid.SetColumn(confirmButton, 0);
            Grid.SetColumn(cancelButton, 1);

            var grid = new Grid()
            {
                Width = Double.NaN,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            grid.Children.Add(pinLockStackPanel);
            grid.Children.Add(pinLockButtonsGrid);

            pinLockRadWindow.Content = grid;
      
            pinLockRadWindow.IsOpen = true;
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

        public static async void ShowViewMasterKey(string masterkey, Action copyAction)
        {
            MessageBoxClosedEventArgs closedEventArgs = await RadMessageBox.ShowAsync(
                buttonsContent: new string[] { UiResources.CopyButton, UiResources.CancelButton },
                title: "MasterKey",
                message: masterkey
                );

            switch (closedEventArgs.ButtonIndex)
            {
                // Share button clicked
                case 0:
                    {
                        copyAction.Invoke();
                        break;
                    }
            }
        }
    }
}
