using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Windows.ApplicationModel.Activation;
using Windows.Storage.Pickers;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Pages;
using MegaApp.Resources;

namespace MegaApp.Services
{
    static class FolderService
    {
        public static bool FolderExists(string path)
        {  
            return Directory.Exists(path);
        }

        public static int GetNumChildFolders(string path)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(path)) return 0;
                return Directory.GetDirectories(path).Length;
            }
            catch (Exception) { return 0; }            
        }

        public static int GetNumChildFiles(string path, bool isOfflineFolder = false)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(path)) return 0;

                string[] childFiles = Directory.GetFiles(path);
                if (childFiles == null) return 0;

                int num = 0;
                if (!isOfflineFolder)
                {
                    num = childFiles.Length;
                }
                else
                {
                    foreach (var filePath in childFiles)
                        if (!FileService.IsPendingTransferFile(Path.GetFileName(filePath))) num++;
                }

                return num;
            }
            catch (Exception) { return 0; }
        }

        public static bool IsEmptyFolder(string path)
        {
            return (Directory.GetDirectories(path).Count() == 0 && Directory.GetFiles(path).Count() == 0) ? true : false;
        }

        public static void CreateFolder(string path)
        {
            Directory.CreateDirectory(path);            
        }
        
        public static void DeleteFolder(string path, bool recursive = false)
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                    Directory.Delete(path, recursive);
            }
            catch (Exception e)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    new CustomMessageDialog(
                        AppMessages.DeleteNodeFailed_Title,
                        String.Format(AppMessages.DeleteNodeFailed, e.Message),
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                });
            }
        }

        public static bool HasIllegalChars(string path)
        {
            var invalidChars = Path.GetInvalidPathChars();
            foreach (var c in invalidChars)
            {
                if (path.Contains(c.ToString())) return true;
            }
            return false;
        }        

        public static void Clear(string path)
        {
            try
            {
                IEnumerable<string> foldersToDelete = Directory.GetDirectories(path);
                if (foldersToDelete != null)
                {
                    foreach (var folder in foldersToDelete)
                    {
                        if (folder != null)
                            Directory.Delete(folder, true);
                    }
                }

                FileService.ClearFiles(Directory.GetFiles(path));
            }
            catch (IOException e)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    new CustomMessageDialog(
                            AppMessages.DeleteNodeFailed_Title,
                            String.Format(AppMessages.DeleteNodeFailed, e.Message),
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialog();
                });
            }
        }

        public static bool IsOfflineRootFolder(string path)
        {
            if (!path.Trim().EndsWith("\\"))
                path = path.Insert(path.Length, "\\");

            return (String.Compare(AppService.GetDownloadDirectoryPath(), path) == 0) ? true : false;
        }

        #if WINDOWS_PHONE_81
        public static void SelectFolder(string operation, NodeViewModel nodeViewModel = null)
        {
            try
            {
                App.FileOpenOrFolderPickerOpenend = true;

                var folderPicker = new FolderPicker
                {
                    SuggestedStartLocation = PickerLocationId.Downloads
                };

                folderPicker.FileTypeFilter.Add("*");

                folderPicker.ContinuationData["Operation"] = operation;
                folderPicker.ContinuationData["NodeData"] = nodeViewModel != null ? nodeViewModel.Base64Handle : null;

                folderPicker.PickFolderAndContinue();
            }
            catch (Exception e)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    new CustomMessageDialog(
                        AppMessages.SelectFolderFailed_Title,
                        String.Format(AppMessages.SelectFolderFailed, e.Message),
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                });
            }            
        }

        public static async Task<bool> SelectDownloadFolder(NodeViewModel nodeViewModel = null)
        {
            if (SettingsService.LoadSetting<bool>(SettingsResources.AskDownloadLocationIsEnabled, false))
            {
                // Ask the user a download location when alsways asking is ON
                SelectFolder("SelectDownloadFolder", nodeViewModel);
                return false;
            }

            if (SettingsService.LoadSetting<string>(SettingsResources.DefaultDownloadLocation, null) != null)
                return true;
            
            await DialogService.ShowOptionsDialog(UiResources.DownloadLocation, AppMessages.NoDownloadLocationSelected,
                new[]
                {
                    new DialogButton(UiResources.SelectFolder, () =>
                    {
                        // Ask the user a download location
                        SelectFolder("SelectDownloadFolder", nodeViewModel);
                    }),
                    new DialogButton(UiResources.Settings, () =>
                    {
                        // Go to preferences page
                        App.CloudDrive.PickerOrDialogIsOpen = false;
                        App.CloudDrive.NoFolderUpAction = true;
                        NavigateService.NavigateTo(typeof (SettingsPage), NavigationParameter.Normal);
                    })
                });
            return false;
        }

        public static void ContinueFolderOpenPicker(FolderPickerContinuationEventArgs args, FolderViewModel folderViewModel)
        {
            if ((args.ContinuationData["Operation"] as string) != "SelectDownloadFolder" || args.Folder == null)
            {
                ResetFolderPicker();
                return;
            }

            if (!App.CloudDrive.IsUserOnline()) return;

            if (args.ContinuationData["NodeData"] != null)
            {
                String base64Handle = (String)args.ContinuationData["NodeData"];
                NodeViewModel node;
                if (App.LinkInformation.PublicNode != null && base64Handle.Equals(App.LinkInformation.PublicNode.getBase64Handle()))
                {
                    node = NodeService.CreateNew(App.MegaSdk, App.AppInformation, App.LinkInformation.PublicNode, ContainerType.PublicLink);
                    App.LinkInformation.Reset();
                }
                else
                {
                    node = NodeService.CreateNew(folderViewModel.MegaSdk, App.AppInformation, 
                        folderViewModel.MegaSdk.getNodeByBase64Handle(base64Handle), folderViewModel.Type);
                }
               
                if(node != null)
                {
                    App.AppInformation.PickerOrAsyncDialogIsOpen = false;
                    node.Download(App.MegaTransfers, args.Folder.Path);
                }                    

                ResetFolderPicker();
                return;
            }

            App.AppInformation.PickerOrAsyncDialogIsOpen = false;

            folderViewModel.MultipleDownload(args.Folder.Path);

            ResetFolderPicker();
        }

        private static void ResetFolderPicker()
        {
            // Reset the picker data
            var app = Application.Current as App;
            if (app != null) app.FolderPickerContinuationArgs = null;            
        }
        #endif
    }    
}
