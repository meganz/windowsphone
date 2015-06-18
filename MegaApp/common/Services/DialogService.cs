using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Pages;
using MegaApp.Resources;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Telerik.Windows.Controls;
#if WINDOWS_PHONE_81

#endif

namespace MegaApp.Services
{
    static class DialogService
    {
        public static async void ShowShareLink(string link)
        {
            var buttonsDataTemplate = (DataTemplate)Application.Current.Resources["ShowShareLinkButtons"];
            MessageBoxClosedEventArgs closedEventArgs = await RadMessageBox.ShowAsync(
                buttonsDataTemplate,
                buttonsContent: new[] { UiResources.Share.ToLower(), UiResources.Copy.ToLower(), UiResources.Cancel.ToLower() },
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

        #if WINDOWS_PHONE_80
        public static async void ShowOpenLink(MNode publicNode, string link, FolderViewModel folderViewModel, bool isImage = false)
        {
            IEnumerable<string> buttons;

            // Only allows download directly if is an image file
            if (isImage)
                buttons = new string[] { UiResources.Import.ToLower(), UiResources.Download.ToLower() };
            else
                buttons = new string[] { UiResources.Import.ToLower() };

            MessageBoxClosedEventArgs closedEventArgs = await RadMessageBox.ShowAsync(
                buttonsContent: buttons,
                title: UiResources.LinkOptions,
                message: publicNode.getName()
                );

            switch (closedEventArgs.ButtonIndex)
            {
                case 0: // Import button clicked
                    folderViewModel.ImportLink(link);
                    break;

                case 1: // Download button clicked
                    folderViewModel.DownloadLink(publicNode);
                    break;
            }
        }
        #elif WINDOWS_PHONE_81
        public static async void ShowOpenLink(MNode publicNode, string link, FolderViewModel folderViewModel)
        {
            MessageBoxClosedEventArgs closedEventArgs = await RadMessageBox.ShowAsync(
                buttonsContent: new [] { UiResources.Import.ToLower(), UiResources.Download.ToLower() },
                title: UiResources.LinkOptions,
                message: publicNode.getName()
                );

            switch (closedEventArgs.ButtonIndex)
            {
                // Import button clicked
                case 0:
                {
                    folderViewModel.ImportLink(link);
                    break;
                }
                case 1: // Download button clicked
                {
                    folderViewModel.DownloadLink(publicNode);
                    break;
                }
            }
        }
        #endif

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
                    (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(new Uri("/Pages/MyAccountPage.xaml?Pivot=1", UriKind.RelativeOrAbsolute));
                    break;

                case 1: // "No" button clicked
                default:
                    break;
            }
        }

        public static void ShowUploadOptions(FolderViewModel folder)
        {
            if (folder.AppInformation.PickerOrAsyncDialogIsOpen) return;

            App.CloudDrive.CurrentRootNode = (NodeViewModel)folder.FolderRootNode;
            folder.AppInformation.PickerOrAsyncDialogIsOpen = true;

            var uploadRadWindow = new RadModalWindow()
            {
                IsFullScreen = true,
                Background = new SolidColorBrush((Color)Application.Current.Resources["PhoneChromeColor"]),
                WindowSizeMode = WindowSizeMode.FitToPlacementTarget,
                HorizontalContentAlignment= HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Top,
                OpenAnimation = AnimationService.GetOpenDialogAnimation(),
                Margin = new Thickness(0)
            };

            uploadRadWindow.WindowClosed += (sender, args) => folder.AppInformation.PickerOrAsyncDialogIsOpen = false;

            var grid = new Grid()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Background = new SolidColorBrush((Color)Application.Current.Resources["PhoneChromeColor"]),
                Margin = new Thickness(24, 0, 24, 0)
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = new GridLength(1, GridUnitType.Auto)
            });
            grid.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = new GridLength(1, GridUnitType.Auto)
            });
            grid.RowDefinitions.Add(new RowDefinition()
            {
                Height = GridLength.Auto
            });
            grid.RowDefinitions.Add(new RowDefinition()
            {
                Height = GridLength.Auto
            });
            grid.RowDefinitions.Add(new RowDefinition()
            {
                Height = GridLength.Auto
            });

            var headerText = new TextBlock()
            {
                Text = UiResources.UploadOptionsTitle.ToUpper(),
                HorizontalAlignment = HorizontalAlignment.Left,
                FontSize = (double)Application.Current.Resources["PhoneFontSizeMedium"],
                FontFamily = new FontFamily("Segoe WP SemiBold"),
                Margin = new Thickness(-1, 16, 0, 14)
            };
            
            grid.Children.Add(headerText);
            Grid.SetColumn(headerText, 0);
            Grid.SetColumnSpan(headerText, 2);
            Grid.SetRow(headerText, 0);

            var hubCamera = UiService.CreateHubTile(UiResources.TakePhoto, 
                new Uri("/Assets/Images/take photos" + ImageService.GetResolutionExtension() + ".png", UriKind.Relative),
                new Thickness(0, 0, 12, 12));
           
            hubCamera.Tap += (sender, args) =>
            {
                uploadRadWindow.IsOpen = false;
                MediaService.CaptureCameraImage(folder);
            };

            var hubSelfie = UiService.CreateHubTile(UiResources.SelfieMode, 
                new Uri("/Assets/Images/selfie_upload" + ImageService.GetResolutionExtension() + ".png", UriKind.Relative),
                new Thickness(0, 0, 0, 12));
           
            hubSelfie.Tap += (sender, args) =>
            {
                uploadRadWindow.IsOpen = false;
                NavigateService.NavigateTo(typeof(PhotoCameraPage), NavigationParameter.Normal);
            };

            #if WINDOWS_PHONE_80
            var hubPicture = UiService.CreateHubTile(UiResources.ImageUpload,
                new Uri("/Assets/Images/picture_upload" + ImageService.GetResolutionExtension() + ".png", UriKind.Relative),
                new Thickness(0, 0, 12, 0));            
            
            hubPicture.Tap += (sender, args) =>
            {
                uploadRadWindow.IsOpen = false;
                NavigateService.NavigateTo(typeof(MediaSelectionPage), NavigationParameter.Normal);
            };
            #elif WINDOWS_PHONE_81
            var hubFile = UiService.CreateHubTile(UiResources.FileUpload, 
                new Uri("/Assets/Images/file upload" +ImageService.GetResolutionExtension() + ".png", UriKind.Relative), 
                new Thickness(0, 0, 12, 0));
           
            hubFile.Tap += (sender, args) =>
            {
                uploadRadWindow.IsOpen = false;
                App.FileOpenOrFolderPickerOpenend = true;
                FileService.SelectMultipleFiles();
            };
            #endif

            grid.Children.Add(hubCamera);
            grid.Children.Add(hubSelfie);
            #if WINDOWS_PHONE_80
                grid.Children.Add(hubPicture);
            #elif WINDOWS_PHONE_81
            grid.Children.Add(hubFile);
            #endif
         
            Grid.SetColumn(hubCamera, 0);
            Grid.SetColumn(hubSelfie, 1);
            #if WINDOWS_PHONE_80
                Grid.SetColumn(hubPicture, 0);
            #elif WINDOWS_PHONE_81
            Grid.SetColumn(hubFile, 0);
            #endif
           
            Grid.SetRow(hubCamera, 1);
            Grid.SetRow(hubSelfie, 1);
            #if WINDOWS_PHONE_80
                Grid.SetRow(hubPicture, 2);
            #elif WINDOWS_PHONE_81
            Grid.SetRow(hubFile, 2);
            #endif
         
            uploadRadWindow.Content = grid;

            uploadRadWindow.IsOpen = true;
        }

        public static void ShowSortDialog(FolderViewModel folder)
        {
            // If rootnode is not determined yet. Do nothing
            if (folder.FolderRootNode == null) return;

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
                Background = new SolidColorBrush((Color)Application.Current.Resources["PhoneChromeColor"])
            };

            var headerText = new TextBlock()
            {
                Text = UiResources.SortByMenuTitle.ToUpper(),
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
                    UiService.SetSortOrder(folder.FolderRootNode.Handle, (int)MSortOrderType.ORDER_DEFAULT_ASC);
                    Task.Run(() => folder.LoadChildNodes());
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
                    UiService.SetSortOrder(folder.FolderRootNode.Handle, (int)MSortOrderType.ORDER_DEFAULT_DESC);
                    Task.Run(() => folder.LoadChildNodes());
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
                    UiService.SetSortOrder(folder.FolderRootNode.Handle, (int)MSortOrderType.ORDER_SIZE_DESC);
                    Task.Run(() => folder.LoadChildNodes());
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
                    UiService.SetSortOrder(folder.FolderRootNode.Handle, (int)MSortOrderType.ORDER_SIZE_ASC);
                    Task.Run(() => folder.LoadChildNodes());
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
                    UiService.SetSortOrder(folder.FolderRootNode.Handle, (int)MSortOrderType.ORDER_MODIFICATION_DESC);
                    Task.Run(() => folder.LoadChildNodes());
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
                    UiService.SetSortOrder(folder.FolderRootNode.Handle, (int)MSortOrderType.ORDER_MODIFICATION_ASC);
                    Task.Run(() => folder.LoadChildNodes());
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
                    UiService.SetSortOrder(folder.FolderRootNode.Handle, (int)MSortOrderType.ORDER_ALPHABETICAL_ASC);
                    Task.Run(() => folder.LoadChildNodes());
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
                    UiService.SetSortOrder(folder.FolderRootNode.Handle, (int)MSortOrderType.ORDER_ALPHABETICAL_DESC);
                    Task.Run(() => folder.LoadChildNodes());
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
            //    UiService.SetSortOrder(folder.FolderRootNode.Handle, (int) MSortOrderType.ORDER_DEFAULT_ASC);
            //    Task.Run(() => folder.LoadChildNodes());
            //};

            //var fileDescButton = new Button()
            //{
            //    Content = "Files type/name descending"
            //};
            //fileDescButton.Tap += (sender, args) =>
            //{
            //    sortRadWindow.IsOpen = false;
            //    UiService.SetSortOrder(folder.FolderRootNode.Handle, (int)MSortOrderType.ORDER_DEFAULT_DESC);
            //    Task.Run(() => folder.LoadChildNodes());
            //};

            //var sizeAscButton = new Button()
            //{
            //    Content = "Size ascending"
            //};
            //sizeAscButton.Tap += (sender, args) =>
            //{
            //    sortRadWindow.IsOpen = false;
            //    UiService.SetSortOrder(folder.FolderRootNode.Handle, (int)MSortOrderType.ORDER_SIZE_ASC);
            //    Task.Run(() => folder.LoadChildNodes());
            //};

            //var sizeDescButton = new Button()
            //{
            //    Content = "Size descending"
            //};
            //sizeDescButton.Tap += (sender, args) =>
            //{
            //    sortRadWindow.IsOpen = false;
            //    UiService.SetSortOrder(folder.FolderRootNode.Handle, (int)MSortOrderType.ORDER_SIZE_DESC);
            //    Task.Run(() => folder.LoadChildNodes());
            //};

            //var modificationAscButton = new Button()
            //{
            //    Content = "Modification date ascending"
            //};
            //modificationAscButton.Tap += (sender, args) =>
            //{
            //    sortRadWindow.IsOpen = false;
            //    UiService.SetSortOrder(folder.FolderRootNode.Handle, (int)MSortOrderType.ORDER_MODIFICATION_ASC);
            //    Task.Run(() => folder.LoadChildNodes());
            //};

            //var modificationDescButton = new Button()
            //{
            //    Content = "Modification date descending"
            //};
            //modificationDescButton.Tap += (sender, args) =>
            //{
            //    sortRadWindow.IsOpen = false;
            //    UiService.SetSortOrder(folder.FolderRootNode.Handle, (int)MSortOrderType.ORDER_MODIFICATION_DESC);
            //    Task.Run(() => folder.LoadChildNodes());
            //};

            //var alphaAscButton = new Button()
            //{
            //    Content = "Alphabetical ascending"
            //};
            //alphaAscButton.Tap += (sender, args) =>
            //{
            //    sortRadWindow.IsOpen = false;
            //    UiService.SetSortOrder(folder.FolderRootNode.Handle, (int)MSortOrderType.ORDER_ALPHABETICAL_ASC);
            //    Task.Run(() => folder.LoadChildNodes());
            //};

            //var alphaDescButton = new Button()
            //{
            //    Content = "Alphabetical descending"
            //};
            //alphaDescButton.Tap += (sender, args) =>
            //{
            //    sortRadWindow.IsOpen = false;
            //    UiService.SetSortOrder(folder.FolderRootNode.Handle, (int)MSortOrderType.ORDER_ALPHABETICAL_DESC);
            //    Task.Run(() => folder.LoadChildNodes());
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
                Background = new SolidColorBrush((Color)Application.Current.Resources["PhoneChromeColor"])
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
                titleLabel.Text = UiResources.ChangePinLock;
                currentPinLock = new NumericPasswordBox()
                {
                    Watermark = UiResources.PinLockWatermark.ToLower(),
                    ClearButtonVisibility = Visibility.Visible
                };
                pinLockStackPanel.Children.Add(currentPinLock);
            }
            else
            {
                titleLabel.Text = UiResources.MakePinLock;
            }

            var pinLock = new NumericPasswordBox()
            {
                Watermark = UiResources.NewPinLockWatermark.ToLower(),
                ClearButtonVisibility = Visibility.Visible
            };


            var confirmPinLock = new NumericPasswordBox()
            {
                Watermark = UiResources.ConfirmPinLockWatermark.ToLower(),
                ClearButtonVisibility = Visibility.Visible
            };

            var confirmButton = new Button()
            {
                Content = UiResources.Done.ToLower(),
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
                            MessageBox.Show(AppMessages.CurrentPinLockCodeDoNotMatch, 
                                AppMessages.CurrentPinLockCodeDoNotMatch_Title, MessageBoxButton.OK);
                            return;
                        }
                    }

                }

                if (pinLock.Password.Length < 4)
                {
                    MessageBox.Show(AppMessages.PinLockTooShort, AppMessages.PinLockTooShort_Title,
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
                Content = UiResources.Cancel.ToLower(),
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
                buttonsContent: new string[] { UiResources.Copy.ToLower(), UiResources.Cancel.ToLower() },
                title: UiResources.MasterKey,
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

        public static void ShowInputDialog(FolderViewModel folder)
        {
            //if (folder.FolderRootNode == null) return;

            //var inputWindow = new RadModalWindow()
            //{
            //    IsFullScreen = true,
            //    Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)),
            //    WindowSizeMode = WindowSizeMode.FitToPlacementTarget,
            //    HorizontalContentAlignment = HorizontalAlignment.Stretch,
            //    VerticalContentAlignment = VerticalAlignment.Stretch,
            //    ShouldHandleApplicationBar = true
            //};

        }
    }
}
