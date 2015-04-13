using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegaApp.Extensions
{
    static class SizeExtensions
    {
        private static readonly string[] SizeSuffixesBytes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        public static string ToStringAndSuffix(this UInt64 value)
        {
            if (value == 0) { return "0.0 bytes"; }

            int mag = (int)Math.Log(value, 1024);
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            return String.Format("{0:n2} {1}", adjustedSize, SizeSuffixesBytes[mag]);
        }

        public static ulong ToReadableSize(this UInt64 value)
        {
            if (value == 0) { return value; }

            int mag = (int)Math.Log(value, 1024);
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            return (ulong)adjustedSize;
        }

        public static string ToReadableUnits(this UInt64 value)
        {
            if (value == 0) { return " "; }

            int mag = (int)Math.Log(value, 1024);
            
            return SizeSuffixesBytes[mag];
        }

        public static string ToStringAndSuffixPerSecond(this UInt64 value)
        {
            if (value == 0) { return "0.0 bytes/s"; }

            int mag = (int)Math.Log(value, 1024);
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            return String.Format("{0:n2} {1}/s", adjustedSize, SizeSuffixesBytes[mag]);
        }

        public static ulong FromKBToBytes (this UInt64 value)
        {
            return value*1024;
        }

        public static ulong FromMBToBytes(this UInt64 value)
        {
            return (value *1024).FromKBToBytes();
        }

        public static ulong FromGBToBytes(this UInt64 value)
        {
            return (value * 1024).FromMBToBytes().FromKBToBytes();
        }
    }
}
