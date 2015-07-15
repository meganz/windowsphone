using System.ComponentModel;
using Microsoft.Phone.Controls;

namespace MegaApp.UserControls
{
    public class MegaPhoneApplicationPage : PhoneApplicationPage
    {
        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            // Needed on every UI interaction
            App.MegaSdk.retryPendingConnections();

            // Check to see if any dialog is open
            // Cancel backpress event so that the dialog can close first
            e.Cancel = App.AppInformation.PickerOrAsyncDialogIsOpen;
           
        }
    }
}
