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
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Media;

namespace ScheduledCameraUploadTaskAgent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        private static MegaSDK MegaSdk { get; set; }
        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        static ScheduledAgent()
        {
            // Subscribe to the managed exception handler
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });
        }

        /// Code to execute on Unhandled Exceptions
        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            ShowAbortToast();

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
            InitializeSdk();
        }

        private void InitializeSdk()
        {
            // Initialize MegaSDK 
            MegaSdk = new MegaSDK("Z5dGhQhL", String.Format("{0}/{1}/{2}",
                GetBackgroundAgentUserAgent(), DeviceStatus.DeviceManufacturer, DeviceStatus.DeviceName),
                ApplicationData.Current.LocalFolder.Path, new MegaRandomNumberProvider());

            FastLogin();
        }

        private void FastLogin()
        {
            var fastLoginListener = new MegaRequestListener();
            fastLoginListener.RequestFinished += (sender, args) =>
            {
                if (!args.Succeeded)
                {
                    ShowAbortToast();
                    this.Abort();
                }
                else
                {
                    FetchNodes();
                }
            };

            
            var sessionToken = SettingsService.LoadSettingFromFile<string>("{85DBF3E5-51E8-40BB-968C-8857B4FC6EF4}");

            if (String.IsNullOrEmpty(sessionToken))
                this.NotifyComplete();
            else
                MegaSdk.fastLogin(sessionToken, fastLoginListener);
        }

        private void FetchNodes()
        {
            var fetchNodesListener = new MegaRequestListener();
            fetchNodesListener.RequestFinished += (sender, args) =>
            {
                if (!args.Succeeded)
                {
                    ShowAbortToast();
                    this.Abort();
                }
                else
                {
                    Upload();
                }
            };

            MegaSdk.fetchNodes(fetchNodesListener);
        }
        
        private async void Upload()
        {
            var lastUploadDate = SettingsService.LoadSettingFromFile<DateTime>("LastUploadDate");

            using (var mediaLibrary = new MediaLibrary())
            {
                var selectDate = lastUploadDate;
                var pictures = mediaLibrary.Pictures.Where(p => p.Date > selectDate).ToList();

                if (!pictures.Any())
                {
                    this.NotifyComplete();
                    return;
                }
                
                foreach (var picture in pictures)
                {
                    using (var imageStream = picture.GetImage())
                    {
                        imageStream.Position = 0;

                        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                        TimeSpan diff = picture.Date.ToUniversalTime() - origin;
                        ulong mtime = (ulong)Math.Floor(diff.TotalSeconds);

                        string fingerprint = MegaSdk.getFileFingerprint(new MegaInputStream(imageStream), mtime);

                        var cameraUploadNode = await GetCameraUploadsNode();

                        var mNode = MegaSdk.getNodeByFingerprint(fingerprint, cameraUploadNode);

                        lastUploadDate = picture.Date;

                        if (mNode != null)
                        {
                             SettingsService.SaveSettingToFile("LastUploadDate", lastUploadDate);
                             continue;
                        }
                        
                        string newFilePath = Path.Combine(
                            Path.Combine(ApplicationData.Current.LocalFolder.Path, "uploads\\"), picture.Name);

                        imageStream.Position = 0;

                        using (var fs = new FileStream(newFilePath, FileMode.Create))
                        {
                            await imageStream.CopyToAsync(fs);
                            await fs.FlushAsync();
                            fs.Close();
                        }
                        
                        var transferListener = new MegaTransferListener();
                        transferListener.TransferFinished += (sender, args) =>
                        {
                            if (args.Succeeded)
                                SettingsService.SaveSettingToFile("LastUploadDate", lastUploadDate);
                            File.Delete(newFilePath);
                            Upload();
                        };

                        MegaSdk.startUploadWithMtime(newFilePath, cameraUploadNode, mtime, transferListener);
                        break;
                    }
                    
                }

            }

        }
       
        private async Task<MNode> GetCameraUploadsNode()
        {
            var rootNode = MegaSdk.getRootNode();
            if (rootNode == null) return null;

            var cameraUploadNode = FindCameraUploadNode(rootNode);

            if (cameraUploadNode != null) return cameraUploadNode;
            
            // Create the Camera Uploads folder
            MegaSdk.createFolder("Camera Uploads", rootNode);
            // Wait 10 seconds before continue
            await Task.Delay(10000);
            
            return FindCameraUploadNode(rootNode);
        }

        private MNode FindCameraUploadNode(MNode rootNode)
        {
            var childs = MegaSdk.getChildren(rootNode);

            for (int x = 0; x < childs.size(); x++)
            {
                var node = childs.get(x);
                if (node.getType() != MNodeType.TYPE_FOLDER) continue;
                if (!node.getName().ToLower().Equals("camera uploads")) continue;
                return node;
            }

            return null;
        }

        private static string GetBackgroundAgentUserAgent()
        {
            return String.Format("MEGAWindowsPhoneBackgroundAgent/{0}", "1.0.0.0");
        }

        private static void ShowAbortToast()
        {
            ShellToast toast = new ShellToast
            {
                Title = "MEGA Camera Uploads",
                Content = "Auto Camera Upload has been disabled"
            };
            toast.Show();
        }
    }
}