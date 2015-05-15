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
                    MessageBox.Show(String.Format(AppMessages.SelectFolderFailed, e.Message),
                        AppMessages.SelectFolderFailed_Title, MessageBoxButton.OK);
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

            if (SettingsService.LoadSetting<string>(SettingsResources.DefaultDownloadLocation, null) == null)
            {

                switch (await DialogService.ShowOptionsDialog(UiResources.DownloadLocation, AppMessages.NoDownloadLocationSelected,
                    new[] { UiResources.SelectFolder.ToLower(), UiResources.Settings.ToLower() }))
                {
                    case -1:
                        {
                            // Back button is pressed
                            App.CloudDrive.PickerOrDialogIsOpen = false;
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
                            App.CloudDrive.PickerOrDialogIsOpen = false;
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
            App.CloudDrive.MultipleDownload(args.Folder);

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
