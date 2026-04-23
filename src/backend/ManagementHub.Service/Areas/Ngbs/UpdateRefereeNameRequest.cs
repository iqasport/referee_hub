namespace ManagementHub.Service.Areas.Ngbs;

/// <summary>
/// Request body for updating a referee's name by an NGB admin.
/// </summary>
public class UpdateRefereeNameRequest
{
	public string? FirstName { get; init; }

	public string? LastName { get; init; }
}
