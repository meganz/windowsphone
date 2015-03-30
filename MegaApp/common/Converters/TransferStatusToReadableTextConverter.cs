using System.IO;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using mega;
using MegaApp.Enums;
using MegaApp.Models;
using MegaApp.Resources;

namespace MegaApp.Converters
{
    public class TransferStatusToReadableTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return UiResources.Transfer_NotStarted;

            switch ((TransferStatus)value)
            {
                case TransferStatus.NotStarted:
                    return UiResources.Transfer_NotStarted.ToLower();
                case TransferStatus.Queued:
                    return UiResources.Transfer_Queued.ToLower();
                case TransferStatus.Downloading:
                    return UiResources.Transfer_Download.ToLower();
                case TransferStatus.Downloaded:
                    return UiResources.Transfer_Downloaded.ToLower();
                case TransferStatus.Uploading:
                    return UiResources.Transfer_Upload.ToLower();
                case TransferStatus.Uploaded:
                    return UiResources.Transfer_Uploaded.ToLower();
                case TransferStatus.Pausing:
                    return UiResources.Transfer_Pausing.ToLower();
                case TransferStatus.Paused:
                    return UiResources.Transfer_Paused.ToLower();
                case TransferStatus.Canceling:
                    return UiResources.Transfer_Canceling.ToLower();
                case TransferStatus.Canceled:
                    return UiResources.Transfer_Canceled.ToLower();                                                
                case TransferStatus.Error:
                    return UiResources.Transfer_Error.ToLower();
                default:
                    throw new ArgumentOutOfRangeException("value");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
