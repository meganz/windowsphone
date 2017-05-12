using System;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using MegaApp.Services;
using ShakeGestures;

namespace MegaApp.UserControls
{
    public class MegaPhoneApplicationPage : PhoneApplicationPage
    {
        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            // Needed on every UI interaction
            SdkService.MegaSdk.retryPendingConnections();

            // Check to see if any dialog is open
            // Cancel backpress event so that the dialog can close first
            e.Cancel = App.AppInformation.PickerOrAsyncDialogIsOpen;
           
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            try
            {
                // Deinitialize ShakeGestures to disable shake detection
                ShakeGesturesHelper.Instance.ShakeGesture -= InstanceOnShakeGesture;
                ShakeGesturesHelper.Instance.Active = false;
            }
            catch (IsolatedStorageException)    { /* Possible bug #4760 */ }
            catch (BadImageFormatException)     { /* Possible bug #6401 */ }
                        
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            try
            {
                // Initialize ShakeGestures to display debug settings
                ShakeGesturesHelper.Instance.ShakeGesture += InstanceOnShakeGesture;
                ShakeGesturesHelper.Instance.MinimumRequiredMovesForShake = 12;
                ShakeGesturesHelper.Instance.Active = true;
            }
            catch (IsolatedStorageException)    { /* Possible bug #4760 */ }
            catch (BadImageFormatException)     { /* Possible bug #6401 */ }

            if (DebugService.DebugSettings.IsDebugMode && DebugService.DebugSettings.ShowDebugAlert)
                DialogService.ShowDebugModeAlert();
        }

        private void InstanceOnShakeGesture(object sender, ShakeGestureEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
                DebugService.DebugSettings.IsDebugMode = !DebugService.DebugSettings.IsDebugMode);
        }
    }
}
