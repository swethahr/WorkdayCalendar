Console.WriteLine("Work day calendars");
var request = new WorkdayRequest
{
    StartDate = new DateTime(2004, 5, 24, 18, 5, 0),
    IncrementInDays = -6.7470217f,
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

var calendar = new WorkdayCalendar(request);

var result = calendar.GetWorkdayIncrement(
    request.StartDate,
    request.IncrementInDays);

string output = $"{request.StartDate:dd-MM-yyyy HH:mm} " +
                $"with the addition of {request.IncrementInDays} working days is " +
                $"{result:dd-MM-yyyy HH:mm}";

Console.WriteLine(output);