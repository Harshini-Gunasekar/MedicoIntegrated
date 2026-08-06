using System;

namespace Booking.Helpers
{
    public static class DateTimeHelper
    {
        public static DateTime? ConvertUtcToIst(DateTime? utcDateTime)
        {
            if (!utcDateTime.HasValue) return null;
            
            // Always treat the incoming time value as UTC, regardless of its DateTimeKind
            var utc = DateTime.SpecifyKind(utcDateTime.Value, DateTimeKind.Utc);
            try
            {
                var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(utc, istZone);
            }
            catch (TimeZoneNotFoundException)
            {
                try
                {
                    var istZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
                    return TimeZoneInfo.ConvertTimeFromUtc(utc, istZone);
                }
                catch
                {
                    // Fallback to adding 5 hours and 30 minutes
                    return utc.AddHours(5).AddMinutes(30);
                }
            }
        }
    }
}
