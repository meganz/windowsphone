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
    public class TransferStatusToIndeterminateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return true;

            switch ((TransferStatus)value)
            {
                case TransferStatus.NotStarted:
                case TransferStatus.Connecting:
                case TransferStatus.Pausing:
                case TransferStatus.Paused:
                    return true;
                case TransferStatus.Downloading:
                case TransferStatus.Uploading:
                case TransferStatus.Finished:
                case TransferStatus.Canceling:
                case TransferStatus.Canceled:
                case TransferStatus.Error:
                    return false;
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
