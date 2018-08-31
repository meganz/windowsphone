using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Telerik.Windows.Controls;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.Dialogs
{
    public abstract class TwoButtonsDialog : MegaDialog
    {
        /// <summary>
        /// Dialog with two buttons ('ok' and 'cancel' by default)
        /// </summary>
        /// <param name="title">Title of the dialog.</param>
        /// <param name="settings">Settings options.</param>
        public TwoButtonsDialog(string title = null, TwoButtonsDialogSettings settings = null) 
            : base(title)
        {
            // Set the settings. If none specfied, create a default set
            var _settings = settings ?? new TwoButtonsDialogSettings();

            var okButtonText = string.IsNullOrWhiteSpace(_settings.OverrideOkButtonText) ?
                    UiResources.Ok : _settings.OverrideOkButtonText;

            var cancelButtonText = string.IsNullOrWhiteSpace(_settings.OverrideCancelButtonText) ?
                    UiResources.Cancel : _settings.OverrideCancelButtonText;

            if (!_settings.ApplicationBarButtons)
            {
                this.CreateButtons(okButtonText, cancelButtonText);
                return;
            }

            // Set the default aspect in case of application bar buttons presence
            this.IsFullScreen = true;
            this.VerticalContentAlignment = VerticalAlignment.Stretch;
            this.OpenAnimation = AnimationService.GetOpenDialogAnimation();
            this.CloseAnimation = AnimationService.GetCloseDialogAnimation();

            this.MainGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.MainGrid.VerticalAlignment = VerticalAlignment.Stretch;

            this.ApplicationBarInfo = new ApplicationBarInfo()
            {
                Buttons =
                {
                    CreateOkApplicationBarButton(okButtonText),         // Default ok button with check icon
                    CreateCancelApplicationBarButton(cancelButtonText), // Default cancel button with cross icon
                }
            };
        }

        #region Events

        /// <summary>
        /// Event triggered when the OK button is tapped.
        /// </summary>
        public event EventHandler OkButtonTapped;

        /// <summary>
        /// Event invocator method called when the OK button is tapped
        /// </summary>
        /// <param name="args">Default event arguments.</param>
        protected virtual void OnOkButtonTapped()
        {
            if (OkButtonTapped != null)
                OkButtonTapped.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event triggered when the CANCEL button is tapped.
        /// </summary>
        public event EventHandler CancelButtonTapped;

        /// <summary>
        /// Event invocator method called when the CANCEL button is tapped
        /// </summary>
        /// <param name="args">Default event arguments.</param>
        protected virtual void OnCancelButtonTapped()
        {
            if (CancelButtonTapped != null)
                CancelButtonTapped(this, EventArgs.Empty);
        }

        #endregion

        #region Methods

        private void CreateButtons(string okButtonText, string cancelButtonText)
        {
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

            var okButton = new Button()
            {
                Content = okButtonText.ToLower(),
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            okButton.Tap += (sender, args) => OnOkButtonTapped();

            var cancelButton = new Button()
            {
                Content = cancelButtonText.ToLower(),
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            cancelButton.Tap += (sender, args) => OnCancelButtonTapped();

            buttonsGrid.Children.Add(okButton);
            buttonsGrid.Children.Add(cancelButton);
            Grid.SetColumn(okButton, 0);
            Grid.SetColumn(cancelButton, 1);

            this.MainGrid.Children.Add(buttonsGrid);
            Grid.SetRow(buttonsGrid, 3);
        }

        /// <summary>
        /// Create the dialog OK button
        /// </summary>
        /// <param name="text">Text for the button label.</param>
        /// <returns>Dialog OK application bar button</returns>
        private ApplicationBarButton CreateOkApplicationBarButton(string text)
        {
            var okButton = new ApplicationBarButton(text,
                new Uri("/Assets/AppBar/check.png", UriKind.Relative),
                true);

            okButton.Click += (sender, args) => OnOkButtonTapped();

            return okButton;
        }

        /// <summary>
        /// Create the dialog CANCEL button.
        /// </summary>
        /// <param name="text">Text for the button label.</param>
        /// <returns>Dialog CANCEL application bar button.</returns>
        private ApplicationBarButton CreateCancelApplicationBarButton(string text)
        {
            var cancelButton = new ApplicationBarButton(text,
                new Uri("/Assets/AppBar/cancel.png", UriKind.Relative),
                true);

            cancelButton.Click += (sender, args) => OnCancelButtonTapped();

            return cancelButton;
        }        

        #endregion
    }

    /// <summary>
    /// Settings options to use in TwoButtonsDialog.
    /// </summary>
    public class TwoButtonsDialogSettings
    {
        /// <summary>
        /// Specifies if use the default application bar buttons or not.
        /// </summary>
        public bool ApplicationBarButtons = false;

        /// <summary>
        /// Overrides the default 'ok' label for the application bar button.
        /// </summary>
        public string OverrideOkButtonText = string.Empty;

        /// <summary>
        /// Overrides the default 'cancel' label for the application bar button.
        /// </summary>
        public string OverrideCancelButtonText = string.Empty;
    }
}
