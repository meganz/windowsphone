using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MegaApp.Resources;
using MegaApp.Services;
using Microsoft.Phone.Controls;
using Telerik.Windows.Controls;

namespace MegaApp.Classes
{
    /// <summary>
    /// Class that provides functionality to show the user a MEGA styled input dialog
    /// </summary>
    public class CustomInputDialog
    {
        #region Events

        public event EventHandler<CustomInputDialogOkButtonArgs> OkButtonTapped;
        public event EventHandler CancelButtonTapped;

        #endregion

        #region Controls

        protected TextBox InputControl { get; set; }
        protected RadModalWindow DialogWindow { get; set; }

        #endregion

        #region Private Properties

        private readonly string _title;
        private readonly string _message;
        private readonly CustomInputDialogSettings _settings;
        private readonly AppInformation _appInformation;

        #endregion

        /// <summary>
        /// Create a CustomInputDialog
        /// </summary>
        /// <param name="title">Title of the dialog</param>
        /// <param name="message">Message above the input control</param>
        /// <param name="appInformation">App information for restricting number of dialogs</param>
        /// <param name="settings">Dialog settings to manipulate the dialog look and behavior</param>
        public CustomInputDialog(string title, string message, AppInformation appInformation,
            CustomInputDialogSettings settings = null)
        {
            _title = title;
            _message = message;
            _appInformation = appInformation;
            
            // Set the settings. If none specfied, create a default set
            _settings = settings ?? new CustomInputDialogSettings()
            {
                DefaultText = String.Empty,
                IgnoreExtensionInSelection = false,
                SelectDefaultText = false,
                OverrideCancelButtonText = String.Empty,
                OverrideOkButtonText = String.Empty
            };
        }


        #region Public Methods
        
        /// <summary>
        /// Display the CustomInputDialog on screen with the specified parameter from the constructor
        /// </summary>
        public void ShowDialog()
        {
            // Create the base canvas that will hold all the controls and application bar functions
            DialogWindow = new RadModalWindow()
            {
                Name = "CustomInputDialog",
                ApplicationBarInfo = new ApplicationBarInfo()
                {
                    Buttons  =
                    {
                        CreateOkButton(), // Default ok button with check icon
                        CreateCancelButton(), // Default cancel button with cross icon
                    }
                }, 
                Background = new SolidColorBrush((Color)Application.Current.Resources["PhoneChromeColor"]),
                IsFullScreen = true,
                WindowSizeMode = WindowSizeMode.FitToPlacementTarget,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                IsClosedOnOutsideTap = false, // Only close on back button press or cancel button tap
                OpenAnimation = AnimationService.GetOpenDialogAnimation(),
            };

            // When the open animation of the dialog has ended, select a text selection if specified
            // Do this after the animation because else the dialog will be pushed to far up by the default
            // behavior of the focus action on a textbox by the WP OS when keyboard slides in
            DialogWindow.OpenAnimation.Ended += (sender, args) =>
            {
                // If specified make a default text selection so the user can do his action more quickly
                if (_settings.SelectDefaultText)
                    SetTextSelection(InputControl, _settings.DefaultText, _settings.IgnoreExtensionInSelection);

                // Focus to make the selection visible and push up the keyboard immediately
                InputControl.Focus(); 
            };

            DialogWindow.WindowOpening += (sender, args) => DialogService.DialogOpening(args);
            DialogWindow.WindowClosed += (sender, args) => DialogService.DialogClosed();

            // Create a Grid to populate with UI controls
            var mainGrid = new Grid()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                RowDefinitions =
                {
                    new RowDefinition() { Height = GridLength.Auto}, // Title row
                    new RowDefinition() { Height = GridLength.Auto}, // Message row
                    new RowDefinition() { Height = GridLength.Auto}, // Input control row
                },
                Margin = new Thickness(24, 0, 24, 0)
            };

            // Create title label
            var title = new TextBlock()
            {
                Text = _title.ToUpper(), // The specified title string in uppercase always
                FontFamily = new FontFamily("Segoe WP Semibold"),
                FontSize = Convert.ToDouble(Application.Current.Resources["PhoneFontSizeMediumLarge"]),
                Foreground = new SolidColorBrush((Color)Application.Current.Resources["PhoneForegroundColor"]),
                Margin = new Thickness(0, 28, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                TextWrapping = TextWrapping.Wrap
            };

            // Add title to the view
            mainGrid.Children.Add(title);
            Grid.SetRow(title, 0);

            // Create message label
            var message = new TextBlock()
            {
                Text = _message, // The specified message
                FontFamily = new FontFamily("Segoe WP SemiLight"),
                FontSize = Convert.ToDouble(Application.Current.Resources["PhoneFontSizeSmall"]),
                Foreground = new SolidColorBrush((Color)Application.Current.Resources["PhoneSubtleColor"]),
                Margin = new Thickness(0, 50, 0, 22),
                HorizontalAlignment = HorizontalAlignment.Left,
                TextWrapping = TextWrapping.Wrap
            };

            // Add message to the view
            mainGrid.Children.Add(message);
            Grid.SetRow(message, 1);

            // Create input control
            InputControl = new TextBox()
            {
                Text = _settings.DefaultText, // The specified default text in the textbox control input area
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(-12) // Compensate for WP default padding
            };

            // Add input to the view
            mainGrid.Children.Add(InputControl);
            Grid.SetRow(InputControl, 2);

            // Set the content on the canvas and show the dialog
            DialogWindow.Content = mainGrid;
            DialogWindow.IsOpen = true;
        }

        #endregion

        #region Virtual Methods

        /// <summary>
        /// Ok button has been tapped
        /// </summary>
        /// <param name="args">Argument with the input text at the moment the ok button was selected</param>
        protected virtual void OnOkButtonTapped(CustomInputDialogOkButtonArgs args)
        {
            if (String.IsNullOrWhiteSpace(args.InputText)) return;

            if (OkButtonTapped != null)
                OkButtonTapped(this, args);

            // Close and remove the dialog
            DialogWindow.IsOpen = false;
        }

        /// <summary>
        /// Cancel button has been tapped
        /// </summary>
        /// <param name="args">Default event arguments</param>
        protected virtual void OnCancelButtonTapped(EventArgs args)
        {
            if (CancelButtonTapped != null)
                CancelButtonTapped(this, args);

            // Close and remove the dialog
            DialogWindow.IsOpen = false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Select a text in the specified textbox control
        /// </summary>
        /// <param name="textBox">Textbox control to select a text</param>
        /// <param name="defaultText">The default text that is in the textbox input area</param>
        /// <param name="ignoreExtension">Specifies if any filename extensions should be ignored during selection</param>
        private void SetTextSelection(TextBox textBox, string defaultText, bool ignoreExtension)
        {
            // If no text is provided, no selection is possible
            if (String.IsNullOrEmpty(defaultText)) return;

            // Selection always start at zero (array)
            textBox.SelectionStart = 0;

            // Select the whole text, or just the filename without extension if an extension is available
            string extension = ignoreExtension ? Path.GetExtension(defaultText) : String.Empty;
            textBox.SelectionLength = String.IsNullOrEmpty(extension)
                ? defaultText.Length
                : defaultText.LastIndexOf(extension, StringComparison.InvariantCulture);
        }

        /// <summary>
        /// Create the dialog OK button
        /// </summary>
        /// <returns>Dialog OK application bar button</returns>
        private ApplicationBarButton CreateOkButton()
        {
            var okButton = new ApplicationBarButton(
                String.IsNullOrEmpty(_settings.OverrideOkButtonText) 
                    ? UiResources.Ok 
                    : _settings.OverrideOkButtonText,
                new Uri("/Assets/AppBar/check.png", UriKind.Relative),
                true);

            
            okButton.Click += (sender, args) =>
            {
                OnOkButtonTapped(new CustomInputDialogOkButtonArgs(InputControl.Text));
            };

            return okButton;
        }

        /// <summary>
        /// Create the dialog CANCEL button
        /// </summary>
        /// <returns>Dialog CANCEL application bar button</returns>
        private ApplicationBarButton CreateCancelButton()
        {
            var cancelButton = new ApplicationBarButton(
                String.IsNullOrEmpty(_settings.OverrideCancelButtonText) 
                    ? UiResources.Cancel 
                    : _settings.OverrideCancelButtonText, 
                new Uri("/Assets/AppBar/cancel.png", UriKind.Relative),
                true);

            cancelButton.Click += (sender, args) =>
            {
                OnCancelButtonTapped(new EventArgs());
            };

            return cancelButton;
        }

        #endregion
    }

    /// <summary>
    /// Settings options to use in CustomInputDialog
    /// </summary>
    public class CustomInputDialogSettings
    {
        /// <summary>
        /// Populate textbox control with a default input text
        /// </summary>
        public string DefaultText { get; set; } 

        /// <summary>
        /// Specifies if the default text has any text selection
        /// </summary>
        public bool SelectDefaultText { get; set; }

        /// <summary>
        /// Specifies to ignore file extensions in the text selection
        /// </summary>
        public bool IgnoreExtensionInSelection { get; set; }

        /// <summary>
        /// Overrides the default 'ok' label for the application bar button
        /// </summary>
        public string OverrideOkButtonText { get; set; }

        /// <summary>
        /// Overrides the default 'cancel' label for the application bar button
        /// </summary>
        public string OverrideCancelButtonText { get; set; }
    }

    /// <summary>
    /// Event arguments for the Ok button tapped event in the CustomInputDialog class
    /// </summary>
    public class CustomInputDialogOkButtonArgs : EventArgs
    {
        public CustomInputDialogOkButtonArgs(string inputText)
        {
            InputText = inputText;
        }

        /// <summary>
        /// The input text at the moment the user tapped the ok button
        /// </summary>
        public string InputText { get; private set; }
    }
}
