# Billing Mailer

A .NET 8 Worker Service that sends monthly billing emails to active customers using data stored in SQL Server. The worker runs automatically on a schedule, sends formatted bill emails through SMTP, and records each send attempt in an email log table.

## Features

- Monthly background billing run on the 1st day of each month at 8:00 AM
- SQL Server persistence for customers, bills, and outbound email history
- Entity Framework Core DbContext and migrations support
- SMTP bill delivery with MailKit
- Email send auditing (success/failure + error details)
- Windows Service hosting support

## Tech Stack

| Area | Technology |
|---|---|
| Runtime | .NET 8 Worker Service |
| Database | SQL Server + Entity Framework Core 8 |
| Email | MailKit (SMTP) |
| Scheduler | BackgroundService with monthly delay logic |

## Prerequisites

- .NET SDK 8.0+
- SQL Server instance (local or remote)
- SMTP provider credentials
- (Optional) EF Core CLI tools for migration commands:
  - `dotnet tool install --global dotnet-ef`

## Setup

1. Clone the repository:

   ```bash
   git clone https://github.com/bcalloo/billing-mailer.git
   cd billing-mailer
   ```

2. Configure `BillingMailer/appsettings.json`:
   - `ConnectionStrings:DefaultConnection`
   - `Email:SmtpHost`, `Email:SmtpPort`, `Email:SenderName`, `Email:SenderAddress`, `Email:Username`, `Email:Password`

3. Create and apply migrations:

   ```bash
   dotnet ef migrations add InitialCreate --project BillingMailer/BillingMailer.csproj
   dotnet ef database update --project BillingMailer/BillingMailer.csproj
   ```

4. Run the worker locally:

   ```bash
   dotnet run --project BillingMailer/BillingMailer.csproj
   ```

## Database Schema

| Table | Purpose | Key Columns |
|---|---|---|
| `Customers` | Stores bill recipients | `Id`, `FullName`, `Email`, `AccountNumber`, `IsActive` |
| `Bills` | Stores bill amounts and billing periods | `Id`, `CustomerId`, `AmountDue`, `BillingPeriodStart`, `BillingPeriodEnd`, `DueDate`, `IsPaid` |
| `EmailLogs` | Tracks each bill email attempt | `Id`, `BillId`, `SentAt`, `Success`, `ErrorMessage` |

## Scheduling Logic

The worker computes the next run as the 1st day of the month at 8:00 AM local server time:

- If current time is before that timestamp, it waits for the current month’s schedule.
- If current time is at/after that timestamp, it schedules for the next month.
- During processing, it only sends bills for the current UTC billing period start and skips bills that already have a successful email log for the same period.

## Deploy as a Windows Service (sc.exe)

1. Publish the worker:

   ```powershell
   dotnet publish .\BillingMailer\BillingMailer.csproj -c Release -o C:\Services\BillingMailer
   ```

2. Create the service:

   ```powershell
   sc.exe create "Monthly Billing Mailer" binPath= "C:\Services\BillingMailer\BillingMailer.exe" start= auto
   ```

3. Start the service:

   ```powershell
   sc.exe start "Monthly Billing Mailer"
   ```

4. (Optional) Stop/Delete:

   ```powershell
   sc.exe stop "Monthly Billing Mailer"
   sc.exe delete "Monthly Billing Mailer"
   ```
