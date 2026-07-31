using System;

namespace Booking.Helpers
{
    public static class DateTimeExtensions
    {
        private static readonly TimeZoneInfo IndianTimeZone = GetIndianTimeZone();

        private static TimeZoneInfo GetIndianTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            }
            catch
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
                }
                catch
                {
                    return TimeZoneInfo.CreateCustomTimeZone("IST", TimeSpan.FromHours(5.5), "India Standard Time", "India Standard Time");
                }
            }
        }

        public static DateTime ToIndianTime(this DateTime dt)
        {
            if (dt == DateTime.MinValue || dt == DateTime.MaxValue)
                return dt;

            if (dt.Kind == DateTimeKind.Utc)
            {
                return TimeZoneInfo.ConvertTimeFromUtc(dt, IndianTimeZone);
            }

            // Treat unspecified/local as UTC since API/DB returns UTC timestamps
            var utcDt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            return TimeZoneInfo.ConvertTimeFromUtc(utcDt, IndianTimeZone);
        }

        public static DateTime? ToIndianTime(this DateTime? dt)
        {
            if (!dt.HasValue) return null;
            return dt.Value.ToIndianTime();
        }

        public static TimeOnly ToIndianTime(this TimeOnly time)
        {
            return time.Add(TimeSpan.FromHours(5.5));
        }

        public static TimeOnly? ToIndianTime(this TimeOnly? time)
        {
            if (!time.HasValue) return null;
            return time.Value.ToIndianTime();
        }

        public static DateTime ToUtcFromIndianTime(this DateTime dt)
        {
            if (dt == DateTime.MinValue || dt == DateTime.MaxValue)
                return dt;

            if (dt.Kind == DateTimeKind.Utc)
            {
                return dt;
            }

            return TimeZoneInfo.ConvertTimeToUtc(dt, IndianTimeZone);
        }

        public static DateTime? ToUtcFromIndianTime(this DateTime? dt)
        {
            if (!dt.HasValue) return null;
            return dt.Value.ToUtcFromIndianTime();
        }

        public static TimeOnly ToUtcFromIndianTime(this TimeOnly time)
        {
            return time.Add(TimeSpan.FromHours(-5.5));
        }

        public static TimeOnly? ToUtcFromIndianTime(this TimeOnly? time)
        {
            if (!time.HasValue) return null;
            return time.Value.ToUtcFromIndianTime();
        }
    }
}
