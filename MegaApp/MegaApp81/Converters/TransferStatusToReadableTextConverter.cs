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
                    return UiResources.Transfer_NotStarted;
                case TransferStatus.Connecting:
                    return UiResources.Transfer_Connecting;
                case TransferStatus.Downloading:
                    return UiResources.Transfer_Download;
                case TransferStatus.Uploading:
                    return UiResources.Transfer_Upload;
                case TransferStatus.Finished:
                    return UiResources.Transfer_Finished;
                case TransferStatus.Canceling:
                    return UiResources.Transfer_Canceling;
                case TransferStatus.Canceled:
                    return UiResources.Transfer_Canceled;
                case TransferStatus.Error:
                    return UiResources.Transfer_Error;
                case TransferStatus.Pausing:
                    return UiResources.Transfer_Pausing;
                case TransferStatus.Paused:
                    return UiResources.Transfer_Paused;                    
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
