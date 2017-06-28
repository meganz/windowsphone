using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mega;
using MegaApp.Services;

namespace MegaApp.Extensions
{
    static class SizeExtensions
    {
        private static readonly string[] SizeSuffixesBytes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        public static string ToStringAndSuffix(this UInt64 value, int numDecimaDigits = 0)
        {
            try
            {
                if (value == 0) { return "0.0 bytes"; }

                int mag = (int)Math.Log(value, 1024);
                decimal adjustedSize = (decimal)value / (1L << (mag * 10));

                var formatString = "{0:n" + numDecimaDigits + "} {1}";

                return String.Format(formatString, adjustedSize, SizeSuffixesBytes[mag]);
            }
            catch (Exception e) 
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error formatting the size value", e);
                return "0.0 bytes"; 
            }
        }

        public static ulong ToReadableSize(this UInt64 value)
        {
            try
            {
                if (value == 0) { return value; }

                int mag = (int)Math.Log(value, 1024);
                decimal adjustedSize = (decimal)value / (1L << (mag * 10));

                return (ulong)adjustedSize;
            }
            catch (Exception e) 
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error formatting the size value", e);
                return value; 
            }
        }

        public static string ToReadableUnits(this UInt64 value)
        {
            try
            {
                if (value == 0) { return " "; }

                int mag = (int)Math.Log(value, 1024);

                return SizeSuffixesBytes[mag];
            }
            catch (Exception e) 
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error formatting the size value", e);
                return " "; 
            }
        }

        public static string ToStringAndSuffixPerSecond(this UInt64 value)
        {
            try
            {
                if (value == 0) { return "0.0 bytes/s"; }

                int mag = (int)Math.Log(value, 1024);
                decimal adjustedSize = (decimal)value / (1L << (mag * 10));

                return String.Format("{0:n2} {1}/s", adjustedSize, SizeSuffixesBytes[mag]);
            }
            catch (Exception e) 
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error formatting the size value", e);
                return "0.0 bytes/s"; 
            }
        }

        public static ulong FromKBToBytes(this UInt64 value)
        {
            return value*1024;
        }

        public static ulong FromMBToBytes(this UInt64 value)
        {
            return (value *1024).FromKBToBytes();
        }

        public static ulong FromGBToBytes(this UInt64 value)
        {
            return (value * 1024).FromMBToBytes();
        }
    }
}
