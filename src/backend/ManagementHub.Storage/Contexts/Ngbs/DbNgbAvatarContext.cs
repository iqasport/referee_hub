using System;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Abstraction.Contexts;
using ManagementHub.Models.Domain.Ngb;
using ManagementHub.Models.Domain.User;
using ManagementHub.Storage.Attachments;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.Contexts.User;

public class DbNgbAvatarContextFactory
{
	private readonly IAttachmentRepository attachmentRepository;
	private readonly IAccessFileCommand accessFile;
	private readonly ILogger logger;

	public DbNgbAvatarContextFactory(IAttachmentRepository attachmentRepository, IAccessFileCommand accessFile, ILogger<DbNgbAvatarContextFactory> logger)
	{
		this.attachmentRepository = attachmentRepository;
		this.accessFile = accessFile;
		this.logger = logger;
	}

	public async Task<Uri?> GetNgbAvatarUriAsync(NgbIdentifier ngbId, CancellationToken cancellationToken)
	{
		this.logger.LogInformation(0, "Getting avatar context for NGB ({ngbId}).", ngbId);

		try
		{
			const string attachmentName = "logo";
			var attachment = await this.attachmentRepository.GetAttachmentAsync(ngbId, attachmentName, cancellationToken);

			if (attachment == null)
			{
				return null;
			}

			// TODO: put expiration in settings
			var avatarUri = await this.accessFile.GetFileAccessUriAsync(attachment.Blob.Key, TimeSpan.FromSeconds(20), cancellationToken);

			return avatarUri;
		}
		catch (Exception ex)
		{
			this.logger.LogError(0, ex, "Error while getting avatar URI for NGB ({ngbId}).", ngbId);
			throw;
		}
	}
}
