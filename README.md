# FinanceAgent

A personal finance agent built with Blazor Server, EF Core and SQL Server. It imports bank CSV exports,
categorizes spending, flags anomalies, and suggests budget changes. Everything runs locally with no
external AI service or API key.

See the [product requirements document](docs/PRD.md) for goals, requirements, and the roadmap.

## Agents

| Agent | What it does |
|---|---|
| `RuleBasedCategorizer` | Keyword rules, plus rules it learns when you correct a category |
| `AnomalyDetectionAgent` | Flags duplicate charges, subscription price changes, unusually large charges for a merchant, and big first-time merchants |
| `BudgetAdvisorAgent` | Compares the last 3 months to your budgets, suggests changes, and warns about categories on pace to go over |

`AgentPipeline` runs keyword rules → anomaly scan after each import. Anything the rules can't place lands in the
**Needs review** queue on the Transactions page. Every agent decision is written to
`AgentActions` and shown on the **Agent Activity** page.

## Accounts

Each person creates an account with a username and password (ASP.NET Core Identity: passwords are salted
and hashed, and an account locks for 5 minutes after 5 wrong passwords). Every account has its own bank
accounts, categories, transactions, rules, and activity log. EF Core global query filters on `UserId` keep one
user's data invisible to everyone else.

The **first** account registered adopts any data that existed before accounts were added.

## Categories

The **Categories** page adds, renames, and deletes categories. Each category is an **Expense** (counts toward
spending and budgets), **Income**, or **Transfer**. Deleting a category that has transactions moves them to a
category you pick. `Uncategorized` is built in and can't be renamed or deleted, since it holds the review queue.

## Setup

1. Run the app (`dotnet run`). It creates or migrates the database on startup.
2. Open the app, choose **Create account**, and sign in.
3. Import `SampleData/sample_transactions.csv` on the **Import** page to try it out.
   `SampleData/sample_debit_credit.csv` shows the Debit/Credit column layout and overlaps with the first file,
   so it also exercises duplicate detection.

## Database (SQL Server / SSMS)

The app uses the `FinanceAgentDb` database on SQL Server LocalDB. To open it in SQL Server Management Studio:

1. Connect with **Server name** `(localdb)\MSSQLLocalDB` and **Authentication** `Windows Authentication`.
   SSMS 20+ defaults to strict encryption; if the connection fails, set **Encryption** to `Optional` or tick
   **Trust server certificate**.
2. Expand **Databases → FinanceAgentDb**.

To create the database from SSMS instead of letting the app do it, create an empty database named
`FinanceAgentDb`, then open and run [`Database/FinanceAgentDb.sql`](Database/FinanceAgentDb.sql). The script is
idempotent, so it only applies the migrations a database is missing. Regenerate it after adding a migration:

```bash
dotnet ef migrations script --idempotent -o Database/FinanceAgentDb.sql
```

To use a different SQL Server instance, change `ConnectionStrings:FinanceDb` in `appsettings.json`.

## CSV format

The importer looks for these headers (case-insensitive):

- Date: `Date`, `Transaction Date`, `Posting Date`, `Posted Date`
- Description: `Description`, `Payee`, `Merchant`, `Name`, `Memo`
- Amount: `Amount` (negative = money out), **or** separate `Debit` / `Credit` columns

Rows already in the database (same account, date, amount and description) are skipped, so re-importing
an overlapping statement is safe.
