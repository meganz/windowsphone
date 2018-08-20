using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Phone.Tasks;
using Telerik.Windows.Controls;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.ViewModels;

namespace MegaApp.Dialogs
{
    public class MultiFactorAuthCodeInputDialog : MegaDialog
    {
        /// <summary>
        /// Creates an input dialog to type the MFA 6-digit code and also executes an action.
        /// </summary>
        /// <param name="dialogAction">Action to execute by the primary button.</param>
        /// <param name="title">Custom title of the input dialog.</param>
        /// <param name="message">Custom message of the input dialog.</param>
        /// <param name="showLostDeviceLink">Indicates if show the lost device link or not.</param>
        public MultiFactorAuthCodeInputDialog(Func<string, bool> dialogAction,
            string title = null, string message = null, bool showLostDeviceLink = true)
            : base(title ?? UiResources.UI_TwoFactorAuth)
        {
            this.dialogAction = dialogAction;
            this.Initialize(message, showLostDeviceLink);
        }

        /// <summary>
        /// Creates an input dialog to type the MFA 6-digit code and also executes an action.
        /// </summary>
        /// <param name="dialogActionAsync">Async action to execute by the primary button.</param>
        /// <param name="title">Custom title of the input dialog.</param>
        /// <param name="message">Custom message of the input dialog.</param>
        /// <param name="showLostDeviceLink">Indicates if show the lost device link or not.</param>
        public MultiFactorAuthCodeInputDialog(Func<string, Task<bool>> dialogActionAsync,
            string title = null, string message = null, bool showLostDeviceLink = true)
            : base(title ?? UiResources.UI_TwoFactorAuth)
        {
            this.dialogActionAsync = dialogActionAsync;
            this.Initialize(message, showLostDeviceLink);
        }

        #region Methods

        private void Initialize(string message, bool showLostDeviceLink)
        {
            this.VerifyCommand = new DelegateCommand(this.Verify);
            this.LostAuthDeviceCommand = new DelegateCommand(this.LostAuthDevice);

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
                Margin = new Thickness(0, 0, 0, 32),
                HorizontalAlignment = HorizontalAlignment.Left,
                Opacity = 0.8,
                Text = message ?? AppMessages.AM_2FA_InputAppCodeDialogMessage,
                TextAlignment = TextAlignment.Left,
                TextWrapping = TextWrapping.Wrap
            };
            contentStackPanel.Children.Add(description);

            this.verificationCode = this.CreateDigitInput();
            contentStackPanel.Children.Add(this.verificationCode);

            this.warningMessageStackPanel = this.CreateErrorMessage();
            contentStackPanel.Children.Add(this.warningMessageStackPanel);

            this.verifyButton = new Button()
            {
                Margin = new Thickness(-12,0,-12,0),
                Content = UiResources.UI_Verify,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Command = this.VerifyCommand,
                IsEnabled = false
            };
            contentStackPanel.Children.Add(this.verifyButton);

            if (showLostDeviceLink)
            {
                var lostAuthDeviceLink = new HyperlinkButton()
                {
                    Margin = new Thickness(0, 28, 0, 28),
                    Command = LostAuthDeviceCommand,
                    Content = UiResources.UI_LostAuthDeviceQuestion,
                    FontSize = Convert.ToDouble(Application.Current.Resources["PhoneFontSizeSmall"]),
                    Foreground = (Brush)Application.Current.Resources["MegaRedColorBrush"],
                    Style = (Style)Application.Current.Resources["HyperlinkButtonStyle"]
                };
                contentStackPanel.Children.Add(lostAuthDeviceLink);
            }

            this.MainGrid.Children.Add(contentStackPanel);
            Grid.SetRow(contentStackPanel, 2);
        }

        private RadTextBox CreateDigitInput()
        {
            var inputScope = new InputScope();
            var inputScopeName = new InputScopeName();
            inputScopeName.NameValue = InputScopeNameValue.Number;
            inputScope.Names.Add(inputScopeName);

            var digitInput = new RadTextBox()
            {
                Margin = new Thickness(-12, 0, -12, 0),
                FontSize = Convert.ToDouble(Application.Current.Resources["PhoneFontSizeMediumLarge"]),
                InputScope = inputScope,
                MaxLength = 6,
                TextAlignment = TextAlignment.Center,
                Watermark = UiResources.UI_SixDigitCode,
            };
            digitInput.TextChanged += (sender, args) =>
            {
                this.verificationCode.Foreground = (Brush)Application.Current.Resources["PhoneTextBoxForegroundBrush"];
                this.IsWarningMessageVisible = false;
                this.verifyButton.IsEnabled = NetworkService.IsNetworkAvailable() &&
                    !string.IsNullOrWhiteSpace(this.verificationCode.Text) &&
                    this.verificationCode.Text.Length == this.verificationCode.MaxLength;
            };
            digitInput.KeyDown += OnInputTextBoxKeyDown;
            
            return digitInput;
        }

        private void OnInputTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key >= Key.D0 && e.Key <= Key.D9) ||
                (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
            {
                e.Handled = false;
                return;
            }

            if (this.verifyButton.IsEnabled && e.Key == Key.Enter)
                this.Verify();

            e.Handled = true;
        }

        private StackPanel CreateErrorMessage()
        {
            var errorStackPanel = new StackPanel()
            {
                Margin = new Thickness(0,4,0,4),
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var errorViewBox = new Viewbox()
            {
                Margin = new Thickness(0,0,12,0),
                Width = 20,
                Height = 20,
            };
            this.warningIcon = new Path()
            {
                Data = (Geometry)XamlReader.Load(
                        "<Geometry xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>"
                        + VisualResources.VR_WarningIconPathData + "</Geometry>"),
                Fill = (Brush)Application.Current.Resources["MegaRedColorBrush"],
                Visibility = Visibility.Collapsed
            };
            errorViewBox.Child = this.warningIcon;

            errorStackPanel.Children.Add(errorViewBox);

            warningMessageTextBlock = new TextBlock()
            {
                Style = (Style)Application.Current.Resources["MegaErrorFontStyle"],
                Text = AppMessages.AM_InvalidCode,
                Visibility = Visibility.Collapsed
            };
            errorStackPanel.Children.Add(warningMessageTextBlock);

            return errorStackPanel;
        }

        private async void Verify(object obj = null)
        {
            if (this.dialogAction != null || this.dialogActionAsync != null)
            {
                this.verifyButton.IsEnabled = false;

                dialogResult = false;
                if (this.dialogAction != null)
                    dialogResult = this.dialogAction.Invoke(this.verificationCode.Text);

                if (this.dialogActionAsync != null)
                    dialogResult = await this.dialogActionAsync.Invoke(this.verificationCode.Text);

                this.verifyButton.IsEnabled = true;

                if (!dialogResult)
                {
                    this.verificationCode.Foreground = (Brush)Application.Current.Resources["MegaRedColorBrush"];
                    return;
                }

                if (this.TaskCompletionSource != null)
                    this.TaskCompletionSource.TrySetResult(true);

                base.CloseDialog();
            }
        }

        private void LostAuthDevice(object obj = null)
        {
            var webBrowserTask = new WebBrowserTask { Uri = new Uri(AppResources.AR_RecoveryUrl) };
            webBrowserTask.Show();
        }

        protected override bool OnWindowClosing()
        {
            if (!this.dialogResult)
            {
                if (this.TaskCompletionSource != null)
                    this.TaskCompletionSource.TrySetResult(false);
            }

            return base.OnWindowClosing();
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command invoked to verify the Multi-Factor code
        /// </summary>
        public ICommand VerifyCommand { get; private set; }

        /// <summary>
        /// Command invoked when the user tap the "lost authentication device" link
        /// </summary>
        public ICommand LostAuthDeviceCommand { get; private set; }

        #endregion

        #region Controls

        private RadTextBox verificationCode;
        private StackPanel warningMessageStackPanel;
        private TextBlock warningMessageTextBlock;
        private Path warningIcon;
        private Button verifyButton;

        #endregion

        #region Properties

        private Func<string, bool> dialogAction;
        private Func<string, Task<bool>> dialogActionAsync;

        private bool dialogResult;

        public bool IsWarningMessageVisible
        {
            set 
            { 
                warningIcon.Visibility = warningMessageTextBlock.Visibility = 
                    value ? Visibility.Visible : Visibility.Collapsed; 
            }
        }

        public string WarningMessageText
        {
            get { return warningMessageTextBlock.Text; }
            set { warningMessageTextBlock.Text = value; }
        }

        #endregion
    }
}
