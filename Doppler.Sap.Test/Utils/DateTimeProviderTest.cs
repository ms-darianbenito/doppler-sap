using Doppler.Sap.Utils;
using System;
using Xunit;

namespace Doppler.Sap.Test.Utils
{
    public class DateTimeProviderTest
    {
        [Fact]
        public void DateTimeProvider_ShouldBeReturnDateInArgentinaTimezone_WhenTimeZoneIsNotNull()
        {
            var timeZoneConfigurations = new TimeZoneConfigurations
            {
                InvoicesTimeZone = TimeZoneHelper.GetTimeZoneByOperativeSystem("Argentina Standard Time")
            };
            var dateTimeProvider = new DateTimeProvider();
            var utcDate = dateTimeProvider.UtcNow;
            var cstZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneConfigurations.InvoicesTimeZone);

            var expectedDate = utcDate.AddMinutes(cstZone.BaseUtcOffset.TotalMinutes);
            var result = dateTimeProvider.GetDateByTimezoneId(utcDate, timeZoneConfigurations.InvoicesTimeZone);

            Assert.Equal(expectedDate, result);
        }

        [Fact]
        public void DateTimeProvider_ShouldBeReturnPreviousDateInArgentinaTimezone_WhenTimeZoneIsNotNullAndTheDateAfter0clock()
        {
            var timeZoneConfigurations = new TimeZoneConfigurations
            {
                InvoicesTimeZone = TimeZoneHelper.GetTimeZoneByOperativeSystem("Argentina Standard Time")
            };
            var dateTimeProvider = new DateTimeProvider();
            var utcDate = new DateTime(2020, 12, 13, 0, 10, 0);
            var expectedDate = new DateTime(2020, 12, 12, 0, 10, 0);

            var result = dateTimeProvider.GetDateByTimezoneId(utcDate, timeZoneConfigurations.InvoicesTimeZone);

            Assert.Equal(expectedDate.Date, result.Date);
        }

        [Fact]
        public void DateTimeProvider_ShouldBeReturnDate_WhenTimeZoneIsNull()
        {
            var timezoneId = "";
            var dateTimeProvider = new DateTimeProvider();
            var utcDate = dateTimeProvider.UtcNow;

            var result = dateTimeProvider.GetDateByTimezoneId(utcDate, timezoneId);

            Assert.Equal(utcDate, result);
        }

        [Fact]
        public void DateTimeProvider_PredefinedValues_ShouldBeReturnDateInArgentinaTimezone_WhenTimeZoneIsNotNull()
        {
            var timeZoneConfigurations = new TimeZoneConfigurations
            {
                InvoicesTimeZone = TimeZoneHelper.GetTimeZoneByOperativeSystem("Argentina Standard Time")
            };
            var dateTimeProvider = new DateTimeProvider();
            var utcDate = new DateTime(2020, 12, 14, 14, 0, 0, DateTimeKind.Utc);
            var expectedDate = new DateTime(2020, 12, 14, 11, 0, 0);

            var result = dateTimeProvider.GetDateByTimezoneId(utcDate, timeZoneConfigurations.InvoicesTimeZone);

            Assert.Equal(expectedDate, result);
        }

        [Fact]
        public void DateTimeProvider_PredefinedValues_ShouldBeReturnPreviousDateInArgentinaTimezone_WhenTimeZoneIsNotNullAndTheDateAfter0clockAndKingUtc()
        {
            var timeZoneConfigurations = new TimeZoneConfigurations
            {
                InvoicesTimeZone = TimeZoneHelper.GetTimeZoneByOperativeSystem("Argentina Standard Time")
            };
            var dateTimeProvider = new DateTimeProvider();
            var date = new DateTime(2020, 12, 13, 0, 10, 0, DateTimeKind.Utc);
            var expectedDate = new DateTime(2020, 12, 12, 21, 10, 0);

            var result = dateTimeProvider.GetDateByTimezoneId(date, timeZoneConfigurations.InvoicesTimeZone);

            Assert.Equal(expectedDate, result);
        }

        [Fact]
        public void DateTimeProvider_PredefinedValues_ShouldBeReturnPreviousDateInArgentinaTimezone_WhenTimeZoneIsNotNullAndTheDateAfter0clockAndKingUnspecified()
        {
            var timeZoneConfigurations = new TimeZoneConfigurations
            {
                InvoicesTimeZone = TimeZoneHelper.GetTimeZoneByOperativeSystem("Argentina Standard Time")
            };
            var dateTimeProvider = new DateTimeProvider();
            var date = new DateTime(2020, 12, 13, 0, 0, 0, DateTimeKind.Unspecified);
            var expectedDate = new DateTime(2020, 12, 12, 21, 0, 0);

            var result = dateTimeProvider.GetDateByTimezoneId(date, timeZoneConfigurations.InvoicesTimeZone);

            Assert.Equal(expectedDate, result);
        }

        [Fact]
        public void DateTimeProvider_PredefinedValues_ShouldBeReturnAnArgumentException_WhenTheKingLocal()
        {
            var timeZoneConfigurations = new TimeZoneConfigurations
            {
                InvoicesTimeZone = TimeZoneHelper.GetTimeZoneByOperativeSystem("Argentina Standard Time")
            };
            var dateTimeProvider = new DateTimeProvider();
            var date = new DateTime(2020, 12, 13, 0, 0, 0, DateTimeKind.Local);

            var ex = Assert.Throws<ArgumentException>(() => dateTimeProvider.GetDateByTimezoneId(date, timeZoneConfigurations.InvoicesTimeZone));
            var isExpectedMessage = ex.Message.Contains("The conversion could not be completed because the supplied DateTime did not have the Kind property set correctly");
            Assert.True(isExpectedMessage);
        }
    }
}
