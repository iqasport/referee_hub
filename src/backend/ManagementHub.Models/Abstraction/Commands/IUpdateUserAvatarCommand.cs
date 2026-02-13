using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.Team;
using ManagementHub.Models.Domain.User;

namespace ManagementHub.Models.Abstraction.Commands;

public interface IUpdateUserAvatarCommand
{
	/// <summary>
	/// Uploads the avatar file (with contents are identified by <paramref name="contentType"/> and <paramref name="avatarStream"/>)
	/// into a blob storage, saves the information about the blob in the database for the user and returns the URI the avatar can be downloaded from going forward.
	/// </summary>
	/// <remarks>
	/// If a user has a previous avatar, the old one should be removed after the transaction for the new one has completed successfully.
	/// </remarks>
	/// <param name="userId">Id of the user whose avatar is updated.</param>
	/// <param name="contentType">Media content type (e.g. <c>image/jpeg</c> or <c>image/png</c>).</param>
	/// <param name="avatarStream">Stream with the avatar file contents.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns></returns>
	Task<Uri> UpdateUserAvatarAsync(UserIdentifier userId, string contentType, Stream avatarStream, CancellationToken cancellationToken);

	/// <summary>
	/// Uploads the avatar file (with contents are identified by <paramref name="contentType"/> and <paramref name="avatarStream"/>)
	/// into a blob storage, saves the information about the blob in the database for the user and returns the URI the avatar can be downloaded from going forward.
	/// </summary>
	/// <remarks>
	/// If NGB has a previous avatar, the old one should be removed after the transaction for the new one has completed successfully.
	/// </remarks>
	/// <param name="ngbId">Id of the NGB whose avatar is updated.</param>
	/// <param name="contentType">Media content type (e.g. <c>image/jpeg</c> or <c>image/png</c>).</param>
	/// <param name="avatarStream">Stream with the avatar file contents.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns></returns>
	Task<Uri> UpdateNgbAvatarAsync(NgbIdentifier ngbId, string contentType, Stream avatarStream, CancellationToken cancellationToken);

	/// <summary>
	/// Uploads the logo file for a team into blob storage and saves the information in the database.
	/// Returns the URI where the logo can be accessed.
	/// </summary>
	/// <remarks>
	/// If team has a previous logo, the old one should be removed after the transaction for the new one has completed successfully.
	/// </remarks>
	/// <param name="teamId">Id of the team whose logo is updated.</param>
	/// <param name="contentType">Media content type (e.g. <c>image/jpeg</c> or <c>image/png</c>).</param>
	/// <param name="logoStream">Stream with the logo file contents.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns></returns>
	Task<Uri> UpdateTeamLogoAsync(TeamIdentifier teamId, string contentType, Stream logoStream, CancellationToken cancellationToken);
}
