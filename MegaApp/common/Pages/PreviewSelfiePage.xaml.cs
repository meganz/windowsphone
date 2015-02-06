using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using MegaApp.Enums;
using MegaApp.Extensions;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace MegaApp.Pages
{
    public partial class PreviewSelfiePage : PhoneApplicationPage
    {
        private readonly PreviewSelfieViewModel _previewSelfieViewModel;
        public PreviewSelfiePage()
        {
            _previewSelfieViewModel = new PreviewSelfieViewModel(NavigateService.GetNavigationData<BitmapImage>());
            this.DataContext = _previewSelfieViewModel;

            InitializeComponent();
        }

        private async void OnUploadClick(object sender, System.EventArgs e)
        {
            string fileName = String.Format("WP_Selfie_{0}{1:D2}{2:D2}{3}{4}.jpg", 
                DateTime.Now.Year,
                DateTime.Now.Month,
                DateTime.Now.Day,
                DateTime.Now.Hour,
                DateTime.Now.Minute);

            try
            {
                string newFilePath = Path.Combine(AppService.GetUploadDirectoryPath(true), fileName);
                using (var fs = new FileStream(newFilePath, FileMode.Create))
                {
                    await fs.WriteAsync(_previewSelfieViewModel.Selfie.ConvertToBytes().ToArray(), 0,
                            _previewSelfieViewModel.Selfie.ConvertToBytes().Count());
                    await fs.FlushAsync();
                    fs.Close();
                }

                var uploadTransfer = new TransferObjectModel(App.MegaSdk, App.CloudDrive.CurrentRootNode, TransferType.Upload, newFilePath);
                App.MegaTransfers.Insert(0, uploadTransfer);
                uploadTransfer.StartTransfer();

                App.CloudDrive.NoFolderUpAction = true;
                this.Dispatcher.BeginInvoke(() => NavigateService.NavigateTo(typeof(TransferPage), NavigationParameter.SelfieSelected));
            }
            catch (Exception)
            {
                MessageBox.Show(AppMessages.UploadSelfieFailed, AppMessages.UploadSelfieFailed_Title,
                    MessageBoxButton.OK);
            }
        }
    }
}