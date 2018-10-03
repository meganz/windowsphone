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
                HorizontalAlignment = HorizontalAlignment.Left,
                Opacity = 0.8,
                Text = message ?? AppMessages.AM_2FA_InputAppCodeDialogMessage,
                TextAlignment = TextAlignment.Left,
                TextWrapping = TextWrapping.Wrap
            };
            contentStackPanel.Children.Add(description);

            this.CreateDigitInput();
            contentStackPanel.Children.Add(this.verificationCode);

            Grid subGrid = new Grid()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                MinHeight = 48,
                RowDefinitions =
                {
                    new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) },
                    new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) }
                }
            };

            this.progressBar = new ProgressBar()
            {
                Width = 160,
                Foreground = (Brush)Application.Current.Resources["MegaRedColorBrush"],
                HorizontalAlignment = HorizontalAlignment.Center,
                IsIndeterminate = true,
                Visibility = Visibility.Collapsed
            };
            subGrid.Children.Add(this.progressBar);
            Grid.SetRow(this.progressBar, 0);

            this.warningMessageGrid = this.CreateErrorMessage();
            subGrid.Children.Add(this.warningMessageGrid);
            Grid.SetRow(this.warningMessageGrid, 0);
            
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
                subGrid.Children.Add(lostAuthDeviceLink);
                Grid.SetRow(lostAuthDeviceLink, 1);
            }

            contentStackPanel.Children.Add(subGrid);

            this.MainGrid.Children.Add(contentStackPanel);
            Grid.SetRow(contentStackPanel, 2);
        }

        private void CreateDigitInput()
        {
            var inputScope = new InputScope();
            var inputScopeName = new InputScopeName();
            inputScopeName.NameValue = InputScopeNameValue.Number;
            inputScope.Names.Add(inputScopeName);

            this.verificationCode = new RadTextBox()
            {
                Margin = new Thickness(-12, 0, -12, 0),
                FontSize = Convert.ToDouble(Application.Current.Resources["PhoneFontSizeMediumLarge"]),
                InputScope = inputScope,
                MaxLength = 6,
                TextAlignment = TextAlignment.Center,
                Watermark = UiResources.UI_SixDigitCode,
            };
            this.verificationCode.TextChanged += OnTextChanged;
            this.verificationCode.KeyDown += OnInputTextBoxKeyDown;
            this.verificationCode.Loaded += ((sender, args) => this.verificationCode.Focus());
        }

        private void OnTextChanged(object sender, EventArgs e)
        {
            this.verificationCode.TextChanged -= OnTextChanged;
            this.verificationCode.Foreground = (Brush)Application.Current.Resources["PhoneTextBoxForegroundBrush"];
            this.verificationCode.TextChanged += OnTextChanged;

            this.IsWarningMessageVisible = false;
            this.isValidVerifyCode = !string.IsNullOrWhiteSpace(this.verificationCode.Text) &&
                this.verificationCode.Text.Length == this.verificationCode.MaxLength;

            if (!this.isValidVerifyCode) return;
            this.Verify();
        }

        private void OnInputTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key >= Key.D0 && e.Key <= Key.D9) ||
                (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
            {
                e.Handled = false;
                return;
            }

            if (this.isValidVerifyCode && e.Key == Key.Enter)
                this.Verify();

            e.Handled = true;
        }

        private Grid CreateErrorMessage()
        {
            var errorGrid = new Grid()
            {
                Margin = new Thickness(0,4,0,4),
                HorizontalAlignment = HorizontalAlignment.Center,
                ColumnDefinitions =
                {
                    new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) },
                    new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) }
                }
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

            errorGrid.Children.Add(errorViewBox);
            Grid.SetColumn(errorViewBox, 0);

            warningMessageTextBlock = new TextBlock()
            {
                Style = (Style)Application.Current.Resources["MegaErrorFontStyle"],
                Text = AppMessages.AM_InvalidCode,
                Visibility = Visibility.Collapsed
            };
            errorGrid.Children.Add(warningMessageTextBlock);
            Grid.SetColumn(warningMessageTextBlock, 1);

            return errorGrid;
        }

        private async void Verify()
        {
            if (this.dialogAction != null || this.dialogActionAsync != null)
            {
                this.progressBar.Visibility = Visibility.Visible;

                this.DialogResult = false;
                if (this.dialogAction != null)
                    this.DialogResult = this.dialogAction.Invoke(this.verificationCode.Text);

                if (this.dialogActionAsync != null)
                    this.DialogResult = await this.dialogActionAsync.Invoke(this.verificationCode.Text);

                this.progressBar.Visibility = Visibility.Collapsed;

                if (!this.DialogResult)
                {
                    this.verificationCode.Foreground = (Brush)Application.Current.Resources["MegaRedColorBrush"];
                    return;
                }

                base.CloseDialog();
            }
        }

        private void LostAuthDevice(object obj = null)
        {
            var webBrowserTask = new WebBrowserTask { Uri = new Uri(AppResources.AR_RecoveryUrl) };
            webBrowserTask.Show();
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command invoked when the user tap the "lost authentication device" link
        /// </summary>
        public ICommand LostAuthDeviceCommand { get; private set; }

        #endregion

        #region Controls

        private RadTextBox verificationCode;
        private Grid warningMessageGrid;
        private TextBlock warningMessageTextBlock;
        private Path warningIcon;
        private ProgressBar progressBar;

        #endregion

        #region Properties

        private Func<string, bool> dialogAction;
        private Func<string, Task<bool>> dialogActionAsync;

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

        /// <summary>
        /// Indicates if the typed verify code has the right format and can be verified
        /// </summary>
        private bool isValidVerifyCode;

        #endregion
    }
}
