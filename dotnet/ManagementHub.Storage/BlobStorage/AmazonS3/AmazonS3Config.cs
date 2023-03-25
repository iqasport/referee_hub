using System.ComponentModel.DataAnnotations;
using Amazon;

namespace ManagementHub.Storage.BlobStorage.AmazonS3;

/// <summary>
/// Amazon S3 config extension for being loaded from the configuration, which additionally allows us to read credentials for the S3 bucket.
/// </summary>
public class AmazonS3Config : Amazon.S3.AmazonS3Config
{
	[Required]
	public required string AccessKeyId { get; set; }
	
	[Required]
	public required string SecretAccessKey { get; set; }
	
	[Required]
	public required string Bucket { get; set; }

	public string Region
	{
		get => this.RegionEndpoint.SystemName;
		set => this.RegionEndpoint = RegionEndpoint.GetBySystemName(value);
	}
}
