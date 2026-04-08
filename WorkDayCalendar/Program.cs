using WC=WorkdayCalendar;
Console.WriteLine("Work day calendars");

// Create input request
var request = new WC.WorkdayRequest
{
    StartDate = new DateTime(2004, 5, 24, 19, 3, 0),
    IncrementInDays = 44.723656f,
    WorkStart = TimeSpan.FromHours(8),
    WorkEnd = TimeSpan.FromHours(16),

    Holidays = new List<DateTime>
    {
        new DateTime(2004, 5, 27),
    },

    RecurringHolidays = new List<(int, int)>
    {
        (5, 17)
    }
};

// Provide timezone
// For India (no DST)
 var timeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

// Example for DST regions:
 // var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");


// Initialize calendar with timezone
var calendar = new WC.WorkdayCalendar(request, timeZone);

// Calculate result (returns DateTimeOffset now)
var result = calendar.GetWorkdayIncrement(
    request.StartDate,
    request.IncrementInDays);


// Convert result to local time for display
var localResult = TimeZoneInfo.ConvertTime(result, timeZone);


//Format output
string output = $"{request.StartDate:dd-MM-yyyy HH:mm} " +
                $"with the addition of {request.IncrementInDays} working days is " +
                $"{localResult:dd-MM-yyyy HH:mm}";

Console.WriteLine(output);
