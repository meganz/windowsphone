using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Phone.Net.NetworkInformation;
using Windows.Networking.Connectivity;
using MegaApp.Classes;
using MegaApp.Resources;

namespace MegaApp.Services
{
    static class NetworkService
    {
        public static bool IsNetworkAvailable(bool showMessageDialog = false)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                if (showMessageDialog)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        new CustomMessageDialog(
                            UiResources.NoInternetConnection.ToUpper(),
                            AppMessages.NoInternetConnectionMessage,
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialog();
                    });                    
                }

                return false;
            }

            return true;
        }

        // Code to detect if the IP has changed and refresh all open connections on this case
        public static void CheckChangesIP()
        {
            List<String> ipAddresses = null;

            // Find the IP of all network devices
            try
            {
                ipAddresses = new List<String>();
                var hostnames = NetworkInformation.GetHostNames();
                foreach (var hn in hostnames)
                {
                    if (hn.IPInformation != null)// && hn.Type == Windows.Networking.HostNameType.Ipv4)
                    {
                        string ipAddress = hn.DisplayName;
                        ipAddresses.Add(ipAddress);
                    }
                }
            }
            catch (ArgumentException) { return; }

            // If no network device is connected, do nothing
            if ((ipAddresses == null) || (ipAddresses.Count < 1))
            {
                App.IpAddress = null;
                return;
            }

            // If the primary IP has changed
            if (ipAddresses[0] != App.IpAddress)
            {
                App.MegaSdk.reconnect();        // Refresh all open connections
                App.IpAddress = ipAddresses[0]; // Storage the new primary IP address
            }
        }
    }
}
