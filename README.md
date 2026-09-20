# ICAS-DeviceCfg

A C# console application that reads warehouse device configuration from SQL Server
(`dbo.t_DeviceCfg`), prints it as a table, and can look up a single device by ID.

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) 10.0 or later
- SQL Server. This project's connection string points at **SQL Server LocalDB**
  (`(localdb)\MSSQLLocalDB`), which ships with Visual Studio and is free to install
  on its own. SQL Server Express or a Docker SQL Server instance also work, but you'll
  need to update the connection string in `Program.cs` (see **Using a different SQL
  Server instance** below).

To check LocalDB is available:

```
sqllocaldb info
```

This should list `MSSQLLocalDB`.

## 1. Create the database

Run the supplied `01-setup.sql` once, from a terminal in this folder, using `sqlcmd`:

```
sqlcmd -S "(localdb)\MSSQLLocalDB" -i "01-setup.sql"
```

This creates the `ICAS_Test` database, the `dbo.t_DeviceCfg` table, and 10 sample rows.
It's safe to re-run — the script drops and recreates the table each time.

To double-check the data loaded:

```
sqlcmd -S "(localdb)\MSSQLLocalDB" -d ICAS_Test -Q "SELECT * FROM dbo.t_DeviceCfg" -W
```

## 2. Build and run

From the project folder:

```
dotnet build
```

Print all devices:

```
dotnet run
```

Print a single device by ID:

```
dotnet run -- 3
```

(The `--` tells `dotnet run` that what follows is an argument for the app, not for
`dotnet` itself. If you build and run the standalone executable instead, pass the
argument directly, e.g. `ICAS-DeviceCfg.exe 3`.)

## What it does when things aren't right

- **Non-numeric ID** (e.g. `dotnet run -- abc`) — prints a plain error message and
  exits with a non-zero exit code, no stack trace.
- **ID that doesn't exist** (e.g. `dotnet run -- 999`) — prints an explicit
  "no device found" message instead of an empty table.
- **Database unreachable** — prints a plain error message with the underlying detail,
  instead of a raw exception.
- **Data that doesn't make sense** — the listing still prints every row it can, but
  adds a `Warnings:` section underneath for rows worth a second look, e.g. a device
  marked enabled with no IP address, or a negative slot index.

## Using a different SQL Server instance

The connection string is a single constant near the top of `Program.cs`:

```csharp
const string ConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=ICAS_Test;Trusted_Connection=True;";
```

Update the `Server=` part to point at SQL Server Express, a Docker instance, or any
other SQL Server, then re-run `01-setup.sql` against that instance before running the app.
