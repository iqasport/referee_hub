using System;
using System.Linq;
using System.Threading.Tasks;
using ManagementHub.Models;
using ManagementHub.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Service.API.Test.DatabaseClient
{
	public static class UserDbExtensions
	{
		public static async Task SetUserAccessType(this DatabaseProvider databaseProvider, string email, UserAccessType accessType)
		{
			using (var db = await databaseProvider.ConnectAsync())
			{
				var dbUser = db.Context.Users
					.Include(x => x.Roles)
					.AsSingleQuery()
					.SingleOrDefault(u => u.Email == email);

				if (dbUser == null)
				{
					throw new InvalidOperationException($"No user with email {email} exists.");
				}

				db.Context.Roles.RemoveRange(dbUser.Roles);
				dbUser.Roles.Add(new Role
				{
					User = dbUser,
					AccessType = accessType,
					CreatedAt = DateTime.Now,
					UpdatedAt = DateTime.Now,
				});

				await db.Context.SaveChangesAsync();
			}
		}

		public static async Task DeleteUserAsync(this DatabaseProvider databaseProvider, string email, bool requireUserToExist = true)
		{
			using (var db = await databaseProvider.ConnectAsync())
			{
				var dbUser = db.Context.Users
					.Include(u => u.CertificationPayments)
					.Include(u => u.NationalGoverningBodyAdmin)
					.Include(u => u.PolicyManagerPortabilityRequests)
					.Include(u => u.PolicyManagerUserTerms)
					.Include(u => u.RefereeAnswers)
					.Include(u => u.RefereeCertifications)
					.Include(u => u.RefereeLocations)
					.Include(u => u.RefereeTeams)
					.Include(u => u.Roles)
					.Include(u => u.TestAttempts)
					.Include(u => u.TestResults)
					.AsSingleQuery()
					.SingleOrDefault(u => u.Email == email);

				if (dbUser == null)
				{
					if (requireUserToExist) throw new InvalidOperationException($"No user with email {email} exists.");
					else return;
				}

				db.Context.CertificationPayments.RemoveRange(dbUser.CertificationPayments);
				db.Context.PolicyManagerPortabilityRequests.RemoveRange(dbUser.PolicyManagerPortabilityRequests);
				db.Context.PolicyManagerUserTerms.RemoveRange(dbUser.PolicyManagerUserTerms);
				db.Context.RefereeAnswers.RemoveRange(dbUser.RefereeAnswers);
				db.Context.RefereeCertifications.RemoveRange(dbUser.RefereeCertifications);
				db.Context.RefereeLocations.RemoveRange(dbUser.RefereeLocations);
				db.Context.RefereeTeams.RemoveRange(dbUser.RefereeTeams);
				db.Context.Roles.RemoveRange(dbUser.Roles);
				db.Context.TestAttempts.RemoveRange(dbUser.TestAttempts);
				db.Context.TestResults.RemoveRange(dbUser.TestResults);

				if (dbUser.NationalGoverningBodyAdmin is not null)
				{
					db.Context.NationalGoverningBodyAdmins.Remove(dbUser.NationalGoverningBodyAdmin);
				}

				db.Context.Users.Remove(dbUser);
				await db.Context.SaveChangesAsync();
			}
		}
	}
}
