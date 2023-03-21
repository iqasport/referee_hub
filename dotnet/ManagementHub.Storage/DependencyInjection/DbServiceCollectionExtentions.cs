using System.Linq;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.User;
using ManagementHub.Processing.Contexts;
using ManagementHub.Service.Areas.Identity;
using ManagementHub.Storage.Commands;
using ManagementHub.Storage.Contexts;
using ManagementHub.Storage.Database;
using ManagementHub.Storage.Database.Transactions;
using ManagementHub.Storage.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace ManagementHub.Storage.DependencyInjection;

public static class DbServiceCollectionExtentions
{
	/// <summary>
	/// Name of the configuration section for Postgres connection.
	/// </summary>
	public const string DatabaseConnectionSection = "DatabaseConnection";
	private const string SharedInMemorySqliteConnectionString = "DataSource=ManagementHub;mode=memory;cache=shared";

	/// <summary>
	/// Adds dependencies for services of the storage based implementations of abstract interfaces.
	/// </summary>
	public static IServiceCollection AddManagementHubStorage(this IServiceCollection services, bool inMemoryStorage)
	{
		if (inMemoryStorage)
		{
			services.AddDbContext<ManagementHubDbContext>((options) =>
			{
				options.UseSqlite(SharedInMemorySqliteConnectionString);
				options.EnableDetailedErrors();
			});

			// mandatory workaround for in memory shared Sqlite
			services.AddHostedService<EnsureDatabaseConnectionKeptAliveService>();

			// for in memory database we run creation script
			services.AddHostedService<EnsureDatabaseCreatedService>();
		}
		else
		{
			// allow creating a connection string builder directly based on configuration
			services.AddOptions<NpgsqlConnectionStringBuilder>()
				.BindConfiguration(DatabaseConnectionSection);

			services.AddDbContext<ManagementHubDbContext>((serviceProvider, options) =>
			{
				var connectionStringBuilder = serviceProvider.GetRequiredService<IOptionsSnapshot<NpgsqlConnectionStringBuilder>>();
				var connectionString = connectionStringBuilder.Value.ConnectionString;
				options.UseNpgsql(connectionString);
			});

			// for hosted database we run migration script
			services.AddHostedService<EnsureDatabaseMigratedService>();
		}

		services.AddScoped<IContextProvider>(sp => new CachedContextProvider(new DbContextProvider(
			sp.GetRequiredService<ManagementHubDbContext>(),
			sp.GetRequiredService<ILoggerFactory>())));

		services.AddTransient<IQueryable<User>>(sp => sp.GetRequiredService<ManagementHubDbContext>().Set<User>());
		services.AddTransient<IQueryable<Language>>(sp => sp.GetRequiredService<ManagementHubDbContext>().Set<Language>());

		services.AddScoped<IUpdateUserDataCommand, UpdateUserDataCommand>();

		services.AddScoped<IDatabaseTransactionProvider, DatabaseTransactionProvider>();

		return services;
	}

	/// <summary>
	/// Adds identity dependencies.
	/// </summary>
	public static IServiceCollection AddManagementHubIdentity(this IServiceCollection services)
	{
		services.AddTransient<IUserIdentityRepository, UserIdentityRepository>();
		services.AddTransient<IUserStore<UserIdentity>, UserStore>();
		services.AddTransient<EmailTokenProvider>();
		
		// custom overrides over user manager
		services.AddScoped<UserManager<UserIdentity>, UserManager>();

		return services;
	}
}
