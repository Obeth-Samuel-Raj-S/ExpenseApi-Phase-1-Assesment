# Assessment Setup Notes

## 1. Context & Starter Creation Rationale
- **Challenge Assigned:** .NET / ASP.NET Core: Expense API Monthly Date Boundaries
- **Rationale for Local Creation:** No facilitator-provided starter repository was supplied. As specified in the assessment guidelines, rather than cloning a monolithic external documentation repository or pretending a starter repository was provided, a clean, minimal ASP.NET Core Web API project was scaffolded locally using the .NET CLI (`dotnet new webapi`).
- **Endpoint Status:** Scaffolding and baseline verification only. The `GET /expenses/monthly` endpoint has intentionally not been implemented at this stage.

---

## 2. Environment & Tooling Selection

- **Operating System:** Linux (Fedora 44, `linux-x64`)
- **Git Version:** `git version 2.55.0`
- **Installed .NET SDK:** .NET SDK `9.0.319` (Host runtime `9.0.21`, base path `/var/home/akame/.dotnet/sdk/9.0.319/`)
- **SDK Binary Location:** `/var/home/akame/.dotnet/dotnet` *(Note: requires `export PATH="$HOME/.dotnet:$PATH"` in subshell sessions where `~/.dotnet` is not yet on `$PATH`)*
- **Target Framework:** `net9.0`
  - *LTS Consideration:* While .NET 8 is the currently supported LTS release, only the .NET 9 SDK (9.0.319) was installed in the local environment. A dry-run test targeting `net8.0` (`dotnet new webapi -f net8.0 --dry-run`) verified that the installed template exclusively supports `net9.0`. Per instructions not to install new SDKs or upgrade dependencies without prior approval, `net9.0` was selected.
- **Template Style:** Minimal API (ASP.NET Core top-level statements with `WebApplication.CreateBuilder` and endpoint mapping, OpenAPI support enabled via `Microsoft.AspNetCore.OpenApi`).

---

## 3. Scaffolding & Git Setup

### Commands Executed:
1. **Directory Check:**
   ```bash
   pwd && ls -la
   # Output: /var/home/akame/Projects (non-empty, existing unrelated project folders present)
   ```
2. **Git Parent Check:**
   ```bash
   git rev-parse --is-inside-work-tree
   # Exit code: 128 (Not inside any git repository)
   ```
3. **Template Verification:**
   ```bash
   dotnet new webapi -o ExpenseApi --dry-run
   # Exit code: 0
   ```
4. **Project Generation:**
   ```bash
   export PATH="$HOME/.dotnet:$PATH" && dotnet new webapi -o ExpenseApi
   # Exit code: 0 (Project generated and NuGet packages restored successfully)
   ```
5. **Git Repository Initialization:**
   ```bash
   cd /var/home/akame/Projects/ExpenseApi && git init
   # Exit code: 0 (Initialized empty Git repository; no commits made)
   ```

---

## 4. Generated Project Structure

```
ExpenseApi/
├── appsettings.Development.json    # Development logging configuration
├── appsettings.json                # Production logging configuration
├── ExpenseApi.csproj               # Project definition (net9.0, OpenApi 9.0.21)
├── ExpenseApi.http                 # HTTP sample request file
├── Program.cs                      # Minimal API entrypoint and weatherforecast sample endpoint
├── Properties/
│   └── launchSettings.json         # Development launch profiles (HTTP port 5194, HTTPS port 7219)
└── .git/                           # Initialized local Git repository
```

---

## 5. Baseline Verification & Checks

| Check Category | Command Executed | Exit Code | Outcome | Status |
| :--- | :--- | :--- | :--- | :--- |
| **Dependency Restore** | `dotnet restore` | 0 | All packages up to date | **Successful** |
| **Clean Build** | `dotnet build` | 0 | 0 Errors, 0 Warnings (`ExpenseApi.dll` generated) | **Successful** |
| **Automated Tests** | `dotnet test` | 0 | No test projects found in the solution | **Unavailable** |
| **Runtime Execution** | `dotnet run --launch-profile http` | N/A (Daemon) | Kestrel started listening on `http://localhost:5194` | **Successful** |
| **Default Endpoint Test** | `curl -s -i http://localhost:5194/weatherforecast` | 0 | HTTP 200 OK, valid JSON array of forecast objects returned | **Successful** |
| **OpenAPI Specification** | `curl -s -i http://localhost:5194/openapi/v1.json` | 0 | HTTP 200 OK, valid OpenAPI 3.0.4 document returned | **Successful** |
| **HTTPS Verification** | `curl https://localhost:7219/...` | N/A | Skipped to avoid self-signed dev certificate issues | **Not run** |

### Verification Evidence:
- **`GET /weatherforecast` response:**
  ```json
  [
    {"date":"2026-10-21","temperatureC":44,"summary":"Chilly","temperatureF":111},
    {"date":"2026-10-22","temperatureC":-12,"summary":"Chilly","temperatureF":11},
    {"date":"2026-10-23","temperatureC":51,"summary":"Chilly","temperatureF":123},
    {"date":"2026-10-24","temperatureC":39,"summary":"Chilly","temperatureF":102},
    {"date":"2026-10-25","temperatureC":-3,"summary":"Warm","temperatureF":27}
  ]
  ```
- **Process Cleanup:** The application daemon was cleanly terminated after endpoint verification; port `5194` confirmed released.

---

## 6. Deviations from Assignment Document
1. **Starter Repository:** The assignment documents assume a facilitator-supplied starter repository. In its absence, a clean Minimal API project was scaffolded locally adhering strictly to standard ASP.NET Core conventions.
2. **Framework Target:** Rather than targeting .NET 8 (LTS), .NET 9 (`net9.0`) was utilized because .NET SDK `9.0.319` is the sole SDK installed in the operating environment, and downloading additional SDKs was prohibited without explicit approval.
3. **No Expense Endpoint:** `GET /expenses/monthly` is omitted from this setup phase per instructions to preserve the untouched baseline before implementation.

---

## 7. Remaining Limitations & Constraints
- **Test Suite:** No test framework (e.g. xUnit, NUnit) or test project is currently scaffolded. A dedicated test project should be added in subsequent steps if automated test validation is required.
- **Environment PATH:** The `dotnet` CLI is installed under `/var/home/akame/.dotnet/dotnet` and is not present in default non-login PATH. Commands require `export PATH="$HOME/.dotnet:$PATH"`.
- **Domain Logic:** Data models and endpoint handlers for expenses, category filters, and monthly date boundary calculations remain to be implemented in Step 2.

---

# Stage 2: Specification, Baseline Evidence, and Regression Test Design

## 1. Stage Objective & Prompt
- **Stage Goal:** Establish the evidence-first specification, API contract, and deterministic regression test design for `GET /expenses/monthly?year=YYYY&month=MM` prior to any code modification or test scaffolding.
- **Prompt Reference:** AI BUILDERS STEP 1 — STAGE 2: Specification, Baseline Evidence and Regression Test Design (Challenge 4, page 10).

## 2. Files Inspected
- `/var/home/akame/Projects/ExpenseApi/assessment-notes.md`: Reviewed previous setup notes, tooling decisions, and baseline verification results.
- `/var/home/akame/Projects/ExpenseApi/ExpenseApi.csproj`: Confirmed SDK style project targeting `net9.0` with `Microsoft.AspNetCore.OpenApi` (version 9.0.21).
- `/var/home/akame/Projects/ExpenseApi/Program.cs`: Confirmed untouched Minimal API setup; contains only `GET /weatherforecast`. No expense models or endpoints exist.
- `/var/home/akame/Projects/ExpenseApi/ExpenseApi.http`: Verified existing sample HTTP request targeting `/weatherforecast`.
- `/var/home/akame/Projects/ExpenseApi/appsettings.json` & `appsettings.Development.json`: Verified baseline logging settings.
- `/var/home/akame/Projects/ExpenseApi/Properties/launchSettings.json`: Verified HTTP (`5194`) and HTTPS (`7219`) profiles.

## 3. Commands Executed & Outcomes
1. `git status` in `/var/home/akame/Projects/ExpenseApi`:
   - Outcome: Exit code `0`. Confirmed master branch, no commits made, untracked starter files preserved cleanly.
2. `git diff` in `/var/home/akame/Projects/ExpenseApi`:
   - Outcome: Exit code `0`. Empty diff.
3. `export PATH="$HOME/.dotnet:$PATH" && cd /var/home/akame/Projects/ExpenseApi && dotnet build`:
   - Outcome: Exit code `0`. Build succeeded (0 Warnings, 0 Errors).
4. `export PATH="$HOME/.dotnet:$PATH" && dotnet new --list test`:
   - Outcome: Exit code `0`. Confirmed available test templates in installed SDK: `xunit`, `nunit`, `mstest`.
5. `export PATH="$HOME/.dotnet:$PATH" && dotnet new xunit -o ExpenseApi.Tests --dry-run`:
   - Outcome: Exit code `0`. Verified non-destructive dry-run for proposed xUnit test project.

## 4. Confirmed Facts vs. Assumptions
- **Confirmed Facts:**
  - Project directory is `/var/home/akame/Projects/ExpenseApi`.
  - Target framework is `net9.0`.
  - Original Minimal API starter builds cleanly with 0 warnings and 0 errors.
  - No expense endpoint, data model, or test project currently exists.
  - The repository has an initialized Git workspace without commits or remote configuration.
- **Assumptions & Architectural Decisions:**
  - **Timezone Contract:** Neither the template nor existing code defines a timezone contract. Consistent with standard ASP.NET Core REST API design, all timestamps are assumed and stored in UTC (`DateTimeKind.Utc` / ISO-8601 `Z`).
  - **Year Boundary:** Query parameter `year` must be a valid calendar year in the range `1..9999` to align with .NET's `DateTime` constructor guarantees.
  - **No Unrelated Features:** No database, external service, category filter, pagination, or authentication will be introduced.

## 5. Proposed API Endpoint Contract
- **Endpoint:** `GET /expenses/monthly?year=YYYY&month=MM`
- **Query Parameters:**
  - `year`: Required integer (`1 <= year <= 9999`).
  - `month`: Required integer (`1 <= month <= 12`).
- **Date Boundary Calculation:**
  - Lower Bound (inclusive): `startOfMonth = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc)`
  - Upper Bound (exclusive): `startOfNextMonth = startOfMonth.AddMonths(1)`
  - Filtering Condition: `expense.Date >= startOfMonth && expense.Date < startOfNextMonth`
  - Half-open interval `[startOfMonth, startOfNextMonth)` guarantees inclusion of late-night and fractional-second transactions on the final day, while strictly excluding the next month's start. Handles leap years and December-to-January transitions accurately via `AddMonths(1)`.
- **Response Schemas:**
  - **HTTP 200 OK:**
    - Content-Type: `application/json; charset=utf-8`
    - Body: Array of expense objects:
      ```json
      [
        {
          "id": 1,
          "description": "Office Supplies",
          "amount": 75.50,
          "date": "2026-10-15T14:30:00Z"
        }
      ]
      ```
    - Empty Result: If no expenses match the requested month, returns HTTP 200 with an empty array: `[]`.
  - **HTTP 400 Bad Request:**
    - Returned when `year` or `month` parameter is missing, non-numeric, or outside valid ranges (`year < 1 || year > 9999`, `month < 1 || month > 12`).
    - Standard ASP.NET Core `ProblemDetails` or error payload.
- **In-Memory Representation:**
  ```csharp
  public record Expense(int Id, string Description, decimal Amount, DateTime Date);
  ```

## 6. Deterministic Test Matrix Design

All tests will use a fixed in-memory seed dataset with deterministic UTC timestamps.

| Test # | Scenario | Input (Query & Seed Timestamp) | Expected Status | Expected Result | Defect Detected |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **1** | First instant of requested month | `year=2026&month=10`<br>Seed: `2026-10-01T00:00:00.0000000Z` | 200 OK | Expense included | Off-by-one lower bound error (e.g. using `>` instead of `>=`). |
| **2** | Ordinary expense inside month | `year=2026&month=10`<br>Seed: `2026-10-15T14:30:00.0000000Z` | 200 OK | Expense included | General filtering breakdown or inverted month/year mapping. |
| **3** | Final-day late-night expense | `year=2026&month=10`<br>Seed: `2026-10-31T23:59:59.0000000Z` | 200 OK | Expense included | Premature end-of-month truncation (e.g. truncating to `00:00:00` of last day). |
| **4** | Final-day fractional seconds | `year=2026&month=10`<br>Seed: `2026-10-31T23:59:59.9999999Z` | 200 OK | Expense included | Upper bound set to exact whole second `<= 23:59:59`, dropping sub-second records. |
| **5** | First instant of next month | `year=2026&month=10`<br>Seed: `2026-11-01T00:00:00.0000000Z` | 200 OK | Expense excluded | Inclusive upper bound error (`<= startOfNextMonth`). |
| **6** | Expense immediately before month | `year=2026&month=10`<br>Seed: `2026-09-30T23:59:59.9999999Z` | 200 OK | Expense excluded | Inclusive lower bound error on preceding month. |
| **7** | Multi-month isolation | `year=2026&month=10`<br>Seed: Sep, Oct (multiple), Nov, Dec | 200 OK | Exactly October expenses returned | Failure to isolate records across multi-month collections. |
| **8a** | Invalid month (0) | `year=2026&month=0` | 400 Bad Request | Validation error | Missing range check causing unhandled runtime exception / 500. |
| **8b** | Invalid month (13) | `year=2026&month=13` | 400 Bad Request | Validation error | Missing upper range check causing unhandled runtime exception / 500. |
| **9a** | Missing month query parameter | `year=2026` | 400 Bad Request | Validation error | Missing parameter validation. |
| **9b** | Missing year query parameter | `month=10` | 400 Bad Request | Validation error | Missing parameter validation. |
| **9c** | Non-numeric month parameter | `year=2026&month=october` | 400 Bad Request | Validation error | Parameter binding exception unhandled. |
| **9d** | Non-numeric year parameter | `year=yyyy&month=10` | 400 Bad Request | Validation error | Parameter binding exception unhandled. |
| **9e** | Year out of range | `year=0&month=10` or `year=10000&month=10` | 400 Bad Request | Validation error | Date constructor `ArgumentOutOfRangeException`. |
| **10** | Empty month | `year=2025&month=5` (no seeded records) | 200 OK | `[]` (empty JSON array) | Null reference or 404 returned instead of empty list. |
| **11** | Dec-to-Jan year rollover | `year=2026&month=12`<br>Seed A: `2026-12-31T23:59:59Z`<br>Seed B: `2027-01-01T00:00:00Z` | 200 OK | Seed A included, Seed B excluded | Year rollover calculation defect (e.g. failing to increment year for month 12). |
| **12** | Response schema & content-type | `year=2026&month=10` | 200 OK | `application/json`, camelCase fields (`id`, `description`, `amount`, `date`) | Serialization or property naming discrepancy. |

## 7. Proposed Test Architecture & Next Commands
- **Framework Recommendation:** `xunit` using `Microsoft.AspNetCore.Mvc.Testing` (version `9.0.21`).
- **Rationale:** `WebApplicationFactory<Program>` executes real HTTP requests against the in-memory TestServer. This tests routing, parameter binding, endpoint filters, and JSON serialization end-to-end, rather than merely testing an isolated LINQ expression.
- **Commands Proposed for Approval:**
  1. `export PATH="$HOME/.dotnet:$PATH" && dotnet new xunit -o ExpenseApi.Tests`
  2. `export PATH="$HOME/.dotnet:$PATH" && dotnet add ExpenseApi.Tests/ExpenseApi.Tests.csproj reference ExpenseApi/ExpenseApi.csproj`
  3. `export PATH="$HOME/.dotnet:$PATH" && dotnet add ExpenseApi.Tests/ExpenseApi.Tests.csproj package Microsoft.AspNetCore.Mvc.Testing --version 9.0.21`
- **Status:** Test project has NOT been scaffolded, and no application files have been modified. Awaiting approval before proceeding.

---

# Stage 3: Test Infrastructure and Red-Phase Regression Tests

## 1. Stage Objective & Prompt
- **Stage Goal:** Build out the test infrastructure and implement genuine red-phase integration tests inside the single assessment repository before implementing `GET /expenses/monthly`.
- **Prompt Reference:** AI BUILDERS STEP 1 — STAGE 3: Test Infrastructure and Red-Phase Regression Tests.

## 2. Repository Layout & Structure
- **Repository Root:** `/var/home/akame/Projects/ExpenseApi` (single Git repository; no nested/duplicate repos).
- **Project Structure:**
  ```
  ExpenseApi/
  ├── ExpenseApi.csproj                           # API project (net9.0, Microsoft.AspNetCore.OpenApi 9.0.21)
  ├── Program.cs                                  # Application entry point with public partial class Program
  ├── ExpenseApi.http                             # Sample HTTP request file
  ├── appsettings.json                            # Configuration file
  ├── appsettings.Development.json                # Development configuration
  ├── Properties/
  │   └── launchSettings.json                     # Launch profiles
  ├── ExpenseApi.Tests/                           # Test project subfolder
  │   ├── ExpenseApi.Tests.csproj                 # xUnit test project referencing ExpenseApi.csproj
  │   └── ExpenseMonthlyEndpointTests.cs          # Integration test suite (21 test executions)
  ├── assessment-notes.md                         # Assessment log
  └── .git/                                       # Single Git repository
  ```

## 3. Commands Executed & Outcomes
1. `export PATH="$HOME/.dotnet:$PATH" && cd /var/home/akame/Projects/ExpenseApi && dotnet new xunit -o ExpenseApi.Tests`
   - Outcome: Exit code `0`. Successfully created xUnit project in `ExpenseApi.Tests/`.
2. `export PATH="$HOME/.dotnet:$PATH" && cd /var/home/akame/Projects/ExpenseApi && dotnet add ExpenseApi.Tests/ExpenseApi.Tests.csproj reference ExpenseApi.csproj`
   - Outcome: Exit code `0`. Added project reference from `ExpenseApi.Tests` to `ExpenseApi`.
3. Initial attempt to add `Microsoft.AspNetCore.Mvc.Testing` without version constraint:
   - Command: `dotnet add ExpenseApi.Tests/ExpenseApi.Tests.csproj package Microsoft.AspNetCore.Mvc.Testing`
   - Outcome: Exit code `1`. Error `NU1202`: NuGet resolved version `10.0.13` (targeting `net10.0`), which is incompatible with `net9.0`.
   - Resolution: Explicitly pinned to version `9.0.21` to match installed .NET 9 SDK runtime.
4. `export PATH="$HOME/.dotnet:$PATH" && cd /var/home/akame/Projects/ExpenseApi && dotnet add ExpenseApi.Tests/ExpenseApi.Tests.csproj package Microsoft.AspNetCore.Mvc.Testing --version 9.0.21`
   - Outcome: Exit code `0`. Package restored successfully.
5. Compilation resolution in `ExpenseApi.csproj`:
   - Adding a subproject inside the parent project directory led MSBuild to include `ExpenseApi.Tests/**/*.cs` and obj files in `ExpenseApi.csproj` compile globs.
   - Resolution: Added `<DefaultItemExcludes>$(DefaultItemExcludes);ExpenseApi.Tests/**</DefaultItemExcludes>` to `ExpenseApi.csproj`.
6. Entry point exposure in `Program.cs`:
   - Added `public partial class Program { }` to allow `WebApplicationFactory<Program>` to reference `Program`.
7. `export PATH="$HOME/.dotnet:$PATH" && cd /var/home/akame/Projects/ExpenseApi/ExpenseApi.Tests && dotnet build`
   - Outcome: Exit code `0`. Build succeeded with 0 warnings and 0 errors.
8. `export PATH="$HOME/.dotnet:$PATH" && cd /var/home/akame/Projects/ExpenseApi/ExpenseApi.Tests && dotnet test`
   - Outcome: Exit code `1` (Failed: 21, Passed: 0, Total: 21).

## 4. Tests Added & Coverage
Implemented in `ExpenseApi.Tests/ExpenseMonthlyEndpointTests.cs`:
1. `GetExpensesMonthly_FirstInstantOfMonth_IsIncluded`: First instant of month (`2026-10-01T00:00:00.0000000Z`) returned with 200 OK.
2. `GetExpensesMonthly_OrdinaryExpenseInsideMonth_IsIncluded`: Mid-month expense (`2026-10-15T14:30:00.0000000Z`) returned with 200 OK.
3. `GetExpensesMonthly_FinalDayLateNightExpense_IsIncluded`: Last day late expense (`2026-10-31T23:59:59.0000000Z`) returned with 200 OK.
4. `GetExpensesMonthly_FinalDayFractionalSecondTimestamp_IsIncluded`: Last day sub-second expense (`2026-10-31T23:59:59.9990000Z`) returned with 200 OK.
5. `GetExpensesMonthly_FirstInstantOfNextMonth_IsStrictlyExcluded`: First instant of following month (`2026-11-01T00:00:00.0000000Z`) excluded.
6. `GetExpensesMonthly_ExpenseImmediatelyBeforeRequestedMonth_IsStrictlyExcluded`: Last instant of prior month (`2026-09-30T23:59:59.9990000Z`) excluded.
7. `GetExpensesMonthly_MultipleAdjacentMonths_OnlyRequestedMonthExpensesReturned`: Multi-month isolation verifies only October 2026 items returned.
8. `GetExpensesMonthly_InvalidMonth_ReturnsBadRequest`: Tests `month=0`, `month=13`, `month=-1` return 400 Bad Request (3 test cases).
9. `GetExpensesMonthly_MissingParameters_ReturnsBadRequest`: Missing `month`, missing `year`, and missing both return 400 Bad Request (3 test cases).
10. `GetExpensesMonthly_MalformedParameters_ReturnsBadRequest`: `month=october` and `year=invalid` return 400 Bad Request (2 test cases).
11. `GetExpensesMonthly_OutOfRangeYear_ReturnsBadRequest`: `year=0`, `year=10000`, `year=-2026` return 400 Bad Request (3 test cases).
12. `GetExpensesMonthly_EmptyMonth_ReturnsOkWithEmptyArray`: May 2025 returns 200 OK with `[]`.
13. `GetExpensesMonthly_DecemberToJanuaryBoundary_CorrectlyRollovers`: December 2026 returns Dec 31 items and excludes Jan 1 items.
14. `GetExpensesMonthly_ReturnsCorrectJsonContentTypeAndShape`: Asserts `application/json` Content-Type and camelCase schema (`id`, `description`, `amount`, `date`).

## 5. Initial Test Results & Failure Analysis (Genuine Red Phase)
- **Execution Summary:**
  - Total tests executed: 21
  - Passed: 0
  - Failed: 21
- **Failure Cause:**
  - Every single test dispatched a real HTTP request to `/expenses/monthly?...`.
  - For tests expecting `200 OK`, Kestrel returned `404 Not Found` (`Assert.Equal() Failure: Expected: OK, Actual: NotFound`).
  - For tests expecting `400 BadRequest`, Kestrel returned `404 Not Found` (`Assert.Equal() Failure: Expected: BadRequest, Actual: NotFound`).
- **Confirmation:**
  - Test harness, in-memory server, and HTTP dispatch mechanisms are 100% operational.
  - Failures are exclusively caused by the missing endpoint mapping for `/expenses/monthly`. No code has been implemented yet.

## 6. Files Changed or Added
- `ExpenseApi.csproj`: Added `<DefaultItemExcludes>$(DefaultItemExcludes);ExpenseApi.Tests/**</DefaultItemExcludes>`.
- `Program.cs`: Added `public partial class Program { }` at EOF.
- `ExpenseApi.Tests/ExpenseApi.Tests.csproj`: Scaffolded and configured test project.
- `ExpenseApi.Tests/ExpenseMonthlyEndpointTests.cs`: Created integration test suite.
- `assessment-notes.md`: Appended Stage 3 notes.

---

# Stage 4: Implement Expense Monthly Boundaries — Green Phase

## 1. Stage Objective & Implementation Prompt
- **Stage Goal:** Implement the `GET /expenses/monthly?year=YYYY&month=MM` endpoint using in-memory deterministic data and strict half-open date boundaries, review and strengthen boundary tests to include the exact final 100-nanosecond tick (`2026-10-31T23:59:59.9999999Z`), and transition all regression tests from Red to Green.
- **Prompt Reference:** AI BUILDERS STEP 1 — STAGE 4: Implement Expense Monthly Boundaries — Green Phase.

## 2. Implementation Decisions & Date-Boundary Logic
- **Data Model:**
  ```csharp
  public record Expense(int Id, string Description, decimal Amount, DateTime Date);
  ```
  Uses standard C# record with `decimal` for monetary values and UTC `DateTime` (`DateTimeKind.Utc`).
- **Date Boundary Formula:**
  - Lower Bound (inclusive):
    `startInclusive = new DateTime(yearVal, monthVal, 1, 0, 0, 0, DateTimeKind.Utc)`
  - Upper Bound (exclusive):
    `nextMonthStartExclusive = startInclusive.AddMonths(1)`
  - Selection Filter:
    `expense.Date >= startInclusive && expense.Date < nextMonthStartExclusive`
- **Defensive Edge Case (December 9999):**
  - Discovered that `DateTime(9999, 12, 1).AddMonths(1)` throws `ArgumentOutOfRangeException` because .NET cannot represent year 10000.
  - Handled explicitly: for `yearVal == 9999 && monthVal == 12`, the query filters with `e.Date >= startInclusive` up to `DateTime.MaxValue`.
- **Query Parameter Validation:**
  - Missing parameters: Checks `string.IsNullOrWhiteSpace` for both `year` and `month`, returning 400 Bad Request with an informative error payload.
  - Malformed parameters: Uses `int.TryParse(..., NumberStyles.Integer, CultureInfo.InvariantCulture, ...)` returning 400 Bad Request if parsing fails.
  - Range validation: Checks `yearVal < 1 || yearVal > 9999` and `monthVal < 1 || monthVal > 12`, returning 400 Bad Request.
  - Prevents any uncaught runtime exceptions or HTTP 500 status codes.
- **Template Compatibility:**
  - Preserved existing `/weatherforecast` Minimal API route and launch configuration.

## 3. Test Suite Updates
- Updated `ExpenseApi.Tests/ExpenseMonthlyEndpointTests.cs`:
  - Added test `GetExpensesMonthly_FinalDayLastTickBeforeNextMonth_IsIncluded` asserting inclusion of `2026-10-31T23:59:59.9999999Z` (the exact final 100-nanosecond tick before November begins).
  - Maintained `2026-10-31T23:59:59.9990000Z` (millisecond level) and `2026-10-31T23:59:59.0000000Z` (whole second level).
  - Maintained strict exclusion checks for previous-month tick (`2026-09-30T23:59:59.9999999Z`) and next-month first instant (`2026-11-01T00:00:00.0000000Z`).
  - Total test executions increased from 21 to 22.

## 4. Commands Executed & Outcomes
1. **Compilation Check:**
   - `export PATH="$HOME/.dotnet:$PATH" && cd /var/home/akame/Projects/ExpenseApi && dotnet build ExpenseApi.csproj && dotnet build ExpenseApi.Tests/ExpenseApi.Tests.csproj`
   - Outcome: Exit code `0` (0 Errors, 0 Warnings for both projects).
2. **Automated Integration Test Run (Green Phase):**
   - `export PATH="$HOME/.dotnet:$PATH" && cd /var/home/akame/Projects/ExpenseApi && dotnet test ExpenseApi.Tests/ExpenseApi.Tests.csproj`
   - Outcome: Exit code `0`.
   - Results: **Passed: 22, Failed: 0, Skipped: 0, Total: 22**.
3. **Runtime Verification:**
   - Started application with `dotnet run --launch-profile http` (listening on `http://localhost:5194`).
   - `curl -s -i "http://localhost:5194/expenses/monthly?year=2026&month=10"`: HTTP 200 OK, returned all 5 October expenses including ticks `.0000000Z`, `.999Z`, and `.9999999Z`.
   - `curl -s -i "http://localhost:5194/expenses/monthly?year=2025&month=5"`: HTTP 200 OK, returned `[]` (empty JSON array).
   - `curl -s -i "http://localhost:5194/expenses/monthly?year=2026&month=13"`: HTTP 400 Bad Request (`'month' must be between 1 and 12.`).
   - `curl -s -i "http://localhost:5194/expenses/monthly?year=2026&month=0"`: HTTP 400 Bad Request (`'month' must be between 1 and 12.`).
   - `curl -s -i "http://localhost:5194/expenses/monthly?year=2026"`: HTTP 400 Bad Request (`Both 'year' and 'month' query parameters are required.`).
   - `curl -s -i "http://localhost:5194/expenses/monthly?year=2026&month=october"`: HTTP 400 Bad Request (`'year' and 'month' must be valid integers.`).
   - `curl -s -i "http://localhost:5194/expenses/monthly?year=10000&month=10"`: HTTP 400 Bad Request (`'year' must be between 1 and 9999.`).
   - `curl -s -i "http://localhost:5194/weatherforecast"`: HTTP 200 OK, returned 5 weather forecast items.
   - Cleanly stopped background server process; confirmed port `5194` released.

## 5. Diff & Workspace Review
- Confirmed with `git status` that all work is kept strictly within `/var/home/akame/Projects/ExpenseApi`.
- No commits made, no remote pushes or deployments.
- No third-party packages or databases introduced; only standard .NET 9 Minimal API and xUnit integration test infrastructure.

## 6. Remaining Limitations
- **Data Persistence:** In-memory seed collection is read-only and static for the assessment. Persisting or mutating expenses via POST/PUT is out of scope for this task.
- **Timezone Assumption:** The endpoint evaluates boundaries in UTC. Clients supplying local time offsets without converting to UTC would query the UTC calendar boundary.

---

# Stage 5: Independent Code Review, Adversarial Testing, and Evidence Validation

## 1. Stage Objective & Review Scope
- **Stage Goal:** Conduct an adversarial code review of date boundary implementations, execute independent build and test passes, audit edge cases (such as leap years and maximum representable dates), challenge test suite completeness, and record an authentic AI-redirection example.
- **Prompt Reference:** AI BUILDERS STEP 1 — STAGE 5: Independent Code Review, Adversarial Testing and Evidence Validation.
- **Files & Code Paths Inspected:**
  - `ExpenseApi/Program.cs`: Full review of Minimal API route, query parsing, validation logic, and date interval comparison.
  - `ExpenseApi/ExpenseApi.csproj`: Inspected project dependencies and build properties.
  - `ExpenseApi.Tests/ExpenseApi.Tests.csproj`: Inspected test framework dependencies.
  - `ExpenseApi.Tests/ExpenseMonthlyEndpointTests.cs`: Comprehensive inspection of all test assertions, fixtures, and HTTP calls.
  - `ExpenseApi/assessment-notes.md`: Audited complete working log across Stages 1–4.

## 2. Commands Executed & Outcomes
1. **Repository Status Check:**
   - Command: `cd /var/home/akame/Projects/ExpenseApi && git status`
   - Outcome: Exit code `0` (branch master, no commits, untracked working files intact).
2. **Initial Verification Run:**
   - Command: `export PATH="$HOME/.dotnet:$PATH" && cd /var/home/akame/Projects/ExpenseApi && dotnet build ExpenseApi.csproj && dotnet build ExpenseApi.Tests/ExpenseApi.Tests.csproj && dotnet test ExpenseApi.Tests/ExpenseApi.Tests.csproj`
   - Outcome: Exit code `0` (Builds: 0 warnings, 0 errors; Tests: Passed: 22, Failed: 0, Total: 22).
3. **Post-Enhancement Test Run (Catching DateTime.MaxValue Timezone Bug):**
   - Command: `dotnet test ExpenseApi.Tests/ExpenseApi.Tests.csproj`
   - Outcome: Exit code `1` (Passed: 23, Failed: 1). Caught timezone bug where `DateTime.MaxValue.ToUniversalTime()` shifted from UTC to local time.
4. **Final Regression Test Run:**
   - Command: `dotnet test ExpenseApi.Tests/ExpenseApi.Tests.csproj`
   - Outcome: Exit code `0`.
   - Results: **Passed: 24, Failed: 0, Skipped: 0, Total: 24**.
5. **Runtime Server Verification:**
   - Started application with `dotnet run --launch-profile http`.
   - `curl -s -i "http://localhost:5194/expenses/monthly?year=2028&month=2"`: HTTP 200 OK (returned leap-year Feb 29 expense ID 13).
   - `curl -s -i "http://localhost:5194/expenses/monthly?year=9999&month=12"`: HTTP 200 OK (returned Dec 9999 items ID 16 and 17).
   - `curl -s -i "http://localhost:5194/expenses/monthly?year=9999&month=11"`: HTTP 200 OK (returned Nov 9999 item ID 15).
   - Cleanly terminated runtime server; port 5194 released.

## 3. Boundary Cases Verified
- **A. Lower Bound ($t = \text{start}$):** `2026-10-01T00:00:00.0000000Z` included (200 OK).
- **B. Upper Bound Final Tick ($t = \text{nextMonthStart} - 1\text{ tick}$):** `2026-10-31T23:59:59.9999999Z` included with exact ID, amount, and timestamp assertion.
- **C. Next Month First Instant ($t = \text{nextMonthStart}$):** `2026-11-01T00:00:00.0000000Z` strictly excluded.
- **D. Previous Month Last Instant:** `2026-09-30T23:59:59.9999999Z` strictly excluded.
- **E. December-to-January Rollover:** Dec 2026 includes `2026-12-31T23:59:59.9999999Z` and excludes `2027-01-01T00:00:00.0000000Z`.
- **F. Leap-Year February (2028):** Feb 2028 includes Feb 29 final tick (`2028-02-29T23:59:59.9999999Z`) and excludes March 1 first instant (`2028-03-01T00:00:00.0000000Z`).
- **G. Maximum Year Edge Case (December 9999):** Dec 9999 includes `9999-12-01T00:00:00Z` and `9999-12-31T23:59:59.9999999Z` (DateTime.MaxValue in UTC), and excludes Nov 9999 (`9999-11-30T23:59:59.9999999Z`).
- **H. Input Validation:** Missing, non-numeric, and out-of-range parameters return HTTP 400 Bad Request with informative JSON error messages.
- **I. Empty Month:** Returns HTTP 200 with `[]`.
- **J. Template Preservation:** `/weatherforecast` route verified functional.

## 4. Coverage Gaps Found & Resolved
1. **Coverage Gap 1: Leap-Year Boundary (Feb 29, 2028):**
   - *Issue:* No test or seed record verified February 29 leap-day handling.
   - *Fix:* Added deterministic seed records for Feb 29 tick and March 1 instant in `Program.cs`, and added `GetExpensesMonthly_LeapYearFebruary2028_IncludesFeb29AndExcludesMarch1` in `ExpenseMonthlyEndpointTests.cs`.
2. **Coverage Gap 2: Year 9999 DateTime.MaxValue Boundary:**
   - *Issue:* The code contained special handling for `year == 9999 && month == 12` to prevent `AddMonths(1)` overflow, but lacked an automated integration test and seed records.
   - *Fix:* Added seed records for Nov 9999 and Dec 9999, and added `GetExpensesMonthly_Year9999December_IncludesAllDecDatesAndExcludesNovember` test.
3. **Coverage Gap 3: Strengthened Assertions:**
   - *Issue:* Boundary tests previously checked `Assert.Contains(expenses, e => e.Date == ...)` without verifying ID or description.
   - *Fix:* Updated tests to assert specific IDs (`item.Id`), descriptions, and amounts.

## 5. Genuine AI-Redirection Example
- **Initial Test Implementation / Gap:**
  When drafting the Year 9999 December boundary test, the test asserted:
  `Assert.Equal(DateTime.MaxValue.ToUniversalTime(), maxTickItem.Date);`
- **Evidence of Failure:**
  The test failed with:
  ```text
  Assert.Equal() Failure: Values differ
  Expected: 9999-12-31T18:29:59.9999999Z
  Actual:   9999-12-31T23:59:59.9999999Z
  ```
- **Root Cause & Corrective Redirection:**
  In .NET, `DateTime.MaxValue` has `DateTimeKind.Unspecified`. Calling `.ToUniversalTime()` on an `Unspecified` DateTime assumes the timestamp is local time, and shifts it according to the local timezone (in this environment, IST / UTC+05:30), resulting in an erroneous offset subtraction of 5 hours and 30 minutes.
- **Resulting Code Correction:**
  Explicitly designated UTC kind using `DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc)`.
- **Verification:**
  Re-running `dotnet test` passed cleanly (24 of 24 tests passed).

## 6. Remaining Risks & Limitations
- **Read-Only Scope:** In-memory repository is static; write/mutation endpoints are outside the assessment scope.
- **Client Timezone Awareness:** The API contract evaluates calendar boundaries in UTC. Clients querying for localized monthly expenses must convert or specify boundaries accordingly.

---

# Final Submission Preparation & Verification

## 1. Final Verification Commands & Exact Outcomes

The following commands were freshly executed from the repository root (`/var/home/akame/Projects/ExpenseApi`):

1. **Environment Check:**
   - Command: `export PATH="$HOME/.dotnet:$PATH" && dotnet --info`
   - Exit Code: `0`
   - Output: .NET SDK `9.0.319`, Runtime `Microsoft.AspNetCore.App 9.0.21` / `Microsoft.NETCore.App 9.0.21`, OS `fedora 44 (linux-x64)`.
2. **Dependency Restore:**
   - Command: `dotnet restore ExpenseApi.csproj`
   - Exit Code: `0` (All projects up-to-date for restore).
3. **API Project Build:**
   - Command: `dotnet build ExpenseApi.csproj --no-restore`
   - Exit Code: `0` (`0 Warning(s)`, `0 Error(s)`).
4. **Integration Test Suite Run:**
   - Command: `dotnet test ExpenseApi.Tests/ExpenseApi.Tests.csproj`
   - Exit Code: `0`
   - Summary: **Total: 24, Passed: 24, Failed: 0, Skipped: 0** (Duration: 418 ms).
5. **Test Project Explicit Build:**
   - Command: `dotnet build ExpenseApi.Tests/ExpenseApi.Tests.csproj`
   - Exit Code: `0` (`0 Warning(s)`, `0 Error(s)`).

## 2. Comprehensive Assessment Progression Summary

| Stage | Focus | Baseline State | Outcome |
| :--- | :--- | :--- | :--- |
| **Stage 1** | Local Environment & Scaffolding | Missing starter repository; inspected SDK 9.0.319 | Scaffolded `net9.0` Minimal API starter; initialized local Git repository (0 commits); verified untouched template (`/weatherforecast`). |
| **Stage 2** | Specification & Test Matrix Design | Proposed API contract & boundary formula | Documented contract `GET /expenses/monthly?year=YYYY&month=MM`, half-open interval $[start, nextMonthStart)$, and 12-scenario test matrix. |
| **Stage 3** | Test Infrastructure & Red Phase | No test suite existed | Created `ExpenseApi.Tests` using xUnit and `Microsoft.AspNetCore.Mvc.Testing` 9.0.21; verified **genuine red phase: 21 executed, 0 passed, 21 failed (404 NotFound)**. |
| **Stage 4** | Implementation & Green Phase | 21 failing tests | Implemented endpoint, input validation, deterministic seed data, and strengthened final-tick boundary; verified **green phase: 22 passed, 0 failed**. |
| **Stage 5** | Adversarial Review & Edge Cases | 22 passing tests | Audited leap year (Feb 29, 2028) and max year (Dec 9999); caught and resolved `DateTime.MaxValue` timezone shift; expanded suite to **24 passed, 0 failed**. |
| **Final** | Hygiene, Documentation & Packaging | Working code and test suite | Generated standard `.gitignore`, authored comprehensive `README.md`, verified clean builds and tests, and packaged `ExpenseApi-Submission.zip`. |

## 3. Confirmed Architecture & Verified Facts vs. Limitations

- **Confirmed Facts:**
  - Standard Minimal API implementation without external database or unnecessary dependencies.
  - Pure mathematical half-open interval $[startInclusive, nextMonthStartExclusive)$ prevents sub-second truncation and eliminates off-by-one errors.
  - Strict input validation prevents unhandled exceptions or HTTP 500 errors.
  - All 24 integration tests exercise real HTTP requests via `WebApplicationFactory<Program>` and assert specific entity IDs, amounts, and timestamps.
- **Assumptions & Documented Limitations:**
  - **In-Memory Volatility:** Data is held in an in-memory collection and is read-only.
  - **UTC Calendar Boundary:** Queries evaluate month boundaries in UTC. Clients must convert local times to UTC when comparing against boundaries.





