using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Phone.Info;
using Microsoft.Phone.Scheduler;
using Microsoft.Xna.Framework.Media;
using Windows.Storage;
using mega;
using ScheduledCameraUploadTaskAgent.MegaApi;
using ScheduledCameraUploadTaskAgent.Services;

namespace ScheduledCameraUploadTaskAgent
{
    public partial class ScheduledAgent : ScheduledTaskAgent
    {
        private static ScheduledAgent scheduledAgent { get; set; }

        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        static ScheduledAgent()
        {
            SdkService.InitializeSdkParams();

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
            // Initialisation SDK succeeded
            scheduledAgent = this;

            // Log message to indicate that the service is invoked and the last exit reason
            LogService.Log(MLogLevel.LOG_LEVEL_INFO, "Service invoked. Last exit reason: " +  
                task.LastExitReason);

            // Add notifications listener
            SdkService.MegaSdk.addGlobalListener(new MegaGlobalListener());

            // Abort the service when storage quota exceeded error is raised in the transferlistener
            // Abort will stop the service and it will not be launched again until the user
            // activates it in the main application
            var megaTransferListener = new MegaTransferListener();
            megaTransferListener.StorageQuotaExceeded += (sender, args) =>
            {
                scheduledAgent.Abort();
            };
            // Notify complete when tramsfer quota exceeded error is raised in the transferlistener
            // Notify complete will retry in the next task run
            megaTransferListener.TransferQuotaExceeded += (sender, args) =>
            {
                scheduledAgent.NotifyComplete();
            };
            
            // Add transfers listener
            SdkService.MegaSdk.addTransferListener(megaTransferListener);
                        
            // Fast login with session token that was saved during MEGA app initial login
            FastLogin();
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
            SdkService.MegaSdk.fastLogin(sessionToken, fastLoginListener);
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
                SdkService.MegaSdk.enableTransferResumption();
            };

            // Init fetch nodes
            SdkService.MegaSdk.fetchNodes(fetchNodesListener);
        }
        
        /// <summary>
        /// Upload files to MEGA Cloud Service
        /// </summary>
        public static async void Upload()
        {
            SdkService.MegaSdk.retryPendingConnections();

            // Get the date of the last uploaded file
            // Needed so that we do not upload the same file twice
            var lastUploadDate = SettingsService.LoadSettingFromFile<DateTime>("LastUploadDate");

            // Open the phone's Media Library
            MediaLibrary mediaLibrary;
            try { mediaLibrary = new MediaLibrary(); }
            catch(Exception e)
            {
                // Error opening the Media Library
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error opening the Media Library", e);                
                scheduledAgent.NotifyComplete();
                return;
            }
            
            using (mediaLibrary)
            {
                List<Picture> pictures;

                var selectDate = lastUploadDate;
                // Find all pictures taken after the last upload date
                try { pictures = mediaLibrary.Pictures.Where(p => p.Date > selectDate).OrderBy(p => p.Date).ToList(); }
                catch (Exception e)
                {
                    // Error getting the pictures taken after the last upload date
                    LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error getting pictures from the media library", e);                    
                    scheduledAgent.NotifyComplete();
                    return;
                }

                if (!pictures.Any())
                {
                    // No pictures is not an error. Maybe all pictures have already been uploaded
                    // Just finish the task for this run
                    LogService.Log(MLogLevel.LOG_LEVEL_INFO, "No new items to upload");
                    scheduledAgent.NotifyComplete();
                    return;
                }

                var cameraUploadNode = await scheduledAgent.GetCameraUploadsNode();
                if (cameraUploadNode == null)
                {
                    // No camera upload node found or created
                    // Just finish this run and try again next time
                    LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "No camera uploads folder");
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
                            ulong mtime = (ulong) Math.Floor(diff.TotalSeconds);

                            // Get the unique fingerprint of the file
                            string fingerprint = SdkService.MegaSdk.getFileFingerprint(new MegaInputStream(imageStream), mtime);

                            // Check if the fingerprint is already in the subfolders of the Camera Uploads
                            var mNode = SdkService.MegaSdk.getNodeByFingerprint(fingerprint, cameraUploadNode);

                            // If node already exists then save the node date and proceed with the next node
                            if (mNode != null)
                            {
                                SettingsService.SaveSettingToFile<DateTime>("LastUploadDate", picture.Date);
                                continue; // skip to next picture
                            }

                            // Create a temporary local path to save the picture for upload
                            string newFilePath = Path.Combine(scheduledAgent.GetTemporaryUploadFolder(), picture.Name);

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
                            SdkService.MegaSdk.startUploadWithMtimeTempSource(newFilePath, cameraUploadNode, mtime, true);
                            break;
                        }
                    }
                    catch (OutOfMemoryException e)
                    {
                        // Something went wrong (could be memory limit)
                        // Just finish this run and try again next time
                        LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error during the item upload", e);                        
                        scheduledAgent.NotifyComplete();
                    }
                    catch (Exception e)
                    {
                        // Send log, process the error and try again
                        LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error during the item upload", e);
                        ErrorProcessingService.ProcessFileError(picture.Name, picture.Date);
                        Upload();
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
            var rootNode = SdkService.MegaSdk.getRootNode();
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
            SdkService.MegaSdk.createFolder("Camera Uploads", rootNode, createFolderListener);

            return await tcs.Task;
        }

        /// <summary>
        /// Locate the Camera Uploads folder node in the specified root
        /// </summary>
        /// <param name="rootNode">Current root node</param>
        /// <returns>Camera Uploads folder node in</returns>
        private MNode FindCameraUploadNode(MNode rootNode)
        {
            var childs = SdkService.MegaSdk.getChildren(rootNode);

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

        /// <summary>
        /// Get the temporary upload folder path
        /// </summary>
        /// <returns>Temporary upload folder path</returns>
        private string GetTemporaryUploadFolder()
        {
            var uploadDir = Path.Combine(ApplicationData.Current.LocalFolder.Path, @"uploads\");

            // Check if the temporary upload folder exists or create it if not exists
            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            return uploadDir;
        }

        public static string GetBackgroundAgentUserAgent()
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