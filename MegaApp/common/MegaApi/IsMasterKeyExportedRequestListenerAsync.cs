using mega;

namespace MegaApp.MegaApi
{
    internal class IsMasterKeyExportedRequestListenerAsync : BaseRequestListenerAsync<bool>
    {
        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

            if (request.getType() == MRequestType.TYPE_GET_ATTR_USER &&
                request.getParamType() == (int)MUserAttrType.USER_ATTR_PWD_REMINDER)
            {
                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK: // Successfull get user attribute process
                        if (Tcs != null)
                            Tcs.TrySetResult(request.getAccess() == 1);
                        break;

                    case MErrorType.API_ENOENT: // User attribute is not set yet
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
