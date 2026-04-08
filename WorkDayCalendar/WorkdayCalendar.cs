namespace WorkdayCalendar
{
    public class WorkdayCalendar
    {
        // One-time holidays (specific dates)
        private readonly HashSet<DateTime> _holidays;

        // Recurring holidays (month + day)
        private readonly HashSet<(int Month, int Day)> _recurringHolidays;

        // Working hours
        private readonly TimeSpan _workStart;
        private readonly TimeSpan _workEnd;

        // Total minutes in a workday
        private readonly int _minutesPerDay;

        // Timezone (important for DST handling)
        private readonly TimeZoneInfo _timeZone;

        // Initializes calendar with working hours and holiday configuration.
        /// Converts input lists into HashSets for faster lookup.
        public WorkdayCalendar(WorkdayRequest request, TimeZoneInfo timeZone)
        {
            _holidays = request.Holidays.Select(d => d.Date).ToHashSet();
            _recurringHolidays = request.RecurringHolidays.ToHashSet();

            _workStart = request.WorkStart;
            _workEnd = request.WorkEnd;

            _minutesPerDay = (int)(_workEnd - _workStart).TotalMinutes;

            if (_minutesPerDay <= 0)
                throw new ArgumentException("Invalid working hours");

            _timeZone = timeZone;
        }

        // Adds or subtracts working days (including fractional) from a given start date.
        // Handles weekends, holidays, and working hour boundaries.
        public DateTimeOffset GetWorkdayIncrement(DateTime startDate, float incrementInWorkdays)
        {
            // Convert start date into timezone-aware value
            var current = ToZonedTime(startDate);

            int direction = incrementInWorkdays >= 0 ? 1 : -1;

            int totalMinutes = (int)Math.Round(Math.Abs(incrementInWorkdays) * _minutesPerDay);

            current = Normalize(current, direction);

            while (totalMinutes > 0)
            {
                if (!IsWorkingDay(current))
                {
                    current = MoveToNextWorkingDay(current, direction);
                    continue;
                }

                // Get boundary (start or end of working day)
                var boundary = direction > 0
                    ? SetTime(current, _workEnd)
                    : SetTime(current, _workStart);

                // Calculate minutes available considering DST
                int available = (int)Math.Abs((boundary - current).TotalMinutes);

                if (available >= totalMinutes)
                    return AddMinutesSafe(current, direction * totalMinutes);

                totalMinutes -= available;
                current = MoveToNextWorkingDay(current, direction);
            }

            return current;
        }

        // Convert DateTime → DateTimeOffset with timezone
        private DateTimeOffset ToZonedTime(DateTime date)
        {
            // Treat input as "local time in target timezone"
            var unspecified = DateTime.SpecifyKind(date, DateTimeKind.Unspecified);

            return new DateTimeOffset(unspecified, _timeZone.GetUtcOffset(unspecified));
        }

        // Safely add minutes with DST awareness
        private DateTimeOffset AddMinutesSafe(DateTimeOffset date, int minutes)
        {
            var utc = date.UtcDateTime.AddMinutes(minutes);
            return TimeZoneInfo.ConvertTimeFromUtc(utc, _timeZone);
        }

        // Normalize time into working hours
        private DateTimeOffset Normalize(DateTimeOffset date, int direction)
        {
            if (!IsWorkingDay(date))
                return MoveToNextWorkingDay(date, direction);

            if (date.TimeOfDay < _workStart)
                return SetTime(date, _workStart);

            if (date.TimeOfDay > _workEnd)
                return SetTime(date, _workEnd);

            return date;
        }

        // Move to next/previous working day
        private DateTimeOffset MoveToNextWorkingDay(DateTimeOffset date, int direction)
        {
            do
            {
                // Move ONLY calendar date (no 24h shift)
                var nextDate = date.Date.AddDays(direction);

                // Rebuild time in target timezone
                date = CreateZoned(nextDate);

            } while (!IsWorkingDay(date));

            return SetTime(date, direction > 0 ? _workStart : _workEnd);
        }

        // Creates a timezone-aware DateTimeOffset WITHOUT shifting time
        private DateTimeOffset CreateZoned(DateTime date)
        {
            // Treat input as local time in the given timezone (VERY IMPORTANT)
            var unspecified = DateTime.SpecifyKind(date, DateTimeKind.Unspecified);

            // Attach correct offset for that timezone
            return new DateTimeOffset(unspecified, _timeZone.GetUtcOffset(unspecified));
        }

        // Add days safely with DST
        private DateTimeOffset AddDaysSafe(DateTimeOffset date, int days)
        {
            // Move date only (ignore time shift)
            var newDate = date.Date.AddDays(days);

            // Keep same time-of-day
            var combined = newDate + date.TimeOfDay;

            return ToZonedTime(combined);
        }

        // Set time (preserving date, adjusting with DST)
        private DateTimeOffset SetTime(DateTimeOffset date, TimeSpan time)
        {
            var localDate = date.Date + time;
            return ToZonedTime(localDate);
        }

        // checks if a given date is a valid working day.
        private bool IsWorkingDay(DateTimeOffset date)
        {
            return !(IsWeekend(date)
                || _holidays.Contains(date.Date)
                || _recurringHolidays.Contains((date.Month, date.Day)));
        }

    // Checks if the date falls on a weekend (Saturday or Sunday).
        private bool IsWeekend(DateTimeOffset date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday ||
                date.DayOfWeek == DayOfWeek.Sunday;
        }
    }
}