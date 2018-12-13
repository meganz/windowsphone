using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Windows;
using Microsoft.Phone.Net.NetworkInformation;
using Windows.Networking.Connectivity;
using mega;
using MegaApp.Classes;
using MegaApp.Resources;

namespace MegaApp.Services
{
    static class NetworkService
    {
        #region Properties

        /// <summary>
        /// MEGA DNS servers
        /// </summary>
        private static string MegaDnsServers;

        #endregion

        #region Methods

        /// <summary>
        /// Returns if there is an available network connection.
        /// </summary>        
        /// <param name="showMessageDialog">Boolean parameter to indicate if show a message if no Intenert connection</param>
        /// <returns>True if there is an available network connection., False in other case.</returns>
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
                            MessageDialogButtons.Ok,
                            MessageDialogImage.NoInternetConnection).ShowDialog();
                    });                    
                }

                LogService.Log(MLogLevel.LOG_LEVEL_INFO, "No network available.");
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
                SdkService.MegaSdk.reconnect();        // Refresh all open connections
                App.IpAddress = ipAddresses[0]; // Storage the new primary IP address
            }
        }

        /// <summary>
        /// Gets the MEGA DNS servers IP addresses.
        /// </summary>
        /// <param name="refresh">Indicates if should refresh the previously stored addresses.</param>
        /// <returns>String with the MEGA DNS servers IP addresses separated by commas.</returns>
        public static string GetMegaDnsServers(bool refresh = false)
        {
            if (!refresh && !string.IsNullOrWhiteSpace(MegaDnsServers))
                return MegaDnsServers;

            try
            {
                if (!IsNetworkAvailable()) return null;

                LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Getting MEGA DNS servers...");
                var endpoint = new DnsEndPoint("ns.mega.co.nz", 0);

                var autoResetEvent = new AutoResetEvent(false);
                DeviceNetworkInformation.ResolveHostNameAsync(endpoint,
                    (NameResolutionResult result) =>
                    {
                        string dnsServers = string.Empty;
                        try
                        {
                            if (result.IPEndPoints != null)
                            {
                                IPEndPoint[] endpoints = result.IPEndPoints;
                                foreach (IPEndPoint address in endpoints)
                                {
                                    if (dnsServers.Length > 0)
                                        dnsServers = string.Concat(dnsServers, ",");

                                    dnsServers = string.Concat(dnsServers, address.Address.ToString());
                                }
                            }

                            if (string.IsNullOrWhiteSpace(dnsServers))
                            {
                                LogService.Log(MLogLevel.LOG_LEVEL_WARNING, "No MEGA DNS servers.");
                                autoResetEvent.Set();
                                return;
                            }

                            LogService.Log(MLogLevel.LOG_LEVEL_INFO, "MEGA DNS servers: " + dnsServers);
                            MegaDnsServers = dnsServers;
                        }
                        catch (Exception e)
                        {
                            LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error getting MEGA DNS servers.", e);
                        }
                        finally
                        {
                            autoResetEvent.Set();
                        }
                    },
                    null);

                autoResetEvent.WaitOne();

                return MegaDnsServers;
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error getting MEGA DNS servers.", e);
                return null;
            }
        }

        #endregion
    }
}
