using System.Windows;
using mega;
using MegaApp.Enums;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    internal class ChangePasswordRequestListenerAsync : BaseRequestListenerAsync<ChangePasswordResult>
    {
        #region MRequestListenerInterface

        public override void onRequestStart(MegaSDK api, MRequest request)
        {
            base.onRequestStart(api, request);

            if (request.getType() != MRequestType.TYPE_CHANGE_PW) return;

            Deployment.Current.Dispatcher.BeginInvoke(() =>
                ProgressService.SetProgressIndicator(true, ProgressMessages.PM_ChangePassword));
        }

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

            if (request.getType() == MRequestType.TYPE_CHANGE_PW)
            {
                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK: // Successfull change password process
                        if (Tcs != null)
                            Tcs.TrySetResult(ChangePasswordResult.Success);
                        break;
                    case MErrorType.API_EFAILED: // Wrong MFA pin.
                    case MErrorType.API_EEXPIRED: // MFA pin is being re-used and is being denied to prevent a replay attack
                        if (Tcs != null)
                            Tcs.TrySetResult(ChangePasswordResult.MultiFactorAuthInvalidCode);
                        return;
                    default: // Default error processing
                        if (Tcs != null)
                            Tcs.TrySetResult(ChangePasswordResult.Unknown);
                        break;
                }
            }
        }

        #endregion
    }
}
