using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Models.Misc.FileUpload;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ManagementHub.Storage.BlobStorage.AmazonS3;

/// <summary>
/// Singleton manager of the Amazon S3 blob storage connection.
/// </summary>
public sealed class AmazonBlobStorageManager : IUploadFileCommand, IAccessFileCommand, IDisposable
{
	private readonly AmazonS3Client amazonClient;
	private readonly AmazonS3Config amazonConfig;
	private readonly ILogger<AmazonBlobStorageManager> logger;

	public AmazonBlobStorageManager(IOptions<AmazonS3Config> amazonConfig, ILogger<AmazonBlobStorageManager> logger)
	{
		this.amazonConfig = amazonConfig.Value;
		this.logger = logger;

		this.amazonClient = new AmazonS3Client(this.amazonConfig.AccessKeyId, this.amazonConfig.SecretAccessKey, this.amazonConfig);
	}

	public void Dispose() => this.amazonClient.Dispose();

	public Task<Uri> GetFileAccessUriAsync(string fileKey, TimeSpan expiration, CancellationToken cancellationToken)
	{
		var bucket = this.amazonConfig.Bucket;

		try
		{
			var request = new GetPreSignedUrlRequest()
			{
				BucketName = bucket,
				Key = fileKey,
				Expires = DateTime.UtcNow.Add(expiration),
			};
			var uri = this.amazonClient.GetPreSignedURL(request);
			return Task.FromResult(new Uri(uri));
		}
		catch (Exception ex)
		{
			this.logger.LogError(-0x6fb55900, ex, "Error occured during generation of file access URI.");
			throw;
		}
	}

	public async Task<FileUploadResult> UploadFileAsync(string contentType, Stream fileContents, CancellationToken cancellationToken)
	{
		var key = FileUtils.GenerateRandomFileName();
		var bucket = this.amazonConfig.Bucket;

		try
		{
			var request = new PutObjectRequest
			{
				BucketName = bucket,
				Key = key,
				ContentType = contentType,
				InputStream = fileContents,

				AutoCloseStream = false,
				AutoResetStreamPosition = false,
				ChecksumAlgorithm = ChecksumAlgorithm.SHA256,
				ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256,
			};

			// TODO: resiliency - retry if returns 500
			var response = await this.amazonClient.PutObjectAsync(request, cancellationToken);
			if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
			{
				this.logger.LogInformation(-0x6fb558ff, "Successfully uploaded file '{key}' to S3 blob storage (bucket: {bucket}).", key, bucket);
				return new FileUploadResult(key, response.ChecksumSHA256);
			}
			else
			{
				this.logger.LogError(-0x6fb558fe, "Could not upload file '{key}' to S3 blob storage (bucket: {bucket}). Status code: {statusCode}, Metadata: {metadata}", key, bucket, response.HttpStatusCode, response.ResponseMetadata.Metadata);
				throw new FileUploadException($"Amazon S3 file upload failed - status code {response.HttpStatusCode}");
			}
		}
		catch (Exception ex)
		{
			this.logger.LogError(-0x6fb558fd, ex, "Error occured during file upload.");
			throw new FileUploadException("Exception encountered during file upload.", ex);
		}
	}
}
