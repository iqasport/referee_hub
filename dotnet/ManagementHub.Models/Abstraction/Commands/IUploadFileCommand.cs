using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Misc.FileUpload;

namespace ManagementHub.Models.Abstraction.Commands;

public interface IUploadFileCommand
{
	/// <summary>
	/// Uploads a file to the storage using the provided <paramref name="contentType"/> and <paramref name="fileContents"/>.
	/// </summary>
	/// <param name="contentType">MIME content type of the file.</param>
	/// <param name="fileContents">A read stream with contents of the file to upload.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>The key used to identify the file in the blob storage and the file checksum.</returns>
	Task<FileUploadResult> UploadFileAsync(string contentType, Stream fileContents, CancellationToken cancellationToken);
}
