using mega;

namespace MegaApp.MegaApi
{
    internal class MoveNodeRequestListenerAsync : BaseRequestListenerAsync<bool>
    {
        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

            if (request.getType() == MRequestType.TYPE_MOVE)
            {
                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK: // Successfull move node process
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
}
