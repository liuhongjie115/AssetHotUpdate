using System;

namespace Assets.Scripts.Core.Utils
{
    public class DateUtils
    {
        public static long GetNowSeconds()
        {
            var time = (DateTime.UtcNow.ToUniversalTime().Ticks - 621355968000000000) / 10000000 - 28800;
            return time;
        }

        public static string GetNowSecondsStr()
        {
            return GetNowSeconds().ToString();
        }

        public static string GetNowYMDFormat1()
        {
            return DateTime.Now.ToString("yyyy-MM-dd");
        }

        public static string GetNowYMDFormatALogFileName()
        {
            return DateTime.Now.ToString("yyyy_MM_dd");
        }

        public static string GetNowYMDFormatALog()
        {
            return DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss");
        }

        public static string GetNowYMDHMSFormat()
        {
            return DateTime.Now.ToString("yyyy_MM_dd-hh_mm_ss");
        }
    }
}