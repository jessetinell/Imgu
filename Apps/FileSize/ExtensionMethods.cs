﻿using System;

namespace Imgu.Apps.FileSize
{
    static class ExtensionMethods
    {
        public static string ToFileSize(this int source)
        {
            return ToFileSize(Convert.ToInt64(source));
        }

        public static string ToFileSize(this long source)
        {
            const int byteConversion = 1024;
            double bytes = Convert.ToDouble(source);

            if (bytes >= Math.Pow(byteConversion, 3)) //GB Range
            {
                return string.Concat(Math.Round(bytes / Math.Pow(byteConversion, 3), 2), " GB");
            }
            else if (bytes >= Math.Pow(byteConversion, 2)) //MB Range
            {
                return string.Concat(Math.Round(bytes / Math.Pow(byteConversion, 2), 2), " MB");
            }
            else if (bytes >= byteConversion) //KB Range
            {
                return string.Concat(Math.Round(bytes / byteConversion, 2), " KB");
            }
            else //Bytes
            {
                return string.Concat(bytes, " Bytes");
            }
        }

        public static long GigabytesToBytes(this long source)
        {
            var bytes = ((source * 1024 * 1024) * 1024);
            return bytes;
        }
        public static long MegabytesToBytes(this long source)
        {
            var bytes = ((source * 1024) * 1024);
            return bytes;
        }

    }
}
