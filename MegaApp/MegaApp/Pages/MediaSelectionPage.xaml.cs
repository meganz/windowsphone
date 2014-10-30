using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Services;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Media;
using Telerik.Windows.Controls;

namespace MegaApp.Pages
{
    public partial class MediaSelectionPage : PhoneApplicationPage
    {
        public MediaSelectionPage()
        {
            var mediaSelectionPageModel = new MediaSelectionPageModel(App.MegaSdk);
            this.DataContext = mediaSelectionPageModel;
            InitializeComponent();

            InteractionEffectManager.AllowedTypes.Add(typeof(RadDataBoundListBoxItem));
        }

        private async void OnAcceptClick(object sender, System.EventArgs e)
        {
            if (LstMediaItems.CheckedItems.Count < 1)
            {
                MessageBox.Show("Please select minimal 1 picture", "No pictures selected", MessageBoxButton.OK);
                return;
            }

            ProgessService.SetProgressIndicator(true, "Preparing uploads...");

            foreach (var checkedItem in LstMediaItems.CheckedItems)
            {
                var item = (BaseMediaViewModel<Picture>) checkedItem;

                string fileName = Path.GetFileName(item.Name);
                if (fileName != null)
                {
                    string newFilePath = Path.Combine(AppService.GetUploadDirectoryPath(), fileName);
                    using (var fs = new FileStream(newFilePath, FileMode.Create))
                    {
                        await item.BaseObject.GetImage().CopyToAsync(fs);
                        await fs.FlushAsync();
                        fs.Close();
                    }
                    var uploadTransfer = new TransferObjectModel(App.MegaSdk, App.CloudDrive.CurrentRootNode, TransferType.Upload, newFilePath);
                    App.MegaTransfers.Add(uploadTransfer);
                    uploadTransfer.StartTransfer();
                }
            }
            ProgessService.SetProgressIndicator(false);

            App.CloudDrive.NoFolderUpAction = true;
            NavigateService.NavigateTo(typeof(TransferPage), NavigationParameter.PictureSelected);
        }

        
    }
}