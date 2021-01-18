# Setting up Security for development

See [https://docs.microsoft.com/en-us/aspnet/core/security/https](https://docs.microsoft.com/en-us/aspnet/core/security/https)

## Migrating

To migrate ASP.NET Identity use `dotnet ef migrations add <name> -c ApplicationDbContext`

To migrate Persisted grant `dotnet ef migrations add <name> -c PersistedGrantDbContext`

To migrate Configuration `dotnet ef migrations add <name> -c ConfigurationDbContext`