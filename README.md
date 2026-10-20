# ExpenseApi — Monthly Date Boundaries

A lightweight, dedicated ASP.NET Core Minimal API implementation for querying expenses within monthly calendar date boundaries.

---

## 1. Prerequisites & Environment

- **SDK:** .NET 9 SDK (e.g. `9.0.319` or higher).
- **Runtime:** ASP.NET Core 9.0.
- **Note on SDK Version:** This project targets `net9.0` because .NET 9 was the installed SDK available in the development/assessment environment. The assignment did not specifically mandate .NET 9; rather, it was selected to match the locally installed tooling without downloading unapproved external packages.

---

## 2. Project Structure

```
ExpenseApi/
├── ExpenseApi.csproj                           # API project file (net9.0, OpenAPI support)
├── Program.cs                                  # Minimal API entrypoint, seed data, and boundary logic
├── ExpenseApi.http                             # Sample HTTP request file
├── appsettings.json                            # Production logging settings
├── appsettings.Development.json                # Development logging settings
├── Properties/
│   └── launchSettings.json                     # Launch profiles (HTTP 5194, HTTPS 7219)
├── .gitignore                                  # Standard .NET gitignore file (ignores bin/, obj/, etc.)
├── README.md                                   # Project documentation and usage guide
├── assessment-notes.md                         # Detailed assessment execution and decision log
└── ExpenseApi.Tests/                           # Automated integration test project
    ├── ExpenseApi.Tests.csproj                 # xUnit test project referencing ExpenseApi.csproj
    └── ExpenseMonthlyEndpointTests.cs          # 24 integration tests verifying boundaries and HTTP behavior
```

---

## 3. Build, Test, and Run Commands

Execute all commands from the repository root:

- **Restore dependencies:**
  ```bash
  dotnet restore ExpenseApi.csproj
  ```
- **Build the API project:**
  ```bash
  dotnet build ExpenseApi.csproj --no-restore
  ```
- **Run the test suite:**
  ```bash
  dotnet test ExpenseApi.Tests/ExpenseApi.Tests.csproj
  ```
- **Run the application:**
  ```bash
  dotnet run --project ExpenseApi.csproj --launch-profile http
  ```
  The API will start listening on `http://localhost:5194`.

---

## 4. API Endpoint Contract

### Route
```http
GET /expenses/monthly?year=YYYY&month=MM
```

### Query Parameters
- `year` (*required, integer*): 4-digit calendar year (`1 <= year <= 9999`).
- `month` (*required, integer*): Calendar month (`1 <= month <= 12`).

---

## 5. Request & Response Examples

### Successful Request (October 2026)
```http
GET /expenses/monthly?year=2026&month=10 HTTP/1.1
Host: localhost:5194
Accept: application/json
```

**Response (`200 OK`):**
```json
[
  {
    "id": 1,
    "description": "October First Instant",
    "amount": 10.00,
    "date": "2026-10-01T00:00:00Z"
  },
  {
    "id": 2,
    "description": "October Mid Month",
    "amount": 25.50,
    "date": "2026-10-15T14:30:00Z"
  },
  {
    "id": 3,
    "description": "October Final Day Whole Second",
    "amount": 30.00,
    "date": "2026-10-31T23:59:59Z"
  },
  {
    "id": 4,
    "description": "October Final Day Milliseconds",
    "amount": 40.00,
    "date": "2026-10-31T23:59:59.999Z"
  },
  {
    "id": 5,
    "description": "October Final Day Last Tick",
    "amount": 45.00,
    "date": "2026-10-31T23:59:59.9999999Z"
  }
]
```

### Empty Month Request (May 2025)
```http
GET /expenses/monthly?year=2025&month=5 HTTP/1.1
Host: localhost:5194
Accept: application/json
```

**Response (`200 OK`):**
```json
[]
```

### Invalid Requests (`400 Bad Request`)

1. **Missing Parameter:**
   `GET /expenses/monthly?year=2026`
   ```json
   {
     "error": "Both 'year' and 'month' query parameters are required."
   }
   ```

2. **Malformed Non-Numeric Parameter:**
   `GET /expenses/monthly?year=2026&month=october`
   ```json
   {
     "error": "'year' and 'month' must be valid integers."
   }
   ```

3. **Month Out of Range:**
   `GET /expenses/monthly?year=2026&month=13`
   ```json
   {
     "error": "'month' must be between 1 and 12."
   }
   ```

4. **Year Out of Range:**
   `GET /expenses/monthly?year=10000&month=10`
   ```json
   {
     "error": "'year' must be between 1 and 9999."
   }
   ```

---

## 6. Date Boundary Logic & Mathematical Correctness

### The Half-Open Interval: `[startInclusive, nextMonthStartExclusive)`
The filtering logic evaluates whether an expense timestamp falls within the requested calendar month using a half-open interval:
$$\text{startInclusive} \le \text{expense}.\text{Date} < \text{nextMonthStartExclusive}$$

Where:
- $\text{startInclusive} = \text{new DateTime}(year, month, 1, 0, 0, 0, \text{DateTimeKind.Utc})$
- $\text{nextMonthStartExclusive} = \text{startInclusive}.\text{AddMonths}(1)$

### Why This Avoids Boundary Defects
1. **No Sub-Second Truncation:** Approximating the month's end using `23:59:59` drops any transaction occurring during the final second with fractional values (such as `.500s` or `.9999999s`). Using `< nextMonthStartExclusive` captures every fraction of a second right up to the final 100-nanosecond tick (`23:59:59.9999999Z`).
2. **Strict Next-Month Exclusion:** Using an inclusive upper bound like `<= nextMonthStart` would incorrectly pull transactions from the first instant of the next month (`00:00:00.0000000Z`) into the current month. The strict inequality `<` prevents this defect.
3. **Calendar Rollovers and Leap Years:** `.AddMonths(1)` natively handles variable month lengths (28, 29, 30, and 31 days), leap years (e.g., February 29 in 2028), and year transitions (e.g., December 31 to January 1).

### UTC Timezone Convention
All dates and timestamps are interpreted and stored in UTC (`DateTimeKind.Utc`). Queries evaluate the calendar month against UTC instants. Clients recording local timestamps must convert to UTC or query the corresponding UTC boundary.

### Year 9999 Edge Case
In .NET, `DateTime.MaxValue` is `9999-12-31T23:59:59.9999999`. Calling `AddMonths(1)` on December 9999 throws an `ArgumentOutOfRangeException` because year 10000 cannot be represented in .NET `DateTime`. The endpoint explicitly handles this edge case: for `year == 9999 && month == 12`, the query filters all items satisfying `expense.Date >= startInclusive`, capturing every instant through `DateTime.MaxValue` without throwing an exception.

---

## 7. Known Limitations

- **In-Memory Volatility:** The endpoint operates on an in-memory collection initialized with deterministic seed records. Mutations (POST/PUT/DELETE) and database persistence are not implemented as they were outside the scope of this assessment.
- **Single Calendar Timezone:** Boundaries are evaluated strictly in UTC.

---

## 8. Test Coverage Summary

The companion integration test suite ([`ExpenseApi.Tests`](ExpenseApi.Tests/ExpenseMonthlyEndpointTests.cs)) uses `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory<Program>`) to dispatch real HTTP requests through the in-memory ASP.NET Core pipeline.

All 24 automated tests pass:
- **Instant Boundaries:** First instant of the month (`00:00:00Z`), final-day whole second (`23:59:59Z`), final-day milliseconds (`.999Z`), and exact final tick (`.9999999Z`).
- **Exclusion Boundaries:** First instant of next month (`00:00:00Z`) and final tick of previous month (`.9999999Z`) strictly excluded.
- **Multi-Month Isolation:** Queries isolate only the targeted month across multi-month datasets.
- **Calendar Edge Cases:** December-to-January year transition, leap-year February 29 (2028), and maximum year (December 9999).
- **Validation Cases:** Missing `year`, missing `month`, non-numeric parameters, out-of-range years (`< 1` and `> 9999`), and invalid months (`0`, `13`, `-1`).
- **HTTP Behavior:** Response shape, camelCase field names, `application/json` Content-Type, and preservation of the `/weatherforecast` template route.
