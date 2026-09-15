using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Data;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.User.Roles;
using ManagementHub.Models.Exceptions;
using ManagementHub.Service.Areas.Teams;
using ManagementHub.Service.Authorization;
using ManagementHub.Service.Contexts;
using ManagementHub.Service.Filtering;
using ManagementHub.Storage;
using ManagementHub.Storage.Collections;
using ManagementHub.Storage.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManagementHub.Service.Areas.Ngbs;

[Authorize(AuthorizationPolicies.NgbAdminPolicy)]
[ApiController]
[Route("api/v2/Ngbs/{ngb}/transfers")]
[Tags("NgbTransfers")]
[Produces("application/json")]
public class NgbTransfersController : ControllerBase
{
	private readonly IUserContextAccessor contextAccessor;
	private readonly ITeamContextProvider teamContextProvider;
	private readonly IReviewNgbTransferCommand reviewNgbTransferCommand;
	private readonly ManagementHubDbContext dbContext;
	private readonly CollectionFilteringContext filteringContext;

	public NgbTransfersController(
		IUserContextAccessor contextAccessor,
		ITeamContextProvider teamContextProvider,
		IReviewNgbTransferCommand reviewNgbTransferCommand,
		ManagementHubDbContext dbContext,
		CollectionFilteringContext filteringContext)
	{
		this.contextAccessor = contextAccessor;
		this.teamContextProvider = teamContextProvider;
		this.reviewNgbTransferCommand = reviewNgbTransferCommand;
		this.dbContext = dbContext;
		this.filteringContext = filteringContext;
	}

	[HttpGet]
	public async Task<Filtered<NgbTransferViewModel>> GetNgbTransfers(
		[FromRoute] NgbIdentifier ngb,
		[FromQuery] FilteringParameters filtering)
	{
		await this.GetAuthorizedNgbAsync(ngb);
		var rows = await this.GetTransferRowsAsync(ngb, filtering);
		var playerNames = await this.GetPlayerNamesAsync(rows);
		var logoUris = await this.GetTeamLogoUrisAsync(rows);

		return rows.Select(row => MapTransfer(row, playerNames, logoUris)).AsFiltered();
	}

	[HttpPost("{invitationId}/approve")]
	public Task<IActionResult> ApproveNgbTransfer(
		[FromRoute] NgbIdentifier ngb,
		[FromRoute] TeamInvitationIdentifier invitationId)
	{
		return this.ReviewTransferAsync(ngb, invitationId, IReviewNgbTransferCommand.ReviewDecision.Approve);
	}

	[HttpPost("{invitationId}/reject")]
	public Task<IActionResult> RejectNgbTransfer(
		[FromRoute] NgbIdentifier ngb,
		[FromRoute] TeamInvitationIdentifier invitationId)
	{
		return this.ReviewTransferAsync(ngb, invitationId, IReviewNgbTransferCommand.ReviewDecision.Reject);
	}

	[HttpPut("~/api/v2/Ngbs/{ngb}/settings/transfers")]
	public async Task<IActionResult> UpdateNgbTransferSettings(
		[FromRoute] NgbIdentifier ngb,
		[FromBody] NgbTransferSettingsRequest request)
	{
		var ngbEntity = await this.GetAuthorizedNgbAsync(ngb);

		ngbEntity.AutoApproveInternalTransfers = request.AutoApproveInternalTransfers;
		await this.dbContext.SaveChangesAsync(this.HttpContext.RequestAborted);
		return this.NoContent();
	}

	private async Task<IActionResult> ReviewTransferAsync(
		NgbIdentifier ngb,
		TeamInvitationIdentifier invitationId,
		IReviewNgbTransferCommand.ReviewDecision decision)
	{
		await this.GetAuthorizedNgbAsync(ngb);
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		var result = await this.reviewNgbTransferCommand.ReviewAsync(
			ngb,
			invitationId,
			userContext.UserId,
			decision,
			this.HttpContext.RequestAborted);

		return result switch
		{
			IReviewNgbTransferCommand.ReviewResultCode.NotFound => this.NotFound(),
			IReviewNgbTransferCommand.ReviewResultCode.AlreadyReviewed => this.Conflict("Transfer has already been reviewed by this NGB."),
			IReviewNgbTransferCommand.ReviewResultCode.Reviewed => this.NoContent(),
			_ => this.StatusCode(StatusCodes.Status500InternalServerError),
		};
	}

	private async Task<NationalGoverningBody> GetAuthorizedNgbAsync(NgbIdentifier ngb)
	{
		var userContext = await this.contextAccessor.GetCurrentUserContextAsync();
		if (!userContext.Roles.OfType<NgbAdminRole>().Any(role => role.Ngb.AppliesTo(ngb)))
		{
			throw new AccessDeniedException($"No permission for NGB {ngb}.");
		}

		return await this.dbContext.NationalGoverningBodies
			.WithIdentifier(ngb)
			.SingleOrDefaultAsync(this.HttpContext.RequestAborted)
			?? throw new NotFoundException(ngb.ToString());
	}

	private async Task<List<NgbTransferRow>> GetTransferRowsAsync(NgbIdentifier ngb, FilteringParameters filtering)
	{
		var transferApprovals =
			from approval in this.dbContext.NgbTransferApprovals
			join nationalGoverningBody in this.dbContext.NationalGoverningBodies.WithIdentifier(ngb)
				on approval.NgbId equals nationalGoverningBody.Id
			select approval;
		var query = this.ApplyFilter(transferApprovals, filtering.Filter);

		if (this.filteringContext.FilteringMetadata != null)
		{
			this.filteringContext.FilteringMetadata.TotalCount = await query.CountAsync(this.HttpContext.RequestAborted);
		}

		return await query
			.OrderBy(approval =>
				approval.ApprovedAt != null
				|| approval.RejectedAt != null
				|| approval.TeamInvitation.AcceptedAt != null
				|| approval.TeamInvitation.DeclinedAt != null
				|| approval.TeamInvitation.RevokedAt != null)
			.ThenByDescending(approval => approval.CreatedAt)
			.Page(filtering)
			.Select(approval => new NgbTransferRow
			{
				TeamInvitationId = approval.TeamInvitationId,
				ApprovedAt = approval.ApprovedAt,
				RejectedAt = approval.RejectedAt,
				InvitationEmail = approval.TeamInvitation.Email,
				InvitationCreatedAt = approval.TeamInvitation.CreatedAt,
				IsInternalTransfer = approval.TeamInvitation.IsInternalTransfer ?? false,
				InvitationAcceptedAt = approval.TeamInvitation.AcceptedAt,
				InvitationDeclinedAt = approval.TeamInvitation.DeclinedAt,
				InvitationRevokedAt = approval.TeamInvitation.RevokedAt,
				DestinationTeamId = approval.TeamInvitation.Team.Id,
				DestinationTeamName = approval.TeamInvitation.Team.Name,
				DestinationNgbCode = approval.TeamInvitation.Team.NationalGoverningBody != null
					? approval.TeamInvitation.Team.NationalGoverningBody.CountryCode
					: null,
				OriginTeamId = approval.TeamInvitation.OriginTeamId,
				OriginTeamName = approval.TeamInvitation.OriginTeam != null ? approval.TeamInvitation.OriginTeam.Name : null,
				OriginNgbCode = approval.TeamInvitation.OriginTeam != null
					&& approval.TeamInvitation.OriginTeam.NationalGoverningBody != null
					? approval.TeamInvitation.OriginTeam.NationalGoverningBody.CountryCode
					: null,
			})
			.ToListAsync(this.HttpContext.RequestAborted);
	}

	private IQueryable<NgbTransferApproval> ApplyFilter(IQueryable<NgbTransferApproval> query, string? filterValue)
	{
		if (string.IsNullOrWhiteSpace(filterValue))
		{
			return query;
		}

		var filter = $"%{filterValue}%";
		return this.dbContext.Database.IsNpgsql()
			? query.Where(approval =>
				EF.Functions.ILike(approval.TeamInvitation.Email, filter)
				|| EF.Functions.ILike(approval.TeamInvitation.Team.Name, filter)
				|| (approval.TeamInvitation.OriginTeam != null && EF.Functions.ILike(approval.TeamInvitation.OriginTeam.Name, filter)))
			: query.Where(approval =>
				EF.Functions.Like(approval.TeamInvitation.Email, filter)
				|| EF.Functions.Like(approval.TeamInvitation.Team.Name, filter)
				|| (approval.TeamInvitation.OriginTeam != null && EF.Functions.Like(approval.TeamInvitation.OriginTeam.Name, filter)));
	}

	private async Task<Dictionary<string, string?>> GetPlayerNamesAsync(IEnumerable<NgbTransferRow> rows)
	{
		var emails = rows.Select(row => row.InvitationEmail.ToLower()).Distinct().ToList();
		var players = await this.dbContext.Users
			.Where(user => emails.Contains(user.Email.ToLower()))
			.Select(user => new { Email = user.Email.ToLower(), user.FirstName, user.LastName })
			.ToListAsync(this.HttpContext.RequestAborted);

		return players.ToDictionary(
			player => player.Email,
			player => TeamInviteHelpers.BuildDisplayName(player.FirstName, player.LastName));
	}

	private async Task<Dictionary<long, Uri?>> GetTeamLogoUrisAsync(IEnumerable<NgbTransferRow> rows)
	{
		var teamIds = rows
			.SelectMany(row => new long?[] { row.OriginTeamId, row.DestinationTeamId })
			.Where(teamId => teamId.HasValue)
			.Select(teamId => teamId!.Value)
			.Distinct();
		var logoUris = new Dictionary<long, Uri?>();

		foreach (var teamId in teamIds)
		{
			logoUris[teamId] = await this.teamContextProvider.GetTeamLogoUriAsync(
				new TeamIdentifier(teamId),
				this.HttpContext.RequestAborted);
		}

		return logoUris;
	}

	private static NgbTransferViewModel MapTransfer(
		NgbTransferRow row,
		IReadOnlyDictionary<string, string?> playerNames,
		IReadOnlyDictionary<long, Uri?> logoUris)
	{
		playerNames.TryGetValue(row.InvitationEmail.ToLower(), out var playerName);

		return new NgbTransferViewModel
		{
			InvitationId = new TeamInvitationIdentifier(row.TeamInvitationId).ToString(),
			PlayerEmail = row.InvitationEmail,
			PlayerName = playerName,
			DestinationTeamId = new TeamIdentifier(row.DestinationTeamId).ToString(),
			DestinationTeamName = row.DestinationTeamName,
			DestinationTeamLogoUri = logoUris.GetValueOrDefault(row.DestinationTeamId),
			DestinationNgbCode = row.DestinationNgbCode,
			OriginTeamId = row.OriginTeamId.HasValue ? new TeamIdentifier(row.OriginTeamId.Value).ToString() : null,
			OriginTeamName = row.OriginTeamName,
			OriginTeamLogoUri = row.OriginTeamId.HasValue ? logoUris.GetValueOrDefault(row.OriginTeamId.Value) : null,
			OriginNgbCode = row.OriginNgbCode,
			IsInternalTransfer = row.IsInternalTransfer,
			ApprovedAt = row.ApprovedAt,
			RejectedAt = row.RejectedAt,
			CreatedAt = row.InvitationCreatedAt,
			Status = GetStatus(row),
		};
	}

	private static TransferApprovalStatus GetStatus(NgbTransferRow row)
	{
		if (row.RejectedAt != null)
			return TransferApprovalStatus.RejectedByNgb;
		if (row.ApprovedAt != null || row.InvitationAcceptedAt != null)
			return TransferApprovalStatus.Approved;
		if (row.InvitationDeclinedAt != null || row.InvitationRevokedAt != null)
			return TransferApprovalStatus.Declined;

		return TransferApprovalStatus.PendingNgbApproval;
	}

	private sealed class NgbTransferRow
	{
		public long TeamInvitationId { get; init; }
		public DateTime? ApprovedAt { get; init; }
		public DateTime? RejectedAt { get; init; }
		public required string InvitationEmail { get; init; }
		public DateTime InvitationCreatedAt { get; init; }
		public bool IsInternalTransfer { get; init; }
		public DateTime? InvitationAcceptedAt { get; init; }
		public DateTime? InvitationDeclinedAt { get; init; }
		public DateTime? InvitationRevokedAt { get; init; }
		public long DestinationTeamId { get; init; }
		public required string DestinationTeamName { get; init; }
		public string? DestinationNgbCode { get; init; }
		public long? OriginTeamId { get; init; }
		public string? OriginTeamName { get; init; }
		public string? OriginNgbCode { get; init; }
	}
}
