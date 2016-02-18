using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Navigation;
using Windows.ApplicationModel.Activation;
using Windows.Networking.Connectivity;
using Windows.Storage;
using mega;
using MegaApp.Classes;
using MegaApp.Database;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Models;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;
using Microsoft.Phone.Info;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Shell;
using MockIAPLib;
using Telerik.Windows.Controls;
#if WINDOWS_PHONE_81
    using Windows.ApplicationModel.DataTransfer.ShareTarget;
    using Windows.Storage.AccessCache;
#endif

namespace MegaApp
{
    public partial class App : Application, MRequestListenerInterface
    {
        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public static RadPhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Provides easy access to usefull application information
        /// </summary>
        public static AppInformation AppInformation { get; private set; }        
        public static String IpAddress { get; set; }
        public static MegaSDK MegaSdk { get; set; }
        public static CloudDriveViewModel CloudDrive { get; set; }
        public static MainPageViewModel MainPageViewModel { get; set; }
        public static TransferQueu MegaTransfers { get; set; }

        public static UserDataViewModel UserData { get; set; }

        public static GlobalDriveListener GlobalDriveListener { get; private set; }

        public static bool FileOpenOrFolderPickerOpenend { get; set; }

        public static String ShortCutBase64Handle { get; set; }

        public static String ActiveImportLink { get; set; }

        public static MNode PublicNode { get; set; }

        // DataBase Name
        public static String DB_PATH = Path.Combine(Path.Combine(ApplicationData.Current.LocalFolder.Path, "MEGA.sqlite"));
        
        #if WINDOWS_PHONE_81
        // Used for multiple file selection
        public FileOpenPickerContinuationEventArgs FilePickerContinuationArgs { get; set; }
        // Used for folder selection
        public FolderPickerContinuationEventArgs FolderPickerContinuationArgs { get; set; }
        
        // Makes the app a share target for files
        public ShareOperation ShareOperation { get; set; }
        #endif
        
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
                Current.Host.Settings.EnableFrameRateCounter = true;

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

            // APP THEME OVERRIDES
            Resources.Remove("PhoneAccentColor");
            Resources.Add("PhoneAccentColor", (Color)Current.Resources["MegaRedColor"]);
            ((SolidColorBrush)Resources["PhoneAccentBrush"]).Color = (Color)Current.Resources["MegaRedColor"];
            ((SolidColorBrush)Resources["PhoneTextBoxEditBorderBrush"]).Color = (Color)Current.Resources["MegaRedColor"];

            // Create the DB if not exists
            if (!FileService.FileExists(DB_PATH))
            {
                using (var db = new SQLiteConnection(DB_PATH))
                {
                    db.CreateTable<SavedForOffline>();
                }
            }

            SetupMockIAP();
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            // Initialize Telerik Diagnostics with the actual app version information
            ApplicationUsageHelper.Init(AppService.GetAppVersion());
            AppInformation.HasPinLockIntroduced = false;
            NetworkService.CheckChangesIP();

            #if WINDOWS_PHONE_81
            // Code to intercept files that are send to MEGA as share target
            var shareEventArgs = e as ShareLaunchingEventArgs;
            if (shareEventArgs != null)
            {
                this.ShareOperation = shareEventArgs.ShareTargetActivatedEventArgs.ShareOperation;
            }
            #endif
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            // Telerik Diagnostics
            ApplicationUsageHelper.OnApplicationActivated();
            AppInformation.IsStartupModeActivate = true;
            AppInformation.HasPinLockIntroduced = false;
            NetworkService.CheckChangesIP();
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            
        }

        // Code to execute when the application detects a Network change.
        private static void NetworkAvailabilityChanged(object sender, NetworkNotificationEventArgs e)
        {
            switch (e.NotificationType)
            {
                case NetworkNotificationType.InterfaceConnected:                    
                case NetworkNotificationType.InterfaceDisconnected:
                    NetworkService.CheckChangesIP();
                    break;
                case NetworkNotificationType.CharacteristicUpdate:                    
                default:
                    break;
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

        // Do not add any additional code to this method
        private async void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new RadPhoneApplicationFrame
            {
                // Add default page transitions animations
                Transition = new RadTransition()
                {
                    ForwardInAnimation = AnimationService.GetPageInAnimation(),
                    ForwardOutAnimation = AnimationService.GetPageOutAnimation(),
                    BackwardInAnimation = AnimationService.GetPageInAnimation(),
                    BackwardOutAnimation = AnimationService.GetPageOutAnimation(),

                }
            };

            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Handle reset requests for clearing the backstack
            RootFrame.Navigated += CheckForResetNavigation;

#if WINDOWS_PHONE_81
            RootFrame.Navigating += RootFrameOnNavigating;

            // Handle contract activation such as returned values from file open or save picker
            PhoneApplicationService.Current.ContractActivated += CurrentOnContractActivated;
#endif

            // Assign the URI-mapper class to the application frame.
            RootFrame.UriMapper = new AssociationUriMapper();

            // Initialize the application information
            AppInformation = new AppInformation();

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
            MegaSdk = new MegaSDK(AppResources.AppKey, String.Format("{0}/{1}/{2}",
                AppService.GetAppUserAgent(), DeviceStatus.DeviceManufacturer, DeviceStatus.DeviceName),
                ApplicationData.Current.LocalFolder.Path, new MegaRandomNumberProvider());
            
            // Initialize the main drive
            CloudDrive = new CloudDriveViewModel(MegaSdk, AppInformation);
            // Add notifications listener. Needs a DriveViewModel
            GlobalDriveListener = new GlobalDriveListener(AppInformation);
            MegaSdk.addGlobalListener(GlobalDriveListener);
            // Add a global request listener to process all.
            MegaSdk.addRequestListener(this);
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
            // Clear settings values we do no longer use
            AppService.ClearObsoleteSettings();
            // Save the app version information for future use (like deleting settings)
            AppService.SaveAppInformation();
            // Set MEGA red as Accent Color
            ((SolidColorBrush)Resources["PhoneAccentBrush"]).Color = (Color)Resources["MegaRedColor"];

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        #if WINDOWS_PHONE_81
        private void RootFrameOnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Reset && FileOpenOrFolderPickerOpenend)
                e.Cancel = true;

            if (e.NavigationMode == NavigationMode.New && FileOpenOrFolderPickerOpenend)
            {
                e.Cancel = true;
                FileOpenOrFolderPickerOpenend = false;
            }
        }

        private void CurrentOnContractActivated(object sender, IActivatedEventArgs activatedEventArgs)
        {
            FileOpenOrFolderPickerOpenend = false;

            var filePickerContinuationArgs = activatedEventArgs as FileOpenPickerContinuationEventArgs;
            if (filePickerContinuationArgs != null)
            {
                this.FilePickerContinuationArgs = filePickerContinuationArgs;
                return;
            }

            var folderPickerContinuationArgs = activatedEventArgs as FolderPickerContinuationEventArgs;
            if (folderPickerContinuationArgs != null)
            {
                CloudDrive.PickerOrDialogIsOpen = false;
                if(folderPickerContinuationArgs.Folder != null)
                {
                    if(!StorageApplicationPermissions.FutureAccessList.CheckAccess(folderPickerContinuationArgs.Folder))
                        StorageApplicationPermissions.FutureAccessList.Add(folderPickerContinuationArgs.Folder);

                    if (folderPickerContinuationArgs.ContinuationData["Operation"].ToString() ==
                        "SelectDefaultDownloadFolder")
                    {
                        SettingsService.SaveSetting(SettingsResources.DefaultDownloadLocation, folderPickerContinuationArgs.Folder.Path);
                        this.FolderPickerContinuationArgs = null;
                    }
                
                    if (folderPickerContinuationArgs.ContinuationData["Operation"].ToString() == "SelectDownloadFolder")
                        this.FolderPickerContinuationArgs = folderPickerContinuationArgs;
                }
            }
        }
        #endif

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
            #if WINDOWS_PHONE_80
                if (e.NavigationMode == NavigationMode.Reset)
                    RootFrame.Navigated += ClearBackStackAfterReset;
            #elif WINDOWS_PHONE_81
            if (e.NavigationMode == NavigationMode.Reset && (!FileOpenOrFolderPickerOpenend && FilePickerContinuationArgs == null))
                RootFrame.Navigated += ClearBackStackAfterReset;
            #endif
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

        private void SetupMockIAP()
        {
#if DEBUG
            MockIAP.Init();
            MockIAP.ClearCache();

            MockIAP.RunInMockMode(true);
            MockIAP.SetListingInformation(1, "en-us", "A description", "1", "TestApp");

            // Add some more items manually.
            ProductListing p = new ProductListing
            {
                Name = "Pro1Monthly",
                ImageUri = null,
                ProductId = "Pro1Monthly",
                ProductType = Windows.ApplicationModel.Store.ProductType.Durable,
                Keywords = new string[] { "Pro1Monthly" },
                Description = "Pro1Monthly",
                FormattedPrice = "1.0",
                Tag = string.Empty
            };
            
            MockIAP.AddProductListing("Pro1Monthly", p);
#endif
        }


        #region MRequestListenerInterface

        public virtual void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            if (e.getErrorCode() == MErrorType.API_ESID || e.getErrorCode() == MErrorType.API_ESSL)
            {
                AppService.LogoutActions();
                
                // Show the init tour / login page with the corresponding navigation parameter
                if(e.getErrorCode() == MErrorType.API_ESID)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        NavigateService.NavigateTo(typeof(InitTourPage), NavigationParameter.API_ESID));
                }                    
                else if(e.getErrorCode() == MErrorType.API_ESSL)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        NavigateService.NavigateTo(typeof(InitTourPage), NavigationParameter.API_ESSL));
                }                    
            }
        }

        public virtual void onRequestStart(MegaSDK api, MRequest request)
        {
            // Not necessary
        }

        public virtual void onRequestTemporaryError(MegaSDK api, MRequest request, MError e)
        {
            // Not necessary
        }

        public virtual void onRequestUpdate(MegaSDK api, MRequest request)
        {
            // Not necessary
        }

        #endregion
    }
}