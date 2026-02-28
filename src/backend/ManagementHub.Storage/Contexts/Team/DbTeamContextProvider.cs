using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.Tournament;
using ManagementHub.Models.Enums;
using ManagementHub.Storage.Attachments;
using ManagementHub.Storage.Collections;
using Microsoft.EntityFrameworkCore;

namespace ManagementHub.Storage.Contexts.Team;

public class DbTeamContextProvider : ITeamContextProvider
{
	private readonly DbTeamContextFactory dbTeamContextFactory;
	private readonly IAttachmentRepository attachmentRepository;
	private readonly IAccessFileCommand accessFileCommand;

	public DbTeamContextProvider(
		ManagementHubDbContext dbContext,
		CollectionFilteringContext filteringContext,
		IAttachmentRepository attachmentRepository,
		IAccessFileCommand accessFileCommand)
	{
		this.dbTeamContextFactory = new DbTeamContextFactory(
			dbContext,
			filteringContext
		);
		this.attachmentRepository = attachmentRepository;
		this.accessFileCommand = accessFileCommand;
	}

	public async Task<bool> CheckTeamExistsInNgbAsync(NgbIdentifier ngb, TeamIdentifier teamId)
	{
		var count = await this.dbTeamContextFactory.QueryTeamsInternal(NgbConstraint.Single(ngb)).Where(t => t.Id == teamId.Id).CountAsync();
		Debug.Assert(count >= 0 && count <= 1);
		return count > 0;
	}

	public Task<ITeamContext> CreateTeamAsync(NgbIdentifier ngb, TeamData teamData)
	{
		return this.dbTeamContextFactory.CreateTeamAsync(ngb, teamData);
	}

	public Task DeleteTeamAsync(NgbIdentifier ngb, TeamIdentifier teamId)
	{
		return this.dbTeamContextFactory.DeleteTeamAsync(teamId);
	}

	public IQueryable<ITeamContext> GetTeams(NgbConstraint ngbs, TeamGroupAffiliation? groupAffiliation = null)
	{
		return this.dbTeamContextFactory.QueryTeams(ngbs, groupAffiliation);
	}

	public Task<ITeamContext> UpdateTeamAsync(NgbIdentifier ngb, TeamIdentifier teamId, TeamData teamData)
	{
		return this.dbTeamContextFactory.UpdateTeamAsync(ngb, teamId, teamData);
	}

	public Task<ITeamContext?> GetTeamAsync(TeamIdentifier teamId, NgbConstraint ngbs)
	{
		return this.dbTeamContextFactory.GetTeamAsync(teamId, ngbs);
	}

	public IQueryable<TeamMemberInfo> QueryTeamMembers(TeamIdentifier teamId, NgbConstraint ngbs)
	{
		return this.dbTeamContextFactory.QueryTeamMembers(teamId, ngbs);
	}

	public Task<IEnumerable<ManagerInfo>> GetTeamManagersAsync(TeamIdentifier teamId, NgbConstraint ngbs)
	{
		return this.dbTeamContextFactory.GetTeamManagersAsync(teamId, ngbs);
	}

	public async Task<Uri?> GetTeamLogoUriAsync(TeamIdentifier teamId, CancellationToken cancellationToken = default)
	{
		const string attachmentName = "logo";
		var attachment = await this.attachmentRepository.GetAttachmentAsync(teamId, attachmentName, cancellationToken);
		if (attachment == null)
		{
			return null;
		}

		// TODO: put expiration in settings
		return await this.accessFileCommand.GetFileAccessUriAsync(attachment.Blob.Key, TimeSpan.FromSeconds(20), cancellationToken);
	}
}
