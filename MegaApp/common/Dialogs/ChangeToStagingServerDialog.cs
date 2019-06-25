using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using mega;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.ViewModels;

namespace MegaApp.Dialogs
{
    public class ChangeToStagingServerDialog : MegaDialog
    {
        public ChangeToStagingServerDialog() : base(AppMessages.AM_ChangeToStagingServer_Title)
        {
            this.ChangeApiUrlCommand = new DelegateCommand(this.ChangeApiUrl);

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

            var description = new TextBlock()
            {
                Margin = new Thickness(0, 0, 0, 20),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Text = AppMessages.AM_ChangeToStagingServer,
                TextWrapping = TextWrapping.Wrap
            };
            contentStackPanel.Children.Add(description);

            this.useSpecialPortCheckBox = new CheckBox()
            {
                Margin = new Thickness(-12, 0, -12, 0),
                Content = new TextBlock()
                {
                    Opacity = 0.8,
                    Text = UiResources.UI_UsePort444,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
            contentStackPanel.Children.Add(this.useSpecialPortCheckBox);

            this.MainGrid.Children.Add(contentStackPanel);
            Grid.SetRow(contentStackPanel, 2);

            var buttonStackPanel = new StackPanel()
            {
                Margin = new Thickness(8, 0, 8, 8),
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            var okButton = new Button()
            {
                Content = UiResources.Ok,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Command = this.ChangeApiUrlCommand
            };

            var cancelButton = new Button()
            {
                Content = UiResources.Cancel,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Command = this.CloseCommand
            };

            buttonStackPanel.Children.Add(okButton);
            buttonStackPanel.Children.Add(cancelButton);

            this.MainGrid.Children.Add(buttonStackPanel);
            Grid.SetRow(buttonStackPanel, 3);
        }

        #region Commands

        /// <summary>
        /// Command invoked to change the API URL to staging server
        /// </summary>
        public ICommand ChangeApiUrlCommand { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Change the API URL to staging server
        /// </summary>
        private void ChangeApiUrl(object obj)
        {
            this.DialogResult = true;

            if (this.useSpecialPortCheckBox.IsChecked == true)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Changing API URL to staging server...");
                SettingsService.SaveSetting<bool>(SettingsResources.UseStagingServer, false);
                SettingsService.SaveSetting<bool>(SettingsResources.UseStagingServerPort444, true);
                SdkService.MegaSdk.changeApiUrl(AppResources.AR_StagingUrlPort444, true);
                SdkService.MegaSdkFolderLinks.changeApiUrl(AppResources.AR_StagingUrlPort444, true);
            }
            else
            {
                LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Changing API URL to staging server (port 444)...");
                SettingsService.SaveSetting<bool>(SettingsResources.UseStagingServer, true);
                SettingsService.SaveSetting<bool>(SettingsResources.UseStagingServerPort444, false);
                SdkService.MegaSdk.changeApiUrl(AppResources.AR_StagingUrl, false);
                SdkService.MegaSdkFolderLinks.changeApiUrl(AppResources.AR_StagingUrl, false);
            }

            this.CloseDialog();
        }

        #endregion

        #region Controls

        /// <summary>
        /// Check box to allow select a special port (444) for the staging server.
        /// </summary>
        private CheckBox useSpecialPortCheckBox { get; set; }
        
        #endregion
    }
}
