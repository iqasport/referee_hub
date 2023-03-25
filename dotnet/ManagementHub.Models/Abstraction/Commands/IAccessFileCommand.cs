using System;
using System.Threading;
using System.Threading.Tasks;

namespace ManagementHub.Models.Abstraction.Commands;

public interface IAccessFileCommand
{
	/// <summary>
	/// Generates a URI to access an uploaded file for a limited time.
	/// </summary>
	/// <param name="fileKey">Identifier of the file in the blob storage.</param>
	/// <param name="expiration">Expiration period after which the file contents should no longer be accessible.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>A URI to access the file from.</returns>
	Task<Uri> GetFileAccessUriAsync(string fileKey, TimeSpan expiration, CancellationToken cancellationToken);
}
