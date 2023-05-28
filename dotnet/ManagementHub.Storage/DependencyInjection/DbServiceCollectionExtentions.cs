using System;
using System.Linq;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.Redis.StackExchange;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Abstraction.Commands.Tests;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.User;
using ManagementHub.Processing.Contexts;
using ManagementHub.Service.Areas.Identity;
using ManagementHub.Storage.Attachments;
using ManagementHub.Storage.BlobStorage.AmazonS3;
using ManagementHub.Storage.BlobStorage.LocalFilesystem;
using ManagementHub.Storage.Commands.Referee;
using ManagementHub.Storage.Commands.Tests;
using ManagementHub.Storage.Commands.User;
using ManagementHub.Storage.Contexts.Referee;
using ManagementHub.Storage.Contexts.Tests;
using ManagementHub.Storage.Contexts.User;
using ManagementHub.Storage.Database;
using ManagementHub.Storage.Database.Transactions;
using ManagementHub.Storage.DbAccessors;
using ManagementHub.Storage.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;
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
			// NOTE: this storage is deleted when application shuts down.
			services.AddDbContext<ManagementHubDbContext>((options) =>
			{
				options.UseSqlite(SharedInMemorySqliteConnectionString);
				options.EnableSensitiveDataLogging();
				options.EnableDetailedErrors();
			});

			// mandatory workaround for in memory shared Sqlite
			services.AddHostedService<EnsureDatabaseConnectionKeptAliveService>();

			// for in memory database we run creation script
			services.AddHostedService<EnsureDatabaseCreatedService>();

			// seed the database with test data
			services.AddHostedService<EnsureDatabaseSeededForTesting>();
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

			// save encryption keys to the database so that they persist across app startups
			services.AddDataProtection()
				.PersistKeysToDbContext<ManagementHubDbContext>();

			// for hosted database we run migration script
			services.AddHostedService<EnsureDatabaseMigratedService>();
		}

		services.AddScoped<IUserContextProvider>(sp => new CachedUserContextProvider(new DbUserContextProvider(
			sp.GetRequiredService<ManagementHubDbContext>(),
			sp.GetRequiredService<IAttachmentRepository>(),
			sp.GetRequiredService<IAccessFileCommand>(),
			sp.GetRequiredService<ILoggerFactory>())));
		services.AddScoped<IRefereeContextProvider, DbRefereeContextProvider>();
		services.AddScoped<ITestContextProvider, DbTestContextProvider>();

		services.AddTransient<IQueryable<User>>(sp => sp.GetRequiredService<ManagementHubDbContext>().Set<User>());
		services.AddTransient<IQueryable<Language>>(sp => sp.GetRequiredService<ManagementHubDbContext>().Set<Language>());

		services.AddScoped<IUpdateUserDataCommand, UpdateUserDataCommand>();
		services.AddScoped<IUpdateUserAvatarCommand, UpdateUserAvatarCommand>();
		services.AddScoped<IUpdateRefereeRoleCommand, UpdateRefereeRoleCommand>();
		services.AddScoped<ISaveSubmittedTestCommand, SaveSubmittedTestCommand>();

		services.AddScoped<IAttachmentRepository, AttachmentRepository>();
		services.AddTransient<IDbAccessorProvider, DbAccessorProvider>();
		services.AddTransient<IDbAccessor<UserIdentifier>, UserDbAccessor>();

		services.AddTransient<ISystemClock, SystemClock>();

		services.AddScoped<IDatabaseTransactionProvider, DatabaseTransactionProvider>();

		return services;
	}

	public static IServiceCollection AddManagementHubBlobStorage(this IServiceCollection services, bool localFileSystem)
	{
		if (localFileSystem)
		{
			services.AddSingleton<LocalFilesystemBlobStorageManager>();
			services.AddSingleton<IUploadFileCommand>(sp => sp.GetRequiredService<LocalFilesystemBlobStorageManager>());
			services.AddSingleton<IAccessFileCommand>(sp => sp.GetRequiredService<LocalFilesystemBlobStorageManager>());
		}
		else
		{
			services.AddOptions<AmazonS3Config>()
				.ValidateOnStart()
				.BindConfiguration("AWS");

			services.AddSingleton<AmazonBlobStorageManager>();
			services.AddSingleton<IUploadFileCommand>(sp => sp.GetRequiredService<AmazonBlobStorageManager>());
			services.AddSingleton<IAccessFileCommand>(sp => sp.GetRequiredService<AmazonBlobStorageManager>());
		}

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
		services.AddTransient<IPasswordHasher<UserIdentity>, BCryptPasswordHasher<UserIdentity>>();

		// custom overrides over user manager
		services.AddScoped<UserManager<UserIdentity>, UserManager>();

		return services;
	}

	public static IServiceCollection AddHangfire(this IServiceCollection services, bool inMemoryStorage)
	{
		return services.AddHangfire(config =>
		{
			if (inMemoryStorage)
			{
				// register job system in memory
				config.UseMemoryStorage();
			}
			else
			{
				// TODO: pass in redis connection string
				config
					.UseRedisStorage()
					.WithJobExpirationTimeout(TimeSpan.FromDays(7));
			}

			config.UseRecommendedSerializerSettings();
		});
	}
}
