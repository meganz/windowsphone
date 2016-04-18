using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Models;
using MegaApp.Pages;
using MegaApp.Resources;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Telerik.Windows.Controls;
using Telerik.Windows.Data;
#if WINDOWS_PHONE_81

#endif

namespace MegaApp.Services
{
    static class DialogService
    {
        public static void ShowShareLink(string link)
        {
            var customMessageDialog = new CustomMessageDialog(UiResources.MegaLinkTitle, link, App.AppInformation,
                new []
                {
                    new DialogButton(UiResources.Share, () =>
                    {
                        var shareLinkTask = new ShareLinkTask {LinkUri = new Uri(link), Title = UiResources.MegaShareLinkMessage};
                        shareLinkTask.Show();
                    }),
                    new DialogButton(UiResources.Copy, () =>
                    {
                        try
                        {
                            Clipboard.SetText(link);
                            new CustomMessageDialog(
                                AppMessages.LinkCopiedToClipboard_Title,
                                AppMessages.LinkCopiedToClipboard,
                                App.AppInformation,
                                MessageDialogButtons.Ok).ShowDialog();
                        }
                        catch(Exception)
                        {
                            new CustomMessageDialog(
                                AppMessages.AM_CopyLinkToClipboardFailed_Title,
                                AppMessages.AM_CopyLinkToClipboardFailed,
                                App.AppInformation,
                                MessageDialogButtons.Ok).ShowDialog();
                        }
                    }),
                    DialogButton.GetCancelButton(), 
                });

            customMessageDialog.ShowDialog();
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
        public static void ShowOpenLink(MNode publicNode, string link, FolderViewModel folderViewModel)
        {
            // Check if a folderviewmodel is available
            if (folderViewModel == null) throw new ArgumentNullException("folderViewModel");

            // Needed to avoid "Implicity captured closure" compiler warning.
            var importFolderViewModel = folderViewModel;
            var downloadFolderViewModel = folderViewModel;

            if (publicNode == null) throw new ArgumentNullException("publicNode");

            var customMessageDialog = new CustomMessageDialog(UiResources.LinkOptions, publicNode.getName(), App.AppInformation,
               new[]
                {
                    new DialogButton(UiResources.Import, () =>
                    {
                        if (String.IsNullOrWhiteSpace(link)) throw new ArgumentNullException("link");
                        importFolderViewModel.ImportLink(link);
                    }),
                    new DialogButton(UiResources.Download, () =>
                    {                        
                        downloadFolderViewModel.DownloadLink(publicNode);
                    }),
                });

            customMessageDialog.ShowDialog();
            
        }
        #endif

        public static void ShowOverquotaAlert()
        {
            var customMessageDialog = new CustomMessageDialog(AppMessages.OverquotaAlert_Title,
                AppMessages.OverquotaAlert, App.AppInformation, MessageDialogButtons.YesNo);
            
            customMessageDialog.OkOrYesButtonTapped += (sender, args) =>
            {
                ((PhoneApplicationFrame) Application.Current.RootVisual).Navigate(
                    new Uri("/Pages/MyAccountPage.xaml?Pivot=1", UriKind.RelativeOrAbsolute));
            };

            customMessageDialog.ShowDialog();
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
            App.CloudDrive.CurrentRootNode = (NodeViewModel)folder.FolderRootNode;            

            var uploadRadWindow = new RadModalWindow()
            {
                IsFullScreen = true,
                Background = new SolidColorBrush((Color)Application.Current.Resources["PhoneChromeColor"]),
                WindowSizeMode = WindowSizeMode.FitToPlacementTarget,
                HorizontalContentAlignment= HorizontalAlignment.Stretch,
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
                    App.MegaSdk.retryPendingConnections();

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
                    App.MegaSdk.retryPendingConnections();

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
                    App.MegaSdk.retryPendingConnections();

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
                    App.MegaSdk.retryPendingConnections();

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
                    App.MegaSdk.retryPendingConnections();

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
                    App.MegaSdk.retryPendingConnections();

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
                    App.MegaSdk.retryPendingConnections();

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
                    App.MegaSdk.retryPendingConnections();

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
                    App.MegaSdk.retryPendingConnections();

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
                    App.MegaSdk.retryPendingConnections();

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
                    App.MegaSdk.retryPendingConnections();

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
                    App.MegaSdk.retryPendingConnections();

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
                    App.MegaSdk.retryPendingConnections();

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
                    App.MegaSdk.retryPendingConnections();

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
                    App.MegaSdk.retryPendingConnections();

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
                    App.MegaSdk.retryPendingConnections();

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
                    App.MegaSdk.retryPendingConnections();

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
                    App.MegaSdk.retryPendingConnections();

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
                 App.MegaSdk.creditCardCancelSubscriptions(reason, new CancelSubscriptionRequestListener());
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
                    Watermark = UiResources.PinLockWatermark.ToLower(),
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

        public static void ShowChangePasswordDialog()
        {
            var changePasswordRadWindow = new RadModalWindow()
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

            changePasswordRadWindow.WindowOpening += (sender, args) => DialogOpening(args);
            changePasswordRadWindow.WindowClosed += (sender, args) => DialogClosed();

            var passwordStackPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                Width = Double.NaN,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(12),
                Background = new SolidColorBrush((Color)Application.Current.Resources["PhoneChromeColor"])
            };

            var passwordButtonsGrid = new Grid()
            {
                Width = Double.NaN,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Bottom,
                Background = new SolidColorBrush((Color)Application.Current.Resources["PhoneChromeColor"])
            };
            passwordButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            passwordButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });


            var titleLabel = new TextBlock()
            {
                Text = UiResources.UI_ChangePassword.ToUpper(),
                Margin = new Thickness(12),
                FontFamily = new FontFamily("Segoe WP Semibold"),
                FontSize = Convert.ToDouble(Application.Current.Resources["PhoneFontSizeLarge"])                
            };
            passwordStackPanel.Children.Add(titleLabel);
                        
            var currentPassword = new RadPasswordBox()
            {
                Watermark = UiResources.PasswordWatermark.ToLower(),
                ClearButtonVisibility = Visibility.Visible
            };
            passwordStackPanel.Children.Add(currentPassword);

            var newPassword = new RadPasswordBox()
            {
                Watermark = UiResources.NewPasswordWatermark.ToLower(),
                ClearButtonVisibility = Visibility.Visible                
            };
            passwordStackPanel.Children.Add(newPassword);

            var confirmPassword = new RadPasswordBox()
            {
                Watermark = UiResources.ConfirmPasswordWatermark.ToLower(),
                ClearButtonVisibility = Visibility.Visible
            };
            passwordStackPanel.Children.Add(confirmPassword);


            var confirmButton = new Button()
            {
                Content = UiResources.Done.ToLower(),
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            confirmButton.Tap += (sender, args) =>
            {
                if (!String.IsNullOrWhiteSpace(currentPassword.Password) && 
                    !String.IsNullOrWhiteSpace(newPassword.Password) && !String.IsNullOrWhiteSpace(confirmPassword.Password))
                {
                    if(!newPassword.Password.Equals(confirmPassword.Password))
                    {
                        new CustomMessageDialog(
                            UiResources.UI_ChangePassword.ToUpper(),
                            AppMessages.PasswordsDoNotMatch,
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialog();
                        return;
                    }

                    if(newPassword.Password.Equals(currentPassword.Password))
                    {
                        new CustomMessageDialog(
                            UiResources.UI_ChangePassword.ToUpper(),
                            AppMessages.NewAndOldPasswordMatch,
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialog();
                        return;
                    }

                    App.MegaSdk.changePassword(currentPassword.Password, newPassword.Password, new ChangePasswordRequestListener());
                }
                else
                {
                    new CustomMessageDialog(
                        AppMessages.RequiredFields_Title.ToUpper(),
                        AppMessages.RequiredFieldsChangePassword,
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                    return;
                }

                changePasswordRadWindow.IsOpen = false;
            };

            var cancelButton = new Button()
            {
                Content = UiResources.Cancel.ToLower(),
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            cancelButton.Tap += (sender, args) =>
            {
                changePasswordRadWindow.IsOpen = false;
            };

            passwordButtonsGrid.Children.Add(confirmButton);
            passwordButtonsGrid.Children.Add(cancelButton);
            Grid.SetColumn(confirmButton, 0);
            Grid.SetColumn(cancelButton, 1);

            var grid = new Grid()
            {
                Width = Double.NaN,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Background = new SolidColorBrush((Color)Application.Current.Resources["PhoneChromeColor"])
            };

            grid.Children.Add(passwordStackPanel);
            grid.Children.Add(passwordButtonsGrid);

            changePasswordRadWindow.Content = grid;

            changePasswordRadWindow.IsOpen = true;
        }

        public static async Task<MessageDialogResult> ShowOptionsDialog(string title, string message, IEnumerable<DialogButton> buttons)
        {
            var customMessageDialog = new CustomMessageDialog(title, message, App.AppInformation, buttons);

            return await customMessageDialog.ShowDialogAsync();
        }

        public static void ShowViewMasterKey(string masterkey, Action copyAction)
        {
            var customMessageDialog = new CustomMessageDialog(UiResources.MasterKey, masterkey, App.AppInformation,
                new[]
                {
                    new DialogButton(UiResources.Copy, copyAction),
                    DialogButton.GetCancelButton(), 
                });

            customMessageDialog.ShowDialog();
        }
    }
}
