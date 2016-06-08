using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using mega;
using MegaApp.Classes;
using MegaApp.Models;
using MegaApp.Resources;

namespace MegaApp.MegaApi
{
    /// <summary>
    /// Common or base listener for folder and file links
    /// </summary>
    abstract class PublicLinkRequestListener : BaseRequestListener
    {
        /// <summary>
        /// Static variable to indicate if has already shown the decryption alert
        /// </summary>
        protected static bool _decryptionAlert { get; private set; }

        /// <summary>
        /// Static variable to store the file/folder link between the different calls 
        /// when the decryption key is missing or is not valid.
        /// </summary>
        protected static string _link { get; private set; }

        /// <summary>
        /// Static variable to store the FolderLinkViewModel when the user is 
        /// trying to open a folder link.
        /// </summary>
        protected static FolderLinkViewModel _folderLinkViewModel { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderLinkViewModel">
        /// Only used if the request type is "TYPE_LOGIN" (login to folder) in case of a folder link.
        /// In other case the received value should be null.
        /// </param>
        public PublicLinkRequestListener(FolderLinkViewModel folderLinkViewModel = null)
        {
            // Only if the received parameter is not null, substitute the static class variable.
            if (folderLinkViewModel != null)
                _folderLinkViewModel = folderLinkViewModel;
        }

        #region Private Methods

        /// <summary>
        /// Show a message indicating that the link is not valid.
        /// </summary>
        protected void ShowLinkNoValidAlert()
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
        protected void ShowUnavailableFileLinkAlert()
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
        /// Show a message indicating that the folder link is no longer available and 
        /// explaining the reasons that could be causing it.
        /// </summary>
        protected void ShowUnavailableFolderLinkAlert()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                new CustomMessageDialog(
                    AppMessages.AM_LinkUnavailableTitle,
                    AppMessages.AM_FolderLinkUnavailable,
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
        protected void ShowDecryptionAlert(MegaSDK api, MRequest request)
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
        protected void ShowDecryptionKeyNotValidAlert(MegaSDK api, MRequest request)
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
            // Get the used link only if the request type is for login to a folder link 
            // or get a public node from a file link
            if (request.getType() == MRequestType.TYPE_LOGIN ||
                request.getType() == MRequestType.TYPE_GET_PUBLIC_NODE)
            {
                _link = request.getLink();
            }                

            string[] splittedLink = SplitLink(_link);

            // If the decryption key already includes the "!" character, delete it.
            if (decryptionKey.StartsWith("!"))
                decryptionKey = decryptionKey.Substring(1);

            string link = String.Format("{0}!{1}!{2}", splittedLink[0], 
                splittedLink[1], decryptionKey);

            // If is a folder link
            if(splittedLink[0].EndsWith("#F"))
                api.loginToFolder(link , new LoginToFolderRequestListener(_folderLinkViewModel));
            else
                api.getPublicNode(link, this);
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
    }
}
