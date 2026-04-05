public class WorkdayRequest
{
    public DateTime StartDate { get; set; }
    public float IncrementInDays { get; set; }

    public TimeSpan WorkStart { get; set; }
    public TimeSpan WorkEnd { get; set; }

    public List<DateTime> Holidays { get; set; } = new();
    public List<(int Month, int Day)> RecurringHolidays { get; set; } = new();
}