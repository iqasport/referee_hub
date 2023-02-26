# Models

Created from the current database schema using `dotnet-ef` tool.

```
dotnet ef dbcontext scaffold "Server=localhost:3050;Database=referee_hub_development;User Id=docker;Password=docker;" Npgsql.EntityFrameworkCore.PostgreSQL -c ManagementHubDbContext --context-dir Context
```

See documentation https://learn.microsoft.com/en-us/ef/core/cli/dotnet
