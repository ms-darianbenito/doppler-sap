using System;

namespace Doppler.Sap.Utils
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }

        DateTime GetDateByTimezoneId(DateTime date, string timezoneId);
    }
}
