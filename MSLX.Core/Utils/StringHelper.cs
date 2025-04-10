using System;

namespace MSLX.Core.Utils
{
    public class StringHelper
    {
        /// <summary>
        /// 将秒级时间戳转换为DateTime类型（基于UTC+8时区）
        /// </summary>
        public static DateTime SecondsToDateTime(long seconds)
        {
            // 使用Unix时间戳起点1970-01-01 UTC
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime utcTime = origin.AddSeconds(seconds);
            TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, cstZone);
        }
    }
}