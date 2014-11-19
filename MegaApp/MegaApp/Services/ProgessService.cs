using Microsoft.Phone.Shell;

namespace MegaApp.Services
{
    public static class ProgessService
    {
        public static void SetProgressIndicator(bool isVisible, string message = null)
        {
            if (!isVisible)
            {
                SystemTray.ProgressIndicator = null;
                return;
            }

            if (SystemTray.ProgressIndicator == null)
                SystemTray.ProgressIndicator = new ProgressIndicator();
          
            SystemTray.ProgressIndicator.Text = message;
            SystemTray.ProgressIndicator.IsIndeterminate = true;
            SystemTray.ProgressIndicator.IsVisible = true;
        }
    }
}
