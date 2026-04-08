namespace WorkdayCalendar
    {
    // Request model to hold all input data for workday calculation
    public class WorkdayRequest
    {
        // Start date and time for calculation
        public DateTime StartDate { get; set; }

        // Number of working days to add or subtract (can be fractional)
        public float IncrementInDays { get; set; }

        // Workday start time (e.g., 08:00 AM)
        public TimeSpan WorkStart { get; set; }

        // Workday end time (e.g., 04:00 PM)
        public TimeSpan WorkEnd { get; set; }

        // List of one-time holidays (specific dates)
        public List<DateTime> Holidays { get; set; } = new();

        // List of recurring holidays (Month, Day format, repeats every year)
        public List<(int Month, int Day)> RecurringHolidays { get; set; } = new();
    }
}