using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Windows.System;
using Telerik.Windows.Controls;
using ZXing;
using ZXing.QrCode;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Resources;
using MegaApp.Services;
using MegaApp.Views;

namespace MegaApp.ViewModels
{
    public class MultiFactorAuthAppSetupViewModel : BaseSdkViewModel
    {
        public MultiFactorAuthAppSetupViewModel() : base(SdkService.MegaSdk)
        {
            this.CopySeedCommand = new DelegateCommand(this.CopySeed);
            this.OpenInCommand = new DelegateCommand(this.OpenIn);
            this.VerifyCommand = new DelegateCommand(this.Verify);

            this.Initialize();
        }

        #region Commands

        public ICommand CopySeedCommand { get; private set; }
        public ICommand OpenInCommand { get; private set; }
        public ICommand VerifyCommand { get; private set; }

        #endregion

        #region Properties

        private WriteableBitmap _qrImage;
        /// <summary>
        /// Image of the QR code to set up the Multi-Factor Authentication
        /// </summary>
        public WriteableBitmap QRImage
        {
            get { return _qrImage; }
            set { SetField(ref _qrImage, value); }
        }

        private string _multiFactorAuthCode;
        /// <summary>
        /// Code or seed needed to enable the Multi-Factor Authentication
        /// </summary>
        public string MultiFactorAuthCode
        {
            get { return _multiFactorAuthCode; }
            set
            {
                if (!SetField(ref _multiFactorAuthCode, value)) return;
                OnPropertyChanged("MultiFactorAuthCodeParts");
            }
        }

        /// <summary>
        /// Code or seed needed to enable the Multi-Factor Authentication
        /// divided in 4-digits groups
        /// </summary>
        public ObservableCollection<string> MultiFactorAuthCodeParts
        {
            get { return this.SplitMultiFactorAuthCode(MultiFactorAuthCode, 4); }
        }

        private string _verifyCode;
        /// <summary>
        /// Code typed by the user to verify that the Multi-Factor Authentication is working
        /// </summary>
        public string VerifyCode
        {
            get { return _verifyCode; }
            set
            {
                SetField(ref _verifyCode, value);
                SetVerifyButtonState();
                this.WarningText = string.Empty;
            }
        }

        private string _warningText;
        /// <summary>
        /// Warning message (verification failed)
        /// </summary>
        public string WarningText
        {
            get { return _warningText; }
            set { SetField(ref _warningText, value); }
        }

        private bool _verifyButtonState;
        /// <summary>
        /// State (enabled/disabled) of the verify button
        /// </summary>
        public bool VerifyButtonState
        {
            get { return _verifyButtonState; }
            set { SetField(ref _verifyButtonState, value); }
        }

        private string codeURI
        {
            get
            {
                return string.Format("otpauth://totp/MEGA:{0}?secret={1}&issuer=MEGA",
                    SdkService.MegaSdk.getMyEmail(), this.MultiFactorAuthCode);
            }
        }

        #endregion

        #region Methods

        private async void Initialize()
        {
            var multiFactorAuthGetCode = new MultiFactorAuthGetCodeRequestListenerAsync();
            this.MultiFactorAuthCode = await multiFactorAuthGetCode.ExecuteAsync(() =>
                SdkService.MegaSdk.multiFactorAuthGetCode(multiFactorAuthGetCode));

            this.SetQR();
        }

        private void CopySeed(object obj = null)
        {
            try
            {
                Clipboard.SetText(this.MultiFactorAuthCode);
                new CustomMessageDialog(
                    AppMessages.AM_MFA_SeedCopied_Title,
                    AppMessages.AM_MFA_SeedCopied,
                    App.AppInformation,
                    MessageDialogButtons.Ok).ShowDialog();
            }
            catch (Exception)
            {
                new CustomMessageDialog(
                    AppMessages.AM_MFA_SeedCopiedFailed_Title,
                    AppMessages.AM_MFA_SeedCopiedFailed,
                    App.AppInformation,
                    MessageDialogButtons.Ok).ShowDialog();
            }
        }

        private async void OpenIn(object obj = null)
        {
            await Launcher.LaunchUriAsync(new Uri(this.codeURI, UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        /// Set the QR code image to set up the Multi-Factor Authentication
        /// </summary>
        private void SetQR()
        {
            var options = new QrCodeEncodingOptions()
            {
                DisableECI = true,
                CharacterSet = "UTF-8",
                Width = 148,
                Height = 148
            };

            BarcodeWriter writer = new BarcodeWriter();
            writer.Format = BarcodeFormat.QR_CODE;
            writer.Options = options;
            this.QRImage = writer.Write(
                string.Format("otpauth://totp/MEGA:{0}?secret={1}&issuer=MEGA",
                SdkService.MegaSdk.getMyEmail(), this.MultiFactorAuthCode));
        }

        /// <summary>
        /// Verify the 6-digit code typed by the user to set up the Multi-Factor Authentication
        /// </summary>
        private async void Verify(object obj = null)
        {
            if (string.IsNullOrWhiteSpace(this.VerifyCode)) return;

            this.ControlState = false;
            this.SetVerifyButtonState();
            this.IsBusy = true;

            var enableMultiFactorAuth = new MultiFactorAuthEnableRequestListenerAsync();
            var result = await enableMultiFactorAuth.ExecuteAsync(() =>
                SdkService.MegaSdk.multiFactorAuthEnable(this.VerifyCode, enableMultiFactorAuth));

            this.ControlState = true;
            this.SetVerifyButtonState();
            this.IsBusy = false;

            if (!result)
            {
                this.WarningText = AppMessages.AM_InvalidCode;
                return;
            }

            OnUiThread(() =>
            {
                NavigateService.NavigateTo(typeof(SettingsPage),
                    NavigationParameter.MFA_Enabled);
            });
        }

        private void SetVerifyButtonState()
        {
            var enabled = NetworkService.IsNetworkAvailable() &&
                this.ControlState && !string.IsNullOrWhiteSpace(this.VerifyCode) &&
                this.VerifyCode.Length == 6;

            OnUiThread(() => this.VerifyButtonState = enabled);
        }

        private ObservableCollection<string> SplitMultiFactorAuthCode(string str, int chunkSize)
        {
            if (string.IsNullOrWhiteSpace(str)) return new ObservableCollection<string>();

            var parts = new ObservableCollection<string>(
                Enumerable.Range(0, str.Length / chunkSize).Select(i => str.Substring(i * chunkSize, chunkSize)));
            parts.Insert(10, string.Empty); //For a correct alignment of the three last blocks
            return parts;
        }

        #endregion
    }
}
