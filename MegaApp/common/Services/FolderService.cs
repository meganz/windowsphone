using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.ApplicationModel.Activation;
using Windows.Devices.Geolocation;
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

        public static bool IsEmptyFolder(string path)
        {
            int val1 = Directory.GetDirectories(path).Count();
            int val2 = Directory.GetFiles(path).Count();

            return (Directory.GetDirectories(path).Count() == 0 && Directory.GetFiles(path).Count() == 0) ? true : false;
        }

        public static void CreateFolder(string path)
        {
            Directory.CreateDirectory(path);
        }

        public static void DeleteFolder (string path)
        {
            Directory.Delete(path);
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
                folderPicker.ContinuationData["NodeData"] = nodeViewModel != null ? nodeViewModel.Handle : 0;

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

        public static void ContinueFolderOpenPicker(FolderPickerContinuationEventArgs args)
        {
            if ((args.ContinuationData["Operation"] as string) != "SelectDownloadFolder" || args.Folder == null)
            {
                ResetFolderPicker();
                return;
            }

            if (!App.CloudDrive.IsUserOnline()) return;

            if (args.ContinuationData["NodeData"] != null && (ulong) args.ContinuationData["NodeData"] != 0)
            {
                var handle = (ulong)args.ContinuationData["NodeData"];
                NodeViewModel node;
                if (App.CloudDrive.PublicNode != null && handle.Equals(App.CloudDrive.PublicNode.getHandle()))
                {
                    node = NodeService.CreateNew(App.MegaSdk, App.AppInformation, App.CloudDrive.PublicNode);
                    App.CloudDrive.PublicNode = null;
                }
                else
                {
                    node = NodeService.CreateNew(App.MegaSdk, App.AppInformation, App.MegaSdk.getNodeByHandle(handle));
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
            
            //App.CloudDrive.MultipleDownload(args.Folder);
            App.MainPageViewModel.ActiveFolderView.MultipleDownload(args.Folder);

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
