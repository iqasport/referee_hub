﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ManagementHub.Models.Enums;
using Service.API.Test.DatabaseClient;
using Service.API.Test.WebsiteClient;

namespace Service.API.Test.Helpers
{
	public class UserContext : IAsyncDisposable
	{
		private static readonly FastHashes.FarmHash64 HashAlgorithm = new();

		private UserContext(DatabaseProvider databaseProvider, RequestBuilder requestBuilder, string email)
		{
			this.Database = databaseProvider ?? throw new ArgumentNullException(nameof(databaseProvider));
			this.WebClient = requestBuilder ?? throw new ArgumentNullException(nameof(requestBuilder));
			this.Email = !string.IsNullOrWhiteSpace(email) ? email : throw new ArgumentException("Email must not be an empty string", nameof(email));
		}

		public DatabaseProvider Database { get; }

		public RequestBuilder WebClient { get; }

		public string Email { get; }

		public async ValueTask DisposeAsync() => await Database.DeleteUserAsync(Email);

		public static Task<UserContext> NewRefereeAsync(DatabaseProvider databaseProvider, RequestBuilder requestBuilder, int seed = 0, [CallerMemberName] string memberName = "")
		{
			var context = new UserContext(databaseProvider, requestBuilder, GenerateEmailFromString(memberName, seed));
			return NewUserAsync(context, UserAccessType.Referee);
		}

		public static Task<UserContext> NewNgbAdminAsync(DatabaseProvider databaseProvider, RequestBuilder requestBuilder, string country, int seed = 0, [CallerMemberName] string memberName = "")
		{
			var context = new UserContext(databaseProvider, requestBuilder, GenerateEmailFromString(memberName, seed));
			return NewNgbAdminInternalAsync(context, country);
		}

		public static Task<UserContext> NewIqaAdminAsync(DatabaseProvider databaseProvider, RequestBuilder requestBuilder, int seed = 0, [CallerMemberName] string memberName = "")
		{
			var context = new UserContext(databaseProvider, requestBuilder, GenerateEmailFromString(memberName, seed));
			return NewUserAsync(context, UserAccessType.IqaAdmin);
		}

		private static async Task<UserContext> NewUserAsync(UserContext context, UserAccessType accessType)
		{
			await context.Database.DeleteUserAsync(context.Email, requireUserToExist: false);
			await context.WebClient.RegisterUserAsync("John", "Smith", context.Email);
			await context.Database.SetUserAccessTypeAsync(context.Email, accessType);

			return context;
		}

		private static async Task<UserContext> NewNgbAdminInternalAsync(UserContext context, string country)
		{
			context = await NewUserAsync(context, UserAccessType.NgbAdmin);
			await context.Database.SetNgbAdminCountryAsync(context.Email, country);

			return context;
		}

		private static string GenerateEmailFromString(string input, int seed = 0)
		{
			var inputAsByteSpan = MemoryMarshal.Cast<char, byte>(input.AsSpan());
			// We're using a stable hashing algorithm here to ensure the same email is use for a given test between runs
			string hash = Convert.ToBase64String(HashAlgorithm.ComputeHash(inputAsByteSpan));
			// Need to call ToLower, because Devise is doing that before saving the email to the database anyways
			return $"user_{hash.ToLowerInvariant()}{(seed != 0 ? seed.ToString() : string.Empty)}@example.com";
		}
	}
}