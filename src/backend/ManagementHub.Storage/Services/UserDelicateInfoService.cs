using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Services;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.User;
using ManagementHub.Storage.Extensions;
using Microsoft.EntityFrameworkCore;

namespace ManagementHub.Storage.Services;

public class UserDelicateInfoService : IUserDelicateInfoService
{
	private readonly ManagementHubDbContext context;

	public UserDelicateInfoService(ManagementHubDbContext context)
	{
		this.context = context;
	}

	public async Task<string?> GetUserGenderAsync(UserIdentifier userId)
	{
		var userDbId = await this.context.Users
			.WithIdentifier(userId)
			.Select(u => u.Id)
			.FirstOrDefaultAsync();

		if (userDbId == 0)
		{
			return null;
		}

		var delicateInfo = await this.context.UserDelicateInfos
			.Where(udi => udi.UserId == userDbId)
			.Select(udi => udi.Gender)
			.FirstOrDefaultAsync();

		return delicateInfo;
	}

	public async Task SetUserGenderAsync(UserIdentifier userId, string? gender)
	{
		var userDbId = await this.context.Users
			.WithIdentifier(userId)
			.Select(u => u.Id)
			.FirstOrDefaultAsync();

		if (userDbId == 0)
		{
			throw new InvalidOperationException($"User {userId} not found");
		}

		var existingInfo = await this.context.UserDelicateInfos
			.FirstOrDefaultAsync(udi => udi.UserId == userDbId);

		if (existingInfo != null)
		{
			existingInfo.Gender = gender;
			existingInfo.UpdatedAt = DateTime.UtcNow;
		}
		else
		{
			var newInfo = new UserDelicateInfo
			{
				UserId = userDbId,
				Gender = gender,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			};
			this.context.UserDelicateInfos.Add(newInfo);
		}

		await this.context.SaveChangesAsync();
	}

	public async Task<Dictionary<UserIdentifier, string?>> GetMultipleUserGendersAsync(IEnumerable<UserIdentifier> userIds)
	{
		var userIdsList = userIds.ToList();
		var result = new Dictionary<UserIdentifier, string?>();

		if (userIdsList.Count == 0)
		{
			return result;
		}

		// Convert all UserIdentifiers to string representations for query
		var userIdStrings = userIdsList.Select(id => id.ToString()).ToList();
		var userIdLegacyIds = userIdsList.Select(id => id.ToLegacyUserId()).ToList();

		// Batch query to resolve all UserIdentifiers to database IDs at once
		var userMapping = await this.context.Users
			.Where(u => (u.UniqueId != null && userIdStrings.Contains(u.UniqueId)) || (u.UniqueId == null && userIdLegacyIds.Contains(u.Id)))
			.Select(u => new
			{
				u.Id,
				UserIdentifier = u.UniqueId != null ? UserIdentifier.Parse(u.UniqueId) : UserIdentifier.FromLegacyUserId(u.Id)
			})
			.ToListAsync();

		if (userMapping.Count == 0)
		{
			return result;
		}

		var userDbIds = userMapping.Select(um => um.Id).ToList();

		// Batch query for gender data
		var genderData = await this.context.UserDelicateInfos
			.Where(udi => userDbIds.Contains(udi.UserId))
			.Select(udi => new { udi.UserId, udi.Gender })
			.ToListAsync();

		// Create a lookup from database ID to gender
		var genderLookup = genderData.ToDictionary(g => g.UserId, g => g.Gender);

		// Map results back to UserIdentifiers
		foreach (var mapping in userMapping)
		{
			if (genderLookup.TryGetValue(mapping.Id, out var gender))
			{
				result[mapping.UserIdentifier] = gender;
			}
		}

		return result;
	}
}
