using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Telerik.Windows.Controls;
using MegaApp.Models;
using MegaApp.Services;

namespace MegaApp.Dialogs
{
    public abstract class MegaDialog : RadModalWindow
    {
        public MegaDialog(string title)
            : base()
        {
            this.CloseCommand = new DelegateCommand(this.CloseDialog);

            this.IsFullScreen = false;
            this.Background = new SolidColorBrush(Color.FromArgb(155, 31, 31, 31));
            this.WindowSizeMode = WindowSizeMode.FitToPlacementTarget;
            this.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            this.VerticalContentAlignment = VerticalAlignment.Top;
            this.IsAnimationEnabled = true;
            this.IsClosedOnBackButton = true;
            this.IsClosedOnOutsideTap = true;
            this.OpenAnimation = AnimationService.GetOpenMessageDialogAnimation();
            this.CloseAnimation = AnimationService.GetCloseMessageDialogAnimation();

            this.WindowOpening += (sender, args) => DialogOpening(args);
            this.WindowClosed += (sender, args) => DialogClosed();

            this.MainGrid = new Grid()
            {
                Background = new SolidColorBrush((Color)Application.Current.Resources["PhoneChromeColor"]),
                Width = Double.NaN,
                RowDefinitions =
                {
                    new RowDefinition() { Height = GridLength.Auto }, // Title row
                    new RowDefinition() { Height = GridLength.Auto }, // Content row
                    new RowDefinition() { Height = GridLength.Auto }, // Buttons row
                }
            };

            this.Title = new TextBlock()
            {
                Text = title.ToUpper(),
                FontFamily = new FontFamily("Segoe WP Semibold"),
                FontSize = Convert.ToDouble(Application.Current.Resources["PhoneFontSizeMedium"]),
                Margin = new Thickness(20, 12, 20, 20),
                TextWrapping = TextWrapping.Wrap
            };

            this.MainGrid.Children.Add(this.Title);
            Grid.SetRow(this.Title, 0);

            this.Content = this.MainGrid;
        }

        #region Controls

        protected Grid MainGrid { get; set; }
        protected TextBlock Title { get; set; }

        #endregion

        #region Properties

        private TaskCompletionSource<bool> _taskCompletionSource;

        #endregion

        #region Commands

        /// <summary>
        /// Command invoked when the user clicks the close button of the 
        /// top-right corner of the dialog
        /// </summary>
        public ICommand CloseCommand { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Display the dialog
        /// </summary>
        public void ShowDialog()
        {
            this.IsOpen = true;
        }

        /// <summary>
        /// Display the dialog
        /// </summary>
        /// <returns>Dialog result</returns>
        public Task<bool> ShowDialogAsync()
        {
            // Needed to make a awaitable task
            _taskCompletionSource = new TaskCompletionSource<bool>();

            // Invoke the main dialog method
            ShowDialog();

            // Return awaitable task
            return _taskCompletionSource.Task;
        }
        
        /// <summary>
        /// Close the dialog
        /// </summary>
        protected virtual void CloseDialog(object obj = null)
        {
            this.IsOpen = false;
        }

        protected void DialogOpening(CancelEventArgs args)
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

        protected void DialogClosed()
        {
            // When the dialog is closed and finished remove this helper property
            App.AppInformation.PickerOrAsyncDialogIsOpen = false;
        }

        #endregion
    }
}
