using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MegaApp.Classes;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.Dialogs
{
    public class PinLockDialog : TwoButtonsDialog
    {
        /// <summary>
        /// Dialog to set or change the PIN lock code.
        /// </summary>
        /// <param name="isChange">True if is to change the PIN lock code or false in other case.</param>
        public PinLockDialog(bool isChange) :
            base(isChange ? UiResources.ChangePinLock : UiResources.MakePinLock,
            new TwoButtonsDialogSettings() { ApplicationBarButtons = true, OverrideOkButtonText = UiResources.Accept })
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

            var warningMessage = new TextBlock()
            {
                Margin = new Thickness(12, 0, 12, 0),
                Foreground = (Brush)Application.Current.Resources["MegaRedColorBrush"],
                Text = string.Empty,
                TextWrapping = TextWrapping.Wrap
            };

            NumericPasswordBox currentPinLock = null;

            if (isChange)
            {
                currentPinLock = new NumericPasswordBox()
                {
                    Watermark = UiResources.UI_PinLock.ToLower(),
                    ClearButtonVisibility = Visibility.Collapsed
                };
                currentPinLock.PasswordChanged += (sender, args) =>
                {
                    warningMessage.Text = string.Empty;
                };

                contentStackPanel.Children.Add(currentPinLock);
            }

            var pinLock = new NumericPasswordBox()
            {
                Watermark = UiResources.UI_NewPinLock.ToLower(),
                ClearButtonVisibility = Visibility.Collapsed
            };
            pinLock.PasswordChanged += (sender, args) =>
            {
                warningMessage.Text = string.Empty;
            };
            contentStackPanel.Children.Add(pinLock);

            var confirmPinLock = new NumericPasswordBox()
            {
                Watermark = UiResources.UI_ConfirmPinLock.ToLower(),
                ClearButtonVisibility = Visibility.Collapsed
            };
            confirmPinLock.PasswordChanged += (sender, args) =>
            {
                warningMessage.Text = string.Empty;
            };
            contentStackPanel.Children.Add(confirmPinLock);

            contentStackPanel.Children.Add(warningMessage);

            this.MainGrid.Children.Add(contentStackPanel);
            Grid.SetRow(contentStackPanel, 2);

            this.OkButtonTapped += (sender, args) =>
            {
                if (isChange)
                {
                    if (currentPinLock != null)
                    {
                        string hashValue = CryptoService.HashData(currentPinLock.Password);

                        if (!hashValue.Equals(SettingsService.LoadSetting<string>(SettingsResources.UserPinLock)))
                        {
                            currentPinLock.Focus();
                            warningMessage.Text = AppMessages.CurrentPinLockCodeDoNotMatch;
                            return;
                        }
                    }
                }

                if (pinLock.Password.Length < 4)
                {
                    pinLock.Focus();
                    warningMessage.Text = AppMessages.PinLockTooShort;
                    return;
                }

                if (!pinLock.Password.Equals(confirmPinLock.Password))
                {
                    confirmPinLock.Focus();
                    warningMessage.Text = AppMessages.PinLockCodesDoNotMatch;
                    return;
                }

                SettingsService.SaveSetting(SettingsResources.UserPinLock, CryptoService.HashData(pinLock.Password));
                SettingsService.SaveSetting(SettingsResources.UserPinLockIsEnabled, true);

                App.AppInformation.HasPinLockIntroduced = true;

                this.DialogResult = true;

                this.CloseDialog();
            };

            this.CancelButtonTapped += (sender, args) =>
            {
                this.CloseDialog();
            };
        }
    }
}
