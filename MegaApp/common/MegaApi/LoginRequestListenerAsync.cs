using System.Windows;
using mega;
using MegaApp.Enums;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    internal class LoginRequestListenerAsync : BaseRequestListenerAsync<LoginResult>
    {
        #region MRequestListenerInterface

        public override void onRequestStart(MegaSDK api, MRequest request)
        {
            base.onRequestStart(api, request);

            Deployment.Current.Dispatcher.BeginInvoke(() =>
                ProgressService.SetProgressIndicator(true, ProgressMessages.PM_Login));
        }

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

            if (request.getType() == MRequestType.TYPE_LOGIN)
            {
                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK: // Login was successful
                        if (Tcs != null)
                            Tcs.TrySetResult(LoginResult.Success);
                        return;
                    case MErrorType.API_ENOENT: // Email unassociated with a MEGA account or Wrong password
                        if (Tcs != null)
                            Tcs.TrySetResult(LoginResult.UnassociatedEmailOrWrongPassword);
                        return;
                    case MErrorType.API_ETOOMANY: // Too many failed login attempts. Wait one hour.
                        if (Tcs != null)
                            Tcs.TrySetResult(LoginResult.TooManyLoginAttempts);
                        return;
                    case MErrorType.API_EINCOMPLETE: // Account not confirmed
                        if (Tcs != null)
                            Tcs.TrySetResult(LoginResult.AccountNotConfirmed);
                        return;
                    case MErrorType.API_EMFAREQUIRED: // Multi-factor authentication required
                        if (Tcs != null)
                            Tcs.TrySetResult(LoginResult.MultiFactorAuthRequired);
                        return;
                    case MErrorType.API_EFAILED: // Wrong MFA pin.
                    case MErrorType.API_EEXPIRED: // MFA pin is being re-used and is being denied to prevent a replay attack
                        if (Tcs != null)
                            Tcs.TrySetResult(LoginResult.MultiFactorAuthInvalidCode);
                        return;
                    default: // Unknown result, but not successful
                        if (Tcs != null)
                            Tcs.TrySetResult(LoginResult.Unknown);
                        return;
                }
            }
        }

        #endregion
    }
}
