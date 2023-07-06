using ManagementHub.Models.Exceptions;
using ManagementHub.Storage.BlobStorage.LocalFilesystem;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManagementHub.Service.Areas.Export;

/// <summary>
/// Actions related to exporting users with the referee role.
/// </summary>
[ApiController]
[Route("api/debug/blob")]
public class LocalBlobController : ControllerBase
{
	private readonly ILogger logger;
	private readonly LocalFilesystemBlobStorageManager blobStorageManager;
	private readonly IHostEnvironment hostingEnvironment;

	public LocalBlobController(ILogger<LocalBlobController> logger, LocalFilesystemBlobStorageManager blobStorageManager, IHostEnvironment hostingEnvironment)
	{
		this.logger = logger;
		this.blobStorageManager = blobStorageManager;
		this.hostingEnvironment = hostingEnvironment;
	}

	[HttpGet("{fileKey}")]
	[Tags("Debug")]
	[Authorize]
	public async Task GetDataFromLocalBlob([FromRoute] string fileKey)
	{
		if (this.hostingEnvironment.IsProduction())
			throw new AccessDeniedException();

		if (fileKey.Contains(".."))
			throw new InvalidOperationException();

		using (var stream = this.blobStorageManager.OpenFile(fileKey))
			await stream.CopyToAsync(this.HttpContext.Response.BodyWriter.AsStream());
	}
}

public class LocalFileSystemBlobUriBaseProvider : ILocalFileSystemBlobUriBaseProvider
{
	private readonly IHttpContextAccessor httpContextAccessor;

	public LocalFileSystemBlobUriBaseProvider(IHttpContextAccessor httpContextAccessor)
	{
		this.httpContextAccessor = httpContextAccessor;
	}

	public string BaseUri
	{
		get
		{
			var ctx = this.httpContextAccessor.HttpContext!;
			return $"{ctx.Request.Scheme}://{ctx.Request.Host}/api/debug/blob";
		}
	}
}
