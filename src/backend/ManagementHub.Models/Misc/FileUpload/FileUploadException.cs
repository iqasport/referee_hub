using System;

namespace ManagementHub.Models.Misc.FileUpload;

/// <summary>
/// Indicates file upload wasn't successful.
/// </summary>
public class FileUploadException : Exception
{
	public FileUploadException(string? message, Exception? innerException = null) : base(message, innerException)
	{
	}
}
