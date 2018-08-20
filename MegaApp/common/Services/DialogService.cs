using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using Telerik.Windows.Controls;
using Telerik.Windows.Data;
using mega;
using MegaApp.Classes;
using MegaApp.Dialogs;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Resources;
using MegaApp.UserControls;
using MegaApp.ViewModels;
using MegaApp.Views;

namespace MegaApp.Services
{
    static class DialogService
    {
        #region Properties

        /// <summary>
        /// Instance of the MFA code input dialog displayed
        /// </summary>
        private static MultiFactorAuthCodeInputDialog MultiFactorAuthCodeInputDialogInstance;

        #endregion

        #region Methods

        /// <summary>
        /// Shows a dialog to allow copy a node link to the clipboard or share it using other app
        /// </summary>
        /// <param name="node">Node to share the link</param>
        public static void ShowShareLink(NodeViewModel node)
        {
            var dialog = new RadModalWindow()
            {
                IsFullScreen = false,
                Background = new SolidColorBrush(Color.FromArgb(155, 31, 31, 31)),
                WindowSizeMode = WindowSizeMode.FitToPlacementTarget,
                VerticalContentAlignment = VerticalAlignment.Top,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                IsAnimationEnabled = true,
                IsClosedOnOutsideTap = false,
                OpenAnimation = AnimationService.GetOpenMessageDialogAnimation(),
                CloseAnimation = AnimationService.GetCloseMessageDialogAnimation()
            };

            dialog.WindowOpening += (sender, args) => DialogOpening(args);
            dialog.WindowClosed += (sender, args) => DialogClosed();

            var mainGrid = new Grid()
            {
                Background = new SolidColorBrush((Color)Application.Current.Resources["PhoneChromeColor"]),                
                Width = Double.NaN,
                Margin = new Thickness(24, 0, 24, 0),
                RowDefinitions =
                {
                    new RowDefinition() { Height = GridLength.Auto}, // Title row
                    new RowDefinition() { Height = GridLength.Auto}, // Content row
                    new RowDefinition() { Height = GridLength.Auto}, // Buttons row
                }
            };

            // Create title label
            var title = new TextBlock()
            {
                Text = UiResources.MegaLinkTitle.ToUpper(),
                FontFamily = new FontFamily("Segoe WP Semibold"),
                FontSize = Convert.ToDouble(Application.Current.Resources["PhoneFontSizeMedium"]),
                Foreground = new SolidColorBrush((Color)Application.Current.Resources["PhoneForegroundColor"]),
                Margin = new Thickness(0, 24, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                TextWrapping = TextWrapping.Wrap
            };

            // Add title to the view
            mainGrid.Children.Add(title);
            Grid.SetRow(title, 0);

            var stackPanel = new StackPanel
            {
                Margin = new Thickness(0, 20, 0, 0)
            };

            var messageText = new TextBlock
            {
                Text = node.OriginalMNode.getPublicLink(true),
                Margin = new Thickness(0, 20, 0, 12),
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            var linkWithoutKey = new RadioButton
            {
                Content = Resources.UiResources.UI_LinkWithoutKey,
                Margin = new Thickness(0, -12, 0, 0)
            };
            linkWithoutKey.Checked += (sender, args) => messageText.Text = node.OriginalMNode.getPublicLink(false);

            var decryptionKey = new RadioButton
            {
                Content = Resources.UiResources.UI_DecryptionKey,
                Margin = new Thickness(0, -12, 0, 0)
            };
            decryptionKey.Checked += (sender, args) => messageText.Text = node.OriginalMNode.getBase64Key();

            var linkWithKey = new RadioButton
            {
                Content = Resources.UiResources.UI_LinkWithKey,
                Margin = new Thickness(0, -12, 0, 0),
                IsChecked = true
            };
            linkWithKey.Checked += (sender, args) => messageText.Text = node.OriginalMNode.getPublicLink(true);

            stackPanel.Children.Add(linkWithoutKey);
            stackPanel.Children.Add(decryptionKey);
            stackPanel.Children.Add(linkWithKey);

            var stackPanelLinkWithExpirationDate = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            var linkWithExpirationDateLabel = new TextBlock
            {
                Text = string.Format("{0} {1}", Resources.UiResources.UI_SetExpirationDate, Resources.UiResources.UI_ProOnly),
                Margin = new Thickness(0, 20, 0, 8),
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            var enableLinkExpirationDateSwitch = new RadToggleSwitch
            {
                IsEnabled = AccountService.AccountDetails.IsProAccount,
                IsChecked = node.LinkWithExpirationTime,
                VerticalAlignment = VerticalAlignment.Center
            };

            var expirationDatePicker = new RadDatePicker
            {
                IsEnabled = enableLinkExpirationDateSwitch.IsChecked && AccountService.AccountDetails.IsProAccount,
                DisplayValueFormat = "dd‎/MM‎/yyyy",
                SelectorFormat = "D/M/Y",
                Value = node.LinkExpirationDate,
                MinValue = DateTime.Today.AddDays(1),
                VerticalAlignment = VerticalAlignment.Center,
                OkButtonText = Resources.UiResources.Accept.ToLower(),
                OkButtonIconUri = new Uri("/Assets/AppBar/check.png", UriKind.Relative),
                CancelButtonText = Resources.UiResources.Cancel.ToLower(),
                CancelButtonIconUri = new Uri("/Assets/AppBar/cancel.png", UriKind.Relative)
            };
            expirationDatePicker.PopupClosed += (sender, args) =>
            {
                if (expirationDatePicker.Value == null)
                    enableLinkExpirationDateSwitch.IsChecked = false;
            };
            expirationDatePicker.ValueChanged += (sender, args) =>
            {
                if (expirationDatePicker.Value == null)
                {
                    enableLinkExpirationDateSwitch.IsChecked = false;
                    if (node.LinkExpirationTime > 0)
                        node.SetLinkExpirationTime(0);
                }
                else if (node.LinkExpirationDate == null ||
                    !node.LinkExpirationDate.Value.Date.ToUniversalTime().Equals(expirationDatePicker.Value.Value.Date.ToUniversalTime()))
                {
                    DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    DateTime select = expirationDatePicker.Value.Value;
                    TimeSpan diff = select.Date.ToUniversalTime() - origin.Date.ToUniversalTime();                    
                    node.SetLinkExpirationTime((long)Math.Floor(diff.TotalSeconds));
                }
            };

            enableLinkExpirationDateSwitch.CheckedChanged += (sender, args) =>
            {
                expirationDatePicker.IsEnabled = enableLinkExpirationDateSwitch.IsChecked;
                if (enableLinkExpirationDateSwitch.IsChecked)
                    expirationDatePicker.Value = node.LinkExpirationDate;
                else
                    expirationDatePicker.Value = null;
            };

            stackPanelLinkWithExpirationDate.Children.Add(enableLinkExpirationDateSwitch);
            stackPanelLinkWithExpirationDate.Children.Add(expirationDatePicker);

            stackPanel.Children.Add(linkWithExpirationDateLabel);
            stackPanel.Children.Add(stackPanelLinkWithExpirationDate);
            stackPanel.Children.Add(messageText);

            // Add content to the view
            mainGrid.Children.Add(stackPanel);
            Grid.SetRow(stackPanel, 1);

            // Create response controls panel
            var buttonsGrid = new Grid()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(-12, 0, -12, 24)
            };
            buttonsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            buttonsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            mainGrid.Children.Add(buttonsGrid);
            Grid.SetRow(buttonsGrid, 2);

            var copyButton = new Button()
            {
                Content = UiResources.Copy.ToLower(),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            copyButton.Tap += (sender, args) =>
            {
                dialog.IsOpen = false;
                try
                {
                    Clipboard.SetText(messageText.Text);
                    new CustomMessageDialog(
                        AppMessages.LinkCopiedToClipboard_Title,
                        AppMessages.LinkCopiedToClipboard,
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                }
                catch (Exception)
                {
                    new CustomMessageDialog(
                        AppMessages.AM_CopyLinkToClipboardFailed_Title,
                        AppMessages.AM_CopyLinkToClipboardFailed,
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                }
            };

            var shareButton = new Button()
            {
                Content = UiResources.Share.ToLower(),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            shareButton.Tap += (sender, args) =>
            {
                dialog.IsOpen = false;
                try
                {
                    var shareLinkTask = new ShareLinkTask
                    {
                        LinkUri = new Uri(messageText.Text),
                        Title = UiResources.MegaShareLinkMessage
                    };
                    shareLinkTask.Show();
                }
                catch(Exception e)
                {
                    LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error sharing a file/folder link", e);
                    new CustomMessageDialog(
                        AppMessages.AM_ShareLinkFailed_Title,
                        AppMessages.AM_ShareLinkFailed,
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                }
            };

            buttonsGrid.Children.Add(copyButton);
            buttonsGrid.Children.Add(shareButton);
            Grid.SetColumn(copyButton, 0);
            Grid.SetColumn(shareButton, 1);

            // Used to color the dialog content backgroud
            var border = new Border
            {
                Background = new SolidColorBrush((Color)Application.Current.Resources["PhoneChromeColor"]),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Child = mainGrid,
            };

            dialog.Content = border;

            dialog.IsOpen = true;
        }

        #if WINDOWS_PHONE_80
        public static void ShowOpenLink(MNode publicNode, FolderViewModel folderViewModel, bool isImage = false)
        {
            // Check if a "folderviewmodel" and "publicNode" is available
            if (folderViewModel == null) throw new ArgumentNullException("folderViewModel");
            if (publicNode == null) throw new ArgumentNullException("publicNode");

            IEnumerable<DialogButton> buttons;

            // Only allows download directly if is an image file
            if (isImage)
            {
                buttons = new[]
                {
                    new DialogButton(UiResources.Import, () => App.MainPageViewModel.SetImportMode()),
                    new DialogButton(UiResources.Download, () => folderViewModel.DownloadLink(publicNode))
                };
            }                
            else
            {
                buttons = new[]
                {
                    new DialogButton(UiResources.Import, () => App.MainPageViewModel.SetImportMode())
                };
            }                

            new CustomMessageDialog(GetShowOpenLinkTitle(publicNode),
                publicNode.getName(), App.AppInformation, buttons).ShowDialog();
        }
        #elif WINDOWS_PHONE_81
        public static void ShowOpenLink(MNode publicNode, FolderViewModel folderViewModel)
        {
            // Check if a "folderviewmodel" and "publicNode" is available
            if (folderViewModel == null) throw new ArgumentNullException("folderViewModel");
            if (publicNode == null) throw new ArgumentNullException("publicNode");

            var customMessageDialog = new CustomMessageDialog(GetShowOpenLinkTitle(publicNode), 
                publicNode.getName(), App.AppInformation,
                new[]
                {
                    new DialogButton(UiResources.Import, () => App.MainPageViewModel.SetImportMode()),
                    new DialogButton(UiResources.Download, () => folderViewModel.DownloadLink(publicNode))
                });

            customMessageDialog.ShowDialog();
        }
        #endif

        private static String GetShowOpenLinkTitle(MNode publicNode)
        {
            switch (publicNode.getType())
            {
                case MNodeType.TYPE_FILE:
                    return UiResources.UI_FileLink;
                case MNodeType.TYPE_FOLDER:
                    return UiResources.UI_FolderLink;
                default:
                    return UiResources.LinkOptions;
            }
        }

        public static void ShowOverquotaAlert()
        {
            var customMessageDialog = new CustomMessageDialog(AppMessages.OverquotaAlert_Title,
                AppMessages.OverquotaAlert, App.AppInformation, MessageDialogButtons.YesNo);
            
            customMessageDialog.OkOrYesButtonTapped += (sender, args) =>
            {
                ((PhoneApplicationFrame) Application.Current.RootVisual).Navigate(
                    new Uri("/Views/MyAccountPage.xaml?Pivot=1", UriKind.RelativeOrAbsolute));
            };

            customMessageDialog.ShowDialog();
        }

        public static void ShowTransferOverquotaWarning()
        {
            var upgradeAccountButton = new DialogButton(
                UiResources.UI_UpgradeAccount, () =>
                {
                    ((PhoneApplicationFrame)Application.Current.RootVisual).Navigate(
                        new Uri("/Views/MyAccountPage.xaml?Pivot=1", UriKind.RelativeOrAbsolute));
                });

            var customMessageDialog = new CustomMessageDialog(AppMessages.AM_TransferOverquotaWarning_Title,
                AppMessages.AM_TransferOverquotaWarning, App.AppInformation,
                new[] { upgradeAccountButton, new DialogButton(UiResources.Dismiss, null) });

            customMessageDialog.ShowDialog();
        }

        /// <summary>
        /// Show a SSL Key error alert and gives the user several options
        /// </summary>
        /// <param name="api">Current SDK instance</param>
        /// <returns>Message dialog result specified by user button tap action</returns>
        public static async Task<MessageDialogResult> ShowSSLKeyErrorAlertAsync(MegaSDK api)
        {
            // "Retry" button
            var retryButton = new DialogButton(
                UiResources.UI_Retry, () => api.reconnect());

            // "Open browser" button
            var openBrowserButton = new DialogButton(
                UiResources.UI_OpenBrowser, () =>
                {
                    var webBrowserTask = new WebBrowserTask { Uri = new Uri(AppResources.AR_MegaUrl) };
                    webBrowserTask.Show();
                });

            // "Ignore" button
            var ignoreButton = new DialogButton(
                UiResources.Ignore, () =>
                {
                    api.setPublicKeyPinning(false);
                    api.reconnect();
                });

            var customMessageDialog = new CustomMessageDialog(
                AppMessages.AM_SSLKeyError_Title,
                AppMessages.AM_SSLKeyError, App.AppInformation,
                new[] { retryButton, openBrowserButton, ignoreButton },
                Orientation.Vertical);

            return await customMessageDialog.ShowDialogAsync();
        }

        public static void DialogOpening(CancelEventArgs args)
        {
            // Do not show dialog when another dialog is already open
            if (App.AppInformation.PickerOrAsyncDialogIsOpen)
            {
                args.Cancel = true;
                return;
            }

            // Needed to only display 1 dialog at a time and to check for in back button press event
            // on the page where the dialog is used to cancel other back button logic
            App.AppInformation.PickerOrAsyncDialogIsOpen = true;
        }

        public static void DialogClosed()
        {
            // When the dialog is closed and finished remove this helper property
            App.AppInformation.PickerOrAsyncDialogIsOpen = false;
        }

        public static void ShowUploadOptions(FolderViewModel folder)
        {
            if (App.CloudDrive == null)
                App.CloudDrive = new CloudDriveViewModel(SdkService.MegaSdk, App.AppInformation);

            bool error = false;            
            try
            {
                if (folder != null && folder.FolderRootNode != null)
                {
                    App.CloudDrive.CurrentRootNode = (NodeViewModel)folder.FolderRootNode;

                    var uploadRadWindow = new RadModalWindow()
                    {
                        IsFullScreen = true,
                        Background = new SolidColorBrush((Color)Application.Current.Resources["PhoneChromeColor"]),
                        WindowSizeMode = WindowSizeMode.FitToPlacementTarget,
                        HorizontalContentAlignment = HorizontalAlignment.Stretch,
                        VerticalContentAlignment = VerticalAlignment.Top,
                        IsAnimationEnabled = true,
                        OpenAnimation = AnimationService.GetOpenDialogAnimation(),
                        CloseAnimation = AnimationService.GetCloseDialogAnimation(),
                        Margin = new Thickness(0)
                    };

                    uploadRadWindow.WindowOpening += (sender, args) => DialogOpening(args);
                    uploadRadWindow.WindowClosed += (sender, args) => DialogClosed();

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
                else
                {
                    error = true;
                    LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error opening the upload options dialog");
                }
                
            }
            catch(Exception e)
            {
                error = true;
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error opening the upload options dialog", e);
            }
            finally
            {
                if(error)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        new CustomMessageDialog(
                            AppMessages.AM_ShowUploadOptionsFailed_Title.ToUpper(),
                            AppMessages.AM_ShowUploadOptionsFailed,
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialog();
                    });
                }
            }
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
                IsAnimationEnabled = true,
                OpenAnimation = AnimationService.GetOpenDialogAnimation(),
                CloseAnimation = AnimationService.GetCloseDialogAnimation()
            };

            sortRadWindow.WindowOpening += (sender, args) => DialogOpening(args);
            sortRadWindow.WindowClosed += (sender, args) => DialogClosed();

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
                Name = UiResources.FilesAscendingSortOption.ToLower(),
                TapAction = () =>
                {
                    // Needed on every UI interaction
                    SdkService.MegaSdk.retryPendingConnections();

                    sortRadWindow.IsOpen = false;
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle, (int)MSortOrderType.ORDER_DEFAULT_ASC);
                    Task.Run(() => folder.LoadChildNodes());
                }
            });

            sortItems.Add(new AdvancedMenuItem()
            {
                Name = UiResources.FilesDescendingSortOption.ToLower(),
                TapAction = () =>
                {
                    // Needed on every UI interaction
                    SdkService.MegaSdk.retryPendingConnections();

                    sortRadWindow.IsOpen = false;
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle, (int)MSortOrderType.ORDER_DEFAULT_DESC);
                    Task.Run(() => folder.LoadChildNodes());
                }
            });

            sortItems.Add(new AdvancedMenuItem()
            {
                Name = UiResources.LargestSortOption.ToLower(),
                TapAction = () =>
                {
                    // Needed on every UI interaction
                    SdkService.MegaSdk.retryPendingConnections();

                    sortRadWindow.IsOpen = false;
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle, (int)MSortOrderType.ORDER_SIZE_DESC);
                    Task.Run(() => folder.LoadChildNodes());
                }
            });

            sortItems.Add(new AdvancedMenuItem()
            {
                Name = UiResources.SmallestSortOption.ToLower(),
                TapAction = () =>
                {
                    // Needed on every UI interaction
                    SdkService.MegaSdk.retryPendingConnections();

                    sortRadWindow.IsOpen = false;
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle, (int)MSortOrderType.ORDER_SIZE_ASC);
                    Task.Run(() => folder.LoadChildNodes());
                }
            });

            sortItems.Add(new AdvancedMenuItem()
            {
                Name = UiResources.NewestSortOption.ToLower(),
                TapAction = () =>
                {
                    // Needed on every UI interaction
                    SdkService.MegaSdk.retryPendingConnections();

                    sortRadWindow.IsOpen = false;
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle, (int)MSortOrderType.ORDER_MODIFICATION_DESC);
                    Task.Run(() => folder.LoadChildNodes());
                }
            });

            sortItems.Add(new AdvancedMenuItem()
            {
                Name = UiResources.OldestSortOption.ToLower(),
                TapAction = () =>
                {
                    // Needed on every UI interaction
                    SdkService.MegaSdk.retryPendingConnections();

                    sortRadWindow.IsOpen = false;
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle, (int)MSortOrderType.ORDER_MODIFICATION_ASC);
                    Task.Run(() => folder.LoadChildNodes());
                }
            });

            sortItems.Add(new AdvancedMenuItem()
            {
                Name = UiResources.NameAscendingSortOption.ToLower(),
                TapAction = () =>
                {
                    // Needed on every UI interaction
                    SdkService.MegaSdk.retryPendingConnections();

                    sortRadWindow.IsOpen = false;
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle, (int)MSortOrderType.ORDER_ALPHABETICAL_ASC);
                    Task.Run(() => folder.LoadChildNodes());
                }
            });

            sortItems.Add(new AdvancedMenuItem()
            {
                Name = UiResources.NameDescendingSortOption.ToLower(),
                TapAction = () =>
                {
                    // Needed on every UI interaction
                    SdkService.MegaSdk.retryPendingConnections();

                    sortRadWindow.IsOpen = false;
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle, (int)MSortOrderType.ORDER_ALPHABETICAL_DESC);
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
            
            buttonStackPanel.Children.Add(headerText);
            buttonStackPanel.Children.Add(sortList);

            sortRadWindow.Content = buttonStackPanel;

            sortRadWindow.IsOpen = true;
        }

        public static void ShowSortDialog(OfflineFolderViewModel folder)
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
                IsAnimationEnabled = true,
                OpenAnimation = AnimationService.GetOpenDialogAnimation(),
                CloseAnimation = AnimationService.GetCloseDialogAnimation()
            };

            sortRadWindow.WindowOpening += (sender, args) => DialogOpening(args);
            sortRadWindow.WindowClosed += (sender, args) => DialogClosed();

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
                Name = UiResources.FilesAscendingSortOption.ToLower(),
                TapAction = () =>
                {
                    // Needed on every UI interaction
                    SdkService.MegaSdk.retryPendingConnections();

                    sortRadWindow.IsOpen = false;
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle, (int)MSortOrderType.ORDER_DEFAULT_ASC);
                    Task.Run(() => folder.LoadChildNodes());
                }
            });

            sortItems.Add(new AdvancedMenuItem()
            {
                Name = UiResources.FilesDescendingSortOption.ToLower(),
                TapAction = () =>
                {
                    // Needed on every UI interaction
                    SdkService.MegaSdk.retryPendingConnections();

                    sortRadWindow.IsOpen = false;
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle, (int)MSortOrderType.ORDER_DEFAULT_DESC);
                    Task.Run(() => folder.LoadChildNodes());
                }
            });

            sortItems.Add(new AdvancedMenuItem()
            {
                Name = UiResources.LargestSortOption.ToLower(),
                TapAction = () =>
                {
                    // Needed on every UI interaction
                    SdkService.MegaSdk.retryPendingConnections();

                    sortRadWindow.IsOpen = false;
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle, (int)MSortOrderType.ORDER_SIZE_DESC);
                    Task.Run(() => folder.LoadChildNodes());
                }
            });

            sortItems.Add(new AdvancedMenuItem()
            {
                Name = UiResources.SmallestSortOption.ToLower(),
                TapAction = () =>
                {
                    // Needed on every UI interaction
                    SdkService.MegaSdk.retryPendingConnections();

                    sortRadWindow.IsOpen = false;
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle, (int)MSortOrderType.ORDER_SIZE_ASC);
                    Task.Run(() => folder.LoadChildNodes());
                }
            });

            sortItems.Add(new AdvancedMenuItem()
            {
                Name = UiResources.NewestSortOption.ToLower(),
                TapAction = () =>
                {
                    // Needed on every UI interaction
                    SdkService.MegaSdk.retryPendingConnections();

                    sortRadWindow.IsOpen = false;
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle, (int)MSortOrderType.ORDER_MODIFICATION_DESC);
                    Task.Run(() => folder.LoadChildNodes());
                }
            });

            sortItems.Add(new AdvancedMenuItem()
            {
                Name = UiResources.OldestSortOption.ToLower(),
                TapAction = () =>
                {
                    // Needed on every UI interaction
                    SdkService.MegaSdk.retryPendingConnections();

                    sortRadWindow.IsOpen = false;
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle, (int)MSortOrderType.ORDER_MODIFICATION_ASC);
                    Task.Run(() => folder.LoadChildNodes());
                }
            });

            sortItems.Add(new AdvancedMenuItem()
            {
                Name = UiResources.NameAscendingSortOption.ToLower(),
                TapAction = () =>
                {
                    // Needed on every UI interaction
                    SdkService.MegaSdk.retryPendingConnections();

                    sortRadWindow.IsOpen = false;
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle, (int)MSortOrderType.ORDER_ALPHABETICAL_ASC);
                    Task.Run(() => folder.LoadChildNodes());
                }
            });

            sortItems.Add(new AdvancedMenuItem()
            {
                Name = UiResources.NameDescendingSortOption.ToLower(),
                TapAction = () =>
                {
                    // Needed on every UI interaction
                    SdkService.MegaSdk.retryPendingConnections();

                    sortRadWindow.IsOpen = false;
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle, (int)MSortOrderType.ORDER_ALPHABETICAL_DESC);
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
            sortList.ItemTap += (sender, args) => ((AdvancedMenuItem)args.Item.DataContext).TapAction.Invoke();

            buttonStackPanel.Children.Add(headerText);
            buttonStackPanel.Children.Add(sortList);

            sortRadWindow.Content = buttonStackPanel;

            sortRadWindow.IsOpen = true;
        }

        public static void ShowSortContactsDialog(ContactsViewModel contacts)
        {            
            if (contacts == null) return;

            var sortRadWindow = new RadModalWindow()
            {
                IsFullScreen = true,
                Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)),
                WindowSizeMode = WindowSizeMode.FitToPlacementTarget,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                IsAnimationEnabled = true,
                OpenAnimation = AnimationService.GetOpenDialogAnimation(),
                CloseAnimation = AnimationService.GetCloseDialogAnimation()
            };

            sortRadWindow.WindowOpening += (sender, args) => DialogOpening(args);
            sortRadWindow.WindowClosed += (sender, args) => DialogClosed();

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
                Name = UiResources.NameAscendingSortOption.ToLower(),
                TapAction = () =>
                {
                    // Needed on every UI interaction
                    SdkService.MegaSdk.retryPendingConnections();

                    sortRadWindow.IsOpen = false;
                    Task.Run(() => contacts.SortContacts(ListSortMode.Ascending));       
                }
            });

            sortItems.Add(new AdvancedMenuItem()
            {
                Name = UiResources.NameDescendingSortOption.ToLower(),
                TapAction = () =>
                {
                    // Needed on every UI interaction
                    SdkService.MegaSdk.retryPendingConnections();

                    sortRadWindow.IsOpen = false;
                    Task.Run(() => contacts.SortContacts(ListSortMode.Descending));
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
            sortList.ItemTap += (sender, args) => ((AdvancedMenuItem)args.Item.DataContext).TapAction.Invoke();

            buttonStackPanel.Children.Add(headerText);
            buttonStackPanel.Children.Add(sortList);

            sortRadWindow.Content = buttonStackPanel;

            sortRadWindow.IsOpen = true;
        }

        public static void ShowCancelSubscriptionFeedbackDialog()
        {
            var feedbackRadWindow = new RadModalWindow()
            {
                IsFullScreen = false,
                Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)),
                WindowSizeMode = WindowSizeMode.FitToPlacementTarget,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                IsAnimationEnabled = true,
                OpenAnimation = AnimationService.GetOpenDialogAnimation(),
                CloseAnimation = AnimationService.GetCloseDialogAnimation()
            };

            feedbackRadWindow.WindowOpening += (sender, args) => DialogOpening(args);
            feedbackRadWindow.WindowClosed += (sender, args) => DialogClosed();

            var feedbackStackPanel = new StackPanel()
            {
                Background = new SolidColorBrush((Color)Application.Current.Resources["PhoneChromeColor"]),
                Orientation = Orientation.Vertical,
                Width = Double.NaN,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0)
            };

            var titleLabel = new TextBlock()
            {
                Text = AppMessages.CancelSubscription_Title,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(24,12,24,12),
                FontSize = Convert.ToDouble(Application.Current.Resources["PhoneFontSizeLarge"])                
            };            
            feedbackStackPanel.Children.Add(titleLabel);

            var textMessage = new TextBlock()
            {
                Text = AppMessages.AM_CancelSubscription,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(24, 12, 24, 12)
            };
            feedbackStackPanel.Children.Add(textMessage);

            var feedbackTextBox = new TextBox()
            {
                TextWrapping = TextWrapping.Wrap,
                Height = 150,
                AcceptsReturn = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
                Margin = new Thickness(12, 0, 12, 0)
            };
            feedbackStackPanel.Children.Add(feedbackTextBox);

            var feedbackButtonsGrid = new Grid()
            {
                Width = Double.NaN,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Bottom,
                Background = new SolidColorBrush((Color)Application.Current.Resources["PhoneChromeColor"]),
                Margin = new Thickness(12)
            };
            feedbackButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            feedbackButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            feedbackStackPanel.Children.Add(feedbackButtonsGrid);

            var confirmButton = new Button()
            {
                Content = UiResources.Send.ToLower(),
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            confirmButton.Tap += (sender, args) =>
            {
                if(!String.IsNullOrWhiteSpace(feedbackTextBox.Text))
                {
                    feedbackRadWindow.IsOpen = false;
                    ShowCancelSubscriptionDialog(feedbackTextBox.Text);                    
                }
                else
                {
                    new CustomMessageDialog(
                            AppMessages.RequiredFields_Title,
                            AppMessages.RequiredFieldsCancelSubscription,
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialog();
                }
            };
            
            var cancelButton = new Button()
            {
                Content = UiResources.Dismiss.ToLower(),
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            cancelButton.Tap += (sender, args) =>
            {                
                feedbackRadWindow.IsOpen = false;
            };
            
            feedbackButtonsGrid.Children.Add(confirmButton);
            feedbackButtonsGrid.Children.Add(cancelButton);
            Grid.SetColumn(confirmButton, 0);
            Grid.SetColumn(cancelButton, 1);
            
            feedbackRadWindow.Content = feedbackStackPanel;

            feedbackRadWindow.IsOpen = true;
        }

        private static async void ShowCancelSubscriptionDialog(string reason)
        {
            var customMessageDialog = new CustomMessageDialog(AppMessages.CancelSubscription_Title,
                AppMessages.AM_CancelSubscriptionConfirmation, App.AppInformation, MessageDialogButtons.YesNo);

            customMessageDialog.OkOrYesButtonTapped += (sender, args) =>
            {
                 SdkService.MegaSdk.creditCardCancelSubscriptions(reason, new CancelSubscriptionRequestListener());
            };

            customMessageDialog.ShowDialog();
        }

        public static void ShowPinLockDialog(bool isChange, SettingsViewModel settingsViewModel)
        {
            var pinLockRadWindow = new RadModalWindow()
            {
                IsFullScreen = true,
                Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)),
                WindowSizeMode = WindowSizeMode.FitToPlacementTarget,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                IsAnimationEnabled = true,
                OpenAnimation = AnimationService.GetOpenDialogAnimation(),
                CloseAnimation = AnimationService.GetCloseDialogAnimation()
            };

            pinLockRadWindow.WindowOpening += (sender, args) => DialogOpening(args);
            pinLockRadWindow.WindowClosed += (sender, args) => DialogClosed();

            var pinLockStackPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                Width = Double.NaN,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(12),
                Background = new SolidColorBrush((Color)Application.Current.Resources["PhoneChromeColor"])
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
                FontFamily = new FontFamily("Segoe WP Semibold"),
                FontSize = Convert.ToDouble(Application.Current.Resources["PhoneFontSizeLarge"])
            };
            pinLockStackPanel.Children.Add(titleLabel);
            
            NumericPasswordBox currentPinLock = null;

            if (isChange)
            {
                titleLabel.Text = UiResources.ChangePinLock.ToUpper();
                currentPinLock = new NumericPasswordBox()
                {
                    Watermark = UiResources.UI_PinLock.ToLower(),
                    ClearButtonVisibility = Visibility.Visible
                };
                pinLockStackPanel.Children.Add(currentPinLock);
            }
            else
            {
                titleLabel.Text = UiResources.MakePinLock.ToUpper();
            }

            var pinLock = new NumericPasswordBox()
            {
                Watermark = UiResources.UI_NewPinLock.ToLower(),
                ClearButtonVisibility = Visibility.Visible
            };


            var confirmPinLock = new NumericPasswordBox()
            {
                Watermark = UiResources.UI_ConfirmPinLock.ToLower(),
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
                            new CustomMessageDialog(
                                    AppMessages.CurrentPinLockCodeDoNotMatch_Title,
                                    AppMessages.CurrentPinLockCodeDoNotMatch,
                                    App.AppInformation,
                                    MessageDialogButtons.Ok).ShowDialog();
                            return;
                        }
                    }

                }

                if (pinLock.Password.Length < 4)
                {
                    new CustomMessageDialog(
                            AppMessages.PinLockTooShort_Title,
                            AppMessages.PinLockTooShort,
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialog();
                    return;
                }

                if (!pinLock.Password.Equals(confirmPinLock.Password))
                {
                    new CustomMessageDialog(
                            AppMessages.PinLockCodesDoNotMatch_Title,
                            AppMessages.PinLockCodesDoNotMatch,
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialog();
                    return;
                }
               
                SettingsService.SaveSetting(SettingsResources.UserPinLock, CryptoService.HashData(pinLock.Password));
                SettingsService.SaveSetting(SettingsResources.UserPinLockIsEnabled, true);

                App.AppInformation.HasPinLockIntroduced = true;

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
                VerticalAlignment = VerticalAlignment.Stretch,
                Background = new SolidColorBrush((Color)Application.Current.Resources["PhoneChromeColor"])
            };

            grid.Children.Add(pinLockStackPanel);
            grid.Children.Add(pinLockButtonsGrid);

            pinLockRadWindow.Content = grid;
      
            pinLockRadWindow.IsOpen = true;
        }

        /// <summary>
        /// Show a dialog to change the account password
        /// </summary>
        public static async void ShowChangePasswordDialog()
        {
            var changePasswordDialog = new ChangePasswordDialog();
            await changePasswordDialog.ShowDialogAsync();
        }

        /// <summary>
        /// Show a dialog to check if the user remember the account password
        /// </summary>
        /// <param name="atLogout">True if the dialog is being displayed just before a logout</param>
        public static void ShowPasswordReminderDialog(bool atLogout)
        {
            var passwordReminderDialog = new PasswordReminderDialog(atLogout);
            passwordReminderDialog.ShowDialog();
        }

        public static async Task<MessageDialogResult> ShowOptionsDialog(string title, string message, IEnumerable<DialogButton> buttons)
        {
            var customMessageDialog = new CustomMessageDialog(title, message, App.AppInformation, buttons);

            return await customMessageDialog.ShowDialogAsync();
        }

        public static void ShowViewRecoveryKey(string recoveryKey, Action copyAction)
        {
            var customMessageDialog = new CustomMessageDialog(UiResources.UI_RecoveryKey, recoveryKey, App.AppInformation,
                new[]
                {
                    new DialogButton(UiResources.Copy, copyAction),
                    DialogButton.GetCancelButton(), 
                });

            customMessageDialog.ShowDialog();
        }

        /// <summary>
        /// Shows an alert dialog to inform that the DEBUG mode is enabled.
        /// <para>Also asks if the user wants to disable it.</para>
        /// </summary>
        public static void ShowDebugModeAlert()
        {
            var customMessageDialog = new CustomMessageDialog(
                AppMessages.AM_DebugModeEnabled_Title,
                AppMessages.AM_DebugModeEnabled_Message,
                App.AppInformation, MessageDialogButtons.YesNo);

            customMessageDialog.OkOrYesButtonTapped += (sender, args) =>
            {
                DebugService.DebugSettings.ShowDebugAlert = false;
                DebugService.DebugSettings.DisableDebugMode();
            };

            customMessageDialog.CancelOrNoButtonTapped += (sender, args) =>
            {
                DebugService.DebugSettings.ShowDebugAlert = false;                
            };

            customMessageDialog.ShowDialog();
        }

        /// <summary>
        /// Prepares and launch the task to share items with other app.
        /// </summary>
        /// <param name="storageItems">Items to share.</param>
        public static void ShowShareMediaTask(List<StorageFile> storageItems)
        {
            if (storageItems == null || storageItems.Count < 1) return;

            try
            {
                DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
                dataTransferManager.DataRequested += (sender, args) =>
                {
                    DataPackage requestData = args.Request.Data;
                    requestData.Properties.ApplicationName = AppResources.ApplicationName;

                    if (storageItems.Count > 1)
                    {
                        requestData.Properties.Title = AppMessages.AM_ShareMultipleItemsFromMega_Title;
                        requestData.Properties.Description = AppMessages.AM_ShareMultipleItemsFromMega_Message;
                        requestData.SetText(AppMessages.AM_ShareMultipleItemsFromMega_Message);
                    }
                    else
                    {
                        requestData.Properties.Title = AppMessages.AM_ShareItemFromMega_Title;
                        requestData.Properties.Description = AppMessages.AM_ShareItemFromMega_Message;
                        requestData.SetText(AppMessages.AM_ShareItemFromMega_Message);
                    }

                    requestData.SetStorageItems(storageItems);
                };

                try { DataTransferManager.ShowShareUI(); }
                catch (Exception) /* P.Bug #5447*/
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        new CustomMessageDialog(
                            AppMessages.AM_ShareFromMegaFailed_Title.ToUpper(),
                            AppMessages.AM_ShareFromMegaAppBusy_Message,
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialog();
                    });                    
                }
            }
            catch (NotSupportedException) 
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    new CustomMessageDialog(
                        AppMessages.AM_ShareFromMegaFailed_Title.ToUpper(),
                        AppMessages.AM_ShareFromMegaFailed_Message,
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                });
                
            }
        }

        /// <summary>
        /// Show a dialog to setup the Multi-Factor Authentication for the account
        /// </summary>
        /// <returns>TRUE if the user continues with the setup process or FALSE in other case</returns>
        public static async Task<bool> ShowMultiFactorAuthSetupDialogAsync()
        {
            var mfaSetupDialog = new MultiFactorAuthSetupDialog();
            var result = await mfaSetupDialog.ShowDialogAsync();
            return result;
        }

        /// <summary>
        /// Show a dialog to indicate that the user has successfully enabled the Multi-Factor Authentication
        /// </summary>
        public static async void ShowMultiFactorAuthEnabledDialog()
        {
            var mfaEnabledDialog = new MultiFactorAuthEnabledDialog();
            await mfaEnabledDialog.ShowDialogAsync();
        }

        /// <summary>
        /// Show a dialog to indicate that the user has successfully disabled the Multi-Factor Authentication
        /// </summary>
        public static async void ShowMultiFactorAuthDisabledDialog()
        {
            var mfaDisabledDialog = new MultiFactorAuthDisabledDialog();
            await mfaDisabledDialog.ShowDialogAsync();
        }

        /// <summary>
        /// Show an input dialog to type the MFA code and execute an action.
        /// </summary>
        /// <param name="dialogAction">Action to do by the primary button.</param>
        /// <param name="title">Custom title of the input dialog.</param>
        /// <param name="message">Custom message of the input dialog.</param>
        /// <returns>The dialog action result as <see cref="bool"/> value.</returns>
        public static async Task<bool> ShowMultiFactorAuthCodeInputDialogAsync(
            Func<string, bool> dialogAction, string title = null, string message = null)
        {
            var dialog = MultiFactorAuthCodeInputDialogInstance =
                new MultiFactorAuthCodeInputDialog(dialogAction, title, message);
            return await dialog.ShowDialogAsync();
        }

        /// <summary>
        /// Show an input dialog to type the MFA code and execute an async action.
        /// </summary>
        /// <param name="dialogActionAsync">Async action to do by the primary button.</param>
        /// <param name="title">Custom title of the input dialog.</param>
        /// <param name="message">Custom message of the input dialog.</param>
        /// <returns>The dialog action result as <see cref="bool"/> value.</returns>
        public static async Task<bool> ShowAsyncMultiFactorAuthCodeInputDialogAsync(
            Func<string, Task<bool>> dialogActionAsync, string title = null, string message = null)
        {
            var dialog = MultiFactorAuthCodeInputDialogInstance =
                new MultiFactorAuthCodeInputDialog(dialogActionAsync, title, message);
            return await dialog.ShowDialogAsync();
        }

        /// <summary>
        /// Set the warning message of the MFA code input dialog displayed
        /// </summary>
        /// <param name="warningMessage">Text of the warning message</param>
        public static void SetMultiFactorAuthCodeInputDialogWarningMessage(string warningMessage = null)
        {
            if (MultiFactorAuthCodeInputDialogInstance == null) return;
            MultiFactorAuthCodeInputDialogInstance.WarningMessageText = warningMessage ??
                AppMessages.AM_InvalidCode;
            MultiFactorAuthCodeInputDialogInstance.IsWarningMessageVisible = true;
        }

        /// <summary>
        /// Close the MFA code input dialog displayed
        /// </summary>
        public static void CloseMultiFactorAuthCodeInputDialog()
        {
            if (MultiFactorAuthCodeInputDialogInstance == null ||
                MultiFactorAuthCodeInputDialogInstance.CloseCommand == null ||
                !MultiFactorAuthCodeInputDialogInstance.CloseCommand.CanExecute(null))
                return;

            MultiFactorAuthCodeInputDialogInstance.CloseCommand.Execute(null);
        }

        #endregion
    }
}
