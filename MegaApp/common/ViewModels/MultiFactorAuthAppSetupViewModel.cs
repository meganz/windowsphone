using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Windows.System;
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
            this.FindAppCommand = new DelegateCommand(this.FindApp);
            this.NextCommand = new DelegateCommand(this.EnableMultiFactorAuth);
            this.OpenInCommand = new DelegateCommand(this.OpenIn);

            this.Initialize();
        }

        #region Commands

        public ICommand CopySeedCommand { get; private set; }
        public ICommand FindAppCommand { get; private set; }
        public ICommand NextCommand { get; private set; }
        public ICommand OpenInCommand { get; private set; }

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

        private async void FindApp(object obj = null)
        {
            await Launcher.LaunchUriAsync(new Uri(
                "ms-windows-store:search?keyword=authenticator",
                UriKind.RelativeOrAbsolute));
        }

        private async void OpenIn(object obj = null)
        {
            await Launcher.LaunchUriAsync(new Uri(this.codeURI, UriKind.RelativeOrAbsolute));
        }

        private async void EnableMultiFactorAuth(object obj = null)
        {
            await DialogService.ShowAsyncMultiFactorAuthCodeInputDialogAsync(
                this.EnableMultiFactorAuthAsync, 
                UiResources.UI_TwoFactorAuth, 
                UiResources.UI_MFA_SetupStep2, 
                false);
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
        /// Enable the Multi-Factor Authentication
        /// </summary>
        private async Task<bool> EnableMultiFactorAuthAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return false;

            var enableMultiFactorAuth = new MultiFactorAuthEnableRequestListenerAsync();
            var result = await enableMultiFactorAuth.ExecuteAsync(() =>
                SdkService.MegaSdk.multiFactorAuthEnable(code, enableMultiFactorAuth));

            if (!result)
            {
                DialogService.SetMultiFactorAuthCodeInputDialogWarningMessage();
                return result;
            }

            OnUiThread(() =>
            {
                NavigateService.NavigateTo(typeof(SettingsPage),
                    NavigationParameter.MFA_Enabled);
            });

            return result;
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
