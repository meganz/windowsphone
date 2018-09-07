using System.Windows;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Extensions;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    /// <summary>
    /// Request to fetch nodes at one account (login) or a folder link (login to folder).
    /// </summary>
    class FetchNodesRequestListenerAsync : BaseRequestListenerAsync<FetchNodesResult>
    {
        /// <summary>
        /// Variable to indicate if has already shown the decryption alert (folder links).
        /// </summary>
        public bool DecryptionAlert { get; set; }

        #region MRequestListenerInterface

        public override void onRequestStart(MegaSDK api, MRequest request)
        {
            base.onRequestStart(api, request);

            if (request.getType() != MRequestType.TYPE_FETCH_NODES) return;

            UiService.OnUiThread(() =>
                ProgressService.SetProgressIndicator(true, ProgressMessages.PM_FetchNodes));
        }

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

            if (request.getType() == MRequestType.TYPE_FETCH_NODES)
            {
                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK: // Successfull fetch nodes process
                        //If getFlag() returns true, the folder link key is invalid.
                        if (request.getFlag())
                        {
                            if (Tcs != null)
                            {
                                Tcs.TrySetResult(DecryptionAlert ?
                                    FetchNodesResult.InvalidDecryptionKey : // No valid decryption key
                                    FetchNodesResult.InvalidHandleOrDecryptionKey); // Handle length or Key length no valid
                            }
                            break;
                        }

                        if (App.MainPageViewModel != null)
                            UiService.OnUiThread(() => App.MainPageViewModel.GetAccountDetails());
                        //AccountService.GetUserData();
                        //AccountService.GetAccountDetails();

                        if (Tcs != null)
                            Tcs.TrySetResult(FetchNodesResult.Success);
                        break;

                    case MErrorType.API_ETOOMANY: // Taken down link and the link owner's account is blocked
                        if (Tcs != null)
                            Tcs.TrySetResult(FetchNodesResult.AssociatedUserAccountTerminated);
                        return;

                    case MErrorType.API_ENOENT: // Link not exists or has been deleted by user
                    case MErrorType.API_EBLOCKED: // Taken down link
                        if (Tcs != null)
                            Tcs.TrySetResult(FetchNodesResult.UnavailableLink);
                        break;

                    default: // Default error processing
                        if (Tcs != null)
                            Tcs.TrySetResult(FetchNodesResult.Unknown);
                        break;
                }
            }
        }

        public override void onRequestUpdate(MegaSDK api, MRequest request)
        {
            base.onRequestUpdate(api, request);

            if (request.getType() != MRequestType.TYPE_FETCH_NODES) return;

            if (request.getTotalBytes() > 0)
            {
                double progressValue = 100.0 * request.getTransferredBytes() / request.getTotalBytes();
                if ((progressValue > 99) || (progressValue < 0))
                {
                    UiService.OnUiThread(() => 
                        ProgressService.SetProgressIndicator(true, ProgressMessages.PM_DecryptNodes));
                }
            }

            if (AppMemoryController.IsThresholdExceeded(75UL.FromMBToBytes()))
            {
                UiService.OnUiThread(() =>
                {
                    new CustomMessageDialog(
                        AppMessages.MemoryLimitError_Title,
                        AppMessages.MemoryLimitError,
                        App.AppInformation,
                        MessageDialogButtons.Ok).ShowDialog();
                    Application.Current.Terminate();
                });
            }
        }

        #endregion
    }
}
