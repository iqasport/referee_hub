using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Storage.Contexts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
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
		public static IServiceCollection AddManagementHubStorage(this IServiceCollection services)
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

			services.AddScoped<IContextProvider, DbContextProvider>();

			return services;
		}
	}
}
