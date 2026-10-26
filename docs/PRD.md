# FinanceAgent — Product Requirements Document

| | |
|---|---|
| **Product** | FinanceAgent: personal finance and expense tracker |
| **Owner** | Eesha Haque |
| **Status** | v1.0 built; this document describes v1 and the planned roadmap |
| **Last updated** | October 26, 2026 |
| **Stack** | .NET 8, Blazor Server, EF Core 8, SQL Server, ASP.NET Core Identity |

---

## 1. Summary

FinanceAgent turns the CSV files people download from their bank into an organized, explainable picture
of their spending. Users import a statement, and a set of rule-based "agents" categorize transactions, flag
anything unusual, and recommend budget changes. Every decision an agent makes is logged with a reason, so
the user can see exactly what changed and why.

The app runs entirely on the user's own machine or server. It needs no third-party AI service or bank
connection, and each user's data stays private to their own account.

## 2. Problem

- **Bank exports are messy.** Descriptions like `TST* PHO SAIGON HOUSTON TX` or `SQ *BLUE BOTTLE` are hard to
  scan, and every bank uses different columns.
- **Categorizing by hand is tedious.** Most people give up on budgeting spreadsheets within weeks.
- **Problems hide in the noise.** Duplicate charges, quiet subscription price increases, and one-off large
  purchases are easy to miss in a long statement.
- **Budgets drift from reality.** People set budgets once and never revisit them, so the numbers stop being
  useful.
- **"Smart" finance apps are opaque.** Users don't know why a transaction was put somewhere, and many apps
  require handing over bank credentials or sending data to outside services.

## 3. Goals and non-goals

### Goals
1. Import a statement from any common US bank CSV layout in under a minute.
2. Categorize most transactions automatically, and get better from each correction the user makes.
3. Surface anomalies (duplicate charges, price increases, unusual amounts) without the user having to look.
4. Give budget recommendations based on the user's actual recent spending.
5. Make every automated decision explainable and auditable.
6. Support multiple users on one installation, with each user's data fully isolated.

### Non-goals (v1)
- Live bank connections (Plaid, Open Banking, etc.)
- Investment, loan, or net-worth tracking
- Personalized financial or investment advice
- Mobile apps (the web UI is responsive, but there is no native app)
- Shared or household accounts where several users see the same data
- Multi-currency (all amounts are treated as USD)

## 4. Users

| Persona | Description | Main need |
|---|---|---|
| **Budget-conscious individual** | Downloads a statement monthly and wants to know where the money went | Fast import, automatic categories, a clear monthly view |
| **Subscription-heavy user** | Has many recurring charges across services | Alerts for price increases and double billing |
| **Privacy-minded user** | Won't give bank credentials to a third-party app | Everything stays local; no external services |
| **Self-hoster / small household** | Runs one instance for several people | Separate logins, with no one seeing anyone else's data |

## 5. User stories

| ID | As a… | I want to… | So that… |
|---|---|---|---|
| US-1 | new user | create an account with a username and password | my data is private to me |
| US-2 | user | upload my bank's CSV without reformatting it | importing takes seconds |
| US-3 | user | re-import an overlapping statement safely | nothing gets counted twice |
| US-4 | user | have transactions categorized automatically | I don't sort them by hand |
| US-5 | user | fix a wrong category once and have it stick for that merchant | the app learns my habits |
| US-6 | user | be told about duplicate charges and price increases | I can dispute or cancel them |
| US-7 | user | see spending vs. budget for each category this month | I know where I stand |
| US-8 | user | get suggested budget changes based on real spending | my budgets stay realistic |
| US-9 | user | create, rename, and delete my own categories | the categories match my life |
| US-10 | user | see a log of everything the app changed | I can trust and audit it |

## 6. Functional requirements

Priority: **P0** = required for v1, **P1** = important, **P2** = nice to have. All P0 and P1 items below are
built in v1.

### 6.1 Accounts and authentication
| ID | Requirement | Priority |
|---|---|---|
| AUTH-1 | Users register with a unique username (3–50 characters: letters, numbers, `- . _ @`) and a password. | P0 |
| AUTH-2 | Passwords are at least 8 characters, with at least one lowercase letter and one number. | P0 |
| AUTH-3 | Passwords are stored only as salted hashes (ASP.NET Core Identity), never in plain text. | P0 |
| AUTH-4 | 5 failed logins lock the account for 5 minutes. | P0 |
| AUTH-5 | Login errors don't reveal whether the username exists. | P0 |
| AUTH-6 | Every page except login, registration, and the error page requires sign-in. | P0 |
| AUTH-7 | "Keep me logged in" keeps a session for up to 7 days (sliding). | P1 |
| AUTH-8 | After login, the user returns to the page they originally requested, but only if it's on this site. | P1 |
| AUTH-9 | New accounts get a default checking account and a starter set of categories. | P1 |

### 6.2 Data isolation
| ID | Requirement | Priority |
|---|---|---|
| ISO-1 | Every account, category, transaction, rule, and activity log entry belongs to exactly one user. | P0 |
| ISO-2 | Users can never read or change another user's data through any page or action. | P0 |
| ISO-3 | Isolation is enforced centrally in the data layer, not page by page. | P0 |
| ISO-4 | The first registered user adopts data that existed before accounts were added. | P1 |

### 6.3 CSV import
| ID | Requirement | Priority |
|---|---|---|
| IMP-1 | Accept CSV files up to 5 MB. | P0 |
| IMP-2 | Detect columns by common header names: date (`Date`, `Transaction Date`, `Posting Date`…), description (`Description`, `Payee`, `Merchant`…), and amount (`Amount`, or separate `Debit`/`Credit`). | P0 |
| IMP-3 | Parse common US date formats, `$` and `,` in amounts, and accounting-style negatives such as `(12.34)`. | P0 |
| IMP-4 | Skip rows already imported (same account, date, amount, and description), while still allowing genuine identical charges within one file. | P0 |
| IMP-5 | Report how many rows were imported, skipped as duplicates, and skipped as unreadable. | P0 |
| IMP-6 | Optionally run the categorization and anomaly agents right after import. | P1 |
| IMP-7 | A maintenance action removes duplicates created before duplicate detection existed, after the user confirms. | P2 |

### 6.4 Categorization
| ID | Requirement | Priority |
|---|---|---|
| CAT-1 | Built-in keyword rules categorize common merchants, matching whole words only (e.g. `SHELL` does not match `SHELLFISH`). | P0 |
| CAT-2 | Transactions no rule matches go to `Uncategorized` and appear in a **Needs review** queue. | P0 |
| CAT-3 | The user can change any transaction's category from the Transactions page. | P0 |
| CAT-4 | With **Remember merchant** on, a correction becomes a rule: it applies to that merchant's other uncategorized transactions right away and to future imports. | P1 |
| CAT-5 | Rules learned from the user take priority over built-in rules. | P1 |

### 6.5 Category management
| ID | Requirement | Priority |
|---|---|---|
| CM-1 | Users can add a category with a name, a type (Expense, Income, or Transfer), and a monthly budget. | P0 |
| CM-2 | Users can rename a category and change its type and budget. | P0 |
| CM-3 | Category names are required, at most 100 characters, and unique per user (case-insensitive). | P0 |
| CM-4 | Deleting a category that has transactions requires choosing another category to move them to. Learned rules move with them. | P0 |
| CM-5 | `Uncategorized` is built in and can't be renamed or deleted. | P0 |
| CM-6 | Only Expense categories count toward spending totals and budgets. | P0 |
| CM-7 | The page shows how many transactions each category has, linking to those transactions. | P1 |

### 6.6 Anomaly detection
| ID | Requirement | Priority |
|---|---|---|
| ANO-1 | Flag a possible duplicate: same merchant and amount within 3 days. | P0 |
| ANO-2 | Flag a subscription charge that differs from the previous charge. | P1 |
| ANO-3 | Flag a charge more than 2× the merchant's median and at least $20 above it (needs 2+ earlier charges). | P1 |
| ANO-4 | Flag a first-time merchant charge of $300 or more. | P1 |
| ANO-5 | Every flag shows a plain-language reason. | P0 |
| ANO-6 | Users can dismiss a flag, and a dismissed flag isn't raised again. | P0 |
| ANO-7 | Income and transfers are never flagged. | P1 |

### 6.7 Budgets and insights
| ID | Requirement | Priority |
|---|---|---|
| BUD-1 | The dashboard shows income, spending, net, and the count of flagged and to-review items for a selected month. | P0 |
| BUD-2 | Each Expense category shows spending vs. budget, highlighted near and over the limit. | P0 |
| BUD-3 | Alerts warn when a category is over budget or on pace to go over this month. | P1 |
| BUD-4 | The advisor reviews the last 3 months and suggests raising budgets that are regularly exceeded, lowering ones that are rarely used, and setting ones that are missing. | P1 |
| BUD-5 | The advisor never suggests lowering a budget that was close to or over its limit in any recent month. | P1 |
| BUD-6 | Suggestions change nothing until the user clicks **Apply**. | P0 |
| BUD-7 | Users can edit any budget directly. | P0 |

### 6.8 Activity log
| ID | Requirement | Priority |
|---|---|---|
| LOG-1 | Every automatic categorization, anomaly flag, budget change, recategorization, and category change is logged with time, actor, input, output, and reason. | P0 |
| LOG-2 | The Agent Activity page lists the latest 300 entries and can filter by agent. | P1 |

## 7. Non-functional requirements

| Area | Requirement |
|---|---|
| **Security** | Hashed passwords, account lockout, antiforgery tokens on forms, HTTPS redirection, no secrets in source control. |
| **Privacy** | No third-party services. Transaction data never leaves the user's database. |
| **Isolation** | Global EF Core query filters on `UserId`. With no signed-in user, user-owned queries return nothing. Rows can't be reassigned to another user. |
| **Reliability** | Pages work without any optional configuration. Deleting a category runs in a single database transaction. |
| **Performance** | A 1,000-row CSV imports and processes in under 5 seconds on a typical laptop. The dashboard loads in under 1 second for a year of data. |
| **Explainability** | No unexplained automated changes: everything goes through the activity log. |
| **Maintainability** | Schema changes go through EF Core migrations. An idempotent SQL script is available for manual setup in SSMS. |
| **Accessibility** | Form fields have labels, there is a visible focus state, and the layout works at phone width. |

## 8. Data model (overview)

| Entity | Purpose | Key fields |
|---|---|---|
| `AspNetUsers` (+ Identity tables) | Login accounts | `UserName`, `PasswordHash`, lockout fields |
| `Accounts` | A bank account | `UserId`, `Name`, `Institution` |
| `Transactions` | One imported transaction | `UserId`, `AccountId`, `Date`, `Amount`, `RawDescription`, `Merchant`, `CategoryId`, anomaly fields |
| `Categories` | Spending/income buckets | `UserId`, `Name` (unique per user), `Kind`, `MonthlyBudget` |
| `CategoryRules` | Merchant rules learned from corrections | `UserId`, `Keyword`, `CategoryId` |
| `AgentActions` | Audit log | `UserId`, `Timestamp`, `AgentName`, `ActionType`, `Input`, `Output`, `Reasoning` |

Every user-owned table has a foreign key to `AspNetUsers`.

## 9. Success metrics

| Metric | Target |
|---|---|
| Transactions categorized automatically on first import | ≥ 75% |
| Share of the review queue cleared after 1 month of corrections | ≥ 90% auto-categorized on later imports |
| Time from file selection to categorized dashboard | < 10 seconds for a typical monthly statement |
| Anomaly flags the user dismisses as noise | < 30% |
| Budget suggestions the user applies | ≥ 25% |
| Cross-user data exposure incidents | 0 |

## 10. Release plan and roadmap

| Phase | Scope | Status |
|---|---|---|
| **v1.0** | Everything in section 6 | Done |
| **v1.1** | Automated tests (import parsing, anomaly rules, data isolation); password change and reset; per-user CSV column mapping for unusual banks | Planned |
| **v1.2** | Multiple bank accounts per user in the UI; spending trend charts; CSV/Excel export | Planned |
| **v2.0** | Optional bank connection through an aggregator; email alerts for anomalies; recurring-bill calendar | Exploring |

## 11. Risks and open questions

| Risk / question | Mitigation / next step |
|---|---|
| Keyword rules miss regional merchants | Learned rules from corrections; consider shipping larger rule sets |
| Anomaly thresholds may be noisy for some users | Dismissals are tracked; consider per-user thresholds in v1.1 |
| No password reset without email | Add an optional email address and reset flow in v1.1 |
| Renaming a category disables the built-in keyword rules tied to its name | Documented on the Categories page; consider tying built-in rules to category kind or ID |
| LocalDB is single-machine only | Connection string can point at any SQL Server for shared hosting |
| Should households share data? | Out of scope for v1; revisit with demand |
