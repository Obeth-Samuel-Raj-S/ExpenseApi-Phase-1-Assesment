using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

var expenses = new List<Expense>
{
    new(1, "October First Instant", 10.00m, DateTime.Parse("2026-10-01T00:00:00.0000000Z", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)),
    new(2, "October Mid Month", 25.50m, DateTime.Parse("2026-10-15T14:30:00.0000000Z", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)),
    new(3, "October Final Day Whole Second", 30.00m, DateTime.Parse("2026-10-31T23:59:59.0000000Z", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)),
    new(4, "October Final Day Milliseconds", 40.00m, DateTime.Parse("2026-10-31T23:59:59.9990000Z", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)),
    new(5, "October Final Day Last Tick", 45.00m, DateTime.Parse("2026-10-31T23:59:59.9999999Z", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)),
    new(6, "November First Instant", 50.00m, DateTime.Parse("2026-11-01T00:00:00.0000000Z", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)),
    new(7, "September Final Instant", 60.00m, DateTime.Parse("2026-09-30T23:59:59.9990000Z", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)),
    new(8, "September Final Tick", 65.00m, DateTime.Parse("2026-09-30T23:59:59.9999999Z", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)),
    new(9, "December Final Milliseconds", 70.00m, DateTime.Parse("2026-12-31T23:59:59.9990000Z", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)),
    new(10, "December Final Tick", 75.00m, DateTime.Parse("2026-12-31T23:59:59.9999999Z", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)),
    new(11, "New Year First Instant", 80.00m, DateTime.Parse("2027-01-01T00:00:00.0000000Z", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)),
    new(12, "November Mid Month", 90.00m, DateTime.Parse("2026-11-15T12:00:00.0000000Z", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)),
    // Leap Year 2028: February 29 boundary test cases
    new(13, "Leap Year Feb 29 Last Tick", 100.00m, DateTime.Parse("2028-02-29T23:59:59.9999999Z", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)),
    new(14, "Leap Year March 1 First Instant", 110.00m, DateTime.Parse("2028-03-01T00:00:00.0000000Z", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)),
    // Year 9999: Maximum representable DateTime boundary test cases
    new(15, "Year 9999 November Final Tick", 120.00m, DateTime.Parse("9999-11-30T23:59:59.9999999Z", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)),
    new(16, "Year 9999 December First Instant", 130.00m, DateTime.Parse("9999-12-01T00:00:00.0000000Z", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)),
    new(17, "Year 9999 December Final Tick", 140.00m, DateTime.Parse("9999-12-31T23:59:59.9999999Z", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)),
};

app.MapGet("/expenses/monthly", (string? year, string? month) =>
{
    // 1. Required parameter presence validation
    if (string.IsNullOrWhiteSpace(year) || string.IsNullOrWhiteSpace(month))
    {
        return Results.BadRequest(new { error = "Both 'year' and 'month' query parameters are required." });
    }

    // 2. Integer format validation
    if (!int.TryParse(year, NumberStyles.Integer, CultureInfo.InvariantCulture, out var yearVal) ||
        !int.TryParse(month, NumberStyles.Integer, CultureInfo.InvariantCulture, out var monthVal))
    {
        return Results.BadRequest(new { error = "'year' and 'month' must be valid integers." });
    }

    // 3. Calendar range validation
    // System.DateTime supports years 1 through 9999 (DateTime.MinValue.Year to DateTime.MaxValue.Year).
    if (yearVal < 1 || yearVal > 9999)
    {
        return Results.BadRequest(new { error = "'year' must be between 1 and 9999." });
    }

    if (monthVal < 1 || monthVal > 12)
    {
        return Results.BadRequest(new { error = "'month' must be between 1 and 12." });
    }

    // 4. Calculate calendar month half-open interval [startInclusive, nextMonthStartExclusive) in UTC.
    var startInclusive = new DateTime(yearVal, monthVal, 1, 0, 0, 0, DateTimeKind.Utc);

    // Special case note: In .NET, System.DateTime.MaxValue is 9999-12-31T23:59:59.9999999.
    // Calling startInclusive.AddMonths(1) on December 9999 throws ArgumentOutOfRangeException because
    // year 10000 cannot be represented in DateTime. For December 9999, the month spans all dates from
    // 9999-12-01T00:00:00Z up to DateTime.MaxValue (i.e., e.Date >= startInclusive), safely avoiding the overflow.
    var matching = (yearVal == 9999 && monthVal == 12)
        ? expenses.Where(e => e.Date >= startInclusive).ToList()
        : expenses.Where(e => e.Date >= startInclusive && e.Date < startInclusive.AddMonths(1)).ToList();

    return Results.Ok(matching);
})
.WithName("GetMonthlyExpenses");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public record Expense(int Id, string Description, decimal Amount, DateTime Date);

public partial class Program { }
