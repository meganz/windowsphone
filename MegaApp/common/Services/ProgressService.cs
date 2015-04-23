using System.Windows.Media;
using Microsoft.Phone.Shell;
using System.Windows.Media;

namespace MegaApp.Services
{
    public static class ProgressService
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

        public static bool GetProgressIndicatorVisibility()
        {
            if (SystemTray.ProgressIndicator == null)
                return false;

            return SystemTray.ProgressIndicator.IsVisible;
        }

        public static void SetProgressBar(bool isVisible, string message = null, int max = 1, int progress = 1)
        {
            if (!isVisible)
            {
                SystemTray.ProgressIndicator = null;
                return;
            }

            if (SystemTray.ProgressIndicator == null)
                SystemTray.ProgressIndicator = new ProgressIndicator();

            SystemTray.ProgressIndicator.Text = message;
            SystemTray.ProgressIndicator.IsIndeterminate = false;
            SystemTray.ProgressIndicator.Value = (double) progress/max;
            SystemTray.ProgressIndicator.IsVisible = true;
        }

        public static void ChangeProgressBarBackgroundColor(Color color)
        {
            SystemTray.BackgroundColor = color;
        }
    }
}
