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
        /// IP address assigned by the network
        /// </summary>
        private static string IpAddress { get; set; }

        /// <summary>
        /// Profile name of the network
        /// </summary>
        private static string ProfileName { get; set; }

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

        /// <summary>
        /// Initialize the network parameters
        /// </summary>
        public static void InitializeNetworkParams()
        {
            HasChangedIP();
            HasChangedProfileName();
        }

        /// <summary>
        /// Code to detect if the network has changed and refresh all open connections on this case
        /// </summary>
        public static void CheckNetworkChange()
        {
            var ipAddressChanged = HasChangedIP();
            var profileNameChanged = HasChangedProfileName();

            if (ipAddressChanged || profileNameChanged)
                SdkService.SetDnsServers();
        }

        /// <summary>
        /// Code to detect if the network profile name has changed
        /// </summary>
        /// <returns>TRUE if the has changed or FALSE in other case.</returns>
        private static bool HasChangedProfileName()
        {
            var internetConnection = NetworkInformation.GetInternetConnectionProfile();

            // If no network device is connected, do nothing
            if (internetConnection == null)
            {
                ProfileName = null;
                return false;
            }

            // If the profile name hasn't changed, do nothing
            if (internetConnection.ProfileName == ProfileName)
                return false;

            // Store the new profile name
            ProfileName = internetConnection.ProfileName;
            return true;
        }

        /// <summary>
        /// Code to detect if the IP has changed
        /// </summary>
        /// <returns>TRUE if the IP has changed or FALSE in other case.</returns>
        private static bool HasChangedIP()
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
            catch (ArgumentException) { return false; }

            // If no network device is connected, do nothing
            if ((ipAddresses == null) || (ipAddresses.Count < 1))
            {
                IpAddress = null;
                return false;
            }

            // If the IP hasn't changed, do nothing
            if (ipAddresses[0] == IpAddress)
                return false;

            // Store the new IP address
            IpAddress = ipAddresses[0];
            return true;
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
