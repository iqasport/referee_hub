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
		var delicateInfo = await this.context.Users
			.WithIdentifier(userId)
			.Join(
				this.context.UserDelicateInfos,
				u => u.Id,
				udi => udi.UserId,
				(u, udi) => udi.Gender)
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

		// Single query using WithIdentifiers to get users and their gender data
		var genderData = await this.context.Users
			.WithIdentifiers(userIdsList)
			.GroupJoin(
				this.context.UserDelicateInfos,
				u => u.Id,
				udi => udi.UserId,
				(u, genders) => new
				{
					UserIdentifier = u.UniqueId != null ? UserIdentifier.Parse(u.UniqueId) : UserIdentifier.FromLegacyUserId(u.Id),
					Gender = genders.Select(g => g.Gender).FirstOrDefault()
				})
			.ToListAsync();

		// Map results to dictionary
		foreach (var data in genderData)
		{
			result[data.UserIdentifier] = data.Gender;
		}

		return result;
	}
}
