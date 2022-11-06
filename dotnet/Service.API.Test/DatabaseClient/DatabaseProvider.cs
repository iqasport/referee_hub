using System;
using System.Data.Common;
using System.Threading.Tasks;
using ManagementHub.Models.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Service.API.Test.Settings;

namespace Service.API.Test.DatabaseClient
{
	public class DatabaseProvider
	{
		private readonly DatabaseSettings databaseSettings;
		private readonly ILoggerFactory loggerFactory;

		public DatabaseProvider(IOptions<DatabaseSettings> databaseSettings, ILoggerFactory loggerFactory)
		{
			this.databaseSettings = databaseSettings.Value;
			this.loggerFactory = loggerFactory;
		}

		public async Task<DatabaseConnection> ConnectAsync()
		{
			var builder = new NpgsqlConnectionStringBuilder
			{
				Host = this.databaseSettings.Host,
				Port = this.databaseSettings.Port,
				Database = this.databaseSettings.Database,
				Username = this.databaseSettings.UserName,
				Password = this.databaseSettings.Password,
			};

			var connection = new NpgsqlConnection(builder.ConnectionString);
			await connection.OpenAsync();

			return new DatabaseConnection(connection, this.loggerFactory);
		}

		public class DatabaseConnection : IDisposable
		{
			private readonly DbConnection connection;
			private readonly ILoggerFactory loggerFactory;
			private readonly Lazy<ManagementHubDbContext> context;

			public DatabaseConnection(DbConnection connection, ILoggerFactory loggerFactory)
			{
				this.connection = connection;
				this.loggerFactory = loggerFactory;
				this.context = new Lazy<ManagementHubDbContext>(() =>
				{
					var optionsBuilder = new DbContextOptionsBuilder<ManagementHubDbContext>()
						.UseNpgsql(this.connection)
						.UseLoggerFactory(this.loggerFactory);

					return new ManagementHubDbContext(optionsBuilder.Options);
				});
			}

			public ManagementHubDbContext Context => context.Value;
			
			public void Dispose()
			{
				connection.Dispose();
			}
		}
	}
}
