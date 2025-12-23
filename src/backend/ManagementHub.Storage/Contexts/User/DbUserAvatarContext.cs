using System;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Domain.User;
using ManagementHub.Storage.Attachments;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Contexts.User;

public record DbUserAvatarContext(Uri? AvatarUri) : IUserAvatarContext
{
}

public class DbUserAvatarContextFactory
{
	private readonly IAttachmentRepository attachmentRepository;
	private readonly IAccessFileCommand accessFile;
	private readonly ILogger<DbUserAvatarContextFactory> logger;

	public DbUserAvatarContextFactory(IAttachmentRepository attachmentRepository, IAccessFileCommand accessFile, ILogger<DbUserAvatarContextFactory> logger)
	{
		this.attachmentRepository = attachmentRepository;
		this.accessFile = accessFile;
		this.logger = logger;
	}

	public async Task<IUserAvatarContext> LoadAsync(UserIdentifier userId, CancellationToken cancellationToken)
	{
		this.logger.LogInformation(0x4727dc00, "Getting avatar context for user ({userId}).", userId);

		try
		{

			const string attachmentName = "avatar";
			var attachment = await this.attachmentRepository.GetAttachmentAsync(userId, attachmentName, cancellationToken);

			if (attachment == null)
			{
				return new DbUserAvatarContext(null);
			}

			// TODO: put expiration in settings
			var avatarUri = await this.accessFile.GetFileAccessUriAsync(attachment.Blob.Key, TimeSpan.FromSeconds(20), cancellationToken);

			return new DbUserAvatarContext(avatarUri);
		}
		catch (Exception ex)
		{
			this.logger.LogError(0x4727dc01, ex, "Error while getting avatar URI for user ({userId}).", userId);
			throw;
		}
	}
}
