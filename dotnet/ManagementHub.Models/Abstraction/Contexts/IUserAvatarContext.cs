using System;

namespace ManagementHub.Models.Abstraction.Contexts;

public interface IUserAvatarContext
{
	/// <summary>
	/// The URI of the avatar image file.
	/// </summary>
	Uri? AvatarUri { get; }
}
