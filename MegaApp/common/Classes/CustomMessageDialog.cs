using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using MegaApp.Extensions;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;
using Telerik.Windows.Controls;

namespace MegaApp.Classes
{
    /// <summary>
    /// Class that provides functionality to show the user a MEGA styled message dialog
    /// </summary>
    public class CustomMessageDialog
    {
        #region Events

        public event EventHandler OkOrYesButtonTapped;
        public event EventHandler CancelOrNoButtonTapped;

        #endregion

        #region Controls
        
        protected RadWindow DialogWindow { get; set; }
        protected IList<Button> Buttons { get; set; }  // Visible buttons on the message dialog

        #endregion

        #region Private Properties

        private readonly string _title;
        private readonly string _message;
        private readonly AppInformation _appInformation;
        private readonly IList<DialogButton> _buttons;
        private readonly Orientation _buttonOrientation;
        private TaskCompletionSource<MessageDialogResult> _taskCompletionSource;
        private Path _image;

        #endregion


        /// <summary>
        /// Create a CustomMessageDialog
        /// </summary>
        /// <param name="title">Title of the dialog</param>
        /// <param name="message">Main message of the dialog</param>
        /// <param name="appInformation">App information for restricting number of dialogs</param>
        /// <param name="messageDialogImage">Extra image to display in top of dialog to the user. Default no image</param>
        public CustomMessageDialog(string title, string message, AppInformation appInformation,
            MessageDialogImage messageDialogImage = MessageDialogImage.None)
        {
            _title = title;
            _message = message;
            _appInformation = appInformation;

            // Default orientatien is vertical
            _buttonOrientation = Orientation.Vertical;

            // Create a default Ok button for this minimal dialog
            _buttons = new List<DialogButton> { DialogButton.GetOkButton() };

            // Set default result to Cancel or No
            this.DialogResult = MessageDialogResult.CancelNo;

            SetDialogImage(messageDialogImage);
        }

        /// <summary>
        /// Create a CustomMessageDialog
        /// </summary>
        /// <param name="title">Title of the dialog</param>
        /// <param name="message">Main message of the dialog</param>
        /// <param name="appInformation">App information for restricting number of dialogs</param>
        /// <param name="dialogButtons">A value that indicaties the button or buttons to display</param>
        /// <param name="messageDialogImage">Extra image to display in top of dialog to the user. Default no image</param>
        public CustomMessageDialog(string title, string message, AppInformation appInformation,
            MessageDialogButtons dialogButtons, MessageDialogImage messageDialogImage = MessageDialogImage.None)
        {
            _title = title;
            _message = message;
            _appInformation = appInformation;

            // Default orientatien is horizontal when using button(s) enumeration
            _buttonOrientation = Orientation.Horizontal;
            _buttons = new List<DialogButton>();
            
            // Create buttons defined in the constructor
            switch (dialogButtons)
            {
                case MessageDialogButtons.Ok:
                    _buttons.Add(DialogButton.GetOkButton());
                    break;
                case MessageDialogButtons.OkCancel:
                    _buttons.Add(DialogButton.GetOkButton());
                    _buttons.Add(DialogButton.GetCancelButton());
                    break;
                case MessageDialogButtons.YesNo:
                    _buttons.Add(DialogButton.GetYesButton());
                    _buttons.Add(DialogButton.GetNoButton());
                    break;
                default:
                    throw new ArgumentOutOfRangeException("dialogButtons", dialogButtons, null);
            }

            // Set default result to Cancel or No
            this.DialogResult = MessageDialogResult.CancelNo;

            SetDialogImage(messageDialogImage);
        }

        /// <summary>
        /// Create a CustomMessageDialog
        /// </summary>
        /// <param name="title">Title of the dialog</param>
        /// <param name="message">Main message of the dialog</param>
        /// <param name="appInformation">App information for restricting number of dialogs</param>
        /// <param name="dialogButtons">A value that indicaties the button or buttons to display</param>
        /// <param name="buttonOrientation">Show buttons on a horizntal row or vertical below each other</param>
        /// <param name="messageDialogImage">Extra image to display in top of dialog to the user. Default no image</param>
        public CustomMessageDialog(string title, string message, AppInformation appInformation,
           IEnumerable<DialogButton> dialogButtons, Orientation buttonOrientation = Orientation.Horizontal,
           MessageDialogImage messageDialogImage = MessageDialogImage.None)
        {
            _title = title;
            _message = message;
            _appInformation = appInformation;
            _buttonOrientation = buttonOrientation;
            _buttons = new List<DialogButton>(dialogButtons);

            // Set default result to custom when using defined buttons
            this.DialogResult = MessageDialogResult.Custom;

            SetDialogImage(messageDialogImage);
        }

        /// <summary>
        /// Display the CustomMessageDialog on screen with the specified parameter from the constructor
        /// </summary>
        public void ShowDialog()
        {
            // Create the base canvas that will hold all the controls and application bar functions
            DialogWindow = new RadModalWindow()
            {
                Name = "CustomMessageDialog",
                Background = new SolidColorBrush(Color.FromArgb(155, 31, 31, 31)),
                WindowSizeMode = WindowSizeMode.FitToPlacementTarget,
                VerticalContentAlignment = VerticalAlignment.Top, // Message dialog dispays top of screen
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                IsClosedOnOutsideTap = false, // Only close on back button press or button tap
                OpenAnimation = AnimationService.GetOpenMessageDialogAnimation(), 
                CloseAnimation = AnimationService.GetCloseMessageDialogAnimation(), // need to slide out nicely
            };

            DialogWindow.WindowOpening += (sender, args) => DialogService.DialogOpening(args);
            DialogWindow.WindowClosed += (sender, args) => DialogService.DialogClosed();

            // Create a Grid to populate with UI controls
            var mainGrid = new Grid()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                RowDefinitions =
                {
                    new RowDefinition() { Height = GridLength.Auto}, // Optional Image row
                    new RowDefinition() { Height = GridLength.Auto}, // Title row
                    new RowDefinition() { Height = GridLength.Auto}, // Message row
                    new RowDefinition() { Height = GridLength.Auto}, // Response control(s) row
                },
                Margin = new Thickness(24, 0, 24, 0)
            };


            if (_image != null)
            {
                // Add image to the view
                mainGrid.Children.Add(_image);
                Grid.SetRow(_image, 0);
            }

            // Create title label
            var title = new TextBlock()
            {
                Text = _title.ToUpper(), // The specified title string in uppercase always
                FontFamily = new FontFamily("Segoe WP Semibold"),
                FontSize = Convert.ToDouble(Application.Current.Resources["PhoneFontSizeMedium"]),
                Foreground = new SolidColorBrush((Color)Application.Current.Resources["PhoneForegroundColor"]),
                Margin = _image == null ? new Thickness(0, 24, 0, 0) : new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            
            // Add title to the view
            mainGrid.Children.Add(title);
            Grid.SetRow(title, 1);

            // Create message label
            var message = new TextBlock()
            {
                Text = _message, // The specified message
                FontFamily = new FontFamily("Segoe WP SemiLight"),
                FontSize = Convert.ToDouble(Application.Current.Resources["PhoneFontSizeMediumLarge"]),
                Foreground = new SolidColorBrush((Color)Application.Current.Resources["PhoneForegroundColor"]),
                Margin = _image == null ? new Thickness(0, 50, 0, 48) : new Thickness(0, 36, 0, 36),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                TextWrapping = TextWrapping.Wrap // Needed for long strings / texts
            };

            // Add message to the view
            mainGrid.Children.Add(message);
            Grid.SetRow(message, 2);

            // Create response controls panel
            var buttonGrid = new Grid()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(-12, 0, -12, 24)
            };

            // Add response controls to the view
            mainGrid.Children.Add(buttonGrid);
            Grid.SetRow(buttonGrid, 3);

            // Create the response control buttons
            foreach (var dialogButton in _buttons)
            {
                var button = new Button()
                {
                    Content = dialogButton.Text.ToLower(), // always lower case, modern ui design guideline
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };

                // Focus the button(s) to let the virtual keyboard slide down when it was open
                button.Loaded += (sender, args) => button.Focus();
                
                var dlgButton = dialogButton;
                button.Tap += (sender, args) =>
                {
                    if (dlgButton.TapAction != null)
                    {
                        // Invoke action after the close animation of the dialog has finished
                        // Used for nice effect of sliding out and see the action
                        DialogWindow.CloseAnimation.Ended += (o, eventArgs) =>
                        {
                            dlgButton.TapAction.Invoke();
                        };
                       
                        // If tap action is defined the dialog result is a custom result
                        this.DialogResult = MessageDialogResult.Custom;

                        // Set result for ShowDialogAsync invocation
                        if (_taskCompletionSource != null)
                            _taskCompletionSource.TrySetResult(this.DialogResult);

                        DialogWindow.IsOpen = false;
                    }
                    else
                    {
                        switch (dlgButton.Type)
                        {
                            case MessageDialogButton.Ok:
                            case MessageDialogButton.Yes:
                                this.DialogResult = MessageDialogResult.OkYes;
                                // Set result for ShowDialogAsync invocation
                                if (_taskCompletionSource != null)
                                    _taskCompletionSource.TrySetResult(this.DialogResult);
                                OnOkOrYesButtonTapped(new EventArgs());
                                break;
                            case MessageDialogButton.Cancel:
                            case MessageDialogButton.No:
                                this.DialogResult = MessageDialogResult.CancelNo;
                                // Set result for ShowDialogAsync invocation
                                if (_taskCompletionSource != null)
                                    _taskCompletionSource.TrySetResult(this.DialogResult);
                                OnCancelOrNoButtonTapped(new EventArgs());
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        // Set result for ShowDialogAsync invocation
                        if (_taskCompletionSource != null)
                            _taskCompletionSource.TrySetResult(this.DialogResult);
                    }
                    
                };

                buttonGrid.Children.Add(button);

                switch (_buttonOrientation)
                {
                    case Orientation.Vertical:
                        buttonGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                        Grid.SetRow(button, buttonGrid.RowDefinitions.Count - 1);
                        break;
                    case Orientation.Horizontal:
                        buttonGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                        Grid.SetColumn(button, buttonGrid.ColumnDefinitions.Count - 1);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
            }

            // Used to color the dialog content backgroud
            var border = new Border
            {
                Background = new SolidColorBrush((Color) Application.Current.Resources["PhoneChromeColor"]),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Child = mainGrid,
            };
            
            // Set the content on the canvas and show the dialog
            DialogWindow.Content = border;
            DialogWindow.IsOpen = true;
        }

        /// <summary>
        /// Display the CustomMessageDialog on screen with the specified parameter from the constructor
        /// </summary>
        /// <returns>Message dialog result specified by user button tap action</returns>
        public Task<MessageDialogResult> ShowDialogAsync()
        {
            // Needed to make a awaitable task
            _taskCompletionSource = new TaskCompletionSource<MessageDialogResult>();
            
            // Invoke the main dialog method
            ShowDialog();
            
            // Return awaitable task
            return _taskCompletionSource.Task;
        }

        #region Private Methods

        private void SetDialogImage(MessageDialogImage messageDialogImage)
        {
            var iconPath = new Path()
            {
                Height = 120,
                Width = 120,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 24, 0, 40),
                Fill = new SolidColorBrush((Color)Application.Current.Resources["PhoneForegroundColor"])
            };
            iconPath.Fill.Opacity = 0.2;

            switch (messageDialogImage)
            {
                case MessageDialogImage.None:
                    _image = null;
                    break;
                case MessageDialogImage.RubbishBin:
                    iconPath.SetDataBinding(VisualResources.DialogRubbishBinPathData);
                    _image = iconPath;
                    break;
                case MessageDialogImage.NoInternetConnection:
                    iconPath.SetDataBinding(VisualResources.NoInternetConnectionViewPathData);
                    _image = iconPath;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("messageDialogImage", messageDialogImage, null);
            }
        }

        #endregion

        #region Virtual Methods

        /// <summary>
        /// Ok or Yes button has been tapped
        /// </summary>
        /// <param name="args">Default event arguments</param>
        protected virtual void OnOkOrYesButtonTapped(EventArgs args)
        {
            if (OkOrYesButtonTapped != null)
                OkOrYesButtonTapped(this, args);

            // Close and remove the dialog
            DialogWindow.IsOpen = false;
        }

        /// <summary>
        /// Cancel or No button has been tapped
        /// </summary>
        /// <param name="args">Default event arguments</param>
        protected virtual void OnCancelOrNoButtonTapped(EventArgs args)
        {
            if (CancelOrNoButtonTapped != null)
                CancelOrNoButtonTapped(this, args);

            // Close and remove the dialog
            DialogWindow.IsOpen = false;
        }

        #endregion


        #region Public Properties

        /// <summary>
        /// Result of the dialog. Defined by the user action
        /// </summary>
        public MessageDialogResult DialogResult { get; private set; }

        #endregion

    }

    
    /// <summary>
    /// Class that defines the properties of a CustomMessageDialog button
    /// </summary>
    public class DialogButton
    {
        /// <summary>
        /// Create a CustomMessageDialog button
        /// </summary>
        /// <param name="text">Text content of the button</param>
        /// <param name="tapAction">Action to invoke on button tap</param>
        public DialogButton(string text, Action tapAction)
        {
            this.Text = text;
            this.TapAction = tapAction;
        }

        /// <summary>
        /// Create a default Ok button
        /// </summary>
        /// <returns>Return a default message dialog Ok button</returns>
        public static DialogButton GetOkButton()
        {
            var result = new DialogButton(UiResources.Ok, null)
            {
                Type = MessageDialogButton.Ok
            };
            return result;
        }

        /// <summary>
        /// Create a default Cancel button
        /// </summary>
        /// <returns>Return a default message dialog Cancel button</returns>
        public static DialogButton GetCancelButton()
        {
            var result = new DialogButton(UiResources.Cancel, null)
            {
                Type = MessageDialogButton.Cancel
            };
            return result;
        }

        /// <summary>
        /// Create a default Yes button
        /// </summary>
        /// <returns>Return a default message dialog Yes button</returns>
        public static DialogButton GetYesButton()
        {
            var result = new DialogButton(UiResources.Yes, null)
            {
                Type = MessageDialogButton.Yes
            };
            return result;
        }

        /// <summary>
        /// Create a default No button
        /// </summary>
        /// <returns>Return a default message dialog No button</returns>
        public static DialogButton GetNoButton()
        {
            var result = new DialogButton(UiResources.No, null)
            {
                Type = MessageDialogButton.No
            };
            return result;
        }

        #region Properties
        
        public string Text { get; private set; }

        public Action TapAction { get; private set; }

        public MessageDialogButton Type { get; private set; }

        #endregion
    }

    public enum MessageDialogButton
    {
        /// <summary>
        /// The Ok button 
        /// </summary>
        Ok,
        /// <summary>
        /// The Cancel button
        /// </summary>
        Cancel,
        /// <summary>
        /// The Yes button
        /// </summary>
        Yes,
        /// <summary>
        /// The No button
        /// </summary>
        No,
    }

    public enum MessageDialogButtons
    {
        /// <summary>
        /// Displays only an Ok button
        /// </summary>
        Ok,
        /// <summary>
        /// Displays an Ok and Cancel button
        /// </summary>
        OkCancel,
        /// <summary>
        /// Displays a Yes and No button
        /// </summary>
        YesNo,
    }

    public enum MessageDialogResult
    {
        /// <summary>
        /// User has pressed Ok or Yes
        /// </summary>
        OkYes,
        /// <summary>
        /// User has pressed Cancel or No
        /// </summary>
        CancelNo,
        /// <summary>
        /// User has pressed a custom defined button
        /// </summary>
        Custom,
    }

    public enum MessageDialogImage
    {
        /// <summary>
        /// Display no image
        /// </summary>
        None,
        /// <summary>
        /// Display a rubbish bin image
        /// </summary>
        RubbishBin,
        /// <summary>
        /// Display a no internet connection image
        /// </summary>
        NoInternetConnection
    }
}
