using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ExpenseApi.Tests;

public record ExpenseResponse(int Id, string Description, decimal Amount, DateTime Date);

public class ExpenseMonthlyEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ExpenseMonthlyEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetExpensesMonthly_FirstInstantOfMonth_IsIncluded()
    {
        // Act
        var response = await _client.GetAsync("/expenses/monthly?year=2026&month=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var expenses = await response.Content.ReadFromJsonAsync<List<ExpenseResponse>>();
        Assert.NotNull(expenses);
        Assert.Contains(expenses, e => e.Id == 1 && e.Date == DateTime.Parse("2026-10-01T00:00:00.0000000Z").ToUniversalTime());
    }

    [Fact]
    public async Task GetExpensesMonthly_OrdinaryExpenseInsideMonth_IsIncluded()
    {
        // Act
        var response = await _client.GetAsync("/expenses/monthly?year=2026&month=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var expenses = await response.Content.ReadFromJsonAsync<List<ExpenseResponse>>();
        Assert.NotNull(expenses);
        Assert.Contains(expenses, e => e.Id == 2 && e.Date == DateTime.Parse("2026-10-15T14:30:00.0000000Z").ToUniversalTime());
    }

    [Fact]
    public async Task GetExpensesMonthly_FinalDayLateNightExpense_IsIncluded()
    {
        // Act
        var response = await _client.GetAsync("/expenses/monthly?year=2026&month=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var expenses = await response.Content.ReadFromJsonAsync<List<ExpenseResponse>>();
        Assert.NotNull(expenses);
        Assert.Contains(expenses, e => e.Id == 3 && e.Date == DateTime.Parse("2026-10-31T23:59:59.0000000Z").ToUniversalTime());
    }

    [Fact]
    public async Task GetExpensesMonthly_FinalDayFractionalSecondTimestamp_IsIncluded()
    {
        // Act
        var response = await _client.GetAsync("/expenses/monthly?year=2026&month=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var expenses = await response.Content.ReadFromJsonAsync<List<ExpenseResponse>>();
        Assert.NotNull(expenses);
        Assert.Contains(expenses, e => e.Id == 4 && e.Date == DateTime.Parse("2026-10-31T23:59:59.9990000Z").ToUniversalTime());
    }

    [Fact]
    public async Task GetExpensesMonthly_FinalDayLastTickBeforeNextMonth_IsIncluded()
    {
        // Act: Verifies the exact final 100-nanosecond tick of October 2026 (2026-10-31T23:59:59.9999999Z)
        var response = await _client.GetAsync("/expenses/monthly?year=2026&month=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var expenses = await response.Content.ReadFromJsonAsync<List<ExpenseResponse>>();
        Assert.NotNull(expenses);
        var item = Assert.Single(expenses, e => e.Id == 5);
        Assert.Equal("October Final Day Last Tick", item.Description);
        Assert.Equal(45.00m, item.Amount);
        Assert.Equal(DateTime.Parse("2026-10-31T23:59:59.9999999Z").ToUniversalTime(), item.Date);
    }

    [Fact]
    public async Task GetExpensesMonthly_FirstInstantOfNextMonth_IsStrictlyExcluded()
    {
        // Act
        var response = await _client.GetAsync("/expenses/monthly?year=2026&month=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var expenses = await response.Content.ReadFromJsonAsync<List<ExpenseResponse>>();
        Assert.NotNull(expenses);
        Assert.DoesNotContain(expenses, e => e.Date == DateTime.Parse("2026-11-01T00:00:00.0000000Z").ToUniversalTime());
    }

    [Fact]
    public async Task GetExpensesMonthly_ExpenseImmediatelyBeforeRequestedMonth_IsStrictlyExcluded()
    {
        // Act
        var response = await _client.GetAsync("/expenses/monthly?year=2026&month=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var expenses = await response.Content.ReadFromJsonAsync<List<ExpenseResponse>>();
        Assert.NotNull(expenses);
        Assert.DoesNotContain(expenses, e => e.Date == DateTime.Parse("2026-09-30T23:59:59.9990000Z").ToUniversalTime());
        Assert.DoesNotContain(expenses, e => e.Date == DateTime.Parse("2026-09-30T23:59:59.9999999Z").ToUniversalTime());
    }

    [Fact]
    public async Task GetExpensesMonthly_MultipleAdjacentMonths_OnlyRequestedMonthExpensesReturned()
    {
        // Act
        var response = await _client.GetAsync("/expenses/monthly?year=2026&month=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var expenses = await response.Content.ReadFromJsonAsync<List<ExpenseResponse>>();
        Assert.NotNull(expenses);
        Assert.NotEmpty(expenses);
        Assert.All(expenses, e =>
        {
            Assert.Equal(2026, e.Date.Year);
            Assert.Equal(10, e.Date.Month);
        });
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    [InlineData(-1)]
    public async Task GetExpensesMonthly_InvalidMonth_ReturnsBadRequest(int invalidMonth)
    {
        // Act
        var response = await _client.GetAsync($"/expenses/monthly?year=2026&month={invalidMonth}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("/expenses/monthly?year=2026")] // missing month
    [InlineData("/expenses/monthly?month=10")]  // missing year
    [InlineData("/expenses/monthly")]           // missing both
    public async Task GetExpensesMonthly_MissingParameters_ReturnsBadRequest(string url)
    {
        // Act
        var response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("/expenses/monthly?year=2026&month=october")]
    [InlineData("/expenses/monthly?year=invalid&month=10")]
    public async Task GetExpensesMonthly_MalformedParameters_ReturnsBadRequest(string url)
    {
        // Act
        var response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10000)]
    [InlineData(-2026)]
    public async Task GetExpensesMonthly_OutOfRangeYear_ReturnsBadRequest(int invalidYear)
    {
        // Act
        var response = await _client.GetAsync($"/expenses/monthly?year={invalidYear}&month=10");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetExpensesMonthly_EmptyMonth_ReturnsOkWithEmptyArray()
    {
        // Act (May 2025 has no expenses seeded)
        var response = await _client.GetAsync("/expenses/monthly?year=2025&month=5");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var expenses = await response.Content.ReadFromJsonAsync<List<ExpenseResponse>>();
        Assert.NotNull(expenses);
        Assert.Empty(expenses);
    }

    [Fact]
    public async Task GetExpensesMonthly_DecemberToJanuaryBoundary_CorrectlyRollovers()
    {
        // Act
        var response = await _client.GetAsync("/expenses/monthly?year=2026&month=12");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var expenses = await response.Content.ReadFromJsonAsync<List<ExpenseResponse>>();
        Assert.NotNull(expenses);
        Assert.Contains(expenses, e => e.Date == DateTime.Parse("2026-12-31T23:59:59.9990000Z").ToUniversalTime());
        Assert.Contains(expenses, e => e.Date == DateTime.Parse("2026-12-31T23:59:59.9999999Z").ToUniversalTime());
        Assert.DoesNotContain(expenses, e => e.Date == DateTime.Parse("2027-01-01T00:00:00.0000000Z").ToUniversalTime());
    }

    [Fact]
    public async Task GetExpensesMonthly_LeapYearFebruary2028_IncludesFeb29AndExcludesMarch1()
    {
        // Act: Verifies leap-year February 29 handling
        var response = await _client.GetAsync("/expenses/monthly?year=2028&month=2");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var expenses = await response.Content.ReadFromJsonAsync<List<ExpenseResponse>>();
        Assert.NotNull(expenses);
        var feb29Item = Assert.Single(expenses, e => e.Id == 13);
        Assert.Equal(100.00m, feb29Item.Amount);
        Assert.Equal(DateTime.Parse("2028-02-29T23:59:59.9999999Z").ToUniversalTime(), feb29Item.Date);
        Assert.DoesNotContain(expenses, e => e.Id == 14); // March 1 excluded
    }

    [Fact]
    public async Task GetExpensesMonthly_Year9999December_IncludesAllDecDatesAndExcludesNovember()
    {
        // Act: Verifies maximum representable DateTime boundary (DateTime.MaxValue)
        var response = await _client.GetAsync("/expenses/monthly?year=9999&month=12");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var expenses = await response.Content.ReadFromJsonAsync<List<ExpenseResponse>>();
        Assert.NotNull(expenses);
        Assert.Contains(expenses, e => e.Id == 16 && e.Date == DateTime.Parse("9999-12-01T00:00:00.0000000Z").ToUniversalTime());
        var maxTickItem = Assert.Single(expenses, e => e.Id == 17);
        Assert.Equal(140.00m, maxTickItem.Amount);
        Assert.Equal(DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc), maxTickItem.Date);
        Assert.DoesNotContain(expenses, e => e.Id == 15); // November 9999 excluded
    }

    [Fact]
    public async Task GetExpensesMonthly_ReturnsCorrectJsonContentTypeAndShape()
    {
        // Act
        var response = await _client.GetAsync("/expenses/monthly?year=2026&month=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content.Headers.ContentType);
        Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);

        var rawJson = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(rawJson);
        Assert.Equal(JsonValueKind.Array, document.RootElement.ValueKind);

        if (document.RootElement.GetArrayLength() > 0)
        {
            var firstItem = document.RootElement[0];
            Assert.True(firstItem.TryGetProperty("id", out _));
            Assert.True(firstItem.TryGetProperty("description", out _));
            Assert.True(firstItem.TryGetProperty("amount", out _));
            Assert.True(firstItem.TryGetProperty("date", out _));
        }
    }
}
