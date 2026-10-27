# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

Primary audience: recruiters and technical reviewers looking at the project as a portfolio showcase of
.NET 8, Blazor Server, EF Core, and SQL Server skills. They usually register a throwaway account, import the
sample CSV, and click through the pages in a few minutes.

The app must also hold up for real personal use: an individual who downloads a bank statement each month and
wants to see where the money went, fix categories, catch odd charges, and keep budgets realistic.

## Product Purpose

Turns bank CSV exports into an organized, explainable picture of spending. Rule-based agents categorize
transactions, flag anomalies, and suggest budget changes. Every automated change is logged with a reason.
Success: a reviewer understands what the app does within a minute of importing sample data, and a real user
trusts the numbers enough to act on them.

## Positioning

Explainable and local. No bank credentials, no third-party AI or data services: categorization comes from
keyword rules that learn from the user's own corrections, and an activity log shows why every change happened.

## Operating Context

- Sign up with a username and password; each user's data is fully isolated.
- Workflow: Import CSV → agents categorize and scan → review queue on Transactions → adjust categories and
  budgets → watch the dashboard and alerts.
- Sample data in `SampleData/` (3 months of realistic transactions) is the usual demo path.
- Runs locally on SQL Server LocalDB; the database is browsable in SSMS.

## Capabilities and Constraints

- Pages: Dashboard, Transactions, Categories, Budgets, Import, Agent Activity, Log in, Create account.
- Data the app actually has: bank accounts (name, institution), transactions (date, description, amount,
  category, anomaly flag/reason), categories (Expense / Income / Transfer, monthly budget), learned merchant
  rules, the agent activity log, budget suggestions, and pace alerts.
- The app does **not** track investments, savings goals, credit card balances, or live account balances.
  UI must only show real data: no placeholder widgets or invented figures.
- Amounts are USD only.
- Stack: .NET 8 Blazor Web App (interactive server pages; login and registration are static server-rendered
  forms), Bootstrap 5 plus custom CSS. No JavaScript chart library is in use.

## Brand Commitments

- Product name stays **FinanceAgent**.
- The user supplied a dark indigo/purple glass-style finance dashboard ("Fintrivo" mockup) as visual
  inspiration for look and layout only; its name and branding are not adopted.

## Evidence on Hand

- `docs/PRD.md`: requirements, roadmap, success metrics.
- `SampleData/sample_transactions.csv`, `SampleData/sample_debit_credit.csv`.
- No real users, testimonials, or usage metrics exist; none may be fabricated.

## Product Principles

1. Show only true numbers; an empty state beats a fake widget.
2. Every automated decision is explainable in plain language.
3. A first-time reviewer should reach a populated, impressive dashboard in a few clicks.
4. Privacy by default: data never leaves the user's own database.
