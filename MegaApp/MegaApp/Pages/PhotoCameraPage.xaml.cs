using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Windows.Phone.Media.Capture;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Services;
using Microsoft.Devices;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Media;

namespace MegaApp.Pages
{
    public partial class PhotoCameraPage : PhoneApplicationPage
    {
        private PhotoCamera _cam;
        private double _canvasWidth;
        private double _canvasHeight;
        private MediaLibrary _library = new MediaLibrary();

        public PhotoCameraPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if ((PhotoCamera.IsCameraTypeSupported(CameraType.Primary) == true) ||
                 (PhotoCamera.IsCameraTypeSupported(CameraType.FrontFacing) == true))
            {
                if (PhotoCamera.IsCameraTypeSupported(CameraType.FrontFacing))
                {
                    _cam = new Microsoft.Devices.PhotoCamera(CameraType.FrontFacing);
                    _cam.Initialized += new EventHandler<CameraOperationCompletedEventArgs>(cam_Initialized);
                    _cam.CaptureImageAvailable += new EventHandler<ContentReadyEventArgs>(cam_CaptureImageAvailable);
                    viewfinderBrush.SetSource(_cam);

                    CameraButtons.ShutterKeyPressed += OnButtonFullPress;
                   
                }
                else if (PhotoCamera.IsCameraTypeSupported(CameraType.Primary))
                {
                    _cam = new Microsoft.Devices.PhotoCamera(CameraType.Primary);
                    _cam.Initialized += new EventHandler<CameraOperationCompletedEventArgs>(cam_Initialized);
                    _cam.CaptureImageAvailable += new EventHandler<ContentReadyEventArgs>(cam_CaptureImageAvailable);
                    viewfinderBrush.SetSource(_cam);

                    CameraButtons.ShutterKeyPressed += OnButtonFullPress;
                
                }
            }
        }

        private double GetCameraAspectRatio()
        {
            IEnumerable<Size> resList = _cam.AvailableResolutions;

            if (resList.Count<Size>() > 0)
            {
                Size res = resList.ElementAt<Size>(0);
                return res.Width / res.Height;
            }

            return 1;
        }

        void cam_Initialized(object sender, Microsoft.Devices.CameraOperationCompletedEventArgs e)
        {
            if (e.Succeeded)
            {
                this.Dispatcher.BeginInvoke(delegate()
                {
                    _canvasHeight = Application.Current.Host.Content.ActualWidth;
                    _canvasWidth = _canvasHeight * GetCameraAspectRatio();

                    viewfinderCanvas.Width = _canvasWidth;
                    viewfinderCanvas.Height = _canvasHeight;
                });
            }
        }


        async void cam_CaptureImageAvailable(object sender, Microsoft.Devices.ContentReadyEventArgs e)
        {
            string fileName = Guid.NewGuid().ToString("N") + ".jpg";
            
            string newFilePath = Path.Combine(AppService.GetUploadDirectoryPath(), fileName);
            using (var fs = new FileStream(newFilePath, FileMode.Create))
            {
                await e.ImageStream.CopyToAsync(fs);
                await fs.FlushAsync();
                fs.Close();
            }

            var uploadTransfer = new TransferObjectModel(App.MegaSdk, App.CloudDrive.CurrentRootNode, TransferType.Upload, newFilePath);
            App.MegaTransfers.Insert(0, uploadTransfer);
            uploadTransfer.StartTransfer();
            
            App.CloudDrive.NoFolderUpAction = true;
            this.Dispatcher.BeginInvoke(delegate()
            {
                NavigateService.NavigateTo(typeof (TransferPage), NavigationParameter.PictureSelected);
            });

            
        }

        private void OnButtonFullPress(object sender, EventArgs e)
        {
            if (_cam != null)
            {
                _cam.CaptureImage();
            }
        }

        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            base.OnOrientationChanged(e);

            if (cameraViewBrushTransform == null) return;
            switch (e.Orientation)
            {
                case PageOrientation.Landscape:
                case PageOrientation.LandscapeLeft:
                    this.cameraViewBrushTransform.Rotation = -180;
                    break;
                case PageOrientation.LandscapeRight:
                    this.cameraViewBrushTransform.Rotation = 180;
                    break;
            }
        }
        
    }
}