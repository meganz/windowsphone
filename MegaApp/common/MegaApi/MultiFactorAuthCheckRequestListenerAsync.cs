using mega;
using MegaApp.Enums;

namespace MegaApp.MegaApi
{
    class MultiFactorAuthCheckRequestListenerAsync : BaseRequestListenerAsync<MultiFactorAuthStatus>
    {
        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

            if (request.getType() == MRequestType.TYPE_MULTI_FACTOR_AUTH_CHECK)
            {
                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK: // Successfull check multi-factor authentication process
                        if (Tcs != null)
                        {
                            Tcs.TrySetResult(request.getFlag() ?
                                MultiFactorAuthStatus.Enabled :
                                MultiFactorAuthStatus.Disabled);
                        }
                        break;

                    default: // Default error processing
                        if (Tcs != null)
                            Tcs.TrySetResult(MultiFactorAuthStatus.Unknown);
                        break;
                }
            }
        }

        #endregion
    }
}
