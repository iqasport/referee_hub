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

		// Resolve all UserIdentifiers to database IDs
		var userIdMap = new Dictionary<long, UserIdentifier>();
		foreach (var userId in userIdsList)
		{
			var userDbId = await this.context.Users
				.WithIdentifier(userId)
				.Select(u => u.Id)
				.FirstOrDefaultAsync();

			if (userDbId != 0)
			{
				userIdMap[userDbId] = userId;
			}
		}

		if (userIdMap.Count == 0)
		{
			return result;
		}

		// Batch query for gender data
		var genderData = await this.context.UserDelicateInfos
			.Where(udi => userIdMap.Keys.Contains(udi.UserId))
			.Select(udi => new { udi.UserId, udi.Gender })
			.ToListAsync();

		// Map results back to UserIdentifiers
		foreach (var data in genderData)
		{
			if (userIdMap.TryGetValue(data.UserId, out var userId))
			{
				result[userId] = data.Gender;
			}
		}

		return result;
	}
}
