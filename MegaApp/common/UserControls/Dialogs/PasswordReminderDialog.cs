using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Telerik.Windows.Controls;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.ViewModels;

namespace MegaApp.Dialogs
{
    public class PasswordReminderDialog : MegaDialog
    {
        /// <summary>
        /// Maximum number of attempts to check the password 
        /// </summary>
        private const int MaxAttempts = 3;

        /// <summary>
        /// Create a dialog to check if the user remember the account password
        /// </summary>
        /// <param name="atLogout">True if the dialog is being displayed just before a logout</param>
        public PasswordReminderDialog(bool atLogout) : base(AppMessages.AM_PasswordReminder_Title)
        {
            this.atLogout = atLogout;

            this.CheckPasswordCommand = new DelegateCommand(this.CheckPassword);
            this.SaveKeyButtonCommand = new DelegateCommand(this.SaveKey);
            this.TestPasswordButtonCommand = new DelegateCommand(this.TestPassword);

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

            this.Description = new TextBlock()
            {
                Margin = new Thickness(0, 0, 0, 20),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Text = AppMessages.AM_PasswordReminder,
                TextWrapping = TextWrapping.Wrap
            };
            contentStackPanel.Children.Add(this.Description);
            
            this.doNotShowMeAgainCheckBox = new CheckBox()
            {
                Margin = new Thickness(-12, 0, -12, 0),
                Content = new TextBlock()
                {
                    Opacity = 0.8,
                    Text = UiResources.UI_DoNotShowMeAgain,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
            contentStackPanel.Children.Add(this.doNotShowMeAgainCheckBox);

            this.passwordBox = new RadPasswordBox()
            {
                Margin = new Thickness(-12, 0, -12, 0),
                Visibility = Visibility.Collapsed
            };
            contentStackPanel.Children.Add(this.passwordBox);

            this.errorMessage = new TextBlock()
            {
                Margin = new Thickness(0, 8, 0, 16),
                Foreground = (Brush)Application.Current.Resources["MegaRedColorBrush"],
                Text = string.Empty,
                TextWrapping = TextWrapping.Wrap
            };
            contentStackPanel.Children.Add(this.errorMessage);

            this.MainGrid.Children.Add(contentStackPanel);
            Grid.SetRow(contentStackPanel, 1);

            var buttonStackPanel = new StackPanel()
            {
                Margin = new Thickness(8, 0, 8, 8),
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            
            this.testPasswordButton = new Button()
            {
                Content = UiResources.UI_TestPassword,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Command = this.TestPasswordButtonCommand
            };
            
            var backupRecoveryKeyButton = new Button()
            {
                Content = UiResources.UI_BackupRecoveryKey,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Command = this.SaveKeyButtonCommand
            };

            this.dismissButton = new Button()
            {
                Content = UiResources.Dismiss,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Command = this.CloseCommand
            };

            buttonStackPanel.Children.Add(this.testPasswordButton);
            buttonStackPanel.Children.Add(backupRecoveryKeyButton);
            buttonStackPanel.Children.Add(this.dismissButton);

            this.MainGrid.Children.Add(buttonStackPanel);
            Grid.SetRow(buttonStackPanel, 2);
        }

        #region Commands

        /// <summary>
        /// Command invoked to check the password typed by the user
        /// </summary>
        public ICommand CheckPasswordCommand { get; private set; }

        /// <summary>
        /// Command invoked when the user select the "Backup Recovery key" option
        /// </summary>
        public ICommand SaveKeyButtonCommand { get; private set; }

        /// <summary>
        /// Command invoked when the user select the "Test Password" option
        /// </summary>
        public ICommand TestPasswordButtonCommand { get; private set; }

        #endregion

        #region Properties

        /// <summary>
        /// Indicates if the dialog is being displayed in a log out scenario
        /// </summary>
        private bool atLogout;

        /// <summary>
        /// Number of failed attempts to check the password
        /// </summary>
        private int failedAttempts = 0;

        /// <summary>
        /// Flag to store if the user has selected the "Test Password" option
        /// </summary>
        private bool isTestPasswordSelected = false;

        /// <summary>
        /// Indicates if the user has checked the password successfully
        /// </summary>
        private bool passwordChecked = false;

        /// <summary>
        /// Indicates if the user has saved the recovery key successfully
        /// </summary>
        private bool recoveryKeySaved = false;

        #endregion

        #region Methods

        /// <summary>
        /// Check the password typed by the user
        /// </summary>
        private void CheckPassword(object obj)
        {
            if (string.IsNullOrWhiteSpace(this.passwordBox.Password)) return;

            this.passwordChecked = SdkService.MegaSdk.checkPassword(this.passwordBox.Password);
            if (!this.passwordChecked)
            {
                this.errorMessage.Text = AppMessages.AM_TestPasswordWarning;

                this.failedAttempts++;
                if (this.failedAttempts < MaxAttempts) return;

                // I user has exceeded the number of attempts, close 
                // this dialog and show the "Change password" dialog
                this.CloseDialog();
                DialogService.ShowChangePasswordDialog();
                return;
            }

            this.dismissButton.Visibility = Visibility.Collapsed;
            this.errorMessage.Foreground = new SolidColorBrush(UiService.GetColorFromHex("#00C0A5"));
            this.errorMessage.Text = AppMessages.AM_TestPasswordSuccess;
            this.testPasswordButton.Command = this.CloseCommand;
            this.testPasswordButton.Content = this.atLogout ?
                UiResources.UI_Logout : UiResources.UI_Close;
        }

        /// <summary>
        /// Backup the Recovery key
        /// </summary>
        private void SaveKey(object obj)
        {
            try
            {
                Clipboard.SetText(SdkService.MegaSdk.exportMasterKey());
                SdkService.MegaSdk.masterKeyExported();
                this.recoveryKeySaved = true;

                this.CloseDialog();
            }
            catch (Exception)
            {
                this.errorMessage.Text = AppMessages.AM_RecoveryKeyClipboardFailed;
            }
        }

        /// <summary>
        /// Change the UI of the dialog to allow the user check the password
        /// </summary>
        private void TestPassword(object obj)
        {
            this.doNotShowMeAgainCheckBox.IsChecked = false;
            this.isTestPasswordSelected = true;
            this.Title.Text = AppMessages.AM_TestPassword_Title.ToUpper();
            this.Description.Text = AppMessages.AM_TestPassword;
            this.doNotShowMeAgainCheckBox.Visibility = Visibility.Collapsed;
            this.passwordBox.Visibility = Visibility.Visible;
            this.testPasswordButton.Command = this.CheckPasswordCommand;
        }

        /// <summary>
        /// Close the dialog
        /// </summary>
        protected override async void CloseDialog(object obj = null)
        {
            base.CloseDialog();

            if (this.recoveryKeySaved)
            {
                await new CustomMessageDialog(
                    AppMessages.AM_RecoveryKeyCopied_Title,
                    AppMessages.AM_RecoveryKeyCopied,
                    App.AppInformation,
                    MessageDialogButtons.Ok).ShowDialogAsync();
            }

            if (this.atLogout)
            {
                var passwordReminderDialogListener = new SetPasswordReminderDialogResultListenerAsync();

                // If user has checked the "Don't show me again" box
                if (!this.isTestPasswordSelected && this.doNotShowMeAgainCheckBox.IsChecked == true)
                {
                    await passwordReminderDialogListener.ExecuteAsync(() =>
                        SdkService.MegaSdk.passwordReminderDialogBlocked(passwordReminderDialogListener));
                }
                // If the user has checked the password successfully
                else if (this.passwordChecked)
                {
                    await passwordReminderDialogListener.ExecuteAsync(() =>
                        SdkService.MegaSdk.passwordReminderDialogSucceeded(passwordReminderDialogListener));
                }
                else
                {
                    await passwordReminderDialogListener.ExecuteAsync(() =>
                        SdkService.MegaSdk.passwordReminderDialogSkipped(passwordReminderDialogListener));
                    
                    // Only log out if the user has saved the recovery key
                    if (!this.recoveryKeySaved) return;
                }

                SdkService.MegaSdk.logout(new LogOutRequestListener());
                return;
            }
            
            // If user has checked the "Don't show me again" box
            if (!this.isTestPasswordSelected && this.doNotShowMeAgainCheckBox.IsChecked == true)
                SdkService.MegaSdk.passwordReminderDialogBlocked();
            // If the user has checked the password successfully
            else if (this.passwordChecked)
                SdkService.MegaSdk.passwordReminderDialogSucceeded();
            else
                SdkService.MegaSdk.passwordReminderDialogSkipped();
        }

        #endregion

        #region Controls

        private TextBlock Description { get; set; }
        private CheckBox doNotShowMeAgainCheckBox { get; set; }
        private RadPasswordBox passwordBox { get; set; }
        private TextBlock errorMessage { get; set; }
        private Button testPasswordButton { get; set; }
        private Button dismissButton { get; set; }

        #endregion
    }
}
