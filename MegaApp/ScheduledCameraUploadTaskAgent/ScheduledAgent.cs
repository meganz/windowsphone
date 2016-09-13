using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Windows.Storage;
using mega;
using MegaApp.MegaApi;
using Microsoft.Phone.Info;
using Microsoft.Phone.Scheduler;
using Microsoft.Xna.Framework.Media;

namespace ScheduledCameraUploadTaskAgent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        private static ScheduledAgent scheduledAgent { get; set; }

        /// <summary>
        /// MEGA Software Development Kit reference
        /// </summary>
        private static MegaSDK MegaSdk { get; set; }

        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        static ScheduledAgent()
        {
            // Enable a custom logger
            MegaSDK.setLoggerObject(new MegaLogger());

            // Subscribe to the managed exception handler
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });
        }

        /// Code to execute on Unhandled Exceptions
        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected override void OnInvoke(ScheduledTask task)
        {
            if (!InitializeSdk())
            {
                // If initialisation of SDK failed
                // Notify complete and retry next task run
                this.NotifyComplete();
                return;
            }

            // Initialisation SDK succeeded
            scheduledAgent = this;

            // Log message to indicate that the service is invoked and the last exit reason
            MegaSDK.log(MLogLevel.LOG_LEVEL_INFO, "Service invoked. Last exit reason: " + 
                task.LastExitReason.ToString());

            // Add notifications listener
            MegaSdk.addGlobalListener(new MegaGlobalListener());
            
            // Add transfers listener
            MegaSdk.addTransferListener(new MegaTransferListener());
                        
            // Fast login with session token that was saved during MEGA app initial login
            FastLogin();
        }

        /// <summary>
        /// Initialize the MegaSDK to be able to perform uploads
        /// </summary>
        /// <returns>True if creation succeeded and False if creation failed</returns>
        private static bool InitializeSdk()
        {
            try
            {
                String folderCameraUploadService = Path.Combine(ApplicationData.Current.LocalFolder.Path, "CameraUploadService");
                if (!Directory.Exists(folderCameraUploadService))
                    Directory.CreateDirectory(folderCameraUploadService);

                MegaSdk = new MegaSDK(
                "Z5dGhQhL",
                String.Format("{0}/{1}/{2}",
                    GetBackgroundAgentUserAgent(),
                    DeviceStatus.DeviceManufacturer,
                    DeviceStatus.DeviceName),
                folderCameraUploadService,
                new MegaRandomNumberProvider());

                return MegaSdk != null;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// Fast login to MEGA account with user session token
        /// </summary>
        private void FastLogin()
        {
            string sessionToken = null;
            try
            {
                // Try to load shared session token file
                sessionToken = SettingsService.LoadSettingFromFile<string>("{85DBF3E5-51E8-40BB-968C-8857B4FC6EF4}");
            }
            catch (Exception)
            {
                // Failed to load shared session token file
                // Notify complete and try next run to load the session string
                this.NotifyComplete();
                return;
            }
           

            if (String.IsNullOrEmpty(sessionToken) || String.IsNullOrWhiteSpace(sessionToken))
            {
                // No shred session token found
                // Notify complete and try next run to load the session string
                this.NotifyComplete();
                return;
            }

            // Do login
            var fastLoginListener = new MegaRequestListener();

            // After the request is finished. Check for success or failure
            fastLoginListener.RequestFinished += (sender, args) =>
            {
                if (!args.Succeeded)
                {
                    // Login failed
                    // Notify complete and try next run to load the session string
                    this.NotifyComplete();
                    return;
                }

                // Login succeeded
                // Fetch nodes. Needed to find the camera upload node later
                FetchNodes();
            };

            // Init fastlogin
            MegaSdk.fastLogin(sessionToken, fastLoginListener);
        }

        /// <summary>
        /// Fetch the MEGA nodes from the server
        /// </summary>
        private void FetchNodes()
        {
            var fetchNodesListener = new MegaRequestListener();

            // After the request is finished. Check for success or failure
            fetchNodesListener.RequestFinished += (sender, args) =>
            {
                if (!args.Succeeded)
                {
                    // When failed to retreive nodes we can not proceed to upload
                    // Notify complete and try next run to load the session string
                    this.NotifyComplete();
                    return;
                }

                // Enable the transfers resumption for the Camera Uploads service
                MegaSdk.enableTransferResumption();
            };

            // Init fetch nodes
            MegaSdk.fetchNodes(fetchNodesListener);
        }
        
        /// <summary>
        /// Upload files to MEGA Cloud Service
        /// </summary>
        public static async void Upload()
        {
            MegaSdk.retryPendingConnections();

            // Get the date of the last uploaded file
            // Needed so that we do not upload the same file twice
            var lastUploadDate = SettingsService.LoadSettingFromFile<DateTime>("LastUploadDate");

            // Open the phone's Media Library
            using (var mediaLibrary = new MediaLibrary())
            {
                var selectDate = lastUploadDate;
                // Find all pictures taken after the last upload date
                var pictures = mediaLibrary.Pictures.Where(p => p.Date > selectDate).OrderBy(p => p.Date).ToList();


                if (!pictures.Any())
                {
                    // No pictures is not an error. Maybe all pictures have already been uploaded
                    // Just finish the task for this run
                    MegaSDK.log(MLogLevel.LOG_LEVEL_INFO, "No new items to upload");
                    scheduledAgent.NotifyComplete();
                    return;
                }

                var cameraUploadNode = await scheduledAgent.GetCameraUploadsNode();
                if (cameraUploadNode == null)
                {
                    // No camera upload node found or created
                    // Just finish this run and try again next time
                    MegaSDK.log(MLogLevel.LOG_LEVEL_ERROR, "No camera uploads folder");
                    scheduledAgent.NotifyComplete();
                    return;
                }

                // Loop all available pictures for upload action
                foreach (var picture in pictures)
                {
                    try
                    {
                        // Retreive the picture bytes as stream
                        using (var imageStream = picture.GetImage())
                        {
                            // Make sure the stream pointer is at the start of the stream
                            imageStream.Position = 0;

                            // Calculate time for fingerprint check
                            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                            TimeSpan diff = picture.Date.ToUniversalTime() - origin;
                            ulong mtime = (ulong)Math.Floor(diff.TotalSeconds);

                            // Get the unique fingerprint of the file
                            string fingerprint = MegaSdk.getFileFingerprint(new MegaInputStream(imageStream), mtime);

                            // Check if the fingerprint is already in the subfolders of the Camera Uploads
                            var mNode = MegaSdk.getNodeByFingerprint(fingerprint, cameraUploadNode);

                            // If node already exists then save the node date and proceed with the next node
                            if (mNode != null)
                            {
                                SettingsService.SaveSettingToFile<DateTime>("LastUploadDate", picture.Date);
                                continue; // skip to next picture
                            }

                            // Create a temporary local path to save the picture for upload
                            string newFilePath = Path.Combine(
                                Path.Combine(ApplicationData.Current.LocalFolder.Path, @"uploads\"), picture.Name);

                            // Reset back to start
                            // Because fingerprint action has moved the position
                            imageStream.Position = 0;

                            // Copy file to local storage to be able to upload
                            using (var fs = new FileStream(newFilePath, FileMode.Create))
                            {
                                await imageStream.CopyToAsync(fs);
                                await fs.FlushAsync();
                                fs.Close();
                            }                            

                            // Init the upload
                            MegaSdk.startUploadWithMtimeTempSource(newFilePath, cameraUploadNode, mtime, true);                            
                            break;
                        }
                    }
                    catch (Exception)
                    {
                        // Something went wrong (could be memory limit)
                        // Just finish this run and try again next time
                        MegaSDK.log(MLogLevel.LOG_LEVEL_ERROR, "Error during the item upload");
                        scheduledAgent.NotifyComplete();
                        return;
                    }
                }
            }
        }
       
        /// <summary>
        /// Locate the Camera Uploads folder node to use as parent for the uploads
        /// </summary>
        /// <returns>Camera Uploads folder node</returns>
        private async Task<MNode> GetCameraUploadsNode()
        {
            // First try to retrieve the Cloud Drive root node
            var rootNode = MegaSdk.getRootNode();
            if (rootNode == null) return null;

            // Locate the camera upload node
            var cameraUploadNode = FindCameraUploadNode(rootNode);

            // If node found, return the node
            if (cameraUploadNode != null) return cameraUploadNode;

            // Node not found, create a new Camera Uploads node
            var tcs = new TaskCompletionSource<MNode>();

            var createFolderListener = new MegaRequestListener();
            // After reqyest finished, return the newly created Camera Uploads node
            createFolderListener.RequestFinished += (sender, args) =>
            {
                tcs.TrySetResult(args.Succeeded ? FindCameraUploadNode(rootNode) : null);
            };
            
            // Init folder creation
            MegaSdk.createFolder("Camera Uploads", rootNode, createFolderListener);

            return await tcs.Task;
        }

        /// <summary>
        /// Locate the Camera Uploads folder node in the specified root
        /// </summary>
        /// <param name="rootNode">Current root node</param>
        /// <returns>Camera Uploads folder node in</returns>
        private MNode FindCameraUploadNode(MNode rootNode)
        {
            var childs = MegaSdk.getChildren(rootNode);

            for (int x = 0; x < childs.size(); x++)
            {
                var node = childs.get(x);
                // Camera Uploads is a folder
                if (node.getType() != MNodeType.TYPE_FOLDER) continue;
                // Check the folder name
                if (!node.getName().ToLower().Equals("camera uploads")) continue;
                return node;
            }

            return null;
        }

        private static string GetBackgroundAgentUserAgent()
        {
            return String.Format("MEGAWindowsPhoneBackgroundAgent/{0}", "1.0.0.0");
        }

        //private static void ShowAbortToast(string message)
        //{
        //    var toast = new ShellToast
        //    {
        //        Title = "MEGA Camera Uploads",
        //        Content = String.Format("Message: {0}", message) 
        //    };
        //    toast.Show();
        //}

     
    }
}