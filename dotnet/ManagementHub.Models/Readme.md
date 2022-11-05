# Models

Created from the current database schema using `dotnet-ef` tool.

```
dotnet user-secrets set DbConnection:ManagementHub "Server=localhost:3050;Database=referee_hub_development;User Id=docker;Password=docker;"
dotnet ef dbcontext scaffold "Name=DbConnection:ManagementHub" Npgsql.EntityFrameworkCore.PostgreSQL -c ManagementHubDbContext --context-dir Context
```

See documentation https://learn.microsoft.com/en-us/ef/core/cli/dotnet
