using mega;

namespace MegaApp.MegaApi
{
    class MultiFactorAuthGetCodeRequestListenerAsync : BaseRequestListenerAsync<string>
    {
        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

            if (request.getType() == MRequestType.TYPE_MULTI_FACTOR_AUTH_GET)
            {
                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK: // Successfull get multi-factor authentication code process
                        if (Tcs != null)
                            Tcs.TrySetResult(request.getText());
                        break;
                    default: // Default error processing
                        if (Tcs != null)
                            Tcs.TrySetResult(null);
                        break;
                }
            }
        }

        #endregion
    }
}
