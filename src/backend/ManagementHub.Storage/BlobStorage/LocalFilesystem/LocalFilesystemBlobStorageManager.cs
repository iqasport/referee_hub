using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Misc.FileUpload;
using ManagementHub.Storage.BlobStorage.AmazonS3;
using Microsoft.Extensions.Logging;

namespace ManagementHub.Storage.BlobStorage.LocalFilesystem;

/// <summary>
/// Blob storage in a temp directory for dev testing.
/// </summary>
public class LocalFilesystemBlobStorageManager : IUploadFileCommand, IAccessFileCommand
{
	private readonly DirectoryInfo tempDir;
	private readonly ILogger<LocalFilesystemBlobStorageManager> logger;
	private readonly ILocalFileSystemBlobUriBaseProvider baseUriProvider;

	public LocalFilesystemBlobStorageManager(ILogger<LocalFilesystemBlobStorageManager> logger, ILocalFileSystemBlobUriBaseProvider baseUriProvider)
	{
		this.tempDir = Directory.CreateTempSubdirectory("ManagementHubBlobStorage");
		this.logger = logger;
		this.baseUriProvider = baseUriProvider;
	}

	public Task<Uri> GetFileAccessUriAsync(string fileKey, TimeSpan expiration, CancellationToken cancellationToken)
	{
		return Task.FromResult(new Uri($"{this.baseUriProvider.BaseUri}/{fileKey}"));
	}

	public FileStream OpenFile(string fileKey) => File.OpenRead(Path.Combine(this.tempDir.FullName, fileKey));

	public async Task<FileUploadResult> UploadFileAsync(string contentType, Stream fileContents, CancellationToken cancellationToken)
	{
		// HACK: for simplicity sake as I can't make the file system provide a specific contentType on download I'll do so with extension, but this ofc fails for many MIME types
		var extension = contentType.Split('/')[1];
		var fileName = $"{FileUtils.GenerateRandomFileName()}.{extension}";
		var path = Path.Combine(this.tempDir.FullName, fileName);
		using (var file = File.OpenWrite(path))
		{
			await fileContents.CopyToAsync(file);
		}

		string checksum;
		using (var sha = SHA256.Create())
		{
			using (var file = File.OpenRead(path))
			{
				var bytes = await sha.ComputeHashAsync(file);
				checksum = Convert.ToBase64String(bytes);
			}
		}

		return new FileUploadResult(fileName, checksum);
	}
}
