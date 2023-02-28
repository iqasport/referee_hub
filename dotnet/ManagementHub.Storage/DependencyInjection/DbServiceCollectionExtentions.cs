using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Domain.User;
using ManagementHub.Service.Areas.Identity;
using ManagementHub.Storage.Contexts;
using ManagementHub.Storage.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace ManagementHub.Storage.DependencyInjection
{
	public static class DbServiceCollectionExtentions
	{
		/// <summary>
		/// Name of the configuration section for Postgres connection.
		/// </summary>
		public const string DatabaseConnectionSection = "DatabaseConnection";

		/// <summary>
		/// Adds dependencies for services of the storage based implementations of abstract interfaces.
		/// </summary>
		public static IServiceCollection AddManagementHubStorage(this IServiceCollection services, bool inMemoryStorage)
		{
			if (inMemoryStorage)
			{
				var storageRoot = new InMemoryDatabaseRoot(); // the storage root allows sharing of data between requests
				services.AddDbContext<ManagementHubDbContext>((serviceProvider, options) =>
				{
					options.UseInMemoryDatabase("ManagementHubMemoryDatabase", storageRoot);
				});
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
			}

			services.AddScoped<IContextProvider>(sp => new CachedContextProvider(new DbContextProvider(
				sp.GetRequiredService<ManagementHubDbContext>(),
				sp.GetRequiredService<ILoggerFactory>())));

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
}
