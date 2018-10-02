using mega;
using MegaApp.Classes;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.MegaApi
{
    internal class FastLoginRequestListenerAsync : BaseRequestListenerAsync<bool>
    {
        #region MRequestListenerInterface

        public override void onRequestStart(MegaSDK api, MRequest request)
        {
            base.onRequestStart(api, request);

            if (request.getType() != MRequestType.TYPE_LOGIN) return;

            UiService.OnUiThread(() =>
                ProgressService.SetProgressIndicator(true, ProgressMessages.FastLogin));
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
                            Tcs.TrySetResult(true);
                        return;
                    case MErrorType.API_ESID: // Bad session ID
                        if (Tcs != null)
                        {
                            Tcs.TrySetException(new BadSessionIdException());
                            Tcs.TrySetResult(false);
                        }
                        return;
                    default: // Unknown result, but not successful
                        if (Tcs != null)
                            Tcs.TrySetResult(false);
                        return;
                }
            }
        }

        #endregion
    }
}
