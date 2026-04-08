using WorkdayCalendar;
using Xunit;

namespace WorkdayCalendar.Tests
{
    public class WorkdayCalendarTests
    {
        private WorkdayCalendar CreateCalendar()
        {
            var request = new WorkdayRequest
            {
                WorkStart = TimeSpan.FromHours(8),
                WorkEnd = TimeSpan.FromHours(16),

                Holidays = new List<DateTime>
                {
                    new DateTime(2004, 5, 27)
                },

                RecurringHolidays = new List<(int, int)>
                {
                    (5, 17)
                }
            };

            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

            return new WorkdayCalendar(request, timeZone);
        }

        // ✅ 1. Basic working day addition
        [Fact]
        public void Should_Add_One_Working_Day()
        {
            var cal = CreateCalendar();

            var start = new DateTime(2024, 4, 1, 10, 0, 0); // Monday
            var result = cal.GetWorkdayIncrement(start, 1);

            Assert.Equal(new DateTime(2024, 4, 2, 10, 0, 0), result.DateTime);
        }

        // ✅ 2. Skip weekend
        [Fact]
        public void Should_Skip_Weekend()
        {
            var cal = CreateCalendar();

            var start = new DateTime(2024, 4, 5, 10, 0, 0); // Friday
            var result = cal.GetWorkdayIncrement(start, 1);

            Assert.Equal(DayOfWeek.Monday, result.DayOfWeek);
        }

        // ✅ 3. Skip one-time holiday
        [Fact]
        public void Should_Skip_Holiday()
        {
            var cal = CreateCalendar();

            var start = new DateTime(2004, 5, 26, 10, 0, 0);
            var result = cal.GetWorkdayIncrement(start, 1);

            Assert.NotEqual(new DateTime(2004, 5, 27), result.Date);
        }

        // ✅ 4. Skip recurring holiday
        [Fact]
        public void Should_Skip_Recurring_Holiday()
        {
            var cal = CreateCalendar();

            var start = new DateTime(2024, 5, 16, 10, 0, 0);
            var result = cal.GetWorkdayIncrement(start, 1);

            Assert.NotEqual(17, result.Day);
        }

        // ✅ 5. Fractional day (0.5 = 4 hours)
        [Fact]
        public void Should_Handle_Fractional_Day()
        {
            var cal = CreateCalendar();

            var start = new DateTime(2024, 4, 1, 8, 0, 0);
            var result = cal.GetWorkdayIncrement(start, 0.5f);

            Assert.Equal(12, result.Hour);
        }

        // ✅ 6. Start time after working hours
        [Fact]
        public void Should_Normalize_After_Working_Hours()
        {
            var cal = CreateCalendar();

            var start = new DateTime(2024, 4, 1, 19, 0, 0);
            var result = cal.GetWorkdayIncrement(start, 0);

            Assert.Equal(16, result.Hour);
        }

        // ✅ 7. Start time before working hours
        [Fact]
        public void Should_Normalize_Before_Working_Hours()
        {
            var cal = CreateCalendar();

            var start = new DateTime(2024, 4, 1, 6, 0, 0);
            var result = cal.GetWorkdayIncrement(start, 0);

            Assert.Equal(8, result.Hour);
        }

        // ✅ 8. Negative increment (go back)
        [Fact]
        public void Should_Handle_Negative_Increment()
        {
            var cal = CreateCalendar();

            var start = new DateTime(2024, 4, 3, 10, 0, 0);
            var result = cal.GetWorkdayIncrement(start, -1);

            Assert.Equal(new DateTime(2024, 4, 2, 10, 0, 0), result.DateTime);
        }

        // ✅ 9. Large increment across multiple days
        [Fact]
        public void Should_Handle_Large_Increment()
        {
            var cal = CreateCalendar();

            var start = new DateTime(2024, 4, 1, 10, 0, 0);
            var result = cal.GetWorkdayIncrement(start, 5);

            Assert.Equal(new DateTime(2024, 4, 8, 10, 0, 0), result.DateTime);
        }

        // ✅ 10. Fraction pushes to next day
        [Fact]
        public void Should_Move_To_Next_Day_When_Overflow()
        {
            var cal = CreateCalendar();

            var start = new DateTime(2024, 4, 1, 15, 0, 0);
            var result = cal.GetWorkdayIncrement(start, 0.5f);

            // 15:00 + 4 hours → next day 11:00
            Assert.Equal(11, result.Hour);
            Assert.Equal(DayOfWeek.Tuesday, result.DayOfWeek);
        }

        // ✅ 11. Exact boundary condition
        [Fact]
        public void Should_Stop_At_Workday_End()
        {
            var cal = CreateCalendar();

            var start = new DateTime(2024, 4, 1, 12, 0, 0);
            var result = cal.GetWorkdayIncrement(start, 0.5f);

            Assert.Equal(16, result.Hour);
        }

        // ✅ 12. Multiple fractional scenarios
        [Theory]
        [InlineData(0.25f, 10)] // 2 hours
        [InlineData(0.75f, 14)] // 6 hours
        public void Should_Handle_Multiple_Fractions(float input, int expectedHour)
        {
            var cal = CreateCalendar();

            var start = new DateTime(2024, 4, 1, 8, 0, 0);
            var result = cal.GetWorkdayIncrement(start, input);

            Assert.Equal(expectedHour, result.Hour);
        }
    }
}