using mega;

namespace MegaApp.MegaApi
{
    abstract class MultiFactorAuthSetRequestListenerAsync : BaseRequestListenerAsync<bool>
    {
        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

            if (request.getType() == MRequestType.TYPE_MULTI_FACTOR_AUTH_SET)
            {
                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK: // Successfull set multi-factor authentication
                        if (Tcs != null)
                            Tcs.TrySetResult(true);
                        break;
                    default: // Default error processing
                        if (Tcs != null)
                            Tcs.TrySetResult(false);
                        break;
                }
            }
        }

        #endregion
    }

    internal class MultiFactorAuthEnableRequestListenerAsync : MultiFactorAuthSetRequestListenerAsync { }
    internal class MultiFactorAuthDisableRequestListenerAsync : MultiFactorAuthSetRequestListenerAsync { }
}
