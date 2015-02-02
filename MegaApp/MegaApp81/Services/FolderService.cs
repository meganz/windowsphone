using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.ApplicationModel.Activation;
using Windows.Devices.Geolocation;
using Windows.Storage.Pickers;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Pages;
using MegaApp.Resources;

namespace MegaApp.Services
{
    static class FolderService
    {
        public static void SelectFolder(string operation, NodeViewModel nodeViewModel = null)
        {
            var folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.Downloads
            };

            folderPicker.FileTypeFilter.Add("*");

            folderPicker.ContinuationData["Operation"] = operation;
            folderPicker.ContinuationData["NodeData"] = nodeViewModel != null ? nodeViewModel.Handle : 0;

            folderPicker.PickFolderAndContinue();
        }

        public static async Task<bool> SelectDownloadFolder(NodeViewModel nodeViewModel = null)
        {
            if (SettingsService.LoadSetting<bool>(SettingsResources.AskDownloadLocationIsEnabled, false))
            {
                // Ask the user a download location when alsways asking is ON
                SelectFolder("SelectDownloadFolder", nodeViewModel);
                return false;
            }

            if (SettingsService.LoadSetting<string>(SettingsResources.DefaultDownloadLocation, null) == null)
            {

                switch (await DialogService.ShowOptionsDialog("Download location", AppMessages.NoDownloadLocationSelected,
                    new[] { "select folder", "preferences" }))
                {
                    case -1:
                        {
                            // Back button is pressed
                            return false;
                        }
                    case 0:
                        {
                            // Ask the user a download location
                            SelectFolder("SelectDownloadFolder", nodeViewModel);
                            return false;
                        }
                    case 1:
                        {
                            // Go to preferences page
                            App.CloudDrive.NoFolderUpAction = true;
                            NavigateService.NavigateTo(typeof(SettingsPage), NavigationParameter.Normal);
                            return false;
                        }
                }

            }

            return true;
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
                var node = NodeService.CreateNew(App.MegaSdk, App.MegaSdk.getNodeByHandle(handle));
                node.Download(args.Folder.Path);
                ResetFolderPicker();
                return;
            }

            App.CloudDrive.MultipleDownload(args.Folder);

            ResetFolderPicker();
        }

        private static void ResetFolderPicker()
        {
            // Reset the picker data
            var app = Application.Current as App;
            if (app != null) app.FolderPickerContinuationArgs = null;
        }
    }
}
