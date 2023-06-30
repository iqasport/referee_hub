using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;
using ManagementHub.Models;
using ManagementHub.Models.Data;
using ManagementHub.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Service.API.Test.DatabaseClient
{
	public static class UserDbExtensions
	{
		public static async Task SetUserAccessTypeAsync(this DatabaseProvider databaseProvider, string email, UserAccessType accessType)
		{
			using (var db = await databaseProvider.ConnectAsync())
			{
				var user = db.Context.Users
					.Include(x => x.Roles)
					.AsSingleQuery()
					.SingleOrDefault(u => u.Email == email);

				if (user == null)
				{
					throw new InvalidOperationException($"No user with email {email} exists.");
				}

				db.Context.Roles.RemoveRange(user.Roles);
				user.Roles.Add(new Role
				{
					User = user,
					AccessType = accessType,
					CreatedAt = DateTime.Now,
					UpdatedAt = DateTime.Now,
				});

				await db.Context.SaveChangesAsync();
			}
		}

		public static async Task SetNgbAdminCountryAsync(this DatabaseProvider databaseProvider, string email, string country)
		{
			using (var db = await databaseProvider.ConnectAsync())
			{
				var user = db.Context.Users
					.Include(x => x.NationalGoverningBodyAdmin)
					.AsSingleQuery()
					.SingleOrDefault(u => u.Email == email);

				if (user == null)
				{
					throw new InvalidOperationException($"No user with email {email} exists.");
				}

				var ngb = db.Context.NationalGoverningBodies
					.SingleOrDefault(n => n.Country == country);

				if (ngb == null)
				{
					throw new InvalidOperationException($"No NGB for country {country} exists.");
				}

				user.NationalGoverningBodyAdmin = new NationalGoverningBodyAdmin
				{
					User = user,
					NationalGoverningBody = ngb,
					CreatedAt = DateTime.Now,
					UpdatedAt = DateTime.Now,
				};

				await db.Context.SaveChangesAsync();
			}
		}

		public static async Task<IEnumerable<PolicyManagerUserTerm>> GetPoliciesAsync(this DatabaseProvider databaseProvider, string email)
		{
			using (var db = await databaseProvider.ConnectAsync())
			{
				var user = db.Context.Users
					.Include(x => x.PolicyManagerUserTerms).ThenInclude(p => p.Term)
					.AsSingleQuery()
					.SingleOrDefault(u => u.Email == email);

				if (user == null)
				{
					throw new InvalidOperationException($"No user with email {email} exists.");
				}

				return user.PolicyManagerUserTerms;
			}
		}

		public static async Task DeleteUserAsync(this DatabaseProvider databaseProvider, string email, bool requireUserToExist = true)
		{
			using (var db = await databaseProvider.ConnectAsync())
			{
				var user = db.Context.Users
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

				if (user == null)
				{
					if (requireUserToExist) throw new InvalidOperationException($"No user with email {email} exists.");
					else return;
				}

				db.Context.CertificationPayments.RemoveRange(user.CertificationPayments);
				db.Context.PolicyManagerPortabilityRequests.RemoveRange(user.PolicyManagerPortabilityRequests);
				db.Context.PolicyManagerUserTerms.RemoveRange(user.PolicyManagerUserTerms);
				db.Context.RefereeAnswers.RemoveRange(user.RefereeAnswers);
				db.Context.RefereeCertifications.RemoveRange(user.RefereeCertifications);
				db.Context.RefereeLocations.RemoveRange(user.RefereeLocations);
				db.Context.RefereeTeams.RemoveRange(user.RefereeTeams);
				db.Context.Roles.RemoveRange(user.Roles);
				db.Context.TestAttempts.RemoveRange(user.TestAttempts);
				db.Context.TestResults.RemoveRange(user.TestResults);

				if (user.NationalGoverningBodyAdmin is not null)
				{
					db.Context.NationalGoverningBodyAdmins.Remove(user.NationalGoverningBodyAdmin);
				}

				db.Context.ActiveStorageAttachments.RemoveRange(db.Context.ActiveStorageAttachments
					.Where(a => a.RecordType == "User" && a.RecordId == user.Id));

				db.Context.Users.Remove(user);
				await db.Context.SaveChangesAsync();
			}
		}
	}
}
