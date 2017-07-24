using System;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using MegaApp.Services;

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
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (DebugService.DebugSettings.IsDebugMode && DebugService.DebugSettings.ShowDebugAlert)
                DialogService.ShowDebugModeAlert();
        }
    }
}
