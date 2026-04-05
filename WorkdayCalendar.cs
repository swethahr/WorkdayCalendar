public class WorkdayCalendar
{
    private readonly HashSet<DateTime> _holidays;
    private readonly HashSet<(int Month, int Day)> _recurringHolidays;

    private readonly TimeSpan _workStart;
    private readonly TimeSpan _workEnd;

    private readonly int _minutesPerDay;

    public WorkdayCalendar(WorkdayRequest request)
    {
        _holidays = request.Holidays.Select(d => d.Date).ToHashSet();
        _recurringHolidays = request.RecurringHolidays.ToHashSet();

        _workStart = request.WorkStart;
        _workEnd = request.WorkEnd;

        _minutesPerDay = (int)(_workEnd - _workStart).TotalMinutes;

        if (_minutesPerDay <= 0)
            throw new ArgumentException("Invalid working hours");
    }

    public DateTime Calculate()
    {
        return GetWorkdayIncrement(
            DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified), 0); // placeholder if needed
    }

    public DateTime GetWorkdayIncrement(DateTime startDate, float incrementInWorkdays)
    {
        int direction = incrementInWorkdays >= 0 ? 1 : -1;

        int totalMinutes = (int)Math.Round(Math.Abs(incrementInWorkdays) * _minutesPerDay);

        DateTime current = Normalize(startDate);

        while (totalMinutes > 0)
        {
            if (!IsWorkingDay(current))
            {
                current = MoveToNextWorkingDay(current, direction);
                continue;
            }

            DateTime boundary = direction > 0
                ? current.Date + _workEnd
                : current.Date + _workStart;

            int available = (int)Math.Abs((boundary - current).TotalMinutes);

            if (available >= totalMinutes)
                return current.AddMinutes(direction * totalMinutes);

            totalMinutes -= available;
            current = MoveToNextWorkingDay(current, direction);
        }

        return current;
    }

    private DateTime Normalize(DateTime date)
    {
        if (!IsWorkingDay(date))
            return MoveToNextWorkingDay(date, 1);

        if (date.TimeOfDay < _workStart)
            return date.Date + _workStart;

        if (date.TimeOfDay > _workEnd)
            return date.Date + _workEnd;

        return date;
    }

    private DateTime MoveToNextWorkingDay(DateTime date, int direction)
    {
        do
        {
            date = date.Date.AddDays(direction);
        } while (!IsWorkingDay(date));

        return date + (direction > 0 ? _workStart : _workEnd);
    }

    private bool IsWorkingDay(DateTime date)
    {
        return !(IsWeekend(date)
            || _holidays.Contains(date.Date)
            || _recurringHolidays.Contains((date.Month, date.Day)));
    }

    private bool IsWeekend(DateTime date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday ||
               date.DayOfWeek == DayOfWeek.Sunday;
    }
}