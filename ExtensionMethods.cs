using System;

namespace Imgu
{
    public static class ExtensionMethods
    {
        public static string UppercaseFirst(this string month)
        {
            return char.ToUpper(month[0]) + month.Substring(1);
        }

        public static string DenyMidnight(this string time)
        {
            if (time == "00:00:00")
            {
                time = "15:00:00";
            }
            return time;
        }
    }
}
