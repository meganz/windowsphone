using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Telerik.Windows.Controls;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.UserControls;

namespace MegaApp.Dialogs
{
    public class ChangePasswordDialog : MegaDialog
    {
        public ChangePasswordDialog() : base(UiResources.UI_ChangePassword)
        {
            var contentStackPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                Width = Double.NaN,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(12),
                Background = new SolidColorBrush((Color)Application.Current.Resources["PhoneChromeColor"])
            };

            var description = new TextBlock()
            {
                Margin = new Thickness(12, 0, 12, 20),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Opacity = 0.8,
                Text = UiResources.UI_ChangePasswordDescription,
                TextWrapping = TextWrapping.Wrap
            };
            contentStackPanel.Children.Add(description);

            var passwordStrengthIndicator = new PasswordStrengthIndicator()
            {
                Height = 4,
                Margin = new Thickness(12, 0, 12, 0),
                IndicatorBackground = (Brush)Application.Current.Resources["PhoneInactiveBrush"],
                IndicatorForeground = (Brush)Application.Current.Resources["MegaRedColorBrush"]
            };
            passwordStrengthIndicator.IndicatorsOpacity.Add(0.4);
            passwordStrengthIndicator.IndicatorsOpacity.Add(0.6);
            passwordStrengthIndicator.IndicatorsOpacity.Add(0.8);
            passwordStrengthIndicator.IndicatorsOpacity.Add(1.0);

            var newPassword = new RadPasswordBox()
            {
                Watermark = UiResources.UI_NewPassword.ToLower(),
                ClearButtonVisibility = Visibility.Visible
            };
            newPassword.PasswordChanged += (sender, args) =>
            {
                passwordStrengthIndicator.Value =
                    ValidationService.CalculatePasswordStrength(newPassword.Password);
            };
            contentStackPanel.Children.Add(newPassword);

            contentStackPanel.Children.Add(passwordStrengthIndicator);

            var confirmPassword = new RadPasswordBox()
            {
                Watermark = UiResources.UI_ConfirmPassword.ToLower(),
                ClearButtonVisibility = Visibility.Visible
            };
            contentStackPanel.Children.Add(confirmPassword);

            var warningMessage = new TextBlock()
            {
                Margin = new Thickness(12, 0, 12, 0),
                Foreground = (Brush)Application.Current.Resources["MegaRedColorBrush"],
                Text = string.Empty,
                TextWrapping = TextWrapping.Wrap
            };
            contentStackPanel.Children.Add(warningMessage);

            this.MainGrid.Children.Add(contentStackPanel);
            Grid.SetRow(contentStackPanel, 2);

            var buttonsGrid = new Grid()
            {
                Width = Double.NaN,
                Margin = new Thickness(12),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Bottom,
                Background = new SolidColorBrush((Color)Application.Current.Resources["PhoneChromeColor"])
            };
            buttonsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            buttonsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            var saveButton = new Button()
            {
                Content = UiResources.Save.ToLower(),
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            saveButton.Tap += async (sender, args) =>
            {
                warningMessage.Text = string.Empty;

                if (string.IsNullOrWhiteSpace(newPassword.Password) || string.IsNullOrWhiteSpace(confirmPassword.Password))
                {
                    warningMessage.Text = AppMessages.AM_EmptyRequiredFields;
                    return;
                }

                // If the new password and the confirmation password don't match
                if (!newPassword.Password.Equals(confirmPassword.Password))
                {
                    warningMessage.Text = AppMessages.PasswordsDoNotMatch;
                    return;
                }

                // If the password strength is very weak
                if (passwordStrengthIndicator.Value == MPasswordStrength.PASSWORD_STRENGTH_VERYWEAK)
                {
                    warningMessage.Text = AppMessages.AM_VeryWeakPassword;
                    return;
                }

                ChangePasswordResult result = ChangePasswordResult.Unknown;
                var changePassword = new ChangePasswordRequestListenerAsync();

                var mfaStatus = await AccountService.CheckMultiFactorAuthStatusAsync();
                if (mfaStatus == MultiFactorAuthStatus.Enabled)
                {
                    this.CloseDialog();
                    await DialogService.ShowAsyncMultiFactorAuthCodeInputDialogAsync(async (string code) =>
                    {
                        result = await changePassword.ExecuteAsync(() =>
                        {
                            SdkService.MegaSdk.multiFactorAuthChangePasswordWithoutOld(
                                newPassword.Password, code, changePassword);
                        });

                        if (result == ChangePasswordResult.MultiFactorAuthInvalidCode)
                        {
                            DialogService.SetMultiFactorAuthCodeInputDialogWarningMessage();
                            return false;
                        }

                        return true;
                    });
                }
                else
                {
                    result = await changePassword.ExecuteAsync(() =>
                        SdkService.MegaSdk.changePasswordWithoutOld(newPassword.Password, changePassword));
                }

                if (result != ChangePasswordResult.Success)
                {
                    warningMessage.Text = AppMessages.AM_PasswordChangeFailed;
                    Deployment.Current.Dispatcher.BeginInvoke(() => this.ShowDialog());
                    return;
                }

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    new CustomMessageDialog(
                        AppMessages.AM_PasswordChanged_Title,
                        AppMessages.AM_PasswordChanged,
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                });
            };

            var cancelButton = new Button()
            {
                Content = UiResources.Cancel.ToLower(),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Command = this.CloseCommand
            };

            buttonsGrid.Children.Add(saveButton);
            buttonsGrid.Children.Add(cancelButton);
            Grid.SetColumn(saveButton, 0);
            Grid.SetColumn(cancelButton, 1);

            this.MainGrid.Children.Add(buttonsGrid);
            Grid.SetRow(buttonsGrid, 3);
        }
    }
}
