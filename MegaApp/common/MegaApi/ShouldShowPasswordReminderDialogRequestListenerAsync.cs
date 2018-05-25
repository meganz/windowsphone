using mega;

namespace MegaApp.MegaApi
{
    internal class ShouldShowPasswordReminderDialogRequestListenerAsync : BaseRequestListenerAsync<bool>
    {
        #region MRequestListenerInterface

        public override void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            base.onRequestFinish(api, request, e);

            if (Tcs.Task.IsFaulted) return;

            if (request.getType() == MRequestType.TYPE_GET_ATTR_USER)
            {
                switch (e.getErrorCode())
                {
                    case MErrorType.API_OK:     // Successfull get user attribute process
                    case MErrorType.API_ENOENT: // Attribute not exists yet but the value still be valid
                        if (Tcs != null)
                            Tcs.TrySetResult(request.getFlag());
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
