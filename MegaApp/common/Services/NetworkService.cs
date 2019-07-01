using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Microsoft.Phone.Net.NetworkInformation;
using Windows.Networking.Connectivity;
using mega;

#if !CAMERA_UPLOADS_SERVICE
using MegaApp.Classes;
using MegaApp.Resources;
#endif

#if CAMERA_UPLOADS_SERVICE
namespace ScheduledCameraUploadTaskAgent.Services
#else
namespace MegaApp.Services
#endif
{
    static class NetworkService
    {
        #region Properties

        /// <summary>
        /// IP address assigned by the network
        /// </summary>
        private static string IpAddress;

        /// <summary>
        /// Profile name of the network
        /// </summary>
        private static string NetworkName;

        /// <summary>
        /// MEGA DNS servers
        /// </summary>
        private static string MegaDnsServers;

        #endregion

        #region Methods

#if CAMERA_UPLOADS_SERVICE
        /// <summary>
        /// Returns if there is an available network connection.
        /// </summary>        
        /// <returns>True if there is an available network connection. False in other case.</returns>
        public static bool IsNetworkAvailable()
#else
        /// <summary>
        /// Returns if there is an available network connection.
        /// </summary>
        /// <param name="showMessageDialog">Boolean parameter to indicate if show a message if no Intenert connection</param>
        /// <returns>True if there is an available network connection. False in other case.</returns>
        public static bool IsNetworkAvailable(bool showMessageDialog = false)
#endif
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
#if !CAMERA_UPLOADS_SERVICE
                if (showMessageDialog)
                {
                    UiService.OnUiThread(() =>
                    {
                        new CustomMessageDialog(
                            UiResources.NoInternetConnection.ToUpper(),
                            AppMessages.NoInternetConnectionMessage,
                            App.AppInformation,
                            MessageDialogButtons.Ok,
                            MessageDialogImage.NoInternetConnection).ShowDialog();
                    });
                }
#endif
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
            HasChangedNetworkName();
        }

        /// <summary>
        /// Code to detect if the network has changed and refresh all open connections on this case
        /// </summary>
        public static void CheckNetworkChange()
        {
            var ipAddressChanged = HasChangedIP();
            var networkNameChanged = HasChangedNetworkName();

            if (ipAddressChanged || networkNameChanged)
                SdkService.SetDnsServers();
        }

        /// <summary>
        /// Code to detect if the network profile name has changed
        /// </summary>
        /// <returns>TRUE if the has changed or FALSE in other case.</returns>
        private static bool HasChangedNetworkName()
        {
            try
            {
                var profile = NetworkInformation.GetInternetConnectionProfile();
                if (profile == null) return false;

                if (profile.ProfileName == NetworkName)
                    return false;

                NetworkName = profile.ProfileName;
                return true;
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_WARNING, "Error checking a possible network name change", e);
                return false;
            }
        }

        /// <summary>
        /// Code to detect if the IP has changed
        /// </summary>
        /// <returns>TRUE if the IP has changed or FALSE in other case.</returns>
        private static bool HasChangedIP()
        {
            try
            {
                var profile = NetworkInformation.GetInternetConnectionProfile();
                if (profile == null || profile.NetworkAdapter == null) return false;

                var hostname = NetworkInformation.GetHostNames().SingleOrDefault(hn =>
                    hn != null && hn.IPInformation != null && hn.IPInformation.NetworkAdapter != null &&
                    hn.IPInformation.NetworkAdapter.NetworkAdapterId == profile.NetworkAdapter.NetworkAdapterId);

                if (hostname == null || string.IsNullOrWhiteSpace(hostname.CanonicalName) || hostname.CanonicalName == IpAddress)
                    return false;

                IpAddress = hostname.CanonicalName;
                return true;
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_WARNING, "Error checking a possible IP address change", e);
                return false;
            }
        }

        /// <summary>
        /// Gets the MEGA DNS servers IP addresses.
        /// </summary>
        /// <param name="refresh">Indicates if should refresh the previously stored addresses.</param>
        /// <returns>String with the MEGA DNS servers IP addresses separated by commas.</returns>
        public static string GetMegaDnsServers(bool refresh = true)
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
