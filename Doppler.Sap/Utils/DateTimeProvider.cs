using System;
using System.Runtime.InteropServices;

namespace Doppler.Sap.Utils
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;

        public DateTime GetDateByTimezoneId(DateTime date, string timezoneId)
        {
            if (!string.IsNullOrEmpty(timezoneId))
            {
                var cstZone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
                return TimeZoneInfo.ConvertTimeFromUtc(date, cstZone);
            }

            return date;
        }
    }
}
