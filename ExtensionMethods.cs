using System;
using System.Globalization;

namespace Imgu
{
    public static class ExtensionMethods
    {
        public static string UppercaseFirst(this string month)
        {
            return char.ToUpper(month[0]) + month.Substring(1);
        }
    }
}
