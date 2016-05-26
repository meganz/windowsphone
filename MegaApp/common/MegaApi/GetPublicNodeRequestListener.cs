using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Extensions;
using MegaApp.Models;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    class GetPublicNodeRequestListener: BaseRequestListener
    {
        private readonly FolderViewModel _folderViewModel;
        private bool _decryptionAlert;
        
        public GetPublicNodeRequestListener(FolderViewModel folderViewModel)
        {
            this._folderViewModel = folderViewModel;
        }

        #region Base Properties

        protected override string ProgressMessage
        {
            get { return ProgressMessages.PM_OpenFileLink; }
        }

        protected override bool ShowProgressMessage
        {
            get { return true; }
        }

        protected override string ErrorMessage
        {
            get { return AppMessages.AM_OpenLinkFailed; }
        }

        protected override string ErrorMessageTitle
        {
            get { return AppMessages.AM_OpenLinkFailed_Title; }
        }

        protected override bool ShowErrorMessage
        {
            get { return true; }
        }

        protected override string SuccessMessage
        {
            get { throw new NotImplementedException(); }
        }

        protected override string SuccessMessageTitle
        {
            get { throw new NotImplementedException(); }
        }

        protected override bool ShowSuccesMessage
        {
            get { return false; }
        }

        protected override bool NavigateOnSucces
        {
            get { return false; }
        }

        protected override bool ActionOnSucces
        {
            get { return true; }
        }

        protected override Type NavigateToPage
        {
            get { throw new NotImplementedException(); }
        }

        protected override NavigationParameter NavigationParameter
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Show a message indicating that the link is not valid.
        /// </summary>
        private void ShowLinkNoValidAlert()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                new CustomMessageDialog(
                    ErrorMessageTitle,
                    AppMessages.AM_InvalidLink,
                    App.AppInformation,
                    MessageDialogButtons.Ok).ShowDialog();
            });
        }

        /// <summary>
        /// Show a message indicating that the file link is no longer available and 
        /// explaining the reasons that could be causing it.
        /// </summary>
        private void ShowUnavailableLinkAlert()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                new CustomMessageDialog(
                    AppMessages.AM_LinkUnavailableTitle,
                    AppMessages.AM_FileLinkUnavailable,
                    App.AppInformation,
                    MessageDialogButtons.Ok).ShowDialog();
            });
        }

        /// <summary>
        /// Show a message indicating that the encrypted file link can't be opened 
        /// because it doesn't include the key to see its contents.
        /// <para>Also asks introduce the decryption key.</para>
        /// </summary>
        /// <param name="api">MegaSDK object that started the request</param>
        /// <param name="request">Information about the request.</param>
        private void ShowDecryptionAlert(MegaSDK api, MRequest request)
        {
            _decryptionAlert = true;            

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                var inputDialog = new CustomInputDialog(
                    AppMessages.AM_DecryptionKeyAlertTitle,
                    AppMessages.AM_DecryptionKeyAlertMessage,
                    App.AppInformation);

                inputDialog.OkButtonTapped += (sender, args) =>
                    OpenLink(api, request, args.InputText);

                inputDialog.ShowDialog();
            });
        }

        /// <summary>
        /// Show a message indicating that the introduced decryption key is not valid.
        /// <para>Also asks introduce the correct decryption key.</para>
        /// </summary>
        /// <param name="api">MegaSDK object that started the request</param>
        /// <param name="request">Information about the request.</param>
        private void ShowDecryptionKeyNotValidAlert(MegaSDK api, MRequest request)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                var inputDialog = new CustomInputDialog(
                    AppMessages.AM_DecryptionKeyNotValid,
                    AppMessages.AM_DecryptionKeyAlertMessage,
                    App.AppInformation);

                inputDialog.OkButtonTapped += (sender, args) =>
                    OpenLink(api, request, args.InputText);

                inputDialog.ShowDialog();
            });
        }

        /// <summary>
        /// Open a MEGA file link providing its decryption key.        
        /// </summary>        
        /// <param name="api">MegaSDK object that started the request</param>
        /// <param name="request">Information about the request.</param>
        /// <param name="decryptionKey">Decryption key of the link.</param>
        private void OpenLink(MegaSDK api, MRequest request, String decryptionKey)
        {
            string[] splittedLink = SplitLink(request.getLink());

            // If the decryption key already includes the "!" character, delete it.
            if (decryptionKey.StartsWith("!"))
                decryptionKey = decryptionKey.Substring(1);

            api.getPublicNode(String.Format("{0}!{1}!{2}", splittedLink[0],
                splittedLink[1], decryptionKey), this);
        }

        /// <summary>
        /// Split the MEGA link in its three parts, separated by the "!" chartacter.
        /// <para>1. MEGA Url address.</para>
        /// <para>2. Node handle.</para>
        /// <para>3. Decryption key.</para>
        /// </summary>        
        /// <param name="link">Link to split.</param>
        /// <returns>Char array with the parts of the link.</returns>
        private string[] SplitLink(string link)
        {
            string delimStr = "!";
            return link.Split(delimStr.ToCharArray(), 3);
        }

        #endregion

        #region Override Methods

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ProgressService.ChangeProgressBarBackgroundColor((Color)Application.Current.Resources["PhoneChromeColor"]);
                ProgressService.SetProgressIndicator(false);
            });

            if (e.getErrorCode() == MErrorType.API_OK)
            {
                //If getFlag() returns true, the file link key is invalid.
                if (request.getFlag())
                {
                    if (_decryptionAlert)
                        ShowDecryptionKeyNotValidAlert(api, request);
                    else
                        ShowLinkNoValidAlert();
                }
                else
                {
                    OnSuccesAction(api, request);
                }    
            }
            else
            {
                switch(e.getErrorCode())
                {
                    case MErrorType.API_EARGS:
                        if (_decryptionAlert)
                            ShowDecryptionKeyNotValidAlert(api, request);
                        else
                            ShowLinkNoValidAlert();
                        break;

                    case MErrorType.API_ENOENT:
                        ShowUnavailableLinkAlert();
                        break;

                    case MErrorType.API_EINCOMPLETE:
                        ShowDecryptionAlert(api, request);
                        break;
                }
            }
        }

        protected override void OnSuccesAction(MegaSDK api, MRequest request)
        {
            App.ActiveImportLink = request.getLink();
            MNode publicNode = App.PublicNode = request.getPublicMegaNode();

            if (publicNode != null)
            {
                #if WINDOWS_PHONE_80
                // Detect if is an image to allow directly download to camera albums
                bool isImage = false;
                if (publicNode.isFile())
                {
                    FileNodeViewModel node = new FileNodeViewModel(api, null, publicNode, ContainerType.PublicLink);
                    isImage = node.IsImage;
                }
                #endif                
                
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        #if WINDOWS_PHONE_80
                        DialogService.ShowOpenLink(publicNode, _folderViewModel, isImage);
                        #elif WINDOWS_PHONE_81
                        DialogService.ShowOpenLink(publicNode, _folderViewModel);
                        #endif
                    }
                    catch (Exception e)
                    {
                        new CustomMessageDialog(
                            ErrorMessageTitle,
                            String.Format(ErrorMessage, e.Message),
                            App.AppInformation,
                            MessageDialogButtons.Ok).ShowDialog();
                    }
                });
            }                
            else
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    new CustomMessageDialog(
                        ErrorMessageTitle,
                        AppMessages.AM_ImportFileFailedNoErrorCode,
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                });
        }

        #endregion
    }
}
