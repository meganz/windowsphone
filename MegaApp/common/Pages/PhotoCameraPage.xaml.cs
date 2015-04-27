using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MegaApp.Enums;
using MegaApp.Resources;
using MegaApp.Services;
using Microsoft.Devices;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using Windows.Phone.Media.Capture;
using Size = Windows.Foundation.Size;

namespace MegaApp.Pages
{
    public partial class PhotoCameraPage : PhoneApplicationPage
    {
        public PhotoCaptureDevice PhotoCaptureDevice;
        
        // Constants
        private readonly Size _defaultCameraResolution = new Size(640, 480);

        private bool _isCameraInitialized = false;
        private bool _capturing = false;
        private readonly Semaphore _focusSemaphore = new Semaphore(1, 1);
        private bool _manuallyFocused = false;
        private Size _focusRegionSize = new Windows.Foundation.Size(80, 80);
        private readonly SolidColorBrush _notFocusedBrush = new SolidColorBrush(Colors.Red);
        private readonly SolidColorBrush _focusedBrush = new SolidColorBrush(Colors.Green);
      
        private MediaLibrary _library = new MediaLibrary();

        public PhotoCameraPage()
        {
            InitializeComponent();

            SetApplicationBar();

            VideoCanvas.Tap += VideoCanvasOnTap;
        }

        private void SetApplicationBar()
        {
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = UiResources.Capture.ToLower();
        }

        private async void VideoCanvasOnTap(object sender, GestureEventArgs e)
        {
            try
            {
                System.Windows.Point uiTapPoint = e.GetPosition(VideoCanvas);

                if (PhotoCaptureDevice.IsFocusRegionSupported(PhotoCaptureDevice.SensorLocation) && _focusSemaphore.WaitOne(0))
                {
                    // Get tap coordinates as a foundation point
                    var tapPoint = new Windows.Foundation.Point(uiTapPoint.X, uiTapPoint.Y);

                    double xRatio = VideoCanvas.ActualHeight / PhotoCaptureDevice.PreviewResolution.Width;
                    double yRatio = VideoCanvas.ActualWidth / PhotoCaptureDevice.PreviewResolution.Height;

                    // adjust to center focus on the tap point
                    var displayOrigin = new Windows.Foundation.Point(
                                tapPoint.Y - _focusRegionSize.Width / 2,
                                (VideoCanvas.ActualWidth - tapPoint.X) - _focusRegionSize.Height / 2);

                    // adjust for resolution difference between preview image and the canvas
                    var viewFinderOrigin = new Windows.Foundation.Point(displayOrigin.X / xRatio, displayOrigin.Y / yRatio);
                    var focusrect = new Windows.Foundation.Rect(viewFinderOrigin, _focusRegionSize);

                    // clip to preview resolution
                    var viewPortRect = new Windows.Foundation.Rect(0, 0, PhotoCaptureDevice.PreviewResolution.Width,
                        PhotoCaptureDevice.PreviewResolution.Height);
                    focusrect.Intersect(viewPortRect);

                    PhotoCaptureDevice.FocusRegion = focusrect;

                    // show a focus indicator
                    FocusIndicator.SetValue(Shape.StrokeProperty, _notFocusedBrush);
                    FocusIndicator.SetValue(Canvas.LeftProperty, uiTapPoint.X - _focusRegionSize.Width / 2);
                    FocusIndicator.SetValue(Canvas.TopProperty, uiTapPoint.Y - _focusRegionSize.Height / 2);
                    FocusIndicator.SetValue(VisibilityProperty, Visibility.Visible);

                    CameraFocusStatus status = await PhotoCaptureDevice.FocusAsync();

                    if (status == CameraFocusStatus.Locked)
                    {
                        FocusIndicator.SetValue(Shape.StrokeProperty, _focusedBrush);
                        _manuallyFocused = true;
                        PhotoCaptureDevice.SetProperty(KnownCameraPhotoProperties.LockedAutoFocusParameters,
                            AutoFocusParameters.Exposure & AutoFocusParameters.Focus & AutoFocusParameters.WhiteBalance);
                    }
                    else
                    {
                        _manuallyFocused = false;
                        PhotoCaptureDevice.SetProperty(KnownCameraPhotoProperties.LockedAutoFocusParameters, AutoFocusParameters.None);
                    }

                    _focusSemaphore.Release();
                }

                await Capture();
            }
            catch (Exception exception)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show(String.Format(AppMessages.CaptureVideoFailed, exception.Message),
                        AppMessages.CaptureVideoFailed_Title, MessageBoxButton.OK);
                });
            }            
        }

        /// <summary>
        /// Initializes camera. Once initialized the instance is set to the
        /// DataContext.Device property for further usage from this or other
        /// pages.
        /// </summary>
        /// <param name="sensorLocation">Camera sensor to initialize.</param>
        private async Task InitializeCamera(CameraSensorLocation sensorLocation)
        {
            // Find out the largest capture resolution available on device
            IReadOnlyList<Size> availableResolutions =
                PhotoCaptureDevice.GetAvailableCaptureResolutions(sensorLocation);

            var captureResolution = new Windows.Foundation.Size(0, 0);

            foreach (Size t in availableResolutions)
            {
                if (captureResolution.Width < t.Width)
                {
                    captureResolution = t;
                }
            }

            PhotoCaptureDevice device =
                await PhotoCaptureDevice.OpenAsync(sensorLocation, _defaultCameraResolution);

            await device.SetPreviewResolutionAsync(_defaultCameraResolution);
            await device.SetCaptureResolutionAsync(captureResolution);

            device.SetProperty(KnownCameraGeneralProperties.EncodeWithOrientation,
                          device.SensorLocation == CameraSensorLocation.Back ?
                          device.SensorRotationInDegrees : -device.SensorRotationInDegrees);

            PhotoCaptureDevice = device;
            _isCameraInitialized = true;
        }

        protected override async void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            try
            {
                ProgressService.SetProgressIndicator(true, "Loading camera...");
                if (Camera.IsCameraTypeSupported(CameraType.FrontFacing))
                {
                    await InitializeCamera(CameraSensorLocation.Front);
                }
                else
                {
                    MessageBox.Show("Your phone does not have a front facing camera for selfies. Back Camera is used");
                    await InitializeCamera(CameraSensorLocation.Back);
                }
                ProgressService.SetProgressIndicator(false);

                videoBrush.RelativeTransform = new CompositeTransform()
                {
                    CenterX = 0.5,
                    CenterY = 0.5,
                    Rotation = PhotoCaptureDevice.SensorLocation == CameraSensorLocation.Back
                        ? PhotoCaptureDevice.SensorRotationInDegrees
                        : -PhotoCaptureDevice.SensorRotationInDegrees,
                };

                videoBrush.SetSource(PhotoCaptureDevice);

                SetScreenButtonsEnabled(true);
                SetCameraButtonsEnabled(true);
            }
            catch(Exception ex)
            {
                MessageBox.Show(String.Format("There was an error during the camera initialization. Please, try again: [{0}]", ex.Message));
            }
            
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            // If the camera was not initialize yet, the app does nothing to avoid crashes
            if (!this._isCameraInitialized)
            {
                e.Cancel = true;
                return;
            }

            if (PhotoCaptureDevice != null && !e.Uri.ToString().Contains("SettingsPage.xaml"))
            {
                PhotoCaptureDevice.Dispose();
                PhotoCaptureDevice = null;
            }

            SetScreenButtonsEnabled(false);
            SetCameraButtonsEnabled(false);

            base.OnNavigatingFrom(e);
        }

        /// <summary>
        /// Enables or disabled on-screen controls.
        /// </summary>
        /// <param name="enabled">True to enable controls, false to disable controls.</param>
        private void SetScreenButtonsEnabled(bool enabled)
        {
            foreach (ApplicationBarIconButton b in ApplicationBar.Buttons)
            {
                b.IsEnabled = enabled;
            }

            foreach (ApplicationBarMenuItem m in ApplicationBar.MenuItems)
            {
                m.IsEnabled = enabled;
            }
        }

        /// <summary>
        /// Enables or disables listening to hardware shutter release key events.
        /// </summary>
        /// <param name="enabled">True to enable listening, false to disable listening.</param>
        private void SetCameraButtonsEnabled(bool enabled)
        {
            if (enabled)
            {
                CameraButtons.ShutterKeyHalfPressed += ShutterKeyHalfPressed;
                CameraButtons.ShutterKeyPressed += ShutterKeyPressed;
            }
            else
            {
                CameraButtons.ShutterKeyHalfPressed -= ShutterKeyHalfPressed;
                CameraButtons.ShutterKeyPressed -= ShutterKeyPressed;
            }
        }

        /// <summary>
        /// Clicking on the capture button initiates autofocus and captures a photo.
        /// </summary>
        private async void OnCaptureClick(object sender, EventArgs e)
        {
            try
            {
                if (!_manuallyFocused)
                {
                    await AutoFocus();
                }

                await Capture();
            }
            catch (Exception exception)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show(String.Format(AppMessages.CapturePhotoFailed, exception.Message),
                        AppMessages.CapturePhotoFailed_Title, MessageBoxButton.OK);
                });
            }            
        }

        /// <summary>
        /// Starts autofocusing, if supported. Capturing buttons are disabled while focusing.
        /// </summary>
        private async Task AutoFocus()
        {
            if (!_capturing && PhotoCaptureDevice.IsFocusSupported(PhotoCaptureDevice.SensorLocation))
            {
                SetScreenButtonsEnabled(false);
                SetCameraButtonsEnabled(false);

                await PhotoCaptureDevice.FocusAsync();

                SetScreenButtonsEnabled(true);
                SetCameraButtonsEnabled(true);

                _capturing = false;
            }
        }

        /// <summary>
        /// Captures a photo. Photo data is stored to DataContext.ImageStream, and application
        /// is navigated to the preview page after capturing.
        /// </summary>
        private async Task Capture()
        {
            try
            {
                bool goToPreview = false;

                var selfieBitmap = new BitmapImage();

                if (!_capturing)
                {
                    _capturing = true;

                    var stream = new MemoryStream();

                    CameraCaptureSequence sequence = PhotoCaptureDevice.CreateCaptureSequence(1);
                    sequence.Frames[0].CaptureStream = stream.AsOutputStream();

                    await PhotoCaptureDevice.PrepareCaptureSequenceAsync(sequence);
                    await sequence.StartCaptureAsync();

                    selfieBitmap = new BitmapImage();
                    selfieBitmap.SetSource(stream);

                    _capturing = false;

                    // Defer navigation as it will release the camera device and the
                    // following Device calls must still work.
                    goToPreview = true;
                }

                _manuallyFocused = false;

                if (PhotoCaptureDevice.IsFocusRegionSupported(PhotoCaptureDevice.SensorLocation))
                {
                    PhotoCaptureDevice.FocusRegion = null;
                }

                FocusIndicator.SetValue(VisibilityProperty, Visibility.Collapsed);
                PhotoCaptureDevice.SetProperty(KnownCameraPhotoProperties.LockedAutoFocusParameters, AutoFocusParameters.None);

                if (goToPreview)
                {
                    NavigateService.NavigateTo(typeof(PreviewSelfiePage), NavigationParameter.Normal, selfieBitmap);
                }
            }
            catch (Exception e)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show(String.Format(AppMessages.CapturePhotoVideoFailed, e.Message),
                        AppMessages.CapturePhotoVideoFailed_Title, MessageBoxButton.OK);
                });
            }            
        }

        private async void ShutterKeyPressed(object sender, EventArgs e)
        {
            await Capture();
        }

        private async void ShutterKeyHalfPressed(object sender, EventArgs e)
        {
            if (_manuallyFocused)
            {
                _manuallyFocused = false;
            }

            FocusIndicator.SetValue(VisibilityProperty, Visibility.Collapsed);
            await AutoFocus();
        }

        // Ensure that the viewfinder is upright in LandscapeRight.
        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            if (PhotoCaptureDevice != null)
            {
                // LandscapeRight rotation when camera is on back of phone.
                int landscapeRightRotation = 180;

                // Change LandscapeRight rotation for front-facing camera.
                if (PhotoCaptureDevice.SensorLocation == CameraSensorLocation.Front) landscapeRightRotation = -180;

                // Rotate video brush from camera.
                if (e.Orientation == PageOrientation.LandscapeRight)
                {
                    // Rotate for LandscapeRight orientation.
                    videoBrush.RelativeTransform =
                        new CompositeTransform() { CenterX = 0.5, CenterY = 0.5, Rotation = landscapeRightRotation };
                }
                else if (e.Orientation == PageOrientation.LandscapeLeft)
                {
                    // Rotate for standard landscape orientation.
                    videoBrush.RelativeTransform =
                        new CompositeTransform() { CenterX = 0.5, CenterY = 0.5, Rotation = 0 };
                }
                else
                {
                    // Rotate for standard landscape orientation.
                    videoBrush.RelativeTransform =
                        new CompositeTransform() { CenterX = 0.5, CenterY = 0.5, Rotation = landscapeRightRotation + 90 };
                }
            }
            
            base.OnOrientationChanged(e);
        }

        //protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        //{
        //    base.OnOrientationChanged(e);

        //    if (PhotoCaptureDevice == null) return;

        //    double canvasAngle;

        //    if (Orientation.HasFlag(PageOrientation.LandscapeLeft))
        //    {
        //        canvasAngle = PhotoCaptureDevice.SensorRotationInDegrees - 90;
        //    }
        //    else if (Orientation.HasFlag(PageOrientation.LandscapeRight))
        //    {
        //        canvasAngle = PhotoCaptureDevice.SensorRotationInDegrees + 90;
        //    }
        //    else if (Orientation.HasFlag(PageOrientation.PortraitUp))
        //    {
        //        canvasAngle = -90;
        //    }
        //    else
        //    {
        //        canvasAngle = 0;
        //    }

        //    videoBrush.RelativeTransform = new RotateTransform()
        //    {
        //        CenterX = 0.5,
        //        CenterY = 0.5,
        //        Angle = canvasAngle
        //    };

        //    PhotoCaptureDevice.SetProperty(KnownCameraGeneralProperties.EncodeWithOrientation, canvasAngle);
        //}


        //async void cam_CaptureImageAvailable(object sender, Microsoft.Devices.ContentReadyEventArgs e)
        //{
        //    string fileName = Guid.NewGuid().ToString("N") + ".jpg";
            
        //    string newFilePath = Path.Combine(AppService.GetUploadDirectoryPath(), fileName);
        //    using (var fs = new FileStream(newFilePath, FileMode.Create))
        //    {
        //        await e.ImageStream.CopyToAsync(fs);
        //        await fs.FlushAsync();
        //        fs.Close();
        //    }

        //    var uploadTransfer = new TransferObjectModel(App.MegaSdk, App.CloudDrive.CurrentRootNode, TransferType.Upload, newFilePath);
        //    App.MegaTransfers.Insert(0, uploadTransfer);
        //    uploadTransfer.StartTransfer();
            
        //    App.CloudDrive.NoFolderUpAction = true;
        //    this.Dispatcher.BeginInvoke(delegate()
        //    {
        //        NavigateService.NavigateTo(typeof (TransferPage), NavigationParameter.PictureSelected);
        //    });

            
        //}

        
    }
}