using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Navigation;
using Telerik.Windows.Controls;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Microsoft.Phone.Controls;

namespace MegaApp
{
    public partial class App : Application
    {
        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public static RadPhoneApplicationFrame RootFrame { get; private set; }

        public static ApplicationEvent AppEvent { get; set; }
        
        public static String IpAddress { get; set; }

        public static MegaSDK MegaSdk { get; set; }
        public static CloudDriveViewModel CloudDrive { get; set; }
        public static TransferQueu MegaTransfers { get; set; }
        
        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions.
            UnhandledException += Application_UnhandledException;

            // Standard XAML initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Language display initialization
            InitializeLanguage();

            // Show graphics profiling information while debugging.
            if (Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode,
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Prevent the screen from turning off while under the debugger by disabling
                // the application's idle detection.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

            // Create Telerik Diagnostics with support e-mail address
            var diagnostics = new RadDiagnostics
            {
                EmailTo = AppResources.DiagnosticsEmailAddress
            };
            diagnostics.Init();

            // Subscribe to the NetworkAvailabilityChanged event
            DeviceNetworkInformation.NetworkAvailabilityChanged += new EventHandler<NetworkNotificationEventArgs>(NetworkAvailabilityChanged);            
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            // Initialize Telerik Diagnostics with the actual app version information
            ApplicationUsageHelper.Init(AppService.GetAppVersion());
            CheckChangesIP();
            AppEvent = ApplicationEvent.Lauching;
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            // Telerik Diagnostics
            ApplicationUsageHelper.OnApplicationActivated();
            CheckChangesIP();
            AppEvent = ApplicationEvent.Activated;
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            AppEvent = ApplicationEvent.Deactivated;
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            AppEvent = ApplicationEvent.Closing;
        }

        // Code to execute when the application detects a Network change.
        private static void NetworkAvailabilityChanged(object sender, NetworkNotificationEventArgs e)
        {
            switch (e.NotificationType)
            {
                case NetworkNotificationType.InterfaceConnected:                    
                case NetworkNotificationType.InterfaceDisconnected:
                    CheckChangesIP();
                    break;
                case NetworkNotificationType.CharacteristicUpdate:                    
                default:
                    break;
            }
        }

        // Code to detect if the IP has changed and refresh all open connections on this case
        private static void CheckChangesIP()
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
            catch(ArgumentException) { return; }

            // If no network device is connected, do nothing
            if ((ipAddresses == null) || (ipAddresses.Count < 1))
            {
                IpAddress = null;
                return;
            }                
            
            // If the primary IP has changed
            if (ipAddresses[0] != IpAddress)
            {
                MegaSdk.reconnect();        // Refresh all open connections
                IpAddress = ipAddresses[0]; // Storage the new primary IP address
            }            
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;
       // private const String appKey = "Z5dGhQhL";
        //private const String userAgent = "MEGAWindowsPhone/1.0.0";

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new RadPhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Handle reset requests for clearing the backstack
            RootFrame.Navigated += CheckForResetNavigation;

            // Assign the URI-mapper class to the application frame.
            RootFrame.UriMapper = new AssociationUriMapper();

            //The next line enables a custom logger, if this function is not used OutputDebugString() is called
            //in the native library and log messages are only readable with the native debugger attached.
            //The default behavior of MegaLogger() is to print logs using Debug.WriteLine() but it could
            //be used to sends log to a file, for example.
            MegaSDK.setLoggerObject(new MegaLogger());

            //You can select the maximum output level for debug messages.
            //By default FATAL, ERROR, WARNING and INFO will be enabled
            //DEBUG and MAX can only be enabled in Debug builds, they are ignored in Release builds
            MegaSDK.setLogLevel(MLogLevel.LOG_LEVEL_DEBUG);

            //You can send messages to the logger using MEGASDK.log(), those messages will be received
            //in the active logger
            MegaSDK.log(MLogLevel.LOG_LEVEL_INFO, "Example log message");
            
            // Initialize MegaSDK 
            MegaSdk = new MegaSDK(AppResources.AppKey, AppResources.UserAgentWP80, ApplicationData.Current.LocalFolder.Path, new MegaRandomNumberProvider());
            // Initialize the main drive
            CloudDrive = new CloudDriveViewModel(MegaSdk);
            // Add notifications listener. Needs a DriveViewModel
            MegaSdk.addGlobalListener(new GlobalDriveListener(CloudDrive));
            // Initialize the transfer listing
            MegaTransfers = new TransferQueu();
            // Initialize Folders
            AppService.InitializeAppFolders();
            // Set the current resolution that we use later on for our image selection
            AppService.CurrentResolution = ResolutionHelper.CurrentResolution;
            // Initialize Debug Settings
            DebugService.DebugSettings = new DebugSettingsViewModel();
            // Clear upload folder. Temporary uploads files are not necessary to keep
            AppService.ClearUploadCache();

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        private void CheckForResetNavigation(object sender, NavigationEventArgs e)
        {
            // If the app has received a 'reset' navigation, then we need to check
            // on the next navigation to see if the page stack should be reset
            if (e.NavigationMode == NavigationMode.Reset)
                RootFrame.Navigated += ClearBackStackAfterReset;
        }

        private void ClearBackStackAfterReset(object sender, NavigationEventArgs e)
        {
            // Unregister the event so it doesn't get called again
            RootFrame.Navigated -= ClearBackStackAfterReset;

            // Only clear the stack for 'new' (forward) and 'refresh' navigations
            if (e.NavigationMode != NavigationMode.New && e.NavigationMode != NavigationMode.Refresh)
                return;

            // For UI consistency, clear the entire page stack
            while (RootFrame.RemoveBackEntry() != null)
            {
                ; // do nothing
            }
        }

        #endregion

        // Initialize the app's font and flow direction as defined in its localized resource strings.
        //
        // To ensure that the font of your application is aligned with its supported languages and that the
        // FlowDirection for each of those languages follows its traditional direction, ResourceLanguage
        // and ResourceFlowDirection should be initialized in each resx file to match these values with that
        // file's culture. For example:
        //
        // AppResources.es-ES.resx
        //    ResourceLanguage's value should be "es-ES"
        //    ResourceFlowDirection's value should be "LeftToRight"
        //
        // AppResources.ar-SA.resx
        //     ResourceLanguage's value should be "ar-SA"
        //     ResourceFlowDirection's value should be "RightToLeft"
        //
        // For more info on localizing Windows Phone apps see http://go.microsoft.com/fwlink/?LinkId=262072.
        //
        private void InitializeLanguage()
        {
            try
            {
                // Set the font to match the display language defined by the
                // ResourceLanguage resource string for each supported language.
                //
                // Fall back to the font of the neutral language if the Display
                // language of the phone is not supported.
                //
                // If a compiler error is hit then ResourceLanguage is missing from
                // the resource file.
                RootFrame.Language = XmlLanguage.GetLanguage(AppResources.ResourceLanguage);

                // Set the FlowDirection of all elements under the root frame based
                // on the ResourceFlowDirection resource string for each
                // supported language.
                //
                // If a compiler error is hit then ResourceFlowDirection is missing from
                // the resource file.
                FlowDirection flow = (FlowDirection)Enum.Parse(typeof(FlowDirection), AppResources.ResourceFlowDirection);
                RootFrame.FlowDirection = flow;
            }
            catch
            {
                // If an exception is caught here it is most likely due to either
                // ResourceLangauge not being correctly set to a supported language
                // code or ResourceFlowDirection is set to a value other than LeftToRight
                // or RightToLeft.

                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                throw;
            }
        }
    }
}