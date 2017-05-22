using System;
using System.Globalization;
using System.Windows.Data;
using mega;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp.Converters
{
    /// <summary>
    /// Converts from a `TransferState` value to a readable text
    /// </summary>
    public class TransferStateToReadableTextConverter : IValueConverter
    {
        /// <summary>
        /// Converts from a `TransferState` value to a readable text
        /// </summary>
        /// <param name="values">Object array with the `TransferType` and `TransferState` values being passed to the target.</param>
        /// <param name="targetType">The type of the target property, as a type reference.</param>
        /// <param name="parameter">Optional parameter. Not used.</param>
        /// <param name="culture">The language of the conversion.</param>
        /// <returns>String whit the transfer state.</returns>
        public object Convert(object values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null)
                return UiResources.UI_TransferStateNotStarted;

            var typeAndState = values as object[];
            if (typeAndState[0] == null || typeAndState[1] == null)
                return UiResources.UI_TransferStateNotStarted;

            var transferType = (MTransferType)typeAndState[0];
            var transferState = (MTransferState)typeAndState[1];

            switch (transferState)
            {
                case MTransferState.STATE_NONE:
                    switch (transferType)
                    {
                        case MTransferType.TYPE_DOWNLOAD:
                            return UiResources.UI_TransferStateNotStarted;
                        case MTransferType.TYPE_UPLOAD:
                            return UiResources.UI_TransferStatePreparing;
                        default:
                            throw new ArgumentOutOfRangeException("transferType", transferType, null);
                    }

                case MTransferState.STATE_QUEUED:
                    return UiResources.UI_TransferStateQueued;

                case MTransferState.STATE_ACTIVE:
                    switch(transferType)
                    {
                        case MTransferType.TYPE_DOWNLOAD:
                            return UiResources.UI_TransferStateDownloading;
                        case MTransferType.TYPE_UPLOAD:
                            return UiResources.UI_TransferStateUploading;
                        default:
                            throw new ArgumentOutOfRangeException("transferType", transferType, null);
                    }

                case MTransferState.STATE_PAUSED:
                    return UiResources.UI_TransferStatePaused;

                case MTransferState.STATE_RETRYING:
                    return UiResources.UI_TransferStateRetrying;

                case MTransferState.STATE_COMPLETING:
                    return UiResources.UI_TransferStateCompleting;

                case MTransferState.STATE_COMPLETED:
                    switch (transferType)
                    {
                        case MTransferType.TYPE_DOWNLOAD:
                            return UiResources.UI_TransferStateDownloaded;
                        case MTransferType.TYPE_UPLOAD:
                            return UiResources.UI_TransferStateUploaded;
                        default:
                            throw new ArgumentOutOfRangeException("transferType", transferType, null);
                    }

                case MTransferState.STATE_CANCELLED:
                    return UiResources.UI_TransferStateCanceled;

                case MTransferState.STATE_FAILED:
                    return UiResources.UI_TransferStateError;

                default:
                    throw new ArgumentOutOfRangeException("transferState", transferState, null);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
